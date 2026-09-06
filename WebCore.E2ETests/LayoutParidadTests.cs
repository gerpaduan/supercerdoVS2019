using Microsoft.Playwright;

namespace WebCore.E2ETests;

// Regresion de 2 bugs reales encontrados el 2026-09-06 comparando WebCore contra el clasico
// (reportados por el usuario: "/Ventas/Index quedo desordenada, el menu de usuario a mitad de la
// vista" y "el teclado del POS en Core se ve mas chico"). Ver docs/DECISIONS.md para el detalle
// completo de la causa raiz de cada uno.
[Collection("WebCore browser")]
public sealed class LayoutParidadTests
{
    private readonly WebCoreFixture _fixture;

    public LayoutParidadTests(WebCoreFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task ContentWrapper_OcupaTodoElAnchoDisponible()
    {
        // Bug real: #content-wrapper (hijo flex de #wrapper) sin flex-grow ocupaba solo el
        // ancho de su contenido -- el topbar/contenido quedaba encogido a la izquierda con una
        // franja gris a la derecha, y el menu de usuario del topbar terminaba a mitad de
        // pantalla en vez de la esquina superior derecha. Fix: #content-wrapper { width: 100%; }
        // (mismas 2 reglas que trae sb-admin-2.css original, nunca copiadas a mano).
        var page = await _fixture.NewAuthenticatedPageAsync(new BrowserNewPageOptions
        {
            ViewportSize = new ViewportSize { Width = 1400, Height = 900 }
        });
        await page.GotoAsync($"{WebCoreFixture.BaseUrl}/Ventas", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });

        var contentWrapperBox = await page.Locator("#content-wrapper").BoundingBoxAsync();
        Assert.NotNull(contentWrapperBox);
        // El sidebar mide 14rem (224px) a este ancho -- #content-wrapper debe ocupar
        // practicamente todo el resto (con margen para redondeos de layout).
        Assert.True(contentWrapperBox!.Width > 1000, $"content-wrapper demasiado angosto: {contentWrapperBox.Width}px (deberia ocupar casi todo el ancho restante despues del sidebar).");

        // El menu de usuario (icono de perfil) tiene que estar pegado al borde derecho real de
        // la ventana, no a mitad de camino.
        var userIconBox = await page.Locator(".fa-user-circle").BoundingBoxAsync();
        Assert.NotNull(userIconBox);
        Assert.True(userIconBox!.X > 1300, $"El icono de usuario quedo lejos del borde derecho (x={userIconBox.X}), señal de que el contenido no ocupa todo el ancho.");

        await page.CloseAsync();
    }

    [Fact]
    public async Task PosCompacto_SeActivaEnPantallasBajas()
    {
        // Bug real: la logica de Web/Views/Ventas/POS.cshtml que activa .pos-compact/.pos-tiny
        // segun la altura util del viewport (pensada para portatiles 1366x768 con zoom de
        // Windows, un caso muy comun) nunca se habia portado a WebCore -- la CSS ya estaba
        // copiada pero quedaba muerta sin este JS, asi que el teclado del POS nunca se
        // compactaba y quedaba desproporcionado en pantallas con poca altura util. Fix:
        // Scripts/app/pos-compact.js (nuevo, port literal de la IIFE del original).
        var page = await _fixture.NewAuthenticatedPageAsync(new BrowserNewPageOptions
        {
            ViewportSize = new ViewportSize { Width = 1366, Height = 768 }
        });
        await page.GotoAsync($"{WebCoreFixture.BaseUrl}/Ventas/POS", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        await page.WaitForTimeoutAsync(300);

        var htmlClasses = await page.EvaluateAsync<string>("document.documentElement.className");
        Assert.Contains("pos-compact", htmlClasses);
        Assert.Contains("pos-tiny", htmlClasses);

        await page.CloseAsync();
    }
}
