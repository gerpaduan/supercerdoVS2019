// Tests de la regla "Unificar pesajes" en un Cierre Stock (WebCore.Helpers.ReglaUnificacionCierre).
// Cada condicion que falla tiene que dejar el pesaje NO unificable. Ver docs/DECISIONS.md (2026-11-26).
using WebCore.Helpers;

namespace WebCore.Tests
{
    public class ReglaUnificacionCierreTests
    {
        private const int IdCierre = 500;
        private const int Sucursal = 2;

        // Caso base unificable: pesaje «Solo productos» de la misma sucursal, sin vinculos, sin ajuste, sin hijos.
        private static PesajeCandidatoCierre Base() => new PesajeCandidatoCierre
        {
            IdCompra = 10,
            EsPesaje = true,
            Estado = "",
            IdSucursal = Sucursal,
            CantMedias = 0,
            KgsMedias = 0f,
            IdVinculado = null,
            IdAjusteGenerado = 0,
            CantidadHijos = 0
        };

        private static string? Motivo(PesajeCandidatoCierre p, int idCierre = IdCierre) =>
            ReglaUnificacionCierre.MotivoNoUnificable(p, idCierre, Sucursal);

        [Fact]
        public void PesajeSoloProductos_SeUnifica() => Assert.Null(Motivo(Base()));

        [Fact]
        public void MediasNulas_SeUnifica()
        {
            var p = Base(); p.CantMedias = null; p.KgsMedias = null;
            Assert.Null(Motivo(p));
        }

        [Fact]
        public void YaVinculadoAEsteCierre_SeUnifica()
        {
            var p = Base(); p.IdVinculado = IdCierre;
            Assert.Null(Motivo(p));
        }

        [Fact]
        public void CierreNuevo_PesajeSinVinculo_SeUnifica() => Assert.Null(Motivo(Base(), idCierre: 0));

        [Fact]
        public void Nulo_NoSeUnifica() => Assert.NotNull(ReglaUnificacionCierre.MotivoNoUnificable(null, IdCierre, Sucursal));

        [Fact]
        public void NoEsPesaje_NoSeUnifica()
        {
            var p = Base(); p.EsPesaje = false;
            Assert.NotNull(Motivo(p));
        }

        [Fact]
        public void Anulado_NoSeUnifica()
        {
            var p = Base(); p.Estado = "Anulado";
            Assert.NotNull(Motivo(p));
        }

        [Fact]
        public void OtraSucursal_NoSeUnifica()
        {
            var p = Base(); p.IdSucursal = 3;
            Assert.NotNull(Motivo(p));
        }

        [Fact]
        public void ConMedias_NoSeUnifica()
        {
            var p = Base(); p.CantMedias = 4;
            Assert.NotNull(Motivo(p));
        }

        [Fact]
        public void ConKilosDeMedias_NoSeUnifica()
        {
            var p = Base(); p.KgsMedias = 120.5f;
            Assert.NotNull(Motivo(p));
        }

        [Fact]
        public void SinPesarConProveedorReal_NoSeUnifica()
        {
            var p = Base(); p.ProveedorReal = true;
            Assert.NotNull(Motivo(p));
        }

        [Fact]
        public void ConAjusteGenerado_NoSeUnifica()
        {
            var p = Base(); p.IdAjusteGenerado = 45;
            Assert.Contains("#45", Motivo(p));
        }

        [Fact]
        public void VinculadoAOtro_NoSeUnifica()
        {
            var p = Base(); p.IdVinculado = 123;
            Assert.Contains("#123", Motivo(p));
        }

        [Fact]
        public void VinculadoAOtro_CierreNuevo_NoSeUnifica()
        {
            var p = Base(); p.IdVinculado = 123;
            Assert.NotNull(Motivo(p, idCierre: 0));
        }

        [Fact]
        public void UnificadoEnCierre_NoSeVinculaAUnPesaje() =>
            Assert.Contains("#500", ReglaUnificacionCierre.MotivoNoVinculableAPesaje(500, true));

        [Theory]
        [InlineData(null)]
        [InlineData(0)]
        [InlineData(123)]
        public void SinCierre_SePuedeVincularAUnPesaje(int? idVinculado) =>
            Assert.Null(ReglaUnificacionCierre.MotivoNoVinculableAPesaje(idVinculado, false));

        [Fact]
        public void ConHijos_NoSeUnifica()
        {
            var p = Base(); p.CantidadHijos = 2;
            Assert.NotNull(Motivo(p));
        }
    }
}
