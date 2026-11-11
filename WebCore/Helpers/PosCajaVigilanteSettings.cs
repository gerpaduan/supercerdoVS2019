// Configuracion de la vigilancia de caja del POS (pos-caja-vigilante.js): cada cuantos segundos el POS pregunta al
// servidor si la caja del vendedor sigue abierta, para cortar la venta si se la cerraron desde otra terminal.
// Se lee de App.config (appSettings, clave "PosCajaVigilante:Segundos"), mismo mecanismo que PosBorradorSettings.
// Ver docs/DECISIONS.md "Vigilancia de caja del POS".
using System.Globalization;

namespace WebCore.Helpers
{
    public static class PosCajaVigilanteSettings
    {
        // Valor por defecto si la clave no esta en App.config. 15 s: la ventana maxima en la que un POS cuya caja ya
        // cerraron puede seguir cargando productos (al guardar, el servidor igual rechaza la venta).
        private const int SegundosDefault = 15;
        private const int SegundosMinimo = 5;
        private const int SegundosMaximo = 300;

        // Cada cuantos segundos el POS verifica que su caja siga abierta.
        public static int Segundos => LeerEntero("PosCajaVigilante:Segundos", SegundosDefault, SegundosMinimo, SegundosMaximo);

        // Lee un entero de config; si falta, es invalido o cae fuera de [min, max], usa el default (o el limite) en
        // vez de romper el POS por una clave mal escrita.
        private static int LeerEntero(string clave, int porDefecto, int min, int max)
        {
            string crudo = (System.Configuration.ConfigurationManager.AppSettings[clave] ?? "").Trim();
            if (!int.TryParse(crudo, NumberStyles.Integer, CultureInfo.InvariantCulture, out int valor))
                return porDefecto;
            if (valor < min) return min;
            if (valor > max) return max;
            return valor;
        }
    }
}
