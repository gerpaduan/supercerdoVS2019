---
pantalla: Ventas.POS
titulo: Punto de venta (POS) — ticket
rol: usuario
modulo: Ventas
orden: 0
permiso: PENDIENTE (no se verificó en esta revisión qué permiso exige la pantalla)
revisada: 2026-10-19
---
## Para qué sirve
Registrar ventas en el mostrador. **Esta ayuda cubre solo la opción de no imprimir el nombre de la empresa en el ticket no fiscal (2026-10-19).** El resto del POS está pendiente de documentar.

## Nombre de la empresa en el ticket
El ticket **no fiscal** (el que dice **X** / «No valido como Factura») imprime arriba el nombre de la empresa y sus slogans (los de [Mi Empresa](ayuda:pantalla/Empresa.Index)). Para que **no** salgan, un administrador tilda **ocultarEncabezadoTicketNoFiscal** en la pantalla **Parámetros** (`/Parametros`) y guarda. Destildado (o sin cargar) el ticket sale como siempre.
- Vale para todas las impresiones del ticket de venta: post-venta, reimpresión desde el historial y modal del comprobante, en 58 y 80 mm.
- El ticket **facturado** (con CAE) no cambia: siempre imprime el nombre, los slogans y los datos fiscales.
- No cambia los PDF ni los demás tickets (recibos de pago o cobro, apertura y cierre de caja).

## Notas
- PENDIENTE: el resto de la pantalla no se documentó en esta revisión.
