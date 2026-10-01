using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.DependencyInjection;
using WebCore.Services;

namespace WebCore.Helpers
{
    // Cuenta corriente reservada (docs/DECISIONS.md, 2026-10-01): bloquea el acceso directo (por URL,
    // por id) a un registro de una persona reservada que el usuario restringido no puede ver.
    // Se pone en la accion indicando la tabla (RestriccionCtaCteReservada.Tabla*) y el nombre del
    // parametro de la accion que trae el id del registro. Si el parametro no esta o es <= 0 (ej.
    // POS sin venta a editar) no hace nada.
    //   [ProtegerRegistroReservado(Entidades.RestriccionCtaCteReservada.TablaVentas, "id")]
    // Para ids que no son del registro (ej. id de factura) el chequeo se hace a mano en la accion.
    [System.AttributeUsage(System.AttributeTargets.Method, AllowMultiple = false)]
    public sealed class ProtegerRegistroReservadoAttribute : TypeFilterAttribute
    {
        public ProtegerRegistroReservadoAttribute(string tabla, string parametroId = "id")
            : base(typeof(ProtegerRegistroReservadoFilter))
        {
            Arguments = new object[] { tabla, parametroId };
        }
    }

    public sealed class ProtegerRegistroReservadoFilter : IActionFilter
    {
        public const string MensajeDenegado = "Este registro pertenece a una cuenta corriente reservada. Solo lo puede ver quien lo cargó desde la apertura de su caja o un usuario con permiso de cuentas corrientes.";

        private readonly string _tabla;
        private readonly string _parametroId;
        private readonly ICtaCteReservadaService _reservada;

        public ProtegerRegistroReservadoFilter(string tabla, string parametroId, ICtaCteReservadaService reservada)
        {
            _tabla = tabla;
            _parametroId = parametroId;
            _reservada = reservada;
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.ActionArguments.TryGetValue(_parametroId, out var valor) || !(valor is int id) || id <= 0)
                return;

            if (!_reservada.RegistroOculto(_tabla, id))
                return;

            context.Result = Denegar(context);
        }

        public void OnActionExecuted(ActionExecutedContext context) { }

        // Resultado comun para el filtro y para los chequeos manuales de los controllers: 403 JSON
        // para pedidos AJAX/fetch, la pantalla "Acceso Denegado" para navegacion normal.
        public static IActionResult Denegar(ActionContext context)
        {
            var request = context.HttpContext.Request;
            bool esAjax = string.Equals(request.Headers["X-Requested-With"], "XMLHttpRequest", System.StringComparison.OrdinalIgnoreCase)
                || (request.Headers["Accept"].ToString().Contains("application/json"));

            if (esAjax)
                return new JsonResult(new { ok = false, success = false, msg = MensajeDenegado, message = MensajeDenegado }) { StatusCode = 403 };

            var metadata = context.HttpContext.RequestServices.GetRequiredService<IModelMetadataProvider>();
            var viewData = new ViewDataDictionary(metadata, new ModelStateDictionary())
            {
                ["MensajePermiso"] = MensajeDenegado
            };
            return new ViewResult
            {
                ViewName = "~/Views/Shared/AccesoDenegado.cshtml",
                ViewData = viewData,
                StatusCode = 403
            };
        }
    }
}
