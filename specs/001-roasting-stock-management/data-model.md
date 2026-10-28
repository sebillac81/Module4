# Data Model: Gestión de Tuestes, Stock y Rentabilidad

**Feature**: 001-roasting-stock-management | **Date**: 2026-10-27

Convenciones (ver research.md R2, R3, R6):

- Cantidades de café: gramos enteros (`bigint`, sufijo `_g`). La API las expone en kg.
- Montos: `numeric(12,2)`. Porcentajes: `numeric(9,2)`. Todos redondeados con
  `Redondeo.Aplicar` (AwayFromZero, 2 decimales) antes de guardarse.
- Nombres: `varchar(120)`, guardados con trim, y con índice único `lower(nombre)` por tabla.
- Todas las tablas tienen `id bigint generated always as identity` y
  `creado_en timestamptz not null default now()`. `creado_en` define el orden de registro
  (FR-028).
- Sin edición ni borrado en v1: no hay `actualizado_en` ni borrado lógico.

## Entidades de catálogo

### proveedor_cafe / proveedor_insumo / cliente

Las tres tablas tienen la misma forma.

| Campo | Tipo | Reglas |
|---|---|---|
| nombre | varchar(120) | obligatorio, único (case-insensitive, trim) — FR-027 |
| telefono | varchar(40) | opcional |
| email | varchar(160) | opcional, formato email si se informa |
| direccion | varchar(200) | opcional |

### cafe_verde

| Campo | Tipo | Reglas |
|---|---|---|
| nombre | varchar(120) | obligatorio, único — FR-027 |
| proveedor_cafe_id | bigint FK → proveedor_cafe | obligatorio — FR-003 |
| precio_kg | numeric(12,2) | ≥ 0,01, hasta 2 decimales — FR-003, FR-029 |
| stock_inicial_g | bigint | ≥ 0, ≤ tope `IngresoVerdeMaxKg`, inmutable — FR-007, FR-030 |
| stock_verde_g | bigint | `CHECK ≥ 0` — FR-022 |
| stock_tostado_g | bigint | `CHECK ≥ 0` — FR-015, FR-022 |
| costo_promedio_kg | numeric(12,2) null | null = nunca tostado ("—"); se conserva cuando el stock tostado llega a 0 — FR-013, FR-019 |

Esta fila es la **unidad de bloqueo** (`SELECT … FOR UPDATE`) de toda operación de stock
sobre el café (research R4). La fila contiene el stock de café verde y el pool de café
tostado de ese café; en el modelo conceptual de la spec, "Stock de café tostado" es una
entidad aparte, pero acá se guarda en la misma fila del café.

### insumo

| Campo | Tipo | Reglas |
|---|---|---|
| nombre | varchar(120) | obligatorio, único |
| proveedor_insumo_id | bigint FK → proveedor_insumo | obligatorio — FR-004 |
| precio_unitario | numeric(12,2) | ≥ 0,01 — FR-004, FR-029 |

### presentacion

| Campo | Tipo | Reglas |
|---|---|---|
| nombre | varchar(120) | obligatorio, único |
| gramos | integer | > 0 — FR-005 |

### presentacion_insumo

| Campo | Tipo | Reglas |
|---|---|---|
| presentacion_id | bigint FK | PK compuesta |
| insumo_id | bigint FK | PK compuesta; cada insumo aparece una sola vez por presentación (una unidad por unidad vendida) |

## Movimientos

### ingreso_stock (FR-007a, FR-007b)

| Campo | Tipo | Reglas |
|---|---|---|
| cafe_verde_id | bigint FK | obligatorio |
| cantidad_g | bigint | > 0 y ≤ tope `IngresoVerdeMaxKg` — FR-030 |
| fecha | date | ≤ hoy (zona configurada) — FR-028 |

Efecto: `cafe_verde.stock_verde_g += cantidad_g`.

### batch (FR-008 a FR-013)

| Campo | Tipo | Reglas |
|---|---|---|
| cafe_verde_id | bigint FK | obligatorio |
| verde_g | bigint | > 0, ≤ stock_verde_g, ≤ tope `BatchVerdeMaxKg` |
| tostado_g | bigint | > 0, `CHECK tostado_g < verde_g` |
| fecha | date | ≤ hoy |
| humedad_pct | numeric(5,2) null | 0–100, hasta 2 decimales (más decimales se rechazan) |
| densidad_gl | numeric(8,2) null | > 0, hasta 2 decimales |
| desarrollo_pct | numeric(5,2) null | 0–100, hasta 2 decimales |
| precio_kg_aplicado | numeric(12,2) | copia de `cafe_verde.precio_kg` al registrar — FR-012 |
| merma_pct | numeric(9,2) | calculado — FR-011 |
| costo_tostado_kg | numeric(12,2) | calculado — FR-012 |
| costo_total | numeric(12,2) | calculado — FR-012a |

Efectos sobre `cafe_verde`: `stock_verde_g -= verde_g`, `stock_tostado_g += tostado_g`,
`costo_promedio_kg = nuevo costo promedio`.

### venta (FR-014 a FR-018)

| Campo | Tipo | Reglas |
|---|---|---|
| cliente_id | bigint FK | obligatorio |
| cafe_verde_id | bigint FK | obligatorio (café vendido) |
| presentacion_id | bigint FK | obligatorio |
| unidades | integer | > 0, ≤ tope `VentaMaxUnidades` |
| precio_unitario | numeric(12,2) | ≥ 0,01 |
| fecha | date | ≤ hoy |
| gramos_presentacion | integer | copia de `presentacion.gramos` |
| gramos_vendidos | bigint | unidades × gramos_presentacion; ≤ stock_tostado_g |
| costo_promedio_kg_aplicado | numeric(12,2) | copia del costo promedio del café al registrar |
| ingreso | numeric(12,2) | calculado |
| costo_cafe_vendido | numeric(12,2) | calculado |
| costos_asociados | numeric(12,2) | calculado |
| ganancia | numeric(12,2) | calculado (puede ser negativo) |
| margen_pct | numeric(9,2) | calculado (puede ser negativo) |

Efecto: `cafe_verde.stock_tostado_g -= gramos_vendidos`. No modifica `costo_promedio_kg`
(FR-015).

### venta_insumo (precios congelados, FR-017)

| Campo | Tipo | Reglas |
|---|---|---|
| venta_id | bigint FK | PK compuesta |
| insumo_id | bigint FK | PK compuesta |
| nombre_insumo | varchar(120) | copia, para mostrar |
| precio_unitario_aplicado | numeric(12,2) | copia del precio vigente |

## Orden de validación (FR-025)

Primero se validan los datos (obligatorios, > 0, decimales, rangos, fecha, topes) y se
informan todos juntos (400). Después se verifican las referencias (404, indicando el campo).
Recién con datos válidos se bloquea la fila del café y se controla el stock (409).

## Cálculos de dominio (`RoastingManager.Domain`, TDD — principio III)

R = `Redondeo.Aplicar` (2 decimales, AwayFromZero). Cada resultado se redondea antes de
usarse en el siguiente cálculo.

| Cálculo | Fórmula | Requisito |
|---|---|---|
| Merma | R((verde_g − tostado_g) / verde_g × 100) | FR-011 |
| Costo del tostado ($/kg) | R(precio_kg × verde_g / tostado_g) | FR-012 |
| Costo total del batch ($) | R(precio_kg × verde_g / 1000) | FR-012a |
| Nuevo costo promedio ($/kg) | R((stock_g × cpp + tostado_g × costo_tostado) / (stock_g + tostado_g)); si stock_g = 0, el resultado es costo_tostado | FR-013 |
| Ingreso | R(precio_unitario × unidades) | FR-018 |
| Costo café vendido | R(cpp × gramos_vendidos / 1000) | FR-018 |
| Costos asociados | R(unidades × Σ precio_unitario de insumos) | FR-017 |
| Ganancia | R(ingreso − (costo_cafe_vendido + costos_asociados)) | FR-018 |
| Margen | R(ganancia / ingreso × 100) | FR-018 |
| kg → g | kg × 1000, exacto; se rechaza si tiene más de 3 decimales | FR-026 |

## Invariante de reconstrucción (FR-031)

```
stock_verde_g   = stock_inicial_g + Σ ingreso_stock.cantidad_g − Σ batch.verde_g
stock_tostado_g = Σ batch.tostado_g − Σ venta.gramos_vendidos
```

Un test de integración verifica este invariante después de secuencias de operaciones,
incluidas las concurrentes.

## Transiciones de estado

Las entidades no tienen estados. Lo único que evoluciona es el saldo de cada `cafe_verde`
(stock verde, stock tostado, costo promedio), mediante las tres operaciones transaccionales
de arriba. `costo_promedio_kg` pasa de null a un valor con el primer batch y nunca vuelve a
null.
