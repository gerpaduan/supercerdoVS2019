// Comprobantes del proveedor (factura, nota de credito/debito) y titular propio de las compras: acciones del modal
// "Comprobantes", estado de factura de la bandeja de Compras/Index y titulares del selector.
// Compras: titular propio y comprobantes del proveedor (2026-11-09, ver docs/DECISIONS.md). Solo Postgres: en SQL Server
// NegocioFactory.CrearComprobanteCompra devuelve null, las acciones responden "no disponible" y las vistas no muestran nada.
//
// Permisos: ver los comprobantes exige "Ver compras" (con la fecha de la compra, como el detalle). Cargar, editar, borrar o
// cambiar el titular exige "Modificar compra", pero SIN el limite de dias: la factura suele llegar semanas despues del remito
// y no puede quedar bloqueada por la ventana de dias de la compra (se evalua con la fecha de hoy y el propio operador como
// creador, asi que solo cuenta que tenga el permiso de edicion). Admin siempre puede.
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using WebCore.Models;

namespace WebCore.Controllers
{
    public partial class ComprasController
    {
        private Negocio.ComprobanteCompra _comprobanteN;
        private Negocio.TitularFiscal _titularN;
        private bool _serviciosComprobantesListos;

        // Se arman la primera vez que se usan: la mayoria de las acciones de Compras no los necesitan.
        private void AsegurarServiciosComprobantes()
        {
            if (_serviciosComprobantesListos) return;
            _comprobanteN = WebCore.Infrastructure.NegocioFactory.CrearComprobanteCompra(_empresa);
            _titularN = WebCore.Infrastructure.NegocioFactory.CrearTitularFiscal(_empresa);
            _serviciosComprobantesListos = true;
        }

        private Negocio.ComprobanteCompra ComprobanteN { get { AsegurarServiciosComprobantes(); return _comprobanteN; } }
        private Negocio.TitularFiscal TitularN { get { AsegurarServiciosComprobantes(); return _titularN; } }

        // La empresa maneja comprobantes de compra (Postgres).
        private bool ComprobantesDisponibles { get { return ComprobanteN != null; } }

        // La empresa tiene 2 o mas titulares: solo entonces se muestran selectores y columnas de titular.
        private bool MostrarTitulares()
        {
            return TitularN != null && TitularN.TieneVariosTitulares(_empresa.IdEmpresa);
        }

        private List<TitularCompraOpcionVm> ListarTitularesCompra()
        {
            if (TitularN == null) return new List<TitularCompraOpcionVm>();
            return TitularN.Listar(_empresa.IdEmpresa, false)
                .Select(t => new TitularCompraOpcionVm
                {
                    IdTitular = t.IdTitular,
                    Nombre = t.NombreCorto,
                    EsPrincipal = t.EsPredeterminado,
                    Seleccionable = t.Habilitado && !t.Revocado
                })
                .ToList();
        }

        // Id del titular principal (el que representa un titular NULL en la base), o 0 si no hay.
        private int IdTitularPrincipal()
        {
            if (TitularN == null) return 0;
            var principal = TitularN.ObtenerPrincipal(_empresa.IdEmpresa);
            return principal != null ? principal.IdTitular : 0;
        }

        // ------------------------------------------------------------------ permisos

        // null = puede. escritura = cargar/editar/borrar comprobantes o cambiar el titular (ver cabecera del archivo).
        private string ErrorPermisoComprobantes(Entidades.Compra compra, bool escritura)
        {
            if (_usuarioActual.EsUsuarioProduccion && ObtenerOperadorModulo("Compras") == null)
                return "Para operar compras hay que autorizar el modulo Compras desde el menu.";

            var operador = ResolverOperadorModulo("Compras", _usuarioActual);
            bool puede = escritura
                ? _oUsuarioN.tienePermiso(operador, Entidades.Permisos.Compra.ModificarCompra, DateTime.Today, operador.Id)
                : _oUsuarioN.tienePermiso(operador, Entidades.Permisos.Compra.VerCompras, compra.FechaCompra, -1);

            return puede ? null : (escritura
                ? "No tenés permiso para modificar compras: no podés cargar ni cambiar comprobantes."
                : "No tenés permiso para consultar esta compra.");
        }

        private JsonResult NoDisponibleJson()
        {
            return Json(new { ok = false, mensaje = "Los comprobantes del proveedor solo están disponibles con la base Postgres." });
        }

        // Id del usuario que opera (el operador autorizado en cuentas de produccion).
        private int IdUsuarioOperador()
        {
            var operador = ResolverOperadorModulo("Compras", _usuarioActual);
            return operador != null ? operador.Id : _usuarioActual.Id;
        }

        // ------------------------------------------------------------------ modal

        // Contenido del modal: la compra, sus comprobantes, el formulario para agregar y los totales.
        [HttpGet]
        public IActionResult Comprobantes(int id)
        {
            if (!ComprobantesDisponibles)
                return StatusCode(404, "Los comprobantes del proveedor solo están disponibles con la base Postgres.");

            var compra = _oCompraN.findById_convertToCompra(id);
            if (compra == null || compra.IdCompra <= 0) return NotFound();

            string sinPermiso = ErrorPermisoComprobantes(compra, false);
            if (sinPermiso != null) return StatusCode(403, sinPermiso);

            var detalle = ComprobanteN.ObtenerDetalle(id);
            if (detalle == null) return NotFound();

            var vm = new ComprobantesCompraVm
            {
                Detalle = detalle,
                Proveedor = compra.Proveedor != null ? compra.Proveedor.RazonSocial : "",
                NroRemito = compra.NroRemito ?? "",
                FechaCompra = compra.FechaCompra,
                PuedeEditar = ErrorPermisoComprobantes(compra, true) == null,
                MostrarTitulares = MostrarTitulares(),
                Titulares = ListarTitularesCompra(),
                AlicuotaDefault = WebCore.Helpers.ComprasComprobantesSettings.AlicuotaIvaDefault,
                Alicuotas = WebCore.Helpers.ComprasComprobantesSettings.Alicuotas,
                CompraAdmiteComprobantes = Negocio.ValidacionComprobanteCompra.TipoCompraAdmiteComprobantes(compra.TipoCompra)
            };
            vm.IdTitularActual = detalle.IdTitular ?? IdTitularPrincipal();

            return PartialView("~/Views/Compras/_ComprobantesCompra.cshtml", vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult GuardarComprobante(ComprobanteCompraFormVm form)
        {
            if (!ComprobantesDisponibles) return NoDisponibleJson();
            if (form == null || form.IdCompra <= 0) return Json(new { ok = false, mensaje = "No se indicó la compra." });

            try
            {
                var compra = _oCompraN.findById_convertToCompra(form.IdCompra);
                if (compra == null || compra.IdCompra <= 0) return Json(new { ok = false, mensaje = "La compra no existe." });

                string sinPermiso = ErrorPermisoComprobantes(compra, true);
                if (sinPermiso != null) return Json(new { ok = false, mensaje = sinPermiso });

                Entidades.ComprobanteCompra comprobante;
                string errorForm = ArmarComprobante(form, out comprobante);
                if (errorForm != null) return Json(new { ok = false, mensaje = errorForm });

                int idComprobante;
                string error = ComprobanteN.Guardar(comprobante, IdUsuarioOperador(), out idComprobante);
                if (error != null) return Json(new { ok = false, mensaje = error });

                return Json(new { ok = true, mensaje = "Comprobante guardado.", idComprobante });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("Error al guardar el comprobante de la compra " + form.IdCompra + ": " + ex);
                return Json(new { ok = false, mensaje = "No se pudo guardar el comprobante. " + ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EliminarComprobante(int idComprobante, int idCompra, string motivo)
        {
            if (!ComprobantesDisponibles) return NoDisponibleJson();

            try
            {
                var compra = _oCompraN.findById_convertToCompra(idCompra);
                if (compra == null || compra.IdCompra <= 0) return Json(new { ok = false, mensaje = "La compra no existe." });

                string sinPermiso = ErrorPermisoComprobantes(compra, true);
                if (sinPermiso != null) return Json(new { ok = false, mensaje = sinPermiso });

                // El comprobante tiene que ser de la compra indicada (el id viaja en el formulario).
                var detalle = ComprobanteN.ObtenerDetalle(idCompra);
                if (detalle == null || detalle.Comprobantes.All(c => c.IdComprobante != idComprobante))
                    return Json(new { ok = false, mensaje = "El comprobante no pertenece a esta compra." });

                string error = ComprobanteN.Eliminar(idComprobante, IdUsuarioOperador(), motivo);
                return error != null ? Json(new { ok = false, mensaje = error }) : Json(new { ok = true, mensaje = "Comprobante eliminado." });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("Error al eliminar el comprobante " + idComprobante + ": " + ex);
                return Json(new { ok = false, mensaje = "No se pudo eliminar el comprobante. " + ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult MarcarSinFactura(int idCompra, bool volverAPendiente = false)
        {
            if (!ComprobantesDisponibles) return NoDisponibleJson();

            try
            {
                var compra = _oCompraN.findById_convertToCompra(idCompra);
                if (compra == null || compra.IdCompra <= 0) return Json(new { ok = false, mensaje = "La compra no existe." });

                string sinPermiso = ErrorPermisoComprobantes(compra, true);
                if (sinPermiso != null) return Json(new { ok = false, mensaje = sinPermiso });

                string error = volverAPendiente
                    ? ComprobanteN.VolverAPendiente(idCompra, IdUsuarioOperador())
                    : ComprobanteN.MarcarSinFactura(idCompra, IdUsuarioOperador());
                return error != null ? Json(new { ok = false, mensaje = error }) : Json(new { ok = true, mensaje = "Listo." });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("Error al cambiar el estado de factura de la compra " + idCompra + ": " + ex);
                return Json(new { ok = false, mensaje = "No se pudo actualizar la compra. " + ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CambiarTitularCompra(int idCompra, int idTitular, bool confirmado = false)
        {
            if (!ComprobantesDisponibles) return NoDisponibleJson();

            try
            {
                var compra = _oCompraN.findById_convertToCompra(idCompra);
                if (compra == null || compra.IdCompra <= 0) return Json(new { ok = false, mensaje = "La compra no existe." });

                string sinPermiso = ErrorPermisoComprobantes(compra, true);
                if (sinPermiso != null) return Json(new { ok = false, mensaje = sinPermiso });

                bool requiereConfirmacion;
                string resultado = ComprobanteN.CambiarTitular(idCompra, idTitular > 0 ? (int?)idTitular : null,
                    IdUsuarioOperador(), confirmado, out requiereConfirmacion);

                if (requiereConfirmacion) return Json(new { ok = false, requiereConfirmacion = true, mensaje = resultado });
                return resultado != null ? Json(new { ok = false, mensaje = resultado }) : Json(new { ok = true, mensaje = "Titular actualizado." });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("Error al cambiar el titular de la compra " + idCompra + ": " + ex);
                return Json(new { ok = false, mensaje = "No se pudo cambiar el titular. " + ex.Message });
            }
        }

        // ------------------------------------------------------------------ helpers

        // Importe con punto decimal y sin separador de miles (la pantalla lo normaliza antes de enviarlo). Vacio = cero.
        internal static bool TryParseImporte(string texto, out decimal valor)
        {
            valor = 0m;
            string limpio = (texto ?? "").Trim();
            if (limpio.Length == 0) return true;
            return decimal.TryParse(limpio, NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out valor);
        }

        // Pasa los campos de texto del formulario a la entidad. Devuelve null si estan bien o el mensaje. La validacion de las reglas
        // (totales, duplicados, una sola factura) la hace Negocio.ComprobanteCompra.
        private static string ArmarComprobante(ComprobanteCompraFormVm form, out Entidades.ComprobanteCompra comprobante)
        {
            comprobante = null;

            DateTime fecha;
            if (!DateTime.TryParseExact((form.Fecha ?? "").Trim(), "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out fecha))
                return "Cargá la fecha del comprobante.";

            decimal neto, iva, otros, total, alicuota;
            if (!TryParseImporte(form.NetoGravado, out neto) || !TryParseImporte(form.Iva, out iva)
                || !TryParseImporte(form.OtrosImpuestos, out otros) || !TryParseImporte(form.Total, out total))
                return "Revisá los importes: tienen que ser números.";

            decimal? alicuotaIva = null;
            if (!string.IsNullOrWhiteSpace(form.AlicuotaIva))
            {
                if (!TryParseImporte(form.AlicuotaIva, out alicuota)) return "La alícuota de IVA no es un número.";
                alicuotaIva = alicuota;
            }

            comprobante = new Entidades.ComprobanteCompra
            {
                IdComprobante = form.IdComprobante,
                IdCompra = form.IdCompra,
                TipoComprobante = (form.TipoComprobante ?? "").Trim(),
                PtoVta = form.PtoVta,
                Numero = form.Numero,
                Fecha = fecha,
                NetoGravado = neto,
                Iva = iva,
                OtrosImpuestos = otros,
                Total = total,
                AlicuotaIva = alicuotaIva
            };
            return null;
        }

        // ------------------------------------------------------------------ bandeja (Compras/Index)

        // Suma al listado las columnas de titular y de factura de cada compra y aplica los filtros "Factura" y "Titular".
        // estadoFactura: "" = todas | PENDIENTE | FACTURADA | NO_LLEVA | SIN_DATO. idTitular 0 = todos (NULL cuenta como el principal).
        // Con SQL Server (sin comprobantes) devuelve el listado como vino.
        private DataTable AgregarInfoFactura(DataTable dt, string estadoFactura, int idTitular)
        {
            if (dt == null || !ComprobantesDisponibles || !dt.Columns.Contains("idCompra")) return dt;

            var ids = dt.Rows.Cast<DataRow>()
                .Where(r => r["idCompra"] != DBNull.Value)
                .Select(r => Convert.ToInt32(r["idCompra"]))
                .ToList();
            var info = ComprobanteN.InfoPorCompras(ids);
            int principal = IdTitularPrincipal();
            var nombres = ListarTitularesCompra().ToDictionary(t => t.IdTitular, t => t.Nombre);

            dt.Columns.Add("estadoFactura", typeof(string));
            dt.Columns.Add("idTitular", typeof(int));
            dt.Columns.Add("titularNombre", typeof(string));
            dt.Columns.Add("facturadoNeto", typeof(decimal));
            dt.Columns.Add("tipoFactura", typeof(string));
            dt.Columns.Add("numeroFactura", typeof(string));
            dt.Columns.Add("tieneNC", typeof(bool));
            dt.Columns.Add("tieneND", typeof(bool));

            foreach (DataRow row in dt.Rows)
            {
                Entidades.InfoFacturaCompra i;
                int idCompra = row["idCompra"] == DBNull.Value ? 0 : Convert.ToInt32(row["idCompra"]);
                if (!info.TryGetValue(idCompra, out i)) continue;

                int titular = i.IdTitular ?? principal;
                row["estadoFactura"] = i.EstadoFactura ?? "";
                row["idTitular"] = titular;
                row["titularNombre"] = nombres.ContainsKey(titular) ? nombres[titular] : "";
                row["facturadoNeto"] = i.FacturadoNeto;
                row["tipoFactura"] = i.TipoFactura ?? "";
                row["numeroFactura"] = i.NumeroFactura ?? "";
                row["tieneNC"] = i.TieneNotaCredito;
                row["tieneND"] = i.TieneNotaDebito;
            }

            bool filtraEstado = !string.IsNullOrWhiteSpace(estadoFactura);
            if (!filtraEstado && idTitular <= 0) return dt;

            // Se borran las filas que no cumplen (el DataTable es propio de esta peticion).
            var aBorrar = new List<DataRow>();
            foreach (DataRow row in dt.Rows)
            {
                string estado = row["estadoFactura"] == DBNull.Value ? "" : Convert.ToString(row["estadoFactura"]);
                bool cumpleEstado = !filtraEstado
                    || (estadoFactura == "SIN_DATO" ? estado.Length == 0 : string.Equals(estado, estadoFactura, StringComparison.Ordinal));
                int titularFila = row["idTitular"] == DBNull.Value ? 0 : Convert.ToInt32(row["idTitular"]);
                bool cumpleTitular = idTitular <= 0 || titularFila == idTitular;
                if (!cumpleEstado || !cumpleTitular) aBorrar.Add(row);
            }
            foreach (var row in aBorrar) dt.Rows.Remove(row);
            return dt;
        }
    }
}
