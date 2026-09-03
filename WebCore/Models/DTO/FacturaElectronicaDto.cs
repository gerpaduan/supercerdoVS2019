// Port de Web/Models/DTO/FacturaElectronicaDTO.cs y LineaVentaDto.cs -- solo los campos que usa
// el flujo "facturar sin venta" (VentasController.NuevaFacturaSinVenta/CrearVentaManualParaFactura/
// GenerarFactura/LimpiarLineasVentaManual, ver docs/DECISIONS.md, mini-spike AFIP).
using System;
using System.Collections.Generic;

namespace WebCore.Models.DTO
{
    public class FacturaElectronicaDto
    {
        public int IdVenta { get; set; }
        public int IdFactura { get; set; }

        public int CodTipoCbteAfip { get; set; }
        public string DescTipoCbteAfip { get; set; }
        public string LetraCbte { get; set; }
        public string PtoVtaAfip { get; set; }
        public string NroCbteAfip { get; set; }
        public DateTime? FechaEmisionAfip { get; set; }

        public string EmisorRazonSocial { get; set; }
        public string EmisorCUIT { get; set; }
        public string EmisorCondicionIVA { get; set; }
        public string EmisorDomicilio { get; set; }
        public string EmisorIngresosBrutos { get; set; }
        public string EmisorInicioActividad { get; set; }

        public string TipoDocAfip { get; set; }
        public string NroDocAfip { get; set; }
        public string RazonSocialAFIP { get; set; }
        public string CondicionIvaAFIP { get; set; }
        public string DomicilioAFIP { get; set; }
        public string Email { get; set; }
        public string Whatsapp { get; set; }

        public string CondicionVenta { get; set; }
        public string FormaPago { get; set; }
        public string DescItemUnitario { get; set; }
        public bool AgruparItemUnitario { get; set; }
        public string Observaciones { get; set; }

        public decimal PorcentajeFacturacion { get; set; }
        public decimal ImporteNetoGravado { get; set; }
        public decimal Iva { get; set; }
        public decimal ImporteTotal { get; set; }

        public string CAE { get; set; }
        public string FecVtoCAE { get; set; }

        public List<LineaVentaDto> Detalle { get; set; } = new List<LineaVentaDto>();
    }

    public class LineaVentaDto
    {
        public int IdLineaVenta { get; set; }
        public int IdCorte { get; set; }
        public long Codigo { get; set; }
        public string Descripcion { get; set; }
        public float CantKg { get; set; }
        public float PrecioKg { get; set; }
        public float Importe { get; set; }
        public float IdAlicuotaIva { get; set; }
        public float AlicuotaIva { get; set; }
        public float Bonificacion { get; set; }
        public int Estado { get; set; }
        public bool Balanza { get; set; }
        public int IndexAnulado { get; set; } = -1;
        public int IdExpendio { get; set; }
    }
}
