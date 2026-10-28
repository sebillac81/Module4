import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { afterEach, describe, expect, it, vi } from 'vitest';
import { Batches } from '../src/pages/Batches';
import { apiFalsa } from './apiFalsa';

const cafes = [{ id: 3, nombre: 'Colombia', proveedorCafe: { id: 1, nombre: 'Finca' }, precioKg: 20, stockInicialKg: 10 }];

afterEach(() => vi.unstubAllGlobals());

async function completarYEnviar() {
  const usuario = userEvent.setup();
  render(<Batches />);
  await screen.findByRole('option', { name: 'Colombia' });
  await usuario.selectOptions(screen.getByRole('combobox'), '3');
  await usuario.type(screen.getByLabelText('Café verde utilizado (kg)'), '5,25');
  await usuario.type(screen.getByLabelText('Café tostado obtenido (kg)'), '4.5');
  await usuario.click(screen.getByRole('button', { name: 'Guardar' }));
}

describe('Página de batches', () => {
  it('envía los kg tal como se cargaron y muestra los valores calculados por la API', async () => {
    const { enviados } = apiFalsa({
      'GET /api/cafes-verdes': { status: 200, body: cafes },
      'POST /api/batches': {
        status: 201,
        body: {
          id: 1, cafeVerdeId: 3, cafeVerde: 'Colombia', fecha: '2026-10-27', verdeKg: 5.25, tostadoKg: 4.5,
          humedadPct: null, densidadGl: null, desarrolloPct: null,
          precioKgAplicado: 20, mermaPct: 14.29, costoTostadoKg: 23.33, costoTotal: 105,
        },
      },
    });

    await completarYEnviar();

    expect(enviados[0].body).toMatchObject({ cafeVerdeId: 3, verdeKg: 5.25, tostadoKg: 4.5, humedadPct: null });
    const resultado = await screen.findByLabelText('Resultado del batch');
    expect(resultado).toHaveTextContent('14,29 %');
    expect(resultado).toHaveTextContent('23,33/kg');
    expect(resultado).toHaveTextContent('105,00');
  });

  it('muestra el stock disponible y el requerido cuando la API responde 409', async () => {
    apiFalsa({
      'GET /api/cafes-verdes': { status: 200, body: cafes },
      'POST /api/batches': {
        status: 409,
        body: { title: 'Stock insuficiente', detail: 'No hay stock suficiente.', disponibleKg: 3, requeridoKg: 5.25 },
      },
    });

    await completarYEnviar();

    expect(await screen.findByRole('alert')).toHaveTextContent(
      'No hay stock suficiente: disponible 3,000 kg, requerido 5,250 kg.',
    );
  });

  it('muestra los errores de validación junto a cada campo', async () => {
    apiFalsa({
      'GET /api/cafes-verdes': { status: 200, body: cafes },
      'POST /api/batches': {
        status: 400,
        body: { title: 'Datos inválidos', errors: { tostadoKg: ['El café tostado debe ser menor al café verde utilizado.'] } },
      },
    });

    await completarYEnviar();

    expect(await screen.findByText('El café tostado debe ser menor al café verde utilizado.')).toBeInTheDocument();
  });
});
