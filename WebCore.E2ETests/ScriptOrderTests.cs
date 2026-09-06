using System.Text.Json;
using Microsoft.Playwright;

namespace WebCore.E2ETests;

// Regresion del bug real relevado el 2026-09-05 (ver docs/DECISIONS.md, "$ is not defined por
// orden de carga de jQuery"): _Layout.cshtml carga jquery.min.js al final del <body> y recien
// despues renderiza @section Scripts (linea 596) -- cualquier <script> de una vista que use
// jQuery fuera de esa seccion corre ANTES de que jQuery exista y tira "$ is not defined",
// abortando el resto del bloque en silencio. Se encontraron 3 vistas reales rotas por esto
// (Personas/Index, Usuarios/Editar, Finanzas/AddOrEditPago) y se corrigieron moviendo sus
// scripts a @section Scripts (o, en AddOrEditPago, agregando un guard de DOMContentLoaded --
// no puede usar @section Scripts porque tambien se sirve como PartialView via AJAX, sin Layout).
[Collection("WebCore browser")]
public sealed class ScriptOrderTests
{
    private readonly WebCoreFixture _fixture;

    public ScriptOrderTests(WebCoreFixture fixture)
    {
        _fixture = fixture;
    }

    private static async Task<(IResponse? Response, List<string> Errors, IPage Page)> GotoAndCollectErrors(
        WebCoreFixture fixture, string url)
    {
        var page = await fixture.NewAuthenticatedPageAsync();
        var errors = new List<string>();
        page.PageError += (_, msg) => errors.Add(msg);

        var response = await page.GotoAsync(url, new PageGotoOptions
        {
            WaitUntil = WaitUntilState.NetworkIdle,
            Timeout = 20000
        });

        return (response, errors, page);
    }

    [Fact]
    public async Task PersonasIndex_SinErroresDeScript_YBusquedaEnVivoFunciona()
    {
        var (response, errors, page) = await GotoAndCollectErrors(_fixture,$"{WebCoreFixture.BaseUrl}/Personas");

        Assert.NotNull(response);
        Assert.Equal(200, response!.Status);
        Assert.Empty(errors);

        // La busqueda en vivo depende de que $input = $("#txtFiltroPersonas") se haya podido
        // ejecutar -- si el bug reaparece, este input nunca dispara /Personas/Listar.
        await page.Locator("#txtFiltroPersonas").FillAsync("a");
        await page.WaitForTimeoutAsync(400); // debounce de 250ms en Personas/Index.cshtml
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var filasVisibles = await page.Locator("#tbodyPersonas tr").CountAsync();
        Assert.True(filasVisibles > 0, "La busqueda en vivo de Personas no actualizo la tabla (jQuery pudo no haberse wireado).");

        await page.CloseAsync();
    }

    [Fact]
    public async Task UsuariosEditar_SinErroresDeScript_YEditReadOnlyQuedaWireado()
    {
        var (response, errors, page) = await GotoAndCollectErrors(_fixture,$"{WebCoreFixture.BaseUrl}/Usuarios/Editar?id=0");

        Assert.NotNull(response);
        Assert.Equal(200, response!.Status);
        Assert.Empty(errors);

        // Alta (id=0) arranca en modo edicion (soloLecturaInicial=false) -- si EditReadOnly.init()
        // nunca corrio (por el bug de orden de scripts), window.EditReadOnly queda undefined y
        // ninguno de los dos botones refleja el estado real.
        var editReadOnlyDefinido = await page.EvaluateAsync<bool>("typeof window.EditReadOnly === 'object' && window.EditReadOnly !== null");
        Assert.True(editReadOnlyDefinido, "window.EditReadOnly nunca se definio -- edit-readonly.js pudo haber cargado antes que jQuery.");

        await page.CloseAsync();
    }

    [Fact]
    public async Task AddOrEditPago_ConLayoutCompleto_SinErroresDeScript()
    {
        // AddOrEditPago no puede usar @section Scripts (tambien se sirve sin layout via
        // PartialView/AJAX) -- este test cubre especificamente el caso CON layout completo,
        // que es el que corria antes de jquery.min.js y rompia.
        var listado = await _fixture.NewAuthenticatedPageAsync();
        var respListado = await listado.GotoAsync($"{WebCoreFixture.BaseUrl}/Personas/Listar?filtro=");
        var json = await respListado!.TextAsync();
        using var doc = JsonDocument.Parse(json);
        var idPersona = doc.RootElement.EnumerateArray().First().GetProperty("idPersona").GetInt32();
        await listado.CloseAsync();

        var (response, errors, page) = await GotoAndCollectErrors(
            _fixture,
            $"{WebCoreFixture.BaseUrl}/Finanzas/AddOrEditPago?idPersona={idPersona}&returnUrl=");

        Assert.NotNull(response);
        Assert.Equal(200, response!.Status);
        Assert.Empty(errors);

        // Si $('#btnGuardarPago').on('click', ...) nunca se ejecuto, el boton sigue en el DOM
        // pero sin handler -- Playwright no puede verificar "sin handler" directo, pero si
        // jQuery pudo enlazarlo sin tirar excepcion es porque el guard de DOMContentLoaded corrio.
        Assert.True(await page.Locator("#btnGuardarPago").IsVisibleAsync());

        await page.CloseAsync();
    }
}
