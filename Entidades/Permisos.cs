using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entidades
{
    public static class Permisos
    {
        public static class Caja
        {
            public const string VerCerrarCaja = "formCerrarCaja";
            public const string CerrarCaja = "formCerrarCaja";
            public const string CierresDeCaja = "formCierresDeCaja";
        }

        public static class Compra
        {
            public const string VerCompras = "formCompras";
            public const string NuevaCompra = "formNuevaCompra";
            public const string ModificarCompra = "formModificarCompra";
        }

        public static class EgresoCaja
        {
            public const string VerEgresosCaja = "formEgresosCaja";
            public const string VerTiposEgresos = "formTiposEgresos";
            public const string AddOrEditEgresoCaja = "formAddOrEditEgresoCaja";
            public const string AddOrEditTipoEgreso = "formAddOrEditTipoEgreso";
        }

        public static class Elaborado
        {
            public const string VerEmbutidos = "formEmbutidos";
            public const string VerFormulas = "formFormulas";
            public const string IngresoEmbutido = "formIngresoEmbutido";
            public const string IngresoFormula = "formIngresoFormula";
            public const string IngresoEmbutidoRapido = "formIngresoEmbutidoRapido";
        }

        public static class Etiqueta
        {
            public const string Etiquetas = "formEtiquetas";
        }

        public static class Finanza
        {
            public const string VerCheques = "formCheques";
            public const string VerPagos = "formPagos";
            public const string AddOrEditPago = "formAddOrEditPago";
            public const string VerCtasCtes = "formCtasCtes";
            public const string VerCtaCtePersona = "formCtaCtePersona";
        }

        public static class Movimiento
        {
            public const string VerMovimientos = "formMovimientos";
            public const string NuevoMovimiento = "formNuevoMovimiento";
        }

        public static class Producto
        {
            public const string ModificarPrecios = "formModificarPrecios";
            public const string NuevoCorte = "formNuevoCorte";
            public const string AddOrEditTipoProducto = "formAddOrEditTipoProducto";
            public const string AddOrEditCostoCobro = "formAddOrEditCostoCobro";
            public const string VerCortes = "formCortes";
            public const string VerTiposProducto = "formTiposProducto";
            public const string VerAddOrEditCostoCobro = "formAddOrEditCostoCobro";
        }

        public static class Reporte
        {
            public const string BalanceEconomico = "Balance Económico";
            public const string VentasPorProducto = "Ventas por Producto";
            public const string ExportarListaPrecioExcel = "ExportarListaPrecioExcel";
            public const string ImprimirListaPreciosTicket = "ImprimirListaPreciosTicket";
            public const string StockActual = "Stock Actual";
            public const string CierreStock = "Cierre Stock";
            public const string StockRetroactivo = "Stock Retroactivo";
            public const string ProyeccionVentasVsStock = "Proyeccion Ventas vs Stock";
        }

        public static class Stock
        {
            public const string VerStock = "formStock";
            public const string AddOrEditStock = "formAddOrEditStock";
        }

        public static class Usuario
        {
            public const string VerUsuarios = "FormUsuarios";
            public const string NuevoUsuario = "FormNuevoUsuario";
        }

        public static class Venta
        {
            public const string Bonificar = "formBonificar";
            public const string VerVentas = "formVentas";
            public const string VerBonificar = "formBonificar";
            public const string VerVentasVendedor = "formVentasVendedor";
            public const string VerGetAllLineaVenta = "formGetAllLineaVenta";
            public const string NuevaVenta = "formNuevaVenta";
            public const string UltimaVenta = "formUltimaVenta";
            public const string GetAllLineaVenta = "formGetAllLineaVenta";
        }
    }

}
