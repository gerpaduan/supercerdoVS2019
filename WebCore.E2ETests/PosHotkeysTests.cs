using Microsoft.Playwright;

namespace WebCore.E2ETests;

// Regresion de los atajos F2/F5/F6/F7 del POS de Ventas y F6 del POS de PuntosExpendio,
// portados 2026-09-06 (retomado del plan de login/permisos reales -- ver docs/DECISIONS.md).
// Cada atajo abre el modal generico #modalFinanzasPOS con contenido real cargado por AJAX
// (renderConScripts + POSFinanzas/POSCompras/POSEgresos) -- este test verifica que la clase
// "show" se aplique de verdad (geometria real de Bootstrap, no solo presencia de markup) y que
// el contenido cargado no este vacio, para detectar si window.POSModalLoading o el AJAX se
// rompen en un cambio futuro.
[Collection("WebCore browser")]
public sealed class PosHotkeysTests
{
    private readonly WebCoreFixture _fixture;

    public PosHotkeysTests(WebCoreFixture fixture)
    {
        _fixture = fixture;
    }

    [Theory]
    [InlineData("F2")]
    [InlineData("F5")]
    [InlineData("F6")]
    [InlineData("F7")]
    public async Task VentasPOS_Hotkey_AbreModalFinanzasConContenidoReal(string tecla)
    {
        var page = await _fixture.NewAuthenticatedPageAsync();
        var errors = new List<string>();
        page.PageError += (_, msg) => errors.Add(msg);

        await page.GotoAsync($"{WebCoreFixture.BaseUrl}/Ventas/POS", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        await page.WaitForTimeoutAsync(500);

        await page.Keyboard.PressAsync(tecla);
        await page.WaitForTimeoutAsync(1500);

        Assert.Equal(1, await page.Locator("#modalFinanzasPOS.show").CountAsync());
        var contenido = await page.Locator("#contenedorFinanzasPOS").InnerTextAsync();
        Assert.False(string.IsNullOrWhiteSpace(contenido), $"Modal de {tecla} quedo sin contenido -- el AJAX pudo haber fallado.");
        Assert.DoesNotContain("Cargando", contenido);
        Assert.Empty(errors);

        await page.CloseAsync();
    }

    [Fact]
    public async Task PuntosExpendioPOS_F6_AbreModalMisExpendios()
    {
        var page = await _fixture.NewAuthenticatedPageAsync();
        var errors = new List<string>();
        page.PageError += (_, msg) => errors.Add(msg);

        await page.GotoAsync($"{WebCoreFixture.BaseUrl}/PuntosExpendio/POS", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        await page.WaitForTimeoutAsync(500);

        // El modal de seleccion de sector se abre solo al entrar (sector obligatorio, ver
        // header de PuntosExpendio/POS.cshtml) -- el guard de abrirMisExpendios() en
        // punto-expendio-pos.js bloquea F6 a proposito mientras ese modal siga abierto (mismo
        // criterio que el clasico: ningun otro modal de trabajo puede estar abierto detras).
        var sectorModal = page.Locator("#modalSectoresPuntoExpendio");
        if (await sectorModal.IsVisibleAsync())
        {
            await sectorModal.Locator("button, a").First.ClickAsync();
            await page.WaitForTimeoutAsync(500);
        }

        await page.Keyboard.PressAsync("F6");
        await page.WaitForTimeoutAsync(1500);

        Assert.Equal(1, await page.Locator("#modalMisExpendiosPuntoExpendio.show").CountAsync());
        Assert.True(await page.Locator("#tablaMisExpendiosPuntoExpendio tbody tr").CountAsync() > 0);
        Assert.Empty(errors);

        await page.CloseAsync();
    }
}
