using Microsoft.Playwright;

namespace WebCore.E2ETests;

// swal-single-confirm.js (cargado globalmente en _Layout.cshtml/_LayoutPOS.cshtml) parchea
// Swal.fire una sola vez: mientras el popup esta abierto, un listener de keydown en FASE DE
// CAPTURA sobre document dispara Swal.clickConfirm() con Enter (no depende de que el boton tenga
// el foco real del DOM, asi que funciona igual con o sin un modal Bootstrap de fondo robandole el
// foco). Hasta 2026-09-10 el parche excluia los Swal con showCancelButton:true (confirmar/
// cancelar) -- ver docs/DECISIONS.md "Batch 1: Enter en SweetAlert2 (items 4+9)". Estos tests
// verifican el mecanismo generico (sin depender del flujo de ningun modulo en particular);
// FacturaElectronicaTests.cs cubre el caso real reportado por el usuario (Swal sobre un modal
// Bootstrap).
[Collection("WebCore browser")]
public sealed class SwalSingleConfirmTests
{
    private readonly WebCoreFixture _fixture;

    public SwalSingleConfirmTests(WebCoreFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task SwalConCancelar_Enter_ConfirmaComoSiSeHubieseClickeadoElBotonOk()
    {
        var page = await _fixture.NewAuthenticatedPageAsync();
        await page.GotoAsync($"{WebCoreFixture.BaseUrl}/Home/Index", new PageGotoOptions { WaitUntil = WaitUntilState.Load, Timeout = 20000 });

        await page.EvaluateAsync(@"() => {
            window.__testSwalPromise = Swal.fire({
                icon: 'question',
                title: 'Test Enter',
                showCancelButton: true,
                confirmButtonText: 'Si',
                cancelButtonText: 'No'
            }).then(function (r) { window.__testSwalResult = r.isConfirmed; });
        }");

        await page.Locator(".swal2-confirm").WaitForAsync();
        await page.Keyboard.PressAsync("Enter");
        await page.WaitForFunctionAsync("() => window.__testSwalResult !== undefined");

        Assert.True(await page.EvaluateAsync<bool>("window.__testSwalResult"));
        // La animacion de cierre de SweetAlert2 deja el nodo en el DOM un instante mas -- esperar
        // a que desaparezca en vez de asertar el conteo inmediatamente despues del resultado.
        await page.Locator(".swal2-popup").WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Hidden });

        await page.CloseAsync();
    }

    [Fact]
    public async Task SwalConCancelar_Escape_CierraSinConfirmar()
    {
        var page = await _fixture.NewAuthenticatedPageAsync();
        await page.GotoAsync($"{WebCoreFixture.BaseUrl}/Home/Index", new PageGotoOptions { WaitUntil = WaitUntilState.Load, Timeout = 20000 });

        await page.EvaluateAsync(@"() => {
            window.__testSwalPromise = Swal.fire({
                icon: 'question',
                title: 'Test Escape',
                showCancelButton: true
            }).then(function (r) { window.__testSwalResult = r.isConfirmed; });
        }");

        await page.Locator(".swal2-confirm").WaitForAsync();
        await page.Keyboard.PressAsync("Escape");
        await page.WaitForFunctionAsync("() => window.__testSwalResult !== undefined");

        Assert.False(await page.EvaluateAsync<bool>("window.__testSwalResult"));

        await page.CloseAsync();
    }

    [Fact]
    public async Task SwalDeUnSoloBoton_Enter_SigueCerrandoIgualQueAntes()
    {
        var page = await _fixture.NewAuthenticatedPageAsync();
        await page.GotoAsync($"{WebCoreFixture.BaseUrl}/Home/Index", new PageGotoOptions { WaitUntil = WaitUntilState.Load, Timeout = 20000 });

        await page.EvaluateAsync(@"() => {
            window.__testSwalPromise = Swal.fire({ icon: 'success', title: 'Listo' })
                .then(function (r) { window.__testSwalResult = r.isConfirmed; });
        }");

        await page.Locator(".swal2-confirm").WaitForAsync();
        await page.Keyboard.PressAsync("Enter");
        await page.WaitForFunctionAsync("() => window.__testSwalResult !== undefined");

        Assert.True(await page.EvaluateAsync<bool>("window.__testSwalResult"));

        await page.CloseAsync();
    }
}
