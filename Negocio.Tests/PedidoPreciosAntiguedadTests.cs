// Tests de la regla "un precio de compra demasiado viejo no se usa" para estimar el importe de un pedido
// (Negocio.PedidoSugerencia.SepararPorAntiguedad). Motivo: con la inflacion, un precio de hace años subestima el importe en ordenes de
// magnitud (medido 2026-10-19: Carre a $76/kg de 2016 contra $9.900 de venta). Ver docs/DECISIONS.md (2026-10-19, "Pedidos: fuente del precio").
using System;
using System.Linq;
using Entidades;
using Negocio;
using Xunit;

namespace Negocio.Tests
{
    public class PedidoPreciosAntiguedadTests
    {
        private static readonly DateTime Ahora = new DateTime(2026, 10, 19, 14, 30, 0);
        private const int Maximo = 365;

        private static ProveedorHabitualVm Compra(int idCorte, decimal? precio, DateTime? fecha, string proveedor = "Prov")
        {
            return new ProveedorHabitualVm { IdCorte = idCorte, IdProveedor = 1, Proveedor = proveedor, UltimoPrecio = precio, FechaUltimaCompra = fecha };
        }

        [Fact]
        public void UnaCompraReciente_DaPrecio()
        {
            var r = PedidoSugerencia.SepararPorAntiguedad(new[] { Compra(1, 5600m, Ahora.AddDays(-30)) }, Ahora, Maximo);

            Assert.Single(r.Vigentes);
            Assert.Empty(r.DescartadosPorAntiguedad);
            Assert.Equal(5600m, r.Vigentes[0].UltimoPrecio);
        }

        [Fact]
        public void UnaCompraDeHaceAnios_NoDaPrecioYQuedaDescartada()
        {
            // Carre: $76,23 el kg en 2016 (hoy se vende a $9.900).
            var r = PedidoSugerencia.SepararPorAntiguedad(new[] { Compra(1, 76.23m, new DateTime(2016, 11, 24)) }, Ahora, Maximo);

            Assert.Empty(r.Vigentes);
            Assert.Single(r.DescartadosPorAntiguedad);
            Assert.Equal(1, r.DescartadosPorAntiguedad[0].IdCorte);
        }

        [Fact]
        public void ElLimiteEsExclusivo_365DiasSiYMasDeUnAnioNo()
        {
            var r = PedidoSugerencia.SepararPorAntiguedad(new[]
            {
                Compra(1, 100m, Ahora.Date.AddDays(-365)),   // justo en el limite: se usa
                Compra(2, 100m, Ahora.Date.AddDays(-366))    // un dia mas: se descarta
            }, Ahora, Maximo);

            Assert.Equal(new[] { 1 }, r.Vigentes.Select(x => x.IdCorte).ToArray());
            Assert.Equal(new[] { 2 }, r.DescartadosPorAntiguedad.Select(x => x.IdCorte).ToArray());
        }

        [Fact]
        public void SinFechaDeCompra_SeConserva()
        {
            // No se puede afirmar que sea viejo: se usa (misma regla que PedidoTextos.PrecioViejo).
            var r = PedidoSugerencia.SepararPorAntiguedad(new[] { Compra(1, 100m, null) }, Ahora, Maximo);

            Assert.Single(r.Vigentes);
            Assert.Empty(r.DescartadosPorAntiguedad);
        }

        [Fact]
        public void SinPrecioOCero_NoApareceEnNingunaLista()
        {
            var r = PedidoSugerencia.SepararPorAntiguedad(new[]
            {
                Compra(1, null, Ahora.AddDays(-1)),
                Compra(2, 0m, Ahora.AddDays(-1)),
                Compra(3, -5m, new DateTime(2016, 1, 1))
            }, Ahora, Maximo);

            Assert.Empty(r.Vigentes);
            Assert.Empty(r.DescartadosPorAntiguedad);
        }

        [Fact]
        public void UnSoloRegistroPorProducto_ElPrimeroGana()
        {
            var r = PedidoSugerencia.SepararPorAntiguedad(new[]
            {
                Compra(1, 5600m, Ahora.AddDays(-10), "Reciente"),
                Compra(1, 76m, new DateTime(2016, 1, 1), "Viejo")
            }, Ahora, Maximo);

            Assert.Single(r.Vigentes);
            Assert.Equal("Reciente", r.Vigentes[0].Proveedor);
            Assert.Empty(r.DescartadosPorAntiguedad);
        }

        [Fact]
        public void ListaNula_DevuelveListasVacias()
        {
            var r = PedidoSugerencia.SepararPorAntiguedad(null, Ahora, Maximo);

            Assert.Empty(r.Vigentes);
            Assert.Empty(r.DescartadosPorAntiguedad);
        }

        [Fact]
        public void ElLimiteSeRespetaCuandoSeConfiguraMasCorto()
        {
            var r = PedidoSugerencia.SepararPorAntiguedad(new[] { Compra(1, 100m, Ahora.AddDays(-100)) }, Ahora, 90);

            Assert.Empty(r.Vigentes);
            Assert.Single(r.DescartadosPorAntiguedad);
        }
    }
}
