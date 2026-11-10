using System;
using System.Collections.Generic;
using System.Linq;

namespace Entidades
{
    // Comprobante que el proveedor emite por una compra: la factura y, si hay, sus notas de credito / debito.
    // Compras: titular propio y comprobantes del proveedor (2026-11-09, ver docs/DECISIONS.md). Solo Postgres.
    // Es el dato FISCAL de la compra: no genera movimientos de cuenta corriente (la deuda es el importe de la compra).
    // Los importes se guardan siempre positivos; el signo (una nota de credito resta) sale del tipo de comprobante.
    public class ComprobanteCompra
    {
        public int IdComprobante { get; set; }
        public int IdCompra { get; set; }
        // Copia del proveedor de la compra (sirve para el indice anti-duplicado proveedor + tipo + punto de venta + numero).
        public int IdProveedor { get; set; }
        public string TipoComprobante { get; set; }
        public int PtoVta { get; set; }
        public long Numero { get; set; }
        public DateTime Fecha { get; set; }
        public decimal NetoGravado { get; set; }
        public decimal Iva { get; set; }
        public decimal OtrosImpuestos { get; set; }
        public decimal Total { get; set; }
        // Alicuota usada para calcular neto e IVA desde el total (10,5 / 21 / ...); null = cargado a mano.
        public decimal? AlicuotaIva { get; set; }
        public DateTime? Creado { get; set; }
        public int? CreadoPor { get; set; }
        public DateTime? Actualizado { get; set; }
        public int? ActualizadoPor { get; set; }
        public bool Eliminado { get; set; }

        // Importe con signo: la nota de credito resta del facturado; factura y nota de debito suman.
        public decimal TotalConSigno { get { return TiposComprobanteCompra.Signo(TipoComprobante) * Total; } }

        // Numero legible "0003-00001902".
        public string NumeroCompleto
        {
            get { return PtoVta.ToString("0000") + "-" + Numero.ToString("00000000"); }
        }
    }

    // Catalogo de tipos de comprobante del proveedor. Se guardan como texto (igual que compras.tipocompra).
    public static class TiposComprobanteCompra
    {
        public const string FacturaA = "Factura A";
        public const string FacturaB = "Factura B";
        public const string FacturaC = "Factura C";
        public const string FacturaM = "Factura M";
        public const string NotaCreditoA = "Nota de credito A";
        public const string NotaCreditoB = "Nota de credito B";
        public const string NotaCreditoC = "Nota de credito C";
        public const string NotaCreditoM = "Nota de credito M";
        public const string NotaDebitoA = "Nota de debito A";
        public const string NotaDebitoB = "Nota de debito B";
        public const string NotaDebitoC = "Nota de debito C";
        public const string NotaDebitoM = "Nota de debito M";
        public const string Otro = "Otro";

        // Orden en el que se ofrecen en el selector.
        public static readonly IReadOnlyList<string> Todos = new List<string>
        {
            FacturaA, FacturaB, FacturaC, FacturaM,
            NotaCreditoA, NotaCreditoB, NotaCreditoC, NotaCreditoM,
            NotaDebitoA, NotaDebitoB, NotaDebitoC, NotaDebitoM,
            Otro
        }.AsReadOnly();

        public static bool EsValido(string tipo)
        {
            return !string.IsNullOrWhiteSpace(tipo) && Todos.Contains(tipo);
        }

        public static bool EsFactura(string tipo)
        {
            return tipo != null && tipo.StartsWith("Factura ", StringComparison.Ordinal);
        }

        public static bool EsNotaCredito(string tipo)
        {
            return tipo != null && tipo.StartsWith("Nota de credito", StringComparison.Ordinal);
        }

        public static bool EsNotaDebito(string tipo)
        {
            return tipo != null && tipo.StartsWith("Nota de debito", StringComparison.Ordinal);
        }

        // -1 para la nota de credito, +1 para el resto.
        public static int Signo(string tipo)
        {
            return EsNotaCredito(tipo) ? -1 : 1;
        }
    }

    // Estado de la factura de una compra (compras.estadofactura). null en la compra = anterior a la funcionalidad.
    public static class EstadoFacturaCompra
    {
        public const string Pendiente = "PENDIENTE";
        public const string Facturada = "FACTURADA";
        public const string NoLleva = "NO_LLEVA";
    }
}
