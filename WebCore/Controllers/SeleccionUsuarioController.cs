using System;
using System.Data;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Utilidades;

namespace WebCore.Controllers
{
    // Port de Web/Controllers/SeleccionUsuarioController.cs (2026-09-09, ver docs/DECISIONS.md
    // "Batch B: permisos reales + operador de produccion"). Pantalla compartida de "quien esta
    // haciendo esto" (SIN contrasena) para Movimientos/Stock/Elaborados cuando la sesion es la
    // cuenta compartida de produccion -- se pide ANTES de entrar a la vista de edicion. Distinto
    // del step-up CON contrasena de Ventas/Compras/PuntosExpendio (ResolverOperadorModulo/POS,
    // ya portado): aca no se valida nada del lado del servidor, el id elegido se revalida igual
    // que siempre al momento de guardar (ResolverUsuarioCreador, ya portado en cada controller).
    // Reusa el mismo _ModalSeleccionUsuario.cshtml/seleccion-usuario.js que Ventas/PuntosExpendio.
    public class SeleccionUsuarioController : Controller
    {
        private readonly WebCore.Services.IUsuarioSesionService _sesion;
        private readonly IEmpresaContext _empresa;
        private readonly IParametrosContext _param;

        public SeleccionUsuarioController(WebCore.Services.IUsuarioSesionService sesion)
        {
            _sesion = sesion;
            _empresa = sesion.Empresa;
            _param = WebCore.Infrastructure.NegocioFactory.CrearParametros(_empresa);
            _param.Reload();
        }

        public IActionResult Index(string returnUrl, string cancelUrl)
        {
            var user = _sesion.UsuarioActual;

            ViewBag.UsuariosActivosEmpresa = ObtenerUsuariosActivosEmpresaParaCombo(user);
            ViewBag.ReturnUrl = Url.IsLocalUrl(returnUrl) ? returnUrl : Url.Action("Index", "Home");
            ViewBag.CancelUrl = Url.IsLocalUrl(cancelUrl) ? cancelUrl : Url.Action("Index", "Home");
            ViewBag.Title = "Seleccionar usuario";

            return View("~/Views/SeleccionUsuario/Index.cshtml");
        }

        // Mismo criterio que el resto de los controllers de esta migracion (Movimientos/Stock/
        // Elaborados/Compras/Cajas ya lo duplican cada uno por su cuenta -- no hay BaseController
        // compartido en WebCore, ver comentario de cabecera de cualquiera de esos archivos).
        private System.Collections.Generic.List<object> ObtenerUsuariosActivosEmpresaParaCombo(Entidades.Usuario usuarioSesion)
        {
            var oUsuarioN = WebCore.Infrastructure.NegocioFactory.CrearUsuario(_empresa, _param);
            var dt = oUsuarioN.obtenerUsuarios(true);
            if (dt == null || !dt.Columns.Contains("id") || !dt.Columns.Contains("nombre"))
                return new System.Collections.Generic.List<object>();

            return dt.AsEnumerable()
                .Select(row => new
                {
                    id = dt.Columns.Contains("id") && int.TryParse(Convert.ToString(row["id"]), out int id) ? id : 0,
                    nombre = dt.Columns.Contains("nombre") ? Convert.ToString(row["nombre"]) ?? "" : ""
                })
                .Where(u => u.id > 0 && !string.IsNullOrWhiteSpace(u.nombre))
                .OrderBy(u => u.nombre, StringComparer.OrdinalIgnoreCase)
                .Cast<object>()
                .ToList();
        }
    }
}
