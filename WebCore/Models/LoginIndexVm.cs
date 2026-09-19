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

        // ID de hardware del dispositivo (CPU ID) que informa el agente de impresion local: lo completa
        // en silencio el JS de Views/Login/Index.cshtml (print-agent.js) si el agente esta corriendo
        // en la PC; vacio en celular o PC sin agente (ahi se identifica por la cookie del navegador,
        // ver Helpers/DispositivoNavegador.cs). Login solo desde dispositivos seguros, 2026-09-19.
        public string NumeroSerieDispositivo { get; set; } = "";
    }
}
