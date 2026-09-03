using System.Linq;

namespace Utilidades
{
    // Validacion de digito verificador EAN-8/EAN-13, sin dependencias de WinForms (a diferencia
    // de GenerarCodigoBarra.ValidarEAN, que arrastra System.Windows.Forms/System.Drawing).
    // Mismo algoritmo que GenerarCodigoBarra.ValidarEAN y que ProductosController.
    // EsCodigoEan13Valido/EsCodigoEan8Valido -- son 3 implementaciones ya existentes en el
    // repo (ninguna reutilizable entre capas), esta es la 4ta, pensada para que
    // Negocio.BarcodeInterpreter (y cualquier otro consumidor de Negocio/Datos) no dependa de
    // Web ni de WinForms. Las 3 anteriores no se tocan (fuera de alcance). Duplicada tal cual
    // en Utilidades.Core/ValidacionEan.cs para el leg net10.0 de Negocio.csproj (mismo
    // criterio que Conexion.cs/Db.cs/etc. en ese proyecto) -- si el algoritmo cambia,
    // replicar el cambio en las dos.
    public static class ValidacionEan
    {
        public static bool EsEanValido(string codigo)
        {
            if (string.IsNullOrWhiteSpace(codigo))
                return false;

            if (codigo.Length == 13)
                return EsEan13Valido(codigo);

            if (codigo.Length == 8)
                return EsEan8Valido(codigo);

            return false;
        }

        public static bool EsEan13Valido(string codigo)
        {
            if (!EsCandidatoNumerico(codigo, 13))
                return false;

            int suma = 0;
            for (int i = 0; i < 12; i++)
            {
                int digito = codigo[i] - '0';
                suma += (i % 2 == 0) ? digito : digito * 3;
            }

            int digitoVerificador = (10 - (suma % 10)) % 10;
            return digitoVerificador == (codigo[12] - '0');
        }

        public static bool EsEan8Valido(string codigo)
        {
            if (!EsCandidatoNumerico(codigo, 8))
                return false;

            int suma = 0;
            for (int i = 0; i < 7; i++)
            {
                int digito = codigo[i] - '0';
                suma += (i % 2 == 0) ? digito * 3 : digito;
            }

            int digitoVerificador = (10 - (suma % 10)) % 10;
            return digitoVerificador == (codigo[7] - '0');
        }

        private static bool EsCandidatoNumerico(string codigo, int longitudEsperada)
        {
            return !string.IsNullOrWhiteSpace(codigo)
                && codigo.Length == longitudEsperada
                && codigo.All(char.IsDigit);
        }
    }
}
