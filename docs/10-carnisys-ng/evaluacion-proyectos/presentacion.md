# Evaluacion de Presentacion

## Proyecto

`Presentacion`

## Funcion actual

Cliente `WinForms` principal del sistema actualmente en produccion.

## Evidencia relevada

- Proyecto `WinExe` en `.NET Framework 4.7.2`.
- Compilacion orientada a `x86`.
- Referencias fuertes a `System.Windows.Forms`, `System.Drawing`, `System.Web`, `PresentationFramework`, `PresentationCore` y `wsAFIPvs2008`.
- Superficie muy amplia: `214` archivos `.cs` relevados.
- Modulos visibles en carpetas: `Caja`, `Ventas`, `Compras`, `Cortes`, `Stock`, `Usuarios`, `Ticket`, `Balanza`, `CuentaCorriente`, `Embutidos`, `Proveedores`, `Movimientos`, `Personas`.
- Incluye componentes de impresion como `RawPrinterHelper`, `ESC_POS_Printer` y formularios de caja/POS.
- `formPOS` instancia directamente `Negocio.*`, usa `wsAFIPvs2008.formFacturaElectronica`, `Utilidades.SingletonLeerPeso`, `ConfigurationManager`, foco por teclado y logica operativa en la propia pantalla.

## Responsabilidad principal

- Interfaz de escritorio.
- Operacion diaria de usuarios.
- Flujos sensibles de POS, caja y administracion.

## Dependencias visibles

- Negocio
- Entidades
- Componentes visuales WinForms
- Utilidades
- wsAFIPvs2008
- Impresion termica
- Configuracion local por `App.config`
- Recursos graficos y formularios por modulo

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

- Fuerte dependencia de WinForms.
- Posible mezcla de UI con reglas de negocio.
- Eventos y flujos historicos sensibles.
- Integraciones operativas metidas en formularios.
- Dependencia a `x86`, lo que complica portabilidad.
- Acoplamiento alto a teclado, timers, foco, balanza e impresion dentro de la misma capa de presentacion.

## Estrategia sugerida

- Solo relevar

## Justificacion

- Es clave como fuente funcional, pero no debe trasladarse como base tecnica a NG.
- El volumen de formularios confirma que es el mejor insumo para relevamiento de comportamiento, no para migracion mecanica.
- `formPOS` por si solo muestra que POS concentra reglas operativas, perifericos, AFIP, parametros, estados y flujo de venta en una pantalla de alta sensibilidad.

## Dependencia temporal permitida en NG

- No

## Condiciones para depender temporalmente

- No aplica. Debe tomarse como referencia de comportamiento.

## Estrategia de salida futura

- Mantener convivencia mientras NG madura, luego retirar gradualmente por modulo o flujo.

## Estado del relevamiento

- Confirmado en primera pasada
