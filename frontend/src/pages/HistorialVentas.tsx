import { listarVentas } from '../api/ventas';
import { Tabla } from '../components/Tabla';
import { useListado } from '../components/useListado';
import { formatearFecha, formatearMoneda, formatearPct } from '../format/formato';

/** Historial de ventas con costos asociados y rentabilidad (FR-021). */
export function HistorialVentas() {
  const { filas, error } = useListado(listarVentas);
  return (
    <section>
      <h1>Historial de ventas</h1>
      {error && <p role="alert">{error}</p>}
      <Tabla
        filas={filas}
        clave={(v) => v.id}
        columnas={[
          { titulo: 'Fecha', valor: (v) => formatearFecha(v.fecha) },
          { titulo: 'Cliente', valor: (v) => v.cliente.nombre },
          { titulo: 'Café', valor: (v) => v.cafeVerde.nombre },
          { titulo: 'Presentación', valor: (v) => v.presentacion.nombre },
          { titulo: 'Unidades', valor: (v) => v.unidades, numerica: true },
          { titulo: 'Precio', valor: (v) => formatearMoneda(v.precioUnitario), numerica: true },
          { titulo: 'Ingreso', valor: (v) => formatearMoneda(v.ingreso), numerica: true },
          { titulo: 'Costo café', valor: (v) => formatearMoneda(v.costoCafeVendido), numerica: true },
          {
            titulo: 'Costos asociados',
            numerica: true,
            valor: (v) => (
              <span
                title={v.insumos.map((i) => `${i.nombre}: ${formatearMoneda(i.precioUnitarioAplicado)}`).join('\n')}
              >
                {formatearMoneda(v.costosAsociados)}
                {v.insumos.length > 0 && <small> ({v.insumos.map((i) => i.nombre).join(', ')})</small>}
              </span>
            ),
          },
          {
            titulo: 'Ganancia',
            numerica: true,
            valor: (v) => <span className={v.ganancia < 0 ? 'negativo' : undefined}>{formatearMoneda(v.ganancia)}</span>,
          },
          {
            titulo: 'Margen',
            numerica: true,
            valor: (v) => <span className={v.margenPct < 0 ? 'negativo' : undefined}>{formatearPct(v.margenPct)}</span>,
          },
        ]}
      />
    </section>
  );
}
