using Xunit;

namespace NegocioTests
{
    // Regla del interruptor "Saldar cuenta" del alta de pagos (2026-10-22, ver docs/DECISIONS.md).
    // Convencion de signo: saldo negativo = la persona debe -> cobro; saldo positivo = a favor -> pago.
    public class SaldarCuentaTests
    {
        [Fact]
        public void SaldoNegativo_EsCobroPorLaDeuda()
        {
            var r = Negocio.SaldarCuenta.Resolver(-20191.29m);

            Assert.True(r.PuedeSaldar);
            Assert.False(r.AProveedor);
            Assert.Equal(20191.29m, r.Importe);
        }

        [Fact]
        public void SaldoPositivo_EsPagoPorElSaldoAFavor()
        {
            var r = Negocio.SaldarCuenta.Resolver(350.5m);

            Assert.True(r.PuedeSaldar);
            Assert.True(r.AProveedor);
            Assert.Equal(350.5m, r.Importe);
        }

        [Fact]
        public void SaldoCero_NoSePuedeSaldar()
        {
            var r = Negocio.SaldarCuenta.Resolver(0m);

            Assert.False(r.PuedeSaldar);
            Assert.Equal(0m, r.Importe);
        }

        [Fact]
        public void SaldoMenorAUnCentavo_SeConsideraSaldado()
        {
            Assert.False(Negocio.SaldarCuenta.Resolver(0.004m).PuedeSaldar);
            Assert.False(Negocio.SaldarCuenta.Resolver(-0.004m).PuedeSaldar);
        }

        [Fact]
        public void RedondeaACentavos_AlejandoseDeCero()
        {
            Assert.Equal(100.13m, Negocio.SaldarCuenta.Resolver(-100.125m).Importe);
            Assert.Equal(100.13m, Negocio.SaldarCuenta.Resolver(100.125m).Importe);
        }
    }
}
