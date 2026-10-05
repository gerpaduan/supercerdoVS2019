using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Negocio
{
    // Estado de las "elevaciones" de formula secreta de UNA sesion web: que productos elaborados (idCorte)
    // tienen la formula visible porque un usuario con permiso se re-autentico, y hasta cuando.
    // Logica pura sobre un string (el controller lo guarda/lee de ISession): sin HttpContext ni reloj
    // global, para poder probarla en Negocio.Tests. Formato del estado: "idCorte,idUsuario,expiraTicksUtc;..."
    // (nunca incluye datos del usuario mas alla de su id). Ver docs/DECISIONS.md "Formula secreta".
    public static class FormulaSecretaElevaciones
    {
        private const char SeparadorEntradas = ';';
        private const char SeparadorCampos = ',';

        private sealed class Entrada
        {
            public int IdCorte;
            public int IdUsuario;
            public long ExpiraTicksUtc;
        }

        // Agrega (o renueva) la elevacion de un producto elaborado. Descarta de paso las ya vencidas.
        public static string Registrar(string estado, int idCorte, int idUsuarioAutorizador, DateTime ahoraUtc, TimeSpan duracion)
        {
            if (idCorte <= 0) throw new ArgumentOutOfRangeException(nameof(idCorte));

            var entradas = Parsear(estado, ahoraUtc);
            entradas.RemoveAll(e => e.IdCorte == idCorte);
            entradas.Add(new Entrada
            {
                IdCorte = idCorte,
                IdUsuario = idUsuarioAutorizador,
                ExpiraTicksUtc = ahoraUtc.Add(duracion).Ticks
            });
            return Serializar(entradas);
        }

        // true si el producto tiene una elevacion vigente (no vencida) en este estado.
        public static bool Vigente(string estado, int idCorte, DateTime ahoraUtc)
        {
            if (idCorte <= 0) return false;
            return Parsear(estado, ahoraUtc).Any(e => e.IdCorte == idCorte);
        }

        // Quita la elevacion de un producto (boton "Ocultar"). existia indica si habia una vigente,
        // para que el caller solo audite el OCULTAR cuando realmente se oculto algo.
        public static string Revocar(string estado, int idCorte, DateTime ahoraUtc, out bool existia)
        {
            var entradas = Parsear(estado, ahoraUtc);
            existia = entradas.RemoveAll(e => e.IdCorte == idCorte) > 0;
            return Serializar(entradas);
        }

        // Quita todas las elevaciones (al salir de la pantalla). cantidad = cuantas estaban vigentes.
        public static string RevocarTodas(string estado, DateTime ahoraUtc, out int cantidad)
        {
            cantidad = Parsear(estado, ahoraUtc).Count;
            return "";
        }

        // Productos con elevacion vigente (para pintar "Ocultar" en los listados).
        public static IReadOnlyList<int> CortesVigentes(string estado, DateTime ahoraUtc)
        {
            return Parsear(estado, ahoraUtc).Select(e => e.IdCorte).Distinct().ToList();
        }

        // Estado corrupto o manipulado: se ignora la entrada mala (nunca se asume una elevacion).
        private static List<Entrada> Parsear(string estado, DateTime ahoraUtc)
        {
            var resultado = new List<Entrada>();
            if (string.IsNullOrEmpty(estado)) return resultado;

            long ahora = ahoraUtc.Ticks;
            foreach (var texto in estado.Split(SeparadorEntradas))
            {
                var partes = texto.Split(SeparadorCampos);
                if (partes.Length != 3) continue;

                int idCorte, idUsuario;
                long expira;
                if (!int.TryParse(partes[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out idCorte)) continue;
                if (!int.TryParse(partes[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out idUsuario)) continue;
                if (!long.TryParse(partes[2], NumberStyles.Integer, CultureInfo.InvariantCulture, out expira)) continue;
                if (idCorte <= 0 || expira <= ahora) continue;

                resultado.Add(new Entrada { IdCorte = idCorte, IdUsuario = idUsuario, ExpiraTicksUtc = expira });
            }
            return resultado;
        }

        private static string Serializar(List<Entrada> entradas)
        {
            return string.Join(SeparadorEntradas.ToString(), entradas.Select(e =>
                e.IdCorte.ToString(CultureInfo.InvariantCulture) + SeparadorCampos
                + e.IdUsuario.ToString(CultureInfo.InvariantCulture) + SeparadorCampos
                + e.ExpiraTicksUtc.ToString(CultureInfo.InvariantCulture)));
        }
    }
}
