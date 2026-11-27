using System;
using System.Collections.Generic;
using Entidades;
using NegocioTests.Fakes;
using Xunit;

namespace NegocioTests
{
    // POS sin conexion (2026-11-26): reglas de la venta offline (Negocio.VentaOffline) y grabacion idempotente con clave
    // unica (Negocio.Venta.agregarVentaConClave). Ver docs/03-modulos/ventas-y-pos.md "POS sin conexion".
    public class VentaOfflineTests
    {
        private const int IdConsumidorFinal = 13;
        private static readonly DateTime Ahora = new DateTime(2026, 11, 26, 12, 0, 0);

        private static Negocio.VentaOffline.Limites Limites() => new Negocio.VentaOffline.Limites
        {
            IdConsumidorFinal = IdConsumidorFinal,
            MaxLineas = 300,
            MinutosToleranciaFuturo = 10,
            DiasMaximosAntiguedad = 7
        };

        private static Negocio.VentaOffline.Pedido PedidoValido() => new Negocio.VentaOffline.Pedido
        {
            IdPersona = IdConsumidorFinal,
            FormaPago = "Efectivo",
            FechaLocal = Ahora.AddHours(-2),
            Lineas = new List<Negocio.VentaOffline.LineaPedido>
            {
                new Negocio.VentaOffline.LineaPedido { Codigo = 101, CantKg = 1.25m, PrecioKg = 9000m }
            }
        };

        // ---------- Validar ----------

        [Fact]
        public void Validar_PedidoCorrecto_NoTieneMotivo()
        {
            Assert.Null(Negocio.VentaOffline.Validar(PedidoValido(), Limites(), Ahora));
        }

        [Theory]
        [InlineData("Efectivo")]
        [InlineData("Debito")]
        [InlineData("credito")]
        [InlineData("Qr")]
        [InlineData("Transferencia")]
        public void Validar_TodasLasFormasDePagoMenosCtaCte_SePermiten(string forma)
        {
            var pedido = PedidoValido();
            pedido.FormaPago = forma;
            Assert.Null(Negocio.VentaOffline.Validar(pedido, Limites(), Ahora));
        }

        [Fact]
        public void Validar_CuentaCorriente_SeRechaza()
        {
            var pedido = PedidoValido();
            pedido.FormaPago = "CtaCte";
            Assert.Contains("Forma de pago", Negocio.VentaOffline.Validar(pedido, Limites(), Ahora));
        }

        [Fact]
        public void Validar_ClienteQueNoEsConsumidorFinal_SeRechaza()
        {
            var pedido = PedidoValido();
            pedido.IdPersona = 500;
            Assert.Contains("Consumidor Final", Negocio.VentaOffline.Validar(pedido, Limites(), Ahora));
        }

        [Fact]
        public void Validar_ConDescuento_SeRechaza()
        {
            var pedido = PedidoValido();
            pedido.Lineas[0].Bonificacion = 10m;
            Assert.Contains("descuentos", Negocio.VentaOffline.Validar(pedido, Limites(), Ahora));
        }

        [Fact]
        public void Validar_SinLineasOCantidadCero_SeRechaza()
        {
            var vacio = PedidoValido();
            vacio.Lineas.Clear();
            Assert.NotNull(Negocio.VentaOffline.Validar(vacio, Limites(), Ahora));

            var cero = PedidoValido();
            cero.Lineas[0].CantKg = 0m;
            Assert.NotNull(Negocio.VentaOffline.Validar(cero, Limites(), Ahora));
        }

        [Fact]
        public void Validar_FechaDentroDeLaToleranciaDelRelojDeLaPC_SeAcepta()
        {
            var pedido = PedidoValido();
            pedido.FechaLocal = Ahora.AddMinutes(9);
            Assert.Null(Negocio.VentaOffline.Validar(pedido, Limites(), Ahora));
        }

        [Fact]
        public void Validar_FechaFutura_SeRechaza()
        {
            var pedido = PedidoValido();
            pedido.FechaLocal = Ahora.AddMinutes(11);
            Assert.Contains("hora de la PC", Negocio.VentaOffline.Validar(pedido, Limites(), Ahora));
        }

        [Fact]
        public void Validar_VentaDemasiadoVieja_SeRechazaParaRevisar()
        {
            var pedido = PedidoValido();
            pedido.FechaLocal = Ahora.AddDays(-8);
            Assert.Contains("administrador", Negocio.VentaOffline.Validar(pedido, Limites(), Ahora));
        }

        // ---------- Vendedor ----------

        [Theory]
        [InlineData(true, false, true, null)]
        [InlineData(false, false, true, "dado de baja")]
        [InlineData(false, true, false, "dado de baja")]
        [InlineData(true, true, true, "bloqueado")]
        [InlineData(true, false, false, "permisos de POS")]
        public void MotivoRevisarVendedor_MarcaBajaBloqueoYFaltaDePermiso(bool activo, bool bloqueado, bool permiso, string esperado)
        {
            string motivo = Negocio.VentaOffline.MotivoRevisarVendedor(activo, bloqueado, permiso);
            if (esperado == null) Assert.Null(motivo);
            else Assert.Contains(esperado, motivo);
        }

        // ---------- ClasificarCaja ----------

        [Fact]
        public void ClasificarCaja_CajaAbiertaAntesDeLaVenta_EsAbierta()
        {
            var abierta = new CierreCaja { Id = 5, FechaHoraInicio = Ahora.AddHours(-5) };
            Assert.Equal(Negocio.VentaOffline.EstadoCaja.Abierta,
                Negocio.VentaOffline.ClasificarCaja(Ahora.AddHours(-1), abierta, null));
        }

        [Fact]
        public void ClasificarCaja_CajaAbiertaDespuesDeLaVentaPeroCerradaQueLaContiene_LlegoTarde()
        {
            // La caja de la manana ya se cerro y el vendedor abrio otra: la venta va a la cerrada (por fecha).
            var abiertaNueva = new CierreCaja { Id = 6, FechaHoraInicio = Ahora.AddHours(-1) };
            var cerrada = new CierreCaja { Id = 5, FechaHoraInicio = Ahora.AddHours(-6), FechaHoraCierre = Ahora.AddHours(-2) };
            Assert.Equal(Negocio.VentaOffline.EstadoCaja.LlegoTarde,
                Negocio.VentaOffline.ClasificarCaja(Ahora.AddHours(-3), abiertaNueva, cerrada));
        }

        [Fact]
        public void ClasificarCaja_NingunaCajaContieneLaFecha_SinCaja()
        {
            var abiertaDespues = new CierreCaja { Id = 6, FechaHoraInicio = Ahora.AddHours(-1) };
            Assert.Equal(Negocio.VentaOffline.EstadoCaja.SinCaja,
                Negocio.VentaOffline.ClasificarCaja(Ahora.AddHours(-3), abiertaDespues, null));
            Assert.Equal(Negocio.VentaOffline.EstadoCaja.SinCaja,
                Negocio.VentaOffline.ClasificarCaja(Ahora.AddHours(-3), null, null));
        }

        // ---------- Factura pendiente y precios ----------

        [Fact]
        public void QuedaPendienteDeFacturar_SoloFormasConFacturaAutomaticaYEmpresaQueFactura()
        {
            var automaticas = new[] { "Debito", "Credito", "Qr", "Transferencia" };
            Assert.True(Negocio.VentaOffline.QuedaPendienteDeFacturar("debito", automaticas, true));
            Assert.False(Negocio.VentaOffline.QuedaPendienteDeFacturar("Efectivo", automaticas, true));
            Assert.False(Negocio.VentaOffline.QuedaPendienteDeFacturar("Debito", automaticas, false));
        }

        [Fact]
        public void DetalleDiferenciasPrecio_PrecioDeCualquierFactorVigente_NoSeMarca()
        {
            var lineas = new[]
            {
                new Negocio.VentaOffline.LineaPedido { Codigo = 101, PrecioKg = 9900m },
                new Negocio.VentaOffline.LineaPedido { Codigo = 102, PrecioKg = 5000.004m }
            };
            var precios = new Dictionary<long, IReadOnlyCollection<decimal>>
            {
                { 101, new[] { 9000m, 9900m } },
                { 102, new[] { 5000m } }
            };
            Assert.Equal("", Negocio.VentaOffline.DetalleDiferenciasPrecio(lineas, precios));
        }

        [Fact]
        public void DetalleDiferenciasPrecio_PrecioDistintoOProductoSinPrecio_SeInforman()
        {
            var lineas = new[]
            {
                new Negocio.VentaOffline.LineaPedido { Codigo = 101, PrecioKg = 8000m },
                new Negocio.VentaOffline.LineaPedido { Codigo = 999, PrecioKg = 100m }
            };
            var precios = new Dictionary<long, IReadOnlyCollection<decimal>> { { 101, new[] { 9000m } } };
            string detalle = Negocio.VentaOffline.DetalleDiferenciasPrecio(lineas, precios);
            Assert.Contains("101: cobrado 8000.00 / lista 9000.00", detalle);
            Assert.Contains("999: sin precio", detalle);
        }

        [Theory]
        [InlineData("SinInternet", "sininternet")]
        [InlineData(" servidor ", "servidor")]
        [InlineData("cualquiera", null)]
        [InlineData(null, null)]
        public void NormalizarCausaCorte_AcotaALaLista(string entrada, string esperado)
        {
            Assert.Equal(esperado, Negocio.VentaOffline.NormalizarCausaCorte(entrada));
        }

        // ---------- agregarVentaConClave ----------

        private static Venta VentaEfectivo() => new Venta
        {
            FormaPago = "Efectivo",
            EnCtaCte = false,
            LineasVenta = new List<LineaVenta>(),
            Vendedor = new Usuario { Id = 2 },
            Persona = new Persona { idPersona = IdConsumidorFinal },
            Sucursal = new Sucursal { IdSucursal = 1 },
            FechaVenta = Ahora.AddHours(-1)
        };

        private static PosClaveUnica Clave(Guid id) => new PosClaveUnica
        {
            ClientId = id,
            Origen = PosClaveUnica.OrigenOffline,
            IdSucursal = 1,
            IdUsuario = 2,
            IdUsuarioSync = 2,
            FechaLocal = Ahora.AddHours(-1)
        };

        private static Negocio.Venta CrearVentaN(FakeVentaRepository repo, FakePosClaveUnicaRepository claves) =>
            new Negocio.Venta(repo, new EmpresaContextFake(1),
                ctaCteN: new Negocio.CuentaCorriente(new FakeCuentaCorrienteRepository(), new EmpresaContextFake(1)))
            {
                PosClaveUnicaRepo = claves
            };

        [Fact]
        public void AgregarVentaConClave_PrimeraVez_GrabaVentaYClaveEnLaMismaTransaccion()
        {
            var uow = new FakeUnitOfWork();
            var repo = new FakeVentaRepository(unitOfWorkAEntregar: uow) { IdVentaAAsignar = 77 };
            var claves = new FakePosClaveUnicaRepository();
            var id = Guid.NewGuid();

            var resultado = CrearVentaN(repo, claves).agregarVentaConClave(VentaEfectivo(), Clave(id));

            Assert.Equal(77, resultado.IdVenta);
            Assert.False(resultado.YaRegistrada);
            Assert.Equal(77, claves.Claves[id].IdRegistro);
            Assert.Equal(PosClaveUnica.TipoVenta, claves.Claves[id].Tipo);
            Assert.Same(uow, claves.UltimoUnitOfWork);
            Assert.True(uow.CompletarLlamado);
        }

        [Fact]
        public void AgregarVentaConClave_Reintento_DevuelveLaVentaExistenteSinGrabarOtra()
        {
            var repo = new FakeVentaRepository(unitOfWorkAEntregar: new FakeUnitOfWork()) { IdVentaAAsignar = 77 };
            var claves = new FakePosClaveUnicaRepository();
            var id = Guid.NewGuid();
            var ventaN = CrearVentaN(repo, claves);
            ventaN.agregarVentaConClave(VentaEfectivo(), Clave(id));

            var repo2 = new FakeVentaRepository(unitOfWorkAEntregar: new FakeUnitOfWork()) { IdVentaAAsignar = 78 };
            var segundo = CrearVentaN(repo2, claves).agregarVentaConClave(VentaEfectivo(), Clave(id));

            Assert.True(segundo.YaRegistrada);
            Assert.Equal(77, segundo.IdVenta);
            Assert.False(repo2.AgregarVentaFueLlamado);
            Assert.Equal(1, claves.Inserciones);
        }

        [Fact]
        public void AgregarVentaConClave_OtraSubidaSimultaneaGano_SeDeshaceYDevuelveLaGanadora()
        {
            var uow = new FakeUnitOfWork();
            var repo = new FakeVentaRepository(unitOfWorkAEntregar: uow) { IdVentaAAsignar = 80 };
            var id = Guid.NewGuid();
            var claves = new FakePosClaveUnicaRepository
            {
                ClaveGanadoraConcurrente = new PosClaveUnica { ClientId = id, Tipo = PosClaveUnica.TipoVenta, IdRegistro = 79 }
            };

            var resultado = CrearVentaN(repo, claves).agregarVentaConClave(VentaEfectivo(), Clave(id));

            Assert.True(resultado.YaRegistrada);
            Assert.Equal(79, resultado.IdVenta);
            // La venta 80 se grabo dentro de la transaccion pero nunca se confirmo: Dispose hace el rollback.
            Assert.False(uow.CompletarLlamado);
            Assert.True(uow.DisposeLlamado);
        }

        [Fact]
        public void AgregarVentaConClave_ClaveDeUnaAperturaDeCaja_NoSeTomaComoReintento()
        {
            var repo = new FakeVentaRepository(unitOfWorkAEntregar: new FakeUnitOfWork());
            var id = Guid.NewGuid();
            var claves = new FakePosClaveUnicaRepository();
            claves.Claves[id] = new PosClaveUnica { ClientId = id, Tipo = PosClaveUnica.TipoCaja, IdRegistro = 4 };

            Assert.Throws<InvalidOperationException>(() => CrearVentaN(repo, claves).agregarVentaConClave(VentaEfectivo(), Clave(id)));
            Assert.False(repo.AgregarVentaFueLlamado);
        }

        [Fact]
        public void AgregarVentaConClave_FallaLaVenta_NoGrabaLaClaveNiConfirma()
        {
            var uow = new FakeUnitOfWork();
            var repo = new FakeVentaRepository(unitOfWorkAEntregar: uow, excepcionAlAgregar: new InvalidOperationException("base caida"));
            var claves = new FakePosClaveUnicaRepository();

            var ex = Assert.Throws<Exception>(() => CrearVentaN(repo, claves).agregarVentaConClave(VentaEfectivo(), Clave(Guid.NewGuid())));

            Assert.Contains("Error en registrar la venta", ex.Message);
            Assert.Equal(0, claves.Inserciones);
            Assert.False(uow.CompletarLlamado);
        }
    }
}
