// Convierte el texto plano que se edita en el modal de mail (asunto/mensaje) al cuerpo HTML del correo, con el pie
// institucional de CarniSys. Es el mismo armado que ya existe, copiado en VentasController / FinanzasController /
// PuntosExpendioController; el codigo NUEVO (egreso de caja) usa esta version unica en vez de sumar una cuarta
// copia. Unificar las tres existentes queda como sugerencia aparte (no se tocan por estar fuera del pedido).
namespace WebCore.Services
{
    public static class MailCuerpoHtml
    {
        public static string DesdeTexto(string texto)
        {
            string safe = System.Net.WebUtility.HtmlEncode(texto ?? "");
            safe = safe.Replace("\r\n", "\n").Replace("\r", "\n");
            string cuerpoHtml = "<p>" + safe.Replace("\n\n", "</p><p>").Replace("\n", "<br />") + "</p>";
            string pieHtml =
                "<div style=\"margin-top:24px; padding-top:12px; border-top:1px solid #ddd; font-size:11px; color:#777; line-height:1.4;\">" +
                "<p>CarniSys es un software de gestión comercial para pequeños y medianos comercios, diseñado para administrar ventas, stock y facturación, con integración a balanzas para agilizar la atención en productos pesables.</p>" +
                "</div>";

            return cuerpoHtml + pieHtml;
        }
    }
}
