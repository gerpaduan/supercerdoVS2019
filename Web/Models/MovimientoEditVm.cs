using System;
using System.Collections.Generic;

namespace Web.Models
{
    public class MovimientoEditVm
    {
        public MovimientoEditVm()
        {
            Lineas = new List<MovimientoLineaVm>();
            FechaMovimiento = DateTime.Now;
        }

        public int IdMovimiento { get; set; }
        public bool EsEdicion { get; set; }
        public bool SoloLecturaInicial { get; set; }
        public bool PuedeHabilitarEdicion { get; set; }
        public long EmpresaCuit { get; set; }
        public bool MostrarColumnasInternas { get; set; }

        public int IdSucursalOrigen { get; set; }
        public int IdSucursalDestino { get; set; }
        public DateTime FechaMovimiento { get; set; }
        public string Observaciones { get; set; }

        public string UsuarioNombre { get; set; }
        public string Creado { get; set; }
        public string CreadoPor { get; set; }
        public string Actualizado { get; set; }
        public string ActualizadoPor { get; set; }
        public string IdMovimientoOrigen { get; set; }
        public string IdMovimientoDestino { get; set; }

        public List<MovimientoLineaVm> Lineas { get; set; }
    }

    public class MovimientoLineaVm
    {
        public int IdCorteMovimiento { get; set; }
        public int IdCorte { get; set; }
        public long Codigo { get; set; }
        public string Producto { get; set; }
        public string TipoProducto { get; set; }
        public bool Pesable { get; set; }
        public float PromedioProducto { get; set; }
        public int CantUnidad { get; set; }
        public float CantKg { get; set; }
        public bool PesoBalanza { get; set; }
        public bool PermitirIngreso { get; set; }
    }
}
