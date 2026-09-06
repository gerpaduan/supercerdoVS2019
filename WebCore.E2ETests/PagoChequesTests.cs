using Microsoft.Playwright;

namespace WebCore.E2ETests;

// Regresion del pago con Cheque/EftvoCheque en AddOrEditPago (2026-09-05, ver docs/DECISIONS.md).
// Port literal de Scripts/app/pago-cheques.js desde Web clasico, reusando el CRUD de cheques ya
// existente (BuscarChequePorNro/GetCheques/GuardarCheque). Bug real encontrado y corregido durante
// la verificacion: ASP.NET Core serializa Json() en camelCase por defecto (a diferencia de MVC5,
// que preservaba el PascalCase de los objetos anonimos) -- el script portado leia cheque.Id/
// .NroCheque/etc. contra una respuesta real con id/nroCheque/etc., fallando en silencio (sin
// pageerror, sin excepcion visible: simplemente nunca agregaba la fila). Corregido normalizando
// una vez en el borde (normalizarChequeServer), no en cada lectura de propiedad ya portada.
[Collection("WebCore browser")]
public sealed class PagoChequesTests
{
    private readonly WebCoreFixture _fixture;

    public PagoChequesTests(WebCoreFixture fixture)
    {
        _fixture = fixture;
    }

    private static async Task<int> BuscarUnaPersonaReal(IPage page)
    {
        var resp = await page.GotoAsync($"{WebCoreFixture.BaseUrl}/Personas/Listar?filtro=");
        var json = await resp!.TextAsync();
        using var doc = System.Text.Json.JsonDocument.Parse(json);
        return doc.RootElement.EnumerateArray().First().GetProperty("idPersona").GetInt32();
    }

    [Fact]
    public async Task FormaPagoCheque_MuestraBloqueYAgregaChequeReal()
    {
        var page = await _fixture.NewAuthenticatedPageAsync();
        var errors = new List<string>();
        page.PageError += (_, msg) => errors.Add(msg);

        // Cheque de prueba propio, con fecha futura para no chocar con la regla de vencimiento --
        // se crea via el mismo endpoint real que usa el modal de alta (GuardarCheque), no un mock.
        // Se hace desde el navegador (fetch) en vez de IAPIRequestContext para no depender de la
        // forma exacta de esa API en esta version de Playwright .NET.
        var nroChequePrueba = "E2E" + DateTime.UtcNow.Ticks;
        await page.GotoAsync(WebCoreFixture.BaseUrl + "/Finanzas/Cheques");
        var guardadoOk = await page.EvaluateAsync<bool>(@"async (nro) => {
            const body = new URLSearchParams({
                NroCheque: nro, Banco: 'NACION', FechaPago: '2099-01-01', Importe: '55',
                Propio: 'false', Titular: 'E2E PagoChequesTests', esAProveedor: 'false'
            });
            const resp = await fetch('/Finanzas/GuardarCheque', { method: 'POST', body });
            const data = await resp.json();
            return !!(data && data.ok);
        }", nroChequePrueba);
        Assert.True(guardadoOk, "No se pudo crear el cheque de prueba via GuardarCheque.");

        var idPersona = await BuscarUnaPersonaReal(page);

        await page.GotoAsync($"{WebCoreFixture.BaseUrl}/Finanzas/AddOrEditPago?idPersona={idPersona}&returnUrl=", new PageGotoOptions
        {
            WaitUntil = WaitUntilState.NetworkIdle,
            Timeout = 20000
        });

        await page.SelectOptionAsync("#FormaPago_", "Cheque");
        await page.WaitForTimeoutAsync(300);
        Assert.True(await page.Locator("#bloqueCheques").IsVisibleAsync());

        await page.FillAsync("#txtNroCheque", nroChequePrueba);
        await page.ClickAsync("#btnAgregarCheque");
        await page.WaitForTimeoutAsync(1000);

        Assert.Empty(errors);
        Assert.Equal(1, await page.Locator("#tablaCheques tbody tr").CountAsync());
        Assert.Equal("55.00", await page.Locator("#txtTotalCheques").InputValueAsync());
        Assert.Equal("55.00", await page.Locator("#txtImporte").InputValueAsync());

        await page.CloseAsync();
    }

    [Fact]
    public async Task FormaPagoEftvoCheque_MuestraBloqueEfectivoYCheques()
    {
        var page = await _fixture.NewAuthenticatedPageAsync();
        var idPersona = await BuscarUnaPersonaReal(page);

        await page.GotoAsync($"{WebCoreFixture.BaseUrl}/Finanzas/AddOrEditPago?idPersona={idPersona}&returnUrl=", new PageGotoOptions
        {
            WaitUntil = WaitUntilState.NetworkIdle,
            Timeout = 20000
        });

        await page.SelectOptionAsync("#FormaPago_", "EftvoCheque");
        await page.WaitForTimeoutAsync(300);

        Assert.True(await page.Locator("#bloqueCheques").IsVisibleAsync());
        Assert.True(await page.Locator("#bloqueEfectivo").IsVisibleAsync());

        await page.CloseAsync();
    }
}
