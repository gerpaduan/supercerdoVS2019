---
pantalla: Elaborados.EditarFormula
titulo: Editar fórmula
rol: usuario
modulo: Elaborados
orden: 5
alias: Elaborados.GuardarFormula
permiso: Elaborado.IngresoFormula (editar)
revisada: 2026-10-05
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

## Resto de la pantalla
PENDIENTE: documentar ingredientes, porcentaje/unidad, ajuste de fórmula, ingreso rápido, pasos de receta y costeo (no verificado contra el código en esta revisión).
