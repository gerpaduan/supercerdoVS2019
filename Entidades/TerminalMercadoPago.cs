using System;

namespace Entidades
{
    // Terminal fisica Mercado Pago Point (Smart/Smart 2) dada de alta contra una Sucursal.
    // No existe en el modelo un concepto previo de "caja/terminal" (ver docs/DECISIONS.md) --
    // una Sucursal puede tener 1 o mas terminales, cada una identificada por el terminal_id que
    // devuelve la API de Mercado Pago al vincularla.
    public class TerminalMercadoPago
    {
        public int Id { get; set; }
        public int IdEmpresa { get; set; }
        public int IdSucursal { get; set; }

        // Id numerico de la Caja/POS en Mercado Pago (POST /v2/pos) -- hace falta para despues
        // consultar que terminal fisica quedo vinculada. Agregado 2026-09-01.
        public string PosId { get; set; }

        // Id de HARDWARE de la terminal fisica (formato "MARCA_MODELO__SERIAL", ej.
        // "NEWLAND_N950__SBX0000001") -- el que usa la API de Orders para cobrar
        // (config.point.terminal_id). Queda en null hasta que el admin empareja la terminal
        // fisica con la app de Mercado Pago (paso manual) y CarniSys lo verifica via
        // GET /terminals/v1/list.
        public string TerminalIdMp { get; set; }

        public string Alias { get; set; }
        public bool Activo { get; set; }
        public DateTime FechaAltaUtc { get; set; }
    }
}
