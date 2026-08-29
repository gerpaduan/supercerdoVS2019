using System.Web.Mvc;

namespace Web.Controllers
{
    // Pantalla compartida de "quien esta haciendo esto" (sin contraseña) para Movimientos/
    // Stock/Elaborados cuando la sesion es la cuenta de produccion -- se pide ANTES de entrar a
    // la vista de edicion, no al guardar como era antes (ver docs/DECISIONS.md, "Mover la
    // seleccion de usuario..."). Reusa el mismo _ModalSeleccionUsuario.cshtml/seleccion-usuario.js
    // ya usado hoy al guardar (BaseController.ObtenerUsuariosActivosEmpresaParaCombo). No exige
    // contraseña ni valida nada del lado del servidor -- el id elegido se revalida igual que
    // siempre en BaseController.ResolverUsuarioCreador al momento de guardar.
    public class SeleccionUsuarioController : BaseController
    {
        public ActionResult Index(string returnUrl, string cancelUrl)
        {
            var user = Session["Usuario"] as Entidades.Usuario;
            if (user == null)
                return RedirectToAction("Index", "Login");

            ViewBag.UsuariosActivosEmpresa = ObtenerUsuariosActivosEmpresaParaCombo();
            ViewBag.ReturnUrl = Url.IsLocalUrl(returnUrl) ? returnUrl : Url.Action("Index", "Home");
            ViewBag.CancelUrl = Url.IsLocalUrl(cancelUrl) ? cancelUrl : Url.Action("Index", "Home");
            ViewBag.Title = "Seleccionar usuario";
            ViewBag.Seccion = "";
            return View("~/Views/SeleccionUsuario/Index.cshtml");
        }
    }
}
