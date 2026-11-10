using System;
using System.Collections.Generic;

namespace Entidades
{
    // Filas crudas que el repositorio entrega para armar el resumen de la cuenta corriente de un proveedor por titular propio
    // (Compras: titular propio y comprobantes del proveedor, 2026-11-09, ver docs/DECISIONS.md). El titular ya viene resuelto:
    // el principal (NULL en la base) llega con su id real; 0 = no hay titulares cargados.

    // Movimiento de cuenta corriente (importe con signo de la base: Credito positivo = se le debe al proveedor).
    public class MovimientoResumen
    {
        public DateTime Fecha { get; set; }
        public string Tabla { get; set; }       // Compras | Ventas | Pagos | MovCtaCte | Liquidaciones
        public decimal Importe { get; set; }
        public int IdTitular { get; set; }
    }

    // Pago a un proveedor (no eliminado). Efectivo = la parte en efectivo (solo cuenta en "Efectivo + Cheques").
    public class PagoResumen
    {
        public DateTime Fecha { get; set; }
        public string FormaPago { get; set; }
        public decimal Importe { get; set; }
        public decimal Efectivo { get; set; }
        public int IdTitular { get; set; }
    }

    // Comprobante activo de una compra no anulada. Fecha = la de la compra; Total ya lleva el signo (la nota de credito resta).
    public class FacturadoResumen
    {
        public DateTime FechaCompra { get; set; }
        public decimal TotalConSigno { get; set; }
        public int IdTitular { get; set; }
    }

    // Una fila del resumen por titular. Calculada por Negocio.ResumenCtaCtePorTitular.
    public class FilaResumenTitular
    {
        public int IdTitular { get; set; }
        public string NombreTitular { get; set; }
        // Bloque deuda
        public decimal SaldoAnterior { get; set; }
        public decimal Comprado { get; set; }
        public decimal Pagado { get; set; }
        public decimal Otros { get; set; }
        public decimal Saldo { get; set; }
        // Bloque control fiscal
        public decimal FacturadoNeto { get; set; }
        public decimal PagadoBancarizado { get; set; }
        public decimal PagadoEfectivo { get; set; }
        // Facturado menos pagado bancarizado: negativo = se pago por banco mas de lo facturado.
        public decimal DiferenciaFacturadoBancarizado { get; set; }
        public decimal SinFactura { get; set; }
        public bool PagadoBancarizadoExcedeFacturado { get; set; }
    }

    public class ResumenProveedor
    {
        public List<FilaResumenTitular> Filas { get; set; } = new List<FilaResumenTitular>();
        public FilaResumenTitular Total { get; set; } = new FilaResumenTitular { NombreTitular = "Total" };
    }

    // Estado de factura de una compra y totales de sus comprobantes, para la bandeja de Compras/Index.
    public class InfoFacturaCompra
    {
        public int IdCompra { get; set; }
        public int? IdTitular { get; set; }
        public string EstadoFactura { get; set; }
        public decimal FacturadoNeto { get; set; }
        public string TipoFactura { get; set; }
        public string NumeroFactura { get; set; }
        public bool TieneNotaCredito { get; set; }
        public bool TieneNotaDebito { get; set; }
    }
}

namespace Entidades
{
    // Todo lo que muestra el modal de comprobantes de una compra.
    public class DetalleComprobantesCompra
    {
        public int IdCompra { get; set; }
        public string TipoCompra { get; set; }
        public int? IdTitular { get; set; }
        public string EstadoFactura { get; set; }
        public decimal TotalCompra { get; set; }
        public decimal FacturadoNeto { get; set; }
        public decimal SinFactura { get; set; }
        public List<ComprobanteCompra> Comprobantes { get; set; } = new List<ComprobanteCompra>();
    }
}
