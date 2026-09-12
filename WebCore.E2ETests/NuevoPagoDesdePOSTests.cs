using Microsoft.Playwright;

namespace WebCore.E2ETests;

// Batch 10 de la quinta ronda de pedidos, 2026-09-10 (ver docs/DECISIONS.md): "Nuevo pago" en POS
// -- roto hoy (el boton "Agregar Pago / Cobro" navegaba de pagina completa y sacaba al usuario de
// /Ventas/POS), pedido explicito de portar el diseno del web clasico: comenzar haciendo foco en
// la opcion Pago/Cobro, atajos de teclado 1/2, modal nuevo (#modalPagoPOS) apilado sobre el modal
// de Finanzas ya abierto, y que el guardado exitoso no navegue afuera del POS (no debe perderse la
// venta en curso).
//
// Causa raiz confirmada por lectura de codigo antes de implementar (5 puntos, ver plan): boton
// "Agregar Pago/Cobro" mal ubicado dentro de @if(puedeExportarCuentaCorriente) en CtaCtePersona.
// cshtml (acoplado a un permiso de exportacion que no tiene nada que ver), sin atributos
// data-pos-ajax/data-pos-title; #modalPagoPOS no existia; AddOrEditPago.cshtml tenia el gate
// "elegi primero Pago/Cobro" recortado a proposito en una ronda anterior; el backend
// (AddOrEditPagoPost) ya estaba 100% listo para desdePos=true (devuelve cerrarModalPago:true) pero
// el JS nunca mandaba el parametro desdePos en el POST.
[Collection("WebCore browser")]
public sealed class NuevoPagoDesdePOSTests
{
    private readonly WebCoreFixture _fixture;

    public NuevoPagoDesdePOSTests(WebCoreFixture fixture)
    {
        _fixture = fixture;
    }

    private static async Task IrAlPOSyAbrirCtaCtePersonaAsync(IPage page)
    {
        await page.GotoAsync($"{WebCoreFixture.BaseUrl}/Ventas/POS", new PageGotoOptions { WaitUntil = WaitUntilState.Load, Timeout = 20000 });
        if (await page.Locator("#modalSucursales.show").CountAsync() > 0)
            await page.Locator("#lnkContinuarSucursalActual").ClickAsync();
        await page.WaitForTimeoutAsync(500);

        await page.Keyboard.PressAsync("F2");
        await page.Locator("#modalFinanzasPOS.show").WaitForAsync(new LocatorWaitForOptions { Timeout = 10000 });
        await page.WaitForTimeoutAsync(500);

        var fila = page.Locator("tr[onclick='irACtaCte(this)']").First;
        int filas = await fila.CountAsync();
        Assert.True(filas > 0, "hace falta al menos una persona con cuenta corriente real en la base de dev para este test");

        await fila.ClickAsync();
        await page.WaitForTimeoutAsync(600);

        Assert.Equal(1, await page.Locator("#modalFinanzasPOS #btnAgregarPagoCtaCte").CountAsync());
    }

    [Fact]
    public async Task DesdePOS_AbrirYGuardarUnPago_NoNavegaAfueraYNoPierdeLaVentaEnCurso()
    {
        var page = await _fixture.NewAuthenticatedPageAsync();
        await PreciosSeedHelper.FijarPreciosCanonicosAsync(page);

        await page.GotoAsync($"{WebCoreFixture.BaseUrl}/Ventas/POS", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        if (await page.Locator("#modalSucursales.show").CountAsync() > 0)
            await page.Locator("#lnkContinuarSucursalActual").ClickAsync();
        await page.WaitForTimeoutAsync(500);

        // Venta en curso real antes de tocar Finanzas -- para confirmar despues que sigue intacta.
        await page.FillAsync("#inputCodigo", "1");
        await page.Keyboard.PressAsync("Enter");
        await page.WaitForTimeoutAsync(600);
        await page.FillAsync("#inputCantidad", "1");
        await page.Keyboard.PressAsync("Enter");
        await page.WaitForTimeoutAsync(600);
        int lineasAntes = await page.Locator("#tablaItems tr.fila-item").CountAsync();
        Assert.True(lineasAntes > 0, "no se pudo cargar una linea real en el carrito antes del test");

        await page.Keyboard.PressAsync("F2");
        await page.Locator("#modalFinanzasPOS.show").WaitForAsync(new LocatorWaitForOptions { Timeout = 10000 });
        await page.WaitForTimeoutAsync(500);

        var fila = page.Locator("tr[onclick='irACtaCte(this)']").First;
        Assert.True(await fila.CountAsync() > 0, "hace falta al menos una persona con cuenta corriente real en la base de dev para este test");
        await fila.ClickAsync();
        await page.WaitForTimeoutAsync(600);

        var btnPago = page.Locator("#modalFinanzasPOS #btnAgregarPagoCtaCte");
        Assert.Equal(1, await btnPago.CountAsync());
        Assert.Equal("true", await btnPago.GetAttributeAsync("data-pos-ajax"));

        await btnPago.ClickAsync();
        await page.WaitForTimeoutAsync(600);

        // No debe haber navegado de pagina completa.
        Assert.Contains("/Ventas/POS", page.Url);
        Assert.Equal(1, await page.Locator("#modalPagoPOS.show").CountAsync());
        Assert.Equal(1, await page.Locator("#modalFinanzasPOS.show").CountAsync());

        // Atajo "2" (Cobro) -- el foco inicial ya cae en un boton (no editable), no hace falta
        // clickear afuera primero (clickear el body podria pegarle al backdrop de #modalPagoPOS y
        // cerrarlo -- confirmado con un diagnostico real durante la implementacion de este test).
        await page.Keyboard.PressAsync("2");
        await page.WaitForTimeoutAsync(300);
        Assert.Equal("false", await page.EvalOnSelectorAsync<string>("#AProveedor", "el => el.value"));
        Assert.False(await page.IsDisabledAsync("#SucursalId"), "tras elegir la operacion, Sucursal deberia habilitarse");

        await page.FillAsync("#txtImporte", "10");
        await page.ClickAsync("#modalPagoPOS #btnGuardarPago");
        await page.Locator("#modalPagoPOS.show").WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Hidden, Timeout = 15000 });

        // Sigue en POS, el modal de Finanzas sigue abierto (recargado con la cuenta corriente), y
        // la venta en curso no se perdio.
        Assert.Contains("/Ventas/POS", page.Url);
        Assert.Equal(1, await page.Locator("#modalFinanzasPOS.show").CountAsync());
        Assert.Equal(lineasAntes, await page.Locator("#tablaItems tr.fila-item").CountAsync());

        await page.CloseAsync();
    }

    [Fact]
    public async Task DesdePOS_AlAbrirElPago_ElFocoInicialQuedaEnElBotonDePago()
    {
        var page = await _fixture.NewAuthenticatedPageAsync();
        await IrAlPOSyAbrirCtaCtePersonaAsync(page);

        await page.Locator("#modalFinanzasPOS #btnAgregarPagoCtaCte").ClickAsync();
        await page.Locator("#modalPagoPOS.show").WaitForAsync(new LocatorWaitForOptions { Timeout = 10000 });
        await page.WaitForTimeoutAsync(500);

        var focoValor = await page.EvaluateAsync<string>("() => document.activeElement && document.activeElement.getAttribute('data-operacion-valor')");
        Assert.Equal("true", focoValor);

        await page.CloseAsync();
    }

    [Fact]
    public async Task NoDesdePOS_AccesoDirectoAAddOrEditPago_SigueFuncionandoIgualQueAntes()
    {
        // Regresion (fuera de POS, navegacion normal): el gate "elegi primero" no debe romper el
        // flujo clasico de alta/edicion de un pago de escritorio.
        var page = await _fixture.NewAuthenticatedPageAsync();

        await page.GotoAsync($"{WebCoreFixture.BaseUrl}/Finanzas/CtasCtes", new PageGotoOptions { WaitUntil = WaitUntilState.Load, Timeout = 20000 });
        await page.WaitForTimeoutAsync(500);

        var fila = page.Locator("tr[onclick='irACtaCte(this)']").First;
        Assert.True(await fila.CountAsync() > 0, "hace falta al menos una persona con cuenta corriente real en la base de dev para este test");
        await fila.ClickAsync();
        await page.WaitForURLAsync(u => u.Contains("/Finanzas/CtaCtePersona"), new PageWaitForURLOptions { Timeout = 15000 });
        await page.WaitForTimeoutAsync(400);

        var btnPago = page.Locator("#btnAgregarPagoCtaCte");
        Assert.Equal(1, await btnPago.CountAsync());
        Assert.Equal("false", await btnPago.GetAttributeAsync("data-pos-ajax"));

        await btnPago.ClickAsync();
        await page.WaitForURLAsync(u => u.Contains("/Finanzas/AddOrEditPago"), new PageWaitForURLOptions { Timeout = 15000 });
        await page.WaitForTimeoutAsync(400);

        // Gate "elegi primero" tambien aplica navegando derecho (no solo desde POS).
        Assert.True(await page.IsDisabledAsync("#SucursalId"));
        await page.ClickAsync(".pago-operacion-opcion[data-operacion-valor='false']");
        await page.WaitForTimeoutAsync(300);
        Assert.False(await page.IsDisabledAsync("#SucursalId"));

        await page.FillAsync("#txtImporte", "5");
        await page.ClickAsync("#btnGuardarPago");
        await page.WaitForURLAsync(u => u.Contains("/Finanzas/CtaCtePersona"), new PageWaitForURLOptions { Timeout = 15000 });

        await page.CloseAsync();
    }

    [Fact]
    public async Task SubmitSinElegirOperacion_QuedaBloqueadoConAvisoReal()
    {
        var page = await _fixture.NewAuthenticatedPageAsync();
        await page.GotoAsync($"{WebCoreFixture.BaseUrl}/Finanzas/AddOrEditPago?idPersona=1", new PageGotoOptions { WaitUntil = WaitUntilState.Load, Timeout = 20000 });
        await page.WaitForTimeoutAsync(400);

        Assert.True(await page.IsDisabledAsync("#SucursalId"));
        await page.ClickAsync("#btnGuardarPago");
        await page.WaitForTimeoutAsync(400);

        Assert.Equal(1, await page.Locator(".swal2-popup:visible").CountAsync());
        Assert.Contains("primero", (await page.Locator(".swal2-popup").InnerTextAsync()).ToLowerInvariant());

        await page.CloseAsync();
    }
}
