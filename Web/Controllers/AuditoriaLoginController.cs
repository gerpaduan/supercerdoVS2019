using System;
using System.Data;
using System.Linq;
using System.Web.Mvc;
using Web.Models;

namespace Web.Controllers
{
    // Pantalla de auditoria de logins: quien se logueo, cuando y desde donde (coordenadas GPS,
    // con link a Google Maps). Gateada estrictamente por el permiso de "crear usuarios"
    // (Entidades.Permisos.Usuario.NuevoUsuario) o Admin -- pedido explicito, deliberadamente MAS
    // estricto que "puedeAdministrarUsuarios" del layout (ese tambien deja pasar con el permiso
    // de solo-ver-usuarios, VerUsuarios, que NO debe dar acceso aca).
    public class AuditoriaLoginController : BaseController
    {
        private Negocio.Usuario oUsuarioN;

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);
            if (filterContext.Result != null) return;

            oUsuarioN = new Negocio.Usuario(empresa, param);
        }

        [HttpGet]
        public ActionResult Index(DateTime? fechaDesde, DateTime? fechaHasta)
        {
            var usuario = Session["Usuario"] as Entidades.Usuario;
            if (usuario == null || usuario.IdEmpresa != empresa.IdEmpresa)
            {
                return VistaAccesoDenegado("Auditoría de accesos");
            }

            if (!PuedeVerAuditoria(usuario))
            {
                return VistaAccesoDenegado("Auditoría de accesos");
            }

            DateTime desde = (fechaDesde ?? DateTime.Today.AddDays(-7)).Date;
            DateTime hasta = (fechaHasta ?? DateTime.Today).Date.AddDays(1).AddSeconds(-1);

            DataTable dt = oUsuarioN.obtenerLoginUbicacionLog(empresa.IdEmpresa, desde, hasta) ?? new DataTable();

            var model = new AuditoriaLoginIndexVm
            {
                FechaDesde = desde.ToString("yyyy-MM-dd"),
                FechaHasta = hasta.Date.ToString("yyyy-MM-dd"),
                Items = dt.AsEnumerable().Select(MapItem).ToList()
            };

            ViewBag.Title = "Auditoría de accesos";
            ViewBag.Seccion = "Auditoría de accesos";
            return View("~/Views/AuditoriaLogin/Index.cshtml", model);
        }

        private AuditoriaLoginItemVm MapItem(DataRow row)
        {
            return new AuditoriaLoginItemVm
            {
                UsuarioNombre = row["UsuarioNombre"] != DBNull.Value ? Convert.ToString(row["UsuarioNombre"]) : "",
                SucursalNombre = row["SucursalNombre"] != DBNull.Value ? Convert.ToString(row["SucursalNombre"]) : "",
                FechaHora = row["FechaHora"] != DBNull.Value ? Convert.ToDateTime(row["FechaHora"]) : DateTime.MinValue,
                Latitud = row["Latitud"] != DBNull.Value ? Convert.ToDecimal(row["Latitud"]) : (decimal?)null,
                Longitud = row["Longitud"] != DBNull.Value ? Convert.ToDecimal(row["Longitud"]) : (decimal?)null,
                PrecisionMetros = row["PrecisionMetros"] != DBNull.Value ? Convert.ToDecimal(row["PrecisionMetros"]) : (decimal?)null,
                DistanciaMetros = row["DistanciaMetros"] != DBNull.Value ? Convert.ToDecimal(row["DistanciaMetros"]) : (decimal?)null,
                Permitido = row["Permitido"] != DBNull.Value && Convert.ToBoolean(row["Permitido"]),
                Motivo = row["Motivo"] != DBNull.Value ? Convert.ToString(row["Motivo"]) : "",
                Ip = row["Ip"] != DBNull.Value ? Convert.ToString(row["Ip"]) : ""
            };
        }

        private bool PuedeVerAuditoria(Entidades.Usuario usuario)
        {
            if (usuario == null)
                return false;

            if (usuario.Admin)
                return true;

            if (usuario.Permisos == null || usuario.Permisos.Count == 0)
                return false;

            return oUsuarioN.tienePermiso(usuario, Entidades.Permisos.Usuario.NuevoUsuario, DateTime.Today, usuario.Id);
        }
    }
}
