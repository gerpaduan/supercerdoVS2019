using System;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using Web.Models;

namespace Web.Controllers
{
    public class ParametrosController : BaseController
    {
        private const long CuitParametrosEspeciales = 20306210786;
        private const int TIPO_STRING = 0;
        private const int TIPO_DECIMAL = 1;
        private const int TIPO_BOOL = 2;
        private const int TIPO_INT = 3;
        private const int TIPO_LONG = 4;

        private Negocio.Parametros oParametrosN;

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);
            if (filterContext.Result != null) return;

            oParametrosN = new Negocio.Parametros(empresa);
        }

        [HttpGet]
        public ActionResult Index()
        {
            var usuario = ObtenerUsuarioActual();
            if (usuario == null || usuario.IdEmpresa != empresa.IdEmpresa)
            {
                return VistaAccesoDenegado("Parámetros");
            }

            var model = CrearViewModel(usuario, true);
            ViewBag.Title = "Parámetros";
            ViewBag.Seccion = "Parámetros";
            return View("~/Views/Parametros/Index.cshtml", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Guardar(ParametrosEmpresaIndexVm model)
        {
            var usuario = ObtenerUsuarioActual();
            if (usuario == null || usuario.IdEmpresa != empresa.IdEmpresa)
            {
                return VistaAccesoDenegado("Parámetros");
            }

            if (!PuedeAdministrar(usuario))
            {
                TempData["AlertType"] = "info";
                TempData["AlertTitle"] = "Parámetros";
                TempData["AlertMsg"] = "Puede consultar la configuración vigente, pero solo un administrador puede modificar los parámetros de la empresa.";
                return RedirectToAction("Index");
            }

            model = model ?? new ParametrosEmpresaIndexVm();
            model.PuedeAdministrar = true;
            model.SoloLecturaInicial = false;
            model.MensajePermiso = null;
            model.Items = FiltrarParametrosVisibles(model.Items, usuario)
                .OrderBy(x => x != null ? x.Nombre ?? "" : "")
                .ToList();

            ValidarModel(model);

            if (!ModelState.IsValid)
            {
                CompletarTipos(model);
                ViewBag.Title = "Parámetros";
                ViewBag.Seccion = "Parámetros";
                return View("~/Views/Parametros/Index.cshtml", model);
            }

            DataTable dtGuardar = CrearDataTableGuardar(model);

            try
            {
                oParametrosN.GuardarGrid(dtGuardar);
                if (param != null)
                {
                    param.Reload();
                    Session["PARAM_CTX"] = param;
                }

                TempData["AlertType"] = "success";
                TempData["AlertTitle"] = "Parámetros";
                TempData["AlertMsg"] = "Los parámetros de la empresa se guardaron correctamente.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                CompletarTipos(model);
                ViewBag.Title = "Parámetros";
                ViewBag.Seccion = "Parámetros";
                return View("~/Views/Parametros/Index.cshtml", model);
            }
        }

        private ParametrosEmpresaIndexVm CrearViewModel(Entidades.Usuario usuario, bool soloLecturaInicial)
        {
            DataTable dt = oParametrosN.ObtenerGrid() ?? new DataTable();
            bool puedeAdministrar = PuedeAdministrar(usuario);

            var model = new ParametrosEmpresaIndexVm
            {
                PuedeAdministrar = puedeAdministrar,
                SoloLecturaInicial = soloLecturaInicial,
                MensajePermiso = puedeAdministrar
                    ? "Los parámetros se muestran inicialmente en modo lectura. Presione Modificar para editar y Guardar para aplicar los cambios a su empresa."
                    : "Puede consultar la configuración vigente de su empresa. Solo un usuario administrador puede modificar estos parámetros."
            };

            model.Items = dt.AsEnumerable()
                .Select(MapItem)
                .Where(x => DebeMostrarParametro(x != null ? x.Nombre : null, usuario))
                .OrderBy(x => x.Nombre ?? "")
                .ToList();

            return model;
        }

        private System.Collections.Generic.IEnumerable<ParametroEmpresaItemVm> FiltrarParametrosVisibles(
            System.Collections.Generic.IEnumerable<ParametroEmpresaItemVm> items,
            Entidades.Usuario usuario)
        {
            return (items ?? Enumerable.Empty<ParametroEmpresaItemVm>())
                .Where(x => x != null && DebeMostrarParametro(x.Nombre, usuario));
        }

        private ParametroEmpresaItemVm MapItem(DataRow row)
        {
            string valor = row == null || row["valor"] == DBNull.Value ? "" : Convert.ToString(row["valor"]);
            int tipo = row == null || row["tipo"] == DBNull.Value ? TIPO_STRING : Convert.ToInt32(row["tipo"]);

            return new ParametroEmpresaItemVm
            {
                IdParametro = row == null || row["idParametro"] == DBNull.Value ? 0 : Convert.ToInt32(row["idParametro"]),
                Nombre = row == null || row["nombre"] == DBNull.Value ? "" : Convert.ToString(row["nombre"]),
                Descripcion = row == null || row["descripcion"] == DBNull.Value ? "" : Convert.ToString(row["descripcion"]),
                Tipo = tipo,
                TipoDescripcion = ObtenerTipoDescripcion(tipo),
                Valor = valor ?? "",
                ValorBool = EsValorBoolTrue(valor)
            };
        }

        private void ValidarModel(ParametrosEmpresaIndexVm model)
        {
            if (model.Items == null || model.Items.Count == 0)
            {
                ModelState.AddModelError("", "No hay parámetros para guardar.");
                return;
            }

            for (int i = 0; i < model.Items.Count; i++)
            {
                var item = model.Items[i];
                if (item == null || item.IdParametro <= 0)
                {
                    ModelState.AddModelError("", "Se detectó un parámetro inválido en la grilla.");
                    continue;
                }

                string valorNormalizado;
                string error = ValidarYNormalizarValor(item, out valorNormalizado);
                item.Valor = valorNormalizado;
                item.TipoDescripcion = ObtenerTipoDescripcion(item.Tipo);

                if (!string.IsNullOrWhiteSpace(error))
                {
                    ModelState.AddModelError("Items[" + i + "].Valor", error);
                }
            }
        }

        private string ValidarYNormalizarValor(ParametroEmpresaItemVm item, out string valorNormalizado)
        {
            valorNormalizado = item != null ? (item.Valor ?? "").Trim() : "";
            if (item == null)
                return "Se detectó un parámetro inválido.";

            if (item.Tipo == TIPO_BOOL)
            {
                valorNormalizado = item.ValorBool ? "1" : "0";
                return null;
            }

            if (item.Tipo == TIPO_INT)
            {
                int valorInt;
                if (!int.TryParse(valorNormalizado, out valorInt))
                    return "Valor inválido (int) para: " + (item.Nombre ?? "parámetro");

                valorNormalizado = valorInt.ToString();
                return null;
            }

            if (item.Tipo == TIPO_LONG)
            {
                long valorLong;
                if (!long.TryParse(valorNormalizado, out valorLong))
                    return "Valor inválido (long) para: " + (item.Nombre ?? "parámetro");

                valorNormalizado = valorLong.ToString();
                return null;
            }

            if (item.Tipo == TIPO_DECIMAL)
            {
                string texto = valorNormalizado.Replace(',', '.');
                decimal valorDecimal;
                if (!decimal.TryParse(texto, NumberStyles.Any, CultureInfo.InvariantCulture, out valorDecimal))
                    return "Valor inválido (decimal) para: " + (item.Nombre ?? "parámetro");

                valorNormalizado = valorDecimal.ToString(CultureInfo.InvariantCulture);
                return null;
            }

            return null;
        }

        private DataTable CrearDataTableGuardar(ParametrosEmpresaIndexVm model)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("idParametro", typeof(int));
            dt.Columns.Add("valor", typeof(string));

            foreach (var item in model.Items)
            {
                if (item == null || item.IdParametro <= 0) continue;

                DataRow row = dt.NewRow();
                row["idParametro"] = item.IdParametro;
                row["valor"] = item.Tipo == TIPO_BOOL
                    ? (item.ValorBool ? "1" : "0")
                    : (object)(item.Valor ?? "") ?? DBNull.Value;
                dt.Rows.Add(row);
            }

            return dt;
        }

        private void CompletarTipos(ParametrosEmpresaIndexVm model)
        {
            if (model == null || model.Items == null) return;

            foreach (var item in model.Items)
            {
                if (item == null) continue;
                item.TipoDescripcion = ObtenerTipoDescripcion(item.Tipo);
            }
        }

        private bool PuedeAdministrar(Entidades.Usuario usuario)
        {
            return usuario != null && usuario.IdEmpresa == empresa.IdEmpresa && usuario.Admin;
        }

        private bool DebeMostrarParametro(string nombreParametro, Entidades.Usuario usuario)
        {
            if (string.IsNullOrWhiteSpace(nombreParametro))
                return false;

            string nombre = nombreParametro.Trim();
            if (EsParametroSiempreOculto(nombre))
                return false;

            if (EsParametroVisibleSoloParaCuitEspecial(nombre))
                return ObtenerEmpresaCuit(usuario) == CuitParametrosEspeciales;

            return true;
        }

        private bool EsParametroSiempreOculto(string nombreParametro)
        {
            switch (nombreParametro)
            {
                case "idCompraEgresoCaja":
                case "idConsumidorFinal":
                case "idCtaCteEgresoCaja":
                case "idIndefinido":
                case "idPagoCobroEgresoCaja":
                case "idPagoTarjetaEgresoCaja":
                    return true;
                default:
                    return false;
            }
        }

        private bool EsParametroVisibleSoloParaCuitEspecial(string nombreParametro)
        {
            switch (nombreParametro)
            {
                case "importeMaxRedondeo":
                case "limiteKgParaAjuste":
                case "loginRapidoElaborado":
                case "loginRapidoMovimiento":
                case "loginRapidoStock":
                    return true;
                default:
                    return false;
            }
        }

        private long ObtenerEmpresaCuit(Entidades.Usuario usuario)
        {
            return usuario != null && usuario.Empresa != null ? usuario.Empresa.Cuit : 0;
        }

        private Entidades.Usuario ObtenerUsuarioActual()
        {
            return Session["Usuario"] as Entidades.Usuario;
        }

        private static bool EsValorBoolTrue(string valor)
        {
            string texto = (valor ?? "").Trim();
            return texto == "1" || string.Equals(texto, "true", StringComparison.OrdinalIgnoreCase);
        }

        private string ObtenerTipoDescripcion(int tipo)
        {
            switch (tipo)
            {
                case TIPO_DECIMAL:
                    return "Decimal";
                case TIPO_BOOL:
                    return "Sí / No";
                case TIPO_INT:
                    return "Entero";
                case TIPO_LONG:
                    return "Número largo";
                default:
                    return "Texto";
            }
        }
    }
}
