// Pruebas del flujo de certificado ARCA: generar CSR -> "ARCA" firma un .crt (aca: autofirmado con la
// misma clave) -> instalar -> leer vencimiento. Un certificado por entorno (AFIP/<cuit>/prod y /homo).
// Usa una carpeta temporal por prueba; nunca toca AFIP/ real.
using System;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using AFIP;

namespace AFIP.Tests
{
    public sealed class CertificadoArcaServiceTests : IDisposable
    {
        private const long Cuit = 20306210786;
        private const bool Prod = false;
        private const bool Homo = true;
        private readonly string _raiz = Path.Combine(Path.GetTempPath(), "arca-tests-" + Guid.NewGuid().ToString("N"));
        private readonly CertificadoArcaService _svc;

        public CertificadoArcaServiceTests()
        {
            Directory.CreateDirectory(_raiz);
            _svc = new CertificadoArcaService(_raiz);
        }

        public void Dispose()
        {
            try { Directory.Delete(_raiz, true); } catch (IOException) { /* limpieza best-effort del temporal */ }
        }

        private string CarpetaCuit => Path.Combine(_raiz, "AFIP", Cuit.ToString());
        private string CarpetaProd => Path.Combine(CarpetaCuit, "prod");
        private string CarpetaHomo => Path.Combine(CarpetaCuit, "homo");
        private string CarpetaDe(bool homologacion) => homologacion ? CarpetaHomo : CarpetaProd;

        // Simula lo que hace ARCA: emite un certificado para la clave del pedido pendiente del entorno.
        private byte[] EmitirCrt(long cuitEnSubject, int diasVigencia, RSA? claveOverride = null, bool pem = false, bool homologacion = false)
        {
            RSA rsa;
            if (claveOverride != null) rsa = claveOverride;
            else
            {
                rsa = RSA.Create();
                rsa.ImportFromPem(File.ReadAllText(Path.Combine(CarpetaDe(homologacion), "pendiente", "clave.key")));
            }
            using (rsa)
            {
                var req = new CertificateRequest(
                    "C=AR, O=Empresa Test, CN=test, SERIALNUMBER=CUIT " + cuitEnSubject,
                    rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
                // diasVigencia negativo = certificado ya vencido (notBefore anterior al vencimiento).
                var desde = DateTimeOffset.UtcNow.AddDays(Math.Min(-1, diasVigencia - 10));
                using var cert = req.CreateSelfSigned(desde, DateTimeOffset.UtcNow.AddDays(diasVigencia));
                return pem
                    ? Encoding.ASCII.GetBytes(cert.ExportCertificatePem())
                    : cert.Export(X509ContentType.Cert);
            }
        }

        // Instala un certificado de prueba en el entorno y devuelve la clave del pfx.
        private string InstalarCert(bool homologacion, int dias, string alias = "a1")
        {
            _svc.GenerarCsr(Cuit, homologacion, "Empresa Test", alias);
            string? clave = null;
            _svc.Instalar(Cuit, homologacion, AfipRutas.NombreCertificadoFijo, EmitirCrt(Cuit, dias, homologacion: homologacion), c => clave = c);
            return clave!;
        }

        // Como los certificados que ya existen en produccion: pfx historico en la raiz del CUIT, exportado con clave vacia.
        private byte[] CrearPfxHistoricoEnLaRaiz(string nombre = "certif-prod.pfx")
        {
            Directory.CreateDirectory(CarpetaCuit);
            using var rsa = RSA.Create(2048);
            var req = new CertificateRequest("CN=historico, SERIALNUMBER=CUIT " + Cuit, rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
            using var cert = req.CreateSelfSigned(DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddDays(200));
            byte[] pfx = cert.Export(X509ContentType.Pfx, "");
            File.WriteAllBytes(Path.Combine(CarpetaCuit, nombre), pfx);
            return pfx;
        }

        [Fact]
        public void GenerarCsr_DejaClavePendienteEnLaCarpetaDelEntornoYDevuelveCsrConElCuit()
        {
            string csr = _svc.GenerarCsr(Cuit, Prod, "Empresa, Test S.A.", "mi-alias");

            Assert.StartsWith("-----BEGIN CERTIFICATE REQUEST-----", csr);
            Assert.True(File.Exists(Path.Combine(CarpetaProd, "pendiente", "clave.key")));
            Assert.False(Directory.Exists(CarpetaHomo)); // las carpetas se crean al escribir, solo la del entorno usado
            Assert.Equal(csr, _svc.LeerCsrPendiente(Cuit, Prod));
            Assert.Null(_svc.LeerCsrPendiente(Cuit, Homo));

            var pedido = CertificateRequest.LoadSigningRequestPem(csr, HashAlgorithmName.SHA256);
            Assert.Contains("CUIT " + Cuit, pedido.SubjectName.Name);
            Assert.Contains("CN=mi-alias", pedido.SubjectName.Name);
        }

        [Theory]
        [InlineData("")]
        [InlineData("alias con espacios")]
        [InlineData("../x")]
        public void GenerarCsr_RechazaAliasInvalido(string alias)
        {
            Assert.Throws<CertificadoArcaException>(() => _svc.GenerarCsr(Cuit, Prod, "Empresa", alias));
        }

        [Fact]
        public void Instalar_ArmaPfxConClaveEnLaCarpetaDelEntorno_LoLeeYGuardaLaClave()
        {
            _svc.GenerarCsr(Cuit, Prod, "Empresa Test", "alias1");
            string? claveGuardada = null;

            var resultado = _svc.Instalar(Cuit, Prod, "cualquier-nombre.pfx", EmitirCrt(Cuit, 730), c => claveGuardada = c);

            Assert.False(string.IsNullOrEmpty(claveGuardada));
            Assert.True(claveGuardada!.Length >= 20);
            Assert.Equal("certificado.pfx", resultado.NombreArchivo); // el nombre es fijo: no depende de lo que se pida
            Assert.True(File.Exists(Path.Combine(CarpetaProd, "certificado.pfx")));
            Assert.False(Directory.Exists(Path.Combine(CarpetaProd, "pendiente")));
            Assert.True(File.Exists(Path.Combine(CarpetaProd, "LoginTemplate.xml"))); // empresa nueva: se crea la plantilla de login
            Assert.Null(resultado.RutaBackup);

            var info = _svc.Leer(Cuit, Prod, null, claveGuardada, new[] { 60, 30, 15 });
            Assert.Equal(EstadoCertificado.Vigente, info.Estado);
            Assert.False(info.EsLegado);
            Assert.InRange(info.DiasRestantes!.Value, 728, 731);
            Assert.False(info.HayPedidoPendiente);
        }

        [Fact]
        public void Instalar_AceptaCrtEnPem()
        {
            _svc.GenerarCsr(Cuit, Prod, "Empresa Test", "alias1");
            string? clave = null;
            _svc.Instalar(Cuit, Prod, AfipRutas.NombreCertificadoFijo, EmitirCrt(Cuit, 100, pem: true), c => clave = c);
            Assert.Equal(EstadoCertificado.Vigente, _svc.Leer(Cuit, Prod, null, clave, new[] { 30 }).Estado);
        }

        [Fact]
        public void Instalar_RechazaCrtDeOtraClave()
        {
            _svc.GenerarCsr(Cuit, Prod, "Empresa Test", "alias1");
            using var otraClave = RSA.Create(2048);
            byte[] crtAjeno = EmitirCrt(Cuit, 100, claveOverride: otraClave);

            var ex = Assert.Throws<CertificadoArcaException>(() => _svc.Instalar(Cuit, Prod, AfipRutas.NombreCertificadoFijo, crtAjeno, _ => { }));
            Assert.Contains("no corresponde", ex.Message);
            Assert.False(File.Exists(Path.Combine(CarpetaProd, "certificado.pfx")));
        }

        [Fact]
        public void Instalar_RechazaCrtDeOtroCuit()
        {
            _svc.GenerarCsr(Cuit, Prod, "Empresa Test", "alias1");
            var ex = Assert.Throws<CertificadoArcaException>(() =>
                _svc.Instalar(Cuit, Prod, AfipRutas.NombreCertificadoFijo, EmitirCrt(20111111112, 100), _ => { }));
            Assert.Contains("CUIT", ex.Message);
        }

        [Fact]
        public void Instalar_RechazaCrtVencido()
        {
            _svc.GenerarCsr(Cuit, Prod, "Empresa Test", "alias1");
            var ex = Assert.Throws<CertificadoArcaException>(() =>
                _svc.Instalar(Cuit, Prod, AfipRutas.NombreCertificadoFijo, EmitirCrt(Cuit, -5), _ => { }));
            Assert.Contains("vencido", ex.Message);
        }

        [Fact]
        public void Instalar_SinPedidoPendienteYArchivoBasura_DanErrorControlado()
        {
            Assert.Throws<CertificadoArcaException>(() => _svc.Instalar(Cuit, Prod, AfipRutas.NombreCertificadoFijo, new byte[] { 1, 2, 3 }, _ => { }));
            _svc.GenerarCsr(Cuit, Prod, "Empresa Test", "alias1");
            Assert.Throws<CertificadoArcaException>(() => _svc.Instalar(Cuit, Prod, AfipRutas.NombreCertificadoFijo, new byte[] { 1, 2, 3 }, _ => { }));
        }

        [Fact]
        public void Instalar_ReemplazaConBackup_BorraSoloLosTicketsDelEntorno()
        {
            // Cert viejo + tickets de produccion (en prod/), de homologacion (en homo/) y los historicos de la raiz.
            string claveVieja = InstalarCert(Prod, 30, "a1");
            File.WriteAllText(Path.Combine(CarpetaProd, "TicketAcceso.txt"), "x");
            File.WriteAllText(Path.Combine(CarpetaProd, "TicketAccesoPerson.txt"), "x");
            Directory.CreateDirectory(CarpetaHomo);
            File.WriteAllText(Path.Combine(CarpetaHomo, "TicketAcceso.txt"), "homo");
            File.WriteAllText(Path.Combine(CarpetaCuit, "TicketAcceso.txt"), "raiz");

            // Renovacion.
            _svc.GenerarCsr(Cuit, Prod, "Empresa Test", "a2");
            string? claveNueva = null;
            var resultado = _svc.Instalar(Cuit, Prod, AfipRutas.NombreCertificadoFijo, EmitirCrt(Cuit, 730), c => claveNueva = c);

            Assert.NotNull(resultado.RutaBackup);
            Assert.True(File.Exists(resultado.RutaBackup));
            Assert.NotEqual(claveVieja, claveNueva);
            Assert.False(File.Exists(Path.Combine(CarpetaProd, "TicketAcceso.txt")));
            Assert.False(File.Exists(Path.Combine(CarpetaProd, "TicketAccesoPerson.txt")));
            Assert.True(File.Exists(Path.Combine(CarpetaHomo, "TicketAcceso.txt")));
            Assert.True(File.Exists(Path.Combine(CarpetaCuit, "TicketAcceso.txt")));
            Assert.InRange(_svc.Leer(Cuit, Prod, null, claveNueva, new[] { 30 }).DiasRestantes!.Value, 728, 731);
        }

        [Fact]
        public void Instalar_SiFallaGuardarLaClave_RestauraElPfxAnterior()
        {
            string claveVieja = InstalarCert(Prod, 30, "a1");
            byte[] pfxViejo = File.ReadAllBytes(Path.Combine(CarpetaProd, "certificado.pfx"));

            _svc.GenerarCsr(Cuit, Prod, "Empresa Test", "a2");
            Assert.Throws<InvalidOperationException>(() =>
                _svc.Instalar(Cuit, Prod, AfipRutas.NombreCertificadoFijo, EmitirCrt(Cuit, 730), _ => throw new InvalidOperationException("base caida")));

            Assert.Equal(pfxViejo, File.ReadAllBytes(Path.Combine(CarpetaProd, "certificado.pfx")));
            Assert.Equal(EstadoCertificado.Vigente, _svc.Leer(Cuit, Prod, null, claveVieja, new[] { 5 }).Estado);
        }

        [Fact]
        public void Leer_EstadosSinCertificadoPorVencerYIlegible()
        {
            Assert.Equal(EstadoCertificado.SinCertificado, _svc.Leer(Cuit, Prod, null, null, new[] { 30 }).Estado);

            string clave = InstalarCert(Prod, 20);

            Assert.Equal(EstadoCertificado.PorVencer, _svc.Leer(Cuit, Prod, null, clave, new[] { 60, 30, 15 }).Estado);
            Assert.Equal(EstadoCertificado.Vigente, _svc.Leer(Cuit, Prod, null, clave, new[] { 10 }).Estado);
            // 100 dias despues ya esta vencido.
            Assert.Equal(EstadoCertificado.Vencido,
                _svc.Leer(Cuit, Prod, null, clave, new[] { 30 }, DateTime.UtcNow.AddDays(100)).Estado);
            // Clave incorrecta y archivo corrupto: Ilegible, sin excepcion.
            Assert.Equal(EstadoCertificado.Ilegible, _svc.Leer(Cuit, Prod, null, "clave-mala", new[] { 30 }).Estado);
            File.WriteAllBytes(Path.Combine(CarpetaProd, "certificado.pfx"), new byte[] { 9, 9, 9 });
            Assert.Equal(EstadoCertificado.Ilegible, _svc.Leer(Cuit, Prod, null, null, new[] { 30 }).Estado);
        }

        [Fact]
        public void Leer_PfxHistoricoDeLaRaizSinClave_FuncionaSoloEnProduccion()
        {
            CrearPfxHistoricoEnLaRaiz();

            var prod = _svc.Leer(Cuit, Prod, "certif-prod.pfx", null, new[] { 30 });
            Assert.Equal(EstadoCertificado.Vigente, prod.Estado);
            Assert.True(prod.EsLegado);
            Assert.InRange(prod.DiasRestantes!.Value, 198, 200);

            // Homologacion nunca cae al historico: es de produccion.
            Assert.Equal(EstadoCertificado.SinCertificado, _svc.Leer(Cuit, Homo, "certif-prod.pfx", null, new[] { 30 }).Estado);
        }

        // ---- lo importante: un entorno nunca toca los archivos del otro ----

        [Fact]
        public void InstalarEnHomologacion_NoTocaNadaDeProduccion()
        {
            // Produccion "en marcha": pfx historico en la raiz + tickets de la raiz + un pedido pendiente de produccion.
            byte[] pfxHistorico = CrearPfxHistoricoEnLaRaiz();
            File.WriteAllText(Path.Combine(CarpetaCuit, "TicketAcceso.txt"), "ticket-prod");
            File.WriteAllText(Path.Combine(CarpetaCuit, "LoginTemplate.xml"), "<t/>");
            _svc.GenerarCsr(Cuit, Prod, "Empresa Test", "prod1");
            string claveProdPendiente = File.ReadAllText(Path.Combine(CarpetaProd, "pendiente", "clave.key"));

            // Se instala un certificado de homologacion (con su propio pedido).
            string claveHomo = InstalarCert(Homo, 100, "homo1");

            // Homologacion quedo en homo/, y produccion no cambio ni un byte.
            Assert.True(File.Exists(Path.Combine(CarpetaHomo, "certificado.pfx")));
            Assert.Equal(EstadoCertificado.Vigente, _svc.Leer(Cuit, Homo, null, claveHomo, new[] { 30 }).Estado);
            Assert.Equal(pfxHistorico, File.ReadAllBytes(Path.Combine(CarpetaCuit, "certif-prod.pfx")));
            Assert.Equal("ticket-prod", File.ReadAllText(Path.Combine(CarpetaCuit, "TicketAcceso.txt")));
            Assert.Equal("<t/>", File.ReadAllText(Path.Combine(CarpetaCuit, "LoginTemplate.xml")));
            Assert.Equal(claveProdPendiente, File.ReadAllText(Path.Combine(CarpetaProd, "pendiente", "clave.key"))); // el pedido de prod sigue
            Assert.False(File.Exists(Path.Combine(CarpetaProd, "certificado.pfx")));
            Assert.Equal(EstadoCertificado.Vigente, _svc.Leer(Cuit, Prod, "certif-prod.pfx", null, new[] { 30 }).Estado);
        }

        [Fact]
        public void InstalarEnProduccion_ConHistorico_DejaElHistoricoIntactoYPasaAUsarElNuevo()
        {
            byte[] pfxHistorico = CrearPfxHistoricoEnLaRaiz();
            File.WriteAllText(Path.Combine(CarpetaCuit, "TicketAcceso.txt"), "ticket-viejo");

            string clave = InstalarCert(Prod, 400);

            // El historico y su ticket quedan como respaldo, sin tocar.
            Assert.Equal(pfxHistorico, File.ReadAllBytes(Path.Combine(CarpetaCuit, "certif-prod.pfx")));
            Assert.Equal("ticket-viejo", File.ReadAllText(Path.Combine(CarpetaCuit, "TicketAcceso.txt")));
            // Y produccion ahora lee el nuevo (prod/ tiene prioridad sobre el historico).
            var info = _svc.Leer(Cuit, Prod, "certif-prod.pfx", clave, new[] { 30 });
            Assert.False(info.EsLegado);
            Assert.InRange(info.DiasRestantes!.Value, 398, 401);
        }

        [Fact]
        public void PedidosPendientes_SonIndependientesPorEntorno()
        {
            string csrProd = _svc.GenerarCsr(Cuit, Prod, "Empresa Test", "p1");
            string csrHomo = _svc.GenerarCsr(Cuit, Homo, "Empresa Test", "h1");

            Assert.NotEqual(csrProd, csrHomo);
            Assert.Equal(csrProd, _svc.LeerCsrPendiente(Cuit, Prod));
            Assert.Equal(csrHomo, _svc.LeerCsrPendiente(Cuit, Homo));

            _svc.DescartarPedidoPendiente(Cuit, Homo);
            Assert.Null(_svc.LeerCsrPendiente(Cuit, Homo));
            Assert.Equal(csrProd, _svc.LeerCsrPendiente(Cuit, Prod));
        }
    }
}


namespace AFIP.Tests
{
    public class BandaAvisoTests
    {
        private static readonly int[] Umbrales = { 60, 30, 15 };

        [Theory]
        [InlineData(200, -1)]
        [InlineData(61, -1)]
        [InlineData(60, 60)]
        [InlineData(45, 60)]
        [InlineData(30, 30)]
        [InlineData(20, 30)]
        [InlineData(15, 15)]
        [InlineData(1, 15)]
        [InlineData(0, 0)]
        [InlineData(-3, 0)]
        public void BandaAviso_ElMenorUmbralQueCubreElPlazo(int dias, int esperada)
        {
            Assert.Equal(esperada, CertificadoArcaService.BandaAviso(dias, Umbrales));
        }

        [Fact]
        public void BandaAviso_SinUmbralesSoloAvisaVencido()
        {
            Assert.Equal(-1, CertificadoArcaService.BandaAviso(5, null));
            Assert.Equal(0, CertificadoArcaService.BandaAviso(0, new int[0]));
        }
    }
}
