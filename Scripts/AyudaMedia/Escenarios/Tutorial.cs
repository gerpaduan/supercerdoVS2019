// Escenario "tutorial" (2026-11-09): videos largos del punto de venta, uno por tema, pedidos por el usuario en lugar
// de un clip por accion. Hoy: "venta" (apertura de caja y venta rapida sin caja, carga manual / con balanza / con
// lector, formas de pago con sus numeros y el pago mixto, y todas las opciones de "Venta completada", incluida la
// factura electronica con su ticket, PDF y email), "cliente", "ctacte", "egreso", "recorrido" y "lineas" (cantidad,
// eliminar y bonificar renglones del carrito, 2026-11-09) y "misventas" (Mis ventas: detalle, reimprimir, modificar
// venta y cambiar la forma de pago). Los atajos que se narran estan verificados en el codigo del POS.
// Escribe en la base de desarrollo (caja, venta, factura de prueba en HOMOLOGACION: PosComun corta si no ve el modo
// prueba). Antes de grabar: caja cerrada (sql/postventa-limpieza-grabacion.sql). Salida: Ventas.POS/tutorial-<tema>.mp4.
using AyudaMedia.Comun;
using Microsoft.Playwright;

namespace AyudaMedia.Escenarios;

public sealed class Tutorial : IEscenario
{
    public string Nombre => "tutorial";
    public string Descripcion => "Ventas.POS: tutorial <venta|cliente|ctacte|egreso|recorrido|lineas|misventas|borradores>  (videos largos con voz)";

    private const string EanEjemplo = "7790000000010";
    private const string NombreEanEjemplo = "Gaseosa cola 1,5 L";

    private static class Frases
    {
        public const string Entrar = "Para vender, entrá a Punto de Venta desde el menú.";
        public const string Apertura = "Si la caja no está abierta, el sistema te pide abrirla con el monto inicial.";
        public const string SinCaja = "Si necesitás vender sin abrir la caja, tildá la venta rápida sin apertura de caja.";
        public const string SinCajaAtipica = "Es una venta atípica: no se registra en la caja del vendedor y no hay movimiento de dinero en su caja.";
        public const string AbrimosCaja = "En este ejemplo abrimos la caja: escribí el monto inicial y tocá Abrir caja.";
        public const string CajaAbierta = "La caja quedó abierta. Si no necesitás el ticket de apertura, tocá Cerrar sin imprimir.";
        public const string Codigo = "Para cargar un producto, escribí su código y presioná Enter.";
        public const string Manual = "Si no hay balanza conectada, escribí la cantidad y presioná Enter.";
        public const string ConBalanza = "Con la balanza conectada, ponés la carne en la balanza y el peso se carga solo.";
        public const string EnterBalanza = "Presioná Enter y el producto queda agregado con ese peso.";
        public const string Lector = "Con el lector de código de barras, el producto se agrega solo, con cantidad uno.";
        public const string Fin = "Para cobrar, presioná la tecla Fin, o tocá Finalizar y cobrar.";
        public const string Numeros = "Cada forma de pago tiene su número: uno efectivo, dos débito y tres crédito.";
        public const string NumerosSigue = "Cuatro cuenta corriente, cinco QR y seis transferencia. También podés tocar el botón.";
        public const string Mixto = "Si el cliente paga con dos medios, activá el pago mixto con la tecla más, o tocando el interruptor.";
        public const string MixtoPartes = "Siempre es una parte en efectivo y otra con alguno de los medios habilitados. Elegí el segundo con su número: el dos es débito.";
        public const string MixtoEfectivo = "Escribí el importe en efectivo y el del otro medio se calcula solo, hasta llegar al total.";
        public const string MixtoOtro = "O escribí el importe del otro medio, y se calcula el efectivo.";
        public const string MixtoFuera = "En este ejemplo cobramos todo en efectivo: con la tecla más se desactiva el pago mixto.";
        public const string ClicEfectivo = "Y tocá Efectivo.";
        public const string Opciones = "La venta quedó registrada. Cada opción tiene su número: uno, nueva venta; dos, ticket; tres, PDF; cuatro, email; y cinco, factura electrónica.";
        public const string Uno = "Con el uno no se imprime nada y queda lista la próxima venta.";
        public const string Ancho = "El dos imprime el ticket en la impresora térmica. Con cambiar elegís el ancho del papel: cincuenta y ocho u ochenta milímetros.";
        public const string Dos = "Presioná el dos.";
        public const string TicketListo = "Se imprime el ticket y el punto de venta queda listo para la siguiente venta.";
        public const string Volvamos = "Volvamos a la misma venta para ver las otras opciones.";
        public const string Tres = "El tres genera el comprobante en PDF, para una impresora común.";
        public const string Cuatro = "El cuatro lo manda por email. Escribí el email del cliente: el asunto y el mensaje ya vienen completos.";
        public const string Enviar = "Tocá Enviar email.";
        public const string Cinco = "El cinco abre la factura electrónica de la venta.";
        public const string FacturaEnter = "Revisá los datos y registrá la factura con Alt más R, o tocando Registrar factura.";
        public const string FacturaLista = "La factura quedó registrada con su número. Ahora la podés imprimir, ver en PDF o mandar por email.";
        public const string FacturaTicket = "El dos imprime el ticket de la factura.";
        public const string FacturaPdf = "El tres muestra la factura en PDF.";
        public const string FacturaEmail = "Y el cuatro la manda por email.";
        public const string Listo = "Listo. Así se completa una venta en el punto de venta.";

        public static readonly string[] Todas =
        {
            Entrar, Apertura, SinCaja, SinCajaAtipica, AbrimosCaja, CajaAbierta, Codigo, Manual, ConBalanza, EnterBalanza,
            Lector, Fin, Numeros, NumerosSigue, Mixto, MixtoPartes, MixtoEfectivo, MixtoOtro, MixtoFuera, ClicEfectivo, Opciones, Uno, Ancho, Dos,
            TicketListo, Volvamos, Tres, Cuatro, Enviar, Cinco, FacturaEnter, FacturaLista, FacturaTicket, FacturaPdf,
            FacturaEmail, Listo
        };
    }

    public async Task EjecutarAsync(IBrowser browser, Config cfg, IReadOnlyList<string> args)
    {
        string tema = args.Count > 0 ? args[0].ToLowerInvariant() : "";
        string[] temas = { "venta", "cliente", "ctacte", "egreso", "recorrido", "lineas", "misventas", "borradores" };
        if (!temas.Contains(tema))
            throw new ArgumentException($"Tema del tutorial desconocido. Disponibles: {string.Join(", ", temas)}");

        var voz = PiperSintetizador.DesdeEntorno(PosComun.VelocidadVoz);
        await using var contexto = await PosComun.NuevoContextoAsync(browser);
        var balanza = new BalanzaSimulada { Conectada = false };
        await balanza.InstalarAsync(contexto);
        await contexto.AddInitScriptAsync(BalanzaSimulada.JsEscenas);
        // Total fijo en "Venta completada" al reabrir la misma venta (ver FijarTotalAsync): el observer lo repone
        // apenas el POS lo reescribe, asi no se llega a ver $ 0,00 ni un instante.
        await contexto.AddInitScriptAsync(@"(function () {
            function fijar() {
                var t = sessionStorage.getItem('__totalFijo'), e = document.getElementById('pvbTotalVenta');
                if (t && e && e.textContent !== t) e.textContent = t;
            }
            new MutationObserver(fijar).observe(document, { childList: true, subtree: true, characterData: true });
        })();");
        var page = await contexto.NewPageAsync();

        await PosComun.IrAlInicioAsync(page, cfg);
        // "cliente" arranca con la caja abierta, una venta previa a cuenta corriente de la clienta (para que tenga saldo
        // y compras en el historial) y los productos de la venta nueva ya cargados: todo fuera de camara.
        if (tema == "cliente") await PrepararClienteAsync(page);
        // "ctacte" arranca en el POS vacio con la caja abierta y la clienta con una deuda (aparece en F2).
        if (tema == "ctacte") await PrepararDeudaClienteAsync(page);
        // "egreso" arranca en el POS vacio con la caja abierta.
        if (tema == "egreso") await PosComun.EntrarAlPosAsync(page, null);
        if (tema == "recorrido") await PrepararRecorridoAsync(page);
        // "lineas" arranca con la caja abierta y tres productos ya cargados en el carrito.
        if (tema == "lineas") await PrepararLineasAsync(page);
        // "misventas" arranca con la caja abierta y dos ventas hechas (una a consumidor final y otra a la clienta).
        (string, string) ventasPrevias = ("", "");
        if (tema == "misventas") ventasPrevias = await PrepararMisVentasAsync(page);
        // "borradores" arranca con la caja abierta y una venta a la clienta cargada en el carrito, sin cobrar.
        if (tema == "borradores") await PrepararBorradoresAsync(page);

        string carpeta = cfg.CarpetaPantalla("Ventas.POS");
        await using var grabador = PosComun.NuevoGrabador(page, voz);
        await grabador.PrepararVocesAsync(tema switch { "venta" => Frases.Todas, "cliente" => FrasesCliente.Todas, "ctacte" => FrasesCtaCte.Todas, "egreso" => FrasesEgreso.Todas, "lineas" => FrasesLineas.Todas, "misventas" => FrasesMisVentas.Todas, "borradores" => FrasesBorradores.Todas, _ => FrasesRecorrido.Todas });
        await page.EvaluateAsync("() => sessionStorage.removeItem('__fugaFicticios')");
        await grabador.IniciarAsync();
        await page.WaitForTimeoutAsync(700);

        string idVenta;
        try
        {
        if (tema == "venta")
        {
            await AbrirCajaAsync(page, grabador);
            await CargarAsync(page, grabador, balanza);
            await CobrarAsync(page, grabador);
            idVenta = await PosComun.EsperarPostVentaAsync(page);
            await PostVentaAsync(page, grabador, idVenta);
        }
        else if (tema == "cliente")
        {
            idVenta = await GrabarClienteAsync(page, grabador);
        }
        else if (tema == "ctacte")
        {
            await GrabarCtaCteAsync(page, grabador);
            idVenta = "-";
        }
        else if (tema == "egreso")
        {
            await GrabarEgresoAsync(page, grabador);
            idVenta = "-";
        }
        else if (tema == "lineas")
        {
            idVenta = await GrabarLineasAsync(page, grabador);
        }
        else if (tema == "borradores")
        {
            idVenta = await GrabarBorradoresAsync(page, grabador);
        }
        else if (tema == "misventas")
        {
            await GrabarMisVentasAsync(page, grabador, ventasPrevias.Item1, ventasPrevias.Item2);
            idVenta = $"{ventasPrevias.Item1} y {ventasPrevias.Item2}";
        }
        else
        {
            await GrabarRecorridoAsync(page, grabador);
            idVenta = "-";
        }
        }
        catch (Exception)
        {
            // Si un paso falla, deja una captura de como quedo la pantalla (para diagnosticar) y sigue el error.
            string captura = Path.Combine(carpeta, $"tutorial-{tema}-error.png");
            await page.ScreenshotAsync(new PageScreenshotOptions { Path = captura });
            Console.WriteLine($"Captura del error: {captura}");
            throw;
        }

        await Ficticios.VerificarSinFugasAsync(page);
        await grabador.FinalizarAsync(Path.Combine(carpeta, $"tutorial-{tema}.mp4"));
        Console.WriteLine($"Venta de prueba: id {idVenta}. tutorial-{tema}.mp4  {new FileInfo(Path.Combine(carpeta, $"tutorial-{tema}.mp4")).Length / 1024} KB");
    }

    // Menu -> POS -> apertura de caja, mostrando (sin usarla) la venta rapida sin apertura.
    private static async Task AbrirCajaAsync(IPage page, Grabador g)
    {
        await page.EvaluateAsync($"() => marcar(document.querySelector(\"{PosComun.SelMenuPos}\"), 1)");
        await g.NarrarAsync(Frases.Entrar);
        await page.EvaluateAsync("() => limpiarMarcas()");
        await PosComun.MoverYClicAsync(page, PosComun.SelMenuPos);
        await page.WaitForURLAsync(url => url.Contains("/Ventas/POS"), new PageWaitForURLOptions { Timeout = 20000 });
        await page.Locator("#modalAbrirCaja").WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible, Timeout = 15000 });
        await page.WaitForTimeoutAsync(600);
        await page.EvaluateAsync("() => marcar(document.getElementById('montoInicial'), 1)");
        await g.NarrarAsync(Frases.Apertura);

        // Venta rapida sin caja: se tilda para que se vea el boton, y se destilda (el ejemplo abre la caja).
        await page.EvaluateAsync("() => { limpiarMarcas(); marcar(document.getElementById('chkOperarSinCaja').closest('.custom-control') || document.getElementById('chkOperarSinCaja'), 2); }");
        await PosComun.MoverYClicAsync(page, "label[for='chkOperarSinCaja']");
        await page.WaitForTimeoutAsync(400);
        await page.EvaluateAsync("() => marcar(document.getElementById('btnContinuarSinCaja'), 3)");
        await g.NarrarAsync(Frases.SinCaja);
        await g.NarrarAsync(Frases.SinCajaAtipica);
        await page.EvaluateAsync("() => limpiarMarcas()");
        await PosComun.MoverYClicAsync(page, "label[for='chkOperarSinCaja']");
        await page.WaitForTimeoutAsync(400);

        await g.NarrarAsync(Frases.AbrimosCaja, 200);
        await PosComun.MoverYClicAsync(page, "#montoInicial");
        await page.Keyboard.TypeAsync(PosComun.MontoInicialCaja, new KeyboardTypeOptions { Delay = 160 });
        await page.WaitForTimeoutAsync(400);
        await PosComun.MoverYClicAsync(page, "#modalAbrirCaja button:has-text('Abrir Caja')");
        await page.Locator("#btnPcCerrar").WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible, Timeout = 15000 });
        await page.WaitForTimeoutAsync(500);
        await g.NarrarAsync(Frases.CajaAbierta);
        await page.RunAndWaitForNavigationAsync(() => PosComun.MoverYClicAsync(page, "#btnPcCerrar"),
            new PageRunAndWaitForNavigationOptions { WaitUntil = WaitUntilState.DOMContentLoaded, Timeout = 20000 });
        await PosComun.EsperarPosListoAsync(page);
    }

    // Carga: manual (sin balanza), con balanza simulada y con el lector de codigo de barras.
    private static async Task CargarAsync(IPage page, Grabador g, BalanzaSimulada balanza)
    {
        // 1) Manual: el agente de balanza no responde.
        await page.EvaluateAsync("() => marcar(document.getElementById('inputCodigo'), 1)");
        await g.NarrarAsync(Frases.Codigo, 200);
        await page.EvaluateAsync("() => limpiarMarcas()");
        await PosComun.MoverYClicAsync(page, "#inputCodigo");
        await page.Keyboard.TypeAsync("9901", new KeyboardTypeOptions { Delay = 170 });
        await PosComun.ApretarAsync(page, "Enter", "Enter");
        await page.WaitForFunctionAsync("() => document.activeElement && document.activeElement.id === 'inputCantidad'");
        await page.WaitForTimeoutAsync(500);
        await g.NarrarAsync(Frases.Manual, 100);
        await page.Keyboard.TypeAsync("1.25", new KeyboardTypeOptions { Delay = 170 });
        await page.WaitForTimeoutAsync(300);
        await PosComun.ApretarAsync(page, "Enter", "Enter");
        await page.WaitForTimeoutAsync(1100);

        // 2) Con balanza: el agente pasa a responder; el POS lo detecta en su consulta periodica (cada 3 s).
        balanza.Peso = 0;
        balanza.Conectada = true;
        await page.WaitForFunctionAsync("() => /Conectada/i.test(document.getElementById('estadoBalanza')?.textContent || '')",
            null, new PageWaitForFunctionOptions { Timeout = 15000, PollingInterval = 250 });
        await page.EvaluateAsync("() => marcar(document.getElementById('estadoBalanza'), 1)");
        await g.NarrarAsync(Frases.ConBalanza, 100);
        await page.EvaluateAsync("() => limpiarMarcas()");
        await PosComun.MoverYClicAsync(page, "#inputCodigo");
        await page.Keyboard.TypeAsync("9902", new KeyboardTypeOptions { Delay = 170 });
        await PosComun.ApretarAsync(page, "Enter", "Enter");
        await page.WaitForTimeoutAsync(500);
        var escena = page.EvaluateAsync("() => window.__balanza(0.8)");
        await page.WaitForTimeoutAsync(1200);
        balanza.Peso = 0.8;
        await escena;
        await page.WaitForFunctionAsync("() => parseFloat((document.getElementById('inputCantidad')?.value || '0').replace(',', '.')) > 0",
            null, new PageWaitForFunctionOptions { Timeout = 10000, PollingInterval = 200 });
        await page.WaitForTimeoutAsync(500);
        await g.NarrarAsync(Frases.EnterBalanza, 100);
        await PosComun.ApretarAsync(page, "Enter", "Enter");
        await page.WaitForTimeoutAsync(900);
        balanza.Peso = 0;
        await page.EvaluateAsync("() => window.__perifericoOcultar()");

        // 3) Lector: muestra la etiqueta y el laser y "tipea" el EAN + Enter como un lector de mano.
        await g.NarrarAsync(Frases.Lector, 100);
        await PosComun.MoverYClicAsync(page, "#inputCodigo");
        await page.EvaluateAsync("(a) => window.__lector(a.ean, a.nombre)", new { ean = EanEjemplo, nombre = NombreEanEjemplo });
        await page.Keyboard.TypeAsync(EanEjemplo, new KeyboardTypeOptions { Delay = 12 });
        await page.Keyboard.PressAsync("Enter");
        await page.WaitForFunctionAsync("() => document.querySelectorAll('#tablaItems tr.fila-item').length >= 3",
            null, new PageWaitForFunctionOptions { Timeout = 10000, PollingInterval = 200 });
        await page.WaitForTimeoutAsync(1200);
        await page.EvaluateAsync("() => window.__perifericoOcultar()");
        await page.Mouse.MoveAsync(PosComun.AnchoVista / 2f, PosComun.AltoVista / 2f);
    }

    // Fin -> formas de pago con sus numeros -> pago mixto explicado y desactivado -> clic en Efectivo.
    private static async Task CobrarAsync(IPage page, Grabador g)
    {
        await page.EvaluateAsync("() => marcar(document.getElementById('btnFinalizar'), 1)");
        await g.NarrarAsync(Frases.Fin, 200);
        await page.EvaluateAsync("() => limpiarMarcas()");
        await PosComun.ApretarAsync(page, "End", "Fin");
        await page.Locator("#modalFormaPago").WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        await page.WaitForTimeoutAsync(600);
        await g.NarrarAsync(Frases.Numeros);
        await g.NarrarAsync(Frases.NumerosSigue);

        await page.EvaluateAsync("() => marcar(document.getElementById('chkPagoMixto').closest('label, .form-check, .custom-control, .form-switch') || document.getElementById('chkPagoMixto'), 1)");
        await g.NarrarAsync(Frases.Mixto, 200);
        await page.EvaluateAsync("() => limpiarMarcas()");
        await PosComun.ApretarAsync(page, "+", "+");
        await page.Locator("#bloquePagoMixto").WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        await page.WaitForTimeoutAsync(600);
        await g.NarrarAsync(Frases.MixtoPartes, 200);
        await PosComun.ApretarAsync(page, "2", "2");
        await page.WaitForFunctionAsync("() => document.activeElement && document.activeElement.id === 'montoEfectivo'");
        await page.WaitForTimeoutAsync(400);
        await g.NarrarAsync(Frases.MixtoEfectivo, 100);
        await page.Keyboard.TypeAsync("10000", new KeyboardTypeOptions { Delay = 170 });
        await page.WaitForTimeoutAsync(800);
        await g.NarrarAsync(Frases.MixtoOtro, 100);
        await PosComun.MoverYClicAsync(page, "#montoOtroPago");
        await page.Keyboard.PressAsync("Control+A");
        await page.Keyboard.TypeAsync("8000", new KeyboardTypeOptions { Delay = 170 });
        await page.WaitForTimeoutAsync(900);
        await g.NarrarAsync(Frases.MixtoFuera, 200);
        await PosComun.ApretarAsync(page, "+", "+");
        await page.Locator("#bloquePagoMixto").WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Hidden });
        await page.WaitForTimeoutAsync(500);
        await g.NarrarAsync(Frases.ClicEfectivo, 100);
        await PosComun.MoverYClicAsync(page, "#modalFormaPago button:has-text('Efectivo')");
    }

    // "Venta completada": todas las opciones; despues del ticket, el PDF y el email se vuelve a abrir la misma venta.
    private static async Task PostVentaAsync(IPage page, Grabador g, string idVenta)
    {
        _totalVenta = await page.EvaluateAsync<string>("() => (document.getElementById('pvbTotalVenta')?.textContent || '').trim()");
        await g.NarrarAsync(Frases.Opciones);
        await page.EvaluateAsync("() => marcar(document.getElementById('btnPvbContinuar'), 1)");
        await g.NarrarAsync(Frases.Uno);

        // Ticket: ancho del papel y el 2.
        await page.EvaluateAsync("() => { limpiarMarcas(); marcar(document.getElementById('pvbTicketCambiarMedida'), 2); }");
        await g.NarrarAsync(Frases.Ancho, 100);
        await page.EvaluateAsync("() => limpiarMarcas()");
        await PosComun.MoverYClicAsync(page, "#pvbTicketCambiarMedida");
        await page.Locator(".swal2-popup").WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        await page.WaitForTimeoutAsync(1500);
        await PosComun.MoverYClicAsync(page, ".swal2-confirm");
        await page.WaitForFunctionAsync("() => !document.querySelector('.swal2-popup')", null, new PageWaitForFunctionOptions { Timeout = 10000 });
        await page.WaitForTimeoutAsync(500);
        await g.NarrarAsync(Frases.Dos, 100);
        await PosComun.ApretarAsync(page, "2", "2");
        await page.Mouse.MoveAsync(PosComun.AnchoVista - 120, 140, new MouseMoveOptions { Steps = 20 });
        await page.WaitForFunctionAsync("() => !!window.__impresionEnCurso", null, new PageWaitForFunctionOptions { Timeout = 15000 });
        await PosComun.EsperarVentaNuevaAsync(page);
        await PosComun.EsperarPosListoAsync(page);
        await g.NarrarAsync(Frases.TicketListo);

        // PDF.
        await ReabrirAsync(page, g, idVenta, Frases.Volvamos);
        await g.NarrarAsync(Frases.Tres, 100);
        await PosComun.ApretarAsync(page, "3", "3");
        await MostrarPdfPendienteAsync(page);

        // Email.
        await ReabrirAsync(page, g, idVenta, null);
        await g.NarrarAsync(Frases.Cuatro, 100);
        await PosComun.ApretarAsync(page, "4", "4");
        await EnviarEmailAsync(page, g);

        // Factura electronica: Alt+R registra; despues ticket, PDF y email de la factura.
        await ReabrirAsync(page, g, idVenta, null);
        await g.NarrarAsync(Frases.Cinco, 100);
        await PosComun.ApretarAsync(page, "5", "5");
        await RegistrarFacturaConAtajoAsync(page, g);
        await FijarTotalAsync(page);
        await g.NarrarAsync(Frases.FacturaLista);
        await g.NarrarAsync(Frases.FacturaTicket, 100);
        await PosComun.ApretarAsync(page, "2", "2");
        await page.Mouse.MoveAsync(PosComun.AnchoVista - 120, 140, new MouseMoveOptions { Steps = 20 });
        await page.WaitForFunctionAsync("() => !!window.__impresionEnCurso", null, new PageWaitForFunctionOptions { Timeout = 15000 });
        await PosComun.EsperarVentaNuevaAsync(page);
        await PosComun.EsperarPosListoAsync(page);

        await ReabrirAsync(page, g, idVenta, null);
        await g.NarrarAsync(Frases.FacturaPdf, 100);
        await PosComun.ApretarAsync(page, "3", "3");
        await MostrarPdfPendienteAsync(page);

        await ReabrirAsync(page, g, idVenta, null);
        await g.NarrarAsync(Frases.FacturaEmail, 100);
        await PosComun.ApretarAsync(page, "4", "4");
        await EnviarEmailAsync(page, g);
        await g.NarrarAsync(Frases.Listo);
        await page.WaitForTimeoutAsync(800);
    }

    // Vuelve a abrir "Venta completada" de la misma venta (corte de edicion, decidido con el usuario).
    private static async Task ReabrirAsync(IPage page, Grabador g, string idVenta, string? frase)
    {
        if (frase != null) await g.NarrarAsync(frase, 100);
        await page.EvaluateAsync("(id) => window.mostrarModalPostVenta(id)", idVenta);
        await page.Locator("#modalPostVentaBasico").WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible, Timeout = 15000 });
        await FijarTotalAsync(page);
        await page.WaitForTimeoutAsync(900);
    }

    // Total de la venta tal como se vio al cobrar. Al reabrir la ventana, el POS calcula el total con el carrito (ya
    // vacio) y mostraria $ 0,00: en la grabacion se repone el total real de la venta.
    private static string _totalVenta = "";
    private static async Task FijarTotalAsync(IPage page)
    {
        if (!string.IsNullOrEmpty(_totalVenta))
            await page.EvaluateAsync("(t) => { sessionStorage.setItem('__totalFijo', t); var e = document.getElementById('pvbTotalVenta'); if (e) e.textContent = t; }", _totalVenta);
    }

    // El POS abrio el PDF (interceptado) y recargo: se muestra la hoja saliendo de la impresora dibujada.
    private static async Task MostrarPdfPendienteAsync(IPage page)
    {
        await page.Mouse.MoveAsync(PosComun.AnchoVista - 120, 140, new MouseMoveOptions { Steps = 20 });
        // Con factura el POS puede preguntar que comprobante imprimir: se confirma la opcion propuesta.
        try
        {
            await page.Locator(".swal2-confirm").WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible, Timeout = 2500 });
            await PosComun.MoverYClicAsync(page, ".swal2-confirm");
        }
        catch (TimeoutException)
        {
            // Sin pregunta: el PDF se abre directo (venta sin factura o factura sin agrupar).
        }
        await PosComun.EsperarVentaNuevaAsync(page);
        await PosComun.EsperarPosListoAsync(page);
        string? urlPdf = await page.EvaluateAsync<string?>("() => window.__pdfPendiente()");
        if (string.IsNullOrEmpty(urlPdf))
            throw new InvalidOperationException("El POS no pidio el PDF (no paso por window.open).");
        await page.EvaluateAsync("(u) => window.__prepararPdf(u)", urlPdf);
        await page.EvaluateAsync("() => window.__mostrarPdf()");
        await page.WaitForTimeoutAsync(2200);
        await page.EvaluateAsync("() => window.__cerrarImpresion()");
        await page.WaitForTimeoutAsync(400);
    }

    // Modal de email: destino ficticio y Enviar (la respuesta la da el navegador: no sale ningun mail).
    private static async Task EnviarEmailAsync(IPage page, Grabador g)
    {
        await page.Locator("#modalEmailPostVentaPOS").WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        await page.WaitForTimeoutAsync(600);
        await PosComun.MoverYClicAsync(page, "#pvbEmailDestino");
        await page.Keyboard.PressAsync("Control+A");
        await page.Keyboard.TypeAsync(PosComun.EmailEjemplo, new KeyboardTypeOptions { Delay = 60 });
        await page.WaitForTimeoutAsync(400);
        await g.NarrarAsync(Frases.Enviar, 100);
        await PosComun.MoverYClicAsync(page, "#btnPvbConfirmarEmail");
        await page.Locator(".swal2-popup").WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible, Timeout = 15000 });
        await PosComun.EsperarVentaNuevaAsync(page);
        await PosComun.EsperarPosListoAsync(page);
    }

    // Factura electronica registrada con Alt+R. Freno de seguridad:
    // sin el banner de modo prueba NO se registra (la empresa estaria en produccion).
    private static async Task RegistrarFacturaConAtajoAsync(IPage page, Grabador g)
    {
        await page.Locator("#modalFacturaElectronica").WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible, Timeout = 20000 });
        await page.Locator("#btnRegistrarFactura").WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible, Timeout = 20000 });
        if (await page.Locator("#feBannerModoPrueba").CountAsync() == 0)
            throw new InvalidOperationException("La factura no esta en modo prueba (homologacion): NO se registra. Poner la empresa de desarrollo en HOMO.");
        await page.EvaluateAsync("() => { var b = document.getElementById('feBannerModoPrueba'); if (b) b.style.display = 'none'; }");
        await page.WaitForTimeoutAsync(1000);
        await g.NarrarAsync(Frases.FacturaEnter, 200);
        // Alt+R registra (atajo de factura-electronica.js, escrito en el pie del modal). Enter dentro de los campos NO
        // registra a proposito (el modal lo bloquea para no emitir sin querer).
        await PosComun.ApretarAsync(page, "Alt+r", "Alt + R");
        await page.WaitForFunctionAsync(
            "() => !!document.querySelector('#modalPostVentaBasico.show') && !document.getElementById('pvbFacturaRegistrada').classList.contains('d-none')",
            null, new PageWaitForFunctionOptions { Timeout = 90000, PollingInterval = 300 });
        await page.WaitForTimeoutAsync(900);
    }

    // ===================== Tema "cliente": cliente, cuenta corriente y cobro con la venta =====================

    private static class FrasesCliente
    {
        public const string F9 = "Para venderle a un cliente, presioná F9 o tocá la lupa, y buscalo por el nombre.";
        public const string Enter = "Con Enter elegís el primero de la lista.";
        public const string Saldo = "Debajo del nombre ves el saldo de su cuenta corriente.";
        public const string Cf = "Si elegiste mal, el botón CF, al lado del buscador, vuelve a consumidor final.";
        public const string Otra = "Volvemos a elegir a la clienta.";
        public const string F8 = "Con un cliente elegido aparece el botón de historial de precios, también con la tecla F8.";
        public const string Compras = "En Compras ves sus últimas compras, con el precio y la fecha de cada producto.";
        public const string Presupuestos = "En Presupuestos están los precios que le pasaste en un presupuesto.";
        public const string Copiar = "Tocá copiar en un producto que ya está en el carrito, y confirmá: queda con el precio del presupuesto.";
        public const string Cerrar = "Cerrá el historial y seguí con la venta.";
        public const string Fin = "Presioná Fin para cobrar.";
        public const string CuadroSaldo = "Arriba ves cómo quedaría su saldo si la venta va a cuenta corriente, con el cuatro.";
        public const string LetraC = "Si además paga una parte de lo que debe, presioná la letra C: el cliente paga ahora.";
        public const string Importe = "Escribí cuánto paga. Abajo ves cuánto le queda debiendo.";
        public const string PagaTodo = "O tildá Paga todo para saldar toda la cuenta.";
        public const string Forma = "Elegí con qué paga: el uno es efectivo. En efectivo, el cobro suma a tu caja.";
        public const string Quedo = "La venta quedó en su cuenta y el pago descontado. Este es el saldo que le queda.";
        public const string Ticket = "Al imprimir el ticket, sale el saldo anterior, la venta, lo que pagó y el saldo que le queda.";
        public const string Listo = "Listo. Así se le vende a un cliente con cuenta corriente.";

        public static readonly string[] Todas =
        {
            F9, Enter, Saldo, Cf, Otra, F8, Compras, Presupuestos, Copiar, Cerrar, Fin, CuadroSaldo, LetraC, Importe,
            PagaTodo, Forma, Quedo, Ticket, Listo
        };
    }

    // Fuera de camara: caja abierta (si hace falta), una venta previa de la clienta a cuenta corriente (le deja saldo
    // y compras para el historial) y los productos de la venta nueva cargados con las cantidades del presupuesto.
    private static async Task PrepararClienteAsync(IPage page)
    {
        await PrepararDeudaClienteAsync(page);
        await CargarSilencioAsync(page, ("9901", "2"), ("9902", "1"));
        await page.Mouse.MoveAsync(PosComun.AnchoVista / 2f, PosComun.AltoVista / 2f);
        await page.WaitForTimeoutAsync(800);
    }

    // Fuera de camara: caja abierta (si hace falta) y una venta de la clienta a cuenta corriente (le deja saldo, un
    // movimiento en F2 y compras en el historial). Termina con el POS vacio.
    private static async Task PrepararDeudaClienteAsync(IPage page)
    {
        await PosComun.EntrarAlPosAsync(page, null);
        await CargarSilencioAsync(page, ("9901", "1.25"));
        await ElegirClienteAsync(page, null);
        await PosComun.ApretarAsync(page, "End", "Fin", false);
        await page.Locator("#modalFormaPago").WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        await page.WaitForTimeoutAsync(600);
        await PosComun.ApretarAsync(page, "4", "4", false);
        await PosComun.EsperarPostVentaAsync(page);
        await PosComun.ApretarAsync(page, "1", "1", false);
        await PosComun.EsperarVentaNuevaAsync(page);
        await PosComun.EsperarPosListoAsync(page);
    }

    private static async Task CargarSilencioAsync(IPage page, params (string Codigo, string Cantidad)[] productos)
    {
        foreach (var (codigo, cantidad) in productos)
        {
            await page.ClickAsync("#inputCodigo");
            await page.Keyboard.TypeAsync(codigo, new KeyboardTypeOptions { Delay = 30 });
            await page.Keyboard.PressAsync("Enter");
            await page.WaitForFunctionAsync("() => document.activeElement && document.activeElement.id === 'inputCantidad'");
            await page.Keyboard.TypeAsync(cantidad, new KeyboardTypeOptions { Delay = 30 });
            await page.Keyboard.PressAsync("Enter");
            await page.WaitForTimeoutAsync(700);
        }
    }

    // F9 (o lupa) -> "Laura" -> Enter. Con grabador lo narra.
    private static async Task ElegirClienteAsync(IPage page, Grabador? g)
    {
        if (g != null) await g.NarrarAsync(FrasesCliente.F9, 200);
        await PosComun.ApretarAsync(page, "F9", "F9", g != null);
        var filtro = page.Locator("#filtroPersona");
        await filtro.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible, Timeout = 15000 });
        await page.WaitForTimeoutAsync(400);
        await filtro.PressSequentiallyAsync("Laura", new LocatorPressSequentiallyOptions { Delay = g != null ? 150 : 30 });
        await page.GetByText(PosComun.ClienteEjemplo, new PageGetByTextOptions { Exact = true }).First
            .WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible, Timeout = 15000 });
        await page.WaitForTimeoutAsync(500);
        if (g != null) await g.NarrarAsync(FrasesCliente.Enter, 100);
        await PosComun.ApretarAsync(page, "Enter", "Enter", g != null);
        await page.WaitForFunctionAsync(
            "(n) => (document.getElementById('razonSocial')?.value || document.getElementById('razonSocial')?.textContent || '').includes(n)",
            PosComun.ClienteEjemplo, new PageWaitForFunctionOptions { Timeout = 15000 });
        await page.WaitForFunctionAsync("() => !document.querySelector('.modal.show')", null, new PageWaitForFunctionOptions { Timeout = 15000 });
        await page.WaitForTimeoutAsync(800);
    }

    private static async Task<string> GrabarClienteAsync(IPage page, Grabador g)
    {
        // Elegir cliente y su saldo.
        await ElegirClienteAsync(page, g);
        await page.EvaluateAsync("() => marcar(document.getElementById('clienteSaldoWrap'), 1)");
        await g.NarrarAsync(FrasesCliente.Saldo);

        // Volver a consumidor final con CF, y elegir otra vez.
        await page.EvaluateAsync("() => { limpiarMarcas(); marcar(document.getElementById('btnConsumidorFinal'), 1); }");
        await g.NarrarAsync(FrasesCliente.Cf, 100);
        await page.EvaluateAsync("() => limpiarMarcas()");
        await PosComun.MoverYClicAsync(page, "#btnConsumidorFinal");
        await page.WaitForFunctionAsync(
            "(n) => !(document.getElementById('razonSocial')?.value || document.getElementById('razonSocial')?.textContent || '').includes(n)",
            PosComun.ClienteEjemplo, new PageWaitForFunctionOptions { Timeout = 10000 });
        await page.WaitForTimeoutAsync(900);
        await g.NarrarAsync(FrasesCliente.Otra, 100);
        await ElegirClienteAsync(page, null);

        // Historial de precios (F8): compras y presupuestos; copiar el precio del presupuesto al carrito.
        await page.EvaluateAsync("() => marcar(document.getElementById('btnHistorialPreciosCliente'), 1)");
        await g.NarrarAsync(FrasesCliente.F8, 200);
        await page.EvaluateAsync("() => limpiarMarcas()");
        await PosComun.ApretarAsync(page, "F8", "F8");
        await page.Locator("#modalFinanzasPOS").WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible, Timeout = 15000 });
        await page.Locator("#hpcTabCompras").WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible, Timeout = 15000 });
        await page.WaitForTimeoutAsync(800);
        // Con presupuestos vigentes el historial abre en la solapa Presupuestos: se muestra primero Compras.
        await PosComun.MoverYClicAsync(page, "#hpcTabCompras");
        await page.WaitForTimeoutAsync(700);
        await g.NarrarAsync(FrasesCliente.Compras);
        await g.NarrarAsync(FrasesCliente.Presupuestos, 100);
        await PosComun.MoverYClicAsync(page, "#hpcTabPresupuestos");
        await page.WaitForTimeoutAsync(900);
        const string SelCopiar = "#hpcPanePresupuestos .js-copiar-precio-historial[data-codigo='9901']";
        await page.Locator(SelCopiar).First.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible, Timeout = 10000 });
        await page.EvaluateAsync("(s) => marcar(document.querySelector(s), 1)", SelCopiar);
        await g.NarrarAsync(FrasesCliente.Copiar, 100);
        await page.EvaluateAsync("() => limpiarMarcas()");
        await PosComun.MoverYClicAsync(page, SelCopiar);
        await page.Locator(".swal2-confirm").WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible, Timeout = 10000 });
        await page.WaitForTimeoutAsync(900);
        await PosComun.MoverYClicAsync(page, ".swal2-confirm");
        await page.WaitForTimeoutAsync(2400);
        await g.NarrarAsync(FrasesCliente.Cerrar, 100);
        await PosComun.ApretarAsync(page, "Escape", "Esc");
        await page.WaitForFunctionAsync("() => !document.querySelector('.modal.show') && !document.querySelector('.swal2-popup')",
            null, new PageWaitForFunctionOptions { Timeout = 10000 });
        await page.WaitForTimeoutAsync(800);

        // Cobro: saldo que quedaria, "El cliente paga ahora" (C) con un pago parcial, en efectivo.
        await g.NarrarAsync(FrasesCliente.Fin, 100);
        await PosComun.ApretarAsync(page, "End", "Fin");
        await page.Locator("#modalFormaPago").WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        await page.Locator("#fpSaldoCtaCteBox").WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible, Timeout = 10000 });
        await page.WaitForTimeoutAsync(700);
        await page.EvaluateAsync("() => marcar(document.getElementById('fpSaldoCtaCteBox'), 1)");
        await g.NarrarAsync(FrasesCliente.CuadroSaldo);
        await page.EvaluateAsync("() => limpiarMarcas()");
        await g.NarrarAsync(FrasesCliente.LetraC, 200);
        await PosComun.ApretarAsync(page, "c", "C");
        await page.WaitForFunctionAsync("() => document.getElementById('chkCobrarSaldoFp')?.checked === true");
        await page.WaitForTimeoutAsync(700);
        await g.NarrarAsync(FrasesCliente.Importe, 100);
        bool enImporte = await page.EvaluateAsync<bool>("() => document.activeElement && document.activeElement.id === 'txtImporteCobroFp'");
        if (!enImporte) await PosComun.MoverYClicAsync(page, "#txtImporteCobroFp");
        await page.Keyboard.PressAsync("Control+A");
        await page.Keyboard.TypeAsync("30000", new KeyboardTypeOptions { Delay = 170 });
        await page.WaitForTimeoutAsync(700);
        await page.EvaluateAsync("() => marcar(document.getElementById('fpCobroSaldoQueda'), 1)");
        await page.WaitForTimeoutAsync(1500);
        await page.EvaluateAsync("() => { limpiarMarcas(); marcar(document.getElementById('chkPagaTodoFp').closest('label, .form-check, .custom-control, .form-switch') || document.getElementById('chkPagaTodoFp'), 1); }");
        await g.NarrarAsync(FrasesCliente.PagaTodo);
        await page.EvaluateAsync("() => limpiarMarcas()");
        // Los numeros eligen la forma de pago solo con el foco fuera del importe.
        await page.EvaluateAsync("() => { if (document.activeElement) document.activeElement.blur(); }");
        await g.NarrarAsync(FrasesCliente.Forma, 200);
        await PosComun.ApretarAsync(page, "1", "1");
        string idVenta = await PosComun.EsperarPostVentaAsync(page);
        // El puntero queda donde estaba el boton de la forma de pago y resaltaria un boton del post-venta.
        await page.Mouse.MoveAsync(PosComun.AnchoVista - 120, 140, new MouseMoveOptions { Steps = 15 });
        await page.EvaluateAsync("() => marcar(document.getElementById('pvbSaldoClienteWrap'), 1)");
        await g.NarrarAsync(FrasesCliente.Quedo);
        await page.EvaluateAsync("() => limpiarMarcas()");

        // Ticket con saldo: con cobro el POS lo imprime sin preguntar.
        await g.NarrarAsync(FrasesCliente.Ticket, 100);
        await PosComun.ApretarAsync(page, "2", "2");
        await page.Mouse.MoveAsync(PosComun.AnchoVista - 120, 140, new MouseMoveOptions { Steps = 20 });
        await page.WaitForFunctionAsync("() => !!window.__impresionEnCurso", null, new PageWaitForFunctionOptions { Timeout = 15000 });
        await PosComun.EsperarVentaNuevaAsync(page);
        await PosComun.EsperarPosListoAsync(page);
        await g.NarrarAsync(FrasesCliente.Listo);
        await page.WaitForTimeoutAsync(800);
        return idVenta;
    }

    // ===================== Tema "ctacte": cuentas corrientes (F2), pagos y cobros, y Mis actividades (F6) ==========

    private static class FrasesCtaCte
    {
        public const string Ayuda = "Las cuentas corrientes se abren con la tecla F2, o desde el botón Ayuda.";
        public const string PanelAyuda = "En la ayuda están todos los atajos. Tocá Cuentas corrientes.";
        public const string Buscar = "Buscá al cliente por nombre, identificación o CUIT.";
        public const string Tocar = "Tocá el cliente para ver su cuenta.";
        public const string Movimientos = "Acá ves sus últimos movimientos: ventas, pagos y cobros, con el saldo de la cuenta.";
        public const string Agregar = "Para registrar dinero que recibís o entregás, tocá Agregar pago o cobro.";
        public const string Tipos = "Recibir un cobro es cuando el cliente te da dinero. Realizar un pago, cuando vos le entregás dinero.";
        public const string Cobro = "Primero un cobro: elegí Recibir un cobro, la forma de pago y el importe.";
        public const string CobroCaja = "En efectivo, el cobro entra en tu caja. Guardá con Alt más Enter.";
        public const string Recibo = "Se guarda y podés imprimir o mandar el recibo. Con el uno se cierra sin imprimir.";
        public const string Pago = "Ahora un pago: elegí Realizar un pago y el importe.";
        public const string PagoCaja = "Un pago en efectivo sale de tu caja: queda registrado como egreso. Guardá con Alt más Enter.";
        public const string Cerrar = "Cerrá la cuenta corriente con Escape.";
        public const string F6 = "Para ver cómo quedó en tu caja, presioná F6: Mis actividades. También está en la ayuda.";
        public const string Signos = "Ahí están el pago y el cobro. El pago figura en positivo: es dinero que salió de tu caja. El cobro, en negativo: es dinero que entró.";
        public const string Filtros = "Con los filtros ves solo egresos, pagos electrónicos o cuenta corriente.";
        public const string Listo = "Listo. Así se registran pagos y cobros de cuenta corriente desde el punto de venta.";

        public static readonly string[] Todas =
        {
            Ayuda, PanelAyuda, Buscar, Tocar, Movimientos, Agregar, Tipos, Cobro, CobroCaja, Recibo, Pago, PagoCaja, Cerrar,
            F6, Signos, Filtros, Listo
        };
    }

    private static async Task GrabarCtaCteAsync(IPage page, Grabador g)
    {
        // Ayuda (F1) -> Cuentas corrientes: muestra donde esta el atajo y lo usa desde el panel.
        await page.EvaluateAsync("() => marcar(document.getElementById('btnAyuda'), 1)");
        await g.NarrarAsync(FrasesCtaCte.Ayuda, 200);
        await page.EvaluateAsync("() => limpiarMarcas()");
        await PosComun.MoverYClicAsync(page, "#btnAyuda");
        await page.Locator("#modalAyudaPOS").WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        await page.WaitForTimeoutAsync(700);
        const string SelAccionF2 = "#modalAyudaPOS .pos-help-action[data-accion='F2']";
        await page.EvaluateAsync("(s) => marcar(document.querySelector(s), 1)", SelAccionF2);
        await g.NarrarAsync(FrasesCtaCte.PanelAyuda, 100);
        await page.EvaluateAsync("() => limpiarMarcas()");
        await PosComun.MoverYClicAsync(page, SelAccionF2);

        // Lista de cuentas corrientes (solo se ve la clienta ficticia) -> buscar -> abrir su cuenta.
        await page.Locator("#txtBuscar").WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible, Timeout = 15000 });
        await page.WaitForTimeoutAsync(700);
        await g.NarrarAsync(FrasesCtaCte.Buscar, 100);
        await PosComun.MoverYClicAsync(page, "#txtBuscar");
        await page.Keyboard.TypeAsync("Laura", new KeyboardTypeOptions { Delay = 150 });
        await page.WaitForTimeoutAsync(900);
        string selFila = $"#tablaCtasCtes tr[data-id-persona='{PosComun.IdClienteEjemplo}']";
        await g.NarrarAsync(FrasesCtaCte.Tocar, 100);
        await PosComun.MoverYClicAsync(page, selFila);
        await page.Locator("#tablaPersona").WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible, Timeout = 15000 });
        await page.WaitForTimeoutAsync(800);
        await page.EvaluateAsync("() => marcar(document.getElementById('tablaPersona'), 1)");
        await g.NarrarAsync(FrasesCtaCte.Movimientos);
        await page.EvaluateAsync("() => { limpiarMarcas(); marcar(document.getElementById('btnAgregarPagoCtaCte'), 1); }");
        await g.NarrarAsync(FrasesCtaCte.Agregar, 100);
        await page.EvaluateAsync("() => limpiarMarcas()");

        // Cobro en efectivo.
        await AbrirPagoCobroAsync(page);
        await g.NarrarAsync(FrasesCtaCte.Tipos);
        await g.NarrarAsync(FrasesCtaCte.Cobro, 100);
        await ElegirTipoYImporteAsync(page, "Recibir un cobro", "5000");
        await g.NarrarAsync(FrasesCtaCte.CobroCaja, 100);
        await GuardarPagoCobroAsync(page, g);

        // Pago en efectivo.
        await AbrirPagoCobroAsync(page);
        await g.NarrarAsync(FrasesCtaCte.Pago, 100);
        await ElegirTipoYImporteAsync(page, "Realizar un pago", "2000");
        await g.NarrarAsync(FrasesCtaCte.PagoCaja, 100);
        await GuardarPagoCobroAsync(page, null);

        // Cerrar F2 y ver Mis actividades (F6).
        await g.NarrarAsync(FrasesCtaCte.Cerrar, 100);
        await PosComun.ApretarAsync(page, "Escape", "Esc");
        await page.WaitForFunctionAsync("() => !document.querySelector('.modal.show')", null, new PageWaitForFunctionOptions { Timeout = 10000 });
        await page.WaitForTimeoutAsync(700);
        await g.NarrarAsync(FrasesCtaCte.F6, 200);
        await PosComun.ApretarAsync(page, "F6", "F6");
        await page.Locator("#btnAbrirMisVentasDesdeActividades").WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible, Timeout = 15000 });
        await page.WaitForTimeoutAsync(1200);
        await page.EvaluateAsync(@"() => {
            var filas = [...document.querySelectorAll('.modal.show tbody tr')].filter(function (r) { return r.offsetParent && /Pago|Cobro/.test(r.innerText); }).slice(0, 2);
            filas.forEach(function (f, i) { marcar(f, i + 1); });
        }");
        await g.NarrarAsync(FrasesCtaCte.Signos);
        await page.EvaluateAsync("() => limpiarMarcas()");
        await page.EvaluateAsync(@"() => {
            var b = [...document.querySelectorAll('.modal.show button, .modal.show a.btn')].filter(function (x) { return x.offsetParent && /^(Todos|Egresos de caja|Pagos electr|Cta Cte)/.test(x.textContent.trim()); });
            b.forEach(function (x, i) { marcar(x, i + 1); });
        }");
        await g.NarrarAsync(FrasesCtaCte.Filtros);
        await page.EvaluateAsync("() => limpiarMarcas()");
        await g.NarrarAsync(FrasesCtaCte.Listo);
        await page.WaitForTimeoutAsync(800);
    }

    private static async Task AbrirPagoCobroAsync(IPage page)
    {
        await PosComun.MoverYClicAsync(page, "#btnAgregarPagoCtaCte");
        await page.Locator("#btnGuardarPago").WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible, Timeout = 15000 });
        await page.WaitForTimeoutAsync(800);
    }

    // Tipo (boton "Recibir un cobro" / "Realizar un pago"), forma Efectivo e importe.
    private static async Task ElegirTipoYImporteAsync(IPage page, string tipo, string importe)
    {
        var boton = page.Locator("#modalPagoPOS").GetByText(tipo, new LocatorGetByTextOptions { Exact = false }).First;
        var caja = await boton.BoundingBoxAsync() ?? throw new InvalidOperationException($"No se ubico '{tipo}'.");
        await page.Mouse.MoveAsync(caja.X + caja.Width / 2, caja.Y + caja.Height / 2, new MouseMoveOptions { Steps = 25 });
        await page.WaitForTimeoutAsync(300);
        await page.Mouse.ClickAsync(caja.X + caja.Width / 2, caja.Y + caja.Height / 2);
        await page.WaitForTimeoutAsync(600);
        await page.SelectOptionAsync("#FormaPago_", "Efectivo");
        await PosComun.MoverYClicAsync(page, "#txtImporte");
        await page.Keyboard.PressAsync("Control+A");
        await page.Keyboard.TypeAsync(importe, new KeyboardTypeOptions { Delay = 170 });
        await page.WaitForTimeoutAsync(600);
    }

    // Alt+Enter guarda; despues aparece "Cobro/Pago guardado" con las opciones del recibo: 1 = cerrar sin imprimir.
    private static async Task GuardarPagoCobroAsync(IPage page, Grabador? g)
    {
        await PosComun.ApretarAsync(page, "Alt+Enter", "Alt + Enter");
        await page.Locator("#btnPcCerrar").WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible, Timeout = 15000 });
        await page.WaitForTimeoutAsync(700);
        if (g != null) await g.NarrarAsync(FrasesCtaCte.Recibo, 100);
        await PosComun.ApretarAsync(page, "1", "1");
        await page.Locator("#modalPostComprobante").WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Hidden, Timeout = 10000 });
        await page.Locator("#btnAgregarPagoCtaCte").WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible, Timeout = 15000 });
        await page.WaitForTimeoutAsync(900);
    }

    // ===================== Tema "egreso": egreso de caja (F7), contador de billetes y detalle =====================

    private static class FrasesEgreso
    {
        public const string F7 = "Para registrar un egreso de caja, presioná F7, o entrá desde el botón Ayuda.";
        public const string Tipo = "Elegí el tipo de egreso.";
        public const string Descripcion = "Escribí una descripción.";
        public const string Contador = "Si entregás muchos billetes, tocá el botón de calcular efectivo, o Alt más B.";
        public const string Cantidades = "Cargá solo la cantidad de cada billete: el total se calcula solo.";
        public const string Aceptar = "Tocá Aceptar y el total pasa al importe del egreso.";
        public const string Detalle = "Si hace falta más información, tildá Detalle y escribila.";
        public const string Guardar = "Tocá Guardar egreso.";
        public const string Recibo = "El egreso queda guardado. Con el uno se cierra sin imprimir el comprobante.";
        public const string MisActividades = "El egreso queda en tu caja: lo ves en Mis actividades, con la tecla F6.";
        public const string Listo = "Listo. Así se registra un egreso de caja.";

        public static readonly string[] Todas = { F7, Tipo, Descripcion, Contador, Cantidades, Aceptar, Detalle, Guardar, Recibo, MisActividades, Listo };
    }

    private static async Task GrabarEgresoAsync(IPage page, Grabador g)
    {
        await page.EvaluateAsync("() => marcar(document.getElementById('btnAyuda'), 1)");
        await g.NarrarAsync(FrasesEgreso.F7, 200);
        await page.EvaluateAsync("() => limpiarMarcas()");
        await PosComun.ApretarAsync(page, "F7", "F7");
        await page.Locator("#egresoMonto").WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible, Timeout = 15000 });
        await page.WaitForTimeoutAsync(800);

        await g.NarrarAsync(FrasesEgreso.Tipo, 100);
        await PosComun.MoverYClicAsync(page, "#idTipoEgresoCaja");
        await page.WaitForTimeoutAsync(400);
        await page.SelectOptionAsync("#idTipoEgresoCaja", new SelectOptionValue { Label = "Flete" });
        await page.WaitForTimeoutAsync(600);
        await g.NarrarAsync(FrasesEgreso.Descripcion, 100);
        await PosComun.MoverYClicAsync(page, "#egresoDescripcion");
        await page.Keyboard.TypeAsync("Flete de mercadería", new KeyboardTypeOptions { Delay = 70 });
        await page.WaitForTimeoutAsync(500);

        // Contador de billetes: 2 de $ 20.000, 1 de $ 10.000 y 3 de $ 2.000.
        await page.EvaluateAsync("() => marcar(document.getElementById('btnCalculadoraBilletesEgreso'), 1)");
        await g.NarrarAsync(FrasesEgreso.Contador, 200);
        await page.EvaluateAsync("() => limpiarMarcas()");
        await PosComun.ApretarAsync(page, "Alt+b", "Alt + B");
        await page.Locator("#modalCalculadoraBilletes").WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible, Timeout = 10000 });
        await page.WaitForTimeoutAsync(800);
        await g.NarrarAsync(FrasesEgreso.Cantidades, 100);
        foreach (var (denominacion, cantidad) in new[] { ("20000", "2"), ("10000", "1"), ("2000", "3") })
        {
            await PosComun.MoverYClicAsync(page, $"#modalCalculadoraBilletes input[data-denominacion='{denominacion}']");
            await page.Keyboard.TypeAsync(cantidad, new KeyboardTypeOptions { Delay = 150 });
            await page.WaitForTimeoutAsync(500);
        }
        await page.WaitForTimeoutAsync(800);
        await g.NarrarAsync(FrasesEgreso.Aceptar, 100);
        await PosComun.MoverYClicAsync(page, "#btnAceptarCalculadoraBilletes");
        await page.Locator("#modalCalculadoraBilletes").WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Hidden, Timeout = 10000 });
        await page.WaitForTimeoutAsync(900);

        // Detalle opcional.
        await g.NarrarAsync(FrasesEgreso.Detalle, 100);
        await PosComun.MoverYClicAsync(page, "label[for='egresoUsarDetalle']");
        await page.WaitForTimeoutAsync(600);
        var detalle = page.Locator("#modalFinanzasPOS textarea:visible").First;
        await detalle.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible, Timeout = 5000 });
        await detalle.ClickAsync();
        await page.Keyboard.TypeAsync("Pago al transportista", new KeyboardTypeOptions { Delay = 70 });
        await page.WaitForTimeoutAsync(600);

        // Guardar: puede aparecer la ventana del comprobante (1 = cerrar sin imprimir).
        await g.NarrarAsync(FrasesEgreso.Guardar, 100);
        await PosComun.MoverYClicAsync(page, "#btnGuardarEgresoCaja");
        await page.WaitForFunctionAsync(
            "() => (document.getElementById('btnPcCerrar') && document.getElementById('btnPcCerrar').offsetParent) || !document.getElementById('egresoMonto') || !document.getElementById('egresoMonto').offsetParent",
            null, new PageWaitForFunctionOptions { Timeout = 20000, PollingInterval = 200 });
        await page.WaitForTimeoutAsync(800);
        if (await page.Locator("#btnPcCerrar").IsVisibleAsync())
        {
            await g.NarrarAsync(FrasesEgreso.Recibo, 100);
            await PosComun.ApretarAsync(page, "1", "1");
            await page.WaitForTimeoutAsync(1000);
        }
        // Cierra lo que quede abierto y muestra el egreso en Mis actividades.
        for (int i = 0; i < 3 && await page.Locator(".modal.show").CountAsync() > 0; i++)
        {
            await page.Keyboard.PressAsync("Escape");
            await page.WaitForTimeoutAsync(600);
        }
        await g.NarrarAsync(FrasesEgreso.MisActividades, 200);
        await PosComun.ApretarAsync(page, "F6", "F6");
        await page.Locator("#btnAbrirMisVentasDesdeActividades").WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible, Timeout = 15000 });
        await page.WaitForTimeoutAsync(1000);
        await page.EvaluateAsync(@"() => {
            var f = [...document.querySelectorAll('.modal.show tbody tr')].find(function (r) { return r.offsetParent && /Flete/.test(r.innerText); });
            if (f) marcar(f, 1);
        }");
        await g.NarrarAsync(FrasesEgreso.Listo);
        await page.EvaluateAsync("() => limpiarMarcas()");
        await page.WaitForTimeoutAsync(800);
    }

    // ===================== Tema "recorrido": la pantalla del punto de venta, de punta a punta =====================

    private static class FrasesRecorrido
    {
        public const string Empresa = "Arriba a la izquierda está el nombre de la empresa.";
        public const string Sucursal = "A la derecha, la sucursal donde estás vendiendo, y la fecha y hora de la venta.";
        public const string Usuario = "Arriba, tu usuario.";
        public const string Duplicar = "Duplicar POS abre otra ventana del punto de venta, para atender a dos clientes a la vez sin que uno espere. Con el mismo botón volvés a la otra.";
        public const string Cancelamos = "En este ejemplo no la abrimos.";
        public const string Salir = "Y Salir cierra el punto de venta.";
        public const string Temas = "Este botón cambia los colores: claro, gris u oscuro.";
        public const string Gris = "Gris.";
        public const string Oscuro = "Oscuro.";
        public const string Claro = "Y claro, que usamos para vender en este ejemplo.";
        public const string Clientes = "Con la lupa, o la tecla F9, buscás un cliente. El botón CF vuelve a consumidor final.";
        public const string Buscador = "Si no sabés el código de un producto, tocá Buscar, o presioná F10, y buscalo por el nombre.";
        public const string ElegirProducto = "Hacé doble clic en el producto para elegirlo. Después, la cantidad y Enter.";
        public const string Comentario = "Comentario agrega una nota a la venta, por ejemplo, cuándo la retiran.";
        public const string Guardar = "Escribila y tocá Guardar. El botón queda marcado.";
        public const string Vender = "Cobramos con Fin y el uno, efectivo.";
        public const string Nueva = "Y con el uno, la próxima venta.";
        public const string Cancelar = "Para descartar una venta en curso, tocá Cancelar venta y confirmá.";
        public const string F1 = "En Ayuda, o con F1, están todos los atajos del punto de venta. Con Escape se cierra.";
        public const string F6 = "Con F6, Mis actividades, ves los movimientos de tu caja: egresos, pagos, cobros y cuenta corriente.";
        public const string MisVentas = "Y en Mis ventas, todas las ventas desde que abriste la caja, con todas sus formas de pago.";
        public const string Filtro = "Podés buscar por cliente o filtrar por forma de pago.";
        public const string Detalle = "Tocá una venta para ver su detalle.";
        public const string Listo = "Listo. Ese es el recorrido por el punto de venta.";

        public static readonly string[] Todas =
        {
            Empresa, Sucursal, Usuario, Duplicar, Cancelamos, Salir, Temas, Gris, Oscuro, Claro, Clientes, Buscador,
            ElegirProducto, Comentario, Guardar, Vender, Nueva, Cancelar, F1, F6, MisVentas, Filtro, Detalle, Listo
        };
    }

    // Fuera de camara: caja abierta y ventas del operador con distintas formas de pago (efectivo, debito, credito, QR y
    // transferencia). Las que piden factura obligatoria se facturan en HOMOLOGACION (corta si no ve el modo prueba).
    // Los pagos, cobros y egresos salen de los videos "ctacte" y "egreso" (grabarlos antes sin limpiar la base).
    private static async Task PrepararRecorridoAsync(IPage page)
    {
        await PosComun.EntrarAlPosAsync(page, null);
        foreach (string forma in new[] { "1", "2", "3", "5", "6" })
        {
            await CargarSilencioAsync(page, ("9902", "0.5"));
            await PosComun.ApretarAsync(page, "End", "Fin", false);
            await page.Locator("#modalFormaPago").WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
            await page.WaitForTimeoutAsync(500);
            await PosComun.ApretarAsync(page, forma, forma, false);
            // Debito, credito, QR y transferencia abren la factura obligatoria.
            await page.WaitForFunctionAsync(
                "() => (document.querySelector('#modalFacturaElectronica.show') && document.getElementById('btnRegistrarFactura')?.offsetParent) || document.querySelector('#modalPostVentaBasico.show')",
                null, new PageWaitForFunctionOptions { Timeout = 30000, PollingInterval = 200 });
            if (await page.Locator("#modalFacturaElectronica.show").CountAsync() > 0)
            {
                if (await page.Locator("#feBannerModoPrueba").CountAsync() == 0)
                    throw new InvalidOperationException("La factura no esta en modo prueba (homologacion): NO se registra.");
                await page.WaitForTimeoutAsync(600);
                await page.Keyboard.PressAsync("Alt+r");
            }
            await PosComun.EsperarPostVentaAsync(page, 90000);
            await PosComun.ApretarAsync(page, "1", "1", false);
            await PosComun.EsperarVentaNuevaAsync(page);
            await PosComun.EsperarPosListoAsync(page);
        }
        await page.Mouse.MoveAsync(PosComun.AnchoVista / 2f, PosComun.AltoVista / 2f);
        await page.WaitForTimeoutAsync(800);
    }

    private static async Task MarcarYNarrarAsync(IPage page, Grabador g, string jsElemento, string frase)
    {
        await page.EvaluateAsync($"() => {{ limpiarMarcas(); var e = {jsElemento}; if (e) marcar(e, 1); }}");
        await g.NarrarAsync(frase);
        await page.EvaluateAsync("() => limpiarMarcas()");
    }

    private static async Task GrabarRecorridoAsync(IPage page, Grabador g)
    {
        // Encabezado.
        await MarcarYNarrarAsync(page, g, "document.querySelector('.pos-layout-brand')", FrasesRecorrido.Empresa);
        await page.EvaluateAsync(@"() => {
            var s = [...document.querySelectorAll('body *')].find(function (e) { return e.children.length <= 2 && /^SUCURSAL/i.test((e.innerText || '').trim()) && e.offsetParent; });
            if (s) marcar(s, 1);
            marcar(document.getElementById('fechaHoraPOSChip'), 2);
        }");
        await g.NarrarAsync(FrasesRecorrido.Sucursal);
        await MarcarYNarrarAsync(page, g,
            "[...document.querySelectorAll('nav a, nav button')].find(function (e) { return /Mart/.test(e.innerText || '') && e.offsetParent; })",
            FrasesRecorrido.Usuario);

        // Duplicar POS (se muestra la confirmacion y se cancela) y Salir.
        await page.EvaluateAsync("() => marcar(document.getElementById('btnDuplicarPOS'), 1)");
        await PosComun.MoverYClicAsync(page, "#btnDuplicarPOS");
        await page.WaitForTimeoutAsync(900);
        await g.NarrarAsync(FrasesRecorrido.Duplicar);
        await page.EvaluateAsync("() => limpiarMarcas()");
        if (await page.Locator(".swal2-cancel").IsVisibleAsync())
        {
            await g.NarrarAsync(FrasesRecorrido.Cancelamos, 100);
            await PosComun.MoverYClicAsync(page, ".swal2-cancel");
            await page.WaitForTimeoutAsync(700);
        }
        await MarcarYNarrarAsync(page, g, "document.getElementById('btnSalirPOS')", FrasesRecorrido.Salir);

        // Temas: gris -> oscuro -> claro.
        await MarcarYNarrarAsync(page, g, "document.getElementById('btnToggleTheme')", FrasesRecorrido.Temas);
        await PosComun.MoverYClicAsync(page, "#btnToggleTheme");
        await page.WaitForTimeoutAsync(900);
        await g.NarrarAsync(FrasesRecorrido.Gris, 300);
        await PosComun.MoverYClicAsync(page, "#btnToggleTheme");
        await page.WaitForTimeoutAsync(900);
        await g.NarrarAsync(FrasesRecorrido.Oscuro, 300);
        await PosComun.MoverYClicAsync(page, "#btnToggleTheme");
        await page.WaitForTimeoutAsync(900);
        await g.NarrarAsync(FrasesRecorrido.Claro);

        // Clientes.
        await page.EvaluateAsync("() => { limpiarMarcas(); marcar(document.getElementById('btnBuscarPersona'), 1); marcar(document.getElementById('btnConsumidorFinal'), 2); }");
        await g.NarrarAsync(FrasesRecorrido.Clientes);
        await page.EvaluateAsync("() => limpiarMarcas()");

        // Buscador de productos (F10): solo se ven los productos de ejemplo.
        await page.EvaluateAsync("() => marcar(document.getElementById('btnAgregarManual'), 1)");
        await g.NarrarAsync(FrasesRecorrido.Buscador, 200);
        await page.EvaluateAsync("() => limpiarMarcas()");
        await PosComun.ApretarAsync(page, "F10", "F10");
        await page.Locator("#modalBuscarProducto").WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible, Timeout = 10000 });
        await page.WaitForTimeoutAsync(600);
        await page.Keyboard.TypeAsync("Chorizo casero", new KeyboardTypeOptions { Delay = 120 });
        await page.WaitForTimeoutAsync(900);
        await g.NarrarAsync(FrasesRecorrido.ElegirProducto, 100);
        // Doble clic en la fila del producto de ejemplo: Enter elegiria la "fila activa o la primera", que puede ser
        // un producto real oculto por el filtro de la grabacion (fuga encontrada el 2026-11-09).
        const string SelFilaEjemplo = "#modalBuscarProducto tbody tr[data-codigo='9902']";
        var fila = page.Locator(SelFilaEjemplo).First;
        await fila.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible, Timeout = 10000 });
        var cajaFila = await fila.BoundingBoxAsync() ?? throw new InvalidOperationException("No se ubico el producto de ejemplo.");
        await page.Mouse.MoveAsync(cajaFila.X + cajaFila.Width / 2, cajaFila.Y + cajaFila.Height / 2, new MouseMoveOptions { Steps = 25 });
        await page.WaitForTimeoutAsync(300);
        await page.Mouse.DblClickAsync(cajaFila.X + cajaFila.Width / 2, cajaFila.Y + cajaFila.Height / 2);
        await page.Locator("#modalBuscarProducto").WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Hidden, Timeout = 10000 });
        await page.WaitForFunctionAsync("() => document.activeElement && document.activeElement.id === 'inputCantidad'", null, new PageWaitForFunctionOptions { Timeout = 10000 });
        await page.Keyboard.TypeAsync("0.75", new KeyboardTypeOptions { Delay = 160 });
        await PosComun.ApretarAsync(page, "Enter", "Enter");
        await page.WaitForTimeoutAsync(1000);

        // Comentario de la venta.
        await page.EvaluateAsync("() => marcar(document.getElementById('btnLimpiar'), 1)");
        await g.NarrarAsync(FrasesRecorrido.Comentario, 100);
        await page.EvaluateAsync("() => limpiarMarcas()");
        await PosComun.MoverYClicAsync(page, "#btnLimpiar");
        await page.Locator("#txtComentarioVenta").WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible, Timeout = 10000 });
        await page.WaitForTimeoutAsync(500);
        await page.Locator("#txtComentarioVenta").ClickAsync();
        await page.Keyboard.TypeAsync("Retira a las 18 h", new KeyboardTypeOptions { Delay = 70 });
        await g.NarrarAsync(FrasesRecorrido.Guardar, 100);
        await PosComun.MoverYClicAsync(page, "#btnGuardarComentarioVenta");
        await page.Locator("#modalComentarioVenta").WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Hidden, Timeout = 10000 });
        await page.EvaluateAsync("() => marcar(document.getElementById('btnLimpiar'), 1)");
        await page.WaitForTimeoutAsync(1500);
        await page.EvaluateAsync("() => limpiarMarcas()");

        // Venta en claro: Fin + 1, y nueva venta.
        await g.NarrarAsync(FrasesRecorrido.Vender, 100);
        await PosComun.ApretarAsync(page, "End", "Fin");
        await page.Locator("#modalFormaPago").WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        await page.WaitForTimeoutAsync(600);
        await PosComun.ApretarAsync(page, "1", "1");
        await PosComun.EsperarPostVentaAsync(page);
        await g.NarrarAsync(FrasesRecorrido.Nueva, 100);
        await PosComun.ApretarAsync(page, "1", "1");
        await PosComun.EsperarVentaNuevaAsync(page);
        await PosComun.EsperarPosListoAsync(page);

        // Cancelar una venta en curso.
        await CargarSilencioAsync(page, ("9901", "1"));
        await page.EvaluateAsync("() => marcar(document.getElementById('btnCancelarItem'), 1)");
        await g.NarrarAsync(FrasesRecorrido.Cancelar, 100);
        await page.EvaluateAsync("() => limpiarMarcas()");
        await PosComun.MoverYClicAsync(page, "#btnCancelarItem");
        await page.Locator(".swal2-confirm").WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        await page.WaitForTimeoutAsync(700);
        await PosComun.MoverYClicAsync(page, ".swal2-confirm");
        await page.WaitForFunctionAsync("() => !document.querySelector('.swal2-popup') && !document.getElementById('tablaItems')?.querySelector('tr.fila-item')",
            null, new PageWaitForFunctionOptions { Timeout = 20000, PollingInterval = 200 });
        await page.WaitForTimeoutAsync(900);

        // Ayuda (F1).
        await g.NarrarAsync(FrasesRecorrido.F1, 100);
        await PosComun.ApretarAsync(page, "F1", "F1");
        await page.Locator("#modalAyudaPOS").WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        await page.WaitForTimeoutAsync(3000);
        await PosComun.ApretarAsync(page, "Escape", "Esc");
        await page.WaitForTimeoutAsync(900);

        // Mis actividades (F6) y Mis ventas.
        await g.NarrarAsync(FrasesRecorrido.F6, 100);
        await PosComun.ApretarAsync(page, "F6", "F6");
        await page.Locator("#btnAbrirMisVentasDesdeActividades").WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible, Timeout = 15000 });
        await page.WaitForTimeoutAsync(2000);
        await page.EvaluateAsync("() => marcar(document.getElementById('btnAbrirMisVentasDesdeActividades'), 1)");
        await g.NarrarAsync(FrasesRecorrido.MisVentas, 100);
        await page.EvaluateAsync("() => limpiarMarcas()");
        await PosComun.MoverYClicAsync(page, "#btnAbrirMisVentasDesdeActividades");
        await page.Locator("#misVentasFormaPago").WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible, Timeout = 15000 });
        await page.WaitForTimeoutAsync(1500);
        await page.EvaluateAsync("() => { marcar(document.getElementById('misVentasBuscarCliente'), 1); marcar(document.getElementById('misVentasFormaPago'), 2); }");
        await g.NarrarAsync(FrasesRecorrido.Filtro);
        await page.EvaluateAsync("() => limpiarMarcas()");
        await g.NarrarAsync(FrasesRecorrido.Detalle, 100);
        // Una venta con importe (la primera de la lista puede ser la cancelada, en $ 0,00).
        var venta = page.Locator("#modalFinanzasPOS .venta-total").Filter(new LocatorFilterOptions { HasNotText = "$ 0,00" }).First;
        var caja = await venta.BoundingBoxAsync();
        if (caja != null)
        {
            await page.Mouse.MoveAsync(caja.X + caja.Width / 2, caja.Y + caja.Height / 2, new MouseMoveOptions { Steps = 25 });
            await page.WaitForTimeoutAsync(300);
            await page.Mouse.ClickAsync(caja.X + caja.Width / 2, caja.Y + caja.Height / 2);
        }
        await page.WaitForTimeoutAsync(2500);
        await g.NarrarAsync(FrasesRecorrido.Listo);
        await page.WaitForTimeoutAsync(800);
    }

    // ===================== Tema "lineas": corregir y bonificar los renglones del carrito =====================
    // Atajos verificados en pos-cart.js (ventana "Linea de venta") y forma-pago.js (2026-11-09): clic en el renglon o
    // F4 (ultimo renglon); B o "/" bonificar; "/" otra vez = por porcentaje; "/" otra vez = aplicar a todos; C cantidad;
    // E eliminar (Enter = Si, N = No; la linea queda anulada y tachada); Enter aplica. En la forma de pago, "/" abre el
    // descuento o recargo a toda la venta, que se rechaza si algun producto ya tiene bonificacion propia.

    private static class FrasesLineas
    {
        public const string Intro = "Cada producto del carrito se puede corregir: tocá su renglón, o presioná F4 para abrir el último.";
        public const string Atajos = "Se abre la línea de venta. Abajo están los atajos: B o barra para bonificar, C para la cantidad y E para eliminar.";
        public const string Cantidad = "Con la C cambiás la cantidad. Escribila, o usá los botones más y menos.";
        public const string CantidadEnter = "Presioná Enter y la cantidad queda aplicada.";
        public const string F4 = "Con F4 se abre la última línea del carrito.";
        public const string Eliminar = "Con la E se elimina el producto. Confirmá con Enter, o con la N si te arrepentiste.";
        public const string Anulada = "La línea queda tachada y ya no suma al total.";
        public const string Precio = "Para bonificar un producto, tocá su renglón y presioná la B, o la barra.";
        public const string PrecioEscribir = "Escribí el precio bonificado: el porcentaje de descuento se calcula solo.";
        public const string Aplicar = "Presioná Enter para aplicar la bonificación.";
        public const string PrecioListo = "El renglón queda con el precio bonificado y el porcentaje al lado.";
        public const string Porcentaje = "Para bonificar por porcentaje, presioná la barra dos veces: la segunda tilda Bonificar por porcentaje.";
        public const string PorcentajeEscribir = "Escribí el porcentaje. Con un número negativo, en vez de descuento se aplica un recargo.";
        public const string Permiso = "Si tu usuario no tiene permiso para bonificar, el sistema pide que autorice otro usuario que lo tenga.";
        public const string TodasAbrir = "Para bonificar todos los productos a la vez, abrí cualquier renglón.";
        public const string TodasBarra = "Como este ya tiene bonificación, se abre directo en el porcentaje. Presioná la barra y se tilda Aplicar a todos los productos.";
        public const string TodasEscribir = "Escribí el porcentaje y presioná Enter.";
        public const string TodasListo = "Todos los productos quedan con el mismo porcentaje, y reemplaza las bonificaciones que tenían.";
        public const string GlobalFin = "Para hacer un descuento a toda la venta, también podés hacerlo al cobrar: presioná Fin y después la barra.";
        public const string GlobalPct = "Escribí el porcentaje y presioná Enter. Con un número negativo es un recargo.";
        public const string GlobalTotal = "Ves el descuento aplicado y el total a cobrar. Después elegís la forma de pago, como siempre.";
        public const string GlobalEscape = "En este ejemplo cerramos la ventana con la cruz: el descuento no se aplica y volvemos al carrito.";
        public const string Fin = "Presioná Fin para cobrar.";
        public const string GlobalNo = "Ojo: el descuento a toda la venta solo se puede si ningún producto tiene bonificación propia. Como acá ya hay, al presionar la barra el sistema avisa.";
        public const string Cobrar = "Cerrá el aviso y cobrá como siempre: el uno es efectivo.";
        public const string Listo = "Listo. Así se corrigen y se bonifican los productos del carrito.";

        public static readonly string[] Todas =
        {
            Intro, Atajos, Cantidad, CantidadEnter, F4, Eliminar, Anulada, Precio, PrecioEscribir, Aplicar, PrecioListo,
            Porcentaje, PorcentajeEscribir, Permiso, TodasAbrir, TodasBarra, TodasEscribir, TodasListo, GlobalFin,
            GlobalPct, GlobalTotal, GlobalEscape, Fin, GlobalNo, Cobrar, Listo
        };
    }

    private const string SelFilaVacio = "#tablaItems tr.fila-item:has-text('Vacío especial')";
    private const string SelFilaChorizo = "#tablaItems tr.fila-item:has-text('Chorizo casero')";

    // Fuera de camara: caja abierta (si hace falta) y tres productos en el carrito (dos pesables y la gaseosa por EAN).
    private static async Task PrepararLineasAsync(IPage page)
    {
        await PosComun.EntrarAlPosAsync(page, null);
        await CargarSilencioAsync(page, ("9901", "1.5"), ("9902", "1"));
        // El EAN tipeado rapido + Enter entra como lectura del lector: se agrega solo con cantidad 1.
        await page.ClickAsync("#inputCodigo");
        await page.Keyboard.TypeAsync(EanEjemplo, new KeyboardTypeOptions { Delay = 12 });
        await page.Keyboard.PressAsync("Enter");
        await page.WaitForFunctionAsync("() => document.querySelectorAll('#tablaItems tr.fila-item').length >= 3",
            null, new PageWaitForFunctionOptions { Timeout = 10000, PollingInterval = 200 });
        await page.Mouse.MoveAsync(PosComun.AnchoVista / 2f, PosComun.AltoVista / 2f);
        await page.WaitForTimeoutAsync(800);
    }

    // Espera la ventana "Linea de venta" abierta (estado = Visible) o cerrada (Hidden), y la animacion.
    private static async Task EsperarLineaAsync(IPage page, WaitForSelectorState estado)
    {
        await page.Locator("#modalLineaVenta").WaitForAsync(new LocatorWaitForOptions { State = estado, Timeout = 10000 });
        await page.WaitForTimeoutAsync(estado == WaitForSelectorState.Visible ? 700 : 500);
    }

    // Deja el cursor en el campo y su contenido seleccionado (el POS ya lo hace al abrir el bloque, pero con la
    // animacion puede no llegar a tiempo): asi lo que se tipea reemplaza el valor.
    private static async Task SeleccionarCampoAsync(IPage page, string id)
    {
        await page.WaitForFunctionAsync("(id) => { var e = document.getElementById(id); return e && e.offsetParent && !e.readOnly; }", id,
            new PageWaitForFunctionOptions { Timeout = 5000, PollingInterval = 100 });
        await page.EvaluateAsync("(id) => { var e = document.getElementById(id); e.focus(); e.select(); }", id);
    }

    private static async Task<string> GrabarLineasAsync(IPage page, Grabador g)
    {
        // 1) Abrir una linea: clic en el renglon. La guia de atajos al pie de la ventana.
        await g.NarrarAsync(FrasesLineas.Intro, 100);
        await PosComun.MoverYClicAsync(page, SelFilaChorizo);
        await EsperarLineaAsync(page, WaitForSelectorState.Visible);
        await page.EvaluateAsync("() => marcar(document.getElementById('posLineaAtajosHint'), 1)");
        await g.NarrarAsync(FrasesLineas.Atajos);
        await page.EvaluateAsync("() => limpiarMarcas()");

        // 2) Cantidad (C): 1 -> 2 unidades del chorizo.
        await PosComun.ApretarAsync(page, "c", "C");
        await page.Locator("#btnAplicarCantidad").WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        await page.WaitForTimeoutAsync(400);
        await page.EvaluateAsync("() => { marcar(document.getElementById('btnCantMenos'), 1); marcar(document.getElementById('btnCantMas'), 2); }");
        await g.NarrarAsync(FrasesLineas.Cantidad, 100);
        await page.EvaluateAsync("() => limpiarMarcas()");
        await SeleccionarCampoAsync(page, "modalCantidad");
        await page.Keyboard.TypeAsync("2", new KeyboardTypeOptions { Delay = 170 });
        await page.WaitForTimeoutAsync(600);
        await g.NarrarAsync(FrasesLineas.CantidadEnter, 100);
        await PosComun.ApretarAsync(page, "Enter", "Enter");
        await EsperarLineaAsync(page, WaitForSelectorState.Hidden);
        await page.WaitForTimeoutAsync(600);

        // 3) Eliminar (F4 -> E -> Enter): la gaseosa queda anulada y tachada.
        await g.NarrarAsync(FrasesLineas.F4, 100);
        await PosComun.ApretarAsync(page, "F4", "F4");
        await EsperarLineaAsync(page, WaitForSelectorState.Visible);
        await g.NarrarAsync(FrasesLineas.Eliminar, 100);
        await PosComun.ApretarAsync(page, "e", "E");
        await page.Locator(".swal2-popup").WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible, Timeout = 5000 });
        await page.WaitForTimeoutAsync(900);
        await PosComun.ApretarAsync(page, "Enter", "Enter");
        await EsperarLineaAsync(page, WaitForSelectorState.Hidden);
        await page.WaitForFunctionAsync("() => !!document.querySelector('#tablaItems tr.fila-anulada')", null,
            new PageWaitForFunctionOptions { Timeout = 5000, PollingInterval = 100 });
        await page.WaitForTimeoutAsync(500);
        await page.EvaluateAsync("() => marcar(document.querySelector('#tablaItems tr.fila-anulada'), 1)");
        await g.NarrarAsync(FrasesLineas.Anulada);
        await page.EvaluateAsync("() => limpiarMarcas()");

        // 3b) Descuento a toda la venta al cobrar (Fin -> "/" -> 10 -> Enter), sin bonificaciones en el carrito todavia;
        // se cierra con la cruz (al reabrir la forma de pago el descuento arranca en cero, ver forma-pago.js). Escape no
        // sirve aca: tras el Enter el campo queda deshabilitado y el foco fuera de la ventana (probado 2026-11-09).
        await g.NarrarAsync(FrasesLineas.GlobalFin, 100);
        await PosComun.ApretarAsync(page, "End", "Fin");
        await page.Locator("#modalFormaPago").WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        await page.WaitForTimeoutAsync(800);
        await PosComun.ApretarAsync(page, "/", "/");
        await page.Locator("#bloquePorcentajeTotalVenta").WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible, Timeout = 5000 });
        await SeleccionarCampoAsync(page, "txtPorcentajeTotalVenta");
        await page.WaitForTimeoutAsync(400);
        await g.NarrarAsync(FrasesLineas.GlobalPct, 100);
        await page.Keyboard.TypeAsync("10", new KeyboardTypeOptions { Delay = 170 });
        await page.WaitForTimeoutAsync(600);
        await PosComun.ApretarAsync(page, "Enter", "Enter");
        await page.WaitForTimeoutAsync(900);
        await page.EvaluateAsync("() => { marcar(document.getElementById('lblTotalConDescuentoTotalVenta'), 1); marcar(document.getElementById('totalVenta'), 2); }");
        await g.NarrarAsync(FrasesLineas.GlobalTotal);
        await page.EvaluateAsync("() => limpiarMarcas()");
        // El aviso breve "Descuento aplicado" (toast de 2,2 s) ya se fue mientras se narraba.
        await g.NarrarAsync(FrasesLineas.GlobalEscape, 100);
        await PosComun.MoverYClicAsync(page, "#modalFormaPago .modal-header .close");
        await page.Locator("#modalFormaPago").WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Hidden, Timeout = 5000 });
        await page.WaitForTimeoutAsync(700);

        // 4) Bonificar por precio (B): vacio de 12.500 a 11.000 el kilo (12 %).
        await g.NarrarAsync(FrasesLineas.Precio, 100);
        await PosComun.MoverYClicAsync(page, SelFilaVacio);
        await EsperarLineaAsync(page, WaitForSelectorState.Visible);
        await PosComun.ApretarAsync(page, "b", "B");
        await page.Locator("#btnAplicarBonificacion").WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        await page.WaitForTimeoutAsync(500);
        await g.NarrarAsync(FrasesLineas.PrecioEscribir, 100);
        await SeleccionarCampoAsync(page, "txtPrecioKg");
        await page.Keyboard.TypeAsync("11000", new KeyboardTypeOptions { Delay = 170 });
        await page.WaitForTimeoutAsync(500);
        await page.EvaluateAsync("() => marcar(document.getElementById('txtPorcentaje'), 1)");
        await page.WaitForTimeoutAsync(1200);
        await page.EvaluateAsync("() => limpiarMarcas()");
        await g.NarrarAsync(FrasesLineas.Aplicar, 100);
        await PosComun.ApretarAsync(page, "Enter", "Enter");
        await EsperarLineaAsync(page, WaitForSelectorState.Hidden);
        // El vacio es el primer renglon (querySelector no entiende :has-text de Playwright).
        await page.EvaluateAsync("() => marcar(document.querySelector('#tablaItems tr.fila-item'), 1)");
        await g.NarrarAsync(FrasesLineas.PrecioListo);
        await page.EvaluateAsync("() => limpiarMarcas()");

        // 5) Bonificar por porcentaje ("/" "/"): chorizo 10 %.
        await PosComun.MoverYClicAsync(page, SelFilaChorizo);
        await EsperarLineaAsync(page, WaitForSelectorState.Visible);
        await g.NarrarAsync(FrasesLineas.Porcentaje, 100);
        await PosComun.ApretarAsync(page, "/", "/");
        await page.Locator("#btnAplicarBonificacion").WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        await SeleccionarCampoAsync(page, "txtPrecioKg");
        await page.WaitForTimeoutAsync(500);
        await PosComun.ApretarAsync(page, "/", "/");
        await page.WaitForFunctionAsync("() => document.getElementById('chkPorcentaje').checked", null,
            new PageWaitForFunctionOptions { Timeout = 5000, PollingInterval = 100 });
        await page.WaitForTimeoutAsync(500);
        await g.NarrarAsync(FrasesLineas.PorcentajeEscribir, 100);
        await SeleccionarCampoAsync(page, "txtPorcentaje");
        await page.Keyboard.TypeAsync("10", new KeyboardTypeOptions { Delay = 170 });
        await page.WaitForTimeoutAsync(700);
        await PosComun.ApretarAsync(page, "Enter", "Enter");
        await EsperarLineaAsync(page, WaitForSelectorState.Hidden);
        await g.NarrarAsync(FrasesLineas.Permiso);

        // 6) Bonificar todas: el vacio ya bonificado se abre en el porcentaje; "/" tilda "Aplicar a todos".
        await g.NarrarAsync(FrasesLineas.TodasAbrir, 100);
        await PosComun.MoverYClicAsync(page, SelFilaVacio);
        await EsperarLineaAsync(page, WaitForSelectorState.Visible);
        await SeleccionarCampoAsync(page, "txtPorcentaje");
        await page.EvaluateAsync("() => marcar(document.getElementById('txtPorcentaje'), 1)");
        await g.NarrarAsync(FrasesLineas.TodasBarra, 100);
        await page.EvaluateAsync("() => limpiarMarcas()");
        await SeleccionarCampoAsync(page, "txtPorcentaje");
        await PosComun.ApretarAsync(page, "/", "/");
        await page.WaitForFunctionAsync("() => document.getElementById('chkBonificarTodos').checked", null,
            new PageWaitForFunctionOptions { Timeout = 5000, PollingInterval = 100 });
        await page.WaitForTimeoutAsync(500);
        await page.EvaluateAsync("() => marcar(document.getElementById('msgBonificarTodos'), 1)");
        await g.NarrarAsync(FrasesLineas.TodasEscribir, 100);
        await page.EvaluateAsync("() => limpiarMarcas()");
        await SeleccionarCampoAsync(page, "txtPorcentaje");
        await page.Keyboard.TypeAsync("15", new KeyboardTypeOptions { Delay = 170 });
        await page.WaitForTimeoutAsync(600);
        await PosComun.ApretarAsync(page, "Enter", "Enter");
        await EsperarLineaAsync(page, WaitForSelectorState.Hidden);
        await page.EvaluateAsync("() => marcar(document.getElementById('tablaItems'), 1)");
        await g.NarrarAsync(FrasesLineas.TodasListo);
        await page.EvaluateAsync("() => limpiarMarcas()");

        // 7) Cobrar: el descuento a toda la venta ("/") ahora se rechaza porque ya hay bonificaciones por producto.
        await g.NarrarAsync(FrasesLineas.Fin, 100);
        await PosComun.ApretarAsync(page, "End", "Fin");
        await page.Locator("#modalFormaPago").WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        await page.WaitForTimeoutAsync(800);
        await g.NarrarAsync(FrasesLineas.GlobalNo, 100);
        await PosComun.ApretarAsync(page, "/", "/");
        await page.Locator(".swal2-popup").WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible, Timeout = 5000 });
        // Tiempo para leer el aviso (la voz ya lo explico antes de la barra).
        await page.WaitForTimeoutAsync(3500);
        await g.NarrarAsync(FrasesLineas.Cobrar, 100);
        await PosComun.MoverYClicAsync(page, ".swal2-confirm");
        await page.Locator(".swal2-popup").WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Hidden, Timeout = 5000 });
        await page.WaitForTimeoutAsync(500);
        await page.Mouse.MoveAsync(PosComun.AnchoVista / 2f, 40, new MouseMoveOptions { Steps = 10 });
        // El aviso devuelve el foco al control anterior; si es un campo de texto, los numeros de forma de pago no
        // responden (forma-pago.js los ignora con el foco en un input).
        await page.EvaluateAsync("() => { if (document.activeElement && document.activeElement.blur) document.activeElement.blur(); }");
        await PosComun.ApretarAsync(page, "1", "1");
        string idVenta = await PosComun.EsperarPostVentaAsync(page);
        await page.WaitForTimeoutAsync(800);
        await g.NarrarAsync(FrasesLineas.Listo);
        await page.WaitForTimeoutAsync(600);
        return idVenta;
    }

    // ===================== Tema "misventas": revisar, reimprimir y corregir ventas de la caja =====================
    // Verificado en el codigo (2026-11-09): F6 -> "Mis ventas" (_MisVentas.cshtml); tocar la tarjeta despliega el panel
    // (_DetalleVentaResumen: "Modificar venta", "Cambiar forma de pago", "Abrir detalle"); el detalle completo
    // (_DetalleVentaCard) suma "Factura / Imprimir" (_ModalComprobanteVenta: ticket, PDF, email, factura). "Modificar
    // venta" abre el POS en edicion (banner "Modificando venta #", "Salir edicion"); en la forma de pago Enter mantiene la
    // actual (forma-pago.js puedeMantenerFormaPagoConEnter) y al guardar sale "Venta actualizada" y vuelve al POS.
    // Reglas (VentasController.PuedeModificarUltimaVenta / PuedeCambiarFormaPago): ventas de la caja abierta del
    // vendedor y dentro de los dias del permiso "POS - Ultimas Ventas"; sin permiso, "(requiere autorizacion)".

    private static class FrasesMisVentas
    {
        public const string F6 = "Para revisar las ventas de tu caja, presioná F6 y tocá Mis ventas.";
        public const string Lista = "Están las ventas desde que abriste la caja. Tocá una para desplegarla.";
        public const string Panel = "Ves los productos y el total. Desde acá también podés modificarla o cambiarle la forma de pago.";
        public const string AbrirDetalle = "Con Abrir detalle ves la venta completa: cliente, comprobante, forma de pago y productos.";
        public const string Imprimir = "Con Factura o Imprimir volvés a sacar el comprobante: el ticket, el PDF, el email o la factura electrónica.";
        public const string Ticket = "Por ejemplo, el ticket otra vez.";
        public const string Modificar = "Para corregir la venta, tocá Modificar venta.";
        public const string Edicion = "Se abre en el punto de venta, en modo edición. Los productos que ya estaban mantienen su precio original.";
        public const string SalirEdicion = "Si te arrepentís, Salir edición vuelve sin cambiar nada.";
        public const string Cambio = "Corregí lo que haga falta. Acá, el chorizo pasa a dos unidades.";
        public const string FinEdicion = "Presioná Fin. Arriba dice la forma de pago actual: con Enter se mantiene.";
        public const string Actualizada = "La venta quedó actualizada. Con Enter volvés al punto de venta.";
        public const string CambiarFp = "Si solo cambia cómo se pagó, por ejemplo la clienta lo deja en su cuenta, usá Cambiar forma de pago.";
        public const string CambiarFpPos = "Los productos quedan fijos. Presioná Fin y elegí la nueva forma: el cuatro es cuenta corriente.";
        public const string Resultado = "En la lista ya se ven los cambios: la venta de la clienta pasó a cuenta corriente y la otra tiene el total nuevo.";
        public const string Reglas = "Solo podés modificar ventas de tu caja abierta, y dentro de los días que permite tu usuario.";
        public const string ReglasPermiso = "Sin ese permiso, el botón dice requiere autorización y otro usuario tiene que autorizarla.";
        public const string Listo = "Listo. Así se revisan, se reimprimen y se corrigen las ventas.";

        public static readonly string[] Todas =
        {
            F6, Lista, Panel, AbrirDetalle, Imprimir, Ticket, Modificar, Edicion, SalirEdicion, Cambio, FinEdicion,
            Actualizada, CambiarFp, CambiarFpPos, Resultado, Reglas, ReglasPermiso, Listo
        };
    }

    // Fuera de camara: caja abierta (si hace falta) y dos ventas en efectivo: una a consumidor final (vacio y chorizo)
    // y otra a la clienta (chorizo). Devuelve sus ids.
    private static async Task<(string, string)> PrepararMisVentasAsync(IPage page)
    {
        await PosComun.EntrarAlPosAsync(page, null);
        string venta1 = await VentaEfectivoSilencioAsync(page, false, ("9901", "1.25"), ("9902", "1"));
        string venta2 = await VentaEfectivoSilencioAsync(page, true, ("9902", "2"));
        await page.Mouse.MoveAsync(PosComun.AnchoVista / 2f, PosComun.AltoVista / 2f);
        await page.WaitForTimeoutAsync(800);
        return (venta1, venta2);
    }

    // Carga los productos, elige a la clienta si se pide, cobra en efectivo y deja el POS en una venta nueva.
    private static async Task<string> VentaEfectivoSilencioAsync(IPage page, bool conCliente, params (string Codigo, string Cantidad)[] productos)
    {
        await CargarSilencioAsync(page, productos);
        if (conCliente) await ElegirClienteAsync(page, null);
        await PosComun.ApretarAsync(page, "End", "Fin", false);
        await page.Locator("#modalFormaPago").WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        await page.WaitForTimeoutAsync(600);
        await PosComun.ApretarAsync(page, "1", "1", false);
        string id = await PosComun.EsperarPostVentaAsync(page);
        await PosComun.ApretarAsync(page, "1", "1", false);
        await PosComun.EsperarVentaNuevaAsync(page);
        await PosComun.EsperarPosListoAsync(page);
        return id;
    }

    // F6 -> Mis ventas (con o sin narracion).
    private static async Task AbrirMisVentasAsync(IPage page, Grabador g, bool narrar)
    {
        if (narrar) await g.NarrarAsync(FrasesMisVentas.F6, 100);
        await PosComun.ApretarAsync(page, "F6", "F6");
        await page.Locator("#btnAbrirMisVentasDesdeActividades").WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible, Timeout = 15000 });
        await page.WaitForTimeoutAsync(1500);
        await PosComun.MoverYClicAsync(page, "#btnAbrirMisVentasDesdeActividades");
        await page.Locator("#misVentasFormaPago").WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible, Timeout = 15000 });
        await page.WaitForTimeoutAsync(1500);
    }

    // Toca la tarjeta de la venta y espera el panel desplegado con sus acciones.
    private static async Task DesplegarVentaAsync(IPage page, string idVenta)
    {
        await PosComun.MoverYClicAsync(page, $".venta-item[data-venta-id='{idVenta}'] .venta-total");
        await page.Locator($".venta-item[data-venta-id='{idVenta}'] .venta-abrir-detalle").WaitForAsync(
            new LocatorWaitForOptions { State = WaitForSelectorState.Visible, Timeout = 15000 });
        await page.WaitForTimeoutAsync(900);
    }

    // Hace clic en un link que navega (POS en edicion) y espera el POS cargado.
    private static async Task ClicYEsperarPosAsync(IPage page, string selector)
    {
        await page.RunAndWaitForNavigationAsync(() => PosComun.MoverYClicAsync(page, selector),
            new PageRunAndWaitForNavigationOptions { WaitUntil = WaitUntilState.DOMContentLoaded, Timeout = 20000 });
        await PosComun.EsperarPosListoAsync(page);
    }

    // "Venta actualizada" -> Enter -> vuelve al POS (navega).
    private static async Task CerrarVentaActualizadaAsync(IPage page, Grabador g)
    {
        await page.Locator(".swal2-popup:has-text('Venta actualizada')").WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible, Timeout = 20000 });
        await page.WaitForTimeoutAsync(700);
        await g.NarrarAsync(FrasesMisVentas.Actualizada, 100);
        await page.RunAndWaitForNavigationAsync(() => PosComun.ApretarAsync(page, "Enter", "Enter"),
            new PageRunAndWaitForNavigationOptions { WaitUntil = WaitUntilState.DOMContentLoaded, Timeout = 20000 });
        await PosComun.EsperarPosListoAsync(page);
    }

    private static async Task GrabarMisVentasAsync(IPage page, Grabador g, string venta1, string venta2)
    {
        // 1) Lista y panel desplegado de la primera venta.
        await AbrirMisVentasAsync(page, g, true);
        await g.NarrarAsync(FrasesMisVentas.Lista, 100);
        await DesplegarVentaAsync(page, venta1);
        await page.EvaluateAsync("(id) => { var p = document.querySelector('.venta-item[data-venta-id=\"' + id + '\"] .venta-detalle-panel'); if (p) marcar(p, 1); }", venta1);
        await g.NarrarAsync(FrasesMisVentas.Panel);
        await page.EvaluateAsync("() => limpiarMarcas()");

        // 2) Detalle completo y reimpresion del ticket (impresora simulada).
        await g.NarrarAsync(FrasesMisVentas.AbrirDetalle, 100);
        await PosComun.MoverYClicAsync(page, $".venta-item[data-venta-id='{venta1}'] .venta-abrir-detalle");
        await page.Locator("#btnFacturaImprimirDetalle").WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible, Timeout = 15000 });
        await page.WaitForTimeoutAsync(1500);
        await g.NarrarAsync(FrasesMisVentas.Imprimir, 100);
        await PosComun.MoverYClicAsync(page, "#btnFacturaImprimirDetalle");
        await page.Locator("#btnCvTicket").WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible, Timeout = 10000 });
        await page.WaitForTimeoutAsync(1200);
        await g.NarrarAsync(FrasesMisVentas.Ticket, 100);
        await PosComun.MoverYClicAsync(page, "#btnCvTicket");
        await page.WaitForFunctionAsync("() => !!window.__impresionEnCurso", null, new PageWaitForFunctionOptions { Timeout = 10000, PollingInterval = 100 });
        await page.EvaluateAsync("() => window.__impresionEnCurso.then(function () { return null; })");
        await page.EvaluateAsync("() => { window.__impresionEnCurso = null; }");
        await page.WaitForTimeoutAsync(500);
        if (await page.Locator("#modalComprobanteVenta.show").CountAsync() > 0)
        {
            await PosComun.MoverYClicAsync(page, "#modalComprobanteVenta .btn-close");
            await page.Locator("#modalComprobanteVenta").WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Hidden, Timeout = 10000 });
            await page.WaitForTimeoutAsync(600);
        }

        // 3) Modificar venta: el chorizo pasa de 1 a 2 unidades y se guarda manteniendo la forma de pago (Enter).
        await page.EvaluateAsync("() => marcar(document.getElementById('btnModificarVentaDetalle'), 1)");
        await g.NarrarAsync(FrasesMisVentas.Modificar, 100);
        await page.EvaluateAsync("() => limpiarMarcas()");
        await ClicYEsperarPosAsync(page, "#btnModificarVentaDetalle");
        await page.EvaluateAsync("() => { var b = document.querySelector('.pos-restore-banner'); if (b) marcar(b, 1); }");
        await g.NarrarAsync(FrasesMisVentas.Edicion);
        await page.EvaluateAsync("() => { limpiarMarcas(); marcar(document.getElementById('btnCancelarItem'), 1); }");
        await g.NarrarAsync(FrasesMisVentas.SalirEdicion);
        await page.EvaluateAsync("() => limpiarMarcas()");
        await g.NarrarAsync(FrasesMisVentas.Cambio, 100);
        await PosComun.MoverYClicAsync(page, SelFilaChorizo);
        await EsperarLineaAsync(page, WaitForSelectorState.Visible);
        await PosComun.ApretarAsync(page, "c", "C");
        await page.Locator("#btnAplicarCantidad").WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        await SeleccionarCampoAsync(page, "modalCantidad");
        await page.WaitForTimeoutAsync(300);
        await page.Keyboard.TypeAsync("2", new KeyboardTypeOptions { Delay = 170 });
        await page.WaitForTimeoutAsync(500);
        await PosComun.ApretarAsync(page, "Enter", "Enter");
        await EsperarLineaAsync(page, WaitForSelectorState.Hidden);
        await page.WaitForTimeoutAsync(600);
        await g.NarrarAsync(FrasesMisVentas.FinEdicion, 100);
        await PosComun.ApretarAsync(page, "End", "Fin");
        await page.Locator("#modalFormaPago").WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        await page.WaitForTimeoutAsync(1000);
        await page.EvaluateAsync("() => { if (document.activeElement && document.activeElement.blur) document.activeElement.blur(); }");
        await PosComun.ApretarAsync(page, "Enter", "Enter");
        await CerrarVentaActualizadaAsync(page, g);

        // 4) Cambiar forma de pago de la venta de la clienta: efectivo -> cuenta corriente.
        await AbrirMisVentasAsync(page, g, false);
        await DesplegarVentaAsync(page, venta2);
        string selCambiar = $".venta-item[data-venta-id='{venta2}'] a.venta-accion-pos:has-text('Cambiar forma de pago')";
        await page.EvaluateAsync("(id) => { var a = [...document.querySelectorAll('.venta-item[data-venta-id=\"' + id + '\"] a.venta-accion-pos')].find(function (x) { return /forma de pago/i.test(x.textContent); }); if (a) marcar(a, 1); }", venta2);
        await g.NarrarAsync(FrasesMisVentas.CambiarFp);
        await page.EvaluateAsync("() => limpiarMarcas()");
        await ClicYEsperarPosAsync(page, selCambiar);
        await page.WaitForTimeoutAsync(500);
        await g.NarrarAsync(FrasesMisVentas.CambiarFpPos, 100);
        await PosComun.ApretarAsync(page, "End", "Fin");
        await page.Locator("#modalFormaPago").WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        await page.WaitForTimeoutAsync(1000);
        await page.EvaluateAsync("() => { if (document.activeElement && document.activeElement.blur) document.activeElement.blur(); }");
        await PosComun.ApretarAsync(page, "4", "4");
        await CerrarVentaActualizadaAsync(page, g);

        // 5) Reglas y permisos, sobre la lista con las dos ventas ya corregidas.
        await AbrirMisVentasAsync(page, g, false);
        await page.Mouse.MoveAsync(PosComun.AnchoVista / 2f, PosComun.AltoVista - 60, new MouseMoveOptions { Steps = 15 });
        await page.EvaluateAsync("(ids) => ids.forEach(function (id, i) { var e = document.querySelector('.venta-item[data-venta-id=\"' + id + '\"]'); if (e) marcar(e, i + 1); })",
            new[] { venta2, venta1 });
        await g.NarrarAsync(FrasesMisVentas.Resultado);
        await page.EvaluateAsync("() => limpiarMarcas()");
        await g.NarrarAsync(FrasesMisVentas.Reglas);
        await g.NarrarAsync(FrasesMisVentas.ReglasPermiso);
        await g.NarrarAsync(FrasesMisVentas.Listo);
        await page.WaitForTimeoutAsync(600);
    }

    // ===================== Tema "borradores": recuperar una venta sin guardar =====================
    // Verificado en el codigo (2026-11-10, pos-borrador.js, POS.cshtml POSDraft, VentasController.Borradores.cs):
    //  - el carrito se guarda en el navegador (localStorage) y en el servidor ("Venta resguardada HH:MM", latido cada
    //    20 s); al volver a entrar al POS en el mismo navegador, POSDraft.restore() lo vuelve a cargar solo;
    //  - si el navegador no lo tiene (otra PC, datos borrados), queda en el servidor: campana de arriba (las propias)
    //    y F1 -> "Ver ventas sin guardar" (toda la sucursal: usuario, inicio, ultima senal, estado, cliente, total);
    //  - "En uso" mientras tuvo latido en los ultimos 5 min (PosBorradorSettings); si es propia se carga igual
    //    confirmando ("Figura en uso" -> "Cargar igual"); "Interrumpida" se carga directo. Cerrar la pestana
    //    (pagehide) la deja interrumpida enseguida; un corte de luz no avisa nada.
    //  - "Descartar" pide motivo (queda registrado y se avisa al administrador); las de otro usuario piden supervisor.
    // Simulacion: el "corte de luz" es una recarga que no avisa al servidor (PosNavegandoInternoPOS) y la "otra
    // computadora" ademas borra el carrito del navegador antes de recargar.

    private static class FrasesBorradores
    {
        public const string Resguardo = "Mientras cargás una venta, el punto de venta la va guardando. Abajo dice venta resguardada, con la hora.";
        public const string Corte = "Si se corta la luz o se cierra el navegador antes de cobrar, la venta no se pierde.";
        public const string MismaPc = "Al volver a entrar al punto de venta en la misma computadora, la venta aparece sola en el carrito.";
        public const string Seguir = "Seguís con la venta como si nada.";
        public const string OtraPc = "Si entrás desde otra computadora, o el navegador no la guardó, la venta queda en el sistema.";
        public const string Campana = "La campana de arriba avisa que tenés una venta sin guardar.";
        public const string F1 = "Para verlas, presioná F1 y tocá Ver ventas sin guardar.";
        public const string Lista = "Ves quién la cargaba, cuándo empezó, la última señal, el estado, el cliente y el total.";
        public const string Items = "Con Ver ítems ves los productos.";
        public const string Descartar = "Si no se va a cobrar, Descartar pide un motivo; queda registrado y se avisa al administrador.";
        public const string Otro = "Las ventas de otro usuario piden la autorización de un supervisor.";
        public const string Cargar = "Para seguirla, tocá Cargar al carrito.";
        public const string EnUso = "Si el corte fue hace un rato, figura en uso unos minutos. Como es tuya, confirmá con Cargar igual.";
        public const string Cargada = "La venta vuelve al carrito. Revisá los precios: son los de cuando se armó la venta.";
        public const string Cobrar = "Y cobrala como siempre: Fin y el uno, efectivo.";
        public const string Listo = "Listo. Así se recupera una venta que quedó sin guardar.";

        public static readonly string[] Todas =
        {
            Resguardo, Corte, MismaPc, Seguir, OtraPc, Campana, F1, Lista, Items, Descartar, Otro, Cargar, EnUso, Cargada,
            Cobrar, Listo
        };
    }

    // Script de inicio: con la marca "__otraPc" (la deja CorteYVolverAsync), borra el carrito guardado en el navegador
    // antes de que el POS lo lea, como si la pagina se abriera en otra computadora.
    private const string ScriptOtraPc = @"(function () {
        try {
            if (sessionStorage.getItem('__otraPc') !== '1') return;
            sessionStorage.removeItem('__otraPc');
            Object.keys(localStorage).filter(function (k) { return k.indexOf('carnisys-pos-venta-en-curso') === 0; })
                .forEach(function (k) { localStorage.removeItem(k); });
        } catch (e) { console.warn('AyudaMedia: no se pudo simular la otra computadora', e); }
    })();";

    // Fuera de camara: caja abierta (si hace falta) y una venta a la clienta en el carrito, ya resguardada en el servidor.
    private static async Task PrepararBorradoresAsync(IPage page)
    {
        await page.Context.AddInitScriptAsync(ScriptOtraPc);
        await PosComun.EntrarAlPosAsync(page, null);
        await CargarSilencioAsync(page, ("9901", "1.5"), ("9902", "1"));
        await ElegirClienteAsync(page, null);
        await page.WaitForFunctionAsync("() => /resguardada/i.test(document.getElementById('posBorradorEstado')?.textContent || '')",
            null, new PageWaitForFunctionOptions { Timeout = 15000, PollingInterval = 200 });
        await page.Mouse.MoveAsync(PosComun.AnchoVista / 2f, PosComun.AltoVista / 2f);
        await page.WaitForTimeoutAsync(1500);
    }

    // Pantalla negra con un cartel (el "corte de luz" / "otra computadora"), y recarga del POS sin avisar al servidor
    // (como un corte de luz). borrarNavegador = ademas se borra el carrito guardado en el navegador.
    private static async Task CorteYVolverAsync(IPage page, string cartel, bool borrarNavegador)
    {
        await page.EvaluateAsync(@"(t) => {
            var d = document.createElement('div');
            d.style.cssText = 'position:fixed;inset:0;background:#0b0f14;z-index:2147483647;display:flex;align-items:center;justify-content:center;color:#fff;font:600 34px Arial,sans-serif;opacity:0;transition:opacity .5s';
            d.textContent = t;
            document.body.appendChild(d);
            requestAnimationFrame(function () { d.style.opacity = '1'; });
        }", cartel);
        await page.WaitForTimeoutAsync(2600);
        // El carrito del navegador no se puede borrar antes de salir: el beforeunload de pos-multi-instance.js lo
        // vuelve a guardar. Se deja una marca y lo borra el script de inicio de la pagina nueva (ScriptOtraPc), antes
        // de que el POS lo lea.
        await page.RunAndWaitForNavigationAsync(() => page.EvaluateAsync(@"(borrar) => {
                window.PosNavegandoInternoPOS = true;
                if (window.desactivarAvisoSalidaPOS) window.desactivarAvisoSalidaPOS();
                if (borrar) sessionStorage.setItem('__otraPc', '1');
                location.reload();
            }", borrarNavegador),
            new PageRunAndWaitForNavigationOptions { WaitUntil = WaitUntilState.DOMContentLoaded, Timeout = 20000 });
        await PosComun.EsperarPosListoAsync(page);
        await page.WaitForTimeoutAsync(800);
    }

    private static async Task<string> GrabarBorradoresAsync(IPage page, Grabador g)
    {
        // 1) El resguardo automatico.
        await page.EvaluateAsync("() => marcar(document.getElementById('posBorradorEstado'), 1)");
        await g.NarrarAsync(FrasesBorradores.Resguardo);
        await page.EvaluateAsync("() => limpiarMarcas()");

        // 2) Corte de luz y vuelta en la misma computadora: el navegador la restaura solo.
        await g.NarrarAsync(FrasesBorradores.Corte, 100);
        await CorteYVolverAsync(page, "Se cortó la luz…", false);
        await page.WaitForFunctionAsync("() => document.querySelectorAll('#tablaItems tr.fila-item').length >= 2", null,
            new PageWaitForFunctionOptions { Timeout = 10000, PollingInterval = 200 });
        await page.EvaluateAsync("() => marcar(document.getElementById('tablaItems'), 1)");
        await g.NarrarAsync(FrasesBorradores.MismaPc);
        await page.EvaluateAsync("() => limpiarMarcas()");
        await g.NarrarAsync(FrasesBorradores.Seguir);

        // 3) Otra computadora: el navegador no la tiene; queda en el servidor.
        await g.NarrarAsync(FrasesBorradores.OtraPc, 100);
        await CorteYVolverAsync(page, "Otra computadora", true);
        await page.WaitForFunctionAsync(
            "() => !document.querySelector('#tablaItems tr.fila-item') && !document.getElementById('campanaVentasPOSContador').classList.contains('d-none')",
            null, new PageWaitForFunctionOptions { Timeout = 15000, PollingInterval = 200 });
        await page.EvaluateAsync("() => marcar(document.getElementById('campanaVentasPOS'), 1)");
        await g.NarrarAsync(FrasesBorradores.Campana);
        await page.EvaluateAsync("() => limpiarMarcas()");

        // 4) F1 -> Ver ventas sin guardar.
        await g.NarrarAsync(FrasesBorradores.F1, 100);
        await PosComun.ApretarAsync(page, "F1", "F1");
        await page.Locator("#colVentasSinCerrarAyuda button").WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible, Timeout = 10000 });
        await page.WaitForTimeoutAsync(800);
        await PosComun.MoverYClicAsync(page, "#colVentasSinCerrarAyuda button");
        string selFila = "#contenedorVentasSinCerrarPOS > .table-responsive > table > tbody > tr[data-fic-revisada='si']:not(.bg-light)";
        await page.Locator(selFila).First.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible, Timeout = 15000 });
        await page.WaitForTimeoutAsync(1200);
        await page.EvaluateAsync("(s) => marcar(document.querySelector(s), 1)", selFila);
        await g.NarrarAsync(FrasesBorradores.Lista);
        await page.EvaluateAsync("() => limpiarMarcas()");
        await g.NarrarAsync(FrasesBorradores.Items, 100);
        await PosComun.MoverYClicAsync(page, selFila + " button.btn-outline-secondary");
        await page.WaitForTimeoutAsync(1800);
        await page.EvaluateAsync("(s) => marcar(document.querySelector(s + ' button.btn-outline-danger'), 1)", selFila);
        await g.NarrarAsync(FrasesBorradores.Descartar);
        await page.EvaluateAsync("() => limpiarMarcas()");
        await g.NarrarAsync(FrasesBorradores.Otro);

        // 5) Cargar al carrito (figura en uso: es propia, se confirma con "Cargar igual").
        await g.NarrarAsync(FrasesBorradores.Cargar, 100);
        await PosComun.MoverYClicAsync(page, selFila + " button.btn-success");
        await page.Locator(".swal2-popup").WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible, Timeout = 10000 });
        await page.WaitForTimeoutAsync(900);
        if (await page.Locator(".swal2-popup:has-text('Figura en uso')").CountAsync() > 0)
        {
            await g.NarrarAsync(FrasesBorradores.EnUso, 100);
            await PosComun.MoverYClicAsync(page, ".swal2-confirm");
        }
        await page.WaitForFunctionAsync("() => document.querySelectorAll('#tablaItems tr.fila-item').length >= 2", null,
            new PageWaitForFunctionOptions { Timeout = 15000, PollingInterval = 200 });
        await page.WaitForTimeoutAsync(900);
        await page.EvaluateAsync("() => marcar(document.getElementById('tablaItems'), 1)");
        await g.NarrarAsync(FrasesBorradores.Cargada);
        await page.EvaluateAsync("() => limpiarMarcas()");
        // El aviso "Venta cargada al carrito" se cierra solo (3,5 s); se espera que no quede encima antes de cobrar.
        await page.WaitForFunctionAsync("() => !document.querySelector('.swal2-popup')", null,
            new PageWaitForFunctionOptions { Timeout = 10000, PollingInterval = 200 });

        // 6) Cobrar.
        await g.NarrarAsync(FrasesBorradores.Cobrar, 100);
        await page.Mouse.MoveAsync(PosComun.AnchoVista / 2f, PosComun.AltoVista - 60, new MouseMoveOptions { Steps = 10 });
        await page.EvaluateAsync("() => { if (document.activeElement && document.activeElement.blur) document.activeElement.blur(); }");
        await PosComun.ApretarAsync(page, "End", "Fin");
        await page.Locator("#modalFormaPago").WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        await page.WaitForTimeoutAsync(900);
        await page.EvaluateAsync("() => { if (document.activeElement && document.activeElement.blur) document.activeElement.blur(); }");
        await PosComun.ApretarAsync(page, "1", "1");
        string idVenta = await PosComun.EsperarPostVentaAsync(page);
        await page.WaitForTimeoutAsync(800);
        await g.NarrarAsync(FrasesBorradores.Listo);
        await page.WaitForTimeoutAsync(600);
        return idVenta;
    }
}
