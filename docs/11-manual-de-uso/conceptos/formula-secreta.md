---
titulo: Fórmula secreta
rol: usuario
modulo: Elaborados
orden: 40
revisada: 2026-10-04
---
# Fórmula secreta

## Para qué sirve
La fórmula de un elaborado (qué ingredientes lleva y en qué proporción) puede ser un secreto de la empresa. Una fórmula marcada como **secreta** no se muestra a quien carga el elaborado: solo la ve, por unos minutos, una persona **con permiso sobre fórmulas** que ingresa **su usuario y su contraseña** en esa misma pantalla.

## Cómo se marca una fórmula como secreta
En [Editar fórmula](ayuda:pantalla/Elaborados.EditarFormula) está la casilla **Fórmula secreta**. Solo puede cambiarla quien tiene permiso de **ingresar fórmulas**. Las fórmulas existentes no cambian hasta que alguien la active. En el listado de [Fórmulas](ayuda:pantalla/Elaborados.Formulas) las secretas llevan un candado.

## Qué ve el operador que carga el elaborado
- En [Ingreso rápido](ayuda:pantalla/Elaborados.EditarIngresoRapido) y en [Carga](ayuda:pantalla/Elaborados.Carga): el producto, la cantidad y un panel *"La fórmula de este elaborado es secreta"* con el botón **Ver fórmula**. No hay ingredientes, porcentajes ni receta.
- **Al guardar, el sistema calcula y descuenta los ingredientes igual** que siempre: el operador no necesita ver la fórmula para registrar la producción.
- En el [listado de elaborados](ayuda:pantalla/Elaborados.Index) (detalle de cada registro) y en [Líneas de elaborado](ayuda:pantalla/Elaborados.Lineas): también se ocultan los ingredientes de esos registros (en *Líneas* aparece una sola fila con el total). Así no se puede reconstruir la fórmula mirando producciones anteriores.

## Cómo se ve la fórmula
1. Tocá **Ver fórmula**.
2. Ingresá **usuario y contraseña** de una persona con permiso de **ver** o **ingresar** fórmulas (puede ser tu propio usuario si lo tenés). Se pide **siempre**, aunque ya hayas iniciado sesión con ese usuario.
3. Se muestra la fórmula y aparece el botón **Ocultar**.

La fórmula vuelve a ocultarse sola a los **5 minutos**, al tocar **Ocultar** o al salir de Ingreso rápido / Carga. Para verla otra vez hay que ingresar de nuevo usuario y contraseña.

## Reglas
- El mensaje de error es siempre el mismo (*"Usuario o contraseña incorrectos, o sin permiso para ver fórmulas"*): no dice si el usuario existe ni si tiene permiso.
- Con **3 intentos fallidos** seguidos, esa sesión queda bloqueada **5 minutos** para probar usuarios y contraseñas.
- No se puede usar un usuario inactivo ni bloqueado.
- Cada vez que alguien ve una fórmula secreta, la oculta, falla al intentarlo, o marca/desmarca una fórmula como secreta, queda un registro (quién se identificó, quién estaba operando, qué producto, desde qué IP y cuándo). PENDIENTE: todavía no hay una pantalla para consultar ese registro.
- Los borradores sin guardar de Ingreso rápido y Carga no guardan la fórmula de un producto secreto.
- El **cierre de caja** no usa fórmulas ni costos de elaborados: esta función no lo afecta.
- La cuenta compartida de **producción** no tiene permiso sobre fórmulas, así que no puede ver una fórmula secreta (ver [roles y permisos](ayuda:concepto/roles-y-permisos)).

Los tiempos y los intentos se pueden cambiar en la configuración del servidor: ver el [mapa de configuraciones](ayuda:referencia/configuraciones).
