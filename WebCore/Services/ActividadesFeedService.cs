using System.Globalization;

namespace WebCore.Services
{
    // Extraido de ActividadesController.cs (2026-09-10, item 4 de la segunda ronda de pedidos --
    // ver docs/DECISIONS.md "Batch 8: Actividades en el dashboard"). Refactor de extraccion pura
    // (sin cambio de comportamiento, verificado con Playwright que /Actividades sigue mostrando
    // exactamente los mismos items antes/despues): junta las 6 fuentes heterogeneas (cambio de
    // precio, venta anulada, venta con bonificacion manual, egreso de caja, movimiento, compra,
    // formula) en una sola lista ordenada por fecha. Reusado por ActividadesController (pantalla
    // completa) y HomeController (bloque "Actividades" del dashboard, ultimas 10).
    public class ActividadesFeedService
    {
        private readonly Utilidades.IEmpresaContext _empresa;
        private readonly Utilidades.IParametrosContext? _parametros;

        public ActividadesFeedService(Utilidades.IEmpresaContext empresa, Utilidades.IParametrosContext? parametros = null)
        {
            _empresa = empresa;
            _parametros = parametros;
        }

        public List<Models.ActividadItemVm> ObtenerActividades(DateTime desdeConHora, DateTime hastaConHora)
        {
            var repo = WebCore.Infrastructure.NegocioFactory.CrearActividadRepository(_empresa);
            var oCierreN = WebCore.Infrastructure.NegocioFactory.CrearCierreCaja(_empresa, _parametros);

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
                string vendedor = row["vendedor"] == DBNull.Value ? "" : Convert.ToString(row["vendedor"]) ?? "";
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
                string vendedor = row["vendedor"] == DBNull.Value ? "" : Convert.ToString(row["vendedor"]) ?? "";
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
                    string desc = row["Descripcion"] == DBNull.Value ? "" : Convert.ToString(row["Descripcion"]) ?? "";
                    string tipo = row["TipoEgresoCaja"] == DBNull.Value ? "" : Convert.ToString(row["TipoEgresoCaja"]) ?? "";
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
                string origen = row["sucursal_origen"] == DBNull.Value ? "?" : Convert.ToString(row["sucursal_origen"]) ?? "?";
                string destino = row["sucursal_destino"] == DBNull.Value ? "?" : Convert.ToString(row["sucursal_destino"]) ?? "?";
                string usuarioMov = row["usuario"] == DBNull.Value ? "" : Convert.ToString(row["usuario"]) ?? "";
                items.Add(new Models.ActividadItemVm
                {
                    Fecha = (DateTime)row["fecha"],
                    Tipo = "Movimiento",
                    Descripcion = $"Movimiento de stock de {origen} a {destino}" + (string.IsNullOrWhiteSpace(usuarioMov) ? "" : $" (por {usuarioMov})")
                });
            }

            foreach (System.Data.DataRow row in repo.ObtenerCompras(desdeConHora, hastaConHora).Rows)
            {
                string tipo = row["tipocompra"] == DBNull.Value ? "Compra" : Convert.ToString(row["tipocompra"]) ?? "Compra";
                string usuarioCompra = row["usuario"] == DBNull.Value ? "" : Convert.ToString(row["usuario"]) ?? "";
                items.Add(new Models.ActividadItemVm
                {
                    Fecha = (DateTime)row["fecha"],
                    Tipo = "Compra/Stock",
                    Descripcion = $"{tipo} #{row["idcompra"]}" + (string.IsNullOrWhiteSpace(usuarioCompra) ? "" : $" (por {usuarioCompra})")
                });
            }

            foreach (System.Data.DataRow row in repo.ObtenerFormulas(desdeConHora, hastaConHora).Rows)
            {
                string producto = row["producto"] == DBNull.Value ? $"#{row["idformula"]}" : Convert.ToString(row["producto"]) ?? "";
                string usuarioFormula = row["usuario"] == DBNull.Value ? "" : Convert.ToString(row["usuario"]) ?? "";
                items.Add(new Models.ActividadItemVm
                {
                    Fecha = (DateTime)row["fecha"],
                    Tipo = "Elaborado",
                    Descripcion = $"Se dio de alta o editó la fórmula de {producto}" + (string.IsNullOrWhiteSpace(usuarioFormula) ? "" : $" (por {usuarioFormula})")
                });
            }

            return items.OrderByDescending(i => i.Fecha).ToList();
        }

        private static decimal ToDecimal(object value)
        {
            if (value == null || value == DBNull.Value) return 0m;
            return Convert.ToDecimal(value, CultureInfo.InvariantCulture);
        }
    }
}
