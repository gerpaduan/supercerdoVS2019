// Tests de la clasificacion de lineas para los filtros de descuento/recargo y anulaciones de Ventas > Lineas.
using System.Collections.Generic;
using System.Linq;
using Entidades;
using WebCore.Services;
using Xunit;

namespace WebCore.Tests
{
    public class LineasVentaClasificacionTests
    {
        private static LineaVenta Linea(int id, float kg = 1, float bonificacion = 0, float ajuste = 0, int estado = 0, int anulaA = 0)
            => new LineaVenta { IdLineaVenta = id, CantKg = kg, PrecioKg = 100, Bonificacion = bonificacion, AjustePrecio = ajuste, Estado = estado, IndexAnulado = anulaA };

        [Theory]
        [InlineData(10f, -10f, "desc", "Desc. 10 %")]
        [InlineData(-5f, 5f, "rec", "Rec. 5 %")]
        [InlineData(0f, -350f, "precio", "Lista - $ 350,00")]
        [InlineData(0f, 12.5f, "precio", "Lista + $ 12,50")]
        [InlineData(0f, 0f, "", "")]
        [InlineData(0.001f, 0.004f, "", "")]   // ruido de float
        public void Descuento_ElPorcentajeMandaYSinElDecideElAjuste(float bonificacion, float ajuste, string tipo, string texto)
        {
            Assert.Equal(tipo, LineasVentaClasificacion.TipoDescuento(bonificacion, ajuste));
            Assert.Equal(texto, LineasVentaClasificacion.TextoDescuento(bonificacion, ajuste));
        }

        [Fact]
        public void Anulacion_MarcaLaOriginalYSuContraYSoloMuestraUnaFilaPorItem()
        {
            var normal = Linea(1);
            var original = Linea(2, kg: 2);
            var contra = Linea(3, kg: -2, estado: 1, anulaA: 2);

            var marcas = LineasVentaClasificacion.Clasificar(new List<LineaVenta> { normal, original, contra });

            Assert.True(marcas.ConAnulacion);
            Assert.False(marcas.Cancelada);
            Assert.Equal("", marcas.Lineas[normal].TipoAnulacion);
            Assert.Equal("original", marcas.Lineas[original].TipoAnulacion);
            Assert.Equal("contra", marcas.Lineas[contra].TipoAnulacion);
            Assert.Equal(new[] { original }, new[] { normal, original, contra }.Where(l => marcas.Lineas[l].SoloAnulado));
        }

        [Fact]
        public void VentaCancelada_CuandoTodoQuedoAnulado()
        {
            var marcas = LineasVentaClasificacion.Clasificar(new List<LineaVenta> { Linea(1), Linea(2, kg: -1, estado: 1, anulaA: 1) });

            Assert.True(marcas.Cancelada);
        }

        [Fact]
        public void ContraSinOriginal_NoSePierdeEnSoloAnulado_NiMarcaLaVentaComoCancelada()
        {
            var suelta = Linea(5, kg: -1, estado: 1, anulaA: -1);

            var marcas = LineasVentaClasificacion.Clasificar(new List<LineaVenta> { suelta });

            Assert.True(marcas.Lineas[suelta].SoloAnulado);
            Assert.True(marcas.ConAnulacion);
            Assert.False(marcas.Cancelada);
        }

        [Fact]
        public void VentaSinAnulaciones_NoSeMarca()
        {
            // IndexAnulado = 0 es el valor de una linea normal (no apunta a ninguna).
            var marcas = LineasVentaClasificacion.Clasificar(new List<LineaVenta> { Linea(1), Linea(2) });

            Assert.False(marcas.ConAnulacion);
            Assert.False(marcas.Cancelada);
            Assert.DoesNotContain(marcas.Lineas.Values, m => m.SoloAnulado);
        }

        [Theory]
        [InlineData("desc", "", false, false, false)]    // la linea no tiene descuento
        [InlineData("", "", false, false, true)]
        [InlineData("", "ventas", false, false, false)]
        [InlineData("", "ventas", true, false, true)]
        [InlineData("", "solo", true, false, false)]
        [InlineData("", "solo", true, true, true)]
        public void Coincide_CombinaLosDosFiltros(string descuento, string anulaciones, bool ventaConAnulacion, bool soloAnulado, bool esperado)
        {
            var linea = new LineaVentaMarcas { SoloAnulado = soloAnulado };

            Assert.Equal(esperado, LineasVentaClasificacion.Coincide(linea, ventaConAnulacion, descuento, anulaciones));
        }

        [Theory]
        [InlineData(" DESC ", "desc", "")]
        [InlineData("solo", "", "solo")]
        [InlineData("cualquiera", "", "")]
        [InlineData(null, "", "")]
        public void FiltrosDeLaUrl_SoloSeAceptanLosConocidos(string? valor, string descuento, string anulaciones)
        {
            Assert.Equal(descuento, LineasVentaClasificacion.NormalizarFiltroDescuento(valor));
            Assert.Equal(anulaciones, LineasVentaClasificacion.NormalizarFiltroAnulaciones(valor));
        }
    }
}
