using Microsoft.Playwright;

namespace WebCore.E2ETests;

// Item 1 de los 12 pendientes (2026-09-09, ver docs/DECISIONS.md "Batch E: recuperación de
// contraseña"): "en Login, hacer la recuperación de contraseña". Portado con mejoras de
// seguridad deliberadas sobre el clásico (confirmadas con el usuario, "sí, mejorarlo"): rate
// limiting propio por IP (PasswordResetRateLimiter) y clave mínima de 6 caracteres (el clásico
// aceptaba 1). El flujo feliz completo (token real -> cambio de clave -> login con la clave
// nueva -> token de un solo uso) se verificó manualmente esta sesión insertando un token de
// prueba directo en la base de datos (usuario "a", id=22) -- no hay un test AUTOMATIZADO
// permanente de ese tramo especifico porque requeriria acceso directo a Postgres desde el
// proyecto de tests (no existe ese helper hoy, ver PreciosSeedHelper.cs: todos los tests actuales
// pasan solo por HTTP/UI) o entrega real de email -- deuda declarada, no un gap silencioso. Los
// tests de abajo cubren todo lo que SI se puede probar sin esa pieza: las 2 pantallas, el mensaje
// generico (no filtra si la cuenta existe), el rechazo de token invalido/ausente, y el rate
// limiting real (que no depende de conocer un token valido).
[Collection("WebCore browser")]
public sealed class PasswordResetTests
{
    private readonly WebCoreFixture _fixture;

    public PasswordResetTests(WebCoreFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task LoginIndex_TieneLinkAOlvidasteTuContrasena()
    {
        var page = await _fixture.Browser.NewPageAsync();
        await page.GotoAsync($"{WebCoreFixture.BaseUrl}/Login", new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });

        var link = page.Locator("a.login-forgot");
        Assert.Equal(1, await link.CountAsync());

        await link.ClickAsync();
        await page.WaitForURLAsync(url => url.Contains("/Login/ForgotPassword"));
        Assert.Equal(1, await page.Locator("#UsuarioOEmail").CountAsync());
    }

    // No compara contra un resultado fijo (el rate limiting real de otro test de esta misma clase,
    // compartiendo la misma IP de origen, puede dejar la cuota consumida antes de que este test
    // corra -- xUnit no garantiza orden entre metodos) -- en cambio, dispara AMBOS identificadores
    // seguidos y compara que el comportamiento observable sea IDENTICO entre "cuenta real" y
    // "cuenta inexistente", que es la propiedad de seguridad real que hay que proteger (no debe
    // haber ninguna forma de distinguir una de otra desde afuera), sea que ese comportamiento sea
    // "genero el mensaje generico" o "quedo bloqueado por rate limit" -- lo que importa es que los
    // dos casos se vean exactamente igual.
    [Fact]
    public async Task ForgotPassword_NoDistingueEntreCuentaRealYCuentaInexistente()
    {
        var page = await _fixture.Browser.NewPageAsync();

        await page.GotoAsync($"{WebCoreFixture.BaseUrl}/Login/ForgotPassword", new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });
        await page.FillAsync("#UsuarioOEmail", "usuario_que_no_existe_e2e");
        await page.ClickAsync("button[type='submit']");
        await page.WaitForTimeoutAsync(600);
        var urlNoExiste = new Uri(page.Url).AbsolutePath;
        var textoNoExiste = await page.InnerTextAsync("body");

        await page.GotoAsync($"{WebCoreFixture.BaseUrl}/Login/ForgotPassword", new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });
        await page.FillAsync("#UsuarioOEmail", "ger");
        await page.ClickAsync("button[type='submit']");
        await page.WaitForTimeoutAsync(600);
        var urlExiste = new Uri(page.Url).AbsolutePath;
        var textoExiste = await page.InnerTextAsync("body");

        Assert.Equal(urlNoExiste, urlExiste);
        Assert.Equal(NormalizarParaComparar(textoNoExiste), NormalizarParaComparar(textoExiste));
    }

    private static string NormalizarParaComparar(string texto)
    {
        // TempData/timers pueden variar segundo a segundo si el rate limiter esta activo (el
        // mensaje incluye "esperá N minuto(s)") -- se recorta a los primeros 80 caracteres, que ya
        // alcanzan para comparar cual de los 2 mensajes posibles se mostro (exito vs bloqueado),
        // sin que un numero de minutos distinto entre ambas llamadas haga fallar la comparacion.
        return texto.Length > 80 ? texto.Substring(0, 80) : texto;
    }

    [Fact]
    public async Task ResetPassword_SinTokenONoValido_NoMuestraElFormulario()
    {
        var page = await _fixture.Browser.NewPageAsync();

        await page.GotoAsync($"{WebCoreFixture.BaseUrl}/Login/ResetPassword", new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });
        Assert.Equal(0, await page.Locator("#NuevaClave").CountAsync());
        Assert.Equal(1, await page.Locator("a:has-text('Solicitar un nuevo enlace')").CountAsync());

        await page.GotoAsync($"{WebCoreFixture.BaseUrl}/Login/ResetPassword?token=token_que_no_existe_en_la_base", new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });
        Assert.Equal(0, await page.Locator("#NuevaClave").CountAsync());
        Assert.Equal(1, await page.Locator("a:has-text('Solicitar un nuevo enlace')").CountAsync());
    }

    // Rate limiting real por IP (PasswordResetRateLimiter, mejora de seguridad nueva sin
    // equivalente en el clasico). No asume un numero exacto de intentos permitidos (el limite es
    // configurable via Security:PasswordResetMaxRequests, default 3) ni parte de un estado limpio
    // (otros tests de esta misma clase ya consumieron cupo de la misma IP en la misma ventana) --
    // solo confirma que, insistiendo lo suficiente, el servidor eventualmente corta con 429.
    [Fact]
    public async Task ForgotPassword_RateLimitingBloqueaTrasVariosIntentos()
    {
        var page = await _fixture.Browser.NewPageAsync();
        bool bloqueado = false;

        for (var i = 0; i < 10 && !bloqueado; i++)
        {
            await page.GotoAsync($"{WebCoreFixture.BaseUrl}/Login/ForgotPassword", new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });
            await page.FillAsync("#UsuarioOEmail", "rate_limit_test_" + i);

            var response = await page.RunAndWaitForResponseAsync(
                async () => await page.ClickAsync("button[type='submit']"),
                r => r.Url.Contains("/Login/ForgotPassword") && r.Request.Method == "POST");

            if (response.Status == 429)
                bloqueado = true;
        }

        Assert.True(bloqueado, "Tras insistir con varios POST a /Login/ForgotPassword desde la misma IP, se esperaba un 429 en algun momento.");
    }
}
