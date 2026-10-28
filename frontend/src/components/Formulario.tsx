import { createContext, FormEvent, ReactNode, useContext, useState } from 'react';
import { ApiError } from '../api/client';
import { formatearKg } from '../format/formato';

const ErroresContexto = createContext<Record<string, string[]>>({});

interface Props {
  titulo: string;
  textoBoton?: string;
  /** Envía los datos; si lanza ApiError, se muestran el mensaje general y los errores por campo. */
  enviar: () => Promise<void>;
  children: ReactNode;
}

export function Formulario({ titulo, textoBoton = 'Guardar', enviar, children }: Props) {
  const [error, setError] = useState<ApiError | null>(null);
  const [enviando, setEnviando] = useState(false);

  const alEnviar = async (e: FormEvent) => {
    e.preventDefault();
    setEnviando(true);
    setError(null);
    try {
      await enviar();
    } catch (err) {
      setError(err instanceof ApiError ? err : new ApiError(0, { detail: String(err) }));
    } finally {
      setEnviando(false);
    }
  };

  return (
    <form className="formulario" onSubmit={alEnviar} noValidate>
      <h2>{titulo}</h2>
      {error && (
        <div role="alert" className="error-general">
          {mensajeGeneral(error)}
        </div>
      )}
      <ErroresContexto.Provider value={error?.errors ?? {}}>{children}</ErroresContexto.Provider>
      <button type="submit" disabled={enviando}>
        {enviando ? 'Guardando…' : textoBoton}
      </button>
    </form>
  );
}

interface CampoProps {
  etiqueta: string;
  /** Nombre del campo en la API, para mostrar su error. */
  nombre: string;
  children: ReactNode;
}

export function Campo({ etiqueta, nombre, children }: CampoProps) {
  const errores = useContext(ErroresContexto)[nombre];
  return (
    <label className="campo">
      <span>{etiqueta}</span>
      {children}
      {errores?.map((m) => (
        <small key={m} className="error-campo">
          {m}
        </small>
      ))}
    </label>
  );
}

function mensajeGeneral(error: ApiError) {
  if (error.status === 400) return 'Revisá los datos marcados.';
  if (error.disponibleKg != null && error.requeridoKg != null) {
    return `No hay stock suficiente: disponible ${formatearKg(error.disponibleKg)}, requerido ${formatearKg(error.requeridoKg)}.`;
  }
  return error.message;
}
