---
pantalla: Compras.Index
titulo: Compras
rol: usuario
modulo: Compras
orden: 1
permiso: Compra.VerCompras (sin el permiso se muestra "Acceso denegado"; a los usuarios de producción les pide elegir el operador de Compras antes de entrar)
revisada: 2026-10-30
---
## Para qué sirve
Ver las **compras de un período** (proveedor, tipo, cantidad, total, sucursal) y abrir el detalle de cada una.

## Filtros
- En la **computadora** todos los filtros van en **una sola fila** (Desde, Hasta, Sucursal, Tipo compra, Proveedor, Descripción y Buscar) para dejar más lugar a la lista. Las fechas se ven siempre completas, y Proveedor y Descripción conservan un ancho mínimo para leer lo que escribiste.
- **Desde / Hasta**: al cambiarlos aparece el aviso; tocá **Buscar** para recargar. En la computadora el aviso es el texto *Cambiaste las fechas: tocá Buscar para actualizar.* de la barra de abajo y el botón **Buscar** se pone **amarillo**; en el celular sigue siendo la alerta amarilla.
- **Sucursal** y **Tipo compra**: al cambiarlos, la pantalla se recarga sola. *Tipo compra* solo se elige en las empresas que lo usan; en el resto figura fijo como *Productos*.
- **Proveedor**: escribiendo buscás por la razón social; con la **lupa** (o **F9**) elegís de una lista, de a uno o varios, y filtra **exacto** por esos proveedores. Ver [cómo funciona el selector](ayuda:concepto/selector-cliente-vendedor-proveedor). Tocá **Buscar** para aplicarlo.
- **Descripción**: busca texto en el proveedor, el número de documento y las observaciones. Es independiente de **Proveedor**: si usás los dos, se piden las dos condiciones.

## Botones
- **Ver eliminados**: lleva a [Registros eliminados](ayuda:pantalla/RegistrosEliminados.Index) de compras (solo para administradores y quien puede modificar compras).
- **Nueva compra** (Alt+N) y **Ver lineas** (Alt+V): abren la carga de una compra y las [líneas de compra](ayuda:pantalla/Compras.Lineas). *Ver lineas* lleva los filtros que ya están aplicados (incluido el proveedor).

## Totales y lista
- Debajo de los filtros (en la computadora, en una misma barra con **Vista completa** y los botones **Nueva compra**, **Ver lineas** y **Ver eliminados**): **Cant. Medias** (donde corresponde), **Total Un./Kg.** y **Total**, de lo que muestra la lista, y el texto de estado (*Resultados actualizados.*). Si la ventana es angosta, los botones bajan a un segundo renglón.
- **Vista completa** despliega el detalle de todas las compras; también se abre el de una sola con el botón de cada fila o con doble clic.

## Notas
- PENDIENTE: el detalle de cada columna y el de las acciones de fila no se revisó en esta versión.

## En el celular
- En pantallas angostas (celular) cada compra se ve como **tarjeta**: arriba el **proveedor**, después fecha, nro. de remito/factura, tipo, unidades/kg, medias reses, **total** y sucursal. No hace falta desplazarse de costado.
- Al pie de la tarjeta están **Detalle** (abre debajo de la tarjeta el detalle de la compra) y **Ver compra**. Las compras anuladas se ven con la tarjeta en rojo.
- Arriba a la derecha de la tarjeta, en gris, va el **número de compra**.
- **Detalle** en el celular: no se muestra el recuadro de datos del registro (lo principal ya está en la tarjeta). Arriba de las líneas aparecen las **observaciones** (si hay) y la etiqueta **Cta. cte.** si la compra va a cuenta corriente. Cada línea muestra el producto y la cantidad arriba, y abajo el código, la **hora** en que se cargó y el icono de **balanza** (azul = pesada en balanza, gris = no). Si la línea se cargó **otro día** que la compra, en vez de la hora sale la fecha y hora en ámbar.
- En la tablet y el celular la pantalla se ve como antes: filtros apilados, con el estado (*Resultados actualizados.*) debajo del recuadro de filtros.
