using System;
using NegocioTests.Fakes;
using Xunit;

namespace NegocioTests
{
    // Bloqueo por intentos segun el origen (dispositivo seguro / no seguro), aviso de intentos restantes
    // y clave rapida (PIN) -- ver docs/DECISIONS.md "Login por CUIT, clave rapida (PIN) y politica de
    // clave". Sin base de datos: repositorio en memoria.
    public class UsuarioLoginBloqueoPinTests
    {
        private const int Maximo = 7;

        private static (Negocio.Usuario negocio, FakeUsuarioRepository repo) CrearSut()
        {
            var repo = new FakeUsuarioRepository();
            return (new Negocio.Usuario(repo, new EmpresaContextFake(1)), repo);
        }

        private static Entidades.Usuario NuevoUsuario(int id = 26)
        {
            return new Entidades.Usuario { Id = id, User = "juan", Activo = true, IdEmpresa = 1 };
        }

        // ---- bloqueo por origen ----

        [Fact]
        public void FallosDesdeDispositivoSeguro_BloqueanLaCuentaCompletaAlSeptimo()
        {
            var (negocio, repo) = CrearSut();
            var usuario = NuevoUsuario();

            for (int i = 1; i < Maximo; i++)
            {
                var parcial = negocio.RegistrarIntentoFallido(usuario, Maximo, dispositivoSeguro: true);
                Assert.False(parcial.SeAcabaDeBloquear);
                Assert.False(usuario.Bloqueado);
            }

            var ultimo = negocio.RegistrarIntentoFallido(usuario, Maximo, dispositivoSeguro: true);

            Assert.True(ultimo.SeAcabaDeBloquear);
            Assert.True(ultimo.BloqueoCuentaCompleta);
            Assert.True(usuario.Bloqueado);
            Assert.True(repo.UltimoEstadoBloqueo.Bloqueado);
            Assert.NotNull(repo.UltimoEstadoBloqueo.FechaBloqueoUtc);
            // El contador "no seguro" no se toco.
            Assert.Equal(0, usuario.IntentosFallidosNoSeguro);
            Assert.False(usuario.BloqueadoNoSeguro);
        }

        [Fact]
        public void FallosDesdeDispositivoNoSeguro_SoloBloqueanElIngresoDesdeDispositivosNoSeguros()
        {
            var (negocio, repo) = CrearSut();
            var usuario = NuevoUsuario();

            Negocio.Usuario.ResultadoIntentoFallido ultimo = null;
            for (int i = 1; i <= Maximo; i++)
                ultimo = negocio.RegistrarIntentoFallido(usuario, Maximo, dispositivoSeguro: false);

            Assert.True(ultimo.SeAcabaDeBloquear);
            Assert.False(ultimo.BloqueoCuentaCompleta);
            // La cuenta NO queda bloqueada: el dueño sigue entrando desde su dispositivo seguro.
            Assert.False(usuario.Bloqueado);
            Assert.Equal(0, usuario.IntentosFallidosLogin);
            Assert.True(usuario.BloqueadoNoSeguro);
            Assert.True(repo.UltimoEstadoBloqueo.BloqueadoNoSeguro);
            Assert.Equal(Maximo, repo.UltimoEstadoBloqueo.IntentosFallidosNoSeguro);
        }

        [Fact]
        public void ElContadorEsPorUsuario_UnoNoConsumeLosIntentosDelOtro()
        {
            var (negocio, _) = CrearSut();
            var ana = NuevoUsuario(1);
            var luis = NuevoUsuario(2);

            for (int i = 0; i < 5; i++)
                negocio.RegistrarIntentoFallido(ana, Maximo, dispositivoSeguro: false);

            var primerFalloDeLuis = negocio.RegistrarIntentoFallido(luis, Maximo, dispositivoSeguro: false);

            Assert.Equal(1, primerFalloDeLuis.IntentosFallidos);
            Assert.Equal(Maximo - 1, primerFalloDeLuis.IntentosRestantes);
        }

        // ---- aviso de intentos restantes (recien desde el 3.er error) ----

        [Theory]
        [InlineData(1, null)]
        [InlineData(2, null)]
        [InlineData(3, 4)]
        [InlineData(4, 3)]
        [InlineData(5, 2)]
        [InlineData(6, 1)]
        [InlineData(7, null)] // ya quedo bloqueado: se muestra el mensaje de bloqueo, no un contador
        public void IntentosRestantesParaAvisar_SoloDesdeElTercerError(int fallo, int? esperado)
        {
            var (negocio, _) = CrearSut();
            var usuario = NuevoUsuario();

            Negocio.Usuario.ResultadoIntentoFallido resultado = null;
            for (int i = 1; i <= fallo; i++)
                resultado = negocio.RegistrarIntentoFallido(usuario, Maximo, dispositivoSeguro: false);

            Assert.Equal(esperado, Negocio.Usuario.IntentosRestantesParaAvisar(resultado));
        }

        // ---- login exitoso y desbloqueo ----

        [Fact]
        public void LoginExitosoDesdeDispositivoSeguro_ReseteaSoloElContadorSeguro()
        {
            var (negocio, repo) = CrearSut();
            var usuario = NuevoUsuario();
            negocio.RegistrarIntentoFallido(usuario, Maximo, dispositivoSeguro: true);
            negocio.RegistrarIntentoFallido(usuario, Maximo, dispositivoSeguro: false);
            negocio.RegistrarIntentoFallido(usuario, Maximo, dispositivoSeguro: false);

            negocio.RegistrarLoginExitoso(usuario, dispositivoSeguro: true);

            Assert.Equal(0, usuario.IntentosFallidosLogin);
            // Alguien sigue adivinando desde afuera: que el dueño entre bien no borra esos intentos.
            Assert.Equal(2, usuario.IntentosFallidosNoSeguro);
            Assert.Equal(2, repo.UltimoEstadoBloqueo.IntentosFallidosNoSeguro);
        }

        [Fact]
        public void LoginExitosoSinIntentosPrevios_NoEscribeEnLaBase()
        {
            var (negocio, repo) = CrearSut();

            negocio.RegistrarLoginExitoso(NuevoUsuario(), dispositivoSeguro: true);

            Assert.Equal(0, repo.VecesActualizoBloqueo);
        }

        [Fact]
        public void DesbloquearUsuario_LimpiaAmbosBloqueosYContadores()
        {
            var (negocio, repo) = CrearSut();

            negocio.DesbloquearUsuario(26);

            var estado = repo.UltimoEstadoBloqueo;
            Assert.Equal(26, estado.Id);
            Assert.False(estado.Bloqueado);
            Assert.Equal(0, estado.IntentosFallidosLogin);
            Assert.Null(estado.FechaBloqueoUtc);
            Assert.False(estado.BloqueadoNoSeguro);
            Assert.Equal(0, estado.IntentosFallidosNoSeguro);
            Assert.Null(estado.FechaBloqueoNoSeguroUtc);
        }

        [Fact]
        public void LaFirmaAnterior_SigueContandoComoFalloDeDispositivoSeguro()
        {
            var (negocio, _) = CrearSut();
            var usuario = NuevoUsuario();

            bool bloqueado = false;
            for (int i = 1; i <= Maximo; i++)
                bloqueado = negocio.RegistrarIntentoFallido(usuario, Maximo);

            Assert.True(bloqueado);
            Assert.True(usuario.Bloqueado);
        }

        // ---- clave rapida (PIN) ----

        [Fact]
        public void ActualizarPin_GuardaSoloElHashYSePuedeVerificar()
        {
            var (negocio, repo) = CrearSut();

            negocio.ActualizarPin(26, "4829");

            Assert.False(string.IsNullOrWhiteSpace(repo.PinHashGuardado));
            Assert.False(string.IsNullOrWhiteSpace(repo.PinSaltGuardado));
            Assert.DoesNotContain("4829", repo.PinHashGuardado);

            var usuario = new Entidades.Usuario
            {
                Id = 26,
                PinHash = repo.PinHashGuardado,
                PinSalt = repo.PinSaltGuardado,
                PinHashIterations = repo.PinIteracionesGuardadas
            };

            Assert.True(usuario.TienePin);
            Assert.True(negocio.VerificarPin(usuario, "4829"));
            Assert.False(negocio.VerificarPin(usuario, "4830"));
            Assert.False(negocio.VerificarPin(usuario, ""));
        }

        [Theory]
        [InlineData("1234")]   // secuencia
        [InlineData("1111")]   // repetido
        [InlineData("12")]     // corto
        [InlineData("26")]     // corto (y es el id)
        [InlineData("abcd")]   // no numerico
        public void ActualizarPin_RechazaPinesInvalidos_YNoGuardaNada(string pin)
        {
            var (negocio, repo) = CrearSut();

            Assert.Throws<ArgumentException>(() => negocio.ActualizarPin(26, pin));
            Assert.Equal("", repo.PinHashGuardado);
        }

        [Fact]
        public void ActualizarPin_RechazaUnPinIgualAlIdDelUsuario()
        {
            var (negocio, _) = CrearSut();

            Assert.Throws<ArgumentException>(() => negocio.ActualizarPin(2600, "2600"));
        }

        [Fact]
        public void VerificarPin_SinPinConfigurado_NuncaCoincide()
        {
            var (negocio, _) = CrearSut();
            var sinPin = NuevoUsuario();

            Assert.False(sinPin.TienePin);
            Assert.False(negocio.VerificarPin(sinPin, "4829"));
            Assert.False(negocio.VerificarPin(null, "4829"));
        }

        [Fact]
        public void QuitarPin_GuardaHashVacio()
        {
            var (negocio, repo) = CrearSut();
            negocio.ActualizarPin(26, "4829");

            negocio.QuitarPin(26);

            Assert.Equal("", repo.PinHashGuardado);
            Assert.Equal("", repo.PinSaltGuardado);
            Assert.Equal(0, repo.PinIteracionesGuardadas);
        }

        // ---- listas livianas ----

        [Fact]
        public void ListarUsuariosParaLogin_PideLosNoAdmin_YListarAdministradoresActivos_LosAdmin()
        {
            var (negocio, repo) = CrearSut();

            negocio.ListarUsuariosParaLogin(1);
            Assert.Equal(false, repo.UltimoPedidoAdmin);

            negocio.ListarAdministradoresActivos(1);
            Assert.Equal(true, repo.UltimoPedidoAdmin);
        }
    }
}
