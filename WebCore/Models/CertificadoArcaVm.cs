// Datos de la pantalla Configuracion > Certificado ARCA (ver CertificadoArcaController).
// La pantalla tiene una tarjeta por entorno (produccion y pruebas/homologacion), cada una con su certificado.
using AFIP;

namespace WebCore.Models
{
    // Un entorno de AFIP para la empresa: su certificado (estado, vencimiento) y donde vive en el servidor.
    public class CertificadoEntornoVm
    {
        // "PROD" o "HOMO" (es lo que viaja en los formularios).
        public string Entorno { get; set; } = AfipEntorno.Produccion;
        public bool EsHomologacion => Entorno == AfipEntorno.Homologacion;

        // true = es el entorno con el que la empresa factura hoy (empresas.entorno_homo_prod).
        public bool EsActivo { get; set; }

        public InfoCertificado Info { get; set; } = new InfoCertificado();

        // Carpeta relativa donde se guarda (ej. AFIP/20123456789/homo), para el que copia un pfx a mano.
        public string Ruta { get; set; } = "";
    }

    public class CertificadoArcaVm
    {
        // false si el ambiente no puede guardar la clave del certificado (SQL Server): la pantalla lo avisa.
        public bool Soportado { get; set; }

        public long Cuit { get; set; }
        public string RazonSocial { get; set; } = "";

        // Entorno con el que la empresa factura hoy: "HOMO" (pruebas, sin validez fiscal) o "PROD" (comprobantes reales).
        public string Entorno { get; set; } = AfipEntorno.Produccion;
        public bool EsHomologacion => Entorno == AfipEntorno.Homologacion;

        // Siempre dos: produccion y homologacion.
        public System.Collections.Generic.List<CertificadoEntornoVm> Entornos { get; set; } = new System.Collections.Generic.List<CertificadoEntornoVm>();

        // Umbrales de aviso configurados (dias), para mostrarlos en la pantalla.
        public int[] DiasAviso { get; set; } = System.Array.Empty<int>();

        // Alias sugerido para el "computador fiscal" en ARCA (editable).
        public string AliasSugerido { get; set; } = "";
    }
}
