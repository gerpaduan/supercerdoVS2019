using Microsoft.Playwright;

namespace WebCore.E2ETests;

// Fixture compartida (xunit ICollectionFixture) para los tests de browser real de WebCore.
// Requiere que WebCore ya este corriendo por separado (dotnet run / el .exe publicado) en
// BaseUrl -- estos tests NO levantan el servidor, solo lo ejercitan como un navegador real lo
// haria. Corren estos tests con: dotnet test WebCore.E2ETests (con WebCore ya arriba).
public sealed class WebCoreFixture : IAsyncLifetime
{
    public const string BaseUrl = "http://localhost:5270";

    public IPlaywright Playwright { get; private set; } = null!;
    public IBrowser Browser { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        Playwright = await Microsoft.Playwright.Playwright.CreateAsync();
        Browser = await Playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = true });
    }

    public async Task DisposeAsync()
    {
        await Browser.CloseAsync();
        Playwright.Dispose();
    }

    // Login real (2026-09-06, ver docs/DECISIONS.md "Login/Sesion real para WebCore") -- desde que
    // el FallbackPolicy global exige autenticacion, todo test que navegue a una pagina protegida
    // necesita loguearse primero o cae en un redirect a /Login. Credenciales de dev ya establecidas
    // en esta migracion (usuario "ger", clave reseteada a "a" via el propio flujo de
    // Usuarios/Guardar, ver docs/DECISIONS.md) -- no son un secreto real, es la base local de
    // desarrollo. Reemplaza los usos directos de Browser.NewPageAsync() en los tests existentes.
    public async Task<IPage> NewAuthenticatedPageAsync(BrowserNewPageOptions? options = null)
    {
        var page = await Browser.NewPageAsync(options ?? new BrowserNewPageOptions());
        await page.GotoAsync($"{BaseUrl}/Login");
        await page.FillAsync("input[name='Usuario']", "ger");
        await page.FillAsync("input[name='Clave']", "a");
        await page.ClickAsync("button[type='submit']");
        await page.WaitForURLAsync(url => !url.Contains("/Login"));
        return page;
    }
}

[CollectionDefinition("WebCore browser")]
public sealed class WebCoreCollection : ICollectionFixture<WebCoreFixture>
{
}
