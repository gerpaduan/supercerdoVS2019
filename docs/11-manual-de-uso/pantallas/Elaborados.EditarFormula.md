---
pantalla: Elaborados.EditarFormula
titulo: Editar fórmula
rol: usuario
modulo: Elaborados
orden: 5
alias: Elaborados.GuardarFormula
permiso: Elaborado.IngresoFormula (editar)
revisada: 2026-10-31
---
## Para qué sirve
Crear o modificar la **fórmula** de un elaborado: sus ingredientes, la proporción de cada uno y la receta. Se llega desde *Elaborados → Fórmulas*.

## Fórmula secreta
La casilla **Fórmula secreta** (junto a *Ingreso rápido*) marca la fórmula como [secreta](ayuda:concepto/formula-secreta). Si está activada, los ingredientes y la receta no se muestran en Ingreso rápido, Carga, el detalle ni las líneas de elaborados hasta que alguien con permiso sobre fórmulas ingrese usuario y contraseña.

- Esta pantalla **no pide** usuario y contraseña: ya exige el permiso de ingresar fórmulas.
- Activar o desactivar la casilla queda registrado (quién y cuándo).
- Se guarda con el resto de la fórmula al tocar **Guardar**.

## Ingredientes con valor negativo
Un ingrediente puede cargarse con porcentaje (o unidades) **negativo**, por ejemplo una línea de ajuste que resta del total. Al guardar, la fórmula **no se rechaza** por tener valores negativos. Solo se exige que cada línea tenga un ingrediente válido.

## Código de ingrediente sin agregar
Si escribiste un código de ingrediente y no lo agregaste a la tabla (botón +), **no se puede guardar la fórmula**: aparece el aviso *"El campo Código tiene un valor sin agregar..."*. Agregá el ingrediente o borrá el código y volvé a guardar.

## Resto de la pantalla
PENDIENTE: documentar ingredientes, porcentaje/unidad, ajuste de fórmula, ingreso rápido, pasos de receta y costeo (no verificado contra el código en esta revisión).

## En la computadora
- Al pasar el mouse por un ingrediente de la tabla, la fila se pinta de azul tenue.

## Datos del registro
- En el celular, **Creado** y **Actualizado** de una fórmula ya guardada se muestran en un bloque compacto de dos columnas, con letra chica; en la computadora siguen los dos campos de siempre.

## En el celular
- La pantalla se reordena en bloques: el **usuario** es una línea en gris; **código y elaborado** (y **código e ingrediente**) van en la misma fila con la lupa al lado; **valor**, **Auto** y el botón **+** van en la fila siguiente.
- Las ayudas largas (*Fórmula secreta*, *Calcular por*) quedan detrás del ícono **(i)**: tocalo para verlas u ocultarlas. En la computadora se ven siempre.
- Los **ingredientes cargados** son una lista: ingrediente y valor arriba, código abajo, un rayo verde si es **Auto** (gris si se carga a mano) y el tacho para quitarlo. Con *No suma peso* o el costeo (Admin) activos, esos datos salen en un renglón debajo de cada ingrediente.
- El **total** queda pegado a la lista. La tarjeta *Totales* solo se ve en celular si sos Admin, para el costeo.
- **Guardar** (o **Modificar**) y **Cancelar** ocupan todo el ancho.
