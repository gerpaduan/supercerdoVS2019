---
titulo: Bloqueo de usuarios por intentos fallidos
rol: usuario
modulo: Acceso y seguridad
orden: 30
revisada: 2026-10-03
---
# Bloqueo de usuarios por intentos fallidos

## Cómo funciona
Cada usuario tiene **7 intentos** de contraseña (o PIN) equivocados. El conteo es **por usuario**: equivocarte con un usuario no gasta los intentos de otro. Desde el **3.er error** el sistema te avisa *"Te quedan N intentos"*; los primeros dos errores no avisan para no alarmar por un tipeo.

## Dos tipos de bloqueo según desde dónde se probó
| Los errores vinieron de… | Al llegar a 7 se bloquea… | Por qué |
|---|---|---|
| Un **dispositivo autorizado** (el del propio usuario) | **El usuario completo** | Es el dueño equivocándose. |
| Un **dispositivo no autorizado** (cualquier otro) | **Solo el ingreso desde dispositivos no autorizados** | Puede ser cualquiera adivinando: así un desconocido no puede dejar afuera al dueño. El usuario sigue entrando desde su equipo habitual. |

Un ingreso correcto reinicia el conteo del lugar desde el que entraste.

## Cómo se desbloquea
- **Por mail (autoservicio):** en la pantalla de ingreso aparece el botón **"Desbloquear usuario por mail"**. Te llega un enlace de **un solo uso** (vence a los 60 minutos) al mail que tenés cargado; al abrirlo, tocás **"Desbloquear mi usuario"**. Por seguridad el sistema responde siempre lo mismo, exista o no el usuario o el mail.
- **Por un administrador:** en *Usuarios* aparece el botón **Desbloquear** (el listado marca **Sí** si el bloqueo es total o **Parcial** si es solo de dispositivos no autorizados).
- Si el usuario **no tiene mail** cargado, solo puede hacerlo un administrador.

## Cuidado con la IP
Aparte del bloqueo por usuario hay un freno por **IP**: demasiados errores desde una misma conexión la frenan unos minutos. **Desde un dispositivo autorizado ese freno no se aplica** (una oficina entera no queda afuera porque alguien se equivoca en un equipo autorizado).

Todo queda registrado en [Auditoría de accesos](ayuda:pantalla/AuditoriaLogin.Index): intentos fallidos, bloqueos y desbloqueos (con quién lo hizo).

Ver también: [clave rápida (PIN)](ayuda:concepto/clave-rapida-pin), [pantalla de ingreso](ayuda:pantalla/Login.Index).
