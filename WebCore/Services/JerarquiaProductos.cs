// Arbol padre/hijo de productos (corte maestro): dado un producto devuelve su "familia", es decir
// el propio producto mas sus descendientes (hijos, nietos, bisnietos...) y/o sus ascendientes (padre,
// abuelo, bisabuelo...). Lo usa el filtro "Familia de producto" de Productos > Filtros avanzados
// (ProductosController.Index). Funcion pura, sin acceso a datos. Ver docs/DECISIONS.md (2026-10-21).
using System.Collections.Generic;

namespace WebCore.Services
{
    public static class JerarquiaProductos
    {
        // maestroPorCorte: idCorte -> idCorte del maestro (0 o ausente = no tiene padre). Cuenta toda relacion
        // de maestro, tanto cortes derivados como presentaciones (cajas/packs).
        // Devuelve siempre el propio idCorte, aunque no figure en el diccionario. Sin tope de profundidad
        // (el limite de 4 niveles de CortePg.CalcularNivel es solo para el campo "nivel"); el conjunto
        // `visitados` corta cualquier ciclo que dejen datos inconsistentes (A -> B -> A).
        public static HashSet<int> ObtenerFamilia(
            IReadOnlyDictionary<int, int> maestroPorCorte,
            int idCorte,
            bool incluirDescendientes,
            bool incluirAscendientes)
        {
            var familia = new HashSet<int> { idCorte };

            if (incluirAscendientes)
            {
                // Se sube por el maestro hasta un producto sin padre (o hasta repetir uno ya visto).
                int actual = idCorte;
                while (maestroPorCorte.TryGetValue(actual, out int padre) && padre > 0 && familia.Add(padre))
                    actual = padre;
            }

            if (incluirDescendientes)
            {
                // Indice inverso padre -> hijos, armado una sola vez para recorrer en ancho.
                var hijosPorPadre = new Dictionary<int, List<int>>();
                foreach (var par in maestroPorCorte)
                {
                    if (par.Value <= 0) continue;
                    if (!hijosPorPadre.TryGetValue(par.Value, out var hijos))
                        hijosPorPadre[par.Value] = hijos = new List<int>();
                    hijos.Add(par.Key);
                }

                var visitados = new HashSet<int> { idCorte };
                var pendientes = new Queue<int>();
                pendientes.Enqueue(idCorte);
                while (pendientes.Count > 0)
                {
                    int padre = pendientes.Dequeue();
                    if (!hijosPorPadre.TryGetValue(padre, out var hijos)) continue;
                    foreach (int hijo in hijos)
                    {
                        if (!visitados.Add(hijo)) continue;
                        familia.Add(hijo);
                        pendientes.Enqueue(hijo);
                    }
                }
            }

            return familia;
        }
    }
}
