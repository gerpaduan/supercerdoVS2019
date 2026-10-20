using System.Linq;
using Entidades;
using Negocio;
using Xunit;

namespace NegocioTests
{
    // Reglas puras de "Ingresar pedido" en Existencia por sucursales (Negocio.PedidoSugerencia, docs/DECISIONS.md 2026-10-19):
    // sugerencia = consumo del periodo - stock, aviso "stock suficiente", chip "bajo punto", modos de "Sugerir segun" e
    // importe estimado. El navegador (existencia-pedido.js) espeja estas reglas: los mismos casos se probaron contra el JS.
    public class PedidoSugerenciaTests
    {
        // ---- Sugerencia por consumo (modo por defecto) ----

        [Fact]
        public void Consumo_SugiereLoQueFaltaParaCubrirElConsumo()
        {
            // Ejemplo del manual: vendido 80,5 + elaborados 12 = 92,5 de consumo; hay 40 -> sugerido 52,5.
            var r = PedidoSugerencia.Calcular(stock: 40, punto: 0, consumo: 92.5, segun: PedidoSugerencia.SegunConsumo);

            Assert.Equal(52.5, r.Sugerido);
            Assert.Equal(52.5, r.FaltaPorConsumo);
            Assert.False(r.StockSuficiente);
        }

        [Fact]
        public void Consumo_SiElStockAlcanzaNoSugiereNadaYAvisaStockSuficiente()
        {
            var r = PedidoSugerencia.Calcular(stock: 100, punto: 60, consumo: 92.5, segun: PedidoSugerencia.SegunConsumo);

            Assert.Equal(0, r.Sugerido);
            Assert.False(r.BajoPunto);
            Assert.True(r.StockSuficiente);
        }

        [Fact]
        public void Consumo_StockIgualAlConsumoNoSugiere()
        {
            var r = PedidoSugerencia.Calcular(stock: 92.5, punto: 0, consumo: 92.5, segun: null);

            Assert.Equal(0, r.Sugerido);
            Assert.True(r.StockSuficiente);
        }

        [Fact]
        public void Consumo_SinConsumoNiPuntoNoHayDatoQueRespalde()
        {
            // Sin consumo y sin punto cargado no se afirma "stock suficiente": no hay con que compararlo.
            var r = PedidoSugerencia.Calcular(stock: 5, punto: 0, consumo: 0, segun: PedidoSugerencia.SegunConsumo);

            Assert.Equal(0, r.Sugerido);
            Assert.False(r.StockSuficiente);
            Assert.False(r.BajoPunto);
        }

        [Fact]
        public void Consumo_StockNegativoSeTomaComoCero()
        {
            // Stock -5 (error de carga): no hay que reponer lo que "falta" de menos de nada.
            var r = PedidoSugerencia.Calcular(stock: -5, punto: 0, consumo: 10, segun: PedidoSugerencia.SegunConsumo);

            Assert.Equal(10, r.Sugerido);
            Assert.Equal(0, r.Stock);
        }

        [Fact]
        public void Consumo_RedondeaATresDecimales()
        {
            var r = PedidoSugerencia.Calcular(stock: 0.1, punto: 0, consumo: 0.3456, segun: PedidoSugerencia.SegunConsumo);

            Assert.Equal(0.246, r.Sugerido);
        }

        // ---- Bajo el punto de stock ----

        [Fact]
        public void BajoPunto_OfreceCompletarLaDiferenciaEntreStockYPunto()
        {
            var r = PedidoSugerencia.Calcular(stock: 40, punto: 60, consumo: 0, segun: PedidoSugerencia.SegunConsumo);

            Assert.True(r.BajoPunto);
            Assert.Equal(20, r.FaltaHastaPunto);
            Assert.Equal(0, r.Sugerido); // sin consumo no hay pista; el chip del punto aparece igual
            Assert.False(r.StockSuficiente);
        }

        [Fact]
        public void BajoPunto_StockIgualAlPuntoNoEstaBajo()
        {
            var r = PedidoSugerencia.Calcular(stock: 60, punto: 60, consumo: 0, segun: PedidoSugerencia.SegunPunto);

            Assert.False(r.BajoPunto);
            Assert.Equal(0, r.Sugerido);
            Assert.True(r.StockSuficiente);
        }

        [Fact]
        public void BajoPunto_SinPuntoCargadoNuncaEstaBajo()
        {
            var r = PedidoSugerencia.Calcular(stock: 0, punto: 0, consumo: 0, segun: PedidoSugerencia.SegunPunto);

            Assert.False(r.BajoPunto);
            Assert.Equal(0, r.Sugerido);
        }

        [Fact]
        public void BajoPunto_StockNegativoCompletaDesdeCero()
        {
            var r = PedidoSugerencia.Calcular(stock: -3, punto: 60, consumo: 0, segun: PedidoSugerencia.SegunPunto);

            Assert.True(r.BajoPunto);
            Assert.Equal(60, r.Sugerido);
        }

        // ---- Sugerir segun ----

        [Theory]
        [InlineData("consumo", 52.5)]   // 92,5 - 40
        [InlineData("punto", 20)]        // 60 - 40
        [InlineData("mayor", 52.5)]
        [InlineData("", 52.5)]           // vacio = por defecto (consumo)
        [InlineData("cualquiercosa", 52.5)]
        public void Segun_EligeLaFuenteDeLaSugerencia(string segun, double esperado)
        {
            var r = PedidoSugerencia.Calcular(stock: 40, punto: 60, consumo: 92.5, segun: segun);

            Assert.Equal(esperado, r.Sugerido);
        }

        [Fact]
        public void Segun_MayorTomaElPuntoSiEsMasGrandeQueElConsumo()
        {
            var r = PedidoSugerencia.Calcular(stock: 10, punto: 100, consumo: 30, segun: PedidoSugerencia.SegunMayor);

            Assert.Equal(90, r.Sugerido);       // punto - stock
            Assert.Equal(20, r.FaltaPorConsumo); // consumo - stock
        }

        [Fact]
        public void Segun_ValoresInvalidosNoRompen()
        {
            var r = PedidoSugerencia.Calcular(double.NaN, double.PositiveInfinity, -4, "punto");

            Assert.Equal(0, r.Sugerido);
            Assert.Equal(0, r.Stock);
            Assert.False(r.BajoPunto);
        }

        // ---- Importe estimado ----

        [Fact]
        public void Importe_EsCantidadPorPrecioADosDecimales()
        {
            Assert.Equal(1500000.00m, PedidoSugerencia.Importe(100, 15000m));
            Assert.Equal(26.88m, PedidoSugerencia.Importe(2.5, 10.75m)); // 26,875 -> 26,88 (Math.Round ToEven: 26,88)
        }

        [Fact]
        public void Importe_UsaLaMismaRegloDeRedondeoQueQuePedir()
        {
            // Math.Round(decimal, 2) sin modo = redondeo bancario (al par): 0,125 -> 0,12 y 0,135 -> 0,14.
            Assert.Equal(0.12m, PedidoSugerencia.Importe(0.125, 1m));
            Assert.Equal(0.14m, PedidoSugerencia.Importe(0.135, 1m));
        }

        [Theory]
        [InlineData(5.0, null)]
        [InlineData(5.0, 0.0)]
        [InlineData(5.0, -1.0)]
        [InlineData(0.0, 100.0)]
        [InlineData(-2.0, 100.0)]
        public void Importe_SinPrecioOSinCantidadEsNullYNoCeroEnSilencio(double cantidad, double? precio)
        {
            Assert.Null(PedidoSugerencia.Importe(cantidad, precio.HasValue ? (decimal?)(decimal)precio.Value : null));
        }

        // ---- Equivalente en unidades ----

        [Fact]
        public void EquivalenteUnidades_SoloPesablesConPromedio()
        {
            Assert.Equal(12.5, PedidoSugerencia.EquivalenteUnidades(25, 2, true));
            Assert.Null(PedidoSugerencia.EquivalenteUnidades(25, 2, false));
            Assert.Null(PedidoSugerencia.EquivalenteUnidades(25, 0, true));
            Assert.Null(PedidoSugerencia.EquivalenteUnidades(0, 2, true));
        }

        // ---- Precios de la ultima compra ----

        private static ProveedorHabitualVm Habitual(int idCorte, decimal? precio, string proveedor = "Prov")
        {
            return new ProveedorHabitualVm { IdCorte = idCorte, IdProveedor = 1, Proveedor = proveedor, UltimoPrecio = precio };
        }

        [Fact]
        public void PreciosVigentes_DescartaSinPrecioOCeroYUnSoloPrecioPorProducto()
        {
            var habituales = new[]
            {
                Habitual(1, 15000m),
                Habitual(2, null),      // sin precio: no es $0, queda afuera
                Habitual(3, 0m),
                Habitual(4, -5m),
                Habitual(1, 99m, "Otro") // repetido: se queda el primero
            };

            var precios = PedidoSugerencia.PreciosVigentes(habituales);

            Assert.Single(precios);
            Assert.Equal(15000m, precios[0].UltimoPrecio);
            Assert.Equal("Prov", precios[0].Proveedor);
        }

        [Fact]
        public void PreciosVigentes_ListaNulaDevuelveVacio()
        {
            Assert.Empty(PedidoSugerencia.PreciosVigentes(null));
        }

        // ---- Consumo del periodo ----

        private static ActividadStockVm Actividad(int idCorte, int idSucursal, float ventas, float elaboracion)
        {
            return new ActividadStockVm { IdCorte = idCorte, IdSucursal = idSucursal, VentasVentana = ventas, ElaboracionVentana = elaboracion };
        }

        [Fact]
        public void ConsumoDeSucursales_RecortaPorSucursalYDescartaLoQueNoSeConsumio()
        {
            var actividad = new[]
            {
                Actividad(1, 1, 80.5f, 12f),   // vendido + elaborado
                Actividad(2, 1, 0f, 5f),        // solo uso en elaborados (cuenta como consumo)
                Actividad(3, 1, 0f, 0f),        // sin consumo: se descarta
                Actividad(1, 2, 7f, 0f)         // otra sucursal
            };

            var soloSucursal1 = PedidoSugerencia.ConsumoDeSucursales(actividad, new[] { 1 });
            var todas = PedidoSugerencia.ConsumoDeSucursales(actividad, new int[0]);

            Assert.Equal(new[] { 1, 2 }, soloSucursal1.Select(x => x.IdCorte).OrderBy(x => x).ToArray());
            Assert.Equal(3, todas.Count);
            Assert.DoesNotContain(todas, x => x.IdCorte == 3);
        }

        [Fact]
        public void ConsumoDeSucursales_ListaNulaDevuelveVacio()
        {
            Assert.Empty(PedidoSugerencia.ConsumoDeSucursales(null, new[] { 1 }));
        }
    }
}
