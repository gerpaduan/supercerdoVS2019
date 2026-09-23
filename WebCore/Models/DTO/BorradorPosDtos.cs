// DTOs de las ventas en curso del POS (borrador en servidor), del aviso de "producto sin agregar" y de
// su gestion por el admin. Ver docs/DECISIONS.md "Ventas en curso: borrador en servidor y
// advertencias del POS". Los campos de tiempo que llegan del navegador son DURACIONES en segundos
// (medidas con performance.now()), nunca fechas: el servidor calcula las horas reales.
using System.Text.Json;

namespace WebCore.Models.DTO
{
    // Guardado del carrito (y latido). Payload tiene la misma forma que POSDraft del navegador.
    public class GuardarBorradorRequest
    {
        public Guid ClientId { get; set; }
        public string PosInstanceId { get; set; } = "";
        public int IdSucursalPOS { get; set; }

        // true = solo "sigo vivo" (el carrito no cambio desde el ultimo guardado).
        public bool SoloLatido { get; set; }

        public JsonElement Payload { get; set; }
    }

    // Aviso del navegador (pagehide) de que se cierra la pestana con la venta sin finalizar.
    public class EventoBorradorRequest
    {
        public Guid ClientId { get; set; }
        public string PosInstanceId { get; set; } = "";
        public int IdSucursalPOS { get; set; }
    }

    public class ProductoSinAgregarRequest
    {
        public Guid? ClientId { get; set; }
        public string PosInstanceId { get; set; } = "";
        public int IdSucursalPOS { get; set; }

        // Codigo del producto ya resuelto en pantalla (producto.codigo), no el texto tipeado.
        public string Codigo { get; set; } = "";
        public decimal CantidadKg { get; set; }
        public int SegundosEnPantalla { get; set; }
        public int SegundosEstables { get; set; }
        public int SegundosDesdeSalida { get; set; }
        public string Origen { get; set; } = "";
        public string Motivo { get; set; } = "";
    }

    // Autorizacion de un supervisor con su clave, para cargar/descartar una venta sin cerrar de otro usuario.
    public class SupervisorAutorizacionDto
    {
        public string Usuario { get; set; } = "";
        public string Clave { get; set; } = "";
    }

    public class ListarBorradoresRequest
    {
        // Carrito de esta pestana (si hay): no se ofrece a si mismo y se detecta si ya se finalizo.
        public Guid? ClientIdLocal { get; set; }
        public string PosInstanceId { get; set; } = "";
        public int IdSucursalPOS { get; set; }
    }

    public class AccionBorradorRequest
    {
        public int Id { get; set; }
        public string PosInstanceId { get; set; } = "";
        public int IdSucursalPOS { get; set; }
        public string Motivo { get; set; } = "";
        public SupervisorAutorizacionDto? Supervisor { get; set; }
    }

    public class RevisionProductoSinAgregarRequest
    {
        public int Id { get; set; }
        public string Revision { get; set; } = "";
        public string Comentario { get; set; } = "";
    }
}
