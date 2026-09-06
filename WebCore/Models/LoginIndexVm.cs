using System.ComponentModel.DataAnnotations;

namespace WebCore.Models
{
    // Port de Web/Models/LoginVm.cs (solo LoginIndexVm -- ForgotPassword/PasswordReset/
    // UnlockAccount/ChangePassword quedan fuera de alcance en v1, ver docs/DECISIONS.md
    // "Login/Sesion real para WebCore"). Sin Latitud/Longitud/PrecisionMetros: son especificos de
    // la geo-validacion de ubicacion, tambien fuera de alcance v1.
    public class LoginIndexVm
    {
        [Required(ErrorMessage = "Ingresá tu usuario o email.")]
        [Display(Name = "Usuario o email")]
        public string Usuario { get; set; } = "";

        [Required(ErrorMessage = "Ingresá tu contraseña.")]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        public string Clave { get; set; } = "";

        public string ReturnUrl { get; set; } = "";
        public string? Error { get; set; }
        public string? Success { get; set; }

        // Numero de serie del dispositivo (CPU ID) -- en el clasico lo completa en silencio el JS
        // del agente de impresion local (print-agent.js, no portado a WebCore, ver
        // docs/10-migracion-aspnet-core/gaps.md). Queda el campo para que "Dispositivo seguro"
        // (DispositivosSegurosController, ya portado) siga funcionando si alguna vez se completa a
        // mano o se porta el agente -- sin el JS de auto-completado, en la practica siempre llega
        // vacio en v1.
        public string NumeroSerieDispositivo { get; set; } = "";
    }
}
