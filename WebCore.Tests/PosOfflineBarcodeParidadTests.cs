// POS sin conexion (2026-11-26): paridad entre la busqueda de productos del servidor y la del navegador
// (WebCore/wwwroot/Scripts/app/pos-offline-buscar.js).
// Este test arma un catalogo y una lista de codigos, calcula el resultado esperado con el Negocio.BarcodeInterpreter
// REAL (repositorios en memoria) siguiendo el mismo orden que VentasController.BuscarProducto, y lo compara con el
// fixture versionado WebCore.Tests/js/fixtures/pos-offline-barcode-paridad.json. El test de Node
// (WebCore.Tests/js/pos-offline-buscar.test.js) corre el JS contra ese mismo fixture. Si cambia la logica en C#, este
// test falla: regenerar con  REGENERAR_FIXTURES=1 dotnet test --filter PosOfflineBarcodeParidad  y adaptar el JS.
// Limite conocido: el orden generico -> interno -> codigo completo se replica aca (Flujo), no se llama al controller.
using System.Text.Json;
using System.Text.Json.Nodes;

namespace WebCore.Tests
{
    public class PosOfflineBarcodeParidadTests
    {
        private const int IdEmpresa = 1;
        private const long CodProdGenerico = 900000;

        private sealed class FormatosEnMemoria : Contratos.IFormatoCodigoBarrasRepository
        {
            public List<Entidades.FormatoCodigoBarras> Formatos { get; } = new();
            public List<Entidades.FormatoCodigoBarras> Listar(int idEmpresa) => Formatos;
            public Entidades.FormatoCodigoBarras ObtenerPorId(int id, int idEmpresa) => Formatos.FirstOrDefault(f => f.Id == id)!;
            public Entidades.FormatoCodigoBarras ObtenerActivoPorPrefijo(int idEmpresa, int prefijo) =>
                Formatos.FirstOrDefault(f => f.Activo && f.Prefijo == prefijo && f.IdEmpresa == idEmpresa)!;
            public bool ExistePrefijo(int idEmpresa, int prefijo, int idExcluir) => false;
            public void Agregar(Entidades.FormatoCodigoBarras formato) => throw new NotSupportedException();
            public void Actualizar(Entidades.FormatoCodigoBarras formato) => throw new NotSupportedException();
        }

        private sealed class CortesEnMemoria : Contratos.ICorteBusquedaSimpleRepository
        {
            public List<Entidades.Corte> Cortes { get; } = new();
            public Entidades.Corte findCorteByCodigo(long codigo, bool buscarMaestro) => Copia(Cortes.FirstOrDefault(c => c.codigo == codigo));
            public Entidades.Corte findCorteByCodigoEmpresa(long codigo, int idEmpresa, bool buscarMaestro) =>
                Copia(Cortes.FirstOrDefault(c => c.codigo == codigo && c.IdEmpresa == idEmpresa));

            // El controller pisa PrecioKg del producto devuelto: se entrega una copia para no ensuciar el catalogo.
            private static Entidades.Corte Copia(Entidades.Corte? c) => c == null ? null! : new Entidades.Corte
            {
                IdCorte = c.IdCorte, codigo = c.codigo, corte = c.corte, precioKg = c.precioKg, Pesable = c.Pesable, IdEmpresa = c.IdEmpresa
            };
        }

        private static Entidades.Corte Producto(int id, long codigo, string nombre, float precio, bool pesable) =>
            new Entidades.Corte { IdCorte = id, codigo = codigo, corte = nombre, precioKg = precio, Pesable = pesable, IdEmpresa = IdEmpresa };

        private static Entidades.FormatoCodigoBarras Formato(int id, int prefijo, string nombre, int total, int posCod, int lenCod,
            int posVal, int lenVal, Entidades.TipoValorCodigoBarras tipo, int decimales) => new Entidades.FormatoCodigoBarras
        {
            Id = id, IdEmpresa = IdEmpresa, Nombre = nombre, Prefijo = prefijo, LongitudTotal = total,
            PosicionCodigo = posCod, LongitudCodigo = lenCod, PosicionValor = posVal, LongitudValor = lenVal,
            TipoValor = tipo, CantidadDecimales = decimales, Activo = true
        };

        // EAN-13 con su digito verificador (Utilidades.ValidacionEan).
        private static string Ean13(string doceDigitos)
        {
            int suma = 0;
            for (int i = 0; i < 12; i++) suma += (i % 2 == 0) ? doceDigitos[i] - '0' : (doceDigitos[i] - '0') * 3;
            return doceDigitos + ((10 - suma % 10) % 10);
        }

        private static (CortesEnMemoria cortes, FormatosEnMemoria formatos) Escenario()
        {
            var cortes = new CortesEnMemoria();
            cortes.Cortes.Add(Producto(10, 998, "Asado", 2.42f, true));
            cortes.Cortes.Add(Producto(11, 1001, "Vacío especial", 9999.99f, true));
            cortes.Cortes.Add(Producto(12, 2002, "Chorizo x unidad", 1234.565f, false));
            cortes.Cortes.Add(Producto(13, CodProdGenerico, "Genérico", 1f, false));
            cortes.Cortes.Add(Producto(14, CodProdGenerico + 2, "Genérico 2", 1f, true));
            cortes.Cortes.Add(Producto(15, long.Parse(Ean13("779123456789")), "Gaseosa", 1500f, false));

            var formatos = new FormatosEnMemoria();
            formatos.Formatos.Add(Formato(1, 20, "Peso", 13, 3, 5, 8, 5, Entidades.TipoValorCodigoBarras.Cantidad, 3));
            formatos.Formatos.Add(Formato(2, 21, "Precio", 13, 3, 4, 7, 6, Entidades.TipoValorCodigoBarras.Precio, 2));
            // Formato mal configurado (largo total 12): toda etiqueta de prefijo 22 es "estructura invalida".
            formatos.Formatos.Add(Formato(3, 22, "Roto", 12, 3, 5, 8, 4, Entidades.TipoValorCodigoBarras.Cantidad, 3));
            return (cortes, formatos);
        }

        private static readonly (string codigo, bool ingresoCantidadX)[] Casos =
        {
            ("998", false),
            ("12345", false),
            ("abc", false),
            ("", false),
            ("1.5.5", true),
            ("1500.50G2", true),
            ("1500.5", true),
            ("99.99G7", true),
            ("12.3", false),
            ("998", true),
            ("x1G2", true),
            ("1234,56", true),
            (Ean13("200099801234"), false),
            (Ean13("210998012345"), false),
            (Ean13("210998100000"), false),
            (Ean13("200200200500"), false),
            (Ean13("205555500100"), false),
            (Ean13("230099801000"), false),
            ("2000998012340", false),
            (Ean13("220099801000"), false),
            (Ean13("779123456789"), false),
            ("1001", false),
            ("2002", false)
        };

        // Mismo orden que VentasController.BuscarProducto, con el BarcodeInterpreter real.
        private static JsonObject Flujo(string codigo, bool ingresoCantidadX, Negocio.BarcodeInterpreter interprete, CortesEnMemoria cortes)
        {
            if (string.IsNullOrWhiteSpace(codigo)) return Error();
            codigo = codigo.Replace(",", ".");
            try
            {
                var generico = interprete.InterpretarCodigoGenerico(codigo, ingresoCantidadX, CodProdGenerico);
                if (generico.FormatoInvalido) return Error();
                if (generico.EsGenerico)
                {
                    var corte = cortes.findCorteByCodigoEmpresa(generico.CodigoProducto, IdEmpresa, false);
                    if (corte == null) return NoEncontrado();
                    corte.PrecioKg = generico.PrecioManual!.Value;
                    return ProductoJson(corte, null);
                }

                var interno = interprete.Interpretar(codigo, IdEmpresa);
                if (interno.EsCodigoInterno)
                {
                    if (interno.Caso == Entidades.CasoInterpretacionBarcode.EstructuraInvalida) return Error();
                    if (interno.Caso == Entidades.CasoInterpretacionBarcode.ProductoNoEncontrado) return NoEncontrado();
                    var corte = interno.Producto;
                    decimal? cantidad = null;
                    if (interno.TipoValor == Entidades.TipoValorCodigoBarras.Precio) corte.PrecioKg = (float)interno.Valor!.Value;
                    else cantidad = interno.Valor;
                    return ProductoJson(corte, cantidad);
                }

                long codigoProducto = Convert.ToInt64(codigo);
                var plano = cortes.findCorteByCodigoEmpresa(codigoProducto, IdEmpresa, false);
                return plano == null ? NoEncontrado() : ProductoJson(plano, null);
            }
            catch (Exception)
            {
                return Error();
            }
        }

        private static JsonObject Error() => new() { ["tipo"] = "error" };
        private static JsonObject NoEncontrado() => new() { ["tipo"] = "noEncontrado" };
        private static JsonObject ProductoJson(Entidades.Corte corte, decimal? cantidad) => new()
        {
            ["tipo"] = "producto",
            ["id"] = corte.IdCorte,
            ["codigo"] = corte.codigo,
            ["precioKg"] = Math.Round((double)corte.PrecioKg, 2),
            ["cantidadSugerida"] = cantidad.HasValue ? JsonValue.Create((double)cantidad.Value) : null
        };

        private static string RutaFixture()
        {
            var dir = new DirectoryInfo(AppContext.BaseDirectory);
            while (dir != null && !Directory.Exists(Path.Combine(dir.FullName, "WebCore.Tests", "js")))
                dir = dir.Parent;
            if (dir == null) throw new DirectoryNotFoundException("No se encontro WebCore.Tests/js subiendo desde " + AppContext.BaseDirectory);
            return Path.Combine(dir.FullName, "WebCore.Tests", "js", "fixtures", "pos-offline-barcode-paridad.json");
        }

        private static JsonObject ArmarFixture()
        {
            var (cortes, formatos) = Escenario();
            var interprete = new Negocio.BarcodeInterpreter(formatos, cortes);

            // Catalogo con la MISMA forma que devuelve VentasController.CatalogoOffline.
            var catalogo = new JsonObject
            {
                ["codProdGenerico"] = CodProdGenerico,
                ["productos"] = new JsonArray(cortes.Cortes.Select(c => (JsonNode)new JsonObject
                {
                    ["id"] = c.IdCorte,
                    ["codigo"] = c.codigo,
                    ["nombre"] = c.corte,
                    ["precioKg"] = Math.Round((double)c.PrecioKg, 2),
                    ["pesable"] = c.Pesable
                }).ToArray()),
                ["formatos"] = new JsonArray(formatos.Formatos.Where(f => f.Activo).Select(f => (JsonNode)new JsonObject
                {
                    ["prefijo"] = f.Prefijo,
                    ["nombre"] = f.Nombre,
                    ["longitudTotal"] = f.LongitudTotal,
                    ["posicionCodigo"] = f.PosicionCodigo,
                    ["longitudCodigo"] = f.LongitudCodigo,
                    ["posicionValor"] = f.PosicionValor,
                    ["longitudValor"] = f.LongitudValor,
                    ["tipoValor"] = f.TipoValor.ToString(),
                    ["cantidadDecimales"] = f.CantidadDecimales
                }).ToArray())
            };

            var casos = new JsonArray(Casos.Select(c => (JsonNode)new JsonObject
            {
                ["codigo"] = c.codigo,
                ["ingresoCantidadX"] = c.ingresoCantidadX,
                ["esperado"] = Flujo(c.codigo, c.ingresoCantidadX, interprete, cortes)
            }).ToArray());

            return new JsonObject { ["catalogo"] = catalogo, ["casos"] = casos };
        }

        [Fact]
        public void Fixture_CoincideConElBarcodeInterpreterReal()
        {
            string generado = ArmarFixture().ToJsonString(new JsonSerializerOptions { WriteIndented = true });
            string ruta = RutaFixture();

            if (Environment.GetEnvironmentVariable("REGENERAR_FIXTURES") == "1")
            {
                Directory.CreateDirectory(Path.GetDirectoryName(ruta)!);
                File.WriteAllText(ruta, generado.Replace("\r\n", "\n") + "\n");
            }

            Assert.True(File.Exists(ruta), "Falta el fixture " + ruta + ": correr con REGENERAR_FIXTURES=1.");
            string guardado = File.ReadAllText(ruta).Replace("\r\n", "\n").TrimEnd();
            Assert.Equal(generado.Replace("\r\n", "\n").TrimEnd(), guardado);
        }

        [Fact]
        public void Fixture_CubreTodosLosCasosDelInterprete()
        {
            // Que el escenario siga ejercitando cada rama: producto, no encontrado, error, etiqueta de precio y de peso.
            var casos = ArmarFixture()["casos"]!.AsArray();
            var tipos = casos.Select(c => c!["esperado"]!["tipo"]!.GetValue<string>()).ToList();
            Assert.Contains("producto", tipos);
            Assert.Contains("noEncontrado", tipos);
            Assert.Contains("error", tipos);
            Assert.Contains(casos, c => c!["esperado"]!["cantidadSugerida"] is JsonValue);
        }
    }
}
