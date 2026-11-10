using System;
using System.Collections.Generic;
using System.Linq;

namespace Negocio
{
    // Reglas puras de los comprobantes del proveedor de una compra (factura y notas de credito/debito), sin base de datos, para
    // poder probarlas. Compras: titular propio y comprobantes del proveedor (2026-11-09, ver docs/DECISIONS.md).
    // Codigo C# 7.3: Negocio tambien compila para net472.
    public static class ValidacionComprobanteCompra
    {
        // Diferencia maxima aceptada entre neto + IVA + otros y el total (redondeos de centavos).
        public const decimal ToleranciaImporte = 0.01m;
        public const int MaxPuntoVenta = 99999;
        public const long MaxNumero = 99999999;

        // Solo las compras reales llevan comprobantes: las operaciones de stock (cierre, ingreso, ajuste...) no.
        public static bool TipoCompraAdmiteComprobantes(string tipoCompra)
        {
            return string.Equals(tipoCompra, Entidades.Compra.tipoCompraToString(Entidades.Compra.tipoCompraEnum.Cortes), StringComparison.Ordinal)
                || string.Equals(tipoCompra, Entidades.Compra.tipoCompraToString(Entidades.Compra.tipoCompraEnum.MediaRes), StringComparison.Ordinal);
        }

        // Valida el comprobante a guardar. "otrosActivos" = comprobantes activos de la misma compra SIN el que se edita.
        // Devuelve null si esta bien o el mensaje para el usuario.
        public static string Validar(Entidades.ComprobanteCompra comprobante, IList<Entidades.ComprobanteCompra> otrosActivos, string tipoCompra)
        {
            if (comprobante == null) return "Faltan los datos del comprobante.";
            if (!TipoCompraAdmiteComprobantes(tipoCompra))
                return "Solo las compras de cortes o media res llevan factura: esta es una operación de stock.";
            if (!Entidades.TiposComprobanteCompra.EsValido(comprobante.TipoComprobante))
                return "Elegí el tipo de comprobante.";
            if (comprobante.PtoVta < 1 || comprobante.PtoVta > MaxPuntoVenta)
                return "El punto de venta tiene que estar entre 1 y " + MaxPuntoVenta + ".";
            if (comprobante.Numero < 1 || comprobante.Numero > MaxNumero)
                return "El número de comprobante tiene que estar entre 1 y " + MaxNumero + ".";
            if (comprobante.Fecha == default(DateTime))
                return "Cargá la fecha del comprobante.";
            if (comprobante.NetoGravado < 0m || comprobante.Iva < 0m || comprobante.OtrosImpuestos < 0m)
                return "Neto, IVA y otros impuestos no pueden ser negativos (las notas de crédito se cargan en positivo).";
            if (comprobante.Total <= 0m)
                return "El total del comprobante tiene que ser mayor a cero.";

            decimal suma = comprobante.NetoGravado + comprobante.Iva + comprobante.OtrosImpuestos;
            if (Math.Abs(suma - comprobante.Total) > ToleranciaImporte)
                return "El total no coincide con neto + IVA + otros impuestos (" + suma.ToString("N2") + ").";

            var activos = (otrosActivos ?? new List<Entidades.ComprobanteCompra>()).Where(c => c != null && !c.Eliminado).ToList();

            if (Entidades.TiposComprobanteCompra.EsFactura(comprobante.TipoComprobante)
                && activos.Any(c => Entidades.TiposComprobanteCompra.EsFactura(c.TipoComprobante)))
                return "La compra ya tiene una factura. Si el proveedor mandó otra, cargala como otra compra.";

            bool esNota = Entidades.TiposComprobanteCompra.EsNotaCredito(comprobante.TipoComprobante)
                || Entidades.TiposComprobanteCompra.EsNotaDebito(comprobante.TipoComprobante);
            if (esNota && !activos.Any(c => Entidades.TiposComprobanteCompra.EsFactura(c.TipoComprobante)))
                return "Para cargar una nota de crédito o de débito la compra tiene que tener primero su factura.";

            return null;
        }

        // No se puede borrar la factura mientras queden notas que cuelgan de ella.
        public static string ValidarBaja(Entidades.ComprobanteCompra aBorrar, IList<Entidades.ComprobanteCompra> activos)
        {
            if (aBorrar == null) return "El comprobante no existe.";
            if (!Entidades.TiposComprobanteCompra.EsFactura(aBorrar.TipoComprobante)) return null;

            bool quedanNotas = (activos ?? new List<Entidades.ComprobanteCompra>()).Any(c => c != null && !c.Eliminado
                && c.IdComprobante != aBorrar.IdComprobante
                && (Entidades.TiposComprobanteCompra.EsNotaCredito(c.TipoComprobante) || Entidades.TiposComprobanteCompra.EsNotaDebito(c.TipoComprobante)));
            return quedanNotas ? "Primero eliminá las notas de crédito y de débito de esta compra." : null;
        }

        // Factura + notas de debito - notas de credito de los comprobantes activos.
        public static decimal FacturadoNeto(IEnumerable<Entidades.ComprobanteCompra> comprobantes)
        {
            if (comprobantes == null) return 0m;
            return comprobantes.Where(c => c != null && !c.Eliminado).Sum(c => c.TotalConSigno);
        }

        // Lo que la compra vale de mas que lo facturado (puede ser negativo si se facturo de mas).
        public static decimal SinFactura(decimal totalCompra, decimal facturadoNeto)
        {
            return totalCompra - facturadoNeto;
        }

        // Estado de la compra despues de guardar o borrar un comprobante: con factura activa es FACTURADA; sin ella vuelve a
        // PENDIENTE, salvo que estuviera marcada "no lleva factura" a mano.
        public static string EstadoResultante(IEnumerable<Entidades.ComprobanteCompra> activos, string estadoActual)
        {
            bool hayFactura = activos != null && activos.Any(c => c != null && !c.Eliminado
                && Entidades.TiposComprobanteCompra.EsFactura(c.TipoComprobante));
            if (hayFactura) return Entidades.EstadoFacturaCompra.Facturada;
            return estadoActual == Entidades.EstadoFacturaCompra.NoLleva
                ? Entidades.EstadoFacturaCompra.NoLleva
                : Entidades.EstadoFacturaCompra.Pendiente;
        }

        // Neto e IVA a partir del total y la alicuota (10,5 / 21 / ...). El neto se redondea a centavos y el IVA es el resto,
        // asi neto + IVA da siempre el total exacto.
        public static void CalcularDesdeTotal(decimal total, decimal alicuotaPorcentaje, out decimal neto, out decimal iva)
        {
            if (alicuotaPorcentaje < 0m) throw new ArgumentOutOfRangeException(nameof(alicuotaPorcentaje));
            neto = Math.Round(total / (1m + alicuotaPorcentaje / 100m), 2, MidpointRounding.AwayFromZero);
            iva = total - neto;
        }
    }
}
