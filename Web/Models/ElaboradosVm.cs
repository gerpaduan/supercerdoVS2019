using System;
using System.Collections.Generic;

namespace Web.Models
{
    public class ElaboradoIndexVm
    {
        public ElaboradoIndexVm()
        {
            Items = new List<ElaboradoResumenVm>();
            Detalles = new Dictionary<int, ElaboradoDetalleVm>();
            Tabs = new List<ElaboradoTabVm>();
            FechaDesde = DateTime.Today.AddDays(-7);
            FechaHasta = DateTime.Today;
        }

        public int IdSucursal { get; set; }
        public string Elaborado { get; set; }
        public DateTime FechaDesde { get; set; }
        public DateTime FechaHasta { get; set; }
        public float TotalKg { get; set; }
        public List<ElaboradoResumenVm> Items { get; set; }
        public Dictionary<int, ElaboradoDetalleVm> Detalles { get; set; }
        public List<ElaboradoTabVm> Tabs { get; set; }
    }

    public class ElaboradoLineasIndexVm
    {
        public ElaboradoLineasIndexVm()
        {
            Items = new List<ElaboradoLineaResumenVm>();
            Tabs = new List<ElaboradoTabVm>();
            FechaDesde = DateTime.Today.AddDays(-7);
            FechaHasta = DateTime.Today;
        }

        public int IdSucursal { get; set; }
        public string Descripcion { get; set; }
        public DateTime FechaDesde { get; set; }
        public DateTime FechaHasta { get; set; }
        public float TotalKg { get; set; }
        public List<ElaboradoLineaResumenVm> Items { get; set; }
        public List<ElaboradoTabVm> Tabs { get; set; }
    }

    public class ElaboradoFormulasIndexVm
    {
        public ElaboradoFormulasIndexVm()
        {
            Items = new List<ElaboradoFormulaResumenVm>();
            Detalles = new Dictionary<int, ElaboradoFormulaDetalleVm>();
            Tabs = new List<ElaboradoTabVm>();
        }

        public string Descripcion { get; set; }
        public bool PuedeEditar { get; set; }
        public bool PuedeEliminar { get; set; }
        public bool PuedeCrear { get; set; }
        public List<ElaboradoFormulaResumenVm> Items { get; set; }
        public Dictionary<int, ElaboradoFormulaDetalleVm> Detalles { get; set; }
        public List<ElaboradoTabVm> Tabs { get; set; }
    }

    public class ElaboradoFormulaEditVm
    {
        public ElaboradoFormulaEditVm()
        {
            Tabs = new List<ElaboradoTabVm>();
            Lineas = new List<ElaboradoFormulaEditLineaVm>();
        }

        public int IdFormula { get; set; }
        public bool EsEdicion { get; set; }
        public bool SoloLecturaInicial { get; set; }
        public int IdElaborado { get; set; }
        public long CodigoElaborado { get; set; }
        public string Elaborado { get; set; }
        public string Receta { get; set; }
        public string UsuarioNombre { get; set; }
        public string Creado { get; set; }
        public string CreadoPor { get; set; }
        public string Actualizado { get; set; }
        public string ActualizadoPor { get; set; }
        public float TotalPorcentaje { get; set; }
        public float TotalUnidades { get; set; }
        public List<ElaboradoFormulaEditLineaVm> Lineas { get; set; }
        public List<ElaboradoTabVm> Tabs { get; set; }
    }

    public class ElaboradoFormulaEditLineaVm
    {
        public int IdCorte { get; set; }
        public long Codigo { get; set; }
        public string Producto { get; set; }
        public float Porcentaje { get; set; }
        public bool AgregarAuto { get; set; }
    }

    public class ElaboradoPlaceholderVm
    {
        public ElaboradoPlaceholderVm()
        {
            Tabs = new List<ElaboradoTabVm>();
        }

        public string Titulo { get; set; }
        public string Descripcion { get; set; }
        public string Nota { get; set; }
        public List<ElaboradoTabVm> Tabs { get; set; }
    }

    public class ElaboradoCargaVm
    {
        public ElaboradoCargaVm()
        {
            Tabs = new List<ElaboradoTabVm>();
            Lineas = new List<ElaboradoCargaLineaVm>();
            Formula = new List<ElaboradoFormulaLineaVm>();
            FechaEmbutido = DateTime.Now;
        }

        public int IdEmbutido { get; set; }
        public bool EsEdicion { get; set; }
        public bool PermiteGuardarEdicion { get; set; }
        public bool PuedeAnular { get; set; }
        public bool EsPesableElaborado { get; set; }
        public int IdSucursal { get; set; }
        public DateTime FechaEmbutido { get; set; }
        public string Observaciones { get; set; }
        public string UsuarioNombre { get; set; }
        public string Estado { get; set; }
        public string Creado { get; set; }
        public string CreadoPor { get; set; }
        public string Actualizado { get; set; }
        public string ActualizadoPor { get; set; }

        public int IdElaborado { get; set; }
        public long CodigoElaborado { get; set; }
        public string Elaborado { get; set; }
        public string Receta { get; set; }
        public bool IngresoRapidoSugerido { get; set; }

        public List<ElaboradoCargaLineaVm> Lineas { get; set; }
        public List<ElaboradoFormulaLineaVm> Formula { get; set; }
        public List<ElaboradoTabVm> Tabs { get; set; }
    }

    public class ElaboradoCargaLineaVm
    {
        public int IdCorte { get; set; }
        public long Codigo { get; set; }
        public string Producto { get; set; }
        public string TipoProducto { get; set; }
        public float CantKg { get; set; }
        public bool PesoBalanza { get; set; }
    }

    public class ElaboradoFormulaLineaVm
    {
        public int IdCorte { get; set; }
        public long Codigo { get; set; }
        public string Producto { get; set; }
        public float Porcentaje { get; set; }
        public bool AgregarAuto { get; set; }
        public float Kgs { get; set; }
    }

    public class ElaboradoResumenVm
    {
        public int Id { get; set; }
        public DateTime Fecha { get; set; }
        public string Sucursal { get; set; }
        public long Codigo { get; set; }
        public string Elaborado { get; set; }
        public float Kgs { get; set; }
        public string Observaciones { get; set; }
        public string Estado { get; set; }
        public string Creado { get; set; }
        public string Actualizado { get; set; }
        public bool EsDesarme { get; set; }
    }

    public class ElaboradoDetalleVm
    {
        public int Id { get; set; }
        public DateTime Fecha { get; set; }
        public string Sucursal { get; set; }
        public long Codigo { get; set; }
        public string Elaborado { get; set; }
        public float Kgs { get; set; }
        public string Observaciones { get; set; }
        public string Estado { get; set; }
        public string Receta { get; set; }
        public string UsuarioCreacion { get; set; }
        public DateTime? FechaCreacion { get; set; }
        public string UsuarioActualizacion { get; set; }
        public DateTime? FechaActualizacion { get; set; }
    }

    public class ElaboradoLineaResumenVm
    {
        public int Id { get; set; }
        public DateTime Fecha { get; set; }
        public string Sucursal { get; set; }
        public long CodigoElaborado { get; set; }
        public string Elaborado { get; set; }
        public long CodigoIngrediente { get; set; }
        public string Ingrediente { get; set; }
        public float Kgs { get; set; }
        public string Estado { get; set; }
        public string Observaciones { get; set; }
    }

    public class ElaboradoFormulaResumenVm
    {
        public int IdFormula { get; set; }
        public long Codigo { get; set; }
        public string Elaborado { get; set; }
        public string Creado { get; set; }
        public string Actualizado { get; set; }
    }

    public class ElaboradoFormulaDetalleVm
    {
        public int IdFormula { get; set; }
        public long Codigo { get; set; }
        public string Elaborado { get; set; }
        public string Receta { get; set; }
        public string Creado { get; set; }
        public string CreadoPor { get; set; }
        public string Actualizado { get; set; }
        public string ActualizadoPor { get; set; }
        public List<ElaboradoFormulaEditLineaVm> Lineas { get; set; }

        public ElaboradoFormulaDetalleVm()
        {
            Lineas = new List<ElaboradoFormulaEditLineaVm>();
        }
    }

    public class ElaboradoTabVm
    {
        public string Titulo { get; set; }
        public string Action { get; set; }
        public bool Activo { get; set; }
    }
}
