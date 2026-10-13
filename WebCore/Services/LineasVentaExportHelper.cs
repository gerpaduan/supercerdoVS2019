// Textos, formatos y totales compartidos por los exports de Ventas > Lineas: el PDF
// (GenerarDocsCore.GenerarPdfLineasVenta) y el Excel (GenerarExcelCore.GenerarExcelLineasVenta).
// Viven aca, y no repetidos en cada generador, para que los dos archivos digan exactamente lo mismo.
// Depende solo de los ViewModels de WebCore/Models/VentasVm.cs. Detalle del modulo: docs/03-modulos/ventas-y-pos.md.
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using WebCore.Models;

namespace WebCore.Services
{
    internal static class LineasVentaExportHelper
    {
        // Cultura fija es-AR (no la del servidor): el archivo muestra "17.800,00" igual en cualquier host.
        // Mismo criterio que GenerarDocsCore.GenerarPdfCtaCtePersona.
        public static readonly CultureInfo CulturaAr = new CultureInfo("es-AR");

        // Separador de los textos de una sola linea ("Cliente: X  ·  Vendedor: Y").
        private const string Separador = "  ·  ";

        public const string Titulo = "Líneas de venta";
        public const string MensajeSinVentas = "No se encontraron ventas con líneas para los filtros seleccionados.";

        // Titulos de la tabla de lineas: los mismos que muestra la pantalla (en mayusculas por CSS).
        public static readonly string[] EncabezadosColumnas = { "CODIGO PRODUCTO", "PRODUCTO", "CANTIDAD / KG", "PRECIO", "TOTAL LINEA" };

        // "Período: 05/10/2026 00:00 a 06/10/2026 23:59  ·  Sucursal: Todas"
        public static string Periodo(VentaLineasIndexVm model, string sucursalNombre)
        {
            return "Período: " + model.FechaDesde.ToString("dd/MM/yyyy HH:mm", CulturaAr)
                + " a " + model.FechaHasta.ToString("dd/MM/yyyy HH:mm", CulturaAr)
                + Separador + "Sucursal: " + sucursalNombre;
        }

        // "Filtros: Producto: carne  ·  Forma de pago: Efectivo, Qr". Devuelve "" si no hay ningun filtro activo
        // (el generador entonces no escribe la linea).
        public static string Filtros(VentaLineasIndexVm model)
        {
            var partes = new List<string>();

            if (!string.IsNullOrWhiteSpace(model.Producto))
                partes.Add("Producto: " + model.Producto.Trim());
            // Con seleccion exacta (selector multi-seleccion) se listan los nombres elegidos; sin ella, el texto escrito.
            string clientes = FiltroPickerService.UnirNombres(model.ClientesSeleccionados);
            if (clientes.Length > 0)
                partes.Add("Cliente: " + clientes);
            else if (!string.IsNullOrWhiteSpace(model.Cliente))
                partes.Add("Cliente: " + model.Cliente.Trim());

            string vendedores = FiltroPickerService.UnirNombres(model.VendedoresSeleccionados);
            if (vendedores.Length > 0)
                partes.Add("Vendedor: " + vendedores);
            else if (!string.IsNullOrWhiteSpace(model.Vendedor))
                partes.Add("Vendedor: " + model.Vendedor.Trim());
            if (model.FormasPagoSeleccionadas.Count > 0)
                partes.Add("Forma de pago: " + string.Join(", ", model.FormasPagoSeleccionadas));

            return partes.Count == 0 ? "" : "Filtros: " + string.Join(Separador, partes);
        }

        // Linea gris bajo el encabezado de cada venta. Son los mismos "Campos" que la pantalla oculta tras
        // "Ver detalles"; se toman por etiqueta (ver VentasController.ConstruirModeloLineas) y en este orden.
        public static string DatosVenta(VentaLineasGrupoVm venta)
        {
            string[] etiquetas = { "Cliente", "Vendedor", "Forma de pago", "Sucursal", "Nro/comprobante" };

            var partes = new List<string>();
            foreach (string etiqueta in etiquetas)
            {
                var campo = venta.Campos.FirstOrDefault(x => x.Etiqueta == etiqueta);
                partes.Add(etiqueta + ": " + (campo != null && !string.IsNullOrWhiteSpace(campo.Valor) ? campo.Valor : "-"));
            }

            return string.Join(Separador, partes);
        }

        // Mismos totales que las tarjetas de la pantalla (recalcTotals en lineas-agrupadas.js): suma de las
        // LINEAS listadas, no de los totales de cada venta (el total del encabezado de una venta es el de
        // toda la venta, aunque un filtro de producto deje solo algunas de sus lineas).
        public static (decimal TotalKg, decimal TotalPesos, int CantidadLineas, int CantidadRegistros) Totales(VentaLineasIndexVm model)
        {
            var lineas = model.Ventas.SelectMany(venta => venta.Lineas).ToList();
            return (lineas.Sum(linea => linea.CantidadKg), lineas.Sum(linea => linea.Total), lineas.Count, model.Ventas.Count);
        }

        // "Cantidad de líneas: 12  ·  Cantidad de registros: 4"
        public static string ResumenCantidades(int cantidadLineas, int cantidadRegistros)
        {
            return "Cantidad de líneas: " + cantidadLineas.ToString("N0", CulturaAr)
                + Separador + "Cantidad de registros: " + cantidadRegistros.ToString("N0", CulturaAr);
        }

        // Formatos de texto (PDF). El Excel no los usa: escribe numeros reales con formato de celda.
        public static string Kg(decimal valor) => valor.ToString("N3", CulturaAr);

        public static string Importe(decimal valor) => valor.ToString("N2", CulturaAr);

        public static string Moneda(decimal valor) => "$ " + Importe(valor);
    }
}
