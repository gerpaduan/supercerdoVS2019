// Datos de un bloque "datos del registro" compacto (Movimientos/Editar, Stock/Editar, Elaborados/EditarFormula): una grilla de dos
// columnas con etiqueta chica, valor y, opcionalmente, un detalle gris debajo (ej. el usuario que creo o actualizo el registro).
// Se renderiza con Views/Shared/_DatosRegistroCompacto.cshtml; estilos en wwwroot/Content/css/datos-registro.css.
using System.Collections.Generic;

namespace WebCore.Models
{
    public class DatosRegistroCompactoVm
    {
        // Titulo opcional del bloque (ej. "Datos del movimiento"); vacio = sin titulo.
        public string Titulo { get; set; } = "";

        public List<DatoRegistroCompactoItem> Items { get; set; } = new List<DatoRegistroCompactoItem>();
    }

    public class DatoRegistroCompactoItem
    {
        public string Etiqueta { get; set; } = "";

        // Texto ya formateado; vacio se muestra como "-".
        public string Valor { get; set; } = "";

        // Linea gris debajo del valor (ej. el usuario); vacia = no se muestra.
        public string Detalle { get; set; } = "";
    }
}
