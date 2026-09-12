using Microsoft.Playwright;

namespace WebCore.E2ETests;

// Batch 6 de la quinta ronda de pedidos, 2026-09-10, ver docs/DECISIONS.md "Batch 6: botón
// Actividades en el pie de Cerrar Caja". Antes, el único punto de entrada a Actividades era el
// botón de la fila en la tabla de cajas abiertas, fuera del modal de Cerrar Caja -- el usuario
// tenía que cerrarlo (o no poder verlo) para llegar ahí.
[Collection("WebCore browser")]
public sealed class CerrarCajaActividadesTests
{
    private readonly WebCoreFixture _fixture;

    public CerrarCajaActividadesTests(WebCoreFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task ModalCerrarCaja_BotonActividades_AbreArribaSinRomperElModalDeFondo()
    {
        var page = await _fixture.NewAuthenticatedPageAsync();
        var errors = new List<string>();
        page.PageError += (_, msg) => errors.Add(msg);

        await page.GotoAsync($"{WebCoreFixture.BaseUrl}/Cajas/CajasAbiertas", new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });
        await page.WaitForTimeoutAsync(600);

        var filas = await page.Locator("#tablaCajas tbody tr").CountAsync();
        Assert.True(filas > 0, "hace falta al menos una caja abierta real en la base de dev para este test");

        await page.Locator("#tablaCajas tbody tr").First.Locator("button.btn-danger").ClickAsync();
        await page.WaitForTimeoutAsync(600);
        Assert.Equal(1, await page.Locator("#modalCerrarCaja.show").CountAsync());

        // Boton nuevo, en el extremo izquierdo del footer.
        var btnActividades = page.Locator("#modalCerrarCaja .modal-footer button:has-text('Actividades')");
        Assert.Equal(1, await btnActividades.CountAsync());

        await btnActividades.ClickAsync();
        await page.WaitForTimeoutAsync(1000);

        // Ambos modales quedan abiertos a la vez (Actividades arriba, Cerrar Caja detras).
        Assert.Equal(1, await page.Locator("#modalActividadesCaja.show").CountAsync());
        Assert.Equal(1, await page.Locator("#modalCerrarCaja.show").CountAsync());
        Assert.Equal(2, await page.Locator(".modal.show").CountAsync());

        var zActividades = await page.Locator("#modalActividadesCaja").EvaluateAsync<string>("el => getComputedStyle(el).zIndex");
        var zCerrarCaja = await page.Locator("#modalCerrarCaja").EvaluateAsync<string>("el => getComputedStyle(el).zIndex");
        Assert.True(int.Parse(zActividades) > int.Parse(zCerrarCaja), "Actividades debería quedar arriba (mayor z-index) que Cerrar Caja.");

        // Cerrar Actividades no debe romper el modal de fondo: sigue abierto, con backdrop, y
        // el campo de arqueo sigue interactuable (riesgo real flageado en el plan: Bootstrap 4
        // no soporta oficialmente modales anidados). Cierre explicito por boton (no Escape, para
        // no depender de que el foco del teclado este realmente sobre el modal de arriba).
        await page.ClickAsync("#modalActividadesCaja .close");
        await page.WaitForTimeoutAsync(600);
        Assert.Equal(0, await page.Locator("#modalActividadesCaja.show").CountAsync());
        Assert.Equal(1, await page.Locator("#modalCerrarCaja.show").CountAsync());
        Assert.True(await page.Locator("body").EvaluateAsync<bool>("el => el.classList.contains('modal-open')"));
        Assert.True(await page.Locator(".modal-backdrop").CountAsync() > 0);

        await page.FillAsync("#CajaCierre", "100");
        Assert.Equal("100", await page.InputValueAsync("#CajaCierre"));

        Assert.Empty(errors);
        await page.CloseAsync();
    }
}
