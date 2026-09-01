# Gaps de la migración ASP.NET Core

Inventario de divergencias de comportamiento detectadas por el juez de paridad al portar cada módulo a `WebCore`, que todavía no están resueltas. Cada entrada espera una decisión humana; al resolverse, migra a `docs/DECISIONS.md` y se borra de acá.

No confundir con `docs/GAPS.md` (raíz), que es específico de la migración SQL Server → PostgreSQL — alcance distinto, no se mezclan.

---

## Abiertos

### Mensajes de validación built-in de ASP.NET Core en ingles (campos de tipo valor no-nullable)

Detectado: 2026-09-01, juez de paridad sobre `SystemAdministration/EditarEmpresa`. Confirmado como patron transversal a los Modulos 1, 2 y 3 (no exclusivo de Empresas): reaparece igual en `EditarSucursal` (IdSucursal, IdEmpresa, CodPuntoVentaAfip), `EditarUsuario` (Id, IdEmpresa, IdSucursalUser, Admin, Activo, PermitirLoginFueraSucursal), `AltaRapidaEmpresa` (Usuario.Admin, Usuario.Activo, Sucursal.CodPuntoVentaAfip, Empresa.CodigoGenericoCodigo, Empresa.CodigoGenericoIdAlicuotaIva), `Personas/Editar` (IdPersona, IdIva, CtaCte) y `Productos/AddOrEdit` (IdCorte, IdMarca, IdCorteMaestro, Porcentaje, PorcentajeHueso, AlicuotaIva, SiguienteIdEdicion, UltimoProductoContinuoId, RetomarProductoId, CargaContinua, Pesable, Habilitado, IngresoRapidoEmbutido, EnCierreStock, Independiente, PuntoStock, IdAlicuotaIva -- el formulario mas grande portado hasta ahora, mismo patron en 16+ campos). Tambien se confirmo que Core no emite `data-val-number` para NINGUN campo numerico (con o sin Required), a diferencia de MVC5 que siempre lo agrega para `int`/`long`/`int?` -- mismo gap, alcance mas amplio de lo que parecia con el primer campo encontrado.

Para propiedades de tipo valor no-nullable sin `[Required]` explicito (`int`, `long`, `bool`), tanto MVC5 como ASP.NET Core infieren un validador "Required" implicito -- eso coincide. Pero:

- MVC5 (`Web`) muestra el mensaje localizado en español ("El campo IdEmpresa es obligatorio.", "El campo EsRRII es obligatorio.") y ademas agrega un validador cliente-side `data-val-number` para los numericos ("El campo X debe ser un numero.").
- ASP.NET Core (`WebCore`) muestra el mensaje en ingles por defecto ("The IdEmpresa field is required.") y NO agrega `data-val-number`.

Confirmado en 4 campos del mismo formulario (`IdEmpresa`, `EsRRII`, `Activa`, `CodigoGenericoCodigo`) -- supera el umbral de CLAUDE.md §5.1, por eso no se parchea campo por campo.

El fix real es configurar localizacion de ASP.NET Core (`AddDataAnnotationsLocalization` + recursos en español para los mensajes built-in de `ModelBindingMessageProvider`, o interceptar `DefaultModelBindingMessageProvider` con las mismas cadenas que ya usa el framework .NET clasico) -- es una tarea de plataforma (afecta a todo `WebCore`, no a una vista puntual), no algo que se resuelva en el slice de Empresas. El validador cliente-side `data-val-number` faltante no es bloqueante (el server-side model binding igual rechaza un valor no numerico), es una degradacion de UX de validacion en el navegador.

Impacto real: bajo (server-side sigue validando correctamente; solo cambia el idioma del mensaje y se pierde feedback instantaneo en el navegador para 2 campos numericos de todo el modulo). No bloquea seguir con el resto de Módulo 1.

Sin decision tomada todavia sobre cuando encarar la localizacion global -- queda abierto.

### Botones "Buscar en AFIP" (Personas, AltaRapidaEmpresa, modal de alta) no funcionan: el modulo AFIP no esta portado

Detectado: 2026-09-01, al portar `AltaRapidaEmpresa.cshtml` (Modulo 1) y confirmado/ampliado el 2026-09-01 al portar `PersonasController`/`Editar.cshtml`/`_AddOrEditPersonaModal.cshtml` (Modulo 2).

`PersonasController.BuscarPadronAfip`/`BuscarPadronAfipAjax` (original en `Web/Controllers/PersonasController.cs`) dependen de `AFIP.ConsultarPadronService`, que usa los 4 proxies SOAP de AFIP (`AFIP/Web References/*`) -- ese es el bloqueante ya identificado en el plan original de la migracion (seccion "Mini-spike de AFIP", la mayor incertidumbre tecnica del programa completo, todavia no ejecutado). NO se portaron esas 2 acciones ni el metodo privado `BuscarDatosAfipDesdeGuardar` (que ademas es codigo muerto en el original: ninguna accion publica lo llama).

Se portaron los 3 botones "Buscar en AFIP" (Personas/Editar, AltaRapidaEmpresa, el modal de alta rapida de persona) con el mismo markup/JS que el original -- paridad visual OK, confirmada por el juez -- pero sus fetch a `.../BuscarPadronAfipAjax` van a devolver 404 en `WebCore` hasta que el modulo AFIP se porte.

Impacto real: bajo -- los 3 formularios funcionan completos sin ese boton (los campos se completan a mano); solo el autocompletado por AFIP no anda. Se resuelve solo (sin decision aparte) cuando se ejecute el mini-spike de AFIP ya planeado -- es una dependencia de orden, no una ambiguedad nueva.

### Modal de alta rapida de persona (`_AddOrEditPersonaModal.cshtml`) depende de un script compartido con POS/Compras, todavia no portado

Detectado: 2026-09-01, al portar el modulo Personas.

El submit real del modal lo maneja `Web/Scripts/app/persona-buscar.js` (delegado en `document`, intercepta el evento `submit` de `#formPersonaModal` y hace `$.ajax` a `window.api.persona.guardarCrear`) -- ese script y el objeto global `window.api` son compartidos con POS/Compras (Modulo 8, todavia no portado). El `<form>` de la vista se porto con `action=""` identico al original (no se lo hizo apuntar a `GuardarPersonaModal` via `asp-action`, porque eso cambiaria el HTML respecto al original). Sin `persona-buscar.js` cargado, el modal en `WebCore` no tiene submit-handler propio -- mismo comportamiento que tendria el HTML original si se sirviera sin ese script, no es una regresion introducida por la migracion.

Impacto real: bajo -- el modal es un componente consumido desde POS/Compras, no desde el modulo Personas en si (`PersonasController.PersonaModal`/`GuardarPersonaModal` ya existen y funcionan via POST directo, probado por el juez). Se resuelve solo cuando se porte el modulo que lo consume -- no requiere decision aparte.

### Bootstrap 5 (WebCore) vs. API jQuery de Bootstrap 4 (JS portado tal cual) -- `$(...).modal is not a function`

Detectado: 2026-09-01, juez de paridad sobre `Stock/Editar` (Modulo 4). Confirmado que ya estaba presente sin documentar desde Modulo 3 (`Productos/AddOrEdit.cshtml` llama `$("#modalInfoIndependiente").modal("show")`/`$("#modalMarcas").modal("hide")`, mismo patron).

`WebCore/wwwroot/lib/bootstrap` es la version 5.3.3 -- Bootstrap 5 elimino la dependencia de jQuery y con ella el plugin `$.fn.modal`/`$.fn.tooltip`/etc. Pero todo el JS portado sin cambios desde `Web/Scripts/` (son assets 100% client-side, se copian tal cual, ver plan original de la migracion) fue escrito contra Bootstrap 4 y usa esa API jQuery en varios puntos (`stock.js`, `Productos/AddOrEdit.cshtml` inline, probablemente mas vistas con modales ya portadas que no se revisaron una por una).

Confirmado con Playwright en `Stock/Editar`: aparece un `pageerror` real en consola (`$(...).modal is not a function`) al cargar la pagina. Pese al error, el modal de "Buscar producto" (F10) igual termina abriendose visualmente (Bootstrap 5 tambien reacciona en paralelo a los atributos `data-bs-toggle`/eventos nativos, asi que el flujo principal probado no quedo bloqueado) -- pero no se probo cada modal de cada vista ya portada, puede haber casos donde el error SI corte la ejecucion del resto de un mismo bloque `<script>` (un error no capturado detiene las sentencias siguientes del mismo IIFE, no las de otros `<script>` tags).

El fix real es una decision de plataforma, no algo que se resuelva en una vista puntual: o se baja `WebCore` a Bootstrap 4.x (mismo major que sigue usando `Web`, cero cambios en el JS portado), o se agrega un shim/polyfill que reimplemente `$.fn.modal` sobre la API nativa de Bootstrap 5, o se reescribe cada uso de `.modal()`/`.tooltip()`/etc. a la API nativa (`bootstrap.Modal.getOrCreateInstance(el).show()`) en cada script portado (mas trabajo, tantas veces como aparezca el patron).

Impacto real: bajo por ahora en lo probado (el flujo principal de agregar productos en Stock sigue funcionando), pero sin auditar exhaustivamente. Sin decision tomada todavia sobre cual de las 3 opciones tomar -- queda abierto.

### `table-scroll-sync.js` (scrollbar flotante para tablas anchas) no cargado en WebCore

Detectado: 2026-09-01, juez de paridad sobre `Stock/Lineas` (Modulo 4). Misma causa raiz que el gap de alertas de TempData (mas abajo): `Web/Views/Shared/_LayoutBase.cshtml` carga `~/Scripts/app/table-scroll-sync.js` globalmente (linea 685) para TODA vista con `.table-responsive.js-sync-scroll-body` -- envuelve la tabla en un `<div class="sync-scroll-host">` con una barra de scroll flotante (`sync-scroll-floating`). El `_Layout.cshtml` minimo de `WebCore` no lo carga, asi que esas tablas se ven sin la barra flotante -- probablemente afecta a TODAS las vistas ya portadas con tablas anchas (`Productos/Index`, `Stock/Index`, etc.), no solo `Lineas`, aunque no se audito cada una.

Confirmado con el juez de paridad que es puramente un wrapper de UI agregado por JS en tiempo de ejecucion (no server-side): el contenido real de las celdas, resumenes y badges de `Stock/Lineas` es identico byte a byte entre `Web` y `WebCore` una vez que se descarta ese wrapper -- no es una discrepancia de datos.

Impacto real: bajo -- en pantallas anchas la tabla sigue siendo usable con el scroll nativo del navegador, solo se pierde la barra flotante de conveniencia en mobile/tablet. Mismo tipo de decision que el gap de alertas (portar el mecanismo global de `_LayoutBase.cshtml`, no ad-hoc por vista) -- sin decision tomada todavia sobre cuando portarlo.

### Alertas de TempData (`AlertType`/`AlertTitle`/`AlertMsg`) no se muestran en WebCore

Detectado: 2026-09-01, patron transversal confirmado en Modulo 1 y Modulo 2 (no es nuevo de Personas, ya estaba presente sin documentar desde `SystemAdministrationController.GuardarEmpresa`/`GuardarSucursal`/`GuardarUsuario`/`GuardarAltaRapidaEmpresa`).

`Web/Views/Shared/_LayoutBase.cshtml` (el layout real de `Web`) lee `TempData["AlertType"]`/`AlertTitle`/`AlertMsg` despues de cada redirect y renderiza un toast/alert. `WebCore/Views/Shared/_Layout.cshtml` es el scaffold minimo default de `dotnet new mvc` -- no lee ni renderiza esas claves. Todo controller portado hasta ahora sigue seteando esas 3 claves de TempData (fidelidad de logica), pero en `WebCore` no se ve nada tras un guardado exitoso o un error de redireccion.

Impacto real: bajo para el juez de paridad de HTML puro (compara la pagina despues del redirect, sin el toast en ninguno de los 2 lados si se navega directo a la URL destino) pero SI afecta la experiencia real de uso (guardar algo en `WebCore` hoy no da ningun feedback visual). Es una tarea de plataforma (portar el mecanismo de alertas de `_LayoutBase.cshtml`, no solo copiar 3 lineas) -- no se resuelve ad-hoc en un modulo puntual.

Sin decision tomada todavia sobre cuando portar el layout completo -- queda abierto.

### Diferencias cosmeticas de encoding HTML/JSON entre motores, sin impacto real (line endings y entidades numericas)

Detectado: 2026-09-01, juez de paridad sobre `Productos/VerGlobales`/`BuscarGlobales`/`VerGlobalesTiposProducto`/`BuscarGlobalesTiposProducto`, con datos reales (50 productos, nombres con "ñ"/"ó" como "Riñón"/"picaña", HTML multilinea con varios atributos `data-*` por linea).

Dos diferencias puramente de bajo nivel entre el motor de renderizado de MVC5 (`System.Web`) y ASP.NET Core, confirmadas SIN impacto funcional (mismo resultado visual y mismo parseo en el navegador/jQuery en ambos casos):

- **Line endings dentro de atributos HTML multilinea**: MVC5 emite `\r\n` entre atributos de un mismo tag cuando el `.cshtml` fuente tiene el tag partido en varias lineas; ASP.NET Core emite solo `\n`. Ejemplo real: `<tr data-id="73"\r\n data-importado="1"...>` (MVC5) vs `<tr data-id="73"\n data-importado="1"...>` (Core). HTML/DOM/jQuery tratan ambos identico.
- **Entidades HTML numericas para caracteres no-ASCII**: MVC5 usa la forma decimal (`&#243;` para "ó", `&#241;` para "ñ"); ASP.NET Core usa la forma hexadecimal (`&#xF3;`, `&#xF1;`). Mismo caracter Unicode, misma renderizacion visual -- solo cambia la representacion textual del entity en el HTML fuente.

Ambas diferencias son consistentes en TODO el HTML generado por cada framework (no son un bug puntual de una vista) -- es de esperar que reaparezcan en cualquier vista futura portada que tenga atributos HTML multilinea o texto con acentos/eñes. No requieren fix: no hay forma de hacer que ASP.NET Core replique byte a byte el encoding de MVC5 sin post-procesar cada respuesta (costo/beneficio no lo justifica, cero impacto real). Documentado para que el juez de paridad de futuros modulos no las reporte como falsos positivos -- normalizar `\r\n`→`\n` y las entidades numericas antes de comparar, como ya se hace con el token antiforgery.

Impacto real: ninguno. No requiere decision -- queda documentado como ruido esperado del metodo de comparacion, no como deuda pendiente.

**Actualizacion 2026-09-01 (Modulo 4, Stock)**: se confirmo un tercer caso del mismo tipo -- MVC5 emite `<option value="X" selected="">` (atributo booleano vacio) para la opcion seleccionada de un `<select>`, ASP.NET Core emite `<option value="X" selected="selected">` (valor explicito). Mismo caracter de efecto (el navegador marca la opcion como seleccionada en ambos casos), agregado a la lista de diferencias de bajo nivel a ignorar en el juez de paridad.

**Actualizacion 2026-09-01 (Modulo 4, Stock/BuscarExistenciaPorSucursales)**: cuarto caso -- el `HtmlEncoder` default de ASP.NET Core codifica el caracter `+` como entidad hexadecimal (`&#x2B;`) cuando aparece en texto plano de una vista (ej. `@(signoDif + dif.ToString("N3"))` en `_TablaExistenciaPorSucursales.cshtml`, el signo de una diferencia positiva). MVC5 no lo codifica, lo emite literal (`+40,000`). Mismo caracter final una vez que el navegador decodifica la entidad -- confirmado sin impacto visual. Agregado a la lista de diferencias de bajo nivel a ignorar en el juez de paridad (decodificar `&#x2B;`->`+` antes de comparar, ademas de `\r\n`->`\n` y las entidades numericas de acentos/eñes).

### Quinto caso de diferencia cosmetica de encoding: JSON con `\uXXXX` en Core vs caracter literal en MVC5

Detectado: 2026-09-01, prueba en vivo de `GenerarAjustePesaje` (Modulo 4, Stock). El `JsonResult` de MVC5 (`System.Web.Mvc`) serializa el mensaje "El Ajuste de Stock se realizó correctamente." con el caracter "ó" literal en UTF-8. El `IActionResult`/`Json()` de ASP.NET Core (`System.Text.Json` por default) serializa el mismo string escapando el caracter como `ó` (`"realizó"`). Mismo valor decodificado, ambos formatos son JSON valido y cualquier `JSON.parse` (el navegador, `$.ajax`, `fetch().then(r=>r.json())`) los interpreta identico -- confirmado sin impacto real. Se agrega como una variante mas del mismo gap de encoding ya documentado arriba (ahi eran diferencias en HTML, esta es la misma clase de diferencia pero en JSON) -- normalizar decodificando escapes Unicode antes de comparar JSON crudo como texto.

Impacto real: ninguno.

### Sexto caso de diferencia cosmetica: atributo `value` presente (vacio) vs ausente segun `null` vs `""`

Detectado: 2026-09-01, juez de paridad sobre `Compras/NuevaCompra` (Modulo 5). Razor (tanto MVC5 como Core, no es un cambio de Core) omite un atributo HTML completo cuando su valor viene de una unica expresion `@algo` que evalua a `null` (ej. `value="@Model.ProveedorNombre"` con `Model.ProveedorNombre == null` -> no se emite `value` en absoluto). El original nunca asigna `ProveedorNombre` en `CrearViewModelNuevo` (queda en el default `null` de `Web.Models.CompraEditVm`), pero el modelo portado a `WebCore.Models.CompraEditVm` inicializa todos los `string` con `= ""` (convencion ya usada en toda esta migracion para evitar warnings de nullable reference types) -- con cadena vacia (no `null`), Razor SI emite el atributo (`value=""`).

Confirmado sin impacto real: un `<input>` de texto se renderiza identico (vacio) con o sin el atributo `value`. Se eligio no reestructurar el modelo a `string?` con nulls explicitos en todos lados solo para igualar este detalle -- rompería la convencion de NRT-safety ya aplicada de forma consistente en decenas de modelos de esta migracion, a cambio de cero beneficio observable. Agregado a la lista de diferencias de bajo nivel a ignorar en el juez de paridad (normalizar comparando solo si el `<input>` tiene contenido visible, no la presencia literal del atributo `value=""`).

Impacto real: ninguno.

### Diferencia numerica real (no cosmetica) encontrada en `BuscarExistenciaPorSucursales`: redondeo de punto flotante distinto en un SUM agregado

Detectado: 2026-09-01, juez de paridad sobre `Stock/BuscarExistenciaPorSucursales` (Modulo 4) con datos reales (1367 valores numericos comparados). De esos 1367, exactamente 4 difieren -- los 4 derivados del mismo agregado: `Web` clasico devuelve consistentemente `-40.611,550` (StockActual de un producto/sucursal puntual con muchisimas lineas de movimientos acumuladas) y `WebCore` devuelve consistentemente `-40.611,551` -- una diferencia de 0,001 en un valor de ~40611,55. Confirmado que **no es un valor flaky/no determinista**: se repitio la consulta 3 veces seguidas contra cada motor por separado y cada uno devuelve siempre el mismo numero (distinto entre si, pero estable dentro de cada motor).

La logica que calcula esto (`Negocio.Corte.ObtenerMatrizExistenciaPorSucursales` / `Datos.Corte`) es **codigo 100% compartido** entre `Web` (compilado `net472`) y `WebCore` (compilado `net10.0`) -- mismo archivo fuente, no hay una linea de logica de negocio distinta entre ambos. La unica diferencia real de bajo nivel entre los dos TFMs en esta capa, ya documentada y aceptada como riesgo calculado en el plan original de la migracion, es el swap condicional `System.Data.SqlClient` (net472) -> `Microsoft.Data.SqlClient` (net10.0). Hipotesis mas probable (no verificada a fondo, ver limite de esta entrada): una diferencia de redondeo entre como cada driver deserializa/acumula un `SUM` de columnas `float`/`real` de SQL Server, o el motor de SQL Server elige un plan de ejecucion/orden de agregacion distinto entre ambas conexiones (el `SUM` de `float` en SQL Server no garantiza el mismo orden de suma entre ejecuciones con planes distintos, y eso SI puede cambiar el ultimo digito de precision).

No se investigo mas a fondo (cambiar el driver o el tipo de dato de la columna es una decision de plataforma que excede el alcance de un slice de Stock, y el impacto es minimo: 0,001 sobre ~40611 en 1 de miles de celdas). Impacto real: bajo -- no cambia el signo, la magnitud, ni el estado (NEGATIVO/BAJO/OK) de ningun producto; es un ultimo-digito de redondeo en un reporte de consulta. Sin decision tomada sobre si vale la pena investigarlo mas -- queda abierto. **Distinto de los gaps cosmeticos de arriba**: este SI es un numero diferente, no solo una representacion distinta del mismo numero -- se documenta aparte a proposito.

**Actualizacion 2026-09-01 (Modulo 5, Compras)**: se confirmo el MISMO patron una segunda vez, ahora en `ComprasController.Index` -- el total agregado (`CalcularTotalImporte`, un `SUM` de la columna `totalS` sobre todas las compras del filtro) dio `3.727.390,00` en `Web` clasico y `3.727.389,50` en `WebCore`, ambos estables y reproducibles (2 corridas seguidas contra cada motor, mismo resultado cada vez). Las 252 celdas individuales de la tabla (`<td>`) son identicas byte a byte entre ambos motores -- la diferencia esta solo en el total acumulado, confirmando que es un problema de agregacion/redondeo de `SUM` sobre `float`/`real`, no un dato mal leido fila por fila. Con 2 apariciones independientes (Modulo 4 y Modulo 5, ambas en un `SUM` de una columna `float` proveniente del mismo tipo de query de `Negocio.Compra`/`Negocio.Corte`), la hipotesis del swap `System.Data.SqlClient`/`Microsoft.Data.SqlClient` (o la falta de determinismo de SQL Server al sumar `float` segun el plan de ejecucion elegido) se refuerza -- **es un patron esperable en cualquier total/subtotal agregado con `SUM` sobre columnas `float`/`real` en el resto de los modulos todavia por portar**, no un caso aislado de Stock. Se mantiene la misma decision: impacto minimo (ultimo o penultimo digito de un total grande), no se investiga ni se corrige a nivel de plataforma dentro de un slice de modulo -- si vuelve a aparecer una tercera vez, corresponderia escalarlo a una decision de plataforma real (CLAUDE.md §5.1).
