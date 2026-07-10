# Migracion por etapas

## Estrategia general

La migracion debe ser gradual y validarse contra el sistema actual.

Durante la transicion, el nuevo proyecto podra convivir en la misma solucion con los proyectos actuales, siempre que mantenga aislamiento arquitectonico.

## Etapas sugeridas

1. Relevamiento funcional y tecnico del sistema vigente.
2. Alta de proyectos `CarniSys.NG` dentro de la misma solucion con separacion clara del sistema actual.
3. Definicion de arquitectura base y solucion NG.
4. Construccion de dominio, seguridad y multitenancy.
5. Migracion de modulos de menor riesgo.
6. Migracion progresiva de POS, caja y punto de expendio.
7. Encapsulamiento o rediseño de integraciones criticas.
8. Pruebas paralelas contra comportamiento actual.
9. Salida gradual a produccion.

## Validaciones minimas por etapa

- comportamiento funcional
- permisos
- rendimiento
- integraciones
- no regresion operativa
