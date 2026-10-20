// Textos y clases CSS del estado de una celda de Existencia por sucursales (Negocio.ExistenciaEstado). Los usan el parcial
// _TablaExistenciaPorSucursales.cshtml y las exportaciones a Excel y PDF, para que los tres digan lo mismo. El JS de la
// pantalla (Scripts/app/existencia-sucursales.js, textoEstado/claseEstado/claseStock) repite estas mismas reglas para
// recalcular sin volver a buscar al editar un punto de stock: si se cambian aca, cambiarlas alla.
using System.Globalization;
using Negocio;

namespace WebCore.Helpers
{
    public static class ExistenciaEstadoTextos
    {
        // Texto corto del estado: "Falta 21,650", "Negativo", "En el punto", "OK", "Sin stock", "Sin punto".
        public static string Texto(ExistenciaEstado estado)
        {
            switch (estado.Codigo)
            {
                case ExistenciaEstado.Negativo: return "Negativo";
                case ExistenciaEstado.Falta: return "Falta " + estado.Faltante.ToString("N3");
                case ExistenciaEstado.EnElPunto: return "En el punto";
                case ExistenciaEstado.Ok: return "OK";
                case ExistenciaEstado.SinStock: return "Sin stock";
                default: return "Sin punto";
            }
        }

        // Texto con el punto de stock al lado, para exportaciones donde no hay una segunda linea: "Falta 21,650 (pto 60)".
        public static string TextoConPunto(ExistenciaEstado estado)
        {
            string texto = Texto(estado);
            return estado.Punto > 0 ? texto + " (pto " + estado.Punto.ToString("N0") + ")" : texto;
        }

        // Clase CSS del texto del estado (colores en la hoja de la vista).
        public static string ClaseEstado(ExistenciaEstado estado)
        {
            switch (estado.Codigo)
            {
                case ExistenciaEstado.Negativo: return "ex-est-neg";
                case ExistenciaEstado.Falta: return "ex-est-falta";
                case ExistenciaEstado.EnElPunto: return "ex-est-punto";
                case ExistenciaEstado.Ok: return "ex-est-ok";
                default: return "ex-est-sin";
            }
        }

        // Color del numero de stock: negativo en rojo, bajo el punto en ambar, sin stock apagado.
        public static string ClaseStock(ExistenciaEstado estado)
        {
            switch (estado.Codigo)
            {
                case ExistenciaEstado.Negativo: return "text-danger font-weight-bold";
                case ExistenciaEstado.Falta:
                case ExistenciaEstado.EnElPunto: return "text-warning font-weight-bold";
                case ExistenciaEstado.SinStock: return "text-muted";
                default: return "";
            }
        }
    }
}
