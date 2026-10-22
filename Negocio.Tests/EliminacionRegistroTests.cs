using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Entidades;
using NegocioTests.Fakes;
using Xunit;

namespace NegocioTests
{
    // Tests unitarios (sin base de datos) de la eliminacion con motivo de Compras y Stock (2026-10-22, ver docs/DECISIONS.md
    // "Eliminar compras, stock y movimientos"): reglas puras (modulo, admin, motivo) y Negocio.Compra.eliminarCompra con
    // fakes -- asiento contrario en la cta cte, egreso de caja opuesto, bloqueos por vinculos y registro ya eliminado.
    public class EliminacionRegistroTests
    {
        private static readonly Persona Proveedor = new Persona { idPersona = 18, razonSocial = "Frigorifico Sur" };
        private static readonly Usuario Elimina = new Usuario { Id = 5, Nombre = "Ana" };
        private static readonly Usuario Sesion = new Usuario { Id = 9, Nombre = "Cuenta produccion" };
        private const string MotivoValido = "Se cargó dos veces por error";

        // ---------------- reglas puras ----------------

        [Theory]
        [InlineData("Cortes", "COMPRA")]
        [InlineData("Media Res", "COMPRA")]
        [InlineData("Ingreso Stock", "STOCK")]
        [InlineData("Egreso Stock", "STOCK")]
        [InlineData("Cierre Stock", "STOCK")]
        [InlineData("Ajuste Stock", "STOCK")]
        [InlineData("Pesaje Cortes", "STOCK")]
        public void ModuloDeTipoCompra_SeparaComprasDeStock(string tipo, string esperado)
        {
            Assert.Equal(esperado, Negocio.EliminacionRegistro.ModuloDeTipoCompra(tipo));
        }

        [Theory]
        [InlineData("Ajuste Stock", true)]
        [InlineData("Cierre Stock", true)]
        [InlineData("Ingreso Stock", false)]
        [InlineData("Pesaje Cortes", false)]
        [InlineData("Cortes", false)]
        public void RequiereAdmin_SoloAjusteYCierre(string tipo, bool esperado)
        {
            Assert.Equal(esperado, Negocio.EliminacionRegistro.RequiereAdmin(tipo));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("corto")]
        public void ValidarMotivo_RechazaVacioOMuyCorto(string motivo)
        {
            Assert.NotNull(Negocio.EliminacionRegistro.ValidarMotivo(motivo, out _));
        }

        [Fact]
        public void ValidarMotivo_RechazaMuyLargoYAceptaUnoValidoRecortado()
        {
            Assert.NotNull(Negocio.EliminacionRegistro.ValidarMotivo(new string('x', Entidades.AuditoriaEliminacion.LargoMaximoMotivo + 1), out _));
            Assert.Null(Negocio.EliminacionRegistro.ValidarMotivo("  " + MotivoValido + "  ", out string normalizado));
            Assert.Equal(MotivoValido, normalizado);
        }

        // ---------------- eliminarCompra ----------------

        private sealed class Escenario
        {
            public FakeCompraRepository Compras;
            public FakeCuentaCorrienteRepository CtaCte;
            public FakeCierreCajaRepository Caja;
            public FakeUnitOfWork UnitOfWork;
            public Negocio.Compra Sut;
        }

        // Una compra de Media Res (no necesita catalogo de productos) o una operacion de stock sin lineas.
        private static Escenario CrearEscenario(string tipoCompra, bool enCtaCte, bool conLineaMediaRes = true)
        {
            var empresa = new EmpresaContextFake(1);
            var uow = new FakeUnitOfWork();
            var compras = new FakeCompraRepository(unitOfWorkAEntregar: uow) { FilaCompra = FilaCompra(tipoCompra, enCtaCte) };
            if (conLineaMediaRes)
            {
                var medias = new DataTable();
                medias.Columns.Add("kgMedia", typeof(double));
                medias.Columns.Add("precioMedia", typeof(double));
                medias.Columns.Add("nroTropa", typeof(string));
                medias.Rows.Add(100.0, 10.0, "T-5");
                medias.Rows.Add(50.0, 12.0, "T-6");
                compras.MediasPorCompra = medias;
            }

            var personas = new FakePersonaRepository();
            personas.PersonasPorId[18] = Proveedor;
            var ctaCteRepo = new FakeCuentaCorrienteRepository();
            var cajaRepo = new FakeCierreCajaRepository();
            var ctaCte = new Negocio.CuentaCorriente(ctaCteRepo, empresa);

            return new Escenario
            {
                Compras = compras,
                CtaCte = ctaCteRepo,
                Caja = cajaRepo,
                UnitOfWork = uow,
                Sut = new Negocio.Compra(compras, empresa,
                    sucursalN: new Negocio.Sucursal(new FakeSucursalRepository(), empresa),
                    cierreCajaN: new Negocio.CierreCaja(cajaRepo, empresa),
                    personaN: new Negocio.Persona(personas, empresa),
                    ctaCteN: ctaCte)
            };
        }

        private static DataTable FilaCompra(string tipoCompra, bool enCtaCte)
        {
            var t = new DataTable();
            foreach (var c in new[] { "idCompra", "nroRemito", "idProveedor", "tipoCompra", "idSucursal", "estado", "observaciones" })
                t.Columns.Add(c, typeof(string));
            t.Columns.Add("fechaCompra", typeof(DateTime));
            t.Columns.Add("cantMedias", typeof(int));
            t.Columns.Add("kgsMedias", typeof(int));
            t.Columns.Add("enCtaCte", typeof(bool));
            t.Columns.Add("creado", typeof(DateTime));
            t.Columns.Add("actualizado", typeof(DateTime));
            t.Columns.Add("creadoPor", typeof(int));
            t.Columns.Add("actualizadoPor", typeof(int));

            var fila = t.NewRow();
            fila["idCompra"] = "321"; fila["nroRemito"] = "R-1"; fila["idProveedor"] = "18"; fila["tipoCompra"] = tipoCompra;
            fila["idSucursal"] = "1"; fila["estado"] = ""; fila["observaciones"] = "obs";
            fila["fechaCompra"] = new DateTime(2026, 10, 1, 9, 0, 0);
            fila["enCtaCte"] = enCtaCte;
            fila["creado"] = new DateTime(2026, 10, 1, 9, 0, 0);
            fila["cantMedias"] = DBNull.Value; fila["kgsMedias"] = DBNull.Value; fila["actualizado"] = DBNull.Value;
            fila["creadoPor"] = DBNull.Value; fila["actualizadoPor"] = DBNull.Value;
            t.Rows.Add(fila);
            return t;
        }

        // Deja en la cta cte el credito que genero la compra (100*10 + 50*12 = 1600).
        private static void SembrarMovimientoDeLaCompra(Escenario e)
        {
            e.CtaCte.Movimientos.Add(new MovCtaCte
            {
                Id = 1, Persona = Proveedor, Fecha = new DateTime(2026, 10, 1), Tabla = MovCtaCte.tablas.Compras.ToString(), IdTabla = 321,
                Detalle = "'Media Res' | Kgs: | Cant:", Tipo = MovCtaCte.tipoMov.Credito.ToString(), Importe = 1600f
            });
        }

        [Fact]
        public void EliminarCompraEnCtaCte_GeneraAsientoContrarioYElNetoEsCero()
        {
            var e = CrearEscenario("Media Res", enCtaCte: true);
            SembrarMovimientoDeLaCompra(e);

            var r = e.Sut.eliminarCompra(321, "COMPRA", MotivoValido, Elimina, Sesion, "10.0.0.5");

            Assert.True(r.Ok, r.Mensaje);
            Assert.Equal(2, e.CtaCte.Movimientos.Count);
            Assert.Equal(0f, e.CtaCte.Movimientos.Sum(m => m.Importe));
            Assert.StartsWith("ELIMINADO - ", e.CtaCte.Movimientos.Last().Detalle);
            Assert.Equal(Elimina.Id, e.CtaCte.Movimientos.Last().CreadoPor.Id);
            Assert.True(e.UnitOfWork.CompletarLlamado);
        }

        [Fact]
        public void EliminarCompraSinCtaCte_NoTocaLaCuentaCorriente()
        {
            var e = CrearEscenario("Media Res", enCtaCte: false);

            var r = e.Sut.eliminarCompra(321, "COMPRA", MotivoValido, Elimina, Sesion, null);

            Assert.True(r.Ok, r.Mensaje);
            Assert.Empty(e.CtaCte.Movimientos);
        }

        [Fact]
        public void EliminarCompraQuitadaDeCtaCte_NoGeneraOtroAsiento()
        {
            var e = CrearEscenario("Media Res", enCtaCte: false);
            SembrarMovimientoDeLaCompra(e);
            e.CtaCte.Movimientos[0].QuitadoCtaCta = true;

            var r = e.Sut.eliminarCompra(321, "COMPRA", MotivoValido, Elimina, Sesion, null);

            Assert.True(r.Ok, r.Mensaje);
            Assert.Single(e.CtaCte.Movimientos);
        }

        [Fact]
        public void EliminarCompra_GuardaLaFotoCompletaEnLaAuditoria()
        {
            var e = CrearEscenario("Media Res", enCtaCte: true);
            SembrarMovimientoDeLaCompra(e);

            var r = e.Sut.eliminarCompra(321, "COMPRA", "  " + MotivoValido + " ", Elimina, Sesion, "10.0.0.5");

            var a = e.Compras.AuditoriaRecibida;
            Assert.Equal("COMPRA", a.Modulo);
            Assert.Equal(321, a.IdRegistro);
            Assert.Equal("Media Res", a.TipoRegistro);
            Assert.Equal(18, a.IdPersona);
            Assert.Equal("R-1", a.NroRemito);
            Assert.Equal(1600d, a.Importe);
            Assert.Equal(150d, a.TotalKg);
            Assert.Equal(2, a.CantLineas);
            Assert.Equal(MotivoValido, a.Motivo);
            Assert.Equal(Elimina.Id, a.IdUsuarioElimino);
            Assert.Equal(Sesion.Id, a.IdUsuarioSesion);
            Assert.Equal("10.0.0.5", a.Ip);
            Assert.Contains("Asiento contrario", a.Efectos);
            Assert.Equal(77, r.IdAuditoria);
            Assert.Same(e.UnitOfWork, e.Compras.UnitOfWorkRecibida);
        }

        [Fact]
        public void EliminarCompra_ConEgresoDeCajaConMonto_RegistraEgresoOpuestoConFechaDeHoy()
        {
            var e = CrearEscenario("Media Res", enCtaCte: false);
            e.Caja.EgresoDeCompra = new EgresoCaja { Id = 40, Monto = 1600f, Descripcion = "Compra a Frigorifico Sur - ID:321", Fecha = new DateTime(2026, 10, 1) };

            var r = e.Sut.eliminarCompra(321, "COMPRA", MotivoValido, Elimina, Sesion, null);

            Assert.True(r.Ok, r.Mensaje);
            var opuesto = Assert.Single(e.Caja.EgresosRecibidos);
            Assert.Equal(0, opuesto.Id);
            Assert.Equal(-1600f, opuesto.Monto);
            Assert.StartsWith("ELIMINADO: ", opuesto.Descripcion);
            Assert.Equal(DateTime.Today, opuesto.Fecha.Date);
            Assert.Equal(Elimina.Id, opuesto.CreadoPor);
        }

        [Fact]
        public void EliminarCompra_ConEgresoEnCeroPorCtaCte_NoRegistraEgresoOpuesto()
        {
            var e = CrearEscenario("Media Res", enCtaCte: true);
            e.Caja.EgresoDeCompra = new EgresoCaja { Id = 40, Monto = 0f };

            e.Sut.eliminarCompra(321, "COMPRA", MotivoValido, Elimina, Sesion, null);

            Assert.Empty(e.Caja.EgresosRecibidos);
        }

        [Fact]
        public void EliminarCompra_ConPesajesVinculados_SeBloqueaYNoSeEscribeNada()
        {
            var e = CrearEscenario("Media Res", enCtaCte: true);
            SembrarMovimientoDeLaCompra(e);
            e.Compras.PesajesVinculados = new List<int> { 400, 401 };

            var r = e.Sut.eliminarCompra(321, "COMPRA", MotivoValido, Elimina, Sesion, null);

            Assert.False(r.Ok);
            Assert.Contains("#400 y #401", r.Mensaje);
            Assert.False(e.Compras.EliminarLlamado);
            Assert.Single(e.CtaCte.Movimientos);
        }

        [Fact]
        public void EliminarPesajeConAjusteGenerado_SeBloquea()
        {
            var e = CrearEscenario("Pesaje Cortes", enCtaCte: false, conLineaMediaRes: false);
            e.Compras.IdAjusteDelPesaje = 555;

            var r = e.Sut.eliminarCompra(321, "STOCK", MotivoValido, Elimina, Sesion, null);

            Assert.False(r.Ok);
            Assert.Contains("#555", r.Mensaje);
            Assert.False(e.Compras.EliminarLlamado);
        }

        [Fact]
        public void EliminarStockDesdeComprasOComprasDesdeStock_SeRechaza()
        {
            var compra = CrearEscenario("Media Res", enCtaCte: false);
            var stock = CrearEscenario("Ingreso Stock", enCtaCte: false, conLineaMediaRes: false);

            Assert.False(compra.Sut.eliminarCompra(321, "STOCK", MotivoValido, Elimina, Sesion, null).Ok);
            Assert.False(stock.Sut.eliminarCompra(321, "COMPRA", MotivoValido, Elimina, Sesion, null).Ok);
            Assert.False(compra.Compras.EliminarLlamado);
            Assert.False(stock.Compras.EliminarLlamado);
        }

        [Fact]
        public void EliminarCompra_SinMotivoValido_NoEscribeNada()
        {
            var e = CrearEscenario("Media Res", enCtaCte: true);

            var r = e.Sut.eliminarCompra(321, "COMPRA", "corto", Elimina, Sesion, null);

            Assert.False(r.Ok);
            Assert.False(e.Compras.EliminarLlamado);
        }

        [Fact]
        public void EliminarCompra_SiOtroUsuarioYaLaElimino_DeshaceElAsientoYNoCompletaLaTransaccion()
        {
            var e = CrearEscenario("Media Res", enCtaCte: true);
            SembrarMovimientoDeLaCompra(e);
            e.Compras.IdAuditoriaADevolver = 0;

            var r = e.Sut.eliminarCompra(321, "COMPRA", MotivoValido, Elimina, Sesion, null);

            Assert.False(r.Ok);
            Assert.Contains("ya fue eliminado", r.Mensaje);
            Assert.False(e.UnitOfWork.CompletarLlamado);
            Assert.Single(e.CtaCte.Movimientos);
        }

        [Fact]
        public void EliminarCompra_RegistroInexistente_DevuelveFallo()
        {
            var e = CrearEscenario("Media Res", enCtaCte: false);
            e.Compras.FilaCompra = new DataTable();

            var r = e.Sut.eliminarCompra(999, "COMPRA", MotivoValido, Elimina, Sesion, null);

            Assert.False(r.Ok);
            Assert.False(e.Compras.EliminarLlamado);
        }

        [Fact]
        public void VistaPrevia_DiceElEfectoEnLaCtaCteConSaldoAntesYDespues()
        {
            var e = CrearEscenario("Media Res", enCtaCte: true);
            SembrarMovimientoDeLaCompra(e);

            var vista = e.Sut.obtenerVistaPreviaEliminacion(321);

            Assert.Equal("COMPRA", vista.Modulo);
            Assert.Equal(1600d, vista.Importe);
            Assert.Equal(2, vista.CantLineas);
            Assert.Empty(vista.Bloqueos);
            Assert.Contains("Frigorifico Sur", vista.EfectoCtaCte);
            Assert.Contains("(a favor)", vista.EfectoCtaCte);
        }
    }
}
