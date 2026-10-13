// Pruebas de lo que AFIP aporta a los titulares fiscales (2026-10-09): el filtro de puntos de venta activos que trae
// ARCA (ConsultarPuntosVentaService) y que dos CUIT de la misma empresa conviven en la misma raiz AFIP/ sin pisarse.
using System.IO;
using System.Linq;
using AFIP;
using AFIP.WSFECore;

namespace AFIP.Tests
{
    public class TitularesFiscalesAfipTests
    {
        // ---- Puntos de venta activos ----

        [Theory]
        [InlineData("N", null, true)]
        [InlineData("N", "", true)]
        [InlineData("N", "NULL", true)]
        [InlineData("n", " null ", true)]
        [InlineData(null, null, true)]
        [InlineData("S", null, false)]
        [InlineData("s", "NULL", false)]
        [InlineData("SI", "", false)]
        [InlineData("N", "20240131", false)]
        [InlineData("S", "20240131", false)]
        public void EsActivo_NoBloqueadoYSinFechaDeBaja(string? bloqueado, string? fchBaja, bool esperado)
        {
            Assert.Equal(esperado, ConsultarPuntosVentaService.EsActivo(bloqueado!, fchBaja!));
        }

        [Fact]
        public void FiltrarActivos_DejaSoloLosHabilitadosOrdenadosYIgnoraNulosYNumeroCero()
        {
            var puntos = new PtoVenta[]
            {
                new PtoVenta { Nro = 7, EmisionTipo = "RECE para aplicativo y web services", Bloqueado = "N", FchBaja = "NULL" },
                new PtoVenta { Nro = 3, EmisionTipo = "Factura Electronica - Monotributo - Web Services", Bloqueado = "N", FchBaja = "" },
                new PtoVenta { Nro = 4, EmisionTipo = "x", Bloqueado = "S", FchBaja = "NULL" },
                new PtoVenta { Nro = 5, EmisionTipo = "x", Bloqueado = "N", FchBaja = "20230101" },
                new PtoVenta { Nro = 0, EmisionTipo = "x", Bloqueado = "N", FchBaja = "NULL" },
            };

            var activos = ConsultarPuntosVentaService.FiltrarActivos(puntos);

            Assert.Equal(new[] { 3, 7 }, activos.Select(p => p.Codigo).ToArray());
        }

        [Fact]
        public void FiltrarActivos_ConListaNulaDevuelveVacio()
        {
            Assert.Empty(ConsultarPuntosVentaService.FiltrarActivos(null!));
        }

        // ---- Dos CUIT (dos titulares) en la misma raiz AFIP/ ----

        private static string Temporal()
        {
            string dir = Path.Combine(Path.GetTempPath(), "titulares-" + System.Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(dir);
            return dir;
        }

        [Fact]
        public void Resolver_DosCuitDistintosUsanCarpetasDistintasPorEntorno()
        {
            string baseDir = Temporal();
            try
            {
                var empresa = AfipRutas.Resolver(baseDir, "20306210786", false, "");
                var titular = AfipRutas.Resolver(baseDir, "27111111113", false, "");
                var titularHomo = AfipRutas.Resolver(baseDir, "27111111113", true, "");

                Assert.NotEqual(empresa.Carpeta, titular.Carpeta);
                Assert.NotEqual(titular.Carpeta, titularHomo.Carpeta);
                Assert.Equal(Path.Combine(baseDir, "AFIP", "27111111113", "prod"), titular.Carpeta);
                Assert.Equal(Path.Combine(baseDir, "AFIP", "27111111113", "homo"), titularHomo.Carpeta);
                // Ninguna ruta de un titular cuelga de la carpeta de otro CUIT (carpetas planas por CUIT).
                Assert.DoesNotContain("20306210786", titular.RutaPfx);
            }
            finally { Directory.Delete(baseDir, true); }
        }

        [Fact]
        public void Resolver_ElPfxHistoricoDeUnCuitNoSeUsaParaOtro()
        {
            string baseDir = Temporal();
            try
            {
                // La empresa tiene su pfx historico en la raiz de su CUIT; el titular no tiene nada cargado.
                string raizEmpresa = AfipRutas.Carpeta(baseDir, "20306210786");
                Directory.CreateDirectory(raizEmpresa);
                File.WriteAllText(Path.Combine(raizEmpresa, "certif-prod.pfx"), "x");

                var empresa = AfipRutas.Resolver(baseDir, "20306210786", false, "");
                var titular = AfipRutas.Resolver(baseDir, "27111111113", false, "");

                Assert.True(empresa.EsLegado);
                Assert.False(titular.EsLegado);
                Assert.False(File.Exists(titular.RutaPfx));
            }
            finally { Directory.Delete(baseDir, true); }
        }

        [Fact]
        public void Resolver_ConNombreLegadoPropioDelTitularBuscaEseArchivoEnLaRaizDeSuCuit()
        {
            string baseDir = Temporal();
            try
            {
                string raizTitular = AfipRutas.Carpeta(baseDir, "27111111113");
                Directory.CreateDirectory(raizTitular);
                File.WriteAllText(Path.Combine(raizTitular, "mi-titular.pfx"), "x");

                var conNombre = AfipRutas.Resolver(baseDir, "27111111113", false, "mi-titular.pfx");
                var sinNombre = AfipRutas.Resolver(baseDir, "27111111113", false, "");

                Assert.True(conNombre.EsLegado);
                Assert.EndsWith("mi-titular.pfx", conNombre.RutaPfx);
                // Sin el nombre propio cae en certif-prod.pfx (que no existe): usa prod/.
                Assert.False(sinNombre.EsLegado);
            }
            finally { Directory.Delete(baseDir, true); }
        }
    }
}
