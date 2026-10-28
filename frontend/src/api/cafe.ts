import { api } from './client';
import type { Contacto } from '../pages/Catalogo/CatalogoContactos';

export interface Referencia {
  id: number;
  nombre: string;
}

export interface CafeVerde {
  id: number;
  nombre: string;
  proveedorCafe: Referencia;
  precioKg: number;
  stockInicialKg: number;
}

export interface Ingreso {
  id: number;
  cafeVerdeId: number;
  cafeVerde: string;
  cantidadKg: number;
  fecha: string;
  stockVerdeResultanteKg: number | null;
}

export interface Batch {
  id: number;
  cafeVerdeId: number;
  cafeVerde: string;
  fecha: string;
  verdeKg: number;
  tostadoKg: number;
  humedadPct: number | null;
  densidadGl: number | null;
  desarrolloPct: number | null;
  precioKgAplicado: number;
  mermaPct: number;
  costoTostadoKg: number;
  costoTotal: number;
}

export interface Stock {
  cafeVerdeId: number;
  cafeVerde: string;
  stockVerdeKg: number;
  stockTostadoKg: number;
  costoPromedioKg: number | null;
}

export interface Topes {
  ingresoVerdeMaxKg: number;
  batchVerdeMaxKg: number;
  ventaMaxUnidades: number;
}

export const listarProveedoresCafe = () => api.get<Contacto[]>('/api/proveedores-cafe');

export const listarCafesVerdes = () => api.get<CafeVerde[]>('/api/cafes-verdes');
export const crearCafeVerde = (body: {
  nombre: string;
  proveedorCafeId: number | null;
  precioKg: number | null;
  stockInicialKg: number | null;
}) => api.post<CafeVerde>('/api/cafes-verdes', body);

export const listarIngresos = (cafeVerdeId?: number) =>
  api.get<Ingreso[]>(cafeVerdeId ? `/api/ingresos-stock?cafeVerdeId=${cafeVerdeId}` : '/api/ingresos-stock');
export const crearIngreso = (body: { cafeVerdeId: number | null; cantidadKg: number | null; fecha: string }) =>
  api.post<Ingreso>('/api/ingresos-stock', body);

export const listarBatches = () => api.get<Batch[]>('/api/batches');
export const crearBatch = (body: {
  cafeVerdeId: number | null;
  verdeKg: number | null;
  tostadoKg: number | null;
  fecha: string;
  humedadPct: number | null;
  densidadGl: number | null;
  desarrolloPct: number | null;
}) => api.post<Batch>('/api/batches', body);

export const consultarStock = () => api.get<Stock[]>('/api/stock');
export const consultarTopes = () => api.get<Topes>('/api/config/topes');
