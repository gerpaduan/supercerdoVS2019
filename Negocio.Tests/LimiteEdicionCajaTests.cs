using System;
using Negocio;
using Xunit;

namespace NegocioTests
{
    // Regla "modificar desde el POS solo desde el inicio de la caja y dentro de los dias editar" (2026-11-02).
    // Ver docs/DECISIONS.md "POS: permisos por funcion".
    public class LimiteEdicionCajaTests
    {
        private static readonly DateTime Hoy = new DateTime(2026, 11, 2);

        [Fact]
        public void CajaDeHace3DiasYPermisoDe4_ElLimiteEsLaApertura()
        {
            var apertura = Hoy.AddDays(-3).AddHours(8);
            var limite = LimiteEdicionCaja.Calcular(apertura, Hoy.AddDays(-4));
            Assert.Equal(apertura, limite);
        }

        [Fact]
        public void CajaDeHace3DiasYPermisoDe0_ElLimiteEsHoy()
        {
            var limite = LimiteEdicionCaja.Calcular(Hoy.AddDays(-3).AddHours(8), Hoy);
            Assert.Equal(Hoy, limite);
        }

        [Fact]
        public void CajaDeHoyYPermisoDe4_ElLimiteEsLaApertura()
        {
            var apertura = Hoy.AddHours(8);
            Assert.Equal(apertura, LimiteEdicionCaja.Calcular(apertura, Hoy.AddDays(-4)));
        }

        [Fact]
        public void PermisoSinTopeDeDias_RigeSoloLaApertura()
        {
            var apertura = Hoy.AddDays(-1).AddHours(8);
            Assert.Equal(apertura, LimiteEdicionCaja.Calcular(apertura, null));
        }

        [Fact]
        public void SinCajaAbierta_NoSeEdita()
        {
            Assert.Null(LimiteEdicionCaja.Calcular(null, Hoy));
            Assert.False(LimiteEdicionCaja.PuedeEditar(Hoy, null, Hoy));
        }

        [Fact]
        public void FechaJustoEnElLimite_SeEdita() =>
            Assert.True(LimiteEdicionCaja.PuedeEditar(Hoy.AddHours(8), Hoy.AddHours(8), Hoy));

        [Fact]
        public void FechaAnteriorALaApertura_NoSeEdita() =>
            Assert.False(LimiteEdicionCaja.PuedeEditar(Hoy.AddDays(-3).AddHours(7), Hoy.AddDays(-3).AddHours(8), Hoy.AddDays(-4)));

        [Fact]
        public void FechaDentroDeLaCajaPeroFueraDeLosDias_NoSeEdita() =>
            Assert.False(LimiteEdicionCaja.PuedeEditar(Hoy.AddDays(-2), Hoy.AddDays(-3).AddHours(8), Hoy));
    }
}
