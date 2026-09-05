# Gaps de la migración ASP.NET Core

Inventario de divergencias de comportamiento detectadas por el juez de paridad al portar cada módulo a `WebCore`, que todavía no están resueltas. Cada entrada espera una decisión humana; al resolverse, migra a `docs/DECISIONS.md` y se borra de acá.

No confundir con `docs/GAPS.md` (raíz), que es específico de la migración SQL Server → PostgreSQL — alcance distinto, no se mezclan.

---

## Abiertos

### Permisos reales de venta y "usuario producción" — bypaseados en WebCore, portar cuando haya login/sesión real

Detectado: 2026-09-04, batch 7 POS UI, iteración 5 (edición de venta existente). Pedido explícito del usuario: **"ESTOS PERMISOS SON PROPIOS DE LA APP, Y MUY IMPORTANTES. ANOTALO PARA MAS ADELANTE PORQUE NECESITAMOS PORTAR ESTAS REGLAS, AL IGUAL QUE UN USUARIO 'PRODUCCION'"**.

`Web/Controllers/VentasController.cs` (clásico) protege la edición de una venta con reglas reales:

- `PuedeModificarUltimaVenta` (2521-2542): admin, o la venta es literalmente `getUltimaVentaVendedor(cierre)` del cierre de caja actual del vendedor + permiso `Permisos.Venta.UltimaVenta` vigente.
- `PuedeCambiarFormaPago` (2618-2643): admin, o misma sucursal + mismo vendedor del cierre actual + `FechaVenta` dentro del rango del cierre.
- `PuedeEditarFechaVenta`: permiso `Permisos.Venta.NuevaVenta` sobre la fecha del vendedor — más estricto que editar productos.
- **"Usuario de producción"** (cuenta compartida de POS): `user.EsUsuarioProduccion` + `PermisosHelper.RegistrarOperadorPOS/ObtenerOperadorPOS(Session, posInstanceId)` — pide login real de un empleado (usuario+contraseña) antes de operar cada instancia de POS; el operador autorizado gobierna permisos finos (Bonificar, editar/anular última venta) dentro de esa pestaña. Ver `Web/Views/Ventas/POS.cshtml:782-808` y `docs/DECISIONS.md` 2026-08-27/28.

`WebCore` corre con un usuario único hardcodeado (`_usuarioActual`, `Id=2/Admin=true/IdEmpresa=1/IdSucursal=2`, sin `Session`) — todo el sistema de permisos de la migración resuelve siempre "permitido" (mismo criterio en Cajas/Finanzas/Reportes/Ventas). Concretamente hoy:

- `ViewBag.PuedeModificarVenta = true` siempre (`VentasController.DetalleVenta`) — el botón "Modificar venta" aparece para cualquier venta, sin importar cierre de caja ni vendedor.
- `ModificarVenta`/`POS` en WebCore no validan caja abierta del vendedor ni sucursal del request contra el usuario (documentado en el header de `VentasController.cs`).
- "Cambiar Forma de Pago" (modo `soloFormaPago`) directamente no está portado.
- No existe ningún concepto de "usuario producción" ni login de operador por pestaña en WebCore.

**No se resuelve ahora** porque no hay sistema de login/sesión real en WebCore todavía (diseño deliberado del spike inicial, ver plan de migración) — portar estas reglas sin eso no se puede probar de verdad (no hay multi-usuario). Queda anotado para cuando se diseñe autenticación real: en ese momento, portar `PuedeModificarUltimaVenta`/`PuedeCambiarFormaPago`/`PuedeEditarFechaVenta`/`TienePermisoAdministrativoSobreVenta` y la feature completa de "usuario producción" (login de operador por instancia de POS) juntas, no por separado — están relacionadas (mismo helper `PermisosHelper`, misma pantalla).

Impacto real: alto una vez que haya usuarios reales — sin esto, cualquier usuario puede editar cualquier venta sin restricción de caja/turno, y no hay forma de compartir una PC de POS entre varios empleados con auditoría real de quién operó qué. Hoy (usuario único de desarrollo) no bloquea nada.


### Mensajes de validación built-in de ASP.NET Core en ingles (campos de tipo valor no-nullable)

Detectado: 2026-09-01, juez de paridad sobre `SystemAdministration/EditarEmpresa`. Confirmado como patron transversal a los Modulos 1, 2 y 3 (no exclusivo de Empresas): reaparece igual en `EditarSucursal` (IdSucursal, IdEmpresa, CodPuntoVentaAfip), `EditarUsuario` (Id, IdEmpresa, IdSucursalUser, Admin, Activo, PermitirLoginFueraSucursal), `AltaRapidaEmpresa` (Usuario.Admin, Usuario.Activo, Sucursal.CodPuntoVentaAfip, Empresa.CodigoGenericoCodigo, Empresa.CodigoGenericoIdAlicuotaIva), `Personas/Editar` (IdPersona, IdIva, CtaCte) y `Productos/AddOrEdit` (IdCorte, IdMarca, IdCorteMaestro, Porcentaje, PorcentajeHueso, AlicuotaIva, SiguienteIdEdicion, UltimoProductoContinuoId, RetomarProductoId, CargaContinua, Pesable, Habilitado, IngresoRapidoEmbutido, EnCierreStock, Independiente, PuntoStock, IdAlicuotaIva -- el formulario mas grande portado hasta ahora, mismo patron en 16+ campos). Tambien se confirmo que Core no emite `data-val-number` para NINGUN campo numerico (con o sin Required), a diferencia de MVC5 que siempre lo agrega para `int`/`long`/`int?` -- mismo gap, alcance mas amplio de lo que parecia con el primer campo encontrado.

Para propiedades de tipo valor no-nullable sin `[Required]` explicito (`int`, `long`, `bool`), tanto MVC5 como ASP.NET Core infieren un validador "Required" implicito -- eso coincide. Pero:

- MVC5 (`Web`) muestra el mensaje localizado en español ("El campo IdEmpresa es obligatorio.", "El campo EsRRII es obligatorio.") y ademas agrega un validador cliente-side `data-val-number` para los numericos ("El campo X debe ser un numero.").
- ASP.NET Core (`WebCore`) muestra el mensaje en ingles por defecto ("The IdEmpresa field is required.") y NO agrega `data-val-number`.

Confirmado en 4 campos del mismo formulario (`IdEmpresa`, `EsRRII`, `Activa`, `CodigoGenericoCodigo`) -- supera el umbral de CLAUDE.md §5.1, por eso no se parchea campo por campo.

El fix real es configurar localizacion de ASP.NET Core (`AddDataAnnotationsLocalization` + recursos en español para los mensajes built-in de `ModelBindingMessageProvider`, o interceptar `DefaultModelBindingMessageProvider` con las mismas cadenas que ya usa el framework .NET clasico) -- es una tarea de plataforma (afecta a todo `WebCore`, no a una vista puntual), no algo que se resuelva en el slice de Empresas. El validador cliente-side `data-val-number` faltante no es bloqueante (el server-side model binding igual rechaza un valor no numerico), es una degradacion de UX de validacion en el navegador.

Impacto real: bajo (server-side sigue validando correctamente; solo cambia el idioma del mensaje y se pierde feedback instantaneo en el navegador para 2 campos numericos de todo el modulo). No bloquea seguir con el resto de Módulo 1.

Sin decision tomada todavia sobre cuando encarar la localizacion global -- queda abierto.

**Investigado 2026-09-05 (sin resolver, revertido)**: se probaron 2 enfoques, ninguno cerro el gap de forma confiable:
1. `app.UseRequestLocalization(es-AR)` -- **no tuvo ningun efecto** en el mensaje (seguia en ingles) para el caso de un campo simplemente ausente del POST (bindea a su default 0/false sin error). **Revertido ademas por un riesgo real detectado a tiempo, no solo por no funcionar**: cambiar la cultura del request afecta tambien el *model binding* automatico de cualquier `decimal`/`double`/`DateTime` que no pase por los parsers manuales ya usados en toda esta migracion (`ParseFloat`, etc., adoptados justamente para evitar depender de la cultura del hilo) -- un cambio de plataforma con blast radius mucho mayor que el bug cosmetico que se queria arreglar, sin forma de auditar cada endpoint afectado en un tiempo razonable.
2. `options.ModelBindingMessageProvider.SetValueMustNotBeNullAccessor(...)` (segun CLAUDE.md §1.3 hay que consultar la doc real de la firma exacta antes de usarla, no adivinar de memoria -- no se tenia acceso a esa doc en esta sesion) -- SI se disparo para un campo numerico posteado con string vacio (confirma que existe una via real para arreglar esto), pero el parametro recibido por el accessor no era el nombre del campo (dio `"El campo  es obligatorio."`, con el nombre vacio) -- signo de haber elegido el accessor/parametro equivocado por adivinar la API en vez de verificarla. Revertido en vez de seguir iterando a ciegas.

**Para la proxima vez que se encare**: confirmar primero, con la documentacion real de `DefaultModelBindingMessageProvider` (via Context7 u otra fuente oficial, CLAUDE.md §1.3), cual accessor exacto corresponde a "campo no-nullable ausente del POST, sin `[Required]` explicito" y que parametro recibe -- y verificar aparte, con un test dedicado que cubra varios endpoints con decimales/fechas, que fijar la cultura del request no cambia ningun resultado de parsing ya validado en este programa.

**Investigado 2026-09-05, segunda pasada (cerrado como "sin fix de plataforma viable sin nueva dependencia" -- decision pendiente del usuario)**: con WebSearch/WebFetch ya disponibles, se confirmo la documentacion real de punta a punta:

1. `DefaultModelBindingMessageProvider.ValueMustNotBeNullAccessor` (el accessor que SI se disparaba en el intento anterior) tiene como texto default real **"The value '{0}' is invalid."** (confirmado via `learn.microsoft.com/.../defaultmodelbindingmessageprovider.valuemustnotbenullaccessor`) -- el `{0}` es el VALOR intentado (string vacio en el repro), no el nombre del campo. Esto explica el bug del intento anterior (`"El campo  es obligatorio."` con hueco vacio: se asumio que `{0}` era el nombre de campo, y ademas ese accessor ni siquiera es el que produce el texto "The X field is required." que se queria traducir -- es un mensaje distinto, de una situacion de binding distinta.
2. El texto real "The {0} field is required." **no sale de `DefaultModelBindingMessageProvider` en absoluto** -- es el default de `System.ComponentModel.DataAnnotations.RequiredAttribute` (la validacion implicita que ASP.NET Core agrega para todo tipo valor no-nullable), confirmado via el issue oficial `dotnet/runtime#24084` ("Provide localization for default error messages in System.ComponentModel.Annotations"): **.NET solo trae el recurso de este mensaje en ingles** -- no hay satelite en espanol ni ningun mecanismo que lo traduzca cambiando la cultura del request (por eso el intento 1, `UseRequestLocalization`, no tuvo ningun efecto: no es un problema de cultura, es que el string en si no esta traducido en el framework).
3. Alternativas reales, ninguna es un "flip de un config":
   - **Manual**: `[Required(ErrorMessage = "...")]` explicito en cada propiedad afectada -- no es un fix de plataforma, es tocar cada DTO/ViewModel uno por uno (decenas de campos ya relevados en este mismo gap).
   - **Recurso propio**: registrar un `IStringLocalizer`/resx propio e interceptarlo via un `IValidationAttributeAdapterProvider` custom -- viable pero es una pieza de arquitectura nueva (no trivial), no una config de una linea.
   - **Paquete de terceros** (`Toolbelt.ComponentModel.Annotations.Resources`, aporta el satelite en espanol para los attributes de `System.ComponentModel.Annotations` via reflection sobre areas no documentadas del framework) -- resuelve esto en una linea (`AddSystemComponentModelAnnotationsLocalization()`), pero es una dependencia nueva que exige pasar por CLAUDE.md §1.2 (comparar contra alternativa OSS -- la alternativa acá es la opción "manual" de arriba -- y esperar confirmación explícita antes de instalar), y el propio autor advierte que usa reflection sobre superficie no documentada (puede romper en futuras versiones de .NET).

**Cierre de esta investigacion**: no hay una solucion de plataforma de bajo costo y bajo riesgo -- las 3 opciones reales tienen costo (tocar cada campo, construir una pieza nueva, o sumar una dependencia con riesgo de mantenimiento). Dado que el impacto real ya esta documentado como bajo (solo cambia el idioma del mensaje, el server-side sigue validando bien), **queda sin resolver a proposito, pendiente de que el usuario decida cual de las 3 alternativas prefiere** (o si prefiere seguir sin resolverlo) -- no se elige una unilateralmente por ser una decision de plataforma (CLAUDE.md §0/§3), no un bug puntual.

### Botones "Buscar en AFIP" (Personas, AltaRapidaEmpresa, modal de alta) no funcionan: el modulo AFIP no esta portado

Detectado: 2026-09-01, al portar `AltaRapidaEmpresa.cshtml` (Modulo 1) y confirmado/ampliado el 2026-09-01 al portar `PersonasController`/`Editar.cshtml`/`_AddOrEditPersonaModal.cshtml` (Modulo 2).

`PersonasController.BuscarPadronAfip`/`BuscarPadronAfipAjax` (original en `Web/Controllers/PersonasController.cs`) dependen de `AFIP.ConsultarPadronService`, que usa los 4 proxies SOAP de AFIP (`AFIP/Web References/*`) -- ese es el bloqueante ya identificado en el plan original de la migracion (seccion "Mini-spike de AFIP", la mayor incertidumbre tecnica del programa completo, todavia no ejecutado). NO se portaron esas 2 acciones ni el metodo privado `BuscarDatosAfipDesdeGuardar` (que ademas es codigo muerto en el original: ninguna accion publica lo llama).

Se portaron los 3 botones "Buscar en AFIP" (Personas/Editar, AltaRapidaEmpresa, el modal de alta rapida de persona) con el mismo markup/JS que el original -- paridad visual OK, confirmada por el juez -- pero sus fetch a `.../BuscarPadronAfipAjax` van a devolver 404 en `WebCore` hasta que el modulo AFIP se porte.

Impacto real: bajo -- los 3 formularios funcionan completos sin ese boton (los campos se completan a mano); solo el autocompletado por AFIP no anda. Se resuelve solo (sin decision aparte) cuando se ejecute el mini-spike de AFIP ya planeado -- es una dependencia de orden, no una ambiguedad nueva.

### Modal de alta rapida de persona (`_AddOrEditPersonaModal.cshtml`) depende de un script compartido con POS/Compras, todavia no portado

Detectado: 2026-09-01, al portar el modulo Personas.

El submit real del modal lo maneja `Web/Scripts/app/persona-buscar.js` (delegado en `document`, intercepta el evento `submit` de `#formPersonaModal` y hace `$.ajax` a `window.api.persona.guardarCrear`) -- ese script y el objeto global `window.api` son compartidos con POS/Compras (Modulo 8, todavia no portado). El `<form>` de la vista se porto con `action=""` identico al original (no se lo hizo apuntar a `GuardarPersonaModal` via `asp-action`, porque eso cambiaria el HTML respecto al original). Sin `persona-buscar.js` cargado, el modal en `WebCore` no tiene submit-handler propio -- mismo comportamiento que tendria el HTML original si se sirviera sin ese script, no es una regresion introducida por la migracion.

Impacto real: bajo -- el modal es un componente consumido desde POS/Compras, no desde el modulo Personas en si (`PersonasController.PersonaModal`/`GuardarPersonaModal` ya existen y funcionan via POST directo, probado por el juez). Se resuelve solo cuando se porte el modulo que lo consume -- no requiere decision aparte.


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

### Bug real (no cosmético) heredado de `Web` clásico: "Todas las sucursales" en Comisiones Electrónicas precarga en $0,00

Detectado: 2026-09-01, Módulo 7 (Cajas), slice 2. `CajasController.CrearModelComisionesElectronicas`/`ObtenerFormasPagoElectronicas` reciben `idSucursal` como `int` (no `int?`) y lo pasan tal cual a `Negocio.Venta.getAllVentas` → `Datos.Venta.getAllVentas`, cuyo SQL usa la convención `AND (@idSucursal = -1 OR v.idSucursal = @idSucursal)` con `idSucursal ?? -1`. Cuando el usuario elige "Todas" (`idSucursal=0`), llega un `0` literal (no `null`, no `-1`) — la consulta filtra por una sucursal inexistente y devuelve cero filas. La grilla de "Calcular comisiones electrónicas" se precarga con $0,00 en vez de los montos reales cuando no hay sucursal fija.

`ObtenerResumenComisionesElectronicas` (la acción detrás del botón "Recalcular totales") **sí** convierte correctamente (`idSucursal > 0 ? idSucursal : null`), así que el usuario puede corregir la vista con un click extra — el bug es de precarga inicial, no de la funcionalidad completa.

**Presente igual en `Web` clásico** (mismo código fuente, ninguna línea distinta entre motores) — se portó tal cual, sin corregirlo, siguiendo el criterio de esta migración de preservar comportamiento exacto salvo decisión explícita. Impacto real: bajo (1 click de más para ver los totales correctos), pero es un bug funcional real, no cosmético — queda documentado para una eventual limpieza futura de `CajasController`, en ambos motores por igual.
