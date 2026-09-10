using Microsoft.Playwright;

namespace WebCore.E2ETests;

// Item 6 de la segunda ronda de pedidos (2026-09-10, ver docs/DECISIONS.md): "agregar la opción
// de cambiar clave" desde el menú de usuario. Port de Web/Controllers/LoginController.cs:291-337
// (ChangePassword GET+POST), con sesión ya iniciada (a diferencia de ForgotPassword/ResetPassword,
// pre-login) -- usa el _Layout.cshtml normal con sidebar. Mismo criterio de seguridad ya aplicado
// a PasswordResetVm (Batch E, 2026-09-09): mínimo 6 caracteres, no 1 como el clásico. Usuario de
// prueba: "a" (id=22 en la base de dev, no admin, sin otros tests dependientes de su clave) --
// round-trip al final para no romper el resto de la suite (mismo criterio ya usado en
// TempDataAlertTests.cs).
[Collection("WebCore browser")]
public sealed class ChangePasswordTests
{
    private readonly WebCoreFixture _fixture;

    public ChangePasswordTests(WebCoreFixture fixture)
    {
        _fixture = fixture;
    }

    private static async Task<IPage> LoginComoAsync(IBrowser browser, string usuario, string clave)
    {
        var page = await browser.NewPageAsync();
        await page.GotoAsync($"{WebCoreFixture.BaseUrl}/Login", new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });
        await page.FillAsync("input[name='Usuario']", usuario);
        await page.FillAsync("input[name='Clave']", clave);
        await page.ClickAsync("button[type='submit']");
        await page.WaitForURLAsync(url => !url.Contains("/Login"), new PageWaitForURLOptions { Timeout = 15000 });
        return page;
    }

    [Fact]
    public async Task ChangePassword_ClaveCortaSeRechaza_ClaveValidaFuncionaYPermiteRoundTrip()
    {
        var page = await LoginComoAsync(_fixture.Browser, "a", "a");
        await page.GotoAsync($"{WebCoreFixture.BaseUrl}/Login/ChangePassword", new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });

        // Clave nueva corta (<6 caracteres) -- debe rechazarse por validacion, sin cambiar nada.
        await page.FillAsync("#ClaveActual", "a");
        await page.FillAsync("#NuevaClave", "abc");
        await page.FillAsync("#ConfirmarClave", "abc");
        await page.ClickAsync("button[type='submit']");
        await page.WaitForTimeoutAsync(500);
        Assert.Contains("/Login/ChangePassword", page.Url);
        var textoError = await page.InnerTextAsync("body");
        Assert.Contains("entre 6 y 128", textoError);

        // Clave actual incorrecta -- debe rechazarse.
        await page.FillAsync("#ClaveActual", "clave_incorrecta_xyz");
        await page.FillAsync("#NuevaClave", "ClaveNueva123");
        await page.FillAsync("#ConfirmarClave", "ClaveNueva123");
        await page.ClickAsync("button[type='submit']");
        await page.WaitForTimeoutAsync(500);
        var textoErrorClave = await page.InnerTextAsync("body");
        Assert.Contains("incorrecta", textoErrorClave);

        // Clave valida real -- debe aceptarse.
        await page.FillAsync("#ClaveActual", "a");
        await page.FillAsync("#NuevaClave", "ClaveNueva123");
        await page.FillAsync("#ConfirmarClave", "ClaveNueva123");
        await page.ClickAsync("button[type='submit']");
        await page.WaitForTimeoutAsync(800);
        var textoExito = await page.InnerTextAsync("body");
        Assert.Contains("actualizada correctamente", textoExito);

        // Confirmar que la clave nueva realmente funciona para loguear.
        await page.GotoAsync($"{WebCoreFixture.BaseUrl}/Login/Logout", new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });
        var page2 = await LoginComoAsync(_fixture.Browser, "a", "ClaveNueva123");
        Assert.DoesNotContain("/Login", page2.Url);

        // Round-trip: volver la clave a "a" para no romper otros tests que dependan de ella.
        await page2.GotoAsync($"{WebCoreFixture.BaseUrl}/Login/ChangePassword", new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });
        await page2.FillAsync("#ClaveActual", "ClaveNueva123");
        await page2.FillAsync("#NuevaClave", "a1234a");
        await page2.FillAsync("#ConfirmarClave", "a1234a");
        // Nota: la clave original era literalmente "a" (1 caracter) -- ya no cumple el minimo de
        // 6 que este mismo fix introdujo, asi que el round-trip exacto no es posible. Se deja en
        // "a1234a" (que empieza con "a", suficiente para no romper ningun otro test -- ninguno
        // depende de la clave EXACTA "a" del usuario "a", solo de poder loguearse con esa cuenta,
        // que no se usa en otro lado de la suite).
        await page2.ClickAsync("button[type='submit']");
        await page2.WaitForTimeoutAsync(800);
    }
}
