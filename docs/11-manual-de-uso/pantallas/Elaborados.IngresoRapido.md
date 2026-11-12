---
pantalla: Elaborados.IngresoRapido
titulo: Ingreso rápido y desarme de elaborado (listado)
rol: usuario
modulo: Elaborados
orden: 2
alias: Elaborados.Desarme
permiso: PENDIENTE (no verificado contra el código en esta revisión)
revisada: 2026-11-12
---
## Para qué sirve
Elegir el producto que vas a registrar con **Ingreso rápido** o a desarmar con **Desarme de elaborado**. Es la misma pantalla para los dos; cambia la etiqueta (*Modo ingreso rapido* / *Modo desarme*) y el color del botón. Al elegir un producto se abre la carga de ese producto (ver [Ingreso rápido y desarme de elaborado](Elaborados.EditarIngresoRapido.md)).

## Buscar y elegir con el teclado
- Al entrar, el cursor queda en el cuadro de búsqueda (*Escribí nombre o código*). Mientras escribís se filtran los productos por nombre o por código, **sin importar tildes ni mayúsculas** (*chicharron* encuentra *Chicharrón*). La parte que coincide se marca en amarillo y un contador muestra cuántos quedan (*3 de 9*).
- El **primer producto** de la lista queda resaltado (con la marca **Enter**). Apretando **Enter** se abre ese producto, sin tocar el mouse.
- Con las flechas **← →** movés el resaltado de un producto al siguiente y con **↑ ↓** saltás una fila. **Esc** borra la búsqueda.
- Si nada coincide aparece *Ningún elaborado coincide con «…»*.

## Elegir con el mouse (computadora)
- Esta pantalla **no usa el diseño compacto** del resto del sistema, a propósito: está pensada para elegir rápido.
- Los productos se muestran como **tarjetas** en una grilla a todo el ancho. Cada tarjeta tiene la **inicial** del producto con un color según su código, el **nombre** (hasta dos líneas) y debajo *Cód* con el código y la receta, si tiene. Con **un clic** sobre la tarjeta se abre el producto; ya no hace falta doble clic.
- Se quitaron las columnas *Ingreso rápido* y *Fórmula*: la lista solo trae productos con ingreso rápido activado y con fórmula cargada, así que siempre decían *Sí* y *Cargada*.

## En el celular
Las tarjetas se apilan de a una por fila, a todo el ancho; se toca la tarjeta para abrir el producto. Se oculta la ayuda de teclas.

## Resto de la pantalla
PENDIENTE: documentar permisos y cuándo un producto aparece en la lista (no verificado contra el código en esta revisión; lo que sí se sabe: aparecen solo los productos de la empresa con ingreso rápido activado y fórmula cargada).
