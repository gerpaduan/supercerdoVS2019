using System;
using System.Data;
using System.Linq;
using Entidades;
using NegocioTests.Fakes;
using Xunit;

namespace NegocioTests
{
    // Cubre Negocio.CierreCaja.CajaAbiertaDelVendedor (2026-10-20, ver docs/DECISIONS.md "Caja del vendedor con la
    // cuenta de produccion"): la caja del vendedor del POS se busca en la sucursal de la SESION, no en la propia del
    // vendedor (con la cuenta compartida de produccion el vendedor es el operador, que puede tener otra sucursal o
    // ninguna). Caso real reproducido: operador sin sucursal -> NullReferenceException en VentasController.POS.
    public class CajaAbiertaDelVendedorTests
    {
        private const int SucursalTerminal = 2;

        private static (Negocio.CierreCaja cierreN, FakeCierreCajaRepository repo) Crear()
        {
            var repo = new FakeCierreCajaRepository();
            var cierreN = new Negocio.CierreCaja(repo, new EmpresaContextFake(1), null, null, new FakeSucursalRepository());
            return (cierreN, repo);
        }

        // Fila de cierrecaja con las columnas que consume Negocio.CierreCaja.convertDatatableToList.
        // usuarioCierre 0 = caja abierta (asi la deja la base mientras nadie la cierra).
        private static DataTable FilaCaja(int id, int idUsuario, int idSucursal, int usuarioCierre)
        {
            var dt = new DataTable();
            foreach (var col in new[]
            {
                "id", "idSucursal", "usuarioInicio", "usuarioCierre", "fechaHoraInicio", "fechaHoraCierre", "cajaInicio",
                "ventas", "gastos", "cajaCierre", "diferencia", "cajaInicioSiguiente", "importeRetirado"
            })
                dt.Columns.Add(col, typeof(object));

            object fechaCierre = usuarioCierre == 0 ? (object)DBNull.Value : new DateTime(2026, 10, 20, 20, 30, 0);
            dt.Rows.Add(id, idSucursal, idUsuario.ToString(), usuarioCierre, new DateTime(2026, 10, 20, 8, 0, 0), fechaCierre,
                1000f, DBNull.Value, DBNull.Value, DBNull.Value, DBNull.Value, DBNull.Value, DBNull.Value);
            return dt;
        }

        [Fact]
        public void OperadorDeOtraSucursal_BuscaEnLaSucursalDeLaSesion()
        {
            var (cierreN, repo) = Crear();
            repo.CajasEncontradas = FilaCaja(500, 9102, SucursalTerminal, 0);
            var operador = new Usuario { Id = 9102, IdSucursal = 1, Sucursal = new Sucursal { IdSucursal = 1 } };

            var caja = cierreN.CajaAbiertaDelVendedor(SucursalTerminal, operador);

            Assert.NotNull(caja);
            Assert.Equal(500, caja.Id);
            var busqueda = Assert.Single(repo.BusquedasCaja);
            Assert.Equal(CierreCaja.tipoBusqueda.FindLast, busqueda.Tipo);
            Assert.Equal(SucursalTerminal, busqueda.Parametro.Sucursal.idSucursal);
            Assert.Equal(9102, busqueda.Parametro.UsuarioInicio.Id);
        }

        [Fact]
        public void OperadorSinSucursal_NoRompeYBuscaEnLaSucursalDeLaSesion()
        {
            var (cierreN, repo) = Crear();
            repo.CajasEncontradas = FilaCaja(501, 9104, SucursalTerminal, 0);
            var operadorSinSucursal = new Usuario { Id = 9104, IdSucursal = 0, Sucursal = null };

            var caja = cierreN.CajaAbiertaDelVendedor(SucursalTerminal, operadorSinSucursal);

            Assert.NotNull(caja);
            Assert.Equal(SucursalTerminal, repo.BusquedasCaja.Single().Parametro.Sucursal.idSucursal);
        }

        [Fact]
        public void UltimaCajaCerrada_DevuelveNull()
        {
            var (cierreN, repo) = Crear();
            repo.CajasEncontradas = FilaCaja(502, 9102, SucursalTerminal, 1);

            Assert.Null(cierreN.CajaAbiertaDelVendedor(SucursalTerminal, new Usuario { Id = 9102, IdSucursal = 2 }));
        }

        [Fact]
        public void SinCajas_DevuelveNull()
        {
            var (cierreN, _) = Crear();

            Assert.Null(cierreN.CajaAbiertaDelVendedor(SucursalTerminal, new Usuario { Id = 9102, IdSucursal = 2 }));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void SesionSinSucursal_DevuelveNullSinConsultar(int idSucursalSesion)
        {
            var (cierreN, repo) = Crear();

            Assert.Null(cierreN.CajaAbiertaDelVendedor(idSucursalSesion, new Usuario { Id = 9102, IdSucursal = 2 }));
            Assert.Empty(repo.BusquedasCaja);
        }

        [Fact]
        public void SinVendedor_DevuelveNullSinConsultar()
        {
            var (cierreN, repo) = Crear();

            Assert.Null(cierreN.CajaAbiertaDelVendedor(SucursalTerminal, null));
            Assert.Null(cierreN.CajaAbiertaDelVendedor(SucursalTerminal, new Usuario { Id = 0 }));
            Assert.Empty(repo.BusquedasCaja);
        }
    }
}
