using System;
using System.Collections.Generic;

namespace Web.Models
{
    public class PuntoExpendioEditVm
    {
        public PuntoExpendioEditVm()
        {
            FechaExpendio = DateTime.Now;
            Lineas = new List<PuntoExpendioLineaVm>();
            SectoresDisponibles = new List<string>();
        }

        public int IdExpendio { get; set; }
        public bool EsGuardado { get; set; }
        public string Sector { get; set; }
        public DateTime FechaExpendio { get; set; }
        public string IdentificacionCliente { get; set; }
        public string Observaciones { get; set; }
        public string SucursalNombre { get; set; }
        public string VendedorNombre { get; set; }
        public bool PermiteEditarPrecio { get; set; }
        public int CantItems { get; set; }
        public float TotalKilos { get; set; }
        public float TotalImporte { get; set; }
        public string TipoImpresion { get; set; }
        public List<string> SectoresDisponibles { get; set; }
        public List<PuntoExpendioLineaVm> Lineas { get; set; }
    }

    public class PuntoExpendioLineaVm
    {
        public int IdCorte { get; set; }
        public long Codigo { get; set; }
        public string Producto { get; set; }
        public float CantKg { get; set; }
        public float PrecioKg { get; set; }
        public bool PesoBalanza { get; set; }
        public float Total { get; set; }
    }

    public class SectorAbmVm
    {
        public SectorAbmVm()
        {
            Sectores = new List<SectorResumenVm>();
        }

        public string SectorOriginal { get; set; }
        public string Nombre { get; set; }
        public List<SectorResumenVm> Sectores { get; set; }
    }

    public class SectorResumenVm
    {
        public string Nombre { get; set; }
        public bool EnUso { get; set; }
    }
}
