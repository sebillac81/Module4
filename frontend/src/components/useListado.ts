import { useCallback, useEffect, useState } from 'react';
import { ApiError } from '../api/client';

/** Carga un listado y permite recargarlo después de un alta. */
export function useListado<T>(cargar: () => Promise<T[]>) {
  const [filas, setFilas] = useState<T[]>([]);
  const [error, setError] = useState<string | null>(null);

  const recargar = useCallback(async () => {
    try {
      setFilas(await cargar());
      setError(null);
    } catch (e) {
      setError(e instanceof ApiError ? e.message : 'No se pudo cargar el listado.');
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  useEffect(() => {
    void recargar();
  }, [recargar]);

  return { filas, error, recargar };
}
