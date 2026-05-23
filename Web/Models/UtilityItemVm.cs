namespace Web.Models
{
    public sealed class UtilityItemVm
    {
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
