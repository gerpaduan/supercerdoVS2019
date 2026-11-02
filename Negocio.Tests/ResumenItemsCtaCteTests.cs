using System;
using System.Globalization;
using Negocio;
using NegocioTests.Fakes;
using Xunit;

namespace NegocioTests
{
    // Linea gris de cada movimiento de la cuenta corriente de una persona ("3 articulos · 13,35 kg"), 2026-11-02, ver
    // docs/DECISIONS.md "Cuenta corriente: filas desplegables". La consulta SQL real se prueba contra la base local;
    // aca se cubre el armado del diccionario y el texto, sin base de datos.
    public class ResumenItemsCtaCteTests
    {
        private static readonly CultureInfo EsAr = new CultureInfo("es-AR");

        private static (Negocio.CuentaCorriente ctaCte, FakeCuentaCorrienteRepository repo) CrearSut()
        {
            var repo = new FakeCuentaCorrienteRepository();
            var ctaCte = new Negocio.CuentaCorriente(repo, new EmpresaContextFake(1));
            return (ctaCte, repo);
        }

        // --- Descripcion ---

        [Fact]
        public void Venta_ConVariosArticulos_DiceCantidadYKilosConComa()
        {
            var resumen = new ResumenItemsMovimientoCtaCte { EsCompra = false, Cantidad = 3, Kg = 13.35m };

            Assert.Equal("3 artículos · 13,35 kg", resumen.Descripcion(EsAr));
        }

        [Fact]
        public void Venta_ConUnSoloArticulo_VaEnSingular()
        {
            var resumen = new ResumenItemsMovimientoCtaCte { EsCompra = false, Cantidad = 1, Kg = 5.5m };

            Assert.Equal("1 artículo · 5,5 kg", resumen.Descripcion(EsAr));
        }

        [Fact]
        public void Compra_DeCortes_DiceProductos()
        {
            var resumen = new ResumenItemsMovimientoCtaCte { EsCompra = true, Cantidad = 2, Kg = 52.5m };

            Assert.Equal("2 productos · 52,5 kg", resumen.Descripcion(EsAr));
        }

        [Fact]
        public void Compra_DeMediasReses_DiceMedias()
        {
            var resumen = new ResumenItemsMovimientoCtaCte { EsCompra = true, Medias = 2, Kg = 218.5m };

            Assert.Equal("2 medias · 218,5 kg", resumen.Descripcion(EsAr));
        }

        [Fact]
        public void Compra_ConCortesYMedias_LasUneConY()
        {
            var resumen = new ResumenItemsMovimientoCtaCte { EsCompra = true, Cantidad = 1, Medias = 1, Kg = 120m };

            Assert.Equal("1 producto y 1 media · 120 kg", resumen.Descripcion(EsAr));
        }

        [Fact]
        public void SinItems_NoDiceNada()
        {
            var resumen = new ResumenItemsMovimientoCtaCte { EsCompra = false, Cantidad = 0, Medias = 0, Kg = 0m };

            Assert.Equal("", resumen.Descripcion(EsAr));
        }

        // --- obtenerResumenItemsCtaCte: arma el diccionario "tabla|idTabla" ---

        [Fact]
        public void ArmaElDiccionarioPorTablaEId()
        {
            var (ctaCte, repo) = CrearSut();
            repo.ResumenItems.Rows.Add("Ventas", 101, 3, 0, 13.35);
            repo.ResumenItems.Rows.Add("Compras", 101, 0, 2, 218.5);

            var resumenes = ctaCte.obtenerResumenItemsCtaCte(7, new DateTime(2026, 11, 2));

            // La misma clave numerica en tablas distintas no se pisa: la tabla va en la clave.
            Assert.Equal(2, resumenes.Count);
            Assert.False(resumenes["Ventas|101"].EsCompra);
            Assert.Equal(3, resumenes["Ventas|101"].Cantidad);
            Assert.True(resumenes["Compras|101"].EsCompra);
            Assert.Equal(2, resumenes["Compras|101"].Medias);
            Assert.Equal(218.5m, resumenes["Compras|101"].Kg);
        }

        [Fact]
        public void LaBusquedaDeLaClaveNoDistingueMayusculas()
        {
            var (ctaCte, repo) = CrearSut();
            repo.ResumenItems.Rows.Add("Ventas", 5, 1, 0, 2.0);

            var resumenes = ctaCte.obtenerResumenItemsCtaCte(7, new DateTime(2026, 11, 2));

            Assert.True(resumenes.ContainsKey("ventas|5"));
        }

        [Fact]
        public void SinFilas_DevuelveDiccionarioVacio()
        {
            var (ctaCte, _) = CrearSut();

            Assert.Empty(ctaCte.obtenerResumenItemsCtaCte(7, new DateTime(2026, 11, 2)));
        }
    }
}
