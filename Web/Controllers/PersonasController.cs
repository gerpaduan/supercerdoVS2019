using Entidades;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Net.Mail;
using System.Web.Mvc;
using Web.Helpers;
using Web.Models;
using AFIP;

namespace Web.Controllers
{
    public class PersonasController : BaseController
    {
        private Negocio.Persona oPersonaN;

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);
            if (filterContext.Result != null) return;

            oPersonaN = new Negocio.Persona(empresa, param);
        }

        [HttpGet]
        public ActionResult Index(string filtro = "")
        {
            var model = new PersonaIndexVm
            {
                Filtro = filtro ?? ""
            };

            DataTable dt = oPersonaN.buscarPersona(model.Filtro, false) ?? new DataTable();
            model.Items = dt.AsEnumerable()
                .Select(MapResumen)
                .OrderBy(x => x.IdEmpresa)
                .ThenBy(x => x.RazonSocial ?? "")
                .ThenBy(x => x.Identificacion ?? "")
                .ToList();

            ViewBag.Title = "Personas";
            ViewBag.Seccion = "Personas";
            return View("~/Views/Personas/Index.cshtml", model);
        }

        [HttpGet]
        public ActionResult Nuevo()
        {
            var model = CrearViewModel(new Entidades.Persona(), false);
            CargarIvas(model);

            ViewBag.Title = "Nueva persona";
            ViewBag.Seccion = "Personas";
            return View("~/Views/Personas/Editar.cshtml", model);
        }

        [HttpGet]
        public ActionResult Editar(int id)
        {
            var persona = oPersonaN.findById(id);
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
            ViewBag.Seccion = "Personas";
            return View("~/Views/Personas/Editar.cshtml", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Guardar(PersonaEditVm model)
        {
            var usuario = Session["Usuario"] as Entidades.Usuario;
            model = model ?? new PersonaEditVm();

            bool esEdicion = model.IdPersona > 0;
            Entidades.Persona personaOriginal = esEdicion ? oPersonaN.findById(model.IdPersona) : new Entidades.Persona();

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

            bool tieneMovimientos = esEdicion && oPersonaN.personaTieneCompras_Ventas(model.IdPersona);
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
                ViewBag.Seccion = "Personas";
                return View("~/Views/Personas/Editar.cshtml", model);
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

            oPersonaN.addOrEditPersona(personaGuardar);

            TempData["PersonasSuccessMessage"] = model.EsEdicion
                ? "La persona se guardó correctamente."
                : "La persona se creó correctamente.";

            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult Buscar()
        {
            return PartialView("~/Views/Personas/_BuscarPersona.cshtml");
        }

        [HttpGet]
        public JsonResult Listar(string filtro)
        {
            filtro = filtro ?? "";
            DataTable dt = oPersonaN.buscarPersona(filtro, false) ?? new DataTable();

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

            return Json(personas, JsonRequestBehavior.AllowGet);
        }

        private ActionResult BuscarDatosAfipDesdeGuardar(PersonaEditVm model)
        {
            model = model ?? new PersonaEditVm();
            model.Cuit = NormalizarCuit(model.Cuit);

            if (string.IsNullOrWhiteSpace(model.Cuit) || model.Cuit.Length != 11)
            {
                ModelState.AddModelError("Cuit", "Ingrese un CUIT válido de 11 dígitos.");
                CargarIvas(model);
                ViewBag.Title = model.EsEdicion ? "Editar persona" : "Nueva persona";
                ViewBag.Seccion = "Personas";
                return View("~/Views/Personas/Editar.cshtml", model);
            }

            try
            {
                var empresaAfip = ObtenerEmpresaAfipActual();

                if (empresaAfip == null || empresaAfip.Cuit <= 0)
                {
                    ModelState.AddModelError("", "No se encontró la configuración AFIP de la empresa actual.");
                }
                else
                {
                    var servicioPadron = new ConsultarPadronService(empresaAfip);
                    var resultado = servicioPadron.ConsultarDatosContribuyente(model.Cuit);

                    if (!resultado.Ok || resultado.Persona == null)
                    {
                        ModelState.AddModelError("", resultado.Mensaje ?? "No se encontraron datos para el CUIT especificado.");
                    }
                    else
                    {
                        model.Identificacion = resultado.Persona.Identificacion ?? model.Identificacion;
                        model.RazonSocial = resultado.Persona.RazonSocial ?? model.RazonSocial;
                        model.Domicilio = resultado.Persona.Domicilio ?? model.Domicilio;
                        model.Ciudad = resultado.Persona.Ciudad ?? model.Ciudad;
                        if (resultado.IdIvaSugerido > 0)
                            model.IdIva = resultado.IdIvaSugerido;

                        TempData["PersonasInfoMessage"] = "Datos recuperados desde AFIP. Revise IVA antes de guardar.";
                    }
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ConsultarPadronService.TraducirMensajeError(ex));
            }

            CargarIvas(model);
            ViewBag.Title = model.EsEdicion ? "Editar persona" : "Nueva persona";
            ViewBag.Seccion = "Personas";
            return View("~/Views/Personas/Editar.cshtml", model);
        }

        [HttpGet]
        public JsonResult BuscarPadronAfip(string cuit)
        {
            try
            {
                string cuitNormalizado = NormalizarCuit(cuit);
                long cuitPersona;
                if (string.IsNullOrWhiteSpace(cuitNormalizado) || cuitNormalizado.Length != 11 || !long.TryParse(cuitNormalizado, out cuitPersona))
                    return Json(new { ok = false, msg = "Ingrese un CUIT válido de 11 dígitos." }, JsonRequestBehavior.AllowGet);

                var usuario = Session["Usuario"] as Entidades.Usuario;
                int idEmpresa = usuario != null ? usuario.IdEmpresa : 0;
                var empresaAfip = usuario != null ? usuario.Empresa : null;

                if ((empresaAfip == null || empresaAfip.Cuit <= 0 || string.IsNullOrWhiteSpace(empresaAfip.NombreCertificado_pfx)) && idEmpresa > 0)
                {
                    var sucursalN = new Negocio.Sucursal(empresa, param);
                    empresaAfip = sucursalN.findEmpresaById(idEmpresa);
                }

                if (empresaAfip == null || empresaAfip.Cuit <= 0)
                    return Json(new { ok = false, msg = "No se encontró la configuración AFIP de la empresa actual." }, JsonRequestBehavior.AllowGet);
                var servicioPadron = new ConsultarPadronService(empresaAfip);
                var resultado = servicioPadron.ConsultarDatosContribuyente(cuitNormalizado);

                if (!resultado.Ok || resultado.Persona == null)
                    return Json(new { ok = false, msg = resultado.Mensaje ?? "No se encontraron datos para el CUIT especificado." }, JsonRequestBehavior.AllowGet);

                return Json(new
                {
                    ok = true,
                    razonSocial = resultado.Persona.RazonSocial,
                    identificacion = resultado.Persona.Identificacion,
                    domicilio = resultado.Persona.Domicilio,
                    ciudad = resultado.Persona.Ciudad
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { ok = false, msg = "No se pudo obtener los datos desde AFIP. " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public JsonResult BuscarPadronAfipAjax(string cuit)
        {
            try
            {
                string cuitNormalizado = NormalizarCuit(cuit);
                long cuitPersona;
                if (string.IsNullOrWhiteSpace(cuitNormalizado) || cuitNormalizado.Length != 11 || !long.TryParse(cuitNormalizado, out cuitPersona))
                    return Json(new { ok = false, msg = "Ingrese un CUIT válido de 11 dígitos.", tipo = "warning" }, JsonRequestBehavior.AllowGet);

                var empresaAfip = ObtenerEmpresaAfipActual();
                if (empresaAfip == null || empresaAfip.Cuit <= 0)
                    return Json(new { ok = false, msg = "No se encontró la configuración AFIP de la empresa actual.", tipo = "error" }, JsonRequestBehavior.AllowGet);

                var servicioPadron = new ConsultarPadronService(empresaAfip);
                var resultado = servicioPadron.ConsultarDatosContribuyente(cuitNormalizado);

                if (!resultado.Ok || resultado.Persona == null)
                {
                    return Json(new
                    {
                        ok = false,
                        msg = resultado.Mensaje ?? "No se encontraron datos para el CUIT ingresado.",
                        tipo = EsMensajeSinDatosAfip(resultado.Mensaje) ? "info" : "error"
                    }, JsonRequestBehavior.AllowGet);
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
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    ok = false,
                    msg = ConsultarPadronService.TraducirMensajeError(ex),
                    tipo = "error"
                }, JsonRequestBehavior.AllowGet);
            }
        }

        private Entidades.Empresa ObtenerEmpresaAfipActual()
        {
            var usuario = Session["Usuario"] as Entidades.Usuario;
            int idEmpresa = usuario != null ? usuario.IdEmpresa : 0;
            var empresaAfip = usuario != null ? usuario.Empresa : null;

            if ((empresaAfip == null || empresaAfip.Cuit <= 0 || string.IsNullOrWhiteSpace(empresaAfip.NombreCertificado_pfx)) && idEmpresa > 0)
            {
                var sucursalN = new Negocio.Sucursal(empresa, param);
                empresaAfip = sucursalN.findEmpresaById(idEmpresa);
            }

            return empresaAfip;
        }

        private bool EsMensajeSinDatosAfip(string mensaje)
        {
            return !string.IsNullOrWhiteSpace(mensaje)
                && mensaje.IndexOf("No se encontraron datos", StringComparison.OrdinalIgnoreCase) >= 0;
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
            var usuario = Session["Usuario"] as Entidades.Usuario;
            int idEmpresaUsuario = usuario != null ? usuario.IdEmpresa : 0;
            return !(idEmpresaUsuario > 0 && idEmpresaPersona == 0);
        }

        private PersonaEditVm CrearViewModel(Entidades.Persona persona, bool esEdicion)
        {
            var usuario = Session["Usuario"] as Entidades.Usuario;
            bool tieneMovimientos = esEdicion && persona != null && persona.IdPersona > 0 && oPersonaN.personaTieneCompras_Ventas(persona.IdPersona);
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
            return usuario != null
                && (usuario.Admin || PermisosHelper.TienePermiso(Session, Permisos.Finanza.VerCtasCtes, null));
        }

        private void CargarIvas(PersonaEditVm model)
        {
            model.Ivas.Clear();

            DataTable dtIva = oPersonaN.getIva() ?? new DataTable();
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

            int idPersonaCuitEncontrado = oPersonaN.existeCuit(model.Cuit);
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
