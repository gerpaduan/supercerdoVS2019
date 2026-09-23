using System;

namespace Entidades
{
    // Producto pesado que quedo en pantalla del POS con cantidad estable durante mas de N segundos y
    // salio SIN agregarse al carrito (fraude "pesar, cobrar y borrar el codigo"), tabla
    // ventaproductosinagregar. Es una ADVERTENCIA para el admin, no una acusacion: un cliente
    // arrepentido o un codigo equivocado tambien la disparan. Inicio/Fin los fija el servidor.
    public class ProductoSinAgregar
    {
        public const string OrigenBalanza = "BALANZA";
        public const string OrigenManual = "MANUAL";

        public const string MotivoCodigoBorrado = "CODIGO_BORRADO";
        public const string MotivoCodigoCambiado = "CODIGO_CAMBIADO";
        public const string MotivoCantidadCero = "CANTIDAD_CERO";
        public const string MotivoFinalizar = "FINALIZAR";
        public const string MotivoCierrePestana = "CIERRE_PESTANA";

        public const string RevisionJustificada = "JUSTIFICADA";
        public const string RevisionSospechosa = "SOSPECHOSA";

        public int Id { get; set; }
        public int IdEmpresa { get; set; }
        public int IdSucursal { get; set; }
        public int IdOperador { get; set; }
        public string NombreOperador { get; set; }
        public int IdUsuarioSesion { get; set; }

        // Carrito activo en ese momento (permite enlazar con la venta sin cerrar), si habia.
        public Guid? ClientId { get; set; }

        public string Codigo { get; set; }
        public string Producto { get; set; }
        public decimal PrecioKg { get; set; }
        public decimal CantidadKg { get; set; }
        public decimal Importe { get; set; }
        public int SegundosEnPantalla { get; set; }
        public int SegundosEstables { get; set; }
        public string Origen { get; set; }
        public string Motivo { get; set; }

        // Cuando se tipeo / aparecio el producto y cuando se borro / salio (reloj del servidor).
        public DateTime Inicio { get; set; }
        public DateTime Fin { get; set; }
        public DateTime Creado { get; set; }

        // Revision del admin (null = sin revisar).
        public string Revision { get; set; }
        public int? RevisadaPor { get; set; }
        public string NombreRevisor { get; set; }
        public DateTime? RevisadaEn { get; set; }
        public string ComentarioRevision { get; set; }

        // Datos de entrada que el navegador manda como duraciones: el servidor calcula Inicio/Fin.
        // No se guardan; viajan solo hasta el repositorio.
        public int SegundosDesdeSalida { get; set; }
    }

    // Totales de advertencias de un operador en un rango (para el contexto de patron del admin).
    public class ResumenProductoSinAgregar
    {
        public int Cantidad { get; set; }
        public decimal Importe { get; set; }
    }
}
