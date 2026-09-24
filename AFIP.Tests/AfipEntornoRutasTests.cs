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
        public void Rutas_ProduccionCoincideConLoQueArmabaElCodigoAnterior()
        {
            string baseDir = Path.Combine(Path.GetTempPath(), "app");
            string cuit = "20306210786";

            // Forma vieja: Path.Combine(base, "AFIP", cuit) y luego Combine con el nombre / ticket.
            string carpetaVieja = Path.Combine(baseDir, "AFIP", cuit);
            Assert.Equal(carpetaVieja, AfipRutas.Carpeta(baseDir, cuit));
            Assert.Equal(Path.Combine(carpetaVieja, "mi-cert.pfx"), AfipRutas.Certificado(carpetaVieja, "mi-cert.pfx"));
            Assert.Equal(Path.Combine(carpetaVieja, "certif-prod.pfx"), AfipRutas.Certificado(carpetaVieja, ""));
            Assert.Equal(Path.Combine(carpetaVieja, "TicketAcceso.txt"), AfipRutas.Ticket(carpetaVieja, esPadron: false, homologacion: false));
            Assert.Equal(Path.Combine(carpetaVieja, "TicketAccesoPerson.txt"), AfipRutas.Ticket(carpetaVieja, esPadron: true, homologacion: false));
            Assert.Equal(Path.Combine(carpetaVieja, "LoginTemplate.xml"), AfipRutas.Template(carpetaVieja));
        }

        [Fact]
        public void Rutas_HomologacionUsaTicketsPropios()
        {
            string carpeta = Path.Combine(Path.GetTempPath(), "AFIP", "20306210786");
            Assert.Equal(Path.Combine(carpeta, "TicketAcceso.homo.txt"), AfipRutas.Ticket(carpeta, false, true));
            Assert.Equal(Path.Combine(carpeta, "TicketAccesoPerson.homo.txt"), AfipRutas.Ticket(carpeta, true, true));
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
