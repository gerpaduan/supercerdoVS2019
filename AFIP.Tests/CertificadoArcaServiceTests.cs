// Pruebas del flujo de certificado ARCA: generar CSR -> "ARCA" firma un .crt (aca: autofirmado con la
// misma clave) -> instalar -> leer vencimiento. Usa una carpeta temporal por prueba; nunca toca AFIP/ real.
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

        // Simula lo que hace ARCA: emite un certificado para la clave del pedido pendiente.
        private byte[] EmitirCrt(long cuitEnSubject, int diasVigencia, RSA? claveOverride = null, bool pem = false)
        {
            RSA rsa;
            if (claveOverride != null) rsa = claveOverride;
            else
            {
                rsa = RSA.Create();
                rsa.ImportFromPem(File.ReadAllText(Path.Combine(CarpetaCuit, "pendiente", "clave.key")));
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

        [Fact]
        public void GenerarCsr_DejaClavePendienteYDevuelveCsrConElCuit()
        {
            string csr = _svc.GenerarCsr(Cuit, "Empresa, Test S.A.", "mi-alias");

            Assert.StartsWith("-----BEGIN CERTIFICATE REQUEST-----", csr);
            Assert.True(File.Exists(Path.Combine(CarpetaCuit, "pendiente", "clave.key")));
            Assert.Equal(csr, _svc.LeerCsrPendiente(Cuit));

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
            Assert.Throws<CertificadoArcaException>(() => _svc.GenerarCsr(Cuit, "Empresa", alias));
        }

        [Fact]
        public void Instalar_ArmaPfxConClave_LoLeeYGuardaLaClave()
        {
            _svc.GenerarCsr(Cuit, "Empresa Test", "alias1");
            string? claveGuardada = null;

            var resultado = _svc.Instalar(Cuit, "certif-prod.pfx", EmitirCrt(Cuit, 730), c => claveGuardada = c);

            Assert.False(string.IsNullOrEmpty(claveGuardada));
            Assert.True(claveGuardada!.Length >= 20);
            Assert.True(File.Exists(Path.Combine(CarpetaCuit, "certif-prod.pfx")));
            Assert.False(Directory.Exists(Path.Combine(CarpetaCuit, "pendiente")));
            Assert.True(File.Exists(Path.Combine(CarpetaCuit, "LoginTemplate.xml"))); // empresa nueva: se crea la plantilla de login
            Assert.Null(resultado.RutaBackup);

            var info = _svc.Leer(Cuit, "certif-prod.pfx", claveGuardada, new[] { 60, 30, 15 });
            Assert.Equal(EstadoCertificado.Vigente, info.Estado);
            Assert.InRange(info.DiasRestantes!.Value, 728, 731);
            Assert.False(info.HayPedidoPendiente);
        }

        [Fact]
        public void Instalar_AceptaCrtEnPem()
        {
            _svc.GenerarCsr(Cuit, "Empresa Test", "alias1");
            string? clave = null;
            _svc.Instalar(Cuit, "certif-prod.pfx", EmitirCrt(Cuit, 100, pem: true), c => clave = c);
            Assert.Equal(EstadoCertificado.Vigente, _svc.Leer(Cuit, "certif-prod.pfx", clave, new[] { 30 }).Estado);
        }

        [Fact]
        public void Instalar_RechazaCrtDeOtraClave()
        {
            _svc.GenerarCsr(Cuit, "Empresa Test", "alias1");
            using var otraClave = RSA.Create(2048);
            byte[] crtAjeno = EmitirCrt(Cuit, 100, claveOverride: otraClave);

            var ex = Assert.Throws<CertificadoArcaException>(() => _svc.Instalar(Cuit, "certif-prod.pfx", crtAjeno, _ => { }));
            Assert.Contains("no corresponde", ex.Message);
            Assert.False(File.Exists(Path.Combine(CarpetaCuit, "certif-prod.pfx")));
        }

        [Fact]
        public void Instalar_RechazaCrtDeOtroCuit()
        {
            _svc.GenerarCsr(Cuit, "Empresa Test", "alias1");
            var ex = Assert.Throws<CertificadoArcaException>(() =>
                _svc.Instalar(Cuit, "certif-prod.pfx", EmitirCrt(20111111112, 100), _ => { }));
            Assert.Contains("CUIT", ex.Message);
        }

        [Fact]
        public void Instalar_RechazaCrtVencido()
        {
            _svc.GenerarCsr(Cuit, "Empresa Test", "alias1");
            var ex = Assert.Throws<CertificadoArcaException>(() =>
                _svc.Instalar(Cuit, "certif-prod.pfx", EmitirCrt(Cuit, -5), _ => { }));
            Assert.Contains("vencido", ex.Message);
        }

        [Fact]
        public void Instalar_SinPedidoPendienteYArchivoBasura_DanErrorControlado()
        {
            Assert.Throws<CertificadoArcaException>(() => _svc.Instalar(Cuit, "certif-prod.pfx", new byte[] { 1, 2, 3 }, _ => { }));
            _svc.GenerarCsr(Cuit, "Empresa Test", "alias1");
            Assert.Throws<CertificadoArcaException>(() => _svc.Instalar(Cuit, "certif-prod.pfx", new byte[] { 1, 2, 3 }, _ => { }));
        }

        [Fact]
        public void Instalar_ReemplazaConBackup_BorraTicketsYConservaElNombre()
        {
            // Cert viejo + tickets de produccion y homologacion.
            _svc.GenerarCsr(Cuit, "Empresa Test", "a1");
            string? claveVieja = null;
            _svc.Instalar(Cuit, "certif-prod.pfx", EmitirCrt(Cuit, 30), c => claveVieja = c);
            File.WriteAllText(Path.Combine(CarpetaCuit, "TicketAcceso.txt"), "x");
            File.WriteAllText(Path.Combine(CarpetaCuit, "TicketAccesoPerson.homo.txt"), "x");

            // Renovacion.
            _svc.GenerarCsr(Cuit, "Empresa Test", "a2");
            string? claveNueva = null;
            var resultado = _svc.Instalar(Cuit, "certif-prod.pfx", EmitirCrt(Cuit, 730), c => claveNueva = c);

            Assert.NotNull(resultado.RutaBackup);
            Assert.True(File.Exists(resultado.RutaBackup));
            Assert.NotEqual(claveVieja, claveNueva);
            Assert.False(File.Exists(Path.Combine(CarpetaCuit, "TicketAcceso.txt")));
            Assert.False(File.Exists(Path.Combine(CarpetaCuit, "TicketAccesoPerson.homo.txt")));
            Assert.InRange(_svc.Leer(Cuit, "certif-prod.pfx", claveNueva, new[] { 30 }).DiasRestantes!.Value, 728, 731);
        }

        [Fact]
        public void Instalar_SiFallaGuardarLaClave_RestauraElPfxAnterior()
        {
            _svc.GenerarCsr(Cuit, "Empresa Test", "a1");
            string? claveVieja = null;
            _svc.Instalar(Cuit, "certif-prod.pfx", EmitirCrt(Cuit, 30), c => claveVieja = c);
            byte[] pfxViejo = File.ReadAllBytes(Path.Combine(CarpetaCuit, "certif-prod.pfx"));

            _svc.GenerarCsr(Cuit, "Empresa Test", "a2");
            Assert.Throws<InvalidOperationException>(() =>
                _svc.Instalar(Cuit, "certif-prod.pfx", EmitirCrt(Cuit, 730), _ => throw new InvalidOperationException("base caida")));

            Assert.Equal(pfxViejo, File.ReadAllBytes(Path.Combine(CarpetaCuit, "certif-prod.pfx")));
            Assert.Equal(EstadoCertificado.Vigente, _svc.Leer(Cuit, "certif-prod.pfx", claveVieja, new[] { 5 }).Estado);
        }

        [Fact]
        public void Leer_EstadosSinCertificadoPorVencerYIlegible()
        {
            Assert.Equal(EstadoCertificado.SinCertificado, _svc.Leer(Cuit, "certif-prod.pfx", null, new[] { 30 }).Estado);

            _svc.GenerarCsr(Cuit, "Empresa Test", "a1");
            string? clave = null;
            _svc.Instalar(Cuit, "certif-prod.pfx", EmitirCrt(Cuit, 20), c => clave = c);

            Assert.Equal(EstadoCertificado.PorVencer, _svc.Leer(Cuit, "certif-prod.pfx", clave, new[] { 60, 30, 15 }).Estado);
            Assert.Equal(EstadoCertificado.Vigente, _svc.Leer(Cuit, "certif-prod.pfx", clave, new[] { 10 }).Estado);
            // 100 dias despues ya esta vencido.
            Assert.Equal(EstadoCertificado.Vencido,
                _svc.Leer(Cuit, "certif-prod.pfx", clave, new[] { 30 }, DateTime.UtcNow.AddDays(100)).Estado);
            // Clave incorrecta y archivo corrupto: Ilegible, sin excepcion.
            Assert.Equal(EstadoCertificado.Ilegible, _svc.Leer(Cuit, "certif-prod.pfx", "clave-mala", new[] { 30 }).Estado);
            File.WriteAllBytes(Path.Combine(CarpetaCuit, "roto.pfx"), new byte[] { 9, 9, 9 });
            Assert.Equal(EstadoCertificado.Ilegible, _svc.Leer(Cuit, "roto.pfx", null, new[] { 30 }).Estado);
        }

        [Fact]
        public void Leer_PfxHistoricoSinClave_Funciona()
        {
            // Como los certificados que ya existen en produccion: pfx exportado con clave vacia.
            Directory.CreateDirectory(CarpetaCuit);
            using var rsa = RSA.Create(2048);
            var req = new CertificateRequest("CN=historico, SERIALNUMBER=CUIT " + Cuit, rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
            using var cert = req.CreateSelfSigned(DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddDays(200));
            File.WriteAllBytes(Path.Combine(CarpetaCuit, "certif-prod.pfx"), cert.Export(X509ContentType.Pfx, ""));

            var info = _svc.Leer(Cuit, "certif-prod.pfx", null, new[] { 30 });
            Assert.Equal(EstadoCertificado.Vigente, info.Estado);
            Assert.InRange(info.DiasRestantes!.Value, 198, 200);
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
