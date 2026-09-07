using Microsoft.Playwright;

namespace WebCore.E2ETests;

// Modal de Factura Electronica (AFIP), 2026-09-06 (retomado -- ver docs/DECISIONS.md). Port
// literal de Web/Views/Ventas/_FacturaElectronica.cshtml + Scripts/app/factura-electronica.js,
// wireado desde el boton "Factura electronica" del post-venta basico (_ModalPostVentaBasico.cshtml)
// via abrirFacturaVentaModal (POS.cshtml). Usa una venta real ya existente en la base local (id
// 1755) en vez de completar una venta nueva por UI, para no acoplar este test al flujo completo
// del POS -- el mismo criterio que otros tests de este proyecto que usan datos de seed conocidos.
[Collection("WebCore browser")]
public sealed class FacturaElectronicaTests
{
    private readonly WebCoreFixture _fixture;

    public FacturaElectronicaTests(WebCoreFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task VentasPOS_AbreModalFacturaDesdePostVenta_YCierraSinFacturar()
    {
        var page = await _fixture.NewAuthenticatedPageAsync();
        var errors = new List<string>();
        page.PageError += (_, msg) => errors.Add(msg);

        await page.GotoAsync($"{WebCoreFixture.BaseUrl}/Ventas/POS", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        await page.WaitForTimeoutAsync(500);

        // Simula el post-venta de una venta real ya existente -- window.mostrarModalPostVenta ya
        // esta expuesto globalmente por POS.cshtml (lo usa el flujo real tras FinalizarVenta).
        await page.EvaluateAsync("window.mostrarModalPostVenta(1755)");
        await page.WaitForTimeoutAsync(300);

        Assert.Equal(1, await page.Locator("#modalPostVentaBasico.show").CountAsync());
        Assert.True(await page.Locator("#btnPvbFactura").IsVisibleAsync());

        await page.ClickAsync("#btnPvbFactura");
        await page.WaitForTimeoutAsync(1500);

        Assert.Equal(1, await page.Locator("#modalFacturaElectronica.show").CountAsync());
        Assert.False(string.IsNullOrWhiteSpace(await page.Locator("#feStickyCliente").InnerTextAsync()));
        Assert.NotEqual("-", (await page.Locator("#feStickyCliente").InnerTextAsync()).Trim());
        Assert.True(await page.Locator("#btnCerrarVentaSinFacturar").IsVisibleAsync());

        // Cerrar sin facturar es una operacion segura (no toca AFIP) -- confirma el SweetAlert y
        // verifica que vuelve al post-venta basico (mismo comportamiento que el clasico).
        await page.ClickAsync("#btnCerrarVentaSinFacturar");
        await page.WaitForTimeoutAsync(400);
        await page.ClickAsync(".swal2-confirm");
        await page.WaitForTimeoutAsync(1200);

        Assert.Equal(0, await page.Locator("#modalFacturaElectronica.show").CountAsync());
        Assert.Equal(1, await page.Locator("#modalPostVentaBasico.show").CountAsync());
        Assert.Empty(errors);

        await page.CloseAsync();
    }
}
