using WebCore.Services;
using Xunit;

namespace WebCore.Tests
{
    // Los exports de Ventas > Lineas reciben sus textos por defecto de LineasExportTextos.Ventas: si cambian,
    // cambia el archivo que ya descargan los usuarios (Expendios generados usa los mismos generadores con otros textos).
    public class LineasExportTextosTests
    {
        [Fact]
        public void TextosDeVentas_ConservanLosOriginales()
        {
            var textos = LineasExportTextos.Ventas;

            Assert.Equal("Líneas de venta", textos.Titulo);
            Assert.Equal("No se encontraron ventas con líneas para los filtros seleccionados.", textos.MensajeSinRegistros);
            Assert.Equal(new[] { "Cliente", "Vendedor", "Forma de pago", "Sucursal", "Nro/comprobante" }, textos.EtiquetasDatos);
            // null = los filtros se arman desde el modelo (comportamiento de Ventas).
            Assert.Null(textos.Filtros);
        }
    }
}
