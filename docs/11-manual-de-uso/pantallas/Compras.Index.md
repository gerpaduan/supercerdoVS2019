---
pantalla: Compras.Index
titulo: Compras
rol: usuario
modulo: Compras
orden: 1
permiso: Compra.VerCompras (sin el permiso se muestra "Acceso denegado"; a los usuarios de producción les pide elegir el operador de Compras antes de entrar)
revisada: 2026-10-22
---
## Para qué sirve
Ver las **compras de un período** (proveedor, tipo, cantidad, total, sucursal) y abrir el detalle de cada una.

## Filtros
- **Desde / Hasta**: al cambiarlos aparece el aviso; tocá **Buscar** para recargar.
- **Sucursal** y **Tipo compra**: al cambiarlos, la pantalla se recarga sola. *Tipo compra* solo se elige en las empresas que lo usan; en el resto figura fijo como *Productos*.
- **Proveedor**: escribiendo buscás por la razón social; con la **lupa** (o **F9**) elegís de una lista, de a uno o varios, y filtra **exacto** por esos proveedores. Ver [cómo funciona el selector](ayuda:concepto/selector-cliente-vendedor-proveedor). Tocá **Buscar** para aplicarlo.
- **Descripción**: busca texto en el proveedor, el número de documento y las observaciones. Es independiente de **Proveedor**: si usás los dos, se piden las dos condiciones.

## Botones
- **Ver eliminados**: lleva a [Registros eliminados](ayuda:pantalla/RegistrosEliminados.Index) de compras (solo para administradores y quien puede modificar compras).
- **Nueva compra** (Alt+N) y **Ver lineas** (Alt+V): abren la carga de una compra y las [líneas de compra](ayuda:pantalla/Compras.Lineas). *Ver lineas* lleva los filtros que ya están aplicados (incluido el proveedor).

## Totales y lista
- Arriba: **Cant. Medias** (donde corresponde), **Total Un./Kg.** y **Total**, de lo que muestra la lista.
- **Vista completa** despliega el detalle de todas las compras; también se abre el de una sola con el botón de cada fila o con doble clic.

## Notas
- PENDIENTE: el detalle de cada columna y el de las acciones de fila no se revisó en esta versión.
