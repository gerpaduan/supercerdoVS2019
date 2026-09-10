using Microsoft.Playwright;

namespace WebCore.E2ETests;

// Smoke test del sidebar global (ver docs/10-migracion-aspnet-core/README.md, "Sidebar de
// navegacion global") -- confirma que el layout con sidebar renderiza de verdad en un navegador
// real, no solo que el HTML servido lo contiene (lo que se verificaba con curl hasta ahora).
[Collection("WebCore browser")]
public sealed class SidebarSmokeTests
{
    private readonly WebCoreFixture _fixture;

    public SidebarSmokeTests(WebCoreFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Personas_RenderizaConSidebar()
    {
        var page = await _fixture.NewAuthenticatedPageAsync();
        var response = await page.GotoAsync($"{WebCoreFixture.BaseUrl}/Personas", new PageGotoOptions
        {
            WaitUntil = WaitUntilState.DOMContentLoaded,
            Timeout = 20000
        });

        Assert.NotNull(response);
        Assert.Equal(200, response!.Status);
        Assert.Contains("Personas", await page.TitleAsync());
        // .sidebar (2026-09-05, ver docs/DECISIONS.md "UI de WebCore igual al clasico"): antes
        // era ".wc-sidebar", renombrado al portar la identidad visual real del clasico.
        Assert.Equal(1, await page.Locator(".sidebar").CountAsync());

        await page.CloseAsync();
    }

    // Bug real (2026-09-08, ver docs/DECISIONS.md): ".collapse-item"/".collapse-inner" nunca tuvo
    // font-size propio -- heredaba ~1rem de Bootstrap contra los .85rem del item de primer nivel,
    // se veia mas grande. Clasico (sb-admin-2.css) define el mismo 0.85rem en .collapse-inner.
    [Fact]
    public async Task Submenu_TieneMismaFuenteQueElItemDePrimerNivel()
    {
        var page = await _fixture.NewAuthenticatedPageAsync();
        await page.GotoAsync($"{WebCoreFixture.BaseUrl}/Home/Index", new PageGotoOptions { WaitUntil = WaitUntilState.Load });
        await page.WaitForTimeoutAsync(500);

        await page.Locator("a[data-bs-toggle='collapse']").First.ClickAsync();
        await page.WaitForTimeoutAsync(300);

        var topFontSize = await page.Locator(".sidebar .nav-link span").First.EvaluateAsync<string>("el => getComputedStyle(el).fontSize");
        var subFontSize = await page.Locator(".sidebar .collapse-item").First.EvaluateAsync<string>("el => getComputedStyle(el).fontSize");
        Assert.Equal(topFontSize, subFontSize);

        await page.CloseAsync();
    }

    // Bug real (2026-09-08, ver docs/DECISIONS.md): ".sidebar" usaba "min-height:100vh" (un piso,
    // no un techo) en vez de "height:100vh" -- el "overflow-y:auto" ya declarado nunca se
    // activaba (la caja crecia sin limite en vez de scrollear internamente), y al ser
    // position:sticky dentro de un #wrapper flex quedaba atado al scroll del body.
    [Fact]
    public async Task Sidebar_TieneAlturaFijaYScrollPropio()
    {
        var page = await _fixture.NewAuthenticatedPageAsync();
        await page.GotoAsync($"{WebCoreFixture.BaseUrl}/Home/Index", new PageGotoOptions { WaitUntil = WaitUntilState.Load });
        await page.WaitForTimeoutAsync(500);

        var overflowY = await page.Locator(".sidebar").First.EvaluateAsync<string>("el => getComputedStyle(el).overflowY");
        var viewportHeight = await page.Locator(".sidebar").First.EvaluateAsync<double>("() => window.innerHeight");
        var sidebarHeight = await page.Locator(".sidebar").First.EvaluateAsync<double>("el => el.getBoundingClientRect().height");

        Assert.Equal("auto", overflowY);
        // Con "height:100vh" (fix) la caja mide exactamente el viewport, nunca mas (antes, con
        // "min-height:100vh" y contenido largo, podia medir mas que el viewport).
        Assert.True(Math.Abs(sidebarHeight - viewportHeight) < 1, $"sidebar height ({sidebarHeight}) deberia igualar el viewport ({viewportHeight})");

        await page.CloseAsync();
    }

    // Item 3 de la segunda ronda de pedidos (2026-09-10, ver docs/DECISIONS.md): la barra de
    // scroll nativa quedaba fea sobre el gradiente del sidebar -- se oculta visualmente
    // (scrollbar-width:none / ::-webkit-scrollbar{display:none}) sin perder el scroll en si, y se
    // compensa con foco de teclado real (tabindex="0" en el <ul>, que habilita flechas/PageUp/
    // PageDown/Home/End nativos del navegador sobre un contenedor con overflow).
    [Fact]
    public async Task Sidebar_ScrollOcultoPeroFocuseableConScrollRealDisponible()
    {
        var page = await _fixture.NewAuthenticatedPageAsync();
        await page.GotoAsync($"{WebCoreFixture.BaseUrl}/Home/Index", new PageGotoOptions { WaitUntil = WaitUntilState.Load });
        await page.WaitForTimeoutAsync(500);

        var sidebar = page.Locator("#accordionSidebar");
        Assert.True(await sidebar.CountAsync() > 0);

        var tabindex = await sidebar.GetAttributeAsync("tabindex");
        Assert.Equal("0", tabindex);

        var scrollbarWidth = await sidebar.EvaluateAsync<string>("el => getComputedStyle(el).scrollbarWidth");
        Assert.Equal("none", scrollbarWidth);

        // Confirma que hay overflow real disponible para scrollear (no solo que la barra esta
        // oculta) -- si esto alguna vez da igual, el fix de foco por teclado no tendria nada que
        // desplazar.
        var scrollHeight = await sidebar.EvaluateAsync<double>("el => el.scrollHeight");
        var clientHeight = await sidebar.EvaluateAsync<double>("el => el.clientHeight");
        Assert.True(scrollHeight > clientHeight, $"Se esperaba overflow real en el sidebar para probar el scroll por teclado (scrollHeight={scrollHeight}, clientHeight={clientHeight}) -- si esto falla, puede ser que el usuario logueado tenga muy pocos items visibles de menu.");

        await page.CloseAsync();
    }
}
