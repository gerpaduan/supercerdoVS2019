using WebCore.Helpers;

namespace WebCore.Models
{
    // Modelos de la ayuda / manual de uso (2026-10-03, ver docs/DECISIONS.md "Manuales por rol y
    // ayuda en cada pantalla"). Un unico parcial (_AyudaContenido) sirve para el panel y para las
    // paginas completas; "Modo" decide que se dibuja.
    public enum ModoAyuda
    {
        Documento,     // un .md renderizado (pantalla, concepto o referencia)
        Indice,        // lista del manual de un rol
        Busqueda,      // resultados de buscar un texto
        SinAyuda       // la pantalla actual todavia no tiene ayuda
    }

    public class AyudaItemVm
    {
        public string Titulo { get; set; } = "";
        public string Modulo { get; set; } = "";
        // Valor para data-ayuda="tipo/clave" (el panel lo abre adentro, sin navegar).
        public string Destino { get; set; } = "";
        public string Fragmento { get; set; } = "";
        public string TipoTexto { get; set; } = "";
    }

    public class AyudaContenidoVm
    {
        public ModoAyuda Modo { get; set; }
        public string Titulo { get; set; } = "";
        public string Html { get; set; } = "";
        // Para Documento: "Revisada el ..." (si el .md lo declara) y el rol al que pertenece.
        public string Revisada { get; set; } = "";
        public string NombreRol { get; set; } = "";
        // URL de la pagina completa de este contenido: la unica salida de la pantalla, y solo si el
        // usuario elige "Abrir en pestaña nueva" (target=_blank).
        public string UrlPagina { get; set; } = "";
        public List<AyudaItemVm> Items { get; set; } = new List<AyudaItemVm>();
        public string Consulta { get; set; } = "";
        // Manuales que el usuario puede abrir (rol -> nombre), para el indice general.
        public List<(string Rol, string Nombre)> ManualesDisponibles { get; set; } = new List<(string, string)>();
        public string Rol { get; set; } = "";
        // Pantalla pedida (para el aviso de SinAyuda y para el log).
        public string Pantalla { get; set; } = "";
    }

    // Pagina completa de un manual (pestaña nueva / imprimir): todos sus documentos de corrido.
    public class AyudaManualVm
    {
        public string Rol { get; set; } = "";
        public string Nombre { get; set; } = "";
        public List<AyudaManualSeccionVm> Secciones { get; set; } = new List<AyudaManualSeccionVm>();
    }

    public class AyudaManualSeccionVm
    {
        public string Anchor { get; set; } = "";
        public string Titulo { get; set; } = "";
        public string Modulo { get; set; } = "";
        public string Html { get; set; } = "";
        public string TipoTexto { get; set; } = "";
    }
}
