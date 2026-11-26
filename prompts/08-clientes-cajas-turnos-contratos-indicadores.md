# 08 - Clientes, cajas, turnos, contratos, función de PostgreSQL, menú e indicadores

**Herramienta:** Claude Code (agente de IA en terminal)

## Prompt

> Ahora haz el CRUD de clientes, cajas con asignación de cajeros, turnos, contratos con sus procesos
> (contratar, pagar, cambio de servicio, cambio de forma de pago, cancelación), la función de
> PostgreSQL, y los endpoints de menú por rol e indicadores.

## Resultado

| Módulo | Endpoints |
|---|---|
| Clientes (`/api/clients`) | CRUD, búsqueda por identificación, cambio de estado y carga masiva |
| Cajas (`/api/cashes`) | CRUD, asignar y retirar cajeros, cajas del cajero, abrir y cerrar caja |
| Turnos (`/api/turns`) | CRUD, iniciar atención y finalizar atención |
| Contratos (`/api/contracts`) | Contratar, cambio de servicio, cambio de forma de pago, cancelación, eliminación |
| Pagos (`/api/payments`) | Registrar y consultar pagos |
| Catálogos (`/api/catalogs`) | Roles, estados, tipos de atención, formas de pago y servicios |
| Menú (`/api/menu`) | Opciones del rol autenticado, leídas de `menu` y `rolmenu` |
| Indicadores (`/api/dashboard`) | Bienvenida por rol e indicadores del administrador con filtro de fechas |

- Función `fn_next_turn_description` (script en `Backend/database/`), aplicada por la migración
  `MenuSeedAndTurnFunction` junto con los datos del menú.
- `IApplicationDbContext`: abstracción del acceso a datos que usan los servicios de Application.

### Comandos usados

```bash
dotnet add src/GestionCaja.Application package Microsoft.EntityFrameworkCore --version "10.*"
dotnet ef migrations add MenuSeedAndTurnFunction --project src/GestionCaja.Infrastructure --startup-project src/GestionCaja.Api --output-dir Persistence/Migrations
dotnet ef database update --project src/GestionCaja.Infrastructure --startup-project src/GestionCaja.Api
```

### Decisiones

- **Dos cajeros no operan la caja a la vez**: `cash.current_user_id` guarda quién la tiene abierta;
  se refuerza con control de concurrencia optimista sobre la fila (`xmin`).
- **Máximo 2 cajeros por caja**, validado al asignar.
- **Descripción del turno** generada por la función de PostgreSQL, con bloqueo por tipo de atención
  dentro de una transacción para que no se repitan números.
- **Cambio de servicio**: el contrato pasa a `SUS` y se crea uno nuevo en `REN` con la misma fecha
  final y referencia al anterior, en una sola transacción.
- **Cancelación**: estado `CAN` y fecha fin igual a la fecha de cancelación.
- Un cliente solo puede tener un contrato en curso.
- "Hoy" se calcula con la hora de Ecuador (UTC-5); las fechas se guardan en UTC.

### Pruebas realizadas (36 casos contra la API levantada)

- Menú distinto para administrador, gestor y cajero.
- Clientes: validaciones de identificación, teléfono, dirección y referencia; identificación duplicada (409);
  búsqueda por identificación; carga masiva con filas válidas e inválidas.
- Cajas: tercer cajero rechazado; asignar un usuario que no es cajero rechazado.
- Turnos: `CS0001`, `CS0002` y `PS0001` (secuencial por tipo); tipo inexistente rechazado.
- Caja en uso: el segundo cajero recibe 409 al intentar abrirla y 403 al intentar atender.
- Contratos: contratación (`VIG`), segundo contrato rechazado, pago por el precio del plan,
  cambio de servicio (`SUS` + `REN` con la misma fecha final), cambio de forma de pago,
  cancelación (`CAN` con nueva fecha fin), operación sobre contrato sustituido rechazada.
- Indicadores: total de turnos atendidos según el rol; indicadores del administrador; cajero recibe 403.
