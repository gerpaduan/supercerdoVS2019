# Mejoras futuras

## Objetivo

Guardar ideas o necesidades detectadas que no forman parte del comportamiento actual confirmado.

## Secciones

- Mejora propuesta
- Problema que resuelve
- Impacto esperado
- Prioridad

## Facturación en lote (agendado 2026-11-26)

- **Mejora propuesta:** pantalla para facturar varias ventas juntas, a la que se llega desde el aviso que muestra el POS al subir las ventas hechas sin conexión y también desde Ventas / Facturas.
  - Filtros por fecha, forma de pago (**incluido Efectivo**), sucursal, «hecha sin conexión» y «pendiente de facturar»; elegir todas o algunas.
  - Las que AFIP rechace van a un apartado de errores con su motivo. Si el motivo es una fecha fuera de rango, proponer la fecha actual: revisar una por una o aplicar la misma fecha a todas.
  - Reusar `VentasController.GenerarFactura` / `AFIP.GenerarFacturaService`, de a una y en secuencia (la numeración `FECompUltimoAutorizado`+1 no tiene bloqueo, ver `docs/06-datos-e-integraciones/afip-y-facturacion.md`).
  - PENDIENTE: averiguar cuántos días hacia atrás admite AFIP la fecha de emisión para productos (no inventar el límite).
- **Problema que resuelve:** sin conexión no se puede facturar; las ventas con tarjeta, QR o transferencia quedan «pendiente de facturar» y hoy se facturan de a una desde Mis ventas.
- **Impacto esperado:** menos trabajo manual después de un corte de internet.
- **Prioridad:** media. El usuario pidió dejarlo para después (2026-11-26).

## Mis ventas: marcar y filtrar las ventas hechas sin conexión (2026-11-26)

- **Mejora propuesta:** en Mis ventas y en el listado de Ventas, una marca para las ventas subidas desde el POS sin conexión («llegó tarde», «para facturar», «precio distinto») y un filtro por origen. Los datos ya están en `posclaveunica`.
- **Problema que resuelve:** hoy esas marcas solo se ven en el aviso del cierre de caja.
- **Impacto esperado:** revisar y facturar después de un corte sin buscar a mano.
- **Prioridad:** media. Quedó fuera de la primera entrega del POS sin conexión para no tocar las consultas de los listados (ver `docs/DECISIONS.md` 2026-11-26 e).
