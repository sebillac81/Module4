interface Props {
  valor: string;
  onChange: (valor: string) => void;
  decimales?: number;
  'aria-label'?: string;
}

/** Input decimal (kg con hasta 3 decimales por defecto). Acepta coma o punto; la conversión a
 * número la hace {@link aNumero} al enviar, sin pasar a gramos: eso lo hace la API. */
export function CampoKg({ valor, onChange, decimales = 3, ...resto }: Props) {
  const patron = new RegExp(`^\\d*([.,]\\d{0,${decimales}})?$`);
  return (
    <input
      type="text"
      inputMode="decimal"
      value={valor}
      onChange={(e) => {
        if (patron.test(e.target.value)) onChange(e.target.value);
      }}
      {...resto}
    />
  );
}

/** "5,25" → 5.25; vacío → null. Es solo parseo del texto ingresado, no un cálculo. */
export const aNumero = (texto: string): number | null =>
  texto.trim() === '' ? null : Number(texto.replace(',', '.'));
