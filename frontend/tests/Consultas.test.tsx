import { render, screen, within } from '@testing-library/react';
import { afterEach, describe, expect, it, vi } from 'vitest';
import { HistorialBatches } from '../src/pages/HistorialBatches';
import { HistorialVentas } from '../src/pages/HistorialVentas';
import { Stock } from '../src/pages/Stock';
import { apiFalsa } from './apiFalsa';

afterEach(() => vi.unstubAllGlobals());

describe('Consulta de stock', () => {
  it('muestra kg con 3 decimales, $ en formato es-AR y "—" si el café nunca se tostó', async () => {
    apiFalsa({
      'GET /api/stock': {
        status: 200,
        body: [
          { cafeVerdeId: 1, cafeVerde: 'Brasil', stockVerdeKg: 12, stockTostadoKg: 0, costoPromedioKg: null },
          { cafeVerdeId: 2, cafeVerde: 'Colombia', stockVerdeKg: 5, stockTostadoKg: 4.25, costoPromedioKg: 23.53 },
        ],
      },
    });

    render(<Stock />);

    const brasil = (await screen.findByText('Brasil')).closest('tr')!;
    expect(within(brasil).getByText('12,000 kg')).toBeInTheDocument();
    expect(within(brasil).getByText('—')).toBeInTheDocument();
    const colombia = screen.getByText('Colombia').closest('tr')!;
    expect(within(colombia).getByText('4,250 kg')).toBeInTheDocument();
    expect(colombia).toHaveTextContent('23,53/kg');
  });

  it('muestra el estado vacío', async () => {
    apiFalsa({ 'GET /api/stock': { status: 200, body: [] } });
    render(<Stock />);
    expect(await screen.findByText('No hay registros.')).toBeInTheDocument();
  });
});

describe('Historiales', () => {
  it('el historial de batches muestra merma y costo de producción ($/kg y total)', async () => {
    apiFalsa({
      'GET /api/batches': {
        status: 200,
        body: [{
          id: 1, cafeVerdeId: 2, cafeVerde: 'Colombia', fecha: '2026-10-27', verdeKg: 5, tostadoKg: 4.25,
          humedadPct: null, densidadGl: null, desarrolloPct: null,
          precioKgAplicado: 20, mermaPct: 15, costoTostadoKg: 23.53, costoTotal: 100,
        }],
      },
    });

    render(<HistorialBatches />);

    const fila = (await screen.findByText('Colombia')).closest('tr')!;
    expect(fila).toHaveTextContent('27/10/2026');
    expect(fila).toHaveTextContent('15,00 %');
    expect(fila).toHaveTextContent('23,53/kg');
    expect(fila).toHaveTextContent('100,00');
  });

  it('el historial de ventas muestra costos asociados, ganancia y margen', async () => {
    apiFalsa({
      'GET /api/ventas': {
        status: 200,
        body: [{
          id: 7, fecha: '2026-10-27', cliente: { id: 2, nombre: 'Bar Central' }, cafeVerde: { id: 3, nombre: 'Colombia' },
          presentacion: { id: 1, nombre: 'Bolsa 250 g', gramos: 250 }, unidades: 4, precioUnitario: 15,
          cantidadVendidaKg: 1, costoPromedioKgAplicado: 22,
          insumos: [{ nombre: 'Bolsa', precioUnitarioAplicado: 0.5 }, { nombre: 'Etiqueta', precioUnitarioAplicado: 0.1 }],
          ingreso: 60, costoCafeVendido: 22, costosAsociados: 2.4, ganancia: 35.6, margenPct: 59.33,
        }],
      },
    });

    render(<HistorialVentas />);

    const fila = (await screen.findByText('Bar Central')).closest('tr')!;
    expect(fila).toHaveTextContent('2,40');
    expect(fila).toHaveTextContent('35,60');
    expect(fila).toHaveTextContent('59,33 %');
    expect(fila).toHaveTextContent('Bolsa');
  });

  it('los historiales vacíos muestran el mensaje', async () => {
    apiFalsa({ 'GET /api/batches': { status: 200, body: [] }, 'GET /api/ventas': { status: 200, body: [] } });
    render(<><HistorialBatches /><HistorialVentas /></>);
    expect(await screen.findAllByText('No hay registros.')).toHaveLength(2);
  });
});
