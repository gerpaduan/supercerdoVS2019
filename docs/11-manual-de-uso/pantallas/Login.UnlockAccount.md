---
pantalla: Login.UnlockAccount
titulo: Desbloquear usuario (enlace del mail)
rol: usuario
modulo: Acceso y seguridad
orden: 6
revisada: 2026-10-03
---
## Para qué sirve
Es la pantalla a la que llegás desde el **enlace del mail de desbloqueo**, cuando tu usuario se bloqueó por demasiados intentos fallidos. Ver [bloqueo de usuarios](ayuda:concepto/bloqueo-de-usuarios).

## Cómo se usa
1. En la pantalla de [ingreso](ayuda:pantalla/Login.Index), cuando tu usuario está bloqueado, tocá **"Desbloquear usuario por mail"**.
2. Abrí el mail que te llega (revisá también *spam*) y tocá el enlace.
3. Tocá **"Desbloquear mi usuario"**. Volvés al ingreso con el aviso *"Tu usuario fue desbloqueado correctamente"*.

## Reglas
- El enlace es de **un solo uso** y vence a los **60 minutos**. Si no es válido o venció, la pantalla lo dice: pedí uno nuevo desde el ingreso.
- **Solo abrir el enlace no desbloquea**: hace falta tocar el botón (así una previsualización del correo no desbloquea sola).
- El mail se manda **solo al mail registrado** de tu usuario. Si tu usuario no tiene mail cargado, pedile a un administrador que lo desbloquee.
- El desbloqueo limpia los intentos fallidos y queda registrado en la auditoría.
