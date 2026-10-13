// Utilidades sobre el codigo de producto (corte.codigo es bigint, ver docs/DECISIONS.md 2026-10-12).
// Vive en Contratos porque lo necesitan la capa de datos (DatosPostgres), WebCore y los tests.
namespace Contratos
{
    public static class CodigoProducto
    {
        /// <summary>
        /// Quita los ceros a la izquierda de un codigo SOLO si es todo digitos, dejando al menos un
        /// caracter ("0123" -> "123", "000" -> "0"). Cualquier otro texto (letras, vacio, null) se
        /// devuelve igual (null -> ""). Un EAN-13 que empieza con 0 se guarda sin ese 0 (bigint):
        /// esto permite comparar lo tipeado/escaneado contra el valor guardado.
        /// </summary>
        public static string SinCerosIzquierda(string valor)
        {
            if (string.IsNullOrEmpty(valor))
                return "";

            string texto = valor.Trim();
            if (texto.Length == 0)
                return "";

            // Solo digitos ASCII: char.IsDigit aceptaria digitos de otros alfabetos.
            for (int i = 0; i < texto.Length; i++)
            {
                if (texto[i] < '0' || texto[i] > '9')
                    return texto;
            }

            int primerNoCero = 0;
            while (primerNoCero < texto.Length - 1 && texto[primerNoCero] == '0')
                primerNoCero++;

            return texto.Substring(primerNoCero);
        }
    }
}
