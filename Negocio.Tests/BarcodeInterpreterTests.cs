using Entidades;
using Negocio;
using NegocioTests.Fakes;
using Xunit;

namespace NegocioTests
{
    // Cubre Negocio.BarcodeInterpreter.Interpretar -- motor de codigos de barra internos de
    // balanza (EAN-13, prefijo 20-29), configurable por empresa. Los codigos de ejemplo usados
    // en estos tests tienen digito verificador EAN-13 real y valido (calculado a mano con el
    // mismo algoritmo de Utilidades.ValidacionEan.EsEan13Valido) -- si el checksum no diera
    // valido, ya caerian en el Caso CodigoInvalido antes de llegar a la logica que se quiere
    // probar.
    public class BarcodeInterpreterTests
    {
        private const int Empresa1 = 1;
        private const int Empresa2 = 2;

        // Prefijo 20, PLU en posicion 3 (5 digitos), valor en posicion 8 (5 digitos, 3
        // decimales, Cantidad). PLU=00123 (123), valor crudo=01250 -> 1.250 kg.
        private const string CodigoPrefijo20 = "2000123012506";

        // Prefijo 25, PLU en posicion 3 (5 digitos), valor en posicion 8 (5 digitos, 2
        // decimales, Precio). PLU=00456 (456), valor crudo=01999 -> 19.99.
        private const string CodigoPrefijo25 = "2500456019994";

        // Prefijo 29, formato con posiciones DISTINTAS a los dos de arriba (PLU 3 digitos en
        // posicion 3, valor 6 digitos en posicion 6, 3 decimales, Cantidad) -- confirma que
        // nada esta hardcodeado. PLU=007 (7), valor crudo=000895 -> 0.895 kg.
        private const string CodigoPrefijo29 = "2900700089504";

        // Prefijo 21, checksum valido, todo cero salvo el prefijo -- usado con un formato de
        // posiciones corruptas para probar que nunca tira excepcion.
        private const string CodigoPrefijo21 = "2100000000005";

        // EAN-13 comercial real (checksum valido), prefijo 77 -- fuera del rango 20-29.
        private const string CodigoComercialNormal = "7791234567898";

        private static BarcodeInterpreter CrearSut(FakeFormatoCodigoBarrasRepository formatos, FakeCorteBusquedaSimpleRepository cortes)
        {
            return new BarcodeInterpreter(formatos, cortes);
        }

        [Fact]
        public void EanComercialNormal_NoEsCodigoInterno()
        {
            var sut = CrearSut(new FakeFormatoCodigoBarrasRepository(), new FakeCorteBusquedaSimpleRepository());

            var resultado = sut.Interpretar(CodigoComercialNormal, Empresa1);

            Assert.Equal(CasoInterpretacionBarcode.CodigoInvalido, resultado.Caso);
            Assert.False(resultado.EsCodigoInterno);
        }

        [Fact]
        public void CodigoVacioONoNumerico_DevuelveCodigoInvalido()
        {
            var sut = CrearSut(new FakeFormatoCodigoBarrasRepository(), new FakeCorteBusquedaSimpleRepository());

            Assert.Equal(CasoInterpretacionBarcode.CodigoInvalido, sut.Interpretar("", Empresa1).Caso);
            Assert.Equal(CasoInterpretacionBarcode.CodigoInvalido, sut.Interpretar(null, Empresa1).Caso);
            Assert.Equal(CasoInterpretacionBarcode.CodigoInvalido, sut.Interpretar("ABC1234567890", Empresa1).Caso);
        }

        [Fact]
        public void Prefijo20SinFormato_DevuelvePrefijoSinFormato()
        {
            var sut = CrearSut(new FakeFormatoCodigoBarrasRepository(), new FakeCorteBusquedaSimpleRepository());

            var resultado = sut.Interpretar(CodigoPrefijo20, Empresa1);

            Assert.Equal(CasoInterpretacionBarcode.PrefijoSinFormato, resultado.Caso);
            Assert.False(resultado.EsCodigoInterno);
        }

        [Fact]
        public void Prefijo20ConFormatoCantidad_ExtraePesoCorrecto()
        {
            var formatos = new FakeFormatoCodigoBarrasRepository()
                .AgregarFormato(Empresa1, prefijo: 20, posicionCodigo: 3, longitudCodigo: 5, posicionValor: 8, longitudValor: 5, tipoValor: TipoValorCodigoBarras.Cantidad, cantidadDecimales: 3);
            var cortes = new FakeCorteBusquedaSimpleRepository().AgregarCorte(Empresa1, codigo: 123, descripcion: "Producto peso");
            var sut = CrearSut(formatos, cortes);

            var resultado = sut.Interpretar(CodigoPrefijo20, Empresa1);

            Assert.Equal(CasoInterpretacionBarcode.Interpretado, resultado.Caso);
            Assert.True(resultado.EsCodigoInterno);
            Assert.Equal(123, resultado.CodigoProductoInterno);
            Assert.Equal(TipoValorCodigoBarras.Cantidad, resultado.TipoValor);
            Assert.Equal(1.250m, resultado.Valor);
            Assert.NotNull(resultado.Producto);
            Assert.Equal("Producto peso", resultado.Producto.CorteDesc);
        }

        [Fact]
        public void Prefijo25ConFormatoPrecio_ExtraePrecioCorrecto()
        {
            var formatos = new FakeFormatoCodigoBarrasRepository()
                .AgregarFormato(Empresa1, prefijo: 25, posicionCodigo: 3, longitudCodigo: 5, posicionValor: 8, longitudValor: 5, tipoValor: TipoValorCodigoBarras.Precio, cantidadDecimales: 2);
            var cortes = new FakeCorteBusquedaSimpleRepository().AgregarCorte(Empresa1, codigo: 456, descripcion: "Producto precio");
            var sut = CrearSut(formatos, cortes);

            var resultado = sut.Interpretar(CodigoPrefijo25, Empresa1);

            Assert.Equal(CasoInterpretacionBarcode.Interpretado, resultado.Caso);
            Assert.Equal(456, resultado.CodigoProductoInterno);
            Assert.Equal(TipoValorCodigoBarras.Precio, resultado.TipoValor);
            Assert.Equal(19.99m, resultado.Valor);
        }

        [Fact]
        public void Prefijo29ConOtroFormatoDistinto_AplicaSuPropiaConfig()
        {
            var formatos = new FakeFormatoCodigoBarrasRepository()
                .AgregarFormato(Empresa1, prefijo: 29, posicionCodigo: 3, longitudCodigo: 3, posicionValor: 6, longitudValor: 6, tipoValor: TipoValorCodigoBarras.Cantidad, cantidadDecimales: 3);
            var cortes = new FakeCorteBusquedaSimpleRepository().AgregarCorte(Empresa1, codigo: 7, descripcion: "Producto prefijo 29");
            var sut = CrearSut(formatos, cortes);

            var resultado = sut.Interpretar(CodigoPrefijo29, Empresa1);

            Assert.Equal(CasoInterpretacionBarcode.Interpretado, resultado.Caso);
            Assert.Equal(7, resultado.CodigoProductoInterno);
            Assert.Equal(0.895m, resultado.Valor);
        }

        [Fact]
        public void CodigoInternoParseadoPeroProductoNoExiste_DevuelveProductoNoEncontrado()
        {
            var formatos = new FakeFormatoCodigoBarrasRepository()
                .AgregarFormato(Empresa1, prefijo: 25, posicionCodigo: 3, longitudCodigo: 5, posicionValor: 8, longitudValor: 5, tipoValor: TipoValorCodigoBarras.Precio, cantidadDecimales: 2);
            // Sin AgregarCorte: el catalogo de la empresa esta vacio.
            var sut = CrearSut(formatos, new FakeCorteBusquedaSimpleRepository());

            var resultado = sut.Interpretar(CodigoPrefijo25, Empresa1);

            Assert.Equal(CasoInterpretacionBarcode.ProductoNoEncontrado, resultado.Caso);
            Assert.True(resultado.EsCodigoInterno);
            Assert.Null(resultado.Producto);
        }

        [Fact]
        public void CodigoInternoConEstructuraInvalida_NuncaTiraExcepcion()
        {
            // PosicionCodigo=50 no entra en un EAN-13 (longitud 13) -- estructura corrupta a
            // proposito, para confirmar que Interpretar la detecta sin explotar.
            var formatos = new FakeFormatoCodigoBarrasRepository()
                .AgregarFormato(Empresa1, prefijo: 21, posicionCodigo: 50, longitudCodigo: 5, posicionValor: 8, longitudValor: 5, tipoValor: TipoValorCodigoBarras.Precio, cantidadDecimales: 2);
            var sut = CrearSut(formatos, new FakeCorteBusquedaSimpleRepository());

            var resultado = sut.Interpretar(CodigoPrefijo21, Empresa1);

            Assert.Equal(CasoInterpretacionBarcode.EstructuraInvalida, resultado.Caso);
            Assert.True(resultado.EsCodigoInterno);
        }

        [Fact]
        public void DosEmpresasMismoPrefijo_AislamientoPorEmpresa()
        {
            // Mismo codigo escaneado, mismo prefijo 20, pero cada empresa tiene su propio
            // formato (posiciones distintas) y su propio catalogo -- deben resolver a
            // productos y valores completamente distintos, sin cruzarse.
            var formatos = new FakeFormatoCodigoBarrasRepository()
                .AgregarFormato(Empresa1, prefijo: 20, posicionCodigo: 3, longitudCodigo: 5, posicionValor: 8, longitudValor: 5, tipoValor: TipoValorCodigoBarras.Cantidad, cantidadDecimales: 3)
                .AgregarFormato(Empresa2, prefijo: 20, posicionCodigo: 3, longitudCodigo: 4, posicionValor: 7, longitudValor: 6, tipoValor: TipoValorCodigoBarras.Precio, cantidadDecimales: 2);

            var cortes = new FakeCorteBusquedaSimpleRepository()
                .AgregarCorte(Empresa1, codigo: 123, descripcion: "ProductoEmpresa1")
                .AgregarCorte(Empresa2, codigo: 12, descripcion: "ProductoEmpresa2");

            var sut = CrearSut(formatos, cortes);

            var resultadoEmpresa1 = sut.Interpretar(CodigoPrefijo20, Empresa1);
            var resultadoEmpresa2 = sut.Interpretar(CodigoPrefijo20, Empresa2);

            Assert.Equal(CasoInterpretacionBarcode.Interpretado, resultadoEmpresa1.Caso);
            Assert.Equal(123, resultadoEmpresa1.CodigoProductoInterno);
            Assert.Equal(TipoValorCodigoBarras.Cantidad, resultadoEmpresa1.TipoValor);
            Assert.Equal(1.250m, resultadoEmpresa1.Valor);
            Assert.Equal("ProductoEmpresa1", resultadoEmpresa1.Producto.CorteDesc);

            Assert.Equal(CasoInterpretacionBarcode.Interpretado, resultadoEmpresa2.Caso);
            Assert.Equal(12, resultadoEmpresa2.CodigoProductoInterno);
            Assert.Equal(TipoValorCodigoBarras.Precio, resultadoEmpresa2.TipoValor);
            Assert.Equal(3012.50m, resultadoEmpresa2.Valor);
            Assert.Equal("ProductoEmpresa2", resultadoEmpresa2.Producto.CorteDesc);
        }
    }
}
