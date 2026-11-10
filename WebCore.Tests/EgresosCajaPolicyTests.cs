// Tests de la regla para modificar un egreso de caja (WebCore.Controllers.EgresosCajaPolicy). Desde el POS cada vendedor
// modifica solo los egresos que cargo el, de su sucursal activa y con la caja abierta; fuera del POS (pantalla admin) solo
// el administrador. Compras, pagos electronicos y cuenta corriente nunca se modifican desde aca.
// Ver docs/DECISIONS.md (2026-11-10, "Egresos propios desde Mis actividades").
using Entidades;
using WebCore.Controllers;

namespace WebCore.Tests
{
    public class EgresosCajaPolicyTests
    {
        private const int IdVendedor = 7;
        private const int IdSucursal = 2;

        private static Usuario Vendedor(bool admin = false, int id = IdVendedor, int idSucursal = IdSucursal)
            => new Usuario { Id = id, IdSucursal = idSucursal, Admin = admin };

        // Egreso comun (tipo 1) cargado por el vendedor 7 en la sucursal 2.
        private static EgresoCaja Egreso(int creadoPor = IdVendedor, int idSucursal = IdSucursal, int idTipo = 1,
            string tabla = "", int? idCompra = null)
            => new EgresoCaja
            {
                Id = 50,
                Fecha = new DateTime(2026, 11, 10, 19, 20, 0),
                IdTipoEgresoCaja = idTipo,
                CreadoPor = creadoPor,
                Sucursal = new Sucursal { idSucursal = idSucursal },
                Tabla = tabla,
                IdCompra = idCompra
            };

        private static EgresoCajaPermisoResultado Evaluar(Usuario usuario, EgresoCaja egreso, bool desdePos = true, bool cajaAbierta = true)
            => EgresosCajaPolicy.EvaluarModificacion(usuario, egreso, desdePos, null!, (fecha, sucursal, vendedor) => cajaAbierta);

        [Fact]
        public void Pos_EgresoPropio_SucursalActiva_CajaAbierta_Puede()
            => Assert.True(Evaluar(Vendedor(), Egreso()).PuedeModificar);

        [Fact]
        public void Pos_EgresoDeOtroVendedor_NoPuede()
        {
            var resultado = Evaluar(Vendedor(), Egreso(creadoPor: 8));

            Assert.False(resultado.PuedeModificar);
            Assert.Equal("Solo podés modificar egresos de caja que cargaste vos.", resultado.MensajeBloqueo);
        }

        [Fact]
        public void Pos_AdminTampocoModificaElEgresoDeOtroDesdeElPos()
            => Assert.False(Evaluar(Vendedor(admin: true), Egreso(creadoPor: 8)).PuedeModificar);

        [Fact]
        public void Pos_OtraSucursal_NoPuede()
            => Assert.False(Evaluar(Vendedor(), Egreso(idSucursal: 3)).PuedeModificar);

        [Fact]
        public void Pos_CajaNoAbierta_NoPuede()
            => Assert.False(Evaluar(Vendedor(), Egreso(), cajaAbierta: false).PuedeModificar);

        [Fact]
        public void FueraDelPos_NoAdmin_NoPuede()
            => Assert.False(Evaluar(Vendedor(), Egreso(), desdePos: false).PuedeModificar);

        [Fact]
        public void FueraDelPos_Admin_Puede()
            => Assert.True(Evaluar(Vendedor(admin: true), Egreso(creadoPor: 8), desdePos: false).PuedeModificar);

        [Fact]
        public void Compra_NoSeModificaDesdeEgresos()
            => Assert.False(Evaluar(Vendedor(), Egreso(idCompra: 99)).PuedeModificar);

        [Fact]
        public void PagoElectronico_NoSeModificaDesdeEgresos()
            => Assert.False(Evaluar(Vendedor(), Egreso(idTipo: EgresoCaja.idPagoTarjeta)).PuedeModificar);

        [Fact]
        public void CuentaCorriente_NoSeModificaDesdeEgresos()
            => Assert.False(Evaluar(Vendedor(), Egreso(idTipo: EgresoCaja.idCtaCte)).PuedeModificar);

        [Fact]
        public void SinSesion_NoPuede()
            => Assert.False(Evaluar(null!, Egreso()).PuedeModificar);
    }
}
