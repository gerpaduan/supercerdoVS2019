// TODO(claude): copia temporal, ver mismo comentario en IEmpresaContext.cs de esta carpeta.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Utilidades
{
    public interface IParametrosContext
    {
        void Reload(); // recarga cache para la empresa actual

        string GetString(string key, string def);
        decimal GetDecimal(string key, decimal def);
        float GetFloat(string key, float def);   // ✅ nuevo
        int GetInt(string key, int def);
        long GetLong(string key, long def);
        bool GetBool01(string key, bool def);

    }
}
