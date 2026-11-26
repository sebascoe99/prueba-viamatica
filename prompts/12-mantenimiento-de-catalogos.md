# 12 - Mantenimiento de catálogos

**Herramienta:** Claude Code (agente de IA en terminal)

## Prompt

> Terminemos lo que quedó parcial: el CRUD de las tablas de catálogo que solo tenían consulta.

## Resultado

- **Backend**: `CatalogAdminService` y los controladores de `/api/services`, `/api/devices`,
  `/api/method-payments` y `/api/attention-types`, todos para el administrador.
- **Migración `CatalogMaintenance`**: agrega la eliminación lógica a `methodpayment` y
  `attentiontype`, y la opción de menú "Servicios" para el administrador.
- **Frontend**: módulo `features/services` con la pantalla de mantenimiento de servicios y sus equipos.
- **Postman**: dos carpetas nuevas con 17 requests.

### Comandos usados

```bash
dotnet ef migrations add CatalogMaintenance --project src/GestionCaja.Infrastructure --startup-project src/GestionCaja.Api --output-dir Persistence/Migrations
dotnet ef database update --project src/GestionCaja.Infrastructure --startup-project src/GestionCaja.Api
```

### Decisiones

- No se permite eliminar un servicio con contratos, una forma de pago usada en contratos o pagos,
  ni un tipo de atención con turnos; el servicio se puede inactivar.
- Un servicio inactivo deja de ofrecerse al cajero pero conserva sus contratos.
- El código del tipo de atención son las 2 letras con las que la función de PostgreSQL arma el turno.
- Quedan de solo lectura los catálogos de estados y los roles (sus códigos son parte de las reglas
  del sistema) y el menú (cada opción corresponde a una ruta del frontend).

### Problema encontrado y corregido

Al actualizar un servicio, la respuesta todavía incluía un equipo recién retirado, aunque en la base
ya estaba eliminado. Se corrigió filtrando los eliminados al armar la respuesta, y se aplicó lo mismo
a la respuesta de retirar un cajero de una caja.

### Pruebas realizadas

- API (26 casos): validaciones, nombre de servicio duplicado, sincronización de equipos, servicio
  inactivo fuera del catálogo del cajero, CRUD de equipos, bloqueo de eliminación cuando hay datos
  relacionados, eliminación lógica verificada en la base, CRUD de formas de pago y tipos de atención,
  y un turno `RT0001` generado con un tipo de atención nuevo.
- Interfaz: el administrador ve "Servicios" en su menú; formulario inválido, crear, editar, eliminar
  y rechazo al eliminar un servicio con contratos; un cajero no puede entrar a la pantalla.
