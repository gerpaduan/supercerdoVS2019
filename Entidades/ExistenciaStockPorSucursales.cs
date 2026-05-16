using System;
using System.Collections.Generic;
using System.Linq;

namespace Entidades
{
    public class ExistenciaStockPorSucursalFiltroVm
    {
        public string Texto { get; set; }
        public int IdSucursal { get; set; }
        public DateTime? FechaHasta { get; set; }
        public string Tipo { get; set; }
        public int IdProveedor { get; set; }
        public int IdMarca { get; set; }
        public int IdCorte { get; set; }
        public bool SoloConStock { get; set; }
        public string EstadoStock { get; set; }

        public List<SucursalColumnaStockVm> SucursalesDisponibles { get; set; }
        public List<string> TiposDisponibles { get; set; }
        public List<Persona> ProveedoresDisponibles { get; set; }
        public List<Persona> MarcasDisponibles { get; set; }
        public List<string> EstadosDisponibles { get; set; }

        public ExistenciaStockPorSucursalFiltroVm()
        {
            Texto = "";
            Tipo = "";
            EstadoStock = "Todos";
            FechaHasta = DateTime.Now;
            SucursalesDisponibles = new List<SucursalColumnaStockVm>();
            TiposDisponibles = new List<string>();
            ProveedoresDisponibles = new List<Persona>();
            MarcasDisponibles = new List<Persona>();
            EstadosDisponibles = new List<string> { "Todos", "OK", "BAJO", "SIN STOCK", "NEGATIVO" };
        }
    }

    public class ExistenciaStockPorSucursalPlanoVm
    {
        public int IdCorte { get; set; }
        public long Codigo { get; set; }
        public string Corte { get; set; }
        public int IdSucursal { get; set; }
        public string Sucursal { get; set; }
        public DateTime? FechaUltimoCierre { get; set; }
        public float StockInicial { get; set; }
        public float Compras { get; set; }
        public float IngresoElaborado { get; set; }
        public float IngresoStock { get; set; }
        public float IngresoMovimiento { get; set; }
        public float AjusteStock { get; set; }
        public float TotalIngresos { get; set; }
        public float EgresoStock { get; set; }
        public float EgresoMovimiento { get; set; }
        public float EgresoElaborado { get; set; }
        public float Ventas { get; set; }
        public float TotalEgresos { get; set; }
        public float StockActual { get; set; }
        public float Promedio { get; set; }
        public float PuntoStock { get; set; }
        public string EstadoStock { get; set; }
    }

    public class ProductoStockPorSucursalVm
    {
        public int IdCorte { get; set; }
        public long Codigo { get; set; }
        public string Corte { get; set; }
        public List<StockSucursalCeldaVm> Celdas { get; set; }
        public List<DetalleStockSucursalVm> Detalles { get; set; }

        public ProductoStockPorSucursalVm()
        {
            Celdas = new List<StockSucursalCeldaVm>();
            Detalles = new List<DetalleStockSucursalVm>();
        }

        public string TextoBusqueda
        {
            get
            {
                return ((Codigo > 0 ? Codigo.ToString() + " " : "") + (Corte ?? "")).Trim().ToUpperInvariant();
            }
        }

        public string EstadosFila
        {
            get
            {
                return string.Join("|", Detalles
                    .Select(x => (x.EstadoStock ?? "").Trim().ToUpperInvariant())
                    .Where(x => x.Length > 0)
                    .Distinct());
            }
        }

        public bool TieneStockPositivo
        {
            get { return Detalles.Any(x => x.StockActual > 0); }
        }
    }

    public class SucursalColumnaStockVm
    {
        public int IdSucursal { get; set; }
        public string Sucursal { get; set; }
    }

    public class StockSucursalCeldaVm
    {
        public int IdSucursal { get; set; }
        public string Sucursal { get; set; }
        public float StockActual { get; set; }
        public string EstadoStock { get; set; }
    }

    public class DetalleStockSucursalVm
    {
        public int IdSucursal { get; set; }
        public string Sucursal { get; set; }
        public DateTime? FechaUltimoCierre { get; set; }
        public float StockInicial { get; set; }
        public float Compras { get; set; }
        public float IngresoElaborado { get; set; }
        public float IngresoStock { get; set; }
        public float IngresoMovimiento { get; set; }
        public float AjusteStock { get; set; }
        public float TotalIngresos { get; set; }
        public float EgresoStock { get; set; }
        public float EgresoMovimiento { get; set; }
        public float EgresoElaborado { get; set; }
        public float Ventas { get; set; }
        public float TotalEgresos { get; set; }
        public float StockActual { get; set; }
        public string EstadoStock { get; set; }
    }

    public class ExistenciaPorSucursalesVm
    {
        public ExistenciaStockPorSucursalFiltroVm Filtro { get; set; }
        public List<SucursalColumnaStockVm> Columnas { get; set; }
        public List<ProductoStockPorSucursalVm> Productos { get; set; }
        public bool ConsultaRealizada { get; set; }
        public string Mensaje { get; set; }

        public ExistenciaPorSucursalesVm()
        {
            Filtro = new ExistenciaStockPorSucursalFiltroVm();
            Columnas = new List<SucursalColumnaStockVm>();
            Productos = new List<ProductoStockPorSucursalVm>();
            Mensaje = "";
        }
    }
}
