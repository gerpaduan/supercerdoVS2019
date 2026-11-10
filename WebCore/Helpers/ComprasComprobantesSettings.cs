// Configuracion de los comprobantes del proveedor en Compras, leida de App.config (appSettings). Mismo mecanismo que
// PedidoPrecioSettings: sin clave vale el default y un valor invalido se ignora en vez de romper la pantalla (CLAUDE.md 1.1: nada
// hardcodeado en la logica). Que hace: docs/DECISIONS.md (2026-11-09, "Compras: titular propio y comprobantes del proveedor").
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace WebCore.Helpers
{
    public static class ComprasComprobantesSettings
    {
        // Alicuota de IVA que se propone al calcular neto e IVA desde el total ("Compras:AlicuotaIvaDefault").
        private const decimal AlicuotaIvaDefaultPorDefecto = 21m;
        // Alicuotas que ofrece el selector ("Compras:AlicuotasIva", separadas por ;). Son las vigentes de IVA en Argentina.
        private static readonly decimal[] AlicuotasPorDefecto = { 10.5m, 21m, 27m };

        public static decimal AlicuotaIvaDefault
        {
            get
            {
                decimal valor;
                return TryLeerDecimal("Compras:AlicuotaIvaDefault", out valor) && valor >= 0m && valor <= 100m ? valor : AlicuotaIvaDefaultPorDefecto;
            }
        }

        public static decimal[] Alicuotas
        {
            get
            {
                string texto = (System.Configuration.ConfigurationManager.AppSettings["Compras:AlicuotasIva"] ?? "").Trim();
                if (texto.Length == 0) return AlicuotasPorDefecto;

                var lista = new List<decimal>();
                foreach (string parte in texto.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    decimal valor;
                    if (decimal.TryParse(parte.Trim(), NumberStyles.Number, CultureInfo.InvariantCulture, out valor) && valor >= 0m && valor <= 100m)
                        lista.Add(valor);
                }
                return lista.Count == 0 ? AlicuotasPorDefecto : lista.Distinct().OrderBy(x => x).ToArray();
            }
        }

        private static bool TryLeerDecimal(string clave, out decimal valor)
        {
            string texto = (System.Configuration.ConfigurationManager.AppSettings[clave] ?? "").Trim();
            return decimal.TryParse(texto, NumberStyles.Number, CultureInfo.InvariantCulture, out valor);
        }
    }
}
