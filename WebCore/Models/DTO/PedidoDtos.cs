using System;
using System.Collections.Generic;

namespace WebCore.Models.DTO
{
    // Cuerpo JSON de Pedidos/Guardar y Pedidos/Modificar. Es input externo: Negocio.Pedido valida todo y toma nombres y codigos del
    // catalogo del servidor; de aca solo se usan ids, cantidades y el contexto informativo (precio de ultima compra, stock, punto).
    public class PedidoLineaDto
    {
        public int IdCorte { get; set; }
        public int IdSucursal { get; set; }
        public decimal Cantidad { get; set; }
        public decimal? PrecioUnitario { get; set; }
        public string Proveedor { get; set; }
        public DateTime? FechaPrecio { get; set; }
        public decimal? StockActual { get; set; }
        public decimal? PuntoStock { get; set; }
    }

    public class PedidoGuardarDto
    {
        // Alta: id por pedido en curso, para que reintentar el guardado no duplique.
        public Guid? ClientId { get; set; }
        public string Nota { get; set; }
        // Modificacion: version del pedido que se estaba editando (control optimista).
        public long? Version { get; set; }
        // Cuenta de produccion: persona elegida (mismo criterio que Stock/Movimientos: ResolverUsuarioCreador). 0 = el usuario de la sesion.
        public int IdUsuarioCreador { get; set; }
        public List<PedidoLineaDto> Lineas { get; set; } = new List<PedidoLineaDto>();
        // Modificacion de un usuario de produccion: usuario y clave de otra persona autorizada.
        public SupervisorAutorizacionDto Supervisor { get; set; }
    }
}
