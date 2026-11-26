# Prueba técnica .NET Core - Angular

Sistema de gestión de caja para una empresa que brinda servicios de internet.

## Estructura del repositorio

| Carpeta | Contenido |
|---|---|
| `Backend/` | API REST en .NET 10 con PostgreSQL. Ver `Backend/README.md` |
| `Frontend/` | Aplicación Angular 20. Ver `Frontend/README.md` |
| `Backend/database/` | Script de la función de base de datos |
| `postman/` | Colección de Postman con los CRUD, el registro de usuarios y el login |
| `prompts/` | Prompts utilizados con el agente de IA durante el desarrollo |
| `docker-compose.yml` | Base de datos, backend y frontend |

## Ejecutar con Docker

Requiere Docker Desktop.

```bash
docker compose up -d --build
```

| Servicio | URL |
|---|---|
| Aplicación | http://localhost:8081 |
| API | http://localhost:8080 |
| Swagger | http://localhost:8080/swagger |
| PostgreSQL | localhost:5432 (base `gestioncaja`, usuario `gestioncaja`, clave `gestioncaja123`) |

El frontend (nginx) reenvía las peticiones `/api` al contenedor del backend, y el backend se conecta
al contenedor de la base de datos. Al iniciar, la API crea las tablas, la función y los datos iniciales.

Para detener:

```bash
docker compose down
```

## Usuario inicial

| Usuario | Contraseña | Rol |
|---|---|---|
| `admin2026` | `Admin2026*` | Administrador |

Con el administrador se crean los usuarios gestores y cajeros.

## Flujo de uso

1. El **administrador** crea gestores y cajeros, y aprueba los usuarios creados por un gestor.
2. El **gestor** crea las cajas, asigna hasta 2 cajeros por caja y asigna los turnos.
3. El **cajero** abre su caja, atiende los turnos, registra clientes y ejecuta los procesos:
   contratación, pagos, cambio de servicio, cambio de forma de pago y cancelación de contrato.

## Postman

Importar `postman/GestionCaja.postman_collection.json`. La variable `baseUrl` apunta a
`http://localhost:8080`. Al ejecutar un login de la carpeta **Auth**, el token se guarda en la
variable `token` y lo usan los demás requests.
