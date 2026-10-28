import { api } from './client';
import type { Referencia } from './cafe';
import type { Contacto } from '../pages/Catalogo/CatalogoContactos';

export interface Insumo {
  id: number;
  nombre: string;
  proveedorInsumo: Referencia;
  precioUnitario: number;
}

export interface Presentacion {
  id: number;
  nombre: string;
  gramos: number;
  insumos: { id: number; nombre: string; precioUnitario: number }[];
}

export interface Venta {
  id: number;
  fecha: string;
  cliente: Referencia;
  cafeVerde: Referencia;
  presentacion: { id: number; nombre: string; gramos: number };
  unidades: number;
  precioUnitario: number;
  cantidadVendidaKg: number;
  costoPromedioKgAplicado: number;
  insumos: { nombre: string; precioUnitarioAplicado: number }[];
  ingreso: number;
  costoCafeVendido: number;
  costosAsociados: number;
  ganancia: number;
  margenPct: number;
}

export const listarProveedoresInsumo = () => api.get<Contacto[]>('/api/proveedores-insumo');
export const listarClientes = () => api.get<Contacto[]>('/api/clientes');

export const listarInsumos = () => api.get<Insumo[]>('/api/insumos');
export const crearInsumo = (body: { nombre: string; proveedorInsumoId: number | null; precioUnitario: number | null }) =>
  api.post<Insumo>('/api/insumos', body);

export const listarPresentaciones = () => api.get<Presentacion[]>('/api/presentaciones');
export const crearPresentacion = (body: { nombre: string; gramos: number | null; insumoIds: number[] }) =>
  api.post<Presentacion>('/api/presentaciones', body);

export const listarVentas = () => api.get<Venta[]>('/api/ventas');
export const crearVenta = (body: {
  clienteId: number | null;
  cafeVerdeId: number | null;
  presentacionId: number | null;
  unidades: number | null;
  precioUnitario: number | null;
  fecha: string;
}) => api.post<Venta>('/api/ventas', body);
