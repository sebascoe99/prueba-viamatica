# 03 - EF Core y primera migración

**Herramienta:** Claude Code (agente de IA en terminal)

## Prompt

> Sigamos con el esquema: configura Entity Framework Core con PostgreSQL y crea las tablas del grupo
> de seguridad con sus columnas e índices.

## Resultado

- `AppDbContext` con eliminación lógica y auditoría automática en `SaveChangesAsync`.
- Una clase de configuración por tabla (Fluent API) con longitudes, claves foráneas e índices.
- Datos iniciales de los catálogos `rol` y `userstatus`.
- `DependencyInjection.AddInfrastructure` para registrar la infraestructura desde la Api.
- Migración `InitialSecurity` aplicada sobre PostgreSQL.

### Comandos usados

```bash
dotnet add src/GestionCaja.Infrastructure package Npgsql.EntityFrameworkCore.PostgreSQL --version "10.*"
dotnet add src/GestionCaja.Infrastructure package EFCore.NamingConventions --version "10.*"
dotnet add src/GestionCaja.Api package Microsoft.EntityFrameworkCore.Design --version "10.*"
dotnet tool install --global dotnet-ef --version "10.*"

dotnet ef migrations add InitialSecurity --project src/GestionCaja.Infrastructure --startup-project src/GestionCaja.Api --output-dir Persistence/Migrations
dotnet ef database update --project src/GestionCaja.Infrastructure --startup-project src/GestionCaja.Api
```

### Decisiones

- Enfoque **code-first**: las tablas se generan desde las entidades mediante migraciones versionadas.
- Nombres en **snake_case** (`user_id`, `created_at`), que es la convención de PostgreSQL.
- Índice único sobre `users.user_name` para garantizar que no existan usuarios duplicados.
- Filtro global `IsDeleted = false` para que las consultas ignoren los registros eliminados lógicamente.
