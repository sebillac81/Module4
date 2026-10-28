import { Referencia } from '../api/cafe';

interface Props {
  opciones: Referencia[];
  valor: number | null;
  onChange: (id: number | null) => void;
  vacio?: string;
}

/** Select de una entidad por id (café, proveedor, cliente…). */
export function SelectorId({ opciones, valor, onChange, vacio = 'Elegí una opción' }: Props) {
  return (
    <select value={valor ?? ''} onChange={(e) => onChange(e.target.value ? Number(e.target.value) : null)}>
      <option value="">{vacio}</option>
      {opciones.map((o) => (
        <option key={o.id} value={o.id}>
          {o.nombre}
        </option>
      ))}
    </select>
  );
}
