---
pantalla: Usuarios.Index
titulo: Usuarios
rol: admin
modulo: Usuarios
orden: 1
alias: Usuarios.DesbloquearUsuario, Usuarios.QuitarPin
permiso: Usuario.VerUsuarios (ver) / Usuario.NuevoUsuario (editar)
revisada: 2026-11-10
---
## Para qué sirve
Ver y administrar los **usuarios de tu empresa**: crearlos, modificarlos, darles permisos, desbloquearlos y quitarles el PIN. Está en *Configuración → Usuarios*. Los botones solo aparecen si sos administrador o tenés permiso de **edición** de usuarios; sin eso ves el listado sin acciones.

## Dónde está
Abrí el menú lateral, tocá **Configuración (1)** y después **Usuarios (2)**.

![Menú lateral: Configuración (1) y Usuarios (2)](media:Usuarios.Index/menu.png)

## Qué ves
Un listado de los usuarios de la empresa, por nombre. Columnas: **Usuario** (iniciales, nombre, usuario e email), **Tipo** (*Empleado*, *Administrador* o *Producción*, con color), **Estado** (*Activo* / *Inactivo*, y *Bloqueado* o *Bloqueo parcial* cuando corresponde), **Sucursal** (por nombre) e **Ingreso** (etiquetas *POS* si entra al punto de venta, *PIN* si tiene clave rápida y *Disp. seguro* si solo puede ingresar desde un dispositivo autorizado).

> PENDIENTE (2026-11-10): las capturas y el video de esta pantalla son del diseño anterior (rediseño del mismo día); regenerarlas.

- **Nuevo usuario** (arriba a la derecha) → [ficha y alta](ayuda:pantalla/Usuarios.Asistente).
- **Buscar:** filtra en vivo por nombre, usuario o email.
- **Filtros** (botones con su cantidad): *Todos*, *Activos*, *Administradores*, *Producción*, *Bloqueados* e *Inactivos*. Se combinan con el buscador. **Al abrir la pantalla está elegido *Activos***: para ver los inactivos tocá *Inactivos* o *Todos*. *Bloqueados* incluye el bloqueo parcial (solo en dispositivos no autorizados).
- **Tocar una fila** abre la ficha del usuario.
- **Menú ⋯** de cada fila (ver más abajo).

## Acciones
Cada fila tiene un menú **⋯** a la derecha (solo si sos administrador o tenés permiso de edición de usuarios): **Modificar** y **Permisos** (abren la [ficha](ayuda:pantalla/Usuarios.Asistente), la segunda ya en la pestaña Permisos) aparecen siempre; **Desbloquear** y **Quitar PIN** solo cuando corresponden.

- **Desbloquear** (solo si está bloqueado total o parcialmente; pide confirmación): limpia **ambos** bloqueos y los intentos fallidos, y deja constancia en la [auditoría](ayuda:pantalla/AuditoriaLogin.Index) de quién lo hizo. El usuario también puede desbloquearse solo por mail si tiene mail cargado. Ver [bloqueo de usuarios](ayuda:concepto/bloqueo-de-usuarios).
- **Quitar PIN** (solo si el usuario tiene [PIN](ayuda:concepto/clave-rapida-pin)): lo elimina; el usuario sigue entrando con su contraseña y puede crear otro PIN. Útil si lo olvidó o sospechás que otra persona lo conoce. También queda en la auditoría. (Las dos acciones también están en la cabecera de la ficha.)
- La grilla completa anterior es la *Vista avanzada de permisos* ([Permisos del usuario](ayuda:pantalla/Usuarios.Permisos)), que se abre desde la ficha.

## Cómo se hace: buscar un usuario y abrir su ficha
Video corto, sin sonido: escribís en **Buscar usuario**, la lista se filtra y con **Modificar** abrís la ficha. No se guarda nada hasta que toques **Guardar**.

![Video: buscar un usuario y abrir su ficha con Modificar](media:Usuarios.Index/buscar-y-modificar.webm)

## Reglas
- Solo ves y administrás usuarios de **tu empresa**.
- El usuario **admin** del sistema es reservado para el desarrollador: no se puede modificar desde acá.
