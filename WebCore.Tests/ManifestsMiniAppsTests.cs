using System.Text.Json;

namespace WebCore.Tests
{
    // Manifests de las mini-apps instalables (wwwroot/manifest-*.json): Stock, Movimientos, Elaborados, Punto de venta y Punto de expendio.
    // Un manifest mal armado (start_url fuera de su scope, id repetido, icono que no existe) hace que el navegador no ofrezca instalar
    // o instale la app equivocada, y no se ve hasta probar en https://carnisys.com. Estos tests lo atrapan al compilar.
    public class ManifestsMiniAppsTests
    {
        // wwwroot de WebCore: se busca subiendo desde la carpeta de salida de los tests hasta la raiz del repo.
        private static string WwwRoot()
        {
            var dir = new DirectoryInfo(AppContext.BaseDirectory);
            while (dir != null)
            {
                var candidato = Path.Combine(dir.FullName, "WebCore", "wwwroot");
                if (File.Exists(Path.Combine(candidato, "manifest.json"))) return candidato;
                dir = dir.Parent;
            }
            throw new DirectoryNotFoundException("No se encontro WebCore/wwwroot subiendo desde " + AppContext.BaseDirectory);
        }

        public static IEnumerable<object[]> ArchivosManifest()
        {
            foreach (var archivo in Directory.GetFiles(WwwRoot(), "manifest-*.json").OrderBy(a => a))
                yield return new object[] { Path.GetFileName(archivo) };
        }

        private static JsonElement Leer(string archivo)
        {
            var json = File.ReadAllText(Path.Combine(WwwRoot(), archivo));
            return JsonDocument.Parse(json).RootElement;
        }

        // "./Stock/Index?ventana=1" -> "Stock/Index?ventana=1"
        private static string SinPuntoBarra(string ruta) => ruta.StartsWith("./") ? ruta.Substring(2) : ruta;

        [Fact]
        public void Estan_Las_Seis_MiniApps()
        {
            var nombres = ArchivosManifest().Select(a => (string)a[0]).ToList();
            foreach (var esperado in new[] { "manifest-stock.json", "manifest-movimientos.json", "manifest-elaborados.json", "manifest-pos.json", "manifest-expendio.json", "manifest-existencia.json" })
                Assert.Contains(esperado, nombres);
        }

        [Theory]
        [MemberData(nameof(ArchivosManifest))]
        public void StartUrl_Cae_Dentro_Del_Scope(string archivo)
        {
            var manifest = Leer(archivo);
            var scope = SinPuntoBarra(manifest.GetProperty("scope").GetString()!);
            var startUrl = SinPuntoBarra(manifest.GetProperty("start_url").GetString()!);
            Assert.StartsWith(scope, startUrl, StringComparison.OrdinalIgnoreCase);
        }

        [Theory]
        [MemberData(nameof(ArchivosManifest))]
        public void Tiene_Nombre_Corto_Y_Modo_Standalone(string archivo)
        {
            var manifest = Leer(archivo);
            Assert.False(string.IsNullOrWhiteSpace(manifest.GetProperty("name").GetString()));
            Assert.False(string.IsNullOrWhiteSpace(manifest.GetProperty("short_name").GetString()));
            Assert.Equal("standalone", manifest.GetProperty("display").GetString());
        }

        [Theory]
        [MemberData(nameof(ArchivosManifest))]
        public void Los_Iconos_Existen_En_Disco(string archivo)
        {
            var manifest = Leer(archivo);
            var icons = manifest.GetProperty("icons").EnumerateArray().ToList();
            Assert.NotEmpty(icons);
            foreach (var icono in icons)
            {
                var ruta = Path.Combine(WwwRoot(), SinPuntoBarra(icono.GetProperty("src").GetString()!).Replace('/', Path.DirectorySeparatorChar));
                Assert.True(File.Exists(ruta), "Falta el icono " + ruta);
            }
        }

        [Fact]
        public void Los_Id_Son_Unicos_Y_Distintos_Del_Manifest_General()
        {
            var ids = ArchivosManifest().Select(a => Leer((string)a[0]).GetProperty("id").GetString()!).ToList();
            ids.Add(Leer("manifest.json").GetProperty("id").GetString()!);
            Assert.Equal(ids.Count, ids.Distinct(StringComparer.OrdinalIgnoreCase).Count());
        }
    }
}
