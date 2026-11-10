// Advertencia del modal "Cerrar Caja": arma, a partir de una fila del historial de egresos (auditoriaegresoscaja), el texto
// de QUE cambio en cada modificacion que hizo el cajero (monto, tipo, fecha, sucursal y descripcion, antes -> despues).
// Funcion pura para poder probarla; la consulta vive en DatosPostgres.CierreCajaPg.obtenerModificacionesEgresosDelDueno.
// Ver docs/DECISIONS.md (2026-11-10, "Advertencia de egresos modificados por el cajero al cerrar caja").
using System.Globalization;

namespace WebCore.Helpers
{
    // Valores de antes y de despues de UNA modificacion (una fila del historial).
    public sealed class ModificacionEgresoCaja
    {
        public double? MontoAnterior { get; set; }
        public double? MontoNuevo { get; set; }
        public int? IdTipoAnterior { get; set; }
        public int? IdTipoNuevo { get; set; }
        public string TipoAnterior { get; set; } = "";
        public string TipoNuevo { get; set; } = "";
        public DateTime? FechaAnterior { get; set; }
        public DateTime? FechaNueva { get; set; }
        public int? IdSucursalAnterior { get; set; }
        public int? IdSucursalNueva { get; set; }
        public string SucursalAnterior { get; set; } = "";
        public string SucursalNueva { get; set; } = "";
        public string DescripcionAnterior { get; set; } = "";
        public string DescripcionNueva { get; set; } = "";
    }

    public static class EgresoModificadoCajero
    {
        // Texto cuando el formulario se guardo sin cambiar ninguno de los campos que se registran (p. ej. solo el detalle o
        // el conteo de billetes, que no se guardan en el historial).
        public const string SinCambiosRegistrados = "cambió solo el detalle o el conteo de billetes";

        private static readonly CultureInfo CulturaAr = CultureInfo.GetCultureInfo("es-AR");

        // Devuelve una linea por campo que cambio, en orden: monto, tipo, fecha, sucursal, descripcion. Lista vacia si no
        // cambio ninguno. Monto con la tolerancia de centavos de Negocio.CambioCajaCerrada; fecha a nivel de minuto (el
        // formulario no manda segundos).
        public static List<string> DescribirCambios(ModificacionEgresoCaja m)
        {
            var cambios = new List<string>();
            if (m == null) return cambios;

            double montoAnterior = m.MontoAnterior ?? 0;
            double montoNuevo = m.MontoNuevo ?? 0;
            if (Math.Abs(montoAnterior - montoNuevo) >= Negocio.CambioCajaCerrada.ToleranciaImporte)
                cambios.Add("monto " + Pesos(montoAnterior) + " → " + Pesos(montoNuevo));

            if ((m.IdTipoAnterior ?? 0) != (m.IdTipoNuevo ?? 0))
                cambios.Add("tipo " + Texto(m.TipoAnterior) + " → " + Texto(m.TipoNuevo));

            if (Negocio.CambioCajaCerrada.CambioFecha(m.FechaAnterior, m.FechaNueva))
                cambios.Add("fecha " + FechaCorta(m.FechaAnterior) + " → " + FechaCorta(m.FechaNueva));

            if ((m.IdSucursalAnterior ?? 0) != (m.IdSucursalNueva ?? 0))
                cambios.Add("sucursal " + Texto(m.SucursalAnterior) + " → " + Texto(m.SucursalNueva));

            string descripcionAnterior = (m.DescripcionAnterior ?? "").Trim();
            string descripcionNueva = (m.DescripcionNueva ?? "").Trim();
            if (!string.Equals(descripcionAnterior, descripcionNueva, StringComparison.Ordinal))
                cambios.Add("descripción «" + descripcionAnterior + "» → «" + descripcionNueva + "»");

            return cambios;
        }

        // "dd/MM HH:mm" (la advertencia siempre es de una misma caja, el año sobra); "-" si no hay fecha.
        public static string FechaCorta(DateTime? fecha)
        {
            return fecha.HasValue ? fecha.Value.ToString("dd/MM HH:mm", CulturaAr) : "-";
        }

        private static string Pesos(double importe)
        {
            return "$ " + importe.ToString("N2", CulturaAr);
        }

        private static string Texto(string valor)
        {
            return string.IsNullOrWhiteSpace(valor) ? "-" : valor.Trim();
        }
    }
}
