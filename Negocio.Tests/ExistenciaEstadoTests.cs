using System.Linq;
using Negocio;
using Xunit;

namespace NegocioTests
{
    // Cubre el estado de una celda de Existencia por sucursales (Negocio.ExistenciaEstado, docs/DECISIONS.md 2026-10-19):
    // la regla (igual a la de la consulta de existencia pero contra el punto REAL de la sucursal), el filtro de la barra y el orden.
    public class ExistenciaEstadoTests
    {
        [Theory]
        [InlineData(-3, 40, ExistenciaEstado.Negativo, ExistenciaEstado.RangoNegativo)]
        [InlineData(-3, 0, ExistenciaEstado.Negativo, ExistenciaEstado.RangoNegativo)]      // negativo gana aunque no haya punto
        [InlineData(10, 40, ExistenciaEstado.Falta, ExistenciaEstado.RangoBajoElPunto)]
        [InlineData(0, 10, ExistenciaEstado.Falta, ExistenciaEstado.RangoBajoElPunto)]      // BAJO gana sobre "sin stock" cuando hay punto
        [InlineData(40, 40, ExistenciaEstado.EnElPunto, ExistenciaEstado.RangoBajoElPunto)] // la regla BAJO es stock <= punto
        [InlineData(41, 40, ExistenciaEstado.Ok, ExistenciaEstado.RangoOk)]
        [InlineData(0, 0, ExistenciaEstado.SinStock, ExistenciaEstado.RangoSinPunto)]
        [InlineData(12.5, 0, ExistenciaEstado.SinPunto, ExistenciaEstado.RangoSinPunto)]
        public void Calcular_ClasificaSegunStockYPunto(double stock, double punto, string codigoEsperado, int rangoEsperado)
        {
            var estado = ExistenciaEstado.Calcular(stock, punto);

            Assert.Equal(codigoEsperado, estado.Codigo);
            Assert.Equal(rangoEsperado, estado.Rango);
        }

        [Fact]
        public void Calcular_Falta_InformaCuantoFaltaParaElPunto()
        {
            var estado = ExistenciaEstado.Calcular(18.75, 25);

            Assert.Equal(6.25, estado.Faltante, 6);
            Assert.Equal(0, ExistenciaEstado.Calcular(30, 25).Faltante);
            Assert.Equal(0, ExistenciaEstado.Calcular(25, 25).Faltante);
        }

        [Fact]
        public void ClaseFiltro_AgrupaFaltantesBienYSinPunto()
        {
            Assert.Equal("falta", ExistenciaEstado.Calcular(-1, 10).ClaseFiltro);
            Assert.Equal("falta", ExistenciaEstado.Calcular(5, 10).ClaseFiltro);
            Assert.Equal("falta", ExistenciaEstado.Calcular(10, 10).ClaseFiltro);
            Assert.Equal("bien", ExistenciaEstado.Calcular(11, 10).ClaseFiltro);
            Assert.Equal("", ExistenciaEstado.Calcular(11, 0).ClaseFiltro);
        }

        [Fact]
        public void SinPuntoCargado_EsPuntoCero_SinImportarElStock()
        {
            Assert.True(ExistenciaEstado.Calcular(0, 0).SinPuntoCargado);
            Assert.True(ExistenciaEstado.Calcular(50, 0).SinPuntoCargado);
            Assert.True(ExistenciaEstado.Calcular(-2, 0).SinPuntoCargado);
            Assert.False(ExistenciaEstado.Calcular(0, 10).SinPuntoCargado);
        }

        [Fact]
        public void ClaveOrden_PonePrimeroLosPeores_YDentroDelRangoElMayorFaltante()
        {
            // Negativo, falta 20, falta 5, en el punto, OK con poco margen, OK con mucho margen, sin punto.
            var ordenado = new[]
            {
                ExistenciaEstado.Calcular(50, 0),     // sin punto
                ExistenciaEstado.Calcular(100, 10),   // OK holgado
                ExistenciaEstado.Calcular(11, 10),    // OK justo
                ExistenciaEstado.Calcular(10, 10),    // en el punto
                ExistenciaEstado.Calcular(5, 10),     // falta 5
                ExistenciaEstado.Calcular(-1, 10),    // negativo
                ExistenciaEstado.Calcular(0, 20)      // falta 20
            };

            var codigos = ordenado.OrderBy(x => x.ClaveOrden).Select(x => x.Codigo + ":" + x.Stock).ToArray();

            Assert.Equal(new[]
            {
                "NEGATIVO:-1", "FALTA:0", "FALTA:5", "EN_PUNTO:10", "OK:11", "OK:100", "SIN_PUNTO:50"
            }, codigos);
        }
    }
}
