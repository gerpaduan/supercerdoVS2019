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
