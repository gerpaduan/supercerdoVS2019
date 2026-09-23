using System;

namespace Entidades
{
    // Venta en curso del POS guardada en el servidor mientras el cajero la arma (tabla ventaborrador,
    // solo Postgres -- ver docs/DECISIONS.md "Ventas en curso"). NO es una venta: vive aparte de
    // ventas/lineaventa a proposito (esas tablas alimentan totales, cierre de caja y stock). Las lineas
    // viajan en Payload (JSON con la misma forma que POSDraft del navegador).
    public class VentaBorrador
    {
        public const string EstadoActiva = "ACTIVA";
        public const string EstadoFinalizada = "FINALIZADA";
        public const string EstadoDescartada = "DESCARTADA";

        public int Id { get; set; }
        public int IdEmpresa { get; set; }
        public int IdSucursal { get; set; }

        // Operador real de la venta (para la cuenta compartida de produccion es quien se autorizo con
        // su clave); IdUsuarioSesion es la cuenta con la que se inicio sesion.
        public int IdOperador { get; set; }
        public string NombreOperador { get; set; }
        public int IdUsuarioSesion { get; set; }

        // Token del carrito generado por el navegador: identifica esta venta en curso aunque cambie
        // la pestana o el posInstanceId.
        public Guid ClientId { get; set; }
        public string PosInstanceId { get; set; }
        public int? IdCierreCaja { get; set; }
        public int? IdPersona { get; set; }
        public string RazonSocial { get; set; }
        public int CantLineas { get; set; }
        public decimal Total { get; set; }

        // JSON del carrito (lineas, expendios, forma de pago, cliente, observaciones, fecha). Se vacia
        // al finalizar/descartar.
        public string Payload { get; set; }

        public string Estado { get; set; }
        public int? IdVenta { get; set; }
        public DateTime Creado { get; set; }
        public DateTime UltimoLatido { get; set; }
        public DateTime? Actualizado { get; set; }
        public DateTime? Finalizado { get; set; }

        // Segundos desde el ultimo latido segun el reloj de la BASE (now() - ultimolatido). Se calcula en
        // la consulta para no depender de que el reloj del servidor web coincida con el de la base.
        public int SegundosSinLatido { get; set; }

        // "Interrumpida" no se guarda: es una venta ACTIVA cuyo ultimo latido es mas viejo que el
        // umbral (minutos). Asi no hace falta un proceso en segundo plano que la marque.
        public bool EstaInterrumpida(int minutosSinLatido)
        {
            return Estado == EstadoActiva && SegundosSinLatido >= minutosSinLatido * 60;
        }
    }

    // Resultado de guardar (upsert) un borrador desde el POS.
    public enum ResultadoGuardarBorrador
    {
        Guardado = 0,

        // Ya estaba FINALIZADA o DESCARTADA: el navegador debe abandonar ese carrito.
        YaCerrado = 1,

        // El clientId existe pero pertenece a otro operador o sucursal: no se toca.
        Ajeno = 2
    }

    // Evento de una venta en curso (append-only, tabla ventaborradorevento).
    public class VentaBorradorEvento
    {
        public const string TipoCierrePestana = "CIERRE_PESTANA";
        public const string TipoLogout = "LOGOUT";
        public const string TipoCierreCaja = "CIERRE_CAJA";
        public const string TipoRecuperada = "RECUPERADA";
        public const string TipoDescartada = "DESCARTADA";

        public int Id { get; set; }
        public int IdEmpresa { get; set; }
        public int IdBorrador { get; set; }
        public DateTime Fecha { get; set; }
        public string Tipo { get; set; }

        // Quien hizo la accion (en RECUPERADA/DESCARTADA puede ser distinto del dueno del borrador).
        public int IdUsuario { get; set; }
        public string NombreUsuario { get; set; }
        public string Detalle { get; set; }

        // Datos de la venta en curso a la que pertenece el evento (los completa la consulta con un JOIN;
        // sirven para describir el evento en Actividades sin una consulta por fila).
        public string NombreOperador { get; set; }
        public decimal Total { get; set; }
        public int CantLineas { get; set; }
    }
}
