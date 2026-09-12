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
        await AsegurarCajaAbiertaAsync();
    }

    // Bug real encontrado corriendo la suite completa tras el Batch 7 (2026-09-11, rediseño del
    // gate de "caja abierta" en POS -- ver docs/DECISIONS.md): "ger" (admin) no tiene una caja
    // REAL abierta en su sucursal en la base de dev -- antes, el simple permiso de admin saltaba
    // el modal de apertura en silencio (ViewBag.CajaAbierta = cajaAbierta || puedeOperarSinCaja),
    // y decenas de tests de esta suite (sin relacion con el gate de caja en si) asumian poder
    // entrar a POS directo tras loguearse. Con el rediseño (pedido explicito del usuario), ese
    // bypass ahora requiere una confirmacion explicita por sesion/pestaña -- rompe esa asuncion
    // para CUALQUIER test que loguee una sesion fresca (cada NewAuthenticatedPageAsync es un
    // browser context nuevo, sin la Session del bypass). Fix: se abre una caja REAL una sola vez
    // por corrida de la suite (aca, en el fixture, no test por test) -- asi el resto de los tests
    // sigue entrando a POS igual que siempre, con caja real, sin depender del checkbox nuevo. Los
    // tests que prueban el gate de caja en si (Batch 7) manejan su propio escenario sin caja aparte.
    private async Task AsegurarCajaAbiertaAsync()
    {
        var page = await NewAuthenticatedPageAsync();
        try
        {
            var fechaHora = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
            await page.EvaluateAsync(@"async (fechaHora) => {
                const params = new URLSearchParams({ cajaInicio: '10000', fechaHora: fechaHora });
                await fetch('/Cajas/AbrirCaja', { method: 'POST', body: params });
            }", fechaHora);
        }
        finally
        {
            await page.CloseAsync();
        }
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
