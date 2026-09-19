using System.Security.Cryptography;
using System.Text;
using Microsoft.Playwright;

namespace WebCore.E2ETests;

// Login solo desde dispositivos seguros (2026-09-19, ver docs/DECISIONS.md "Dispositivo seguro
// como condicion de login"). Cubre la regla real de punta a punta con el switch de empresa de
// "Mi Empresa": un no-admin ("cajero") desde un navegador nuevo cae en la pantalla de dispositivo
// no autorizado, el admin ("ger") entra directo, un dispositivo cargado por el admin (por ID de
// hardware o por el hash de la cookie del navegador) entra, uno bloqueado no, y la auditoria de
// accesos muestra el dispositivo. El envio real del codigo por mail no se prueba aca (no hay forma
// de leer el mail desde Playwright): la logica del codigo esta cubierta por tests unitarios
// (Negocio.Tests/VerificacionDispositivoStoreTests) y la prueba con SMTP real es manual.
// Todos los tests dejan el switch de empresa APAGADO y borran lo que crean (misma base de dev
// compartida que el resto de la suite, que corre en serie dentro de la coleccion).
[Collection("WebCore browser")]
public sealed class DispositivoSeguroLoginTests
{
    private readonly WebCoreFixture _fixture;

    public DispositivoSeguroLoginTests(WebCoreFixture fixture)
    {
        _fixture = fixture;
    }

    private static string SerieDeToken(string token)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(token.Trim()));
        return "web:" + Convert.ToHexString(hash).ToLowerInvariant();
    }

    private async Task<IPage> NuevaPaginaAsync(IBrowserContext contexto)
    {
        return await contexto.NewPageAsync();
    }

    // Click que dispara una navegacion (POST + redirect): espera a que termine de cargar la pagina destino.
    // (WaitForLoadState solo, justo despues del click, vuelve enseguida por la pagina vieja ya cargada.)
    private static async Task ClickYEsperarAsync(IPage page, string selector)
    {
        await page.RunAndWaitForNavigationAsync(() => page.ClickAsync(selector), new PageRunAndWaitForNavigationOptions { WaitUntil = WaitUntilState.Load });
    }

    private async Task LoginAsync(IPage page, string usuario, string clave = "a")
    {
        await page.GotoAsync($"{WebCoreFixture.BaseUrl}/Login");
        await page.FillAsync("input[name='Usuario']", usuario);
        await page.FillAsync("input[name='Clave']", clave);
        await ClickYEsperarAsync(page, "button[type='submit']");
    }

    private async Task PonerSwitchEmpresaAsync(IPage adminPage, bool activo)
    {
        await adminPage.GotoAsync($"{WebCoreFixture.BaseUrl}/Empresa", new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });
        await adminPage.ClickAsync("#btnHabilitarEdicionEmpresa");
        await adminPage.SetCheckedAsync("#ExigirDispositivoSeguro", activo);
        await ClickYEsperarAsync(adminPage, "#btnGuardarEmpresa");
        await adminPage.GotoAsync($"{WebCoreFixture.BaseUrl}/Empresa", new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });
        Assert.Equal(activo, await adminPage.IsCheckedAsync("#ExigirDispositivoSeguro"));
    }

    private async Task AgregarDispositivoAsync(IPage adminPage, string serie, string descripcion)
    {
        await adminPage.GotoAsync($"{WebCoreFixture.BaseUrl}/DispositivosSeguros", new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });
        await adminPage.FillAsync("#NumeroSerie", serie);
        await adminPage.FillAsync("#Descripcion", descripcion);
        await ClickYEsperarAsync(adminPage, "#formAgregarDispositivo button[type='submit']");
    }

    private async Task EliminarDispositivoAsync(IPage adminPage, string descripcion)
    {
        await adminPage.GotoAsync($"{WebCoreFixture.BaseUrl}/DispositivosSeguros", new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });
        var fila = adminPage.Locator("table tbody tr", new PageLocatorOptions { HasText = descripcion });
        if (await fila.CountAsync() == 0) return;

        adminPage.Dialog += (_, d) => _ = d.AcceptAsync();
        await adminPage.RunAndWaitForNavigationAsync(() => fila.First.Locator("form[action*='Eliminar'] button[type='submit']").ClickAsync());
    }

    [Fact]
    public async Task ConSwitchDeEmpresa_NoAdminEnNavegadorNuevo_CaeEnDispositivoNoAutorizado_YAdminEntraDirecto()
    {
        var admin = await _fixture.NewAuthenticatedPageAsync();
        try
        {
            await PonerSwitchEmpresaAsync(admin, true);

            var ctxCajero = await _fixture.Browser.NewContextAsync();
            var cajero = await NuevaPaginaAsync(ctxCajero);
            await LoginAsync(cajero, "cajero");

            Assert.Contains("/Login/DispositivoNoAutorizado", cajero.Url);
            var texto = await cajero.Locator("body").InnerTextAsync();
            Assert.Contains("Este dispositivo no está autorizado", texto);
            // ambas vias de autorizacion se ofrecen y explican
            Assert.True(await cajero.Locator("#opcionPc").IsVisibleAsync());
            Assert.True(await cajero.Locator("#opcionMail").IsVisibleAsync());
            await ctxCajero.CloseAsync();

            var ctxAdmin = await _fixture.Browser.NewContextAsync();
            var adminFresco = await NuevaPaginaAsync(ctxAdmin);
            await LoginAsync(adminFresco, "ger");
            Assert.DoesNotContain("/Login", adminFresco.Url);
            await ctxAdmin.CloseAsync();
        }
        finally
        {
            await PonerSwitchEmpresaAsync(admin, false);
            await admin.CloseAsync();
        }
    }

    [Fact]
    public async Task SwitchApagado_NoAdminEntraComoSiempre()
    {
        var ctx = await _fixture.Browser.NewContextAsync();
        var cajero = await NuevaPaginaAsync(ctx);
        await LoginAsync(cajero, "cajero");

        Assert.DoesNotContain("/Login", cajero.Url);
        await ctx.CloseAsync();
    }

    [Fact]
    public async Task DispositivoDelNavegadorCargadoPorAdmin_Entra_YAlBloquearloNoEntra_YAuditoriaMuestraElDispositivo()
    {
        var descripcion = "E2E navegador " + DateTime.UtcNow.Ticks;
        var admin = await _fixture.NewAuthenticatedPageAsync();
        try
        {
            await PonerSwitchEmpresaAsync(admin, true);

            // El "cajero" abre el login (el servidor le emite la cookie cs_dev) y cae en la pantalla.
            var ctxCajero = await _fixture.Browser.NewContextAsync();
            var cajero = await NuevaPaginaAsync(ctxCajero);
            await LoginAsync(cajero, "cajero");
            Assert.Contains("/Login/DispositivoNoAutorizado", cajero.Url);

            var cookies = await ctxCajero.CookiesAsync();
            var token = cookies.First(c => c.Name == "cs_dev").Value;
            Assert.Equal(22, token.Length);
            Assert.True(cookies.First(c => c.Name == "cs_dev").HttpOnly);

            // El admin autoriza ese navegador (mismo hash que calcula el servidor).
            await AgregarDispositivoAsync(admin, SerieDeToken(token), descripcion);

            await LoginAsync(cajero, "cajero");
            Assert.DoesNotContain("/Login", cajero.Url);
            await cajero.CloseAsync();

            // La auditoria de accesos muestra el dispositivo desde el que se logueo.
            await admin.GotoAsync($"{WebCoreFixture.BaseUrl}/AuditoriaLogin", new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });
            var tabla = await admin.Locator("table tbody").InnerTextAsync();
            Assert.Contains(descripcion, tabla);

            // El admin lo bloquea: el mismo navegador ya no entra y se le dice por que.
            await admin.GotoAsync($"{WebCoreFixture.BaseUrl}/DispositivosSeguros", new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });
            var fila = admin.Locator("table tbody tr", new PageLocatorOptions { HasText = descripcion });
            await admin.RunAndWaitForNavigationAsync(() => fila.Locator("form[action*='CambiarBloqueo'] button[type='submit']").ClickAsync());
            Assert.Contains("Bloqueado", await admin.Locator("table tbody tr", new PageLocatorOptions { HasText = descripcion }).InnerTextAsync());

            var cajero2 = await NuevaPaginaAsync(ctxCajero);
            await LoginAsync(cajero2, "cajero");
            Assert.Contains("/Login", cajero2.Url);
            Assert.DoesNotContain("DispositivoNoAutorizado", cajero2.Url);
            Assert.Contains("bloqueado por el administrador", await cajero2.Locator("body").InnerTextAsync());
            await ctxCajero.CloseAsync();
        }
        finally
        {
            await EliminarDispositivoAsync(admin, descripcion);
            await PonerSwitchEmpresaAsync(admin, false);
            await admin.CloseAsync();
        }
    }

    [Fact]
    public async Task DispositivoPorIdDeHardwareCargadoPorAdmin_Entra()
    {
        var descripcion = "E2E hardware " + DateTime.UtcNow.Ticks;
        var serie = "E2E-HW-" + DateTime.UtcNow.Ticks;
        var admin = await _fixture.NewAuthenticatedPageAsync();
        try
        {
            await PonerSwitchEmpresaAsync(admin, true);
            await AgregarDispositivoAsync(admin, serie, descripcion);

            var ctx = await _fixture.Browser.NewContextAsync();
            var pc = await NuevaPaginaAsync(ctx);
            await pc.GotoAsync($"{WebCoreFixture.BaseUrl}/Login");
            // Sin el agente real, el hidden se completa a mano (es lo que hace print-agent.js).
            await pc.EvaluateAsync("(s) => { document.getElementById('NumeroSerieDispositivo').value = s; }", serie);
            await pc.FillAsync("input[name='Usuario']", "cajero");
            await pc.FillAsync("input[name='Clave']", "a");
            await ClickYEsperarAsync(pc, "button[type='submit']");

            Assert.DoesNotContain("/Login", pc.Url);
            await ctx.CloseAsync();
        }
        finally
        {
            await EliminarDispositivoAsync(admin, descripcion);
            await PonerSwitchEmpresaAsync(admin, false);
            await admin.CloseAsync();
        }
    }

    // Pedido del usuario: que en la pantalla de empresa y en la de usuario se le explique al
    // administrador, de forma clara, como funciona el permiso de dispositivo seguro.
    [Fact]
    public async Task PantallasDeEmpresaYUsuario_ExplicanAlAdminComoFuncionaDispositivoSeguro()
    {
        var admin = await _fixture.NewAuthenticatedPageAsync();
        try
        {
            async Task VerificarModalAsync()
            {
                await admin.ClickAsync("#btnInfoDispositivoSeguro");
                var modal = admin.Locator("#modalInfoDispositivoSeguro.show");
                await modal.WaitForAsync(new LocatorWaitForOptions { Timeout = 10000 });
                var texto = await modal.InnerTextAsync();
                Assert.Contains("A quién se le exige", texto);
                Assert.Contains("administradores nunca quedan afectados", texto);
                Assert.Contains("código de 6 dígitos a su mail", texto);
                Assert.Contains("bloquear", texto);
                Assert.Contains("mail de cada empleado", texto);
                await admin.ClickAsync("#modalInfoDispositivoSeguro .modal-footer button");
                await admin.Locator("#modalInfoDispositivoSeguro.show").WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Hidden, Timeout = 10000 });
            }

            await admin.GotoAsync($"{WebCoreFixture.BaseUrl}/Empresa", new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });
            await VerificarModalAsync();

            await admin.GotoAsync($"{WebCoreFixture.BaseUrl}/Usuarios", new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });
            var href = await admin.Locator("table tbody tr", new PageLocatorOptions { HasText = "cajero" }).Locator("a[href*='Editar']").First.GetAttributeAsync("href");
            await admin.GotoAsync(new Uri(new Uri(WebCoreFixture.BaseUrl), href!).ToString(), new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });
            await VerificarModalAsync();
        }
        finally
        {
            await admin.CloseAsync();
        }
    }

    [Fact]
    public async Task TildePorUsuario_SeGuardaYSeMuestraEnLaEdicionDelUsuario()
    {
        var admin = await _fixture.NewAuthenticatedPageAsync();
        try
        {
            await admin.GotoAsync($"{WebCoreFixture.BaseUrl}/Usuarios", new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });
            var enlace = admin.Locator("table tbody tr", new PageLocatorOptions { HasText = "cajero" }).Locator("a[href*='Editar']").First;
            var href = await enlace.GetAttributeAsync("href");
            Assert.False(string.IsNullOrEmpty(href));

            async Task PonerTildeAsync(bool valor)
            {
                await admin.GotoAsync(new Uri(new Uri(WebCoreFixture.BaseUrl), href!).ToString(), new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });
                await admin.ClickAsync("#btnHabilitarEdicionUsuario");
                await admin.SetCheckedAsync("#RequiereDispositivoSeguro", valor);
                await ClickYEsperarAsync(admin, "#btnGuardarUsuario");
                await admin.GotoAsync(new Uri(new Uri(WebCoreFixture.BaseUrl), href!).ToString(), new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });
                Assert.Equal(valor, await admin.IsCheckedAsync("#RequiereDispositivoSeguro"));
            }

            await PonerTildeAsync(true);

            // Con el tilde puesto (y el switch de empresa apagado) el cajero cae en la pantalla.
            var ctx = await _fixture.Browser.NewContextAsync();
            var cajero = await NuevaPaginaAsync(ctx);
            await LoginAsync(cajero, "cajero");
            Assert.Contains("/Login/DispositivoNoAutorizado", cajero.Url);
            await ctx.CloseAsync();

            await PonerTildeAsync(false);
        }
        finally
        {
            await admin.CloseAsync();
        }
    }
}
