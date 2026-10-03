---
pantalla: Usuarios.Index
titulo: Usuarios
rol: admin
modulo: Usuarios
orden: 1
alias: Usuarios.DesbloquearUsuario, Usuarios.QuitarPin
permiso: Usuario.VerUsuarios (ver) / Usuario.NuevoUsuario (editar)
revisada: 2026-10-03
---
## Para qué sirve
Ver y administrar los **usuarios de tu empresa**: crearlos, modificarlos, darles permisos, desbloquearlos y quitarles el PIN. Está en *Configuración → Usuarios*. Los botones solo aparecen si sos administrador o tenés permiso de **edición** de usuarios; sin eso ves el listado sin acciones.

## Qué ves
Un listado con **todos** los usuarios de la empresa (también los inactivos), por nombre. Columnas: ID, Nombre, Usuario, **Admin**, **Activo**, **Bloqueado**, Email, sucursal e ID de empresa.
- **Buscar usuario:** filtra en vivo por id, nombre, usuario o email.
- **Bloqueado:** **Sí** (rojo) = el usuario está bloqueado por completo; **Parcial** (amarillo) = bloqueado solo para ingresar desde dispositivos **no** autorizados (puede seguir entrando desde el suyo); **No**.

## Acciones
- **Crear usuario nuevo** → [Editar usuario](ayuda:pantalla/Usuarios.Editar).
- **Modificar** → [Editar usuario](ayuda:pantalla/Usuarios.Editar).
- **Ver permisos** → [Permisos del usuario](ayuda:pantalla/Usuarios.Permisos).
- **Desbloquear** (solo si está bloqueado total o parcialmente; pide confirmación): limpia **ambos** bloqueos y los intentos fallidos, y deja constancia en la [auditoría](ayuda:pantalla/AuditoriaLogin.Index) de quién lo hizo. El usuario también puede desbloquearse solo por mail si tiene mail cargado. Ver [bloqueo de usuarios](ayuda:concepto/bloqueo-de-usuarios).
- **Quitar PIN** (solo si el usuario tiene [PIN](ayuda:concepto/clave-rapida-pin)): lo elimina; el usuario sigue entrando con su contraseña y puede crear otro PIN. Útil si lo olvidó o sospechás que otra persona lo conoce. También queda en la auditoría.

## Reglas
- Solo ves y administrás usuarios de **tu empresa**.
- El usuario **admin** del sistema es reservado para el desarrollador: no se puede modificar desde acá.
