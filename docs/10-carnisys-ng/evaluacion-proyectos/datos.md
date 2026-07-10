# Evaluacion de Datos

## Proyecto

`Datos`

## Funcion actual

Capa de acceso a datos del sistema actual.

## Evidencia relevada

- Proyecto `Class Library` en `.NET Framework 4.7.2`.
- Referencias directas a `Entidades` y `Utilidades`.
- Estructura compacta y espejo de `Negocio`: `13` archivos `.cs`.
- Uso de `DataSet` tipado y carpeta `DB-Procedures`.
- Dependencia explícita de paquetes legacy/compatibilidad como `Microsoft.Bcl.AsyncInterfaces`, `System.Threading.Tasks.Extensions` y `System.Runtime.CompilerServices.Unsafe`.

## Responsabilidad principal

- Consultas a base de datos.
- Ejecucion de procedimientos almacenados.
- Persistencia y transacciones.

## Dependencias visibles

- SQL Server.
- Esquema de base actual.
- Procedimientos almacenados y consultas historicas.
- `Entidades`
- `Utilidades`
- `DB-Procedures`

## Nivel de acoplamiento

- Alto

## Compatibilidad con .NET moderno

- Media

## Compatibilidad con Linux

- Media

## Valor funcional acumulado

- Alto

## Riesgo operativo si se toca mal

- Alto

## Problemas o deuda tecnica probable

- Mezcla de acceso a datos con decisiones de negocio.
- Dependencia fuerte de estructura historica.
- Riesgo de consultas complejas con efectos laterales.
- Acoplamiento a `DataTable`, `DataSet` y contratos de datos heredados.
- Posible correspondencia uno a uno con clases de `Negocio`, lo que sugiere una capa de datos no abstraida sino expuesta.

## Estrategia sugerida

- Solo relevar

## Justificacion

- Debe usarse para entender persistencia actual, no como dependencia estructural final de NG.
- La estructura actual favorece el relevamiento de tablas, procedimientos y transacciones, pero no una reutilizacion limpia en una arquitectura `ASP.NET Core`.
- Es clave para mapear comportamiento actual de persistencia antes de rediseñar infraestructura propia.

## Dependencia temporal permitida en NG

- No

## Condiciones para depender temporalmente

- No aplica salvo excepcion muy justificada y documentada.

## Estrategia de salida futura

- Reescribir la infraestructura de persistencia con contratos propios de NG.

## Estado del relevamiento

- Confirmado en primera pasada
