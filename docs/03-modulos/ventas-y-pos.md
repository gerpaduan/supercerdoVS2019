# Ventas y POS

## Objetivo

Documentar el flujo de ventas, caja, articulos, pesaje y uso del POS.

## Secciones

- Pantallas involucradas
- Flujo de venta
- Validaciones
- Impresion y comprobantes
- Dependencias con balanza o perifericos
- Riesgos

## Ventas en curso (borrador en servidor) y advertencias del POS

Ver decisión completa, verificación y lista de archivos en `docs/DECISIONS.md` ("Ventas en curso:
borrador en servidor y advertencias del POS", 2026-09-21). Resumen operativo:

- **Motores.** Postgres: completa y prendida por defecto. **SQL Server (`SuperCerdo`: Servidor SM,
  San Lorenzo): desde 2026-09-23 hay una versión reducida, opt-in** (`PosBorrador:Habilitado=true` +
  `Datos/DB-Procedures/20260923-Create_Borradores.sql`; ver `docs/DECISIONS.md` "Borradores en SQL
  Server"): ventas en curso, recuperación, campana propia del cajero y "Ventas sin guardar" **sí**;
  campana/notificaciones del admin y advertencia "producto pesado sin agregar" **no** (etapa 2, apagadas
  con `PosBorradorSettings.SoportaNotificaciones`). Apagada (default en SQL Server), el POS sigue con
  el `POSDraft` local de siempre.
- **Qué guarda**: mientras el cajero arma la venta, el carrito se resguarda en la tabla
  `ventaborrador` (identificado por un `clientid` GUID del navegador), con autoguardado en cada
  cambio y un latido periódico. **Nunca toca `ventas`/`lineaventa`** — esas tablas se escriben recién
  al "Finalizar venta", como siempre.
- **Recuperación**: panel de Ayuda (F1) → "Ver ventas sin cerrar" lista las de toda la sucursal con
  usuario e ítems; el cajero carga/descarta solo las propias, una ajena pide autorización de un
  supervisor (usuario+clave). Una venta figura "En uso" (no cargable) mientras su último latido sea
  más nuevo que `PosBorrador:MinutosSinLatidoInterrumpida` (default 5 min); cuando la pestaña avisa
  su cierre (`pagehide` → `EventoBorradorPOS`) el latido se retrocede 1 día
  (`IVentaBorradorRepository.EnvejecerLatidoPorCierre`) y la venta queda "Interrumpida" y cargable
  **al instante**. Solo un corte sin aviso (luz, crash del navegador) espera el umbral. (Corregido
  2026-09-24: antes el POS no envejecía el latido y siempre había que esperar los 5 min; los
  formularios genéricos ya lo hacían.)
- **Al cerrar la caja** con una venta en curso del dueño de esa caja, se avisa y hace falta confirmar;
  si se confirma, queda registrado (evento + notificación al admin) y la venta sigue recuperable.
- **Advertencia "producto pesado sin agregar"**: si un producto queda en pantalla con peso/cantidad
  estable más de `PosBorrador:SegundosProductoSinAgregar` segundos (config, default 7) y sale sin
  agregarse al carrito, se registra en `ventaproductosinagregar` como advertencia silenciosa (el
  cajero no se entera). Se agrupa por operador y día en una sola notificación.
- **Admin**: campana en el topbar (solo admin, polling ~60 s) + `/Actividades` muestran las ventas
  interrumpidas/descartadas y las advertencias, con detalle y revisión (Justificada/Sospechosa).
- **Configuración**: sección `PosBorrador:*` en `WebCore.dll.config`/`App.config` (umbrales de
  latido, minutos sin señal para considerar "interrumpida", segundos de producto en pantalla, etc.).
  Ninguno está hardcodeado — ver `WebCore/Helpers/PosBorradorSettings.cs` para los defaults.

**Deuda conocida**: `FinalizarVenta` sigue confiando en el precio que manda el cliente (una venta
recuperada conserva los precios de cuando se armó); la detección de "producto sin agregar" es del
lado del navegador y puede tener falsos positivos (cliente arrepentido, código equivocado); no hay
proceso en segundo plano, así que sin un admin activo no hay alerta push (el rastro en la base queda
igual). Detalle completo en `docs/DECISIONS.md`.

## Punto de Expendio: sectores PRESUPUESTO y REMITOS (2026-09-26)

Controlador `WebCore/Controllers/PuntosExpendioController.cs`; reglas puras en `Negocio/SectorPuntoExpendio.cs` (tests: `Negocio.Tests/SectorPuntoExpendioTests.cs`).

- **Sectores globales y reservados**: `PRESUPUESTO` y `REMITOS` son filas con `idempresa = 0` en `sectores` (las ven todas las empresas). No se crean, renombran ni eliminan desde `Sectores` (lo rechaza el controlador y `VentaPg` filtra `idempresa <> 0` en UPDATE/DELETE). Se siembran con `DatosPostgres/DB-Migrations/20260926-Seed_sectores_globales.sql`.
- **Fecha del expendio**: solo en esos dos sectores el chip "Fecha y hora" del POS es clickeable (`punto-expendio-pos.js`, `fechaManual` evita que el reloj de 60 s la pise). REMITOS: nunca futura (tolerancia de reloj 5 min). PRESUPUESTO: futura hasta 365 días. Otros sectores: siempre "ahora". Se valida en `FinalizarPOS` (`SectorPuntoExpendio.ResolverFecha`).
- **PRESUPUESTO como documento**: `ImprimirPdf(id, formato)` / `EnviarComprobanteEmailExpendio(..., formato)`. `formato=precios` (lista de precios) = Código, Descripción, Precio (sin cantidades, totales, sucursal ni vendedor) con el texto "Nos dirigimos a ustedes desde {empresa}, con el fin de hacerles llegar los nuevos precios a partir del {fecha}.", vigencia y datos fiscales; sin `formato` (Ver / imprimir, completo) = agrega Cantidad, Importe y totales, con sucursal y vendedor, pero sin texto, vigencia, CUIT, IIBB, razón social ni cond. IVA. Diseño igual al comprobante de venta (rojo de marca, encabezado rosado). El modal post-expendio ofrece ambos PDF y elige el adjunto del email. Los listados ("Mis expendios", "Expendios generados") imprimen el formato completo.
- **REMITOS (Entrega 2)**: el POS pide `Nro de remito` (obligatorio, sugerido `PPPP-NNNNNNNN` = id de sucursal + correlativo por sucursal, editable, unico por sucursal; columna `expendios.nroremito`, migración `20260926b-Alter_expendios_add_nroremito.sql`). Se busca por ese número en "Mis expendios", "Expendios generados" y el buscador de expendios de Ventas. Al guardar/modificar una venta con expendios REMITOS, `Negocio.Venta.AgregarLeyendaRemitosAsociados` agrega a las observaciones `REMITOS ASOCIADOS: a / b` (regla en `Negocio/NroRemito.cs`, tests `NroRemitoTests`/`VentaRemitosAsociadosTests`). Solo Postgres.
- **PDF de todos los sectores (2026-09-26)**: `PuntosExpendioController.GenerarPdfExpendio` usa el diseño del comprobante de venta para cualquier sector (rojo de marca, letra central, encabezado de tabla rosado, pie con paginación). Presupuesto: letra "P", "PRESUPUESTO"; el resto: letra "X", "EXPENDIO", con Sector y (REMITOS) Nro remito. Ya no existe el PDF simple anterior.
