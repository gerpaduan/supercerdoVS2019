# Productos y personas - reglas observadas

## Objetivo

Concentrar en un solo lugar las reglas observadas en `Web` MVC, las decisiones ya confirmadas y las restricciones que deben respetarse al implementar `Productos` y `Personas` en `CarniSys NG`.

## Alcance del relevamiento

Este documento surge de:

- lectura de `Web` MVC;
- lectura de controladores, vistas y modelos;
- consultas de solo lectura sobre la base actual;
- validaciones funcionales ya confirmadas con el arquitecto funcional.

## Personas

### Regla base

`Personas` debe mantenerse como una entidad generica, tal como funciona hoy.

No debe separarse artificialmente en tablas o modulos distintos si el comportamiento actual no lo hace.

### Tipos observados

En base actual se observaron al menos:

- `Cliente`
- `Proveedor`
- `Empleado`
- `Otro`
- `tipo` vacio

### Decision confirmada

- mantener `tipo` vacio como esta actualmente;
- no eliminarlo aunque este en desuso;
- evitar normalizaciones prematuras que puedan romper comportamiento historico.

### Marcas

Se mantiene el tratamiento actual.

Si hoy existe relacion funcional con `Personas`, en NG debe respetarse hasta tener evidencia suficiente para separar mejor el modelo.

### Reglas observadas en MVC

- una persona global (`idEmpresa = 0`) no puede modificarse desde una empresa;
- al editar, si la persona ya tiene compras o ventas:
- un usuario no admin no puede cambiar `Razon Social`, `CUIT` ni `Identificacion`;
- un admin si puede hacerlo;
- el CUIT no puede duplicarse con otra persona;
- la gestion de `Cuenta Corriente` depende de permisos;
- IVA se selecciona desde lista cargada desde negocio;
- AFIP puede completar datos de la persona desde el padron.

### Campos observados en uso MVC

- `Identificacion`
- `RazonSocial`
- `IdIva`
- `Cuit`
- `Telefono`
- `Email`
- `Domicilio`
- `Ciudad`
- `OtrosDatos`
- `CtaCte`
- `Bonificacion`

### Decision confirmada para NG

- mantener el comportamiento actual;
- no reinterpretar `Personas` como solo clientes;
- no separar por ahora clientes, proveedores o marcas en modelos aislados por moda;
- conservar la restriccion de personas globales no editables desde empresa;
- conservar la proteccion de campos sensibles cuando hay movimientos.

## Productos

### Regla base

`Productos` debe implementarse tomando como referencia principal el proyecto `Web` MVC actual.

### Catalogo global

Reglas observadas en MVC:

- el catalogo global es una pantalla y flujo aparte;
- los productos globales no se usan directamente en operaciones;
- se importan o clonan a la empresa actual;
- la importacion requiere permiso de producto;
- el sistema controla si ya fue importado;
- el sistema controla si el codigo ya existe en la empresa;
- si el codigo esta ocupado, sugiere uno libre;
- si existe dependencia con `CorteMaestro`, obliga a importar primero el maestro.

### Decision confirmada para NG

- mantener comportamiento MVC actual;
- mantener sugerencia de codigo libre;
- mantener validacion de dependencia con `CorteMaestro`;
- mantener trazabilidad de importacion global;
- mantener tratamiento de marcas asociado al producto como hoy.

### Precio al importar

Decision confirmada:

- mantener el comportamiento MVC actual;
- el precio se ingresa y valida durante la importacion;
- no forzar `precio = 0` por defecto si eso contradice el flujo probado hoy.

### Alta rapida por codigo de barras

Reglas observadas en MVC:

- solo autocompleta desde global si el codigo es `EAN-8` o `EAN-13` valido;
- si el codigo ya existe en la empresa, bloquea;
- la descripcion es obligatoria;
- el precio debe ser mayor o igual a `0`;
- si el producto global no existe, informa error;
- si el producto depende de maestro no importado, bloquea.

### Decision confirmada para NG en alta por EAN

- si en alta de producto se ingresa un `EAN-8` o `EAN-13` valido y existe coincidencia en catalogo global, se deben recuperar esos datos;
- el sistema debe mostrar una etiqueta o aviso visible indicando que los datos fueron recuperados desde el catalogo global;
- esta recuperacion automatica aplica solo en alta;
- en edicion no debe ejecutarse esta logica.

### Productos visibles en operacion

La busqueda ligera usada por POS lista productos de la empresa actual, no productos globales.

### Punto stock por sucursal

Funcionalidad nueva confirmada para NG:

- aplica solo a productos de empresa;
- no aplica a productos globales;
- al crear o clonar producto se generan registros para todas las sucursales de la empresa;
- el valor inicial es `0`;
- si `punto stock = 0`, no se controla faltante;
- si falta el registro, se asume `0`;
- hay faltante cuando `stock actual <= punto stock`.

### Edicion de punto stock

Comportamiento confirmado:

- en ficha de producto se puede ingresar un valor general y copiarlo a todas las sucursales;
- debe existir opcion para editar por sucursal en una grilla;
- debe existir mas adelante una edicion masiva por lote;
- la visualizacion debe mostrar diferencia y alerta visual.

### Permisos para punto stock

- usa el mismo permiso de edicion de producto.

## Regla comun para ambos modulos

Antes de implementar cualquier comportamiento nuevo en `Productos` o `Personas`:

- revisar MVC;
- revisar base actual;
- revisar permisos;
- validar cualquier mejora funcional con el arquitecto funcional.

No se deben introducir cambios de comportamiento sin consulta previa cuando el flujo actual ya funciona correctamente.
