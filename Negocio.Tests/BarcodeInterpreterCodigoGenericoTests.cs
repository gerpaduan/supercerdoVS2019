using Entidades;
using Negocio;
using NegocioTests.Fakes;
using Xunit;

namespace NegocioTests
{
    // Cubre Negocio.BarcodeInterpreter.InterpretarCodigoGenerico -- migracion 1:1 (mismo regex,
    // mismo umbral) del mecanismo "codigo generico" que antes vivia duplicado en
    // VentasController.BuscarProducto y PuntosExpendioController.BuscarProductoPOS. Los casos
    // de aca blindan que la migracion no cambio el comportamiento original.
    public class BarcodeInterpreterCodigoGenericoTests
    {
        private const long CodigoBaseGenerico = 90000;

        private static BarcodeInterpreter CrearSut() =>
            new BarcodeInterpreter(new FakeFormatoCodigoBarrasRepository(), new FakeCorteBusquedaSimpleRepository());

        [Fact]
        public void MasDeUnPunto_DevuelveFormatoInvalido()
        {
            var resultado = CrearSut().InterpretarCodigoGenerico("12.50.30", ingresoCantidadX: true, codigoBaseGenerico: CodigoBaseGenerico);

            Assert.True(resultado.FormatoInvalido);
        }

        [Fact]
        public void CodigoEanNormal_ConIngresoCantidadXFalse_NoEsGenerico()
        {
            var resultado = CrearSut().InterpretarCodigoGenerico("7791234567898", ingresoCantidadX: false, codigoBaseGenerico: CodigoBaseGenerico);

            Assert.False(resultado.FormatoInvalido);
            Assert.False(resultado.EsGenerico);
        }

        [Fact]
        public void SufijoG_ConIngresoCantidadX_EsGenerico_SumaElNumeroDeG()
        {
            // "12.50G3" -- precio manual 12.50, generico base + 3 (ej. para variar por
            // alicuota de IVA sin cambiar el flujo de carga).
            var resultado = CrearSut().InterpretarCodigoGenerico("12.50G3", ingresoCantidadX: true, codigoBaseGenerico: CodigoBaseGenerico);

            Assert.False(resultado.FormatoInvalido);
            Assert.True(resultado.EsGenerico);
            Assert.Equal(CodigoBaseGenerico + 3, resultado.CodigoProducto);
            Assert.Equal(12.50f, resultado.PrecioManual);
        }

        [Fact]
        public void PrecioManualSinSufijoG_EsGenerico_UsaElCodigoBaseSinSumar()
        {
            var resultado = CrearSut().InterpretarCodigoGenerico("15.75", ingresoCantidadX: true, codigoBaseGenerico: CodigoBaseGenerico);

            Assert.True(resultado.EsGenerico);
            Assert.Equal(CodigoBaseGenerico, resultado.CodigoProducto);
            Assert.Equal(15.75f, resultado.PrecioManual);
        }

        [Fact]
        public void CodigoCortoSinPuntoNiG_ConIngresoCantidadX_EsGenericoPorLongitud()
        {
            // Longitud < 8 (umbral cantMinDig_EAN8 del mecanismo original) + ingresoCantidadX
            // ya alcanza para considerarlo generico, aunque no tenga "." ni "G".
            var resultado = CrearSut().InterpretarCodigoGenerico("1234", ingresoCantidadX: true, codigoBaseGenerico: CodigoBaseGenerico);

            Assert.True(resultado.EsGenerico);
            Assert.Equal(CodigoBaseGenerico, resultado.CodigoProducto);
            Assert.Equal(1234f, resultado.PrecioManual);
        }

        [Fact]
        public void CodigoCortoSinPuntoNiG_ConIngresoCantidadXFalse_NoEsGenerico()
        {
            // Mismo codigo corto que el test anterior, pero sin ingresoCantidadX -- la
            // condicion es un AND, asi que la longitud corta sola no alcanza.
            var resultado = CrearSut().InterpretarCodigoGenerico("1234", ingresoCantidadX: false, codigoBaseGenerico: CodigoBaseGenerico);

            Assert.False(resultado.EsGenerico);
        }
    }
}
