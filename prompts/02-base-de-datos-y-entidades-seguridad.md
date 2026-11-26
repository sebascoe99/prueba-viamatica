# 02 - Base de datos y entidades de seguridad

**Herramienta:** Claude Code (agente de IA en terminal)

## Prompt

> Como el enunciado no especifica la base de datos, usemos PostgreSQL. Vamos armando poco a poco
> el esquema con sus tablas, columnas e índices. También hay que usar un procedimiento almacenado.

## Resultado

- `docker-compose.yml` en la raíz con el servicio de PostgreSQL 17 (base `gestioncaja`).
- Primer grupo de entidades en la capa Domain: `Rol`, `UserStatus`, `User`, `Menu`, `RolMenu`,
  la clase base `BaseEntity` (auditoría y eliminación lógica) y las constantes de catálogos.

### Comandos usados

```bash
docker compose up -d
dotnet build
```

### Decisiones

- La tabla de usuarios se llama `users` porque `user` es palabra reservada en PostgreSQL.
- Se agregaron al esquema de ejemplo los campos `Identification`, `FirstName`, `LastName`,
  `SessionActive`, `FailedAttempts` y `LastLogin`, además de las tablas `menu` y `rolmenu`,
  para cubrir el cifrado de la identificación, los indicadores del dashboard y el menú por rol.
- Las entidades no referencian a Entity Framework ni a PostgreSQL: el dominio no depende de la infraestructura.
