using System;
using Negocio;
using Xunit;

namespace NegocioTests
{
    // Regla "eliminar pagos/cobros de la propia caja abierta" (2026-10-13): cubre el caso permitido y cada motivo de
    // denegacion. Ver docs/DECISIONS.md "Eliminar pagos y cobros de la propia caja abierta".
    public class PagoEnCajaAbiertaTests
    {
        private static readonly DateTime Apertura = new DateTime(2026, 10, 13, 8, 0, 0);
        private const int Operador = 7;
        private const int Sucursal = 3;

        // Valores por defecto = caso permitido; cada test cambia un solo dato.
        private static bool Evaluar(
            DateTime? pagoCreado = null, int pagoCreadoPor = Operador, int pagoSucursal = Sucursal,
            int? egresoCreadoPor = null, int operador = Operador,
            DateTime? inicio = null, DateTime? cierre = null, int cajaSucursal = Sucursal)
        {
            return PagoEnCajaAbierta.Evaluar(
                pagoCreado ?? Apertura.AddHours(2), pagoCreadoPor, pagoSucursal,
                egresoCreadoPor, operador,
                inicio ?? Apertura, cierre, cajaSucursal);
        }

        [Fact]
        public void PagoCreadoPorElOperadorDespuesDeLaApertura_Permite() => Assert.True(Evaluar());

        [Fact]
        public void CreadoJustoALaApertura_Permite() => Assert.True(Evaluar(pagoCreado: Apertura));

        [Fact]
        public void CreadoAntesDeLaApertura_Deniega() => Assert.False(Evaluar(pagoCreado: Apertura.AddMinutes(-1)));

        [Fact]
        public void SinFechaDeCreacion_Deniega() =>
            Assert.False(PagoEnCajaAbierta.Evaluar(null, Operador, Sucursal, null, Operador, Apertura, null, Sucursal));

        [Fact]
        public void CajaCerrada_Deniega() => Assert.False(Evaluar(cierre: Apertura.AddHours(5)));

        [Fact]
        public void SinCajaAbierta_Deniega() =>
            Assert.False(PagoEnCajaAbierta.Evaluar(Apertura.AddHours(1), Operador, Sucursal, null, Operador, null, null, Sucursal));

        [Fact]
        public void PagoDeOtraSucursal_Deniega() => Assert.False(Evaluar(pagoSucursal: Sucursal + 1));

        [Fact]
        public void PagoSinSucursal_Deniega() => Assert.False(Evaluar(pagoSucursal: 0, cajaSucursal: 0));

        [Fact]
        public void CreadoPorOtroUsuarioSinEgreso_Deniega() => Assert.False(Evaluar(pagoCreadoPor: Operador + 1));

        [Fact]
        public void ConEgresoDelOperador_Permite_AunqueElPagoLoHayaCreadoLaCuentaCompartida() =>
            Assert.True(Evaluar(pagoCreadoPor: 99, egresoCreadoPor: Operador));

        [Fact]
        public void ConEgresoDeOtroOperador_Deniega_AunqueElPagoFigureComoDelOperador() =>
            Assert.False(Evaluar(pagoCreadoPor: Operador, egresoCreadoPor: Operador + 1));

        [Fact]
        public void CuentaCompartidaSinEgreso_Deniega() => Assert.False(Evaluar(pagoCreadoPor: 99));

        [Fact]
        public void OperadorInvalido_Deniega() => Assert.False(Evaluar(operador: 0, pagoCreadoPor: 0));
    }
}
