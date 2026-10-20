using System;
using System.Collections.Generic;

namespace Entidades
{
    // Un pedido de compra armado desde Stock > Existencia por sucursales ("Ingresar pedido"): una cantidad por producto
    // y sucursal. UN pedido puede cubrir varias sucursales (la cabecera no tiene sucursal propia). Solo Postgres. Las
    // lineas son un snapshot de lo que se veia al guardar; los importes son una ESTIMACION (cantidad x precio de la
    // ultima compra). Reglas y porques: docs/DECISIONS.md (2026-10-19, Pedidos) y Negocio/Pedido.cs.
    public class Pedido
    {
        public int Id { get; set; }
        public int IdEmpresa { get; set; }

        // Id que genera el navegador por pedido en curso: reintentar el guardado no duplica el pedido. Null = sin idempotencia.
        public Guid? ClientId { get; set; }

        public DateTime Fecha { get; set; }
        public int IdUsuario { get; set; }
        public string Usuario { get; set; }
        public string Nota { get; set; }

        // Suma de los importes de las lineas CON precio. Las lineas sin precio no suman (se cuentan en LineasSinPrecio).
        public decimal TotalEstimado { get; set; }
        public int LineasSinPrecio { get; set; }

        public DateTime? Creado { get; set; }
        public DateTime? Modificado { get; set; }
        public int? ModificadoPor { get; set; }

        public List<PedidoLinea> Lineas { get; set; } = new List<PedidoLinea>();
        public List<PedidoEvento> Eventos { get; set; } = new List<PedidoEvento>();
    }

    // Una cantidad a pedir de un producto para una sucursal, con el contexto de ese momento.
    public class PedidoLinea
    {
        public int Id { get; set; }
        public int IdCorte { get; set; }
        public int IdSucursal { get; set; }

        // Snapshot: nombre y codigo del producto y nombre de la sucursal al guardar (no se confia en lo que manda el navegador).
        public long Codigo { get; set; }
        public string Producto { get; set; }
        public string Sucursal { get; set; }

        public decimal Cantidad { get; set; }
        public bool Pesable { get; set; }
        // Kilos por unidad (solo pesables); permite mostrar el equivalente en unidades.
        public decimal? Promedio { get; set; }

        // Contexto al guardar (informativo).
        public decimal? StockActual { get; set; }
        public decimal? PuntoStock { get; set; }

        // Ultimo precio de compra del producto (por kg o unidad), el proveedor y la fecha de esa compra. Null = no habia precio.
        public decimal? PrecioUnitario { get; set; }
        public string Proveedor { get; set; }
        public DateTime? FechaPrecio { get; set; }

        // Cantidad x PrecioUnitario redondeado a 2 decimales; null si no habia precio.
        public decimal? Importe { get; set; }
    }

    // Historial append-only de un pedido: alta y modificaciones. Si quien modifica es un usuario de produccion, hace falta la
    // clave de otra persona: queda como autorizador.
    public class PedidoEvento
    {
        public const string TipoAlta = "ALTA";
        public const string TipoModificacion = "MODIFICACION";

        public int Id { get; set; }
        public int IdPedido { get; set; }
        public DateTime Fecha { get; set; }
        public string Tipo { get; set; }
        public int IdUsuario { get; set; }
        public string Usuario { get; set; }
        public int? IdAutorizador { get; set; }
        public string Autorizador { get; set; }
        public string Detalle { get; set; }
    }

    // Fila del listado de pedidos (sin cargar las lineas).
    public class PedidoResumen
    {
        public int Id { get; set; }
        public DateTime Fecha { get; set; }
        public string Usuario { get; set; }
        public string Nota { get; set; }
        // Nombres de las sucursales que cubre, separados por coma.
        public string Sucursales { get; set; }
        public int Productos { get; set; }
        public int Lineas { get; set; }
        public decimal Unidades { get; set; }
        public decimal TotalEstimado { get; set; }
        public int LineasSinPrecio { get; set; }
        public DateTime? Modificado { get; set; }
    }

    // Filtros del listado. Fechas inclusivas (el hasta incluye todo ese dia). Texto: busca en nota, usuario, producto y codigo.
    public class PedidoFiltro
    {
        public DateTime? Desde { get; set; }
        public DateTime? Hasta { get; set; }
        public int IdSucursal { get; set; }
        public string Usuario { get; set; }
        public string Texto { get; set; }
        // Tope de filas devueltas (el listado muestra lo mas reciente primero).
        public int Maximo { get; set; } = 500;
    }
}
