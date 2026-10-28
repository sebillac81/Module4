import { listarBatches } from '../api/cafe';
import { Tabla } from '../components/Tabla';
import { useListado } from '../components/useListado';
import { formatearFecha, formatearKg, formatearMoneda, formatearMonedaKg, formatearPct } from '../format/formato';

/** Historial de batches con su costo de producción: $/kg y total (FR-020, FR-012a). */
export function HistorialBatches() {
  const { filas, error } = useListado(listarBatches);
  return (
    <section>
      <h1>Historial de batches</h1>
      {error && <p role="alert">{error}</p>}
      <Tabla
        filas={filas}
        clave={(b) => b.id}
        columnas={[
          { titulo: 'Fecha', valor: (b) => formatearFecha(b.fecha) },
          { titulo: 'Café', valor: (b) => b.cafeVerde },
          { titulo: 'Verde', valor: (b) => formatearKg(b.verdeKg), numerica: true },
          { titulo: 'Tostado', valor: (b) => formatearKg(b.tostadoKg), numerica: true },
          { titulo: 'Merma', valor: (b) => formatearPct(b.mermaPct), numerica: true },
          { titulo: 'Precio verde', valor: (b) => formatearMonedaKg(b.precioKgAplicado), numerica: true },
          { titulo: 'Costo tostado', valor: (b) => formatearMonedaKg(b.costoTostadoKg), numerica: true },
          { titulo: 'Costo total', valor: (b) => formatearMoneda(b.costoTotal), numerica: true },
        ]}
      />
    </section>
  );
}
