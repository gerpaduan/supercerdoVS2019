---
pantalla: Home.Utilidades
titulo: Utilidades (contador de billetes, aplicaciones instalables y agentes locales)
rol: usuario
modulo: Configuración
orden: 1
permiso: ninguno (el ítem Utilidades del menú Configuración lo ve todo usuario; verificado en `Views/Shared/_Layout.cshtml`)
revisada: 2026-11-17
---
## Para qué sirve
Reúne las herramientas auxiliares que cualquier usuario puede necesitar sin entrar a otra pantalla: el **contador de billetes** y la descarga de los **agentes locales** (impresora y balanza) que se instalan en cada PC.

## Contador de billetes
- Arriba de la pantalla hay una tarjeta **Contador de billetes** con el botón **Abrir contador**. También se abre con **Alt + B** desde esta pantalla y desde cualquier otra pantalla del sistema con menú lateral, siempre que no haya una ventana (modal) abierta; si la hay y no trae su propio botón del contador, Alt + B no hace nada. En el punto de venta Alt + B / F3 abren el mismo contador.
- Se abre la misma calculadora que usa el punto de venta con **F3**: se carga la cantidad de billetes de cada denominación y las monedas, y suma el total.
- Funciona **suelta**: no vuelca el total en ningún campo. Al **Aceptar** con un total mayor a cero ofrece qué hacer con el conteo: no imprimir, ticket, PDF o WhatsApp. Es el mismo modal de siempre, no tiene configuración propia.
- Atajos dentro del contador: **Alt + Enter** acepta, **Esc** o **Alt + C** cierran (si hay algo contado pregunta antes).

## Aplicaciones instalables
- La tarjeta **Aplicaciones instalables** tiene un botón **Instalar** para **Stock**, **Movimientos**, **Elaborados**, **Punto de venta**, **Punto de expendio** y **Existencia por sucursal**. Cada una se instala como aplicación independiente: con **su nombre** (ej. «Punto de venta»), **su ícono** y su propia ventana, **sin menú lateral**, y solo se puede usar esa pantalla (los enlaces a otras secciones muestran «No disponible en esta ventana»).
- Al tocar **Instalar** se abre una **ventana chica** con el nombre y el ícono de la app y un botón **Instalar**. Al tocarlo, el navegador pide confirmar; cuando la app queda instalada, esa ventana se cierra sola y la app se abre desde su ícono.
- Para tener el ícono en la **barra de tareas**: clic derecho sobre el ícono de la app (con la app abierta) → **Anclar a la barra de tareas**. El navegador no deja hacerlo desde la página.
- Si el botón no aparece, la ventana muestra los pasos manuales (menú ⋮ del navegador → «Guardar y compartir» → «Instalar página como aplicación»). Pasa cuando la app ya estaba instalada o cuando el sitio no es `https://carnisys.com`: en Servidor SM y San Lorenzo el navegador no deja instalar (certificado no válido). Si el navegador bloquea la ventana chica, el botón abre la misma página en una pestaña.
- Si por error se instaló una app llamada **CarniSys** (la app general), se desinstala desde `chrome://apps` (clic derecho → Quitar).
- Funciona con Chrome o Edge. PENDIENTE: no se probó la instalación real en producción.

## Agentes locales
Cada tarjeta muestra el agente (impresión, balanza), su versión y un botón **Descargar**; la nota de la tarjeta dice dónde instalarlo. Debajo, el sector **Más herramientas** queda reservado para sumar utilidades.

## Tamaño de las apps instaladas
Las apps instaladas y las ventanas independientes de carga (Stock, Movimientos, Elaborados, Existencia) tienen los botones **− / +** junto a «… sin guardar» para achicar o agrandar el **ancho** de la ventana un 10 % por clic; ver la sección «Botones − / + de ancho» de cada pantalla (por ejemplo [Stock](ayuda:pantalla/Stock.Editar)).
