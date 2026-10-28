# AGENTS.md

## Propósito
RoastingManager gestiona el stock de café verde y tostado, y el registro de batches de tueste de una tostaduría. Calcula automáticamente merma, costos y rentabilidad de cada batch.

## Stack
- Backend: .NET 9 (ASP.NET Core) + Entity Framework Core con Npgsql.EntityFrameworkCore.PostgreSQL para acceso a datos. Migraciones de esquema con DbUp (no usar `dotnet ef migrations`).
- Frontend: React + Vite, gestor de paquetes npm.
- Base de datos: PostgreSQL, levantada vía Docker Compose.

## Cómo correr
```bash
# Base de datos
docker compose up -d

# Backend
dotnet restore
dotnet run --project src/RoastingManager.Api
dotnet test

# Frontend
npm install
npm run dev
npm test
```

## Qué NO hacer
- No implementar CRM, facturación, e-commerce, gestión de compras ni soporte multitenancy (Fuera de Alcance, PRD).
- No permitir registrar un batch sin stock suficiente de café verde, ni una venta sin stock suficiente de café tostado, ni dejar el stock en negativo (RF-10, RF-16, RNF-02, AC-10, AC-16).
- No usar otra base de datos que no sea PostgreSQL, ni generar migraciones con `dotnet ef` (el proyecto usa DbUp).
