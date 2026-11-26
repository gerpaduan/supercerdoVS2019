// Pedidos que manda el POS sin conexion al subir lo que hizo mientras no habia red (2026-11-26). Ver
// VentasController.PosOffline.cs y docs/03-modulos/ventas-y-pos.md "POS sin conexion".
using System;
using System.Collections.Generic;

namespace WebCore.Models.DTO
{
    // Una venta hecha sin conexion. Todo lo que trae es dato del navegador: el servidor valida (Negocio.VentaOffline),
    // re-resuelve cada producto por codigo y nunca confia en la sucursal ni en el vendedor sin compararlos con la sesion.
    public class SincronizarVentaOfflineRequest
    {
        // Codigo unico de la venta generado en la PC: hace idempotente la subida.
        public Guid ClientId { get; set; }

        // Pestana de POS (para resolver el operador con la cuenta compartida de produccion).
        public string? PosInstanceId { get; set; }

        // Vendedor que entro con PIN sin conexion. Tiene que ser el operador de la sesion que sube.
        public int IdVendedor { get; set; }

        public int IdSucursalPOS { get; set; }
        public int IdPersona { get; set; }

        // Hora de la venta en la PC (la venta se graba con esta fecha).
        public DateTime FechaLocal { get; set; }

        public string? FormaPago { get; set; }
        public bool EsPagoMixto { get; set; }
        public decimal Efectivo { get; set; }
        public string? Observaciones { get; set; }

        // 'sinred', 'sininternet' o 'servidor' (ver pos-offline-red.js).
        public string? CausaCorte { get; set; }

        // Version del catalogo con la que se vendio (para revisar diferencias de precio).
        public string? VersionCatalogo { get; set; }

        public List<LineaVentaOfflineDto>? Lineas { get; set; }
    }

    public class LineaVentaOfflineDto
    {
        public long Codigo { get; set; }
        public decimal CantKg { get; set; }
        public decimal PrecioKg { get; set; }
        public decimal Bonificacion { get; set; }
        public bool Balanza { get; set; }

        // Linea anulada en el carrito (misma convencion que LineaVentaDto.Estado / IndexAnulado del POS online).
        public int Estado { get; set; }
        public int IndexAnulado { get; set; } = -1;

        // De donde salio el precio: 'catalogo' (lista con factor de forma de pago), 'etiqueta' (precio impreso en la
        // etiqueta de balanza) o 'generico' (codigo generico con precio cargado a mano). Solo 'catalogo' se compara
        // contra la lista del servidor.
        public string? OrigenPrecio { get; set; }
    }

    // Apertura de caja hecha sin conexion.
    public class SincronizarAperturaCajaOfflineRequest
    {
        public Guid ClientId { get; set; }
        public string? PosInstanceId { get; set; }
        public int IdVendedor { get; set; }
        public int IdSucursalPOS { get; set; }

        // Hora real de la apertura en la PC y monto inicial.
        public DateTime FechaLocal { get; set; }
        public decimal CajaInicio { get; set; }
        public string? CausaCorte { get; set; }
    }
}
