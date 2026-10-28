# Implementation Plan: Gestión de Tuestes, Stock y Rentabilidad

**Branch**: `001-roasting-stock-management` (sin rama git; se trabaja en `main`) | **Date**: 2026-10-27 | **Spec**: [spec.md](spec.md)

**Input**: Feature specification from `/specs/001-roasting-stock-management/spec.md`

## Summary

Aplicación web interna para que una tostaduría registre su catálogo (proveedores, cafés
verdes, insumos, presentaciones, clientes), los ingresos de stock de café verde, los batches
de tueste y las ventas. Calcula automáticamente la merma, el costo del tostado, el costo
promedio ponderado por café, los costos asociados, la ganancia y el margen. El enfoque
técnico tiene cinco pilares:

- Cálculos puros en un proyecto `Domain`, desarrollado con TDD.
- Cantidades en gramos enteros y montos en `decimal`, con redondeo AwayFromZero en un único
  punto.
- Operaciones de stock transaccionales con bloqueo por fila del café y `CHECK ≥ 0`.
- Esquema gestionado con DbUp.
- API REST consumida por un frontend React que solo formatea, sin calcular.

## Technical Context

**Language/Version**: C# 13 / .NET 9 (SDK 9.0.313); TypeScript 5 / Node.js 22

**Primary Dependencies**: ASP.NET Core 9 (Minimal APIs), EF Core 9 +
Npgsql.EntityFrameworkCore.PostgreSQL + EFCore.NamingConventions, dbup-postgresql; React 18,
Vite, React Router

**Storage**: PostgreSQL 16 vía Docker Compose; esquema con DbUp (scripts SQL embebidos)

**Testing**: xUnit, Testcontainers.PostgreSql y Microsoft.AspNetCore.Mvc.Testing (backend);
Vitest y React Testing Library (frontend)

**Target Platform**: servidor Linux/Windows con Docker; navegador de escritorio moderno

**Project Type**: aplicación web (API + SPA)

**Performance Goals**: p95 < 3 s en operaciones principales con 5 usuarios concurrentes
(RNF-01, SC-005); trivial para este volumen

**Constraints**: stock nunca negativo, incluso con operaciones concurrentes; cálculos
reproducibles al centavo; sin secretos en el repositorio; zona horaria configurable para
"hoy"

**Scale/Scope**: 1 tostaduría, 2 personas, ~5 usuarios concurrentes; decenas de cafés y
miles de batches y ventas; ~9 pantallas

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| # | Principio | Evaluación | Estado |
|---|---|---|---|
| I | IA aislada | La feature no incluye IA. La estructura deja lugar a un futuro proyecto `RoastingManager.Ai` sin dependencias desde `Domain` | ✅ N/A |
| II | Análisis de IA basado en datos | Sin IA en esta feature | ✅ N/A |
| III | Cálculos test-first | Todos los cálculos viven en `RoastingManager.Domain` (sin dependencias de infraestructura); `tasks.md` pone los tests de `Domain.Tests` antes de cada implementación | ✅ |
| IV | Integridad de stock | Transacción por operación + `SELECT … FOR UPDATE` sobre la fila del café + `CHECK ≥ 0` + test de concurrencia contra PostgreSQL real (research R4, R10) | ✅ |
| V | Precisión monetaria y de pesos | Gramos en `long`/`bigint` (entero exacto: 1 unidad = 1 g, sin parte fraccionaria que perder), montos en `decimal`/`numeric`, sin `float`/`double`; `Redondeo.Aplicar` único; el frontend solo formatea (R2, R3, R11) | ✅ |
| VI | Sin secretos | `.env` ignorado + `.env.example`; connection string por variable de entorno o user-secrets (R12) | ✅ |
| VII | Alcance acotado al PRD | Todo traza a RF-01..RF-25 de `PRD2.md`; sin edición ni borrado, login, FIFO ni blends | ✅ |
| — | Restricciones técnicas | .NET 9, EF Core + Npgsql, DbUp (sin `dotnet ef migrations`), PostgreSQL, React + Vite con npm | ✅ |

**Re-check post-design**: ✅ sin violaciones. `data-model.md` y `contracts/api.md` mantienen
los gramos internos, los montos `numeric`, la unidad de bloqueo por café, y no exponen
operaciones fuera del PRD.

## Project Structure

### Documentation (this feature)

```text
specs/001-roasting-stock-management/
├── plan.md              # This file
├── research.md          # Phase 0: decisiones técnicas R1–R12
├── data-model.md        # Phase 1: tablas, reglas, fórmulas, invariantes
├── quickstart.md        # Phase 1: guía de validación end-to-end
├── contracts/
│   └── api.md           # Phase 1: contrato REST
├── checklists/          # requirements.md, calculos-stock.md
└── tasks.md             # Phase 2 (/speckit-tasks)
```

### Source Code (repository root)

```text
RoastingManager.sln
docker-compose.yml
.env.example
package.json                         # npm workspaces → frontend (dev, test)

src/
├── RoastingManager.Domain/          # sin dependencias externas
│   ├── Calculos/                    # Redondeo, Cantidad (kg↔g), CalculoBatch,
│   │                                #   CalculoCostoPromedio, CalculoVenta
│   ├── Reglas/                      # validaciones de dominio (rangos, topes, fechas)
│   └── Errores/                     # StockInsuficienteException, ValidacionException
├── RoastingManager.Infrastructure/
│   ├── Migrations/                  # 0001_esquema_inicial.sql … (DbUp, embebidos)
│   ├── Persistence/                 # RoastingDbContext, entidades EF, configuraciones
│   ├── Migrador.cs                  # ejecuta DbUp
│   └── Operaciones/                 # RegistrarBatch, RegistrarVenta, RegistrarIngreso
│                                    #   (transacción + FOR UPDATE + dominio)
└── RoastingManager.Api/
    ├── Endpoints/                   # Catalogo, Ingresos, Batches, Ventas, Stock, Config
    ├── Contracts/                   # DTOs request/response
    ├── Configuracion/               # TopesOptions, AppOptions (zona horaria), IReloj
    ├── ErrorHandling/               # mapeo a ProblemDetails (400/404/409)
    ├── appsettings.json
    └── Program.cs

tests/
├── RoastingManager.Domain.Tests/    # TDD de cálculos y reglas (unit)
└── RoastingManager.Api.Tests/       # integración HTTP + PostgreSQL (Testcontainers)

frontend/
├── package.json
├── vite.config.ts                   # proxy /api → http://localhost:5080 (puerto fijo en launchSettings)
├── src/
│   ├── api/                         # client.ts + funciones por recurso
│   ├── components/                  # Formulario, Tabla, ErrorDeCampo, CampoKg
│   ├── format/                      # formateo es-AR de $, %, kg (sin aritmética)
│   └── pages/                       # Catalogo/*, Ingresos, Batches, Ventas, Stock
└── tests/
```

**Structure Decision**: aplicación web con backend .NET en `src/` (respetando el comando
`dotnet run --project src/RoastingManager.Api` de `AGENTS.md`) y frontend en `frontend/`,
expuesto en la raíz mediante npm workspaces para que `npm install`, `npm run dev` y
`npm test` funcionen desde la raíz. El backend se divide en Domain / Infrastructure / Api
para aislar los cálculos (principio III) y dejar lugar al futuro módulo de IA (principio I).

## Complexity Tracking

Sin violaciones de la constitución que justificar.
