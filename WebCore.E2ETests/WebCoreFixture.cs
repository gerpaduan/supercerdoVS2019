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
}

[CollectionDefinition("WebCore browser")]
public sealed class WebCoreCollection : ICollectionFixture<WebCoreFixture>
{
}
