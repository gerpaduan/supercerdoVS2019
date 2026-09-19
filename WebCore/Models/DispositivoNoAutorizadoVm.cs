namespace WebCore.Models
{
    // Pantalla "Este dispositivo no esta autorizado" (login solo desde dispositivos seguros,
    // 2026-09-19, ver docs/DECISIONS.md): se muestra despues de poner bien la clave, sin sesion
    // emitida todavia.
    public class DispositivoNoAutorizadoVm
    {
        public string UsuarioNombre { get; set; } = "";
        public bool TieneEmail { get; set; }
        public string EmailEnmascarado { get; set; } = "";
        public bool SmtpConfigurado { get; set; }
        public bool CodigoEnviado { get; set; }
        public string? Error { get; set; }
        public string? Success { get; set; }
    }
}
