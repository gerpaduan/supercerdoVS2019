// Modelos de las pantallas de comprobantes del proveedor de una compra (factura, nota de credito/debito) y del titular propio.
// Compras: titular propio y comprobantes del proveedor (2026-11-09, ver docs/DECISIONS.md). Solo Postgres.
using System;
using System.Collections.Generic;

namespace WebCore.Models
{
    // Opcion del selector de titular propio (compras y pagos). Seleccionable = habilitado y no cancelado (los demas solo se ven
    // para mostrar la historia).
    public class TitularCompraOpcionVm
    {
        public int IdTitular { get; set; }
        public string Nombre { get; set; } = "";
        public bool EsPrincipal { get; set; }
        public bool Seleccionable { get; set; }
    }

    // Contenido del modal "Comprobantes de la compra".
    public class ComprobantesCompraVm
    {
        public Entidades.DetalleComprobantesCompra Detalle { get; set; }
        public string Proveedor { get; set; } = "";
        public string NroRemito { get; set; } = "";
        public DateTime FechaCompra { get; set; }
        // El usuario puede cargar, editar y borrar comprobantes (permiso "Modificar compra").
        public bool PuedeEditar { get; set; }
        // La empresa tiene 2 o mas titulares: se muestra el titular de la compra.
        public bool MostrarTitulares { get; set; }
        public int IdTitularActual { get; set; }
        public List<TitularCompraOpcionVm> Titulares { get; set; } = new List<TitularCompraOpcionVm>();
        // Alicuota de IVA por defecto (configuracion Compras:AlicuotaIvaDefault).
        public decimal AlicuotaDefault { get; set; } = 21m;
        public decimal[] Alicuotas { get; set; } = new decimal[0];
        public IReadOnlyList<string> Tipos { get; set; } = Entidades.TiposComprobanteCompra.Todos;
        public bool CompraAdmiteComprobantes { get; set; }
    }

    // Datos que manda el formulario del modal para guardar un comprobante. Los importes viajan como texto con punto decimal
    // (la pantalla los normaliza): el servidor no depende de la cultura del proceso.
    public class ComprobanteCompraFormVm
    {
        public int IdComprobante { get; set; }
        public int IdCompra { get; set; }
        public string TipoComprobante { get; set; } = "";
        public int PtoVta { get; set; }
        public long Numero { get; set; }
        // yyyy-MM-dd
        public string Fecha { get; set; } = "";
        public string AlicuotaIva { get; set; } = "";
        public string NetoGravado { get; set; } = "";
        public string Iva { get; set; } = "";
        public string OtrosImpuestos { get; set; } = "";
        public string Total { get; set; } = "";
    }
}
