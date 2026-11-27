// Reglas de las ventas hechas sin conexion en el POS y subidas despues (2026-11-26). Logica pura, sin base ni HTTP:
// la usa VentasController.SincronizarVentaOffline y se prueba con Negocio.Tests/VentaOfflineTests.cs.
// Detalle funcional en docs/03-modulos/ventas-y-pos.md "POS sin conexion".
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Negocio
{
    public static class VentaOffline
    {
        // Formas de pago que se pueden elegir sin conexion: todas las del POS menos Cuenta Corriente (sin conexion la
        // venta es siempre a Consumidor Final, que no tiene cuenta corriente). Decision del usuario 2026-11-17.
        public static readonly IReadOnlyCollection<string> FormasPagoPermitidas = new[]
        {
            Entidades.Venta.formaPagoEnum.Efectivo.ToString(),
            Entidades.Venta.formaPagoEnum.Debito.ToString(),
            Entidades.Venta.formaPagoEnum.Credito.ToString(),
            Entidades.Venta.formaPagoEnum.Qr.ToString(),
            Entidades.Venta.formaPagoEnum.Transferencia.ToString()
        };

        // Causas de corte que informa el POS (ver pos-offline-red.js). Otra cosa se guarda como null.
        public static readonly IReadOnlyCollection<string> CausasCorteValidas = new[] { "sinred", "sininternet", "servidor" };

        // Diferencia de precio por kg (en pesos) a partir de la cual se marca difprecio. Un centavo cubre el redondeo
        // de float entre el navegador y el servidor.
        public const decimal ToleranciaDiferenciaPrecio = 0.01m;

        // Linea tal como la valida esta clase (independiente del DTO web).
        public sealed class LineaPedido
        {
            public long Codigo { get; set; }
            public decimal CantKg { get; set; }
            public decimal PrecioKg { get; set; }
            public decimal Bonificacion { get; set; }
        }

        // Datos minimos de la venta offline para validarla.
        public sealed class Pedido
        {
            public int IdPersona { get; set; }
            public string FormaPago { get; set; }
            public bool EsPagoMixto { get; set; }
            public decimal EfectivoMixto { get; set; }
            public DateTime FechaLocal { get; set; }
            public IList<LineaPedido> Lineas { get; set; }
        }

        // Limites configurables (PosOfflineSettings) que aplica Validar.
        public sealed class Limites
        {
            public int IdConsumidorFinal { get; set; }
            public int MaxLineas { get; set; }

            // Minutos que la hora de la PC puede adelantar a la del servidor (relojes desfasados).
            public int MinutosToleranciaFuturo { get; set; }

            // Antiguedad maxima de una venta offline al llegar. Mas vieja se rechaza: un admin la revisa.
            public int DiasMaximosAntiguedad { get; set; }
        }

        public enum EstadoCaja
        {
            // Hay una caja abierta del vendedor que empezo antes de la venta: la venta cae ahi.
            Abierta,
            // La caja del turno ya se cerro: la venta cae igual en ella (por fecha) y se marca "llego tarde".
            LlegoTarde,
            // Ninguna caja del vendedor contiene la fecha de la venta: se acepta (ya se cobro) y se marca.
            SinCaja
        }

        // Devuelve el motivo de rechazo, o null si la venta se puede grabar. Rechazar deja la venta en la cola de la PC
        // marcada para revisar: nunca se pierde.
        public static string Validar(Pedido pedido, Limites limites, DateTime ahora)
        {
            if (pedido == null) return "La venta llegó vacía.";
            if (limites == null) throw new ArgumentNullException(nameof(limites));

            if (pedido.Lineas == null || pedido.Lineas.Count == 0)
                return "La venta no tiene productos.";
            if (limites.MaxLineas > 0 && pedido.Lineas.Count > limites.MaxLineas)
                return "La venta tiene demasiadas líneas (" + pedido.Lineas.Count + ", máximo " + limites.MaxLineas + ").";

            // Sin conexion solo se vende a Consumidor Final (no hay historial ni cuenta corriente del cliente).
            if (limites.IdConsumidorFinal <= 0 || pedido.IdPersona != limites.IdConsumidorFinal)
                return "Sin conexión solo se puede vender a Consumidor Final.";

            if (string.IsNullOrWhiteSpace(pedido.FormaPago)
                || !FormasPagoPermitidas.Contains(pedido.FormaPago, StringComparer.OrdinalIgnoreCase))
                return "Forma de pago no permitida sin conexión: " + (pedido.FormaPago ?? "(vacía)") + ".";

            foreach (var linea in pedido.Lineas)
            {
                if (linea == null) return "La venta tiene una línea vacía.";
                if (linea.Codigo <= 0) return "La venta tiene una línea sin código de producto.";
                if (linea.CantKg == 0) return "El producto " + linea.Codigo + " tiene cantidad 0.";
                if (linea.PrecioKg < 0) return "El producto " + linea.Codigo + " tiene precio negativo.";
                // Sin conexion no hay descuentos (decision del usuario 2026-11-17).
                if (linea.Bonificacion != 0) return "Sin conexión no se pueden aplicar descuentos (producto " + linea.Codigo + ").";
            }

            if (pedido.EsPagoMixto && pedido.EfectivoMixto < 0)
                return "El efectivo del pago mixto no puede ser negativo.";

            if (pedido.FechaLocal == default(DateTime))
                return "La venta no tiene fecha.";
            if (pedido.FechaLocal > ahora.AddMinutes(Math.Max(0, limites.MinutosToleranciaFuturo)))
                return "La fecha de la venta (" + pedido.FechaLocal.ToString("dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture) +
                       ") es posterior a la hora del servidor: revisá la hora de la PC.";
            if (limites.DiasMaximosAntiguedad > 0 && pedido.FechaLocal < ahora.AddDays(-limites.DiasMaximosAntiguedad))
                return "La venta tiene más de " + limites.DiasMaximosAntiguedad + " días: la tiene que revisar un administrador.";

            return null;
        }

        // En que caja cae la venta, por fecha. cajaAbierta: la caja abierta del vendedor en la sucursal (o null).
        // cajaCerradaQueContiene: la caja ya cerrada del vendedor cuyo rango contiene la fecha (o null; en SQL Server
        // siempre null porque esa busqueda solo existe en Postgres, y la venta queda como SinCaja).
        public static EstadoCaja ClasificarCaja(DateTime fechaVenta, Entidades.CierreCaja cajaAbierta, Entidades.CierreCaja cajaCerradaQueContiene)
        {
            if (cajaAbierta != null && cajaAbierta.FechaHoraInicio.HasValue && cajaAbierta.FechaHoraInicio.Value <= fechaVenta)
                return EstadoCaja.Abierta;
            if (cajaCerradaQueContiene != null)
                return EstadoCaja.LlegoTarde;
            return EstadoCaja.SinCaja;
        }

        // true si la forma de pago pedia factura automatica (parametro formasPagoFacturaAutomatica) y la empresa factura:
        // sin conexion no se pudo emitir, queda "pendiente de facturar".
        public static bool QuedaPendienteDeFacturar(string formaPago, IEnumerable<string> formasFacturaAutomatica, bool empresaFactura)
        {
            if (!empresaFactura || string.IsNullOrWhiteSpace(formaPago) || formasFacturaAutomatica == null)
                return false;
            return formasFacturaAutomatica.Any(f => string.Equals(f, formaPago, StringComparison.OrdinalIgnoreCase));
        }

        // Precio cobrado vs precios vigentes del servidor. Devuelve el detalle de las diferencias (vacio si no hay). No
        // rechaza: el cliente ya pago lo cobrado; solo queda marcado para revisar.
        // preciosAceptables: codigo -> precios por kg validos hoy (el de lista por cada factor de forma de pago, porque en
        // pago mixto o con la forma cambiada a ultimo momento el POS pudo usar cualquiera). Un codigo ausente se informa.
        // Las lineas con precio de etiqueta de balanza o de codigo generico no se comparan (lineasAComparar las excluye).
        public static string DetalleDiferenciasPrecio(IEnumerable<LineaPedido> lineasAComparar, IDictionary<long, IReadOnlyCollection<decimal>> preciosAceptables)
        {
            if (lineasAComparar == null) return "";
            var sb = new StringBuilder();
            foreach (var linea in lineasAComparar)
            {
                if (linea == null) continue;
                if (preciosAceptables == null || !preciosAceptables.TryGetValue(linea.Codigo, out IReadOnlyCollection<decimal> validos)
                    || validos == null || validos.Count == 0)
                {
                    Agregar(sb, linea.Codigo + ": sin precio en el servidor");
                    continue;
                }
                if (!validos.Any(v => Math.Abs(linea.PrecioKg - v) <= ToleranciaDiferenciaPrecio))
                    Agregar(sb, linea.Codigo + ": cobrado " + linea.PrecioKg.ToString("0.00", CultureInfo.InvariantCulture) +
                                " / lista " + validos.Min().ToString("0.00", CultureInfo.InvariantCulture));
            }
            return sb.ToString();
        }

        // Motivo por el que una venta ya cobrada tiene que revisarse por su vendedor (null = nada que revisar). La venta se
        // carga igual (la plata ya se cobro), queda marcada y se avisa al administrador (decision del usuario 2026-11-26).
        public static string MotivoRevisarVendedor(bool activo, bool bloqueado, bool tienePermisoPos)
        {
            if (!activo) return "el vendedor está dado de baja";
            if (bloqueado) return "el vendedor está bloqueado";
            if (!tienePermisoPos) return "el vendedor ya no tiene permisos de POS";
            return null;
        }

        // Normaliza la causa del corte informada por el navegador (texto libre de un cliente: se acota a la lista).
        public static string NormalizarCausaCorte(string causa)
        {
            if (string.IsNullOrWhiteSpace(causa)) return null;
            string c = causa.Trim().ToLowerInvariant();
            return CausasCorteValidas.Contains(c) ? c : null;
        }

        private static void Agregar(StringBuilder sb, string texto)
        {
            if (sb.Length > 0) sb.Append("; ");
            sb.Append(texto);
        }
    }
}
