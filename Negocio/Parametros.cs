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
        private readonly Contratos.IParametrosRepository datos;

        // Cache por empresa: evita mezclar
        private readonly Dictionary<int, Dictionary<string, string>> cache
            = new Dictionary<int, Dictionary<string, string>>();

        public Parametros(IEmpresaContext empresaContext)
        {
            if (empresaContext == null) throw new ArgumentNullException("empresaContext");
            empresa = empresaContext;
            datos = new Datos.Parametros(empresa);
        }

        // Constructor nuevo, aditivo: inyecta cualquier implementacion de IParametrosRepository
        // (ej. DatosPostgres.ParametrosPg). Solo lo usa el controller de comparacion.
        public Parametros(Contratos.IParametrosRepository repositorio, IEmpresaContext empresaContext)
        {
            if (empresaContext == null) throw new ArgumentNullException("empresaContext");
            empresa = empresaContext;
            datos = repositorio ?? throw new ArgumentNullException(nameof(repositorio));
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
        // Guardado puntual por nombre (varias claves en una sola transaccion)
        // ==========================================================
        // Para pantallas que editan unos pocos parametros fuera del grid generico (ej. el modal de
        // "Ajuste por forma de pago" de Productos). Resuelve idParametro por nombre desde el
        // catalogo y delega en GuardarGrid, que ya es transaccional en ambos motores: o se
        // guardan todas las claves o ninguna. Lanza InvalidOperationException si alguna clave no
        // existe en el catalogo de parametros (no la crea: el catalogo es global).
        public void GuardarValores(IDictionary<string, string> valoresPorNombre)
        {
            if (valoresPorNombre == null) throw new ArgumentNullException("valoresPorNombre");
            if (valoresPorNombre.Count == 0) return;

            DataTable grid = datos.ObtenerGrid();

            // Mapa nombre -> idParametro (comparacion sin distinguir mayusculas, igual que el diccionario de valores).
            var idPorNombre = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            foreach (DataRow fila in grid.Rows)
            {
                if (fila["nombre"] == DBNull.Value || fila["idParametro"] == DBNull.Value) continue;
                idPorNombre[Convert.ToString(fila["nombre"])] = Convert.ToInt32(fila["idParametro"]);
            }

            var aGuardar = new DataTable();
            aGuardar.Columns.Add("idParametro", typeof(int));
            aGuardar.Columns.Add("valor", typeof(string));

            foreach (var par in valoresPorNombre)
            {
                int idParametro;
                if (!idPorNombre.TryGetValue(par.Key, out idParametro))
                    throw new InvalidOperationException("No existe el parámetro '" + par.Key + "' en el catálogo de parámetros.");

                aGuardar.Rows.Add(idParametro, par.Value);
            }

            GuardarGrid(aGuardar); // transaccional + Reload()
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
