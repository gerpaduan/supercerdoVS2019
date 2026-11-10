// Port de Web/Controllers/ParametrosController.cs (ver docs/DECISIONS.md, migracion ASP.NET Core,
// Modulo 6 -- Reportes y administracion). Grilla de parametros de la empresa actual (catalogo
// general + valor particular por tenant). Usuario/empresa reales via IUsuarioSesionService
// (login real, ver docs/DECISIONS.md 2026-09-06) -- ya no hay stub hardcodeado.
//
// Rediseno 2026-11-10: la pantalla agrupa los parametros con titulo/explicacion
// (WebCore/Models/ParametroAyudaCatalogo.cs) y oculta los que la web no usa (ver
// EsParametroOculto). El antiguo chequeo por CUIT (EsParametroVisibleSoloParaCuitEspecial)
// quedo sin efecto: sus 5 parametros pasan a estar ocultos para todos.
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Utilidades;
using WebCore.Models;

namespace WebCore.Controllers
{
    public class ParametrosController : Controller
    {
        private const int TIPO_STRING = 0;
        private const int TIPO_DECIMAL = 1;
        private const int TIPO_BOOL = 2;
        private const int TIPO_INT = 3;
        private const int TIPO_LONG = 4;

        private readonly WebCore.Services.IUsuarioSesionService _sesion;
        private readonly IEmpresaContext _empresa;
        private readonly IParametrosContext _param;
        private readonly Negocio.Parametros _oParametrosN;

        private Entidades.Usuario _usuarioActual => _sesion.UsuarioActual;

        public ParametrosController(WebCore.Services.IUsuarioSesionService sesion)
        {
            _sesion = sesion;
            _empresa = sesion.Empresa;
            _param = WebCore.Infrastructure.NegocioFactory.CrearParametros(_empresa);
            _param.Reload();
            _oParametrosN = WebCore.Infrastructure.NegocioFactory.CrearParametros(_empresa);
        }

        [HttpGet]
        public IActionResult Index()
        {
            var model = CrearViewModel(_usuarioActual, true);
            ViewBag.Title = "Parámetros";
            ViewBag.Seccion = "Parámetros";
            return View("~/Views/Parametros/Index.cshtml", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Guardar(ParametrosEmpresaIndexVm model)
        {
            if (!PuedeAdministrar(_usuarioActual))
            {
                TempData["AlertType"] = "info";
                TempData["AlertTitle"] = "Parámetros";
                TempData["AlertMsg"] = "Puede consultar la configuración vigente, pero solo un administrador puede modificar los parámetros de la empresa.";
                return RedirectToAction("Index");
            }

            model = model ?? new ParametrosEmpresaIndexVm();
            model.PuedeAdministrar = true;
            model.SoloLecturaInicial = false;
            model.MensajePermiso = "";
            model.Items = FiltrarParametrosVisibles(model.Items, _usuarioActual)
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
                _oParametrosN.GuardarGrid(dtGuardar);
                _param.Reload();

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
            DataTable dt = _oParametrosN.ObtenerGrid() ?? new DataTable();
            bool puedeAdministrar = PuedeAdministrar(usuario);

            var model = new ParametrosEmpresaIndexVm
            {
                PuedeAdministrar = puedeAdministrar,
                SoloLecturaInicial = soloLecturaInicial,
                MensajePermiso = puedeAdministrar
                    ? "Los cambios se aplican solo a su empresa. Presione Modificar para editarlos."
                    : "Puede consultar la configuración vigente de su empresa. Solo un usuario administrador puede modificar estos parámetros."
            };

            model.Items = dt.AsEnumerable()
                .Select(MapItem)
                .Where(x => DebeMostrarParametro(x != null ? x.Nombre : null))
                .OrderBy(x => x.Nombre ?? "")
                .ToList();

            return model;
        }

        private IEnumerable<ParametroEmpresaItemVm> FiltrarParametrosVisibles(
            IEnumerable<ParametroEmpresaItemVm> items,
            Entidades.Usuario usuario)
        {
            return (items ?? Enumerable.Empty<ParametroEmpresaItemVm>())
                .Where(x => x != null && DebeMostrarParametro(x.Nombre));
        }

        private ParametroEmpresaItemVm MapItem(DataRow row)
        {
            string valor = row == null || row["valor"] == DBNull.Value ? "" : Convert.ToString(row["valor"]);
            int tipo = row == null || row["tipo"] == DBNull.Value ? TIPO_STRING : Convert.ToInt32(row["tipo"]);

            string nombre = row == null || row["nombre"] == DBNull.Value ? "" : Convert.ToString(row["nombre"]) ?? "";
            string descripcion = row == null || row["descripcion"] == DBNull.Value ? "" : Convert.ToString(row["descripcion"]) ?? "";

            return new ParametroEmpresaItemVm
            {
                IdParametro = row == null || row["idParametro"] == DBNull.Value ? 0 : Convert.ToInt32(row["idParametro"]),
                Nombre = nombre,
                Descripcion = descripcion,
                Tipo = tipo,
                TipoDescripcion = ObtenerTipoDescripcion(tipo),
                Valor = valor ?? "",
                ValorBool = EsValorBoolTrue(valor),
                Ayuda = ParametroAyudaCatalogo.Obtener(nombre, descripcion),
                FormasPagoSeleccionadas = EsFormasPagoFactura(nombre)
                    ? WebCore.Helpers.FormasPagoFacturaAutomatica.Parse(valor).ToList()
                    : new List<string>()
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

            // Casillas por forma de pago: el valor se arma con las tildadas (ninguna = "Ninguna").
            if (EsFormasPagoFactura(item.Nombre))
            {
                valorNormalizado = WebCore.Helpers.FormasPagoFacturaAutomatica.Serializar(item.FormasPagoSeleccionadas);
                return "";
            }

            if (item.Tipo == TIPO_BOOL)
            {
                valorNormalizado = item.ValorBool ? "1" : "0";
                return "";
            }

            // Un valor vacio es valido en los numericos: el sistema usa el valor por defecto de cada parametro
            // (saldoMinimoInformar vacio = 100, codProdGenerico vacio = sin producto de precio libre; los getters de
            // Negocio.Parametros devuelven el default ante un valor en blanco). Antes un numerico sin cargar rechazaba
            // el guardado de TODA la pantalla ("Valor invalido (decimal)"), aunque el administrador no lo hubiera tocado.
            if (valorNormalizado.Length == 0 && item.Tipo != TIPO_STRING)
                return "";

            // Parametros que son un entero de hasta N digitos (ej. codProdGenerico: se compara con el codigo del POS):
            // el formato lo fija el catalogo de ayuda, mas estricto que el tipo decimal con el que figura en la base.
            var ayudaItem = ParametroAyudaCatalogo.Obtener(item.Nombre, item.Descripcion);
            if (ayudaItem.EnteroMaxDigitos is int maxDigitos)
                return ParametroAyudaCatalogo.ValidarEntero(valorNormalizado, maxDigitos, out valorNormalizado);

            if (item.Tipo == TIPO_INT)
            {
                int valorInt;
                if (!int.TryParse(valorNormalizado, out valorInt))
                    return "Valor inválido (int) para: " + (item.Nombre ?? "parámetro");

                valorNormalizado = valorInt.ToString();
                return "";
            }

            if (item.Tipo == TIPO_LONG)
            {
                long valorLong;
                if (!long.TryParse(valorNormalizado, out valorLong))
                    return "Valor inválido (long) para: " + (item.Nombre ?? "parámetro");

                valorNormalizado = valorLong.ToString();
                return "";
            }

            if (item.Tipo == TIPO_DECIMAL)
            {
                string texto = valorNormalizado.Replace(',', '.');
                decimal valorDecimal;
                if (!decimal.TryParse(texto, NumberStyles.Any, CultureInfo.InvariantCulture, out valorDecimal))
                    return "Valor inválido (decimal) para: " + (item.Nombre ?? "parámetro");

                valorNormalizado = valorDecimal.ToString(CultureInfo.InvariantCulture);
                return "";
            }

            return "";
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
                item.Ayuda = ParametroAyudaCatalogo.Obtener(item.Nombre, item.Descripcion);
            }
        }

        private static bool EsFormasPagoFactura(string? nombre)
        {
            return string.Equals(nombre?.Trim(), Entidades.ParamKeys.FormasPagoFacturaAutomatica, StringComparison.OrdinalIgnoreCase);
        }

        private bool PuedeAdministrar(Entidades.Usuario usuario)
        {
            return usuario != null && usuario.IdEmpresa == _empresa.IdEmpresa && usuario.Admin;
        }

        // Publico solo para poder testearlo (WebCore.Tests); un metodo estatico no es una accion MVC.
        public static bool DebeMostrarParametro(string? nombreParametro)
        {
            if (string.IsNullOrWhiteSpace(nombreParametro))
                return false;

            return !EsParametroOculto(nombreParametro.Trim());
        }

        private static bool EsParametroOculto(string nombreParametro)
        {
            switch (nombreParametro)
            {
                case "idCompraEgresoCaja":
                case "idConsumidorFinal":
                case "idCtaCteEgresoCaja":
                case "idIndefinido":
                case "idPagoCobroEgresoCaja":
                case "idPagoTarjetaEgresoCaja":
                // Ajuste por forma de pago: desde 2026-09-25 se edita SOLO en Productos (modal
                // "Ajuste por forma de pago", como porcentaje). Ocultos aca para no editarlos dos
                // veces ni como factor. porcAjBilletera: sin forma de pago asociada, huerfano.
                case "porcAjEfectivo":
                case "porcAjDebito":
                case "porcAjCredito":
                case "porcAjCtaCte":
                case "porcAjQr":
                case "porcAjTranf":
                case "porcAjBilletera":
                // Rediseno 2026-11-10: ninguna pantalla de WebCore los lee. Los 5 primeros los usa solo la
                // app de escritorio (Presentacion/), que el usuario sigue manejando hasta migrarla a WebCore;
                // los 2 ultimos no los lee nadie. Siguen en la base y conservan su valor (el guardado hace
                // upsert solo de las filas posteadas). Al migrar una funcion a WebCore: sacar el parametro de
                // esta lista y darlo de alta en ParametroAyudaCatalogo.
                case "importeMaxRedondeo":
                case "limiteKgParaAjuste":
                case "loginRapidoElaborado":
                case "loginRapidoMovimiento":
                case "mayuscula":
                case "loginRapidoStock":
                case "minAccesoUltimaVentaVendedor":
                    return true;
                default:
                    return false;
            }
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
