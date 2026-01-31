using System;
using System.Collections.Generic;
using System.Data;              // ✅ necesario por DataTable
using System.Globalization;
using Utilidades;

namespace Negocio
{
    public class Parametros : IParametrosContext
    {
        private readonly IEmpresaContext empresa;
        private readonly Datos.Parametros datos;

        // Cache por empresa: evita mezclar
        private readonly Dictionary<int, Dictionary<string, string>> cache
            = new Dictionary<int, Dictionary<string, string>>();

        public Parametros(IEmpresaContext empresaContext)
        {
            if (empresaContext == null) throw new ArgumentNullException("empresaContext");
            empresa = empresaContext;
            datos = new Datos.Parametros(empresa);
        }

        // ==========================================================
        // ✅ NUEVO: para el WinForm (grid de configuración)
        // ==========================================================
        public DataTable ObtenerGrid()
        {
            return datos.ObtenerGrid();
        }

        // ==========================================================
        // ✅ NUEVO: guardar desde el WinForm y refrescar cache
        // ==========================================================
        public void GuardarGrid(DataTable dtParametros)
        {
            datos.GuardarGrid(dtParametros);
            Reload(); // refresca diccionario cacheado
        }

        // ==========================================================
        // Cache
        // ==========================================================
        private Dictionary<string, string> CacheEmpresa()
        {
            int id = empresa.IdEmpresa;

            Dictionary<string, string> dict;
            if (!cache.TryGetValue(id, out dict))
            {
                dict = datos.ObtenerDiccionario();
                cache[id] = dict;
            }
            return dict;
        }

        public void Reload()
        {
            cache[empresa.IdEmpresa] = datos.ObtenerDiccionario();
        }

        public string GetString(string key, string def)
        {
            var dict = CacheEmpresa();
            string v;
            if (dict.TryGetValue(key, out v)) return v;
            return def;
        }

        public decimal GetDecimal(string key, decimal def)
        {
            string raw = GetString(key, null);
            if (string.IsNullOrWhiteSpace(raw)) return def;

            raw = raw.Trim().Replace(',', '.');

            decimal v;
            if (decimal.TryParse(raw, NumberStyles.Any, CultureInfo.InvariantCulture, out v))
                return v;

            return def;
        }

        public float GetFloat(string key, float def)
        {
            string raw = GetString(key, null);
            if (string.IsNullOrWhiteSpace(raw)) return def;

            raw = raw.Trim().Replace(',', '.');

            float v;
            if (float.TryParse(raw, NumberStyles.Any, CultureInfo.InvariantCulture, out v))
                return v;

            return def;
        }

        public int GetInt(string key, int def)
        {
            string raw = GetString(key, null);
            int v;
            return int.TryParse(raw, out v) ? v : def;
        }

        public long GetLong(string key, long def)
        {
            string raw = GetString(key, null);
            long v;
            return long.TryParse(raw, out v) ? v : def;
        }

        public bool GetBool01(string key, bool def)
        {
            string raw = GetString(key, null);
            if (raw == null) return def;

            raw = raw.Trim();
            if (raw == "1") return true;
            if (raw == "0") return false;

            bool b;
            if (bool.TryParse(raw, out b)) return b;

            return def;
        }
    }
}
