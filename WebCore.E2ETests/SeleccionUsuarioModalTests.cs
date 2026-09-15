using Microsoft.Playwright;

namespace WebCore.E2ETests;

// Pedido real (2026-09-15, ver docs/DECISIONS.md): en el modal de #modalSeleccionUsuario
// (compartido por "Autorizate para operar este Punto de Venta", step-up de Cierre de Caja, etc.)
// el usuario/contraseña quedaban con texto autocompletado por el navegador pese a
// autocomplete="off" -- Chrome/Edge ignoran ese atributo para campos de credenciales. Fix:
// re-limpiar ambos campos en shown.bs.modal (backstop tras un autocompletado tardío) y
// seleccionar todo el texto de la contraseña al enfocarla, para que escribir la reemplace en vez
// de insertarse en el medio.
[Collection("WebCore browser")]
public sealed class SeleccionUsuarioModalTests
{
    private readonly WebCoreFixture _fixture;

    public SeleccionUsuarioModalTests(WebCoreFixture fixture)
    {
        _fixture = fixture;
    }

    // "ger" tiene Permisos.Caja.CerrarCaja directo -- cierra sin pedir step-up. Se necesita un
    // usuario SIN ese permiso (mismo criterio que CierreCajaStepUpTests) para que el modal
    // realmente se abra.
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
    public async Task ModalConPassword_ArrancaVacioYSeleccionaTodoElTextoAlEnfocarLaContrasena()
    {
        var page = await LoginCajero();
        await page.GotoAsync($"{WebCoreFixture.BaseUrl}/Cajas/CajasAbiertas", new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });
        await page.WaitForTimeoutAsync(600);

        var filas = await page.Locator("#tablaCajas tbody tr").CountAsync();
        Assert.True(filas > 0, "hace falta al menos una caja abierta real en la base de dev para este test");

        await page.Locator("#tablaCajas tbody tr").First.Locator("button.btn-danger").ClickAsync();
        await page.Locator("#modalSeleccionUsuario.show").WaitForAsync(new LocatorWaitForOptions { Timeout = 10000 });
        await page.WaitForTimeoutAsync(300);

        // El bug real: ambos campos deben arrancar vacios (aunque el navegador haya intentado
        // autocompletar algo antes de que corriera el re-limpiado de shown.bs.modal).
        Assert.Equal("", await page.Locator("#txtSeleccionUsuario").InputValueAsync());
        Assert.Equal("", await page.Locator("#passSeleccionUsuario").InputValueAsync());

        // Simula "el navegador dejo algo autocompletado" en la contraseña -- se llena por JS
        // (equivalente a un autofill) y se verifica que enfocarla selecciona todo el texto.
        await page.Locator("#passSeleccionUsuario").FillAsync("valorViejoAutocompletado");
        await page.Locator("#txtSeleccionUsuario").ClickAsync();
        await page.Locator("#passSeleccionUsuario").ClickAsync();

        var seleccionCompleta = await page.EvaluateAsync<bool>(@"() => {
            var el = document.getElementById('passSeleccionUsuario');
            return el === document.activeElement
                && el.selectionStart === 0
                && el.selectionEnd === el.value.length
                && el.value.length > 0;
        }");
        Assert.True(seleccionCompleta, "al enfocar la contraseña con texto preexistente, deberia quedar todo seleccionado");

        await page.CloseAsync();
    }
}
