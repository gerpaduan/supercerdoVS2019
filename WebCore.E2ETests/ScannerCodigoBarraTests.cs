using Microsoft.Playwright;

namespace WebCore.E2ETests;

// Item 7 de los 12 pendientes (2026-09-09, ver docs/DECISIONS.md "Batch D: scanner + buscador de
// productos"): "es en addoredit, existe el icono boton de codigo de barra pero no se muestra el
// bloque de escaner con la camara activa como lo hace pos de venta que si funciona" (aclaracion
// del usuario). Causa real: el HTML/JS de Productos/AddOrEdit.cshtml (boton #btnScanner, bloque
// #scannerContainer, inicializacion completa de "new BarcodeScanner({...})") ya estaba completo,
// pero TODO ese bloque vive adentro de "if (typeof BarcodeScanner !== 'undefined')" -- como
// _Layout.cshtml nunca cargaba scanner.js (a diferencia de Web/_LayoutBase.cshtml, que lo carga
// global), la condicion fallaba en silencio y el listener del boton nunca se registraba: click sin
// efecto, sin error visible. Fix: scanner.js/ZXing pasan a cargarse global en _Layout.cshtml
// (paginas con Layout default) y en _LayoutPOS.cshtml (paginas de POS, que usan un layout aparte)
// -- mismo patron que clasico. Los 4 includes puntuales que ya existian (Movimientos/Editar,
// Stock/Editar, Ventas/POS, PuntosExpendio/POS) se sacaron: cargar "class BarcodeScanner" 2 veces
// en la misma pagina es un SyntaxError real en el navegador, no solo redundancia.
[Collection("WebCore browser")]
public sealed class ScannerCodigoBarraTests
{
    private readonly WebCoreFixture _fixture;

    public ScannerCodigoBarraTests(WebCoreFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task ProductosAddOrEdit_BotonScanner_MuestraElBloqueDeCamara()
    {
        var page = await _fixture.NewAuthenticatedPageAsync();
        var errors = new List<string>();
        page.PageError += (_, msg) => errors.Add(msg);

        // Producto real, ya usado por otros tests de esta suite (PreciosSeedHelper/etc): id 1
        // ("CARRE") si existe, si no cae a la lista y toma el primer link real.
        await page.GotoAsync($"{WebCoreFixture.BaseUrl}/Productos", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        var href = await page.EvaluateAsync<string?>("() => { const a = document.querySelector('a[href*=\"/Productos/AddOrEdit/\"]'); return a ? a.getAttribute('href') : null; }");
        Assert.NotNull(href);

        await page.GotoAsync($"{WebCoreFixture.BaseUrl}{href}", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        await page.WaitForTimeoutAsync(500);

        var barcodeScannerDefinido = await page.EvaluateAsync<bool>("() => typeof BarcodeScanner !== 'undefined'");
        Assert.True(barcodeScannerDefinido, "window.BarcodeScanner deberia estar definido -- ¿se saco scanner.js de _Layout.cshtml?");

        if (await page.Locator("#btnHabilitarEdicionProducto").IsVisibleAsync())
        {
            await page.ClickAsync("#btnHabilitarEdicionProducto");
            await page.WaitForTimeoutAsync(500);
        }

        Assert.False(await page.Locator("#scannerContainer").IsVisibleAsync());

        await page.ClickAsync("#btnScanner");
        await page.WaitForTimeoutAsync(800);

        Assert.True(await page.Locator("#scannerContainer").IsVisibleAsync(), "Clickear #btnScanner deberia mostrar #scannerContainer (bloque de camara).");
        Assert.Empty(errors);

        await page.CloseAsync();
    }

    // Regresion del bug real introducido al mover scanner.js a global (2026-09-09): las 4 vistas
    // que ya cargaban scanner.js/zxing puntual (y siguen necesitandolo) no deben volver a
    // declararlo -- "class BarcodeScanner" duplicada tira un SyntaxError real que corta TODO el
    // script de la pagina, no solo el scanner.
    [Theory]
    [InlineData("/Movimientos/Editar/0")]
    [InlineData("/Stock/Editar/0")]
    [InlineData("/Ventas/POS")]
    [InlineData("/PuntosExpendio/POS")]
    public async Task VistasQueYaUsabanScanner_NoDuplicanLaClaseBarcodeScanner(string ruta)
    {
        var page = await _fixture.NewAuthenticatedPageAsync();
        var errors = new List<string>();
        page.PageError += (_, msg) => errors.Add(msg);

        await page.GotoAsync($"{WebCoreFixture.BaseUrl}{ruta}", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        await page.WaitForTimeoutAsync(500);

        var barcodeScannerDefinido = await page.EvaluateAsync<bool>("() => typeof BarcodeScanner !== 'undefined'");
        Assert.True(barcodeScannerDefinido, $"window.BarcodeScanner deberia estar definido en {ruta}.");
        Assert.DoesNotContain(errors, e => e.Contains("already been declared"));

        await page.CloseAsync();
    }
}
