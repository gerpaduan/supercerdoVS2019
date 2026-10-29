// Tests de la excepcion de permiso para operar compras propias de la caja desde el POS (WebCore.Helpers.CompraPropiaCajaPos).
// Cada condicion que falla tiene que dejar la excepcion apagada. Ver docs/DECISIONS.md (2026-10-28).
using WebCore.Helpers;

namespace WebCore.Tests
{
    public class CompraPropiaCajaPosTests
    {
        // Caso base que cumple todo: compra 10 creada por la persona 7, sucursal 2 igual a la de la sesion, dentro de su caja.
        private static bool Evaluar(bool desdePos = true, int idCompra = 10, int idCreador = 7, int idPersona = 7,
            int sucursalCompra = 2, int sucursalSesion = 2, bool enCaja = true)
            => CompraPropiaCajaPos.PuedeOperarSinPermiso(desdePos, idCompra, idCreador, idPersona, sucursalCompra, sucursalSesion, enCaja);

        [Fact]
        public void CompraPropiaDeLaCajaAbierta_Puede() => Assert.True(Evaluar());

        [Fact]
        public void FueraDelPos_NoPuede() => Assert.False(Evaluar(desdePos: false));

        [Fact]
        public void CompraNueva_NoPuede() => Assert.False(Evaluar(idCompra: 0));

        [Fact]
        public void CompraDeOtraPersona_NoPuede() => Assert.False(Evaluar(idCreador: 8));

        [Fact]
        public void PersonaSinIdentificar_NoPuede() => Assert.False(Evaluar(idCreador: 0, idPersona: 0));

        [Fact]
        public void OtraSucursal_NoPuede() => Assert.False(Evaluar(sucursalCompra: 3));

        [Fact]
        public void SucursalSinResolver_NoPuede() => Assert.False(Evaluar(sucursalCompra: 0, sucursalSesion: 0));

        [Fact]
        public void FueraDeLaCajaAbierta_NoPuede() => Assert.False(Evaluar(enCaja: false));
    }
}
