# Evaluacion de Carnisys.Balanza.Agent

## Proyecto

`Carnisys.Balanza.Agent`

## Funcion actual

Componente asociado a la integracion con balanzas u otros dispositivos de pesaje.

## Evidencia relevada

- Proyecto `WinExe` en `.NET Framework 4.7.2`.
- Estructura chica y bastante mas modular que el soporte de balanza embebido en `Presentacion`: `17` archivos `.cs`.
- Componentes visibles: `BalanzaReaderService`, `IBalanzaDriver`, `BalanzaDriverRegistry`, `KretzDriver`, `SystelDriver`, `LocalBalanzaServer`, `ConfigStore`, `ConfigForm`, `StartupRegistration`.
- Usa `SerialPort`, puertos `COM`, registro de Windows para autoarranque y una UI de configuracion propia.
- Expone un servidor local para publicar lecturas de peso.
- Separa marcas/modelos de balanza (`Kretz`, `Systel`) mediante drivers.

## Responsabilidad principal

- Comunicacion con balanzas.
- Soporte operativo a lectura o transferencia de peso.

## Dependencias visibles

- Dispositivos fisicos.
- Configuraciones de hardware.
- Posibles APIs o puertos especificos del entorno actual.
- WinForms
- puertos seriales
- registro de Windows
- servidor local HTTP/JSON

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

- Dependencia de hardware y entorno.
- Posible acoplamiento a Windows o configuraciones locales.
- Sigue siendo un ejecutable Windows con configuracion local y autoarranque en registro.
- No es directamente portable a Linux en su forma actual.

## Estrategia sugerida

- Encapsular

## Justificacion

- La integracion con balanza es critica, pero no conviene arrastrar su diseño actual al centro de NG.
- A diferencia del soporte incrustado en WinForms, este agente ya muestra una direccion mas sana: drivers, servicio lector y servidor local.
- Conviene tratarlo como referencia principal para rediseñar la integracion de dispositivos en NG.

## Dependencia temporal permitida en NG

- Si

## Condiciones para depender temporalmente

- Solo como adaptador externo.
- Con una interfaz clara del lado de NG.

## Estrategia de salida futura

- Rediseñar una capa de dispositivos multiplataforma o aislar la dependencia en un servicio especializado.

## Estado del relevamiento

- Confirmado en primera pasada
