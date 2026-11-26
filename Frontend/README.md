# Frontend - Gestión de Caja

Aplicación web en Angular 20 para la gestión de caja de una empresa de servicios de internet.

## Tecnologías

- Angular 20 con NgModules y carga diferida de rutas
- Bootstrap 5 como plantilla de diseño e iconos Material
- SheetJS (`xlsx`) para leer los archivos de la carga masiva
- nginx para servir la aplicación en Docker

## Requisitos

- Node.js 22
- La API corriendo en `http://localhost:5009` (ver `../Backend/README.md`)

## Ejecutar en desarrollo

```bash
npm install
npm start
```

La aplicación queda en `http://localhost:4200`. Las llamadas a `/api` se reenvían a la API
mediante `proxy.conf.json`.

## Compilar

```bash
npx ng build
```

El resultado queda en `dist/Frontend/browser`.

## Estructura

```
src/app
├── core            Modelos, servicios, guards e interceptor
├── shared          Componentes reutilizables (modal y carga masiva)
├── layout          Menú lateral, barra superior y rutas privadas
└── features
    ├── auth            Login y recuperación de contraseña
    ├── home            Bienvenida
    ├── dashboard       Indicadores (administrador)
    ├── users           Usuarios y carga masiva
    ├── cashes          Cajas y asignación de cajeros
    ├── turns           Asignación de turnos (gestor)
    ├── cash-process    Procesos de caja (cajero)
    └── clients         Clientes y carga masiva (cajero)
```

## Puntos del enunciado

| Requisito | Dónde está |
|---|---|
| Desarrollo modularizado | Un módulo por funcionalidad en `features/` |
| Rutas llamadas a los módulos | `loadChildren` en `app-routing-module.ts` y `layout/layout.module.ts` |
| Protección de rutas | `core/guards/auth.guard.ts` (`authGuard`, `guestGuard`, `roleGuard`) |
| Interceptor | `core/interceptors/api.interceptor.ts` (JWT, loading y manejo de errores) |
| Menú desde la base según el rol | `layout/layout.component.ts` consume `GET /api/menu` |
| Mensajes de error y de éxito | `NotificationService` en `core/services/ui.service.ts` |
| Loading | `LoadingService`, activado por el interceptor |
| Carga masiva (.xlsx o .csv) | `shared/bulk-upload.component.ts` |

## Pantallas por rol

| Rol | Pantallas |
|---|---|
| Administrador | Inicio, Dashboard, Usuarios, Cajas |
| Gestor | Inicio, Usuarios, Cajas, Asignación de turnos |
| Cajero | Inicio, Procesos de caja, Clientes |

## Carga masiva

En Usuarios y Clientes, el botón **Carga masiva** permite descargar una plantilla, llenarla y subirla.
Se aceptan archivos `.xlsx` y `.csv`. El resultado se muestra fila por fila.
