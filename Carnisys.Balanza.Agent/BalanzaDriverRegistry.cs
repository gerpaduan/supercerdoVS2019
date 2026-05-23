using System;
using System.Collections.Generic;
using System.Linq;

namespace Carnisys.Balanza.Agent
{
    internal sealed class BalanzaDriverRegistry
    {
        private readonly List<IBalanzaDriver> _drivers;

        public BalanzaDriverRegistry()
        {
            _drivers = new List<IBalanzaDriver>
            {
                new SystelDriver(),
                new KretzDriver()
            };
        }

        public IReadOnlyList<IBalanzaDriver> GetAll()
        {
            return _drivers;
        }

        public IBalanzaDriver Create(string marca)
        {
            return _drivers.FirstOrDefault(d =>
                string.Equals(d.Marca, marca ?? string.Empty, StringComparison.OrdinalIgnoreCase));
        }
    }
}
