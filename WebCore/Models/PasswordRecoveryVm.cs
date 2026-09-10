using System.ComponentModel.DataAnnotations;

namespace WebCore.Models
{
    // Port de Web/Models/LoginVm.cs (PasswordRecoveryRequestVm/PasswordResetVm), 2026-09-09 --
    // Batch E, ver docs/DECISIONS.md "Batch E: recuperacion de contraseña". Mejora de seguridad
    // deliberada (confirmada con el usuario, "si, mejorarlo"): NuevaClave pasa de
    // MinimumLength=1 a MinimumLength=6 -- el clasico aceptaba literalmente cualquier clave de 1
    // caracter para el reset por email (ver el propio texto de su vista: "podés guardar una clave
    // de 1 o más caracteres"), lo cual es un piso de seguridad real por que endurecer ahora que
    // se agrega esta pantalla, no un cambio de comportamiento que rompa nada existente (paridad no
    // es un requisito de seguridad).
    public class PasswordRecoveryRequestVm
    {
        [Required(ErrorMessage = "Ingresá tu usuario o email.")]
        [Display(Name = "Usuario o email")]
        public string UsuarioOEmail { get; set; } = "";

        public string? Mensaje { get; set; }
    }

    public class PasswordResetVm
    {
        [Required]
        public string Token { get; set; } = "";

        [Required(ErrorMessage = "Ingresá la nueva contraseña.")]
        [StringLength(128, MinimumLength = 6, ErrorMessage = "La contraseña debe tener entre 6 y 128 caracteres.")]
        [DataType(DataType.Password)]
        [Display(Name = "Nueva contraseña")]
        public string NuevaClave { get; set; } = "";

        [Required(ErrorMessage = "Confirmá la nueva contraseña.")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirmar contraseña")]
        [Compare(nameof(NuevaClave), ErrorMessage = "La confirmación no coincide con la nueva contraseña.")]
        public string ConfirmarClave { get; set; } = "";

        public bool TokenValido { get; set; }
        public string? Mensaje { get; set; }
    }

    // Cambiar clave desde el menu de usuario, con sesion ya iniciada (2026-09-10, item 6 de la
    // segunda ronda de pedidos, ver docs/DECISIONS.md). Port de Web/Models/LoginVm.cs:78-99
    // (ChangePasswordVm) -- mismo criterio de seguridad que PasswordResetVm arriba: minimo 6
    // caracteres, no 1 como el clasico.
    public class ChangePasswordVm
    {
        [Required(ErrorMessage = "Ingresá tu contraseña actual.")]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña actual")]
        public string ClaveActual { get; set; } = "";

        [Required(ErrorMessage = "Ingresá la nueva contraseña.")]
        [StringLength(128, MinimumLength = 6, ErrorMessage = "La contraseña debe tener entre 6 y 128 caracteres.")]
        [DataType(DataType.Password)]
        [Display(Name = "Nueva contraseña")]
        public string NuevaClave { get; set; } = "";

        [Required(ErrorMessage = "Confirmá la nueva contraseña.")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirmar contraseña")]
        [Compare(nameof(NuevaClave), ErrorMessage = "La confirmación no coincide con la nueva contraseña.")]
        public string ConfirmarClave { get; set; } = "";

        public string? Error { get; set; }
        public string? Success { get; set; }
    }
}
