// POS sin conexion (2026-11-26): el login sin conexion verifica el PIN en el navegador (pos-offline-pin.js, WebCrypto)
// contra el hash que guarda el servidor (Utilidades.PasswordSecurity). Este test asegura que el fixture
// WebCore.Tests/js/fixtures/pos-offline-pin.json es un hash VALIDO del servidor; el test de Node
// (WebCore.Tests/js/pos-offline-pin.test.js) verifica que el JS calcula el mismo hash. Regenerar con
// REGENERAR_FIXTURES=1 (crea una sal nueva). Iteraciones bajas solo para que el test sea rapido.
using System.Text.Json;
using System.Text.Json.Nodes;

namespace WebCore.Tests
{
    public class PosOfflinePinParidadTests
    {
        private const string Pin = "4826";
        // Con ñ y simbolo: el navegador tiene que codificar en UTF-8 igual que el servidor.
        private const string Clave = "Señal-2026!";
        private const int Iteraciones = 1000;

        private static string RutaFixture()
        {
            var dir = new DirectoryInfo(AppContext.BaseDirectory);
            while (dir != null && !Directory.Exists(Path.Combine(dir.FullName, "WebCore.Tests", "js")))
                dir = dir.Parent;
            if (dir == null) throw new DirectoryNotFoundException("No se encontro WebCore.Tests/js subiendo desde " + AppContext.BaseDirectory);
            return Path.Combine(dir.FullName, "WebCore.Tests", "js", "fixtures", "pos-offline-pin.json");
        }

        [Fact]
        public void Fixture_EsUnHashValidoDelServidor()
        {
            string ruta = RutaFixture();
            if (Environment.GetEnvironmentVariable("REGENERAR_FIXTURES") == "1")
            {
                var hash = Utilidades.PasswordSecurity.HashPassword(Pin, Iteraciones);
                var json = new JsonObject
                {
                    ["pin"] = Pin,
                    ["pinSalt"] = hash.Salt,
                    ["pinHash"] = hash.Hash,
                    ["pinIteraciones"] = hash.Iterations
                };
                var hashClave = Utilidades.PasswordSecurity.HashPassword(Clave, Iteraciones);
                json["clave"] = Clave;
                json["claveSalt"] = hashClave.Salt;
                json["claveHash"] = hashClave.Hash;
                json["claveIteraciones"] = hashClave.Iterations;
                File.WriteAllText(ruta, json.ToJsonString(new JsonSerializerOptions { WriteIndented = true }).Replace("\r\n", "\n") + "\n");
            }

            Assert.True(File.Exists(ruta), "Falta el fixture " + ruta + ": correr con REGENERAR_FIXTURES=1.");
            var leido = JsonNode.Parse(File.ReadAllText(ruta))!;
            Assert.Equal(Pin, leido["pin"]!.GetValue<string>());
            Assert.Equal(Clave, leido["clave"]!.GetValue<string>());
            Assert.True(Utilidades.PasswordSecurity.VerifyPassword(
                Clave, leido["claveHash"]!.GetValue<string>(), leido["claveSalt"]!.GetValue<string>(),
                leido["claveIteraciones"]!.GetValue<int>()));
            Assert.True(Utilidades.PasswordSecurity.VerifyPassword(
                leido["pin"]!.GetValue<string>(), leido["pinHash"]!.GetValue<string>(),
                leido["pinSalt"]!.GetValue<string>(), leido["pinIteraciones"]!.GetValue<int>()));
            Assert.False(Utilidades.PasswordSecurity.VerifyPassword(
                "4827", leido["pinHash"]!.GetValue<string>(), leido["pinSalt"]!.GetValue<string>(), leido["pinIteraciones"]!.GetValue<int>()));
        }
    }
}
