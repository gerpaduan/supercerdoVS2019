using Microsoft.Playwright;

namespace WebCore.E2ETests;

// Item 8 de la cuarta ronda de pedidos, 2026-09-10, ver docs/DECISIONS.md "Batch 6: Dispositivos
// Seguros -- numero de serie + auto-deteccion + autoservicio". Dos bugs/gaps reales:
// (1) print-agent.js (window.CarniSysPrintAgent) nunca se habia portado a WebCore, asi que el
//     campo NumeroSerie de /DispositivosSeguros nunca se autocompletaba, en silencio.
// (2) Un usuario NO-admin no tenia ninguna forma de ver el numero de serie de su propia PC: el
//     link del menu "Configuracion" es admin-only, y el campo autocompletado en
//     /DispositivosSeguros esta detras de @if (Model.PuedeAdministrar). Nuevo modal de
//     autoservicio (_ModalMiDispositivo.cshtml), disparado desde el dropdown de usuario,
//     disponible para cualquier usuario logueado.
[Collection("WebCore browser")]
public sealed class DispositivosSegurosAutoservicioTests
{
    private readonly WebCoreFixture _fixture;

    public DispositivosSegurosAutoservicioTests(WebCoreFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task PrintAgentJs_QuedaDefinidoGlobalmente()
    {
        var page = await _fixture.NewAuthenticatedPageAsync();
        await page.GotoAsync($"{WebCoreFixture.BaseUrl}/Home/Index", new PageGotoOptions { WaitUntil = WaitUntilState.Load, Timeout = 20000 });

        Assert.True(await page.EvaluateAsync<bool>("() => !!window.CarniSysPrintAgent && typeof window.CarniSysPrintAgent.getDeviceId === 'function'"));

        await page.CloseAsync();
    }

    [Fact]
    public async Task DispositivosSeguros_CampoNumeroSerieSeAutocompletaParaAdmin()
    {
        // "ger" es admin -- ve el formulario de alta con el campo NumeroSerie.
        var page = await _fixture.NewAuthenticatedPageAsync();
        await page.GotoAsync($"{WebCoreFixture.BaseUrl}/DispositivosSeguros", new PageGotoOptions { WaitUntil = WaitUntilState.Load, Timeout = 20000 });

        await page.WaitForFunctionAsync("() => document.getElementById('NumeroSerie') && document.getElementById('NumeroSerie').value.length > 0", new PageWaitForFunctionOptions { Timeout = 5000 });

        var valor = await page.InputValueAsync("#NumeroSerie");
        Assert.False(string.IsNullOrWhiteSpace(valor));

        await page.CloseAsync();
    }

    [Fact]
    public async Task DropdownDeUsuario_LinkDispositivosSeguros_EsAdminOnly()
    {
        // Regresion: el link "Dispositivos seguros" del menu "Configuracion" sigue siendo
        // admin-only -- el autoservicio nuevo NO amplia ese gate.
        var page = await LoginNoAdminAsync();
        await page.GotoAsync($"{WebCoreFixture.BaseUrl}/Home/Index", new PageGotoOptions { WaitUntil = WaitUntilState.Load, Timeout = 20000 });

        Assert.Equal(0, await page.Locator("a[href*='/DispositivosSeguros']").CountAsync());

        await page.CloseAsync();
    }

    [Fact]
    public async Task Autoservicio_VisibleParaUsuarioNoAdmin_MuestraSuPropioNumeroDeSerie()
    {
        var page = await LoginNoAdminAsync();
        await page.GotoAsync($"{WebCoreFixture.BaseUrl}/Home/Index", new PageGotoOptions { WaitUntil = WaitUntilState.Load, Timeout = 20000 });

        // Disponible para cualquier usuario logueado, a diferencia del link de arriba.
        await page.ClickAsync("#userDropdown");
        await page.WaitForTimeoutAsync(200);
        Assert.Equal(1, await page.Locator("a[data-bs-target='#modalMiDispositivo']").CountAsync());

        await page.ClickAsync("a[data-bs-target='#modalMiDispositivo']");
        await page.Locator("#modalMiDispositivo.show").WaitForAsync(new LocatorWaitForOptions { Timeout = 5000 });

        await page.WaitForFunctionAsync("() => document.getElementById('miDispositivoNumeroSerie').value.length > 0 || document.getElementById('miDispositivoEstado').textContent.length > 0", new PageWaitForFunctionOptions { Timeout = 5000 });

        var valor = await page.InputValueAsync("#miDispositivoNumeroSerie");
        var estado = await page.InnerTextAsync("#miDispositivoEstado");
        Assert.True(!string.IsNullOrWhiteSpace(valor) || !string.IsNullOrWhiteSpace(estado));

        await page.CloseAsync();
    }

    private async Task<IPage> LoginNoAdminAsync()
    {
        var page = await _fixture.Browser.NewPageAsync();
        await page.GotoAsync($"{WebCoreFixture.BaseUrl}/Login");
        await page.FillAsync("input[name='Usuario']", "cajero");
        await page.FillAsync("input[name='Clave']", "a");
        await page.ClickAsync("button[type='submit']");
        await page.WaitForURLAsync(u => !u.Contains("/Login"));
        return page;
    }

    // Batch 11 de la quinta ronda de pedidos, 2026-09-10, ver docs/DECISIONS.md "Batch 11: ajustes
    // chicos al autoservicio de número de serie". El texto largo "Ver número de serie de este
    // dispositivo" quedaba confuso en el dropdown -- se acorta, sin cambiar el destino del link.
    [Fact]
    public async Task DropdownDeUsuario_TextoDeNumeroDeSerieEsCorto()
    {
        var page = await _fixture.NewAuthenticatedPageAsync();
        await page.GotoAsync($"{WebCoreFixture.BaseUrl}/Home/Index", new PageGotoOptions { WaitUntil = WaitUntilState.Load, Timeout = 20000 });
        await page.ClickAsync("#userDropdown");
        await page.WaitForTimeoutAsync(200);

        var link = page.Locator("a[data-bs-target='#modalMiDispositivo']");
        Assert.Equal(1, await link.CountAsync());
        var texto = (await link.InnerTextAsync()).Trim();
        Assert.True(texto.Length <= 20, $"El texto del dropdown sigue siendo largo ({texto.Length} caracteres): '{texto}'");
        Assert.DoesNotContain("dispositivo", texto, System.StringComparison.OrdinalIgnoreCase);

        // El destino no cambia -- click sigue abriendo el mismo modal.
        await link.ClickAsync();
        await page.WaitForTimeoutAsync(500);
        Assert.Equal(1, await page.Locator("#modalMiDispositivo.show").CountAsync());

        await page.CloseAsync();
    }

    // Batch 11: el parrafo largo sobre que es el numero de serie se movio a un modal on-demand
    // (boton "i"), en vez de quedar siempre visible ocupando espacio permanente.
    [Fact]
    public async Task DispositivosSeguros_BotonInfo_MuestraElTextoLargoEnUnModal()
    {
        var page = await _fixture.NewAuthenticatedPageAsync();
        await page.GotoAsync($"{WebCoreFixture.BaseUrl}/DispositivosSeguros", new PageGotoOptions { WaitUntil = WaitUntilState.Load, Timeout = 20000 });
        await page.WaitForTimeoutAsync(500);

        // El parrafo largo ya no esta siempre visible en la pagina.
        var textoPagina = await page.Locator(".dispositivos-seguros-page").InnerTextAsync();
        Assert.DoesNotContain("No es un número que se pueda buscar en la PC a mano", textoPagina);

        var btnInfo = page.Locator("[data-bs-target='#modalInfoNumeroSerie']");
        Assert.Equal(1, await btnInfo.CountAsync());
        await btnInfo.ClickAsync();
        await page.WaitForTimeoutAsync(500);

        Assert.Equal(1, await page.Locator("#modalInfoNumeroSerie.show").CountAsync());
        var textoModal = await page.Locator("#modalInfoNumeroSerie .modal-body").InnerTextAsync();
        Assert.Contains("No es un número que se pueda buscar en la PC a mano", textoModal);
        Assert.Contains("agente de impresión CarniSys", textoModal);

        await page.CloseAsync();
    }
}
