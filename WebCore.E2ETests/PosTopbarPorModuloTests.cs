using Microsoft.Playwright;

namespace WebCore.E2ETests;

// Pedido explicito del usuario (2026-09-07, ver docs/DECISIONS.md): "copiar el topbar tal como
// estaba en web clasico para pos ventas y expendio" -- Ventas/POS y PuntosExpendio/POS
// compartian el mismo topbar azul con el texto fijo "CarniSys" (simplificacion de cuando este
// layout no tenia sesion real todavia). Port literal del clasico: Ventas/POS usa el gradiente
// azul del sidebar + el nombre real de la empresa (Sesion.UsuarioActual.Empresa.NombreFantasia);
// PuntosExpendio/POS usa un gradiente ambar/rojizo + el sector elegido, via
// ViewBag.PosBrandLabel/PosBrandName seteados en PuntosExpendioController.POS.
[Collection("WebCore browser")]
public sealed class PosTopbarPorModuloTests
{
    private readonly WebCoreFixture _fixture;

    public PosTopbarPorModuloTests(WebCoreFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task VentasPOS_TopbarAzul_ConNombreRealDeEmpresa()
    {
        var page = await _fixture.NewAuthenticatedPageAsync();
        await page.GotoAsync($"{WebCoreFixture.BaseUrl}/Ventas/POS", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        await page.WaitForTimeoutAsync(500);

        var nav = page.Locator("nav.card-header");
        Assert.True(await nav.EvaluateAsync<bool>("el => el.classList.contains('pos-header-venta')"));
        Assert.False(await nav.EvaluateAsync<bool>("el => el.classList.contains('pos-header-expendio')"));

        var label = (await page.Locator(".pos-brand-label").InnerTextAsync()).Trim();
        var empresa = (await page.Locator(".pos-empresa").InnerTextAsync()).Trim();
        Assert.Equal("PUNTO DE VENTA", label.ToUpperInvariant());
        Assert.NotEqual("CarniSys", empresa);
        Assert.NotEmpty(empresa);

        await page.CloseAsync();
    }

    [Fact]
    public async Task ExpendiosPOS_TopbarAmbar_ConSectorElegido()
    {
        var page = await _fixture.NewAuthenticatedPageAsync();
        await page.GotoAsync($"{WebCoreFixture.BaseUrl}/PuntosExpendio/POS", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        await page.WaitForTimeoutAsync(500);

        var modalSectores = page.Locator("#modalSectoresPuntoExpendio.show");
        string sectorElegido;
        if (await modalSectores.CountAsync() > 0)
        {
            var primerSector = page.Locator(".js-sector-item").First;
            sectorElegido = (await primerSector.InnerTextAsync()).Trim();
            await primerSector.ClickAsync();
            await page.WaitForTimeoutAsync(300);
        }
        else
        {
            sectorElegido = "Sin seleccionar";
        }

        var nav = page.Locator("nav.card-header");
        Assert.True(await nav.EvaluateAsync<bool>("el => el.classList.contains('pos-header-expendio')"));
        Assert.False(await nav.EvaluateAsync<bool>("el => el.classList.contains('pos-header-venta')"));

        var label = (await page.Locator(".pos-brand-label").InnerTextAsync()).Trim();
        var marca = (await page.Locator(".pos-empresa").InnerTextAsync()).Trim();
        Assert.Equal("PUNTO DE EXPENDIO", label.ToUpperInvariant());
        Assert.Equal(sectorElegido, marca);

        await page.CloseAsync();
    }

    // Item 6 de la segunda ronda de pedidos (2026-09-10, ver docs/DECISIONS.md): menu de usuario
    // en el topbar de POS -- nombre + opcion de cerrar sesion (antes no existia ninguna, ver
    // cabecera anterior de _LayoutPOS.cshtml, que documentaba la ausencia como "diferencia
    // deliberada"). El bloqueo de cambio de sucursal es puramente visual (dropdown-item disabled),
    // igual que el clasico.
    [Fact]
    public async Task VentasPOS_MenuDeUsuario_MuestraNombreYOpciones()
    {
        var page = await _fixture.NewAuthenticatedPageAsync();
        await page.GotoAsync($"{WebCoreFixture.BaseUrl}/Ventas/POS", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        await page.WaitForTimeoutAsync(500);

        var dropdownToggle = page.Locator("#posUserDropdown");
        Assert.Equal(1, await dropdownToggle.CountAsync());

        await dropdownToggle.ClickAsync();
        await page.WaitForTimeoutAsync(300);

        var menu = page.Locator(".dropdown-menu.show");
        Assert.Equal(1, await menu.CountAsync());
        var texto = await menu.InnerTextAsync();
        Assert.Contains("Cambiar clave", texto);
        Assert.Contains("Cerrar sesión", texto);

        // El item de sucursal existe pero esta deshabilitado (bloqueo puramente visual, sin
        // validacion server-side -- ninguno de los 2 sistemas la tiene hoy en CambiarSucursal).
        var itemSucursal = menu.Locator(".dropdown-item.disabled");
        Assert.Equal(1, await itemSucursal.CountAsync());

        await page.CloseAsync();
    }

    // Con usuario de produccion, el operador resuelto debe verse en el menu ("Operador: X") --
    // ViewBag.OperadorPOSNombre ya lo seteaba el controller, solo faltaba leerlo en la vista.
    // Reusa el mismo flujo ya probado y estable de OperadorPOSPersisteEntreVentasTests.cs (login
    // produccion -> autorizar con ger/a -> reload) en vez de un test aparte: un browser context
    // nuevo con el mismo login puede -segun timing- encontrar un operador ya resuelto de una
    // corrida previa en la misma sesion de servidor, dejando el caso "recien autorizado" sin
    // reproducir de forma confiable.
    [Fact]
    public async Task VentasPOS_UsuarioProduccion_MenuMuestraOperadorResueltoTrasAutorizar()
    {
        var page = await _fixture.Browser.NewPageAsync();
        await page.GotoAsync($"{WebCoreFixture.BaseUrl}/Login", new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });
        await page.FillAsync("input[name='Usuario']", "produccion");
        await page.FillAsync("input[name='Clave']", "a");
        await page.ClickAsync("button[type='submit']");
        await page.WaitForURLAsync(url => !url.Contains("/Login"));

        await page.GotoAsync($"{WebCoreFixture.BaseUrl}/Ventas/POS", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        await page.WaitForTimeoutAsync(600);

        if (await page.Locator("#modalSeleccionUsuario.show").CountAsync() == 0)
        {
            // Ya habia un operador resuelto en sesion de una corrida previa -- confirmar que el
            // dropdown lo refleja igual, sin necesidad de re-autorizar. En viewport de escritorio
            // (d-lg-inline), el nombre+operador se muestra en el propio boton toggle -- el
            // <h6 class="dropdown-header d-lg-none"> de adentro del menu es SOLO para mobile y
            // queda oculto (display:none) en este viewport, innerText no lo incluye.
            Assert.Contains("Operador:", await page.Locator("#posUserDropdown").InnerTextAsync());
            return;
        }

        await page.FillAsync("#txtSeleccionUsuario", "ger");
        await page.FillAsync("#passSeleccionUsuario", "a");
        await page.ClickAsync("#btnConfirmarSeleccionUsuario");
        await page.WaitForTimeoutAsync(2500);

        Assert.Equal(0, await page.Locator("#modalSeleccionUsuario.show").CountAsync());
        Assert.Contains("Operador:", await page.Locator("#posUserDropdown").InnerTextAsync());
    }
}
