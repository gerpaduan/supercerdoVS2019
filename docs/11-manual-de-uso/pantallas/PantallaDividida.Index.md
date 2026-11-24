---
pantalla: PantallaDividida.Index
titulo: Pantalla dividida (varios módulos lado a lado)
rol: usuario
modulo: Inicio
orden: 2
permiso: ninguno propio (el ítem del menú lo ve todo usuario; cada panel muestra la pantalla real y aplica los permisos de esa pantalla)
revisada: 2026-11-24
---
## Para qué sirve
Ver **varios módulos a la vez en una sola pantalla** para compararlos: por ejemplo **Stock** en una columna, **Movimientos** en otra y un **Reporte** en la tercera. Vos decidís cuántos paneles hay y cómo se reparten el espacio.

Cada panel es la pantalla real del módulo (sin menú lateral ni barra superior): lo que ves y podés hacer en un panel es lo mismo que en una pestaña normal, con **tus mismos permisos**.

Solo está en **computadora** (pantalla de 992 px o más). En el celular o la tablet se muestra un aviso: abrí cada módulo por separado.

## Cómo se usa
1. Entrá por **Pantalla dividida** (arriba de todo en el menú lateral). La primera vez hay **un panel vacío** con la lista de módulos y un buscador.
2. **Elegí un módulo** en la lista del panel vacío, o tocá un enlace del **menú lateral**: se abre **en el panel activo** (el que tiene el borde azul; el último que tocaste).
3. **Agregá un panel** con el botón **Agregar panel** de la cabecera de otro: el panel nuevo aparece **a la derecha** de ese, vacío y activo. Con la flechita ▾ elegís otra posición: **al extremo izquierdo**, **a la izquierda de este panel**, **a la derecha de este panel** o **al extremo derecho** (las que no cambian nada no aparecen). Junto a un panel, ese panel **cede la mitad de su ancho** y los demás no se mueven; en un extremo, el panel nuevo toma **un tercio** del ancho y los demás se achican parejo.
4. **Arrastrá los divisores** (las franjas entre paneles) para cambiar los tamaños. Con el divisor enfocado (Tab) también se mueve con las **flechas del teclado**. Al agregar un panel o arrastrar un divisor ningún panel queda más chico que unos 220 px (si no hay lugar, avisa); al sumar paneles con el clic derecho del menú pueden quedar algo más angostos (ajustalos arrastrando los divisores).
5. Atajos en la barra de arriba: **3 columnas** y **4 en cuadro** acomodan los paneles **conservando lo que ya tenías abierto**, en orden. Si tenés **más pantallas abiertas que lugares** (por ejemplo 4 en cuadro con 4 pantallas y tocás *3 columnas*), el botón queda **apagado** y al tocarlo avisa cuántos paneles cerrar: **no se descarta ninguna pantalla** (desde 2026-11-24; antes la sobrante desaparecía sin avisar). Un panel **vacío** no cuenta: se puede usar el atajo con 4 paneles si uno está vacío.

Hay un máximo de **6 paneles**; el contador (*n/6 paneles*) está arriba a la derecha.

## Entrar desde cualquier pantalla: «Dividir pantalla»
Podés entrar de **dos maneras**, y las dos hacen lo mismo con la pantalla que tenés abierta: el ícono de columnas de la **barra superior** o el ítem **Pantalla dividida** del **menú lateral** (desde 2026-11-21; antes el menú lateral navegaba sin llevar la pantalla y pedía confirmar «Salir sin guardar»). Con Ctrl, Mayús o clic del medio, o estando en Inicio, el POS o el login, el ítem del menú navega normal. **Estando ya en la pantalla dividida**, el ítem **Pantalla dividida** del menú queda **inhabilitado** (atenuado, con el aviso «Ya estás en la pantalla dividida»): tocarlo no hace nada, ya no recarga la página ni los paneles (desde 2026-11-24). El botón *Dividir pantalla* de la barra superior directamente no aparece acá.

En la **barra superior** de todas las pantallas del layout (a la izquierda del modo oscuro; solo en computadora) está el botón **Dividir pantalla**: lleva **esa pantalla** a la pantalla dividida como un panel y deja **un panel vacío y activo al lado** (con la lista de módulos y el buscador) para que elijas con cuál compararla. Si ya había un panel vacío se usa ese; con 6 paneles no se agrega y avisa. Si ya tenías una distribución guardada se **suma**: la pantalla entra en el primer panel vacío o en uno nuevo a la derecha. Si estabas **a mitad de una carga**, seguís con ella en el panel (ver *Seguir con lo cargado*).

## Seguir con lo cargado
En una carga **nueva** de **Elaborados**, **Elaborados rápido** (ingreso rápido), **Movimientos** o **Stock**, y en el alta de una **compra**, de una **persona** o de un **producto**, lo que tenés cargado **viaja con la pantalla**, sin el aviso «Salir sin guardar», al:
- **Dividir pantalla** (de la pantalla normal al panel),
- **Cerrar pantalla dividida** (del panel a la pantalla normal),
- **Pasar a una ventana independiente** y **Traer de vuelta al panel**.

Compras, personas y productos **ya guardados** no viajan: se vuelven a cargar del servidor. El alta de un **usuario** todavía **no** viaja: sigue mostrando el aviso del navegador al salir.

La pantalla se vuelve a abrir en el lugar nuevo con los mismos datos y avisa *«Seguís con lo que tenías cargado.»*. Sigue siendo **el mismo borrador** (en *… sin guardar* no aparece uno nuevo por cada paso). En el resto de las pantallas, y al **editar** un registro ya guardado, con cambios sin guardar sigue apareciendo el aviso «Salir sin guardar».

## Si ya tenías otro «Nuevo» abierto
Al abrir un **Nuevo** de Movimientos, Stock o Elaborados (también en un panel) con otro del mismo tipo abierto en otro panel, pestaña o ventana, aparece un aviso con dos opciones: **Seguir con ese acá** (trae lo cargado y la otra pantalla **se cierra sola**: el panel se cierra, la ventana independiente se cierra, una pestaña del navegador vuelve al listado) o **Empezar uno nuevo** (deja el otro como está). Si el otro está **vacío** se cierra solo, sin preguntar. Detalle en la ficha de cada pantalla.

## Cerrar la pantalla dividida
El botón **Cerrar pantalla dividida** (en la barra de arriba, junto a *3 columnas* y *4 en cuadro*) deja de dividir y muestra **esa pantalla entera en el layout normal**, con el menú lateral. Solo se puede usar cuando queda **un único panel con una pantalla cargada**:
- Con **2 o más paneles** el botón se ve apagado y, al tocarlo, avisa que hay que cerrar o juntar paneles hasta que quede uno.
- Con **un único panel vacío** el botón lleva al **dashboard** (Inicio) y borra la distribución guardada.
- Con la pantalla **en una ventana independiente**, avisa por qué no se puede (primero **Traer de vuelta al panel**).
- Una carga nueva de Elaborados, Elaborados rápido, Movimientos o Stock **sigue con lo cargado** en la pantalla entera; en otras pantallas con **cambios sin guardar** antes aparece el aviso «Salir sin guardar».
- Al cerrar se **borra la distribución guardada**: la próxima vez que entres a la pantalla dividida arranca con un panel vacío.

## Botones de la cabecera de cada panel
- **Atrás** y **Recargar**: actúan solo sobre ese panel. **Atrás** vuelve a la pantalla anterior **de ese panel** (la que tenía antes de tocar un enlace adentro); si no hay una, el panel **vuelve al menú de módulos** (queda como un panel nuevo). Nunca sale de la pantalla dividida ni mueve otros paneles (desde 2026-11-24; antes usaba el historial de toda la pestaña). En un panel que ya está en el menú de módulos el botón queda apagado. Con cambios sin guardar pide el aviso «Salir sin guardar», igual que antes. Si el navegador no soporta el historial por panel (solo Chrome/Edge lo tienen hoy), **Atrás** lleva siempre al menú de módulos.
- **Agregar panel ▾** (desde 2026-11-24; reemplaza a *Dividir a lo ancho / a lo alto* y *Maximizar*): ver el paso 3 de *Cómo se usa*. Con el máximo de **6 paneles** queda apagado y avisa que hay que cerrar uno. El texto «Agregar panel» se oculta en los paneles angostos (menos de unos 330 px) y queda solo el **+**. Ya **no hay Maximizar** ni **Dividir a lo alto**, y el doble clic en la cabecera no hace nada. Para ver un panel más grande, arrastrá los divisores o pasalo a una ventana independiente. Los paneles apilados de una distribución guardada antes siguen funcionando, y *4 en cuadro* sigue acomodándolos en cuadro.
- **Pasar a una ventana independiente** (*Independizar*): **mueve** esa pantalla a una ventana propia (por ejemplo para otro monitor). Ver *Pasar un panel a una ventana independiente*.
- **Cerrar panel**: el espacio lo ocupa el panel vecino. Si es el último, queda un panel vacío.

El título de cada panel muestra **«Sección · Pantalla»**.

## Clic derecho en el menú lateral
Sobre cualquier módulo del menú (no el punto de venta):
- **Estando en otra pantalla**: **Abrir en pantalla dividida** va a la pantalla dividida con ese módulo cargado en un panel.
- **Estando en la pantalla dividida**: **Abrir en un panel nuevo** (a la derecha de todo, o en el primer panel vacío) y **Abrir en el panel activo**.
- **Abrir en ventana independiente** sigue disponible como siempre (**copia**: abre la ventana y no toca los paneles).

## Pasar toda la pantalla dividida a una ventana independiente
El botón **Pasar a ventana independiente** (en la barra de arriba, junto a *3 columnas*, *4 en cuadro* y *Cerrar pantalla dividida*) **mueve toda la distribución** —todos los paneles, con sus tamaños y lo que tiene cada uno— a **una ventana nueva sin menú lateral ni barra superior**. Esta pestaña **vuelve a Inicio**.
- Una carga **nueva** de Elaborados, Elaborados rápido, Movimientos, Stock, Compras, Personas o Productos **viaja con lo cargado**, sin avisos. Si algún panel tiene **cambios sin guardar que no pueden viajar** (por ejemplo, la edición de un registro ya guardado), aparece **una vez** el aviso «Salir sin guardar».
- Los paneles que ya estaban en su **propia ventana independiente** siguen apuntando a esa ventana.
- Hay **una sola** pantalla dividida independiente: si ya hay una, se la trae al frente y se avisa (cerrala antes de pasar otra).
- La ventana guarda **su propia distribución** (no la de la pestaña). Dentro de la ventana el botón no aparece.
- Si el navegador **bloquea la ventana emergente**, se avisa y todo queda como estaba: permitila para este sitio y probá de nuevo.

## Pasar un panel a una ventana independiente
El botón **Pasar a una ventana independiente** (el de la flecha hacia afuera, en la cabecera del panel) **mueve** la pantalla a una ventana propia, sin menú lateral ni barra superior:
1. La ventana se abre **donde estaba el panel** y con su tamaño (mínimo 480 × 360; es aproximado porque el navegador agrega su barra de título) y carga la misma pantalla.
2. El panel se vacía y muestra **«<Pantalla> se abrió en una ventana independiente»** con dos botones: **Ir a la pantalla independiente** (trae la ventana al frente) y **Traer de vuelta al panel** (cierra la ventana y vuelve a cargar la pantalla en el panel).
3. Una carga nueva de Elaborados, Elaborados rápido, Movimientos o Stock **pasa con lo cargado** a la ventana, y vuelve con lo cargado al traerla de vuelta. En otras pantallas con **cambios sin guardar** antes aparece el aviso «Salir sin guardar», porque la pantalla se vuelve a abrir vacía. Si ya había una ventana de esa misma pantalla abierta, la carga **no** se pasa (pisaría la otra): se avisa y el panel sigue como estaba.
4. Si **cerrás la ventana**, el panel avisa **«La ventana independiente se cerró»** con el botón **Volver a cargarla en el panel** (no vuelve solo).
5. Una pantalla tiene **una sola ventana**: si ya estaba abierta (por ejemplo desde el clic derecho del menú), el panel queda con el aviso y esa ventana se trae al frente sin recargarla.
6. El panel con el aviso se puede **cerrar** o **agregarle un panel al lado** como cualquier otro; al cargarle otra pantalla (menú lateral o listado) se convierte en un panel común y la ventana sigue abierta.
7. La marca se **recuerda** al recargar la pantalla dividida; como el navegador no deja seguir a la ventana tras recargar, que se cerró se descubre al tocar **Ir a la pantalla independiente**.

Al usar **3 columnas / 4 en cuadro**, los paneles en ventana independiente no se reacomodan: sus ventanas siguen abiertas. Si cerrás un panel que está en una ventana, la ventana **sigue abierta** pero deja de contar para el permiso de operador autorizado.

## Cambios sin guardar
Si cerrás un panel, o cargás otra pantalla en él, y tiene **cambios sin guardar**, aparece el aviso **«Salir sin guardar»** (ver *Aviso de salir sin guardar*). Lo mismo si usás **3 columnas / 4 en cuadro** con algún panel con cambios. Agregar un panel o arrastrar un divisor **no recargan** los paneles: no se pierde nada.

**F5 sin querer** (2026-11-21): si algún panel tiene una carga o una edición sin guardar, **F5, Ctrl+R, Ctrl+F5 y Ctrl+Mayús+R no recargan**: aparece el aviso **«Salir sin guardar»** con **«No, quedarme»** marcado por omisión, así que un F5 seguido de Enter **no pierde nada** (hay que elegir «Sí, salir» a propósito, con el mouse, con la tecla S o con las flechas). Pasa igual con el foco dentro de un panel, y también con los botones **Recargar** y **Atrás** de la cabecera del panel. Sin cambios sin guardar, F5 funciona normal. **Límite:** el botón de recargar del navegador, la barra de direcciones y cerrar la pestaña no se pueden interceptar: ahí sigue el aviso del propio navegador (con «Recargar» marcado). 

**Si se recarga igual** (botón de recargar del navegador, aviso del navegador aceptado, el equipo se cuelga): al volver a abrirse la pantalla dividida, cada panel con una **carga nueva** (Elaborados, Elaborados rápido, Movimientos, Stock, Compras, Personas o Productos) **recupera lo que tenía**, con el aviso «Seguís con lo que tenías cargado.». La pantalla dividida guarda una copia de lo cargado en cada panel **cada pocos segundos** (en este navegador, hasta 12 horas), así que lo que escribiste en los últimos 3 segundos antes de recargar puede no estar. Detalles:
- **Solo cargas nuevas.** La **edición de un registro ya guardado** no se recupera: al recargar vuelve a abrirse como está guardado.
- **Solo al recargar.** Si entrás de nuevo por un enlace o el menú, o elegís **«Sí, salir»** en el aviso de la app, o tocás **Recargar** de la cabecera de un panel y confirmás, se entiende que querías descartar y **no** se recupera nada.
- Si **guardás** la carga o la **cerrás**, la copia se borra sola.
- La contraseña de un usuario nunca se guarda en estas copias. Al ser datos guardados en el navegador de la PC, quedan ahí hasta que guardes, descartes o pasen las 12 horas.
- La carga recuperada sigue siendo **el mismo borrador** en **«… sin guardar»** (no aparece uno nuevo).

## Permisos especiales (operador autorizado)
Algunas pantallas piden **autorizar a un operador** (Compras, Ventas, Finanzas / cuenta corriente, Egresos de Caja). Esa autorización **dura mientras esa pantalla siga abierta en algún panel**: podés trabajar en los otros paneles sin perderla, y **se pierde al cerrar ese panel** o al cargar otra pantalla ahí. La pantalla dividida avisa al servidor qué pantallas siguen abiertas cada vez que un panel carga o se cierra.

## Modificar una venta desde el detalle de un panel
Desde 2026-11-24, en el **detalle de una venta** abierto en un panel, **Modificar venta** y **Cambiar forma de pago** (los enlaces al POS) **no sacan al panel de su pantalla**:
1. Aparece el aviso *«La edición de la venta #N se abre en una ventana aparte…»* y el POS se abre **en una ventana nueva** (una por venta; si ya está abierta, se la trae al frente).
2. El **detalle sigue en el panel**, con una **franja azul** arriba: *«Modificando la venta #N en otra ventana. Al guardar, este detalle se actualiza solo.»* y el botón **Ir a esa ventana**.
3. Al **guardar**, la ventana del POS **se cierra sola**, el detalle del panel **se recarga** con los datos nuevos y aparece *«Venta #N actualizada.»*.
4. Si usás **Salir de la edición** (o cerrás la ventana), no se guarda nada: la ventana se cierra, la franja desaparece y el detalle queda como estaba.
Si el navegador bloquea la ventana emergente, avisa y hay que permitirla para este sitio. Solo se recargan los paneles que muestran el detalle **de esa venta**; un listado de ventas abierto en otro panel no se actualiza solo.

## Qué no se abre en un panel
- **El punto de venta**: tiene su propia ventana. Si un panel termina yendo al POS (por un enlace de adentro), el panel muestra *«El punto de venta se abre aparte»* con un botón para abrirlo en una ventana independiente (si desde ahí volvés **Atrás**, el panel recupera su pantalla anterior; desde 2026-11-24 el marco ya no queda oculto). La excepción es **Modificar venta / Cambiar forma de pago** del detalle de una venta: ver *Modificar una venta desde el detalle de un panel*. El enlace **Punto de Venta** del menú lateral funciona como siempre (sale de la pantalla dividida).
- **Inicio**, el **login** y la propia pantalla dividida.

## Se recuerda tu distribución
La última distribución (paneles, tamaños y qué pantalla tiene cada uno) se guarda **en esta computadora y para tu usuario**; al volver a entrar se reabre igual. No se guarda en el servidor: en otra PC arranca vacía. Si lo guardado no es válido, se ignora y arranca con un panel vacío.

## Aviso de documento abierto
Si abrís el **mismo documento** (por ejemplo la misma compra o el mismo movimiento) en dos paneles, o en un panel y en una pestaña, la app avisa que ya está abierto, igual que con dos pestañas; en el aviso el panel figura como *«un panel de la Pantalla dividida»*.

## Límites conocidos
- Dos pantallas **«Nuevo»** del mismo tipo abiertas a la vez disparan el aviso de documento abierto.
- Cerrar la pantalla dividida (o recargarla) recarga todos los paneles; los que tienen cambios sin guardar piden confirmación (con el teclado, por el aviso de la app; con el botón de recargar del navegador, por el aviso del navegador).
- Verificado en el navegador el 2026-11-20: 3 columnas con Stock, Movimientos y Reportes sin menú adentro; dividir, arrastrar divisores, maximizar y restaurar **sin recargar** los paneles; clic en el menú lateral en el panel activo; clic derecho (panel nuevo y desde otra pantalla); cerrar con el aviso «Salir sin guardar»; el POS se rechaza con su aviso; la distribución se restaura al recargar; el aviso de pantalla angosta.
- Verificado en el navegador el 2026-11-20 (tercera ronda): Nuevo Elaborado con elaborado y una línea sin guardar, **sin ningún aviso**: pantalla normal → **Dividir pantalla** → panel → **Pasar a una ventana independiente** → **Traer de vuelta al panel** → **Cerrar pantalla dividida** → pantalla normal, con los mismos datos en cada paso y **un solo borrador** en el servidor (la ventana se simuló con un iframe del mismo sitio).
- Verificado en el navegador el 2026-11-20 (segunda ronda): botón **Dividir pantalla** de la barra superior (también con aviso por cambios sin guardar), **Cerrar pantalla dividida** (apagado con 2 paneles y con la pantalla en ventana independiente; con 1 panel abre la pantalla entera y borra lo guardado), y **Independizar** con la apertura de ventana simulada (nombre, ubicación y tamaño pedidos, aviso en el panel, ir a la ventana, detección de ventana cerrada, volver a cargarla, traer de vuelta con confirmación y recuerdo tras recargar).
- **PENDIENTE de verificar en el navegador**: la ventana popup **real** (el navegador integrado de la herramienta de prueba no abre popups: se simuló `window.open`), que el operador autorizado de Compras/Ventas se pierda al cerrar su panel (probado solo con tests automáticos y llamando al servidor), y el aviso de documento abierto entre dos paneles.
