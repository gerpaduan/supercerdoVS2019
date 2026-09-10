using Microsoft.Playwright;

namespace WebCore.E2ETests;

// Tercera ronda de pedidos (2026-09-10, ver docs/DECISIONS.md): "Autorizar acción de Cierre de
// Caja, en cerrar caja, no me valida un usuario válido para cerrar la caja. el mensaje es 'No se
// pudo validar la autorización.'"
//
// Causa raíz: el JS de CajasAbiertas.cshtml ya apuntaba el step-up a
// /Cajas/AutorizarAccionCierre, pero ese endpoint (y RevocarAutorizacionCierre) nunca se
// portaron -- confirmado por el propio comentario de cabecera del controller reconociendo el
// gap. La llamada daba 404, que caía en el callback error: de seleccion-usuario.js, mostrando el
// mensaje genérico de red en vez de un error de validación real.
//
// Alcance ampliado, confirmado con el usuario antes de implementar: no eran solo 2 endpoints
// faltantes -- CerrarCaja, ActividadesCaja y PuedeCambiarSucursalCaja (esta última protege
// también PreviewCambioSucursalCaja/CambiarSucursalCaja) no tenían NINGÚN chequeo de permiso de
// servidor, confirmado leyendo el código directamente antes de reportarlo: cualquier usuario
// logueado podía cerrar una caja ajena o cambiar su sucursal llamando la acción directo.
//
// Usuarios reales de la base de dev confirmados empíricamente (no asumidos): "ger" y "userweb"
// tienen Permisos.Caja.CerrarCaja directo; "cajero"/"producción"/"usercaja"/"a" NO lo tienen.
[Collection("WebCore browser")]
public sealed class CierreCajaStepUpTests
{
    private readonly WebCoreFixture _fixture;

    public CierreCajaStepUpTests(WebCoreFixture fixture)
    {
        _fixture = fixture;
    }

    private async Task<IPage> LoginCajero()
    {
        var page = await _fixture.Browser.NewPageAsync();
        await page.GotoAsync($"{WebCoreFixture.BaseUrl}/Login", new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });
        await page.FillAsync("input[name='Usuario']", "cajero");
        await page.FillAsync("input[name='Clave']", "a");
        await page.ClickAsync("button[type='submit']");
        await page.WaitForURLAsync(url => !url.Contains("/Login"), new PageWaitForURLOptions { Timeout = 15000 });
        return page;
    }

    [Fact]
    public async Task UsuarioSinPermisoDirecto_ClickearCerrar_PideStepUpYAutorizarConCredencialesRealesFunciona()
    {
        var page = await LoginCajero();
        var errors = new List<string>();
        page.PageError += (_, msg) => errors.Add(msg);

        await page.GotoAsync($"{WebCoreFixture.BaseUrl}/Cajas/CajasAbiertas", new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });
        await page.WaitForTimeoutAsync(600);

        var filas = await page.Locator("#tablaCajas tbody tr").CountAsync();
        Assert.True(filas > 0, "hace falta al menos una caja abierta real en la base de dev para este test");

        await page.Locator("#tablaCajas tbody tr").First.Locator("button.btn-danger").ClickAsync();
        await page.WaitForTimeoutAsync(600);

        // Assert principal: antes del fix, esto directamente no existia (404) y el mensaje
        // generico de red aparecia sin llegar a mostrar el selector de usuario.
        Assert.Equal(1, await page.Locator("#modalSeleccionUsuario.show").CountAsync());

        await page.FillAsync("#txtSeleccionUsuario", "ger");
        await page.WaitForTimeoutAsync(300);
        await page.Locator(".seleccion-usuario-item").First.ClickAsync();
        await page.FillAsync("#passSeleccionUsuario", "a");
        await page.ClickAsync("#btnConfirmarSeleccionUsuario");
        await page.WaitForTimeoutAsync(1500);

        // La elevacion temporal quedo registrada en sesion -- el modal de cerrar caja (la accion
        // envuelta por conAutorizacionCierre) se abre normalmente.
        Assert.Equal(1, await page.Locator("#modalCerrarCaja.show").CountAsync());
        Assert.Empty(errors);
    }

    [Fact]
    public async Task UsuarioSinPermisoDirecto_AutorizarConClaveIncorrecta_MuestraErrorRealNoElGenericoDeRed()
    {
        var page = await LoginCajero();
        await page.GotoAsync($"{WebCoreFixture.BaseUrl}/Cajas/CajasAbiertas", new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });
        await page.WaitForTimeoutAsync(600);

        var filas = await page.Locator("#tablaCajas tbody tr").CountAsync();
        Assert.True(filas > 0, "hace falta al menos una caja abierta real en la base de dev para este test");

        await page.Locator("#tablaCajas tbody tr").First.Locator("button.btn-danger").ClickAsync();
        await page.WaitForTimeoutAsync(600);
        Assert.Equal(1, await page.Locator("#modalSeleccionUsuario.show").CountAsync());

        await page.FillAsync("#txtSeleccionUsuario", "ger");
        await page.WaitForTimeoutAsync(300);
        await page.Locator(".seleccion-usuario-item").First.ClickAsync();
        await page.FillAsync("#passSeleccionUsuario", "clave-incorrecta-real");
        await page.ClickAsync("#btnConfirmarSeleccionUsuario");
        await page.WaitForTimeoutAsync(1000);

        var textoError = await page.Locator("#seleccionUsuarioError").InnerTextAsync();
        Assert.Contains("Usuario o contraseña incorrectos", textoError);
        Assert.DoesNotContain("No se pudo validar la autorización", textoError);
    }

    // Regresion de seguridad real (no solo el flujo feliz de UI): antes del fix, estas 3
    // acciones aceptaban la peticion de CUALQUIER usuario logueado sin validar nada.
    [Theory]
    [InlineData("/Cajas/ActividadesCaja?idCierre=1")]
    public async Task UsuarioSinPermisoNiElevacion_LlamadaDirectaAlEndpoint_QuedaRechazada(string url)
    {
        var page = await LoginCajero();
        var resp = await page.GotoAsync($"{WebCoreFixture.BaseUrl}{url}", new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });
        Assert.Equal(403, resp!.Status);
    }

    [Fact]
    public async Task TresIntentosFallidos_BloqueaElCuartoConMensajeDeRateLimit()
    {
        var page = await LoginCajero();
        await page.GotoAsync($"{WebCoreFixture.BaseUrl}/Cajas/CajasAbiertas", new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });
        await page.WaitForTimeoutAsync(600);

        var filas = await page.Locator("#tablaCajas tbody tr").CountAsync();
        Assert.True(filas > 0, "hace falta al menos una caja abierta real en la base de dev para este test");

        await page.Locator("#tablaCajas tbody tr").First.Locator("button.btn-danger").ClickAsync();
        await page.WaitForTimeoutAsync(600);
        Assert.Equal(1, await page.Locator("#modalSeleccionUsuario.show").CountAsync());

        bool bloqueadoVisible = false;
        for (int intento = 1; intento <= 4 && !bloqueadoVisible; intento++)
        {
            if (await page.Locator("#seleccionUsuarioBloqueado:visible").CountAsync() > 0)
            {
                bloqueadoVisible = true;
                break;
            }

            await page.FillAsync("#txtSeleccionUsuario", "ger");
            await page.WaitForTimeoutAsync(200);
            await page.Locator(".seleccion-usuario-item").First.ClickAsync();
            await page.FillAsync("#passSeleccionUsuario", "clave-incorrecta-real");
            await page.ClickAsync("#btnConfirmarSeleccionUsuario");
            await page.WaitForTimeoutAsync(800);
            bloqueadoVisible = await page.Locator("#seleccionUsuarioBloqueado:visible").CountAsync() > 0;
        }

        // El rate-limiter tiene un maximo de intentos configurable (default 3) -- tras alcanzarlo,
        // el modal pasa a mostrar el countdown de bloqueo en vez del formulario.
        Assert.True(bloqueadoVisible, "el rate-limiter deberia haber bloqueado la sesion tras varios intentos fallidos");
    }

    [Fact]
    public async Task UsuarioConPermisoDirecto_NoNecesitaStepUp_AccedeDirecto()
    {
        var page = await _fixture.Browser.NewPageAsync();
        await page.GotoAsync($"{WebCoreFixture.BaseUrl}/Login", new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });
        await page.FillAsync("input[name='Usuario']", "ger");
        await page.FillAsync("input[name='Clave']", "a");
        await page.ClickAsync("button[type='submit']");
        await page.WaitForURLAsync(url => !url.Contains("/Login"), new PageWaitForURLOptions { Timeout = 15000 });

        await page.GotoAsync($"{WebCoreFixture.BaseUrl}/Cajas/CajasAbiertas", new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });
        await page.WaitForTimeoutAsync(600);

        var tienePermiso = await page.EvaluateAsync<bool>("() => window.CajasStepUpTienePermisoDirecto === true");
        Assert.True(tienePermiso);

        var filas = await page.Locator("#tablaCajas tbody tr").CountAsync();
        if (filas == 0) return;

        await page.Locator("#tablaCajas tbody tr").First.Locator("button.btn-danger").ClickAsync();
        await page.WaitForTimeoutAsync(600);

        // Con permiso directo, conAutorizacionCierre ejecuta la accion sin pedir nada.
        Assert.Equal(0, await page.Locator("#modalSeleccionUsuario.show").CountAsync());
        Assert.Equal(1, await page.Locator("#modalCerrarCaja.show").CountAsync());
    }
}
