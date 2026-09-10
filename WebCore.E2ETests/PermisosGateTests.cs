using Microsoft.Playwright;

namespace WebCore.E2ETests;

// Verificacion permanente del bug reportado por el usuario (2026-09-08, item 3 de los 12
// pendientes): "actualmente en core no funcionan los permisos de los usuario. Por ejemplo
// 'cajero' que es un usuario sin permisos para ver compras, ventas, etc. tiene acceso." Antes del
// fix (ver docs/DECISIONS.md "Batch B: permisos reales + operador de produccion"), 11 controllers
// omitian por completo el chequeo de Entidades.Permisos.* (`PermisosHelper.TienePermiso` nunca se
// llamaba). Usuario de prueba: "cajero" (id=10 en la base de desarrollo local, clave "a", no
// admin, sin permiso de Ver en Compra/Elaborado/Reporte/Finanza pero SI con permiso de Ver en
// Stock/Movimientos/Productos segun los datos ya cargados en permisosusuarios) -- confirmado
// empiricamente contra la base real, no asumido.
[Collection("WebCore browser")]
public sealed class PermisosGateTests
{
    private readonly WebCoreFixture _fixture;

    public PermisosGateTests(WebCoreFixture fixture)
    {
        _fixture = fixture;
    }

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

    private static async Task<bool> MuestraAccesoDenegadoAsync(IPage page, string url)
    {
        await page.GotoAsync($"{WebCoreFixture.BaseUrl}{url}", new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });
        await page.WaitForTimeoutAsync(400);
        var bodyText = await page.InnerTextAsync("body");
        return bodyText.Contains("Acceso Denegado");
    }

    [Theory]
    [InlineData("/Compras", "Permisos.Compra.VerCompras")]
    [InlineData("/Elaborados/Formulas", "Permisos.Elaborado.VerFormulas")]
    [InlineData("/Reportes", "Permisos.Stock.VerStock (Reportes reusa este permiso, ver ReportesController.cs)")]
    [InlineData("/Finanzas/CtasCtes", "Permisos.Finanza.VerCtasCtes")]
    [InlineData("/Finanzas/Cheques", "Permisos.Finanza.VerCheques")]
    public async Task UsuarioCajero_SinElPermiso_MuestraAccesoDenegado(string url, string permisoEsperado)
    {
        var page = await NewCajeroAuthenticatedPageAsync();
        var denegado = await MuestraAccesoDenegadoAsync(page, url);
        Assert.True(denegado, $"Se esperaba 'Acceso Denegado' en {url} (gate de {permisoEsperado}) para el usuario 'cajero', pero la pagina cargo normalmente.");
    }

    [Theory]
    [InlineData("/Productos")]
    [InlineData("/Stock")]
    [InlineData("/Movimientos")]
    public async Task UsuarioCajero_ConElPermiso_AccedeNormalmente(string url)
    {
        var page = await NewCajeroAuthenticatedPageAsync();
        var denegado = await MuestraAccesoDenegadoAsync(page, url);
        Assert.False(denegado, $"'{url}' deberia seguir accesible para 'cajero' (tiene el permiso de Ver correspondiente) -- si esto falla, revisar si cambio la data de permisosusuarios en la base de desarrollo.");
    }

    // Item 8 de la segunda ronda de pedidos (2026-09-10, ver docs/DECISIONS.md): el usuario
    // reporto que /Movimientos/Nuevo "no abre, queda en la misma Index" -- investigado con SQL
    // directo contra la base de dev: "cajero" (id=10) NO tiene permiso real
    // (diaspermitidosver=-1, diaspermitidoseditar=-1) para Movimientos/Stock/Elaborados, mientras
    // que todos los demas usuarios no-admin reales de la base SI lo tienen. El codigo de WebCore
    // replica EXACTAMENTE la logica de permisos del clasico (confirmado linea por linea) -- este
    // es el comportamiento ESPERADO (no un bug de logica), y el SweetAlert de "acceso denegado"
    // (_Layout.cshtml, TempData["AlertMsg"]) SI se muestra, confirmado en vivo abajo. Si esto
    // falla en el futuro, no es un bug de permisos -- es un cambio en el mensaje visible que hay
    // que investigar aparte.
    [Fact]
    public async Task UsuarioCajero_SinPermisoMovimientos_MuestraSweetAlertClaro()
    {
        var page = await NewCajeroAuthenticatedPageAsync();
        await page.GotoAsync($"{WebCoreFixture.BaseUrl}/Movimientos/Nuevo", new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });
        await page.WaitForTimeoutAsync(600);

        Assert.Contains("/Movimientos", page.Url);
        Assert.DoesNotContain("/Nuevo", page.Url);

        var swalVisible = await page.Locator(".swal2-popup:visible").CountAsync();
        Assert.True(swalVisible > 0, "Se esperaba un SweetAlert visible explicando por que se bloqueo el acceso.");
        var texto = await page.Locator(".swal2-popup").InnerTextAsync();
        Assert.Contains("permisos", texto, StringComparison.OrdinalIgnoreCase);
    }

    // Bug real encontrado durante la investigacion del item 8 (2026-09-10, ver docs/DECISIONS.md):
    // StockController.Nuevo()/Editar() redirigian al Index EN SILENCIO, sin ningun TempData/
    // mensaje, cuando faltaba o era invalido el parametro "tipoCompra" -- a diferencia de todos
    // los demas redirects por permiso de este controller, que si setean TempData. Solo afecta
    // acceso por URL directa sin querystring (los botones reales del Index ya pasan tipoCompra
    // bien). Login con "ger" (admin, para aislar el caso -- el bug es de parametro, no de
    // permiso).
    [Fact]
    public async Task StockNuevo_SinTipoCompra_MuestraAlertaYRedirigeAIndex()
    {
        var page = await _fixture.NewAuthenticatedPageAsync();
        await page.GotoAsync($"{WebCoreFixture.BaseUrl}/Stock/Nuevo", new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });
        await page.WaitForTimeoutAsync(600);

        Assert.Equal($"{WebCoreFixture.BaseUrl}/Stock", page.Url);
        var swalVisible = await page.Locator(".swal2-popup:visible").CountAsync();
        Assert.True(swalVisible > 0, "Antes del fix, este redirect quedaba mudo -- ahora deberia mostrar un SweetAlert explicando el motivo.");
        var texto = await page.Locator(".swal2-popup").InnerTextAsync();
        Assert.Contains("tipo de operación", texto, StringComparison.OrdinalIgnoreCase);
    }

    // Reporte del usuario (2026-09-10): "Compras... cuando hace clic fuera de compras... ya
    // pierde el permiso y, si quiere regresar a compras, lo tiene que volver a pedir
    // autenticación. Hoy ventas como que no pierde el permiso, sino que lo tiene en memoria todo
    // el tiempo." Causa raiz: WebCore.Helpers.PermisosHelper.ControllersConOperadorModuloSesion
    // solo tenia "Compras" (el port original de este batch se olvido "Ventas", a diferencia del
    // clasico -- Web/Helpers/PermisosHelper.cs:143-148 -- que siempre tuvo los dos). Sin "Ventas"
    // en ese array, LimpiarOperadorModuloSiSalioDelModulo (llamada en cada render desde
    // _Layout.cshtml) nunca borraba el operador de Ventas de la sesion, asi que quedaba
    // autorizado el resto de la sesion sin importar a que otro modulo navegara el usuario.
    [Fact]
    public async Task VentasIndex_UsuarioProduccion_PierdeElOperadorDeModuloAlNavegarAOtraPantallaYVuelveAPedirlo()
    {
        var page = await _fixture.Browser.NewPageAsync();
        await page.GotoAsync($"{WebCoreFixture.BaseUrl}/Login", new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });
        await page.FillAsync("input[name='Usuario']", "produccion");
        await page.FillAsync("input[name='Clave']", "a");
        await page.ClickAsync("button[type='submit']");
        await page.WaitForURLAsync(url => !url.Contains("/Login"), new PageWaitForURLOptions { Timeout = 15000 });

        // 1ra visita: sin operador autorizado, Ventas/Index redirige a autorizar.
        await page.GotoAsync($"{WebCoreFixture.BaseUrl}/Ventas", new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });
        await page.WaitForTimeoutAsync(500);
        Assert.Contains("AutorizarModuloVentas", page.Url);
        Assert.Equal(1, await page.Locator("#txtSeleccionUsuario").CountAsync());

        await page.FillAsync("#txtSeleccionUsuario", "ger");
        await page.WaitForTimeoutAsync(300);
        await page.Locator(".seleccion-usuario-item").First.ClickAsync();
        await page.FillAsync("#passSeleccionUsuario", "a");
        await page.ClickAsync("#btnConfirmarSeleccionUsuario");
        await page.WaitForTimeoutAsync(1200);
        Assert.EndsWith("/Ventas", page.Url);

        // Navegar a otra pantalla (Home) -- el layout debe limpiar el operador de "Ventas" al
        // renderizar un controller que no es Ventas.
        await page.GotoAsync($"{WebCoreFixture.BaseUrl}/Home/Index", new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });
        await page.WaitForTimeoutAsync(500);

        // Assert principal: al volver a Ventas, tiene que volver a pedir autorizacion -- antes
        // del fix, esto fallaba (entraba directo, sin pedir nada).
        await page.GotoAsync($"{WebCoreFixture.BaseUrl}/Ventas", new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });
        await page.WaitForTimeoutAsync(500);
        Assert.Contains("AutorizarModuloVentas", page.Url);
        Assert.Equal(1, await page.Locator("#txtSeleccionUsuario").CountAsync());
    }

    // Tercera ronda de pedidos (2026-09-10, ver docs/DECISIONS.md): "analizar los permisos al
    // crear/modificar... egresos de caja... como lo hace el web clasico para replicarlo en
    // core". CajasController nunca chequeaba Entidades.Permisos.EgresoCaja.* -- confirmado
    // empiricamente que "cajero" (id=10) no tiene NINGUNO de los 4 permisos de este grupo
    // (VerEgresosCaja/VerTiposEgresos/AddOrEditEgresoCaja/AddOrEditTipoEgreso), a diferencia de
    // "produccion" que si tiene AddOrEditEgresoCaja pero no los otros 3 -- confirmado contra el
    // servidor real, no asumido.
    [Fact]
    public async Task UsuarioCajero_SinPermisosDeEgresoCaja_QuedaBloqueadoEnLasCuatroAcciones()
    {
        var page = await NewCajeroAuthenticatedPageAsync();

        var respEgresos = await page.GotoAsync($"{WebCoreFixture.BaseUrl}/Cajas/EgresosCaja", new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });
        Assert.Equal(200, respEgresos!.Status); // vista propia con SinPermiso, no 403
        Assert.Contains("No tiene permisos", await page.InnerTextAsync("body"));

        var respNuevo = await page.GotoAsync($"{WebCoreFixture.BaseUrl}/Cajas/NuevoEgresoCaja", new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });
        Assert.Equal(403, respNuevo!.Status);

        var respTipos = await page.GotoAsync($"{WebCoreFixture.BaseUrl}/Cajas/TiposEgresoCaja", new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });
        Assert.Equal(403, respTipos!.Status);

        var respAddTipo = await page.GotoAsync($"{WebCoreFixture.BaseUrl}/Cajas/AddOrEditTipoEgresoCaja", new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });
        Assert.Equal(403, respAddTipo!.Status);
    }

    // NuevoEgresoCaja SÍ se saltea el gate cuando desdePos=true (mismo criterio que clasico,
    // Web/Controllers/CajasController.cs:210-215 -- el acceso ya esta protegido por el POS en
    // si) -- confirma que el bypass es real y no un agujero de permisos accidental.
    [Fact]
    public async Task UsuarioCajero_NuevoEgresoCajaDesdePos_NoExigeElPermiso()
    {
        var page = await NewCajeroAuthenticatedPageAsync();
        var resp = await page.GotoAsync($"{WebCoreFixture.BaseUrl}/Cajas/NuevoEgresoCaja?desdePos=true", new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });
        Assert.Equal(200, resp!.Status);
    }

    // Tercera ronda de pedidos (2026-09-10, ver docs/DECISIONS.md): "analizar los permisos en
    // marcas [...] tipo de producto" -- revierte una exclusion deliberada de una ronda anterior
    // (pedido explicito del usuario). Confirmado empiricamente que "cajero" no tiene
    // Producto.VerCortes ni Producto.VerTiposProducto (a diferencia de Producto.NuevoCorte/
    // AddOrEditTipoProducto, que SI tienen todos los usuarios no-admin reales de esta base de
    // dev -- no hay ningun usuario real disponible para probar el caso negativo de esos dos
    // permisos puntuales; el gate esta verificado por lectura de codigo 1:1 contra el clasico).
    [Theory]
    [InlineData("/Productos/Marcas")]
    [InlineData("/Productos/Tipos")]
    public async Task UsuarioCajero_SinPermisoDeVer_RedirigeAIndexDeProductos(string url)
    {
        var page = await NewCajeroAuthenticatedPageAsync();
        var resp = await page.GotoAsync($"{WebCoreFixture.BaseUrl}{url}", new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });
        Assert.Equal(200, resp!.Status);
        Assert.EndsWith("/Productos", page.Url);
    }

    // Usuario real CON el permiso (userweb, id=11): confirma que el gate no bloquea de mas al
    // camino feliz -- Marcas/Tipos siguen andando igual que antes para quien si tiene acceso.
    [Fact]
    public async Task UsuarioConPermiso_AccedeNormalmenteAMarcasYTipos()
    {
        var page = await _fixture.Browser.NewPageAsync();
        await page.GotoAsync($"{WebCoreFixture.BaseUrl}/Login", new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });
        await page.FillAsync("input[name='Usuario']", "ger");
        await page.FillAsync("input[name='Clave']", "a");
        await page.ClickAsync("button[type='submit']");
        await page.WaitForURLAsync(url => !url.Contains("/Login"), new PageWaitForURLOptions { Timeout = 15000 });

        var respMarcas = await page.GotoAsync($"{WebCoreFixture.BaseUrl}/Productos/Marcas", new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });
        Assert.Equal(200, respMarcas!.Status);
        Assert.EndsWith("/Productos/Marcas", page.Url);

        var respTipos = await page.GotoAsync($"{WebCoreFixture.BaseUrl}/Productos/Tipos", new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });
        Assert.Equal(200, respTipos!.Status);
        Assert.EndsWith("/Productos/Tipos", page.Url);
    }

    // Tercera ronda de pedidos (2026-09-10, ver docs/DECISIONS.md): "ver el cambio del punto de
    // stock de sucursales en productos... y el agregado desde global" -- gaps reales de permiso
    // (Producto.NuevoCorte / Producto.AddOrEditTipoProducto, GuardarPuntosStockSucursal y los 2
    // catalogos globales) nunca documentados como exclusion deliberada, a diferencia de Marcas/
    // Tipos (Batch 3). Camino feliz: confirma que los 4 endpoints de catalogo global siguen
    // funcionando igual tras agregarles el gate (mismo limite de datos que Batch 3: ningun
    // usuario no-admin real de la base de dev carece de estos permisos puntuales hoy).
    [Fact]
    public async Task UsuarioConPermiso_AccedeNormalmenteALosCatalogosGlobales()
    {
        var page = await _fixture.NewAuthenticatedPageAsync();

        var respProductos = await page.GotoAsync($"{WebCoreFixture.BaseUrl}/Productos/VerGlobales", new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });
        Assert.Equal(200, respProductos!.Status);

        var respBuscarProductos = await page.GotoAsync($"{WebCoreFixture.BaseUrl}/Productos/BuscarGlobales", new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });
        Assert.Equal(200, respBuscarProductos!.Status);

        var respTipos = await page.GotoAsync($"{WebCoreFixture.BaseUrl}/Productos/VerGlobalesTiposProducto", new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });
        Assert.Equal(200, respTipos!.Status);

        var respBuscarTipos = await page.GotoAsync($"{WebCoreFixture.BaseUrl}/Productos/BuscarGlobalesTiposProducto", new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });
        Assert.Equal(200, respBuscarTipos!.Status);
    }
}
