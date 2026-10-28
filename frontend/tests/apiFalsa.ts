import { vi } from 'vitest';

type Respuesta = { status: number; body: unknown };

/** Reemplaza fetch: responde según "MÉTODO ruta" y registra los bodies enviados. */
export function apiFalsa(rutas: Record<string, Respuesta | (() => Respuesta)>) {
  const enviados: { ruta: string; body: unknown }[] = [];
  const fetchFalso = vi.fn(async (ruta: string, init?: RequestInit) => {
    const metodo = init?.method ?? 'GET';
    if (init?.body) enviados.push({ ruta, body: JSON.parse(String(init.body)) });
    const definida = rutas[`${metodo} ${ruta}`];
    const r = typeof definida === 'function' ? definida() : definida ?? { status: 404, body: {} };
    return new Response(JSON.stringify(r.body), { status: r.status });
  });
  vi.stubGlobal('fetch', fetchFalso);
  return { enviados };
}
