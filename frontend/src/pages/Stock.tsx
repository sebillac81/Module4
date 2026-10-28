import { consultarStock } from '../api/cafe';
import { Tabla } from '../components/Tabla';
import { useListado } from '../components/useListado';
import { formatearKg, formatearMonedaKg } from '../format/formato';

/** Stock por café (FR-019). Con tostado en 0 se ve el último costo promedio; "—" si nunca se tostó. */
export function Stock() {
  const { filas, error } = useListado(consultarStock);
  return (
    <section>
      <h1>Stock</h1>
      {error && <p role="alert">{error}</p>}
      <Tabla
        filas={filas}
        clave={(s) => s.cafeVerdeId}
        columnas={[
          { titulo: 'Café', valor: (s) => s.cafeVerde },
          { titulo: 'Stock verde', valor: (s) => formatearKg(s.stockVerdeKg), numerica: true },
          { titulo: 'Stock tostado', valor: (s) => formatearKg(s.stockTostadoKg), numerica: true },
          { titulo: 'Costo promedio', valor: (s) => formatearMonedaKg(s.costoPromedioKg), numerica: true },
        ]}
      />
    </section>
  );
}
