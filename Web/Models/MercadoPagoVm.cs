using System;

namespace Web.Models
{
    // ViewModel de la pantalla "Mercado Pago" (panel de conexion OAuth, Fase 2 de la
    // integracion con Point -- ver docs/DECISIONS.md). No expone tokens ni ningun dato
    // sensible -- solo el estado de conexion para mostrar en pantalla.
    public class MercadoPagoIndexVm
    {
        public bool PuedeAdministrar { get; set; }
        public bool Conectado { get; set; }
        public DateTime? FechaConexionUtc { get; set; }
        public DateTime? TokenExpiraUtc { get; set; }
    }
}
