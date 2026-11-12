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

        // Columna "Creado" del listado: textos ya formateados (dd/MM/yyyy HH:mm:ss), vacios si no hay dato.
        public string Creado { get; set; } = "";
        public string CreadoPor { get; set; } = "";
        public string Actualizado { get; set; } = "";
        public string ActualizadoPor { get; set; } = "";
    }

    public class MovimientoDetalleVm
    {
        public MovimientoDetalleVm()
        {
            Lineas = new List<MovimientoLineaVm>();
        }

        public int IdMovimiento { get; set; }
        // Nombres de las sucursales (franja de chips del detalle, 2026-11-12): "Sale de" -> "Entra a". Vacios si no se pudieron resolver.
        public string Origen { get; set; } = "";
        public string Destino { get; set; } = "";
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

        // Tiempo de envio en minutos (solo movimiento nuevo): se carga con el parametro minutosEnvioMovimiento y se puede ajustar.
        // Al guardar, la fecha del movimiento queda en la hora actual + estos minutos (MovimientoTiempos.CalcularLlegada).
        public int TiempoEnvioMinutos { get; set; }
        // true si el usuario tipeo la fecha a mano en un movimiento nuevo (ej. cargar uno de ayer): se respeta y no se suman minutos.
        public bool FechaManual { get; set; }
        // Solo edicion: hora de la primera linea cargada ("14:58", o con fecha si fue otro dia que el guardado) y dia del guardado
        // (yyyy-MM-dd) contra el que la pantalla decide si la hora de una linea es "de otro dia". Vacios si no hay dato.
        public string InicioCarga { get; set; } = "";
        public string FechaReferenciaCarga { get; set; } = "";

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
        // Hora en que se cargo la linea, "dd/MM/yyyy HH:mm" (vacio en las lineas anteriores a 2026-11-11). Viaja ida y vuelta para
        // que editar el movimiento no la pierda (ver MovimientoTiempos.ResolverCreadoLinea).
        public string CreadoTexto { get; set; } = "";
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
        // Fecha del movimiento: la vista plana de Lineas la usa para ordenar (data-fecha de la tarjeta).
        public DateTime Fecha { get; set; }
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
        // true si la linea se cargo con el peso de la balanza (columna "Balanza" de Lineas, 2026-11-11).
        public bool Balanza { get; set; }
        public string Observacion { get; set; }
        public decimal CantidadKg { get; set; }
        public decimal CantidadUnidad { get; set; }
    }
}
