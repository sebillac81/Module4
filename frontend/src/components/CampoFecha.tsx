import { hoyIso } from '../format/formato';

interface Props {
  valor: string;
  onChange: (valor: string) => void;
}

/** Fecha precargada con hoy; no permite elegir fechas futuras (FR-028). */
export function CampoFecha({ valor, onChange }: Props) {
  return <input type="date" value={valor} max={hoyIso()} onChange={(e) => onChange(e.target.value)} />;
}
