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

        // Ticket WSAA dentro de una carpeta (la del entorno, o la raiz legada): TicketAcceso.txt / TicketAccesoPerson.txt.
        // Ya no hay sufijo ".homo": cada entorno tiene su propia carpeta, asi un ticket nunca se cruza.
        public static string Ticket(string carpeta, bool esPadron)
        {
            return Path.Combine(carpeta, esPadron ? TicketPadron : TicketFactura);
        }

        public static string Template(string carpeta)
        {
            return Path.Combine(carpeta, TemplateLogin);
        }

        // ------------------------------------------------------------------ carpetas por entorno

        // Un certificado por entorno, cada uno en su carpeta y con nombre fijo (no depende de
        // empresas.nombrecertificado_pfx):
        //   AFIP/<cuit>/prod/certificado.pfx  +  LoginTemplate.xml  +  Ticket*.txt  +  pendiente/
        //   AFIP/<cuit>/homo/certificado.pfx  +  ...
        // Lo historico (AFIP/<cuit>/certif-prod.pfx, TicketAcceso.txt, LoginTemplate.xml en la raiz del CUIT) sigue
        // funcionando como respaldo de SOLO LECTURA para produccion: nunca se mueve ni se borra.
        public const string CarpetaProd = "prod";
        public const string CarpetaHomo = "homo";
        public const string NombreCertificadoFijo = "certificado.pfx";

        // AFIP/<cuit>/prod o AFIP/<cuit>/homo (no exige que exista).
        public static string CarpetaEntorno(string baseDirectory, string cuit, bool homologacion)
        {
            return Path.Combine(Carpeta(baseDirectory, cuit), homologacion ? CarpetaHomo : CarpetaProd);
        }

        // Donde estan (o estaran) los archivos de una empresa para un entorno.
        public sealed class UbicacionAfip
        {
            public UbicacionAfip(string carpeta, string rutaPfx, bool esLegado)
            {
                Carpeta = carpeta;
                RutaPfx = rutaPfx;
                EsLegado = esLegado;
            }

            // Carpeta con el pfx, el LoginTemplate.xml y los tickets (la que se le pasa a LoginClass).
            public string Carpeta { get; private set; }
            public string RutaPfx { get; private set; }
            // true = se esta usando el certificado historico de la raiz del CUIT (solo produccion).
            public bool EsLegado { get; private set; }
            public string RutaTemplate { get { return Template(Carpeta); } }
            public string RutaTicket(bool esPadron) { return Ticket(Carpeta, esPadron); }
        }

        // Resuelve donde leer el certificado de una empresa:
        //  - homologacion: siempre AFIP/<cuit>/homo/certificado.pfx (no hay historico).
        //  - produccion: AFIP/<cuit>/prod/certificado.pfx si existe; si no, el pfx historico de la raiz
        //    (nombreLegado o certif-prod.pfx) si existe; si no hay ninguno, prod/ (para el mensaje "no hay certificado").
        // El CUIT debe ser solo digitos (no puede salir de AFIP/). No crea nada ni exige que existan carpetas.
        public static UbicacionAfip Resolver(string baseDirectory, string cuit, bool homologacion, string nombreLegado)
        {
            string raiz = Carpeta(baseDirectory, cuit);
            string carpetaEntorno = Path.Combine(raiz, homologacion ? CarpetaHomo : CarpetaProd);
            string pfxEntorno = Path.Combine(carpetaEntorno, NombreCertificadoFijo);

            if (!homologacion && !File.Exists(pfxEntorno))
            {
                string pfxLegado = Certificado(raiz, nombreLegado);
                if (File.Exists(pfxLegado))
                    return new UbicacionAfip(raiz, pfxLegado, true);
            }
            return new UbicacionAfip(carpetaEntorno, pfxEntorno, false);
        }

        // Una carpeta nueva (prod/ u homo/) con un pfx copiado a mano puede no tener el LoginTemplate.xml: se crea
        // el estandar (lo completa LoginClass en cada login). Nunca pisa uno existente ni toca la carpeta legada.
        public static void AsegurarPlantilla(UbicacionAfip ubicacion)
        {
            if (ubicacion == null || ubicacion.EsLegado) return;
            if (!File.Exists(ubicacion.RutaPfx) || File.Exists(ubicacion.RutaTemplate)) return;
            EscribirPlantillaEstandar(ubicacion.Carpeta);
        }

        // Crea LoginTemplate.xml (el estandar de WSAA) si no existe en la carpeta.
        public static void EscribirPlantillaEstandar(string carpeta)
        {
            string ruta = Template(carpeta);
            if (File.Exists(ruta)) return;
            const string plantilla =
                "<?xml version=\"1.0\" encoding=\"utf-8\" ?>\r\n" +
                "<loginTicketRequest>\r\n" +
                "  <header>\r\n" +
                "    <uniqueId></uniqueId>\r\n" +
                "    <generationTime></generationTime>\r\n" +
                "    <expirationTime></expirationTime>\r\n" +
                "  </header>\r\n" +
                "  <service></service>\r\n" +
                "</loginTicketRequest>\r\n";
            File.WriteAllText(ruta, plantilla, new System.Text.UTF8Encoding(false));
        }

        private static bool EsSoloDigitos(string valor)
        {
            foreach (char c in valor)
                if (c < '0' || c > '9') return false;
            return true;
        }
    }
}
