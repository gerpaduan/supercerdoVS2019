using System;
using System.Collections.Generic;
using Entidades;
using NegocioTests.Fakes;
using Xunit;

namespace NegocioTests
{
    // Mismo contrato de IUnitOfWork que VentaIUnitOfWorkTests, pero sobre
    // Negocio.Venta.modificarVenta -- metodo separado de agregarVenta, con su propio wrapper
    // TransactionScope-vs-IUnitOfWork (ver docs/DECISIONS.md, "modificarVenta: mismo fix de
    // IUnitOfWork..."). A diferencia de Compra (donde el mismo AddOrEditCompra cubre alta y
    // edicion), Venta separa agregarVenta/modificarVenta en 2 metodos -- cada uno con su propio
    // contrato a verificar.
    public class VentaModificarIUnitOfWorkTests
    {
        private static Venta CrearVentaMinima(int idVenta = 61) => new Venta
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
        public void ModificarVenta_ConIUnitOfWork_CompletaLaTransaccionSiTodoSaleBien()
        {
            var unitOfWork = new FakeUnitOfWork();
            var repo = new FakeVentaRepository(unitOfWorkAEntregar: unitOfWork);
            var venta = new Negocio.Venta(repo, new EmpresaContextFake(1), ctaCteN: CrearCtaCteFake());

            venta.modificarVenta(CrearVentaMinima(), SucAnterior: 1, eliminarLineas: false, lineaNuevosAnulados: null);

            Assert.True(repo.ModificarVentaFueLlamado);
            Assert.True(unitOfWork.CompletarLlamado);
            Assert.True(unitOfWork.DisposeLlamado);
        }

        [Fact]
        public void ModificarVenta_ConIUnitOfWork_NoCompletaLaTransaccionSiFalla()
        {
            var unitOfWork = new FakeUnitOfWork();
            var fallaEsperada = new InvalidOperationException("fallo simulado de la base");
            var repo = new FakeVentaRepository(unitOfWorkAEntregar: unitOfWork, excepcionAlModificar: fallaEsperada);
            var venta = new Negocio.Venta(repo, new EmpresaContextFake(1), ctaCteN: CrearCtaCteFake());

            var ex = Assert.Throws<Exception>(() =>
                venta.modificarVenta(CrearVentaMinima(), SucAnterior: 1, eliminarLineas: false, lineaNuevosAnulados: null));

            Assert.Contains("Error en registrar la venta", ex.Message);
            Assert.Same(fallaEsperada, ex.InnerException);
            Assert.False(unitOfWork.CompletarLlamado);
            Assert.True(unitOfWork.DisposeLlamado);
        }
    }
}
