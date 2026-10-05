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

## Dónde está
Abrí el menú lateral, tocá **Configuración (1)** y después **Usuarios (2)**.

![Menú lateral: Configuración (1) y Usuarios (2)](media:Usuarios.Index/menu.png)

## Qué ves
Un listado con **todos** los usuarios de la empresa (también los inactivos), por nombre. Columnas: ID, Nombre, Usuario, **Admin**, **Activo**, **Bloqueado**, Email, sucursal e ID de empresa.

![Pantalla Usuarios: Crear usuario nuevo (1), Buscar usuario (2) y Acciones de cada fila (3)](media:Usuarios.Index/listado.png)

- **(1) Crear usuario nuevo.**
- **(2) Buscar usuario:** filtra en vivo por id, nombre, usuario o email.
- **(3) Acciones** de cada fila (ver más abajo).
- **Bloqueado:** **Sí** (rojo) = el usuario está bloqueado por completo; **Parcial** (amarillo) = bloqueado solo para ingresar desde dispositivos **no** autorizados (puede seguir entrando desde el suyo); **No**.

## Acciones
Cada fila tiene sus botones a la derecha. **Modificar (1)** y **Ver permisos (2)** aparecen siempre; **Desbloquear** y **Quitar PIN** solo cuando corresponden.

![Botones de una fila: Modificar (1) y Ver permisos (2)](media:Usuarios.Index/acciones.png)

- **Crear usuario nuevo** → [Editar usuario](ayuda:pantalla/Usuarios.Editar).
- **Modificar** → [Editar usuario](ayuda:pantalla/Usuarios.Editar).
- **Ver permisos** → [Permisos del usuario](ayuda:pantalla/Usuarios.Permisos).
- **Desbloquear** (solo si está bloqueado total o parcialmente; pide confirmación): limpia **ambos** bloqueos y los intentos fallidos, y deja constancia en la [auditoría](ayuda:pantalla/AuditoriaLogin.Index) de quién lo hizo. El usuario también puede desbloquearse solo por mail si tiene mail cargado. Ver [bloqueo de usuarios](ayuda:concepto/bloqueo-de-usuarios).
- **Quitar PIN** (solo si el usuario tiene [PIN](ayuda:concepto/clave-rapida-pin)): lo elimina; el usuario sigue entrando con su contraseña y puede crear otro PIN. Útil si lo olvidó o sospechás que otra persona lo conoce. También queda en la auditoría.

## Cómo se hace: buscar un usuario y abrir su ficha
Video corto, sin sonido: escribís en **Buscar usuario**, la lista se filtra y con **Modificar** abrís la ficha. No se guarda nada hasta que toques **Guardar**.

![Video: buscar un usuario y abrir su ficha con Modificar](media:Usuarios.Index/buscar-y-modificar.webm)

## Reglas
- Solo ves y administrás usuarios de **tu empresa**.
- El usuario **admin** del sistema es reservado para el desarrollador: no se puede modificar desde acá.
