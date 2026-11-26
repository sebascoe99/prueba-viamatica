# 01 - Creación de los proyectos base

**Herramienta:** Claude Code (agente de IA en terminal)

## Prompt

> Tengo una prueba técnica de .NET con Angular. Te paso el enunciado y me vas diciendo paso a paso
> qué vamos haciendo. Primero hay que enviar el primer commit con la creación de los dos proyectos
> (Backend y Frontend). Dime cómo se crean los proyectos desde cero.

## Resultado

El agente revisó las herramientas instaladas, instaló el SDK de .NET 10 y generó la estructura base
de ambos proyectos con los comandos oficiales de cada CLI. No se escribió lógica de negocio en este paso.

### Comandos usados

Instalación del SDK:

```bash
winget install --id Microsoft.DotNet.SDK.10 -e
```

Repositorio:

```bash
git init -b main
```

Backend (arquitectura Onion en 4 capas):

```bash
cd Backend
dotnet new sln -n GestionCaja
dotnet new classlib -n GestionCaja.Domain         -o src/GestionCaja.Domain
dotnet new classlib -n GestionCaja.Application    -o src/GestionCaja.Application
dotnet new classlib -n GestionCaja.Infrastructure -o src/GestionCaja.Infrastructure
dotnet new webapi   -n GestionCaja.Api            -o src/GestionCaja.Api --use-controllers

dotnet sln add src/GestionCaja.Domain src/GestionCaja.Application src/GestionCaja.Infrastructure src/GestionCaja.Api

dotnet add src/GestionCaja.Application    reference src/GestionCaja.Domain
dotnet add src/GestionCaja.Infrastructure reference src/GestionCaja.Application
dotnet add src/GestionCaja.Api            reference src/GestionCaja.Application src/GestionCaja.Infrastructure

dotnet new gitignore
```

Frontend (Angular 20 basado en módulos):

```bash
npx @angular/cli@20 new Frontend --no-standalone --routing --style=scss --ssr=false --skip-git --skip-tests
```

### Decisiones

- **.NET 10** por ser la versión LTS vigente.
- **Onion Architecture**: las dependencias apuntan hacia el dominio
  (Api -> Infrastructure -> Application -> Domain).
- **`--no-standalone`** en Angular para trabajar con NgModules y carga diferida de rutas,
  como pide el enunciado (desarrollo modularizado y rutas llamadas a los módulos).
