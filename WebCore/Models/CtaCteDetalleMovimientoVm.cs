// Modelo del panel desplegable de un movimiento de la cuenta corriente de una persona
// (Finanzas/DetalleMovimiento -> Views/Finanzas/_CtaCteDetalleMovimiento.cshtml). Solo lectura.
// Las ventas reusan Venta (Ventas/_VentaLineas); las compras y los pagos traen sus lineas ya formateadas.
using System.Collections.Generic;

namespace WebCore.Models
{
    public class CtaCteDetalleMovimientoVm
    {
        // "Ventas" | "Compras" | "Pagos" (mismo valor que la columna Tabla del movimiento).
        public string Tabla { get; set; } = "";

        // Solo en ventas: la partial _VentaLineas pinta lineas y total a partir de la venta.
        public Entidades.Venta Venta { get; set; }

        // Compras y pagos: renglones ya formateados (es-AR) y total.
        public string TituloLineas { get; set; } = "";
        public List<CtaCteLineaDetalleVm> Lineas { get; set; } = new List<CtaCteLineaDetalleVm>();
        public string TotalTexto { get; set; } = "";

        // Datos sueltos sobre el registro ("Forma de pago: Efectivo", "Facturada", "Tipo: Cortes", ...).
        public List<string> Notas { get; set; } = new List<string>();
    }

    public class CtaCteLineaDetalleVm
    {
        public string Nombre { get; set; } = "";
        // "cod 210" o vacio.
        public string Codigo { get; set; } = "";
        // "40 kg × $ 7.100,00" (o solo "40 kg" cuando no se muestran precios).
        public string Calculo { get; set; } = "";
        // Subtotal de la linea; vacio cuando el usuario no puede ver precios.
        public string SubtotalTexto { get; set; } = "";
    }
}
