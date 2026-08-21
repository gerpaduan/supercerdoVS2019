using System;
using System.Collections.Generic;
using Entidades;
using NegocioTests.Fakes;
using Xunit;

namespace NegocioTests
{
    // Cubre el contrato de Negocio.Venta.agregarVenta con el camino IUnitOfWork (Postgres):
    // ver Contratos/IUnitOfWork.cs y docs/DECISIONS.md ("Venta resuelta de fondo", 2026-08-20).
    // Es el mecanismo que reemplazo a TransactionScope para el camino Postgres -- si algun
    // cambio futuro rompe el invariante "Completar() solo se llama si todo salio bien", una
    // transaccion real de Postgres podria quedar commiteada a medias o nunca hacer rollback.
    //
    // No cubre el camino SQL Server/TransactionScope (unitOfWork==null): eso sigue siendo el
    // mecanismo de siempre, sin cambios de esta migracion, y TransactionScope no es facil de
    // observar desde un test unitario sin una base real.
    public class VentaIUnitOfWorkTests
    {
        private static Venta CrearVentaMinima(int idVenta = 0) => new Venta
        {
            IdVenta = idVenta,
            FormaPago = Venta.FormaPagoEnum.Efectivo.ToString(),
            EnCtaCte = true,
            LineasVenta = new List<LineaVenta>(),
            ListaExpendios = null,
            Vendedor = new Usuario { Id = 2 },
            Persona = new Persona { idPersona = 13 },
            Sucursal = new Sucursal { IdSucursal = 1 },
            FechaVenta = new DateTime(2026, 8, 20, 10, 0, 0),
            Creado = new DateTime(2026, 8, 20, 10, 0, 0),
            NroRemito = "R-001",
        };

        private static Negocio.CuentaCorriente CrearCtaCteFake() =>
            new Negocio.CuentaCorriente(new FakeCuentaCorrienteRepository(), new EmpresaContextFake(1));

        [Fact]
        public void AgregarVenta_ConIUnitOfWork_CompletaLaTransaccionSiTodoSaleBien()
        {
            var unitOfWork = new FakeUnitOfWork();
            var repo = new FakeVentaRepository(unitOfWorkAEntregar: unitOfWork) { IdVentaAAsignar = 99 };
            var venta = new Negocio.Venta(repo, new EmpresaContextFake(1), ctaCteN: CrearCtaCteFake());

            int idVenta = venta.agregarVenta(CrearVentaMinima());

            Assert.Equal(99, idVenta);
            Assert.True(repo.AgregarVentaFueLlamado);
            Assert.True(unitOfWork.CompletarLlamado);
            Assert.True(unitOfWork.DisposeLlamado);
        }

        [Fact]
        public void AgregarVenta_ConIUnitOfWork_NoCompletaLaTransaccionSiFalla()
        {
            var unitOfWork = new FakeUnitOfWork();
            var fallaEsperada = new InvalidOperationException("fallo simulado de la base");
            var repo = new FakeVentaRepository(unitOfWorkAEntregar: unitOfWork, excepcionAlAgregar: fallaEsperada);
            var venta = new Negocio.Venta(repo, new EmpresaContextFake(1), ctaCteN: CrearCtaCteFake());

            var ex = Assert.Throws<Exception>(() => venta.agregarVenta(CrearVentaMinima()));

            Assert.Contains("Error en registrar la venta", ex.Message);
            Assert.Same(fallaEsperada, ex.InnerException);
            // El punto central del test: sobre excepcion, Completar() NUNCA se llama --
            // UnitOfWorkPg.Dispose() hace el rollback automatico porque no se confirmo.
            Assert.False(unitOfWork.CompletarLlamado);
            Assert.True(unitOfWork.DisposeLlamado);
        }
    }
}
