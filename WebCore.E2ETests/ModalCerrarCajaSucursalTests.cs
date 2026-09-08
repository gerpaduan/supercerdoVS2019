using Microsoft.Playwright;

namespace WebCore.E2ETests;

// Pedido explicito del usuario (2026-09-07, ver docs/DECISIONS.md): en el modal "Cerrar Caja"
// (Cajas/CajasAbiertas), "Sucursal" iba en su propia fila arriba como texto plano
// (<div class="font-weight-bold">, sin label real) en vez de al lado de "Usuario Inicio" como
// label + input readonly, igual que el resto de los campos del modal.
[Collection("WebCore browser")]
public sealed class ModalCerrarCajaSucursalTests
{
    private readonly WebCoreFixture _fixture;

    public ModalCerrarCajaSucursalTests(WebCoreFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task ModalCerrarCaja_SucursalAlLadoDeUsuarioInicio_ComoInputReadonly()
    {
        var page = await _fixture.NewAuthenticatedPageAsync();
        await page.GotoAsync($"{WebCoreFixture.BaseUrl}/Cajas/CajasAbiertas", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        await page.WaitForTimeoutAsync(500);

        var botonCerrar = page.Locator("#tablaCajas button.btn-danger:has-text('Cerrar')").First;
        Assert.True(await botonCerrar.CountAsync() > 0, "no se encontro ningun boton 'Cerrar' -- ¿cambio el fixture de datos (caja abierta)?");
        await botonCerrar.ClickAsync();
        await page.WaitForSelectorAsync("#modalCerrarCaja.show", new PageWaitForSelectorOptions { Timeout = 10000 });
        await page.WaitForTimeoutAsync(400);

        // "Sucursal" ahora es un <input readonly>, no un <div>, y comparte fila con "Usuario Inicio".
        var inputSucursal = page.Locator("#labelSucursal");
        Assert.Equal("input", await inputSucursal.EvaluateAsync<string>("el => el.tagName.toLowerCase()"));
        Assert.True(await inputSucursal.EvaluateAsync<bool>("el => el.readOnly"));
        Assert.NotEmpty((await inputSucursal.InputValueAsync()).Trim());

        var mismaFila = await page.EvaluateAsync<bool>(@"
            () => {
                const sucursal = document.querySelector('#labelSucursal');
                const usuarioInicio = document.querySelector('#CajaDesc');
                if (!sucursal || !usuarioInicio) return false;
                return sucursal.closest('.row') === usuarioInicio.closest('.row');
            }");
        Assert.True(mismaFila, "Sucursal deberia estar en la misma fila (.row) que Usuario Inicio");

        await page.CloseAsync();
    }
}
