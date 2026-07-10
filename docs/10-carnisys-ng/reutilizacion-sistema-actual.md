# Reutilizacion del sistema actual

## Proyectos actuales identificados

- `Presentacion`
- `Utilidades`
- `Entidades`
- `Negocio`
- `Datos`
- `wsAFIPvs2008`
- `Web`
- `AFIP`
- `Carnisys.Balanza.Agent`

## Recomendacion inicial

- `Presentacion`: usar como referencia funcional, no como base tecnica.
- `Web`: relevar para rescatar flujos y decisiones utiles.
- `Negocio`: analizar para extraer reglas y casos de uso.
- `Datos`: usar para mapear persistencia actual y puntos sensibles.
- `Entidades`: revisar y seleccionar conceptos reutilizables.
- `Utilidades`: filtrar utilidad por utilidad.
- `AFIP` y `wsAFIPvs2008`: encapsular conocimiento funcional critico.
- `Carnisys.Balanza.Agent`: tomar como referencia para rediseñar integracion de dispositivos.

## Ficha pendiente por proyecto

Cada proyecto debera documentarse luego con:

- valor funcional
- deuda tecnica
- dependencias
- compatibilidad con .NET moderno
- compatibilidad con Linux
- riesgo operativo
- decision final

## Siguiente paso documental

Las fichas individuales quedaron organizadas en [evaluacion-proyectos](./evaluacion-proyectos/README.md).
