using Microsoft.Playwright;

namespace WebCore.E2ETests;

// Regresion del bug real encontrado el 2026-09-05 (ver docs/DECISIONS.md, "modo oscuro: cuerpo de
// tablas quedaba blanco"): Bootstrap 5.3 pinta CADA celda (th/td) de una .table con
// background-color: var(--bs-table-bg), que por defecto vale var(--bs-body-bg) -- blanco fijo,
// nunca se entera de nuestro .dark-mode (no usamos el atributo nativo data-bs-theme). El fix
// original solo cubria el thead (con !important puntual) -- el cuerpo de CUALQUIER tabla del
// sitio quedaba blanco en modo oscuro. Corregido pisando las variables --bs-table-* en
// ui-refresh.css. Este test evita que la regresion vuelva sin que nadie lo note.
[Collection("WebCore browser")]
public sealed class DarkModeTests
{
    private readonly WebCoreFixture _fixture;

    public DarkModeTests(WebCoreFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task CajasAbiertas_TablasNoQuedanBlancasEnModoOscuro()
    {
        var page = await _fixture.NewAuthenticatedPageAsync();
        await page.GotoAsync($"{WebCoreFixture.BaseUrl}/Cajas/CajasAbiertas", new PageGotoOptions
        {
            WaitUntil = WaitUntilState.NetworkIdle,
            Timeout = 20000
        });

        await page.EvaluateAsync("localStorage.setItem('carnisys-theme', 'dark')");
        await page.ReloadAsync(new PageReloadOptions { WaitUntil = WaitUntilState.NetworkIdle });
        await page.WaitForTimeoutAsync(300);

        Assert.True(await page.EvaluateAsync<bool>("document.documentElement.classList.contains('dark-mode')"));

        // Las dos tablas de esta vista (#tablaCajas con filas reales, #tablaCierresHistoricos con
        // el estado vacio) son las que se vieron blancas en el hallazgo original -- se comprueba
        // que ninguna celda del cuerpo quede con el blanco fijo que pinta Bootstrap por defecto.
        foreach (var selector in new[] { "#tablaCajas tbody td", "#tablaCierresHistoricos tbody td" })
        {
            var count = await page.Locator(selector).CountAsync();
            Assert.True(count > 0, $"No se encontro ninguna celda para {selector} -- ¿cambio el fixture de datos?");

            var bg = await page.Locator(selector).First.EvaluateAsync<string>("el => getComputedStyle(el).backgroundColor");
            Assert.NotEqual("rgb(255, 255, 255)", bg);
        }

        await page.CloseAsync();
    }

    // Mismo bug, encontrado por el usuario en una hoja de estilos distinta: POS (_LayoutPOS.cshtml)
    // no carga ui-refresh.css -- tiene su propio pos.css con variables --pos-*, separado del resto
    // de la app, y quedo afuera del fix de arriba. El carrito (#tablaItems/.tabla-productos) se
    // veia blanco en modo oscuro, mas visible en el estado vacio ("No hay productos agregados").
    [Fact]
    public async Task PosCarrito_NoQuedaBlancoEnModoOscuro()
    {
        var page = await _fixture.NewAuthenticatedPageAsync();
        await page.GotoAsync($"{WebCoreFixture.BaseUrl}/Ventas/POS", new PageGotoOptions
        {
            WaitUntil = WaitUntilState.NetworkIdle,
            Timeout = 20000
        });

        await page.EvaluateAsync("localStorage.setItem('carnisys-theme', 'dark')");
        await page.ReloadAsync(new PageReloadOptions { WaitUntil = WaitUntilState.NetworkIdle });
        await page.WaitForTimeoutAsync(300);

        Assert.True(await page.EvaluateAsync<bool>("document.documentElement.classList.contains('dark-mode')"));

        var count = await page.Locator(".tabla-productos tbody td").CountAsync();
        Assert.True(count > 0, "No se encontro la fila del estado vacio del carrito -- ¿cambio el markup?");

        var bg = await page.Locator(".tabla-productos tbody td").First.EvaluateAsync<string>("el => getComputedStyle(el).backgroundColor");
        Assert.NotEqual("rgb(255, 255, 255)", bg);

        await page.CloseAsync();
    }

    // Barrido real encontrado el 2026-09-05 (ver docs/DECISIONS.md): patron "*-meta-card" copiado
    // en varias vistas (Stock, Compras, Elaborados) con background:#fff hardcodeado sin variante
    // oscura -- reportado por el usuario en /Stock (panel de detalle de una fila con fondo blanco
    // sobre la fila ya oscura). Corregido reemplazando el color fijo por var(--ui-surface, #fff)
    // en cada vista afectada (Stock/_StockDetalle.cshtml, Compras/_ComprasDetalle.cshtml,
    // Elaborados/_Styles.cshtml). Este test cubre el caso original reportado.
    [Fact]
    public async Task StockDetalleFila_MetaCardNoQuedaBlancaEnModoOscuro()
    {
        var page = await _fixture.NewAuthenticatedPageAsync();

        // fechaDesde/idSucursal explicitos (2026-09-09, ver docs/DECISIONS.md "Fix: 2 tests de
        // Stock con fecha por defecto"): StockController.Index() usa desde=DateTime.Today cuando
        // no se pasa fechaDesde -- sin un movimiento cargado ESE MISMO dia, la tabla queda vacia y
        // no hay boton "Detalles" que clickear. La base compartida de desarrollo no se re-siembra
        // a diario, asi que confiar en el default hacia que este test fallara solo por el paso del
        // calendario real. idSucursal=1 (San Martin) es la sucursal real de "ger" -- se deja
        // explicito por claridad, aunque ya coincide con el default del usuario logueado.
        string fechaDesde = DateTime.UtcNow.AddYears(-1).ToString("yyyy-MM-dd");
        await page.GotoAsync($"{WebCoreFixture.BaseUrl}/Stock?idSucursal=1&fechaDesde={fechaDesde}", new PageGotoOptions
        {
            WaitUntil = WaitUntilState.NetworkIdle,
            Timeout = 20000
        });

        await page.EvaluateAsync("localStorage.setItem('carnisys-theme', 'dark')");
        await page.ReloadAsync(new PageReloadOptions { WaitUntil = WaitUntilState.NetworkIdle });
        await page.WaitForTimeoutAsync(300);

        var btn = page.Locator(".btn-detalles-stock").First;
        Assert.True(await btn.CountAsync() > 0, "No se encontro ningun boton 'Detalles' -- ¿cambio el fixture de datos?");

        await btn.ClickAsync();
        await page.WaitForTimeoutAsync(800);

        var metaCard = page.Locator(".stock-detalle-meta-card").First;
        Assert.True(await metaCard.CountAsync() > 0, "No se cargo el panel de detalle (.stock-detalle-meta-card)");

        var bg = await metaCard.EvaluateAsync<string>("el => getComputedStyle(el).backgroundColor");
        Assert.NotEqual("rgb(255, 255, 255)", bg);

        await page.CloseAsync();
    }
}
