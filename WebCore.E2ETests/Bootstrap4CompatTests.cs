using Microsoft.Playwright;

namespace WebCore.E2ETests;

// Regresion del bug real encontrado el 2026-09-04 y cerrado el 2026-09-05 (ver docs/DECISIONS.md
// y docs/10-migracion-aspnet-core/gaps.md): WebCore corre Bootstrap 5.3.3, que elimino el plugin
// jQuery ($.fn.modal/.collapse/.alert/etc.). El markup y JS portados desde Web (16+ archivos:
// AddOrEditPago, Stock/Index, Compras/Index, Personas/Index, Productos/Index, calculadora-billetes.js,
// stock.js, egresos-caja.js, forma-pago.js, compras.js, y varios mas de POS) siguen llamando a esos
// metodos como jQuery plugin -- sin el shim, tira "$(...).modal is not a function" (pageerror real)
// y puede cortar el resto del bloque <script>. Resuelto extendiendo bootstrap4-compat.js con un
// puente jQuery -> API real de BS5 (getOrCreateInstance), en vez de reescribir cada llamada.
[Collection("WebCore browser")]
public sealed class Bootstrap4CompatTests
{
    private readonly WebCoreFixture _fixture;

    public Bootstrap4CompatTests(WebCoreFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task JQueryFnModalCollapseAlert_QuedanDefinidosComoPluginReal()
    {
        var page = await _fixture.NewAuthenticatedPageAsync();
        var errors = new List<string>();
        page.PageError += (_, msg) => errors.Add(msg);

        await page.GotoAsync($"{WebCoreFixture.BaseUrl}/Compras", new PageGotoOptions
        {
            WaitUntil = WaitUntilState.NetworkIdle,
            Timeout = 20000
        });

        Assert.Empty(errors);
        Assert.True(await page.EvaluateAsync<bool>("typeof window.jQuery.fn.modal === 'function'"));
        Assert.True(await page.EvaluateAsync<bool>("typeof window.jQuery.fn.collapse === 'function'"));
        Assert.True(await page.EvaluateAsync<bool>("typeof window.jQuery.fn.alert === 'function'"));

        await page.CloseAsync();
    }

    [Fact]
    public async Task StockEditar_SinErrorDeScriptAlCargar()
    {
        // Repro original del gap (docs/10-migracion-aspnet-core/gaps.md, ya cerrado): esta vista
        // tiraba "$(...).modal is not a function" apenas cargaba la pagina, antes de cualquier click.
        var page = await _fixture.NewAuthenticatedPageAsync();
        var errors = new List<string>();
        page.PageError += (_, msg) => errors.Add(msg);

        var response = await page.GotoAsync($"{WebCoreFixture.BaseUrl}/Stock/Editar?idCorte=20", new PageGotoOptions
        {
            WaitUntil = WaitUntilState.NetworkIdle,
            Timeout = 20000
        });

        Assert.NotNull(response);
        Assert.Equal(200, response!.Status);
        Assert.Empty(errors);

        await page.CloseAsync();
    }

    // Regresion del bug real encontrado 2026-09-06 al portar el modal de Factura Electronica (ver
    // docs/DECISIONS.md): "$(el).modal({backdrop:'static', keyboard:false, show:true})" -- el
    // modismo de Bootstrap 4 usado en varios archivos de Web/Scripts/app/*.js -- nunca mostraba el
    // modal. Causa real, dificil de encontrar: esta version de bootstrap.bundle.min.js (5.0/5.1)
    // trae su PROPIA interfaz jQuery nativa con el mismo defecto (ignora "show" en un objeto de
    // opciones), y la registra en un listener de DOMContentLoaded que corre DESPUES del shim de
    // este archivo -- pisando la version corregida otra vez. El fix reaplica el registro tambien
    // en DOMContentLoaded para quedar con la ultima palabra. Sin este fix, el modal queda con
    // class="modal fade" (sin "show"), display:none, invisible, sin ningun error en consola.
    [Fact]
    public async Task ModalConOpcionShowTrue_SeMuestraDeVerdad()
    {
        var page = await _fixture.NewAuthenticatedPageAsync();
        var errors = new List<string>();
        page.PageError += (_, msg) => errors.Add(msg);

        await page.GotoAsync($"{WebCoreFixture.BaseUrl}/Compras", new PageGotoOptions
        {
            WaitUntil = WaitUntilState.NetworkIdle,
            Timeout = 20000
        });

        // No hace falta un modal real de la vista -- se agrega uno de prueba minimo y se ejercita
        // el mismo modismo exacto de Bootstrap 4 que rompia antes del fix.
        await page.EvaluateAsync(@"() => {
            var div = document.createElement('div');
            div.id = 'modalPruebaShowTrue';
            div.className = 'modal fade';
            div.innerHTML = '<div class=""modal-dialog""><div class=""modal-content"">x</div></div>';
            document.body.appendChild(div);
            $('#modalPruebaShowTrue').modal({ backdrop: 'static', keyboard: false, show: true });
        }");
        // El show() real de Bootstrap 5 aplica la clase "show" en el siguiente tick (fuerza un
        // reflow antes de la transicion CSS) -- no es sincronico con la llamada a .modal().
        await page.WaitForTimeoutAsync(300);

        var quedoVisible = await page.Locator("#modalPruebaShowTrue.show").CountAsync() == 1;
        Assert.True(quedoVisible, "El modismo Bootstrap 4 '.modal({...show:true})' no mostro el modal.");
        Assert.Empty(errors);

        await page.CloseAsync();
    }
}
