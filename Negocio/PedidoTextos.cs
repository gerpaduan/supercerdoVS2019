// Textos y reglas de presentacion del precio estimado de un pedido: UNA sola fuente para la pantalla de armado, el detalle, el PDF,
// el Excel y el mail, para que todos digan lo mismo (el ticket termico no lleva importes: desde 2026-10-20 es solo producto, codigo y cantidad). Puro (sin base ni configuracion): el limite de "precio viejo" lo pasa quien llama.
// Por que existe: el importe del pedido es una ESTIMACION con el ultimo precio de compra de cada producto SIN distinguir proveedor, y muchos
// productos no tienen precio; el usuario tiene que verlo en el mismo lugar donde ve el total. Ver docs/DECISIONS.md (2026-10-19, Pedidos).
using System;

namespace Negocio
{
    public static class PedidoTextos
    {
        // Aclaracion fija de donde sale el importe (la misma frase en todas las salidas).
        public const string AclaracionPrecio =
            "Importes estimados con el precio de la última compra de cada producto, sin tener en cuenta el proveedor.";

        // Con productos sin precio el total esta incompleto: se rotula "parcial" en vez de mostrar un numero que parece completo.
        public static string EtiquetaTotal(int lineasSinPrecio)
        {
            return lineasSinPrecio > 0 ? "Total estimado (parcial)" : "Total estimado";
        }

        // "2 líneas sin precio de compra: no suman al total." (vacio si no hay ninguna).
        public static string AvisoSinPrecio(int lineasSinPrecio)
        {
            if (lineasSinPrecio <= 0) return "";
            return Pedido.Contar(lineasSinPrecio, "línea", "líneas") + " sin precio de compra: " + Pedido.NoSuman(lineasSinPrecio) + " al total.";
        }

        // Aclaracion + aviso de lo que no suma, en una sola frase para pie de pagina o cuerpo de mail.
        public static string Aclaracion(int lineasSinPrecio)
        {
            string aviso = AvisoSinPrecio(lineasSinPrecio);
            return aviso.Length == 0 ? AclaracionPrecio : AclaracionPrecio + " " + aviso;
        }

        // "Los precios marcados tienen más de 90 días" (leyenda de la marca de precio viejo).
        public static string AvisoPrecioViejo(int diasMaximo)
        {
            return "Los precios marcados son de hace más de " + Pedido.Contar(diasMaximo, "día", "días") + ".";
        }

        // Dias entre la compra que dio el precio y la fecha de referencia (la del pedido). Null si no se sabe de cuando es el precio.
        // Nunca negativo: una compra "posterior" a la referencia (reloj o carga rara) cuenta como 0.
        public static int? DiasDelPrecio(DateTime? fechaPrecio, DateTime referencia)
        {
            if (!fechaPrecio.HasValue) return null;
            int dias = (int)(referencia.Date - fechaPrecio.Value.Date).TotalDays;
            return dias < 0 ? 0 : dias;
        }

        // Un precio es "viejo" si la compra que lo dio es de hace MAS de diasMaximo dias respecto de la referencia. Sin fecha no se marca
        // (no se puede afirmar que sea viejo; la fecha ausente se ve como tal en la pantalla).
        public static bool PrecioViejo(DateTime? fechaPrecio, DateTime referencia, int diasMaximo)
        {
            int? dias = DiasDelPrecio(fechaPrecio, referencia);
            return dias.HasValue && dias.Value > diasMaximo;
        }

        // "Proveedor X · 16/09/2026" (o solo la parte que se conoce); vacio si no se sabe nada.
        public static string DescribirOrigenPrecio(string proveedor, DateTime? fechaPrecio)
        {
            string prov = (proveedor ?? "").Trim();
            string fecha = fechaPrecio.HasValue ? fechaPrecio.Value.ToString("dd/MM/yyyy") : "";
            if (prov.Length > 0 && fecha.Length > 0) return prov + " · " + fecha;
            return prov.Length > 0 ? prov : fecha;
        }
    }
}
