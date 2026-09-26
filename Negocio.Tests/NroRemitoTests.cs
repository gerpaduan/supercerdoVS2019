using Negocio;
using Xunit;

namespace NegocioTests
{
    // Cubre Negocio.NroRemito: formato PPPP-NNNNNNNN por sucursal, lectura del correlativo y
    // la leyenda "REMITOS ASOCIADOS: a / b" en las observaciones de la venta.
    public class NroRemitoTests
    {
        [Theory]
        [InlineData(3, 12, "0003-00000012")]
        [InlineData(1, 1, "0001-00000001")]
        [InlineData(125, 99999999, "0125-99999999")]
        public void Formatear_arma_prefijo_de_sucursal_y_correlativo(int idSucursal, long correlativo, string esperado)
        {
            Assert.Equal(esperado, NroRemito.Formatear(idSucursal, correlativo));
        }

        [Fact]
        public void Siguiente_sin_remitos_previos_arranca_en_uno()
        {
            Assert.Equal("0002-00000001", NroRemito.Siguiente(2, 0));
        }

        [Fact]
        public void Siguiente_suma_uno_al_ultimo()
        {
            Assert.Equal("0002-00000013", NroRemito.Siguiente(2, 12));
        }

        [Theory]
        [InlineData("0002-00000012", 2, true, 12)]
        [InlineData(" 0002-00000012 ", 2, true, 12)]
        [InlineData("0003-00000012", 2, false, 0)]      // otra sucursal
        [InlineData("REM-15", 2, false, 0)]              // formato libre editado a mano
        [InlineData("0002-", 2, false, 0)]
        [InlineData("0002-12a", 2, false, 0)]
        [InlineData("", 2, false, 0)]
        [InlineData(null, 2, false, 0)]
        public void TryLeerCorrelativo_solo_reconoce_el_formato_de_la_sucursal(string nro, int idSucursal, bool esperadoOk, long esperadoCorrelativo)
        {
            bool ok = NroRemito.TryLeerCorrelativo(nro, idSucursal, out long correlativo);

            Assert.Equal(esperadoOk, ok);
            Assert.Equal(esperadoCorrelativo, correlativo);
        }

        [Fact]
        public void Normalizar_recorta_y_compacta_espacios()
        {
            Assert.Equal("A 15", NroRemito.Normalizar("  A    15 "));
            Assert.Equal("", NroRemito.Normalizar(null));
        }

        [Fact]
        public void ComponerLeyenda_un_remito()
        {
            Assert.Equal("REMITOS ASOCIADOS: 0001-00000005", NroRemito.ComponerLeyenda(new[] { "0001-00000005" }));
        }

        [Fact]
        public void ComponerLeyenda_varios_se_separan_con_barra()
        {
            Assert.Equal(
                "REMITOS ASOCIADOS: 0001-00000005 / 0001-00000006 / 0002-00000001",
                NroRemito.ComponerLeyenda(new[] { "0001-00000005", "0001-00000006", "0002-00000001" }));
        }

        [Fact]
        public void ComponerLeyenda_ignora_vacios_y_repetidos()
        {
            Assert.Equal(
                "REMITOS ASOCIADOS: A / B",
                NroRemito.ComponerLeyenda(new[] { "A", "", "  ", "a", "B", null }));
        }

        [Fact]
        public void ComponerLeyenda_sin_remitos_es_vacia()
        {
            Assert.Equal("", NroRemito.ComponerLeyenda(new string[0]));
            Assert.Equal("", NroRemito.ComponerLeyenda(null));
        }

        [Fact]
        public void AplicarAObservaciones_agrega_la_leyenda_al_final_conservando_el_texto()
        {
            string resultado = NroRemito.AplicarAObservaciones("Entregar por la tarde", new[] { "0001-00000005" });

            Assert.Equal("Entregar por la tarde\nREMITOS ASOCIADOS: 0001-00000005", resultado);
        }

        [Fact]
        public void AplicarAObservaciones_sin_observaciones_previas_deja_solo_la_leyenda()
        {
            Assert.Equal("REMITOS ASOCIADOS: X", NroRemito.AplicarAObservaciones("", new[] { "X" }));
            Assert.Equal("REMITOS ASOCIADOS: X", NroRemito.AplicarAObservaciones(null, new[] { "X" }));
        }

        [Fact]
        public void AplicarAObservaciones_es_idempotente_al_guardar_de_nuevo()
        {
            string primera = NroRemito.AplicarAObservaciones("Nota", new[] { "A", "B" });
            string segunda = NroRemito.AplicarAObservaciones(primera, new[] { "A", "B" });

            Assert.Equal(primera, segunda);
        }

        [Fact]
        public void AplicarAObservaciones_reemplaza_una_leyenda_anterior_con_otros_remitos()
        {
            string resultado = NroRemito.AplicarAObservaciones("Nota\nREMITOS ASOCIADOS: A", new[] { "A", "B" });

            Assert.Equal("Nota\nREMITOS ASOCIADOS: A / B", resultado);
        }

        [Fact]
        public void AplicarAObservaciones_suma_los_remitos_nuevos_a_los_que_ya_figuraban()
        {
            string resultado = NroRemito.AplicarAObservaciones("Nota\nREMITOS ASOCIADOS: A / B", new[] { "C" });

            Assert.Equal("Nota\nREMITOS ASOCIADOS: A / B / C", resultado);
        }

        [Fact]
        public void AplicarAObservaciones_sin_remitos_no_toca_las_observaciones()
        {
            Assert.Equal("Nota\nREMITOS ASOCIADOS: A", NroRemito.AplicarAObservaciones("Nota\nREMITOS ASOCIADOS: A", new string[0]));
        }
    }
}
