---
pantalla: Usuarios.Permisos
titulo: Permisos del usuario
rol: admin
modulo: Usuarios
orden: 3
alias: Usuarios.GuardarPermisos
permiso: Usuario.NuevoUsuario (editar)
revisada: 2026-11-10
---
## Para qué sirve
**Vista avanzada** de los permisos: la grilla completa, con todas las filas y los cinco controles. Para elegir los permisos paso a paso, agrupados por módulo y mostrando solo lo que cambia algo, usá la pestaña **Permisos** de la [ficha del usuario](ayuda:pantalla/Usuarios.Asistente) (se llega a esta vista avanzada con el botón **Vista avanzada de permisos** de la ficha). Esta grilla se mantiene para casos puntuales y para ver los permisos que el asistente no ofrece. (Un Admin no necesita permisos: puede todo.)

## Cómo se usa
Hay **una fila por módulo/formulario**. Para cada una:
- **Puede ver:** si puede consultar. **Días ver:** hasta cuántos días hacia atrás (**0 = solo hoy**).
- **Puede editar:** si puede modificar/crear. **Días editar:** modificar registros de hasta N días atrás.
- **Alcance:** **Propios** (solo lo que cargó él) o **Todos**.
Cambiá lo que necesites y tocá **Guardar permisos**: *"Los permisos se guardaron correctamente."*

## Reglas y efectos
- Sin **Puede ver** se desactiva *Días ver*; sin **Puede editar** se desactivan *Días editar* y *Alcance* (queda en Propios).
- **Cierres de Caja:** *Puede ver* activa también *Puede editar* con los mismos días (van juntos).
- **Sin efecto hoy:** una fila con la marca *sin efecto hoy* es un permiso que ninguna pantalla controla porque esa pantalla solo existe en el sistema clásico (Costo por tipo de cobro, Exportar/Imprimir lista de precios). Tildarla no cambia nada; el asistente no la muestra.
- **Reportes (filas «Reporte …»):** cada tipo de reporte se habilita con su propia fila (*Puede ver*; *Días ver* = hasta cuántos días atrás puede poner la fecha «desde»). Antes se habilitaban todos con *Stocks*.
- **Productos y Tipos Productos:** *Puede editar* = crear, modificar y eliminar (en Productos también cambiar precios).
- **Los días solo restringen donde el sistema compara con la fecha del registro** (ver el asistente). En el resto el sistema mira siempre «hoy» y cualquier valor desde 0 da lo mismo.
- **Usuario de producción:** aunque tildes Ventas, Finanzas, Fórmulas de Elaborados o Empleados/Liquidaciones, al guardar se **fuerzan a "sin acceso"**.
- Sin permiso, el usuario ve *"Acceso denegado"* al entrar a esa pantalla.
- **Puede operar POS** (en [Editar usuario](ayuda:pantalla/Usuarios.Editar)) tilda o destilda *Puede editar* de la fila de Ventas de esta grilla (registrar ventas). No da acceso al POS por sí solo: para entrar al punto de venta hacen falta los permisos «POS - …».

Ver también: [roles y permisos](ayuda:concepto/roles-y-permisos)
