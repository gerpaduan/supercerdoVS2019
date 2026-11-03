---
pantalla: Elaborados.IngresoRapido
titulo: Ingreso rápido y desarme de elaborado (listado)
rol: usuario
modulo: Elaborados
orden: 2
alias: Elaborados.Desarme
permiso: PENDIENTE (no verificado contra el código en esta revisión)
revisada: 2026-11-02
---
## Para qué sirve
Elegir el producto que vas a registrar con **Ingreso rápido** o a desarmar con **Desarme de elaborado**. Es la misma pantalla para los dos; cambia la etiqueta (*Modo ingreso rapido* / *Modo desarme*) y el color del botón. Al elegir un producto se abre la carga de ese producto (ver [Ingreso rápido y desarme de elaborado](Elaborados.EditarIngresoRapido.md)).

## Buscar
El cuadro *Producto o codigo* filtra la lista mientras escribís, por nombre o por código. Si nada coincide aparece *No hay productos que coincidan con el filtro actual*.

## Elegir un producto (computadora)
- Esta pantalla **no usa el diseño compacto** del resto del sistema, a propósito: está pensada para elegir rápido. Las **filas son altas** y la letra es cómoda.
- El cuadro de búsqueda y la tabla ocupan **la mitad del ancho** de la pantalla, pegados a la **izquierda** (con un mínimo de unos 416 px).
- La tabla tiene dos columnas: **Codigo** y **Elaborado** (con la receta debajo, si tiene). Se elige con **doble clic** sobre cualquier parte de la fila, o con **Enter** si la fila tiene el foco (se llega con Tab). Ya no hay botón **Seleccionar** a la vista.
- Se quitaron las columnas *Ingreso rápido* y *Fórmula*: la lista solo trae productos con ingreso rápido activado y con fórmula cargada, así que siempre decían *Sí* y *Cargada*.

## En el celular
Cada producto se muestra como una **tarjeta** a todo el ancho: arriba el nombre con el código en gris a la derecha (y la receta debajo, si tiene), y el botón **Seleccionar** a todo el ancho. En el celular no hay doble clic, por eso conserva el botón.

## Resto de la pantalla
PENDIENTE: documentar permisos y cuándo un producto aparece en la lista (no verificado contra el código en esta revisión; lo que sí se sabe: aparecen solo los productos de la empresa con ingreso rápido activado y fórmula cargada).
