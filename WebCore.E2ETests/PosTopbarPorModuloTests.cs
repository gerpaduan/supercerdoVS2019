using Microsoft.Playwright;

namespace WebCore.E2ETests;

// Pedido explicito del usuario (2026-09-07, ver docs/DECISIONS.md): "copiar el topbar tal como
// estaba en web clasico para pos ventas y expendio" -- Ventas/POS y PuntosExpendio/POS
// compartian el mismo topbar azul con el texto fijo "CarniSys" (simplificacion de cuando este
// layout no tenia sesion real todavia). Port literal del clasico: Ventas/POS usa el gradiente
// azul del sidebar + el nombre real de la empresa (Sesion.UsuarioActual.Empresa.NombreFantasia);
// PuntosExpendio/POS usa un gradiente ambar/rojizo + el sector elegido, via
// ViewBag.PosBrandLabel/PosBrandName seteados en PuntosExpendioController.POS.
[Collection("WebCore browser")]
public sealed class PosTopbarPorModuloTests
{
    private readonly WebCoreFixture _fixture;

    public PosTopbarPorModuloTests(WebCoreFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task VentasPOS_TopbarAzul_ConNombreRealDeEmpresa()
    {
        var page = await _fixture.NewAuthenticatedPageAsync();
        await page.GotoAsync($"{WebCoreFixture.BaseUrl}/Ventas/POS", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        await page.WaitForTimeoutAsync(500);

        var nav = page.Locator("nav.card-header");
        Assert.True(await nav.EvaluateAsync<bool>("el => el.classList.contains('pos-header-venta')"));
        Assert.False(await nav.EvaluateAsync<bool>("el => el.classList.contains('pos-header-expendio')"));

        var label = (await page.Locator(".pos-brand-label").InnerTextAsync()).Trim();
        var empresa = (await page.Locator(".pos-empresa").InnerTextAsync()).Trim();
        Assert.Equal("PUNTO DE VENTA", label.ToUpperInvariant());
        Assert.NotEqual("CarniSys", empresa);
        Assert.NotEmpty(empresa);

        await page.CloseAsync();
    }

    [Fact]
    public async Task ExpendiosPOS_TopbarAmbar_ConSectorElegido()
    {
        var page = await _fixture.NewAuthenticatedPageAsync();
        await page.GotoAsync($"{WebCoreFixture.BaseUrl}/PuntosExpendio/POS", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        await page.WaitForTimeoutAsync(500);

        var modalSectores = page.Locator("#modalSectoresPuntoExpendio.show");
        string sectorElegido;
        if (await modalSectores.CountAsync() > 0)
        {
            var primerSector = page.Locator(".js-sector-item").First;
            sectorElegido = (await primerSector.InnerTextAsync()).Trim();
            await primerSector.ClickAsync();
            await page.WaitForTimeoutAsync(300);
        }
        else
        {
            sectorElegido = "Sin seleccionar";
        }

        var nav = page.Locator("nav.card-header");
        Assert.True(await nav.EvaluateAsync<bool>("el => el.classList.contains('pos-header-expendio')"));
        Assert.False(await nav.EvaluateAsync<bool>("el => el.classList.contains('pos-header-venta')"));

        var label = (await page.Locator(".pos-brand-label").InnerTextAsync()).Trim();
        var marca = (await page.Locator(".pos-empresa").InnerTextAsync()).Trim();
        Assert.Equal("PUNTO DE EXPENDIO", label.ToUpperInvariant());
        Assert.Equal(sectorElegido, marca);

        await page.CloseAsync();
    }
}
