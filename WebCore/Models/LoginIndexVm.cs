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

        // ---- Login por CUIT (2026-10-02, ver docs/DECISIONS.md "Login por CUIT, clave rapida (PIN)
        // y politica de clave"). Todo esto lo completa el controller (no viene del formulario). ----

        // CUIT de la URL /Login/{cuit}. null = login clasico (/Login). Solo se muestra si la empresa
        // existe y esta activa; si no, el formulario se comporta como el login generico.
        public long? Cuit { get; set; }
        public string EmpresaNombre { get; set; } = "";

        // Lista de usuarios activos (no admin) de la empresa, SOLO si este dispositivo ya esta
        // autorizado para esa empresa: la URL con CUIT es publica y listar nombres a cualquiera
        // filtraria empleados. Vacia = se muestra el input de texto normal.
        public List<LoginUsuarioItem> UsuariosLista { get; set; } = new List<LoginUsuarioItem>();
        public bool MostrarListaUsuarios => UsuariosLista.Count > 0;

        // El dispositivo es seguro para la empresa (habilita la lista y el PIN).
        public bool DispositivoSeguro { get; set; }

        // Se muestra el boton "Desbloquear usuario" (el usuario quedo bloqueado). UsuarioDesbloqueo
        // es el identificador que se manda al accion SolicitarDesbloqueo.
        public bool OfrecerDesbloqueo { get; set; }
    }

    // Item de la lista de usuarios del login por CUIT: se muestra el nombre y se envia el nombre de
    // acceso (Usuario) al POST, que sigue el mismo camino que el login tipeado.
    public class LoginUsuarioItem
    {
        public string Usuario { get; set; } = "";
        public string Nombre { get; set; } = "";
    }
}
