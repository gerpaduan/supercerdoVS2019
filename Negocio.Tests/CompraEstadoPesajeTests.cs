using System;
using System.Collections.Generic;
using Entidades;
using NegocioTests.Fakes;
using Xunit;

namespace NegocioTests
{
    // Cubre el momento en que Negocio.Compra.AddOrEditCompra actualiza el estado del ajuste de un
    // Pesaje de medias (actualizarEstadoPesaje). Con Postgres (unidad de trabajo) tiene que ser
    // DESPUES del commit: usa otra conexion, y al editar un pesaje existente quedaba esperando el
    // lock de la fila que la propia transaccion ya habia modificado, hasta el timeout de 30 s
    // ("Exception while reading from stream"; ver docs/DECISIONS.md 2026-10-16). Con SQL Server
    // (TransactionScope) se mantiene dentro de la transaccion, como siempre.
    public class CompraEstadoPesajeTests
    {
        private static Compra CrearPesaje(int? cantMedias, float? kgsMedias) => new Compra
        {
            IdCompra = 11575,
            TipoCompra = Compra.tipoCompraToString(Compra.tipoCompraEnum.PesajeCortes),
            CantMedias = cantMedias,
            KgsMedias = kgsMedias,
            EnCtaCte = false,
            LineasMediasReses = new List<MediaRes>(),
            LineasCortes = new List<CortePorCompra>(),
            Proveedor = new Persona { idPersona = 18 },
            Sucursal = new Sucursal { IdSucursal = 1 },
            FechaCompra = new DateTime(2026, 9, 14, 9, 45, 0),
            Creado = new DateTime(2026, 9, 14, 9, 45, 0),
            CreadoPor = new Usuario { Id = 2 },
            NroRemito = "",
        };

        private static Negocio.Compra CrearCompraN(FakeCompraRepository repo) =>
            new Negocio.Compra(repo, new EmpresaContextFake(1),
                ctaCteN: new Negocio.CuentaCorriente(new FakeCuentaCorrienteRepository(), new EmpresaContextFake(1)));

        private static void Guardar(Negocio.Compra compra, Compra pesaje) =>
            compra.AddOrEditCompra(pesaje, Compra.tipoCompraToString(Compra.tipoCompraEnum.PesajeCortes),
                new List<MediaRes>(), new List<CortePorCompra>(), esEgresoCaja: false, oEgresoCajaE: null);

        [Fact]
        public void ConUnitOfWork_PesajeDeMedias_ActualizaElEstadoDespuesDelCommit()
        {
            var unitOfWork = new FakeUnitOfWork();
            var repo = new FakeCompraRepository(unitOfWorkAEntregar: unitOfWork) { IdCompraAAsignar = 11575 };

            Guardar(CrearCompraN(repo), CrearPesaje(cantMedias: 12, kgsMedias: 200.95f));

            Assert.Equal(1, repo.ActualizarEstadoPesajeLlamadas);
            Assert.True(repo.UnitOfWorkCompletadaAlActualizarEstado);
        }

        [Fact]
        public void ConUnitOfWork_PesajeSinMedias_NoActualizaElEstado()
        {
            var repo = new FakeCompraRepository(unitOfWorkAEntregar: new FakeUnitOfWork());

            Guardar(CrearCompraN(repo), CrearPesaje(cantMedias: 0, kgsMedias: 0f));

            Assert.Equal(0, repo.ActualizarEstadoPesajeLlamadas);
        }

        [Fact]
        public void ConUnitOfWork_NoActualizaElEstadoSiLaCompraFalla()
        {
            var unitOfWork = new FakeUnitOfWork();
            var repo = new FakeCompraRepository(unitOfWorkAEntregar: unitOfWork,
                excepcionAlAgregar: new InvalidOperationException("fallo simulado de la base"));

            Assert.Throws<Exception>(() => Guardar(CrearCompraN(repo), CrearPesaje(cantMedias: 12, kgsMedias: 200.95f)));

            Assert.False(unitOfWork.CompletarLlamado);
            Assert.Equal(0, repo.ActualizarEstadoPesajeLlamadas);
        }

        [Fact]
        public void ConUnitOfWork_SiFallaElEstadoLaCompraYaEstaGuardadaYElMensajeLoDice()
        {
            var unitOfWork = new FakeUnitOfWork();
            var falla = new InvalidOperationException("se cayo la conexion");
            var repo = new FakeCompraRepository(unitOfWorkAEntregar: unitOfWork) { IdCompraAAsignar = 11575, ExcepcionAlActualizarEstado = falla };

            var ex = Assert.Throws<Exception>(() => Guardar(CrearCompraN(repo), CrearPesaje(cantMedias: 12, kgsMedias: 200.95f)));

            Assert.True(unitOfWork.CompletarLlamado);
            Assert.Contains("11575 se guardó", ex.Message);
            Assert.DoesNotContain("Error en registrar la compra", ex.Message);
            Assert.Same(falla, ex.InnerException);
        }

        [Fact]
        public void SinUnitOfWork_SqlServer_ActualizaElEstadoDentroDeLaTransaccion()
        {
            var repo = new FakeCompraRepository { IdCompraAAsignar = 11575 };

            Guardar(CrearCompraN(repo), CrearPesaje(cantMedias: 12, kgsMedias: 200.95f));

            Assert.Equal(1, repo.ActualizarEstadoPesajeLlamadas);
            Assert.Null(repo.UnitOfWorkCompletadaAlActualizarEstado);
        }

        [Fact]
        public void OtroTipoDeCompra_NoActualizaElEstadoDelPesaje()
        {
            var repo = new FakeCompraRepository(unitOfWorkAEntregar: new FakeUnitOfWork());
            var compraCortes = CrearPesaje(cantMedias: 12, kgsMedias: 200.95f);
            compraCortes.TipoCompra = Compra.tipoCompraToString(Compra.tipoCompraEnum.Cortes);

            CrearCompraN(repo).AddOrEditCompra(compraCortes, Compra.tipoCompraToString(Compra.tipoCompraEnum.Cortes),
                new List<MediaRes>(), new List<CortePorCompra>(), esEgresoCaja: false, oEgresoCajaE: null);

            Assert.Equal(0, repo.ActualizarEstadoPesajeLlamadas);
        }
    }
}
