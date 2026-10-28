---

description: "Task list for 001-roasting-stock-management"
---

# Tasks: Gestión de Tuestes, Stock y Rentabilidad

**Input**: Design documents from `/specs/001-roasting-stock-management/`

**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/api.md, quickstart.md

**Tests**: INCLUIDOS. La constitución (principio III, NO NEGOCIABLE) exige TDD para los
cálculos de dominio: cada test de `RoastingManager.Domain.Tests` se escribe y se ve fallar
antes de su implementación. El principio IV exige verificar la integridad de stock contra
PostgreSQL real (`RoastingManager.Api.Tests` con Testcontainers).

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (US1, US2, US3)
- Include exact file paths in descriptions

## Path Conventions

- Backend: `src/RoastingManager.{Domain,Infrastructure,Api}/`, tests en `tests/`
- Frontend: `frontend/src/`, tests en `frontend/tests/`
- Referencias: fórmulas y columnas en `data-model.md`; rutas y JSON en `contracts/api.md`;
  decisiones técnicas en `research.md` (R1–R12)

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Inicializar la solución, el entorno de base de datos y el frontend

- [X] T001 Crear `RoastingManager.sln` y los proyectos `src/RoastingManager.Domain` (classlib, net9.0, sin paquetes), `src/RoastingManager.Infrastructure` (classlib), `src/RoastingManager.Api` (web, Minimal APIs), `tests/RoastingManager.Domain.Tests` y `tests/RoastingManager.Api.Tests` (xUnit), con referencias Api→Infrastructure→Domain, Domain.Tests→Domain y Api.Tests→Api; fijar en `src/RoastingManager.Api/Properties/launchSettings.json` el puerto http 5080
- [X] T002 Agregar paquetes NuGet: en `src/RoastingManager.Infrastructure/RoastingManager.Infrastructure.csproj` Npgsql.EntityFrameworkCore.PostgreSQL 9.x, EFCore.NamingConventions 9.x y dbup-postgresql; en `tests/RoastingManager.Api.Tests/RoastingManager.Api.Tests.csproj` Microsoft.AspNetCore.Mvc.Testing 9.x y Testcontainers.PostgreSql
- [X] T003 [P] Crear `docker-compose.yml` con un servicio `postgres:16` (puerto 5432, volumen nombrado) que lea `POSTGRES_USER`, `POSTGRES_PASSWORD` y `POSTGRES_DB` del `.env`, y crear `.env.example` con esas claves sin valores reales (research R12)
- [X] T004 [P] Crear `.gitignore` para .NET, Node, `.env` y `bin/`/`obj/`/`node_modules/`/`dist/`
- [X] T005 [P] Crear el frontend con Vite (plantilla react-ts) en `frontend/`, agregar react-router-dom, vitest, @testing-library/react, @testing-library/jest-dom y jsdom; configurar `frontend/vite.config.ts` con un proxy de `/api` a `http://localhost:5080` y el entorno jsdom de Vitest
- [X] T006 [P] Crear `package.json` raíz con `"workspaces": ["frontend"]` y scripts `dev` (`npm run dev -w frontend`) y `test` (`npm test -w frontend`), para que los comandos de `AGENTS.md` funcionen desde la raíz

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Núcleo compartido por todas las historias: redondeo, cantidades, reglas comunes, esquema, persistencia base, manejo de errores, configuración y base del frontend

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

### Tests primero (TDD — principio III)

- [X] T007 [P] Escribir tests de `Redondeo.Aplicar` en `tests/RoastingManager.Domain.Tests/Calculos/RedondeoTests.cs`: 2,345→2,35; 2,344→2,34; −2,345→−2,35; 23,5294→23,53; un valor ya redondeado queda igual (FR-024, research R3). Verificar que fallan
- [X] T008 [P] Escribir tests de `Cantidad` en `tests/RoastingManager.Domain.Tests/Calculos/CantidadTests.cs`: `DesdeKg(5.250m)`=5250 g; `DesdeKg(69m)`=69000 g; `DesdeKg(0.0005m)` lanza ValidacionException (más de 3 decimales); `AKg(4250)`=4.250m (FR-026). Verificar que fallan
- [X] T009 [P] Escribir tests de reglas comunes en `tests/RoastingManager.Domain.Tests/Reglas/ReglasComunesTests.cs`: monto con 3 decimales o < 0,01 se rechaza y 0,01 se acepta (FR-029); fecha mañana se rechaza y hoy/ayer se aceptan según un `hoy` dado (FR-028); `NormalizarNombre("  Colombia ")`="Colombia" y nombre vacío se rechaza (FR-027); un valor sobre el tope se rechaza con el tope en el mensaje (FR-030). Verificar que fallan

### Implementación

- [X] T010 [P] Crear `ValidacionException` (campo + mensaje) y `StockInsuficienteException` (disponibleG, requeridoG) y `NoEncontradoException` (campo opcional + mensaje) en `src/RoastingManager.Domain/Errores/`
- [X] T011 Implementar `Redondeo.Aplicar` con `Math.Round(v, 2, MidpointRounding.AwayFromZero)` en `src/RoastingManager.Domain/Calculos/Redondeo.cs` (hace pasar T007)
- [X] T012 Implementar `Cantidad.DesdeKg(decimal kg)`→`long` y `Cantidad.AKg(long g)`→`decimal` en `src/RoastingManager.Domain/Calculos/Cantidad.cs` (hace pasar T008)
- [X] T013 Implementar `ReglasComunes` (ValidarMonto, ValidarFechaNoFutura(fecha, hoy), NormalizarNombre, ValidarTope) en `src/RoastingManager.Domain/Reglas/ReglasComunes.cs` (hace pasar T009)
- [X] T014 Escribir el esquema completo de `data-model.md` en `src/RoastingManager.Infrastructure/Migrations/0001_esquema_inicial.sql`: todas las tablas, FKs, CHECKs (stock ≥ 0, tostado_g < verde_g, cantidades > 0, precios ≥ 0,01, rangos de datos de proceso) e índices únicos `lower(nombre)` por tabla de catálogo; marcar los `.sql` como EmbeddedResource en el `.csproj`
- [X] T015 Implementar `Migrador.Ejecutar(connectionString)` con DbUp (EnsureDatabase + scripts embebidos, falla si el upgrade no tiene éxito) en `src/RoastingManager.Infrastructure/Migrador.cs`
- [X] T016 Crear `RoastingDbContext` con `UseSnakeCaseNamingConvention()` y sin migraciones EF, en `src/RoastingManager.Infrastructure/Persistence/RoastingDbContext.cs`; las entidades se agregan en cada historia
- [X] T017 [P] Crear `TopesOptions` (IngresoVerdeMaxKg=1000, BatchVerdeMaxKg=100, VentaMaxUnidades=1000), `AppOptions` (ZonaHoraria="America/Argentina/Buenos_Aires"), `IReloj` y `RelojSistema` (Hoy() en la zona configurada) en `src/RoastingManager.Api/Configuracion/`, y las secciones `Topes` y `App` en `src/RoastingManager.Api/appsettings.json`, sin connection string (research R7, R8, R12)
- [X] T018 [P] Implementar el manejo de errores en `src/RoastingManager.Api/ErrorHandling/ManejadorErrores.cs`: ValidacionException→400 con `errors[campo]`; StockInsuficienteException→409 con `disponibleKg`/`requeridoKg`; violación única `23505`→409 "Ya existe un registro con ese nombre"; NoEncontradoException y violación de FK `23503`→404 con `campo` (research R9)
- [X] T019 Configurar `src/RoastingManager.Api/Program.cs`: leer `ConnectionStrings:RoastingManager` (variable de entorno o user-secrets), ejecutar `Migrador.Ejecutar` al iniciar, registrar DbContext, Options, IReloj, ProblemDetails y el manejador de errores, exponer `GET /api/config/topes` y la clase `public partial class Program` para los tests
- [X] T020 Crear el fixture de integración en `tests/RoastingManager.Api.Tests/Infra/ApiFixture.cs`: contenedor PostgreSQL de Testcontainers, `WebApplicationFactory<Program>` con la connection string del contenedor, un `IReloj` fijo reemplazable, y un helper para limpiar tablas entre tests
- [X] T021 [P] Crear la base del frontend: `frontend/src/api/client.ts` (fetch JSON que convierte ProblemDetails en un error tipado con `errors`, `disponibleKg`, `requeridoKg`), `frontend/src/format/formato.ts` (formatear $, % y kg con `Intl.NumberFormat('es-AR')`, sin aritmética), y `frontend/src/App.tsx` con el router y un menú de navegación
- [X] T022 [P] Crear componentes comunes en `frontend/src/components/`: `Formulario.tsx` (muestra errores por campo y el mensaje general), `Tabla.tsx` (con estado vacío "No hay registros"), `CampoKg.tsx` (input decimal de hasta 3 decimales, enviado como número sin conversiones) y `CampoFecha.tsx` (precargado con hoy, sin fechas futuras)
- [X] T023 [P] Crear un componente reutilizable de catálogo de contactos (nombre, teléfono, email, dirección) con alta y listado en `frontend/src/pages/Catalogo/CatalogoContactos.tsx`, parametrizado por ruta de API y título

**Checkpoint**: `dotnet test` corre los tests de T007–T009 en verde, la API inicia y aplica la migración, y el frontend compila con navegación vacía

---

## Phase 3: User Story 1 - Registrar un batch de tueste con stock y costo automáticos (Priority: P1) 🎯 MVP

**Goal**: Registrar proveedores de café, cafés verdes con stock inicial, ingresos de stock y batches, con descuento y suma de stock, merma, costo del tostado y costo promedio ponderado

**Independent Test**: Crear un proveedor de café y el café "Colombia" ($20,00/kg, 2 kg), ingresar 8 kg, registrar un batch de 5 kg de verde y 4,25 kg de tostado, y consultar `GET /api/stock`: verde 5,000 kg, tostado 4,250 kg, merma 15,00 %, costo $23,53/kg, costo promedio $23,53

### Tests for User Story 1 (escribir primero y verificar que fallan) ⚠️

- [X] T024 [P] [US1] Escribir tests de `CalculoBatch` en `tests/RoastingManager.Domain.Tests/Calculos/CalculoBatchTests.cs`: 5000 g verde, 4250 g tostado, $20,00/kg → merma 15,00, costo 23,53/kg y costo total 100,00; tostado ≥ verde → ValidacionException; verde o tostado ≤ 0 → ValidacionException; caso de 1 g de tostado (FR-008, FR-011, FR-012, CHK028)
- [X] T025 [P] [US1] Escribir tests de `CalculoCostoPromedio` en `tests/RoastingManager.Domain.Tests/Calculos/CalculoCostoPromedioTests.cs`: stock 4000 g a $20,00 + batch 2000 g a $26,00 → 22,00; stock 0 (con o sin costo previo) → costo del batch; entrada redondeada 23,53 usada tal cual (FR-013, FR-024)
- [X] T026 [P] [US1] Escribir tests de reglas de batch e ingreso en `tests/RoastingManager.Domain.Tests/Reglas/ReglasBatchTests.cs`: humedad y desarrollo fuera de 0–100 o densidad ≤ 0 o cualquier dato con más de 2 decimales se rechazan, null se acepta (FR-008); verde > stock → StockInsuficienteException con disponible y requerido (FR-010); verde > tope de batch o ingreso > tope de ingreso → ValidacionException (FR-030)
- [X] T027 [P] [US1] Escribir tests de integración en `tests/RoastingManager.Api.Tests/US1/CafeVerdeEIngresosTests.cs`: alta de proveedor de café y café verde (201); precio ≤ 0 o con 3 decimales → 400; nombre duplicado con otra capitalización → 409; ingreso de 8 kg suma stock; ingreso ≤ 0 o con fecha futura → 400; historial de ingresos ordenado por fecha descendente
- [X] T028 [P] [US1] Escribir tests de integración en `tests/RoastingManager.Api.Tests/US1/BatchesTests.cs`: el escenario del Independent Test; batch sin stock → 409 con `disponibleKg`/`requeridoKg` y stock intacto; el batch guarda y devuelve `precioKgAplicado`; dos batches seguidos actualizan el costo promedio; un batch de 150 kg sobre 5 kg de stock responde 400 (tope) y no 409 (FR-025)
- [X] T029 [P] [US1] Escribir el test de concurrencia en `tests/RoastingManager.Api.Tests/US1/ConcurrenciaBatchTests.cs`: 5 batches simultáneos de 3 kg sobre un café con 10 kg → exactamente 3 aceptados, 2 con 409, stock verde final 1 kg, nunca negativo (FR-022, SC-004)

### Implementation for User Story 1

- [X] T030 [P] [US1] Implementar `CalculoBatch.Calcular(verdeG, tostadoG, precioKg)` → (MermaPct, CostoTostadoKg, CostoTotal) en `src/RoastingManager.Domain/Calculos/CalculoBatch.cs` (hace pasar T024)
- [X] T031 [P] [US1] Implementar `CalculoCostoPromedio.Calcular(stockG, costoPromedioActual?, tostadoG, costoTostadoKg)` en `src/RoastingManager.Domain/Calculos/CalculoCostoPromedio.cs` (hace pasar T025)
- [X] T032 [P] [US1] Implementar `ReglasBatch` (datos de proceso, stock suficiente, topes de batch e ingreso) en `src/RoastingManager.Domain/Reglas/ReglasBatch.cs` (hace pasar T026)
- [X] T033 [P] [US1] Crear las entidades EF `ProveedorCafe`, `CafeVerde`, `IngresoStock` y `Batch` en `src/RoastingManager.Infrastructure/Persistence/Entidades/` con sus configuraciones (tipos `numeric`, `bigint`, `date`) en `src/RoastingManager.Infrastructure/Persistence/Configuraciones/`, y registrarlas en `RoastingDbContext`
- [X] T034 [US1] Implementar `BloqueoCafe.ObtenerParaActualizar(db, cafeVerdeId)` con `SELECT * FROM cafe_verde WHERE id = @id FOR UPDATE` (vía `FromSqlInterpolated`) en `src/RoastingManager.Infrastructure/Operaciones/BloqueoCafe.cs`; lanza un 404 si el café no existe
- [X] T035 [US1] Implementar `RegistrarIngreso` (transacción → bloqueo → validar cantidad, tope y fecha → sumar stock verde → insertar → commit) en `src/RoastingManager.Infrastructure/Operaciones/RegistrarIngreso.cs`
- [X] T036 [US1] Implementar `RegistrarBatch` (transacción → bloqueo → validar → `CalculoBatch` → `CalculoCostoPromedio` → actualizar stock verde, stock tostado y costo promedio → insertar el batch con `precio_kg_aplicado` → commit) en `src/RoastingManager.Infrastructure/Operaciones/RegistrarBatch.cs`
- [X] T037 [P] [US1] Crear los DTOs de US1 según `contracts/api.md` (proveedor, café verde, ingreso, batch, stock) en `src/RoastingManager.Api/Contracts/`
- [X] T038 [US1] Implementar `POST/GET /api/proveedores-cafe` y `POST/GET /api/cafes-verdes` (nombre normalizado, precio, stock inicial en kg → g) en `src/RoastingManager.Api/Endpoints/CatalogoCafeEndpoints.cs`
- [X] T039 [US1] Implementar `POST/GET /api/ingresos-stock` (filtro `cafeVerdeId`, fecha por defecto `IReloj.Hoy()`) en `src/RoastingManager.Api/Endpoints/IngresosEndpoints.cs`
- [X] T040 [US1] Implementar `POST /api/batches`, `GET /api/batches` (fecha desc, luego `creado_en` desc) y `GET /api/batches/{id}` en `src/RoastingManager.Api/Endpoints/BatchesEndpoints.cs`
- [X] T041 [US1] Implementar `GET /api/stock` (verde, tostado y `costoPromedioKg`, null si nunca se tostó) en `src/RoastingManager.Api/Endpoints/StockEndpoints.cs` y registrar todos los endpoints de US1 en `Program.cs` (hace pasar T027–T029)
- [X] T042 [P] [US1] Crear funciones de API para proveedores de café, cafés verdes, ingresos, batches y stock en `frontend/src/api/cafe.ts`
- [X] T043 [P] [US1] Crear la página de proveedores de café (reusa `CatalogoContactos`) y la de cafés verdes (alta con proveedor, precio por kg y stock inicial en kg, y listado) en `frontend/src/pages/Catalogo/ProveedoresCafe.tsx` y `frontend/src/pages/Catalogo/CafesVerdes.tsx`
- [X] T044 [P] [US1] Crear la página de ingresos de stock (alta con café, kg y fecha, e historial) en `frontend/src/pages/Ingresos.tsx`
- [X] T045 [US1] Crear la página de batches (alta con café, verde kg, tostado kg, fecha y datos de proceso opcionales; muestra el resultado con merma, costo y precio aplicado; muestra el error de stock insuficiente con disponible y requerido) en `frontend/src/pages/Batches.tsx`
- [X] T046 [P] [US1] Escribir el test de componente de la página de batches (envía los valores sin transformar y muestra el 409 con disponible y requerido) en `frontend/tests/Batches.test.tsx`

**Checkpoint**: US1 funciona y se puede demostrar sola (MVP): stock verde y tostado, merma, costo y costo promedio

---

## Phase 4: User Story 2 - Registrar una venta con costos y margen automáticos (Priority: P2)

**Goal**: Registrar proveedores de insumos, insumos, presentaciones, clientes y ventas, con descuento de stock tostado del café vendido, costos asociados, ganancia y margen congelados

**Independent Test**: Con 5 kg de tostado de "Colombia" a un costo promedio de $22,00 (sembrado vía US1 o en el fixture), crear "Bolsa" $0,50, "Etiqueta" $0,10, "Bolsa 250 g" y un cliente; vender 4 unidades a $15,00 → stock tostado −1 kg, ingreso $60,00, costo del café vendido $22,00, costos asociados $2,40, ganancia $35,60, margen 59,33 %

### Tests for User Story 2 (escribir primero y verificar que fallan) ⚠️

- [X] T047 [P] [US2] Escribir tests de `CalculoVenta` en `tests/RoastingManager.Domain.Tests/Calculos/CalculoVentaTests.cs`: el ejemplo del Independent Test; con costo promedio 23,53 → ganancia 34,07 y margen 56,78; presentación sin insumos → costos asociados 0,00; precio bajo → ganancia y margen negativos redondeados AwayFromZero; cada paso usa el valor redondeado anterior (FR-017, FR-018, FR-024)
- [X] T048 [P] [US2] Escribir tests de reglas de venta en `tests/RoastingManager.Domain.Tests/Reglas/ReglasVentaTests.cs`: unidades ≤ 0 o > tope → ValidacionException; gramos vendidos > stock tostado → StockInsuficienteException; gramos vendidos = stock → se acepta (FR-014, FR-016, FR-030)
- [X] T049 [P] [US2] Escribir tests de integración de catálogo en `tests/RoastingManager.Api.Tests/US2/CatalogoVentaTests.cs`: alta de proveedor de insumos, insumo (precio ≤ 0 → 400), presentación (gramos ≤ 0 → 400; con 0..n insumos) y cliente; nombres duplicados → 409
- [X] T050 [P] [US2] Escribir tests de integración en `tests/RoastingManager.Api.Tests/US2/VentasTests.cs`: el escenario del Independent Test; venta sin stock → 409 sin efectos; la venta no cambia el costo promedio del café; un batch posterior no cambia la venta guardada; la venta devuelve los precios de insumos y el costo promedio aplicados; vender otro café no toca el stock de "Colombia"; el batch tras vender todo el tostado toma el costo del batch (movido desde T028)
- [X] T051 [P] [US2] Escribir el test de concurrencia de ventas en `tests/RoastingManager.Api.Tests/US2/ConcurrenciaVentaTests.cs`: 5 ventas simultáneas de 1 kg sobre 3 kg de tostado → 3 aceptadas, 2 con 409, stock final 0

### Implementation for User Story 2

- [X] T052 [P] [US2] Implementar `CalculoVenta.Calcular(precioUnitario, unidades, gramosVendidos, costoPromedioKg, preciosInsumos)` → (Ingreso, CostoCafeVendido, CostosAsociados, Ganancia, MargenPct) en `src/RoastingManager.Domain/Calculos/CalculoVenta.cs` (hace pasar T047)
- [X] T053 [P] [US2] Implementar `ReglasVenta` en `src/RoastingManager.Domain/Reglas/ReglasVenta.cs` (hace pasar T048)
- [X] T054 [P] [US2] Crear las entidades EF `ProveedorInsumo`, `Insumo`, `Presentacion`, `PresentacionInsumo`, `Cliente`, `Venta` y `VentaInsumo` con sus configuraciones en `src/RoastingManager.Infrastructure/Persistence/Entidades/` y `.../Configuraciones/`, y registrarlas en `RoastingDbContext`
- [X] T055 [US2] Implementar `RegistrarVenta` (transacción → `BloqueoCafe` → validar → leer presentación e insumos → `CalculoVenta` con el costo promedio actual → restar stock tostado sin tocar el costo promedio → insertar la venta y `venta_insumo` con precios copiados → commit) en `src/RoastingManager.Infrastructure/Operaciones/RegistrarVenta.cs`
- [X] T056 [P] [US2] Crear los DTOs de US2 según `contracts/api.md` en `src/RoastingManager.Api/Contracts/`
- [X] T057 [US2] Implementar `POST/GET` de `/api/proveedores-insumo`, `/api/insumos`, `/api/presentaciones` (con `insumoIds`) y `/api/clientes` en `src/RoastingManager.Api/Endpoints/CatalogoVentaEndpoints.cs`
- [X] T058 [US2] Implementar `POST /api/ventas`, `GET /api/ventas` (fecha desc) y `GET /api/ventas/{id}` en `src/RoastingManager.Api/Endpoints/VentasEndpoints.cs` y registrarlos en `Program.cs` (hace pasar T049–T051)
- [X] T059 [P] [US2] Crear funciones de API para el catálogo de venta y las ventas en `frontend/src/api/ventas.ts`
- [X] T060 [P] [US2] Crear las páginas de proveedores de insumos y clientes (reusan `CatalogoContactos`), insumos y presentaciones (gramos y selección múltiple de insumos) en `frontend/src/pages/Catalogo/`
- [X] T061 [US2] Crear la página de ventas (alta con cliente, café, presentación, unidades, precio y fecha; muestra el resultado con ingreso, costo del café, costos asociados, ganancia y margen; muestra el error de stock insuficiente) en `frontend/src/pages/Ventas.tsx`
- [X] T062 [P] [US2] Escribir el test de componente de la página de ventas (formatea sin recalcular y muestra el margen negativo) en `frontend/tests/Ventas.test.tsx`

**Checkpoint**: US1 y US2 funcionan; se puede vender y ver la rentabilidad de cada venta

---

## Phase 5: User Story 3 - Consultar stock, historiales, costos y rentabilidad (Priority: P3)

**Goal**: Pantallas de consulta: stock por café (con el último costo promedio conocido), historial de batches con su costo de producción, historial de ventas con costos asociados y rentabilidad, e historial de ingresos

**Independent Test**: Con batches y ventas cargados, cada consulta muestra los valores guardados al registrar; con el tostado en 0 el stock muestra el último costo promedio; un café nunca tostado muestra "—"; los listados vacíos muestran "No hay registros"

### Tests for User Story 3 (escribir primero y verificar que fallan) ⚠️

- [X] T063 [P] [US3] Escribir tests de integración en `tests/RoastingManager.Api.Tests/US3/ConsultasTests.cs`: el stock con tostado 0 conserva el costo promedio y el café nunca tostado devuelve null (FR-019); historiales de batches y ventas ordenados por fecha descendente, incluida una carga con fecha pasada (FR-028); los listados vacíos devuelven `[]`
- [X] T064 [P] [US3] Escribir el test del invariante de reconstrucción en `tests/RoastingManager.Api.Tests/US3/InvarianteStockTests.cs`: tras una secuencia mixta (ingresos, batches, ventas y rechazos, incluidas operaciones concurrentes), el stock de cada café coincide con stock inicial + ingresos − verde de batches y con tostado de batches − gramos vendidos (FR-031)
- [X] T065 [P] [US3] Escribir los tests de componente de stock e historiales en `frontend/tests/Consultas.test.tsx`: "—" cuando `costoPromedioKg` es null, kg con 3 decimales, $ y % en formato es-AR, y estado vacío

### Implementation for User Story 3

- [X] T066 [US3] Verificar que `GET /api/stock`, `GET /api/batches` y `GET /api/ventas` (ya implementados en T040, T041 y T058) pasen T063–T064; corregir solo desvíos
- [X] T067 [P] [US3] Crear la página de stock (tabla por café: verde kg, tostado kg, costo promedio $/kg o "—") en `frontend/src/pages/Stock.tsx`
- [X] T068 [P] [US3] Crear la página de historial de batches (fecha, café, verde, tostado, merma, precio aplicado, costo de producción: $/kg y total) en `frontend/src/pages/HistorialBatches.tsx`
- [X] T069 [P] [US3] Crear la página de historial de ventas (fecha, cliente, café, presentación, unidades, precio, costos asociados, ganancia, margen) con detalle de insumos aplicados en `frontend/src/pages/HistorialVentas.tsx`
- [X] T070 [US3] Agregar las rutas y el menú de las páginas de consulta en `frontend/src/App.tsx` (hace pasar T065)

**Checkpoint**: las tres historias funcionan de forma independiente

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Rendimiento, documentación y validación final

- [X] T071 [P] Agregar índices para historiales (`batch(fecha desc, creado_en desc)`, `venta(fecha desc, creado_en desc)`, `ingreso_stock(cafe_verde_id, fecha desc)`) en `src/RoastingManager.Infrastructure/Migrations/0002_indices_historial.sql`
- [X] T072 [P] Agregar logging estructurado de operaciones de stock (operación, café, cantidades, resultado o rechazo) en `src/RoastingManager.Infrastructure/Operaciones/` usando `ILogger`
- [X] T073 [P] Escribir un test de humo de rendimiento en `tests/RoastingManager.Api.Tests/RendimientoTests.cs`: 5 clientes concurrentes × 20 operaciones mixtas → p95 < 3 s (RNF-01, SC-005)
- [X] T074 [P] Escribir `README.md` con prerrequisitos, configuración de `.env` y connection string, comandos y topes configurables, enlazando `quickstart.md`
- [X] T075 Revisar que no haya secretos en el repo (`appsettings*.json`, `docker-compose.yml`, tests) y que `.env` esté ignorado (principio VI)
- [X] T076 Ejecutar la validación completa de `specs/001-roasting-stock-management/quickstart.md` (pasos 1–9 y topes configurables) y corregir desvíos

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: sin dependencias
- **Foundational (Phase 2)**: depende de Setup; BLOQUEA todas las historias
- **US1 (Phase 3)**: depende de Foundational
- **US2 (Phase 4)**: depende de Foundational. Usa `BloqueoCafe` (T034) y las entidades `CafeVerde`/`Batch` (T033) de US1; sus tests de integración siembran el stock tostado con el endpoint de batches o directamente en la base desde el fixture
- **US3 (Phase 5)**: depende de US1 y US2 (consulta sus datos)
- **Polish (Phase 6)**: depende de las historias que se entreguen

### User Story Dependencies

```text
Setup → Foundational → US1 (MVP) → US2 → US3 → Polish
                          └── T033, T034 son prerrequisito de T054/T055 (US2)
```

### Within Each User Story

- Tests de dominio e integración primero; deben FALLAR antes de implementar (principio III)
- Cálculos de dominio → entidades EF → operaciones transaccionales → endpoints → frontend
- Cada `[P]` toca un archivo distinto y no depende de tareas incompletas de su fase

### Parallel Opportunities

- Setup: T003, T004, T005 y T006 en paralelo
- Foundational: los tests T007–T009 en paralelo; luego T010, T017, T018, T021, T022 y T023 en paralelo con la cadena T011→T012→T013→T014→T015→T016→T019→T020
- US1: los tests T024–T029 en paralelo; T030–T033 en paralelo; frontend T042–T044 y T046 en paralelo una vez definidos los DTOs
- US2: los tests T047–T051 en paralelo; T052–T054 y T056 en paralelo; frontend T059, T060 y T062 en paralelo
- US3: los tests T063–T065 en paralelo; páginas T067–T069 en paralelo

---

## Parallel Example: User Story 1

```bash
# Tests primero (todos en paralelo, deben fallar):
Task: "Tests de CalculoBatch en tests/RoastingManager.Domain.Tests/Calculos/CalculoBatchTests.cs"
Task: "Tests de CalculoCostoPromedio en tests/RoastingManager.Domain.Tests/Calculos/CalculoCostoPromedioTests.cs"
Task: "Tests de integración de batches en tests/RoastingManager.Api.Tests/US1/BatchesTests.cs"
Task: "Test de concurrencia en tests/RoastingManager.Api.Tests/US1/ConcurrenciaBatchTests.cs"

# Luego, implementación de dominio y entidades en paralelo:
Task: "CalculoBatch en src/RoastingManager.Domain/Calculos/CalculoBatch.cs"
Task: "CalculoCostoPromedio en src/RoastingManager.Domain/Calculos/CalculoCostoPromedio.cs"
Task: "Entidades EF de US1 en src/RoastingManager.Infrastructure/Persistence/Entidades/"
```

## Parallel Example: User Story 2

```bash
Task: "Tests de CalculoVenta en tests/RoastingManager.Domain.Tests/Calculos/CalculoVentaTests.cs"
Task: "Tests de ventas en tests/RoastingManager.Api.Tests/US2/VentasTests.cs"
Task: "Test de concurrencia en tests/RoastingManager.Api.Tests/US2/ConcurrenciaVentaTests.cs"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Completar Phase 1 (Setup) y Phase 2 (Foundational)
2. Completar Phase 3 (US1)
3. **PARAR Y VALIDAR**: correr el Independent Test de US1 y los pasos 1–4 de `quickstart.md`
4. Demo: catálogo de cafés, ingresos, batches, stock, merma y costos

### Incremental Delivery

1. Setup + Foundational → base lista
2. US1 → validar → MVP
3. US2 → validar (ventas y rentabilidad)
4. US3 → validar (consultas)
5. Polish → `quickstart.md` completo

---

## Notes

- [P] = archivos distintos, sin dependencias pendientes
- Toda tarea de cálculo tiene su test previo; no implementar sin ver el test fallar
- Ningún cálculo de negocio en el frontend: solo formatear lo que devuelve la API
- No usar `dotnet ef migrations`: todo cambio de esquema es un nuevo script SQL de DbUp
- Commit al terminar cada tarea o grupo lógico (Conventional Commits)
