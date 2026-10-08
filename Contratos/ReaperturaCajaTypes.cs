using System;

namespace Contratos
{
    // Tipos de retorno de ICierreCajaRepository.obtenerPreviewReapertura / reabrirCierreCaja
    // (reapertura de la ultima caja cerrada de un usuario, 2026-10-06, ver docs/DECISIONS.md).
    // POCOs puros (Contratos es netstandard2.0), igual que CambioSucursalCajaTypes.
    public sealed class PreviewReapertura
    {
        // false = no se puede reabrir; Mensaje explica por que (no es la ultima, ya hay otra abierta, etc.).
        public bool PuedeReabrir { get; set; }
        public string Mensaje { get; set; }

        public int IdCierreCaja { get; set; }
        public int IdUsuarioCaja { get; set; }
        public string UsuarioCaja { get; set; }
        public int IdSucursal { get; set; }
        public string SucursalNombre { get; set; }
        public DateTime? FechaApertura { get; set; }
        public DateTime? FechaCierre { get; set; }
        public string CerradaPor { get; set; }

        // Lo que se deshace: se va a perder al reabrir (queda en la auditoria).
        public double? CajaCierre { get; set; }
        public double? ImporteRetirado { get; set; }
        public double? CajaInicioSiguiente { get; set; }

        // Movimientos del usuario en la sucursal fechados DESPUES del cierre original: al reabrir quedan dentro de la
        // caja (la caja pasa a ser [apertura, ahora]) y se suman al proximo cierre.
        public int VentasDelHueco { get; set; }
        public double ImporteVentasDelHueco { get; set; }
        public int EgresosDelHueco { get; set; }
        public double ImporteEgresosDelHueco { get; set; }
        public int PagosDelHueco { get; set; }

        // Advertencia (no bloquea): el usuario tiene otra caja abierta en otra sucursal.
        public bool TieneCajaAbiertaEnOtraSucursal { get; set; }
    }

    public sealed class ResultadoReapertura
    {
        public bool Ok { get; set; }
        public string Mensaje { get; set; }
    }
}
