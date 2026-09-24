// Rutas en disco del certificado y los tickets de acceso (WSAA) de una empresa. Un solo lugar para
// la regla "<base>/AFIP/<CUIT>/<archivo>" que antes estaba repetida en GenerarFacturaService y
// ConsultarPadronService. Los nombres de produccion son los historicos (no cambian); homologacion
// usa tickets propios para que un ticket de un entorno nunca se reuse contra el otro.
using System;
using System.IO;

namespace AFIP
{
    public static class AfipRutas
    {
        public const string NombreCertificadoPorDefecto = "certif-prod.pfx";
        public const string TicketFactura = "TicketAcceso.txt";
        public const string TicketPadron = "TicketAccesoPerson.txt";
        public const string TemplateLogin = "LoginTemplate.xml";
        public const string CarpetaClavePendiente = "pendiente";

        // <base>/AFIP/<cuit>. baseDirectory vacio = AppDomain.CurrentDomain.BaseDirectory (Web clasico).
        public static string Carpeta(string baseDirectory, string cuit)
        {
            if (string.IsNullOrWhiteSpace(cuit) || !EsSoloDigitos(cuit))
                throw new ArgumentException("CUIT inválido para resolver la carpeta AFIP.", nameof(cuit));

            string raiz = string.IsNullOrWhiteSpace(baseDirectory)
                ? AppDomain.CurrentDomain.BaseDirectory
                : baseDirectory;

            return Path.Combine(raiz, "AFIP", cuit);
        }

        // Carpeta del certificado de la PLATAFORMA (el del padron compartido por todas las empresas):
        // <base>/AFIP/_plataforma. El guion bajo inicial la distingue de las carpetas por CUIT (solo digitos),
        // asi ningun CUIT puede colisionar con ella.
        public const string NombreCarpetaPlataforma = "_plataforma";
        public const string NombreCertificadoPlataforma = "padron-plataforma.pfx";

        public static string CarpetaPlataforma(string baseDirectory)
        {
            string raiz = string.IsNullOrWhiteSpace(baseDirectory)
                ? AppDomain.CurrentDomain.BaseDirectory
                : baseDirectory;
            return Path.Combine(raiz, "AFIP", NombreCarpetaPlataforma);
        }

        // Nombre del pfx: el configurado en la empresa, o el historico si esta vacio. Se descarta
        // cualquier ruta (solo el nombre de archivo): el valor viene de la base y no debe poder salir
        // de la carpeta del CUIT.
        public static string NombreCertificado(string nombreCertificadoPfx)
        {
            if (string.IsNullOrWhiteSpace(nombreCertificadoPfx))
                return NombreCertificadoPorDefecto;
            string soloNombre = Path.GetFileName(nombreCertificadoPfx.Trim());
            return string.IsNullOrWhiteSpace(soloNombre) ? NombreCertificadoPorDefecto : soloNombre;
        }

        public static string Certificado(string carpeta, string nombreCertificadoPfx)
        {
            return Path.Combine(carpeta, NombreCertificado(nombreCertificadoPfx));
        }

        // Produccion: TicketAcceso.txt / TicketAccesoPerson.txt (igual que siempre).
        // Homologacion: TicketAcceso.homo.txt / TicketAccesoPerson.homo.txt.
        public static string Ticket(string carpeta, bool esPadron, bool homologacion)
        {
            string baseName = esPadron ? TicketPadron : TicketFactura;
            if (homologacion)
                baseName = Path.GetFileNameWithoutExtension(baseName) + ".homo" + Path.GetExtension(baseName);
            return Path.Combine(carpeta, baseName);
        }

        public static string Template(string carpeta)
        {
            return Path.Combine(carpeta, TemplateLogin);
        }

        private static bool EsSoloDigitos(string valor)
        {
            foreach (char c in valor)
                if (c < '0' || c > '9') return false;
            return true;
        }
    }
}
