# Sintesis de decisiones por proyecto legacy

## Objetivo

Concentrar la recomendacion actual de migracion para cada proyecto principal del sistema legacy y facilitar decisiones consistentes al construir `CarniSys NG`.

## Estado

Este documento resume una primera pasada de relevamiento tecnico.

Las decisiones podran ajustarse cuando aparezca evidencia funcional mas profunda, pero el criterio base ya queda definido.

## Matriz de decision

| Proyecto | Valor funcional | Riesgo | Estrategia sugerida | Uso en NG |
|---|---|---:|---|---|
| `Presentacion` | Alto | Alto | Solo relevar | Fuente funcional principal |
| `Negocio` | Alto | Alto | Reutilizar parcialmente | Extraccion gradual de reglas |
| `Datos` | Alto | Alto | Solo relevar | Mapa de persistencia actual |
| `Entidades` | Medio/Alto | Medio | Reutilizar parcialmente | Mapa conceptual del dominio |
| `Utilidades` | Medio | Medio | Reutilizar parcialmente | Seleccion puntual de piezas |
| `Web` | Alto | Alto | Reutilizar parcialmente | Relevar flujos web y permisos |
| `AFIP` | Alto | Alto | Encapsular | Referencia principal de integracion |
| `wsAFIPvs2008` | Alto | Alto | Solo relevar | Referencia funcional legacy |
| `Carnisys.Balanza.Agent` | Alto | Alto | Encapsular | Referencia principal de dispositivos |

## Regla general resultante

- No migrar proyectos legacy completos al nuevo core.
- Extraer reglas y conocimiento, no arrastrar acoplamientos.
- Encapsular integraciones criticas.
- Redefinir el dominio y la infraestructura de `CarniSys NG` con contratos propios.

## Proyectos que sirven principalmente para relevamiento

- `Presentacion`
- `Datos`
- `wsAFIPvs2008`

## Proyectos que sirven para extraccion o reutilizacion parcial

- `Negocio`
- `Entidades`
- `Utilidades`
- `Web`

## Proyectos que conviene tomar como base de encapsulamiento

- `AFIP`
- `Carnisys.Balanza.Agent`

## Consecuencia para la arquitectura NG

`CarniSys NG` no debera depender estructuralmente de la solucion legacy.

La solucion actual servira como:

- fuente de reglas;
- fuente de validacion;
- fuente de comportamiento real;
- apoyo temporal durante la transicion.

Pero el nuevo sistema debera tener:

- proyectos propios;
- dominio propio;
- infraestructura propia;
- integraciones propias;
- pruebas propias.

## Consecuencia para UX y operacion

La referencia principal de UX operativa para `CarniSys NG` sera la `Web` actual cuando el flujo ya haya sido trabajado con foco en teclado, velocidad y minimo uso del mouse.

`Presentacion` WinForms se utilizara principalmente para:

- validar reglas;
- detectar diferencias funcionales;
- rescatar casos historicos sensibles.
