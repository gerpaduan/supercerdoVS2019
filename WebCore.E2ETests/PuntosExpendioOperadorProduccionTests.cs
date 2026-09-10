using Microsoft.Playwright;

namespace WebCore.E2ETests;

// Bug reportado por el usuario (2026-09-08, durante la revision de Batch B del plan de permisos):
// "no funciona el logueo del user operativo con el login de usuario produccion [en PuntosExpendio].
// Muestra el modal de logueo pero no se puede escribir para buscar usuario ni contraseña, da
// error." Causa raiz (ver docs/DECISIONS.md "Batch B: permisos reales + operador de produccion"):
// PuntosExpendio/POS.cshtml dispara pedirOperador(true) (modal #modalSeleccionUsuario) y, en
// paralelo, punto-expendio-pos.js dispara openSectorModal() (modal #modalSectoresPuntoExpendio,
// backdrop "static") cuando no hay sector elegido -- los dos ".modal('show')" de Bootstrap 5
// compiten por el mismo backdrop y el modal de sector termina absorbiendo los clicks/teclas del
// modal de operador. Fix: punto-expendio-pos.js no dispara el modal de sector mientras
// PosOperadorConfig.requiereOperadorPOS este pendiente (el operador se resuelve primero; al
// recargar la pagina sin ese flag, el modal de sector se abre solo, sin competencia).
//
// Usuario de prueba: "produccion" (id=16 en la base de desarrollo local, EsUsuarioProduccion=true,
// idempresa=1, ya existente) -- clave reseteada a "a" via el propio flujo de Usuarios/Guardar
// (mismo mecanismo ya usado para "ger", ver WebCoreFixture.cs), no es un secreto real.
[Collection("WebCore browser")]
public sealed class PuntosExpendioOperadorProduccionTests
{
    private readonly WebCoreFixture _fixture;

    public PuntosExpendioOperadorProduccionTests(WebCoreFixture fixture)
    {
        _fixture = fixture;
    }

    private async Task<IPage> NewProduccionAuthenticatedPageAsync()
    {
        var page = await _fixture.Browser.NewPageAsync();
        await page.GotoAsync($"{WebCoreFixture.BaseUrl}/Login", new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });
        await page.FillAsync("input[name='Usuario']", "produccion");
        await page.FillAsync("input[name='Clave']", "a");
        await page.ClickAsync("button[type='submit']");
        await page.WaitForURLAsync(url => !url.Contains("/Login"), new PageWaitForURLOptions { Timeout = 15000 });
        return page;
    }

    [Fact]
    public async Task PuntosExpendioPOS_UsuarioProduccionSinSector_ModalOperadorEsUsableYNoCompiteConModalSector()
    {
        var page = await NewProduccionAuthenticatedPageAsync();

        // Usuario "produccion" recien logueado, sin sector elegido en esta sesion -- escenario
        // exacto que dispara ambos modales.
        await page.GotoAsync($"{WebCoreFixture.BaseUrl}/PuntosExpendio/POS", new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });
        await page.WaitForSelectorAsync("#modalSeleccionUsuario.show", new PageWaitForSelectorOptions { Timeout = 10000 });

        Assert.True(await page.Locator("#modalSeleccionUsuario").IsVisibleAsync());
        Assert.False(await page.Locator("#modalSectoresPuntoExpendio").IsVisibleAsync());

        // El sintoma reportado era que no se podia tipear en estos inputs -- confirmar que
        // realmente aceptan foco y texto.
        await page.Locator("#txtSeleccionUsuario").ClickAsync(new LocatorClickOptions { Timeout = 5000 });
        await page.Locator("#txtSeleccionUsuario").FillAsync("ger", new LocatorFillOptions { Timeout = 5000 });
        Assert.Equal("ger", await page.Locator("#txtSeleccionUsuario").InputValueAsync());
        await page.FillAsync("#passSeleccionUsuario", "a");

        // Resolver el operador con "ger" (admin, tiene Permisos.Venta.NuevaVenta) -- tras la
        // recarga, el modal de sector debe abrirse solo, sin el modal de operador de por medio.
        await page.ClickAsync("#btnConfirmarSeleccionUsuario");
        await page.WaitForSelectorAsync("#modalSectoresPuntoExpendio.show", new PageWaitForSelectorOptions { Timeout = 10000 });

        Assert.False(await page.Locator("#modalSeleccionUsuario").IsVisibleAsync());
        Assert.True(await page.Locator("#modalSectoresPuntoExpendio").IsVisibleAsync());
    }
}
