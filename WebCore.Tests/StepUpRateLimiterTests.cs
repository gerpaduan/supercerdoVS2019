// Tests de los 3 limitadores de intentos del step-up por sesion (operador de POS/modulo, "Ver formula",
// Cierre de Caja) y de la regla del aviso "Te quedan N intentos" (docs/DECISIONS.md, 2026-10-20):
// 5 intentos, aviso desde el 3.er error y bloqueo informado en el mismo intento que agota el cupo.
// Los 3 limitadores tienen el mismo contrato, asi que se prueban con la misma bateria para que no diverjan.
// No se prueba el vencimiento de la ventana de tiempo: el reloj es DateTimeOffset.UtcNow, sin abstraccion.
using System.Text.Json;
using WebCore.Helpers;

namespace WebCore.Tests
{
    public class StepUpRateLimiterTests
    {
        // Adaptador para tratar igual a los 3 limitadores (son clases estaticas independientes).
        private sealed record Limitador(
            Func<string, StepUpFailureResult> Registrar,
            Func<string, (bool Bloqueado, TimeSpan Espera)> EstaBloqueado,
            Action<string> Reiniciar);

        public static IEnumerable<object[]> NombresLimitadores()
        {
            yield return new object[] { "PosOperador" };
            yield return new object[] { "Formula" };
            yield return new object[] { "CierreCaja" };
        }

        private static Limitador Crear(string nombre)
        {
            switch (nombre)
            {
                case "PosOperador":
                    return new Limitador(
                        PosOperadorStepUpRateLimiter.RegisterFailure,
                        s => (PosOperadorStepUpRateLimiter.IsBlocked(s, out var espera), espera),
                        PosOperadorStepUpRateLimiter.Reset);
                case "Formula":
                    return new Limitador(
                        FormulaStepUpRateLimiter.RegisterFailure,
                        s => (FormulaStepUpRateLimiter.IsBlocked(s, out var espera), espera),
                        FormulaStepUpRateLimiter.Reset);
                case "CierreCaja":
                    return new Limitador(
                        CierreCajaStepUpRateLimiter.RegisterFailure,
                        s => (CierreCajaStepUpRateLimiter.IsBlocked(s, out var espera), espera),
                        CierreCajaStepUpRateLimiter.Reset);
                default:
                    throw new ArgumentOutOfRangeException(nameof(nombre), nombre, "Limitador desconocido.");
            }
        }

        // Cada test usa su propia sesion: el estado de los limitadores es estatico y compartido.
        private static string NuevaSesion() => "test-" + Guid.NewGuid().ToString("N");

        // Registra n fallos seguidos y devuelve el resultado del ultimo.
        private static StepUpFailureResult Fallar(Limitador limitador, string sesion, int veces)
        {
            StepUpFailureResult? ultimo = null;
            for (int i = 0; i < veces; i++)
                ultimo = limitador.Registrar(sesion);
            return ultimo!;
        }

        // ---- aviso de intentos restantes: solo desde el 3.er error, y el 5.º bloquea ----

        [Theory]
        [MemberData(nameof(NombresLimitadores))]
        public void LosDosPrimerosErroresNoAvisanElContador(string nombre)
        {
            var limitador = Crear(nombre);
            var sesion = NuevaSesion();

            var primero = Fallar(limitador, sesion, 1);
            Assert.Null(primero.RestantesParaAvisar);
            Assert.False(primero.SeBloqueo);
            Assert.Equal("Usuario o contraseña incorrectos.", primero.ConAviso("Usuario o contraseña incorrectos."));

            var segundo = limitador.Registrar(sesion);
            Assert.Null(segundo.RestantesParaAvisar);
            Assert.False(segundo.SeBloqueo);
        }

        [Theory]
        [MemberData(nameof(NombresLimitadores))]
        public void DesdeElTercerErrorAvisaCuantosQuedan(string nombre)
        {
            var limitador = Crear(nombre);
            var sesion = NuevaSesion();

            Fallar(limitador, sesion, 2);
            var tercero = limitador.Registrar(sesion);
            Assert.Equal(2, tercero.RestantesParaAvisar);
            Assert.False(tercero.SeBloqueo);
            Assert.Equal("Clave mala. Te quedan 2 intentos antes del bloqueo.", tercero.ConAviso("Clave mala."));

            var cuarto = limitador.Registrar(sesion);
            Assert.Equal(1, cuarto.RestantesParaAvisar);
            Assert.False(cuarto.SeBloqueo);
            // En singular cuando queda uno solo.
            Assert.Equal("Clave mala. Te queda 1 intento antes del bloqueo.", cuarto.ConAviso("Clave mala."));
        }

        [Theory]
        [MemberData(nameof(NombresLimitadores))]
        public void ElQuintoErrorBloqueaYSeInformaEnEseMismoIntento(string nombre)
        {
            var limitador = Crear(nombre);
            var sesion = NuevaSesion();

            Fallar(limitador, sesion, 4);
            Assert.False(limitador.EstaBloqueado(sesion).Bloqueado); // con 4 fallos todavia puede intentar

            var quinto = limitador.Registrar(sesion);
            Assert.True(quinto.SeBloqueo);
            Assert.Equal(0, quinto.Restantes);
            Assert.Null(quinto.RestantesParaAvisar); // se informa el bloqueo, no un contador
            Assert.Equal(300, quinto.SegundosBloqueo); // 5 minutos por defecto

            var estado = limitador.EstaBloqueado(sesion);
            Assert.True(estado.Bloqueado);
            Assert.InRange(estado.Espera.TotalSeconds, 290, 300);
        }

        // ---- contador por sesion ----

        [Theory]
        [MemberData(nameof(NombresLimitadores))]
        public void UnIngresoCorrectoReiniciaElContador(string nombre)
        {
            var limitador = Crear(nombre);
            var sesion = NuevaSesion();

            Fallar(limitador, sesion, 4);
            limitador.Reiniciar(sesion);

            // Despues del reinicio vuelve a contar desde cero: el 1.er error nuevo no avisa ni bloquea.
            var nuevo = limitador.Registrar(sesion);
            Assert.Equal(1, nuevo.Failures);
            Assert.Null(nuevo.RestantesParaAvisar);
            Assert.False(limitador.EstaBloqueado(sesion).Bloqueado);
        }

        [Theory]
        [MemberData(nameof(NombresLimitadores))]
        public void UnaSesionBloqueadaNoBloqueaA_Otra(string nombre)
        {
            var limitador = Crear(nombre);
            var bloqueada = NuevaSesion();
            var otra = NuevaSesion();

            Fallar(limitador, bloqueada, 5);

            Assert.True(limitador.EstaBloqueado(bloqueada).Bloqueado);
            Assert.False(limitador.EstaBloqueado(otra).Bloqueado);
        }

        // ---- forma de la respuesta JSON que consumen los clientes (seleccion-usuario.js, pos-bloqueo.js) ----

        [Fact]
        public void LaRespuestaJsonNoBloqueadaLlevaMensajeConAviso()
        {
            var resultado = new StepUpFailureResult(failures: 3, maxAttempts: 5, lockout: TimeSpan.FromMinutes(5), seBloqueo: false);

            using var json = JsonDocument.Parse(JsonSerializer.Serialize(resultado.ComoRespuestaJson("Usuario o contraseña incorrectos.")));
            var raiz = json.RootElement;

            Assert.False(raiz.GetProperty("ok").GetBoolean());
            Assert.Equal("Usuario o contraseña incorrectos. Te quedan 2 intentos antes del bloqueo.", raiz.GetProperty("msg").GetString());
            Assert.False(raiz.TryGetProperty("bloqueado", out _));
        }

        [Fact]
        public void LaRespuestaJsonDelQuintoErrorUsaLaFormaDeYaBloqueado()
        {
            var resultado = new StepUpFailureResult(failures: 5, maxAttempts: 5, lockout: TimeSpan.FromMinutes(5), seBloqueo: true);

            using var json = JsonDocument.Parse(JsonSerializer.Serialize(resultado.ComoRespuestaJson("ignorado")));
            var raiz = json.RootElement;

            Assert.False(raiz.GetProperty("ok").GetBoolean());
            Assert.True(raiz.GetProperty("bloqueado").GetBoolean());
            Assert.Equal(300, raiz.GetProperty("segundosRestantes").GetInt32());
        }

        [Fact]
        public void ElMensajeDeBloqueoIndicaLosSegundosDeEspera()
        {
            var resultado = new StepUpFailureResult(failures: 5, maxAttempts: 5, lockout: TimeSpan.FromMinutes(5), seBloqueo: true);

            Assert.Equal("Demasiados intentos. Esperá 300 segundos.", resultado.MensajeBloqueo);
        }
    }
}
