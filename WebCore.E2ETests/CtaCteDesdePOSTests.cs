using Microsoft.Playwright;

namespace WebCore.E2ETests;

// Modal Cuenta Corriente desde POS (F2) -- item 6 de la cuarta ronda de pedidos, 2026-09-10, ver
// docs/DECISIONS.md "Batch 4: modal Cuenta Corriente desde POS (F2)". Bug real:
// FinanzasController.CtasCtes no tenia el parametro desdePos -- el gate de permiso
// (Finanza.VerCtasCtes) corria siempre, asi que F2 quedaba bloqueado (AccesoDenegado) para
// cualquier usuario sin el permiso general, en vez de entrar igual (consulta puntual) con el
// saldo oculto. Confirmado contra la base de dev: "cajero" (no-admin) carece de
// Finanza.VerCtasCtes.
[Collection("WebCore browser")]
public sealed class CtaCteDesdePOSTests
{
    private readonly WebCoreFixture _fixture;

    public CtaCteDesdePOSTests(WebCoreFixture fixture)
    {
        _fixture = fixture;
    }

    private static async Task<IPage> LoginAsync(WebCoreFixture fixture, string usuario, string clave)
    {
        var page = await fixture.Browser.NewPageAsync();
        await page.GotoAsync($"{WebCoreFixture.BaseUrl}/Login");
        await page.FillAsync("input[name='Usuario']", usuario);
        await page.FillAsync("input[name='Clave']", clave);
        await page.ClickAsync("button[type='submit']");
        await page.WaitForURLAsync(u => !u.Contains("/Login"));
        return page;
    }

    [Fact]
    public async Task F2DesdePOS_UsuarioSinPermisoGeneral_EntraIgualConSaldoOculto()
    {
        var page = await LoginAsync(_fixture, "cajero", "a");
        await page.GotoAsync($"{WebCoreFixture.BaseUrl}/Ventas/POS", new PageGotoOptions { WaitUntil = WaitUntilState.Load, Timeout = 20000 });
        if (await page.Locator("#modalSucursales.show").CountAsync() > 0)
            await page.Locator("#lnkContinuarSucursalActual").ClickAsync();
        await page.WaitForTimeoutAsync(500);

        await page.Keyboard.PressAsync("F2");
        await page.Locator(".modal.show").WaitForAsync(new LocatorWaitForOptions { Timeout = 10000 });

        var html = await page.Locator(".modal.show .modal-body").First.InnerHTMLAsync();
        Assert.DoesNotContain("Acceso denegado", html, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("txtBuscar", html);

        // Saldo oculto ("***", ViewBag.OcultarSaldo en Finanzas/CtasCtes.cshtml) -- si hay al
        // menos una fila con saldo, debe estar enmascarado, nunca un numero real.
        var celdasSaldo = page.Locator(".modal.show table tbody tr td:last-child");
        if (await celdasSaldo.CountAsync() > 0)
        {
            var primerSaldo = (await celdasSaldo.First.InnerTextAsync()).Trim();
            if (primerSaldo.Length > 0)
                Assert.Equal("***", primerSaldo);
        }

        await page.CloseAsync();
    }

    [Fact]
    public async Task AccesoDirectoSinDesdePos_SigueBloqueadoParaUsuarioSinPermiso()
    {
        // Regresion de seguridad: acceder directo a /Finanzas/CtasCtes (sin pasar por POS/F2) debe
        // seguir bloqueado igual que antes para un usuario sin el permiso general.
        var page = await LoginAsync(_fixture, "cajero", "a");
        await page.GotoAsync($"{WebCoreFixture.BaseUrl}/Finanzas/CtasCtes", new PageGotoOptions { WaitUntil = WaitUntilState.Load, Timeout = 20000 });
        await page.WaitForTimeoutAsync(300);

        var texto = await page.Locator("body").InnerTextAsync();
        Assert.Contains("Acceso denegado", texto, StringComparison.OrdinalIgnoreCase);

        await page.CloseAsync();
    }

    [Fact]
    public async Task F2DesdePOS_UsuarioConPermiso_VeSaldoReal()
    {
        // "ger" es Admin -- PuedeVerSaldosCuentaCorriente/tienePermiso lo trata como acceso total.
        var page = await _fixture.NewAuthenticatedPageAsync();
        await page.GotoAsync($"{WebCoreFixture.BaseUrl}/Ventas/POS", new PageGotoOptions { WaitUntil = WaitUntilState.Load, Timeout = 20000 });
        if (await page.Locator("#modalSucursales.show").CountAsync() > 0)
            await page.Locator("#lnkContinuarSucursalActual").ClickAsync();
        await page.WaitForTimeoutAsync(500);

        await page.Keyboard.PressAsync("F2");
        await page.Locator(".modal.show").WaitForAsync(new LocatorWaitForOptions { Timeout = 10000 });

        var html = await page.Locator(".modal.show .modal-body").First.InnerHTMLAsync();
        Assert.DoesNotContain("Acceso denegado", html, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("***", html);

        await page.CloseAsync();
    }
}
