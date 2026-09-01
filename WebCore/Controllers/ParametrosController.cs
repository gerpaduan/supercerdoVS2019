// Port de Web/Controllers/ParametrosController.cs (ver docs/DECISIONS.md, migracion ASP.NET Core,
// Modulo 6 -- Reportes y administracion). Grilla de parametros de la empresa actual (catalogo
// general + valor particular por tenant). Mismo criterio de stub que Empresa/SucursalController.
//
// ObtenerEmpresaCuit(usuario) en el original compara Session["Usuario"].Empresa.Cuit contra el
// mismo CUIT fijo (20306210786) ya verificado real para la empresa del stub en otros controllers
// de esta migracion (ver Compras/ExistenciaPorSucursales) -- se hardcodea a true en vez de
// reproducir el chequeo via Session.
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

        private sealed class StubEmpresaContext : IEmpresaContext
        {
            public int IdEmpresa => 1;
        }

        private readonly IEmpresaContext _empresa = new StubEmpresaContext();
        private readonly IParametrosContext _param;
        private readonly Negocio.Parametros _oParametrosN;

        private readonly Entidades.Usuario _usuarioActual = new Entidades.Usuario
        {
            Id = 2,
            Admin = true,
            IdEmpresa = 1,
            IdSucursal = 2,
            Nombre = "ger"
        };

        public ParametrosController()
        {
            _param = new Negocio.Parametros(_empresa);
            _param.Reload();
            _oParametrosN = new Negocio.Parametros(_empresa);
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

        private IEnumerable<ParametroEmpresaItemVm> FiltrarParametrosVisibles(
            IEnumerable<ParametroEmpresaItemVm> items,
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
                Nombre = row == null || row["nombre"] == DBNull.Value ? "" : Convert.ToString(row["nombre"]) ?? "",
                Descripcion = row == null || row["descripcion"] == DBNull.Value ? "" : Convert.ToString(row["descripcion"]) ?? "",
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
                return "";
            }

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
            }
        }

        private bool PuedeAdministrar(Entidades.Usuario usuario)
        {
            return usuario != null && usuario.IdEmpresa == _empresa.IdEmpresa && usuario.Admin;
        }

        private static bool DebeMostrarParametro(string? nombreParametro, Entidades.Usuario usuario)
        {
            if (string.IsNullOrWhiteSpace(nombreParametro))
                return false;

            string nombre = nombreParametro.Trim();
            if (EsParametroSiempreOculto(nombre))
                return false;

            if (EsParametroVisibleSoloParaCuitEspecial(nombre))
                return true; // TODO(claude): ver comentario de cabecera -- CUIT del stub ya verificado real.

            return true;
        }

        private static bool EsParametroSiempreOculto(string nombreParametro)
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

        private static bool EsParametroVisibleSoloParaCuitEspecial(string nombreParametro)
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
