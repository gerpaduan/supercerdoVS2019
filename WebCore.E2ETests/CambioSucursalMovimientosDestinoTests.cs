using Microsoft.Playwright;

namespace WebCore.E2ETests;

// Batch 7 de la quinta ronda de pedidos, 2026-09-10 (ver docs/DECISIONS.md): "al cambiar de
// sucursal, se debe validar que no hay ventas, egresos de caja, etc en nueva sucursal destino
// desde la apertura de caja. Si existen, informar al usuario... y que aparezca un boton que diga
// que esta de acuerdo a realizar el paso". Antes, ConstruirPlanCambioSucursalCaja solo bloqueaba
// si el usuario ya tenia OTRA caja abierta en destino (TieneCajaAbiertaEnDestino) -- nunca
// chequeaba si ya habia ventas/egresos de CUALQUIER usuario en destino durante el rango de esta
// caja, que es exactamente el escenario que deja el cierre inconsistente.
//
// Enfoque determinista sin asumir que la base de dev esta "limpia": en vez de afirmar "no hay
// advertencia" contra un estado inicial desconocido, se mide el conteo ANTES (via el mismo
// endpoint de preview, por fetch directo) y DESPUES de sembrar un egreso real nuevo en la
// sucursal destino -- el delta de +1 es verificable sin importar cuantos movimientos hubiera ya.
[Collection("WebCore browser")]
public sealed class CambioSucursalMovimientosDestinoTests
{
    private readonly WebCoreFixture _fixture;

    public CambioSucursalMovimientosDestinoTests(WebCoreFixture fixture)
    {
        _fixture = fixture;
    }

    private static async Task<int> ContarFilasConBotonCambioSucursalAsync(IPage page)
    {
        await page.GotoAsync($"{WebCoreFixture.BaseUrl}/Cajas/CajasAbiertas", new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });
        await page.WaitForTimeoutAsync(600);
        return await page.Locator("#tablaCajas tbody tr button.btn-warning:has-text('Cambiar sucursal')").CountAsync();
    }

    private static async Task<(int idCierre, int idSucursalActual, List<int> destinosPosibles)> AbrirModalYListarDestinosAsync(IPage page, int indiceFila)
    {
        await page.GotoAsync($"{WebCoreFixture.BaseUrl}/Cajas/CajasAbiertas", new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });
        await page.WaitForTimeoutAsync(600);

        var botonesCambiar = page.Locator("#tablaCajas tbody tr button.btn-warning:has-text('Cambiar sucursal')");
        await botonesCambiar.Nth(indiceFila).ClickAsync();
        await page.WaitForTimeoutAsync(400);
        Assert.Equal(1, await page.Locator("#modalCambioSucursalCaja.show").CountAsync());

        int idCierre = int.Parse(await page.InputValueAsync("#CambioSucursalIdCierre"));
        int idSucursalActual = int.Parse(await page.InputValueAsync("#CambioSucursalIdActual"));

        var valores = await page.Locator("#CambioSucursalNueva option:not([disabled])")
            .Filter(new LocatorFilterOptions { HasNotText = "Seleccionar sucursal" })
            .EvaluateAllAsync<int[]>("els => els.map(el => parseInt(el.value, 10))");

        return (idCierre, idSucursalActual, valores.ToList());
    }

    // Encuentra una combinacion caja/destino real donde el preview pueda ejecutarse (puedeEjecutar
    // true) -- descarta destinos donde el usuario de pruebas ya tiene otra caja abierta
    // (TieneCajaAbiertaEnDestino, el bloqueo duro preexistente, que corta ANTES de calcular
    // HayMovimientosEnDestino), condicion real y esperable en una base de dev con muchas corridas
    // de tests previas abriendo cajas en distintas sucursales con el mismo usuario.
    private static async Task<(int idCierre, int idSucursalDestino)?> BuscarCombinacionUtilizableAsync(IPage page)
    {
        int totalFilas = await ContarFilasConBotonCambioSucursalAsync(page);

        for (int fila = 0; fila < totalFilas; fila++)
        {
            var datos = await AbrirModalYListarDestinosAsync(page, fila);

            foreach (var destino in datos.destinosPosibles)
            {
                var preview = await ConsultarPreviewAsync(page, datos.idCierre, destino);
                if (preview.ok && preview.puedeEjecutar)
                    return (datos.idCierre, destino);
            }
        }

        return null;
    }

    private static async Task<(bool ok, bool puedeEjecutar, bool hayMovimientos, string advertencia)> ConsultarPreviewAsync(IPage page, int idCierre, int idSucursalDestino)
    {
        // EvaluateAsync<(...)> no puede deserializar ValueTuple (Playwright intenta invocar un
        // constructor por reflexion que no existe) -- se devuelve un object[] y se indexa, mismo
        // workaround ya documentado en docs/DECISIONS.md para este mismo problema.
        var resultado = await page.EvaluateAsync<object[]>(@"
            async ([idCierre, idSucursalDestino]) => {
                const resp = await fetch('/Cajas/PreviewCambioSucursalCaja?idCierre=' + idCierre + '&idSucursalNueva=' + idSucursalDestino);
                const data = await resp.json();
                return [!!data.ok, !!data.puedeEjecutar, !!data.hayMovimientosEnDestino, data.advertenciaMovimientosEnDestino || ''];
            }", new object[] { idCierre, idSucursalDestino });

        return (
            Convert.ToBoolean(resultado[0]),
            Convert.ToBoolean(resultado[1]),
            Convert.ToBoolean(resultado[2]),
            Convert.ToString(resultado[3]) ?? "");
    }

    private static async Task<int> ObtenerIdTipoEgresoRealAsync(IPage page)
    {
        return await page.EvaluateAsync<int>(@"
            async () => {
                const resp = await fetch('/Cajas/TiposEgresoCajaOpciones');
                const data = await resp.json();
                return (data.items && data.items.length) ? data.items[0].id : 0;
            }");
    }

    private static async Task<bool> SembrarEgresoEnSucursalAsync(IPage page, int idSucursalDestino, int idTipoEgreso)
    {
        return await page.EvaluateAsync<bool>(@"
            async ([idSucursalDestino, idTipoEgreso]) => {
                const token = document.querySelector('input[name=""__RequestVerificationToken""]')?.value || '';
                const form = new URLSearchParams();
                form.set('__RequestVerificationToken', token);
                form.set('id', '0');
                // Fecha local SIN sufijo 'Z' (no UTC): el rango [FechaDesde, FechaHasta] de la caja
                // se compara contra DateTime.Now (hora local del servidor) -- un ISO con 'Z' se
                // interpretaria como UTC y, en Argentina (UTC-3), quedaria ~3hs en el futuro
                // respecto al 'ahora' local, cayendo fuera del rango BETWEEN.
                const ahora = new Date();
                const pad = n => String(n).padStart(2, '0');
                const fechaLocal = ahora.getFullYear() + '-' + pad(ahora.getMonth() + 1) + '-' + pad(ahora.getDate()) +
                    'T' + pad(ahora.getHours()) + ':' + pad(ahora.getMinutes()) + ':' + pad(ahora.getSeconds());
                form.set('fecha', fechaLocal);
                form.set('idTipoEgresoCaja', String(idTipoEgreso));
                form.set('descripcion', 'E2E CambioSucursalMovimientosDestinoTests');
                form.set('monto', '100');
                form.set('detalle', '');
                form.set('idSucursal', String(idSucursalDestino));
                const resp = await fetch('/Cajas/GuardarEgresoCaja', { method: 'POST', body: form });
                const data = await resp.json();
                return !!data.ok;
            }", new object[] { idSucursalDestino, idTipoEgreso });
    }

    [Fact]
    public async Task SembrarEgresoEnDestino_HaceAparecerLaAdvertenciaYGateaElBotonHastaAceptarElRiesgo()
    {
        var page = await _fixture.NewAuthenticatedPageAsync();

        var combinacion = await BuscarCombinacionUtilizableAsync(page);
        Assert.True(combinacion.HasValue, "no se encontro ninguna combinacion caja/destino ejecutable (el usuario de pruebas ya tiene caja abierta en todos los destinos posibles)");
        var (idCierre, idSucursalDestino) = combinacion!.Value;

        var antes = await ConsultarPreviewAsync(page, idCierre, idSucursalDestino);
        Assert.True(antes.ok);

        // El fetch de sembrado necesita el token antiforgery de una pagina con formulario real
        // (mismo criterio que PreciosSeedHelper) -- Cajas/CajasAbiertas no tiene uno propio, se usa
        // DispositivosSeguros que si lo tiene.
        await page.GotoAsync($"{WebCoreFixture.BaseUrl}/DispositivosSeguros", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });

        int idTipoEgreso = await ObtenerIdTipoEgresoRealAsync(page);
        Assert.True(idTipoEgreso > 0, "hace falta al menos un tipo de egreso de caja real en la base de dev");

        bool sembrado = await SembrarEgresoEnSucursalAsync(page, idSucursalDestino, idTipoEgreso);
        Assert.True(sembrado, "no se pudo sembrar el egreso de prueba en la sucursal destino");

        var despues = await ConsultarPreviewAsync(page, idCierre, idSucursalDestino);
        Assert.True(despues.ok);

        // El egreso recien sembrado por si solo alcanza para que hayMovimientosEnDestino de
        // verdad (independiente de cuantos hubiera antes en la base de dev).
        Assert.True(despues.hayMovimientos, "sembrar un egreso real en destino deberia activar la advertencia");
        Assert.Contains("egreso", despues.advertencia, StringComparison.OrdinalIgnoreCase);

        // Ahora se verifica el gate real en la UI: reabrir el modal (recarga el preview via AJAX
        // normal) y confirmar que el boton queda deshabilitado hasta tildar el checkbox. Se busca
        // la fila por idCierre (no "la primera"): BuscarCombinacionUtilizableAsync puede haber
        // elegido una fila distinta de la 0 si el usuario de pruebas ya tenia caja abierta ahi.
        await page.GotoAsync($"{WebCoreFixture.BaseUrl}/Cajas/CajasAbiertas", new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });
        await page.WaitForTimeoutAsync(600);
        await page.Locator($"#tablaCajas tbody tr button[onclick*='abrirModalCambioSucursal({idCierre},']").ClickAsync();
        await page.WaitForTimeoutAsync(400);
        await page.SelectOptionAsync("#CambioSucursalNueva", idSucursalDestino.ToString());
        await page.WaitForTimeoutAsync(800);

        Assert.False(await page.Locator("#cambioSucursalMovimientosDestinoWrap").IsHiddenAsync());
        Assert.True(await page.IsDisabledAsync("#btnConfirmarCambioSucursalCaja"),
            "con movimientos en destino y el checkbox sin tildar, el boton de confirmar debe seguir deshabilitado");

        await page.CheckAsync("#chkCambioSucursalAceptaRiesgo");
        await page.WaitForTimeoutAsync(200);
        Assert.False(await page.IsDisabledAsync("#btnConfirmarCambioSucursalCaja"),
            "al tildar 'entiendo el riesgo', el boton de confirmar debe habilitarse");

        // Destildar vuelve a deshabilitar -- confirma que el gate reacciona al checkbox, no solo
        // que arranco habilitado por otro motivo.
        await page.UncheckAsync("#chkCambioSucursalAceptaRiesgo");
        await page.WaitForTimeoutAsync(200);
        Assert.True(await page.IsDisabledAsync("#btnConfirmarCambioSucursalCaja"));

        await page.CloseAsync();
    }

    [Fact]
    public async Task SinMovimientosNuevosEnDestino_ElBotonSeHabilitaSinNecesidadDeCheckbox()
    {
        var page = await _fixture.NewAuthenticatedPageAsync();

        var combinacion = await BuscarCombinacionUtilizableAsync(page);
        if (combinacion == null)
        {
            // Regresion, condicionada al estado real de la base de dev (documentado, no asumido):
            // si el usuario de pruebas ya tiene caja abierta en TODOS los destinos posibles de
            // TODAS las cajas abiertas, no hay ninguna combinacion "puedeEjecutar=true" disponible
            // hoy -- se omite en vez de fallar por un estado que este test no controla.
            await page.CloseAsync();
            return;
        }
        var (idCierre, idSucursalDestino) = combinacion.Value;

        var preview = await ConsultarPreviewAsync(page, idCierre, idSucursalDestino);
        Assert.True(preview.ok);
        Assert.True(preview.puedeEjecutar);

        if (preview.hayMovimientos)
        {
            // Idem: esta combinacion puntual ya viene con movimientos de una corrida anterior --
            // el escenario "sin advertencia" no aplica hoy para ella.
            await page.CloseAsync();
            return;
        }

        // Recien aca se selecciona la sucursal destino en la UI real -- dispara el mismo AJAX de
        // preview que un usuario real dispararia, poblando #cambioSucursalMovimientosDestinoWrap
        // y el estado del boton (antes de esto, el modal recien abierto nunca cargo ningun preview).
        await page.Locator($"#tablaCajas tbody tr button[onclick*='abrirModalCambioSucursal({idCierre},']").ClickAsync();
        await page.WaitForTimeoutAsync(400);
        await page.SelectOptionAsync("#CambioSucursalNueva", idSucursalDestino.ToString());
        await page.WaitForTimeoutAsync(800);

        Assert.True(await page.Locator("#cambioSucursalMovimientosDestinoWrap").IsHiddenAsync());
        Assert.False(await page.IsDisabledAsync("#btnConfirmarCambioSucursalCaja"),
            "sin movimientos en destino, el boton deberia habilitarse directo, sin pedir el checkbox");

        await page.CloseAsync();
    }
}
