# Evaluacion de AFIP

## Proyecto

`AFIP`

## Funcion actual

Proyecto vinculado a integraciones de facturacion o servicios relacionados con AFIP dentro del ecosistema actual.

## Evidencia relevada

- Proyecto `Class Library` en `.NET Framework 4.7.2`.
- Estructura relativamente contenida: `10` archivos `.cs` relevados.
- Servicios visibles: `ConsultarPadronService`, `GenerarFacturaService`, `LoginClass`, `AfipTest`.
- Usa `Web References` a `WSAA`, `WSFEHOMO`, `WSPSA13` y `WSPSA4`.
- Referencia solo a `Entidades` como proyecto local, sin dependencia directa a `Negocio`.
- Usa `System.ServiceModel`, `System.Web.Services` y configuracion por `Settings`.

## Responsabilidad principal

- Integracion funcional critica con AFIP.
- Soporte de facturacion electronica o procesos asociados.

## Dependencias visibles

- Referencias web o servicios externos.
- Configuracion y credenciales operativas.
- Posible dependencia de componentes historicos de .NET Framework.
- `Entidades`
- `Web References` de AFIP
- `System.ServiceModel`
- `System.Web.Services`

## Nivel de acoplamiento

- Alto

## Compatibilidad con .NET moderno

- Media

## Compatibilidad con Linux

- Baja

## Valor funcional acumulado

- Alto

## Riesgo operativo si se toca mal

- Alto

## Problemas o deuda tecnica probable

- Integraciones sensibles.
- Posibles dependencias legacy.
- Riesgo de mezclar conocimiento funcional con implementacion tecnica antigua.
- Dependencia a proxies generados por `Web References`.
- Configuracion legacy de endpoints y settings.

## Estrategia sugerida

- Encapsular

## Justificacion

- El conocimiento funcional es critico y debe preservarse.
- La implementacion tecnica probablemente necesite revision para NG.
- Frente a `wsAFIPvs2008`, este proyecto aparece mas encapsulado y con mejor perfil para servir como referencia principal de integracion AFIP.

## Dependencia temporal permitida en NG

- Si

## Condiciones para depender temporalmente

- Solo mediante un adaptador bien aislado.
- Sin contaminar el dominio ni el modelo de aplicacion de NG.

## Estrategia de salida futura

- Reemplazar gradualmente por una integracion moderna equivalente y validada.

## Estado del relevamiento

- Confirmado en primera pasada
