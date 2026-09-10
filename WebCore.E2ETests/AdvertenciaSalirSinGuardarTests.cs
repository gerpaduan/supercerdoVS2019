using Microsoft.Playwright;

namespace WebCore.E2ETests;

// Item 2 de la segunda ronda de pedidos (2026-09-09/10, ver docs/DECISIONS.md "Batch 10:
// advertencia de salir sin guardar"): edit-page-guard.js (idéntico byte a byte al clásico) ya
// calculaba window.__protegerSalida, pero WebCore nunca tenía el "Mecanismo B" (enforcement real:
// beforeunload + intercept de clicks con SweetAlert) que sí existe en Web clásico
// (_LayoutBase.cshtml:940-1024) -- se portó a WebCore/Views/Shared/_Layout.cshtml. Además se
// descubrió, durante esta migración, que edit-page-guard.js NUNCA se cargaba (ni siquiera con
// <script src>) en 8 de las 9 vistas objetivo (todas menos Stock/Editar.cshtml) -- llamar
// .init() sin el script cargado hacía no-op en silencio. Se agregó el <script src> + .init()
// real en las 9 vistas, con los IDs reales de cada formulario.
[Collection("WebCore browser")]
public sealed class AdvertenciaSalirSinGuardarTests
{
    private readonly WebCoreFixture _fixture;

    public AdvertenciaSalirSinGuardarTests(WebCoreFixture fixture)
    {
        _fixture = fixture;
    }

    private static async Task<bool> ConfirmarYVerificarSweetAlert(IPage page)
    {
        var visible = await page.Locator(".swal2-popup:visible").CountAsync() > 0;
        if (visible)
        {
            await page.ClickAsync(".swal2-confirm");
            await page.WaitForTimeoutAsync(500);
        }
        return visible;
    }

    [Fact]
    public async Task ProductosAddOrEdit_TipearEnElFormulario_ActivaProtegerSalida()
    {
        var page = await _fixture.NewAuthenticatedPageAsync();

        await page.GotoAsync($"{WebCoreFixture.BaseUrl}/Productos", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        var href = await page.EvaluateAsync<string?>("() => { const a = document.querySelector('a[href*=\"/Productos/AddOrEdit/\"]'); return a ? a.getAttribute('href') : null; }");
        Assert.NotNull(href);

        await page.GotoAsync($"{WebCoreFixture.BaseUrl}{href}", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        await page.WaitForTimeoutAsync(500);

        Assert.False(await page.EvaluateAsync<bool>("() => window.__protegerSalida === true"), "no deberia estar sucio antes de tocar nada");

        if (await page.Locator("#btnHabilitarEdicionProducto").IsVisibleAsync())
        {
            await page.ClickAsync("#btnHabilitarEdicionProducto");
            await page.WaitForTimeoutAsync(300);
        }

        var valorActual = await page.InputValueAsync("#CorteDesc");
        await page.FillAsync("#CorteDesc", valorActual + " (editado)");
        await page.WaitForTimeoutAsync(1000); // polling de edit-page-guard.js cada 700ms

        Assert.True(await page.EvaluateAsync<bool>("() => window.__protegerSalida === true"), "EditPageGuard deberia haber marcado el formulario como sucio");
    }

    [Fact]
    public async Task MovimientosEditar_ClickSidebarConDatosSucios_MuestraSweetAlertYNoNavegaHastaConfirmar()
    {
        var page = await _fixture.NewAuthenticatedPageAsync();

        await page.GotoAsync($"{WebCoreFixture.BaseUrl}/Movimientos/Editar", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        // #txtCodigoProducto es solo un buscador (sin atributo name, no entra en $form.serialize()) --
        // #Observaciones es un campo real del form, mismo criterio usado en el resto de esta clase.
        await page.FillAsync("#Observaciones", "prueba de advertencia de salida");
        await page.WaitForTimeoutAsync(1000);
        Assert.True(await page.EvaluateAsync<bool>("() => window.__protegerSalida === true"));

        var linkSidebar = page.Locator(".sidebar a[href]").First;
        await linkSidebar.ClickAsync();
        await page.WaitForTimeoutAsync(500);

        Assert.True(await page.Locator(".swal2-popup:visible").CountAsync() > 0, "deberia aparecer el SweetAlert de confirmacion");
        Assert.Contains("/Movimientos/Editar", page.Url); // todavia no navego

        var confirmo = await ConfirmarYVerificarSweetAlert(page);
        Assert.True(confirmo);
        Assert.DoesNotContain("/Movimientos/Editar", page.Url); // ahora si navego
    }

    [Fact]
    public async Task MovimientosEditar_ClickSidebarSinTocarNada_NavegaDirectoSinAlerta()
    {
        var page = await _fixture.NewAuthenticatedPageAsync();

        await page.GotoAsync($"{WebCoreFixture.BaseUrl}/Movimientos/Editar", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        await page.WaitForTimeoutAsync(300);

        var linkSidebar = page.Locator(".sidebar a[href]").First;
        await linkSidebar.ClickAsync();
        await page.WaitForTimeoutAsync(500);

        Assert.Equal(0, await page.Locator(".swal2-popup:visible").CountAsync());
        Assert.DoesNotContain("/Movimientos/Editar", page.Url);
    }

    [Fact]
    public async Task ElaboradosCarga_BotonCancelarConDatosSucios_MuestraSweetAlert()
    {
        var page = await _fixture.NewAuthenticatedPageAsync();

        await page.GotoAsync($"{WebCoreFixture.BaseUrl}/Elaborados/Carga", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        await page.FillAsync("#Observaciones", "prueba de advertencia de salida");
        await page.WaitForTimeoutAsync(1000);
        Assert.True(await page.EvaluateAsync<bool>("() => window.__protegerSalida === true"));

        var btnCancelar = page.Locator("a.btn-outline-secondary", new PageLocatorOptions { HasTextString = "Cancelar" });
        await btnCancelar.ClickAsync();
        await page.WaitForTimeoutAsync(500);

        Assert.True(await page.Locator(".swal2-popup:visible").CountAsync() > 0, "el boton Cancelar ya no tiene data-ignore-exit -- debe advertir igual que el resto");
        Assert.Contains("/Elaborados/Carga", page.Url);

        var confirmo = await ConfirmarYVerificarSweetAlert(page);
        Assert.True(confirmo);
        Assert.Contains("/Elaborados", page.Url);
        Assert.DoesNotContain("/Carga", page.Url);
    }

    [Fact]
    public async Task PersonasEditar_ClickConDatosSucios_MuestraSweetAlertPeseAlListenerLocalDeSpinner()
    {
        var page = await _fixture.NewAuthenticatedPageAsync();

        // Persona real ya usada como target de diagnostico durante este mismo batch (id=9007,
        // solo lectura/no destructivo).
        await page.GotoAsync($"{WebCoreFixture.BaseUrl}/Personas/Editar/9007", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        await page.WaitForTimeoutAsync(500);

        Assert.True(await page.EvaluateAsync<bool>("() => !!window.EditPageGuard"), "edit-page-guard.js deberia estar cargado en Personas/Editar");

        var btnHabilitar = page.Locator("#btnHabilitarEdicionPersona");
        if (await btnHabilitar.CountAsync() > 0 && await btnHabilitar.IsVisibleAsync())
        {
            await btnHabilitar.ClickAsync();
            await page.WaitForTimeoutAsync(300);
        }

        var valorOriginal = await page.InputValueAsync("#Telefono");
        await page.FillAsync("#Telefono", valorOriginal + "9");
        await page.WaitForTimeoutAsync(1000);
        Assert.True(await page.EvaluateAsync<bool>("() => window.__protegerSalida === true"));

        // Personas/Editar.cshtml tiene su propio listener de click (spinner "Cargando
        // solicitud...") sobre los links de la pagina -- sin stopImmediatePropagation() en el
        // Mecanismo B, ese listener navegaria de largo antes de que el SweetAlert confirme
        // (ver docs/DECISIONS.md, desviacion documentada del port literal).
        var linkSidebar = page.Locator(".sidebar a[href]").First;
        await linkSidebar.ClickAsync();
        await page.WaitForTimeoutAsync(500);

        Assert.True(await page.Locator(".swal2-popup:visible").CountAsync() > 0);
        Assert.Contains("/Personas/Editar", page.Url);

        var confirmo = await ConfirmarYVerificarSweetAlert(page);
        Assert.True(confirmo);
        Assert.DoesNotContain("/Personas/Editar", page.Url);
    }
}
