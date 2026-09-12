using Microsoft.Playwright;

namespace WebCore.E2ETests;

// Modal de seleccion de sucursal (_ModalSucursales.cshtml). Disparo manual (dropdown de usuario,
// 2026-09-07 -- ver docs/DECISIONS.md) ya existia; el disparo AUTOMATICO post-login (item 3 de la
// cuarta ronda de pedidos, 2026-09-10 -- ver docs/DECISIONS.md "Batch 2: modal de sucursal
// automatico al loguearse") nunca se habia portado -- no era un bug, faltaba directo. Port de
// Web/Controllers/LoginController.cs:151-157 (TempData en vez de Session, mismo mecanismo ya
// establecido en este proyecto para AlertMsg) + Web/Views/Shared/_LayoutBase.cshtml:678-685.
[Collection("WebCore browser")]
public sealed class ModalSucursalesTests
{
    private readonly WebCoreFixture _fixture;

    public ModalSucursalesTests(WebCoreFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Login_ConEmpresaDeDosOMasSucursales_MuestraElModalAutomaticamenteUnaSolaVez()
    {
        var page = await _fixture.NewAuthenticatedPageAsync();
        // Sin navegar de nuevo: NewAuthenticatedPageAsync ya hizo el POST real de login (form +
        // submit), que es donde LoginController setea el TempData que dispara el modal en la
        // primera vista que se renderiza despues del redirect.
        await page.WaitForTimeoutAsync(500);

        Assert.Equal(1, await page.Locator("#modalSucursales.show").CountAsync());
        // "ger" en la base de dev opera San Martin, con San Lorenzo como segunda sucursal real de
        // la misma empresa -- confirmado navegando el modal antes de escribir este test.
        Assert.Contains("San Martin", await page.Locator("#modalSucursales .modal-body").InnerTextAsync());
        Assert.Contains("San Lorenzo", await page.Locator("#modalSucursales .modal-body").InnerTextAsync());

        await page.Locator("#lnkContinuarSucursalActual").ClickAsync();
        await page.WaitForTimeoutAsync(400);
        Assert.Equal(0, await page.Locator("#modalSucursales.show").CountAsync());

        // El TempData ya se leyo/consumio en la primera vista -- navegar de nuevo NO debe volver a
        // dispararlo (a diferencia del disparo manual del dropdown, que siempre debe seguir andando).
        await page.GotoAsync($"{WebCoreFixture.BaseUrl}/Home/Index", new PageGotoOptions { WaitUntil = WaitUntilState.Load, Timeout = 20000 });
        await page.WaitForTimeoutAsync(500);
        Assert.Equal(0, await page.Locator("#modalSucursales.show").CountAsync());

        await page.CloseAsync();
    }

    [Fact]
    public async Task DropdownDeUsuario_DisparoManual_SigueFuncionandoIgualQueAntes()
    {
        var page = await _fixture.NewAuthenticatedPageAsync();
        await page.WaitForTimeoutAsync(500);

        // Cerrar el modal automatico primero (mismo usuario/empresa dispara el automatico) para
        // poder probar el disparo manual desde un estado limpio.
        if (await page.Locator("#modalSucursales.show").CountAsync() > 0)
        {
            await page.Locator("#lnkContinuarSucursalActual").ClickAsync();
            await page.WaitForTimeoutAsync(400);
        }

        await page.ClickAsync("#userDropdown");
        await page.WaitForTimeoutAsync(200);
        await page.ClickAsync("a[data-bs-target='#modalSucursales']");
        await page.WaitForTimeoutAsync(400);

        Assert.Equal(1, await page.Locator("#modalSucursales.show").CountAsync());

        await page.CloseAsync();
    }
}
