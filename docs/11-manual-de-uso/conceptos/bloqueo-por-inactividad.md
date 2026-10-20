---
titulo: Bloqueo por inactividad (sesión y POS)
rol: usuario
modulo: Acceso y seguridad
orden: 12
revisada: 2026-10-20
---
# Bloqueo por inactividad

## Para qué sirve
En una PC que usan varios empleados, evita que alguien use la sesión que otro dejó abierta. Si pasa un tiempo sin que se toque el mouse, el teclado o la pantalla, el sistema protege la sesión.

## Qué pasa en el sistema (todas las pantallas menos el POS)
1. Pasados los **minutos de inactividad** del usuario aparece **"¿Extender sesión?"** con una cuenta regresiva de **5 minutos**.
2. **Extender sesión:** sigue todo igual y el contador empieza de nuevo.
3. **Cerrar sesión**, o si se agotan los 5 minutos: se **cierra la sesión** y hay que ingresar de nuevo. Lo que estaba sin guardar en una pantalla se pierde (las ventas en curso del POS no: quedan resguardadas).

## Qué pasa en el POS
- Pasados los minutos de inactividad del **vendedor**, el POS se **bloquea** con una pantalla que pide **su contraseña**. **La venta en curso no se pierde**: queda tal cual y, además, resguardada en el servidor.
- Solo la contraseña **del mismo vendedor** desbloquea. La de otro usuario no sirve, aunque sea correcta. Los errores tienen un límite de **5 intentos** (avisa cuántos quedan desde el 3.º; al 5.º se espera 5 minutos): ver [intentos de clave al autorizar](ayuda:concepto/intentos-de-clave-al-autorizar).
- Si mientras estaba bloqueado **le cerraron la caja**, al desbloquear el POS se recarga y vuelve a pedir abrir caja.
- **Otro vendedor necesita la PC:**
  - Cuenta de **producción**: botón **Cambiar de vendedor** (elige usuario y pone su contraseña). La venta bloqueada queda en *Ayuda (F1) → Ver ventas sin cerrar* a nombre de su dueño; solo el dueño la retoma, o un supervisor con su autorización. El nuevo vendedor empieza una venta limpia y necesita **su propia caja abierta** para vender.
  - Usuario **normal**: botón **Cerrar sesión**; el otro vendedor ingresa con su usuario.
- En *Punto de expendio* no se puede cambiar de vendedor si hay una venta en curso (no se resguarda en el servidor): el vendedor debe desbloquear.

## Quién se bloquea y quién no
| Caso | Se bloquea |
|---|---|
| Usuario normal con minutos > 0 (viene en **60**) | Sí |
| Usuario con **0** minutos | **Nunca** |
| Usuario de **producción** (cuenta compartida) | **Nunca** (en el POS sí se bloquea el *vendedor* que eligió, según sus propios minutos) |
| Sesión iniciada desde un dispositivo marcado **No bloquear sesión** | **Nunca** (rige desde el próximo ingreso) |

## Dónde se configura
- **Minutos por usuario:** [Editar usuario](ayuda:pantalla/Usuarios.Editar) → *Minutos de inactividad*.
- **Por dispositivo:** [Dispositivos seguros](ayuda:pantalla/DispositivosSeguros.Index) → *No bloquear sesión* (por ejemplo, la PC propia de un administrador).
- Mapa completo: [configuraciones](ayuda:referencia/configuraciones).

## Límites conocidos
- El tiempo se mide en el navegador: si se cierra el navegador sin cerrar sesión, la sesión sigue abierta hasta 12 horas.
- El bloqueo del POS se hace en la pantalla del navegador (protege del compañero que pasa por la PC, no de alguien con conocimientos técnicos).
- Al migrar, **todos los usuarios quedan en 60 minutos**; si alguien no debe bloquearse, hay que ponerle 0 o marcar su PC como *No bloquear sesión*.
- **PENDIENTE:** el cierre automático al agotarse la cuenta regresiva de 5 minutos y el bloqueo en *Punto de expendio* no se probaron en el navegador (sí el aviso, "Extender", "Cerrar sesión", el bloqueo del POS de Ventas, el cambio de vendedor y "No bloquear sesión").
