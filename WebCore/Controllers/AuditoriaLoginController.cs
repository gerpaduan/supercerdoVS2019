// Port de Web/Controllers/AuditoriaLoginController.cs (ver docs/DECISIONS.md, migracion a
// ASP.NET Core). Misma logica de negocio y mapeo que el original, MISMA Negocio.Usuario
// compartida. Gate real portado 2026-09-06 (Batch 4 del plan de login/permisos reales, ver
// docs/DECISIONS.md): PuedeVerAuditoria usa el mismo criterio que el original (Admin, o permiso
// de EDICION -- no solo "ver" -- sobre Entidades.Permisos.Usuario.NuevoUsuario, deliberadamente
// mas estricto que "administrar usuarios" del sidebar, ver comentario original), ahora contra
// IUsuarioSesionService.UsuarioActual en vez de Session["Usuario"].
using System;
using System.Data;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Utilidades;
using WebCore.Models;

namespace WebCore.Controllers
{
    public class AuditoriaLoginController : Controller
    {
        private readonly WebCore.Services.IUsuarioSesionService _sesion;

        public AuditoriaLoginController(WebCore.Services.IUsuarioSesionService sesion)
        {
            _sesion = sesion;
        }

        [HttpGet]
        public IActionResult Index(DateTime? fechaDesde, DateTime? fechaHasta)
        {
            var usuario = _sesion.UsuarioActual;
            var empresa = _sesion.Empresa;
            var oUsuarioN = WebCore.Infrastructure.NegocioFactory.CrearUsuario(empresa);

            if (!PuedeVerAuditoria(usuario, oUsuarioN))
            {
                ViewBag.Title = "Auditoria de accesos";
                ViewBag.Seccion = "Auditoria de accesos";
                return View("~/Views/Shared/AccesoDenegado.cshtml");
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

            return View(model);
        }

        // Port de Web/Controllers/AuditoriaLoginController.cs (PuedeVerAuditoria) -- deliberadamente
        // mas estricto que "administrar usuarios" del sidebar: solo mira el permiso de EDICION
        // (idCreador=usuario.Id, no -1) sobre NuevoUsuario, no el de "ver" ni VerUsuarios.
        private bool PuedeVerAuditoria(Entidades.Usuario usuario, Negocio.Usuario oUsuarioN)
        {
            if (usuario == null) return false;
            if (usuario.Admin) return true;
            if (usuario.Permisos == null || usuario.Permisos.Count == 0) return false;
            return oUsuarioN.tienePermiso(usuario, Entidades.Permisos.Usuario.NuevoUsuario, DateTime.Today, usuario.Id);
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
    }
}
