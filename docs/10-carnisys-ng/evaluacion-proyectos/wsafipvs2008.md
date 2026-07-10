# Evaluacion de wsAFIPvs2008

## Proyecto

`wsAFIPvs2008`

## Funcion actual

Proyecto legacy asociado a servicios web de AFIP o integraciones historicas de facturacion.

## Evidencia relevada

- Proyecto `WinExe` en `.NET Framework 4.7.2`.
- Estructura chica pero muy acoplada: `15` archivos `.cs` relevados.
- Contiene un formulario propio `formFacturaElectronica`.
- Incluye `Program.cs`, `EmpresaContextWin.cs`, `RawPrinterHelper` y `CrearTicket`.
- Referencia a `Entidades`, `Negocio` y `Utilidades`.
- Usa `Web References` a `WSAA`, `WSFEHOMO`, `WSPSA13` y `WSPSA4`.
- `Presentacion` lo consume directamente desde `FormPrincipal` y `formPOS`.

## Responsabilidad principal

- Conectividad con servicios externos de AFIP.
- Soporte a operaciones de facturacion vinculadas.

## Dependencias visibles

- Web references historicas.
- Tecnologias antiguas de integracion.
- WinForms
- `Negocio`
- `Utilidades`
- impresion local
- contexto Windows

## Nivel de acoplamiento

- Alto

## Compatibilidad con .NET moderno

- Baja

## Compatibilidad con Linux

- Baja

## Valor funcional acumulado

- Alto

## Riesgo operativo si se toca mal

- Alto

## Problemas o deuda tecnica probable

- Tecnologia obsoleta.
- Integracion sensible.
- Riesgo de conocimiento funcional atrapado en implementacion antigua.
- Mezcla de UI, impresion y servicios AFIP en el mismo proyecto.
- Dependencia directa del flujo operativo de escritorio.
- Perfil poco compatible con una migracion limpia a `ASP.NET Core`.

## Estrategia sugerida

- Solo relevar

## Justificacion

- Debe preservarse el conocimiento y comportamiento, pero no conviene usarlo como dependencia estructural del nuevo sistema.
- Su valor principal hoy es documental y funcional: muestra como se resolvio facturacion electronica en el cliente de escritorio.
- La existencia paralela del proyecto `AFIP` hace menos justificable arrastrar `wsAFIPvs2008` al nuevo backend.

## Dependencia temporal permitida en NG

- No

## Condiciones para depender temporalmente

- No aplica salvo necesidad excepcional, aislada y documentada.

## Estrategia de salida futura

- Reemplazo por una integracion moderna basada en contratos nuevos y validacion funcional estricta.

## Estado del relevamiento

- Confirmado en primera pasada
