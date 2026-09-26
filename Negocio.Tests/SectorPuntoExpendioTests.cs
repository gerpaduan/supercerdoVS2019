using System;
using Negocio;
using Xunit;

namespace NegocioTests
{
    // Cubre Negocio.SectorPuntoExpendio: nombres reservados (PRESUPUESTO / REMITOS) y regla de
    // fecha del expendio por sector (REMITOS nunca futura, PRESUPUESTO futura con tope, resto
    // ignora la fecha manual).
    public class SectorPuntoExpendioTests
    {
        private static readonly DateTime Ahora = new DateTime(2026, 9, 26, 10, 0, 0);

        [Theory]
        [InlineData("PRESUPUESTO", true)]
        [InlineData("presupuesto", true)]
        [InlineData("  Presupuesto ", true)]
        [InlineData("REMITOS", false)]
        [InlineData("Carnicería", false)]
        [InlineData("", false)]
        [InlineData(null, false)]
        public void EsPresupuesto_compara_sin_mayusculas_ni_espacios(string sector, bool esperado)
        {
            Assert.Equal(esperado, SectorPuntoExpendio.EsPresupuesto(sector));
        }

        [Theory]
        [InlineData("REMITOS", true)]
        [InlineData("remitos", true)]
        [InlineData("Remito", false)]
        [InlineData("PRESUPUESTO", false)]
        public void EsRemitos_exige_el_nombre_completo(string sector, bool esperado)
        {
            Assert.Equal(esperado, SectorPuntoExpendio.EsRemitos(sector));
        }

        [Theory]
        [InlineData("PRESUPUESTO", true)]
        [InlineData("remitos", true)]
        [InlineData("Carnicería", false)]
        [InlineData("Ramos Generales", false)]
        public void EsReservado_solo_para_los_dos_sectores_globales(string sector, bool esperado)
        {
            Assert.Equal(esperado, SectorPuntoExpendio.EsReservado(sector));
        }

        [Fact]
        public void ResolverFecha_sin_fecha_solicitada_usa_ahora()
        {
            var fecha = SectorPuntoExpendio.ResolverFecha("REMITOS", null, Ahora, out string error);

            Assert.Null(error);
            Assert.Equal(Ahora, fecha);
        }

        [Fact]
        public void ResolverFecha_otro_sector_ignora_la_fecha_manual()
        {
            var fecha = SectorPuntoExpendio.ResolverFecha("Carnicería", Ahora.AddDays(-3), Ahora, out string error);

            Assert.Null(error);
            Assert.Equal(Ahora, fecha);
        }

        [Fact]
        public void ResolverFecha_remitos_acepta_fecha_pasada()
        {
            var pedida = Ahora.AddDays(-40);

            var fecha = SectorPuntoExpendio.ResolverFecha("REMITOS", pedida, Ahora, out string error);

            Assert.Null(error);
            Assert.Equal(pedida, fecha);
        }

        [Fact]
        public void ResolverFecha_remitos_rechaza_fecha_futura()
        {
            SectorPuntoExpendio.ResolverFecha("REMITOS", Ahora.AddDays(1), Ahora, out string error);

            Assert.NotNull(error);
        }

        [Fact]
        public void ResolverFecha_remitos_tolera_reloj_del_cliente_levemente_adelantado()
        {
            var fecha = SectorPuntoExpendio.ResolverFecha("REMITOS", Ahora.AddMinutes(3), Ahora, out string error);

            Assert.Null(error);
            Assert.Equal(Ahora, fecha);
        }

        [Fact]
        public void ResolverFecha_presupuesto_acepta_fecha_futura_dentro_del_tope()
        {
            var pedida = Ahora.AddDays(30);

            var fecha = SectorPuntoExpendio.ResolverFecha("Presupuesto", pedida, Ahora, out string error);

            Assert.Null(error);
            Assert.Equal(pedida, fecha);
        }

        [Fact]
        public void ResolverFecha_presupuesto_rechaza_fecha_mas_alla_del_tope()
        {
            SectorPuntoExpendio.ResolverFecha(
                "PRESUPUESTO",
                Ahora.AddDays(SectorPuntoExpendio.DiasMaximosFuturoPresupuesto + 1),
                Ahora,
                out string error);

            Assert.NotNull(error);
        }
    }
}
