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
        var page = await _fixture.NewAuthenticatedPageAsync();
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
        // Esperar el Swal DIRECTO (no NetworkIdle + sleep fijo): el Swal de exito tiene
        // opts.timer=2000 (auto-cierra a los 2s, _Layout.cshtml) -- con NetworkIdle+sleep fijo,
        // el tiempo total hasta interactuar con el Swal dependia de cuanto tardara OTRA actividad
        // de red no relacionada (en esta maquina de dev, /DispositivosSeguros dispara un fetch
        // real a getDeviceId() en window.load, ~1.1s de ida y vuelta, ver docs/DECISIONS.md
        // "Batch 6" -- window.CarniSysPrintAgent, agente de impresion real corriendo en esta PC).
        // Esa demora empujaba el tiempo acumulado peligrosamente cerca del auto-cierre de 2s,
        // causando fallas intermitentes reales (a veces "elemento inestable/desprendido del DOM"
        // clickeando casi al mismo tiempo que el auto-cierre, a veces el Swal ya cerrado del
        // todo) -- no es un bug del producto, es una carrera de timing del test contra un timer
        // que no depende de la red en absoluto.
        await page.Locator(".swal2-popup:visible").WaitForAsync(new LocatorWaitForOptions { Timeout = 5000 });

        Assert.Empty(errors);
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
