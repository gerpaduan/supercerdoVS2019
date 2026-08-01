# Bitacora de cambios

## Objetivo

Registrar cambios funcionales o tecnicos realizados sobre el sistema.

## Secciones

- Fecha
- Cambio
- Archivos o modulos afectados
- Motivo
- Resultado

## 2026-07-29

- Cambio: primer deploy de produccion hecho a traves de Claude, contra la VM Windows (`carnisys.com`), commit `79f7d53f` de `codex_ia`. Incluye el fix de atajos de teclado de `CtaCtePersona` (ver `docs/07-operacion-y-soporte/incidencias-frecuentes.md`) y la limpieza de `AGENTS.md`/`PROMPT_CARNISYS.md`/`docs/10-carnisys-ng/`.
- Archivos o modulos afectados: sistema web legacy en `C:\inetpub\wwwroot\CarniSysWeb` de la VM.
- Motivo: pedido explicito de deploy a produccion.
- Resultado: publish Release precompilado, subido por SFTP y aplicado con el App Pool `CarniSys` detenido brevemente. Se preservaron intactos `Config\connectionStrings.config`, `Config\appSettings.secrets.config`, `AFIP\` y `App_Data\` de la VM (no se tocan en un deploy: son estado vivo, no codigo). Backup completo del `CarniSysWeb` previo en `C:\inetpub\wwwroot\web\backups\CarniSysWeb_20260729_210501` (rollback: restaurar esa carpeta con robocopy /MIR y reiniciar el App Pool). Verificado con `200 OK` en `/Login/Index` sin excepciones tras el deploy. Ver riesgo documentado en `riesgos-conocidos.md` sobre `requireSSL`.

## 2026-07-29 (segundo deploy, mismo dia) - Fix de POS/Movimientos/pagos rotos en produccion

- Cambio: se agregaron a `Web.csproj` 6 archivos `<Content Include>` de `Scripts\app` que existian en disco y se usaban desde vistas reales pero nunca se habian agregado al proyecto: `modal-request-loading.js`, `modal-postmovimiento.js`, `movimientos.js`, `pago-cheques.js`, `pago-ux.js`, `swal-single-confirm.js`. Se agrego ademas un header explicito de antiforgery en `Web/Scripts/app/forma-pago.js` (refuerzo, no la causa raiz).
- Archivos o modulos afectados: `Web.csproj`, `Web/Scripts/app/forma-pago.js`.
- Motivo: el usuario reporto que el POS no permitia finalizar ninguna venta al elegir forma de pago. Investigando se encontro que `modal-request-loading.js` (el script que auto-inyecta el token CSRF en los POST JSON) daba `404` en produccion, aunque funcionaba perfecto en local. Causa: el publish de produccion solo empaqueta lo que `Web.csproj` lista como `Content`; en local, IIS Express sirve cualquier archivo del disco sin mirar el `.csproj`, por eso el problema era invisible en desarrollo. Se verifico con Chrome real (via CDP) contra `carnisys.com` con el codigo original, confirmando `400` en `/Ventas/FinalizarVenta` siempre. Se audito el resto del proyecto (130 vistas, 7 css) y no hay mas archivos huerfanos.
- Resultado: republish + mismo procedimiento de deploy documentado en `despliegue-y-publicacion.md`. Backup previo en `C:\inetpub\wwwroot\web\backups\CarniSysWeb_20260729_220100`. Verificado con Chrome real contra produccion: `window.ModalRequestLoading` ya existe, y `FinalizarVenta` responde `200` (antes `400` en el 100% de los intentos desde el 2026-07-28 21:23). De paso quedaron arreglados Movimientos, pagos/cheques y las confirmaciones de SweetAlert en toda la app, que dependian de los mismos archivos huerfanos.

## 2026-07-30 - Lectura de codigo de barra por camara, cross-browser

- Cambio: `Permissions-Policy` ya no bloquea la camara (`camera=()` -> `camera=(self)`), y se agrego ZXing (`@zxing/library@0.23.0`, vendorizado) como motor de decodificacion de respaldo para navegadores sin `BarcodeDetector` (Firefox, Safari/iOS).
- Archivos o modulos afectados: `Web/Helpers/SecurityRuntime.cs`, `Web/Scripts/app/scanner.js`, `Web/Content/vendor/zxing/zxing.min.js` (nuevo), `Web/Web.csproj`, `Web/Views/Shared/_LayoutBase.cshtml`, `Web/Views/Shared/_LayoutPOS.cshtml`.
- Motivo: el usuario reporto que la lectura de codigo de barra por camara dejo de funcionar hace varios dias, en las 5 pantallas donde se usa (POS de Ventas, POS de PuntosExpendio, y 3 escaners en Productos). Ver detalle de la causa e investigacion en `docs/07-operacion-y-soporte/incidencias-frecuentes.md`.
- Resultado: compila y precompila sin errores (Release). Verificado con Chrome real: round-trip de ZXing con una imagen EAN-13 real decodifica correctamente, e integracion completa de `scanner.js` con el motor nativo deshabilitado a proposito (simulando Firefox/Safari) procesa frames y detecta codigos sin excepciones. Pendiente: probar en un dispositivo real con Firefox y con Safari/iOS, y con un lector fisico de codigo de barra apuntando a productos reales — no se pudo automatizar esa parte.

## 2026-07-30 - POS: mejoras de UI/UX en modo responsive y desktop (dos rondas)

- Cambio: primera ronda (7 puntos, solo responsive) — el panel inferior arranca scrolleado hacia el boton Finalizar al iniciar/tras una venta; total e importe con tipografia mas chica; input cantidad mas chico (alto y fuente, igualado al de codigo); descripcion del producto mas oscura en modo claro; bloque "Conexion balanza" oculto en responsive; los 3 botones del pie (ayuda/comentario/cancelar) a la misma altura; inputs del modal de forma de pago ensanchados a 12 caracteres. Segunda ronda (8 puntos, mezcla de responsive y general): se oculta la flechita "volver arriba" en responsive (no tiene sentido en el flujo del POS); el badge "1" de comentario se apila debajo del boton en vez de salirse del borde; la calculadora de billetes ya no abre el modal de impresion si el resultado es $0 (ver `incidencias-frecuentes.md`); estilo nuevo del encabezado del modal "Mis actividades" (antes generico/plano); el aviso de salida del POS (`beforeunload`) ahora tambien dispara si hay algun modal abierto, no solo con venta en curso; fix de texto invisible en hover del boton "Duplicar POS" en modo claro (ver `incidencias-frecuentes.md` y el riesgo relacionado en `riesgos-conocidos.md`); el modal de factura electronica ya no se puede cerrar por click afuera / Esc, y la X del header pasa por la misma logica que "Cancelar"; estilo nuevo de la fila de filtros del modal "Mis ventas" (buscar cliente + forma de pago + volver).
- Archivos o modulos afectados: `Web/Views/Ventas/POS.cshtml`, `Web/Views/Shared/_LayoutPOS.cshtml`, `Web/Views/Ventas/_FormaPagoModal.cshtml`, `Web/Views/Ventas/_ModalComentarioVenta.cshtml` (solo lectura, sin cambios), `Web/Scripts/app/calculadora-billetes.js`, `Web/Scripts/app/pos-cart.js`, `Web/Scripts/app/factura-electronica.js`, `Web/Views/Ventas/_FacturaElectronica.cshtml`, `Web/Views/Ventas/_MisVentas.cshtml`, `Web/Views/Cajas/_MisEgresosCaja.cshtml` (solo lectura), mas el bump de cache-busting (`?v=`) de los 3 `.js` tocados en `_LayoutBase.cshtml`, `_LayoutPOS.cshtml`, `DetalleFactura.cshtml`, `DetalleVenta.cshtml`, `Ventas/POS.cshtml` y `PuntosExpendio/POS.cshtml`.
- Motivo: pedido explicito del usuario, en dos tandas, de pulir la experiencia del POS (principalmente mobile/responsive) sin tocar la vista de escritorio ni romper funcionalidad existente.
- Resultado: probado en Chrome real (headless via CDP) logueado con el usuario de pruebas de `~/hosts/carnisys-web-local.env`, con cache de navegador deshabilitada (se detecto que el navegador tenia cacheado el JS viejo por el `?v=` sin bump — ver leccion en `incidencias-frecuentes.md`). Confirmado en vivo: flechita oculta en `<=991.98px`, badge de comentario apilado, calculadora cierra sin post-modal con $0 y sigue abriendo el post-modal con monto real (sin regresion), header de "Mis actividades" con el color nuevo, condicion de aviso de salida detecta modal abierto, `modalFacturaElectronica` con `data-backdrop=static data-keyboard=false`, filtros de "Mis ventas" en una sola fila en desktop y apilados a 100% de ancho en mobile. No se pudo probar en vivo, por alcance/tiempo: el hover del boton "Duplicar POS" (se verifico por calculo de especificidad CSS, no por simulacion real de `:hover`) y el flujo completo de facturacion electronica de punta a punta (requiere completar una venta real).

## 2026-07-31 - Fix header "Mis actividades" en responsive + deploy a produccion (carnisys.com)

- Cambio: `flex-direction: row` explicito para `.mis-actividades-modal .egresos-pos-toolbar` en mobile (el toolbar heredaba `column` de una regla generica de `custom.css`, y los `flex-basis` en px de main/actions pasaban de ser ancho a ser alto, explotando el header a ~890px — ver `incidencias-frecuentes.md`). Reordenamiento con `order` de flexbox (Mis ventas + Nuevo Egreso emparejados, filtros en fila con scroll horizontal) sin tocar el DOM. Fix de seguimiento: los botones de filtro habian quedado ocupando 100% del ancho por un `width:100%` heredado que `flex-basis:auto` no pisaba (detectado por el usuario, corregido con `width:auto` explicito).
- Archivos o modulos afectados: `Web/Views/Cajas/_MisEgresosCaja.cshtml`, commit `8f95c311` en `codex_ia`.
- Motivo: pedido explicito del usuario tras ver el header roto en mobile.
- Resultado: verificado con Chrome real (CDP, 390x844): header de 890px a 165px, los 4 filtros visibles sin scroll (54-127px cada uno segun contenido). Build de `Web.csproj` (Debug) sin errores antes de commitear.
- **Deploy a produccion** (mismo dia, a pedido explicito): publish Release de todo `codex_ia` (incluye este fix + el lote completo de UI/UX del POS del 2026-07-30 arriba) a la VM Windows (`carnisys.com`). Pasos: publish Release local -> ajuste manual de `requireSSL`/`Security:CookieRequireSsl` a `false` en las 3 ubicaciones conocidas (`EnforceHttps` ya coincidia en `true`, no hizo falta tocarlo) -> se saco `Config\connectionStrings.config` y `Config\appSettings.secrets.config` del paquete -> backup de `CarniSysWeb` en `C:\inetpub\wwwroot\web\backups\CarniSysWeb_20260731_115154` -> subida por SFTP y extraccion en `C:\inetpub\wwwroot\web\_deploy\carnisys-deploy-20260731` -> swap con `robocopy /MIR` de `bin`/`Content`/`Scripts`/`Views`/`fonts` + copia de sueltos -> `Start-WebAppPool` fallo la primera vez con "El servicio no acepta mensajes de control en este momento" (transitorio, W3SVC/WAS seguian `RUNNING`) -> reintento inmediato, arranco bien. Verificado: `https://carnisys.com/` -> `302` a `/Login/Index`, `/Login/Index` -> `200` sin stack trace, y los 3 `.js` con la version nueva (`calculadora-billetes.js?v=3238`, `pos-cart.js?v=27`, `factura-electronica.js?v=12`) responden `200` desde el servidor real. Rollback disponible: restaurar el backup de arriba con `robocopy /MIR` y reiniciar el App Pool `CarniSys`.

## 2026-08-01 - POS: resumen de cantidades en el carrito, atajo F4 y atajos B/C/E en modal "Linea de venta"

- Cambio: debajo de "Total" ahora se muestran dos lineas separadas: a la izquierda "N items activos" (antes estaba a la derecha), a la derecha la suma de cantidades del carrito distinguiendo por peso (kgs) de por unidad (ej. "3,650 kgs | 5 unidades"), usando el campo nuevo `pesable` de cada linea. Atajo **F4** simula el click sobre la ultima fila del carrito para abrir el modal "Linea de venta" de esa linea (agregado a `_ModalAyudaPOS.cshtml` y al dispatcher compartido de `pos-help.js`). Dentro de ese modal: **B** despliega el bloque de bonificacion y Enter sobre el precio/porcentaje modificado dispara "Aplicar Bonificacion"; **C** ya despliega cantidad (atajo preexistente) y ahora Enter aplica el valor (antes Enter no hacia nada ahi); **E** abre la confirmacion de eliminar y ahora "N" cancela (Enter ya confirmaba por default de SweetAlert2). Tambien: al cerrar el aviso de "linea anulada", el foco vuelve a `#inputCodigo` (antes quedaba en ningun lado tras cerrar con Enter).
- Archivos o modulos afectados: `Web/Views/Ventas/POS.cshtml`, `Web/Scripts/app/pos-cart.js`, `Web/Scripts/app/pos-help.js`, `Web/Scripts/app/ventas-expendios-pos.js`, `Web/Views/Ventas/_ModalAyudaPOS.cshtml`, mas bump de `?v=` de `pos-cart.js`, `pos-help.js` y `ventas-expendios-pos.js`.
- Motivo: pedido explicito del usuario, confirmado punto por punto antes de codificar (incluida una vuelta de ajustes sobre la propuesta original: orden kgs-antes-que-unidades, alineacion de "items activos", y el detalle del foco en el aviso de linea anulada).
- Resultado: **compila** y **verificado en Chrome real** (CDP) con productos reales de la base de prueba (CARRE cod.1 pesable, TURRON UNIDAD cod.10 no pesable). Resumen: "2 ítems activos" a la izquierda, "2,500 kgs | 5 unidades" a la derecha, total exacto ($30.500). F4 abrió el modal de la ultima linea agregada (verificado con `window.lineaSeleccionada`). B mostró el bloque de bonificación con foco en el precio, y Enter aplicó un recargo (5u x $10.000 = $50.000, total recalculado a $80.000). C mostró el bloque de cantidad con el valor preseleccionado, y Enter aplicó la nueva cantidad (8u x $10.000 = $80.000). E abrió la confirmación y "N" canceló sin borrar nada; el flujo de confirmar-y-eliminar en sí se probó con click nativo en "Sí" (soft-delete a `fila-anulada`, total recalculado) — **el Enter nativo de SweetAlert2 no se pudo probar por automatización**: un Swal de control sin ningún código propio tampoco respondió a un Enter disparado por CDP en el Chrome headless, así que es una limitación del entorno de prueba, no algo verificable ni atribuible a este cambio (SweetAlert2 enfoca "Sí" por default en cualquier confirm, comportamiento de la librería ya usado en producción en otros lados de la app). Clic en una línea anulada mostró el aviso y, al cerrarlo, el foco volvió correctamente a `#inputCodigo`.

## 2026-08-01 - Elaborados: receta en pasos estructurados en vez de texto libre

- Cambio: en `Elaborados/EditarFormula.cshtml`, el campo "Receta" pasa de ser un unico textarea de texto libre (con auto-numerado "Paso N:" al presionar Enter) a una lista de pasos individuales, cada uno con su propio textarea y boton para eliminarlo, mas un boton "Agregar paso". El formato guardado en la base sigue siendo el mismo texto plano `Paso N: <texto>` por linea (se parsea al cargar y se reconstruye al guardar), asi que las recetas ya cargadas antes de este cambio se siguen viendo bien. En `Elaborados/Carga.cshtml` (pantalla de solo carga/consulta, sin edicion de formula) se reordeno el layout: la receta paso a su propia tarjeta en la columna lateral derecha en vez de compartir fila con codigo/nombre del elaborado.
- Archivos o modulos afectados: `Web/Views/Elaborados/EditarFormula.cshtml`, `Web/Views/Elaborados/Carga.cshtml`.
- Motivo: **PENDIENTE** — este cambio ya estaba codificado (completo y compilando) al retomar la sesion; no quedo registrado en esta bitacora el pedido original ni la verificacion en navegador. Se commitea a pedido explicito del usuario ("commitear todo"), pero **no se probo en Chrome real en esta sesion** — a diferencia de los otros dos cambios de este mismo dia (scanner y atajos de POS), que si se verificaron en vivo.
- Resultado: compila (`MSBuild Web.csproj /p:Configuration=Debug`, sin errores). Pendiente de prueba en navegador real: agregar/eliminar pasos, que el texto plano guardado se parsee bien al reabrir una receta vieja (con o sin el prefijo "Paso N:"), y que el layout nuevo de `Carga.cshtml` se vea bien en mobile.

## 2026-08-01 - Lectura de codigo de barra por camara en Movimientos y Stock

- Cambio: se agrego el mismo flujo de escaneo de codigo de barra que ya existia en `Productos/AddOrEdit` (boton con icono `fa-barcode`, tarjeta de camara colapsable, doble lectura consecutiva + validacion de checksum EAN-8/EAN-13) a `Movimientos/Nuevo` (`Movimientos/Editar.cshtml`) y `Stock/Editar.cshtml`. Al leer un codigo valido, se pisa `#txtCodigoProducto` y se simula la tecla Enter sobre ese input — reusa 100% la logica de busqueda de producto que ya tenia cada vista (no se duplico ni se toco esa logica).
- Archivos o modulos afectados: `Web/Scripts/app/barcode-code-input.js` (nuevo, modulo compartido: puerto generico de la logica de `AddOrEdit.cshtml`, que **no se modifico**), `Web/Views/Shared/_ScannerCodigoBarra.cshtml` (nuevo, partial con el markup de la camara), `Web/Views/Movimientos/Editar.cshtml`, `Web/Views/Stock/Editar.cshtml` (los dos modos, con y sin layout — el modo AJAX no tenia `scanner.js`/ZXing cargado, se agrego ahi tambien), `Web/Content/css/custom.css` (estilos del scanner, copia de los que ya tenia `AddOrEdit.cshtml` inline), `Web/Web.csproj`.
- Motivo: pedido explicito del usuario, con estudio previo confirmado antes de codificar (prioridad: no romper `AddOrEdit.cshtml` ni la logica de busqueda existente de las otras dos vistas).
- Resultado: **compila**. Verificado con Chrome real (CDP) en las dos vistas: doble lectura con EAN-13 valido (`5901234123457`) se acepta recien en la segunda lectura identica, dispara la busqueda existente (probado con codigo inexistente -> "No existe o sin coincidencia", igual que tipeando a mano + Enter) y deja el foco donde el flujo original lo dejaria; un EAN con digito verificador incorrecto se rechaza sin tocar el input ni disparar nada. No se pudo probar con camara fisica real ni en Firefox/Safari reales (mismo `TODO` que ya existia para el scanner de `AddOrEdit`, ver `incidencias-frecuentes.md` 2026-07-30). El modo AJAX de `Stock/Editar.cshtml` (`Request.IsAjaxRequest()`) se cubrio agregando los scripts ahi tambien, pero no se encontro ni se probo un caller real que dispare ese modo especifico.

## 2026-07-31 - Elaborados: receta con pasos numerados + segundo deploy a produccion (carnisys.com)

- Cambio: en `EditarFormula` (edicion de formula de un elaborado), la "Receta" pasa de un textarea suelto dentro del box de datos del elaborado a su propia tarjeta en la columna lateral, con auto-resize (`autoResizeTextarea`) y Enter que propone el siguiente paso numerado (`Paso N: `, contando lineas ya escritas; Shift+Enter deja salto de linea libre dentro de un mismo paso). Los buscadores de elaborado/ingrediente (`btnBuscarElaboradoFormula`, `btnBuscarIngredienteFormula`) dejan la logica de modal inline (`openModal` local) y pasan a usar la funcion compartida `window.abrirBuscarProductoModal` de `modal-productos.js`, igual que el resto de pantallas que ya la usan. Tambien se reordeno el grid: el box "Elaborado" ahora ocupa todo el ancho de la columna principal (antes compartia fila con "Receta").
- Archivos o modulos afectados: `Web/Views/Elaborados/EditarFormula.cshtml`, commit `1e09bd49` en `codex_ia`.
- Motivo: pedido explicito del usuario.
- Resultado: build Release sin errores antes de commitear. No se corrio una prueba en vivo de esta pantalla en particular (formulario de Elaborados, requiere un elaborado real cargado) — pendiente que el usuario la pruebe en produccion.
- **Deploy a produccion**: publish Release de `codex_ia` en `1e09bd49` (incluye este cambio + `EditarFormula.cshtml`) a la VM Windows (`carnisys.com`), mismo procedimiento que el deploy anterior (ver entrada de arriba), esta vez empaquetado como `.tar.gz` en vez de zip (tar viene incluido en Windows Server desde 2019, sin dependencias nuevas) y subido/extraido/swapeado por SSH real (`Invoke-SSHCommand` con script en base64, ya que el shell por defecto de esta VM es `cmd.exe`). Backup en `C:\inetpub\wwwroot\web\backups\CarniSysWeb_20260731_194226`. Verificado: `https://carnisys.com/` -> `302` a `/Login/Index?ReturnUrl=%2f`, `/Login/Index` -> `200` sin stack trace ni excepciones, cookie `Secure`/`HttpOnly` (la agrega Caddy, consistente con `requireSSL=false` de este lado), y las dos versiones de `.js` que cambiaron en este lote (`calculadora-billetes.js?v=3238`, `pos-product.js?v=9`) responden `200`. Rollback: restaurar el backup de arriba con `robocopy /MIR` y reiniciar el App Pool `CarniSys`.

## 2026-07-31 - POS Ventas: resumen de cantidades del carrito + atajo F4 + atajos B/C/E en "Linea de venta"

- Cambio (3 pedidos del usuario, confirmados antes de codificar):
  1. Debajo de "Total" en el POS ahora se muestran dos lineas: a la izquierda "N ítems activos" (antes estaba a la derecha, sola), a la derecha la suma de cantidades del carrito separando lo pesable de lo que se vende por unidad, en el orden `kgs | unidades` (ej. `2,350 kgs | 5 unidades`; se omite el lado que da cero). Requirio agregar un campo `pesable` a la linea del carrito en los 3 lugares donde se arma (`pos-cart.js` al agregar producto normal, `ventas-expendios-pos.js` al agregar por expendio — ahi siempre `true`, son ventas por peso —, y el `lineasEdicionPos` server-side de `POS.cshtml` al editar una venta existente, leyendo `linea.Corte.Pesable`).
  2. Atajo **F4**: simula el click sobre la ultima fila del carrito para abrir el modal "Linea de venta" de esa linea (reusa el mismo handler de click real, sin logica duplicada). Se agrego al dispatcher compartido de atajos (`pos-help.js`, usado tambien por `PuntosExpendio/POS.cshtml`) y al modal de ayuda (F1) como "Editar última línea". De paso, el aviso de "línea anulada" (cuando F4 o un click apuntan a una linea ya anulada) ahora devuelve el foco a `#inputCodigo` al cerrarse (el boton "OK" ya quedaba enfocado por default de SweetAlert2, eso no hizo falta tocarlo).
  3. Dentro del modal "Linea de venta": **B** despliega el bloque Bonificar (mismo click que el boton), Enter sobre el precio bonificado o el porcentaje dispara "Aplicar bonificación". **C** despliega el bloque Cantidad y selecciona el contenido del input (antes solo lo enfocaba), Enter dispara "Aplicar cantidad". **E** dispara "Eliminar" (el aviso de SweetAlert); Enter confirma (default de la libreria) y se agrego **N** para cancelar (no existe nativo). Los tres atajos se ignoran si el foco esta en un input de texto o si hay un SweetAlert encima.
- Archivos o modulos afectados: `Web/Scripts/app/pos-cart.js`, `Web/Scripts/app/ventas-expendios-pos.js`, `Web/Scripts/app/pos-help.js`, `Web/Views/Ventas/POS.cshtml`, `Web/Views/Ventas/_ModalAyudaPOS.cshtml`, mas el bump de cache-busting (`?v=`) de `pos-cart.js`, `ventas-expendios-pos.js` y `pos-help.js` en `Ventas/POS.cshtml` y, donde aplica, `PuntosExpendio/POS.cshtml`.
- Motivo: pedido explicito del usuario, con plan discutido y confirmado antes de escribir codigo (alcance y detalle de cada atajo).
- Resultado: probado en Chrome real (CDP) con dos productos de prueba insertados a mano en la base local (`Corte` estaba vacia; se sumo `sp_set_session_context 'IdEmpresa', 1` porque la tabla tiene RLS) y borrados al terminar. Se detecto y corrigio en el momento un bug propio: el atajo F4 no respondia porque `pos-help.js` se edito sin subir su `?v=5`→`?v=6` (mismo problema documentado en `incidencias-frecuentes.md` el 2026-07-30) — el navegador servia la version cacheada sin el `case 'F4'`. Confirmado en vivo, con productos reales agregados via el flujo normal (buscar + cantidad + agregar): resumen `"2,350 kgs | 5 unidades"` con un producto pesable y uno por unidad; F4 abre la linea correcta; B/Enter aplica bonificacion; C enfoca y selecciona la cantidad, Enter la aplica; E abre el aviso, N cancela sin borrar, "Sí" (clickeado, Enter-en-boton-enfocado es comportamiento nativo del navegador y no se pudo simular con eventos sinteticos) confirma y borra; el aviso de línea anulada devuelve el foco a código. Build de `Web.csproj` (Debug) sin errores antes de las pruebas. No commiteado todavia — pendiente confirmacion del usuario.

## 2026-07-10

- Cambio: se unifico la base NG de `Stock` en una sola pantalla `Edit` por `tipoCompra`, alineada a la estructura del MVC.
- Archivos o modulos afectados: `src/CarniSys.NG.Web`.
- Motivo: el modulo legacy usa una sola vista compartida para `Ingreso`, `Egreso`, `Cierre`, `Ajuste` y `Pesaje`, con bloques condicionales por accion; mantener pantallas separadas en NG iba contra ese flujo probado.
- Resultado: NG ahora entra a una unica vista base con combo de accion y secciones visuales que cambian segun `tipoCompra`; todavia faltan la grilla operativa real, modales y persistencia.

- Cambio: se agrego el `index` operativo NG de `Stock` con accesos separados a `Ingreso`, `Egreso`, `Cierre` y `Ajuste`.
- Archivos o modulos afectados: `src/CarniSys.NG.Web`.
- Motivo: alinear la navegacion del modulo con el legacy antes de implementar escrituras reales.
- Resultado: NG ya muestra las entradas operativas del modulo protegidas por `formAddOrEditStock`; cada una abre una pantalla placeholder controlada, sin guardar datos ni alterar stock real.

- Cambio: se agrego base NG de `punto stock por sucursal` en la edicion de productos, manteniendo `puntoStock` general como respaldo compatible.
- Archivos o modulos afectados: `src/CarniSys.NG.Application`, `src/CarniSys.NG.Infrastructure`, `src/CarniSys.NG.Web`.
- Motivo: continuar el vertical de `Stock` con el menor impacto posible sobre el comportamiento legacy ya estable, antes de abrir edicion masiva o flujos operativos mas sensibles.
- Resultado: NG ya puede persistir puntos de stock por sucursal para productos de empresa; la matriz de stock recalcula `BAJO`, `SIN STOCK`, `NEGATIVO` y diferencias usando esos valores cuando existen, y si todavia no hay configuracion especifica mantiene el valor general actual como fallback seguro.

- Cambio: se agrego base NG de `punto stock por sucursal` en la edicion de productos, manteniendo `puntoStock` general como respaldo compatible.
- Archivos o modulos afectados: `src/CarniSys.NG.Application`, `src/CarniSys.NG.Infrastructure`, `src/CarniSys.NG.Web`.
- Motivo: continuar el vertical de `Stock` con el menor impacto posible sobre el comportamiento legacy ya estable, antes de abrir edicion masiva o flujos operativos mas sensibles.
- Resultado: NG ya puede persistir puntos de stock por sucursal para productos de empresa; la matriz de stock recalcula `BAJO`, `SIN STOCK`, `NEGATIVO` y diferencias usando esos valores cuando existen, y si todavia no hay configuracion especifica mantiene el valor general actual como fallback seguro.

- Cambio: se agrego `Stock` NG en modo consulta con matriz de existencia por sucursales y detalle por producto.
- Archivos o modulos afectados: `src/CarniSys.NG.Application`, `src/CarniSys.NG.Infrastructure`, `src/CarniSys.NG.Web`.
- Motivo: continuar con el proximo vertical funcional usando el stored procedure legacy ya existente, sin abrir todavia ingresos, egresos, cierres ni ajustes.
- Resultado: NG ya permite consultar stock actual por producto y sucursal con filtro por texto, tipo, estado y fecha; mantiene fuera de alcance cualquier escritura operativa del modulo.

## 2026-07-10

- Cambio: se agrego la base de `Etapa 1` para `CarniSys NG` con modelos y contratos minimos de autenticacion, empresa, sucursal y permisos.
- Archivos o modulos afectados: `src/CarniSys.NG.Domain`, `src/CarniSys.NG.Application`, `src/CarniSys.NG.Infrastructure`, `src/CarniSys.NG.Web`.
- Motivo: continuar despues del scaffold inicial con un avance chico, compilable y alineado al plan tecnico.
- Resultado: `src/CarniSys.NG.Web` compila con servicios placeholder y reglas base reutilizables, sin tocar el comportamiento del sistema legacy.

## 2026-07-10

- Cambio: se agrego autenticacion por cookie, pantalla de login NG, claims de empresa/sucursal/permisos y proteccion de la home autenticada.
- Archivos o modulos afectados: `src/CarniSys.NG.Infrastructure`, `src/CarniSys.NG.Web`.
- Motivo: avanzar la `Etapa 1` sin introducir todavia dependencias estructurales al sistema legacy.
- Resultado: `src/CarniSys.NG.Web` compila con flujo de ingreso y sesion autenticada listos para reemplazar el backend placeholder por autenticacion real.

## 2026-07-10

- Cambio: se reemplazo el login placeholder por un adaptador SQL propio en NG contra `Usuarios`, `Empresas`, `Sucursal` y `PermisosUsuarios`, sin referenciar proyectos legacy.
- Archivos o modulos afectados: `src/CarniSys.NG.Infrastructure`, `src/CarniSys.NG.Web`.
- Motivo: avanzar a autenticacion real manteniendo la arquitectura NG desacoplada del legacy.
- Resultado: el proyecto compila con autenticacion real configurable y manejo controlado de errores de conexion; la validacion contra la base legacy quedo limitada por accesibilidad del SQL configurado en `Web.config`.

## 2026-07-10

- Cambio: se agrego autorizacion reutilizable por permiso legacy y pantallas NG de contexto para validar lectura y edicion sin tocar modulos productivos.
- Archivos o modulos afectados: `src/CarniSys.NG.Web`.
- Motivo: dejar una base de permisos reutilizable antes de bajar a un modulo real.
- Resultado: `Home` ahora muestra empresa, sucursal y resumen de permisos; `PermissionsController` valida permisos reales con filtros NG y el proyecto compila sin warnings.

## 2026-07-10

- Cambio: se agrego el primer modulo vertical NG en solo lectura para `Productos`, consumiendo `Corte` con filtro por empresa y permiso real de consulta.
- Archivos o modulos afectados: `src/CarniSys.NG.Application`, `src/CarniSys.NG.Infrastructure`, `src/CarniSys.NG.Web`.
- Motivo: validar de punta a punta autenticacion, empresa y permisos sobre una pantalla real sin riesgo de escritura.
- Resultado: `ProductsController` y su vista ya funcionan como base segura para avanzar luego a detalle o edicion, con compilacion correcta.

## 2026-07-10

- Cambio: se agrego detalle de producto NG en solo lectura, enlazado desde el listado y protegido por el mismo permiso legacy de consulta.
- Archivos o modulos afectados: `src/CarniSys.NG.Application`, `src/CarniSys.NG.Infrastructure`, `src/CarniSys.NG.Web`.
- Motivo: consolidar el primer modulo vertical antes de habilitar cualquier escritura.
- Resultado: `Productos` ya cubre listado y ficha de detalle sin tocar el flujo de edicion legacy, con compilacion correcta.

## 2026-07-10

- Cambio: se habilito una edicion minima y controlada de producto NG sobre campos directos del registro `Corte` de la empresa actual.
- Archivos o modulos afectados: `src/CarniSys.NG.Application`, `src/CarniSys.NG.Infrastructure`, `src/CarniSys.NG.Web`.
- Motivo: abrir la primera escritura en NG con alcance chico, reversible y sin invocar la logica completa del stored procedure legacy.
- Resultado: NG ya puede editar `precioKg`, `pesable`, `promedio`, `puntoStock`, `enCierreStock`, `habilitado` e `ingresoRapidoEmbutido`, manteniendo fuera de alcance codigo, tipo, marca, corte maestro, IVA y catalogo global.

## 2026-07-10

- Cambio: se agrego `Catalogo Global` NG en solo lectura, con listado y detalle de productos `idEmpresa = 0`.
- Archivos o modulos afectados: `src/CarniSys.NG.Application`, `src/CarniSys.NG.Infrastructure`, `src/CarniSys.NG.Web`.
- Motivo: completar el primer vertical de productos sin habilitar todavia importacion ni escritura sobre registros globales.
- Resultado: NG ya expone productos propios y catalogo global como referencias separadas, preservando la regla de que los globales no se editan ni operan directamente.

## 2026-07-10

- Cambio: se agrego `Personas` NG en solo lectura, con listado y ficha de detalle para registros propios y globales.
- Archivos o modulos afectados: `src/CarniSys.NG.Application`, `src/CarniSys.NG.Infrastructure`, `src/CarniSys.NG.Web`.
- Motivo: abrir un segundo vertical funcional sin tocar escrituras, AFIP ni reglas sensibles del legacy.
- Resultado: NG ya consulta `Personas` filtrando por empresa activa y registros globales; como el legacy no expone un permiso dedicado para este modulo, la pantalla quedo protegida por autenticacion sin inventar un mapeo nuevo.

## 2026-07-10

- Cambio: se agrego `Usuarios` NG en solo lectura, con listado y ficha dentro de la empresa activa usando el permiso legacy real de consulta.
- Archivos o modulos afectados: `src/CarniSys.NG.Application`, `src/CarniSys.NG.Infrastructure`, `src/CarniSys.NG.Web`.
- Motivo: seguir ampliando cobertura funcional con un modulo administrativo de bajo riesgo antes de habilitar altas o ediciones nuevas.
- Resultado: NG ya permite consultar usuarios de la empresa con filtro por activos y detalle basico, reutilizando `FormUsuarios` para autorizacion y manteniendo fuera de alcance la edicion de usuarios y permisos.

## 2026-07-10

- Cambio: se agrego consulta NG de `Permisos de usuario` enlazada desde `Usuarios`, reutilizando la misma grilla conceptual del MVC.
- Archivos o modulos afectados: `src/CarniSys.NG.Application`, `src/CarniSys.NG.Infrastructure`, `src/CarniSys.NG.Web`.
- Motivo: separar correctamente el alcance funcional indicado por negocio: `Permisos de usuario` los administra la empresa, mientras `Sucursales` queda reservada al flujo de super usuario.
- Resultado: NG ya puede mostrar permisos por formulario para un usuario de la empresa activa; el acceso a esta pantalla exige `FormNuevoUsuario` en modo edicion, alineado al control del MVC, y todavia no habilita guardado desde NG.

## 2026-07-10

- Cambio: se habilito el guardado NG de `Permisos de usuario`, manteniendo la misma normalizacion de dias y alcance usada por el MVC.
- Archivos o modulos afectados: `src/CarniSys.NG.Application`, `src/CarniSys.NG.Infrastructure`, `src/CarniSys.NG.Web`.
- Motivo: completar el flujo empresarial de administracion de permisos sin abrir todavia modulos reservados a super usuario como `Sucursales`.
- Resultado: NG ya permite editar y persistir la grilla de permisos por formulario; si el usuario editado es el mismo autenticado, la cookie se refresca para reconstruir sus claims y aplicar los cambios de sesion inmediatamente.

## 2026-07-10

- Cambio: se habilito alta y edicion NG de `Usuarios` con sucursales existentes de la empresa activa.
- Archivos o modulos afectados: `src/CarniSys.NG.Application`, `src/CarniSys.NG.Infrastructure`, `src/CarniSys.NG.Web`.
- Motivo: completar el flujo empresarial de usuarios sin mezclarlo con la administracion global de `Sucursales`, que sigue reservada al super usuario.
- Resultado: NG ya puede crear y modificar usuarios con nombre, login, email, sucursal, clave, admin, activo y login fuera de sucursal; mantiene validaciones equivalentes al MVC, protege la escritura con `FormNuevoUsuario` y refresca la sesion si el usuario modifica sus propios datos.

## 2026-07-10

- Cambio: se agregaron listados NG de `Tipos de producto` para empresa activa y catalogo global, ambos en solo lectura.
- Archivos o modulos afectados: `src/CarniSys.NG.Application`, `src/CarniSys.NG.Infrastructure`, `src/CarniSys.NG.Web`.
- Motivo: seguir ampliando cobertura administrativa con un modulo de productos de bajo riesgo antes de tocar importacion o edicion de tipos.
- Resultado: NG ya muestra la grilla de tipos propios y reservados con `formTiposProducto`, y el catalogo global de tipos con `formAddOrEditTipoProducto`, replicando el alcance actual del MVC sin habilitar todavia importacion, alta ni modificacion de tipos.

## 2026-07-10

- Cambio: se habilito alta y edicion NG de `Tipos de producto` para la empresa actual.
- Archivos o modulos afectados: `src/CarniSys.NG.Application`, `src/CarniSys.NG.Infrastructure`, `src/CarniSys.NG.Web`.
- Motivo: completar el flujo administrativo basico del modulo sin abrir todavia importacion global ni eliminaciones.
- Resultado: NG ya permite crear y modificar tipos propios con `formAddOrEditTipoProducto`, mantiene bloqueada la edicion de tipos reservados del sistema y actualiza tambien el campo `tipo` de `Corte` al renombrar un tipo existente, igual que el MVC actual.

## 2026-07-10

- Cambio: se habilito importacion NG de `Tipos globales` hacia la empresa actual.
- Archivos o modulos afectados: `src/CarniSys.NG.Application`, `src/CarniSys.NG.Infrastructure`, `src/CarniSys.NG.Web`.
- Motivo: completar el flujo aditivo de tipos de producto antes de evaluar borrados o cambios mas sensibles.
- Resultado: NG ya permite seleccionar tipos del catalogo global y agregarlos a la empresa activa, omitiendo los que ya existen y manteniendo transaccion completa ante fallos, en linea con el comportamiento del MVC.

## 2026-07-10

- Cambio: se habilito eliminacion NG de `Tipos de producto` propios de la empresa actual.
- Archivos o modulos afectados: `src/CarniSys.NG.Application`, `src/CarniSys.NG.Infrastructure`, `src/CarniSys.NG.Web`.
- Motivo: cerrar el ciclo basico del modulo manteniendo las mismas restricciones del MVC para tipos reservados y tipos usados por productos.
- Resultado: NG ya permite eliminar tipos no reservados; si existen `Corte` asociados, la operacion se bloquea con mensaje equivalente al legacy y no se permite borrar tipos reservados del sistema.

## 2026-07-10

- Cambio: se agrego `Marcas` NG en solo lectura, con listado y ficha de detalle dentro del area de productos.
- Archivos o modulos afectados: `src/CarniSys.NG.Application`, `src/CarniSys.NG.Infrastructure`, `src/CarniSys.NG.Web`.
- Motivo: seguir ampliando cobertura funcional con un modulo chico y seguro antes de habilitar edicion de marcas, que tiene mas reglas de negocio y advertencias.
- Resultado: NG ya consulta marcas y propietario asociado usando el permiso de lectura de productos (`formCortes`), respetando el filtro por empresa activa o registros globales y sin habilitar todavia altas, cambios ni confirmaciones por similitud.

## 2026-07-10

- Cambio: se habilito alta y edicion NG de `Marcas` dentro del area de productos.
- Archivos o modulos afectados: `src/CarniSys.NG.Application`, `src/CarniSys.NG.Infrastructure`, `src/CarniSys.NG.Web`.
- Motivo: completar el flujo operativo de marcas manteniendo las reglas ya existentes del MVC para nombre, propietario y advertencias de similitud.
- Resultado: NG ya permite crear y modificar marcas con propietario opcional; en edicion, los usuarios no administradores no pueden cambiar el nombre, y si se detectan marcas parecidas se exige confirmacion explicita antes de guardar.

## 2026-07-10

- Cambio: se habilito eliminacion NG de `Marcas` propias de la empresa actual.
- Archivos o modulos afectados: `src/CarniSys.NG.Application`, `src/CarniSys.NG.Infrastructure`, `src/CarniSys.NG.Web`.
- Motivo: cerrar el ciclo basico del modulo de marcas antes de pasar a otro vertical.
- Resultado: NG ya permite eliminar marcas no globales; la operacion se bloquea si la marca tiene compras/ventas asociadas o si esta vinculada a productos/cortes, y no se habilita borrado sobre marcas globales.

## 2026-07-10

- Cambio: se habilito alta y edicion NG de `Personas`, incluyendo consulta de padron AFIP/ARCA por CUIT y autocompletado de datos.
- Archivos o modulos afectados: `src/CarniSys.NG.Application`, `src/CarniSys.NG.Infrastructure`, `src/CarniSys.NG.Web`.
- Motivo: continuar el vertical de personas preservando las reglas del MVC actual antes de pasar a otro modulo administrativo.
- Resultado: NG ya permite crear y modificar personas propias; mantiene bloqueada la edicion de personas globales, respeta la restriccion sobre Razon Social/CUIT/Identificacion cuando la persona tiene compras o ventas y el usuario no es administrador, reutiliza el permiso de `formCtasCtes` solo para la bandera de cuenta corriente, y expone una consulta AFIP/ARCA encapsulada que autocompleta datos si la empresa tiene configurado su certificado y template.

## 2026-07-10

- Cambio: se agrego `Movimientos` NG en modo consulta, con listado filtrable y ficha de detalle con lineas.
- Archivos o modulos afectados: `src/CarniSys.NG.Application`, `src/CarniSys.NG.Infrastructure`, `src/CarniSys.NG.Web`.
- Motivo: avanzar con el siguiente vertical usando un modulo operativo de alcance acotado y bajo riesgo antes de abrir la edicion de movimientos.
- Resultado: NG ya consulta movimientos por sucursal origen/destino y rango de fechas reutilizando `formMovimientos`, muestra cabecera y lineas de cada movimiento, y agrega el acceso al modulo en el menu principal sin tocar todavia guardado, impresion ni carga de lineas.

## 2026-07-10

- Cambio: se habilito alta y edicion NG de `Movimientos`, con ingreso rapido por codigo y cache inicial de productos frecuentes.
- Archivos o modulos afectados: `src/CarniSys.NG.Application`, `src/CarniSys.NG.Infrastructure`, `src/CarniSys.NG.Web`.
- Motivo: cerrar el vertical de movimientos replicando el flujo operativo principal antes de pasar a otro modulo.
- Resultado: NG ya permite crear y modificar movimientos con `formNuevoMovimiento`, guarda cabecera y lineas usando los mismos stored procedures legacy, precarga hasta 1000 productos de codigo `0-999` al abrir el formulario, resuelve primero desde ese cache local y solo consulta al servidor por codigo exacto cuando hace falta; con `Enter` busca de inmediato, si encuentra salta al campo de kilos y si no encuentra deja el foco en el codigo mostrando el mensaje correspondiente.
