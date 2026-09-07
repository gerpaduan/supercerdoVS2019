using Microsoft.Playwright;

namespace WebCore.E2ETests;

// Flujo de foco/teclado del input de descuento global en Forma de Pago, 2026-09-07 (pedido
// explicito del usuario -- ver docs/DECISIONS.md): mientras el input tiene foco, los atajos
// numericos 1-6 de forma de pago deben quedar inhabilitados (para no finalizar la venta a mitad
// de tipeo un "10%"); Enter confirma y deshabilita el input hasta que "/" lo vuelva a habilitar;
// recien ahi los atajos numericos vuelven a funcionar.
[Collection("WebCore browser")]
public sealed class DescuentoInputFocoTests
{
    private readonly WebCoreFixture _fixture;

    public DescuentoInputFocoTests(WebCoreFixture fixture)
    {
        _fixture = fixture;
    }

    private static async Task CargarCarritoYAbrirFormaPagoAsync(IPage page)
    {
        await page.GotoAsync($"{WebCoreFixture.BaseUrl}/Ventas/POS", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        await page.WaitForTimeoutAsync(500);

        await page.FillAsync("#inputCodigo", "1");
        await page.Keyboard.PressAsync("Enter");
        await page.WaitForTimeoutAsync(600);
        await page.FillAsync("#inputCantidad", "1");
        await page.Keyboard.PressAsync("Enter");
        await page.WaitForTimeoutAsync(600);

        await page.Keyboard.PressAsync("End");
        await page.WaitForTimeoutAsync(800);
        await page.Locator("#modalFormaPago.show").WaitForAsync(new LocatorWaitForOptions { Timeout = 10000 });
    }

    [Fact]
    public async Task ConFocoEnInput_AtajosNumericosNoFinalizanLaVenta_YSeInvierteElEnfasisVisual()
    {
        var page = await _fixture.NewAuthenticatedPageAsync();
        var payloadCapturado = false;
        await page.RouteAsync("**/Ventas/FinalizarVenta", async route =>
        {
            payloadCapturado = true;
            await route.FulfillAsync(new RouteFulfillOptions { Status = 200, ContentType = "application/json", Body = "{\"ok\":false}" });
        });

        await CargarCarritoYAbrirFormaPagoAsync(page);

        await page.Keyboard.PressAsync("/");
        await page.WaitForTimeoutAsync(300);
        Assert.True(await page.Locator("#modalFormaPago").EvaluateAsync<bool>("el => el.classList.contains('pos-descuento-input-activo')"));

        // Tipear "10" no debe disparar los atajos "1"=Efectivo/"0" a mitad de tipeo.
        await page.Keyboard.TypeAsync("10");
        await page.WaitForTimeoutAsync(300);
        Assert.False(payloadCapturado, "los atajos numericos finalizaron la venta mientras se tipeaba el descuento");
        Assert.Equal("10", await page.Locator("#txtPorcentajeTotalVenta").InputValueAsync());

        await page.CloseAsync();
    }

    [Fact]
    public async Task Enter_ConfirmaYDeshabilitaElInput_BarraLoVuelveAHabilitar_YLuegoElAtajoNumericoFunciona()
    {
        var page = await _fixture.NewAuthenticatedPageAsync();
        string? payloadCapturado = null;
        await page.RouteAsync("**/Ventas/FinalizarVenta", async route =>
        {
            payloadCapturado = route.Request.PostData;
            await route.FulfillAsync(new RouteFulfillOptions { Status = 200, ContentType = "application/json", Body = "{\"ok\":false}" });
        });

        await CargarCarritoYAbrirFormaPagoAsync(page);

        await page.Keyboard.PressAsync("/");
        await page.WaitForTimeoutAsync(300);
        await page.Keyboard.TypeAsync("10");
        await page.Keyboard.PressAsync("Enter");
        await page.WaitForTimeoutAsync(400);

        Assert.True(await page.Locator("#txtPorcentajeTotalVenta").IsDisabledAsync());
        Assert.False(await page.Locator("#modalFormaPago").EvaluateAsync<bool>("el => el.classList.contains('pos-descuento-input-activo')"));
        Assert.True(await page.Locator(".swal2-toast").IsVisibleAsync());

        // El atajo numerico ahora SI debe funcionar (el foco ya no esta en el input).
        await page.Keyboard.PressAsync("1");
        await page.WaitForTimeoutAsync(600);
        Assert.NotNull(payloadCapturado);

        await page.CloseAsync();
    }

    [Fact]
    public async Task Barra_ReabreElInputYaConfirmado_SinPerderElDescuentoCargado()
    {
        var page = await _fixture.NewAuthenticatedPageAsync();
        await page.RouteAsync("**/Ventas/FinalizarVenta", async route =>
        {
            await route.FulfillAsync(new RouteFulfillOptions { Status = 200, ContentType = "application/json", Body = "{\"ok\":false}" });
        });

        await CargarCarritoYAbrirFormaPagoAsync(page);

        await page.Keyboard.PressAsync("/");
        await page.WaitForTimeoutAsync(300);
        await page.Keyboard.TypeAsync("15");
        await page.Keyboard.PressAsync("Enter");
        await page.WaitForTimeoutAsync(2500); // deja que el toast desaparezca

        await page.Keyboard.PressAsync("/");
        await page.WaitForTimeoutAsync(300);

        Assert.False(await page.Locator("#txtPorcentajeTotalVenta").IsDisabledAsync());
        Assert.Equal("15", await page.Locator("#txtPorcentajeTotalVenta").InputValueAsync());
        Assert.True(await page.Locator("#modalFormaPago").EvaluateAsync<bool>("el => el.classList.contains('pos-descuento-input-activo')"));

        await page.CloseAsync();
    }
}
