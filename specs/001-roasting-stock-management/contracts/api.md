# API Contract: RoastingManager v1

Base: `/api`. JSON con nombres en camelCase. Cantidades de café en kg (`number`, hasta 3
decimales) salvo `gramos` de presentaciones (entero). Montos con hasta 2 decimales.
Fechas como `YYYY-MM-DD`.

## Errores (research R9)

`application/problem+json`:

| Status | Cuándo | Extensiones |
|---|---|---|
| 400 | Validación (campo obligatorio, ≤ 0, decimales de más, rango, fecha futura, tope) | `errors: { campo: [mensaje] }` |
| 404 | Una referencia (`cafeVerdeId`, `clienteId`, …) o un recurso por id no existe | `campo` (cuando es una referencia del body) |
| 409 | Nombre duplicado | `campo: "nombre"` |
| 409 | Stock insuficiente | `disponibleKg`, `requeridoKg` |

Orden de validación: primero 400 (todos los errores de datos juntos), después 404 y por
último 409 por stock insuficiente.

## Catálogo

Alta (`POST`, responde 201 con el recurso creado) y consulta (`GET` del listado ordenado por
nombre, y `GET /{id}`). No hay `PUT`, `PATCH` ni `DELETE` en v1.

| Recurso | Ruta | Body de alta |
|---|---|---|
| Proveedor de café | `/api/proveedores-cafe` | `{ nombre, telefono?, email?, direccion? }` |
| Proveedor de insumos | `/api/proveedores-insumo` | `{ nombre, telefono?, email?, direccion? }` |
| Cliente | `/api/clientes` | `{ nombre, telefono?, email?, direccion? }` |
| Café verde | `/api/cafes-verdes` | `{ nombre, proveedorCafeId, precioKg, stockInicialKg }` |
| Insumo | `/api/insumos` | `{ nombre, proveedorInsumoId, precioUnitario }` |
| Presentación | `/api/presentaciones` | `{ nombre, gramos, insumoIds: [] }` |

Respuesta de café verde:
`{ id, nombre, proveedorCafe: { id, nombre }, precioKg, stockInicialKg }`.
Respuesta de presentación: `{ id, nombre, gramos, insumos: [{ id, nombre, precioUnitario }] }`.

## Ingresos de stock de café verde

- `POST /api/ingresos-stock` — `{ cafeVerdeId, cantidadKg, fecha? }` (si no se envía fecha,
  se toma hoy). Responde 201 con `{ id, cafeVerdeId, cafeVerde, cantidadKg, fecha,
  stockVerdeResultanteKg }`.
- `GET /api/ingresos-stock?cafeVerdeId=` — listado ordenado por fecha descendente y, a igual
  fecha, por orden de registro descendente.

## Batches

- `POST /api/batches` —
  `{ cafeVerdeId, verdeKg, tostadoKg, fecha?, humedadPct?, densidadGl?, desarrolloPct? }`.
  Responde 201 con el batch.
- `GET /api/batches` — historial ordenado por fecha descendente. `GET /api/batches/{id}`.

Batch:

```json
{
  "id": 1, "cafeVerdeId": 3, "cafeVerde": "Colombia Huila", "fecha": "2026-10-27",
  "verdeKg": 5.000, "tostadoKg": 4.250,
  "humedadPct": null, "densidadGl": null, "desarrolloPct": null,
  "precioKgAplicado": 20.00, "mermaPct": 15.00, "costoTostadoKg": 23.53, "costoTotal": 100.00
}
```

## Ventas

- `POST /api/ventas` —
  `{ clienteId, cafeVerdeId, presentacionId, unidades, precioUnitario, fecha? }`.
  Responde 201 con la venta.
- `GET /api/ventas` — historial ordenado por fecha descendente. `GET /api/ventas/{id}`.

Venta:

```json
{
  "id": 7, "fecha": "2026-10-27",
  "cliente": { "id": 2, "nombre": "Bar Central" },
  "cafeVerde": { "id": 3, "nombre": "Colombia Huila" },
  "presentacion": { "id": 1, "nombre": "Bolsa 250 g", "gramos": 250 },
  "unidades": 4, "precioUnitario": 15.00, "cantidadVendidaKg": 1.000,
  "costoPromedioKgAplicado": 22.00,
  "insumos": [ { "nombre": "Bolsa", "precioUnitarioAplicado": 0.50 },
               { "nombre": "Etiqueta", "precioUnitarioAplicado": 0.10 } ],
  "ingreso": 60.00, "costoCafeVendido": 22.00, "costosAsociados": 2.40,
  "ganancia": 35.60, "margenPct": 59.33
}
```

## Stock

- `GET /api/stock` — un ítem por café verde, ordenado por nombre:

```json
[ { "cafeVerdeId": 3, "cafeVerde": "Colombia Huila",
    "stockVerdeKg": 12.000, "stockTostadoKg": 0.000, "costoPromedioKg": 22.00 } ]
```

`costoPromedioKg` es `null` si el café nunca se tostó (la UI muestra "—").

## Configuración expuesta

- `GET /api/config/topes` — `{ ingresoVerdeMaxKg, batchVerdeMaxKg, ventaMaxUnidades }`. La UI
  lo usa solo para mostrar ayudas; la validación ocurre en el servidor.
