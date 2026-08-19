using System;
using System.Collections.Generic;

namespace Contratos
{
    // Tipos de retorno de ICierreCajaRepository.obtenerPreviewCambioSucursalCaja/
    // cambiarSucursalCaja. Movidos desde Datos.CierreCaja (donde vivian como clases anidadas)
    // porque tanto Datos como DatosPostgres implementan la interfaz y ambos necesitan devolver
    // el mismo tipo. Son POCOs puros, sin cambios de forma respecto al original. Ver
    // docs/DECISIONS.md, Etapa 10.
    public sealed class CambioSucursalCajaTabla
    {
        public string Tabla { get; set; }
        public int Cantidad { get; set; }
    }

    public sealed class CambioSucursalCajaPreview
    {
        public bool PuedeEjecutar { get; set; }
        public string Mensaje { get; set; }
        public int IdCierreCaja { get; set; }
        public int IdCierreCajaNuevo { get; set; }
        public int IdSucursalActual { get; set; }
        public string SucursalActual { get; set; }
        public int IdSucursalNueva { get; set; }
        public string SucursalNueva { get; set; }
        public int IdUsuarioCaja { get; set; }
        public string UsuarioCaja { get; set; }
        public DateTime FechaDesde { get; set; }
        public DateTime FechaHasta { get; set; }
        public bool TieneCajaAbiertaEnDestino { get; set; }
        public List<CambioSucursalCajaTabla> Tablas { get; set; } = new List<CambioSucursalCajaTabla>();
    }

    public sealed class CambioSucursalCajaResultado
    {
        public bool Ok { get; set; }
        public string Mensaje { get; set; }
        public List<CambioSucursalCajaTabla> Tablas { get; set; } = new List<CambioSucursalCajaTabla>();
    }
}
