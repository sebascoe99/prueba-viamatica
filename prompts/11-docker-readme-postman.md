# 11 - Docker, README y Postman

**Herramienta:** Claude Code (agente de IA en terminal)

## Prompt

> Cerremos la entrega: Dockerfile de backend y frontend con docker-compose garantizando su
> comunicación, README de cada proyecto y la colección de Postman.

## Resultado

- `Backend/Dockerfile`: compilación en la imagen del SDK y ejecución en la imagen de ASP.NET.
- `Frontend/Dockerfile`: compilación con Node y publicación con nginx; `Frontend/nginx.conf` reenvía
  `/api` al backend y resuelve las rutas de Angular.
- `docker-compose.yml` con tres servicios: `db`, `api` y `frontend`.
- `README.md` en la raíz, en `Backend/` y en `Frontend/`.
- `postman/GestionCaja.postman_collection.json` con 58 requests: login, registro de usuarios y el
  CRUD de cada tabla. El login guarda el token en una variable de la colección.

### Comandos usados

```bash
docker compose build
docker compose up -d
```

### Decisiones

- La API espera a que la base esté lista (`depends_on` con `service_healthy`) y aplica las
  migraciones al arrancar, por lo que el entorno queda operativo con un solo comando.
- El frontend no conoce la dirección de la API: llama a `/api` y nginx lo reenvía al servicio `api`
  por la red interna de Docker.
- La configuración de la API en Docker se pasa por variables de entorno.
- Los logs de la API se guardan en un volumen.

### Pruebas realizadas

Con los tres contenedores arriba:

| Prueba | Resultado |
|---|---|
| Aplicación en `http://localhost:8081` | 200 |
| Ruta de Angular `/users` servida por nginx | 200 |
| Swagger en `http://localhost:8080/swagger` | 200 |
| Login a través de nginx (frontend → api → base) | Token del administrador |
| Menú a través de nginx | Opciones del rol leídas de la base |
| Archivos de log dentro del contenedor | `gestioncaja-<fecha>.log` y `alertas-<fecha>.log` |
