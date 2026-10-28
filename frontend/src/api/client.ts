// Cliente HTTP mínimo. Convierte ProblemDetails en un ApiError tipado (research R9).

export class ApiError extends Error {
  status: number;
  errors: Record<string, string[]>;
  disponibleKg?: number;
  requeridoKg?: number;
  campo?: string;

  constructor(status: number, problema: Record<string, unknown>) {
    super(String(problema.detail ?? problema.title ?? `Error ${status}`));
    this.status = status;
    this.errors = (problema.errors as Record<string, string[]>) ?? {};
    this.disponibleKg = problema.disponibleKg as number | undefined;
    this.requeridoKg = problema.requeridoKg as number | undefined;
    this.campo = problema.campo as string | undefined;
    if (this.campo && !this.errors[this.campo]) {
      this.errors = { ...this.errors, [this.campo]: [this.message] };
    }
  }
}

async function pedir<T>(ruta: string, init?: RequestInit): Promise<T> {
  const respuesta = await fetch(ruta, {
    ...init,
    headers: { 'Content-Type': 'application/json', Accept: 'application/json' },
  });
  const texto = await respuesta.text();
  const cuerpo = texto ? JSON.parse(texto) : null;
  if (!respuesta.ok) {
    throw new ApiError(respuesta.status, cuerpo ?? {});
  }
  return cuerpo as T;
}

export const api = {
  get: <T>(ruta: string) => pedir<T>(ruta),
  post: <T>(ruta: string, body: unknown) =>
    pedir<T>(ruta, { method: 'POST', body: JSON.stringify(body) }),
};
