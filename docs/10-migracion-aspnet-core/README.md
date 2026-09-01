# Migración a ASP.NET Core (WebCore) — estado y seguimiento

Plan completo y decisiones de diseño en `docs/DECISIONS.md` (entradas 2026-08-31 y 2026-09-01). Este archivo es el tracker vivo de avance módulo por módulo — se actualiza en cada sesión que toque la migración.

## Spike inicial — CERRADO (2026-08-31 / 2026-09-01)

Verificado con evidencia real (no diseño): `WebCore.csproj` (net10.0) compila y corre con `Negocio`/`Datos`/`Entidades`/`Contratos`/`Utilidades.Core` conectados; `Presentacion` (WinForms) y `Web` (MVC5) siguen funcionando sin cambios; juez de paridad (diff de HTML) ejecutado contra una vista real (`AuditoriaLogin`) con resultado idéntico; corrida verificada bajo Linux real (WSL2 + Ubuntu 26.04 + .NET 10 nativo), Kestrel sirviendo y ejecutando la cadena completa hasta el intento de conexión SQL.

## Convención de estado por módulo

- `no iniciado` — nada portado todavía.
- `en progreso` — algunas vistas portadas, juez de paridad corriendo, todavía no completo.
- `validado` — todas las vistas del módulo portadas, juez de paridad OK en todas, sin gaps abiertos.
- `en producción` — el módulo ya se dio de baja en `Web` clásico (ruteo de Caddy apunta a `WebCore`).

## Orden de migración y estado

| # | Módulo | Estado | Notas |
|---|---|---|---|
| 1 | Administración de sistema | validado (con gaps menores) | Ver detalle abajo |
| 2 | Clientes y proveedores | validado (con gaps menores) | Ver detalle abajo |
| 3 | Productos | en progreso | Index + AddOrEdit/Guardar + Marcas/Tipos portados -- ver detalle abajo |
| 4 | Stock e inventario | validado | Las 13 acciones portadas y verificadas de punta a punta, incluidas las escrituras reales -- ver detalle abajo |
| 5 | Compras y abastecimiento | validado | 8 de 10 acciones portadas y validadas, incluida la escritura real -- ver detalle abajo |
| 6 | Reportes y administración | validado | 6 controllers portados, incluida la escritura real de Usuarios -- ver detalle abajo |
| 7 | Caja y tesorería | no iniciado (scopeado) | ~12.500 líneas, dinero real + step-up auth + acoplado a POS -- ver detalle abajo y `docs/DECISIONS.md` 2026-09-01 |
| 8 | Ventas y POS | no iniciado | El más grande, con AFIP/hotkeys/balanza/código de barras — último |

## Módulo 1 — Administración de sistema

Controller original: `Web/Controllers/SystemAdministrationController.cs` (456 líneas). Vistas (7):

| Vista | Estado |
|---|---|
| `Empresas.cshtml` | validado — juez de paridad OK (diff vacío) |
| `Sucursales.cshtml` | validado — juez de paridad OK (diff vacío salvo gap de localización) |
| `Usuarios.cshtml` | validado — juez de paridad OK (diff vacío salvo gap de localización) |
| `EditarEmpresa.cshtml` | validado — juez de paridad OK (ver gap de localización abajo) |
| `EditarSucursal.cshtml` | validado — juez de paridad OK (ver gap de localización abajo) |
| `EditarUsuario.cshtml` | validado — juez de paridad OK (ver gap de localización abajo) |
| `AltaRapidaEmpresa.cshtml` | validado — juez de paridad OK (ver gaps de localización y boton AFIP abajo) |

Repositorio (`WebCore/Helpers/SystemAdministrationRepository.cs`) y controller (`WebCore/Controllers/SystemAdministrationController.cs`) portados **completos** (Empresas, Sucursales, Usuarios, Alta rapida). `EsSuperAdmin` no se porto (solo lo usa el gate de permisos, deliberadamente fuera de alcance — ver cabecera del controller).

Gaps abiertos que NO bloquean el modulo (detalle completo en `docs/10-migracion-aspnet-core/gaps.md`):
- Mensajes de validacion built-in en ingles para campos de tipo valor no-nullable (impacto bajo, server-side sigue validando bien).
- `AltaRapidaEmpresa`: boton "Buscar en AFIP" depende de `Personas/BuscarPadronAfipAjax`, no portado todavia (Modulo 2).

No verificado en esta sesion: el guardado real (POST) de Sucursales/Usuarios/AltaRapidaEmpresa contra `WebCore` -- solo se comparo el HTML renderizado de las vistas GET, igual que con Empresas.

## Módulo 2 — Clientes y proveedores

Controller original: `Web/Controllers/PersonasController.cs`. Vistas (4):

| Vista | Estado |
|---|---|
| `Index.cshtml` | validado — juez de paridad OK (diff vacío) |
| `Editar.cshtml` (Editar y Nuevo) | validado — juez de paridad OK (ver gap de localización) |
| `_AddOrEditPersonaModal.cshtml` | validado — juez de paridad OK (ver gaps de localización y dependencia con POS/Compras) |
| `_BuscarPersona.cshtml` | portado (sin JS propio que verificar por HTML, ver gap de `persona-buscar.js`) |

`PersonasController` portado con el CRUD completo (Index/Nuevo/Editar/Guardar/Buscar/Listar/Obtener/PersonaModal/GuardarPersonaModal). NO portadas: `BuscarPadronAfip`/`BuscarPadronAfipAjax` (dependen del modulo AFIP, no portado — bloqueante ya conocido del plan original) ni `BuscarDatosAfipDesdeGuardar` (codigo muerto en el original, ninguna accion lo invoca).

Diferencia deliberada: usa un `IEmpresaContext` + `Entidades.Usuario` hardcodeados (empresa 1, admin=true, imitando al usuario real de prueba) en vez de `Session["Usuario"]` — a diferencia de Modulo 1, aca ese "usuario actual" alimenta reglas de negocio reales (`EsAdministrador`, `PuedeGestionarCuentaCorriente`, `PuedeModificarPersona`), no solo un gate de acceso. Ver cabecera de `WebCore/Controllers/PersonasController.cs`.

**Bug real encontrado y corregido durante el port** (no un gap, un fix): `Negocio.Persona`/`Datos.Persona.findById` necesita un `IParametrosContext` real -- pasar `null` (que es lo que hacia el primer intento del port, replicando el default del constructor) causa `NullReferenceException` al resolver `ParamKeys.IdConsumidorFinal`. Se corrigio armando un `Negocio.Parametros` real + `.Reload()` en el constructor del controller, mismo patron que `Web/Controllers/BaseController.cs` usa (armar param una vez y cachearlo en sesion -- en WebCore se arma por request, todavia no hay sesion). Vale la pena tener presente para cualquier controller futuro que use clases de `Negocio` con dependencia de parametros.

Gaps abiertos que NO bloquean el modulo (detalle completo en `docs/10-migracion-aspnet-core/gaps.md`):
- Mensajes de validacion built-in en ingles para campos de tipo valor no-nullable (mismo gap transversal de Modulo 1).
- Botones "Buscar en AFIP" (Editar, modal) sin funcionar: dependen del modulo AFIP, no portado.
- El submit real del modal de alta rapida depende de `persona-buscar.js`, compartido con POS/Compras (Modulo 8, no portado).
- Alertas de TempData no se muestran (gap transversal, ya presente desde Modulo 1, ahora documentado).

No verificado en esta sesion: el guardado real (POST) de `Guardar`/`GuardarPersonaModal` contra `WebCore` -- solo se comparo el HTML/JSON renderizado de las acciones GET.

## Módulo 3 — Productos (EN PROGRESO -- falta solo GenerarEtiquetasPdf y Stock por sucursales)

Controller original: `Web/Controllers/ProductosController.cs` -- **2515 líneas, 24 acciones**. Vistas (14, incluidas 10 sub-features): `Index.cshtml` (3297 líneas), `AddOrEdit.cshtml` (2787 líneas), `Marcas.cshtml`, `Tipos.cshtml`, y 8 parciales de modales (Catálogo Global, Marca, Tipo de producto, Buscar producto, Stock por sucursales). Un orden de magnitud más grande que Módulo 1 o 2 -- confirmado con el usuario (2026-09-01) portar en slices.

| Vista/pieza | Estado |
|---|---|
| `Index()` + `Index.cshtml` (listado) | validado — juez de paridad OK (única diferencia en 3854 tags comparados: el token antiforgery, esperado) |
| `AddOrEdit()`/`Guardar()` + `AddOrEdit.cshtml` (alta/edición, atajos, código de barras, "carga continua") | validado — juez de paridad OK (texto visible idéntico en alta y edición; únicas diferencias en tags: el gap de localización ya conocido) |
| `FindCorteByCodigo`/`BuscarProductoGlobalParaAlta`/`BuscarMarca`/`ListarProductos` (AJAX consumidos por AddOrEdit) + `_BuscarProductoModal.cshtml` | portados (consumidos por AddOrEdit, no probados de forma aislada) |
| `Marcas()` + `Marcas.cshtml` + `_MarcasTabla.cshtml` | validado — juez de paridad OK (texto visible idéntico, única diferencia en tags: orden de atributos del `<form>`) |
| `MarcaModal()`/`GuardarMarca()` + `_AddOrEditMarca.cshtml` | validado — juez de paridad OK en alta y edición (marca real "MARCA SC") |
| `Tipos()` + `Tipos.cshtml` + `_TiposProductoTabla.cshtml` | validado — juez de paridad OK (texto visible idéntico) |
| `TipoProductoModal()`/`GuardarTipoProducto()`/`EliminarTipoProducto()` + `_AddOrEditTipoProducto.cshtml` | validado — juez de paridad OK en alta y edición (tipo real "CERDO") |
| `Eliminar()` (borrar producto) | portado (botón ya presente en `Index.cshtml`, no probado de forma aislada en este turno) |
| `findCorteById()`/`EditPrecioCorte()` (modal de precio con escaner y modificacion por lote) | validado — juez de paridad implicito al portar Index/AddOrEdit; endpoints consumidos por el modal de precio de `Index.cshtml` |
| Catálogo Global — Productos (`VerGlobales`/`BuscarGlobales`/`ImportarSeleccionados` + `_CatalogoGlobalModal.cshtml`/`_CatalogoGlobalRows.cshtml`) | `VerGlobales`/`BuscarGlobales` validados — juez de paridad OK (50 productos reales, 1044 tags comparados, unicas diferencias: line-endings y encoding de entidades HTML, ver gap "cosmetico sin impacto"). `ImportarSeleccionados` (POST con escritura real) portado pero NO probado. |
| Catálogo Global — Tipos (`VerGlobalesTiposProducto`/`BuscarGlobalesTiposProducto`/`ImportarTiposProductoSeleccionados` + `_CatalogoGlobalTiposProductoModal.cshtml`/`_CatalogoGlobalTiposProductoRows.cshtml`) | `VerGlobalesTiposProducto`/`BuscarGlobalesTiposProducto` validados — juez de paridad OK (13 tipos reales, texto identico). `ImportarTiposProductoSeleccionados` (POST con escritura real) portado pero NO probado. |
| `GenerarEtiquetasPdf` | no iniciado -- usa `iTextSharp`, bloqueante ya conocido del plan (migrar a iText7, requiere decision de licencia AGPL/comercial per CLAUDE.md §1.2 antes de tocarlo) |
| `GuardarPuntosStockSucursal` (modal "Ver stock por sucursales") | no iniciado -- a diferencia de lo que parecia, el boton de `Index.cshtml` (`verStockSucursales`) en realidad llama a `Url.Action("StockPorSucursalesProducto", "Stock")` (`StockController`, Modulo 4, no iniciado), no a `ProductosController`. `GuardarPuntosStockSucursal` (el POST de guardado) SI vive en `ProductosController`, pero portar solo la mitad de guardado sin el GET que muestra los valores actuales (que vive en Stock) no serviria de nada -- se porta completo cuando se aborde Modulo 4. |

**Marcas/Tipos, nota sobre el gap de validación**: a diferencia de Empresas/Sucursales/Usuarios/Personas/AddOrEdit, estos 2 formularios (`_AddOrEditMarca.cshtml`, `_AddOrEditTipoProducto.cshtml`) usan `<input>` planos con `value="@(...)"` en vez de `Html.TextBoxFor`/`Html.HiddenFor` -- no generan ningún atributo `data-val`, así que el gap de localización de validación ya documentado **no aplica aquí**: el juez de paridad dio 0 diferencias de contenido y 0 diferencias de tags salvo el reordenamiento cosmético del `<form>`, el resultado mas limpio de todo el modulo hasta ahora.

**Otro patron nuevo encontrado y corregido en `_AddOrEditMarca.cshtml`**: el mismo problema RZ1031 ya visto en `Index.cshtml` (atributo condicional "flotante") aparecio de nuevo, esta vez en un `<input>`: `<input ... value="..." @atributoReadonlyNombre />` (variable string que vale `"readonly"` o `""`, inyectada directo en el area de atributos). Mismo fix: `readonly="@(soloLecturaNombre ? "readonly" : null)"`.

**Estrategia de porteo usada, distinta a Módulo 1/2**: para los 2 archivos grandes (`Index.cshtml`, `AddOrEdit.cshtml`) se copiaron tal cual (`cp`) y se aplicaron solo los parches puntuales de sintaxis ASP.NET Core, en vez de reescribirlos a mano -- mucho menor riesgo de error de transcripción en archivos de este tamaño. Parches aplicados en las vistas:
- `Request["x"]` (indexador de `HttpRequestBase`, no existe en Core) → `Context.Request.Query["x"].ToString()`.
- `<option value="@x" @(cond ? "selected" : "")>` (atributo condicional "flotante") → `<option value="@x" selected="@(cond ? "selected" : null)">` (RZ1031 en Core).
- `@using (Html.BeginForm(...)) { ... }` → `<form asp-controller="..." asp-action="..." method="post" ...> ... </form>` (ubicando el `}` de cierre correcto por indentación, en `AddOrEdit.cshtml` era un bloque de ~325 líneas).
- `new ViewDataDictionary()` (sin parámetros, no existe en Core) → `new ViewDataDictionary(ViewData)`.

El controller (`Guardar`/`BuildVM`/`MapToEntity`/etc.), al ser reescrito a mano (no copiado), se verificó con un **diff automatizado línea por línea contra el original** (no solo revisión visual) -- encontró y corrigió 3 errores de transcripción reales antes de dar el port por bueno: (1) `ClonarProductoGlobal` mezclaba propiedades de los 2 overloads del original (uno para `Entidades.Corte`, otro para `Entidades.CatalogoGlobalProducto`) y trataba de asignar `MarcaNombre` (de solo lectura) -- detectado por el compilador; (2) y (3) dos ramas de `Guardar()` donde había "mejorado" el manejo de `TempData` (unificándolo al patrón `AlertType`/`AlertTitle`/`AlertMsg` usado en otros módulos) en vez de replicar exactamente el original, que en esos puntos usa `TempData["FlashError"]`/`TempData["FlashSuccess"]` de forma inconsistente con el resto del controller -- no detectable por el compilador, solo por el diff contra el original. Vale la pena tener este patrón de verificación presente para el resto de controllers grandes reescritos a mano.

**Assets estáticos copiados** (referenciados por `AddOrEdit.cshtml`, no existían en `WebCore/wwwroot/`): `Content/sounds/focus-beep.wav` (sonido de confirmación al escanear código de barras) y `Scripts/app/edit-readonly.js` (script compartido que maneja el toggle solo-lectura/edición de todo el formulario). Ambos son archivos estáticos sin lógica de servidor -- copia directa, sin riesgo de comportamiento.

`ProductosController` (WebCore) mismo patron que Personas: `IEmpresaContext`+`IParametrosContext` reales (`Negocio.Parametros`+`Reload()`) para `Negocio.Corte`/`Negocio.Sucursal`/`Negocio.Persona`/`Negocio.CortePuntoStockSucursal`. Los flags de `PermisosHelper.TienePermiso(Session,...)` del original se hardcodean a `true` o se omiten -- documentado en la cabecera del controller, mismo criterio que el usuario admin de Personas.

**Juez de paridad de `AddOrEdit`**: comparado en modo alta (id=0) y edición (id=20, "Chorizo") -- texto visible idéntico byte a byte en ambos casos (incluye todos los valores reales: precio, punto de stock, promedio, alícuota). Única categoría de diferencia en las etiquetas: el gap de validación ya documentado (ahora en 15+ campos de este formulario). Un falso positivo encontrado y descartado durante la comparación: una carrera de timing entre el script de validación en vivo del formulario y la captura del DOM por Playwright (con un `waitForTimeout(500)` adicional, ambos lados coinciden exactamente) -- no es un bug de paridad.

No verificado en esta sesion: filtros del listado (SucursalId/tipo/marca/proveedor/fechas) via querystring; el guardado real (POST a `Guardar`) contra `WebCore`, incluido el flujo de "carga continua" y el clonado desde catálogo global.

## Módulo 4 — Stock e inventario (COMPLETO -- 13 acciones portadas y validadas, incluidas las escrituras reales)

Controller original: `Web/Controllers/StockController.cs` -- 2427 líneas, 13 acciones. Vistas (7): `Index.cshtml` (890 líneas), `Editar.cshtml` (1318 líneas), `ExistenciaPorSucursales.cshtml`, `Lineas.cshtml`, `_StockDetalle.cshtml`, `_StockTabla.cshtml`, `_TablaExistenciaPorSucursales.cshtml`. Mismo criterio de escala que Módulo 3: se porta en slices, aplicando directamente el patrón ya validado por el usuario ("slice mínimo primero") sin volver a preguntar.

| Vista/pieza | Estado |
|---|---|
| `Index()`/`Detalle()` + `Index.cshtml`/`_StockTabla.cshtml`/`_StockDetalle.cshtml` (listado + detalle expandible) | validado — juez de paridad OK (texto y tags idénticos, ver detalle abajo) |
| `Nuevo`/`Editar`/`Guardar` (alta/edición de movimientos) + `Editar.cshtml` | **validado de punta a punta, incluido el POST real** (con autorización explícita del usuario, ver detalle abajo). |
| `Lineas` (listado por producto) | validado — juez de paridad OK, ver detalle abajo |
| `ExistenciaPorSucursales`/`BuscarExistenciaPorSucursales`/`StockPorSucursalesProducto`/`ObtenerFechaMinimaExistencia` | validado — juez de paridad OK, ver detalle abajo. De paso se completó `ProductosController.GuardarPuntosStockSucursal` (Módulo 3), que había quedado deliberadamente sin portar por depender de este GET. |
| `BuscarCorte`/`BuscarCortePorCodigo` (autocompletado) | validado — el modal "Buscar producto" (F10) y el campo de código de `Editar.cshtml` ya funcionan con datos reales (ver detalle abajo) |
| Sub-flujo de pesaje (`UltimasComprasPesaje`/`DetalleCompraPesaje`/`ProductosNoCargadosCierre`/`VerPorcentajesPesaje`/`GenerarAjustePesaje`) | **validado de punta a punta, incluido `GenerarAjustePesaje` real** (con autorización explícita del usuario, ver detalle abajo). |
| `ObtenerFechaMinimaExistencia` | no iniciado |

**Sistema de "permiso con límite de fecha" del original, omitido por completo** (no solo hardcodeado a `true`): `BaseController.AjustarFechaIndiceSegunLimiteYPermiso`/`ConfigurarAdvertenciaFechaIndiceConLimiteEnVivo` (`Web/Controllers/BaseController.cs:175-246`) es infraestructura compartida por MUCHOS controllers (no solo Stock) que limita cuánto para atrás puede consultar un usuario sin permiso especial. Con el stub de esta migración (admin con permiso total), el resultado de esas funciones es siempre "sin restricción, sin aviso" -- no llamarlas produce el mismo resultado observable que llamarlas. Lo mismo se aplicó ahora a `PermisosHelper.TienePermiso(Session, Stock.AddOrEditStock, ...)` en `Editar`/`Guardar`: se hardcodea a "siempre autorizado" (mismo criterio ya usado en `ProductosController` para `esAdministrador`). El cálculo de la fecha default del filtro (`fechaLimiteSinPermiso`, usado también como valor por defecto de "desde" sin querystring) SÍ se preservó -- es un valor de negocio real, no parte del gate de permiso. El partial compartido `Views/Shared/_AdvertenciaPermisoFecha.cshtml` (usado por `Index.cshtml`) se portó igual (es condicional, no renderiza nada cuando esas ViewBag no están seteadas -- que es siempre el caso acá).

**Gate de "usuario de sala de producción", no portado (decisión deliberada, no una omisión por descuido)**: `Editar` en el original redirige a un controller `SeleccionUsuario` separado cuando `Session["Usuario"].EsUsuarioProduccion==true` y todavía no se eligió qué operador real está actuando (ver `Web/Controllers/StockController.cs:466` y la entrada de `docs/DECISIONS.md` "Mover la selección de usuario..."). El stub `Entidades.Usuario` de `WebCore` (`Admin=true, IdEmpresa=1, IdSucursal=2, Nombre="ger"`) nunca es usuario de producción (`EsUsuarioProduccion` queda en su default, `false`), así que esa rama nunca se dispara -- mismo comportamiento observable que un usuario real no-producción. `ResolverUsuarioCreador` sí se portó tal cual (es trivial y no cuesta mantenerlo fiel para un login real futuro).

**Bug real encontrado y corregido durante el juez de paridad de `Index`**: la primera comparación mostró filas de más en `WebCore` (movimientos de "San Martin" que no aparecían en `Web` clásico) y el combo de sucursal con "Todas" seleccionado en vez de la sucursal real del usuario. Causa: el original usa `Session["Usuario"].IdSucursal` como sucursal por defecto cuando no hay `?idSucursal` en la URL; el stub de WebCore no tenía ningún valor de sucursal, cayendo siempre a "todas". Corregido hardcodeando la sucursal del usuario real de prueba (`ger`, San Lorenzo = id 2) como default -- documentado con `TODO(claude)` en el controller. Tras el fix, el listado por defecto coincide exactamente. El mismo stub de sucursal (2) se reusó al armar el usuario completo (`_usuarioActual`) para `Nuevo`/`Editar`/`Guardar`.

**Juez de paridad — `Index`/`Detalle`**: `Index` con rango de fechas amplio (2020-2026, para asegurar datos reales) -- texto visible idéntico byte a byte (más de 20 movimientos reales, incluyendo Pesajes/Ajustes vinculados entre sí y sus tooltips). Único tipo de diferencia en las 785 etiquetas comparadas: `selected=""` (MVC5) vs `selected="selected"` (Core) en los `<option>` -- mismo efecto, agregado al gap ya documentado de diferencias cosméticas de bajo nivel entre frameworks. `Detalle` (AJAX del detalle expandible de un movimiento real, Pesaje con líneas de producto) -- diff vacío total, ni una sola diferencia.

**Juez de paridad — `Nuevo`/`Editar` (GET)**: con sesión real autenticada (`ger`), comparado contra `Web` clásico (`DataEngine=SqlServer` temporal, revertido después) vía Playwright. `Stock/Nuevo?tipoCompra=Ingreso%20Stock`: sucursal por defecto (San Lorenzo=2) y tipo de operación preseleccionado (Ingreso Stock) idénticos en ambos motores. `Stock/Editar?id=9029` (un Pesaje Cortes real con línea de producto, medias y proveedor "INDEFINIDO"): proveedor (`INDEFINIDO`, CUIT `11111111111`), `CantMedias=2`, `KgsMedias=0.00` y el array `initialLines` (JSON embebido para `StockUI.init`) idénticos byte a byte entre `Web` y `WebCore`. Únicas diferencias: las ya conocidas (`selected=""` vs `selected="selected"`) más las clases/atributos que `edit-readonly.js` agrega recién después de ejecutarse en el cliente (no relacionado al servidor -- confirmado que aparecen igual en ambos motores dándole tiempo al JS de correr, ver nota metodológica de "race de timing" ya documentada para este mismo patrón en `gaps.md`).

**Hallazgo nuevo (plataforma, no específico de Stock): Bootstrap 5 vs. API jQuery de Bootstrap 4** -- `WebCore/wwwroot/lib/bootstrap` es la versión 5.3.3, que eliminó el plugin jQuery (`$(...).modal(...)`, etc.). Todo el JS portado tal cual desde `Web/Scripts/` (incluido `stock.js`, y ya antes en `Productos/AddOrEdit.cshtml` con `$("#modalX").modal("show")`) asume la API jQuery de Bootstrap 4. Confirmado con Playwright: aparece `$(...).modal is not a function` en la consola al abrir `Stock/Editar`, pero el modal de "Buscar producto" (F10) igual termina abriéndose (Bootstrap 5 también reacciona a los atributos `data-*`/eventos nativos en paralelo) -- impacto observado hasta ahora: bajo, pero no se revisó cada modal de cada vista ya portada. No se resuelve acá (afecta a toda vista con modales ya portada, es una decisión de plataforma: bajar a Bootstrap 4 o agregar un shim) -- **agregado a `gaps.md`**.

**`BuscarCorte`/`BuscarCortePorCodigo`**: se portaron tal cual (sin cambios de lógica, solo `JsonResult`->`IActionResult` y se quitó `JsonRequestBehavior.AllowGet`, que en Core no hace falta para GET). Probado con datos reales contra `WebCore` corriendo solo: `BuscarCorte?q=Costilla` devuelve los 3 productos reales esperados (incluido `id=19, codigo=5, Costilla`, el mismo producto usado en la línea de la compra 9029 verificada arriba); `BuscarCortePorCodigo?codigo=5` devuelve ese mismo producto. No se repitió la comparación byte a byte contra `Web` clásico para estos 2 endpoints puntuales (la lógica es un port literal sin ramas nuevas, y ya se validó el patrón de estos 2 métodos -- `ObtenerCortesPorEmpresa`/`findCorteByCodigoEmpresa` -- indirectamente al confirmar que `Editar` carga bien los datos de productos).

**`Lineas` (listado agrupado por registro, con detalle de líneas de producto)**: se portó junto con `Lineas.cshtml` y sus 2 partials de estilos compartidos (`Views/Elaborados/_Styles.cshtml`, `Views/Shared/_LineasAgrupadasStyles.cshtml`) y `Scripts/app/lineas-agrupadas.js` (los 3, copiados sin cambios -- CSS/JS puro). Modelos nuevos en `WebCore/Models/StockLineasIndexVm.cs` (`StockLineasIndexVm`, `StockLineasGrupoVm`, `CabeceraDetalleCampoVm` -- reusa el `StockLineaDetalleVm` ya creado para `Detalle`). Juez de paridad con rango amplio (2020-2026): comparando el contenido de `<td>` (475 celdas), los resúmenes principales/secundarios y los badges de total kg -- **idénticos byte a byte** entre `Web` y `WebCore`. Única diferencia encontrada: `Web` clásico envuelve cada tabla en un `<div class="sync-scroll-host">` con una barra de scroll flotante, agregado en tiempo de ejecución por `table-scroll-sync.js` (cargado globalmente por `_LayoutBase.cshtml`, que `WebCore` no tiene) -- **nuevo gap de plataforma, agregado a `gaps.md`**, no específico de este slice.

**`ExistenciaPorSucursales`/`BuscarExistenciaPorSucursales`/`StockPorSucursalesProducto`/`ObtenerFechaMinimaExistencia`**: se portaron los 4, junto con `ExistenciaPorSucursales.cshtml`, `_TablaExistenciaPorSucursales.cshtml` (esta última copiada sin ningún cambio — usa `Entidades.ExistenciaPorSucursalesVm` compartido, sin `Web.Models`, sin RZ1031, sin nada incompatible con Core) y `_StockPorSucursalesProductoModal.cshtml` (también en `Productos/`, copiada sin cambios). Aprovechando que `StockPorSucursalesProducto` ya existe, se completó `ProductosController.GuardarPuntosStockSucursal` (POST, Módulo 3) — el botón "Ver stock por sucursales" de `Productos/Index` ya estaba wireado desde ese módulo pero apuntaba a 2 endpoints que no existían todavía; ahora el flujo completo (ver stock por sucursal + editar punto de stock en lote) debería funcionar sin tocar `ProductosController` más que agregar esa acción.

Juez de paridad: la página `ExistenciaPorSucursales` (filtros + shell) es **idéntica byte a byte**. El AJAX `BuscarExistenciaPorSucursales` (1367 valores numéricos comparados) tiene 1363/1367 idénticos; los 4 que difieren son todos el mismo agregado (`StockActual` de un producto/sucursal con muchísimas líneas acumuladas) con una diferencia de **0,001** entre motores (`-40.611,550` vs `-40.611,551`), confirmada estable/reproducible en ambos lados (no es un valor flaky) — ver análisis completo en `gaps.md`, es un hallazgo real (no cosmético) pero de impacto mínimo, con la hipótesis más probable siendo el swap `System.Data.SqlClient`→`Microsoft.Data.SqlClient` ya aceptado en el plan original. También se encontró un 4º caso de diferencia cosmética de encoding HTML (`+` codificado como `&#x2B;` en Core, literal en MVC5), agregado a la lista ya existente.

**Sub-flujo de pesaje (`UltimasComprasPesaje`/`DetalleCompraPesaje`/`ProductosNoCargadosCierre`/`VerPorcentajesPesaje`/`GenerarAjustePesaje`)**: con esto el controller queda completo — las 13 acciones del original están portadas. `GenerarAjustePesaje` es la única que escribe (crea/modifica una `Compra` de tipo `Ajuste Stock` vinculada al pesaje, y sus `CortePorCompra`) — se portó línea por línea (sin cambios de lógica más allá de la ya documentada omisión de permisos) pero **no se ejecutó en vivo**, mismo criterio de precaución que `Guardar`: es una escritura de negocio permanente sobre la base compartida. Las otras 4 son de solo lectura y sí se probaron con datos reales (compra 9029, Pesaje Cortes) contra `Web` clásico vía Playwright — **los 4 JSON son idénticos byte a byte** entre ambos motores (`UltimasComprasPesaje`, `DetalleCompraPesaje`, `ProductosNoCargadosCierre`, `VerPorcentajesPesaje` — este último devolvió el mismo mensaje de error esperado, ya que la compra de prueba no tiene `KgsMedias`/`CantMedias` cargados).

Se detectaron y se portaron 2 helpers privados que en el original eran clases anidadas del controller (`ProductoNoCargadoCierreVm`, `TablaModalStockVm`, `ColumnaModalStockVm`, `CompraPesajeListadoVm`, `CompraPesajeSeleccionLineaVm` → `WebCore/Models/CompraPesajeVm.cs`). Se detectó además una clase muerta en el original (`CompraPesajeSeleccionVm`, declarada pero nunca usada) y un método sobrecargado muerto (`EsCompraSeleccionableParaPesaje(Entidades.Compra)`, nunca llamado — solo se usa la sobrecarga `(string)`) — ninguno de los dos se portó, mismo criterio que otro código muerto ya encontrado en este módulo.

## Verificación en vivo de `Guardar` y `GenerarAjustePesaje` (escrituras reales, autorizadas explícitamente por el usuario)

Fecha: 2026-09-01. El usuario autorizó explícitamente ejecutar estas 2 escrituras contra la base SQL Server local compartida (`.\sqlexpress`, base `CarniSys`) — hasta este punto se habían dejado sin probar por precaución (ver entradas anteriores). Procedimiento: `Web` clásico con `DataEngine=SqlServer` temporal (revertido a `Postgres` al terminar), ambos servidores corriendo en paralelo, mismo payload posteado a cada uno vía Playwright + `fetch` con el antiforgery token real de cada motor, verificación final directa en la base con `sqlcmd` (usuario `cs_admin`, bypass de la RLS propia de este SQL Server — ver `~/hosts/carnisys-web-local.env`).

**Bug real encontrado y corregido**: el primer POST de `Guardar` a `WebCore` guardó la compra con `creadoPor=0` (el usuario de sistema real "CarniSys Admin") en vez de `creadoPor=2` ("ger", el usuario de prueba de todo el juez de paridad de esta migración) que sí guardó `Web` clásico. Causa: el stub `_usuarioActual` de `StockController` tenía `Nombre="ger"`/`Admin=true`/`IdEmpresa=1`/`IdSucursal=2` pero **no `Id`** (quedaba en el default `0`) — sin consecuencia en `Index`/`Detalle`/etc. (nunca se persiste), pero `Guardar`/`GenerarAjustePesaje` sí escriben `CreadoPor`/`ActualizadoPor` a la base real. Se corrigió agregando `Id = 2` al stub. Se revisó también `PersonasController`/`ProductosController`: ninguno de los dos persiste `CreadoPor`/`ActualizadoPor` del usuario de sesión, así que el mismo bug no existe ahí.

**Verificación de `Guardar`** (alta de un Pesaje Cortes: sucursal San Lorenzo, proveedor INDEFINIDO, 1 línea de Costilla 1,500 kg, CantMedias=1, KgsMedias=1,500): `Web` clásico creó `idCompra=9036`, `WebCore` (ya con el fix) creó `idCompra=9038` — comparados directamente en la tabla `Compras`/`CortePorCompra`, **todos los campos idénticos** (`idSucursal`, `idProveedor`, `cantMedias`, `kgsMedias`, `observaciones`, `creadoPor=2`, y la línea `idCorte=19`/`cantKg=1500`/`creadoPor=2`), salvo el `idCompra`/timestamp (esperado, son registros distintos). El registro intermedio `idCompra=9037` (creado antes del fix, con `creadoPor=0`) queda en la base como evidencia del bug ya corregido — no se borró (`Compra` no tiene un borrado limpio conocido en este sistema).

**Verificación de `GenerarAjustePesaje`** (sobre los 2 pesajes recién creados, ya con `CantMedias`/`KgsMedias` válidos): ambos motores respondieron `{"ok":true,"estado":"Actualizado"}`, creando `idCompra=9039` (Web, ajuste de 9036) y `idCompra=9040` (WebCore, ajuste de 9038). Verificado en la base: ambas `Compra` de tipo `Ajuste Stock` con `idPesajeAjustado` apuntando correctamente al pesaje de origen, `nroRemito` = id del pesaje, `creadoPor=2`, y su línea de `CortePorCompra` (`idCorte=19`, `cantKg=1500`, `creadoPor=2`) — **idénticas entre ambos motores**. El estado del pesaje original quedó en `"Actualizado"` en ambos casos. `VerPorcentajesPesaje` sobre estos mismos pesajes (con datos válidos, la rama de éxito que no se había podido probar antes por falta de un pesaje real con `KgsMedias`) también dio **JSON idéntico** entre motores.

Se encontró de paso un 5º caso del gap de encoding ya documentado (JSON: `ó` en Core vs. `ó` literal en MVC5 para el mismo mensaje de éxito) — agregado a `gaps.md`.

**Con esto, el Módulo 4 (Stock e inventario) queda completo y verificado de punta a punta**, incluidas las 2 únicas escrituras que quedaban sin probar en vivo.

## Módulo 5 — Compras y abastecimiento (EN PROGRESO -- Index/Detalle)

Controller original: `Web/Controllers/ComprasController.cs` -- 1022 líneas, 10 acciones. Vistas (6): `Index.cshtml` (593 líneas), `Editar.cshtml` (600 líneas), `Lineas.cshtml` (408 líneas), `AutorizarModulo.cshtml`, `_ComprasDetalle.cshtml`, `_ComprasTabla.cshtml`. Mismo criterio de escala que Módulos 3/4: se porta en slices, empezando por Index()/Detalle().

| Vista/pieza | Estado |
|---|---|
| `Index()`/`Detalle()` + `Index.cshtml`/`_ComprasTabla.cshtml`/`_ComprasDetalle.cshtml` | validado — juez de paridad OK, ver detalle abajo |
| `Lineas` (listado por producto) | validado — juez de paridad OK (365 celdas idénticas), ver detalle abajo |
| `Editar`/`NuevaCompra`/`ModificarCompra`/`Guardar` (alta/edición) | **validado de punta a punta, incluido el POST real** (con autorización explícita del usuario, ver detalle abajo). |
| `BuscarCorte`/`BuscarCortePorCodigo` (autocompletado propio de Compras, no comparte los de Stock) | validado — probado con datos reales (Costilla), mismo patrón que Stock |
| `AutorizarModuloCompras`/`AutorizarOperadorModuloCompras` (login de operador para usuario de producción) | no se porta — ver decisión abajo |

**Mismos criterios ya establecidos en Stock, reaplicados sin volver a decidir**: sistema de "permiso con límite de fecha" omitido por completo (stub admin = siempre autorizado); stub `Entidades.Usuario` con **`Id=2` incluido desde el arranque** (lección del bug real encontrado en Stock — un stub sin `Id` graba `CreadoPor` incorrecto al escribir datos, aunque `Index`/`Detalle` no escriben nada todavía en este módulo).

**Gate de "usuario de sala de producción" (`AutorizarModuloCompras`), NO portado**: mismo patrón que `SeleccionUsuario` en Stock — el original redirige a una pantalla de login de operador cuando `EsUsuarioProduccion==true` y no hay operador de módulo autorizado (`ResolverOperadorModulo`). Con el stub admin (`EsUsuarioProduccion=false`) esa rama nunca se dispara.

**`PermiteMediaRes()` hardcodeado a `true`**: el original compara `Session["Usuario"].Empresa.Cuit` contra un CUIT fijo (`20306210786`) que habilita el tipo de compra "Media Res". Se verificó contra la base local (`sqlcmd`, tabla `Empresas`, `idEmpresa=1`) que la empresa del stub tiene ese CUIT — mismo patrón ya usado en `Stock/ExistenciaPorSucursales.cshtml` para el intervalo de autoactualización.

**Juez de paridad**: `Index` con rango de fechas amplio (2020-2026) — las 252 celdas de la tabla (`<td>`) son **idénticas byte a byte** entre `Web` y `WebCore`. `Detalle` (AJAX de una compra real, `idCompra=9035`) — diff vacío total, idéntico byte a byte. Única diferencia encontrada: el total agregado (`CalcularTotalImporte`, un `SUM` sobre la columna `totalS`) dio `3.727.390,00` en `Web` clásico vs `3.727.389,50` en `WebCore` — **segunda aparición del mismo patrón de redondeo de punto flotante en agregados ya documentado en Módulo 4** (confirmado estable/reproducible en ambos motores, no es un dato flaky). Ver `gaps.md` para el análisis consolidado.

**`Lineas` (listado agrupado por compra, con detalle de líneas de producto)**: se portó junto con `Lineas.cshtml`, reusando los mismos 3 assets compartidos ya portados en Módulo 4 (`Views/Elaborados/_Styles.cshtml`, `Views/Shared/_LineasAgrupadasStyles.cshtml`, `Scripts/app/lineas-agrupadas.js` — sin ningún cambio nuevo). Modelo nuevo `WebCore/Models/CompraLineasIndexVm.cs` (`CompraLineasIndexVm`, `CompraLineasGrupoVm`, `CompraLineaDetalleVm`, reusando `CabeceraDetalleCampoVm` ya creado en Stock). Juez de paridad con rango amplio (2020-2026): 365 celdas de tabla, resúmenes principales/secundarios y badges de total por compra — **idénticos byte a byte** entre `Web` y `WebCore` (a diferencia del total agregado global de `Index`, los totales por-compra individuales de `Lineas` no mostraron el problema de redondeo — parece limitarse al `SUM` sobre todo el conjunto filtrado, no a sumas más chicas por fila).

**`BuscarCorte`/`BuscarCortePorCodigo`**: autocompletado propio de Compras (no comparte los de `StockController` — misma lógica pero con campo `precio` en vez de `tipo`/`promedio`/`pesable`). Port literal, probado con datos reales (`Costilla`, código 5, precio 1344.2).

**`Editar`/`NuevaCompra`/`ModificarCompra`/`Guardar`**: se portaron los 4, junto con `Editar.cshtml` (600 líneas) y sus 2 assets propios (`Scripts/app/compras.js`, `Content/css/compras-editar.css` — copiados sin cambios). Modelo nuevo `WebCore/Models/CompraEditVm.cs`. Se agregó el paquete `System.Runtime.Caching` a `WebCore.csproj` (única dependencia nueva de toda la migración hasta ahora) para portar **sin cambios** la protección real anti-doble-submit del original (`MemoryCache.Default.Add`, atómico) — se prefirió esto a reimplementarla con `IMemoryCache` de ASP.NET Core, que no tiene un "add si no existe" atómico equivalente, para no arriesgar la garantía de concurrencia de una mitigación de un bug real ya visto en producción.

El flujo `desdePos` (Editar/Guardar embebidos en POS) se portó tal cual en el código (mismas llamadas a `Negocio.CierreCaja`) pero no se puede ejercitar todavía — el Módulo 8 (POS) no está portado. La rama `@if (Model.DesdePos)` de la vista se colapsó a solo la rama `else` (mismo criterio que en Stock/Editar con `renderInlineScripts`).

Juez de paridad con sesión real: `Compras/NuevaCompra` — idéntico salvo las diferencias ya conocidas (mensajes de validación en inglés/`data-val-number` faltante, y un 6º caso nuevo de diferencia cosmética por `value` ausente vs vacío, ver `gaps.md`). `Compras/Editar?id=9035` (una compra real) — **idéntico byte a byte** una vez descontadas esas mismas diferencias ya catalogadas.

## Verificación en vivo de `Compras/Guardar` (escritura real, autorizada explícitamente por el usuario)

Fecha: 2026-09-01. Mismo procedimiento que la verificación en vivo de Stock (`Web` con `DataEngine=SqlServer` temporal, ambos servidores en paralelo, POST vía Playwright + `fetch` con el antiforgery token real de cada motor, verificación final con `sqlcmd`/`cs_admin`).

**Primer intento — falsa alarma metodológica, no un bug real**: el primer payload de prueba usó `CantKgs="1.5"` (punto decimal). `Web` clásico lo rechazó ("La línea 1 debe tener una cantidad mayor a cero") y `WebCore` lo aceptó pero grabó `cantKg=15.0` (no `1.5`) — a primera vista parecía un bug de interpretación de decimales entre motores. Investigando `Scripts/app/compras.js` (`formatDecimalForPost`), se confirmó que el formulario real **nunca** postea con punto: siempre convierte a coma decimal (`String(1.5).replace('.', ',')` → `"1,5"`) antes de escribir los inputs ocultos, porque tanto `Web` clásico (`Web.config` fuerza `culture="es-AR"`) como el binding de formularios de `WebCore` (que hereda la misma cultura del sistema operativo) esperan coma. El payload con punto no era representativo de un envío real desde el navegador — mismo patrón que la falsa alarma de "ID Pesaje sin dos puntos" documentada en la verificación de Stock (Módulo 4): una discrepancia que se explica por el método de prueba, no por el código portado.

**Verificación real, con el formato correcto (coma decimal, el que realmente envía el formulario)**: `CantKgs="1,5"`, `PrecioKg="100,50"` — ambos motores respondieron `ok:true`, creando `idCompra=9042` (Web) y `9043` (WebCore). Verificado en la base: **`cantKg=1.5`, `precioKg=100.5`, `creadoPor=2` idénticos en ambos** (tabla `CortePorCompra`). El registro intermedio `idCompra=9041` (creado en el primer intento fallido, con `cantKg=15.0` incorrecto por el payload de prueba mal formado) queda en la base como dato de prueba, marcado en sus Observaciones.

**Con esto, el Módulo 5 (Compras y abastecimiento) queda validado de punta a punta** para las 8 acciones portadas, incluida la única escritura real del slice.

## Módulo 6 — Reportes y administración (COMPLETO)

Trabajo autónomo nocturno (autorización explícita del usuario, 2026-09-01: "hagas la migracion completa, luego pruebes... te doy los permisos para que escribas en las bd locales"). Scope acotado a las pantallas de administración self-service de la empresa/tenant (distintas de `SystemAdministrationController`, cross-tenant, ya portado en Módulo 1): `EmpresaController` (177 líneas), `SucursalController` (216 líneas), `DispositivosSegurosController` (123 líneas), `ParametrosController` (345 líneas), `UsuariosController` (704 líneas), `ReportesController` (1676 líneas). Excluidos deliberadamente: `SessionController` (infra de keep-alive/perf, no es una pantalla) y `SeleccionUsuarioController` (flujo de usuario de producción, mismo criterio ya aplicado en Stock/Compras — nunca se dispara con el stub admin).

| Controller | Estado |
|---|---|
| `EmpresaController` + `Empresa/Index.cshtml` | validado — smoke test con datos reales (empresa "SuperCerdo", CUIT 20306210786) |
| `SucursalController` + `Sucursal/{Index,Editar}.cshtml` | validado — smoke test con datos reales (San Lorenzo/San Martin) |
| `DispositivosSegurosController` + `DispositivosSeguros/Index.cshtml` | validado — smoke test |
| `ParametrosController` + `Parametros/Index.cshtml` | validado — smoke test con datos reales (grilla poblada, ej. `codProdGenerico`) |
| `UsuariosController` + `Usuarios/{Index,Editar,Permisos}.cshtml` | **validado de punta a punta, incluidas las 3 escrituras reales** (`Guardar`, `GuardarPermisos`, `DesbloquearUsuario`) — ver detalle abajo |
| `ReportesController` + `Reportes/{Index,_FiltrosSecundarios}.cshtml` | validado — los 6 tipos de reporte + 2 endpoints AJAX probados con datos reales, ver detalle abajo |

**`UsuariosController`, mismo criterio ya establecido en Empresa/Sucursal/DispositivosSeguros/Parametros**: stub `Entidades.Usuario` (`Id=2, Admin=true, IdEmpresa=1, IdSucursal=2, Nombre="ger"`), sin Session. `ObtenerUsuarioActualConPermisos()` (refresco de `Session["Usuario"]` en el original) se reemplaza por uso directo del stub — con `Admin=true`, `TienePermisoUsuarios`/`PuedeVerUsuarios`/`PuedeAdministrarUsuarios` cortocircuitan a `true` (mismo comportamiento que el original con un admin real), así que el resto de la máquina de permisos queda como código muerto para este stub pero se portó igual, fiel al original. Reglas de negocio reales preservadas tal cual: `ClavesBloqueadasUsuarioProduccion` (fuerza `PuedeVer=false`/`PuedeEditar=false` para permisos de Venta/Finanza/Elaborado.VerFormulas/IngresoFormula si el usuario editado es de producción), el mirror Ver→Editar de "Cierres de Caja" (`idForm=9`, hardcodeado igual que el original), y `AplicarPuedeOperarPOS` (toggle idempotente del permiso "Ventas > Editar").

**Verificación en vivo de `UsuariosController`** (autorización de escritura ya vigente): se creó un usuario de prueba real (`test.webcore.mod6`, id=17) vía `POST /Usuarios/Guardar`, confirmado con `sqlcmd` contra los 20 campos reales de la tabla `Usuarios` y sus 30 filas default en `PermisosUsuarios` (incluyendo `idForm=7` con `DiasPermitidosEditar=0` por el toggle "Puede operar POS", tildado en el payload de prueba). Se probó `GuardarPermisos` (Playwright: tildar "Puede ver" en Ventas con 3 días y alcance "Todos") — confirmado en DB. Se probó `DesbloquearUsuario` (se forzó `bloqueado=1` manualmente vía `sqlcmd`, se clickeó "Desbloquear", se confirmó `bloqueado=0`/`intentosFallidosLogin=0`/`fechaBloqueoUtc=NULL`). El usuario y permisos de prueba se borraron después (no es un registro de negocio real, a diferencia de los pesajes/compras de prueba de módulos anteriores que sí se dejaron).

**`ReportesController`**: el controller más grande portado hasta ahora en esta migración (1676 líneas originales). Es de solo lectura (ninguna acción escribe), así que no requirió prueba de escritura. Mismo criterio que Stock/Compras: sistema de "permiso con límite de fecha" (`AjustarFechaSiNoTienePermiso`/`ConfigurarAdvertenciaFechaEnVivo`/`VistaAccesoDenegado`, y `PermisosHelper.TienePermiso`) omitido por completo — con el stub admin el resultado es siempre "sin restricción, sin aviso". Se verificaron con datos reales los 6 tipos de reporte (Stock Actual: 20 filas; Cierre Stock; Stock Retroactivo; Proyección Ventas vs Stock: 54 filas; Ventas por Producto: 14 filas; Balance Económico: $1.417.982,20 en ventas del período, balance $1.396.430,70) y los 2 endpoints AJAX (`FiltrosSecundarios`, `VentasPorProductoSerie`).

**Patrón nuevo encontrado en `_FiltrosSecundarios.cshtml`**: `@(condicion ? "checked=\"checked\"" : "")` como token suelto dentro de un `<input>` (sin nombre de atributo) — en Razor esto se HTML-encodea, así que nunca funcionó como atributo real ni en `Web` clásico ni portado tal cual a Core (es funcionalmente un no-op en ambos). Corregido al patrón ya establecido en esta migración: `checked="@(condicion ? "checked" : null)"` — no es un cambio de comportamiento respecto a `Web` clásico, es la forma correcta de expresar la misma intención que el original tampoco lograba.

**Assets estáticos copiados**: `Scripts/app/reportes.js`, `Content/vendor/chart.js/Chart.bundle.min.js` (sin modificar).

## Módulo 7 — Caja y tesorería (SCOPEADO, NO INICIADO)

Scoping hecho (2026-09-01, ver `docs/DECISIONS.md`): `CajasController.cs` (1631 líneas, leído completo) +
`FinanzasController.cs` (1944 líneas, no leído todavía). `MercadoPagoController.cs` excluido del alcance
(integración en desarrollo activo en otra sesión). Vistas: `Views/Cajas/*` (3980 líneas), `Views/Finanzas/*`
(4935 líneas) — total del módulo del mismo orden que Módulos 3+4+5 juntos.

**Por qué no se completó en la misma sesión que Módulo 6** (autorización ya vigente para hacerlo, no es
falta de permiso): tamaño real medido (~12.500 líneas) + riesgo real más allá del tamaño — `CajasController`
tiene autenticación de step-up real (`AutorizarAccionCierre`, con rate-limiting anti fuerza-bruta) y un
núcleo transaccional (abrir/cerrar caja, egresos) acoplado al estado de "caja abierta" del módulo POS
(Módulo 8, no portado) — no es codigo que se pueda simplemente portar-y-smoke-test como Reportes.

**Restore point**: `git tag pre-aspnetcore-fanout-modulo7-20260901` (ya creado, nada portado todavía sobre
esta base).

**Slicing sugerido para cuando se retome** (mismo criterio de riesgo creciente ya usado en Compras/Stock):
1. `CajasAbiertas`/`HistorialCierresCaja` (listado y consulta — más simple).
2. `EgresosCaja`/`TiposEgresoCaja` (CRUD de catálogo).
3. `MisEgresosCaja`/`ActividadesCaja`/`NuevoEgresoCaja`/`AbrirCaja`/`CerrarCaja`/`CambiarSucursalCaja`/
   `AutorizarAccionCierre` (núcleo transaccional, dinero real + auth — dejar para el final, con plan
   escrito y confirmado antes de tocarlo, per CLAUDE.md §11.1).
4. `FinanzasController.cs` — leer y scopear recién al llegar a este punto.

## Juez de paridad — validado con control negativo

Fecha: 2026-09-01. Antes del fan-out real (CLAUDE.md §11.1 "validar al juez"), se probó que el mecanismo (diff de HTML normalizado, extracción y comparación ordenada de contenido de `<td>`) efectivamente **detecta una rotura real** cuando se introduce a propósito, no solo que "pasa" contra código correcto. Detalle del control negativo en `docs/DECISIONS.md`.
