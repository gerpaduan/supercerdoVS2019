// Comandos ESC/POS de formato (negrita, doble tamaño, fuente B) para las lineas de los tickets
// termicos que WebCore manda al PrintAgent (ticketLines). El WinForms los incrusta en el texto
// (Presentacion/Ticket/CrearTicket.cs: Negrita, DobleTamanoA/B, FuenteB); el agente los deja pasar
// tal cual (solo codifica a CP850, y todos los bytes de abajo son < 0x80), asi que no hace falta
// tocar ni reinstalar el agente. Ver docs/DECISIONS.md (2026-09-30).
// Solo aplica al camino con agente: el fallback por navegador imprime el HTML del ticket y no usa estas lineas.
namespace WebCore.Services
{
    public static class EscPosFormato
    {
        // ESC E n -- negrita.
        private const string NegritaOn = "\x1B\x45\x01";
        private const string NegritaOff = "\x1B\x45\x00";

        // GS ! n -- tamaño de caracter; 0x11 = doble ancho y doble alto, 0x00 = normal.
        private const string DobleTamanoOn = "\x1D\x21\x11";
        private const string DobleTamanoOff = "\x1D\x21\x00";

        // ESC M n -- fuente B (mas angosta que la A).
        private const string FuenteBOn = "\x1B\x4D\x01";
        private const string FuenteBOff = "\x1B\x4D\x00";

        // Envuelve una linea en negrita. Abre y cierra dentro de la misma linea: si una linea se
        // pierde o se corta, el formato no queda "colgado" en el resto del ticket.
        public static string Negrita(string linea)
        {
            return NegritaOn + linea + NegritaOff;
        }

        // Fuente B sola (mas chica que la A, equivale a FuenteB del WinForms). La termica es monocromo:
        // no existe "gris"; la fuente B es la forma de destacar menos una linea.
        public static string FuenteB(string linea)
        {
            return FuenteBOn + linea + FuenteBOff;
        }

        // Doble ancho y alto (equivale a DobleTamanoA del WinForms). Ocupa el doble de ancho:
        // el texto ya tiene que venir centrado/truncado con AnchoDobleTamano().
        public static string DobleTamano(string linea)
        {
            return DobleTamanoOn + linea + DobleTamanoOff;
        }

        // Doble tamaño + fuente B (equivale a DobleTamanoB del WinForms, que se usa para el nombre
        // del negocio). Igual que arriba: el ancho util es AnchoDobleTamanoFuenteB().
        public static string DobleTamanoFuenteB(string linea)
        {
            return FuenteBOn + DobleTamanoOn + linea + DobleTamanoOff + FuenteBOff;
        }

        // Caracteres por linea en doble tamaño (fuente A): la mitad del ancho normal.
        // Ej. 32 -> 16 (58 mm), 43 -> 21 (80 mm).
        public static int AnchoDobleTamano(int anchoNormal)
        {
            return anchoNormal / 2;
        }

        // Caracteres por linea en doble tamaño con fuente B. La fuente B tiene ~1,33x mas columnas
        // que la A, asi que rinde 2/3 del ancho normal (WinForms: 22 de 32 y 32 de 48 -> misma
        // proporcion). Redondea hacia abajo: es preferible cortar el texto a que la impresora lo parta en dos lineas.
        // Ej. 32 -> 21 (58 mm), 43 -> 28 (80 mm).
        public static int AnchoDobleTamanoFuenteB(int anchoNormal)
        {
            return anchoNormal * 2 / 3;
        }
    }
}
