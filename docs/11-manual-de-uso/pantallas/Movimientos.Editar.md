---
pantalla: Movimientos.Editar
titulo: Nuevo movimiento entre sucursales
rol: usuario
modulo: Movimientos
orden: 2
permiso: Movimientos (PENDIENTE: documentar el permiso exacto; esta página describe solo el panel «Stock destino»)
revisada: 2026-10-27
---
## Para qué sirve
Armar un movimiento de mercadería de una sucursal a otra. Esta página documenta por ahora solo el panel **Stock destino**; el resto de la pantalla (origen, destino, fecha, agregar productos, guardar) no está documentado todavía (**PENDIENTE**).

## Stock destino: ver el stock de la sucursal que recibe
Para decidir qué mandar conviene mirar cuánto stock tiene la sucursal **destino**. El botón **Stock destino** (arriba, junto a *Movimientos sin guardar*; solo en pantallas de escritorio) muestra ese stock **al lado** del movimiento:
- Con pantalla de **1200 px o más**: el **stock a la izquierda** y la **carga del movimiento a la derecha**. El botón lo enciende y apaga; se recuerda en este navegador. Con menos ancho no hay lugar para dos columnas: el botón abre el stock en una **ventana independiente**.
- **↗ (en la cabecera del panel)**: pasa el stock a una ventana independiente (un tercio del ancho de la pantalla) y cierra el panel. **⟳**: actualiza el stock ahora. **✕**: cierra el panel.
- Es la misma pantalla de [Existencia por sucursales](ayuda:pantalla/Stock.ExistenciaPorSucursales) en modo compacto, solo con la sucursal destino y sin pedido ni exportación. Si **cambiás la sucursal destino** del movimiento, el stock cambia solo a esa sucursal.
- Con el stock al lado, *Agregar producto* se acomoda al lugar que queda: si no entra en una línea, Código y Producto van arriba y **Cant. Un., Kgs. y +** abajo, siempre juntos (también con el teclado digital abierto).
- Se actualiza **solo cada 15 minutos** (como Existencia por sucursales) y con **⟳** cuando quieras.

### Cargar un producto desde el stock
- **Doble click en una fila** del stock: carga ese producto en *Agregar producto* y deja el cursor en **Cant. Un.** (tenés que confirmar la cantidad y tocar **+**).
- Los productos que **ya van en el movimiento** se marcan en el stock con un fondo suave y **«+12,3 kg en mov.»** (o unidades).

### Sugerido a enviar
La columna **Sugerido** estima lo que conviene mandar: **ventas + uso en elaborados del período, menos el stock actual** del destino (la misma regla que la sugerencia de *Ingresar pedido*; un stock negativo cuenta como 0; **✔ alcanza** si el stock cubre el consumo). El período se elige arriba del stock con los atajos **1, 2, 3, 7 y 15 días** (por defecto 7) o escribiendo la fecha **desde** y, con **+ Fecha hasta**, la fecha **hasta**.
- El bloque del período y la explicación **arranca siempre compacto**: una sola línea («Sugerido: 13/10 al hoy (7 d) · Recalculado 20:07 · sin cambios»). **Cambiar período** (o un click en cualquier parte de esa línea) lo abre para cambiar las fechas y **Ocultar** lo vuelve a cerrar (no se recuerda: la próxima vez vuelve a empezar compacto).
- Los **atajos** (1, 2, 3, 7, 15 d) recalculan las sugerencias **al instante**.
- Una **fecha escrita a mano** no recalcula sola (así podés recorrer fechas sin esperar): la línea se pone **ámbar** con *«Período cambiado a …: sugerencias sin actualizar»* y el botón **Actualizar sugerencias**. También se aplica con **Enter** en la fecha o con **⟳**. Mientras tanto los números sugeridos se ven atenuados y **no se pueden cargar** al movimiento (son del período anterior). Si volvés a las fechas de antes, el aviso desaparece.
- La actualización automática de cada 15 minutos usa el período ya aplicado: no aplica una fecha que todavía no confirmaste.
- **Click en el número sugerido**: carga el producto **y** esa cantidad (kilos si es por peso, con las unidades equivalentes si tiene promedio; unidades enteras si no). Sigue sin agregarse sola: revisá y tocá **+**.
- Cada vez que el stock se actualiza las sugerencias se **recalculan solas** y la leyenda dice *«Recalculado 11:23 · 3 cambiaron»*. Las celdas cuyo sugerido **cambió** quedan marcadas en ámbar (el tooltip dice el valor anterior); si ese producto ya está en el movimiento, su «en mov.» se pinta en ámbar para que **revises la cantidad**. El movimiento nunca se modifica solo.
- Con un período **cerrado** (fecha hasta anterior a hoy) las actualizaciones solo releen el stock.
- Solo con la base de datos nueva (Postgres): con la anterior la columna avisa que no está disponible.

## En el celular
- **Agregar producto:** Cant.Un., Kgs. y el botón **+** van en el mismo renglón (con el teclado numérico en pantalla abierto se apilan como antes).
- **Líneas cargadas:** se ven como una lista. Arriba el producto, las unidades y los kilos; abajo el número de orden y el código (`#3 · Cód. 5`) y dos iconos: **balanza** (azul = pesado en balanza, gris = no) e **ingreso permitido** (verde = la línea se autorizó aunque los kilos no coincidían con las unidades, gris = normal). El botón rojo de la derecha **quita la línea**.
- **Orden:** en el celular arranca con **lo último cargado arriba**. Con el selector de la derecha se puede cambiar (primeros primero, código, producto, cantidad, kilos). En la computadora se ordena tocando los encabezados, como siempre.
- El orden en que se ven las líneas **no cambia el orden en que se guardan**: siempre se guardan en el orden en que se cargaron.
- El selector de orden y la **lupa de búsqueda** **siguen activos en el modo ver** (cuando los campos y la tabla están bloqueados): solo cambian cómo se ven las líneas.
- **Datos del movimiento** (origen, destino, creado, actualizado) se muestra compacto, en dos columnas y con letra chica, para ocupar poca altura.

## Eliminar un movimiento
El botón rojo **Eliminar** aparece al abrir un movimiento **ya guardado** para quien puede modificarlo. Muestra el resumen y las líneas, pide **motivo obligatorio** (10 a 300 caracteres) y **usuario y contraseña en cada eliminación** (ver [eliminar registros](ayuda:concepto/eliminar-registros)). El movimiento **se borra**, el stock del origen y del destino se recalcula sin él, y queda la constancia (quién, cuándo, motivo y todas las líneas) en [Registros eliminados](ayuda:pantalla/RegistrosEliminados.Index). No se puede deshacer desde el sistema.
- **Deshabilitado donde los movimientos se replican entre dos bases** (hoy Servidor SM y San Lorenzo): la copia en la otra base no se borra sola, así que el botón figura apagado con la explicación. Se habilita cuando se unifiquen las bases (configuración `Movimientos:ReplicaEntreBases`, ver el [mapa de configuraciones de la plataforma](ayuda:referencia/configuraciones-plataforma)).

## Si algo no anda
- El botón **Stock destino** no aparece: es solo para pantallas de escritorio, y necesita un navegador reciente.
- El stock dice *Elegí la sucursal destino*: elegí primero el destino en el movimiento.
- El doble click no carga el producto: si el movimiento está abierto solo para ver (un movimiento ya guardado), tocá **Modificar** primero.
