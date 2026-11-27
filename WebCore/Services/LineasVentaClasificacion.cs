// Clasifica las lineas de una venta para los filtros "Descuento / recargo" y "Anulaciones" de Ventas > Lineas
// (2026-11-27, ver docs/DECISIONS.md). La usan la pantalla (marcas y atributos de cada fila) y los exports (filtrado).
// El filtrado en pantalla lo hace lineas-agrupadas.js con las mismas reglas. Sin dependencias de base: tiene tests.
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Entidades;

namespace WebCore.Services
{
    // Resultado por linea.
    public class LineaVentaMarcas
    {
        public string TipoDescuento { get; set; } = "";
        public string DescuentoTexto { get; set; } = "";
        // "original" = item que despues se anulo; "contra" = el renglon negativo que lo anula; "" = linea normal.
        public string TipoAnulacion { get; set; } = "";
        // Entra en "Solo lo anulado": una fila por item anulado.
        public bool SoloAnulado { get; set; }
    }

    // Resultado de una venta completa.
    public class VentaMarcas
    {
        public bool ConAnulacion { get; set; }
        // Todo lo cargado quedo anulado (la venta cancelada desde el POS queda asi).
        public bool Cancelada { get; set; }
        public Dictionary<LineaVenta, LineaVentaMarcas> Lineas { get; } = new Dictionary<LineaVenta, LineaVentaMarcas>();
    }

    public static class LineasVentaClasificacion
    {
        public const string Descuento = "desc";
        public const string Recargo = "rec";
        public const string PrecioDistinto = "precio";
        public const string Original = "original";
        public const string Contra = "contra";
        public const string VentasConAnulaciones = "ventas";
        public const string SoloAnulado = "solo";

        // Diferencias menores son ruido de los float con los que se guardan precio y porcentaje.
        private const float BonificacionMinima = 0.005f;
        private const float AjusteMinimo = 0.01f;

        private static readonly CultureInfo CulturaAr = new CultureInfo("es-AR");

        // El porcentaje manda: bonificacion > 0 es descuento y < 0 recargo. Sin porcentaje, ajustePrecio (precio cobrado
        // menos precio de lista al grabar) distinto de cero marca "precio distinto al de lista".
        public static string TipoDescuento(float bonificacion, float ajustePrecio)
        {
            if (bonificacion > BonificacionMinima) return Descuento;
            if (bonificacion < -BonificacionMinima) return Recargo;
            return Math.Abs(ajustePrecio) >= AjusteMinimo ? PrecioDistinto : "";
        }

        public static string TextoDescuento(float bonificacion, float ajustePrecio)
        {
            switch (TipoDescuento(bonificacion, ajustePrecio))
            {
                case Descuento: return "Desc. " + bonificacion.ToString("0.##", CulturaAr) + " %";
                case Recargo: return "Rec. " + Math.Abs(bonificacion).ToString("0.##", CulturaAr) + " %";
                case PrecioDistinto: return "Lista " + (ajustePrecio < 0 ? "-" : "+") + " $ " + Math.Abs(ajustePrecio).ToString("N2", CulturaAr);
                default: return "";
            }
        }

        // Valores de filtro recibidos por querystring (input externo): solo los conocidos; lo demas es "sin filtro".
        public static string NormalizarFiltroDescuento(string? valor)
        {
            string limpio = (valor ?? "").Trim().ToLowerInvariant();
            return limpio == Descuento || limpio == Recargo || limpio == PrecioDistinto ? limpio : "";
        }

        public static string NormalizarFiltroAnulaciones(string? valor)
        {
            string limpio = (valor ?? "").Trim().ToLowerInvariant();
            return limpio == VentasConAnulaciones || limpio == SoloAnulado ? limpio : "";
        }

        // Recibe TODAS las lineas de la venta (antes de cualquier filtro): la anulacion se reconoce por la pareja.
        public static VentaMarcas Clasificar(IReadOnlyCollection<LineaVenta> lineas)
        {
            var resultado = new VentaMarcas();
            var ids = new HashSet<int>(lineas.Select(l => l.IdLineaVenta));
            // Ids de las lineas anuladas: cada renglon que anula (estado Anulado) apunta a la original.
            var anuladas = new HashSet<int>(lineas
                .Where(l => LineaVenta.esAnulado(l.Estado) && l.IndexAnulado > 0)
                .Select(l => l.IndexAnulado));

            foreach (var linea in lineas)
            {
                bool esContra = LineaVenta.esAnulado(linea.Estado);
                bool esOriginal = !esContra && anuladas.Contains(linea.IdLineaVenta);
                resultado.Lineas[linea] = new LineaVentaMarcas
                {
                    TipoDescuento = TipoDescuento(linea.Bonificacion, linea.AjustePrecio),
                    DescuentoTexto = TextoDescuento(linea.Bonificacion, linea.AjustePrecio),
                    TipoAnulacion = esContra ? Contra : (esOriginal ? Original : ""),
                    // Un renglon de anulacion sin su original (datos viejos) se muestra el, para no perderlo.
                    SoloAnulado = esOriginal || (esContra && !ids.Contains(linea.IndexAnulado))
                };
            }

            resultado.ConAnulacion = resultado.Lineas.Values.Any(m => m.TipoAnulacion == Contra);
            resultado.Cancelada = resultado.Lineas.Values.Any(m => m.TipoAnulacion == Original)
                && resultado.Lineas.Values.All(m => m.TipoAnulacion != "");
            return resultado;
        }

        // Mismas reglas que lineMatchesFilters de lineas-agrupadas.js. Los filtros llegan ya normalizados.
        public static bool Coincide(LineaVentaMarcas linea, bool ventaConAnulacion, string filtroDescuento, string filtroAnulaciones)
        {
            if (filtroDescuento != "" && linea.TipoDescuento != filtroDescuento) return false;
            if (filtroAnulaciones == SoloAnulado) return linea.SoloAnulado;
            return filtroAnulaciones != VentasConAnulaciones || ventaConAnulacion;
        }
    }
}
