// Modulo "Actividades" (solo admin), 2026-09-07 -- pedido explicito del usuario, ver
// docs/DECISIONS.md. Junta 6 fuentes heterogeneas (cambios de precio de Corte, ventas con lineas
// anuladas, ventas con bonificacion/recargo aplicado manualmente por el cajero, egresos de caja
// que son gastos, movimientos de stock entre sucursales, compras/registros de stock, y altas o
// ediciones de formulas de elaborados) en una sola linea de tiempo ordenada por fecha, con
// filtro de rango (max 7 dias, default hoy) y paginacion de 30 items. Mismo patron de gate de
// permiso que AuditoriaLoginController (solo admin -- este modulo es exclusivamente de negocio/
// dueño, no hay permiso granular como en Auditoria de accesos).
using Microsoft.AspNetCore.Mvc;
using System.Globalization;

namespace WebCore.Controllers
{
    public class ActividadesController : Controller
    {
        private const int MaxDiasRango = 7;
        private const int ItemsPorPagina = 30;

        private readonly WebCore.Services.IUsuarioSesionService _sesion;

        public ActividadesController(WebCore.Services.IUsuarioSesionService sesion)
        {
            _sesion = sesion;
        }

        [HttpGet]
        public IActionResult Index(DateTime? fechaDesde, DateTime? fechaHasta, int pagina = 1)
        {
            var usuario = _sesion.UsuarioActual;
            if (!usuario.Admin)
                return View("~/Views/Shared/AccesoDenegado.cshtml");

            DateTime hasta = (fechaHasta ?? DateTime.Today).Date;
            DateTime desde = (fechaDesde ?? hasta).Date;

            // Intervalo maximo 7 dias (pedido explicito del usuario) -- si el request pide mas,
            // se recorta silenciosamente a los ultimos 7 dias del rango pedido en vez de tirar
            // error (mismo criterio "tolerante" que el resto de los filtros de fecha del sistema).
            if ((hasta - desde).TotalDays > MaxDiasRango)
                desde = hasta.AddDays(-MaxDiasRango);

            DateTime desdeConHora = desde.Date;
            DateTime hastaConHora = hasta.Date.AddDays(1).AddTicks(-1);

            var repo = WebCore.Infrastructure.NegocioFactory.CrearActividadRepository(_sesion.Empresa);
            var oCierreN = WebCore.Infrastructure.NegocioFactory.CrearCierreCaja(_sesion.Empresa, _sesion.Parametros);

            var items = new List<Models.ActividadItemVm>();

            foreach (System.Data.DataRow row in repo.ObtenerCambiosPrecio(desdeConHora, hastaConHora).Rows)
            {
                decimal anterior = ToDecimal(row["precio_anterior"]);
                decimal nuevo = ToDecimal(row["precio_nuevo"]);
                double pct = anterior != 0 ? (double)((nuevo - anterior) / anterior * 100) : 0;
                string signo = pct >= 0 ? "" : "-";
                items.Add(new Models.ActividadItemVm
                {
                    Fecha = (DateTime)row["fecha"],
                    Tipo = "Precio",
                    Descripcion = $"Se modificó el precio de {row["corte"]} a $ {nuevo:N2}, {signo}{Math.Abs(pct):N1}% (precio anterior $ {anterior:N2})"
                });
            }

            foreach (System.Data.DataRow row in repo.ObtenerVentasConLineasAnuladas(desdeConHora, hastaConHora).Rows)
            {
                int idVenta = Convert.ToInt32(row["idventa"]);
                string vendedor = row["vendedor"] == DBNull.Value ? "" : Convert.ToString(row["vendedor"]);
                items.Add(new Models.ActividadItemVm
                {
                    Fecha = (DateTime)row["fecha"],
                    Tipo = "Venta anulada",
                    Descripcion = $"Hubo ítems anulados en la venta #{idVenta}" + (string.IsNullOrWhiteSpace(vendedor) ? "" : $" (vendedor: {vendedor})"),
                    IdVenta = idVenta
                });
            }

            foreach (System.Data.DataRow row in repo.ObtenerVentasConBonificacionManual(desdeConHora, hastaConHora).Rows)
            {
                int idVenta = Convert.ToInt32(row["idventa"]);
                string vendedor = row["vendedor"] == DBNull.Value ? "" : Convert.ToString(row["vendedor"]);
                items.Add(new Models.ActividadItemVm
                {
                    Fecha = (DateTime)row["fecha"],
                    Tipo = "Precio modificado en venta",
                    Descripcion = $"Se aplicó un descuento/recargo manual en la venta #{idVenta}" + (string.IsNullOrWhiteSpace(vendedor) ? "" : $" (vendedor: {vendedor})"),
                    IdVenta = idVenta
                });
            }

            // Egresos de caja que son gastos (idSucursal=0/idUsuario=-1/idTipoEgresoCaja=0 ->
            // trae todas las sucursales sin filtrar, ver DatosPostgres/CierreCajaPg.cs).
            var egresos = oCierreN.obtenerEgresosCaja(0, -1, 0, "", desdeConHora, hastaConHora);
            if (egresos != null)
            {
                foreach (System.Data.DataRow row in egresos.Rows)
                {
                    bool esGasto = row.Table.Columns.Contains("Gasto") && row["Gasto"] != DBNull.Value && Convert.ToBoolean(row["Gasto"]);
                    if (!esGasto) continue;

                    decimal monto = ToDecimal(row["Monto"]);
                    string desc = row["Descripcion"] == DBNull.Value ? "" : Convert.ToString(row["Descripcion"]);
                    string tipo = row["TipoEgresoCaja"] == DBNull.Value ? "" : Convert.ToString(row["TipoEgresoCaja"]);
                    items.Add(new Models.ActividadItemVm
                    {
                        Fecha = (DateTime)row["Fecha"],
                        Tipo = "Egreso de caja",
                        Descripcion = $"Egreso de caja ({tipo}): $ {monto:N2}" + (string.IsNullOrWhiteSpace(desc) ? "" : $" -- {desc}")
                    });
                }
            }

            foreach (System.Data.DataRow row in repo.ObtenerMovimientos(desdeConHora, hastaConHora).Rows)
            {
                string origen = row["sucursal_origen"] == DBNull.Value ? "?" : Convert.ToString(row["sucursal_origen"]);
                string destino = row["sucursal_destino"] == DBNull.Value ? "?" : Convert.ToString(row["sucursal_destino"]);
                string usuarioMov = row["usuario"] == DBNull.Value ? "" : Convert.ToString(row["usuario"]);
                items.Add(new Models.ActividadItemVm
                {
                    Fecha = (DateTime)row["fecha"],
                    Tipo = "Movimiento",
                    Descripcion = $"Movimiento de stock de {origen} a {destino}" + (string.IsNullOrWhiteSpace(usuarioMov) ? "" : $" (por {usuarioMov})")
                });
            }

            foreach (System.Data.DataRow row in repo.ObtenerCompras(desdeConHora, hastaConHora).Rows)
            {
                string tipo = row["tipocompra"] == DBNull.Value ? "Compra" : Convert.ToString(row["tipocompra"]);
                string usuarioCompra = row["usuario"] == DBNull.Value ? "" : Convert.ToString(row["usuario"]);
                items.Add(new Models.ActividadItemVm
                {
                    Fecha = (DateTime)row["fecha"],
                    Tipo = "Compra/Stock",
                    Descripcion = $"{tipo} #{row["idcompra"]}" + (string.IsNullOrWhiteSpace(usuarioCompra) ? "" : $" (por {usuarioCompra})")
                });
            }

            foreach (System.Data.DataRow row in repo.ObtenerFormulas(desdeConHora, hastaConHora).Rows)
            {
                string producto = row["producto"] == DBNull.Value ? $"#{row["idformula"]}" : Convert.ToString(row["producto"]);
                string usuarioFormula = row["usuario"] == DBNull.Value ? "" : Convert.ToString(row["usuario"]);
                items.Add(new Models.ActividadItemVm
                {
                    Fecha = (DateTime)row["fecha"],
                    Tipo = "Elaborado",
                    Descripcion = $"Se dio de alta o editó la fórmula de {producto}" + (string.IsNullOrWhiteSpace(usuarioFormula) ? "" : $" (por {usuarioFormula})")
                });
            }

            items = items.OrderByDescending(i => i.Fecha).ToList();

            int totalItems = items.Count;
            int totalPaginas = Math.Max(1, (int)Math.Ceiling(totalItems / (double)ItemsPorPagina));
            pagina = Math.Max(1, Math.Min(pagina, totalPaginas));
            var pagina_items = items.Skip((pagina - 1) * ItemsPorPagina).Take(ItemsPorPagina).ToList();

            var model = new Models.ActividadesIndexVm
            {
                FechaDesde = desde.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                FechaHasta = hasta.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                Pagina = pagina,
                TotalPaginas = totalPaginas,
                TotalItems = totalItems,
                Items = pagina_items
            };

            return View(model);
        }

        private static decimal ToDecimal(object value)
        {
            if (value == null || value == DBNull.Value) return 0m;
            return Convert.ToDecimal(value, CultureInfo.InvariantCulture);
        }
    }
}
