using Microsoft.Playwright;

namespace WebCore.E2ETests;

// Pedido real (2026-09-15, ver docs/DECISIONS.md): en el modal "Productos no cargados" de
// Stock/Editar (Cierre de Stock), al agregar productos con "Agregar con stock actual" o
// "Agregar como sin stock", no habia ningun aviso visible del exito -- el mensaje de
// showFeedback queda detras del modal, que sigue abierto. Fix: SweetAlert de exito (mismo
// patron ya usado en este archivo para "Pesaje vinculado correctamente").
[Collection("WebCore browser")]
public sealed class StockProductosNoCargadosSweetAlertTests
{
    private readonly WebCoreFixture _fixture;

    public StockProductosNoCargadosSweetAlertTests(WebCoreFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task AgregarConStockActual_MuestraSweetAlertDeExito()
    {
        var page = await _fixture.NewAuthenticatedPageAsync();

        await page.GotoAsync($"{WebCoreFixture.BaseUrl}/Stock/Nuevo?tipoCompra=Cierre Stock", new PageGotoOptions
        {
            WaitUntil = WaitUntilState.NetworkIdle,
            Timeout = 20000
        });

        await page.ClickAsync("#btnProductosNoCargados");
        await page.Locator("#modalProductosNoCargadosStock.show").WaitForAsync(new LocatorWaitForOptions { Timeout = 10000 });
        await page.WaitForTimeoutAsync(500);

        var filas = await page.Locator("#tablaNoCargadosStock tbody tr").CountAsync();
        Assert.True(filas > 0, "hace falta al menos un producto real sin contar en la base de dev para este test");

        await page.Locator("#tablaNoCargadosStock tbody .js-no-cargado-check").First.CheckAsync();
        await page.ClickAsync("#btnAgregarNoCargadosStockActual");

        // El bug real: sin el fix, no aparecia ningun SweetAlert -- el modal seguia abierto y el
        // unico aviso (showFeedback) quedaba oculto detras.
        var swal = page.Locator(".swal2-popup.swal2-icon-success");
        await swal.WaitForAsync(new LocatorWaitForOptions { Timeout = 5000 });
        Assert.Contains("Se agregaron", await swal.Locator(".swal2-title").InnerTextAsync());

        await page.CloseAsync();
    }

    [Fact]
    public async Task AgregarSinStock_MuestraSweetAlertDeExito()
    {
        var page = await _fixture.NewAuthenticatedPageAsync();

        await page.GotoAsync($"{WebCoreFixture.BaseUrl}/Stock/Nuevo?tipoCompra=Cierre Stock", new PageGotoOptions
        {
            WaitUntil = WaitUntilState.NetworkIdle,
            Timeout = 20000
        });

        await page.ClickAsync("#btnProductosNoCargados");
        await page.Locator("#modalProductosNoCargadosStock.show").WaitForAsync(new LocatorWaitForOptions { Timeout = 10000 });
        await page.WaitForTimeoutAsync(500);

        var filas = await page.Locator("#tablaNoCargadosStock tbody tr").CountAsync();
        Assert.True(filas > 0, "hace falta al menos un producto real sin contar en la base de dev para este test");

        await page.Locator("#tablaNoCargadosStock tbody .js-no-cargado-check").First.CheckAsync();
        await page.ClickAsync("#btnAgregarNoCargadosSinStock");

        var swal = page.Locator(".swal2-popup.swal2-icon-success");
        await swal.WaitForAsync(new LocatorWaitForOptions { Timeout = 5000 });
        Assert.Contains("Se agregaron", await swal.Locator(".swal2-title").InnerTextAsync());

        await page.CloseAsync();
    }
}
