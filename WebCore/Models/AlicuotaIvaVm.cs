namespace WebCore.Models
{
    // Fila del combo de alicuotas de IVA para "Facturar sin venta" (modal Factura Electronica,
    // 2026-09-06 -- ver docs/DECISIONS.md). Reemplaza el DataTable que usa Web/Views/Ventas/
    // _FacturaElectronica.cshtml -- mismos nombres de campo (idIva/iva), tipado fuerte.
    public class AlicuotaIvaVm
    {
        public int IdIva { get; set; }
        public double Iva { get; set; }
    }
}
