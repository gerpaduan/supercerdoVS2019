// Evita que un mismo guardado se grabe dos veces (doble clic, Enter repetido, reintento tras perder la respuesta).
// Un pedido repetido -- mismo usuario, misma accion, mismo TokenGuardado de la pantalla y MISMOS datos -- no vuelve a
// ejecutar la accion: espera al primero y devuelve su mismo resultado. Vive en memoria del proceso, como la Session.
// Ver docs/DECISIONS.md (2026-11-26, "Guardado unico") y docs/TROUBLESHOOTING.md.
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;

namespace WebCore.Helpers
{
    // Lo que el primer pedido dejo para devolverle a los repetidos: el resultado ya listo para volver a ejecutarse
    // (JSON o redireccion a una URL fija) y los avisos de TempData que puso la accion (ej. "Stock guardado").
    public sealed class ResultadoGuardado
    {
        public ResultadoGuardado(IActionResult resultado, IReadOnlyDictionary<string, object?> tempData)
        {
            Resultado = resultado;
            TempData = tempData;
        }

        public IActionResult Resultado { get; }
        public IReadOnlyDictionary<string, object?> TempData { get; }
    }

    public enum TipoTurnoGuardado
    {
        // Nadie esta guardando esto: el pedido ejecuta la accion y despues llama a Completar o Liberar.
        Ejecutar,
        // Ya se guardo hace poco: se devuelve ese mismo resultado sin grabar nada.
        Repetir,
        // El primero sigue guardando despues de esperar el maximo: se avisa en vez de grabar otro.
        EsperaAgotada
    }

    public sealed class TurnoGuardado
    {
        internal TurnoGuardado(TipoTurnoGuardado tipo, ResultadoGuardado? resultado)
        {
            Tipo = tipo;
            Resultado = resultado;
        }

        public TipoTurnoGuardado Tipo { get; }
        // Solo con Tipo = Repetir.
        public ResultadoGuardado? Resultado { get; }
    }

    // Registro en memoria (singleton) de los guardados en curso y de los recien terminados. No sabe nada de MVC:
    // asi se prueba solo (WebCore.Tests/GuardadoUnicoTests.cs).
    // Limite conocido: es por proceso. Un reinicio o un reciclado de IIS lo olvida, y con varias instancias haria
    // falta una tabla (hoy la Session tambien es en memoria, asi que hay una sola instancia por instalacion).
    public sealed class RegistroGuardadoUnico
    {
        private sealed class Entrada
        {
            public TaskCompletionSource<bool> Termino { get; } = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            public ResultadoGuardado? Resultado { get; set; }
            public System.DateTime? VenceUtc { get; set; }
        }

        // Cada cuanto se barren las entradas vencidas (se hace al pasar, sin timers).
        private static readonly System.TimeSpan IntervaloLimpieza = System.TimeSpan.FromMinutes(1);

        private readonly object _lock = new object();
        private readonly Dictionary<string, Entrada> _entradas = new Dictionary<string, Entrada>(System.StringComparer.Ordinal);
        private readonly System.Func<System.DateTime> _ahoraUtc;
        private System.DateTime _proximaLimpiezaUtc;

        public RegistroGuardadoUnico() : this(() => System.DateTime.UtcNow) { }

        // El reloj se inyecta para los tests (vencimiento de la ventana sin esperar minutos reales).
        public RegistroGuardadoUnico(System.Func<System.DateTime> ahoraUtc)
        {
            _ahoraUtc = ahoraUtc;
            _proximaLimpiezaUtc = ahoraUtc() + IntervaloLimpieza;
        }

        // Cantidad de entradas guardadas (solo para tests y diagnostico).
        public int Cantidad
        {
            get { lock (_lock) return _entradas.Count; }
        }

        // Pide turno para la clave. Si hay otro pedido igual en curso, espera (hasta esperaMax) a que termine:
        // si salio bien devuelve Repetir con su resultado; si fallo, este pedido pasa a ejecutar.
        public async Task<TurnoGuardado> TomarTurnoAsync(string clave, System.TimeSpan esperaMax)
        {
            var limite = _ahoraUtc() + esperaMax;
            while (true)
            {
                Task esperar;
                lock (_lock)
                {
                    LimpiarVencidasSiToca();
                    if (!_entradas.TryGetValue(clave, out var entrada) || EstaVencida(entrada))
                    {
                        _entradas[clave] = new Entrada();
                        return new TurnoGuardado(TipoTurnoGuardado.Ejecutar, null);
                    }

                    if (entrada.Resultado != null)
                        return new TurnoGuardado(TipoTurnoGuardado.Repetir, entrada.Resultado);

                    esperar = entrada.Termino.Task;
                }

                // El primero sigue guardando: se espera fuera del lock a que complete o libere.
                var restante = limite - _ahoraUtc();
                if (restante <= System.TimeSpan.Zero)
                    return new TurnoGuardado(TipoTurnoGuardado.EsperaAgotada, null);
                var ganador = await Task.WhenAny(esperar, Task.Delay(restante)).ConfigureAwait(false);
                if (ganador != esperar)
                    return new TurnoGuardado(TipoTurnoGuardado.EsperaAgotada, null);
                // Termino: se vuelve a mirar el estado (Repetir si salio bien, Ejecutar si se libero).
            }
        }

        // El guardado salio bien: los pedidos iguales que lleguen durante "ventana" reciben este resultado.
        public void Completar(string clave, ResultadoGuardado resultado, System.TimeSpan ventana)
        {
            Entrada? entrada;
            lock (_lock)
            {
                if (!_entradas.TryGetValue(clave, out entrada)) return;
                entrada.Resultado = resultado;
                entrada.VenceUtc = _ahoraUtc() + ventana;
            }
            entrada.Termino.TrySetResult(true);
        }

        // El guardado fallo (validacion, permiso, excepcion): se olvida la clave para que un reintento ejecute.
        public void Liberar(string clave)
        {
            Entrada? entrada;
            lock (_lock)
            {
                if (!_entradas.TryGetValue(clave, out entrada)) return;
                _entradas.Remove(clave);
            }
            entrada.Termino.TrySetResult(false);
        }

        private bool EstaVencida(Entrada entrada)
        {
            return entrada.VenceUtc.HasValue && entrada.VenceUtc.Value <= _ahoraUtc();
        }

        // Llamar con el lock tomado. Las entradas en curso no vencen: las cierra siempre el finally del filtro.
        private void LimpiarVencidasSiToca()
        {
            var ahora = _ahoraUtc();
            if (ahora < _proximaLimpiezaUtc) return;
            _proximaLimpiezaUtc = ahora + IntervaloLimpieza;
            foreach (var clave in _entradas.Where(par => EstaVencida(par.Value)).Select(par => par.Key).ToList())
                _entradas.Remove(clave);
        }
    }

    // Se pone en la accion de guardado:  [EvitarGuardadoDoble]
    // Va despues de [ValidateAntiForgeryToken] (los filtros de autorizacion corren antes que los de accion).
    [System.AttributeUsage(System.AttributeTargets.Method, AllowMultiple = false)]
    public sealed class EvitarGuardadoDobleAttribute : TypeFilterAttribute
    {
        public EvitarGuardadoDobleAttribute() : base(typeof(EvitarGuardadoDobleFilter)) { }
    }

    public sealed class EvitarGuardadoDobleFilter : IAsyncActionFilter
    {
        // Campo oculto que cada pantalla de guardado trae con un Guid nuevo por cada vez que se dibuja.
        public const string CampoToken = "TokenGuardado";
        public const string MensajeEnCurso = "Ese guardado ya se está procesando. Esperá unos segundos y revisá el listado antes de volver a guardar.";

        // Campos que cambian entre dos envios del mismo formulario sin que cambien los datos.
        private static readonly HashSet<string> CamposExcluidosDeLaHuella = new HashSet<string>(System.StringComparer.OrdinalIgnoreCase)
        {
            "__RequestVerificationToken",
            CampoToken
        };

        private readonly RegistroGuardadoUnico _registro;
        private readonly WebCore.Services.IUsuarioSesionService _sesion;
        private readonly ITempDataDictionaryFactory _tempDataFactory;
        private readonly IUrlHelperFactory _urlHelperFactory;
        private readonly ILogger<EvitarGuardadoDobleFilter> _log;

        public EvitarGuardadoDobleFilter(
            RegistroGuardadoUnico registro,
            WebCore.Services.IUsuarioSesionService sesion,
            ITempDataDictionaryFactory tempDataFactory,
            IUrlHelperFactory urlHelperFactory,
            ILogger<EvitarGuardadoDobleFilter> log)
        {
            _registro = registro;
            _sesion = sesion;
            _tempDataFactory = tempDataFactory;
            _urlHelperFactory = urlHelperFactory;
            _log = log;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var request = context.HttpContext.Request;
            var usuario = _sesion.UsuarioActual;
            // Sin formulario o sin usuario no hay nada que comparar: sigue como siempre.
            if (!request.HasFormContentType || usuario == null)
            {
                await next();
                return;
            }

            var form = await request.ReadFormAsync();
            string token = (form[CampoToken].ToString() ?? "").Trim();
            string accion = context.ActionDescriptor.RouteValues.TryGetValue("controller", out var ctrl) ? ctrl + "/" : "";
            accion += context.ActionDescriptor.RouteValues.TryGetValue("action", out var act) ? act : "";
            string clave = ArmarClave(usuario.IdEmpresa, usuario.Id, accion, token,
                form.Select(par => new KeyValuePair<string, IEnumerable<string>>(par.Key, par.Value.Select(v => v ?? ""))));

            // Sin token (pantalla vieja en cache tras un deploy) se frena igual el doble clic, pero por menos tiempo.
            var ventana = System.TimeSpan.FromSeconds(token.Length > 0 ? GuardadoUnicoSettings.VentanaSegundos : GuardadoUnicoSettings.VentanaSinTokenSegundos);
            var turno = await _registro.TomarTurnoAsync(clave, System.TimeSpan.FromSeconds(GuardadoUnicoSettings.EsperaMaxSegundos));

            if (turno.Tipo == TipoTurnoGuardado.Repetir && turno.Resultado != null)
            {
                _log.LogWarning("Guardado repetido frenado en {Accion} (empresa {IdEmpresa}, usuario {IdUsuario}): se devuelve el resultado del primero.",
                    accion, usuario.IdEmpresa, usuario.Id);
                var tempData = _tempDataFactory.GetTempData(context.HttpContext);
                foreach (var par in turno.Resultado.TempData)
                    tempData[par.Key] = par.Value;
                context.Result = turno.Resultado.Resultado;
                return;
            }

            if (turno.Tipo == TipoTurnoGuardado.EsperaAgotada)
            {
                _log.LogWarning("Guardado repetido en {Accion} (empresa {IdEmpresa}, usuario {IdUsuario}) mientras el primero seguia en curso: no se ejecuta.",
                    accion, usuario.IdEmpresa, usuario.Id);
                context.Result = RespuestaEnCurso(context);
                return;
            }

            // Turno para ejecutar: se toma una foto de TempData para saber despues que avisos agrego la accion.
            var tempDataAntes = FotoTempData(context);
            bool completado = false;
            try
            {
                var ejecutado = await next();
                if (ejecutado.Exception == null || ejecutado.ExceptionHandled)
                {
                    var guardado = ResultadoParaRepetir(ejecutado);
                    if (guardado != null)
                    {
                        _registro.Completar(clave, new ResultadoGuardado(guardado, AvisosNuevos(context, tempDataAntes)), ventana);
                        completado = true;
                    }
                }
            }
            finally
            {
                // Cualquier otro final (error, validacion, excepcion) libera: un reintento tiene que poder guardar.
                if (!completado) _registro.Liberar(clave);
            }
        }

        // Clave del guardado: empresa + usuario + accion + token + huella SHA-256 de los campos (ordenados, sin los
        // excluidos). Publica y estatica para poder probarla sin HttpContext.
        public static string ArmarClave(int idEmpresa, int idUsuario, string accion, string token, IEnumerable<KeyValuePair<string, IEnumerable<string>>> campos)
        {
            var texto = new StringBuilder();
            foreach (var campo in campos.Where(c => !CamposExcluidosDeLaHuella.Contains(c.Key)).OrderBy(c => c.Key, System.StringComparer.Ordinal))
            {
                foreach (var valor in campo.Value)
                {
                    // Largo + contenido: "a=b&c" y "a=b,c" no pueden dar la misma huella.
                    texto.Append(campo.Key.Length).Append(':').Append(campo.Key).Append('=')
                         .Append(valor.Length).Append(':').Append(valor).Append('\n');
                }
            }
            string huella = System.Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(texto.ToString())));
            return idEmpresa + "|" + idUsuario + "|" + accion + "|" + token + "|" + huella;
        }

        // Devuelve el resultado listo para devolverle a un repetido, o null si la accion no termino bien (entonces
        // se libera). Bien = redireccion sin aviso de error, o JSON con ok = true.
        private IActionResult? ResultadoParaRepetir(ActionExecutedContext ejecutado)
        {
            switch (ejecutado.Result)
            {
                case JsonResult json:
                    return EsJsonOk(json.Value) ? json : null;
                case RedirectToActionResult redirect:
                    if (TieneAvisoDeError(ejecutado)) return null;
                    // Se resuelve la URL ahora: el resultado se vuelve a ejecutar en otro pedido.
                    var url = _urlHelperFactory.GetUrlHelper(ejecutado).Action(redirect.ActionName, redirect.ControllerName, redirect.RouteValues);
                    return string.IsNullOrEmpty(url) ? null : new RedirectResult(url);
                case RedirectResult redirectUrl:
                    return TieneAvisoDeError(ejecutado) ? null : redirectUrl;
                case LocalRedirectResult local:
                    return TieneAvisoDeError(ejecutado) ? null : local;
                default:
                    return null;
            }
        }

        // Las acciones de guardado devuelven objetos anonimos con "ok" (ej. new { ok = true, ... }): se lee por
        // reflexion porque no hay un tipo comun. Sin propiedad ok, no se considera exito (no se repite nada).
        public static bool EsJsonOk(object? valor)
        {
            if (valor == null) return false;
            var propiedad = valor.GetType().GetProperty("ok");
            return propiedad != null && propiedad.GetValue(valor) is bool ok && ok;
        }

        // Stock redirige al listado tambien en algunos errores (ej. "no se encontro"), con AlertType = error.
        private bool TieneAvisoDeError(ActionContext context)
        {
            var tempData = _tempDataFactory.GetTempData(context.HttpContext);
            return string.Equals(tempData.Peek("AlertType") as string, "error", System.StringComparison.OrdinalIgnoreCase);
        }

        private Dictionary<string, object?> FotoTempData(ActionContext context)
        {
            var tempData = _tempDataFactory.GetTempData(context.HttpContext);
            return tempData.Keys.ToList().ToDictionary(k => k, k => tempData.Peek(k), System.StringComparer.OrdinalIgnoreCase);
        }

        // Lo que la accion agrego o cambio en TempData (los avisos de "guardado correctamente").
        private IReadOnlyDictionary<string, object?> AvisosNuevos(ActionContext context, Dictionary<string, object?> antes)
        {
            var tempData = _tempDataFactory.GetTempData(context.HttpContext);
            var nuevos = new Dictionary<string, object?>(System.StringComparer.OrdinalIgnoreCase);
            foreach (var clave in tempData.Keys.ToList())
            {
                var valor = tempData.Peek(clave);
                if (!antes.TryGetValue(clave, out var anterior) || !Equals(anterior, valor))
                    nuevos[clave] = valor;
            }
            return nuevos;
        }

        // El primero sigue guardando despues de la espera maxima: JSON para las pantallas AJAX, aviso en el listado
        // para el POST tradicional de Stock.
        private IActionResult RespuestaEnCurso(ActionExecutingContext context)
        {
            var request = context.HttpContext.Request;
            bool esAjax = string.Equals(request.Headers["X-Requested-With"], "XMLHttpRequest", System.StringComparison.OrdinalIgnoreCase)
                || request.Headers["Accept"].ToString().Contains("application/json");
            if (esAjax)
                return new JsonResult(new { ok = false, mensaje = MensajeEnCurso });

            var tempData = _tempDataFactory.GetTempData(context.HttpContext);
            tempData["AlertType"] = "warning";
            tempData["AlertTitle"] = "Guardando";
            tempData["AlertMsg"] = MensajeEnCurso;
            return new RedirectToActionResult("Index", null, null);
        }
    }

    // Configuracion, leida de App.config (appSettings "GuardadoUnico:*"), mismo mecanismo que BorradorGenericoSettings.
    public static class GuardadoUnicoSettings
    {
        // Cuanto tiempo se reconoce un guardado repetido con el token de la pantalla (cubre el reintento manual
        // despues de que se corto la respuesta).
        private const int VentanaSegundosDefault = 600;
        // Sin token (pantalla vieja en cache): solo el doble clic / Enter repetido.
        private const int VentanaSinTokenSegundosDefault = 30;
        // Cuanto espera un repetido a que termine el primero antes de avisar "ya se esta guardando".
        private const int EsperaMaxSegundosDefault = 30;

        public static int VentanaSegundos => LeerEntero("GuardadoUnico:VentanaSegundos", VentanaSegundosDefault, 5, 86400);
        public static int VentanaSinTokenSegundos => LeerEntero("GuardadoUnico:VentanaSinTokenSegundos", VentanaSinTokenSegundosDefault, 5, 600);
        public static int EsperaMaxSegundos => LeerEntero("GuardadoUnico:EsperaMaxSegundos", EsperaMaxSegundosDefault, 1, 120);

        // Lee un entero de config; si falta o es invalido usa el default, y lo recorta a [min, max].
        private static int LeerEntero(string clave, int porDefecto, int min, int max)
        {
            string texto = System.Configuration.ConfigurationManager.AppSettings[clave] ?? "";
            if (!int.TryParse(texto.Trim(), System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out int valor))
                return porDefecto;
            if (valor < min) return min;
            if (valor > max) return max;
            return valor;
        }
    }
}
