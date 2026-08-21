using System;
using System.Collections.Generic;
using Entidades;
using NegocioTests.Fakes;
using Xunit;

namespace NegocioTests
{
    // Mismo patron que VentaIUnitOfWorkTests/CompraIUnitOfWorkTests, aplicado a
    // Negocio.CuentaCorriente.addOrEditPago -- el tercer y ultimo caller real de IUnitOfWork en
    // esta migracion (ver docs/DECISIONS.md, entrada de Pagos/Cobros). Distinto de los otros 2
    // en un detalle de diseno: aca no hay un "ctaCteN" separado inyectado -- el mismo
    // FakeCuentaCorrienteRepository sirve tanto para IniciarUnitOfWork/addOrEditPago (el pago
    // en si) como para getMovCtaCteBy/addOrEditMovCtaCte (via crearMovCtaCtePago, que siempre
    // se ejecuta despues).
    public class CuentaCorrientePagoIUnitOfWorkTests
    {
        private static Pago CrearPagoMinimo() => new Pago
        {
            Id = 0,
            Persona = new Persona { idPersona = 13 },
            Sucursal = new Sucursal { IdSucursal = 1 },
            CreadoPor = new Usuario { Id = 2 },
            Cheques = new List<Cheque>(),
            Fecha = new DateTime(2026, 8, 20, 10, 0, 0),
            Creado = new DateTime(2026, 8, 20, 10, 0, 0),
            NroRecibo = "R-001",
            FormaPago = "Efectivo",
            AProveedor = true,
            Importe = 100,
        };

        [Fact]
        public void AddOrEditPago_ConIUnitOfWork_CompletaLaTransaccionSiTodoSaleBien()
        {
            var unitOfWork = new FakeUnitOfWork();
            var repo = new FakeCuentaCorrienteRepository(unitOfWorkAEntregar: unitOfWork);
            var ctaCte = new Negocio.CuentaCorriente(repo, new EmpresaContextFake(1));

            var resultado = ctaCte.addOrEditPago(CrearPagoMinimo(), oCierreCajaE: null, oPagoSinMod: null);

            Assert.True(resultado.Id > 0);
            Assert.True(repo.AddOrEditPagoFueLlamado);
            Assert.Single(repo.Movimientos);
            Assert.True(unitOfWork.CompletarLlamado);
            Assert.True(unitOfWork.DisposeLlamado);
        }

        [Fact]
        public void AddOrEditPago_ConIUnitOfWork_NoCompletaLaTransaccionSiFalla()
        {
            var unitOfWork = new FakeUnitOfWork();
            var fallaEsperada = new InvalidOperationException("fallo simulado de la base");
            var repo = new FakeCuentaCorrienteRepository(unitOfWorkAEntregar: unitOfWork, excepcionAlAddOrEditPago: fallaEsperada);
            var ctaCte = new Negocio.CuentaCorriente(repo, new EmpresaContextFake(1));

            var ex = Assert.Throws<Exception>(() => ctaCte.addOrEditPago(CrearPagoMinimo(), oCierreCajaE: null, oPagoSinMod: null));

            Assert.Contains("Error en addOrEditPago", ex.Message);
            Assert.Same(fallaEsperada, ex.InnerException);
            Assert.False(unitOfWork.CompletarLlamado);
            Assert.True(unitOfWork.DisposeLlamado);
            // Sobre excepcion en addOrEditPago (paso 1), crearMovCtaCtePago (paso 2) nunca se
            // llega a ejecutar -- ningun MovCtaCte deberia quedar creado.
            Assert.Empty(repo.Movimientos);
        }
    }
}
