---
pantalla: PantallaDividida.Index
titulo: Pantalla dividida (varios módulos lado a lado)
rol: usuario
modulo: Inicio
orden: 2
permiso: ninguno propio (el ítem del menú lo ve todo usuario; cada panel muestra la pantalla real y aplica los permisos de esa pantalla)
revisada: 2026-11-20
---
## Para qué sirve
Ver **varios módulos a la vez en una sola pantalla** para compararlos: por ejemplo **Stock** en una columna, **Movimientos** en otra y un **Reporte** en la tercera. Vos decidís cuántos paneles hay y cómo se reparten el espacio.

Cada panel es la pantalla real del módulo (sin menú lateral ni barra superior): lo que ves y podés hacer en un panel es lo mismo que en una pestaña normal, con **tus mismos permisos**.

Solo está en **computadora** (pantalla de 992 px o más). En el celular o la tablet se muestra un aviso: abrí cada módulo por separado.

## Cómo se usa
1. Entrá por **Pantalla dividida** (arriba de todo en el menú lateral). La primera vez hay **un panel vacío** con la lista de módulos y un buscador.
2. **Elegí un módulo** en la lista del panel vacío, o tocá un enlace del **menú lateral**: se abre **en el panel activo** (el que tiene el borde azul; el último que tocaste).
3. **Dividí** un panel con los botones de su cabecera: **Dividir a lo ancho** (uno al lado del otro) o **Dividir a lo alto** (uno arriba del otro). El panel nuevo queda vacío y activo.
4. **Arrastrá los divisores** (las franjas entre paneles) para cambiar los tamaños. Con el divisor enfocado (Tab) también se mueve con las **flechas del teclado**. Al dividir un panel o arrastrar un divisor ningún panel queda más chico que unos 220 px; al sumar paneles con el clic derecho del menú pueden quedar algo más angostos (ajustalos arrastrando los divisores).
5. Atajos en la barra de arriba: **3 columnas** y **4 en cuadro** acomodan los paneles **conservando lo que ya tenías abierto**, en orden.

Hay un máximo de **6 paneles**; el contador (*n/6 paneles*) está arriba a la derecha.

## Entrar desde cualquier pantalla: «Dividir pantalla»
En la **barra superior** de todas las pantallas del layout (a la izquierda del modo oscuro; solo en computadora) está el botón **Dividir pantalla**: lleva **esa pantalla** a la pantalla dividida como un panel, y desde ahí seguís dividiendo. Si ya tenías una distribución guardada se **suma**: la pantalla entra en el primer panel vacío o en uno nuevo a la derecha. Si estabas **a mitad de una carga**, seguís con ella en el panel (ver *Seguir con lo cargado*).

## Seguir con lo cargado
En una carga **nueva** de **Elaborados**, **Elaborados rápido** (ingreso rápido), **Movimientos** o **Stock**, lo que tenés cargado **viaja con la pantalla**, sin el aviso «Salir sin guardar», al:
- **Dividir pantalla** (de la pantalla normal al panel),
- **Cerrar pantalla dividida** (del panel a la pantalla normal),
- **Pasar a una ventana independiente** y **Traer de vuelta al panel**.

La pantalla se vuelve a abrir en el lugar nuevo con los mismos datos y avisa *«Seguís con lo que tenías cargado.»*. Sigue siendo **el mismo borrador** (en *… sin guardar* no aparece uno nuevo por cada paso). En el resto de las pantallas, y al **editar** un registro ya guardado, con cambios sin guardar sigue apareciendo el aviso «Salir sin guardar».

## Cerrar la pantalla dividida
El botón **Cerrar pantalla dividida** (en la barra de arriba, junto a *3 columnas* y *4 en cuadro*) deja de dividir y muestra **esa pantalla entera en el layout normal**, con el menú lateral. Solo se puede usar cuando queda **un único panel con una pantalla cargada**:
- Con **2 o más paneles** el botón se ve apagado y, al tocarlo, avisa que hay que cerrar o juntar paneles hasta que quede uno.
- Con el panel **vacío**, o con su pantalla **en una ventana independiente**, también avisa por qué no se puede (en el segundo caso, primero **Traer de vuelta al panel**).
- Una carga nueva de Elaborados, Elaborados rápido, Movimientos o Stock **sigue con lo cargado** en la pantalla entera; en otras pantallas con **cambios sin guardar** antes aparece el aviso «Salir sin guardar».
- Al cerrar se **borra la distribución guardada**: la próxima vez que entres a la pantalla dividida arranca con un panel vacío.

## Botones de la cabecera de cada panel
- **Atrás** y **Recargar**: actúan solo sobre ese panel.
- **Dividir a lo ancho / a lo alto**.
- **Maximizar / Restaurar**: el panel ocupa toda la zona (también con doble clic en la cabecera); los demás siguen cargados detrás y no pierden nada.
- **Pasar a una ventana independiente** (*Independizar*): **mueve** esa pantalla a una ventana propia (por ejemplo para otro monitor). Ver *Pasar un panel a una ventana independiente*.
- **Cerrar panel**: el espacio lo ocupa el panel vecino. Si es el último, queda un panel vacío.

El título de cada panel muestra **«Sección · Pantalla»**.

## Clic derecho en el menú lateral
Sobre cualquier módulo del menú (no el punto de venta):
- **Estando en otra pantalla**: **Abrir en pantalla dividida** va a la pantalla dividida con ese módulo cargado en un panel.
- **Estando en la pantalla dividida**: **Abrir en un panel nuevo** (a la derecha de todo, o en el primer panel vacío) y **Abrir en el panel activo**.
- **Abrir en ventana independiente** sigue disponible como siempre (**copia**: abre la ventana y no toca los paneles).

## Pasar un panel a una ventana independiente
El botón **Pasar a una ventana independiente** (el de la flecha hacia afuera, en la cabecera del panel) **mueve** la pantalla a una ventana propia, sin menú lateral ni barra superior:
1. La ventana se abre **donde estaba el panel** y con su tamaño (mínimo 480 × 360; es aproximado porque el navegador agrega su barra de título) y carga la misma pantalla.
2. El panel se vacía y muestra **«<Pantalla> se abrió en una ventana independiente»** con dos botones: **Ir a la pantalla independiente** (trae la ventana al frente) y **Traer de vuelta al panel** (cierra la ventana y vuelve a cargar la pantalla en el panel).
3. Una carga nueva de Elaborados, Elaborados rápido, Movimientos o Stock **pasa con lo cargado** a la ventana, y vuelve con lo cargado al traerla de vuelta. En otras pantallas con **cambios sin guardar** antes aparece el aviso «Salir sin guardar», porque la pantalla se vuelve a abrir vacía. Si ya había una ventana de esa misma pantalla abierta, la carga **no** se pasa (pisaría la otra): se avisa y el panel sigue como estaba.
4. Si **cerrás la ventana**, el panel avisa **«La ventana independiente se cerró»** con el botón **Volver a cargarla en el panel** (no vuelve solo).
5. Una pantalla tiene **una sola ventana**: si ya estaba abierta (por ejemplo desde el clic derecho del menú), el panel queda con el aviso y esa ventana se trae al frente sin recargarla.
6. El panel con el aviso se puede **dividir, maximizar o cerrar** como cualquier otro; al cargarle otra pantalla (menú lateral o listado) se convierte en un panel común y la ventana sigue abierta.
7. La marca se **recuerda** al recargar la pantalla dividida; como el navegador no deja seguir a la ventana tras recargar, que se cerró se descubre al tocar **Ir a la pantalla independiente**.

Al usar **3 columnas / 4 en cuadro**, los paneles en ventana independiente no se reacomodan: sus ventanas siguen abiertas. Si cerrás un panel que está en una ventana, la ventana **sigue abierta** pero deja de contar para el permiso de operador autorizado.

## Cambios sin guardar
Si cerrás un panel, o cargás otra pantalla en él, y tiene **cambios sin guardar**, aparece el aviso **«Salir sin guardar»** (ver *Aviso de salir sin guardar*). Lo mismo si usás **3 columnas / 4 en cuadro** con algún panel con cambios. Dividir, arrastrar un divisor o maximizar **no recargan** los paneles: no se pierde nada.

## Permisos especiales (operador autorizado)
Algunas pantallas piden **autorizar a un operador** (Compras, Ventas, Finanzas / cuenta corriente, Egresos de Caja). Esa autorización **dura mientras esa pantalla siga abierta en algún panel**: podés trabajar en los otros paneles sin perderla, y **se pierde al cerrar ese panel** o al cargar otra pantalla ahí. La pantalla dividida avisa al servidor qué pantallas siguen abiertas cada vez que un panel carga o se cierra.

## Qué no se abre en un panel
- **El punto de venta**: tiene su propia ventana. Si un panel termina yendo al POS (por un enlace de adentro), el panel muestra *«El punto de venta se abre aparte»* con un botón para abrirlo en una ventana independiente. El enlace **Punto de Venta** del menú lateral funciona como siempre (sale de la pantalla dividida).
- **Inicio**, el **login** y la propia pantalla dividida.

## Se recuerda tu distribución
La última distribución (paneles, tamaños y qué pantalla tiene cada uno) se guarda **en esta computadora y para tu usuario**; al volver a entrar se reabre igual. No se guarda en el servidor: en otra PC arranca vacía. Si lo guardado no es válido, se ignora y arranca con un panel vacío.

## Aviso de documento abierto
Si abrís el **mismo documento** (por ejemplo la misma compra o el mismo movimiento) en dos paneles, o en un panel y en una pestaña, la app avisa que ya está abierto, igual que con dos pestañas; en el aviso el panel figura como *«un panel de la Pantalla dividida»*.

## Límites conocidos
- Dos pantallas **«Nuevo»** del mismo tipo abiertas a la vez disparan el aviso de documento abierto.
- Cerrar la pantalla dividida (o recargarla) recarga todos los paneles; los que tienen cambios sin guardar piden confirmación por el aviso del navegador.
- Verificado en el navegador el 2026-11-20: 3 columnas con Stock, Movimientos y Reportes sin menú adentro; dividir, arrastrar divisores, maximizar y restaurar **sin recargar** los paneles; clic en el menú lateral en el panel activo; clic derecho (panel nuevo y desde otra pantalla); cerrar con el aviso «Salir sin guardar»; el POS se rechaza con su aviso; la distribución se restaura al recargar; el aviso de pantalla angosta.
- Verificado en el navegador el 2026-11-20 (tercera ronda): Nuevo Elaborado con elaborado y una línea sin guardar, **sin ningún aviso**: pantalla normal → **Dividir pantalla** → panel → **Pasar a una ventana independiente** → **Traer de vuelta al panel** → **Cerrar pantalla dividida** → pantalla normal, con los mismos datos en cada paso y **un solo borrador** en el servidor (la ventana se simuló con un iframe del mismo sitio).
- Verificado en el navegador el 2026-11-20 (segunda ronda): botón **Dividir pantalla** de la barra superior (también con aviso por cambios sin guardar), **Cerrar pantalla dividida** (apagado con 2 paneles y con la pantalla en ventana independiente; con 1 panel abre la pantalla entera y borra lo guardado), y **Independizar** con la apertura de ventana simulada (nombre, ubicación y tamaño pedidos, aviso en el panel, ir a la ventana, detección de ventana cerrada, volver a cargarla, traer de vuelta con confirmación y recuerdo tras recargar).
- **PENDIENTE de verificar en el navegador**: la ventana popup **real** (el navegador integrado de la herramienta de prueba no abre popups: se simuló `window.open`), que el operador autorizado de Compras/Ventas se pierda al cerrar su panel (probado solo con tests automáticos y llamando al servidor), y el aviso de documento abierto entre dos paneles.
