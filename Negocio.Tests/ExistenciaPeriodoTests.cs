// Tests de la regla del periodo (desde obligatorio, hasta opcional) de Existencia por sucursales: Negocio.ExistenciaPeriodo.Validar.
// Lo usan el filtro "Con movimiento" y las sugerencias de "Ingresar pedido". Ver docs/DECISIONS.md (2026-10-20, "Periodo con fecha hasta").
using System;
using Negocio;
using Xunit;

namespace Negocio.Tests
{
    public class ExistenciaPeriodoTests
    {
        // Martes 20/10/2026 a media tarde.
        private static readonly DateTime Ahora = new DateTime(2026, 10, 20, 15, 30, 0);

        private static string Validar(DateTime? desde, DateTime? hasta, out DateTime desdeFecha, out DateTime? limite)
        {
            return ExistenciaPeriodo.Validar(desde, hasta, Ahora, out desdeFecha, out limite);
        }

        [Fact]
        public void SoloDesde_EsValido_YNoHayLimiteSuperior()
        {
            string error = Validar(new DateTime(2026, 9, 20), null, out DateTime desde, out DateTime? limite);

            Assert.Null(error);
            Assert.Equal(new DateTime(2026, 9, 20), desde);
            Assert.Null(limite); // sin "hasta": se cuenta hasta ahora
        }

        [Fact]
        public void SinDesde_SePideLaFecha()
        {
            string error = Validar(null, null, out _, out _);

            Assert.Equal(ExistenciaPeriodo.MensajeDesdeObligatorio, error);
        }

        [Fact]
        public void SinDesde_AunqueHayaHasta_SePideElDesde()
        {
            string error = Validar(null, new DateTime(2026, 10, 1), out _, out _);

            Assert.Equal(ExistenciaPeriodo.MensajeDesdeObligatorio, error);
        }

        [Fact]
        public void DesdeFuturo_SeRechaza()
        {
            string error = Validar(new DateTime(2026, 10, 21), null, out _, out _);

            Assert.Equal(ExistenciaPeriodo.MensajeDesdeFuturo, error);
        }

        [Fact]
        public void DesdeHoy_EsValido()
        {
            // Hoy a las 00:00 ya paso (son las 15:30): es un periodo valido.
            Assert.Null(Validar(new DateTime(2026, 10, 20), null, out _, out _));
        }

        [Fact]
        public void DesdeMasViejoQueElTope_SeRechaza_YElLimiteExactoSeAcepta()
        {
            DateTime limite = Ahora.Date.AddDays(-ExistenciaPeriodo.MaxDiasHaciaAtras);

            Assert.Null(Validar(limite, null, out _, out _));
            string error = Validar(limite.AddDays(-1), null, out _, out _);
            Assert.NotNull(error);
            Assert.Contains(ExistenciaPeriodo.MaxDiasHaciaAtras.ToString(), error);
        }

        [Fact]
        public void LaHoraDelDesdeSeIgnora()
        {
            string error = Validar(new DateTime(2026, 9, 20, 18, 45, 10), null, out DateTime desde, out _);

            Assert.Null(error);
            Assert.Equal(new DateTime(2026, 9, 20), desde);
        }

        [Fact]
        public void ConHasta_ElLimiteEsElUltimoInstanteDeEseDia()
        {
            string error = Validar(new DateTime(2026, 9, 1), new DateTime(2026, 9, 30), out DateTime desde, out DateTime? limite);

            Assert.Null(error);
            Assert.Equal(new DateTime(2026, 9, 1), desde);
            Assert.True(limite.HasValue);
            // El dia 30 entero entra (23:59:59.9999999) y el 1/10 a las 00:00 ya no.
            Assert.Equal(new DateTime(2026, 9, 30).AddDays(1).AddTicks(-1), limite.Value);
            Assert.True(limite.Value < new DateTime(2026, 10, 1));
            Assert.True(limite.Value > new DateTime(2026, 9, 30, 23, 59, 59));
        }

        [Fact]
        public void LaHoraDelHastaSeIgnora_ElDiaEnteroEntra()
        {
            Validar(new DateTime(2026, 9, 1), new DateTime(2026, 9, 30, 8, 0, 0), out _, out DateTime? limite);

            Assert.Equal(new DateTime(2026, 9, 30).AddDays(1).AddTicks(-1), limite.Value);
        }

        [Fact]
        public void UnPeriodoDeUnSoloDia_EsValido()
        {
            string error = Validar(new DateTime(2026, 10, 5), new DateTime(2026, 10, 5), out _, out DateTime? limite);

            Assert.Null(error);
            Assert.Equal(new DateTime(2026, 10, 6).AddTicks(-1), limite.Value);
        }

        [Fact]
        public void HastaAnteriorAlDesde_SeRechaza_YNoDevuelveLimite()
        {
            string error = Validar(new DateTime(2026, 10, 5), new DateTime(2026, 10, 4), out _, out DateTime? limite);

            Assert.Equal(ExistenciaPeriodo.MensajeHastaAnterior, error);
            Assert.Null(limite);
        }

        [Fact]
        public void HastaFuturo_SeRechaza()
        {
            string error = Validar(new DateTime(2026, 10, 1), new DateTime(2026, 10, 21), out _, out DateTime? limite);

            Assert.Equal(ExistenciaPeriodo.MensajeHastaFuturo, error);
            Assert.Null(limite);
        }

        [Fact]
        public void HastaHoy_EsValido()
        {
            string error = Validar(new DateTime(2026, 10, 1), new DateTime(2026, 10, 20, 23, 0, 0), out _, out DateTime? limite);

            Assert.Null(error);
            Assert.True(limite.HasValue);
        }

        [Fact]
        public void UnDesdeInvalido_ProduceElErrorDelDesde_AunqueElHastaTambienFalle()
        {
            // Futuro y ademas hasta anterior: se informa primero el desde (el usuario arregla un campo por vez, en el orden en que los ve).
            string error = Validar(new DateTime(2026, 11, 1), new DateTime(2026, 10, 1), out _, out _);

            Assert.Equal(ExistenciaPeriodo.MensajeDesdeFuturo, error);
        }
    }
}
