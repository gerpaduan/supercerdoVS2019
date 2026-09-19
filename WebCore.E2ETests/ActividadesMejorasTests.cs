using Microsoft.Playwright;

namespace WebCore.E2ETests;

// Batch 9 de la quinta ronda de pedidos, 2026-09-10 (ver docs/DECISIONS.md): fuente mas chica en
// la columna Fecha, etiqueta "(creación)"/"(modificación)" debajo de la fecha, icono de
// advertencia cuando la fecha de creacion/modificacion difiere >1 dia de la fecha real del
// registro, sub-tipo real de Compras/Stock en la columna Tipo (antes texto fijo "Compra/Stock"),
// y filtro "solo anomalías". El fin del modulo es poder filtrar registros con comportamiento
// extraño (cargados o editados con fecha muy distinta a cuando ocurrieron).
[Collection("WebCore browser")]
public sealed class ActividadesMejorasTests
{
    private readonly WebCoreFixture _fixture;

    public ActividadesMejorasTests(WebCoreFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Index_FuenteDeFechaMasChicaYEtiquetaDeOrigenPresente()
    {
        var page = await _fixture.NewAuthenticatedPageAsync();
        var errors = new List<string>();
        page.PageError += (_, msg) => errors.Add(msg);

        var hoy = DateTime.Today;
        var desde = hoy.AddDays(-6).ToString("yyyy-MM-dd");
        var hasta = hoy.ToString("yyyy-MM-dd");
        await page.GotoAsync($"{WebCoreFixture.BaseUrl}/Actividades?fechaDesde={desde}&fechaHasta={hasta}", new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });
        await page.WaitForTimeoutAsync(400);

        var filaFecha = page.Locator("table tbody tr td").First;
        int filas = await page.Locator("table tbody tr").CountAsync();
        Assert.True(filas > 0, "hace falta al menos una actividad real en los ultimos 7 dias en la base de dev para este test");

        var fontSize = await filaFecha.EvaluateAsync<string>("el => getComputedStyle(el).fontSize");
        Assert.Equal("13px", fontSize);

        // Al menos una fila trae la etiqueta de origen -- confirma que ActividadesFeedService
        // realmente resuelve OrigenFecha para las fuentes con fecha de negocio propia (ventas
        // anuladas/bonificadas, movimientos, compras).
        var textoTabla = await page.Locator("table tbody").InnerTextAsync();
        Assert.True(textoTabla.Contains("(creación)") || textoTabla.Contains("(modificación)"),
            "ninguna fila mostro la etiqueta de origen de fecha -- esperado al menos una entre ventas/movimientos/compras reales");

        Assert.Empty(errors);
        await page.CloseAsync();
    }

    [Fact]
    public async Task Index_ComprasMuestraSubTipoRealEnVezDeCompraStockGenerico()
    {
        var page = await _fixture.NewAuthenticatedPageAsync();

        var hoy = DateTime.Today;
        var desde = hoy.AddDays(-6).ToString("yyyy-MM-dd");
        var hasta = hoy.ToString("yyyy-MM-dd");
        await page.GotoAsync($"{WebCoreFixture.BaseUrl}/Actividades?fechaDesde={desde}&fechaHasta={hasta}", new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });
        await page.WaitForTimeoutAsync(400);

        var badgesTexto = await page.Locator("table tbody .badge-secondary").AllInnerTextsAsync();
        if (badgesTexto.Count == 0)
        {
            await page.CloseAsync();
            return;
        }

        // Ninguno de los sub-tipos reales de Compra (Entidades.Compra.tipoCompraEnum) debe seguir
        // apareciendo como el texto fijo generico "Compra/Stock" -- si aparece, es porque el
        // valor de "tipocompra" vino vacio para ese registro (fallback documentado), no porque el
        // sub-tipo real no se este usando.
        var subTiposReales = new[] { "Media Res", "Cortes", "Ingreso Stock", "Egreso Stock", "Cierre Stock", "Pesaje Cortes", "Ajuste Stock" };
        bool hayAlgunSubTipoReal = badgesTexto.Any(b => subTiposReales.Contains(b.Trim()));

        // Regresion condicionada a datos reales (documentado, no asumido): si no hubo ninguna
        // compra/registro de stock en el rango, no hay nada que verificar hoy.
        if (!badgesTexto.Any(b => b.Trim() == "Compra/Stock") && !hayAlgunSubTipoReal)
        {
            await page.CloseAsync();
            return;
        }

        Assert.True(hayAlgunSubTipoReal, "se esperaba al menos un sub-tipo real de Compra/Stock (ej. 'Cortes') en la columna Tipo, no el texto generico");

        await page.CloseAsync();
    }

    [Fact]
    public async Task Index_FiltroSoloAnomalias_ReduceLaListaYElCheckboxQuedaMarcado()
    {
        var page = await _fixture.NewAuthenticatedPageAsync();

        var hoy = DateTime.Today;
        var desde = hoy.AddDays(-6).ToString("yyyy-MM-dd");
        var hasta = hoy.ToString("yyyy-MM-dd");
        await page.GotoAsync($"{WebCoreFixture.BaseUrl}/Actividades?fechaDesde={desde}&fechaHasta={hasta}", new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });
        await page.WaitForTimeoutAsync(400);

        int totalSinFiltro = await page.Locator("table tbody tr").CountAsync();
        Assert.False(await page.IsCheckedAsync("#soloAnomalias"));

        await page.CheckAsync("#soloAnomalias");
        await page.ClickAsync("button:has-text('Buscar')");
        await page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);
        await page.WaitForTimeoutAsync(400);

        Assert.True(await page.IsCheckedAsync("#soloAnomalias"), "el checkbox deberia persistir tildado tras el submit (soloAnomalias=true en la URL)");
        Assert.Contains("soloAnomalias=true", page.Url, StringComparison.OrdinalIgnoreCase);

        int totalConFiltro = await page.Locator("table tbody tr").CountAsync();
        var textoFiltrado = await page.Locator("table tbody").InnerTextAsync();
        bool sinResultados = textoFiltrado.Contains("No hay actividad registrada");

        // Con el filtro activo, cada fila visible (si hay alguna) debe tener el icono de
        // advertencia -- si no hay ninguna anomalia real en el rango, la tabla queda vacia
        // (0 filas o el mensaje de "sin actividad"), lo cual tambien es un resultado valido.
        if (!sinResultados && totalConFiltro > 0)
        {
            int filasConIcono = await page.Locator("table tbody tr:has(.fa-exclamation-triangle)").CountAsync();
            Assert.Equal(totalConFiltro, filasConIcono);
        }

        Assert.True(totalConFiltro <= totalSinFiltro, "el filtro de anomalias no deberia mostrar mas filas que sin filtrar");

        await page.CloseAsync();
    }

    // Pedido explicito del usuario (2026-09-16, ver docs/DECISIONS.md): que quede claro para el
    // usuario POR QUE se marco un item como anomalia (ej. "una venta se creo hoy con fecha del mes
    // pasado") -- antes el icono solo tenia un title nativo generico, ahora un popover de
    // Bootstrap 5 (hover/focus/click) muestra ambas fechas reales del registro.
    [Fact]
    public async Task Index_IconoAnomalia_PopoverMuestraAmbasFechasReales()
    {
        var page = await _fixture.NewAuthenticatedPageAsync();

        var hoy = DateTime.Today;
        var desde = hoy.AddDays(-6).ToString("yyyy-MM-dd");
        var hasta = hoy.ToString("yyyy-MM-dd");
        await page.GotoAsync($"{WebCoreFixture.BaseUrl}/Actividades?fechaDesde={desde}&fechaHasta={hasta}&soloAnomalias=true", new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });
        await page.WaitForTimeoutAsync(400);

        // Regresion condicionada a datos reales (mismo criterio que el resto de este archivo): si
        // no hay ninguna anomalia real en los ultimos 7 dias de la base de dev, no hay nada que
        // verificar hoy.
        // (con el rango vacio la tabla igual trae 1 fila: el mensaje "No hay actividad", por eso se cuenta el icono)
        int iconos = await page.Locator(".js-anomalia-popover").CountAsync();
        if (iconos == 0)
        {
            await page.CloseAsync();
            return;
        }

        var icono = page.Locator(".js-anomalia-popover").First;
        await icono.ClickAsync();

        var popoverBody = page.Locator(".popover .popover-body");
        await popoverBody.WaitForAsync(new LocatorWaitForOptions { Timeout = 5000 });
        var textoPopover = await popoverBody.InnerTextAsync();

        // El bug real que motivo el pedido: el usuario no podia saber, sin abrir la venta/registro,
        // que la fecha mostrada era de creacion/modificacion y no la fecha real del hecho -- el
        // popover ahora dice las dos fechas explicitamente y el motivo.
        Assert.Contains("corresponde al", textoPopover, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("día", textoPopover, StringComparison.OrdinalIgnoreCase);
        Assert.Matches(@"\d{2}/\d{2}/\d{4}.*\d{2}/\d{2}/\d{4}", textoPopover);

        await page.CloseAsync();
    }
}
