# Evaluacion de Entidades

## Proyecto

`Entidades`

## Funcion actual

Proyecto de entidades o modelos compartidos del sistema actual.

## Evidencia relevada

- Proyecto `Class Library` en `.NET Framework 4.7.2`.
- Estructura relativamente amplia pero ordenada: `41` archivos `.cs` relevados.
- No referencia proyectos locales desde el `.csproj`.
- Contiene modelos centrales del negocio: `Venta`, `Compra`, `Corte`, `Persona`, `Sucursal`, `Usuario`, `FacturaElectronica`, `LineaVenta`, `TemporalLineaVenta`, `Permisos`, `Parametros`, `EgresoCaja`, `Pago`, `Empresa`.
- Incluye conceptos nuevos o ya alineados con NG, como `CatalogoGlobalImportacionProducto` y `UsuarioPasswordResetToken`.
- Las referencias del proyecto son moderadas y mayormente base (`System`, `DataAnnotations`, `System.Data`).

## Responsabilidad principal

- Representacion de datos y estructuras de intercambio.

## Dependencias visibles

- Negocio
- Datos
- Posible uso transversal en WinForms y Web
- Modelos usados por toda la solucion actual
- `System.ComponentModel.DataAnnotations`

## Nivel de acoplamiento

- Medio

## Compatibilidad con .NET moderno

- Media

## Compatibilidad con Linux

- Media

## Valor funcional acumulado

- Medio

## Riesgo operativo si se toca mal

- Medio

## Problemas o deuda tecnica probable

- Modelos anemicos.
- Mezcla de conceptos de dominio con DTOs o estructuras de persistencia.
- Posible coexistencia de entidades de dominio con estructuras pensadas para UI o persistencia historica.
- Nomenclatura irregular en algunos archivos, lo que sugiere crecimiento organico.

## Estrategia sugerida

- Reutilizar parcialmente

## Justificacion

- Puede aportar nombres, conceptos y estructuras utiles, pero no debe adoptarse sin depuracion conceptual.
- Es el proyecto con mejor perfil para servir como mapa del dominio actual.
- Aun asi, NG debera redefinir sus propias entidades, value objects y DTOs, evitando copiar modelos sin filtrar.

## Dependencia temporal permitida en NG

- No

## Condiciones para depender temporalmente

- Solo como referencia documental o de mapeo.

## Estrategia de salida futura

- Redefinir entidades de dominio y DTOs propios en NG.

## Estado del relevamiento

- Confirmado en primera pasada
