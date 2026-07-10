# Evaluacion de Negocio

## Proyecto

`Negocio`

## Funcion actual

Capa donde probablemente se concentra una parte importante de las reglas de negocio del sistema actual.

## Evidencia relevada

- Proyecto `Class Library` en `.NET Framework 4.7.2`.
- Referencias directas a `Datos`, `Entidades` y `Utilidades`.
- Estructura chica y concentrada: `14` archivos `.cs`.
- Clases principales alineadas por modulo: `Venta`, `CierreCaja`, `Compra`, `Corte`, `CuentaCorriente`, `Persona`, `Sucursal`, `Usuario`, `Parametros`.
- Uso directo de `TransactionScope`, `DataTable`, `DataRow` y tipos concretos de `Datos`.

## Responsabilidad principal

- Logica funcional.
- Orquestacion de procesos.
- Reglas validadas por uso real.

## Dependencias visibles

- Entidades
- Datos
- Posible consumo desde Presentacion y Web
- Uso de `Utilidades`
- Dependencia directa de `System.Transactions`
- Tipos concretos como `Datos.Venta`, `Datos.CierreCaja`, `Datos.Sucursal`

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

- Mezcla de logica valiosa con deuda historica.
- Acoplamiento a datos o estructuras de UI.
- No trabaja contra interfaces de persistencia propias.
- Parte de la logica devuelve o transforma `DataTable`.
- Mezcla reglas de negocio con coordinacion transaccional e invocacion directa a capa de datos.

## Estrategia sugerida

- Reutilizar parcialmente

## Justificacion

- Debe analizarse para extraer reglas y casos de uso, pero no conviene tomarlo completo como base de NG.
- La clase `Venta` confirma valor funcional alto: comisiones, lineas anuladas, expendios, egresos de caja, cuenta corriente y factura electronica.
- La clase `CierreCaja` confirma que parte de la logica sigue atada a `DataTable` y a lecturas directas desde `Datos`.

## Dependencia temporal permitida en NG

- Si

## Condiciones para depender temporalmente

- Solo en etapas controladas.
- Con encapsulamiento y plan de reemplazo.

## Estrategia de salida futura

- Migrar reglas a `Domain` y `Application` de NG de forma progresiva y validada.

## Estado del relevamiento

- Confirmado en primera pasada
