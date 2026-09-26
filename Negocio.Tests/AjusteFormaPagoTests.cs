using Negocio;
using Xunit;

namespace NegocioTests
{
    // Cubre Negocio.AjusteFormaPago: conversion porcentaje <-> factor (el POS lee factor, el
    // usuario carga porcentaje) y validacion del porcentaje tipeado en el modal de Productos.
    public class AjusteFormaPagoTests
    {
        [Theory]
        [InlineData(0, 1.0)]
        [InlineData(10, 1.10)]
        [InlineData(-5, 0.95)]
        [InlineData(3.25, 1.0325)]
        public void PorcentajeAFactor_convierte_a_factor_del_POS(double porcentaje, double esperado)
        {
            Assert.Equal((decimal)esperado, AjusteFormaPago.PorcentajeAFactor((decimal)porcentaje));
        }

        [Theory]
        [InlineData(1.0, 0)]
        [InlineData(1.10, 10)]
        [InlineData(0.95, -5)]
        [InlineData(1.0325, 3.25)]
        public void FactorAPorcentaje_es_la_inversa(double factor, double esperado)
        {
            Assert.Equal((decimal)esperado, AjusteFormaPago.FactorAPorcentaje((decimal)factor));
        }

        [Fact]
        public void FactorATexto_usa_punto_decimal_invariante()
        {
            Assert.Equal("1.1", AjusteFormaPago.FactorATexto(1.10m));
            Assert.Equal("1", AjusteFormaPago.FactorATexto(1m));
            Assert.Equal("0.95", AjusteFormaPago.FactorATexto(0.95m));
        }

        [Theory]
        [InlineData("10", 10)]
        [InlineData("+10", 10)]
        [InlineData("-5", -5)]
        [InlineData("-5,5", -5.5)]
        [InlineData("3.25", 3.25)]
        [InlineData(" 7 ", 7)]
        [InlineData("500", 500)]
        [InlineData("-99.99", -99.99)]
        public void TryParsePorcentaje_acepta_valores_validos(string texto, double esperado)
        {
            Assert.True(AjusteFormaPago.TryParsePorcentaje(texto, out var porcentaje));
            Assert.Equal((decimal)esperado, porcentaje);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("abc")]
        [InlineData("10%")]
        [InlineData("1e2")]
        [InlineData("1,000.5")]
        [InlineData("-100")]      // precio 0: el POS lo descarta
        [InlineData("-150")]
        [InlineData("500.01")]
        [InlineData("1000")]
        [InlineData("3.256")]     // mas de 2 decimales: se rechaza, no se redondea en silencio
        public void TryParsePorcentaje_rechaza_valores_invalidos(string texto)
        {
            Assert.False(AjusteFormaPago.TryParsePorcentaje(texto, out _));
        }

        [Fact]
        public void Formas_no_incluye_CtaCte_ni_Billetera_y_usa_las_claves_del_POS()
        {
            var claves = new System.Collections.Generic.List<string>();
            foreach (var forma in AjusteFormaPago.Formas) claves.Add(forma.Clave);

            Assert.Equal(new[] { "Efectivo", "Debito", "Credito", "Qr", "Transferencia" }, claves);
        }
    }
}
