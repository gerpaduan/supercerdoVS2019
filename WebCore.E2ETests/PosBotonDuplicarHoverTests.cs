using Microsoft.Playwright;

namespace WebCore.E2ETests;

// Bug real reportado por el usuario (2026-09-07, ver docs/DECISIONS.md): en modo claro, al pasar
// el cursor sobre "Duplicar POS" el texto se volvia invisible. Causa: ".pos-btn-layout" pinta el
// boton con "background: linear-gradient(...)" (shorthand, fija tambien background-image); el fix
// previo para el :hover solo sobreescribia "background-color", asi que el degrade opaco seguia
// pintado ENCIMA y tapaba el color solido -- el texto blanco que pone Bootstrap en su :hover
// quedaba sobre un fondo casi blanco. Fix real: usar el shorthand "background" en el :hover para
// que tambien resetee background-image. Cubre ambas vistas que comparten el mismo boton/CSS
// (Ventas/POS y PuntosExpendio/POS, ver WebCore/Views/Shared/_LayoutPOS.cshtml).
[Collection("WebCore browser")]
public sealed class PosBotonDuplicarHoverTests
{
    private readonly WebCoreFixture _fixture;

    public PosBotonDuplicarHoverTests(WebCoreFixture fixture)
    {
        _fixture = fixture;
    }

    private static async Task AssertHoverEsLegibleAsync(IPage page, string url)
    {
        await page.GotoAsync(url, new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle, Timeout = 20000 });

        // PuntosExpendio/POS sin "sector" en la URL abre el modal bloqueante de seleccion de
        // sector (data-backdrop="static") -- hay que elegir uno para poder interactuar con la
        // topbar de atras, igual que haria un usuario real.
        var modalSectores = page.Locator("#modalSectoresPuntoExpendio.show");
        if (await modalSectores.CountAsync() > 0)
        {
            await page.Locator(".js-sector-item").First.ClickAsync();
            await page.WaitForTimeoutAsync(300);
        }

        // Modo claro explicito -- el bug es especifico de este modo.
        await page.EvaluateAsync("localStorage.removeItem('carnisys-theme')");
        Assert.False(await page.EvaluateAsync<bool>("document.documentElement.classList.contains('dark-mode')"));

        var boton = page.Locator("#btnDuplicarPOS");
        Assert.True(await boton.CountAsync() > 0, $"No se encontro #btnDuplicarPOS en {url}");

        await boton.HoverAsync();
        await page.WaitForTimeoutAsync(150);

        var bg = await boton.EvaluateAsync<string>("el => getComputedStyle(el).backgroundColor");
        var bgImage = await boton.EvaluateAsync<string>("el => getComputedStyle(el).backgroundImage");
        var color = await boton.EvaluateAsync<string>("el => getComputedStyle(el).color");

        // El fondo solido esperado es #4e73df = rgb(78, 115, 223); ademas no debe quedar ningun
        // gradiente pintado encima (backgroundImage debe ser "none").
        Assert.Equal("rgb(78, 115, 223)", bg);
        Assert.Equal("none", bgImage);
        // El texto de Bootstrap en :hover es blanco -- legible solo si el fondo de arriba es solido.
        Assert.Equal("rgb(255, 255, 255)", color);
    }

    [Fact]
    public async Task VentasPOS_HoverDuplicar_TextoLegibleEnModoClaro()
    {
        var page = await _fixture.NewAuthenticatedPageAsync();
        await AssertHoverEsLegibleAsync(page, $"{WebCoreFixture.BaseUrl}/Ventas/POS");
        await page.CloseAsync();
    }

    [Fact]
    public async Task ExpendiosPOS_HoverDuplicar_TextoLegibleEnModoClaro()
    {
        var page = await _fixture.NewAuthenticatedPageAsync();
        await AssertHoverEsLegibleAsync(page, $"{WebCoreFixture.BaseUrl}/PuntosExpendio/POS");
        await page.CloseAsync();
    }
}
