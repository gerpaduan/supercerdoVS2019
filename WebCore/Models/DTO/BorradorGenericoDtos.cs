// DTOs de los borradores en servidor de Compras/Stock/Movimientos/Embutidos y de su gestion por el
// admin. Ver docs/DECISIONS.md "Borradores de Compras/Stock/Movimientos/Embutidos". Calco de
// BorradorPosDtos.cs (ventas en curso del POS), con Modulo/IdRegistro/IdOperador/NombreOperador en vez
// de PosInstanceId (estos 4 modulos no tienen terminal de POS que resolver: el operador ya viaja
// resuelto por el JS de cada pantalla, igual que hoy viaja idUsuarioCreador en el POST de Guardar).
using System.Text.Json;

namespace WebCore.Models.DTO
{
    // Guardado del formulario (y latido). Payload tiene la forma especifica de cada modulo (opaca para
    // el servidor, salvo Resumen/CantLineas que llegan ya calculados por el JS).
    public class GuardarBorradorGenericoRequest
    {
        public string Modulo { get; set; } = "";
        public Guid ClientId { get; set; }
        public int IdSucursal { get; set; }
        public int? IdRegistro { get; set; }
        public int IdOperador { get; set; }
        public string NombreOperador { get; set; } = "";

        // true = solo "sigo vivo" (el formulario no cambio desde el ultimo guardado).
        public bool SoloLatido { get; set; }

        public string Resumen { get; set; } = "";
        public int CantLineas { get; set; }
        public JsonElement Payload { get; set; }
    }

    // Aviso del navegador (pagehide) de que se cierra la pestana con el formulario sin guardar.
    public class EventoBorradorGenericoRequest
    {
        public string Modulo { get; set; } = "";
        public Guid ClientId { get; set; }
        public int IdSucursal { get; set; }
        public int IdOperador { get; set; }
    }

    public class ListarBorradoresGenericoRequest
    {
        public string Modulo { get; set; } = "";
        public int IdSucursal { get; set; }
        public int IdOperador { get; set; }

        // Formulario de esta pestana (si hay): no se ofrece a si mismo y se detecta si ya se guardo.
        public Guid? ClientIdLocal { get; set; }
    }

    public class AccionBorradorGenericoRequest
    {
        public string Modulo { get; set; } = "";
        public int Id { get; set; }
        public int IdSucursal { get; set; }
        public int IdOperador { get; set; }
        public string Motivo { get; set; } = "";
        public SupervisorAutorizacionDto? Supervisor { get; set; }
    }

    // El registro real ya se guardo (Compras/Movimientos/Embutidos, flujo AJAX): marca el borrador
    // FINALIZADA y vacia su payload. A diferencia de Stock (POST tradicional, lo marca el propio
    // servidor en el mismo request de Guardar), estos 4 modulos avisan aparte tras la respuesta.
    public class MarcarFinalizadoBorradorGenericoRequest
    {
        public string Modulo { get; set; } = "";
        public Guid ClientId { get; set; }
        public int IdResultado { get; set; }
    }
}
