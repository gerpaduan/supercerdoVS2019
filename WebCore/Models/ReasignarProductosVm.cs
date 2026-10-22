// Modal "productos que usan este tipo / esta marca" (Productos > Tipos / Marcas, 2026-10-22): lista de productos
// con su tipo o marca actual y un selector para pasarlos a otro en una sola operacion. Ver docs/DECISIONS.md.
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace WebCore.Models
{
    public class ReasignarProductosVm
    {
        // "Tipo" o "Marca": decide los textos del modal y a que endpoint se envia el guardado.
        public string Campo { get; set; } = "";

        // Nombre visible del tipo/marca que se quiere eliminar.
        public string OrigenTexto { get; set; } = "";

        // Valor que viaja al guardar: el texto del tipo, o el id de la marca.
        public string OrigenValor { get; set; } = "";

        // Marca: permite elegir "(Sin marca)" (el producto queda sin marca; la marca es opcional en el producto).
        public bool PermiteSinValor { get; set; }

        public string UrlGuardar { get; set; } = "";

        public List<ReasignarProductoItemVm> Items { get; set; } = new List<ReasignarProductoItemVm>();

        // Destinos posibles (ya sin el origen).
        public List<SelectListItem> Opciones { get; set; } = new List<SelectListItem>();
    }

    public class ReasignarProductoItemVm
    {
        public int IdCorte { get; set; }
        public long Codigo { get; set; }
        public string Producto { get; set; } = "";
        public string Actual { get; set; } = "";
    }

    // Una fila del guardado: Nuevo vacio = sin cambio; en Marca "0" = sin marca.
    public class ReasignacionCambioVm
    {
        public int IdCorte { get; set; }
        public string Nuevo { get; set; } = "";
    }
}
