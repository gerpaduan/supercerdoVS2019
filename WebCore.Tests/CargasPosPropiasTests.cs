// Tests de la regla "propio de la caja" de las cargas del POS (2026-11-19, docs/DECISIONS.md "Cargas desde el POS: modificar lo
// cargado"): desde cuando se cuentan las cargas del operador y a que origen de datos pertenece cada tipo de la lista.
using WebCore.Helpers;

namespace WebCore.Tests
{
    public class CargasPosPropiasTests
    {
        [Fact]
        public void Con_caja_abierta_cuenta_desde_la_apertura_menos_el_margen()
        {
            var apertura = new DateTime(2026, 11, 19, 8, 30, 0);
            var desde = CargasPosPropias.CalcularDesde(apertura, new DateTime(2026, 11, 19, 10, 0, 0), out bool conCaja);

            Assert.True(conCaja);
            Assert.Equal(new DateTime(2026, 11, 19, 8, 28, 0), desde);
        }

        [Fact]
        public void Sin_caja_cuenta_desde_hoy_a_las_cero_horas()
        {
            var desde = CargasPosPropias.CalcularDesde(null, new DateTime(2026, 11, 19, 15, 42, 10), out bool conCaja);

            Assert.False(conCaja);
            Assert.Equal(new DateTime(2026, 11, 19, 0, 0, 0), desde);
        }

        [Theory]
        [InlineData("ingreso", "stock")]
        [InlineData("egreso", "stock")]
        [InlineData("pesaje", "stock")]
        [InlineData("ajuste", "stock")]
        [InlineData("cierre", "stock")]
        [InlineData("movimiento", "movimiento")]
        [InlineData("elaborado", "elaborado")]
        [InlineData("rapido", "elaborado")]
        [InlineData("desarme", "elaborado")]
        [InlineData(" movimiento ", "movimiento")]
        public void Cada_tipo_de_la_lista_tiene_su_origen(string tipo, string origen)
        {
            Assert.Equal(origen, CargasPosPropias.OrigenDe(tipo));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("compra")]
        [InlineData("Ingreso")]
        [InlineData("stock; DROP TABLE compras")]
        public void Un_tipo_desconocido_no_tiene_origen(string? tipo)
        {
            Assert.Null(CargasPosPropias.OrigenDe(tipo));
        }

        [Fact]
        public void Todo_tipo_de_las_cargas_nuevas_tiene_origen_y_titulo()
        {
            foreach (var carga in CargasPosCatalogo.Todas)
            {
                Assert.NotNull(CargasPosPropias.OrigenDe(carga.Tipo));
                Assert.NotEqual("Carga", CargasPosPropias.TituloDe(carga.Tipo));
            }
        }

        [Fact]
        public void El_titulo_de_un_tipo_desconocido_es_generico()
        {
            Assert.Equal("Carga", CargasPosPropias.TituloDe("otro"));
            Assert.Equal("Carga", CargasPosPropias.TituloDe(null));
        }
    }
}
