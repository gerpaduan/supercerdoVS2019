---
pantalla: Personas.Editar
alias: Personas.Nuevo, Personas.Guardar, Personas.Eliminar
titulo: Ver / modificar persona
rol: usuario
modulo: Personas
orden: 2
permiso: sin permiso propio para ver o modificar; eliminar es solo del administrador
revisada: 2026-11-02
---
## Para qué sirve
Ver los datos de una persona, **modificarlos**, cargar una **nueva** o **eliminarla**. Se llega con **Ver persona** o **Nueva persona** desde [Personas](Personas.Index.md).

## Ver y modificar
- Una persona existente **abre en solo lectura**. Para cambiar algo tocá **Modificar**; aparece **Guardar persona**. **Cancelar** vuelve al listado.
- La ficha tiene tres filas: **ID, CUIT, Identificación y Razón social**; **Condición frente al IVA, Teléfono, Email y Ciudad**; **Domicilio, Cuenta corriente reservada y Otros datos**.
- La lupa junto al CUIT **busca en AFIP/ARCA** (11 dígitos) y completa identificación, razón social, domicilio, ciudad e IVA.
- Razón social, CUIT, Identificación e IVA quedan **bloqueados** si la persona ya tiene compras o ventas; solo un administrador puede cambiarlos. El aviso de arriba lo explica.
- **Cuenta corriente reservada** solo aparece para el administrador y para quien tiene el permiso **Ver cuentas corrientes reservadas**.
- Una persona global (gris en el listado) no se puede abrir desde una empresa.

## Eliminar persona
- El botón **Eliminar persona** está **abajo a la derecha** y **solo lo ve un administrador**, en una persona ya guardada. No aparece en una persona nueva.
- Se puede eliminar **solo si**: es una persona de **la empresa en la que estás trabajando**, **no tiene ningún registro a su nombre**, no es el Consumidor Final y no es una marca (las marcas se eliminan desde Productos).
- «Ningún registro» cuenta todo lo que esté a su nombre: ventas, compras, cortes por proveedor, pagos, movimientos de cuenta corriente, expendios, borradores de venta, empleados, titulares fiscales, la auditoría de registros eliminados y de cambios de persona en pagos, y las marcas de las que es propietaria. **Los registros anulados o eliminados también cuentan.** Por eso una persona a la que se le cargó algo por error y se lo borró puede seguir sin poder eliminarse.
- Si no se puede, el botón queda **deshabilitado** y al lado se lee el motivo, por ejemplo: «No se puede eliminar: tiene registros a su nombre: ventas (89), pagos (4047)».
- Al tocarlo pide confirmación («Esta acción no se puede deshacer»). Al confirmar vuelve al listado con el aviso **La persona se eliminó correctamente.**
- Si entre que abriste la ficha y confirmaste le cargaron algo, o se cambiaron las condiciones, se vuelve a la ficha con el motivo y no se elimina.
