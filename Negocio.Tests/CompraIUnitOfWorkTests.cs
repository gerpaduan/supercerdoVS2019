using System;
using System.Collections.Generic;
using Entidades;
using NegocioTests.Fakes;
using Xunit;

namespace NegocioTests
{
    // Mismo patron que VentaIUnitOfWorkTests, aplicado a Negocio.Compra.AddOrEditCompra: el
    // contrato de IUnitOfWork (Completar() solo si todo sale bien) es identico en los dos --
    // mismo diseño, misma etapa de la migracion (ver docs/DECISIONS.md, "Compra: mismo fix de
    // IUnitOfWork...").
    public class CompraIUnitOfWorkTests
    {
        private static Compra CrearCompraMinima(int idCompra = 0) => new Compra
        {
            IdCompra = idCompra,
            TipoCompra = Compra.tipoCompraToString(Compra.tipoCompraEnum.Cortes),
            EnCtaCte = true,
            LineasMediasReses = new List<MediaRes>(),
            LineasCortes = new List<CortePorCompra>(),
            Proveedor = new Persona { idPersona = 18 },
            Sucursal = new Sucursal { IdSucursal = 1 },
            FechaCompra = new DateTime(2026, 8, 20, 10, 0, 0),
            Creado = new DateTime(2026, 8, 20, 10, 0, 0),
            CreadoPor = new Usuario { Id = 2 },
            NroRemito = "R-001",
        };

        private static Negocio.CuentaCorriente CrearCtaCteFake() =>
            new Negocio.CuentaCorriente(new FakeCuentaCorrienteRepository(), new EmpresaContextFake(1));

        [Fact]
        public void AddOrEditCompra_ConIUnitOfWork_CompletaLaTransaccionSiTodoSaleBien()
        {
            var unitOfWork = new FakeUnitOfWork();
            var repo = new FakeCompraRepository(unitOfWorkAEntregar: unitOfWork) { IdCompraAAsignar = 77 };
            var compra = new Negocio.Compra(repo, new EmpresaContextFake(1), ctaCteN: CrearCtaCteFake());

            int idCompra = compra.AddOrEditCompra(CrearCompraMinima(), Compra.tipoCompraToString(Compra.tipoCompraEnum.Cortes),
                new List<MediaRes>(), new List<CortePorCompra>(), esEgresoCaja: false, oEgresoCajaE: null);

            Assert.Equal(77, idCompra);
            Assert.True(repo.AddOrEditCompraFueLlamado);
            Assert.True(unitOfWork.CompletarLlamado);
            Assert.True(unitOfWork.DisposeLlamado);
        }

        [Fact]
        public void AddOrEditCompra_ConIUnitOfWork_NoCompletaLaTransaccionSiFalla()
        {
            var unitOfWork = new FakeUnitOfWork();
            var fallaEsperada = new InvalidOperationException("fallo simulado de la base");
            var repo = new FakeCompraRepository(unitOfWorkAEntregar: unitOfWork, excepcionAlAgregar: fallaEsperada);
            var compra = new Negocio.Compra(repo, new EmpresaContextFake(1), ctaCteN: CrearCtaCteFake());

            var ex = Assert.Throws<Exception>(() => compra.AddOrEditCompra(CrearCompraMinima(),
                Compra.tipoCompraToString(Compra.tipoCompraEnum.Cortes), new List<MediaRes>(), new List<CortePorCompra>(),
                esEgresoCaja: false, oEgresoCajaE: null));

            Assert.Contains("Error en registrar la compra", ex.Message);
            Assert.Same(fallaEsperada, ex.InnerException);
            Assert.False(unitOfWork.CompletarLlamado);
            Assert.True(unitOfWork.DisposeLlamado);
        }
    }
}
