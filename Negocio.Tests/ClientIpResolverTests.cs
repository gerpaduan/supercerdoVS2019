using System.Net;
using Negocio;
using Xunit;

namespace NegocioTests
{
    // IP real del cliente detras de un proxy (Fase 4, ver Negocio/ClientIpResolver.cs). Sin red ni BD.
    public class ClientIpResolverTests
    {
        private static string Resolver(string remote, string forwardedFor, params string[] proxiesExtra)
        {
            return ClientIpResolver.Resolve(IPAddress.Parse(remote), forwardedFor, proxiesExtra);
        }

        [Fact]
        public void SinProxy_UsaLaIpDeLaConexion_IgnoraElHeader()
        {
            // Conexion directa desde internet: el header lo puso el propio cliente y no vale.
            Assert.Equal("203.0.113.9", Resolver("203.0.113.9", "1.2.3.4"));
        }

        [Fact]
        public void ProxyEnLoopback_TomaLaIpDelHeader()
        {
            Assert.Equal("203.0.113.9", Resolver("127.0.0.1", "203.0.113.9"));
        }

        [Fact]
        public void ProxyEnRedPrivada_TomaLaIpDelHeader()
        {
            Assert.Equal("203.0.113.9", Resolver("172.18.0.2", "203.0.113.9"));
            Assert.Equal("203.0.113.9", Resolver("192.168.0.151", "203.0.113.9"));
            Assert.Equal("203.0.113.9", Resolver("10.1.2.3", "203.0.113.9"));
        }

        [Fact]
        public void HeaderFalsificadoPorElCliente_SeIgnora_SeTomaLaIpQueAgregoElProxy()
        {
            // El cliente mando "X-Forwarded-For: 1.2.3.4" y el proxy le agrego a la derecha la IP real.
            // La primera entrada es del atacante; la que vale es la ultima no confiable.
            Assert.Equal("203.0.113.9", Resolver("127.0.0.1", "1.2.3.4, 203.0.113.9"));
        }

        [Fact]
        public void CadenaDeProxies_SaltaLosConfiables()
        {
            // cliente, proxy interno confiable
            Assert.Equal("203.0.113.9", Resolver("127.0.0.1", "203.0.113.9, 10.0.0.7"));
        }

        [Fact]
        public void ProxyPublicoNoListado_NoSeConfia()
        {
            Assert.Equal("198.51.100.4", Resolver("198.51.100.4", "203.0.113.9"));
        }

        [Fact]
        public void ProxyPublicoListadoEnConfiguracion_SeConfia()
        {
            Assert.Equal("203.0.113.9", Resolver("198.51.100.4", "203.0.113.9", "198.51.100.4"));
            Assert.Equal("203.0.113.9", Resolver("198.51.100.77", "203.0.113.9", "198.51.100.0/24"));
        }

        [Fact]
        public void HeaderIlegible_UsaLaIpDeLaConexion()
        {
            Assert.Equal("127.0.0.1", Resolver("127.0.0.1", "no-es-una-ip"));
            Assert.Equal("127.0.0.1", Resolver("127.0.0.1", "203.0.113.9, basura"));
        }

        [Fact]
        public void SinHeader_UsaLaIpDeLaConexion()
        {
            Assert.Equal("127.0.0.1", Resolver("127.0.0.1", ""));
            Assert.Equal("127.0.0.1", Resolver("127.0.0.1", null));
        }

        [Fact]
        public void Ipv4MapeadaEnIpv6_SeNormaliza()
        {
            Assert.Equal("203.0.113.9", ClientIpResolver.Resolve(IPAddress.Parse("::ffff:127.0.0.1"), "203.0.113.9"));
        }

        [Fact]
        public void IpConPuerto_SeParsea()
        {
            Assert.Equal("203.0.113.9", Resolver("127.0.0.1", "203.0.113.9:5555"));
        }

        [Fact]
        public void SinConexion_DevuelveUnknown()
        {
            Assert.Equal("unknown", ClientIpResolver.Resolve(null, "1.2.3.4"));
        }

        [Fact]
        public void ParsearConfiguracion_AceptaSeparadores()
        {
            var entradas = new System.Collections.Generic.List<string>(ClientIpResolver.ParsearConfiguracion("1.1.1.1, 2.2.2.0/24;3.3.3.3"));
            Assert.Equal(new[] { "1.1.1.1", "2.2.2.0/24", "3.3.3.3" }, entradas);
        }
    }
}
