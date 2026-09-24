using System;

namespace Entidades
{
    // Notificacion al admin (campana del topbar), tabla notificaciones, solo Postgres. "Atendida" es
    // compartido entre admins: queda quien la reviso. (Idempresa, Tipo, RefId) es unico, por eso se
    // pueden crear de forma idempotente.
    public class Notificacion
    {
        public const string TipoVentaInterrumpida = "VENTA_INTERRUMPIDA";
        public const string TipoVentaDescartada = "VENTA_DESCARTADA";
        public const string TipoCierreCajaConVenta = "CIERRE_CAJA_CON_VENTA";
        public const string TipoProductoSinAgregar = "PRODUCTO_SIN_AGREGAR";

        // Pagos/cobros de Finanzas. RefId = id del pago (una notificacion por pago y tipo).
        public const string TipoPagoEliminado = "PAGO_ELIMINADO";
        public const string TipoPagoFechaDistinta = "PAGO_FECHA_DISTINTA";

        // Borrador generico descartado (Compras/Stock/Movimientos/Embutidos) -- un tipo por modulo para
        // que NotificacionesController.DetalleUrl() pueda enrutar el detalle sin parsear el mensaje.
        public const string PrefijoBorradorGenericoDescartado = "BORRADOR_DESCARTADO_";
        public const string TipoBorradorDescartadoCompra = PrefijoBorradorGenericoDescartado + "COMPRA";
        public const string TipoBorradorDescartadoStock = PrefijoBorradorGenericoDescartado + "STOCK";
        public const string TipoBorradorDescartadoMovimiento = PrefijoBorradorGenericoDescartado + "MOVIMIENTO";
        public const string TipoBorradorDescartadoEmbutidoCarga = PrefijoBorradorGenericoDescartado + "EMBUTIDO_CARGA";
        public const string TipoBorradorDescartadoEmbutidoRapido = PrefijoBorradorGenericoDescartado + "EMBUTIDO_RAPIDO";

        // Borrador generico interrumpido (deteccion perezosa, mismo criterio que TipoVentaInterrumpida):
        // se calcula cuando el admin consulta la campana, no hay proceso en segundo plano.
        public const string PrefijoBorradorGenericoInterrumpido = "BORRADOR_INTERRUMPIDO_";
        public const string TipoBorradorInterrumpidoCompra = PrefijoBorradorGenericoInterrumpido + "COMPRA";
        public const string TipoBorradorInterrumpidoStock = PrefijoBorradorGenericoInterrumpido + "STOCK";
        public const string TipoBorradorInterrumpidoMovimiento = PrefijoBorradorGenericoInterrumpido + "MOVIMIENTO";
        public const string TipoBorradorInterrumpidoEmbutidoCarga = PrefijoBorradorGenericoInterrumpido + "EMBUTIDO_CARGA";
        public const string TipoBorradorInterrumpidoEmbutidoRapido = PrefijoBorradorGenericoInterrumpido + "EMBUTIDO_RAPIDO";

        public const string SeveridadAdvertencia = "ADVERTENCIA";
        public const string SeveridadInfo = "INFO";

        public int Id { get; set; }
        public int IdEmpresa { get; set; }
        public int? IdSucursal { get; set; }
        public string Tipo { get; set; }
        public string Severidad { get; set; }
        public string Titulo { get; set; }
        public string Mensaje { get; set; }

        // Id de lo que la origina: el id del borrador, o la clave operador+dia en PRODUCTO_SIN_AGREGAR.
        public long RefId { get; set; }

        public DateTime Creado { get; set; }
        public DateTime? Actualizado { get; set; }
        public int? AtendidaPor { get; set; }
        public string NombreAtendidaPor { get; set; }
        public DateTime? AtendidaEn { get; set; }

        public bool EstaAtendida
        {
            get { return AtendidaEn.HasValue; }
        }

        // Clave de refid para PRODUCTO_SIN_AGREGAR: una sola notificacion por operador y dia.
        public static long RefIdOperadorDia(int idOperador, DateTime dia)
        {
            return (long)idOperador * 100000000L + (long)dia.Year * 10000L + dia.Month * 100L + dia.Day;
        }

        // Inversa de RefIdOperadorDia. false si el refid no tiene una fecha valida.
        public static bool TryDecodificarRefIdOperadorDia(long refId, out int idOperador, out DateTime dia)
        {
            idOperador = (int)(refId / 100000000L);
            long ymd = refId % 100000000L;
            int anio = (int)(ymd / 10000L);
            int mes = (int)((ymd / 100L) % 100L);
            int diaDelMes = (int)(ymd % 100L);
            dia = DateTime.MinValue;
            if (anio < 2000 || mes < 1 || mes > 12 || diaDelMes < 1 || diaDelMes > 31) return false;
            try
            {
                dia = new DateTime(anio, mes, diaDelMes);
                return true;
            }
            catch (ArgumentOutOfRangeException)
            {
                return false;
            }
        }
    }
}
