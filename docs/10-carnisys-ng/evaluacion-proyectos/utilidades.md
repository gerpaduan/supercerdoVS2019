# Evaluacion de Utilidades

## Proyecto

`Utilidades`

## Funcion actual

Proyecto de helpers y componentes auxiliares usados por el sistema actual.

## Evidencia relevada

- Proyecto `Class Library` en `.NET Framework 4.7.2`.
- Superficie intermedia: `30` archivos `.cs` relevados.
- Mezcla helpers puros con formularios y utilidades de UI.
- Incluye contratos transversales como `IEmpresaContext` e `IParametrosContext`.
- Incluye componentes sensibles como `Conexion`, `Db`, `SingletonLeerPeso`, `PasswordSecurity`, `PerformanceInstrumentation`, `GenerarCodigoBarra`, `GenerarDocs`.
- Tambien contiene formularios como `FormLogin`, `FormAppConfig`, `FormPesoBalanza`, `FormTestBalanza`, `BarraProgreso` y `Util_Form`.
- Usa referencias a `System.Windows.Forms`, `System.Management`, `System.Web`, `PresentationFramework`, bibliotecas de codigos de barra y componentes criptograficos.

## Responsabilidad principal

- Funciones transversales.
- Soporte tecnico a distintos modulos.

## Dependencias visibles

- Posible uso compartido por varios proyectos legacy.
- `Entidades`
- WinForms
- Windows/WMI por `System.Management`
- generacion de codigos de barra
- lectura de peso
- configuracion local

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

- Helpers genericos mezclados con dependencias especificas.
- Riesgo de utilidades que esconden acoplamientos fuertes.
- Mezcla de infraestructura, UI, contexto, seguridad y perifericos en un mismo proyecto.
- Contiene piezas muy reutilizables y otras fuertemente atadas a Windows.

## Estrategia sugerida

- Reutilizar parcialmente

## Justificacion

- Algunas utilidades pueden rescatarse, pero requieren revision individual.
- `IEmpresaContext` e `IParametrosContext` tienen valor conceptual claro para NG.
- `SingletonLeerPeso` y formularios de balanza muestran que parte del soporte de dispositivos esta mezclado en utilidades y no deberia trasladarse asi.

## Dependencia temporal permitida en NG

- No

## Condiciones para depender temporalmente

- Solo de forma excepcional y documentada.

## Estrategia de salida futura

- Extraer utilidades validas y recrearlas de forma nativa en NG.

## Estado del relevamiento

- Confirmado en primera pasada
