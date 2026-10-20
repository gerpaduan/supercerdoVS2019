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

## Saldo del cliente en el POS y en el ticket — 2026-10-19

Manual: `docs/11-manual-de-uso/pantallas/Ventas.POS.md`. Decisión completa y verificación: `docs/DECISIONS.md` (2026-10-19).

- **Convención**: saldo = `SUM(movctacte.importe)` de la persona; **negativo = el cliente debe**, positivo = a favor. Lo lee `ICuentaCorrienteRepository.obtenerSaldoPersona(idPersona)` (`decimal?`, `null` = sin movimientos; `CuentaCorrientePg`, `Datos/CuentaCorriente` para SQL Server, `Negocio.CuentaCorriente`).
- **Regla de negocio**: la venta actual se **suma al saldo solo si es a cuenta corriente** (ahí la deuda crece); si se pagó de otra forma, solo se muestra el saldo anterior (carrito y ticket).
- **Reglas puras** (testeadas): `Negocio/CalculoSaldoCliente.cs` — saldo previo a la venta (si la venta fue a cta cte, su Débito ya está en el saldo y se le suma el importe para sacarla), total con saldo (`venta − saldoPrevio`) y umbral.
- **Umbral**: parámetro por empresa `saldoMinimoInformar` (`ParamKeys.SaldoMinimoInformar`, tabla `parametros`/`empresaparametros`; alta del catálogo en `DatosPostgres/DB-Migrations/20261019c-…sql`); sin valor rige `SaldoMinimoInformarPorDefecto` = 100. Un saldo cuyo valor absoluto no lo supera no se muestra ni se ofrece.
- **POS**: `VentasController.SaldoCliente` (GET, JSON) alimenta el renglón `#clienteSaldoWrap` (rojo `(debe)` / verde `(a favor)`); lo dispara `actualizarSaldoCliente` desde `actualizarAccesoHistorialPreciosCliente`, el punto por el que pasa todo cambio de cliente. Estilos en `wwwroot/Content/css/pos.css` (`.pos-cliente-saldo*`, con modo oscuro).
- **Total c/saldo en el carrito**: `#posTotalConSaldoWrap` (debajo de `#lblSubtotal`, letra chica). `window.actualizarTotalConSaldo(total)` (definida en `POS.cshtml`) la llaman `pos-cart.js` → `recalculateTotal` en cada cambio del carrito, `pos-state.js` al cambiar la forma de pago preseleccionada (`set/clearFormaPagoPreseleccionada`, `clear`) y `actualizarSaldoCliente` cuando llega el saldo (guard `typeof`: el POS de Expendio comparte los scripts y no la define). Si la forma de pago preseleccionada es `CtaCte` muestra «Total c/saldo» (total − `saldoParaTotal` con signo); con cualquier otra, o ninguna todavía, muestra «Saldo anterior (debe|a favor)» sin sumar. Con carrito vacío o sin saldo informable se oculta. `SaldoCliente` devuelve `mostrarTotal`/`saldoParaTotal`; al modificar una venta manda `idVentaEditar` y, si es del mismo cliente, el servidor usa el saldo ANTERIOR a esa venta (`ObtenerSaldoPrevioParaTicket`). Respeta reservadas/umbral porque todo sale del mismo `ObtenerSaldoInformable`.
- **Ticket**: al tocar *Ticket* del post-venta, `VentasController.SaldoParaTicket` dice si hay saldo; si lo hay el POS pregunta con SweetAlert (el texto cambia según `enCtaCte`) y agrega `incluirSaldo=true` a `ImprimirTicketHtml` / `ImprimirTicketPayload`. **El servidor recalcula todo** (el JS solo pide el bloque). El bloque lo arma `WebCore/Services/TicketSaldoLineas.cs` (`Construir(..., sumarVenta: venta.EnCtaCte)`), fuente única de las dos copias del ticket (`ConstruirLineasTicketVenta` y `Views/Ventas/_TicketHTML.cshtml`): venta a cta cte → «Saldo anterior» + «TOTAL c/saldo»; otra forma de pago → solo «Saldo anterior» (en 58 mm, «Saldo ant.»).
- **Reservadas**: `ICtaCteReservadaService.SaldoOculto` — nada se muestra, pregunta ni imprime a un usuario restringido con un cliente reservado.
- **Límites conocidos**: la reimpresión desde el historial, el PDF y el email no ofrecen saldo; el renglón no se refresca si se cobra con F2 sin cambiar de cliente; el armado de líneas del ticket (WebCore) no tiene test automático (`Negocio.Tests` no referencia WebCore).

## Ocultar nombre y slogans en el ticket de venta no fiscal — 2026-10-19

Manual: `docs/11-manual-de-uso/pantallas/Ventas.POS.md` (sección «Nombre de la empresa en el ticket»). Decisión: `docs/DECISIONS.md` (2026-10-19).

- **Parámetro** por empresa `ocultarEncabezadoTicketNoFiscal` (`ParamKeys.OcultarEncabezadoTicketNoFiscal`, tipo 2 = Sí/No, pantalla Parámetros). Sin valor = No. Alta del catálogo: `DatosPostgres/DB-Migrations/20261019d-…sql` y, para SQL Server, `Datos/DB-Procedures/20261019-Insert_Parametro_OcultarEncabezadoTicketNoFiscal.sql` (INSERT dinámico según el esquema; no corrido todavía).
- **Lectura**: `VentasController.OcultarEncabezadoTicketNoFiscal()`. Lo aplican las dos copias del ticket de venta con el mismo criterio `mostrarDatosEmpresa = esFacturada || !ocultar`: `ConstruirLineasTicketVenta` (ESC/POS, `ImprimirTicketPayload`) y `Views/Ventas/_TicketHTML.cshtml` (vía `ViewBag.OcultarEncabezadoNoFiscal`, `ImprimirTicketHtml`). Se omiten el nombre (`ObtenerNombreEmpresaVenta`) y `Slogan1..3`.
- **No afecta**: ticket facturado (con CAE), PDF de la venta, recibos de pago/cobro y tickets de caja (`ComprobanteTickets`), ticket X del WinForms.

## Líneas de venta: exportar a PDF y Excel — 2026-10-13

Pantalla `Ventas/Lineas` (`WebCore/Views/Ventas/Lineas.cshtml`; manual: `docs/11-manual-de-uso/pantallas/Ventas.Lineas.md`). Botones **PDF** y **Excel** en la cabecera → `VentasController.LineasPdf` / `LineasExcel` (GET, misma firma que `Lineas`, mismo gate de usuario de producción).

- **Modelo compartido**: `VentasController.ConstruirModeloLineas` arma las tarjetas por venta (rango sin paginar, ventas reservadas ocultas por `_ctaCteReservada`, filtros de cliente / vendedor / forma de pago / producto). `Lineas` y los dos exports lo usan, así el archivo no puede divergir de la pantalla.
- **Filtros**: producto, cliente, vendedor y formas de pago son filtros "en vivo" del navegador (sin `name`, los aplica `lineas-agrupadas.js`). El script inline de la vista arma el `href` de cada botón con `idSucursal`, `fechaDesde`/`fechaHasta` **aplicadas** (`fechasAplicadas`) y esos filtros (`formasPago` en CSV); sin ventas visibles el click se cancela con un aviso.
- **PDF**: `GenerarDocsCore.GenerarPdfLineasVenta` (QuestPDF, A4). Una tabla por venta; la banda (fecha, Venta ID, total), la línea de datos (cliente, vendedor, forma de pago, sucursal, nro/comprobante) y los títulos de columna van en el `Header` de la tabla, así la banda no queda sola al pie de una hoja y se repite al continuar la venta.
- **Excel**: `GenerarExcelCore.GenerarExcelLineasVenta` (ClosedXML 0.105.1, .xlsx real): misma estructura, cantidades e importes numéricos con formato, anchos fijos.
- **Excel compacto para imprimir**: botón "Excel compacto" → ventana con casillas (vendedor, sucursal, cliente, forma de pago, Venta ID, comprobante; se recuerdan en `localStorage`) → `VentasController.LineasExcelCompacto` (mismos filtros + `datos` CSV) → `WebCore/Services/ExcelLineasVentaCompacto.cs`. Sin colores, letra 8, A4 vertical, ventas de la más vieja a la más nueva, dos bloques lado a lado (A:E y G:K) con 64 filas de datos por hoja, saltos de página manuales y fila 1 de títulos repetida. Por venta: renglón con fecha y hora (+ vendedor/sucursal) y el total de la venta, renglón extra opcional y las líneas. Detalle y riesgos: `docs/DECISIONS.md` (2026-10-13, Excel compacto).
- **Textos y totales compartidos** (período, filtros, datos de la venta, totales = suma de las líneas listadas): `WebCore/Services/LineasVentaExportHelper.cs`. El total de cada venta es el de **toda** la venta (igual que la pantalla).
- **Límites conocidos**: sin tope de filas (la pantalla tampoco pagina); sin permiso propio (igual que la vista). Licencia de las dependencias, alternativas descartadas y riesgos: `docs/DECISIONS.md` (2026-10-13). Tests E2E: `WebCore.E2ETests/ExportarLineasVentaTests.cs`.

## Modificar una venta de una caja ya cerrada y validación de caja al finalizar — 2026-10-06

- **Finalizar venta** (`VentasController.FinalizarVenta`): la caja se valida siempre con la fecha de **creación** (`fechaCreacion`, ahora), nunca con la fecha de la venta (editable). El salteo de caja para Admin / permiso `Venta.UltimaVenta` se mantiene (decisión 2026-09-07).
- **Modificar una venta** (`ModificarVenta`): si la venta pertenece a una caja **ya cerrada** (vendedor + sucursal + fecha original), el POS en modo edición muestra el banner `#alertaVentaDeCajaCerrada`; si el cambio altera **monto, forma de pago, fecha o sucursal**, el servidor responde `requiereConfirmacion` y `forma-pago.js` pide confirmar (reenvía con `confirmarCajaCerrada`). Siempre que sea caja cerrada queda un aviso en el cierre (`auditoriacierrecaja`, origen `VENTA`). Detalle: `docs/03-modulos/caja-y-tesoreria.md` y `docs/DECISIONS.md` (2026-10-06).

## Bloqueo por inactividad del POS — 2026-10-15

- **Qué hace.** Tras los minutos de inactividad del **vendedor activo** (`usuarios.minutosinactividad`; 0 = nunca; producción y dispositivos "No bloquear sesión" exentos) tapa el POS con un overlay que pide su contraseña, **sin recargar** (el carrito queda intacto). Concepto para el usuario: `docs/11-manual-de-uso/conceptos/bloqueo-por-inactividad.md`.
- **Código.** Partial `Views/Shared/_BloqueoPOS.cshtml` (incluido por `Views/Ventas/POS.cshtml` y `Views/PuntosExpendio/POS.cshtml`) + `wwwroot/Scripts/app/pos-bloqueo.js`. Los minutos los calcula `WebCore/Helpers/BloqueoPosHelper.cs` en `POS()` de ambos controllers (`ViewBag.MinutosBloqueoPOS`).
- **Desbloqueo.** `VentasController.DesbloquearPOS` (también lo usa Punto de expendio, misma Session `OperadorPOS_<posInstanceId>`): el vendedor se resuelve en el servidor; la clave de otro usuario no desbloquea; comparte el rate-limit del step-up de operador (3 intentos / 5 min). En Ventas informa si la caja del vendedor sigue abierta; si no, el cliente recarga para que rija el gate de caja.
- **Antes de bloquear** se fuerza `POSDraft.save()` y `POSBorrador.guardarAhora()` (nuevo en `pos-borrador.js`). "Cambiar de vendedor" (solo cuenta de producción) exige que el servidor ya tenga la venta; después limpia el carrito local (`POSDraft.clear()`) para que el nuevo operador no la herede. La venta la retoma solo su dueño o un supervisor (`RecuperarBorradorPOS`).
- **`FinalizarVenta`** rechaza (`requiereOperador`) si la cuenta es de producción y el operador de Session venció, en vez de registrar a nombre de la cuenta compartida.
- **Límites.** Bloqueo de cliente (se puede saltar con las herramientas de desarrollo); Punto de expendio no tiene borrador en servidor, así que no permite cambiar de vendedor con venta en curso. Decisión completa: `docs/DECISIONS.md` (2026-10-15).

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
  su cierre (`pagehide`) se registra el evento `CIERRE_PESTANA` (`ventaborradorevento`) y, al ser
  posterior al último latido, la venta queda "Interrumpida" y cargable **al instante**
  (`Entidades.VentaBorrador.CerradaPestana`; el repositorio lee la señal efectiva = hora del cierre,
  sin tocar `ultimolatido`). Solo un corte sin aviso (luz, crash del navegador) espera el umbral.
  Si al cerrar queda algo sin mandar (último cambio dentro del debounce de 1,5 s, guardado en vuelo o
  primer guardado que nunca llegó), el `pagehide` manda el carrito completo con `cierrePestana=true`
  a `GuardarBorradorPOS` (una sola llamada keepalive; el evento se registra después del upsert). Si el
  carrito supera ~56 KB se cae al aviso solo (`EventoBorradorPOS`). El listado ("Ver ventas sin
  guardar" y campana) va del más reciente al más antiguo por hora de inicio.
  La "Última señal" que se muestra es la fecha y hora reales (latido o cierre), no "hace N d".
  (Corregido 2026-10-05: antes se retrocedía `ultimolatido` 1 día con `EnvejecerLatidoPorCierre`,
  método eliminado, y la señal salía siempre ≥ "1 d"; ver `docs/DECISIONS.md` 2026-10-05.) Desde 2026-10-04, una venta **propia** que figura "En uso"
  también se puede cargar sin esperar: el listado marca `puedeForzar`, el botón pide confirmación y
  el servidor (`Recuperar(..., tomarSiEstaEnUso)`) rota el `clientid` para que un carrito viejo vivo no
  la pise (ver `docs/DECISIONS.md` 2026-10-04). Las de otro usuario siguen esperando el umbral.
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
- **Formato del ticket térmico por agente (2026-09-30)**: `Ventas/ImprimirTicketPayload` incrusta negrita/doble tamaño/fuente B en `ticketLines` con `WebCore/Services/EscPosFormato.cs` (título y negocio en doble tamaño, Total en negrita), igual que el WinForms. Detalle y anchos en `DECISIONS.md` 2026-09-30.
- **Modal post-expendio (2026-09-30)**: opciones 1 Nuevo expendio, 2 Ticket térmico (`ImprimirTicketHtml`/`ImprimirTicketPayload`, tamaño en `localStorage` `postventa_ticket_mm`), 3 PDF, 4 Email, 5 PDF lista de precios (solo PRESUPUESTO). Elegir una limpia la vista; Escape pregunta "¿modificar el expendio ya generado?" (Sí = editable y Finalizar actualiza el mismo expendio vía `actualizarExpendio`; No = vuelve al modal). Detalle en `DECISIONS.md` 2026-09-30. El PDF lista de precios usa tabla `Producto [cod: N]` + `Precio`, y el chip del POS dice "Vigencia de precios desde" en PRESUPUESTO.
- **REMITOS (Entrega 2)**: el POS pide `Nro de remito` (obligatorio, sugerido `PPPP-NNNNNNNN` = id de sucursal + correlativo por sucursal, editable, unico por sucursal; columna `expendios.nroremito`, migración `20260926b-Alter_expendios_add_nroremito.sql`). Se busca por ese número en "Mis expendios", "Expendios generados" y el buscador de expendios de Ventas. Al guardar/modificar una venta con expendios REMITOS, `Negocio.Venta.AgregarLeyendaRemitosAsociados` agrega a las observaciones `REMITOS ASOCIADOS: a / b` (regla en `Negocio/NroRemito.cs`, tests `NroRemitoTests`/`VentaRemitosAsociadosTests`). Solo Postgres.
- **PDF de todos los sectores (2026-09-26)**: `PuntosExpendioController.GenerarPdfExpendio` usa el diseño del comprobante de venta para cualquier sector (rojo de marca, letra central, encabezado de tabla rosado, pie con paginación). Presupuesto: letra "P", "PRESUPUESTO"; el resto: letra "X", "EXPENDIO", con Sector y (REMITOS) Nro remito. Ya no existe el PDF simple anterior.
- **Modal de forma de pago: atajos y Pago Mixto (2026-09-30)**: `/` abre el descuento/recargo; `+` activa/desactiva el Pago Mixto desde cualquier lado del modal (también con el foco en un input); `1`-`6` eligen la forma de pago (en mixto, el 2.º medio; Efectivo y Cta. Cte. quedan bloqueados); `End` confirma "Finalizar Pago". Con mixto activo la leyenda junto al switch guía ("además de Efectivo") y, al elegir el 2.º medio, el foco pasa a `#montoEfectivo`. El modo compacto (`pos-modal-compacto`) se activa con descuento desplegado o mixto activo. Código: `_FormaPagoModal.cshtml` + `wwwroot/Scripts/app/forma-pago.js`; detalle en `docs/DECISIONS.md`.
- **Carga automática de expendio por código (2026-09-30)**: en `Ventas/POS`, al escribir/escanear en `#inputCodigo` un código `PE<número>F` (regex `^PE(\d+)F$`, la F marca el final) el expendio se carga solo, sin Enter (`pos-product.js`: `bindLiveSearch` + `cargarExpendioPorCodigo`; guard de "en curso" para no duplicar cuando la pistola además manda Enter). Con prefijo `PE<dígitos>` no se abre el modal de forma de pago. Enter sigue funcionando. Solo aplica donde existe `POSExpendiosCurrent` (Ventas/POS), no en `PuntosExpendio/POS`.
- **Cliente fijo, caducidad e historial de precios (2026-10-04)**: en el POS Expendio, elegir un cliente real con F9 deja el campo en solo lectura (✕ para soltarlo y volver a texto libre) y guarda su id en `expendios.idpersona` en **todos** los sectores (Consumidor Final no se guarda; tras guardar la pantalla vuelve vacía y manual). En **PRESUPUESTO** aparece "Válido por (días)" (default 90, 1 a 365; el servidor guarda la fecha en `expendios.fechacaducidad`, ver `SectorPuntoExpendio.ResolverCaducidad`). El F8 "Historial de precios" (botón junto al cliente en Expendio, mismo modal que en Ventas) tiene solapas **Compras | Presupuestos (n)**; reglas de estados (Futuro / Reemplazado / Caducado / Aplicable) en `Negocio/PresupuestosCliente.cs`, consulta en `DatosPostgres/PresupuestoClientePg.cs`, vistas `Views/Ventas/_HistorialPreciosCliente{ConPresupuestos,Expendio}.cshtml`. **Ventas**: solo el producto Aplicable se copia al carrito; los demás informan el motivo al tocarlos. **Expendio**: referencia, cada fila tiene precio nuevo y cantidad y "Agregar" mete la línea al carrito; la solapa Compras solo para admin o permiso `NuevaVenta`. Un cliente sin presupuestos ve el F8 de Ventas exactamente como antes. Detalle y alternativas descartadas en `DECISIONS.md` 2026-10-04. Solo Postgres.
- **Modal "Expendios" de Ventas/POS: filtro por persona y por sector (2026-10-04)**: "Persona (cliente del expendio)" es un buscador incrustado (2+ letras sobre `Personas/Listar`); al elegir una persona se buscan en el servidor los expendios guardados con ese cliente (`BuscarExpendiosPOS?idPersona=`, `DatosPostgres/ExpendioClientePg`), "Fecha desde" pasa a hace 6 meses (se respeta una fecha editada a mano) y la ✕ vuelve a la búsqueda de siempre con la fecha de hoy. "Sector" está en los filtros avanzados (opciones = sectores de los resultados, filtra en el navegador). "Limpiar filtros" resetea todo. Solo Postgres; los expendios sin cliente fijo no aparecen al filtrar por persona. Detalle en `DECISIONS.md` 2026-10-04.
