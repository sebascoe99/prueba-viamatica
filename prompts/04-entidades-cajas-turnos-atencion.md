# 04 - Entidades de cajas, turnos y atención

**Herramienta:** Claude Code (agente de IA en terminal)

## Prompt

> Arranquemos con el siguiente grupo del esquema: cajas, asignación de cajeros, turnos y atención,
> con sus catálogos, columnas e índices.

## Resultado

- Entidades `Cash`, `UserCash`, `Turn`, `AttentionType`, `AttentionStatus`, `Attention` y `Client`.
- Configuración de cada tabla con claves foráneas e índices.
- Datos iniciales de `attentiontype` y `attentionstatus`.
- Migración `CashTurnAttention` aplicada sobre PostgreSQL.

### Comandos usados

```bash
dotnet ef migrations add CashTurnAttention --project src/GestionCaja.Infrastructure --startup-project src/GestionCaja.Api --output-dir Persistence/Migrations
dotnet ef database update --project src/GestionCaja.Infrastructure --startup-project src/GestionCaja.Api
```

### Decisiones

- `cash.current_user_id` indica qué cajero opera la caja en ese momento; un índice único parcial
  impide que un mismo cajero opere dos cajas a la vez.
- `usercash` usa clave propia e índice único parcial (`is_deleted = false`) para permitir reasignar
  un cajero a una caja después de una eliminación lógica.
- `turn.description` tiene un `CHECK` con expresión regular (2 letras mayúsculas + 4 números).
- La clave de `attentiontype` son las 2 letras con las que inicia la descripción del turno.
- Índices compuestos con fecha para las consultas de turnos por caja, por gestor y de atenciones por día.
