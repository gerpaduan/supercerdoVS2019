namespace WebCore.Models
{
    public sealed class UtilityItemVm
    {
        // Identifica utilidades con interfaz propia en la vista (ej. "agente-balanza": boton Instalar/Actualizar).
        public string Codigo { get; set; }
        public string Nombre { get; set; }
        public string Categoria { get; set; }
        public string Descripcion { get; set; }
        public string Estado { get; set; }
        public string Version { get; set; }
        public string ArchivoUrl { get; set; }
        public string ArchivoNombre { get; set; }
        public string ArchivoTamano { get; set; }
        public string NotaInstalacion { get; set; }
        public bool Disponible { get; set; }
    }
}
