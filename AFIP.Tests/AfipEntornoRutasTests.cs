// Pruebas de las reglas compartidas de AFIP: entorno (homologacion/produccion), rutas en disco y
// endpoints. Las rutas se prueban como "caracterizacion": deben dar exactamente lo que armaban antes
// GenerarFacturaService y ConsultarPadronService a mano.
using System.IO;
using AFIP;

namespace AFIP.Tests
{
    public class AfipEntornoRutasTests
    {
        [Theory]
        [InlineData("HOMO", true)]
        [InlineData("homo", true)]
        [InlineData(" Homologación ", true)]
        [InlineData("PROD", false)]
        [InlineData("Producción", false)]
        [InlineData("", false)]
        [InlineData(null, false)]
        [InlineData("   ", false)]
        public void EsHomologacion_SoloSiDiceHomoExplicito(string valor, bool esperado)
        {
            Assert.Equal(esperado, AfipEntorno.EsHomologacion(valor));
            Assert.Equal(esperado ? "HOMO" : "PROD", AfipEntorno.Normalizar(valor));
        }

        [Fact]
        public void Rutas_LegadoCoincideConLoQueArmabaElCodigoAnterior()
        {
            string baseDir = Path.Combine(Path.GetTempPath(), "app");
            string cuit = "20306210786";

            // Forma vieja: Path.Combine(base, "AFIP", cuit) y luego Combine con el nombre / ticket.
            string carpetaVieja = Path.Combine(baseDir, "AFIP", cuit);
            Assert.Equal(carpetaVieja, AfipRutas.Carpeta(baseDir, cuit));
            Assert.Equal(Path.Combine(carpetaVieja, "mi-cert.pfx"), AfipRutas.Certificado(carpetaVieja, "mi-cert.pfx"));
            Assert.Equal(Path.Combine(carpetaVieja, "certif-prod.pfx"), AfipRutas.Certificado(carpetaVieja, ""));
            Assert.Equal(Path.Combine(carpetaVieja, "TicketAcceso.txt"), AfipRutas.Ticket(carpetaVieja, esPadron: false));
            Assert.Equal(Path.Combine(carpetaVieja, "TicketAccesoPerson.txt"), AfipRutas.Ticket(carpetaVieja, esPadron: true));
            Assert.Equal(Path.Combine(carpetaVieja, "LoginTemplate.xml"), AfipRutas.Template(carpetaVieja));
        }

        [Fact]
        public void CarpetaEntorno_ProdYHomoSonSubcarpetasDelCuit()
        {
            string baseDir = Path.Combine(Path.GetTempPath(), "app");
            Assert.Equal(Path.Combine(baseDir, "AFIP", "20306210786", "prod"), AfipRutas.CarpetaEntorno(baseDir, "20306210786", false));
            Assert.Equal(Path.Combine(baseDir, "AFIP", "20306210786", "homo"), AfipRutas.CarpetaEntorno(baseDir, "20306210786", true));
            Assert.Throws<System.ArgumentException>(() => AfipRutas.CarpetaEntorno(baseDir, "../x", true));
        }

        // ---- Resolver: donde se lee el certificado de cada entorno ----

        private static string Temporal()
        {
            string dir = Path.Combine(Path.GetTempPath(), "rutas-" + System.Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(dir);
            return dir;
        }

        [Fact]
        public void Resolver_ProduccionSinNadaApuntaAProdParaElMensajeDeSinCertificado()
        {
            string baseDir = Temporal();
            try
            {
                var u = AfipRutas.Resolver(baseDir, "20306210786", false, "");
                Assert.False(u.EsLegado);
                Assert.Equal(Path.Combine(baseDir, "AFIP", "20306210786", "prod", "certificado.pfx"), u.RutaPfx);
                Assert.Equal(Path.Combine(baseDir, "AFIP", "20306210786", "prod"), u.Carpeta);
                Assert.Equal(Path.Combine(u.Carpeta, "TicketAcceso.txt"), u.RutaTicket(false));
                Assert.Equal(Path.Combine(u.Carpeta, "TicketAccesoPerson.txt"), u.RutaTicket(true));
                Assert.Equal(Path.Combine(u.Carpeta, "LoginTemplate.xml"), u.RutaTemplate);
            }
            finally { Directory.Delete(baseDir, true); }
        }

        [Fact]
        public void Resolver_ProduccionConPfxHistoricoUsaLaRaizComoSiempre()
        {
            string baseDir = Temporal();
            try
            {
                string raiz = Path.Combine(baseDir, "AFIP", "20306210786");
                Directory.CreateDirectory(raiz);
                File.WriteAllText(Path.Combine(raiz, "mi-cert.pfx"), "x");

                var u = AfipRutas.Resolver(baseDir, "20306210786", false, "mi-cert.pfx");
                Assert.True(u.EsLegado);
                Assert.Equal(raiz, u.Carpeta);
                Assert.Equal(Path.Combine(raiz, "mi-cert.pfx"), u.RutaPfx);
                Assert.Equal(Path.Combine(raiz, "TicketAcceso.txt"), u.RutaTicket(false)); // el ticket de siempre
                Assert.Equal(Path.Combine(raiz, "LoginTemplate.xml"), u.RutaTemplate);
            }
            finally { Directory.Delete(baseDir, true); }
        }

        [Fact]
        public void Resolver_ProduccionConPfxHistoricoSinNombreConfiguradoUsaCertifProd()
        {
            string baseDir = Temporal();
            try
            {
                string raiz = Path.Combine(baseDir, "AFIP", "20306210786");
                Directory.CreateDirectory(raiz);
                File.WriteAllText(Path.Combine(raiz, "certif-prod.pfx"), "x");
                Assert.True(AfipRutas.Resolver(baseDir, "20306210786", false, null).EsLegado);
            }
            finally { Directory.Delete(baseDir, true); }
        }

        [Fact]
        public void Resolver_ProdConPfxNuevoGanaAlHistorico()
        {
            string baseDir = Temporal();
            try
            {
                string raiz = Path.Combine(baseDir, "AFIP", "20306210786");
                Directory.CreateDirectory(Path.Combine(raiz, "prod"));
                File.WriteAllText(Path.Combine(raiz, "certif-prod.pfx"), "viejo");
                File.WriteAllText(Path.Combine(raiz, "prod", "certificado.pfx"), "nuevo");

                var u = AfipRutas.Resolver(baseDir, "20306210786", false, "certif-prod.pfx");
                Assert.False(u.EsLegado);
                Assert.Equal(Path.Combine(raiz, "prod", "certificado.pfx"), u.RutaPfx);
                Assert.Equal(Path.Combine(raiz, "prod", "TicketAcceso.txt"), u.RutaTicket(false));
            }
            finally { Directory.Delete(baseDir, true); }
        }

        [Fact]
        public void Resolver_HomologacionSiempreVaAHomoYNuncaAlHistorico()
        {
            string baseDir = Temporal();
            try
            {
                string raiz = Path.Combine(baseDir, "AFIP", "20306210786");
                Directory.CreateDirectory(raiz);
                File.WriteAllText(Path.Combine(raiz, "certif-prod.pfx"), "prod");
                File.WriteAllText(Path.Combine(raiz, "TicketAcceso.txt"), "ticket-prod");

                var u = AfipRutas.Resolver(baseDir, "20306210786", true, "certif-prod.pfx");
                Assert.False(u.EsLegado);
                Assert.Equal(Path.Combine(raiz, "homo", "certificado.pfx"), u.RutaPfx);
                Assert.Equal(Path.Combine(raiz, "homo", "TicketAcceso.txt"), u.RutaTicket(false));
                Assert.NotEqual(AfipRutas.Resolver(baseDir, "20306210786", false, "certif-prod.pfx").RutaTicket(false), u.RutaTicket(false));
            }
            finally { Directory.Delete(baseDir, true); }
        }

        [Fact]
        public void Resolver_NoPuedeSalirDeAfipNiConCuitNiConNombreLegado()
        {
            string baseDir = Temporal();
            try
            {
                Assert.Throws<System.ArgumentException>(() => AfipRutas.Resolver(baseDir, "../x", false, ""));
                string raiz = Path.Combine(baseDir, "AFIP", "20306210786");
                Directory.CreateDirectory(raiz);
                File.WriteAllText(Path.Combine(raiz, "cert.pfx"), "x");
                // Un nombre con ruta se reduce al nombre de archivo: sigue dentro de la carpeta del CUIT.
                var u = AfipRutas.Resolver(baseDir, "20306210786", false, "../../cert.pfx");
                Assert.Equal(Path.Combine(raiz, "cert.pfx"), u.RutaPfx);
            }
            finally { Directory.Delete(baseDir, true); }
        }

        [Fact]
        public void AsegurarPlantilla_CreaLoginTemplateEnCarpetaNuevaConPfxPeroNoEnLaLegada()
        {
            string baseDir = Temporal();
            try
            {
                string raiz = Path.Combine(baseDir, "AFIP", "20306210786");
                Directory.CreateDirectory(Path.Combine(raiz, "homo"));
                File.WriteAllText(Path.Combine(raiz, "homo", "certificado.pfx"), "x"); // copiado a mano, sin plantilla
                var homo = AfipRutas.Resolver(baseDir, "20306210786", true, "");
                AfipRutas.AsegurarPlantilla(homo);
                Assert.True(File.Exists(homo.RutaTemplate));

                // Legado: nunca se escribe nada en la raiz.
                File.WriteAllText(Path.Combine(raiz, "certif-prod.pfx"), "x");
                var legado = AfipRutas.Resolver(baseDir, "20306210786", false, "");
                Assert.True(legado.EsLegado);
                AfipRutas.AsegurarPlantilla(legado);
                Assert.False(File.Exists(Path.Combine(raiz, "LoginTemplate.xml")));
            }
            finally { Directory.Delete(baseDir, true); }
        }

        [Theory]
        [InlineData("..")]
        [InlineData("2030/../x")]
        [InlineData("abc")]
        [InlineData("")]
        public void Carpeta_RechazaCuitInvalido(string cuit)
        {
            Assert.Throws<System.ArgumentException>(() => AfipRutas.Carpeta("x", cuit));
        }

        [Fact]
        public void NombreCertificado_NoPuedeSalirDeLaCarpeta()
        {
            Assert.Equal("cert.pfx", AfipRutas.NombreCertificado("../../cert.pfx"));

            // Con separador de Windows: en Windows se descarta la ruta; en Linux "\" es un caracter
            // de nombre valido (no separador) y el resultado sigue siendo un nombre plano, sin salir.
            string conBarraInvertida = AfipRutas.NombreCertificado("..\\..\\cert.pfx");
            Assert.Equal(Path.GetFileName(conBarraInvertida), conBarraInvertida);
        }

        [Fact]
        public void Endpoints_PorDefectoSonLosOficialesYSeparanEntornos()
        {
            var cfg = AfipConfig.PorDefecto();
            Assert.Equal("https://wsaa.afip.gov.ar/ws/services/LoginCms", cfg.Para("PROD").WsaaUrl);
            Assert.Equal("https://wsaa.afip.gov.ar/ws/services/LoginCms", cfg.Para("").WsaaUrl);
            Assert.Equal("https://servicios1.afip.gov.ar/wsfev1/service.asmx", cfg.Para(null).WsfeUrl);
            Assert.Equal("https://wsaahomo.afip.gov.ar/ws/services/LoginCms", cfg.Para("HOMO").WsaaUrl);
            Assert.Equal("https://wswhomo.afip.gov.ar/wsfev1/service.asmx", cfg.Para("HOMO").WsfeUrl);
            Assert.Equal("https://awshomo.afip.gov.ar/sr-padron/webservices/personaServiceA13", cfg.Para("HOMO").PadronUrl);
        }
    }
}
