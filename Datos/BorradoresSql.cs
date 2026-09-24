using System;
using System.Data;
using System.Data.SqlClient;

namespace Datos
{
    // Helpers compartidos por los repositorios de borradores en SQL Server (Datos.BorradorGenerico y
    // Datos.VentaBorrador): lectura tolerante a NULL y reintento del upsert. Interno: no es API.
    internal static class BorradoresSql
    {
        internal static string Texto(IDataRecord dr, string columna)
        {
            object valor = dr[columna];
            return valor == DBNull.Value ? null : Convert.ToString(valor);
        }

        internal static int? EnteroNulo(IDataRecord dr, string columna)
        {
            object valor = dr[columna];
            return valor == DBNull.Value ? (int?)null : Convert.ToInt32(valor);
        }

        internal static DateTime? FechaNula(IDataRecord dr, string columna)
        {
            object valor = dr[columna];
            return valor == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(valor);
        }

        internal static object ONulo(object valor)
        {
            return valor ?? DBNull.Value;
        }

        // Ejecuta el upsert (UPDATE + INSERT bajo UPDLOCK/HOLDLOCK) y lo reintenta UNA vez si pierde la
        // carrera contra otro guardado del mismo clientId: dos requests casi simultaneos (autoguardado con
        // debounce + latido, o dos pestanas) pueden chocar en el indice unico (2601/2627) o quedar como
        // victima de un deadlock (1205). El segundo intento ya encuentra la fila y solo actualiza.
        internal static T ConReintentoPorCarrera<T>(Func<T> accion)
        {
            try
            {
                return accion();
            }
            catch (SqlException ex) when (ex.Number == 2601 || ex.Number == 2627 || ex.Number == 1205)
            {
                return accion();
            }
        }
    }
}
