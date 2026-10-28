# Quickstart: validación end-to-end

**Feature**: 001-roasting-stock-management

Guía para comprobar que la feature funciona. Para formas de request/response, ver
[contracts/api.md](contracts/api.md); para reglas y fórmulas, ver
[data-model.md](data-model.md).

## Prerrequisitos

- Docker (con Docker Compose), .NET SDK 9, Node.js 22 con npm.
- `cp .env.example .env` y completar `POSTGRES_PASSWORD` (el archivo `.env` no se commitea).
- Exportar la connection string de la API, por ejemplo
  `export ConnectionStrings__RoastingManager="Host=localhost;Port=5432;Database=roasting;Username=roasting;Password=<la del .env>"`.

## Levantar

```bash
docker compose up -d                          # PostgreSQL
dotnet restore
dotnet run --project src/RoastingManager.Api  # aplica migraciones DbUp al iniciar
npm install
npm run dev                                   # frontend con proxy de /api al backend
```

## Tests automáticos

```bash
dotnet test   # Domain.Tests (cálculos, TDD) + Api.Tests (Testcontainers, requiere Docker)
npm test      # Vitest del frontend
```

Esperado: todo en verde. `Domain.Tests` cubre los ejemplos numéricos de la spec, y
`Api.Tests` incluye el test de concurrencia (SC-004) y el invariante de reconstrucción
(FR-031).

## Escenario manual (desde la UI o con HTTP)

Reproduce los ejemplos de la spec, en orden:

1. Crear un proveedor de café, "Finca Huila", y un café verde, "Colombia", con precio
   $20,00/kg y stock inicial 2 kg.
2. Registrar un ingreso de 8 kg de "Colombia". **Esperado:** stock verde de 10,000 kg.
3. Registrar un batch de "Colombia" con 5 kg de verde y 4,25 kg de tostado. **Esperado:**
   merma 15,00 %, costo del tostado $23,53/kg, costo total $100,00, stock verde 5,000 kg, stock tostado
   4,250 kg, costo promedio $23,53.
4. Intentar un batch con 6 kg de verde. **Esperado:** 409 "stock insuficiente" con
   disponible 5,000 kg y requerido 6,000 kg; el stock no cambia.
5. Crear un proveedor de insumos, los insumos "Bolsa" ($0,50) y "Etiqueta" ($0,10), la
   presentación "Bolsa 250 g" (250 g, con ambos insumos) y un cliente.
6. Vender 4 unidades de "Colombia" en "Bolsa 250 g" a $15,00. **Esperado:** stock tostado
   3,250 kg, ingreso $60,00, costo del café vendido $23,53, costos asociados $2,40,
   ganancia $34,07, margen 56,78 %.
7. Registrar otro batch de "Colombia". **Esperado:** la venta del paso 6 conserva sus
   valores.
8. Validaciones (todas responden 400 sin guardar nada): un batch con tostado ≥ verde, una
   fecha futura, un precio de 0,001, un batch de 150 kg (tope 100), un nombre duplicado con
   distinta capitalización (este último responde 409).
9. Consultar el stock: "Colombia" muestra stock verde, stock tostado y costo promedio.
   Vender todo el tostado y verificar que el costo promedio sigue mostrando el último valor.

## Topes configurables

Iniciar la API con `Topes__BatchVerdeMaxKg=150` y repetir el batch de 150 kg (con stock
suficiente). **Esperado:** se acepta.
