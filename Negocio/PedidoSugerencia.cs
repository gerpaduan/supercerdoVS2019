// "Ingresar pedido" de Existencia por sucursales: reglas PURAS (sin base de datos) de la cantidad sugerida a pedir, del
// consumo del periodo y del importe estimado. Las usa Negocio.ReposicionStock (consumo) y las espeja el navegador
// (WebCore/wwwroot/Scripts/app/existencia-pedido.js: si cambia una regla, cambiarla en los dos lados y en los tests).
// Tests: Negocio.Tests/PedidoSugerenciaTests.cs. Porque y ejemplos: docs/DECISIONS.md (2026-10-19) y
// docs/11-manual-de-uso/pantallas/Stock.ExistenciaPorSucursales.md ("Ingresar pedido: de donde sale la sugerencia").
//
// Sintaxis a proposito conservadora (C# 7.3): Negocio tambien compila para net472.
using System;
using System.Collections.Generic;
using System.Linq;
using Entidades;

namespace Negocio
{
    public static class PedidoSugerencia
    {
        // "Sugerir segun": de donde sale la cantidad que se ofrece.
        public const string SegunConsumo = "consumo";   // consumo del periodo menos el stock actual (por defecto)
        public const string SegunPunto = "punto";       // completar hasta el punto de stock
        public const string SegunMayor = "mayor";       // el mayor de los dos

        // Mismos decimales que el stock (N3): el punto de stock es entero pero el consumo y el stock no.
        public const int Decimales = 3;

        public sealed class Resultado
        {
            // Datos de entrada ya saneados (negativos tomados como 0, ver Calcular).
            public double Consumo { get; set; }
            public double Stock { get; set; }
            public double Punto { get; set; }

            // Lo que falta para cubrir el consumo del periodo, ya descontado el stock (0 = el stock alcanza).
            public double FaltaPorConsumo { get; set; }
            // Lo que falta para llegar al punto de stock (0 = no esta bajo el punto o no hay punto).
            public double FaltaHastaPunto { get; set; }
            // La sugerencia segun el modo pedido; 0 = no se sugiere nada.
            public double Sugerido { get; set; }
            // Stock por debajo del punto de stock (con punto cargado).
            public bool BajoPunto { get; set; }
            // El stock cubre el consumo del periodo y el punto, y hay algun dato (consumo o punto) que lo respalde.
            public bool StockSuficiente { get; set; }
        }

        public static string NormalizarSegun(string segun)
        {
            if (string.Equals((segun ?? "").Trim(), SegunPunto, StringComparison.OrdinalIgnoreCase)) return SegunPunto;
            if (string.Equals((segun ?? "").Trim(), SegunMayor, StringComparison.OrdinalIgnoreCase)) return SegunMayor;
            return SegunConsumo;
        }

        // Cantidad sugerida de UNA celda (producto x sucursal).
        //  - stock: el que se ve en la pantalla; si es NEGATIVO se toma como 0 (un stock negativo es un error de carga, no
        //    hay "menos de nada" que reponer: contarlo inflaria el pedido).
        //  - punto: punto de stock de ESA sucursal (0 = sin punto).
        //  - consumo: ventas + uso en elaborados del periodo elegido (en la misma unidad que el stock).
        // Redondeo a Decimales, hacia arriba en el empate (la cantidad nunca queda por debajo de lo que falta).
        public static Resultado Calcular(double stock, double punto, double consumo, string segun)
        {
            double s = Saneado(stock);
            double p = Saneado(punto);
            double c = Saneado(consumo);

            var r = new Resultado { Stock = s, Punto = p, Consumo = c };
            r.FaltaPorConsumo = c > s ? Redondear(c - s) : 0;
            r.BajoPunto = p > 0 && s < p;
            r.FaltaHastaPunto = r.BajoPunto ? Redondear(p - s) : 0;

            switch (NormalizarSegun(segun))
            {
                case SegunPunto: r.Sugerido = r.FaltaHastaPunto; break;
                case SegunMayor: r.Sugerido = Math.Max(r.FaltaPorConsumo, r.FaltaHastaPunto); break;
                default: r.Sugerido = r.FaltaPorConsumo; break;
            }

            r.StockSuficiente = s >= c && s >= p && (c > 0 || p > 0);
            return r;
        }

        // Importe estimado de una linea: Total pedido x precio de la ultima compra, a 2 decimales. Misma regla que "Que pedir"
        // (Negocio.ReposicionStock): solo si hay precio > 0; sin precio NO es $0 sino "sin precio" (null), y no suma.
        public static decimal? Importe(double cantidad, decimal? precio)
        {
            if (!precio.HasValue || precio.Value <= 0m) return null;
            if (double.IsNaN(cantidad) || double.IsInfinity(cantidad) || cantidad <= 0) return null;
            return Math.Round((decimal)cantidad * precio.Value, 2);
        }

        // Equivalente en unidades (solo productos pesables con promedio cargado), la misma regla que
        // Entidades.StockUnidadesHelper.FormatearEquivalenteUnidades pero numerica. null = no aplica.
        public static double? EquivalenteUnidades(double cantidad, double promedio, bool pesable)
        {
            if (!pesable || promedio <= 0 || cantidad <= 0) return null;
            return cantidad / promedio;
        }

        // Consumo (ventas + uso en elaborados) por producto y sucursal desde la fecha pedida: recorta la actividad a las
        // sucursales indicadas (vacio = todas) y descarta lo que no se consumio (la consulta puede traer pares con 0 kg).
        public static List<ActividadStockVm> ConsumoDeSucursales(IEnumerable<ActividadStockVm> actividad, ICollection<int> idsSucursales)
        {
            bool todas = idsSucursales == null || idsSucursales.Count == 0;
            var resultado = new List<ActividadStockVm>();
            foreach (var fila in actividad ?? Enumerable.Empty<ActividadStockVm>())
            {
                if (fila == null) continue;
                if (!todas && !idsSucursales.Contains(fila.IdSucursal)) continue;
                if ((double)fila.VentasVentana + (double)fila.ElaboracionVentana <= 0) continue;
                resultado.Add(fila);
            }
            return resultado;
        }

        // Precio de la ultima compra de cada producto (proveedor habitual = el de la compra mas reciente): se descartan los que no
        // tienen precio o es <= 0 (sin precio NO es $0: el producto queda "sin precio" y no suma al total estimado), y se deja un
        // solo precio por producto (el primero que llega; la consulta ya devuelve uno por producto).
        public static List<ProveedorHabitualVm> PreciosVigentes(IEnumerable<ProveedorHabitualVm> habituales)
        {
            var vistos = new HashSet<int>();
            var resultado = new List<ProveedorHabitualVm>();
            foreach (var fila in habituales ?? Enumerable.Empty<ProveedorHabitualVm>())
            {
                if (fila == null || !fila.UltimoPrecio.HasValue || fila.UltimoPrecio.Value <= 0m) continue;
                if (!vistos.Add(fila.IdCorte)) continue;
                resultado.Add(fila);
            }
            return resultado;
        }

        // Separa los precios de la ultima compra por antiguedad: un precio de una compra de hace MAS de diasMaximo dias (respecto de
        // `ahora`) no se usa para estimar importes. Por que: con la inflacion, un precio de hace años subestima el importe en ordenes
        // de magnitud (medido 2026-10-19: Carre a $76/kg de 2016 contra $9.900 de venta) y, sumado al total, lo falsea; es mejor que el
        // producto quede "sin precio" (no suma, y el total se rotula "parcial"). Sin fecha de compra no se puede afirmar que sea viejo:
        // se conserva (misma regla que PedidoTextos.PrecioViejo). Primero aplica PreciosVigentes (sin precio / <= 0 / repetidos).
        public static UltimosPreciosCompraVm SepararPorAntiguedad(IEnumerable<ProveedorHabitualVm> ultimasCompras, DateTime ahora, int diasMaximo)
        {
            var resultado = new UltimosPreciosCompraVm();
            foreach (var fila in PreciosVigentes(ultimasCompras))
            {
                if (PedidoTextos.PrecioViejo(fila.FechaUltimaCompra, ahora, diasMaximo)) resultado.DescartadosPorAntiguedad.Add(fila);
                else resultado.Vigentes.Add(fila);
            }
            return resultado;
        }

        private static double Saneado(double valor)
        {
            if (double.IsNaN(valor) || double.IsInfinity(valor) || valor < 0) return 0;
            return valor;
        }

        private static double Redondear(double valor)
        {
            return Math.Round(valor, Decimales, MidpointRounding.AwayFromZero);
        }
    }
}
