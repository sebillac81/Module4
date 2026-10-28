// Solo formatea valores que ya calculó la API: sin aritmética de negocio (principio V).

const moneda = new Intl.NumberFormat('es-AR', {
  style: 'currency',
  currency: 'ARS',
  minimumFractionDigits: 2,
  maximumFractionDigits: 2,
});
const dosDecimales = new Intl.NumberFormat('es-AR', {
  minimumFractionDigits: 2,
  maximumFractionDigits: 2,
});
const tresDecimales = new Intl.NumberFormat('es-AR', {
  minimumFractionDigits: 3,
  maximumFractionDigits: 3,
});

export const formatearMoneda = (valor: number | null | undefined) =>
  valor == null ? '—' : moneda.format(valor);

export const formatearMonedaKg = (valor: number | null | undefined) =>
  valor == null ? '—' : `${moneda.format(valor)}/kg`;

export const formatearPct = (valor: number) => `${dosDecimales.format(valor)} %`;

export const formatearKg = (valor: number) => `${tresDecimales.format(valor)} kg`;

export const formatearDecimal = (valor: number | null | undefined) =>
  valor == null ? '—' : dosDecimales.format(valor);

/** "2026-10-27" → "27/10/2026", sin pasar por Date para no depender de la zona horaria. */
export const formatearFecha = (iso: string) => {
  const [anio, mes, dia] = iso.split('-');
  return `${dia}/${mes}/${anio}`;
};

/** Fecha de hoy del navegador en formato ISO, para precargar formularios. */
export const hoyIso = () => {
  const ahora = new Date();
  const mes = String(ahora.getMonth() + 1).padStart(2, '0');
  const dia = String(ahora.getDate()).padStart(2, '0');
  return `${ahora.getFullYear()}-${mes}-${dia}`;
};
