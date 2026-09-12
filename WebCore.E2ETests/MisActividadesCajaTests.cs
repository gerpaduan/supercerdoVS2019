using Microsoft.Playwright;

namespace WebCore.E2ETests;

// Bug real reportado por el usuario 2026-09-11 (ver docs/DECISIONS.md "Batch 2: F6 Mis
// actividades bloqueado para no-admin"): F6 en POS apuntaba a CajasController.ActividadesCaja,
// gateada por el permiso de CERRAR caja (Caja.CerrarCaja) -- un vendedor sin ese permiso no podia
// ver ni su propia actividad. Se porto MisActividadesCaja, sin ese gate (solo exige que la caja
// consultada sea la propia), sin tocar ActividadesCaja (sigue siendo la pantalla admin de Cajas
// Abiertas). Usuario de prueba: "cajero" (id=10, clave "a", no admin -- mismo usuario ya
// establecido en PermisosGateTests.cs para probar gates de permisos reales), con una caja real ya
// abierta en la base de dev (id=220000022, sucursal 2).
[Collection("WebCore browser")]
public sealed class MisActividadesCajaTests
{
    private readonly WebCoreFixture _fixture;

    public MisActividadesCajaTests(WebCoreFixture fixture)
    {
        _fixture = fixture;
    }

    private const int IdCierreCajeroAbierto = 220000022;

    private async Task<IPage> NewCajeroAuthenticatedPageAsync()
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
    public async Task UsuarioCajero_ActividadesCajaAdmin_SigueBloqueada()
    {
        // Regresion: la pantalla admin (Cajas Abiertas) NO debe abrirse para "cajero" -- sigue
        // gateada por Caja.CerrarCaja, sin cambios de este batch.
        var page = await NewCajeroAuthenticatedPageAsync();

        var resp = await page.GotoAsync(
            $"{WebCoreFixture.BaseUrl}/Cajas/ActividadesCaja?idCierre={IdCierreCajeroAbierto}",
            new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });

        Assert.Equal(403, resp!.Status);

        await page.CloseAsync();
    }

    [Fact]
    public async Task UsuarioCajero_MisActividadesCaja_VeSuPropiaCajaSinElPermisoDeCerrar()
    {
        var page = await NewCajeroAuthenticatedPageAsync();

        var resp = await page.GotoAsync(
            $"{WebCoreFixture.BaseUrl}/Cajas/MisActividadesCaja?idCierre={IdCierreCajeroAbierto}",
            new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });

        Assert.Equal(200, resp!.Status);
        var texto = await page.InnerTextAsync("body");
        Assert.DoesNotContain("No tiene permisos", texto);

        await page.CloseAsync();
    }

    [Fact]
    public async Task UsuarioCajero_MisActividadesCaja_NoPuedeVerLaCajaDeOtroUsuario()
    {
        // Seguridad: MisActividadesCaja recibe idCierre del cliente -- no debe confiar
        // ciegamente en el, solo mostrar la propia caja.
        var page = await NewCajeroAuthenticatedPageAsync();

        // 10000001: caja historica de "ger" (usuarioinicio=2), no de "cajero" -- ver verificacion
        // manual de esta sesion contra la base de dev.
        var resp = await page.GotoAsync(
            $"{WebCoreFixture.BaseUrl}/Cajas/MisActividadesCaja?idCierre=10000001",
            new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });

        Assert.Equal(403, resp!.Status);

        await page.CloseAsync();
    }

    [Fact]
    public async Task UsuarioAdmin_F6EnPOS_UsaMisActividadesCajaNoLaAccionAdmin()
    {
        // Regresion del rewire de F6 en POS.cshtml: la URL que arma window.EgresosCajaUrls.mis
        // debe apuntar a la accion nueva, no a la vieja.
        var page = await _fixture.NewAuthenticatedPageAsync();
        await page.GotoAsync($"{WebCoreFixture.BaseUrl}/Ventas/POS", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        await page.WaitForTimeoutAsync(500);

        var url = await page.EvaluateAsync<string>("window.EgresosCajaUrls?.mis || ''");
        Assert.Contains("MisActividadesCaja", url);

        await page.CloseAsync();
    }
}
