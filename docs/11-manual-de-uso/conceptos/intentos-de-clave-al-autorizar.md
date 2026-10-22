---
titulo: Intentos de clave al autorizar (POS, Cierre de Caja, Ver fórmula)
rol: usuario
modulo: Acceso y seguridad
orden: 35
revisada: 2026-10-20
---
# Intentos de clave al autorizar

## Dónde aplica
Cada vez que el sistema pide **usuario y clave para autorizar algo** (no es el ingreso al sistema, ese tiene su [propio límite](ayuda:concepto/bloqueo-de-usuarios)):

| Pantalla / acción | Contador |
|---|---|
| **POS** (Ventas y Punto de expendio): elegir o cambiar de vendedor, desbloquear el POS tras la inactividad, autorizar a un supervisor para tocar la venta en curso de otro | Común |
| **Compras / Ventas**: elegir el operador del módulo (cuenta de producción) | Común |
| **Clave para modificar** un registro anterior a hoy (Movimientos, Stock, Elaborados) | Común |
| **Modificar un pedido** (autorización de quien tiene permiso) | Común |
| **Eliminar** una compra, una operación de stock o un movimiento (usuario y contraseña en cada eliminación; ver [eliminar registros](ayuda:concepto/eliminar-registros)) | Común |
| **Cerrar caja** cuando no tenés el permiso (se pide usuario y clave de quien lo tiene) | Propio |
| **Ver fórmula** de una fórmula secreta | Propio |

"Común" significa que se suman entre sí; "Propio" tiene su cuenta aparte: equivocarte en *Ver fórmula* no te gasta intentos del POS ni del cierre de caja.

## Cómo funciona
- Tenés **5 intentos** equivocados en una ventana de **5 minutos**.
- Los dos primeros errores muestran el aviso normal. **Desde el 3.º** el mensaje agrega *"Te quedan N intentos antes del bloqueo"* (el mismo criterio del [ingreso al sistema](ayuda:concepto/bloqueo-de-usuarios)).
- El **5.º error** bloquea **5 minutos**: se avisa en ese mismo intento, con cuenta regresiva.
- Una clave correcta reinicia el conteo. Si pasan 5 minutos desde el primer error sin llegar al máximo, también.

## Qué se bloquea (y qué no)
- **No se bloquea el usuario.** Su usuario y su clave siguen sirviendo para ingresar al sistema; estos errores no cuentan para los 7 intentos del ingreso.
- **No se bloquea el dispositivo.** El conteo es de la **sesión del navegador** en la que estás trabajando: otra PC u otro navegador tienen su propio conteo.
- **En una PC de producción el conteo es compartido por todos los vendedores**: esa PC es una sola sesión, así que los errores de distintas personas se suman. Por eso el máximo es 5.
- **No hay excepción para dispositivos seguros.** Un dispositivo autorizado no garantiza quién tiene el teclado, y este límite existe justamente para frenar a quien prueba claves de compañeros desde un equipo ya abierto. (En el ingreso al sistema sí cambia: ver [dispositivo seguro](ayuda:concepto/dispositivo-seguro).)
- El conteo vive en la memoria del servidor: si el servidor se reinicia, se borra.

## Configuración
Los máximos, la ventana y la duración del bloqueo se cambian en el servidor: ver el [mapa de configuraciones](ayuda:referencia/configuraciones).

PENDIENTE: no se probó en el navegador si cerrar sesión y volver a entrar en la misma PC reinicia el conteo (depende de la sesión del navegador).

Ver también: [bloqueo por inactividad](ayuda:concepto/bloqueo-por-inactividad), [fórmula secreta](ayuda:concepto/formula-secreta).
