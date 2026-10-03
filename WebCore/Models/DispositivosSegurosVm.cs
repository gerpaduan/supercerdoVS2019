using System.Collections.Generic;

namespace WebCore.Models
{
    public class DispositivosSegurosIndexVm
    {
        public bool PuedeAdministrar { get; set; }
        public List<Entidades.DispositivoSeguro> Items { get; set; } = new List<Entidades.DispositivoSeguro>();

        // Solicitudes de autorizacion de dispositivo pendientes (2026-10-02, Fase 1c): las crean los
        // usuarios desde la pantalla "dispositivo no autorizado" del login; el admin las aprueba o rechaza.
        public List<Entidades.DispositivoSolicitud> Solicitudes { get; set; } = new List<Entidades.DispositivoSolicitud>();
    }
}
