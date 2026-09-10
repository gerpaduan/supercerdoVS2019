using Microsoft.Playwright;

namespace WebCore.E2ETests;

// Item 9 de la segunda ronda de pedidos (2026-09-10, ver docs/DECISIONS.md): "con user
// producción logueado pide la autenticación del usuario operador en cada venta... solo debe
// pedirla una vez y permanece mientras la vista pos quede abierta." Causa raíz exacta: el
// listener global 'pagehide' (Ventas/POS.cshtml) manda un sendBeacon hacia CerrarOperadorPOS
// (que borra el operador de sesión) SIEMPRE que window.PosNavegandoInternoPOS no esté en true --
// y pvbContinuarNuevaVenta() (el botón "Nueva venta" del modal post-venta, que se aprieta
// después de CADA venta) hacía window.location.reload() sin marcar ese flag antes, a diferencia
// de recargarConOperadorAutorizado() (que sí lo hace, patrón ya documentado en el propio código).
// Fix: 2 líneas, agregar el flag antes de cada reload que vuelve a la misma instancia de POS.
[Collection("WebCore browser")]
public sealed class OperadorPOSPersisteEntreVentasTests
{
    private readonly WebCoreFixture _fixture;

    public OperadorPOSPersisteEntreVentasTests(WebCoreFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task VentasPOS_UsuarioProduccion_AutorizaOperadorUnaVezYPersisteTrasVentaCompleta()
    {
        var page = await _fixture.Browser.NewPageAsync();
        await page.GotoAsync($"{WebCoreFixture.BaseUrl}/Login", new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });
        await page.FillAsync("input[name='Usuario']", "produccion");
        await page.FillAsync("input[name='Clave']", "a");
        await page.ClickAsync("button[type='submit']");
        await page.WaitForURLAsync(url => !url.Contains("/Login"), new PageWaitForURLOptions { Timeout = 15000 });

        // Intercepta el POST real a FinalizarVenta -- responde ok sin crear una venta real en la
        // base compartida de desarrollo.
        await page.RouteAsync("**/Ventas/FinalizarVenta", async route =>
        {
            await route.FulfillAsync(new RouteFulfillOptions
            {
                Status = 200,
                ContentType = "application/json",
                Body = "{\"ok\":true,\"idVenta\":999999}"
            });
        });

        await page.GotoAsync($"{WebCoreFixture.BaseUrl}/Ventas/POS", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        await page.WaitForTimeoutAsync(600);

        Assert.Equal(1, await page.Locator("#modalSeleccionUsuario.show").CountAsync());

        await page.FillAsync("#txtSeleccionUsuario", "ger");
        await page.FillAsync("#passSeleccionUsuario", "a");
        await page.ClickAsync("#btnConfirmarSeleccionUsuario");
        await page.WaitForTimeoutAsync(2000);

        Assert.Equal(0, await page.Locator("#modalSeleccionUsuario.show").CountAsync());

        // Venta completa real (código + cantidad + forma de pago) -- este es el escenario exacto
        // que reproducía el bug: antes del fix, el reload post-venta volvía a pedir el operador.
        await page.FillAsync("#inputCodigo", "1");
        await page.Keyboard.PressAsync("Enter");
        await page.WaitForTimeoutAsync(600);
        await page.FillAsync("#inputCantidad", "1");
        await page.Keyboard.PressAsync("Enter");
        await page.WaitForTimeoutAsync(600);
        await page.Keyboard.PressAsync("End");
        await page.WaitForTimeoutAsync(600);

        Assert.Equal(1, await page.Locator("#modalFormaPago.show").CountAsync());
        await page.ClickAsync("#modalFormaPago .btn-forma-pago[data-tipo='Efectivo']");
        await page.WaitForTimeoutAsync(1500);

        Assert.Equal(1, await page.Locator("#modalPostVentaBasico.show").CountAsync());
        await page.ClickAsync("#btnPvbContinuar");
        await page.WaitForTimeoutAsync(2000);

        // Assert principal: tras "Nueva venta", el modal de operador NO debe reaparecer -- antes
        // del fix, esto fallaba (el operador se limpiaba en cada venta).
        Assert.Equal(0, await page.Locator("#modalSeleccionUsuario.show").CountAsync());
    }

    // Item nuevo reportado 2026-09-10: "en ventas al dar acceso, no se termina al cliquear
    // afuera. Basate en Compras, ahi funciona bien." Causa raíz: SeleccionUsuario.abrir() (para
    // la autorización OBLIGATORIA inicial, esInicial=true) mostraba el modal con el backdrop
    // default de Bootstrap -- un click afuera (accidental o no) cerraba el modal SIN resolver,
    // y el listener 'hidden.bs.modal' de Ventas/POS.cshtml interpretaba eso como cancelado y
    // redirigía a Home, perdiendo lo que el usuario ya había tipeado. Compras no tenía este
    // problema porque su equivalente (AutorizarModuloCompras) es una pantalla completa, sin
    // backdrop que se pueda clickear por afuera. Fix: seleccion-usuario.js ahora acepta
    // opciones.obligatorio -- con eso en true, el modal usa backdrop:'static' + keyboard:false
    // (mismo idiom ya probado en bootstrap4-compat.js), así que solo los botones explícitos
    // "Cancelar"/"Confirmar" pueden cerrarlo.
    [Fact]
    public async Task VentasPOS_UsuarioProduccion_ClickAfueraDelModalObligatorioNoAbortaLaAutorizacion()
    {
        var page = await _fixture.Browser.NewPageAsync();
        await page.GotoAsync($"{WebCoreFixture.BaseUrl}/Login", new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });
        await page.FillAsync("input[name='Usuario']", "produccion");
        await page.FillAsync("input[name='Clave']", "a");
        await page.ClickAsync("button[type='submit']");
        await page.WaitForURLAsync(url => !url.Contains("/Login"), new PageWaitForURLOptions { Timeout = 15000 });

        await page.GotoAsync($"{WebCoreFixture.BaseUrl}/Ventas/POS", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        await page.WaitForTimeoutAsync(600);
        Assert.Equal(1, await page.Locator("#modalSeleccionUsuario.show").CountAsync());

        await page.FillAsync("#txtSeleccionUsuario", "ger");
        await page.WaitForTimeoutAsync(300);
        await page.Locator(".seleccion-usuario-item").First.ClickAsync();
        await page.FillAsync("#passSeleccionUsuario", "a");

        // Click en la esquina del viewport -- fuera del modal-dialog, sobre el backdrop.
        await page.Mouse.ClickAsync(5, 5);
        await page.WaitForTimeoutAsync(600);

        Assert.DoesNotContain("/Home/Index", page.Url);
        Assert.Equal(1, await page.Locator("#modalSeleccionUsuario.show").CountAsync());
        Assert.Equal("a", await page.InputValueAsync("#passSeleccionUsuario"));

        // La autorizacion sigue completando normalmente con el boton explicito.
        await page.ClickAsync("#btnConfirmarSeleccionUsuario");
        await page.WaitForTimeoutAsync(1500);
        Assert.Equal(0, await page.Locator("#modalSeleccionUsuario.show").CountAsync());
        Assert.Contains("/Ventas/POS", page.Url);
    }
}
