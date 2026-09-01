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
