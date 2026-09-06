using Microsoft.Playwright;

namespace WebCore.E2ETests;

// Smoke test del sidebar global (ver docs/10-migracion-aspnet-core/README.md, "Sidebar de
// navegacion global") -- confirma que el layout con sidebar renderiza de verdad en un navegador
// real, no solo que el HTML servido lo contiene (lo que se verificaba con curl hasta ahora).
[Collection("WebCore browser")]
public sealed class SidebarSmokeTests
{
    private readonly WebCoreFixture _fixture;

    public SidebarSmokeTests(WebCoreFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Personas_RenderizaConSidebar()
    {
        var page = await _fixture.NewAuthenticatedPageAsync();
        var response = await page.GotoAsync($"{WebCoreFixture.BaseUrl}/Personas", new PageGotoOptions
        {
            WaitUntil = WaitUntilState.DOMContentLoaded,
            Timeout = 20000
        });

        Assert.NotNull(response);
        Assert.Equal(200, response!.Status);
        Assert.Contains("Personas", await page.TitleAsync());
        // .sidebar (2026-09-05, ver docs/DECISIONS.md "UI de WebCore igual al clasico"): antes
        // era ".wc-sidebar", renombrado al portar la identidad visual real del clasico.
        Assert.Equal(1, await page.Locator(".sidebar").CountAsync());

        await page.CloseAsync();
    }
}
