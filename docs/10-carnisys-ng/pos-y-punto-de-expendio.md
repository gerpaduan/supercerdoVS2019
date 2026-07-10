# POS y punto de expendio

## Prioridad

Es la parte mas sensible del sistema y debe condicionar decisiones de UX, rendimiento e integracion.

## Requisitos base

- flujo veloz con teclado
- foco automatico
- minimo numero de clics
- respuesta inmediata
- lectura de codigos
- integracion con balanza
- integracion con impresion termica
- tolerancia a errores de operador

## Aclaracion para flujos web

La UX operativa ya construida en la web MVC actual tambien debe tomarse como referencia valida cuando haya atajos de teclado, navegacion rapida y decisiones orientadas a minimizar el uso del mouse.

No debe asumirse que solo WinForms contiene conocimiento operativo valioso.

## Decision rectora para NG

En `CarniSys NG`, la `Web` actual debe considerarse la guia principal de UX operativa cuando el flujo web existente ya este bien resuelto.

WinForms debe usarse como respaldo funcional y de validacion, no como molde principal de experiencia de uso para la nueva plataforma web.

## Regla

Si una mejora visual empeora la velocidad real en caja, debe rechazarse.
