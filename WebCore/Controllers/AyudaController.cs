// Manual de uso y ayuda por pantalla (2026-10-03, ver docs/DECISIONS.md "Manuales por rol y ayuda en
// cada pantalla"). Endpoints:
//   Panel / Doc / Indice / Buscar  -> fragmentos HTML que el panel lateral carga por AJAX y muestra
//                                     ENCIMA de la pantalla actual (pedir ayuda nunca saca al usuario
//                                     de su pantalla: sin navegar, sin recargar).
//   Index / Ver / Manual           -> paginas completas, solo para "Abrir en pestaña nueva" e imprimir.
// Solo explica: no reemplaza ni relaja el control de permisos de cada pantalla. Cada usuario ve los
// documentos de su nivel o inferiores (usuario < admin de empresa < super admin).
using Microsoft.AspNetCore.Mvc;
using WebCore.Helpers;
using WebCore.Models;

namespace WebCore.Controllers
{
    public class AyudaController : Controller
    {
        private readonly IAyudaService _ayuda;
        private readonly WebCore.Services.IUsuarioSesionService _sesion;
        private readonly ILogger<AyudaController> _logger;

        public AyudaController(IAyudaService ayuda, WebCore.Services.IUsuarioSesionService sesion, ILogger<AyudaController> logger)
        {
            _ayuda = ayuda;
            _sesion = sesion;
            _logger = logger;
        }

        // Nivel del usuario logueado: super admin de plataforma > admin de empresa > usuario. Mismo
        // chequeo de super admin que usa SystemAdministrationController (columna usuarios.superadmin).
        private NivelAyuda NivelUsuario()
        {
            var usuario = _sesion.UsuarioActual;
            if (usuario == null)
                return NivelAyuda.Usuario;

            try
            {
                if (WebCore.Infrastructure.NegocioFactory.CrearSystemAdministrationRepository().EsSuperAdmin(usuario.Id))
                    return NivelAyuda.SuperAdmin;
            }
            catch (Exception ex)
            {
                // Sin poder confirmar que es super admin, no se le muestra lo de ese nivel.
                _logger.LogWarning(ex, "No se pudo verificar si el usuario {IdUsuario} es super admin para la ayuda.", usuario.Id);
            }

            return usuario.Admin ? NivelAyuda.Admin : NivelAyuda.Usuario;
        }

        private static string ClaveRol(NivelAyuda nivel) => nivel switch
        {
            NivelAyuda.SuperAdmin => "superadmin",
            NivelAyuda.Admin => "admin",
            _ => "usuario"
        };

        private static string NombreRol(NivelAyuda nivel) => nivel switch
        {
            NivelAyuda.SuperAdmin => "Manual del super administrador",
            NivelAyuda.Admin => "Manual del administrador",
            _ => "Manual del usuario"
        };

        private static bool TryParseRol(string? rol, out NivelAyuda nivel)
        {
            switch ((rol ?? "").Trim().ToLowerInvariant())
            {
                case "usuario": nivel = NivelAyuda.Usuario; return true;
                case "admin": nivel = NivelAyuda.Admin; return true;
                case "superadmin": nivel = NivelAyuda.SuperAdmin; return true;
                default: nivel = NivelAyuda.Usuario; return false;
            }
        }

        private static string TextoTipo(DocAyuda doc) => doc.Tipo switch
        {
            TipoDocAyuda.Pantalla => "Pantalla",
            TipoDocAyuda.Concepto => "Concepto",
            _ => "Referencia"
        };

        private static string Destino(DocAyuda doc) => doc.Tipo.ToString().ToLowerInvariant() + "/" + doc.Clave;

        private List<(string Rol, string Nombre)> ManualesDisponibles(NivelAyuda nivel)
        {
            var lista = new List<(string, string)>();
            foreach (var n in new[] { NivelAyuda.Usuario, NivelAyuda.Admin, NivelAyuda.SuperAdmin })
            {
                if (n <= nivel && _ayuda.ManualDe(n).Count > 0)
                    lista.Add((ClaveRol(n), NombreRol(n)));
            }
            return lista;
        }

        private AyudaContenidoVm VmDocumento(DocAyuda doc)
        {
            return new AyudaContenidoVm
            {
                Modo = ModoAyuda.Documento,
                Titulo = doc.Titulo,
                Html = _ayuda.RenderizarHtml(doc),
                Revisada = doc.Revisada,
                NombreRol = NombreRol(doc.Nivel),
                UrlPagina = Url.Action(nameof(Ver), "Ayuda", new { tipo = doc.Tipo.ToString().ToLowerInvariant(), clave = doc.Clave }) ?? ""
            };
        }

        // ---------------- Fragmentos para el panel (AJAX) ----------------

        // Ayuda de la pantalla actual (controller/action de la ruta). Sin documento: aviso + acceso a
        // los manuales, y queda en el log para detectar huecos de cobertura.
        [HttpGet]
        public IActionResult Panel(string? c, string? a)
        {
            var nivel = NivelUsuario();
            var doc = _ayuda.BuscarPantalla(c ?? "", a ?? "", nivel);
            if (doc != null)
                return PartialView("_AyudaContenido", VmDocumento(doc));

            _logger.LogInformation("Pantalla sin ayuda: {Pantalla}", (c ?? "") + "." + (a ?? ""));
            return PartialView("_AyudaContenido", new AyudaContenidoVm
            {
                Modo = ModoAyuda.SinAyuda,
                Titulo = "Ayuda",
                Pantalla = (c ?? "") + "." + (a ?? ""),
                ManualesDisponibles = ManualesDisponibles(nivel),
                UrlPagina = Url.Action(nameof(Index), "Ayuda") ?? ""
            });
        }

        // Cualquier documento por tipo (pantalla | concepto | referencia) y clave: lo usan los enlaces
        // "Ver tambien" de dentro de la ayuda.
        [HttpGet]
        public IActionResult Doc(string? tipo, string? clave)
        {
            if (!Enum.TryParse<TipoDocAyuda>(tipo ?? "", true, out var tipoDoc))
                return NotFound();

            var doc = _ayuda.Buscar(tipoDoc, clave ?? "", NivelUsuario());
            if (doc == null)
                return NotFound();

            return PartialView("_AyudaContenido", VmDocumento(doc));
        }

        [HttpGet]
        public IActionResult Indice(string? rol)
        {
            var nivel = NivelUsuario();
            if (!TryParseRol(rol, out var nivelManual) || nivelManual > nivel)
                return PartialView("_AyudaContenido", new AyudaContenidoVm
                {
                    Modo = ModoAyuda.Indice,
                    Titulo = "Manuales",
                    ManualesDisponibles = ManualesDisponibles(nivel),
                    UrlPagina = Url.Action(nameof(Index), "Ayuda") ?? ""
                });

            var vm = new AyudaContenidoVm
            {
                Modo = ModoAyuda.Indice,
                Titulo = NombreRol(nivelManual),
                Rol = ClaveRol(nivelManual),
                ManualesDisponibles = ManualesDisponibles(nivel),
                UrlPagina = Url.Action(nameof(Manual), "Ayuda", new { rol = ClaveRol(nivelManual) }) ?? ""
            };
            foreach (var doc in _ayuda.ManualDe(nivelManual))
            {
                vm.Items.Add(new AyudaItemVm
                {
                    Titulo = doc.Titulo,
                    Modulo = string.IsNullOrEmpty(doc.Modulo) ? "General" : doc.Modulo,
                    Destino = Destino(doc),
                    TipoTexto = TextoTipo(doc)
                });
            }

            return PartialView("_AyudaContenido", vm);
        }

        [HttpGet]
        public IActionResult Buscar(string? q)
        {
            var nivel = NivelUsuario();
            q = (q ?? "").Trim();

            var vm = new AyudaContenidoVm
            {
                Modo = ModoAyuda.Busqueda,
                Titulo = "Resultados",
                Consulta = q,
                UrlPagina = Url.Action(nameof(Index), "Ayuda") ?? ""
            };
            foreach (var doc in _ayuda.Buscar(q, nivel))
            {
                vm.Items.Add(new AyudaItemVm
                {
                    Titulo = doc.Titulo,
                    Modulo = string.IsNullOrEmpty(doc.Modulo) ? "General" : doc.Modulo,
                    Destino = Destino(doc),
                    Fragmento = _ayuda.FragmentoParaBusqueda(doc, q),
                    TipoTexto = TextoTipo(doc)
                });
            }

            return PartialView("_AyudaContenido", vm);
        }

        // ---------------- Paginas completas (pestaña nueva / imprimir) ----------------

        [HttpGet]
        public IActionResult Index()
        {
            ViewBag.Title = "Manual de uso";
            return View(new AyudaContenidoVm
            {
                Modo = ModoAyuda.Indice,
                Titulo = "Manual de uso",
                ManualesDisponibles = ManualesDisponibles(NivelUsuario())
            });
        }

        [HttpGet]
        public IActionResult Ver(string? tipo, string? clave)
        {
            if (!Enum.TryParse<TipoDocAyuda>(tipo ?? "", true, out var tipoDoc))
                return NotFound();

            var doc = _ayuda.Buscar(tipoDoc, clave ?? "", NivelUsuario());
            if (doc == null)
                return NotFound();

            ViewBag.Title = doc.Titulo;
            return View(VmDocumento(doc));
        }

        [HttpGet]
        public IActionResult Manual(string? rol)
        {
            var nivel = NivelUsuario();
            if (!TryParseRol(rol, out var nivelManual) || nivelManual > nivel)
                return NotFound();

            var vm = new AyudaManualVm { Rol = ClaveRol(nivelManual), Nombre = NombreRol(nivelManual) };
            foreach (var doc in _ayuda.ManualDe(nivelManual))
            {
                vm.Secciones.Add(new AyudaManualSeccionVm
                {
                    Anchor = "doc-" + doc.Tipo.ToString().ToLowerInvariant() + "-" + doc.Clave.Replace('.', '-'),
                    Titulo = doc.Titulo,
                    Modulo = string.IsNullOrEmpty(doc.Modulo) ? "General" : doc.Modulo,
                    Html = _ayuda.RenderizarHtml(doc),
                    TipoTexto = TextoTipo(doc)
                });
            }

            ViewBag.Title = vm.Nombre;
            return View(vm);
        }
    }
}
