// Datos de la pantalla Configuracion > Certificado ARCA (ver CertificadoArcaController).
using AFIP;

namespace WebCore.Models
{
    public class CertificadoArcaVm
    {
        // false si el ambiente no puede guardar la clave del certificado (SQL Server): la pantalla lo avisa.
        public bool Soportado { get; set; }

        public long Cuit { get; set; }
        public string RazonSocial { get; set; } = "";

        // "HOMO" (pruebas, sin validez fiscal) o "PROD" (comprobantes reales).
        public string Entorno { get; set; } = AfipEntorno.Produccion;
        public bool EsHomologacion => Entorno == AfipEntorno.Homologacion;

        public InfoCertificado Info { get; set; } = new InfoCertificado();

        // Umbrales de aviso configurados (dias), para mostrarlos en la pantalla.
        public int[] DiasAviso { get; set; } = System.Array.Empty<int>();

        // Alias sugerido para el "computador fiscal" en ARCA (editable).
        public string AliasSugerido { get; set; } = "";

        // Aviso cuando el nombre del archivo no parece corresponder al entorno (ej. HOMO con certif-prod.pfx).
        public string AdvertenciaEntorno { get; set; } = "";
    }
}
