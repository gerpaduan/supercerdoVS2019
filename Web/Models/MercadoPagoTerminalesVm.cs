using System.Collections.Generic;

namespace Web.Models
{
    // ViewModel de "Terminales Mercado Pago" (Fase 3 -- alta de Sucursal/Caja/Terminal contra
    // la API real de Mercado Pago, ver docs/DECISIONS.md 2026-09-01).
    public class MercadoPagoTerminalesIndexVm
    {
        public bool PuedeAdministrar { get; set; }
        public bool Conectado { get; set; }
        public List<SucursalTerminalesVm> Sucursales { get; set; } = new List<SucursalTerminalesVm>();
    }

    public class SucursalTerminalesVm
    {
        public int IdSucursal { get; set; }
        public string NombreSucursal { get; set; }
        public string MpStoreId { get; set; }
        public List<TerminalItemVm> Terminales { get; set; } = new List<TerminalItemVm>();
    }

    public class TerminalItemVm
    {
        public int Id { get; set; }
        public string Alias { get; set; }
        public string PosId { get; set; }
        public string TerminalIdMp { get; set; }
        public bool Activo { get; set; }
    }
}
