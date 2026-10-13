---
pantalla: Ventas.Index
titulo: Ventas
rol: usuario
modulo: Ventas
orden: 1
permiso: Venta.VerVentas (sin el permiso se muestra "Acceso denegado"; a los usuarios de producción les pide elegir el operador de Ventas antes de entrar)
revisada: 2026-10-13
---
## Para qué sirve
Ver las **ventas de un período**, agrupadas por día, con la cantidad y el total de lo que se está mostrando. Tocando una venta se abre su detalle.

## Filtros
- **Desde / Hasta**: al cambiarlos aparece el aviso de que hay filtros sin aplicar; tocá **Buscar** para recargar.
- **Sucursal**: al cambiarla, la pantalla se recarga sola.
- **Vendedor** y **Cliente**: escribiendo buscás por letras; con la **lupa** (o **F9** vendedor, **F10** cliente) elegís de una lista, de a uno o varios, y filtra **exacto** por esas personas. Ver [cómo funciona el selector](ayuda:concepto/selector-cliente-vendedor-proveedor). Cliente busca por razón social o identificación.
- **Forma de pago** y **Tipo comprobante**: se tildan en el desplegable.
- Vendedor, Cliente, Forma de pago y Tipo comprobante se aplican **en el momento** sobre lo ya cargado y recalculan **Cant.** y **Total**. Los filtros quedan recordados en la sesión del navegador (al volver a la pantalla siguen puestos).

## Botones de arriba
- **Facturas**: abre el listado de [facturas](ayuda:pantalla/Ventas.Facturas) del mismo período y sucursal (no lleva Vendedor ni Cliente).
- **Ver líneas**: abre las [líneas de venta](ayuda:pantalla/Ventas.Lineas) con los mismos filtros (fechas, sucursal, vendedor, cliente y formas de pago). Si elegiste personas de la lista viajan **esas** personas; si escribiste, viaja el texto.

## Notas
- PENDIENTE: no se verificó en esta revisión qué permiso muestra la entrada de esta pantalla en el menú.
