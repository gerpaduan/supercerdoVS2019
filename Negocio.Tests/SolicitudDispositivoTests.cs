using System;
using Entidades;
using NegocioTests.Fakes;
using Xunit;

namespace NegocioTests
{
    // Solicitud de autorizacion de dispositivo al administrador (Fase 1c, ver docs/DECISIONS.md "Login
    // por CUIT, clave rapida (PIN) y politica de clave"). Sin base de datos: repositorio en memoria.
    public class SolicitudDispositivoTests
    {
        private static (Negocio.DispositivoSeguro negocio, FakeDispositivoSeguroRepository repo) CrearSut()
        {
            var repo = new FakeDispositivoSeguroRepository();
            return (new Negocio.DispositivoSeguro(repo), repo);
        }

        private static DispositivoSolicitud Nueva(string serie = "web:abc", string nombre = "Celular de Juan", int idUsuario = 7)
        {
            return new DispositivoSolicitud { IdEmpresa = 1, IdUsuario = idUsuario, Serie = serie, Nombre = nombre, Ip = "203.0.113.9" };
        }

        [Fact]
        public void CrearSolicitud_QuedaPendienteYConFecha()
        {
            var (negocio, repo) = CrearSut();

            int id = negocio.CrearSolicitud(Nueva());

            var s = Assert.Single(repo.Solicitudes);
            Assert.Equal(id, s.Id);
            Assert.Equal(DispositivoSolicitud.EstadoPendiente, s.Estado);
            Assert.True(s.CreadaUtc > DateTime.UtcNow.AddMinutes(-1));
        }

        [Fact]
        public void CrearSolicitud_ElMismoDispositivoNoDuplica()
        {
            var (negocio, repo) = CrearSut();

            int primero = negocio.CrearSolicitud(Nueva(nombre: "Celular"));
            int segundo = negocio.CrearSolicitud(Nueva(nombre: "Celular de Juan"));

            Assert.Equal(primero, segundo);
            Assert.Single(repo.Solicitudes);
            Assert.Equal("Celular de Juan", repo.Solicitudes[0].Nombre);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public void CrearSolicitud_ExigeUnNombre(string nombre)
        {
            var (negocio, repo) = CrearSut();

            Assert.Throws<ArgumentException>(() => negocio.CrearSolicitud(Nueva(nombre: nombre)));
            Assert.Empty(repo.Solicitudes);
        }

        [Fact]
        public void CrearSolicitud_RechazaNombreDemasiadoLargo_YRecortaElMensaje()
        {
            var (negocio, repo) = CrearSut();

            Assert.Throws<ArgumentException>(() => negocio.CrearSolicitud(Nueva(nombre: new string('x', Negocio.DispositivoSeguro.LargoMaximoNombreSolicitud + 1))));

            var conMensajeLargo = Nueva();
            conMensajeLargo.Mensaje = new string('m', Negocio.DispositivoSeguro.LargoMaximoMensajeSolicitud + 50);
            negocio.CrearSolicitud(conMensajeLargo);
            Assert.Equal(Negocio.DispositivoSeguro.LargoMaximoMensajeSolicitud, repo.Solicitudes[0].Mensaje.Length);
        }

        [Fact]
        public void AprobarSolicitud_DaDeAltaElDispositivoConOrigenSolicitud_YLaResuelve()
        {
            var (negocio, repo) = CrearSut();
            int id = negocio.CrearSolicitud(Nueva());

            bool ok = negocio.AprobarSolicitud(id, 1, idUsuarioAdmin: 2, descripcion: "Celular de Juan (ventas)");

            Assert.True(ok);
            var dispositivo = Assert.Single(repo.Dispositivos);
            Assert.Equal("web:abc", dispositivo.NumeroSerie);
            Assert.Equal("Celular de Juan (ventas)", dispositivo.Descripcion); // editada por el admin
            Assert.Equal(Negocio.DispositivoSeguro.OrigenSolicitud, dispositivo.Origen);
            Assert.Equal(2, dispositivo.IdUsuarioCreador);
            Assert.Equal(DispositivoSolicitud.EstadoAprobada, repo.Solicitudes[0].Estado);
            Assert.Equal(2, repo.Solicitudes[0].ResueltaPor);
            Assert.True(repo.ExisteSerieSegura("web:abc", 1));
        }

        [Fact]
        public void AprobarSolicitud_SinDescripcionUsaElNombreDelUsuario()
        {
            var (negocio, repo) = CrearSut();
            int id = negocio.CrearSolicitud(Nueva(nombre: "Notebook de Ana"));

            negocio.AprobarSolicitud(id, 1, 2, "");

            Assert.Equal("Notebook de Ana", repo.Dispositivos[0].Descripcion);
        }

        [Fact]
        public void AprobarSolicitud_YaResuelta_NoHaceNada()
        {
            var (negocio, repo) = CrearSut();
            int id = negocio.CrearSolicitud(Nueva());
            negocio.RechazarSolicitud(id, 1, 2);

            Assert.False(negocio.AprobarSolicitud(id, 1, 3, ""));
            Assert.Empty(repo.Dispositivos);
        }

        [Fact]
        public void AprobarSolicitud_DeUnDispositivoBloqueado_NoLoReautoriza()
        {
            var (negocio, repo) = CrearSut();
            repo.Agregar(new DispositivoSeguro { IdEmpresa = 1, NumeroSerie = "web:abc", Descripcion = "viejo", Bloqueado = true });
            int id = negocio.CrearSolicitud(Nueva());

            Assert.False(negocio.AprobarSolicitud(id, 1, 2, ""));
            Assert.True(repo.Dispositivos[0].Bloqueado);
            Assert.Equal(DispositivoSolicitud.EstadoPendiente, repo.Solicitudes[0].Estado);
        }

        [Fact]
        public void RechazarSolicitud_NoDaDeAltaNada()
        {
            var (negocio, repo) = CrearSut();
            int id = negocio.CrearSolicitud(Nueva());

            Assert.True(negocio.RechazarSolicitud(id, 1, 2));

            Assert.Empty(repo.Dispositivos);
            Assert.Equal(DispositivoSolicitud.EstadoRechazada, repo.Solicitudes[0].Estado);
            Assert.Empty(negocio.ListarSolicitudesPendientes(1));
        }

        [Fact]
        public void LasSolicitudesNoSeMezclanEntreEmpresas()
        {
            var (negocio, repo) = CrearSut();
            int id = negocio.CrearSolicitud(Nueva());

            Assert.Empty(negocio.ListarSolicitudesPendientes(2));
            Assert.False(negocio.AprobarSolicitud(id, 2, 9, ""));
            Assert.Empty(repo.Dispositivos);
        }
    }
}
