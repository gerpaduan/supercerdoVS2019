using System;
using System.Data;
using System.Web.Mvc;
using Negocio;
using Entidades;
using Usuario = Negocio.Usuario;
using Web.Helpers;

namespace Web.Controllers
{
    public class FinanzasController : Controller
    {
        private CuentaCorriente oCtaCteN = new CuentaCorriente();
        private Usuario oUsuarioN = new Usuario();

        // ***********************************************************
        //  Estos valores se pasan como QueryString o TempData
        // ***********************************************************
        public bool DesdePOS
        {
            get { return (bool)(TempData["DesdePOS"] ?? false); }
            set { TempData["DesdePOS"] = value; }
        }

        public Entidades.CierreCaja OCierreCajaE
        {
            get { return TempData["OCierreCajaE"] as Entidades.CierreCaja; }
            set { TempData["OCierreCajaE"] = value; }
        }


        // ============================================================
        // GET: /Finanzas/CtasCtes
        // ============================================================
        public ActionResult CtasCtes(string buscar = "", string ordenSaldo = "DESC")
        {
            try
            {
                // ==============================
                // Validación de permisos IGUAL que WinForms
                // ==============================
                if (!DesdePOS)
                {
                    var user = Session["Usuario"] as Entidades.Usuario;
                    if (!PermisosHelper.TienePermiso(Session, Permisos.Finanza.VerCtasCtes, null))
                    {
                        ViewBag.Seccion = "Agregar/Modificar Productos";
                        return View("~/Views/Shared/AccesoDenegado.cshtml");
                    }
                }

                ViewBag.Buscar = buscar;
                ViewBag.DesdePOS = DesdePOS;
                ViewBag.OrdenSaldo = ordenSaldo;

                // ==============================
                // Obtener DataTable como WinForms
                // ==============================
                DataTable dt = oCtaCteN.obtenerCtasCtes(buscar, null);


                // Ordenar por saldo
                DataView dv = dt.DefaultView;
                dv.Sort = $"Saldo {ordenSaldo}";
                dt = dv.ToTable();

                return View("CtasCtes", dt); // usa la vista que generamos antes
            }
            catch (Exception ex)
            {
                return Content("Error: " + ex.Message);
            }
        }


        // ============================================================
        // POST: /Finanzas/SeleccionarCtaCte
        // (equivalente a btnSeleccionar_Click)
        // ============================================================
        [HttpPost]
        public ActionResult SeleccionarCtaCte(int id)
        {
            try
            {
                // Guardamos valores como el WinForm
                TempData["IdPersona"] = id;
                TempData["DesdePOS"] = DesdePOS;
                TempData["OCierreCajaE"] = OCierreCajaE;

                return Json(new { ok = true });
            }
            catch (Exception ex)
            {
                return Json(new { ok = false, error = ex.Message });
            }
        }
    }
}
