// Consulta de los registros eliminados (Compras, Stock y Movimientos): quien los elimino, cuando, el motivo y todo lo que tenian.
// Solo administradores. La eliminacion en si vive en Compras/Stock/Movimientos (accion Eliminar). Ver docs/DECISIONS.md
// "Eliminar compras, stock y movimientos".
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using WebCore.Models;

namespace WebCore.Controllers
{
    public class RegistrosEliminadosController : Controller
    {
        private readonly WebCore.Services.IUsuarioSesionService _sesion;
        private readonly Negocio.AuditoriaEliminaciones _auditoriaN;

        public RegistrosEliminadosController(WebCore.Services.IUsuarioSesionService sesion)
        {
            _sesion = sesion;
            _auditoriaN = WebCore.Infrastructure.NegocioFactory.CrearAuditoriaEliminaciones(sesion.Empresa);
        }

        // Modulos cuyos eliminados puede ver quien esta logueado: el admin los tres; el resto, los de los modulos que puede modificar
        // (todos los eliminados de ese modulo, no solo los suyos). Ver EliminacionRegistroWeb.PuedeVerEliminados.
        private List<string> ModulosPermitidos()
        {
            var oUsuarioN = WebCore.Infrastructure.NegocioFactory.CrearUsuario(_sesion.Empresa);
            return WebCore.Helpers.EliminacionRegistroWeb.ModulosConEliminadosVisibles(_sesion.UsuarioActual, oUsuarioN);
        }

        private IActionResult AccesoDenegado()
        {
            ViewBag.Title = "Registros eliminados";
            ViewBag.Seccion = "Registros eliminados";
            return View("~/Views/Shared/AccesoDenegado.cshtml");
        }

        [HttpGet]
        public IActionResult Index(string modulo = "", DateTime? fechaDesde = null, DateTime? fechaHasta = null, string texto = "")
        {
            var permitidos = ModulosPermitidos();
            if (permitidos.Count == 0) return AccesoDenegado();

            DateTime desde = (fechaDesde ?? DateTime.Today.AddDays(-30)).Date;
            DateTime hasta = (fechaHasta ?? DateTime.Today).Date;
            string moduloNormalizado = (modulo ?? "").Trim().ToUpperInvariant();
            if (moduloNormalizado != Entidades.AuditoriaEliminacion.ModuloCompra
                && moduloNormalizado != Entidades.AuditoriaEliminacion.ModuloStock
                && moduloNormalizado != Entidades.AuditoriaEliminacion.ModuloMovimiento)
                moduloNormalizado = "";
            // Un modulo que el usuario no puede ver se ignora (como si no se hubiera pedido).
            if (moduloNormalizado != "" && !permitidos.Contains(moduloNormalizado))
                moduloNormalizado = "";

            // Sin modulo elegido: el admin ve todo en una consulta; quien no es admin, la union de los modulos que puede ver.
            DataTable dt;
            if (moduloNormalizado == "" && permitidos.Count < 3)
            {
                dt = new DataTable();
                foreach (string m in permitidos)
                {
                    DataTable parcial = _auditoriaN.listar(m, desde, hasta, texto ?? "") ?? new DataTable();
                    if (dt.Columns.Count == 0) dt = parcial.Clone();
                    foreach (DataRow fila in parcial.Rows) dt.ImportRow(fila);
                }
                dt.DefaultView.Sort = "fechaEliminacion DESC";
                dt = dt.DefaultView.ToTable();
            }
            else
            {
                dt = _auditoriaN.listar(moduloNormalizado, desde, hasta, texto ?? "") ?? new DataTable();
            }

            var model = new RegistrosEliminadosIndexVm
            {
                Modulo = moduloNormalizado,
                ModulosPermitidos = permitidos,
                FechaDesde = desde.ToString("yyyy-MM-dd"),
                FechaHasta = hasta.ToString("yyyy-MM-dd"),
                Texto = texto ?? "",
                Items = dt.AsEnumerable().Select(MapItem).ToList()
            };

            ViewBag.Title = "Registros eliminados";
            return View("~/Views/RegistrosEliminados/Index.cshtml", model);
        }

        [HttpGet]
        public IActionResult Detalle(int id)
        {
            var permitidos = ModulosPermitidos();
            if (permitidos.Count == 0) return AccesoDenegado();

            var auditoria = _auditoriaN.obtener(id);
            if (auditoria == null) return NotFound();
            if (!permitidos.Contains(auditoria.Modulo)) return AccesoDenegado();

            ViewBag.Title = "Registro eliminado";

            // Recuperar: abre el alta del modulo precargada con este registro (se guarda como uno nuevo). Solo si se puede recuperar y
            // la persona puede crear en ese modulo; si no, se explica por que no.
            var oUsuarioN = WebCore.Infrastructure.NegocioFactory.CrearUsuario(_sesion.Empresa);
            string? noRecuperable = WebCore.Helpers.EliminacionRegistroWeb.MotivoNoRecuperable(auditoria);
            if (noRecuperable != null)
                ViewBag.MotivoNoRecuperable = noRecuperable;
            else if (!WebCore.Helpers.EliminacionRegistroWeb.PuedeCrearComoRecuperacion(_sesion.UsuarioActual, oUsuarioN, auditoria.Modulo))
                ViewBag.MotivoNoRecuperable = "No tiene permiso para cargar este tipo de registro.";
            else
                ViewBag.UrlRecuperar = UrlRecuperar(auditoria);
            return View("~/Views/RegistrosEliminados/Detalle.cshtml", auditoria);
        }

        private string? UrlRecuperar(Entidades.AuditoriaEliminacion auditoria)
        {
            if (auditoria.Modulo == Entidades.AuditoriaEliminacion.ModuloCompra)
                return Url.Action("Editar", "Compras", new { id = 0, desdeEliminado = auditoria.Id });
            if (auditoria.Modulo == Entidades.AuditoriaEliminacion.ModuloStock)
                return Url.Action("Editar", "Stock", new { id = 0, tipoCompra = auditoria.TipoRegistro, desdeEliminado = auditoria.Id });
            return Url.Action("Editar", "Movimientos", new { id = 0, desdeEliminado = auditoria.Id });
        }

        private static RegistrosEliminadosItemVm MapItem(DataRow row)
        {
            return new RegistrosEliminadosItemVm
            {
                Id = Convert.ToInt32(row["id"]),
                Modulo = Convert.ToString(row["modulo"]) ?? "",
                IdRegistro = Convert.ToInt32(row["idRegistro"]),
                TipoRegistro = Texto(row, "tipoRegistro"),
                FechaRegistro = row["fechaRegistro"] != DBNull.Value ? Convert.ToDateTime(row["fechaRegistro"]) : (DateTime?)null,
                Sucursal = Texto(row, "sucursal"),
                Persona = Texto(row, "persona"),
                Importe = row["importe"] != DBNull.Value ? Convert.ToDouble(row["importe"], CultureInfo.InvariantCulture) : (double?)null,
                CantLineas = row["cantLineas"] != DBNull.Value ? Convert.ToInt32(row["cantLineas"]) : 0,
                UsuarioElimino = Texto(row, "usuarioElimino"),
                UsuarioSesion = Texto(row, "usuarioSesion"),
                FechaEliminacion = Convert.ToDateTime(row["fechaEliminacion"]),
                Motivo = Texto(row, "motivo")
            };
        }

        private static string Texto(DataRow row, string columna)
        {
            return row[columna] != DBNull.Value ? Convert.ToString(row[columna]) ?? "" : "";
        }
    }
}
