using System;

namespace Entidades
{
    // Config del toggle "Conectado con Point Mercado Pago" por Sucursal (no existe concepto de
    // "caja" persistente en el modelo, ver docs/DECISIONS.md -- posInstanceId es efimero por
    // pestana de navegador y no sirve para esto). ConectadoPointDefault lo fija un admin una
    // vez; ConectadoPointUltimaEleccion se sobreescribe cada vez que el cajero cambia el toggle
    // en el modal de forma de pago, y es lo que se usa para precargarlo la proxima vez.
    public class MercadoPagoSucursalConfig
    {
        public int IdSucursal { get; set; }
        public int IdEmpresa { get; set; }

        // Id de "Store" en Mercado Pago (Fase 3) -- null hasta que se crea desde CarniSys.
        // Agregado 2026-09-01, no existia en el esquema original de la Fase 1.
        public string MpStoreId { get; set; }

        public bool ConectadoPointDefault { get; set; }
        public bool ConectadoPointUltimaEleccion { get; set; }
        public DateTime? FechaActualizacionUtc { get; set; }
    }
}
