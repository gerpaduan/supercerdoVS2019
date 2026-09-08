// Port de Web/Controllers/LandingController.cs -- landing page de marketing pre-login. Sin logica
// de negocio: [AllowAnonymous] porque el FallbackPolicy global de Program.cs exige sesion en todo
// controller que no lo marque explicitamente (ver comentario en Program.cs).
//
// Bug real encontrado y corregido 2026-09-07 (ver docs/DECISIONS.md): la ruta "PublicHome"
// (pattern "" en Program.cs) ocupa la URL raiz "/" -- pero "/" TAMBIEN es la URL que genera
// RedirectToAction("Index","Home") (la ruta "default" tiene esos mismos valores como default,
// asi que el generador de links de ASP.NET Core la colapsa a "/"). Resultado: el login exitoso
// redirigia (via RedirigirPostLogin en LoginController) a una URL que terminaba sirviendo el
// Landing en vez del Home real. Fix: si ya hay sesion valida, esta accion reenvia a Home/Index en
// vez de mostrar el landing -- asi cualquier navegacion a "/" (incluido el colapso de ruta del
// login) termina en el dashboard real para un usuario ya logueado.
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebCore.Controllers
{
    [AllowAnonymous]
    public class LandingController : Controller
    {
        private readonly WebCore.Services.IUsuarioSesionService _sesion;

        public LandingController(WebCore.Services.IUsuarioSesionService sesion)
        {
            _sesion = sesion;
        }

        [HttpGet]
        public IActionResult Index()
        {
            // Redirect LITERAL, no RedirectToAction("Index","Home"): esos valores son los
            // defaults de la propia ruta "default" (Program.cs), asi que el generador de links
            // los colapsa de vuelta a "/" -- causaria un loop infinito de 302 contra este mismo
            // action. Ver el comentario grande de arriba.
            if (_sesion.EstaAutenticado)
                return Redirect("/Home/Index");

            return View("~/Views/Landing/Index.cshtml");
        }
    }
}
