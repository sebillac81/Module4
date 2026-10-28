# Research: Gestión de Tuestes, Stock y Rentabilidad

**Feature**: 001-roasting-stock-management | **Date**: 2026-10-27

El stack viene fijado por `AGENTS.md` y la constitución (.NET 9 + EF Core/Npgsql, DbUp,
PostgreSQL vía Docker Compose, React + Vite con npm). Esta investigación resuelve las
decisiones de diseño que la spec deja abiertas para la implementación.

## R1. Estructura de la solución

- **Decision**: Solución .NET con tres proyectos de producción (`RoastingManager.Domain`,
  `RoastingManager.Infrastructure`, `RoastingManager.Api`) y dos de test
  (`RoastingManager.Domain.Tests`, `RoastingManager.Api.Tests`); frontend en `frontend/`
  con un `package.json` raíz que usa npm workspaces para que `npm install`, `npm run dev` y
  `npm test` funcionen desde la raíz, como indica `AGENTS.md`.
- **Rationale**: el principio III exige cálculos test-first y deterministas; aislarlos en
  `Domain` (sin EF ni ASP.NET) permite testearlos rápido y sin base. `Infrastructure` aísla
  EF Core y DbUp. Deja lugar a un futuro `RoastingManager.Ai` (principio I) sin tocar el
  dominio.
- **Alternatives considered**: un único proyecto Api (más simple, pero mezcla cálculos con
  acceso a datos y dificulta TDD puro); Clean Architecture completa con capa Application y
  CQRS (excesiva para el tamaño del curso).

## R2. Representación de cantidades y montos

- **Decision**: cantidades de café en **gramos enteros** (`long` en .NET, `bigint` en
  PostgreSQL). Montos en `decimal` / `numeric(12,2)`; porcentajes en `decimal` /
  `numeric(9,2)`. La API recibe y devuelve kg como número decimal con hasta 3 decimales
  (`cantidadKg`) y gramos enteros para presentaciones; la conversión kg↔g ocurre solo en el
  backend (`Cantidad.DesdeKg` / `Cantidad.AKg`).
- **Rationale**: FR-026 (gramos internos, exactos), principio V (tipos exactos, nada de
  `float`/`double`; el frontend no recalcula). System.Text.Json deserializa números JSON a
  `decimal` sin pérdida, así que `5.250` llega exacto.
- **Alternatives considered**: kg en `numeric(12,3)` (equivalente en exactitud pero pierde
  el invariante "entero = gramo"); gramos en la API (obligaría al frontend a convertir kg con
  aritmética de JS, contra el principio V).

## R3. Redondeo

- **Decision**: una única función `Redondeo.Aplicar(decimal)` en `Domain` que hace
  `Math.Round(valor, 2, MidpointRounding.AwayFromZero)`. Cada valor calculado se redondea al
  construirse el resultado y los cálculos siguientes reciben el valor ya redondeado
  (FR-024).
- **Rationale**: `AwayFromZero` es el redondeo comercial pedido (2,345 → 2,35;
  −2,345 → −2,35). El default de .NET es `ToEven` (bancario), que daría 2,34: hay que
  pasarlo explícito. Un único punto cumple el principio V.
- **Alternatives considered**: redondear en PostgreSQL (`round()` también aleja de cero, pero
  dispersa la regla entre capas).

## R4. Integridad de stock y concurrencia

- **Decision**: el stock verde, el stock tostado y el costo promedio ponderado de cada café
  viven en la fila de `cafe_verde`. Cada batch, ingreso o venta corre en una transacción que
  primero toma `SELECT ... FOR UPDATE` sobre la fila del café, valida stock y topes, calcula
  con el dominio, actualiza la fila e inserta el registro, y hace `COMMIT`. Además, la tabla
  tiene `CHECK (stock_verde_g >= 0 AND stock_tostado_g >= 0)` como última defensa.
- **Rationale**: principio IV y FR-022/FR-023: el bloqueo por fila serializa operaciones
  concurrentes sobre el mismo café (la segunda ve el stock ya descontado y se rechaza) sin
  bloquear a otros cafés. Con un solo bloqueo por operación no hay riesgo de deadlock.
- **Alternatives considered**: `UPDATE ... WHERE stock >= x` atómico (sirve para descontar,
  pero el costo promedio necesita leer y escribir); aislamiento `SERIALIZABLE` (requiere
  reintentos); concurrencia optimista con `xmin` (también requiere reintentos y complica los
  mensajes de error).

## R5. Migraciones y acceso a datos

- **Decision**: scripts SQL numerados (`0001_esquema_inicial.sql`, …) embebidos en
  `RoastingManager.Infrastructure/Migrations/`, ejecutados por DbUp al iniciar la API y
  también desde los tests de integración. EF Core mapea al esquema existente con
  `UseSnakeCaseNamingConvention` y no genera migraciones.
- **Rationale**: regla explícita de `AGENTS.md` y de la constitución (no usar
  `dotnet ef migrations`).
- **Alternatives considered**: ninguna; está fijado.

## R6. Unicidad de nombres

- **Decision**: el backend guarda el nombre con `Trim()`, y cada tabla de catálogo tiene un
  índice único `lower(nombre)`. Si se viola (`23505`), la API responde 409 con el mensaje
  "Ya existe un registro con ese nombre". Antes de insertar se hace una validación previa
  para dar el mensaje sin depender solo del error de la base.
- **Rationale**: FR-027. El índice garantiza la regla también ante altas simultáneas.
- **Alternatives considered**: extensión `citext` (agrega una dependencia sin ventaja real).

## R7. Topes configurables

- **Decision**: sección `Topes` en `appsettings.json` (`IngresoVerdeMaxKg: 1000`,
  `BatchVerdeMaxKg: 100`, `VentaMaxUnidades: 1000`) ligada con el patrón Options
  (`IOptions<TopesOptions>`). Se puede sobrescribir por archivo de entorno o por variable de
  entorno (`Topes__BatchVerdeMaxKg=150`).
- **Rationale**: FR-030 ("configurables sin modificar el sistema, con valores por defecto"),
  como pidió el usuario.
- **Alternatives considered**: tabla de configuración en la base (requiere una pantalla para
  administrarla, fuera de alcance).

## R8. Fechas

- **Decision**: `DateOnly` en .NET / `date` en PostgreSQL. "Hoy" se calcula en una zona
  horaria configurable (`App:ZonaHoraria`, por defecto `America/Argentina/Buenos_Aires`)
  mediante un `IReloj` inyectable para testear. El frontend precarga la fecha de hoy.
- **Rationale**: FR-028. Sin una zona explícita, un servidor en UTC rechazaría como "futura"
  una carga hecha a las 22 h en Argentina.
- **Alternatives considered**: usar la fecha del cliente (no se puede validar de forma
  confiable en el servidor).

## R9. Formato de errores de la API

- **Decision**: `ProblemDetails` (RFC 7807). 400 para validaciones, con un diccionario
  `errors` por campo; 404 para referencias inexistentes; 409 para nombre duplicado y para
  stock insuficiente, con extensiones `disponibleKg` y `requeridoKg`.
- **Rationale**: FR-025 (mensaje por campo y regla) y FR-010/FR-016 (mostrar disponible y
  requerido). Es el formato nativo de ASP.NET Core.
- **Alternatives considered**: 422 para reglas de negocio (válido; se prefirió 409 porque el
  conflicto es con el estado actual del stock).

## R10. Testing

- **Decision**: backend con xUnit. `Domain.Tests` son tests unitarios puros escritos antes
  de la implementación (TDD, principio III). `Api.Tests` usa `WebApplicationFactory` y
  Testcontainers (`Testcontainers.PostgreSql`) con un PostgreSQL real, e incluye un test de
  concurrencia con operaciones en paralelo. Frontend con Vitest y React Testing Library.
- **Rationale**: la integridad de stock (FOR UPDATE, CHECK, transacciones) solo se puede
  verificar contra PostgreSQL real; un proveedor en memoria no la reproduce.
- **Alternatives considered**: EF InMemory o SQLite (no soportan `FOR UPDATE` ni el mismo
  comportamiento transaccional).

## R11. Frontend

- **Decision**: React + Vite + TypeScript, React Router para la navegación, `fetch` con un
  cliente propio pequeño (`src/api/client.ts`), sin librerías de estado global. Los valores
  se formatean con `Intl.NumberFormat('es-AR')`; no hay aritmética de negocio. En desarrollo,
  Vite proxea `/api` al backend.
- **Rationale**: es la opción más simple que cumple el alcance (formularios de alta, listados
  y consultas). El principio V prohíbe recalcular en el frontend.
- **Alternatives considered**: React Query o Redux (innecesarios para este tamaño).

## R12. Secretos y configuración

- **Decision**: `docker-compose.yml` lee `POSTGRES_USER`, `POSTGRES_PASSWORD` y `POSTGRES_DB`
  de un `.env` (ignorado por git), con un `.env.example` versionado. La API lee
  `ConnectionStrings__RoastingManager` de variables de entorno o de user-secrets en
  desarrollo. `appsettings.json` no contiene credenciales.
- **Rationale**: principio VI.
- **Alternatives considered**: ninguna.
