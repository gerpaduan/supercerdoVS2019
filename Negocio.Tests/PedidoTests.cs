using System;
using System.Collections.Generic;
using System.Linq;
using Negocio;
using NegocioTests.Fakes;
using Xunit;

namespace NegocioTests
{
    // Cubre las reglas de Pedidos (Negocio.Pedido, docs/DECISIONS.md 2026-10-19): solo se guardan lineas con cantidad, los nombres salen del
    // catalogo del servidor, importes estimados (sin precio no suma), idempotencia del alta, historial con autorizador y control optimista.
    public class PedidoTests
    {
        private static readonly DateTime Ahora = new DateTime(2026, 10, 19, 10, 0, 0);

        private static Entidades.Corte Producto(int id, long codigo, string nombre, bool pesable = false, float promedio = 0f)
        {
            return new Entidades.Corte { IdCorte = id, Codigo = codigo, CorteDesc = nombre, Pesable = pesable, Promedio = promedio };
        }

        private static List<Entidades.Corte> Catalogo()
        {
            return new List<Entidades.Corte>
            {
                Producto(10, 1021, "Bife ancho", pesable: true, promedio: 0.45f),
                Producto(11, 1033, "Matambre"),
                Producto(12, 2310, "Chorizo")
            };
        }

        private static List<Entidades.Sucursal> Sucursales()
        {
            return new List<Entidades.Sucursal>
            {
                new Entidades.Sucursal { IdSucursal = 1, SucursalNombre = "San Lorenzo" },
                new Entidades.Sucursal { IdSucursal = 2, SucursalNombre = "San Martín" }
            };
        }

        private static Pedido.LineaSolicitada Linea(int idCorte, int idSucursal, decimal cantidad, decimal? precio = null)
        {
            return new Pedido.LineaSolicitada { IdCorte = idCorte, IdSucursal = idSucursal, Cantidad = cantidad, PrecioUnitario = precio };
        }

        private static Pedido.Solicitud Solicitud(params Pedido.LineaSolicitada[] lineas)
        {
            return new Pedido.Solicitud { Lineas = lineas.ToList() };
        }

        private static Entidades.Usuario Usuario(int id = 5, string nombre = "Ana", bool produccion = false)
        {
            return new Entidades.Usuario { Id = id, Nombre = nombre, EsUsuarioProduccion = produccion };
        }

        private static Entidades.Pedido Armar(Pedido.Solicitud solicitud, out string error)
        {
            return Pedido.Armar(solicitud, Catalogo(), Sucursales(), null, out error);
        }

        // ---------- Armado y validacion ----------

        [Fact]
        public void Armar_DescartaLasLineasSinCantidad_YSoloDejaLasPedidas()
        {
            string error;
            var pedido = Armar(Solicitud(Linea(10, 1, 5m), Linea(10, 2, 0m), Linea(11, 1, -3m), Linea(12, 2, 2m)), out error);

            Assert.Null(error);
            Assert.Equal(2, pedido.Lineas.Count);
            Assert.DoesNotContain(pedido.Lineas, l => l.IdCorte == 11);
        }

        [Fact]
        public void Armar_SinNingunaCantidad_NoHayPedido()
        {
            string error;
            var pedido = Armar(Solicitud(Linea(10, 1, 0m), Linea(11, 2, 0m)), out error);

            Assert.Null(pedido);
            Assert.Equal("El pedido no tiene ninguna cantidad cargada.", error);
        }

        [Fact]
        public void Armar_TomaNombresYCodigosDelCatalogo_NoDelNavegador()
        {
            string error;
            var pedido = Armar(Solicitud(Linea(11, 2, 4m)), out error);

            var linea = pedido.Lineas.Single();
            Assert.Equal("Matambre", linea.Producto);
            Assert.Equal(1033L, linea.Codigo);
            Assert.Equal("San Martín", linea.Sucursal);
        }

        [Fact]
        public void Armar_ImporteEsCantidadPorPrecio_YElTotalSumaSoloLasLineasConPrecio()
        {
            string error;
            var pedido = Armar(Solicitud(Linea(10, 1, 12.5m, 100m), Linea(11, 1, 3m, 33.333m), Linea(12, 1, 7m)), out error);

            Assert.Equal(1250m, pedido.Lineas.Single(l => l.IdCorte == 10).Importe);
            Assert.Equal(100m, pedido.Lineas.Single(l => l.IdCorte == 11).Importe);   // 99,999 redondeado a 2 decimales
            Assert.Null(pedido.Lineas.Single(l => l.IdCorte == 12).Importe);           // sin precio: no vale $0, no suma
            Assert.Equal(1350m, pedido.TotalEstimado);
            Assert.Equal(1, pedido.LineasSinPrecio);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-50)]
        public void Armar_UnPrecioNoPositivo_SeTrataComoSinPrecio(int precio)
        {
            string error;
            var pedido = Armar(Solicitud(Linea(10, 1, 2m, precio)), out error);

            var linea = pedido.Lineas.Single();
            Assert.Null(linea.PrecioUnitario);
            Assert.Null(linea.Importe);
            Assert.Equal(1, pedido.LineasSinPrecio);
            Assert.Equal(0m, pedido.TotalEstimado);
        }

        [Fact]
        public void Armar_RechazaUnProductoOUnaSucursalQueNoExisten()
        {
            string error;

            Assert.Null(Armar(Solicitud(Linea(999, 1, 1m)), out error));
            Assert.Contains("producto", error);

            Assert.Null(Armar(Solicitud(Linea(10, 77, 1m)), out error));
            Assert.Contains("sucursal", error);
        }

        [Fact]
        public void Armar_RechazaElMismoProductoRepetidoParaUnaSucursal()
        {
            string error;
            var pedido = Armar(Solicitud(Linea(10, 1, 1m), Linea(10, 1, 2m)), out error);

            Assert.Null(pedido);
            Assert.Contains("repetido", error);
        }

        [Fact]
        public void Armar_RedondeaLaCantidadATresDecimales_YUnaCantidadQueQuedaEnCeroNoCuenta()
        {
            string error;
            var pedido = Armar(Solicitud(Linea(10, 1, 2.34567m), Linea(11, 1, 0.0004m)), out error);

            Assert.Null(error);
            var unica = pedido.Lineas.Single();
            Assert.Equal(2.346m, unica.Cantidad);
        }

        [Fact]
        public void Armar_RechazaUnaNotaDemasiadoLarga_YUnaCantidadAbsurda()
        {
            string error;
            var conNota = Solicitud(Linea(10, 1, 1m));
            conNota.Nota = new string('x', Pedido.MaxLargoNota + 1);
            Assert.Null(Armar(conNota, out error));
            Assert.Contains("nota", error);

            Assert.Null(Armar(Solicitud(Linea(10, 1, Pedido.MaxCantidad + 1m)), out error));
            Assert.Contains("cantidad", error);
        }

        [Fact]
        public void Armar_ElPromedioSoloSeGuardaEnLosPesablesConPromedio()
        {
            string error;
            var pedido = Armar(Solicitud(Linea(10, 1, 1m), Linea(11, 1, 1m)), out error);

            Assert.Equal(0.45m, pedido.Lineas.Single(l => l.IdCorte == 10).Promedio);
            Assert.Null(pedido.Lineas.Single(l => l.IdCorte == 11).Promedio);
        }

        // ---------- Alta ----------

        [Fact]
        public void Guardar_PersisteCabeceraLineasYEventoDeAlta_EnUnaTransaccionCompletada()
        {
            var repo = new FakePedidoRepository();
            var negocio = new Pedido(repo);

            var resultado = negocio.Guardar(Solicitud(Linea(10, 1, 5m, 100m), Linea(11, 2, 2m)), Catalogo(), Sucursales(), Usuario(), Ahora);

            Assert.True(resultado.Ok);
            Assert.Equal(1, resultado.IdPedido);
            Assert.Equal(1, repo.Transacciones);
            Assert.True(repo.UltimaTransaccion.CompletarLlamado);
            Assert.Equal(2, repo.Pedidos.Single().Lineas.Count);
            Assert.Equal("Ana", repo.Pedidos.Single().Usuario);

            var evento = repo.Eventos.Single();
            Assert.Equal(Entidades.PedidoEvento.TipoAlta, evento.Tipo);
            Assert.Equal(resultado.IdPedido, evento.IdPedido);
            Assert.Null(evento.IdAutorizador);
        }

        [Fact]
        public void Guardar_SinLineas_NoAbreNiUnaTransaccion()
        {
            var repo = new FakePedidoRepository();
            var negocio = new Pedido(repo);

            var resultado = negocio.Guardar(Solicitud(Linea(10, 1, 0m)), Catalogo(), Sucursales(), Usuario(), Ahora);

            Assert.False(resultado.Ok);
            Assert.Equal(0, repo.Transacciones);
            Assert.Empty(repo.Pedidos);
        }

        [Fact]
        public void Guardar_ConElMismoClientId_NoDuplicaElPedido()
        {
            var repo = new FakePedidoRepository();
            var negocio = new Pedido(repo);
            var clientId = Guid.NewGuid();
            var solicitud = Solicitud(Linea(10, 1, 5m));
            solicitud.ClientId = clientId;

            var primero = negocio.Guardar(solicitud, Catalogo(), Sucursales(), Usuario(), Ahora);
            var reintento = negocio.Guardar(solicitud, Catalogo(), Sucursales(), Usuario(), Ahora.AddSeconds(30));

            Assert.True(primero.Ok);
            Assert.False(primero.YaExistia);
            Assert.True(reintento.Ok);
            Assert.True(reintento.YaExistia);
            Assert.Equal(primero.IdPedido, reintento.IdPedido);
            Assert.Single(repo.Pedidos);
            Assert.Single(repo.Eventos);
        }

        [Fact]
        public void Guardar_SiLaBaseFalla_LaTransaccionNoSeCompleta_YElErrorSePropaga()
        {
            var repo = new FakePedidoRepository { FallarAlAgregar = true };
            var negocio = new Pedido(repo);

            Assert.Throws<InvalidOperationException>(() =>
                negocio.Guardar(Solicitud(Linea(10, 1, 5m)), Catalogo(), Sucursales(), Usuario(), Ahora));

            Assert.False(repo.UltimaTransaccion.CompletarLlamado);
            Assert.True(repo.UltimaTransaccion.DisposeLlamado);   // equivale al rollback
            Assert.Empty(repo.Eventos);
        }

        // ---------- Modificacion ----------

        private static (FakePedidoRepository repo, Pedido negocio, int id) PedidoGuardado()
        {
            var repo = new FakePedidoRepository();
            var negocio = new Pedido(repo);
            var resultado = negocio.Guardar(
                Solicitud(Linea(10, 1, 10m, 100m), Linea(11, 2, 4m, 50m)), Catalogo(), Sucursales(), Usuario(), Ahora);
            return (repo, negocio, resultado.IdPedido);
        }

        [Fact]
        public void Modificar_UsuarioDeProduccionSinAutorizador_SeRechaza_YNoTocaNada()
        {
            var (repo, negocio, id) = PedidoGuardado();
            int transaccionesAntes = repo.Transacciones;

            var resultado = negocio.Modificar(id, Solicitud(Linea(10, 1, 20m)), Catalogo(), Sucursales(),
                Usuario(9, "Cuenta produccion", produccion: true), null, Ahora.AddHours(1));

            Assert.False(resultado.Ok);
            Assert.Contains("autorizada", resultado.Error);
            Assert.Equal(transaccionesAntes, repo.Transacciones);
            Assert.Equal(10m, repo.Pedidos.Single().Lineas.Single(l => l.IdCorte == 10).Cantidad);
        }

        [Fact]
        public void Modificar_ConAutorizador_GuardaElCambioYElHistorialDicePorQuienYConLaClaveDeQuien()
        {
            var (repo, negocio, id) = PedidoGuardado();
            var autorizador = Usuario(7, "Luis");

            var resultado = negocio.Modificar(id, Solicitud(Linea(10, 1, 20m), Linea(11, 2, 4m)), Catalogo(), Sucursales(),
                Usuario(9, "Cuenta produccion", produccion: true), autorizador, Ahora.AddHours(1));

            Assert.True(resultado.Ok);
            Assert.Equal(20m, repo.Pedidos.Single().Lineas.Single(l => l.IdCorte == 10).Cantidad);
            Assert.True(repo.UltimaTransaccion.CompletarLlamado);

            var evento = repo.Eventos.Last();
            Assert.Equal(Entidades.PedidoEvento.TipoModificacion, evento.Tipo);
            Assert.Equal(9, evento.IdUsuario);
            Assert.Equal(7, evento.IdAutorizador);
            Assert.Equal("Luis", evento.Autorizador);
            Assert.Contains("Bife ancho (San Lorenzo): 10 → 20", evento.Detalle);
        }

        [Fact]
        public void Modificar_UsuarioComun_NoNecesitaAutorizador()
        {
            var (repo, negocio, id) = PedidoGuardado();

            var resultado = negocio.Modificar(id, Solicitud(Linea(10, 1, 11m), Linea(11, 2, 4m)), Catalogo(), Sucursales(),
                Usuario(), null, Ahora.AddHours(1));

            Assert.True(resultado.Ok);
            Assert.Null(repo.Eventos.Last().IdAutorizador);
        }

        [Fact]
        public void Modificar_LasLineasQueSeConservan_MantienenElPrecioGuardado_AunqueElNavegadorMandeOtro()
        {
            var (repo, negocio, id) = PedidoGuardado();

            // El navegador "manda" un precio nuevo para la linea existente: se ignora, el importe usa el precio guardado (100).
            var resultado = negocio.Modificar(id, Solicitud(Linea(10, 1, 20m, 999m), Linea(11, 2, 4m)), Catalogo(), Sucursales(),
                Usuario(), null, Ahora.AddHours(1));

            Assert.True(resultado.Ok);
            var linea = repo.Pedidos.Single().Lineas.Single(l => l.IdCorte == 10);
            Assert.Equal(100m, linea.PrecioUnitario);
            Assert.Equal(2000m, linea.Importe);
            Assert.Equal(2200m, repo.Pedidos.Single().TotalEstimado);   // 20 x 100 + 4 x 50
        }

        [Fact]
        public void Modificar_PuedeAgregarYQuitarLineas_YElHistorialLoDescribe()
        {
            var (repo, negocio, id) = PedidoGuardado();

            var resultado = negocio.Modificar(id, Solicitud(Linea(10, 1, 10m), Linea(12, 1, 6m, 20m)), Catalogo(), Sucursales(),
                Usuario(), null, Ahora.AddHours(1));

            Assert.True(resultado.Ok);
            var detalle = repo.Eventos.Last().Detalle;
            Assert.Contains("Se agregó Chorizo (San Lorenzo): 6", detalle);
            Assert.Contains("Se quitó Matambre (San Martín): 4", detalle);
            Assert.Equal(2, repo.Pedidos.Single().Lineas.Count);
            Assert.Equal(1120m, repo.Pedidos.Single().TotalEstimado);   // 10 x 100 + 6 x 20
        }

        [Fact]
        public void Modificar_SinCambios_SeRechaza()
        {
            var (repo, negocio, id) = PedidoGuardado();
            int eventosAntes = repo.Eventos.Count;

            var resultado = negocio.Modificar(id, Solicitud(Linea(10, 1, 10m), Linea(11, 2, 4m)), Catalogo(), Sucursales(),
                Usuario(), null, Ahora.AddHours(1));

            Assert.False(resultado.Ok);
            Assert.Equal("No hay cambios para guardar.", resultado.Error);
            Assert.Equal(eventosAntes, repo.Eventos.Count);
        }

        [Fact]
        public void Modificar_ConUnaVersionVieja_SeRechazaPorqueOtraPersonaLoCambio()
        {
            var (repo, negocio, id) = PedidoGuardado();
            long versionAlAbrir = Pedido.Version(repo.Pedidos.Single());

            // Otra persona lo modifica primero.
            var otra = negocio.Modificar(id, Solicitud(Linea(10, 1, 15m), Linea(11, 2, 4m)), Catalogo(), Sucursales(), Usuario(3, "Otra"), null, Ahora.AddHours(1));
            Assert.True(otra.Ok);

            var solicitud = Solicitud(Linea(10, 1, 30m), Linea(11, 2, 4m));
            solicitud.Version = versionAlAbrir;
            var resultado = negocio.Modificar(id, solicitud, Catalogo(), Sucursales(), Usuario(), null, Ahora.AddHours(2));

            Assert.False(resultado.Ok);
            Assert.Contains("otra persona", resultado.Error);
            Assert.Equal(15m, repo.Pedidos.Single().Lineas.Single(l => l.IdCorte == 10).Cantidad);
        }

        [Fact]
        public void Modificar_ConLaVersionVigente_SeAcepta()
        {
            var (repo, negocio, id) = PedidoGuardado();

            var solicitud = Solicitud(Linea(10, 1, 15m), Linea(11, 2, 4m));
            solicitud.Version = Pedido.Version(repo.Pedidos.Single());
            var resultado = negocio.Modificar(id, solicitud, Catalogo(), Sucursales(), Usuario(), null, Ahora.AddHours(1));

            Assert.True(resultado.Ok, resultado.Error);
        }

        [Fact]
        public void Version_EntraEnUnEnteroSeguroDeJavaScript()
        {
            // La pantalla de Modificar lee la version con parseInt de JS: por encima de 2^53 se redondea y el control optimista
            // rechazaria todo guardado (bug real encontrado probando en el navegador, 2026-10-19).
            const long maximoSeguroJs = 9007199254740991L;
            var (repo, _, _) = PedidoGuardado();

            long version = Pedido.Version(repo.Pedidos.Single());

            Assert.InRange(version, 1L, maximoSeguroJs);
            Assert.Equal(version, (long)(double)version);
        }

        [Fact]
        public void Modificar_UnPedidoQueNoExiste_DaError()
        {
            var negocio = new Pedido(new FakePedidoRepository());

            var resultado = negocio.Modificar(123, Solicitud(Linea(10, 1, 1m)), Catalogo(), Sucursales(), Usuario(), null, Ahora);

            Assert.False(resultado.Ok);
            Assert.Equal("No se encontró el pedido.", resultado.Error);
        }
    }
}
