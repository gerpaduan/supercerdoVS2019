// Tests de WebCore.Services.JerarquiaProductos (filtro "Familia de producto" de Productos): hijos/nietos,
// padres/abuelos, ambos sentidos, ramas hermanas, ciclos de datos y ids desconocidos.
// Ver docs/DECISIONS.md (2026-10-21).
using System.Collections.Generic;
using System.Linq;
using WebCore.Services;

namespace WebCore.Tests
{
    public class JerarquiaProductosTests
    {
        // Arbol de prueba:
        //   1 (res) -> 2 (media res) -> 3 (cuarto) -> 4 (bife)
        //                           \-> 5 (costillar)
        //   1 -> 6 (otra rama)
        //   7 (suelto, sin padre ni hijos)
        private static Dictionary<int, int> Arbol() => new Dictionary<int, int>
        {
            { 1, 0 }, { 2, 1 }, { 3, 2 }, { 4, 3 }, { 5, 2 }, { 6, 1 }, { 7, 0 },
        };

        private static int[] Ordenado(HashSet<int> ids) => ids.OrderBy(x => x).ToArray();

        [Fact]
        public void SoloHijos_IncluyeAlElegidoYTodaSuDescendencia()
        {
            var familia = JerarquiaProductos.ObtenerFamilia(Arbol(), 2, incluirDescendientes: true, incluirAscendientes: false);

            Assert.Equal(new[] { 2, 3, 4, 5 }, Ordenado(familia));
        }

        [Fact]
        public void SoloPadres_IncluyeAlElegidoYTodosSusAscendientes()
        {
            var familia = JerarquiaProductos.ObtenerFamilia(Arbol(), 4, incluirDescendientes: false, incluirAscendientes: true);

            Assert.Equal(new[] { 1, 2, 3, 4 }, Ordenado(familia));
        }

        [Fact]
        public void AmbosSentidos_NoTraeRamasHermanas()
        {
            // Desde 3: sube a 2 y 1, baja a 4. El 5 (hermano de 3) y el 6 (tio) no entran.
            var familia = JerarquiaProductos.ObtenerFamilia(Arbol(), 3, incluirDescendientes: true, incluirAscendientes: true);

            Assert.Equal(new[] { 1, 2, 3, 4 }, Ordenado(familia));
        }

        [Fact]
        public void SinNingunSentido_DevuelveSoloAlElegido()
        {
            var familia = JerarquiaProductos.ObtenerFamilia(Arbol(), 2, incluirDescendientes: false, incluirAscendientes: false);

            Assert.Equal(new[] { 2 }, Ordenado(familia));
        }

        [Fact]
        public void ProductoSinParientes_DevuelveSoloAlElegido()
        {
            var familia = JerarquiaProductos.ObtenerFamilia(Arbol(), 7, incluirDescendientes: true, incluirAscendientes: true);

            Assert.Equal(new[] { 7 }, Ordenado(familia));
        }

        [Fact]
        public void IdInexistente_DevuelveSoloEseId()
        {
            var familia = JerarquiaProductos.ObtenerFamilia(Arbol(), 999, incluirDescendientes: true, incluirAscendientes: true);

            Assert.Equal(new[] { 999 }, Ordenado(familia));
        }

        [Fact]
        public void CicloEnLosDatos_NoCuelgaYDevuelveLosInvolucrados()
        {
            // 10 -> 11 -> 10: dato inconsistente que el recorrido debe tolerar.
            var ciclo = new Dictionary<int, int> { { 10, 11 }, { 11, 10 } };

            var familia = JerarquiaProductos.ObtenerFamilia(ciclo, 10, incluirDescendientes: true, incluirAscendientes: true);

            Assert.Equal(new[] { 10, 11 }, Ordenado(familia));
        }

        [Fact]
        public void CadenaProfunda_NoTieneTopeDeNiveles()
        {
            // 8 niveles: mas que el tope de 4 de CortePg.CalcularNivel.
            var cadena = new Dictionary<int, int> { { 1, 0 } };
            for (int id = 2; id <= 8; id++) cadena[id] = id - 1;

            var abajo = JerarquiaProductos.ObtenerFamilia(cadena, 1, true, false);
            var arriba = JerarquiaProductos.ObtenerFamilia(cadena, 8, false, true);

            Assert.Equal(8, abajo.Count);
            Assert.Equal(8, arriba.Count);
        }
    }
}
