---
pantalla: Personas.Index
titulo: Personas
rol: usuario
modulo: Personas
orden: 1
permiso: sin permiso propio para entrar; el icono de cuenta corriente exige el permiso de ver cuenta corriente de una persona
revisada: 2026-11-02
---
## Para qué sirve
Buscar **clientes, proveedores y demás personas** de la empresa, abrir su ficha y llegar a su cuenta corriente. Se llega desde **Personas** en el menú.

## Buscar y filtrar
- Todo está en **una sola fila**: el cuadro de búsqueda, la **condición de IVA**, **Limpiar** y **Nueva persona**.
- El cuadro filtra **mientras escribís**, por razón social, CUIT o identificación. No hace falta apretar un botón; con **Enter** también se busca.
- **Condición IVA** muestra solo las personas de esa condición (Consumidor Final, Responsable Inscripto, Monotributista, Exento). Se combina con el texto escrito y también filtra al cambiarla. **Todas las condiciones IVA** quita el filtro.
- **Limpiar** vuelve a la lista completa, sin texto ni condición.

## Columnas y acciones
- Columnas: ID, Identificación, Razón social, IVA, CUIT, Teléfono, Domicilio, Ciudad, Otros datos y Acciones. Una persona con **cuenta corriente reservada** lleva la etiqueta **Reservada**.
- **Ver persona** abre la ficha ([Ver / modificar persona](Personas.Editar.md)). Las personas en gris son **globales** (compartidas por todas las empresas): desde una empresa no se pueden abrir y el botón queda deshabilitado.
- El **icono de cuenta corriente** (al lado de Ver persona) abre la [cuenta corriente](Finanzas.CtaCtePersona.md) de esa persona y, al volver, regresa a este listado con el mismo filtro. Lo ven los usuarios con permiso de ver cuenta corriente de una persona y las cuentas de producción (que se identifican al entrar). Sin ese permiso el icono no aparece.

## En el celular
Cada persona es una tarjeta: arriba la razón social y, debajo, solo los datos que tienen valor. **Ver persona** y el icono de cuenta corriente quedan al pie de la tarjeta. El filtro de IVA ocupa todo el ancho.
