# RoastingManager

Aplicación interna para una tostaduría de café: catálogo (proveedores, cafés verdes, insumos,
presentaciones, clientes), ingresos de stock de café verde, batches de tueste y ventas. Calcula
merma, costo del tostado, costo promedio ponderado por café, costos asociados, ganancia y margen.

- Especificación: [`specs/001-roasting-stock-management/spec.md`](specs/001-roasting-stock-management/spec.md)
- Validación end-to-end: [`specs/001-roasting-stock-management/quickstart.md`](specs/001-roasting-stock-management/quickstart.md)
- Contrato de la API: [`specs/001-roasting-stock-management/contracts/api.md`](specs/001-roasting-stock-management/contracts/api.md)

## Prerrequisitos

- Docker con Docker Compose
- .NET SDK 9
- Node.js 22 con npm

## Configuración

1. Copiá `.env.example` a `.env` y completá `POSTGRES_PASSWORD`. Si el puerto 5432 está ocupado,
   cambiá `POSTGRES_PORT`. El archivo `.env` está en `.gitignore` y no se commitea.
2. Exportá la connection string de la API (no va en `appsettings.json`):

   ```bash
   export ConnectionStrings__RoastingManager="Host=localhost;Port=5432;Database=roasting;Username=roasting;Password=<la del .env>"
   ```

   En desarrollo también podés usar user-secrets:
   `dotnet user-secrets init --project src/RoastingManager.Api` y luego
   `dotnet user-secrets set ConnectionStrings:RoastingManager "<...>" --project src/RoastingManager.Api`.

## Cómo correr

```bash
docker compose up -d                          # PostgreSQL
dotnet restore
dotnet run --project src/RoastingManager.Api  # http://localhost:5080; aplica las migraciones DbUp al iniciar
npm install
npm run dev                                   # frontend en http://localhost:5173, con proxy de /api a la API
```

> **WSL + .NET de Windows:** si la API corre con el `dotnet.exe` de Windows y `npm run dev` corre
> dentro de WSL, el proxy de Vite no llega a `localhost:5080` de Windows. Corré ambos del mismo
> lado (los dos en Windows o los dos en WSL), o levantá la API con
> `--urls http://0.0.0.0:5080` y apuntá el proxy con `API_URL=http://<ip-de-windows>:5080 npm run dev`.

## Tests

```bash
dotnet test   # Domain.Tests (cálculos, TDD) + Api.Tests (PostgreSQL real con Testcontainers; requiere Docker)
npm test      # Vitest + React Testing Library
```

## Topes configurables

Los topes por operación (FR-030) están en la sección `Topes` de
`src/RoastingManager.Api/appsettings.json` y se pueden sobrescribir por variable de entorno:

| Clave | Por defecto | Variable de entorno |
|---|---|---|
| `IngresoVerdeMaxKg` (ingreso y stock inicial) | 1000 | `Topes__IngresoVerdeMaxKg` |
| `BatchVerdeMaxKg` | 100 | `Topes__BatchVerdeMaxKg` |
| `VentaMaxUnidades` | 1000 | `Topes__VentaMaxUnidades` |

La zona horaria que define "hoy" (para rechazar fechas futuras) se configura en
`App:ZonaHoraria` (por defecto `America/Argentina/Buenos_Aires`).

## Estructura

```text
src/RoastingManager.Domain/          cálculos y reglas puras (sin dependencias)
src/RoastingManager.Infrastructure/  EF Core, migraciones DbUp, operaciones transaccionales
src/RoastingManager.Api/             Minimal APIs, ProblemDetails, configuración
tests/                               xUnit (dominio e integración)
frontend/                            React + Vite
```

El esquema se gestiona con scripts SQL en `src/RoastingManager.Infrastructure/Migrations/`
(DbUp). No usar `dotnet ef migrations`.
