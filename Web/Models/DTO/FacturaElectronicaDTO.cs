using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Web.Models.DTO
{
    public class FacturaElectronicaDTO
    {
        // ===== Identificación =====
        public int IdVenta { get; set; }
        public int IdFactura { get; set; }

        // ===== Comprobante AFIP =====
        public int CodTipoCbteAfip { get; set; }
        public string DescTipoCbteAfip { get; set; }
        public string LetraCbte { get; set; }
        public string PtoVtaAfip { get; set; }
        public string NroCbteAfip { get; set; }
        public DateTime? FechaEmisionAfip { get; set; }

        // ===== EMISOR =====
        public string EmisorRazonSocial { get; set; }
        public string EmisorCUIT { get; set; }
        public string EmisorCondicionIVA { get; set; }
        public string EmisorDomicilio { get; set; }
        public string EmisorIngresosBrutos { get; set; }
        public string EmisorInicioActividad { get; set; }

        // ===== Cliente =====
        public string TipoDocAfip { get; set; }
        public string NroDocAfip { get; set; }
        public string RazonSocialAFIP { get; set; }
        public string CondicionIvaAFIP { get; set; }
        public string DomicilioAFIP { get; set; }
        public string Email { get; set; }
        public string Whatsapp { get; set; }

        // ===== Venta =====
        public string CondicionVenta { get; set; }
        public string FormaPago { get; set; }
        public string DescItemUnitario { get; set; }
        public bool AgruparItemUnitario { get; set; }
        public string Observaciones { get; set; }

        // ===== Importes =====

        public decimal PorcentajeFacturacion { get; set; }
        public decimal ImporteNetoGravado { get; set; }
        public decimal Iva { get; set; }
        public decimal ImporteTotal { get; set; }

        // ===== CAE =====
        public string CAE { get; set; }
        public string FecVtoCAE { get; set; }

        // ===== Detalle =====
        public List<LineaVentaDto> Detalle { get; set; }
            = new List<LineaVentaDto>();
    }
}
