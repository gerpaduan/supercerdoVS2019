using Microsoft.Playwright;

namespace WebCore.E2ETests;

// Regresion del gap cerrado 2026-09-05 (ver docs/DECISIONS.md): todo controller portado seteaba
// TempData["AlertType"/"Title"/"Msg"] tras un guardado/error (fidelidad de logica), pero
// _Layout.cshtml -- a diferencia de Web/Views/Shared/_LayoutBase.cshtml -- nunca lo leia: guardar
// algo en WebCore no daba ningun feedback visual. Port literal del mismo mecanismo (SweetAlert2,
// ya cargado globalmente).
[Collection("WebCore browser")]
public sealed class TempDataAlertTests
{
    private readonly WebCoreFixture _fixture;

    public TempDataAlertTests(WebCoreFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task AgregarDispositivoSeguro_MuestraAlertaYSeLimpiaSolo()
    {
        var page = await _fixture.Browser.NewPageAsync();
        var errors = new List<string>();
        page.PageError += (_, msg) => errors.Add(msg);

        await page.GotoAsync($"{WebCoreFixture.BaseUrl}/DispositivosSeguros", new PageGotoOptions
        {
            WaitUntil = WaitUntilState.NetworkIdle,
            Timeout = 20000
        });

        var nroSerie = "E2E-ALERT-" + DateTime.UtcNow.Ticks;
        await page.FillAsync("#NumeroSerie", nroSerie);
        await page.FillAsync("#Descripcion", "E2E TempDataAlertTests");
        await page.ClickAsync("button[type='submit']");
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await page.WaitForTimeoutAsync(400);

        Assert.Empty(errors);
        Assert.Equal(1, await page.Locator(".swal2-popup:visible").CountAsync());
        Assert.Contains("agregó correctamente", await page.Locator(".swal2-popup").InnerTextAsync());

        // Limpieza: mismo dispositivo recien creado, round-trip sin dejar rastro (mismo criterio
        // ya usado en esta migracion para datos de prueba reversibles). El boton "Eliminar" pide
        // confirm() nativo -- hay que armar el handler ANTES de clickear.
        await page.Locator(".swal2-confirm").ClickAsync();
        page.Dialog += async (_, dialog) => await dialog.AcceptAsync();
        var filaCreada = page.Locator($"tr:has-text('{nroSerie}')");
        await filaCreada.Locator("button[type='submit']").ClickAsync();
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        await page.CloseAsync();
    }
}
