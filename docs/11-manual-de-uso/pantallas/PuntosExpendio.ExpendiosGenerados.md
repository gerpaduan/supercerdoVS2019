---
pantalla: PuntosExpendio.ExpendiosGenerados
titulo: Expendios generados
rol: usuario
modulo: Punto de Expendio
orden: 2
permiso: Venta.NuevaVenta (el mismo que "Nueva venta"); sin él la pantalla y los archivos PDF/Excel redirigen al inicio con un aviso
revisada: 2026-10-30
---
## Para qué sirve
Consultar **todos los expendios** de la empresa de un período (remitos, presupuestos, carnicería…), con sus líneas, de todas las sucursales y usuarios. Se puede ver **una tarjeta por expendio** o **una sola tabla con todas las líneas**, filtrar por cliente (eligiéndolo de una lista), y **exportar a PDF o Excel** lo que se ve.

## Filtros
- **Fecha desde / hasta**: al cambiarlas aparece un aviso; tocá **Buscar** para volver a consultar. Es lo único (junto con el cliente elegido) que se consulta al servidor.
- **Estado** (Pendiente / Asignado), **Sucursal**, **Sector**, **Usuario**, **Producto**: se aplican en el momento sobre lo ya cargado.
- **Cliente / Nro remito**: se puede **escribir** (busca por letras en la identificación del expendio o en el nro de remito, en el momento) o **elegir de una lista** con la lupa (o **F10**), de a uno o varios: así filtra **exactamente** por esas personas. Ver [cómo funciona el selector](ayuda:concepto/selector-cliente-vendedor-proveedor). La **✕** del campo quita la selección y vuelve a mostrar todos. Elegir o quitar una persona **vuelve a consultar** al servidor.
- **Producto**: busca por **código o descripción** y deja en cada expendio **solo las líneas de ese producto**; los kilos y el total de la tarjeta y de la barra de totales suman solo esas líneas, y un expendio sin ninguna línea del producto desaparece.

### Qué trae "una persona elegida"
Los expendios cuyo cliente se guardó con su id **o** cuya *Identificación cliente* es **igual** a la identificación de la persona (sin distinguir mayúsculas ni acentos). Es la misma regla del filtro *Persona* del modal de expendios del POS.

## Totales
La barra de arriba suma lo que se ve: **Expendios**, **Líneas**, **Kgs.** y **Total**. Se muestran con la misma letra que los encabezados de la tabla (desde 2026-10-30; antes iban en un tamaño de título).

## Ver solo líneas
El botón **Ver solo líneas** cambia de una tarjeta por expendio a **una tabla con una fila por línea**; *Ver agrupado por expendio* vuelve a las tarjetas.
- Columnas: Fecha, Expendio, Estado, Cliente, Nro remito, Sector, Usuario, Sucursal, Código, Producto, Kgs., Precio, Total y un botón para abrir el **PDF** del expendio.
- **Ordenar**: tocá el título de una columna; otro toque invierte el orden (▲▼ marca la activa). Arranca por **Fecha**, de la más reciente a la más vieja.
- Los filtros y los totales funcionan igual en las dos vistas. El navegador **recuerda la vista elegida**.
- Los botones **Email** de cada expendio solo están en la vista de tarjetas.

## Exportar a PDF o Excel
1. Dejá la pantalla como la querés (fechas ya aplicadas con **Buscar** y filtros).
2. Tocá **PDF** o **Excel**, arriba a la derecha. Se descarga `Expendios_generados_AAAAMMDD_HHMM.pdf` o `.xlsx`.
- Es el mismo formato que [Líneas de venta](ayuda:pantalla/Ventas.Lineas): un corte por expendio (banda con fecha, número, estado y total; una línea gris con **cliente, estado, sector, usuario, sucursal y nro de remito**; y sus líneas), el período, los **filtros activos** y los totales generales.
- El archivo respeta **lo que ves**: fechas y persona ya consultadas y todos los filtros. Con *Producto*, el total de cada expendio es el de las líneas listadas.
- Si no hay ningún expendio visible, avisa *No hay expendios para exportar* y no descarga nada.
- Excel: cantidades e importes son números. No hay *Excel compacto* en esta pantalla.

## Límites
- Sin persona elegida se cargan como máximo los **300 expendios más recientes** del rango; con persona, hasta **5000**. Si se llega al máximo, un aviso lo dice: acotá las fechas. Los exportes tienen el mismo tope.
- En **SQL Server** el expendio no guarda el id de la persona, así que elegir una persona filtra solo por identificación.

## Notas
- PENDIENTE: `ExpendiosGeneradosData` (el listado que usa la pantalla) no valida el permiso `Venta.NuevaVenta`; la pantalla sí. Existía así antes de esta revisión.
