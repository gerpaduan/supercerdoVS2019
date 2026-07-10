# Multitenancy y catalogo global

## Multitenancy obligatorio

CarniSys NG debe diseñarse como multitenant real desde el inicio.

## Requisitos minimos

- aislamiento de datos por empresa
- usuarios y permisos por tenant
- configuracion por empresa
- sucursales o puntos de venta por empresa
- trazabilidad por tenant

## Catalogo global

Debe contemplarse un catalogo global de productos con `IdEmpresa = 0`.

## Puntos a definir

- reglas de herencia entre catalogo global y catalogo por empresa
- estrategia de personalizacion
- criterio de busqueda y resolucion
- impacto en precios, stock y permisos
