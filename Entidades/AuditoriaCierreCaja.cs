using System;

namespace Entidades
{
    // Registro append-only sobre un cierre de caja (tabla auditoriacierrecaja, solo Postgres):
    //  - MODIFICACION: se dio de alta / modifico / elimino una venta, compra, pago o egreso que cae DENTRO de una
    //    caja ya cerrada (IdCierreCaja). Deja el cierre guardado desactualizado; el historial lo muestra como aviso.
    //  - REAPERTURA: el encargado reabrio la ultima caja cerrada de un usuario (Detalle = snapshot de lo que se deshizo + motivo).
    // Ver docs/DECISIONS.md (2026-10-06, "Cambios en cajas cerradas y reapertura").
    public class AuditoriaCierreCaja
    {
        public const string TipoModificacion = "MODIFICACION";
        public const string TipoReapertura = "REAPERTURA";

        public const string OrigenVenta = "VENTA";
        public const string OrigenCompra = "COMPRA";
        public const string OrigenPago = "PAGO";
        public const string OrigenEgreso = "EGRESO";

        public const string AccionAlta = "ALTA";
        public const string AccionModificacion = "MODIFICACION";
        public const string AccionEliminacion = "ELIMINACION";

        public int Id { get; set; }
        public int IdCierreCaja { get; set; }
        public string Tipo { get; set; }
        public string Origen { get; set; }
        public int? IdOrigen { get; set; }
        public string Accion { get; set; }
        public int IdUsuario { get; set; }
        public string Usuario { get; set; }
        public DateTime Fecha { get; set; }
        public double? ImporteAnterior { get; set; }
        public double? ImporteNuevo { get; set; }
        public string FormaPagoAnterior { get; set; }
        public string FormaPagoNueva { get; set; }
        public string Detalle { get; set; }
    }
}
