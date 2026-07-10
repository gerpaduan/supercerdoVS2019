# Organizacion de la solucion

## Estructura sugerida

- `src/CarniSys.NG.Domain`
- `src/CarniSys.NG.Application`
- `src/CarniSys.NG.Infrastructure`
- `src/CarniSys.NG.Web`
- `src/CarniSys.NG.Integrations.AFIP`
- `src/CarniSys.NG.Integrations.Devices`
- `tests/CarniSys.NG.UnitTests`
- `tests/CarniSys.NG.IntegrationTests`

## Proyectos a crear primero

Primera base recomendada dentro de `CarniSys.sln`:

- `CarniSys.NG.Domain`
- `CarniSys.NG.Application`
- `CarniSys.NG.Infrastructure`
- `CarniSys.NG.Web`
- `CarniSys.NG.Integrations.AFIP`
- `CarniSys.NG.Integrations.Devices`
- `CarniSys.NG.UnitTests`
- `CarniSys.NG.IntegrationTests`

No conviene crear mas proyectos al inicio salvo necesidad real validada.

## Convivencia en la misma solucion

CarniSys NG puede crearse dentro de la misma solucion `CarniSys.sln` donde hoy conviven `Presentacion` WinForms, `Web` MVC y otros proyectos historicos.

Esto se considera conveniente durante la transicion porque permite relevamiento, comparacion y migracion gradual sin romper produccion.

## Regla de separacion

Aunque convivan en la misma solucion, los proyectos nuevos deben quedar claramente aislados de los actuales:

- nombres nuevos con prefijo `CarniSys.NG`
- carpetas propias
- dependencias minimizadas hacia proyectos legacy
- documentacion de toda reutilizacion temporal
- arquitectura propia del nuevo sistema

## Modulos funcionales sugeridos

- Ventas
- POS
- Caja
- Stock
- Compras
- Clientes
- Proveedores
- Facturacion
- Seguridad
- Configuracion

## Criterio

La organizacion debe facilitar migracion gradual, relevamiento por modulo y validacion contra el sistema actual.

La estructura detallada y las referencias sugeridas quedaron especificadas en [estructura-inicial-proyectos-ng.md](./estructura-inicial-proyectos-ng.md).
