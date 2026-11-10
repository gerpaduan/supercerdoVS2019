// Tests de la pantalla Parametros (rediseño 2026-11-10): el catalogo de ayuda cubre los parametros que la web usa,
// un parametro desconocido igual se muestra, y los 7 que la web no lee quedan ocultos sin pisar los ocultos de antes.
using System.Linq;
using WebCore.Controllers;
using WebCore.Models;
using Xunit;

namespace WebCore.Tests
{
    public class ParametrosAyudaTests
    {
        // Los 4 parametros que WebCore realmente lee (relevamiento del 2026-11-10).
        private static readonly string[] VisiblesEnWeb =
        {
            "saldoMinimoInformar", "codProdGenerico", "ocultarEncabezadoTicketNoFiscal", "diasLimitFechaDesde"
        };

        // Solo escritorio (5) + sin uso (2): siguen en la base pero no se muestran en la web.
        private static readonly string[] OcultosPorNoUsarseEnWeb =
        {
            "importeMaxRedondeo", "limiteKgParaAjuste", "loginRapidoElaborado", "loginRapidoMovimiento", "mayuscula",
            "loginRapidoStock", "minAccesoUltimaVentaVendedor"
        };

        [Fact]
        public void Catalogo_TieneEntradaPropiaParaLosParametrosVisibles()
        {
            foreach (var nombre in VisiblesEnWeb)
            {
                var ayuda = ParametroAyudaCatalogo.Obtener(nombre, "descripcion de la base");

                Assert.NotEqual(nombre, ayuda.Titulo);
                Assert.NotEqual(ParametroAyudaCatalogo.GrupoOtros, ayuda.Grupo);
                Assert.False(string.IsNullOrWhiteSpace(ayuda.Explicacion));
                Assert.Contains(ayuda.Grupo, ParametroAyudaCatalogo.OrdenGrupos);
            }
        }

        [Fact]
        public void Catalogo_ParametroDesconocido_UsaNombreYDescripcionDeLaBase()
        {
            var ayuda = ParametroAyudaCatalogo.Obtener("parametroNuevo", "  Descripcion original  ");

            Assert.Equal("parametroNuevo", ayuda.Titulo);
            Assert.Equal("Descripcion original", ayuda.Explicacion);
            Assert.Equal(ParametroAyudaCatalogo.GrupoOtros, ayuda.Grupo);
        }

        [Fact]
        public void Catalogo_NoDistingueMayusculas()
        {
            var ayuda = ParametroAyudaCatalogo.Obtener("SALDOMINIMOINFORMAR", "");

            Assert.Equal(ParametroAyudaCatalogo.GrupoPuntoDeVenta, ayuda.Grupo);
        }

        [Fact]
        public void Visibilidad_MuestraLosQueLaWebUsa()
        {
            Assert.All(VisiblesEnWeb, nombre => Assert.True(ParametrosController.DebeMostrarParametro(nombre), nombre));
        }

        [Fact]
        public void Visibilidad_OcultaLosQueLaWebNoUsa()
        {
            Assert.All(OcultosPorNoUsarseEnWeb, nombre => Assert.False(ParametrosController.DebeMostrarParametro(nombre), nombre));
        }

        [Fact]
        public void Visibilidad_ConservaLosOcultosDeSiempre()
        {
            string[] ocultosDeAntes = { "idConsumidorFinal", "idCtaCteEgresoCaja", "porcAjEfectivo", "porcAjQr", "porcAjBilletera" };

            Assert.All(ocultosDeAntes, nombre => Assert.False(ParametrosController.DebeMostrarParametro(nombre), nombre));
        }

        [Fact]
        public void Visibilidad_ParametroNuevoSeMuestra_YNombreVacioNo()
        {
            Assert.True(ParametrosController.DebeMostrarParametro("parametroNuevo"));
            Assert.False(ParametrosController.DebeMostrarParametro("  "));
            Assert.False(ParametrosController.DebeMostrarParametro(null));
        }
    }
}
