// Permisos de los reportes (Reportes/Index): cada tipo de reporte tiene su propia fila de `formularios` (idform 25-30,
// claves "Stock Actual", "Cierre Stock", etc.). Hasta 2026-11-10 esas filas no las miraba nadie y los seis reportes se
// habilitaban con el permiso "Stocks" (formStock); desde entonces cada reporte exige su fila. Los dias "ver" de la fila
// limitan hasta cuantos dias atras puede poner la fecha "desde" (igual que antes con Stocks).
// Detalle y porques: docs/DECISIONS.md (2026-11-10, "Permisos que no controlaban nada").
using Entidades;

namespace WebCore.Helpers
{
    public static class PermisosReportes
    {
        // Tipos de reporte, en el orden del selector. Los valores son los que usa ReportesController (y el parametro
        // `tipoReporte` de la URL); la clave del permiso sale de Entidades.Permisos.Reporte.
        public const string StockActual = "Stock Actual";
        public const string CierreStock = "Cierre Stock";
        public const string StockRetroactivo = "Stock Retroactivo";
        public const string Proyeccion = "Proyeccion Ventas vs Stock";
        public const string VentasPorProducto = "Ventas por Producto";
        public const string BalanceEconomico = "Balance Economico";

        public static readonly IReadOnlyList<string> TiposEnOrden = new[]
        {
            StockActual, CierreStock, StockRetroactivo, Proyeccion, VentasPorProducto, BalanceEconomico
        };

        // El selector del reporte de balance va sin tilde ("Balance Economico") pero la clave del permiso en la base lleva
        // tilde ("Balance Económico"): por eso es un mapa y no una comparacion directa.
        private static readonly Dictionary<string, string> ClavePorTipo = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            [StockActual] = Permisos.Reporte.StockActual,
            [CierreStock] = Permisos.Reporte.CierreStock,
            [StockRetroactivo] = Permisos.Reporte.StockRetroactivo,
            [Proyeccion] = Permisos.Reporte.ProyeccionVentasVsStock,
            [VentasPorProducto] = Permisos.Reporte.VentasPorProducto,
            [BalanceEconomico] = Permisos.Reporte.BalanceEconomico
        };

        // Clave del permiso del reporte, o null si el tipo no existe.
        public static string? ClaveDe(string? tipoReporte)
        {
            return ClavePorTipo.TryGetValue((tipoReporte ?? "").Trim(), out var clave) ? clave : null;
        }

        // true si el usuario puede ver ese tipo de reporte con la fecha "desde" pedida (Admin pasa siempre). Un tipo
        // desconocido nunca se permite.
        public static bool Permite(Negocio.Usuario negocioUsuario, Entidades.Usuario? usuario, string? tipoReporte, DateTime desde)
        {
            var clave = ClaveDe(tipoReporte);
            return clave != null && negocioUsuario.tienePermiso(usuario, clave, desde, -1);
        }

        // Tipos de reporte que el usuario puede ver con esa fecha "desde", en el orden del selector.
        public static List<string> TiposPermitidos(Negocio.Usuario negocioUsuario, Entidades.Usuario? usuario, DateTime desde)
        {
            return TiposEnOrden.Where(t => Permite(negocioUsuario, usuario, t, desde)).ToList();
        }
    }
}
