# Incidencias frecuentes

## Objetivo

Registrar fallas repetidas, sintomas, diagnostico y resolucion conocida.

## Secciones

- Sintoma
- Causa probable
- Verificaciones
- Resolucion

---

## 2026-07-29 - "Error de instancia" al iniciar sesion en la web

- **Sintoma**: `System.InvalidOperationException: Error de instancia.` en `Utilidades\Conexion.cs` linea 72 (`cn.Open()`) al hacer login.
- **Causa**: en `Web\Config\connectionStrings.config` el `Data Source` estaba escrito `.\\sqlexpress` (doble backslash). El parser de connection strings de .NET no interpreta el backslash como escape, por lo que busca una instancia literal inexistente.
- **Verificacion**: abrir una `SqlConnection` con esa cadena falla; con `.\sqlexpress` devuelve `ServerVersion` y `Database`.
- **Resolucion**: dejar un solo backslash: `Data Source=.\sqlexpress`. El archivo es local y esta fuera de git (ver `connectionStrings.config.example`).

## 2026-07-29 - Atajos de teclado muertos en Cuenta Corriente

- **Sintoma**: en `Finanzas/CtaCtePersona` no respondia ningun atajo (Alt+Enter / Alt++ para nuevo pago, Alt+F, Alt+S, Enter en "Desde") ni los botones de enviar por mail.
- **Causa**: el layout carga jQuery al final del `<body>`, pero el script inline de la vista se ejecuta antes. El `$(document)` de ese bloque lanzaba `ReferenceError: $ is not defined` y cortaba la funcion antes de llegar a `bindAtajosCuentaCorrienteCaptura()`, dejando todo el bloque sin efecto.
- **Verificacion**: en el navegador, `window.__ctacteAtajosCapturaBound` quedaba `false` al terminar de cargar la pagina.
- **Resolucion**: envolver el bloque en `inicializarCtaCtePersona()` y llamarlo desde un `esperarJQuery()` (mismo patron que `Views\Personas\Editar.cshtml`). Aplica a cualquier vista que corra scripts con jQuery fuera de una seccion `scripts`.

## 2026-07-29 - No se puede finalizar ninguna venta del POS (400 en /Ventas/FinalizarVenta)

- **Sintoma**: al elegir la forma de pago en el POS, SweetAlert muestra "no se pudo finalizar la venta". En produccion, `App_Data\perf-web.log` muestra `status=400 | total=0-9 ms | db=0 ms/0 calls` en el 100% de los intentos desde el 2026-07-28 21:23 en adelante (osea, ni siquiera llega al controller).
- **Causa**: `Web/Filters/ValidateAppAntiForgeryTokenAttribute.cs` es un filtro global (`FilterConfig.RegisterGlobalFilters`) que exige el antiforgery token en `Request.Form` o en el header `RequestVerificationToken`. El `$.ajax()` de `finalizarVenta()` en `forma-pago.js` manda `contentType: 'application/json'` con el body crudo (`JSON.stringify(payload)`), asi que `Request.Form` queda vacio. El auto-inyector global de headers (`modal-request-loading.js`, hook de `ajaxSend`) deberia agregar el header solo, pero en la practica no llega a tiempo/no corre en el flujo del POS y la request siempre se rechaza antes de llegar al controller.
- **Verificacion**: reproducido con un POST manual incluyendo el header `RequestVerificationToken` (tomado del mismo `input#globalAntiForgeryToken` de la pagina) -> pasa el filtro y llega a la logica de negocio. Sin el header -> `400` inmediato, siempre.
- **Resolucion**: en `Web/Scripts/app/forma-pago.js`, el `$.ajax()` que llama a `FinalizarVenta`/`ModificarVenta` ahora lee el token directo del DOM (`#globalAntiForgeryToken input[name="__RequestVerificationToken"]`) y lo manda explicito en `headers: { RequestVerificationToken: ... }`, sin depender del auto-inyector global.
- **CAUSA RAIZ REAL (encontrada tras probar contra produccion con Chrome real vía CDP)**: `Web/Scripts/app/modal-request-loading.js` daba `404` en produccion (`window.ModalRequestLoading` quedaba `undefined` a pesar de que jQuery si cargaba). El archivo existe en el repo y funciona en local porque IIS Express sirve cualquier archivo del disco sin importar el `.csproj`, pero el *publish* de produccion solo empaqueta lo que `Web.csproj` lista explicitamente como `<Content Include>`. Se encontraron **6 archivos JS huerfanos** (en disco, referenciados por vistas reales, pero ausentes del `.csproj` y por lo tanto ausentes de todo deploy): `modal-request-loading.js`, `modal-postmovimiento.js`, `movimientos.js`, `pago-cheques.js`, `pago-ux.js`, `swal-single-confirm.js`. Se confirmo con Views (130/130 ok) y Content/css (7/7 ok) que el resto del proyecto no tiene el mismo problema.
- **Resolucion real**: se agregaron los 6 `<Content Include="Scripts\app\...">` faltantes a `Web.csproj`. El fix puntual en `forma-pago.js` (mandar el header a mano) se mantiene como refuerzo pero ya no es la causa: una vez que `modal-request-loading.js` carga en produccion, el auto-inyector global vuelve a funcionar solo.
- **Leccion**: cualquier archivo nuevo bajo `Web/` creado por una sesion de IA o herramienta externa a Visual Studio (que no pasa por "Add Existing Item") puede quedar fuera del `.csproj` sin que nada avise — funciona perfecto en `local` (IIS Express) y desaparece silenciosamente en cada deploy a produccion. Ante un bug que "solo pasa en produccion", verificar primero si el archivo estatico relevante devuelve 404 ahi (`curl -sk https://carnisys.com/<ruta> -o /dev/null -w "%{http_code}"`) antes de asumir una causa de codigo/logica.

## 2026-07-30 - Lectura de codigos de barra por camara dejo de andar (todas las vistas)

- **Sintoma**: el boton de escanear codigo de barra (icono de barcode) no abrÃ­a la camara ni mostraba error visible, en `Ventas/POS`, `PuntosExpendio/POS`, `Productos/Index` (3 escaners: precio, catalogo global, codigo global) y `Productos/AddOrEdit`. Reportado "hace varios dias".
- **Causa**: `Web/Helpers/SecurityRuntime.cs` manda el header `Permissions-Policy` con `camera=()` desde el commit `8c749537` (2026-07-16, "Refuerza seguridad web y manejo de secretos"). Una politica vacia bloquea `getUserMedia({video})` para **cualquier origen, incluido el propio sitio**, antes de que el navegador muestre el dialogo de permiso — por eso no habia error visible.
- **Causa adicional (side-quest del mismo pedido)**: el motor de deteccion (`Web/Scripts/app/scanner.js`) usaba solo la API nativa `BarcodeDetector` (Shape Detection API), que **no existe en Firefox ni Safari/iOS** — ahi el boton mostraba "Este navegador no soporta BarcodeDetector" incluso con la politica bien configurada.
- **Resolucion**:
  1. `camera=()` -> `camera=(self)` en `SecurityRuntime.cs` (mismo patron que `geolocation=(self)`).
  2. Se vendorizo `@zxing/library@0.23.0` (UMD, `Content/vendor/zxing/zxing.min.js`, agregado a `Web.csproj`) como motor de respaldo para navegadores sin `BarcodeDetector`. `scanner.js` ahora detecta que motor usar en tiempo de ejecucion: Chromium sigue usando `BarcodeDetector` nativo sin cambios; Firefox/Safari/iOS usan `ZXing.BrowserMultiFormatReader.decodeFromStream()` sobre la misma camara, restringido a los mismos formatos (`EAN_13`, `EAN_8`, `CODE_128`, `UPC_A`). La API publica de la clase `BarcodeScanner` no cambio, asi que ninguna vista necesito modificarse mas alla de cargar el script nuevo.
  3. El script de ZXing se agrego en los dos layouts que lo usan (`_LayoutBase.cshtml`, `_LayoutPOS.cshtml`), antes de `scanner.js` — cubre las 4 vistas (5 escaners) sin tocar cada una.
- **Verificacion**: se probo con Chrome real (headless via CDP), sin poder usar camara fisica real: (a) round-trip de ZXing con una imagen EAN-13 real (Wikimedia, valor conocido `5901234123457`) decodificado correctamente con el bundle vendorizado; (b) integracion real de `scanner.js` con `BarcodeDetector` borrado a proposito (simula Firefox/Safari) y una camara falsa (`canvas.captureStream()`): el motor ZXing se activa, procesa frames continuamente (callback disparado cientos de veces), y `cerrar()` libera la camara sin excepciones. No se pudo probar con un lector fisico real ni en Firefox/Safari reales — pendiente de confirmacion del usuario en dispositivo real.
- **Nota**: `decodeFromStream` de esta version de ZXing no devuelve un objeto de control usable (resuelve `undefined`); el corte del escaneo se hace deteniendo el `MediaStream` directamente en `cerrar()`, igual que el motor nativo.

## 2026-07-30 - Calculadora de billetes abria el modal de impresion con resultado $0

- **Sintoma**: al cerrar/aceptar la calculadora de billetes (F3) sin cargar ningun billete, igual se abria el modal de "que haces con esto" (imprimir/pdf/whatsapp) con un comprobante vacio.
- **Causa**: `Web/Scripts/app/calculadora-billetes.js` llamaba `abrirPostModal()` sin condicion en los handlers de `#btnAceptarCalculadoraBilletes` y `#btnCancelarCalculadoraBilletes, #btnCerrarCalculadoraBilletes`.
- **Resolucion**: nuevo helper `hayMontoParaImprimir()` (chequea `getPrintData().total > 0`); si no hay monto, `cerrarCalculadoraSinPost()` cierra el modal directo (via el `permitiendoCerrar` que ya usaba el guard existente de `hide.bs.modal`), sin pasar por el post-modal.
- **Verificacion**: probado en Chrome real (CDP) con cache de navegador deshabilitada: total $0 -> cierra sin post-modal; total $2.000 -> post-modal sigue abriendo igual que antes (sin regresion).

## 2026-07-30 - Texto invisible en hover de botones outline (modo claro)

- **Sintoma**: al pasar el mouse sobre el boton "Duplicar POS" (`Ventas/POS.cshtml`), en modo claro el texto se volvia ilegible (blanco sobre fondo claro).
- **Causa**: `Content/css/ui-refresh.css` tiene `body.app-shell .btn-outline-primary, .btn-outline-secondary, .btn-outline-dark { background: transparent; }`. Esa regla (por especificidad: 1 tipo + 2 clases) le gana al `:hover` propio de Bootstrap (`sb-admin-2.css`, 2 clases/pseudo-clases) y lo pisa **incluso en estado hover**, dejando el fondo transparente mientras Bootstrap ya puso el texto en blanco -> texto blanco invisible sobre fondo claro.
- **Resolucion aplicada (acotada)**: `#btnDuplicarPOS.btn-outline-primary:hover { background-color/color !important; }` en `POS.cshtml`, especifico para este boton.
- **Riesgo pendiente (no corregido, ver `riesgos-conocidos.md`)**: la regla de `ui-refresh.css` afecta a **todo** `.btn-outline-primary/secondary/dark` dentro de `body.app-shell`, no solo a este boton — cualquier otro boton outline que dependa del hover blanco-sobre-solido de Bootstrap tiene el mismo problema latente.

## 2026-07-30 - JS de POS servido desde cache del navegador tras editar el archivo

- **Sintoma**: al probar un fix en `calculadora-billetes.js` en un navegador ya usado antes en la sesion, el comportamiento viejo seguia apareciendo pese a que el archivo en disco y lo que devolvia el servidor (verificado con `curl`) ya tenian el fix.
- **Causa**: los scripts de POS se referencian con cache-busting manual por query string (`calculadora-billetes.js?v=3237`, `pos-cart.js?v=26`, `factura-electronica.js?v=11`). Si se edita el archivo sin subir el numero de version, el navegador sigue sirviendo la copia cacheada de esa URL exacta.
- **Resolucion**: al tocar cualquiera de estos archivos, **subir el numero de `?v=` en TODAS las vistas/layouts que lo referencian** (buscar `grep -rn "nombre-archivo.js?v="` en `Web/Views`), no alcanza con guardar el `.cs`/`.js`.
- **Leccion**: esto no es solo un problema de testing — cualquier usuario real que haya usado la funcion antes del deploy va a seguir corriendo el JS viejo hasta que la version cambie. Regla para el futuro: **todo cambio a un archivo con `?v=` en su tag `<script>` debe incluir el bump de esa version en el mismo commit**, sin excepcion.

## 2026-07-31 - Header del modal "Mis actividades" con altura descomunal en responsive (~890px)

- **Sintoma**: en el POS, abriendo "Mis actividades" (F6) en mobile, el encabezado de filtros ocupaba muchisima altura (controles separados, la tabla quedaba fuera de pantalla).
- **Causa**: `Content/css/custom.css` tiene `.egresos-pos-toolbar { flex-direction: column; }` dentro de `@media (max-width: 767.98px)`, pensado para el toolbar simple de la pantalla "Egresos de Caja" sola. En `Web/Views/Cajas/_MisEgresosCaja.cshtml`, `.egresos-pos-toolbar-main` y `.egresos-pos-toolbar-actions` tienen `flex: 1 1 180px` / `flex: 999 1 680px` — esos numeros estaban pensados como **ancho** (flex-basis en modo fila). Al heredar `flex-direction: column` de la regla generica, el mismo `flex-basis` pasa a interpretarse como **alto**, y `actions` encima tiene `flex-grow: 999` (dominante) → el toolbar terminaba con ~890px de alto medido en Chrome real (390x844), solo para el header.
- **Verificacion**: medido con Chrome real (CDP) el `getBoundingClientRect()` de cada elemento antes y despues del fix: `main` pasaba de 180px a 25px de alto, el toolbar completo de ~890px a 165px.
- **Resolucion**: `flex-direction: row` explicito para `.mis-actividades-modal .egresos-pos-toolbar` dentro del mismo media query, scopeado para no afectar el toolbar simple de "Egresos de Caja" sola (que sigue usando `column` sin problema porque ahi no hay flex-basis en pixeles grandes de por medio).
- **Leccion**: un `flex-basis` en px asume una direccion de flex fija. Si una regla generica en otro archivo puede cambiar `flex-direction` a un breakpoint dado (via CSS de mas alto nivel, no visible en el archivo que se esta editando), ese basis se reinterpreta silenciosamente como el otro eje. Antes de asumir "la altura viene de mi cambio", medir el `flex-direction` computado real del contenedor, no solo leer el CSS del archivo que se toco.
- **Regresion propia detectada por el usuario (mismo cambio)**: al reordenar los botones de filtro ("Todos"/"Egresos de caja"/"Pagos electronicos"/"Cta Cte") en una fila con scroll horizontal (`flex: 0 0 auto`), cada boton igual quedaba ocupando el 100% del ancho del contenedor — solo se veia uno a la vez al scrollear, pareciendo que los demas habian desaparecido. Causa: `custom.css` tiene `.egresos-pos-toolbar .btn { width: 100%; }` en el mismo breakpoint; `flex-basis: auto` (de `flex: 0 0 auto`) **usa el `width` existente como base** en vez de ignorarlo, asi que el `width:100%` heredado seguia ganando. Fix: agregar `width: auto` explicito junto al `flex`. Verificado con Chrome real: los 4 botones vuelven a medir segun su contenido (54px/109px/127px/62px) y entran los 4 sin necesidad de scroll en 390px de ancho. **Leccion**: `flex-basis: auto` no es "ancho automatico puro", delega en la propiedad `width` si esta esta declarada en otro lado — al tocar `flex` de un elemento hay que revisar tambien si algo mas en la cascada le fija `width` explicito.

## 2026-07-31 - Timeout SQL en /Elaborados (Index) al buscar embutidos

- **Sintoma**: `System.Data.SqlClient.SqlException: Tiempo de espera de ejecucion agotado` (Win32Exception subyacente) en `ElaboradosController.Index` -> `Corte.buscarEmbutido` (`Utilidades\Db.cs:188`, `da.Fill(dt)`), timeout de 30s (`Web.config` -> `timeOut`).
- **Causa**: no es la consulta ni los datos — en la PC de testing donde se reprodujo, `Embutidos`/`CortePorEmbutido`/`Corte` estaban vacias (0 filas). La sesion SQL quedo en wait `RESOURCE_SEMAPHORE` (esperando memory grant): la PC tenia ~6.96 GB de RAM total y solo ~476 MB libres en el momento del error (SQL Server en si solo usaba 146 MB — no es el que acapara memoria). Con tan poco margen, SQL Server no consigue el memory grant a tiempo ni para un query trivial.
- **Verificacion**: `sys.dm_exec_requests` mostro la sesion en `suspended`/`RESOURCE_SEMAPHORE` con 25s de espera; `sys.dm_os_sys_memory` confirmo `avail_mb=476` sobre `total_mb=6958`; `sys.dm_os_process_memory` confirmo que SQL Server usaba solo 146 MB.
- **Resolucion**: no aplica un fix de codigo — es contencion de RAM del sistema operativo en esa PC de testing (Visual Studio + SQL Server + IIS Express + navegador simultaneos sobre ~7GB). Liberar memoria (cerrar apps no usadas) mientras se prueba; si es recurrente, evaluar si esa PC alcanza para correr el stack completo de desarrollo a la vez.
- **Hallazgo aparte (mismo pedido, bug real no relacionado al timeout)**: el SP `dbo.buscarEmbutido` tenia el filtro de sucursal roto — `... or dbo.Embutidos.idSucursal > 0` es casi siempre verdadero, asi que el combo "Sucursal" de `/Elaborados/Index` nunca filtraba, devolvia todas las sucursales sin importar la seleccionada. Corregido con `ALTER PROCEDURE` a `(@idSucursal <= 0 and dbo.Embutidos.idSucursal > 0)`, mismo patron ya usado correctamente en `Corte.cs:828` (`obtenerUltimosElaboradosDashboard`). **Aplicado solo en la base local (`.\sqlexpress`) de esta PC** — falta aplicar el mismo `ALTER PROCEDURE` en el/los servidores reales (VM `carnisys.com` y/o SM) cuando el usuario lo confirme, ya que los SPs no estan versionados en este repo.
