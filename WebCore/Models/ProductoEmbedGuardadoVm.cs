namespace WebCore.Models
{
    // Datos que Productos/EmbedGuardado le devuelve por postMessage a la pantalla que embebe el alta
    // completa de producto (hoy Compras/Editar).
    public class ProductoEmbedGuardadoVm
    {
        public int IdCorte { get; set; }
        public long Codigo { get; set; }
        public string Nombre { get; set; } = "";
    }
}
