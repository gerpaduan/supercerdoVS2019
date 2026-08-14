using System.ComponentModel.DataAnnotations;

namespace Web.Models
{
    public class LoginIndexVm
    {
        [Required(ErrorMessage = "Ingresá tu usuario o email.")]
        [Display(Name = "Usuario o email")]
        public string Usuario { get; set; }

        [Required(ErrorMessage = "Ingresá tu contraseña.")]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        public string Clave { get; set; }

        public string ReturnUrl { get; set; }
        public string Error { get; set; }
        public string Success { get; set; }
        public string Latitud { get; set; }
        public string Longitud { get; set; }
        public string PrecisionMetros { get; set; }

        // Numero de serie del dispositivo (CPU ID), completado en silencio por JS si el agente
        // de impresion esta corriendo -- ver Web/Views/Login/Index.cshtml y
        // LoginController.Index(POST). Vacio si no hay agente instalado, el login sigue igual.
        public string NumeroSerieDispositivo { get; set; }
    }

    public class LoginUbicacionVm
    {
        public string ReturnUrl { get; set; }
        public string Error { get; set; }
        public string Success { get; set; }
        public string Latitud { get; set; }
        public string Longitud { get; set; }
        public string PrecisionMetros { get; set; }
    }

    public class PasswordRecoveryRequestVm
    {
        [Required(ErrorMessage = "Ingresá tu usuario o email.")]
        [Display(Name = "Usuario o email")]
        public string UsuarioOEmail { get; set; }

        public string Mensaje { get; set; }
    }

    public class PasswordResetVm
    {
        [Required]
        public string Token { get; set; }

        [Required(ErrorMessage = "Ingresá la nueva contraseña.")]
        [StringLength(128, MinimumLength = 1, ErrorMessage = "La contraseña debe tener entre 1 y 128 caracteres.")]
        [DataType(DataType.Password)]
        [Display(Name = "Nueva contraseña")]
        public string NuevaClave { get; set; }

        [Required(ErrorMessage = "Confirmá la nueva contraseña.")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirmar contraseña")]
        [Compare("NuevaClave", ErrorMessage = "La confirmación no coincide con la nueva contraseña.")]
        public string ConfirmarClave { get; set; }

        public bool TokenValido { get; set; }
        public string Mensaje { get; set; }
    }

    public class UnlockAccountVm
    {
        [Required]
        public string Token { get; set; }

        public bool TokenValido { get; set; }
        public string Mensaje { get; set; }
    }

    public class ChangePasswordVm
    {
        [Required(ErrorMessage = "IngresÃ¡ tu contraseÃ±a actual.")]
        [DataType(DataType.Password)]
        [Display(Name = "ContraseÃ±a actual")]
        public string ClaveActual { get; set; }

        [Required(ErrorMessage = "IngresÃ¡ la nueva contraseÃ±a.")]
        [StringLength(128, MinimumLength = 1, ErrorMessage = "La contraseÃ±a debe tener entre 1 y 128 caracteres.")]
        [DataType(DataType.Password)]
        [Display(Name = "Nueva contraseÃ±a")]
        public string NuevaClave { get; set; }

        [Required(ErrorMessage = "ConfirmÃ¡ la nueva contraseÃ±a.")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirmar contraseÃ±a")]
        [Compare("NuevaClave", ErrorMessage = "La confirmaciÃ³n no coincide con la nueva contraseÃ±a.")]
        public string ConfirmarClave { get; set; }

        public string Error { get; set; }
        public string Success { get; set; }
    }
}
