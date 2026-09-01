// Port de spike de Web/Controllers/AuditoriaLoginController.cs (ver docs/DECISIONS.md,
// migracion a ASP.NET Core). Misma logica de negocio y mapeo que el original, MISMA
// Negocio.Usuario compartida. Diferencia deliberada: usa un IEmpresaContext hardcodeado en vez
// de Session["Usuario"] (todavia no hay login/sesion en WebCore) y NO reproduce el chequeo de
// permisos PuedeVerAuditoria (depende de sesion real) -- ambos son parte del diseño de
// autenticacion pendiente (Forms Auth -> Cookie Auth), no de esta prueba de paridad puntual.
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
        private sealed class SpikeEmpresaContext : IEmpresaContext
        {
            public int IdEmpresa => 1;
        }

        [HttpGet]
        public IActionResult Index(DateTime? fechaDesde, DateTime? fechaHasta)
        {
            var empresa = new SpikeEmpresaContext();
            var oUsuarioN = new Negocio.Usuario(empresa);

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
