// Clave unica de una operacion del POS generada en el navegador (venta o apertura de caja), con los datos de la
// subida offline. Tabla posclaveunica / PosClaveUnica. Ver docs/03-modulos/ventas-y-pos.md "POS sin conexion".
using System;

namespace Entidades
{
    public class PosClaveUnica
    {
        public const string TipoVenta = "venta";
        public const string TipoCaja = "caja";
        public const string OrigenOnline = "online";
        public const string OrigenOffline = "offline";

        public int Id { get; set; }
        public Guid ClientId { get; set; }

        // TipoVenta o TipoCaja.
        public string Tipo { get; set; }

        // ventas.idventa (TipoVenta) o cierrecaja.id (TipoCaja).
        public int IdRegistro { get; set; }

        // OrigenOnline u OrigenOffline.
        public string Origen { get; set; }

        public int IdSucursal { get; set; }

        // Vendedor de la operacion (con la cuenta de produccion, el operador).
        public int IdUsuario { get; set; }

        // Usuario de la sesion que la subio al servidor (con la cuenta de produccion, la cuenta compartida).
        public int IdUsuarioSync { get; set; }

        // Hora de la operacion en la PC del POS.
        public DateTime FechaLocal { get; set; }

        // Hora en que llego al servidor (la pone la base).
        public DateTime Recibido { get; set; }

        public bool LlegoTarde { get; set; }
        public bool SinCaja { get; set; }
        public bool PendienteFacturar { get; set; }
        public bool DifPrecio { get; set; }
        public string CausaCorte { get; set; }
        public string VersionCatalogo { get; set; }
        public string Detalle { get; set; }
    }
}
