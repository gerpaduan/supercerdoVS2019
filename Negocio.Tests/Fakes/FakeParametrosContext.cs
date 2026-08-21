using System.Collections.Generic;
using Utilidades;

namespace NegocioTests.Fakes
{
    // Fake en memoria de IParametrosContext -- valores fijados a mano por clave, sin tocar
    // ninguna base. Sirve para probar logica de Negocio que lee parametros (ej. porcentajes de
    // comision de tarjeta) sin depender de la tabla real de parametros.
    public sealed class FakeParametrosContext : IParametrosContext
    {
        private readonly Dictionary<string, float> _floats = new Dictionary<string, float>();

        public FakeParametrosContext ConFloat(string key, float valor)
        {
            _floats[key] = valor;
            return this;
        }

        public void Reload() { }

        public float GetFloat(string key, float def) => _floats.TryGetValue(key, out var v) ? v : def;

        public string GetString(string key, string def) => def;
        public decimal GetDecimal(string key, decimal def) => def;
        public int GetInt(string key, int def) => def;
        public long GetLong(string key, long def) => def;
        public bool GetBool01(string key, bool def) => def;
    }
}
