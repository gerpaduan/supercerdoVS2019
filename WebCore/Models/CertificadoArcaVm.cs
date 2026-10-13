// Datos de la pantalla Configuracion > Certificado ARCA (ver CertificadoArcaController).
// La pantalla tiene una tarjeta por entorno (produccion y pruebas/homologacion), cada una con su certificado, y
// (titulares fiscales, 2026-10-09) se opera sobre UN titular: el principal (el CUIT de la empresa) o uno de los demas.
using AFIP;

namespace WebCore.Models
{
    // Un entorno de AFIP para el titular elegido: su certificado (estado, vencimiento) y donde vive en el servidor.
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

    // Titular de la lista del selector de la pantalla.
    public class CertificadoTitularOpcionVm
    {
        public int IdTitular { get; set; }
        public string Nombre { get; set; } = "";
        public long Cuit { get; set; }
        public bool EsPrincipal { get; set; }
        public bool Habilitado { get; set; }
    }

    public class CertificadoArcaVm
    {
        // false si el ambiente no puede guardar la clave del certificado (SQL Server): la pantalla lo avisa.
        public bool Soportado { get; set; }

        // Titular sobre el que se opera (0 = el principal / la empresa) y los titulares entre los que se puede elegir.
        public int IdTitular { get; set; }
        public bool EsPrincipal { get; set; } = true;
        public System.Collections.Generic.List<CertificadoTitularOpcionVm> Titulares { get; set; } = new System.Collections.Generic.List<CertificadoTitularOpcionVm>();

        public long Cuit { get; set; }
        public string RazonSocial { get; set; } = "";

        // Entorno con el que la empresa factura hoy: "HOMO" (pruebas, sin validez fiscal) o "PROD" (comprobantes reales).
        // Es de la EMPRESA (lo cambia el super-admin): todos sus titulares emiten por el mismo entorno.
        public string Entorno { get; set; } = AfipEntorno.Produccion;
        public bool EsHomologacion => Entorno == AfipEntorno.Homologacion;

        // Siempre dos: produccion y homologacion.
        public System.Collections.Generic.List<CertificadoEntornoVm> Entornos { get; set; } = new System.Collections.Generic.List<CertificadoEntornoVm>();

        // Umbrales de aviso configurados (dias), para mostrarlos en la pantalla.
        public int[] DiasAviso { get; set; } = System.Array.Empty<int>();

        // Alias sugerido para el "computador fiscal" en ARCA (editable).
        public string AliasSugerido { get; set; } = "";

        // Titular compartido entre empresas (2026-10-12): el certificado de un CUIT lo administra UNA sola empresa (la que lo
        // tiene como principal); las demas solo ven su estado y lo usan. PuedeAdministrar = pedir, instalar o descartar.
        public bool PuedeAdministrar { get; set; } = true;
        public string EmpresaAdministradora { get; set; } = "";
        // true si otras empresas usan este mismo CUIT (se avisa al renovar: el cambio les llega a todas).
        public bool ComparteCertificado { get; set; }
        public string EmpresasQueLoComparten { get; set; } = "";
    }
}
