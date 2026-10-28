import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { afterEach, describe, expect, it, vi } from 'vitest';
import { Ventas } from '../src/pages/Ventas';
import { apiFalsa } from './apiFalsa';

afterEach(() => vi.unstubAllGlobals());

const ventaBase = {
  id: 7, fecha: '2026-10-27',
  cliente: { id: 2, nombre: 'Bar Central' },
  cafeVerde: { id: 3, nombre: 'Colombia' },
  presentacion: { id: 1, nombre: 'Bolsa 250 g', gramos: 250 },
  unidades: 4, precioUnitario: 15, cantidadVendidaKg: 1, costoPromedioKgAplicado: 22,
  insumos: [{ nombre: 'Bolsa', precioUnitarioAplicado: 0.5 }],
};

function catalogos(venta: object) {
  return apiFalsa({
    'GET /api/clientes': { status: 200, body: [{ id: 2, nombre: 'Bar Central' }] },
    'GET /api/cafes-verdes': { status: 200, body: [{ id: 3, nombre: 'Colombia' }] },
    'GET /api/presentaciones': { status: 200, body: [{ id: 1, nombre: 'Bolsa 250 g', gramos: 250, insumos: [] }] },
    'POST /api/ventas': { status: 201, body: venta },
  });
}

async function registrar(unidades: string, precio: string) {
  const usuario = userEvent.setup();
  render(<Ventas />);
  await screen.findByRole('option', { name: 'Bar Central' });
  const [cliente, cafe, presentacion] = screen.getAllByRole('combobox');
  await usuario.selectOptions(cliente, '2');
  await usuario.selectOptions(cafe, '3');
  await usuario.selectOptions(presentacion, '1');
  await usuario.type(screen.getByLabelText('Unidades'), unidades);
  await usuario.type(screen.getByLabelText('Precio por unidad ($)'), precio);
  await usuario.click(screen.getByRole('button', { name: 'Guardar' }));
}

describe('Página de ventas', () => {
  it('muestra los valores de la API formateados, sin recalcular', async () => {
    // Valores deliberadamente distintos de un recálculo local: la UI solo los formatea.
    const { enviados } = catalogos({
      ...ventaBase, ingreso: 60, costoCafeVendido: 22, costosAsociados: 2.4, ganancia: 35.6, margenPct: 59.33,
    });

    await registrar('4', '15');

    expect(enviados[0].body).toMatchObject({ clienteId: 2, cafeVerdeId: 3, presentacionId: 1, unidades: 4, precioUnitario: 15 });
    const resultado = await screen.findByLabelText('Resultado de la venta');
    expect(resultado).toHaveTextContent('60,00');
    expect(resultado).toHaveTextContent('2,40');
    expect(resultado).toHaveTextContent('35,60');
    expect(resultado).toHaveTextContent('59,33 %');
  });

  it('muestra la ganancia y el margen negativos', async () => {
    catalogos({ ...ventaBase, ingreso: 4, costoCafeVendido: 22, costosAsociados: 2.4, ganancia: -20.4, margenPct: -510 });

    await registrar('4', '1');

    const resultado = await screen.findByLabelText('Resultado de la venta');
    expect(resultado).toHaveTextContent('-510,00 %');
    expect(screen.getByText('-510,00 %')).toHaveClass('negativo');
  });
});
