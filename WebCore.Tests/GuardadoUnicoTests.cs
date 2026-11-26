// Tests del guardado unico (Helpers/GuardadoUnico.cs, docs/DECISIONS.md 2026-11-26): un envio repetido con los mismos
// datos -- doble clic, Enter repetido, reintento tras perder la respuesta -- no ejecuta la accion de guardado dos veces.
// Se prueba el registro en memoria solo y el filtro MVC entero con un HttpContext armado a mano (sin servidor ni base).
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Primitives;
using WebCore.Helpers;

namespace WebCore.Tests
{
    public class GuardadoUnicoTests
    {
        private static readonly TimeSpan Ventana = TimeSpan.FromMinutes(10);
        private static readonly TimeSpan Espera = TimeSpan.FromSeconds(5);

        private static ResultadoGuardado Resultado(object valor) =>
            new ResultadoGuardado(new JsonResult(valor), new Dictionary<string, object?>());

        // ---------- Registro en memoria ----------

        [Fact]
        public async Task Primer_pedido_ejecuta_y_el_repetido_recibe_el_mismo_resultado()
        {
            var registro = new RegistroGuardadoUnico();
            var primero = await registro.TomarTurnoAsync("k", Espera);
            Assert.Equal(TipoTurnoGuardado.Ejecutar, primero.Tipo);

            var resultado = Resultado(new { ok = true, id = 7 });
            registro.Completar("k", resultado, Ventana);

            var repetido = await registro.TomarTurnoAsync("k", Espera);
            Assert.Equal(TipoTurnoGuardado.Repetir, repetido.Tipo);
            Assert.Same(resultado, repetido.Resultado);
        }

        [Fact]
        public async Task Repetido_que_llega_mientras_guarda_espera_y_no_ejecuta()
        {
            var registro = new RegistroGuardadoUnico();
            await registro.TomarTurnoAsync("k", Espera);

            var esperando = registro.TomarTurnoAsync("k", Espera);
            await Task.Delay(50);
            Assert.False(esperando.IsCompleted); // sigue esperando al primero

            registro.Completar("k", Resultado(new { ok = true }), Ventana);
            var turno = await esperando;
            Assert.Equal(TipoTurnoGuardado.Repetir, turno.Tipo);
        }

        [Fact]
        public async Task Si_el_primero_falla_el_repetido_ejecuta()
        {
            var registro = new RegistroGuardadoUnico();
            await registro.TomarTurnoAsync("k", Espera);
            var esperando = registro.TomarTurnoAsync("k", Espera);

            registro.Liberar("k");
            var turno = await esperando;
            Assert.Equal(TipoTurnoGuardado.Ejecutar, turno.Tipo);
        }

        [Fact]
        public async Task Espera_agotada_no_ejecuta()
        {
            var registro = new RegistroGuardadoUnico();
            await registro.TomarTurnoAsync("k", Espera);

            var turno = await registro.TomarTurnoAsync("k", TimeSpan.FromMilliseconds(80));
            Assert.Equal(TipoTurnoGuardado.EsperaAgotada, turno.Tipo);
        }

        [Fact]
        public async Task Pasada_la_ventana_se_vuelve_a_ejecutar()
        {
            var ahora = new DateTime(2026, 11, 26, 10, 0, 0, DateTimeKind.Utc);
            var registro = new RegistroGuardadoUnico(() => ahora);
            await registro.TomarTurnoAsync("k", Espera);
            registro.Completar("k", Resultado(new { ok = true }), TimeSpan.FromSeconds(30));

            ahora = ahora.AddSeconds(29);
            Assert.Equal(TipoTurnoGuardado.Repetir, (await registro.TomarTurnoAsync("k", Espera)).Tipo);

            ahora = ahora.AddSeconds(2);
            Assert.Equal(TipoTurnoGuardado.Ejecutar, (await registro.TomarTurnoAsync("k", Espera)).Tipo);
        }

        [Fact]
        public async Task Las_entradas_vencidas_se_barren()
        {
            var ahora = new DateTime(2026, 11, 26, 10, 0, 0, DateTimeKind.Utc);
            var registro = new RegistroGuardadoUnico(() => ahora);
            await registro.TomarTurnoAsync("a", Espera);
            registro.Completar("a", Resultado(new { ok = true }), TimeSpan.FromSeconds(30));

            ahora = ahora.AddMinutes(5);
            await registro.TomarTurnoAsync("b", Espera); // al pasar, barre "a"
            Assert.Equal(1, registro.Cantidad);
        }

        // ---------- Clave y exito ----------

        private static IEnumerable<KeyValuePair<string, IEnumerable<string>>> Campos(params (string Clave, string Valor)[] campos) =>
            campos.Select(c => new KeyValuePair<string, IEnumerable<string>>(c.Clave, new[] { c.Valor }));

        [Fact]
        public void La_clave_ignora_el_orden_y_el_token_antiforgery()
        {
            var a = EvitarGuardadoDobleFilter.ArmarClave(1, 5, "Stock/Guardar", "t1",
                Campos(("IdCompra", "0"), ("Lineas[0].CantKgs", "2,5"), ("__RequestVerificationToken", "x")));
            var b = EvitarGuardadoDobleFilter.ArmarClave(1, 5, "Stock/Guardar", "t1",
                Campos(("Lineas[0].CantKgs", "2,5"), ("__RequestVerificationToken", "otro"), ("IdCompra", "0")));
            Assert.Equal(a, b);
        }

        [Fact]
        public void La_clave_cambia_con_otros_datos_otro_token_u_otro_usuario()
        {
            var baseClave = EvitarGuardadoDobleFilter.ArmarClave(1, 5, "Stock/Guardar", "t1", Campos(("CantKgs", "2")));
            Assert.NotEqual(baseClave, EvitarGuardadoDobleFilter.ArmarClave(1, 5, "Stock/Guardar", "t1", Campos(("CantKgs", "3"))));
            Assert.NotEqual(baseClave, EvitarGuardadoDobleFilter.ArmarClave(1, 5, "Stock/Guardar", "t2", Campos(("CantKgs", "2"))));
            Assert.NotEqual(baseClave, EvitarGuardadoDobleFilter.ArmarClave(1, 6, "Stock/Guardar", "t1", Campos(("CantKgs", "2"))));
            Assert.NotEqual(baseClave, EvitarGuardadoDobleFilter.ArmarClave(2, 5, "Stock/Guardar", "t1", Campos(("CantKgs", "2"))));
            Assert.NotEqual(baseClave, EvitarGuardadoDobleFilter.ArmarClave(1, 5, "Movimientos/Guardar", "t1", Campos(("CantKgs", "2"))));
        }

        [Fact]
        public void La_huella_no_confunde_valores_que_concatenados_son_iguales()
        {
            var a = EvitarGuardadoDobleFilter.ArmarClave(1, 5, "X", "t", Campos(("a", "b\nc=d")));
            var b = EvitarGuardadoDobleFilter.ArmarClave(1, 5, "X", "t", Campos(("a", "b"), ("c", "d")));
            Assert.NotEqual(a, b);
        }

        [Fact]
        public void Json_ok_se_lee_del_objeto_anonimo()
        {
            Assert.True(EvitarGuardadoDobleFilter.EsJsonOk(new { ok = true, id = 1 }));
            Assert.False(EvitarGuardadoDobleFilter.EsJsonOk(new { ok = false }));
            Assert.False(EvitarGuardadoDobleFilter.EsJsonOk(new { success = true }));
            Assert.False(EvitarGuardadoDobleFilter.EsJsonOk(null));
        }

        // ---------- Filtro MVC entero ----------

        private sealed class SesionFalsa : WebCore.Services.IUsuarioSesionService
        {
            public bool EstaAutenticado => true;
            public Entidades.Usuario UsuarioActual { get; } = new Entidades.Usuario { Id = 5, IdEmpresa = 1 };
            public Utilidades.IEmpresaContext Empresa => throw new NotSupportedException();
            public Utilidades.IParametrosContext Parametros => throw new NotSupportedException();
        }

        private sealed class TempDataEnMemoria : ITempDataProvider
        {
            public IDictionary<string, object> LoadTempData(HttpContext context) => new Dictionary<string, object>();
            public void SaveTempData(HttpContext context, IDictionary<string, object> values) { }
        }

        private sealed class UrlHelperFalso : IUrlHelper
        {
            public UrlHelperFalso(ActionContext context) { ActionContext = context; }
            public ActionContext ActionContext { get; }
            public string? Action(UrlActionContext actionContext) => "/" + (actionContext.Controller ?? "Stock") + "/" + actionContext.Action;
            public string? Content(string? contentPath) => contentPath;
            public bool IsLocalUrl(string? url) => true;
            public string? Link(string? routeName, object? values) => null;
            public string? RouteUrl(UrlRouteContext routeContext) => null;
        }

        private sealed class UrlHelperFactoryFalsa : IUrlHelperFactory
        {
            public IUrlHelper GetUrlHelper(ActionContext context) => new UrlHelperFalso(context);
        }

        private static EvitarGuardadoDobleFilter CrearFiltro(RegistroGuardadoUnico registro, ITempDataDictionaryFactory tempData) =>
            new EvitarGuardadoDobleFilter(registro, new SesionFalsa(), tempData, new UrlHelperFactoryFalsa(), NullLogger<EvitarGuardadoDobleFilter>.Instance);

        private static ActionExecutingContext CrearContexto(Dictionary<string, string> campos, bool ajax = true)
        {
            var http = new DefaultHttpContext();
            http.Request.Method = "POST";
            http.Request.ContentType = "application/x-www-form-urlencoded";
            http.Request.Form = new FormCollection(campos.ToDictionary(c => c.Key, c => new StringValues(c.Value)));
            if (ajax) http.Request.Headers["X-Requested-With"] = "XMLHttpRequest";
            var descriptor = new ActionDescriptor();
            descriptor.RouteValues["controller"] = "Stock";
            descriptor.RouteValues["action"] = "Guardar";
            var actionContext = new ActionContext(http, new RouteData(), descriptor);
            return new ActionExecutingContext(actionContext, new List<IFilterMetadata>(), new Dictionary<string, object?>(), new object());
        }

        // Simula la accion de guardado: cuenta cuantas veces corre y tarda un poco (como el INSERT real).
        private static ActionExecutionDelegate Accion(ActionExecutingContext ctx, Func<IActionResult> resultado, Action alCorrer, int demoraMs = 100)
        {
            return async () =>
            {
                alCorrer();
                await Task.Delay(demoraMs);
                return new ActionExecutedContext(ctx, new List<IFilterMetadata>(), new object()) { Result = resultado() };
            };
        }

        private static Dictionary<string, string> FormStock(string token = "tok1", string kgs = "2,5") => new Dictionary<string, string>
        {
            ["TokenGuardado"] = token,
            ["__RequestVerificationToken"] = Guid.NewGuid().ToString(),
            ["TipoCompra"] = "Egreso Stock",
            ["Lineas[0].IdCorte"] = "61",
            ["Lineas[0].CantKgs"] = kgs
        };

        [Fact]
        public async Task Doble_envio_simultaneo_ejecuta_una_sola_vez_y_los_dos_reciben_el_resultado()
        {
            var registro = new RegistroGuardadoUnico();
            var tempData = new TempDataDictionaryFactory(new TempDataEnMemoria());
            int ejecuciones = 0;

            var ctx1 = CrearContexto(FormStock());
            var ctx2 = CrearContexto(FormStock());
            var t1 = CrearFiltro(registro, tempData).OnActionExecutionAsync(ctx1, Accion(ctx1, () => new JsonResult(new { ok = true, id = 99 }), () => Interlocked.Increment(ref ejecuciones)));
            var t2 = CrearFiltro(registro, tempData).OnActionExecutionAsync(ctx2, Accion(ctx2, () => new JsonResult(new { ok = true, id = 100 }), () => Interlocked.Increment(ref ejecuciones)));
            await Task.WhenAll(t1, t2);

            Assert.Equal(1, ejecuciones);
            // El que no ejecuto recibe el resultado del que si (mismo id, no otro registro).
            var repetido = Assert.IsType<JsonResult>((ctx1.Result ?? ctx2.Result));
            Assert.Contains("99", System.Text.Json.JsonSerializer.Serialize(repetido.Value));
        }

        [Fact]
        public async Task Reintento_posterior_con_los_mismos_datos_no_ejecuta()
        {
            var registro = new RegistroGuardadoUnico();
            var tempData = new TempDataDictionaryFactory(new TempDataEnMemoria());
            int ejecuciones = 0;

            var ctx1 = CrearContexto(FormStock());
            await CrearFiltro(registro, tempData).OnActionExecutionAsync(ctx1, Accion(ctx1, () => new JsonResult(new { ok = true }), () => ejecuciones++, 0));
            var ctx2 = CrearContexto(FormStock());
            await CrearFiltro(registro, tempData).OnActionExecutionAsync(ctx2, Accion(ctx2, () => new JsonResult(new { ok = true }), () => ejecuciones++, 0));

            Assert.Equal(1, ejecuciones);
            Assert.IsType<JsonResult>(ctx2.Result);
        }

        [Fact]
        public async Task Otros_datos_con_el_mismo_token_si_se_guardan()
        {
            var registro = new RegistroGuardadoUnico();
            var tempData = new TempDataDictionaryFactory(new TempDataEnMemoria());
            int ejecuciones = 0;

            var ctx1 = CrearContexto(FormStock(kgs: "2,5"));
            await CrearFiltro(registro, tempData).OnActionExecutionAsync(ctx1, Accion(ctx1, () => new JsonResult(new { ok = true }), () => ejecuciones++, 0));
            var ctx2 = CrearContexto(FormStock(kgs: "3"));
            await CrearFiltro(registro, tempData).OnActionExecutionAsync(ctx2, Accion(ctx2, () => new JsonResult(new { ok = true }), () => ejecuciones++, 0));

            Assert.Equal(2, ejecuciones);
        }

        [Fact]
        public async Task Si_el_primero_devuelve_error_el_reintento_ejecuta()
        {
            var registro = new RegistroGuardadoUnico();
            var tempData = new TempDataDictionaryFactory(new TempDataEnMemoria());
            int ejecuciones = 0;

            var ctx1 = CrearContexto(FormStock());
            await CrearFiltro(registro, tempData).OnActionExecutionAsync(ctx1, Accion(ctx1, () => new JsonResult(new { ok = false, mensaje = "caja cerrada" }), () => ejecuciones++, 0));
            var ctx2 = CrearContexto(FormStock());
            await CrearFiltro(registro, tempData).OnActionExecutionAsync(ctx2, Accion(ctx2, () => new JsonResult(new { ok = true }), () => ejecuciones++, 0));

            Assert.Equal(2, ejecuciones);
        }

        [Fact]
        public async Task Si_la_accion_tira_excepcion_se_libera()
        {
            var registro = new RegistroGuardadoUnico();
            var tempData = new TempDataDictionaryFactory(new TempDataEnMemoria());

            var ctx1 = CrearContexto(FormStock());
            await Assert.ThrowsAsync<InvalidOperationException>(() => CrearFiltro(registro, tempData).OnActionExecutionAsync(ctx1,
                () => throw new InvalidOperationException("se corto la base")));
            Assert.Equal(0, registro.Cantidad);
        }

        [Fact]
        public async Task Stock_post_tradicional_el_repetido_redirige_al_listado_con_el_aviso_de_guardado()
        {
            var registro = new RegistroGuardadoUnico();
            var tempData = new TempDataDictionaryFactory(new TempDataEnMemoria());
            int ejecuciones = 0;

            var ctx1 = CrearContexto(FormStock(), ajax: false);
            await CrearFiltro(registro, tempData).OnActionExecutionAsync(ctx1, Accion(ctx1, () =>
            {
                var td = tempData.GetTempData(ctx1.HttpContext);
                td["AlertType"] = "success";
                td["AlertMsg"] = "El movimiento de stock se registró correctamente.";
                return new RedirectToActionResult("Index", null, null);
            }, () => ejecuciones++, 0));

            var ctx2 = CrearContexto(FormStock(), ajax: false);
            await CrearFiltro(registro, tempData).OnActionExecutionAsync(ctx2, Accion(ctx2, () => new RedirectToActionResult("Index", null, null), () => ejecuciones++, 0));

            Assert.Equal(1, ejecuciones);
            var redirect = Assert.IsType<RedirectResult>(ctx2.Result);
            Assert.Equal("/Stock/Index", redirect.Url);
            Assert.Equal("success", tempData.GetTempData(ctx2.HttpContext).Peek("AlertType"));
        }

        [Fact]
        public async Task Stock_redireccion_con_aviso_de_error_no_cuenta_como_guardado()
        {
            var registro = new RegistroGuardadoUnico();
            var tempData = new TempDataDictionaryFactory(new TempDataEnMemoria());
            int ejecuciones = 0;

            var ctx1 = CrearContexto(FormStock(), ajax: false);
            await CrearFiltro(registro, tempData).OnActionExecutionAsync(ctx1, Accion(ctx1, () =>
            {
                tempData.GetTempData(ctx1.HttpContext)["AlertType"] = "error";
                return new RedirectToActionResult("Index", null, null);
            }, () => ejecuciones++, 0));
            var ctx2 = CrearContexto(FormStock(), ajax: false);
            await CrearFiltro(registro, tempData).OnActionExecutionAsync(ctx2, Accion(ctx2, () => new RedirectToActionResult("Index", null, null), () => ejecuciones++, 0));

            Assert.Equal(2, ejecuciones);
        }

        [Fact]
        public async Task Sin_token_igual_frena_el_doble_clic()
        {
            var registro = new RegistroGuardadoUnico();
            var tempData = new TempDataDictionaryFactory(new TempDataEnMemoria());
            int ejecuciones = 0;

            var ctx1 = CrearContexto(FormStock(token: ""));
            var ctx2 = CrearContexto(FormStock(token: ""));
            await Task.WhenAll(
                CrearFiltro(registro, tempData).OnActionExecutionAsync(ctx1, Accion(ctx1, () => new JsonResult(new { ok = true }), () => Interlocked.Increment(ref ejecuciones))),
                CrearFiltro(registro, tempData).OnActionExecutionAsync(ctx2, Accion(ctx2, () => new JsonResult(new { ok = true }), () => Interlocked.Increment(ref ejecuciones))));

            Assert.Equal(1, ejecuciones);
        }
    }
}
