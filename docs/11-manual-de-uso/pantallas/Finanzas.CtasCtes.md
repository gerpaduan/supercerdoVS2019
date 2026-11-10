---
pantalla: Finanzas.CtasCtes
titulo: Cuentas corrientes
rol: usuario
modulo: Finanzas
orden: 1
permiso: Finanza.VerCtasCtes (Cuentas corrientes)
revisada: 2026-11-10
---
## Para qué sirve
Ver las **cuentas corrientes** de todas las personas con su **saldo**, buscar una y abrirla. Tocar (o hacer clic en) una persona abre su [cuenta corriente](Finanzas.CtaCtePersona.md).

## Quién puede entrar
Los administradores y quienes tienen el permiso **Cuentas corrientes** (se da en [Permisos del usuario](ayuda:pantalla/Usuarios.Asistente), módulo Finanzas; su *Días ver* limita hasta cuántos días atrás ve los movimientos de cada persona). Quien no lo tiene ve «Acceso denegado».

Las personas con **cuenta corriente reservada** figuran en la lista, pero quien no tiene el permiso **Ver cuentas corrientes reservadas** ve **«Reservada»** en lugar del saldo.

## Buscar y ordenar
- Al abrir la pantalla el cursor queda en el cuadro de búsqueda. Con las flechas ↑ ↓ elegís una persona y con **Enter** abrís su cuenta.
- El cuadro de búsqueda filtra mientras escribís, por **nombre** (identificación), **razón social** o **CUIT**. El CUIT se encuentra con o sin guiones (`20-12345678-9`, `20123456789` o un pedazo, como `2012345`). El número interno de persona ya no se muestra ni se busca.
- El saldo se puede ordenar de mayor a menor o al revés. Si tu usuario no puede ver saldos, se muestra `***` y no se puede ordenar. Abierta desde el POS (F2), el orden se cambia dentro de la misma ventana, sin salir del POS.

## En el celular
Las personas se muestran como una lista: arriba el nombre, debajo la razón social y el CUIT (si está cargado) en gris, y a la derecha el saldo, en **verde** si es positivo y en **rojo** si es negativo. Arriba de la lista se ven la cantidad de cuentas (cambia con la búsqueda) y el link para ordenar por saldo. En la computadora la tabla muestra las columnas Persona, CUIT y Saldo.

## Desde el POS
La misma lista se ve en el celular al abrir las cuentas corrientes desde el POS (F2), en modo claro y oscuro.

## Resto de la pantalla
PENDIENTE: documentar permisos, uso desde el POS y atajos de teclado (no verificado contra el código en esta revisión).
