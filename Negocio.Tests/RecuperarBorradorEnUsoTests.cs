using System;
using System.Collections.Generic;
using Xunit;

namespace NegocioTests
{
    // Cubre "cargar igual" un borrador/venta PROPIO que figura "en uso" (corte de luz o cuelgue sin esperar
    // el umbral de latido): Negocio.BorradorGenerico.Recuperar y Negocio.VentaBorrador.Recuperar con
    // tomarSiEstaEnUso. Reglas: solo si es del mismo operador, solo si el operador lo confirmo, y al
    // tomarlo se rota el clientId (la pestana vieja no pisa lo cargado). Repositorios falsos en memoria.
    public class RecuperarBorradorEnUsoTests
    {
        private const int Operador = 11;
        private const int OtroOperador = 12;
        private const int UmbralMinutos = 5;
        private const int SesionActual = 99;

        // ===================== BorradorGenerico (Compras/Stock/Movimientos/Embutidos) =====================

        private static (Negocio.BorradorGenerico negocio, FakeGenerico repo, Entidades.BorradorGenerico borrador) NuevoGenerico(
            int idOperador, int segundosSinLatido)
        {
            var borrador = new Entidades.BorradorGenerico
            {
                Id = 7,
                Modulo = Entidades.BorradorGenerico.ModuloMovimiento,
                Estado = Entidades.BorradorGenerico.EstadoActiva,
                IdOperador = idOperador,
                ClientId = Guid.NewGuid(),
                SegundosSinLatido = segundosSinLatido
            };
            var repo = new FakeGenerico(borrador);
            return (new Negocio.BorradorGenerico(repo), repo, borrador);
        }

        [Fact]
        public void Generico_en_uso_propio_sin_confirmar_se_rechaza()
        {
            var (negocio, repo, _) = NuevoGenerico(Operador, 60);

            var r = negocio.Recuperar(7, Entidades.BorradorGenerico.ModuloMovimiento, Operador, SesionActual, Operador, null, UmbralMinutos, false, out string error);

            Assert.Null(r);
            Assert.Contains("en uso", error);
            Assert.False(repo.Tomado);
            Assert.False(repo.Rotado);
        }

        [Fact]
        public void Generico_en_uso_propio_confirmado_se_toma_y_rota_el_clientId()
        {
            var (negocio, repo, borrador) = NuevoGenerico(Operador, 60);
            var clientIdViejo = borrador.ClientId;

            var r = negocio.Recuperar(7, Entidades.BorradorGenerico.ModuloMovimiento, Operador, SesionActual, Operador, null, UmbralMinutos, true, out string error);

            Assert.NotNull(r);
            Assert.Null(error);
            Assert.True(repo.Tomado);
            Assert.True(repo.Rotado);
            Assert.NotEqual(clientIdViejo, r.ClientId);
            Assert.Equal(repo.NuevoClientIdRotado, r.ClientId);
            Assert.Contains("en uso", repo.Eventos[0].Detalle);
        }

        [Fact]
        public void Generico_en_uso_de_otro_operador_no_se_toma_ni_con_confirmacion_ni_con_supervisor()
        {
            var (negocio, repo, _) = NuevoGenerico(OtroOperador, 60);

            var r = negocio.Recuperar(7, Entidades.BorradorGenerico.ModuloMovimiento, Operador, SesionActual, Operador, "Supervisor", UmbralMinutos, true, out string error);

            Assert.Null(r);
            Assert.Contains("en uso", error);
            Assert.False(repo.Tomado);
            Assert.False(repo.Rotado);
        }

        [Fact]
        public void Generico_interrumpido_propio_se_toma_sin_rotar_el_clientId()
        {
            var (negocio, repo, borrador) = NuevoGenerico(Operador, UmbralMinutos * 60 + 1);
            var clientIdViejo = borrador.ClientId;

            var r = negocio.Recuperar(7, Entidades.BorradorGenerico.ModuloMovimiento, Operador, SesionActual, Operador, null, UmbralMinutos, false, out _);

            Assert.NotNull(r);
            Assert.True(repo.Tomado);
            Assert.False(repo.Rotado);
            Assert.Equal(clientIdViejo, r.ClientId);
        }

        [Fact]
        public void Generico_si_falla_la_rotacion_no_se_carga()
        {
            var (negocio, repo, _) = NuevoGenerico(Operador, 60);
            repo.RotarDevuelve = false;

            var r = negocio.Recuperar(7, Entidades.BorradorGenerico.ModuloMovimiento, Operador, SesionActual, Operador, null, UmbralMinutos, true, out string error);

            Assert.Null(r);
            Assert.NotNull(error);
            Assert.Empty(repo.Eventos);
        }

        // ===================== VentaBorrador (POS) =====================

        private static (Negocio.VentaBorrador negocio, FakeVenta repo, Entidades.VentaBorrador borrador) NuevoVenta(
            int idOperador, int segundosSinLatido)
        {
            var borrador = new Entidades.VentaBorrador
            {
                Id = 3,
                Estado = Entidades.VentaBorrador.EstadoActiva,
                IdOperador = idOperador,
                ClientId = Guid.NewGuid(),
                SegundosSinLatido = segundosSinLatido
            };
            var repo = new FakeVenta(borrador);
            return (new Negocio.VentaBorrador(repo), repo, borrador);
        }

        [Fact]
        public void Venta_en_uso_propia_sin_confirmar_se_rechaza()
        {
            var (negocio, repo, _) = NuevoVenta(Operador, 60);

            var r = negocio.Recuperar(3, Operador, SesionActual, Operador, null, UmbralMinutos, false, out string error);

            Assert.Null(r);
            Assert.Contains("en uso", error);
            Assert.False(repo.Tomado);
            Assert.False(repo.Rotado);
        }

        [Fact]
        public void Venta_en_uso_propia_confirmada_se_toma_y_rota_el_clientId()
        {
            var (negocio, repo, borrador) = NuevoVenta(Operador, 60);
            var clientIdViejo = borrador.ClientId;

            var r = negocio.Recuperar(3, Operador, SesionActual, Operador, null, UmbralMinutos, true, out string error);

            Assert.NotNull(r);
            Assert.Null(error);
            Assert.True(repo.Tomado);
            Assert.True(repo.Rotado);
            Assert.NotEqual(clientIdViejo, r.ClientId);
            Assert.Equal(repo.NuevoClientIdRotado, r.ClientId);
            Assert.Contains("en uso", repo.Eventos[0].Detalle);
        }

        [Fact]
        public void Venta_en_uso_de_otro_operador_no_se_toma_ni_con_confirmacion_ni_con_supervisor()
        {
            var (negocio, repo, _) = NuevoVenta(OtroOperador, 60);

            var r = negocio.Recuperar(3, Operador, SesionActual, Operador, "Supervisor", UmbralMinutos, true, out string error);

            Assert.Null(r);
            Assert.Contains("en uso", error);
            Assert.False(repo.Tomado);
            Assert.False(repo.Rotado);
        }

        [Fact]
        public void Venta_interrumpida_propia_se_toma_sin_rotar_el_clientId()
        {
            var (negocio, repo, borrador) = NuevoVenta(Operador, UmbralMinutos * 60 + 1);
            var clientIdViejo = borrador.ClientId;

            var r = negocio.Recuperar(3, Operador, SesionActual, Operador, null, UmbralMinutos, false, out _);

            Assert.NotNull(r);
            Assert.True(repo.Tomado);
            Assert.False(repo.Rotado);
            Assert.Equal(clientIdViejo, r.ClientId);
        }

        [Fact]
        public void Venta_si_falla_la_rotacion_no_se_carga()
        {
            var (negocio, repo, _) = NuevoVenta(Operador, 60);
            repo.RotarDevuelve = false;

            var r = negocio.Recuperar(3, Operador, SesionActual, Operador, null, UmbralMinutos, true, out string error);

            Assert.Null(r);
            Assert.NotNull(error);
            Assert.Empty(repo.Eventos);
        }

        // ===================== Repositorios falsos =====================
        // Solo implementan lo que usa Recuperar; el resto lanza NotImplementedException a proposito (si un
        // test nuevo lo necesita, que falle fuerte en vez de devolver datos inventados).

        private sealed class FakeGenerico : Contratos.IBorradorGenericoRepository
        {
            private readonly Entidades.BorradorGenerico _borrador;
            public bool Tomado;
            public bool Rotado;
            public bool RotarDevuelve = true;
            public Guid NuevoClientIdRotado;
            public readonly List<Entidades.BorradorGenericoEvento> Eventos = new List<Entidades.BorradorGenericoEvento>();

            public FakeGenerico(Entidades.BorradorGenerico borrador) { _borrador = borrador; }

            public Entidades.BorradorGenerico ObtenerPorId(int id) { return _borrador; }
            public bool TomarBorrador(int id, int idOperador, int idUsuarioSesion) { Tomado = true; return true; }
            public bool RotarClientId(int id, Guid nuevoClientId)
            {
                if (!RotarDevuelve) return false;
                Rotado = true;
                NuevoClientIdRotado = nuevoClientId;
                return true;
            }
            public void AgregarEvento(Entidades.BorradorGenericoEvento evento) { Eventos.Add(evento); }

            public Entidades.ResultadoGuardarBorradorGenerico Guardar(Entidades.BorradorGenerico borrador) { throw new NotImplementedException(); }
            public bool RegistrarLatido(Guid clientId, int idOperador, int idSucursal, string modulo) { throw new NotImplementedException(); }
            public Entidades.BorradorGenerico ObtenerPorClientId(Guid clientId, string modulo) { throw new NotImplementedException(); }
            public List<Entidades.BorradorGenerico> ListarActivasPorSucursal(int idSucursal, string modulo) { throw new NotImplementedException(); }
            public List<Entidades.BorradorGenerico> ListarActivasPorUsuarioSesion(int idUsuarioSesion) { throw new NotImplementedException(); }
            public bool MarcarFinalizada(Guid clientId, string modulo, int idResultado) { throw new NotImplementedException(); }
            public bool MarcarDescartada(int id) { throw new NotImplementedException(); }
            public int PurgarFinalizadasAntiguas(string modulo, int dias) { throw new NotImplementedException(); }
            public List<Entidades.BorradorGenericoEvento> ListarEventosPorBorrador(int idBorrador) { throw new NotImplementedException(); }
            public void UpsertNotificacion(Entidades.Notificacion notificacion, bool reabrir) { throw new NotImplementedException(); }
            public int CrearNotificacionesInterrumpidas(string modulo, int minutosSinLatido) { throw new NotImplementedException(); }
        }

        private sealed class FakeVenta : Contratos.IVentaBorradorRepository
        {
            private readonly Entidades.VentaBorrador _borrador;
            public bool Tomado;
            public bool Rotado;
            public bool RotarDevuelve = true;
            public Guid NuevoClientIdRotado;
            public readonly List<Entidades.VentaBorradorEvento> Eventos = new List<Entidades.VentaBorradorEvento>();

            public FakeVenta(Entidades.VentaBorrador borrador) { _borrador = borrador; }

            public Entidades.VentaBorrador ObtenerPorId(int id) { return _borrador; }
            public bool TomarBorrador(int id, int idOperador, int idUsuarioSesion) { Tomado = true; return true; }
            public bool RotarClientId(int id, Guid nuevoClientId)
            {
                if (!RotarDevuelve) return false;
                Rotado = true;
                NuevoClientIdRotado = nuevoClientId;
                return true;
            }
            public void AgregarEvento(Entidades.VentaBorradorEvento evento) { Eventos.Add(evento); }

            public Entidades.ResultadoGuardarBorrador Guardar(Entidades.VentaBorrador borrador) { throw new NotImplementedException(); }
            public bool RegistrarLatido(Guid clientId, int idOperador, int idSucursal) { throw new NotImplementedException(); }
            public Entidades.VentaBorrador ObtenerPorClientId(Guid clientId) { throw new NotImplementedException(); }
            public List<Entidades.VentaBorrador> ListarActivasPorSucursal(int idSucursal) { throw new NotImplementedException(); }
            public List<Entidades.VentaBorrador> ListarActivasPorOperador(int idOperador, int idSucursal) { throw new NotImplementedException(); }
            public List<Entidades.VentaBorrador> ListarActivasPorUsuarioSesion(int idUsuarioSesion) { throw new NotImplementedException(); }
            public bool MarcarFinalizada(Guid clientId, int idVenta) { throw new NotImplementedException(); }
            public bool MarcarDescartada(int id) { throw new NotImplementedException(); }
            public int PurgarFinalizadasAntiguas(int dias) { throw new NotImplementedException(); }
            public List<Entidades.VentaBorrador> ListarInterrumpidasPorRango(DateTime desde, DateTime hasta, int minutosSinLatido) { throw new NotImplementedException(); }
            public List<Entidades.VentaBorradorEvento> ListarEventosPorBorrador(int idBorrador) { throw new NotImplementedException(); }
            public List<Entidades.VentaBorradorEvento> ListarEventosPorRango(DateTime desde, DateTime hasta) { throw new NotImplementedException(); }
            public int AgregarProductoSinAgregar(Entidades.ProductoSinAgregar producto) { throw new NotImplementedException(); }
            public Entidades.ProductoSinAgregar ObtenerProductoSinAgregar(int id) { throw new NotImplementedException(); }
            public List<Entidades.ProductoSinAgregar> ListarProductoSinAgregarPorRango(DateTime desde, DateTime hasta) { throw new NotImplementedException(); }
            public List<Entidades.ProductoSinAgregar> ListarProductoSinAgregarDelDia(int idOperador, DateTime dia) { throw new NotImplementedException(); }
            public Entidades.ResumenProductoSinAgregar ResumirProductoSinAgregar(int idOperador, DateTime desde, DateTime hasta) { throw new NotImplementedException(); }
            public bool RevisarProductoSinAgregar(int id, string revision, string comentario, int idUsuario) { throw new NotImplementedException(); }
            public void UpsertNotificacion(Entidades.Notificacion notificacion, bool reabrir) { throw new NotImplementedException(); }
            public List<Entidades.Notificacion> ListarNotificaciones(bool soloPendientes, int max) { throw new NotImplementedException(); }
            public int ContarNotificacionesPendientes() { throw new NotImplementedException(); }
            public Entidades.Notificacion ObtenerNotificacion(int id) { throw new NotImplementedException(); }
            public bool AtenderNotificacion(int id, int idUsuario) { throw new NotImplementedException(); }
            public int CrearNotificacionesVentasInterrumpidas(int minutosSinLatido) { throw new NotImplementedException(); }
        }
    }
}
