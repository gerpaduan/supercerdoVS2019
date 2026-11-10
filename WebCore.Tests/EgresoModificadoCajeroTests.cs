// Tests del texto "que cambio" de la advertencia del modal Cerrar Caja (WebCore.Helpers.EgresoModificadoCajero): una linea
// por campo cambiado (monto, tipo, fecha, sucursal, descripcion), con la tolerancia de centavos y la fecha a nivel minuto.
// Ver docs/DECISIONS.md (2026-11-10, "Advertencia de egresos modificados por el cajero al cerrar caja").
using WebCore.Helpers;

namespace WebCore.Tests
{
    public class EgresoModificadoCajeroTests
    {
        // Modificacion "sin cambios": todos los valores anteriores iguales a los nuevos.
        private static ModificacionEgresoCaja SinCambios() => new ModificacionEgresoCaja
        {
            MontoAnterior = 500, MontoNuevo = 500,
            IdTipoAnterior = 4, IdTipoNuevo = 4, TipoAnterior = "Gastos Varios", TipoNuevo = "Gastos Varios",
            FechaAnterior = new DateTime(2026, 11, 10, 14, 5, 0), FechaNueva = new DateTime(2026, 11, 10, 14, 5, 0),
            IdSucursalAnterior = 2, IdSucursalNueva = 2, SucursalAnterior = "San Martín", SucursalNueva = "San Martín",
            DescripcionAnterior = "Bolsas", DescripcionNueva = "Bolsas"
        };

        [Fact]
        public void SinCambiosEnLosCamposRegistrados_ListaVacia()
            => Assert.Empty(EgresoModificadoCajero.DescribirCambios(SinCambios()));

        [Fact]
        public void SoloMonto_UnaLineaConAntesYDespues()
        {
            var m = SinCambios();
            m.MontoNuevo = 2500;

            var cambios = EgresoModificadoCajero.DescribirCambios(m);

            Assert.Equal(new[] { "monto $ 500,00 → $ 2.500,00" }, cambios);
        }

        [Fact]
        public void MontoDentroDeLaTolerancia_NoCuenta()
        {
            var m = SinCambios();
            m.MontoNuevo = 500.004;

            Assert.Empty(EgresoModificadoCajero.DescribirCambios(m));
        }

        [Fact]
        public void SoloDescripcion_UnaLinea()
        {
            var m = SinCambios();
            m.DescripcionNueva = "Bolsas y descartables";

            Assert.Equal(new[] { "descripción «Bolsas» → «Bolsas y descartables»" }, EgresoModificadoCajero.DescribirCambios(m));
        }

        [Fact]
        public void FechaConDistintosSegundosEnElMismoMinuto_NoCuenta()
        {
            var m = SinCambios();
            m.FechaNueva = new DateTime(2026, 11, 10, 14, 5, 42);

            Assert.Empty(EgresoModificadoCajero.DescribirCambios(m));
        }

        [Fact]
        public void VariosCampos_EnOrdenMontoTipoFechaSucursalDescripcion()
        {
            var m = SinCambios();
            m.MontoNuevo = 100;
            m.IdTipoNuevo = 6; m.TipoNuevo = "Entregas Dinero";
            m.FechaNueva = new DateTime(2026, 11, 10, 12, 50, 0);
            m.IdSucursalNueva = 3; m.SucursalNueva = "San Lorenzo";
            m.DescripcionNueva = "ger";

            var cambios = EgresoModificadoCajero.DescribirCambios(m);

            Assert.Equal(new[]
            {
                "monto $ 500,00 → $ 100,00",
                "tipo Gastos Varios → Entregas Dinero",
                "fecha 10/11 14:05 → 10/11 12:50",
                "sucursal San Martín → San Lorenzo",
                "descripción «Bolsas» → «ger»"
            }, cambios);
        }
    }
}
