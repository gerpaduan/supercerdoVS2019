// Tests del parametro formasPagoFacturaAutomatica (2026-11-10): que formas de pago abren la factura sola en el POS.
// Valor guardado <-> lista de formas, con el default de siempre cuando no hay valor.
using System.Linq;
using WebCore.Helpers;
using WebCore.Models;
using Xunit;

namespace WebCore.Tests
{
    public class FormasPagoFacturaAutomaticaTests
    {
        private static readonly string[] DefectoDeSiempre = { "Debito", "Credito", "Qr", "Transferencia" };

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Parse_SinValor_UsaElDefaultDeSiempre(string? valor)
        {
            Assert.Equal(DefectoDeSiempre, FormasPagoFacturaAutomatica.Parse(valor));
        }

        [Theory]
        [InlineData("Ninguna")]
        [InlineData("ninguna")]
        [InlineData("  NINGUNA ")]
        public void Parse_Ninguna_NoAbreNunca(string valor)
        {
            Assert.Empty(FormasPagoFacturaAutomatica.Parse(valor));
        }

        [Fact]
        public void Parse_ConEfectivo_ReconoceTodoYRespetaElOrdenDeLasOpciones()
        {
            var lista = FormasPagoFacturaAutomatica.Parse("Transferencia, qr,EFECTIVO,Debito,Credito");

            Assert.Equal(new[] { "Efectivo", "Debito", "Credito", "Qr", "Transferencia" }, lista);
        }

        [Fact]
        public void Parse_IgnoraDesconocidos_YSiSoloHayDesconocidosUsaElDefault()
        {
            Assert.Equal(new[] { "Qr" }, FormasPagoFacturaAutomatica.Parse("Qr,Bitcoin"));
            Assert.Equal(DefectoDeSiempre, FormasPagoFacturaAutomatica.Parse("Bitcoin"));
        }

        [Fact]
        public void Parse_CtaCteNoEsOpcion()
        {
            Assert.DoesNotContain("CtaCte", FormasPagoFacturaAutomatica.Opciones.Select(o => o.Clave));
            Assert.DoesNotContain("CtaCte", FormasPagoFacturaAutomatica.Parse("CtaCte,Qr"));
        }

        [Fact]
        public void Serializar_OrdenaYDevuelveNingunaSiNoHayNinguna()
        {
            Assert.Equal("Efectivo,Qr", FormasPagoFacturaAutomatica.Serializar(new[] { "qr", "Efectivo" }));
            Assert.Equal("Ninguna", FormasPagoFacturaAutomatica.Serializar(new string[0]));
            Assert.Equal("Ninguna", FormasPagoFacturaAutomatica.Serializar(null));
            Assert.Equal("Ninguna", FormasPagoFacturaAutomatica.Serializar(new[] { "Bitcoin" }));
        }

        [Fact]
        public void IdaYVuelta_ConservaLaSeleccion()
        {
            foreach (var seleccion in new[] { "Efectivo", "Debito,Credito", "Efectivo,Debito,Credito,Qr,Transferencia" })
            {
                var lista = FormasPagoFacturaAutomatica.Parse(seleccion);
                Assert.Equal(seleccion, FormasPagoFacturaAutomatica.Serializar(lista));
            }

            Assert.Empty(FormasPagoFacturaAutomatica.Parse(FormasPagoFacturaAutomatica.Serializar(new string[0])));
        }

        [Fact]
        public void Catalogo_TieneElEditorDeCasillasParaElParametro()
        {
            var ayuda = ParametroAyudaCatalogo.Obtener("formasPagoFacturaAutomatica", "");

            Assert.Equal(ParametroAyudaCatalogo.EditorFormasPago, ayuda.Editor);
            Assert.Equal(ParametroAyudaCatalogo.GrupoPuntoDeVenta, ayuda.Grupo);
        }
    }
}
