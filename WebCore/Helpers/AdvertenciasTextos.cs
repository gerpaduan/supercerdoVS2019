// Textos legibles (para el admin) de los codigos internos de las advertencias del POS: motivo por el
// que salio un producto sin agregarse, origen de la cantidad y tipos de evento de una venta en curso.
// Un solo lugar para que Actividades, la campana y el detalle digan lo mismo.
namespace WebCore.Helpers
{
    public static class AdvertenciasTextos
    {
        public static string Motivo(string motivo)
        {
            switch (motivo)
            {
                case Entidades.ProductoSinAgregar.MotivoCodigoBorrado: return "se borró el código";
                case Entidades.ProductoSinAgregar.MotivoCodigoCambiado: return "se cambió por otro código";
                case Entidades.ProductoSinAgregar.MotivoCantidadCero: return "la cantidad volvió a 0";
                case Entidades.ProductoSinAgregar.MotivoFinalizar: return "se apretó Finalizar sin agregarlo";
                case Entidades.ProductoSinAgregar.MotivoCierrePestana: return "se cerró la pestaña";
                default: return motivo ?? "";
            }
        }

        public static string Origen(string origen)
        {
            return origen == Entidades.ProductoSinAgregar.OrigenBalanza ? "Balanza" : "Manual";
        }

        public static string Revision(string revision)
        {
            switch (revision)
            {
                case Entidades.ProductoSinAgregar.RevisionJustificada: return "Justificada";
                case Entidades.ProductoSinAgregar.RevisionSospechosa: return "Sospechosa";
                default: return "Sin revisar";
            }
        }

        // Nombre del tipo de evento de una venta en curso, para Actividades y la linea de tiempo.
        public static string TipoEvento(string tipo)
        {
            switch (tipo)
            {
                case Entidades.VentaBorradorEvento.TipoCierrePestana: return "Cierre de pestaña con venta en curso";
                case Entidades.VentaBorradorEvento.TipoLogout: return "Sesión cerrada con venta en curso";
                case Entidades.VentaBorradorEvento.TipoCierreCaja: return "Cierre de caja con venta en curso";
                case Entidades.VentaBorradorEvento.TipoRecuperada: return "Venta en curso recuperada";
                case Entidades.VentaBorradorEvento.TipoDescartada: return "Venta en curso descartada";
                default: return tipo ?? "";
            }
        }

        // Nombre del tipo de evento de un borrador de Compras/Stock/Movimientos/Embutidos, para la linea
        // de tiempo de su detalle. Mismos valores de string que TipoEvento (CIERRE_PESTANA/LOGOUT/
        // RECUPERADA/DESCARTADA, sin CIERRE_CAJA: estos modulos no tienen caja), texto distinto porque no
        // es "una venta".
        public static string TipoEventoGenerico(string tipo)
        {
            switch (tipo)
            {
                case Entidades.BorradorGenericoEvento.TipoCierrePestana: return "Cierre de pestaña con el formulario sin guardar";
                case Entidades.BorradorGenericoEvento.TipoLogout: return "Sesión cerrada con el formulario sin guardar";
                case Entidades.BorradorGenericoEvento.TipoRecuperada: return "Borrador recuperado";
                case Entidades.BorradorGenericoEvento.TipoDescartada: return "Borrador descartado";
                default: return tipo ?? "";
            }
        }
    }
}
