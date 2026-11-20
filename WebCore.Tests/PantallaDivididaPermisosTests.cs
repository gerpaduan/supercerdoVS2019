// Tests de la Pantalla dividida (2026-11-20, docs/DECISIONS.md "Pantalla dividida"):
//  - PantallaDivididaRutas.Parsear: de URLs de paneles a (controller, action), tratando la entrada como hostil.
//  - PermisosHelper.LimpiarOperadoresFueraDe: el operador autorizado vive solo mientras su pantalla sigue abierta en algun panel.
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using WebCore.Helpers;

namespace WebCore.Tests
{
    public class PantallaDivididaPermisosTests
    {
        // ISession en memoria (mismo recurso que OperadorEgresosCajaCaducidadTests).
        private sealed class SesionEnMemoria : ISession
        {
            private readonly Dictionary<string, byte[]> _valores = new Dictionary<string, byte[]>();

            public bool IsAvailable => true;
            public string Id => "sesion-de-prueba";
            public IEnumerable<string> Keys => _valores.Keys;
            public void Clear() => _valores.Clear();
            public Task CommitAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
            public Task LoadAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
            public void Remove(string key) => _valores.Remove(key);
            public void Set(string key, byte[] value) => _valores[key] = value;
            public bool TryGetValue(string key, out byte[] value) => _valores.TryGetValue(key, out value!);
        }

        private static readonly Entidades.Usuario Operador = new Entidades.Usuario { Id = 7, Nombre = "Operador de prueba", Activo = true };

        private static SesionEnMemoria SesionConOperadores(params string[] modulos)
        {
            var sesion = new SesionEnMemoria();
            foreach (var modulo in modulos) PermisosHelper.RegistrarOperadorModulo(sesion, modulo, Operador);
            return sesion;
        }

        private static bool Tiene(ISession sesion, string modulo) => PermisosHelper.ObtenerOperadorModulo(sesion, modulo) != null;

        private static IReadOnlyCollection<(string Controller, string Action)> Rutas(params (string, string)[] rutas) => rutas;

        // ---------- Parsear ----------

        [Theory]
        [InlineData("/Stock/Index", "Stock", "Index")]
        [InlineData("/Stock", "Stock", "Index")]
        [InlineData("/Stock/Index/15", "Stock", "Index")]            // parametro de ruta: no cuenta
        [InlineData("/Reportes/Index?popup=true", "Reportes", "Index")]
        [InlineData("/Compras/Editar#arriba", "Compras", "Editar")]
        [InlineData("/", "Home", "Index")]
        public void Parsear_RutasRelativas_DevuelveControllerYAccion(string url, string controller, string accion)
        {
            var r = PantallaDivididaRutas.Parsear(new[] { url }, "");

            Assert.Single(r);
            Assert.Equal((controller, accion), r[0]);
        }

        [Fact]
        public void Parsear_QuitaElPathBase_SoloEnLimiteDeSegmento()
        {
            var r = PantallaDivididaRutas.Parsear(new[] { "/carnisys/Stock/Index", "/carnisysx/Movimientos/Index" }, "/carnisys");

            Assert.Equal(2, r.Count);
            Assert.Equal(("Stock", "Index"), r[0]);
            // "/carnisysx/..." NO tiene el prefijo "/carnisys/": se interpreta como controller "carnisysx".
            Assert.Equal(("carnisysx", "Movimientos"), r[1]);
        }

        [Theory]
        [InlineData("http://otro.com/Stock/Index")]
        [InlineData("//otro.com/Stock/Index")]
        [InlineData("javascript:alert(1)")]
        [InlineData("Stock/Index")]                 // sin barra inicial
        [InlineData("/Sto ck/Index")]
        [InlineData("/Stock/<script>")]
        [InlineData("/Stock/../../etc")]
        [InlineData("")]
        [InlineData("   ")]
        public void Parsear_EntradaHostil_SeIgnora(string url)
        {
            Assert.Empty(PantallaDivididaRutas.Parsear(new[] { url }, ""));
        }

        [Fact]
        public void Parsear_NullYTope()
        {
            Assert.Empty(PantallaDivididaRutas.Parsear(null, ""));

            var muchas = Enumerable.Range(0, PantallaDivididaRutas.MaxPaneles + 5).Select(i => "/Stock/Index").ToList();
            Assert.Equal(PantallaDivididaRutas.MaxPaneles, PantallaDivididaRutas.Parsear(muchas, "").Count);
        }

        // ---------- LimpiarOperadoresFueraDe ----------

        [Fact]
        public void ConElPanelDeComprasAbierto_ElOperadorDeComprasSigueVivo()
        {
            var sesion = SesionConOperadores("Compras");

            PermisosHelper.LimpiarOperadoresFueraDe(sesion, Rutas(("Stock", "Index"), ("Compras", "Index")));

            Assert.True(Tiene(sesion, "Compras"));
        }

        [Fact]
        public void SinElPanelDeComprasAbierto_ElOperadorDeComprasSePierde()
        {
            var sesion = SesionConOperadores("Compras");

            PermisosHelper.LimpiarOperadoresFueraDe(sesion, Rutas(("Stock", "Index"), ("Movimientos", "Index")));

            Assert.False(Tiene(sesion, "Compras"));
        }

        [Fact]
        public void SinPaneles_BorraTodosLosOperadores()
        {
            var sesion = SesionConOperadores("Compras", "Ventas", "Finanzas", PermisosHelper.ModuloOperadorEgresosCaja);

            PermisosHelper.LimpiarOperadoresFueraDe(sesion, Rutas());

            Assert.False(Tiene(sesion, "Compras"));
            Assert.False(Tiene(sesion, "Ventas"));
            Assert.False(Tiene(sesion, "Finanzas"));
            Assert.False(Tiene(sesion, PermisosHelper.ModuloOperadorEgresosCaja));
        }

        [Fact]
        public void LaListaNull_SeTrataComoSinPaneles()
        {
            var sesion = SesionConOperadores("Compras");

            PermisosHelper.LimpiarOperadoresFueraDe(sesion, null);

            Assert.False(Tiene(sesion, "Compras"));
        }

        [Fact]
        public void SoloSeBorraElModuloQueSeCerro()
        {
            var sesion = SesionConOperadores("Compras", "Finanzas");

            PermisosHelper.LimpiarOperadoresFueraDe(sesion, Rutas(("Finanzas", "Index")));

            Assert.False(Tiene(sesion, "Compras"));
            Assert.True(Tiene(sesion, "Finanzas"));
        }

        [Theory]
        [InlineData("Cajas", "EgresosCaja", true)]
        [InlineData("cajas", "egresoscaja", true)]    // sin distinguir mayusculas
        [InlineData("Cajas", "CajasAbiertas", false)]  // otra vista del mismo controller: ya no es la pantalla
        [InlineData("Compras", "EgresosCaja", false)]
        public void EgresosCaja_SigueVivoSoloSiSuPantallaExactaSigueAbierta(string controller, string accion, bool sigueVivo)
        {
            var sesion = SesionConOperadores(PermisosHelper.ModuloOperadorEgresosCaja);

            PermisosHelper.LimpiarOperadoresFueraDe(sesion, Rutas((controller, accion)));

            Assert.Equal(sigueVivo, Tiene(sesion, PermisosHelper.ModuloOperadorEgresosCaja));
        }

        [Fact]
        public void SinOperadorEnSesion_NoHaceNadaYNoFalla()
        {
            var sesion = new SesionEnMemoria();

            PermisosHelper.LimpiarOperadoresFueraDe(sesion, Rutas(("Stock", "Index")));
            PermisosHelper.LimpiarOperadoresFueraDe(null, Rutas(("Stock", "Index")));

            Assert.Empty(sesion.Keys);
        }
    }
}
