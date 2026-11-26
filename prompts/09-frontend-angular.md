# 09 - Frontend Angular

**Herramienta:** Claude Code (agente de IA en terminal)

## Prompt

> Ahora haz el Frontend Angular: login con recuperación de contraseña, layout con menú desde la base,
> guards, interceptor, y las pantallas (bienvenida, dashboard, usuarios, cajas, turnos, procesos de
> caja, clientes) con carga masiva desde Excel.

## Resultado

Estructura de `Frontend/src/app`:

| Carpeta | Contenido |
|---|---|
| `core/` | Modelos, servicios (`AuthService`, servicios de la API, notificaciones y loading), guards e interceptor |
| `shared/` | `SharedModule` con la ventana modal y el componente de carga masiva |
| `layout/` | `LayoutModule`: menú lateral cargado desde la base, barra superior y rutas privadas |
| `features/auth/` | Login y recuperación de contraseña |
| `features/home/` | Bienvenida |
| `features/dashboard/` | Indicadores del administrador con filtro de fechas |
| `features/users/` | Mantenimiento de usuarios y carga masiva |
| `features/cashes/` | Cajas y asignación de cajeros |
| `features/turns/` | Asignación de turnos del gestor |
| `features/cash-process/` | Procesos de caja del cajero |
| `features/clients/` | Mantenimiento de clientes y carga masiva |

### Comandos usados

```bash
npm install bootstrap@5.3.3 material-icons@1.13.12 xlsx@0.18.5
ng build
ng serve
```

### Decisiones

- **Desarrollo modularizado**: cada funcionalidad es un `NgModule` propio y las rutas los cargan
  con `loadChildren` (carga diferida).
- **Protección de rutas**: `authGuard` (sesión válida), `guestGuard` (el login no se muestra con sesión)
  y `roleGuard` (roles permitidos declarados en `data.roles` de cada ruta).
- **Interceptor único** (`ApiInterceptor`): adjunta el JWT y las cabeceras, controla el indicador de
  "procesando" y centraliza los mensajes de error y el cierre de sesión cuando el token expira.
- **Menú desde la base**: el layout pide `/api/menu` y pinta lo que devuelve la API según el rol.
- **Plantilla de diseño**: Bootstrap 5 con iconos Material.
- **Carga masiva**: el navegador lee el archivo `.xlsx` o `.csv` con SheetJS y envía las filas a la API;
  se puede descargar una plantilla con los encabezados.
- **Proxy de desarrollo** (`proxy.conf.json`): el frontend llama a `/api` y el servidor de Angular lo
  reenvía a la API, sin problemas de CORS.
- Los formularios validan los datos antes de consumir el servicio, con las mismas reglas de la API.

### Pruebas realizadas

Compilación de producción sin errores y recorrido automatizado en un navegador sin interfaz
(Playwright sobre Chrome) con los tres roles:

- Ruta privada sin sesión redirige al login; el login vacío muestra los errores y no llama al servicio.
- Cada rol ve su menú y todas sus pantallas cargan sin errores de consola.
- Una ruta de otro rol redirige a la bienvenida; el botón Salir regresa al login.
- Gestor: asigna un turno (`CS0003`).
- Cajero: abre la caja, busca un cliente inexistente, lo registra, inicia la atención, contrata,
  paga, cambia el servicio, cambia la forma de pago, finaliza el turno y cierra la caja.
- Administrador: crea un usuario desde el formulario y abre la carga masiva.

No se probó con un archivo Excel real la lectura de la carga masiva en la interfaz (sí el endpoint).
