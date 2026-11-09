// Piezas comunes de los videos del POS (escenario "tutorial", 2026-11-09): navegador preparado para grabar (datos
// ficticios, filtros de listas con datos reales, avisos de homologacion ocultos, email simulado), entrada al POS con
// apertura de caja, esperas del POS y helpers de mouse y teclado. Ver docs/DECISIONS.md 2026-11-09.
using AyudaMedia.Comun;
using Microsoft.Playwright;

namespace AyudaMedia.Escenarios;

internal static class PosComun
{
    // Monto inicial de la caja y datos del cliente en los videos (ficticios; example.com es un dominio reservado).
    public const string MontoInicialCaja = "20000";
    public const string ClienteEjemplo = "Laura Fernández";
    public const int IdClienteEjemplo = 9000001;
    public const string EmailEjemplo = "laura.fernandez@example.com";
    // Velocidades (pedido del usuario, 2026-11-09): voz 1,1x (la genera asi Piper) e imagenes 1,4x.
    public const double VelocidadVoz = 1.1;
    public const double VelocidadVideo = 1.4;
    // Vista de la app (1280x640) + franja de subtitulos de 80 px debajo = video de 1280x720 (pedido del usuario: los
    // subtitulos y las teclas no tapan los controles).
    public const int AnchoVista = 1280;
    public const int AltoVista = 640;
    public const int AltoFranja = 80;

    public static Grabador NuevoGrabador(IPage page, ISintetizadorVoz voz) =>
        new(page, voz, VelocidadVideo, AnchoVista, AltoVista, AltoFranja);

    public const string SelMenuPos = "a.nav-link[href='/Ventas/POS']";
    // POS en una venta nueva y vacia, sin ventanas encima.
    public const string JsPosListo =
        "() => !document.querySelector('.modal.show') && !document.getElementById('__impresion') && !!document.getElementById('inputCodigo') && !document.getElementById('tablaItems')?.querySelector('tr:not(#filaVacia)')";

    // Frases compartidas: comienzo (menu, caja), carga de productos y cobro en efectivo.
    public static class Frases
    {
        public const string EntrarAlPos = "Para vender, entrá a Punto de Venta desde el menú.";
        public const string CajaMontoInicial = "Si la caja no está abierta, el sistema te pide abrirla. Escribí el monto inicial.";
        public const string CajaAbrir = "Y tocá Abrir caja.";
        public const string CajaAbierta = "La caja quedó abierta. Si no necesitás el ticket de apertura, tocá Cerrar sin imprimir.";
        public const string CodigoYEnter = "Escribí el código del producto y presioná Enter.";
        public const string SiguienteProducto = "Lo mismo con el siguiente producto: código, Enter, cantidad y Enter.";
        public const string CantidadYBalanza = "El cursor pasa a la cantidad. Si hay una balanza conectada, el peso se carga solo desde la balanza.";
        public const string CantidadAMano = "Si no, escribí la cantidad.";
        public const string EnterAgrega = "Presioná Enter otra vez y el producto queda agregado a la venta.";
        public const string TeclaFin = "Para finalizar la venta, presioná la tecla Fin. También podés tocar Finalizar y cobrar.";
        public const string FormaPagoUno = "Elegí la forma de pago con su número. El uno es efectivo.";
        public const string VentaRegistrada = "La venta quedó registrada. En esta ventana elegís qué hacer con el comprobante.";

        public static readonly string[] Todas =
        {
            EntrarAlPos, CajaMontoInicial, CajaAbrir, CajaAbierta, CodigoYEnter, SiguienteProducto, CantidadYBalanza,
            CantidadAMano, EnterAgrega, TeclaFin, FormaPagoUno, VentaRegistrada
        };
    }

    // Contexto del navegador listo para grabar: en cada pagina (el POS recarga seguido) datos ficticios, puntero
    // visible, carteles, marcas, teclas dibujadas y la intercepcion del PDF; tablero del inicio oculto y envio de
    // email respondido en el navegador.
    public static async Task<IBrowserContext> NuevoContextoAsync(IBrowser browser)
    {
        var ficticios = Ficticios.CargarDesdeEntorno();
        var contexto = await browser.NewContextAsync(new BrowserNewContextOptions
        {
            ViewportSize = new ViewportSize { Width = AnchoVista, Height = AltoVista },
            Locale = "es-AR"
        });
        await contexto.AddInitScriptAsync(Ficticios.ScriptInicio(ficticios));
        await contexto.AddInitScriptAsync(Overlay.JsPuntero);
        await contexto.AddInitScriptAsync(Overlay.JsHelpers);
        await contexto.AddInitScriptAsync(Overlay.JsTeclas);
        await contexto.AddInitScriptAsync(Impresion.JsInterceptarPdf);
        // El tablero del inicio muestra importes de la base (ventas del dia, saldos): en el video solo interesa el
        // menu, asi que en esa pantalla se oculta el contenido (datos ficticios siempre, pedido del usuario).
        await contexto.AddInitScriptAsync(@"(function () {
            if (!/^\/(Home(\/Index)?)?\/?$/i.test(location.pathname)) return;
            document.addEventListener('DOMContentLoaded', function () {
                var s = document.createElement('style');
                s.textContent = '#content > .container-fluid { visibility: hidden !important; }';
                document.head.appendChild(s);
            });
        })();");
        // Etiqueta "MODO PRUEBA (AFIP)" del encabezado: aparece solo con la empresa en homologacion (necesaria para
        // grabar la factura); en produccion no esta, asi que en el video se oculta.
        await contexto.AddInitScriptAsync(@"(function () {
            function ocultar() {
                if (!document.documentElement || document.getElementById('__ficModoPrueba')) return;
                var s = document.createElement('style');
                s.id = '__ficModoPrueba';
                s.textContent = 'a.topbar-aviso-badge[href*=""CertificadoArca""]{display:none !important}';
                document.documentElement.appendChild(s);
            }
            ocultar();
            new MutationObserver(ocultar).observe(document, { childList: true, subtree: true });
        })();");
        // Buscador de clientes (F9): sin filtro lista TODOS los clientes reales (nombres, CUIT). En la grabacion las filas
        // quedan ocultas por defecto y solo se muestran las permitidas (la clienta ficticia y Consumidor final); el
        // observer marca cada fila apenas entra al DOM, antes de que se pinte (fuga encontrada el 2026-11-09).
        await contexto.AddInitScriptAsync(@"(function () {
            var permitidos = ['" + ClienteEjemplo + @"', 'CONSUMIDOR FINAL'];
            function revisar() {
                document.querySelectorAll('#tablaPersonas tr:not([data-fic-revisada])').forEach(function (tr) {
                    var t = tr.textContent || '';
                    tr.setAttribute('data-fic-revisada', permitidos.some(function (p) { return t.indexOf(p) >= 0; }) ? 'si' : 'no');
                });
            }
            function ocultarPorDefecto() {
                if (!document.documentElement || document.getElementById('__ficPersonas')) return;
                var st = document.createElement('style');
                st.id = '__ficPersonas';
                // Cuentas corrientes (F2): la lista trae todos los clientes reales con su saldo; solo queda la ficticia.
                st.textContent = '#tablaPersonas tr:not([data-fic-revisada=""si""]){display:none !important}'
                    + '#tablaCtasCtes tbody tr:not([data-id-persona=""" + IdClienteEjemplo + @"""]){display:none !important}'
                    // Buscador de productos (F10): solo los productos de ejemplo (el catalogo real tiene nombres y precios).
                    + '#modalBuscarProducto tbody tr:not([data-codigo=""9901""]):not([data-codigo=""9902""]):not([data-codigo=""7790000000010""]){display:none !important}';
                document.documentElement.appendChild(st);
            }
            ocultarPorDefecto();
            new MutationObserver(function () { ocultarPorDefecto(); revisar(); })
                .observe(document, { childList: true, subtree: true, characterData: true });
        })();");
        // El envio de email del video NO sale: el SMTP de desarrollo es real (manda mails de verdad). Se responde en
        // el navegador lo mismo que el servidor cuando el envio sale bien ({ ok: true }, ver EnviarComprobanteEmail).
        await contexto.RouteAsync("**/Ventas/EnviarComprobanteEmail**", ruta => ruta.FulfillAsync(new RouteFulfillOptions
        {
            Status = 200,
            ContentType = "application/json",
            Body = "{\"ok\":true}"
        }));
        return contexto;
    }

    // Login (sin grabar) y pantalla de inicio con el menu visible.
    public static async Task IrAlInicioAsync(IPage page, Config cfg)
    {
        await Sesion.IniciarAsync(page, cfg);
        await page.EvaluateAsync("() => localStorage.setItem('postventa_ticket_mm', '80')");
        await page.GotoAsync($"{cfg.BaseUrl}/", new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });
        await page.Locator(SelMenuPos).WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        await page.Mouse.MoveAsync(Config.AnchoVista / 2f, Config.AltoVista / 2f);
        await page.WaitForTimeoutAsync(1000);
    }

    // Menu -> Punto de Venta -> apertura de caja si hace falta -> POS listo. Con grabador = null no narra (preparacion
    // fuera de camara de los clips que arrancan directo en la accion).
    public static async Task EntrarAlPosAsync(IPage page, Grabador? grabador)
    {
        if (grabador != null)
        {
            await page.EvaluateAsync($"() => marcar(document.querySelector(\"{SelMenuPos}\"), 1)");
            await grabador.NarrarAsync(Frases.EntrarAlPos);
            await page.EvaluateAsync("() => { limpiarMarcas(); cartel(''); }");
        }
        await MoverYClicAsync(page, SelMenuPos);
        await page.WaitForURLAsync(url => url.Contains("/Ventas/POS"), new PageWaitForURLOptions { Timeout = 20000 });

        // Si la caja de la sucursal no esta abierta, el POS muestra la apertura apenas carga.
        bool pideCaja;
        try
        {
            await page.Locator("#modalAbrirCaja").WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible, Timeout = 6000 });
            pideCaja = true;
        }
        catch (TimeoutException)
        {
            pideCaja = false;
        }

        if (pideCaja)
        {
            await page.WaitForTimeoutAsync(600);
            if (grabador != null) await grabador.NarrarAsync(Frases.CajaMontoInicial);
            await MoverYClicAsync(page, "#montoInicial");
            await page.Keyboard.TypeAsync(MontoInicialCaja, new KeyboardTypeOptions { Delay = 160 });
            await page.WaitForTimeoutAsync(400);
            if (grabador != null) await grabador.NarrarAsync(Frases.CajaAbrir, 200);
            await MoverYClicAsync(page, "#modalAbrirCaja button:has-text('Abrir Caja')");

            // Aviso "Caja abierta" con la opcion de imprimir el ticket de apertura.
            await page.Locator("#btnPcCerrar").WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible, Timeout = 15000 });
            await page.WaitForTimeoutAsync(500);
            if (grabador != null) await grabador.NarrarAsync(Frases.CajaAbierta);
            // El POS recarga con la caja abierta: se espera la navegacion nueva, no el estado de la pagina vieja.
            await page.RunAndWaitForNavigationAsync(() => MoverYClicAsync(page, "#btnPcCerrar"),
                new PageRunAndWaitForNavigationOptions { WaitUntil = WaitUntilState.DOMContentLoaded, Timeout = 20000 });
        }

        await EsperarPosListoAsync(page);
    }

    // Espera el POS cargado y sin ventanas, e instala la impresion simulada (TicketPrint ya existe).
    public static async Task EsperarPosListoAsync(IPage page)
    {
        await page.Locator("#inputCodigo").WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible, Timeout = 20000 });
        await page.WaitForFunctionAsync("() => !document.querySelector('.modal.show')", null, new PageWaitForFunctionOptions { Timeout = 15000 });
        await page.WaitForFunctionAsync("() => !!(window.TicketPrint && window.jQuery)", null, new PageWaitForFunctionOptions { Timeout = 20000 });
        await page.WaitForTimeoutAsync(800);
        await page.EvaluateAsync(Impresion.JsEscena);
        await page.EvaluateAsync(Impresion.JsInterceptarTicket);
    }

    // Espera la ventana "Venta completada" y devuelve el id de la venta.
    public static async Task<string> EsperarPostVentaAsync(IPage page, float timeoutMs = 30000)
    {
        await page.Locator("#modalPostVentaBasico").WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible, Timeout = timeoutMs });
        await page.WaitForTimeoutAsync(700);
        return await page.EvaluateAsync<string>("() => String(window.jQuery('#btnPvbTicket').data('venta-id') || '')");
    }

    // Espera a que el POS quede en una venta nueva y vacia (las acciones recargan la pagina) y reinstala la impresion.
    public static async Task EsperarVentaNuevaAsync(IPage page)
    {
        await page.WaitForFunctionAsync(JsPosListo, null, new PageWaitForFunctionOptions { Timeout = 30000, PollingInterval = 200 });
        await page.WaitForTimeoutAsync(900);
    }

    // Muestra la tecla dibujada en pantalla (si se pide) y la aprieta de verdad (tecla = nombre de Playwright,
    // rotulo = lo que se ve).
    public static async Task ApretarAsync(IPage page, string tecla, string rotulo, bool mostrar = true)
    {
        if (mostrar)
        {
            // Con franja la tecla se dibuja abajo, fuera de la app (no tapa el campo); sin franja, sobre la pagina.
            if (Grabador.Actual is { ConFranja: true } g) g.MarcarTecla(rotulo);
            else await page.EvaluateAsync("(r) => tecla(r)", rotulo);
            await page.WaitForTimeoutAsync(450);
        }
        // Un solo caracter ("+", "/", letras, numeros) se tipea: PressAsync("+") lo tomaria como combinacion de teclas.
        if (tecla.Length == 1)
            await page.Keyboard.TypeAsync(tecla);
        else
            await page.Keyboard.PressAsync(tecla);
    }

    // Lleva el puntero visible hasta el centro del control (movimiento suave) y hace clic.
    public static async Task MoverYClicAsync(IPage page, string selector)
    {
        var caja = await page.Locator(selector).First.BoundingBoxAsync()
            ?? throw new InvalidOperationException($"No se pudo ubicar {selector}.");
        float x = caja.X + caja.Width / 2, y = caja.Y + caja.Height / 2;
        await page.Mouse.MoveAsync(x, y, new MouseMoveOptions { Steps = 28 });
        await page.WaitForTimeoutAsync(350);
        await page.Mouse.DownAsync();
        await page.WaitForTimeoutAsync(90);
        await page.Mouse.UpAsync();
    }
}
