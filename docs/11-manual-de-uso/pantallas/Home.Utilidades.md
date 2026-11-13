---
pantalla: Home.Utilidades
titulo: Utilidades (contador de billetes, aplicaciones instalables y agentes locales)
rol: usuario
modulo: Configuración
orden: 1
permiso: ninguno (el ítem Utilidades del menú Configuración lo ve todo usuario; verificado en `Views/Shared/_Layout.cshtml`)
revisada: 2026-11-13
---
## Para qué sirve
Reúne las herramientas auxiliares que cualquier usuario puede necesitar sin entrar a otra pantalla: el **contador de billetes** y la descarga de los **agentes locales** (impresora y balanza) que se instalan en cada PC.

## Contador de billetes
- Arriba de la pantalla hay una tarjeta **Contador de billetes** con el botón **Abrir contador**. También se abre con **Alt + B** desde esta pantalla y desde cualquier otra pantalla del sistema con menú lateral, siempre que no haya una ventana (modal) abierta; si la hay y no trae su propio botón del contador, Alt + B no hace nada. En el punto de venta Alt + B / F3 abren el mismo contador.
- Se abre la misma calculadora que usa el punto de venta con **F3**: se carga la cantidad de billetes de cada denominación y las monedas, y suma el total.
- Funciona **suelta**: no vuelca el total en ningún campo. Al **Aceptar** con un total mayor a cero ofrece qué hacer con el conteo: no imprimir, ticket, PDF o WhatsApp. Es el mismo modal de siempre, no tiene configuración propia.
- Atajos dentro del contador: **Alt + Enter** acepta, **Esc** o **Alt + C** cierran (si hay algo contado pregunta antes).

## Aplicaciones instalables
- La tarjeta **Aplicaciones instalables** tiene un botón **Instalar** para **Stock**, **Movimientos** y **Elaborados**. Cada una se instala como aplicación propia: abre en su ventana y tiene su ícono en la barra de tareas.
- Al tocar **Instalar** se abre la pantalla en una pestaña nueva y arriba aparece una barra azul con el botón **Instalar**; al tocarlo el navegador pide confirmar. Después se puede cerrar la pestaña y abrir la app desde su ícono.
- Si la barra no ofrece el botón, muestra los pasos manuales (menú ⋮ del navegador → «Guardar y compartir» → «Instalar página como aplicación»). Pasa cuando la app ya estaba instalada o cuando el sitio no es `https://carnisys.com`: en Servidor SM y San Lorenzo el navegador no deja instalar (certificado no válido).
- Funciona con Chrome o Edge. PENDIENTE: no se probó la instalación real en producción.

## Agentes locales
Cada tarjeta muestra el agente (impresión, balanza), su versión y un botón **Descargar**; la nota de la tarjeta dice dónde instalarlo. Debajo, el sector **Más herramientas** queda reservado para sumar utilidades.
