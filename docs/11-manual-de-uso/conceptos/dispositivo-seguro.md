---
titulo: Dispositivo seguro (qué es y cómo se configura)
rol: admin
modulo: Acceso y seguridad
orden: 10
revisada: 2026-10-03
---
# Dispositivo seguro

## Para qué sirve
Evita que una persona **ajena a la empresa** pueda ingresar aunque conozca la contraseña de un empleado. Si se exige, el empleado solo puede iniciar sesión desde un **dispositivo autorizado**: una PC o celular que la empresa conoce. Desde cualquier otro dispositivo, aunque la contraseña sea correcta, no entra hasta autorizarlo.

## Qué es un "dispositivo"
- **Una PC con el agente de impresión de CarniSys:** se identifica por el *ID de hardware* del equipo (el "número de serie" que ves en *Dispositivos seguros*).
- **Un celular o una PC sin agente:** se identifica por el **navegador** (una marca que el sistema le deja guardada). Chrome y Edge de la misma PC son dos dispositivos distintos, y si el empleado borra los datos del navegador o usa una ventana de incógnito tiene que autorizarlo de nuevo.

## Dónde se activa (hay dos interruptores y se suman)
| Interruptor | Dónde | Efecto |
|---|---|---|
| **Exigir dispositivo seguro a no-administradores** | *Mi Empresa* | Aplica a **todos** los empleados de la empresa que no son administradores. |
| **Requiere dispositivo seguro** | *Usuarios → Editar usuario* | Aplica solo a ese usuario, aunque el de la empresa esté apagado. |

Si cualquiera de los dos está activo, se le exige al usuario. **Los dos vienen apagados** por defecto: no cambia nada hasta que los actives. Los **administradores nunca quedan afectados**: siempre pueden ingresar (así nunca te quedás sin acceso para autorizar dispositivos).

Con la **empresa en "apagado"** los empleados pueden ingresar desde cualquier dispositivo con su contraseña (y no se les ofrece la lista de usuarios ni la clave rápida, que solo valen en dispositivos autorizados).

## Cómo autoriza un empleado su dispositivo
Cuando pone bien su contraseña desde un dispositivo no autorizado ve la pantalla *"Este dispositivo no está autorizado"* con tres caminos:
1. **PC con el agente de impresión:** ve el *código del equipo* y se lo pasa a un administrador, que lo carga en *Dispositivos seguros*.
2. **Celular o PC sin agente, con mail cargado:** recibe un **código de 6 dígitos por mail**, lo escribe en la misma pantalla, le pone un nombre al dispositivo y queda autorizado **sin que intervenga un administrador**. Requiere que el usuario tenga mail y que el servidor tenga el envío de mails configurado.
3. **Pedirle autorización al administrador** (sirve en cualquier dispositivo, aunque no haya mail): el empleado le pone un nombre y envía la solicitud. Te llega un aviso por mail (si hay envío de mails) y la ves en *Dispositivos seguros* como **solicitud pendiente**, para aprobarla o rechazarla.

Ver [Dispositivos seguros](ayuda:pantalla/DispositivosSeguros.Index) y [Mi Empresa](ayuda:pantalla/Empresa.Index).

## Cómo lo administrás
- En *Dispositivos seguros* ves todos los dispositivos con su **origen**: *Manual* (lo cargó un administrador), *Autoservicio* (lo autorizó el empleado por mail) o *Solicitud* (lo aprobó un administrador a pedido del empleado).
- Podés **bloquear** cualquier dispositivo: deja de servir para ingresar y el empleado **no lo puede volver a autorizar por su cuenta** (ni por mail ni por solicitud). Para volver a usarlo hay que desbloquearlo ahí.
- En [Auditoría de accesos](ayuda:pantalla/AuditoriaLogin.Index) ves desde qué dispositivo se ingresó cada vez y los intentos desde dispositivos no autorizados o bloqueados.

## Cosas para tener en cuenta
> **Antes de activar el interruptor de la empresa**, cargá el mail de cada empleado o avisales que te pidan la autorización: sin mail no pueden autorizar su dispositivo por su cuenta.

- El mail de cada empleado tiene que ser **personal**: si dos personas comparten un mail, cualquiera puede autorizar dispositivos del otro.
- Esto frena a personas externas; un equipo ya autorizado sigue permitiendo el acceso a quien lo use. Si perdés un equipo o un empleado se va, **bloqueá sus dispositivos**.
- Para apagarlo en cualquier momento, desactivá el interruptor: es inmediato y nadie queda sin acceso.

Ver también: [clave rápida (PIN)](ayuda:concepto/clave-rapida-pin), [mapa de configuraciones](ayuda:referencia/configuraciones).
