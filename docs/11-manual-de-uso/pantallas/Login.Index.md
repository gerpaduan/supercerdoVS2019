---
pantalla: Login.Index
titulo: Ingreso al sistema
rol: usuario
modulo: Acceso y seguridad
orden: 1
revisada: 2026-10-03
---
## Para qué sirve
Es la pantalla para entrar al sistema. Funciona igual desde la dirección general (`/Login`) y desde la dirección de tu empresa (`/Login/<CUIT>`, ver [ingresar con la dirección de tu empresa](ayuda:concepto/login-por-cuit)).

## Cómo se usa
1. **Elegí tu usuario**:
   - En un **dispositivo autorizado** con la dirección de tu empresa ves una **lista de nombres**: escribí unas letras en *"Buscá tu nombre"* y tocá el tuyo.
   - En cualquier otro caso escribí tu **usuario o tu email**. Con la dirección de tu empresa, debajo del campo aparece el aviso *"En un dispositivo autorizado por tu empresa podés elegir tu usuario de una lista"* (con `/Login` a secas no aparece).
   - Si no figurás en la lista (por ejemplo sos administrador) tocá **"Ingresar con otro usuario o email"**.
2. Escribí tu **contraseña**. En un dispositivo autorizado también podés usar tu [clave rápida (PIN)](ayuda:concepto/clave-rapida-pin).
3. Tocá **Ingresar**.
4. Si tu equipo lo permite, el botón **"Ingresar con huella"** (arriba) entra sin escribir nada.

## Reglas y efectos
- Tenés **7 intentos** por usuario; desde el 3.er error el sistema avisa cuántos quedan. Al agotarlos el usuario se bloquea y aparece **"Desbloquear usuario por mail"**. Ver [bloqueo de usuarios](ayuda:concepto/bloqueo-de-usuarios).
- Si tu empresa **exige dispositivo seguro** y entrás desde un dispositivo no autorizado, después de poner bien la contraseña verás [una pantalla para autorizarlo](ayuda:pantalla/Login.DispositivoNoAutorizado). Los administradores no pasan por ese paso.
- Fuera del **horario laboral** de la empresa los empleados (no administradores) no pueden iniciar sesión.
- Si **olvidaste la contraseña**, usá *"¿Olvidaste tu contraseña?"* ([ayuda](ayuda:pantalla/Login.ForgotPassword)).
- Desde tu usuario ya ingresado podés [cambiar tu contraseña](ayuda:pantalla/Login.ChangePassword) o [configurar tu PIN](ayuda:pantalla/Login.ChangePin).

## Configuraciones relacionadas
[Dispositivo seguro](ayuda:concepto/dispositivo-seguro) · [mapa de configuraciones](ayuda:referencia/configuraciones)
