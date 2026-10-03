using System;

namespace Entidades
{
    // Pedido de un usuario para que el administrador autorice un dispositivo (2026-10-02, ver
    // docs/DECISIONS.md "Login por CUIT, clave rapida (PIN) y politica de clave", Fase 1c). Se crea
    // desde la pantalla "dispositivo no autorizado" del login (el usuario ya puso bien su clave).
    // Serie = el mismo "numero de serie" que se guardaria en DispositivoSeguro.NumeroSerie.
    public class DispositivoSolicitud
    {
        public const string EstadoPendiente = "pendiente";
        public const string EstadoAprobada = "aprobada";
        public const string EstadoRechazada = "rechazada";

        public int Id { get; set; }
        public int IdEmpresa { get; set; }
        public int IdUsuario { get; set; }
        // Nombre y usuario de quien pide (solo lectura: se completa con un JOIN al listar).
        public string NombreUsuario { get; set; }
        public string Usuario { get; set; }
        public string Serie { get; set; }
        // Nombre que el usuario le puso al dispositivo (ej. "Celular de Juan").
        public string Nombre { get; set; }
        public string Mensaje { get; set; }
        public string Ip { get; set; }
        public string Estado { get; set; } = EstadoPendiente;
        public DateTime CreadaUtc { get; set; }
        public int? ResueltaPor { get; set; }
        public DateTime? ResueltaUtc { get; set; }
    }
}
