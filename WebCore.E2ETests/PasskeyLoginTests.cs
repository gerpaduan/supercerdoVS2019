using Microsoft.Playwright;

namespace WebCore.E2ETests;

// Login por huella (passkeys WebAuthn, 2026-09-21, ver docs/DECISIONS.md "Login por huella
// (passkeys)"). Usa un AUTENTICADOR VIRTUAL de Chromium (CDP WebAuthn.addVirtualAuthenticator): hace
// de "huella" sin sensor real, con verificacion de usuario simulada. Requiere WebCore corriendo en
// BaseUrl con Passkeys:Enabled=true y Passkeys:Origins incluyendo esa URL (ver App.config.example) y
// la tabla usuariopasskeys ya migrada. Es autocontenido (sin WebCoreFixture: la fixture compartida
// abre una caja real en la base de dev en cada corrida y esto no lo necesita). Deja el usuario de
// prueba sin huellas registradas al terminar. Usuario "ger" (admin de dev, ver WebCoreFixture).
public sealed class PasskeyLoginTests : IAsyncLifetime
{
    private IPlaywright _playwright = null!;
    private IBrowser _browser = null!;

    public async Task InitializeAsync()
    {
        _playwright = await Playwright.CreateAsync();
        _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = true });
    }

    public async Task DisposeAsync()
    {
        await _browser.CloseAsync();
        _playwright.Dispose();
    }

    // Habilita WebAuthn en la pagina y agrega un autenticador "interno" (como Windows Hello / Touch ID)
    // con clave residente (credencial descubrible) y verificacion de usuario resuelta a "OK".
    private static async Task AgregarAutenticadorVirtualAsync(IBrowserContext context, IPage page)
    {
        var cdp = await context.NewCDPSessionAsync(page);
        await cdp.SendAsync("WebAuthn.enable");
        await cdp.SendAsync("WebAuthn.addVirtualAuthenticator", new Dictionary<string, object>
        {
            ["options"] = new Dictionary<string, object>
            {
                ["protocol"] = "ctap2",
                ["transport"] = "internal",
                ["hasResidentKey"] = true,
                ["hasUserVerification"] = true,
                ["isUserVerified"] = true,
                ["automaticPresenceSimulation"] = true
            }
        });
    }

    // Usuario/clave de la prueba: E2E_USER / E2E_PASSWORD (la base local migrada ya no tiene "ger");
    // por defecto los de la suite. Tiene que ser un usuario admin activo (los no-admin quedan
    // sujetos a horario laboral y dispositivo seguro, ver LoginController.EvaluarReglasPostCredencial).
    private static readonly string Usuario = Environment.GetEnvironmentVariable("E2E_USER") ?? "ger";
    private static readonly string Clave = Environment.GetEnvironmentVariable("E2E_PASSWORD") ?? "a";

    private static async Task LoginConClaveAsync(IPage page)
    {
        await page.GotoAsync($"{WebCoreFixture.BaseUrl}/Login", new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });
        await page.FillAsync("input[name='Usuario']", Usuario);
        await page.FillAsync("input[name='Clave']", Clave);
        await page.ClickAsync("button.login-submit");
        await page.WaitForURLAsync(url => !url.Contains("/Login"), new PageWaitForURLOptions { Timeout = 15000 });
    }

    private static async Task AbrirModalMiHuellaAsync(IPage page)
    {
        await page.EvaluateAsync("new bootstrap.Modal(document.getElementById('modalMiHuella')).show()");
        await page.WaitForSelectorAsync("#modalMiHuella.show", new PageWaitForSelectorOptions { State = WaitForSelectorState.Visible });
    }

    [Fact]
    public async Task Huella_RegistrarIngresarQuitar_Completo()
    {
        var context = await _browser.NewContextAsync();
        var page = await context.NewPageAsync();
        await AgregarAutenticadorVirtualAsync(context, page);

        // --- Alta desde "Mi huella" (sesion iniciada con clave) ---
        await LoginConClaveAsync(page);
        Assert.True(await page.Locator("a.dropdown-item:has-text('Mi huella')").CountAsync() >= 1, "Falta el item 'Mi huella' en el menu de usuario.");

        await AbrirModalMiHuellaAsync(page);
        await page.FillAsync("#miHuellaNombre", "E2E virtual");
        await page.ClickAsync("#btnMiHuellaAgregar");
        await page.WaitForFunctionAsync("document.getElementById('miHuellaEstado').textContent.includes('Huella registrada')", null, new PageWaitForFunctionOptions { Timeout = 15000 });
        // La lista se recarga con un fetch aparte tras el mensaje de exito: aserto con reintento.
        await Assertions.Expect(page.Locator("#miHuellaLista")).ToContainTextAsync("E2E virtual");

        // --- Login con huella: el boton es la PRIMERA opcion (arriba de usuario/clave) ---
        await page.GotoAsync($"{WebCoreFixture.BaseUrl}/Login/Logout", new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });
        await page.GotoAsync($"{WebCoreFixture.BaseUrl}/Login", new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });

        var boton = page.Locator("#btnPasskeyLogin");
        await Assertions.Expect(boton).ToBeVisibleAsync();
        var cajaBoton = await boton.BoundingBoxAsync();
        var cajaUsuario = await page.Locator("input[name='Usuario']").BoundingBoxAsync();
        Assert.NotNull(cajaBoton);
        Assert.NotNull(cajaUsuario);
        Assert.True(cajaBoton!.Y < cajaUsuario!.Y, "El boton de huella debe aparecer arriba del campo Usuario.");

        await boton.ClickAsync();
        await page.WaitForURLAsync(url => !url.Contains("/Login"), new PageWaitForURLOptions { Timeout = 15000 });
        Assert.Contains("/Home", page.Url);

        // --- Baja: quitar la huella y comprobar que ya no sirve ---
        await AbrirModalMiHuellaAsync(page);
        page.Dialog += async (_, dialogo) => await dialogo.AcceptAsync();
        await page.ClickAsync("#miHuellaLista tr:has-text('E2E virtual') button:has-text('Quitar')");
        await Assertions.Expect(page.Locator("#miHuellaLista")).Not.ToContainTextAsync("E2E virtual");

        await page.GotoAsync($"{WebCoreFixture.BaseUrl}/Login/Logout", new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });
        await page.GotoAsync($"{WebCoreFixture.BaseUrl}/Login", new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });
        await page.ClickAsync("#btnPasskeyLogin");
        var error = page.Locator("#passkeyLoginEstado");
        await Assertions.Expect(error).ToBeVisibleAsync();
        await Assertions.Expect(error).ToContainTextAsync("No fue posible iniciar sesión con la huella");
        Assert.Contains("/Login", page.Url);

        await context.CloseAsync();
    }
}
