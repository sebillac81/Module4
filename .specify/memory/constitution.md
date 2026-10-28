<!--
Sync Impact Report
- Version change: 2.0.0 → 2.0.1 (PATCH: corrección de referencias al PRD en principio IV:
  RNF-03, AC-04 → RF-10, RF-16, RNF-02, AC-10, AC-16)
- Cambio previo: 1.0.0 → 2.0.0 (MAJOR: principios redefinidos/reemplazados)
- Principios modificados:
  - I. IA Aislada en Módulo Dedicado → I. IA Aislada en Módulo Dedicado (contexto: análisis
    de curvas de tueste de Artisan, funcionalidad futura)
  - II. Respuestas Fundamentadas en la KB → II. Análisis de IA Fundamentado en Datos
    (redefinido: se apoya en los datos de la curva, no en una KB; ante la duda, revisión humana)
  - III. Clasificación Test-First → III. Cálculos de Dominio Test-First (redefinido: merma,
    costos, costo promedio ponderado, rentabilidad y futura lógica de análisis)
  - IV. Sin Secretos en el Código → VI. Sin Secretos en el Código (renumerado)
  - V. Alcance Acotado al PRD Vigente → VII. Alcance Acotado al PRD Vigente (renumerado)
- Principios agregados: IV. Integridad de Stock, V. Precisión Monetaria y de Pesos
- Secciones eliminadas: ninguna
- Templates:
  - ✅ .specify/templates/plan-template.md — "Constitution Check" genérico; se completa por
    feature verificando los siete principios.
  - ✅ .specify/templates/spec-template.md — sin secciones obligatorias nuevas.
  - ✅ .specify/templates/tasks-template.md — sin cambios; el principio III exige que las
    tareas de test precedan a las de implementación de cálculos.
  - ✅ AGENTS.md — consistente.
- TODOs diferidos:
  - Principios I y II aplican cuando un PRD incorpore el análisis de curvas de Artisan;
    hasta entonces el principio VII impide implementarlo.
-->

# RoastingManager Constitution

RoastingManager es una plataforma para ayudar a los tostadores de café a gestionar el
stock de café verde y tostado, los batches de tueste, las ventas y su rentabilidad. A
futuro incorporará análisis asistido por IA de curvas de tueste exportadas desde Artisan.

## Core Principles

### I. IA Aislada en Módulo Dedicado

- Toda la lógica de IA (importación y análisis de curvas de Artisan, llamadas a modelos,
  prompts, post-procesamiento) MUST vivir en un módulo/proyecto dedicado.
- La lógica de negocio (stock, batches, merma, costos, ventas, rentabilidad) MUST NOT
  invocar proveedores de IA directamente ni contener prompts; consume el módulo de IA
  únicamente a través de una interfaz definida.
- El módulo de IA MUST NOT modificar estado de negocio (stock, costos, batches); devuelve
  resultados que la capa de negocio o el usuario validan y aplican.

**Rationale**: los cálculos de stock y costos deben ser deterministas y testeables sin
depender de un modelo; aislar la IA permite reemplazarla, mockearla y auditarla.

### II. Análisis de IA Fundamentado en Datos

- Todo análisis o sugerencia generada por IA MUST basarse exclusivamente en los datos
  disponibles (la curva de Artisan y los datos del batch registrados en el sistema);
  MUST NOT afirmar hechos que no surjan de esos datos.
- Si los datos son insuficientes, inconsistentes o la confianza es baja, el sistema MUST
  indicarlo explícitamente y derivar la interpretación al tostador, en lugar de concluir.
- La IA sugiere, no decide: toda conclusión SHOULD mostrar en qué datos de la curva se
  apoya (fases, temperaturas, tiempos, RoR) para que una persona pueda verificarla.

**Rationale**: una conclusión inventada sobre un tueste es peor que ninguna; ante la duda,
decide el tostador.

### III. Cálculos de Dominio Test-First (NO NEGOCIABLE)

- Los cálculos de dominio (merma, costo del café tostado, costo promedio ponderado,
  costo de presentaciones con insumos, margen y rentabilidad) MUST desarrollarse con TDD:
  se escriben los tests, se verifica que fallen y recién entonces se implementa
  (Red → Green → Refactor).
- La misma regla aplica a cualquier lógica de clasificación o análisis futura.
- Cada cálculo MUST tener tests que cubran casos normales, bordes (cantidades cero,
  stock exacto, redondeos) y entradas inválidas.

**Rationale**: los números son el producto; un error de cálculo invalida costos y
rentabilidad sin que nadie lo note.

### IV. Integridad de Stock

- Toda operación que modifique stock (registro de batch, venta) MUST ejecutarse en una
  única transacción de base de datos junto con el registro que la origina.
- El stock de café verde y tostado MUST NOT quedar en negativo; una operación sin stock
  suficiente MUST rechazarse sin efectos parciales (RF-10, RF-16, RNF-02, AC-10, AC-16).
- La validación de stock MUST ser robusta ante concurrencia (bloqueo o verificación
  atómica en la base), no solo en la UI.

**Rationale**: el stock es la fuente de verdad del negocio; un descuadre se propaga a
costos y ventas.

### V. Precisión Monetaria y de Pesos

- Montos y cantidades (kg, g) MUST representarse con tipos decimales exactos (`decimal`
  en .NET, `numeric` en PostgreSQL); MUST NOT usarse `float`/`double`.
- Las reglas de redondeo (cantidad de decimales y modo) MUST estar definidas
  explícitamente y aplicarse en un único lugar del dominio.
- El frontend MUST NOT recalcular valores de negocio; muestra los que calcula el backend.

**Rationale**: los errores de punto flotante se acumulan en costos promedio y márgenes.

### VI. Sin Secretos en el Código

- Credenciales, API keys, connection strings con contraseña y cualquier secreto MUST
  provenir de variables de entorno (o del mecanismo de configuración que las lea).
- Ningún secreto MUST commitearse al repositorio, incluidos archivos de configuración,
  `docker-compose.yml`, tests y fixtures; se usan archivos `.env` ignorados por git y
  un `.env.example` sin valores reales.
- Un secreto commiteado por error MUST rotarse; borrarlo del historial no es suficiente.

**Rationale**: los secretos en el repositorio se filtran con cada clon, fork o log.

### VII. Alcance Acotado al PRD Vigente

- Solo se implementan funcionalidades descriptas en el PRD vigente (`PRD2.md`).
- Lo listado en "Fuera de Alcance" del PRD (CRM, facturación, e-commerce, gestión de
  compras, multitenancy, login, control de acceso por rol, FIFO, blends, edición y
  eliminación de catálogos) MUST NOT implementarse.
- Una funcionalidad nueva (p. ej., el análisis de curvas de Artisan) requiere primero
  actualizar el PRD; specs, planes y tareas MUST trazarse a requerimientos (RF/RNF/AC).

**Rationale**: evita scope creep y mantiene el producto alineado con lo acordado.

## Restricciones Técnicas

- Backend: .NET 9 (ASP.NET Core) + Entity Framework Core con Npgsql.
- Migraciones de esquema MUST hacerse con DbUp; MUST NOT usarse `dotnet ef migrations`.
- Base de datos: PostgreSQL exclusivamente, levantada vía Docker Compose.
- Frontend: React + Vite, con npm como gestor de paquetes.

## Flujo de Desarrollo y Quality Gates

- Cada feature sigue el flujo Spec Kit: specify → clarify → plan → tasks → implement.
- El "Constitution Check" de cada plan MUST verificar los siete principios; toda
  excepción se justifica en "Complexity Tracking".
- `dotnet test` y `npm test` MUST pasar antes de integrar cambios.
- Los commits siguen Conventional Commits.

## Governance

- Esta constitución prevalece sobre cualquier otra práctica o guía del proyecto.
- Las enmiendas se hacen modificando este archivo mediante `/speckit-constitution`,
  documentando el cambio en el Sync Impact Report y propagándolo a los templates.
- Versionado semántico: MAJOR para eliminar o redefinir principios de forma incompatible;
  MINOR para agregar principios/secciones o ampliar guías materialmente; PATCH para
  aclaraciones y redacción.
- Toda revisión de código MUST verificar el cumplimiento de estos principios. La guía
  operativa del día a día está en `AGENTS.md`.

**Version**: 2.0.1 | **Ratified**: 2026-10-27 | **Last Amended**: 2026-10-27
