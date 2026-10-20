// Resultado de registrar un intento fallido en un limitador de step-up por sesion (operador de POS/
// modulo, "Ver formula", Cierre de Caja). Concentra la regla del aviso "Te quedan N intentos" para que
// los 3 limitadores y todos los puntos que los usan digan lo mismo (docs/DECISIONS.md, 2026-10-20).
// Depende de Negocio.Usuario solo por la constante FalloDesdeElQueSeAvisaIntentosRestantes: es la misma
// regla del login principal (aviso recien desde el 3.er error, para no alarmar por un tipeo).
using System.Globalization;

namespace WebCore.Helpers
{
    public sealed class StepUpFailureResult
    {
        public StepUpFailureResult(int failures, int maxAttempts, TimeSpan lockout, bool seBloqueo)
        {
            Failures = failures;
            MaxAttempts = maxAttempts;
            Lockout = lockout;
            SeBloqueo = seBloqueo;
        }

        // Fallos acumulados en la ventana actual, contando el que se acaba de registrar.
        public int Failures { get; }

        public int MaxAttempts { get; }

        // true si este fallo fue el ultimo permitido: la sesion quedo bloqueada y hay que informarlo ya,
        // en vez de dejar que el usuario descubra el bloqueo recien en su proximo intento.
        public bool SeBloqueo { get; }

        // Duracion del bloqueo aplicado (solo tiene sentido si SeBloqueo).
        public TimeSpan Lockout { get; }

        public int Restantes => Math.Max(0, MaxAttempts - Failures);

        public int SegundosBloqueo => (int)Math.Ceiling(Lockout.TotalSeconds);

        // Null si no corresponde avisar: todavia no se llego al 3.er error, o ya quedo bloqueada
        // (en ese caso se informa el bloqueo, no un contador).
        public int? RestantesParaAvisar =>
            SeBloqueo || Failures < Negocio.Usuario.FalloDesdeElQueSeAvisaIntentosRestantes ? (int?)null : Restantes;

        // Mensaje cuando este fallo bloqueo la sesion. Mismo texto que ya mostraban los endpoints
        // cuando el bloqueo se detectaba en el intento siguiente.
        public string MensajeBloqueo =>
            "Demasiados intentos. Esperá " + SegundosBloqueo.ToString(CultureInfo.InvariantCulture) + " segundos.";

        // Agrega "Te quedan N intentos antes del bloqueo." al mensaje de error cuando corresponde avisar.
        public string ConAviso(string mensajeBase)
        {
            int? restantes = RestantesParaAvisar;
            if (!restantes.HasValue)
                return mensajeBase;

            string aviso = restantes.Value == 1
                ? "Te queda 1 intento antes del bloqueo."
                : "Te quedan " + restantes.Value.ToString(CultureInfo.InvariantCulture) + " intentos antes del bloqueo.";
            return mensajeBase + " " + aviso;
        }

        // Cuerpo JSON para los endpoints de step-up. Si este fallo bloqueo la sesion usa la misma forma que
        // la respuesta "ya bloqueado" (bloqueado + segundosRestantes), que los clientes ya saben mostrar con
        // cuenta regresiva; si no, el mensaje de error con el aviso de intentos restantes cuando corresponde.
        public object ComoRespuestaJson(string mensajeBase)
        {
            if (SeBloqueo)
                return new { ok = false, bloqueado = true, segundosRestantes = SegundosBloqueo };

            return new { ok = false, msg = ConAviso(mensajeBase) };
        }
    }
}
