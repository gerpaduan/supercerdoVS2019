// Port de Web/Controllers/PersonasController.cs (ver docs/DECISIONS.md, migracion ASP.NET Core,
// Modulo 2 -- Clientes y proveedores). Se porta el CRUD completo (Index, Nuevo, Editar, Guardar,
// Buscar, Listar, Obtener, PersonaModal, GuardarPersonaModal) MAS las 2 acciones de AFIP
// (BuscarPadronAfip, BuscarPadronAfipAjax -- ver docs/DECISIONS.md 2026-09-07, mismo patron WCF
// que GenerarFacturaService/LoginClass) MENOS BuscarDatosAfipDesdeGuardar (metodo privado del
// original que ademas no lo llama ninguna accion publica, ver Web/Controllers/PersonasController.cs
// -- codigo muerto, no se porta).
//
// Usuario/empresa reales via IUsuarioSesionService (login real, ver docs/DECISIONS.md
// 2026-09-06) -- ya no hay stub hardcodeado. El "usuario actual" no es solo un gate de acceso:
// alimenta reglas de negocio reales (EsAdministrador, PuedeGestionarCuentaCorriente,
// PuedeModificarPersona) que afectan que se guarda.
using System;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Utilidades;
using WebCore.Models;

namespace WebCore.Controllers
{
    public class PersonasController : Controller
    {
        private readonly IRazorViewEngine _viewEngine;
        private readonly ITempDataProvider _tempDataProvider;
        private readonly IWebHostEnvironment _env;
        private readonly WebCore.Services.IUsuarioSesionService _sesion;
        private readonly IEmpresaContext _empresa;
        private readonly IParametrosContext _param;
        private readonly Negocio.Persona _oPersonaN;
        private readonly Negocio.Usuario _oUsuarioN;
        private Entidades.Usuario _usuarioActual => _sesion.UsuarioActual;

        public PersonasController(IRazorViewEngine viewEngine, ITempDataProvider tempDataProvider, IWebHostEnvironment env, WebCore.Services.IUsuarioSesionService sesion)
        {
            _viewEngine = viewEngine;
            _tempDataProvider = tempDataProvider;
            _env = env;
            _sesion = sesion;
            _empresa = sesion.Empresa;

            // Negocio.Persona/Datos.Persona.findById necesita un IParametrosContext real (no null)
            // para resolver ParamKeys.IdConsumidorFinal -- Web/Controllers/BaseController.cs lo arma
            // igual (NegocioFactory.CrearParametros(empresa) + Reload()) y lo cachea en
            // Session["PARAM_CTX"]; aca se crea uno nuevo por request en vez de cachearlo (mismo
            // criterio que el resto de los controllers de esta migracion).
            _param = WebCore.Infrastructure.NegocioFactory.CrearParametros(_empresa);
            _param.Reload();

            _oPersonaN = WebCore.Infrastructure.NegocioFactory.CrearPersona(_empresa, _param);
            _oUsuarioN = WebCore.Infrastructure.NegocioFactory.CrearUsuario(_empresa, _param);
        }

        [HttpGet]
        public IActionResult Index(string filtro = "")
        {
            var model = new PersonaIndexVm { Filtro = filtro ?? "" };

            DataTable dt = _oPersonaN.buscarPersona(model.Filtro, false) ?? new DataTable();
            model.Items = dt.AsEnumerable()
                .Select(MapResumen)
                .OrderBy(x => x.IdEmpresa)
                .ThenBy(x => x.RazonSocial ?? "")
                .ThenBy(x => x.Identificacion ?? "")
                .ToList();

            ViewBag.Title = "Personas";
            return View(model);
        }

        [HttpGet]
        public IActionResult Nuevo()
        {
            var model = CrearViewModel(new Entidades.Persona(), false);
            CargarIvas(model);

            ViewBag.Title = "Nueva persona";
            return View("Editar", model);
        }

        [HttpGet]
        public IActionResult Editar(int id)
        {
            var persona = _oPersonaN.findById(id);
            if (persona == null || persona.IdPersona <= 0)
            {
                TempData["AlertType"] = "warning";
                TempData["AlertTitle"] = "Personas";
                TempData["AlertMsg"] = "No se encontró la persona seleccionada.";
                return RedirectToAction("Index");
            }

            if (!PuedeModificarPersona(persona))
            {
                TempData["AlertType"] = "warning";
                TempData["AlertTitle"] = "Personas";
                TempData["AlertMsg"] = "No tiene permisos para modificar personas globales.";
                return RedirectToAction("Index");
            }

            var model = CrearViewModel(persona, true);
            CargarIvas(model);

            ViewBag.Title = "Editar persona";
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Guardar(PersonaEditVm model)
        {
            var usuario = _usuarioActual;
            model = model ?? new PersonaEditVm();

            bool esEdicion = model.IdPersona > 0;
            Entidades.Persona personaOriginal = esEdicion ? _oPersonaN.findById(model.IdPersona) : new Entidades.Persona();

            if (esEdicion && (personaOriginal == null || personaOriginal.IdPersona <= 0))
            {
                TempData["AlertType"] = "warning";
                TempData["AlertTitle"] = "Personas";
                TempData["AlertMsg"] = "No se encontró la persona a modificar.";
                return RedirectToAction("Index");
            }

            if (esEdicion && !PuedeModificarPersona(personaOriginal))
            {
                TempData["AlertType"] = "warning";
                TempData["AlertTitle"] = "Personas";
                TempData["AlertMsg"] = "No tiene permisos para modificar personas globales.";
                return RedirectToAction("Index");
            }

            bool tieneMovimientos = esEdicion && _oPersonaN.personaTieneCompras_Ventas(model.IdPersona);
            bool esAdministrador = usuario != null && usuario.Admin;
            bool puedeGestionarCuentaCorriente = PuedeGestionarCuentaCorriente(usuario);

            model.EsEdicion = esEdicion;
            model.SoloLecturaInicial = false;
            model.TieneMovimientos = tieneMovimientos;
            model.EsAdministrador = esAdministrador;
            model.PuedeGestionarCuentaCorriente = puedeGestionarCuentaCorriente;
            model.PuedeEditarCamposProtegidos = !esEdicion || !tieneMovimientos || esAdministrador;
            model.MensajeRestriccion = ConstruirMensajeRestriccion(tieneMovimientos, esAdministrador);

            float bonificacion = 0f;
            ValidarPersona(model, personaOriginal, tieneMovimientos, esAdministrador, out bonificacion);

            if (!ModelState.IsValid)
            {
                CargarIvas(model);
                ViewBag.Title = model.EsEdicion ? "Editar persona" : "Nueva persona";
                return View("Editar", model);
            }

            var personaGuardar = esEdicion ? personaOriginal : new Entidades.Persona();

            personaGuardar.Identificacion = NormalizarTexto(model.Identificacion, true);
            personaGuardar.razonSocial = NormalizarTexto(model.RazonSocial, true);
            personaGuardar.IdIva = model.IdIva ?? 0;
            personaGuardar.Cuit = NormalizarCuit(model.Cuit);
            personaGuardar.Telefono = (model.Telefono ?? "").Trim();
            personaGuardar.Email = (model.Email ?? "").Trim();
            personaGuardar.Domicilio = NormalizarTexto(model.Domicilio, true);
            personaGuardar.Ciudad = NormalizarTexto(model.Ciudad, true);
            personaGuardar.otrosDatos = (model.OtrosDatos ?? "").Trim();
            personaGuardar.CtaCte = puedeGestionarCuentaCorriente
                ? model.CtaCte
                : (esEdicion && personaOriginal != null && personaOriginal.IdPersona > 0 && personaOriginal.CtaCte);
            personaGuardar.Bonificacion = bonificacion;
            personaGuardar.tipo = personaGuardar.tipo ?? "";
            personaGuardar.Marca = false;

            if (personaGuardar.Propietario == null && personaGuardar.IdPropietario.HasValue && personaGuardar.IdPropietario.Value > 0)
            {
                personaGuardar.Propietario = new Entidades.Persona { idPersona = personaGuardar.IdPropietario.Value };
            }

            _oPersonaN.addOrEditPersona(personaGuardar);

            TempData["AlertType"] = "success";
            TempData["AlertTitle"] = "Personas";
            TempData["AlertMsg"] = model.EsEdicion ? "La persona se guardó correctamente." : "La persona se creó correctamente.";

            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult Buscar()
        {
            return PartialView("_BuscarPersona");
        }

        [HttpGet]
        public JsonResult Listar(string filtro)
        {
            filtro = filtro ?? "";
            DataTable dt = _oPersonaN.buscarPersona(filtro, false) ?? new DataTable();

            var personas = dt.AsEnumerable()
                .Select(row => new
                {
                    idPersona = LeerInt(row, "idPersona"),
                    idEmpresa = LeerInt(row, "idEmpresa"),
                    iva = LeerString(row, "iva"),
                    razonSocial = LeerString(row, "razonSocial"),
                    cuit = LeerString(row, "cuit"),
                    identificacion = LeerString(row, "nombreIdentif"),
                    telefono = LeerString(row, "telefono"),
                    domicilio = LeerString(row, "domicilio"),
                    ciudad = LeerString(row, "ciudad"),
                    otrosDatos = LeerString(row, "otrosDatos"),
                    puedeModificar = PuedeModificarPersona(LeerInt(row, "idEmpresa"))
                })
                .OrderBy(x => x.idEmpresa)
                .ThenBy(x => x.razonSocial ?? "")
                .ThenBy(x => x.identificacion ?? "")
                .ToList();

            return Json(personas);
        }

        [HttpGet]
        public JsonResult Obtener(int id)
        {
            if (id <= 0)
                return Json(new { ok = false, msg = "Persona inválida." });

            var persona = _oPersonaN.findById(id);
            if (persona == null || persona.IdPersona <= 0)
                return Json(new { ok = false, msg = "No se encontró la persona." });

            return Json(new
            {
                ok = true,
                idPersona = persona.IdPersona,
                razonSocial = persona.RazonSocial ?? "",
                identificacion = persona.Identificacion ?? "",
                cuit = persona.Cuit ?? "",
                ctaCte = persona.CtaCte
            });
        }

        [HttpGet]
        public async Task<IActionResult> PersonaModal()
        {
            try
            {
                var model = CrearViewModel(new Entidades.Persona(), false);
                CargarIvas(model);

                string html = await RenderPartialViewToStringAsync("_AddOrEditPersonaModal", model);
                return Content(html, "text/html");
            }
            catch (Exception ex)
            {
                string detalle = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                return Content("<div class='alert alert-danger mb-0'>No se pudo abrir el formulario: " + System.Net.WebUtility.HtmlEncode(detalle) + "</div>", "text/html");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult GuardarPersonaModal(PersonaEditVm model)
        {
            model = model ?? new PersonaEditVm();
            model.IdPersona = 0;
            model.EsEdicion = false;

            var usuario = _usuarioActual;
            bool esAdministrador = usuario != null && usuario.Admin;
            bool puedeGestionarCuentaCorriente = PuedeGestionarCuentaCorriente(usuario);

            float bonificacion;
            ValidarPersona(model, new Entidades.Persona(), tieneMovimientos: false, esAdministrador: esAdministrador, out bonificacion);

            if (!ModelState.IsValid)
            {
                string mensaje = string.Join(" ", ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .Where(m => !string.IsNullOrWhiteSpace(m))
                    .Distinct());

                return Json(new { success = false, message = string.IsNullOrWhiteSpace(mensaje) ? "Revise los datos ingresados." : mensaje });
            }

            var personaGuardar = new Entidades.Persona
            {
                Identificacion = NormalizarTexto(model.Identificacion, true),
                razonSocial = NormalizarTexto(model.RazonSocial, true),
                IdIva = model.IdIva ?? 0,
                Cuit = NormalizarCuit(model.Cuit),
                Telefono = (model.Telefono ?? "").Trim(),
                Email = (model.Email ?? "").Trim(),
                Domicilio = NormalizarTexto(model.Domicilio, true),
                Ciudad = NormalizarTexto(model.Ciudad, true),
                otrosDatos = (model.OtrosDatos ?? "").Trim(),
                CtaCte = puedeGestionarCuentaCorriente && model.CtaCte,
                Bonificacion = bonificacion,
                tipo = "",
                Marca = false
            };

            try
            {
                int idPersona = _oPersonaN.addOrEditPersonaConId(personaGuardar);
                return Json(new
                {
                    success = true,
                    message = "La persona se creó correctamente.",
                    idPersona,
                    razonSocial = personaGuardar.razonSocial,
                    identificacion = personaGuardar.Identificacion,
                    cuit = personaGuardar.Cuit
                });
            }
            catch (Exception ex)
            {
                string detalle = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                return Json(new { success = false, message = "No se pudo guardar la persona: " + detalle });
            }
        }

        // Port de Web/Controllers/PersonasController.cs:370 (BuscarPadronAfip). Usada por el botón
        // "Buscar en AFIP" del alta rápida de empresa (ver AltaRapidaEmpresa.cshtml, gap ya cerrado
        // en docs/DECISIONS.md 2026-09-07).
        [HttpGet]
        public JsonResult BuscarPadronAfip(string cuit)
        {
            try
            {
                string cuitNormalizado = NormalizarCuit(cuit);
                long cuitPersona;
                if (string.IsNullOrWhiteSpace(cuitNormalizado) || cuitNormalizado.Length != 11 || !long.TryParse(cuitNormalizado, out cuitPersona))
                    return Json(new { ok = false, msg = "Ingrese un CUIT válido de 11 dígitos." });

                var empresaAfip = ObtenerEmpresaAfipActual();
                if (empresaAfip == null || empresaAfip.Cuit <= 0)
                    return Json(new { ok = false, msg = "No se encontró la configuración AFIP de la empresa actual." });

                var servicioPadron = new AFIP.ConsultarPadronService(empresaAfip, _env.ContentRootPath);
                var resultado = servicioPadron.ConsultarDatosContribuyente(cuitNormalizado);

                if (!resultado.Ok || resultado.Persona == null)
                    return Json(new { ok = false, msg = resultado.Mensaje ?? "No se encontraron datos para el CUIT especificado." });

                return Json(new
                {
                    ok = true,
                    razonSocial = resultado.Persona.RazonSocial,
                    identificacion = resultado.Persona.Identificacion,
                    domicilio = resultado.Persona.Domicilio,
                    ciudad = resultado.Persona.Ciudad
                });
            }
            catch (Exception ex)
            {
                return Json(new { ok = false, msg = "No se pudo obtener los datos desde AFIP. " + ex.Message });
            }
        }

        // Port de Web/Controllers/PersonasController.cs:413 (BuscarPadronAfipAjax). Consumida por el
        // botón "Buscar en AFIP" de Personas/Editar y del modal de alta rápida de persona (JS ya
        // portado literal en las vistas, ver docs/DECISIONS.md 2026-09-07).
        [HttpGet]
        public JsonResult BuscarPadronAfipAjax(string cuit)
        {
            try
            {
                string cuitNormalizado = NormalizarCuit(cuit);
                long cuitPersona;
                if (string.IsNullOrWhiteSpace(cuitNormalizado) || cuitNormalizado.Length != 11 || !long.TryParse(cuitNormalizado, out cuitPersona))
                    return Json(new { ok = false, msg = "Ingrese un CUIT válido de 11 dígitos.", tipo = "warning" });

                var empresaAfip = ObtenerEmpresaAfipActual();
                if (empresaAfip == null || empresaAfip.Cuit <= 0)
                    return Json(new { ok = false, msg = "No se encontró la configuración AFIP de la empresa actual.", tipo = "error" });

                var servicioPadron = new AFIP.ConsultarPadronService(empresaAfip, _env.ContentRootPath);
                var resultado = servicioPadron.ConsultarDatosContribuyente(cuitNormalizado);

                if (!resultado.Ok || resultado.Persona == null)
                {
                    return Json(new
                    {
                        ok = false,
                        msg = resultado.Mensaje ?? "No se encontraron datos para el CUIT ingresado.",
                        tipo = EsMensajeSinDatosAfip(resultado.Mensaje) ? "info" : "error"
                    });
                }

                return Json(new
                {
                    ok = true,
                    razonSocial = resultado.Persona.RazonSocial,
                    identificacion = resultado.Persona.Identificacion,
                    domicilio = resultado.Persona.Domicilio,
                    ciudad = resultado.Persona.Ciudad,
                    idIva = resultado.IdIvaSugerido,
                    condicionIva = resultado.CondicionIvaAfip ?? "",
                    estadoClave = resultado.EstadoClaveAfip ?? "",
                    actividadPrincipal = resultado.ActividadPrincipalAfip ?? "",
                    msg = "Datos recuperados desde AFIP/ARCA."
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    ok = false,
                    msg = AFIP.ConsultarPadronService.TraducirMensajeError(ex),
                    tipo = "error"
                });
            }
        }

        // Port de Web/Controllers/PersonasController.cs:464 (ObtenerEmpresaAfipActual). El usuario
        // logueado puede traer la Empresa ya cargada (IUsuarioSesionService); si falta o esta
        // incompleta la config AFIP, se resuelve de nuevo via Sucursal (misma logica que el original).
        private Entidades.Empresa ObtenerEmpresaAfipActual()
        {
            var usuario = _usuarioActual;
            int idEmpresa = usuario != null ? usuario.IdEmpresa : 0;
            var empresaAfip = usuario != null ? usuario.Empresa : null;

            if ((empresaAfip == null || empresaAfip.Cuit <= 0 || string.IsNullOrWhiteSpace(empresaAfip.NombreCertificado_pfx)) && idEmpresa > 0)
            {
                var sucursalN = WebCore.Infrastructure.NegocioFactory.CrearSucursal(_empresa, _param);
                empresaAfip = sucursalN.findEmpresaById(idEmpresa);
            }

            return empresaAfip;
        }

        private bool EsMensajeSinDatosAfip(string mensaje)
        {
            return !string.IsNullOrWhiteSpace(mensaje)
                && mensaje.IndexOf("No se encontraron datos", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private async Task<string> RenderPartialViewToStringAsync(string viewName, object model)
        {
            ViewData.Model = model;

            using (var sw = new StringWriter())
            {
                ViewEngineResult viewResult = _viewEngine.FindView(ControllerContext, viewName, isMainPage: false);
                if (viewResult.View == null)
                    throw new InvalidOperationException("No se encontró la vista parcial '" + viewName + "'.");

                var viewContext = new ViewContext(
                    ControllerContext,
                    viewResult.View,
                    ViewData,
                    new TempDataDictionary(HttpContext, _tempDataProvider),
                    sw,
                    new HtmlHelperOptions());

                await viewResult.View.RenderAsync(viewContext);
                return sw.ToString();
            }
        }

        private PersonaResumenVm MapResumen(DataRow row)
        {
            return new PersonaResumenVm
            {
                IdPersona = LeerInt(row, "idPersona"),
                IdEmpresa = LeerInt(row, "idEmpresa"),
                Identificacion = LeerString(row, "nombreIdentif"),
                RazonSocial = LeerString(row, "razonSocial"),
                Iva = LeerString(row, "iva"),
                Cuit = LeerString(row, "cuit"),
                Telefono = LeerString(row, "telefono"),
                Domicilio = LeerString(row, "domicilio"),
                Ciudad = LeerString(row, "ciudad"),
                OtrosDatos = LeerString(row, "otrosDatos"),
                CtaCte = LeerBool(row, "ctaCte"),
                Bonificacion = LeerFloat(row, "bonificacion"),
                PuedeModificar = PuedeModificarPersona(LeerInt(row, "idEmpresa"))
            };
        }

        private bool PuedeModificarPersona(Entidades.Persona persona)
        {
            return persona != null && PuedeModificarPersona(persona.IdEmpresa);
        }

        private bool PuedeModificarPersona(int idEmpresaPersona)
        {
            var usuario = _usuarioActual;
            int idEmpresaUsuario = usuario != null ? usuario.IdEmpresa : 0;
            return !(idEmpresaUsuario > 0 && idEmpresaPersona == 0);
        }

        private PersonaEditVm CrearViewModel(Entidades.Persona persona, bool esEdicion)
        {
            var usuario = _usuarioActual;
            bool tieneMovimientos = esEdicion && persona != null && persona.IdPersona > 0 && _oPersonaN.personaTieneCompras_Ventas(persona.IdPersona);
            bool esAdministrador = usuario != null && usuario.Admin;
            bool puedeGestionarCuentaCorriente = PuedeGestionarCuentaCorriente(usuario);

            return new PersonaEditVm
            {
                IdPersona = persona != null ? persona.IdPersona : 0,
                EsEdicion = esEdicion,
                SoloLecturaInicial = esEdicion,
                TieneMovimientos = tieneMovimientos,
                EsAdministrador = esAdministrador,
                PuedeGestionarCuentaCorriente = puedeGestionarCuentaCorriente,
                PuedeEditarCamposProtegidos = !esEdicion || !tieneMovimientos || esAdministrador,
                MensajeRestriccion = ConstruirMensajeRestriccion(tieneMovimientos, esAdministrador),
                Identificacion = persona != null ? persona.Identificacion : "",
                RazonSocial = persona != null ? persona.RazonSocial : "",
                IdIva = persona != null ? (int?)persona.IdIva : null,
                Cuit = persona != null ? persona.Cuit : "",
                Telefono = persona != null ? persona.Telefono : "",
                Email = persona != null ? persona.Email : "",
                Domicilio = persona != null ? persona.Domicilio : "",
                Ciudad = persona != null ? persona.Ciudad : "",
                OtrosDatos = persona != null ? persona.OtrosDatos : "",
                CtaCte = persona != null && persona.CtaCte,
                BonificacionTexto = persona != null
                    ? persona.Bonificacion.ToString("0.##", CultureInfo.InvariantCulture)
                    : "0"
            };
        }

        private bool PuedeGestionarCuentaCorriente(Entidades.Usuario usuario)
        {
            // Port de Web/Controllers/PersonasController.cs:550-554 (2026-09-09, Batch B, ver
            // docs/DECISIONS.md "Batch B: permisos reales + operador de produccion"). Antes solo
            // chequeaba Admin -- la rama de permiso puntual (Permisos.Finanza.VerCtasCtes) quedo
            // sin portar mientras WebCore no tenia sesion real; ya la tiene.
            return usuario != null && (usuario.Admin || _oUsuarioN.tienePermiso(usuario, Entidades.Permisos.Finanza.VerCtasCtes, DateTime.Today, -1));
        }

        private void CargarIvas(PersonaEditVm model)
        {
            model.Ivas.Clear();

            DataTable dtIva = _oPersonaN.getIva() ?? new DataTable();
            foreach (DataRow row in dtIva.Rows)
            {
                int id = LeerInt(row, "id");
                string texto = LeerString(row, "iva");

                model.Ivas.Add(new SelectListItem
                {
                    Value = id.ToString(CultureInfo.InvariantCulture),
                    Text = texto,
                    Selected = id == (model.IdIva ?? 0)
                });
            }
        }

        private void ValidarPersona(PersonaEditVm model, Entidades.Persona personaOriginal, bool tieneMovimientos, bool esAdministrador, out float bonificacion)
        {
            bonificacion = 0f;

            if (string.IsNullOrWhiteSpace(model.Identificacion))
                ModelState.AddModelError("Identificacion", "La identificación es obligatoria.");

            if (string.IsNullOrWhiteSpace(model.RazonSocial))
                ModelState.AddModelError("RazonSocial", "La razón social es obligatoria.");

            if (model.IdIva <= 0)
                ModelState.AddModelError("IdIva", "Seleccione una condición frente al IVA.");

            int idPersonaCuitEncontrado = _oPersonaN.existeCuit(model.Cuit);
            bool existeCuit = idPersonaCuitEncontrado > 0 && model.IdPersona != idPersonaCuitEncontrado;
            if (!string.IsNullOrWhiteSpace(NormalizarCuit(model.Cuit)) && existeCuit)
                ModelState.AddModelError("Cuit", "El CUIT ingresado ya existe para otra persona.");

            if (!TryParseBonificacion(model.BonificacionTexto, out bonificacion))
                ModelState.AddModelError("BonificacionTexto", "La bonificación debe ser numérica.");

            if (tieneMovimientos && !esAdministrador && CamposProtegidosCambiaron(model, personaOriginal))
            {
                ModelState.AddModelError("", "Esta persona ya tiene compras o ventas registradas. Por seguridad, solo un administrador puede modificar Razón Social, CUIT o Identificación.");
            }
        }

        private bool CamposProtegidosCambiaron(PersonaEditVm model, Entidades.Persona personaOriginal)
        {
            if (model == null || personaOriginal == null)
                return false;

            return !string.Equals(NormalizarTexto(model.Identificacion, true), NormalizarTexto(personaOriginal.Identificacion, true), StringComparison.OrdinalIgnoreCase)
                || !string.Equals(NormalizarTexto(model.RazonSocial, true), NormalizarTexto(personaOriginal.RazonSocial, true), StringComparison.OrdinalIgnoreCase)
                || !string.Equals(NormalizarCuit(model.Cuit), NormalizarCuit(personaOriginal.Cuit), StringComparison.OrdinalIgnoreCase);
        }

        private bool TryParseBonificacion(string texto, out float valor)
        {
            valor = 0f;
            string limpio = (texto ?? "").Trim();
            if (string.IsNullOrWhiteSpace(limpio))
                return true;

            return float.TryParse(limpio, NumberStyles.Any, CultureInfo.GetCultureInfo("es-AR"), out valor)
                || float.TryParse(limpio, NumberStyles.Any, CultureInfo.InvariantCulture, out valor)
                || float.TryParse(limpio, NumberStyles.Any, CultureInfo.CurrentCulture, out valor);
        }

        private string ConstruirMensajeRestriccion(bool tieneMovimientos, bool esAdministrador)
        {
            if (!tieneMovimientos)
                return "";

            if (esAdministrador)
                return "Esta persona ya tiene compras o ventas registradas. Revise con cuidado los cambios en Razón Social, CUIT e Identificación porque impactan en datos históricos.";

            return "Esta persona ya tiene compras o ventas registradas. Por seguridad, solo un administrador puede modificar Razón Social, CUIT o Identificación.";
        }

        private string NormalizarTexto(string valor, bool upper)
        {
            string texto = (valor ?? "").Trim();
            return upper ? texto.ToUpperInvariant() : texto;
        }

        private string NormalizarCuit(string valor)
        {
            return string.IsNullOrWhiteSpace(valor)
                ? ""
                : valor.Trim().Replace("-", "").Replace(" ", "");
        }

        private int LeerInt(DataRow row, string columna)
        {
            if (row == null || row.Table == null || !row.Table.Columns.Contains(columna) || row[columna] == DBNull.Value)
                return 0;

            int valor;
            return int.TryParse(Convert.ToString(row[columna]), out valor) ? valor : 0;
        }

        private string LeerString(DataRow row, string columna)
        {
            if (row == null || row.Table == null || !row.Table.Columns.Contains(columna) || row[columna] == DBNull.Value)
                return "";

            return Convert.ToString(row[columna]);
        }

        private bool LeerBool(DataRow row, string columna)
        {
            return row != null
                && row.Table != null
                && row.Table.Columns.Contains(columna)
                && row[columna] != DBNull.Value
                && Convert.ToBoolean(row[columna]);
        }

        private float LeerFloat(DataRow row, string columna)
        {
            if (row == null || row.Table == null || !row.Table.Columns.Contains(columna) || row[columna] == DBNull.Value)
                return 0f;

            float valor;
            return float.TryParse(Convert.ToString(row[columna]), NumberStyles.Any, CultureInfo.InvariantCulture, out valor)
                || float.TryParse(Convert.ToString(row[columna]), NumberStyles.Any, CultureInfo.GetCultureInfo("es-AR"), out valor)
                || float.TryParse(Convert.ToString(row[columna]), NumberStyles.Any, CultureInfo.CurrentCulture, out valor)
                ? valor
                : 0f;
        }
    }
}
