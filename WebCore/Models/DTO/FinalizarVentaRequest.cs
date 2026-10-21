// Port de Web/Models/DTO/FinalizarVentaRequest.cs -- ver docs/10-migracion-aspnet-core/PLAN-POS.md.
// FechaVenta/PosInstanceId agregados 2026-09-06 (Batch 5 del plan de login/permisos reales, ver
// docs/DECISIONS.md): el cliente (forma-pago.js) ya los mandaba desde antes de esta migracion --
// llegaban al servidor y se descartaban en silencio por no existir en el DTO. PosInstanceId
// identifica la pestaña de POS para ResolverOperadorPOS (usuario de produccion); FechaVenta
// habilita la edicion de fecha de venta real (PuedeEditarFechaVenta).
using System;
using System.Collections.Generic;

namespace WebCore.Models.DTO
{
    public class FinalizarVentaRequest
    {
        public int IdVenta { get; set; }
        public DateTime? FechaVenta { get; set; }
        public string FormaPago { get; set; }
        public bool EsPagoMixto { get; set; }
        public float Efectivo { get; set; }
        public int IdPersona { get; set; }
        public int IdSucursalPOS { get; set; }
        public bool SoloFormaPago { get; set; }
        public string Observaciones { get; set; }
        public List<LineaVentaDto> LineasVenta { get; set; }
        public List<int> ListaExpendios { get; set; }
        public string PosInstanceId { get; set; }

        // Token del carrito (ventas en curso, ver VentasController.Borradores.cs). Opcional: los clientes
        // viejos no lo mandan. Con el, FinalizarVenta es idempotente (un reintento no duplica la venta) y
        // marca la venta en curso como FINALIZADA.
        public Guid? ClientId { get; set; }

        // Confirmacion del usuario para modificar una venta que cae en una caja YA CERRADA (2026-10-06, ver
        // docs/DECISIONS.md "Cambios en cajas cerradas y reapertura"). ModificarVenta responde requiereConfirmacion
        // si hace falta y el cliente reenvia el mismo request con esto en true (caja-cerrada-confirm.js).
        public bool ConfirmarCajaCerrada { get; set; }

        // Cobro que el cliente paga en el momento, junto con la venta (2026-10-21, ver docs/DECISIONS.md "Cobrar el
        // saldo del cliente al finalizar la venta"). Opcional: sin esto la venta se registra como siempre. Con esto la
        // venta se fuerza a cuenta corriente (FormaPago/EsPagoMixto/Efectivo de arriba se ignoran) y la forma de pago
        // elegida pasa a ser la del cobro.
        public CobroSaldoDto CobroSaldo { get; set; }
    }

    // Datos del cobro junto con la venta. El servidor valida todo (Negocio.CobroJuntoVenta) y, con PagaTodo,
    // calcula el importe con el saldo real del cliente: el navegador nunca decide cuanto se cobra de mas.
    public class CobroSaldoDto
    {
        // Forma del cobro; en pago mixto, el "otro medio" (el efectivo va aparte, en Efectivo).
        public string FormaPago { get; set; }
        public bool EsMixto { get; set; }
        public decimal Efectivo { get; set; }

        // Importe que paga el cliente (ignorado si PagaTodo).
        public decimal Importe { get; set; }

        // "Paga todo": se cobra lo que el cliente debe despues de esta venta, calculado por el servidor.
        public bool PagaTodo { get; set; }
    }
}
