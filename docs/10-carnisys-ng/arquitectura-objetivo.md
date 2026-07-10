# Arquitectura objetivo

## Vision

CarniSys NG debera implementarse como un monolito modular en `ASP.NET Core`, preparado para evolucionar sin depender de Windows, IIS ni de una UI de escritorio.

## Capas sugeridas

- `CarniSys.NG.Domain`
- `CarniSys.NG.Application`
- `CarniSys.NG.Infrastructure`
- `CarniSys.NG.Web`
- `CarniSys.NG.Integrations.AFIP`
- `CarniSys.NG.Integrations.Devices`
- `CarniSys.NG.UnitTests`
- `CarniSys.NG.IntegrationTests`

## Principios

- Priorizar reglas de negocio sobre tecnologia.
- Evitar microservicios al inicio.
- Mantener separacion de responsabilidades.
- Diseñar para SQL Server hoy y PostgreSQL a futuro.
- Permitir ejecucion en Linux y Windows.

## Regla de arranque

La primera version de la solucion NG no debera nacer con proyectos de mas.

Debe comenzar con un conjunto corto de proyectos con responsabilidades claras, evitando:

- carpetas sin uso;
- capas vacias;
- librerias prematuras;
- separaciones artificiales.
