# Backend - Gestión de Caja

API REST en .NET 10 para la gestión de caja de una empresa de servicios de internet.

## Tecnologías

- .NET 10 (ASP.NET Core Web API)
- Entity Framework Core 10 con PostgreSQL 17
- JWT con claims cifradas (AES-256)
- BCrypt para contraseñas
- Serilog
- Swagger

## Arquitectura

Onion Architecture en cuatro proyectos. Las dependencias apuntan hacia el dominio.

```
src
├── GestionCaja.Domain          Entidades y constantes. No depende de nadie.
├── GestionCaja.Application     Servicios, DTOs, validaciones e interfaces.
├── GestionCaja.Infrastructure  EF Core, repositorios, cifrado, JWT y migraciones.
└── GestionCaja.Api             Controladores, Swagger, middleware y configuración.
```

## Requisitos

- SDK de .NET 10
- Docker (para PostgreSQL)

## Ejecutar en desarrollo

Desde la raíz del repositorio, levantar la base de datos:

```bash
docker compose up -d db
```

Desde la carpeta `Backend`:

```bash
dotnet run --project src/GestionCaja.Api
```

- API: `http://localhost:5009`
- Swagger: `http://localhost:5009/swagger`

Al arrancar, la API aplica las migraciones pendientes y crea el usuario administrador inicial.

## Usuario inicial

| Usuario | Contraseña | Rol |
|---|---|---|
| `admin2026` | `Admin2026*` | Administrador |

## Configuración

Los valores están en `src/GestionCaja.Api/appsettings.json` y se pueden reemplazar con variables de entorno.

| Clave | Variable de entorno | Descripción |
|---|---|---|
| `ConnectionStrings:DefaultConnection` | `ConnectionStrings__DefaultConnection` | Conexión a PostgreSQL |
| `Jwt:Key` | `Jwt__Key` | Clave de firma del token |
| `Jwt:ExpirationMinutes` | `Jwt__ExpirationMinutes` | Duración del token |
| `Encryption:Key` | `Encryption__Key` | Clave de cifrado AES |

Las claves incluidas son solo para desarrollo.

## Migraciones

```bash
dotnet tool install --global dotnet-ef --version "10.*"

dotnet ef migrations add <Nombre> --project src/GestionCaja.Infrastructure --startup-project src/GestionCaja.Api --output-dir Persistence/Migrations
dotnet ef database update --project src/GestionCaja.Infrastructure --startup-project src/GestionCaja.Api
```

## Función de base de datos

`database/fn_next_turn_description.sql` contiene la función `fn_next_turn_description`, que genera
la descripción del turno según el tipo de atención (por ejemplo `AC0001`, `PS0001`). La crea la
migración `MenuSeedAndTurnFunction` y la usa `POST /api/turns`.

## Endpoints

| Módulo | Ruta | Roles |
|---|---|---|
| Autenticación | `/api/auth` | Público y autenticado |
| Usuarios | `/api/users` | Administrador y gestor |
| Clientes | `/api/clients` | Cajero |
| Cajas | `/api/cashes` | Administrador, gestor y cajero |
| Turnos | `/api/turns` | Gestor y cajero |
| Contratos | `/api/contracts` | Cajero |
| Pagos | `/api/payments` | Cajero |
| Catálogos | `/api/catalogs` | Autenticado |
| Menú | `/api/menu` | Autenticado |
| Indicadores | `/api/dashboard` | Autenticado; `indicators` solo administrador |

El detalle de cada endpoint está en Swagger y en la colección de Postman (`../postman`).

## Puntos del enunciado

| Requisito | Dónde está |
|---|---|
| Onion Architecture | Los cuatro proyectos de `src/` |
| Eliminaciones lógicas | `AppDbContext.SaveChangesAsync` y filtro `IsDeleted` en cada configuración |
| Swagger | `Api/Extensions/SwaggerExtensions.cs` |
| Entity Framework | `Infrastructure/Persistence` |
| JWT con claims cifradas | `Infrastructure/Security/JwtTokenService.cs` y `Api/Extensions/JwtAuthenticationExtensions.cs` |
| Función de base de datos | `database/fn_next_turn_description.sql` |
| Contraseña, identificación y correo protegidos | `BcryptPasswordHasher` y `AesEncryptionService` |
| Serilog | `Api/Program.cs` |

## Logs

Serilog escribe en consola y en la carpeta `logs/` de la API:

- `gestioncaja-<fecha>.log`: creaciones, actualizaciones, eliminaciones e inicios de sesión.
- `alertas-<fecha>.log`: advertencias y errores.
