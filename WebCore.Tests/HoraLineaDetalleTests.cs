using System;
using WebCore.Helpers;
using Xunit;

namespace WebCore.Tests
{
    // Hora de carga de las lineas del detalle de Compras / Stock en la tarjeta de celular: solo hora el mismo dia,
    // fecha y hora (resaltada) si la linea se cargo otro dia que el registro.
    public class HoraLineaDetalleTests
    {
        private static readonly DateTime Registro = new DateTime(2026, 9, 16, 8, 0, 0);

        [Fact]
        public void MismoDia_SoloHora()
        {
            var r = HoraLineaDetalle.Formatear("16/09/2026 10:37", Registro);
            Assert.Equal("10:37", r.Texto);
            Assert.False(r.OtroDia);
        }

        [Fact]
        public void OtroDiaMismoAnio_FechaSinAnio()
        {
            var r = HoraLineaDetalle.Formatear("17/09/2026 08:15", Registro);
            Assert.Equal("17/09 08:15", r.Texto);
            Assert.True(r.OtroDia);
        }

        [Fact]
        public void DiaAnterior_TambienSeMarca()
        {
            var r = HoraLineaDetalle.Formatear("15/09/2026 23:50", Registro);
            Assert.Equal("15/09 23:50", r.Texto);
            Assert.True(r.OtroDia);
        }

        [Fact]
        public void OtroAnio_FechaConAnio()
        {
            var r = HoraLineaDetalle.Formatear("02/01/2027 09:05", Registro);
            Assert.Equal("02/01/27 09:05", r.Texto);
            Assert.True(r.OtroDia);
        }

        [Fact]
        public void SinFechaDeRegistro_SoloHora()
        {
            var r = HoraLineaDetalle.Formatear("17/09/2026 08:15", null);
            Assert.Equal("08:15", r.Texto);
            Assert.False(r.OtroDia);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("-")]
        [InlineData("2026-09-16 10:37")]
        public void SinDatoOIlegible_Vacio(string creado)
        {
            var r = HoraLineaDetalle.Formatear(creado, Registro);
            Assert.Equal("", r.Texto);
            Assert.False(r.OtroDia);
        }
    }
}
