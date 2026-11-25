---
pantalla: PuntosExpendio.POS
titulo: Punto de expendio (pantalla de trabajo y app instalada)
rol: usuario
modulo: Punto de Expendio
orden: 1
permiso: cualquier usuario activo puede operar Expendio (verificado en `PuntosExpendioController.POS`); no pide permiso POS ni apertura de caja
revisada: 2026-11-25
---
## Para qué sirve
Pantalla de trabajo del **Punto de expendio**: se elige el sector y se cargan los expendios. **Esta ayuda cubre solo cómo usarlo como aplicación instalada (desde 2026-11-17).** El resto de la pantalla está PENDIENTE de documentar.

## Como aplicación instalada
- Se instala desde **Configuración → Utilidades → Aplicaciones instalables → Punto de expendio → Instalar** (ver [Utilidades](ayuda:pantalla/Home.Utilidades)). Queda con el nombre **Punto de expendio**, su propio ícono y su propia ventana.
- **Desde 2026-11-25, en una PC con mouse, el clic izquierdo común sobre «Abrir punto de expendio» del menú lateral abre siempre una ventana independiente** (ya no reemplaza la pestaña actual); con Ctrl, Mayús o Alt se deja lo del navegador.
- Abre directo en esta pantalla, sin menú lateral. Los enlaces a otras secciones del sistema no funcionan en esta ventana: muestran «No disponible en esta ventana».
- El botón **Cerrar** (arriba; fuera de la app instalada se llama **Inicio** y va a Inicio) y **Cancelar** al elegir el sector **cierran la ventana** en lugar de ir a Inicio. Si hay un expendio a medio cargar, el navegador pregunta antes de cerrar.
- **Elección de operador (cuenta de producción, desde 2026-11-21):** el cartel «Autorizate para operar este Punto de Expendio» **no se puede cerrar**: no tiene X ni «Cancelar», y si se cerrara por algún motivo se vuelve a abrir solo (no queda la pantalla en blanco ni hace falta F5). Para salir, cerrá la ventana con la X; en una pestaña normal queda el enlace **Ir a Inicio**. «Cambiar operario» sigue con su «Cancelar».
- Si se vence la sesión, la ventana pide usuario y clave y vuelve a esta pantalla. Lo mismo si **cerrás sesión** dentro de la app (o se cierra por inactividad): al volver a ingresar en esa ventana entrás de nuevo acá, no a Inicio (desde 2026-11-21). Con la cuenta de producción aparece la elección de operador y después la del sector.
- **Sin menú solo en las apps de Utilidades:** únicamente las apps instaladas desde **Utilidades** abren sin menú lateral. Si usás la app general **CarniSys** (o una pestaña del navegador) y entrás a esta pantalla, el **menú se ve** y los enlaces funcionan; la ventana de **Abrir en ventana** sí va sin menú. Si instalaste esta app antes del 2026-11-17, **desinstalala y reinstalala desde Utilidades** para que tome esta regla (Stock, Movimientos y Elaborados ya la traían).
- PENDIENTE: no se probó la instalación real en producción.

## Total en vivo y producto sin agregar (desde 2026-11-18)
Igual que en el POS de Ventas (ver [Punto de venta](ayuda:pantalla/Ventas.POS), sección «Total en vivo y producto sin agregar»):
- El **Total** suma el **Subtotal** del producto cargado antes de **Agregar a la venta** (con la etiqueta «con producto sin agregar» junto a «Total») y cambia en vivo con la cantidad; es solo visual, el expendio guarda lo agregado.
- **Finalizar** no avanza mientras el campo **Código** tenga algo escrito: avisa «El campo Código tiene un valor sin agregar…» y devuelve el cursor al campo. Hay que agregar el producto o borrar el código.

## Balanza y la tecla * (2026-11-19)
- **Sin balanza conectada, `*` deja el campo de cantidad vacío.** Si ya habías tipeado un valor a mano (por ejemplo 1,5) y apretás `*` para leer la balanza, aparece el aviso de que no hay balanza y la cantidad queda **vacía**: así un Enter distraído no guarda el valor viejo.
- **Al desactivar la balanza** (`*` otra vez o destildar el icono) mientras se estaba leyendo, la cantidad también se **vacía**.
- **Si la balanza se desconecta sola** con la lectura activa (y solo si en algún momento desde que se abrió la pantalla se había leído la balanza), la pantalla pasa a carga manual, vacía la cantidad y avisa «Se perdió la conexión con la balanza…». Espera dos chequeos seguidos sin conexión (aprox. 1 s) para no cortar por una falla momentánea.
- Detalle técnico y motivo: `docs/DECISIONS.md` (2026-11-19, tecla * de la balanza). Verificado solo por lectura de código; PENDIENTE probarlo con una balanza real.

## Peso siempre actualizado de la balanza (2026-11-20)
- El POS ya leía la balanza todo el tiempo desde que se abre; lo que cambia es qué pasa cuando la balanza **deja de mandar pesos nuevos** (abajo).
- **Cantidad muestra solo un peso fresco.** Si la balanza deja de mandar pesos nuevos (agente de balanza trabado o que no responde, cable flojo), en aprox. 1,5 s el campo **se vacía** y el estado de la balanza dice *«Lectura congelada (sin peso nuevo)»* o *«Agente no detectado»*; enseguida se pasa a carga manual con el aviso *«Se perdió la conexión con la balanza…»*. Nunca queda a la vista un peso viejo.
- **Enter (o Agregar) con la balanza activa** agrega la cantidad que muestra el campo, que con la balanza activa es siempre la última lectura fresca. **No agrega** y avisa si: no hay lectura fresca (*«Esperando lectura de la balanza…»*), el peso está **inestable** (*«Peso inestable, espere y presione Enter»*, debajo de Cantidad) o la balanza marca cero.
- El peso inestable ya se rechazaba; lo nuevo es que sin lectura fresca tampoco agrega (antes, si el agente no respondía, aceptaba lo que hubiera en Cantidad).
- Si vuelve a pasar que el peso no se actualiza, ver `docs/07-operacion-y-soporte/incidencias-frecuentes.md` (2026-11-20). Detalle técnico: `docs/DECISIONS.md` (2026-11-20, «Balanza: lectura vigente»).
- Verificado el 2026-11-20 en el POS de ventas (mismo código de balanza, `pos-balanza.js`) con un agente simulado. PENDIENTE: probarlo en esta pantalla y con una balanza real.

## Leer el código con la cámara (2026-11-20)
El recuadro de la cámara es más alto (120 px) y muestra el **centro** de la imagen, que es lo que se lee; el celular además aplica un zoom moderado si lo permite. **No cambia** la pausa: el mismo código no se vuelve a cargar antes de 2 s, y el alta rápida de un producto nuevo solo se ofrece con la segunda lectura igual. Detalle y consejos: concepto «Leer códigos de barra con la cámara del celular» (`conceptos/lectura-codigo-barra-camara.md`).
