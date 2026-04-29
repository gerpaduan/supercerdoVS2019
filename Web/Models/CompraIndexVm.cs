using System;
using System.Collections.Generic;
using System.Data;

namespace Web.Models
{
    public class CompraIndexVm
    {
        public DataTable Compras { get; set; }

        public Dictionary<int, CompraIndexDetalleVm> Detalles { get; set; }

        public CompraIndexVm()
        {
            Compras = new DataTable();
            Detalles = new Dictionary<int, CompraIndexDetalleVm>();
        }
    }

    public class CompraIndexDetalleVm
    {
        public int IdCompra { get; set; }
        public DateTime? FechaCompra { get; set; }
        public string NumeroDocumento { get; set; }
        public string Proveedor { get; set; }
        public string TipoCompra { get; set; }
        public float Cantidad { get; set; }
        public int CantidadMedias { get; set; }
        public float Total { get; set; }
        public string Sucursal { get; set; }
        public string Observaciones { get; set; }
        public string Estado { get; set; }
        public bool EnCtaCte { get; set; }
        public string UsuarioCreacion { get; set; }
        public DateTime? FechaCreacion { get; set; }
        public string UsuarioActualizacion { get; set; }
        public DateTime? FechaActualizacion { get; set; }
    }
}
