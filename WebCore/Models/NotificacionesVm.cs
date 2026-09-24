// ViewModels de las notificaciones/advertencias del admin (campana del topbar, detalle de una venta en
// curso y de las advertencias "producto pesado sin agregar"). Ver docs/DECISIONS.md "Ventas en curso".
namespace WebCore.Models
{
    // Detalle de una venta en curso (interrumpida, descartada o cortada por cierre de caja).
    public class DetalleBorradorVm
    {
        public Entidades.VentaBorrador Borrador { get; set; } = new Entidades.VentaBorrador();
        public List<WebCore.Services.BorradorPosPayload.Item> Items { get; set; } = new();
        public List<Entidades.VentaBorradorEvento> Eventos { get; set; } = new();

        // Texto del estado actual: "En curso", "Interrumpida (sin señal hace X)", "Finalizada como venta #N", "Descartada".
        public string EstadoTexto { get; set; } = "";

        // Notificacion asociada (para el boton "Marcar atendida"); null si no hay o ya se atendio.
        public Entidades.Notificacion? Notificacion { get; set; }
        public int MinutosSinLatido { get; set; }
    }

    // Detalle de un borrador de Compras/Stock/Movimientos/Embutidos (interrumpido o descartado). Sin
    // Items (a diferencia de DetalleBorradorVm): el payload tiene una forma distinta por modulo, no se
    // justifica un parser por modulo para una pantalla de auditoria poco usada -- se muestra el Resumen
    // guardado y el JSON crudo, de solo lectura, en la vista. Ver docs/DECISIONS.md "Borradores de
    // Compras/Stock/Movimientos/Embutidos".
    public class DetalleBorradorGenericoVm
    {
        public Entidades.BorradorGenerico Borrador { get; set; } = new Entidades.BorradorGenerico();
        public List<Entidades.BorradorGenericoEvento> Eventos { get; set; } = new();

        // Texto del estado actual: "En curso", "Interrumpido (sin señal hace X)", "Guardado como #N", "Descartado".
        public string EstadoTexto { get; set; } = "";

        // Notificacion asociada (para el boton "Marcar atendida"); null si no hay o ya se atendio.
        public Entidades.Notificacion? Notificacion { get; set; }
        public int MinutosSinLatido { get; set; }
    }

    // Advertencias "producto pesado sin agregar" de un operador en un dia, con el contexto de patron.
    public class DetalleProductoSinAgregarVm
    {
        public int IdOperador { get; set; }
        public string NombreOperador { get; set; } = "";
        public DateTime Dia { get; set; }
        public List<Entidades.ProductoSinAgregar> Eventos { get; set; } = new();

        // Contexto: cuantas advertencias tuvo el operador en los ultimos 7 y 30 dias (con importe).
        public Entidades.ResumenProductoSinAgregar Ultimos7Dias { get; set; } = new Entidades.ResumenProductoSinAgregar();
        public Entidades.ResumenProductoSinAgregar Ultimos30Dias { get; set; } = new Entidades.ResumenProductoSinAgregar();

        public Entidades.Notificacion? Notificacion { get; set; }

        // Valores reales de configuracion, para que el texto de ayuda nunca quede desactualizado.
        public int SegundosUmbral { get; set; }
        public int SegundosCantidadCero { get; set; }
    }

    // Una fila de la campana.
    public class NotificacionItemVm
    {
        public int Id { get; set; }
        public string Tipo { get; set; } = "";
        public string Severidad { get; set; } = "";
        public string Titulo { get; set; } = "";
        public string Mensaje { get; set; } = "";
        public string Fecha { get; set; } = "";
        public string DetalleUrl { get; set; } = "";
    }
}
