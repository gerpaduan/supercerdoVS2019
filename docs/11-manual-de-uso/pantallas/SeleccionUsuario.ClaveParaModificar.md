---
pantalla: SeleccionUsuario.ClaveParaModificar
titulo: Ingresá tu clave para modificar el registro
rol: operador
modulo: Producción
orden: 9
permiso: ninguno propio; se muestra solo a la cuenta de producción que ya eligió su nombre (Movimientos, Stock y Elaborados)
revisada: 2026-10-15
---
## Para qué sirve
La sala de producción trabaja con **una cuenta compartida**: cada persona elige su nombre (sin clave) y recién ahí carga o abre registros. Hasta ahora, al abrir un registro **anterior a la fecha de hoy** la pantalla decía "no tiene permisos" aunque **la persona elegida sí pudiera** modificar registros de esa fecha, porque se miraban los permisos de la cuenta compartida y no los de la persona.

Esta pantalla pide **la clave de esa persona** para verificar **su** permiso por fecha.

## Cómo se usa
1. Abrí un registro anterior a hoy en **Movimientos**, **Stock** (ojo del listado) o **Elaborados**, y elegí tu nombre si te lo pide.
2. Si la cuenta de producción no alcanza para ese registro, aparece esta pantalla: **"Ingresá tu clave para modificar el registro"**, con tu nombre.
3. Escribí **tu clave** y tocá **Continuar**.
   - Si es correcta, volvés al registro y se aplica **tu** permiso: si podés modificar registros de esa fecha, el registro queda editable; si solo podés consultarlo, se abre en modo vista.
   - Si es incorrecta, lo avisa y podés probar de nuevo. Después de varios intentos fallidos se bloquea unos minutos (el mismo límite que el login de Ventas y Compras).
   - Si tu usuario **no tiene permiso para esa fecha**, te avisa: *"<nombre> no tiene permiso para modificar registros con una fecha anterior a la de su permiso"* y, si tu permiso tiene un límite de días, desde qué fecha sí podés.
4. **Cancelar** vuelve al listado sin hacer nada.

## Registros que se abren en modo vista
Si la cuenta de producción **sí** puede consultar el registro pero no modificarlo, se abre en modo vista como siempre y aparece el botón **Modificar con mi clave**, que lleva a esta pantalla.

## Cuánto dura la clave
**15 minutos por módulo** (Movimientos, Stock y Elaborados cuentan por separado) y solo para la persona elegida: alcanza para abrir el registro y guardarlo sin pedir la clave dos veces. Si elegís otro nombre, se pide de nuevo. La clave no se guarda.

## Notas
- Solo aplica a registros **anteriores a hoy** y a la **cuenta de producción**. Un usuario que entra con su propio usuario y clave no ve esta pantalla: se le aplica su permiso directamente.
- La clave de la persona **suma** permisos a los de la cuenta de producción; nunca los quita.
- PENDIENTE: la pantalla de Compras y Ventas usa otro mecanismo (autorizar al operador una vez por sesión); no cambió.
