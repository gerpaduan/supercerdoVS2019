---
pantalla: Usuarios.Permisos
titulo: Permisos del usuario
rol: admin
modulo: Usuarios
orden: 3
alias: Usuarios.GuardarPermisos
permiso: Usuario.NuevoUsuario (editar)
revisada: 2026-10-03
---
## Para qué sirve
Definir **qué puede hacer cada usuario en cada módulo**. Se llega desde [Usuarios](ayuda:pantalla/Usuarios.Index) con **Ver permisos**. (Un Admin no necesita permisos: puede todo.)

## Cómo se usa
Hay **una fila por módulo/formulario**. Para cada una:
- **Puede ver:** si puede consultar. **Días ver:** hasta cuántos días hacia atrás (**0 = solo hoy**).
- **Puede editar:** si puede modificar/crear. **Días editar:** modificar registros de hasta N días atrás.
- **Alcance:** **Propios** (solo lo que cargó él) o **Todos**.
Cambiá lo que necesites y tocá **Guardar permisos**: *"Los permisos se guardaron correctamente."*

## Reglas y efectos
- Sin **Puede ver** se desactiva *Días ver*; sin **Puede editar** se desactivan *Días editar* y *Alcance* (queda en Propios).
- **Cierres de Caja:** *Puede ver* activa también *Puede editar* con los mismos días (van juntos).
- **Usuario de producción:** aunque tildes Ventas, Finanzas, Fórmulas de Elaborados o Empleados/Liquidaciones, al guardar se **fuerzan a "sin acceso"**.
- Sin permiso, el usuario ve *"Acceso denegado"* al entrar a esa pantalla.
- **Puede operar POS** (en [Editar usuario](ayuda:pantalla/Usuarios.Editar)) modifica la fila de Ventas de esta grilla.

Ver también: [roles y permisos](ayuda:concepto/roles-y-permisos)
