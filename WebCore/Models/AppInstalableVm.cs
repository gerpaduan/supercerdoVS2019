namespace WebCore.Models
{
    // Datos de la pagina anfitriona de instalacion de una mini-app (Views/InstalarApp/Index.cshtml).
    public sealed class AppInstalableVm
    {
        public string Nombre { get; set; }            // nombre que ve el usuario: "Punto de venta", "Stock"...
        public string ArchivoManifest { get; set; }    // archivo en wwwroot, ej. "manifest-pos.json"
        public string ArchivoIcono { get; set; }       // PNG de 192 px en Content/img/ventanas, ej. "pos-192.png"
    }
}
