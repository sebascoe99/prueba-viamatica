# 05 - Entidades de servicios, contratos y pagos

**Herramienta:** Claude Code (agente de IA en terminal)

## Prompt

> Continuemos con el resto del esquema: servicios, equipos, contratos, formas de pago y pagos.

## Resultado

- Entidades `Service`, `Device`, `StatusContract`, `MethodPayment`, `Contract` y `Payment`.
- Configuración de cada tabla con claves foráneas, restricciones e índices.
- Datos iniciales de `statuscontract`, `methodpayment` y tres planes de ejemplo con sus equipos.
- Migración `ServiceContractPayment` aplicada sobre PostgreSQL.

### Comandos usados

```bash
dotnet ef migrations add ServiceContractPayment --project src/GestionCaja.Infrastructure --startup-project src/GestionCaja.Api --output-dir Persistence/Migrations
dotnet ef database update --project src/GestionCaja.Infrastructure --startup-project src/GestionCaja.Api
```

### Decisiones

- `service.price` pasa de texto (esquema de ejemplo) a `numeric(10,2)` y se agrega `speed_mbps`,
  necesario para comparar planes en el cambio de servicio.
- `contract.previous_contract_id` enlaza el contrato nuevo con el que sustituyó, para conservar el historial.
- `payments` incluye `contract_id`, `amount`, `method_payment_id` y el cajero que registró el pago.
- Restricciones `CHECK`: precio no negativo, velocidad mayor a cero, monto mayor a cero y
  fecha fin mayor o igual a fecha de inicio.
