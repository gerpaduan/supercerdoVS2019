---
pantalla: Usuarios.Editar
titulo: Nuevo / Modificar usuario
rol: admin
modulo: Usuarios
orden: 2
alias: Usuarios.Guardar
permiso: Usuario.NuevoUsuario (editar)
revisada: 2026-11-10
---
## Para qué sirve
> **Desde 2026-11-10 esta pantalla ya no se abre desde el listado:** la dirección *Usuarios/Editar* redirige a la [ficha y alta de usuarios](ayuda:pantalla/Usuarios.Asistente) (pestañas Datos, Ingreso y seguridad y Permisos), que hace lo mismo y más. Lo de abajo queda como referencia de lo que significa cada campo; esta vista solo reaparece si falla un guardado antiguo.

Crear un usuario o modificar uno existente. Se llega desde [Usuarios](ayuda:pantalla/Usuarios.Index) con **Modificar**. Para crear un usuario con sus permisos de una vez se usa el [asistente](ayuda:pantalla/Usuarios.Asistente).

## Cómo se usa
1. En un usuario existente la pantalla abre en **solo lectura**: tocá **Modificar** para habilitar los campos. En un alta ya están habilitados.
2. Completá los datos y tocá **Guardar usuario**. Avisos: *"El usuario se creó correctamente."* / *"El usuario se actualizó correctamente."*

## Campos
- **Nombre** y **Usuario** (nombre de acceso): obligatorios. El usuario no puede repetirse **en ninguna empresa** del sistema.
- **Email:** opcional pero **importante**: sirve para recuperar la contraseña, para que el usuario se desbloquee solo y para autorizar su celular. No puede repetirse dentro de la empresa.
- **ID Sucursal User:** sucursal por defecto (*"Sin sucursal por defecto"* es válido).
- **Clave:** obligatoria en un alta. En una edición, vacía = se conserva la actual. Debe tener **mínimo 8 caracteres, con letra, número y un carácter especial, sin espacios**, y no ser igual al usuario ni una clave demasiado común.
- **Admin:** acceso total a la empresa; está exento de horario laboral, de dispositivo seguro y no usa PIN. Tildar *Admin* destilda *Usuario de producción*.
- **Activo:** un usuario inactivo no puede ingresar.
- **Permitir login fuera de sucursal:** lo exceptúa de la validación de ubicación (GPS) que tienen algunas sucursales (los administradores ya están exceptuados). No tiene relación con elegir otra sucursal.
- **Usuario de producción:** cuenta compartida de un sector. Al guardar movimientos/stock/elaborados no pide contraseña: se elige de una lista quién lo hizo. **No puede ser Admin**, pierde Ventas, Finanzas y las Fórmulas de Elaborados, y queda sin *Ajuste de stock*. Ver [roles](ayuda:concepto/roles-y-permisos).
- **Requiere dispositivo seguro:** le exige a **este** usuario ingresar solo desde un dispositivo autorizado, aunque la empresa no lo exija a todos (no aplica a administradores). Ver [dispositivo seguro](ayuda:concepto/dispositivo-seguro).
- **Puede operar POS** (viene **tildado** en un alta): tilda *Ventas → Puede editar* (registrar ventas, edición solo del día). **No da acceso al POS por sí solo**: para entrar al punto de venta hacen falta los permisos «POS - …». Se apaga solo si es usuario de producción. Los permisos finos se ajustan en [Permisos](ayuda:pantalla/Usuarios.Permisos).
- **Minutos de inactividad** (viene en **60**): si pasan esos minutos sin que el usuario toque el sistema, se le pregunta "¿Extender sesión?" y, si no responde en 5 minutos, se cierra la sesión (en el POS se bloquea y pide su contraseña). **0 = nunca.** Un usuario de producción nunca se bloquea, sea cual sea el valor. Entre 0 y 1440. Ver [bloqueo por inactividad](ayuda:concepto/bloqueo-por-inactividad).

## Reglas y efectos
- Cambiar la clave **no** cambia el PIN del usuario.
- Un usuario de producción no puede ser Admin: el sistema lo impide.
- Los avisos de error aparecen en un recuadro rojo arriba.
- **PENDIENTE:** el permiso de *Puede operar POS* está atado al formulario de Ventas por un número fijo en el código; si cambiara en la base, dejaría de funcionar.

Ver también: [mapa de configuraciones](ayuda:referencia/configuraciones)
