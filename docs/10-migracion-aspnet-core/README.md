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
| 4 | Stock e inventario | no iniciado | |
| 5 | Compras y abastecimiento | no iniciado | |
| 6 | Reportes y administración | no iniciado | |
| 7 | Caja y tesorería | no iniciado | |
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

## Juez de paridad — validado con control negativo

Fecha: 2026-09-01. Antes del fan-out real (CLAUDE.md §11.1 "validar al juez"), se probó que el mecanismo (diff de HTML normalizado, extracción y comparación ordenada de contenido de `<td>`) efectivamente **detecta una rotura real** cuando se introduce a propósito, no solo que "pasa" contra código correcto. Detalle del control negativo en `docs/DECISIONS.md`.
