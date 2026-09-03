using System.Linq;

namespace Utilidades
{
    // Copia de Utilidades/ValidacionEan.cs para el facade net472;net10.0 -- mismo criterio que
    // el resto de los archivos duplicados en este proyecto (Conexion.cs, Db.cs,
    // EmpresaContextNulo.cs, IEmpresaContext.cs, IParametrosContext.cs, PasswordSecurity.cs):
    // Negocio.csproj referencia Utilidades.Core (no Utilidades, que trae WinForms/COM) para su
    // leg net10.0, y BarcodeInterpreter.cs necesita este tipo en ambos TFMs. Si el algoritmo
    // cambia en un archivo, replicar el cambio en el otro.
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
