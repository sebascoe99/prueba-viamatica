# 07 - Swagger, Serilog y CRUD de usuarios

**Herramienta:** Claude Code (agente de IA en terminal)

## Prompt

> Hagamos el siguiente paso: Swagger, Serilog y el CRUD de usuarios con sus reglas. Ve haciendo
> pruebas tú también.

## Resultado

- **Swagger** en `/swagger`, con el botón Authorize para enviar el token JWT.
- **Serilog** a consola y a dos archivos diarios: `logs/gestioncaja-<fecha>.log` (procesos de creación,
  actualización, eliminación e inicios de sesión) y `logs/alertas-<fecha>.log` (advertencias y errores).
- **CRUD de usuarios** (`/api/users`): listar con filtros, consultar, crear, carga masiva, actualizar,
  cambiar estado, aprobar y eliminar (lógico).
- **Recuperación de contraseña** (`/api/auth/recover-password`) validando usuario, identificación y correo.
- `ICurrentUser` para conocer en los servicios quién hace la petición.
- Respuesta uniforme `{ message, errors }` en los errores de validación y CORS para el frontend.

### Comandos usados

```bash
dotnet remove src/GestionCaja.Api package Microsoft.AspNetCore.OpenApi
dotnet add src/GestionCaja.Api package Swashbuckle.AspNetCore --version "10.*"
dotnet add src/GestionCaja.Api package Serilog.AspNetCore
```

### Decisiones

- Las reglas de usuario y contraseña del enunciado están como expresiones regulares en `UserRules`
  y se aplican con atributos en los DTO; la carga masiva reutiliza las mismas reglas.
- Lo que crea un administrador queda activo; lo que crea un gestor queda pendiente de aprobación.
- El gestor solo ve los usuarios que él creó.
- Un nombre de usuario eliminado lógicamente no se puede volver a usar.
- La carga masiva procesa cada fila por separado y devuelve el resultado fila por fila.

### Pruebas realizadas (contra la API levantada)

| Caso | Resultado |
|---|---|
| Swagger JSON y UI | 200 |
| Administrador crea gestor | 201, estado ACT |
| Nombre de usuario duplicado | 409 |
| Usuario, contraseña, correo e identificación inválidos | 400 con los mensajes de cada regla |
| Crear un usuario con rol Administrador | 400 |
| Gestor crea cajero | estado PEN |
| Cajero pendiente intenta iniciar sesión | 403 |
| Gestor intenta aprobar | 403 |
| Administrador aprueba | estado ACT |
| Cajero consulta usuarios | 403 |
| Gestor lista usuarios | solo los creados por él |
| Actualizar datos y cambiar estado | 200 |
| Tres intentos fallidos | usuario bloqueado |
| Recuperar contraseña con datos incorrectos / correctos | 400 / 200 |
| Carga masiva con 1 fila válida y 2 inválidas | 1 creada, 2 rechazadas con su motivo |
| Eliminar | 204; la fila sigue en la base con `is_deleted = true` |

Durante las pruebas se detectó que la carga masiva rechazaba todo el archivo cuando una fila era
inválida; se corrigió con un DTO propio (`UserBulkRow`) sin validación automática.
