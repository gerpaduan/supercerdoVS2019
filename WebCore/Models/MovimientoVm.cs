// ViewModels de Movimientos (traslados de stock entre sucursales), port literal de
// Web/Models/MovimientoIndexVm.cs, Web/Models/MovimientoEditVm.cs y las clases de Movimiento en
// Web/Models/LineasAgrupadasVm.cs. CabeceraDetalleCampoVm ya existe en StockLineasIndexVm.cs -- se
// reusa, no se duplica (CLAUDE.md §8.4).
using System;
using System.Collections.Generic;

namespace WebCore.Models
{
    public class MovimientoIndexVm
    {
        public MovimientoIndexVm()
        {
            Movimientos = new List<MovimientoResumenVm>();
        }

        public int IdSucursalOrigen { get; set; }
        public int IdSucursalDestino { get; set; }
        public DateTime FechaDesde { get; set; }
        public DateTime FechaHasta { get; set; }
        public bool VerDetalles { get; set; }
        public bool MostrarColumnasInternas { get; set; }
        public List<MovimientoResumenVm> Movimientos { get; set; }
    }

    public class MovimientoResumenVm
    {
        public int IdMovimiento { get; set; }
        public DateTime FechaMovimiento { get; set; }
        public string Origen { get; set; }
        public string Destino { get; set; }
        public string DeOrigen { get; set; }
        public string Estado { get; set; }
        public decimal TotalUnidad { get; set; }
        public decimal TotalKilos { get; set; }
        public string Observaciones { get; set; }
        public bool TieneObservaciones { get; set; }
    }

    public class MovimientoDetalleVm
    {
        public MovimientoDetalleVm()
        {
            Lineas = new List<MovimientoLineaVm>();
        }

        public int IdMovimiento { get; set; }
        public string Observaciones { get; set; }
        public DateTime? Creado { get; set; }
        public string CreadoPor { get; set; }
        public DateTime? Actualizado { get; set; }
        public string ActualizadoPor { get; set; }
        public List<MovimientoLineaVm> Lineas { get; set; }
    }

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

    public class MovimientoLineasIndexPageVm
    {
        public MovimientoLineasIndexPageVm()
        {
            Movimientos = new List<MovimientoLineasGrupoVm>();
        }

        public int IdSucursalOrigen { get; set; }
        public int IdSucursalDestino { get; set; }
        public string Producto { get; set; }
        public DateTime FechaDesde { get; set; }
        public DateTime FechaHasta { get; set; }
        public List<MovimientoLineasGrupoVm> Movimientos { get; set; }
    }

    public class MovimientoLineasGrupoVm
    {
        public MovimientoLineasGrupoVm()
        {
            Campos = new List<CabeceraDetalleCampoVm>();
            Lineas = new List<MovimientoLineaDetalleItemVm>();
        }

        public int IdMovimiento { get; set; }
        public string CollapseId { get; set; }
        public string Titulo { get; set; }
        public string Subtitulo { get; set; }
        public string ResumenCompacto { get; set; }
        public string ResumenSecundario { get; set; }
        public string EditUrl { get; set; }
        public decimal TotalKg { get; set; }
        public decimal TotalUnidades { get; set; }
        public List<CabeceraDetalleCampoVm> Campos { get; set; }
        public List<MovimientoLineaDetalleItemVm> Lineas { get; set; }
    }

    public class MovimientoLineaDetalleItemVm
    {
        public string Codigo { get; set; }
        public string Producto { get; set; }
        public string CantidadKgTexto { get; set; }
        public string CantidadUnidadTexto { get; set; }
        public string Observacion { get; set; }
        public decimal CantidadKg { get; set; }
        public decimal CantidadUnidad { get; set; }
    }
}
