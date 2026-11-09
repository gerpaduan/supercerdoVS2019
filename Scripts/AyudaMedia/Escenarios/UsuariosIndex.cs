// Escenario Usuarios.Index (piloto del 2026-10-03): 3 capturas con controles numerados (menu, listado, acciones de
// una fila) y un clip mudo con carteles "buscar un usuario y abrir su edicion" (no guarda nada).
// Movido sin cambios de comportamiento desde el Program.cs original (2026-11-09, generador por escenarios).
using AyudaMedia.Comun;
using Microsoft.Playwright;

namespace AyudaMedia.Escenarios;

public sealed class UsuariosIndex : IEscenario
{
    public string Nombre => "usuarios";
    public string Descripcion => "Usuarios.Index: capturas (menu, listado, acciones) y clip buscar-y-modificar.webm";

    // Selectores (en constantes para no repetirlos entre la captura y el clip).
    private const string SelCrearUsuario = "a.btn-primary[href*='/Usuarios/Editar']";
    private const string SelBuscador = "#filtroUsuarios";
    private const string SelFilas = "#tablaUsuarios tbody tr.js-usuario-row";
    private const string SelMenuUsuarios = "a.collapse-item[href$='/Usuarios']";

    public async Task EjecutarAsync(IBrowser browser, Config cfg, IReadOnlyList<string> args)
    {
        const int AnchoVista = Config.AnchoVista;
        const int AltoVista = Config.AltoVista;
        string baseUrl = cfg.BaseUrl;
        string carpetaPantalla = cfg.CarpetaPantalla("Usuarios.Index");
        string carpetaVideoTmp = Path.Combine(Path.GetTempPath(), "ayuda-media-" + Guid.NewGuid().ToString("N"));

        // Contexto 1: login y capturas, SIN video (asi el clip no muestra el usuario escribiendo sus datos).
        await using var contextoCapturas = await browser.NewContextAsync(new BrowserNewContextOptions
        {
            ViewportSize = new ViewportSize { Width = AnchoVista, Height = AltoVista }
        });
        var page = await contextoCapturas.NewPageAsync();
        await Sesion.IniciarAsync(page, cfg);

        // ---- Captura 0: donde esta el menu (Configuracion -> Usuarios), desde el inicio, con el submenu desplegado ----
        // El inicio hace polling (notificaciones) y nunca queda "idle": se espera el menu, no la red.
        await page.GotoAsync($"{baseUrl}/", new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });
        await page.WaitForSelectorAsync("a[data-bs-target='#collapseConfiguracion']");
        await page.EvaluateAsync(Overlay.JsHelpers);
        await page.ClickAsync("a[data-bs-target='#collapseConfiguracion']");
        await page.WaitForSelectorAsync($"#collapseConfiguracion.show {SelMenuUsuarios}");
        await page.WaitForTimeoutAsync(600); // termina la animacion del submenu
        await page.EvaluateAsync(@"(a) => {
    var enlace = document.querySelector(a.usuarios);
    enlace.scrollIntoView({ block: 'center' });
    marcar(document.querySelector(a.toggle), 1);
    marcar(enlace, 2);
}", new { usuarios = SelMenuUsuarios, toggle = "a[data-bs-target='#collapseConfiguracion']" });
        await page.ScreenshotAsync(new PageScreenshotOptions
        {
            Path = Path.Combine(carpetaPantalla, "menu.png"),
            Clip = new Clip { X = 0, Y = 0, Width = 320, Height = AltoVista }
        });
        await page.EvaluateAsync("limpiarMarcas()");

        await page.GotoAsync($"{baseUrl}/Usuarios", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        await page.EvaluateAsync(Overlay.JsHelpers);

        // ---- Captura 1: el listado con los 3 controles principales numerados ----
        // Fila "modelo" para marcar las acciones: la que tenga mas botones (asi se ve Desbloquear / Quitar PIN si existen).
        int filaModelo = await page.EvaluateAsync<int>(@"(sel) => {
    var filas = Array.from(document.querySelectorAll(sel));
    var mejor = 0, max = -1;
    filas.forEach(function (f, i) { var n = f.querySelectorAll('td:last-child a, td:last-child button').length; if (n > max) { max = n; mejor = i; } });
    return mejor;
}", SelFilas);
        await page.EvaluateAsync(@"(a) => {
    marcar(document.querySelector(a.crear), 1);
    marcar(document.querySelector(a.buscador).closest('.input-group'), 2);
    marcar(document.querySelectorAll(a.filas)[a.fila].querySelector('td:last-child'), 3);
}", new { crear = SelCrearUsuario, buscador = SelBuscador, filas = SelFilas, fila = filaModelo });
        await page.ScreenshotAsync(new PageScreenshotOptions { Path = Path.Combine(carpetaPantalla, "listado.png") });
        await page.EvaluateAsync("limpiarMarcas()");

        // ---- Captura 2: recorte de la fila modelo con cada boton de la fila numerado ----
        await page.EvaluateAsync(@"(a) => { document.querySelectorAll(a.filas)[a.fila].scrollIntoView({ block: 'center' }); }",
            new { filas = SelFilas, fila = filaModelo });
        await page.EvaluateAsync(@"(a) => {
    var f = document.querySelectorAll(a.filas)[a.fila];
    Array.from(f.querySelectorAll('td:last-child a, td:last-child button')).forEach(function (b, i) { marcar(b, i + 1); });
}", new { filas = SelFilas, fila = filaModelo });
        var caja = await page.Locator(SelFilas).Nth(filaModelo).BoundingBoxAsync()
            ?? throw new InvalidOperationException("No se pudo medir la fila modelo.");
        float recorteY = Math.Max(0, caja.Y - 40);
        await page.ScreenshotAsync(new PageScreenshotOptions
        {
            Path = Path.Combine(carpetaPantalla, "acciones.png"),
            // Desde x=240 para dejar afuera el menu lateral: en la ayuda solo interesa la tabla.
            Clip = new Clip { X = 240, Y = recorteY, Width = AnchoVista - 240, Height = Math.Min(AltoVista - recorteY, caja.Height + 80) }
        });
        await page.EvaluateAsync("limpiarMarcas()");

        // ---- Clip: buscar un usuario y abrir su edicion (no modifica nada: no se guarda) ----
        // Contexto 2: reusa la sesion del contexto 1 (storage state), asi el video arranca ya dentro del listado.
        string estadoSesion = await contextoCapturas.StorageStateAsync();
        await contextoCapturas.CloseAsync();
        await using var contextoVideo = await browser.NewContextAsync(new BrowserNewContextOptions
        {
            ViewportSize = new ViewportSize { Width = AnchoVista, Height = AltoVista },
            StorageState = estadoSesion,
            RecordVideoDir = carpetaVideoTmp,
            RecordVideoSize = new RecordVideoSize { Width = AnchoVista, Height = AltoVista }
        });
        await contextoVideo.AddInitScriptAsync(Overlay.JsPuntero);
        page = await contextoVideo.NewPageAsync();
        await page.GotoAsync($"{baseUrl}/Usuarios", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        await page.EvaluateAsync(Overlay.JsHelpers);
        await page.Mouse.MoveAsync(AnchoVista / 2, 120);
        await page.EvaluateAsync("cartel('Para encontrar un usuario, escribí en Buscar usuario')");
        await page.WaitForTimeoutAsync(1500);
        var cajaFiltro = await page.Locator(SelBuscador).BoundingBoxAsync()
            ?? throw new InvalidOperationException("No se pudo medir el buscador.");
        float xFiltro = cajaFiltro.X + 40;
        float yFiltro = cajaFiltro.Y + cajaFiltro.Height / 2;
        await page.Mouse.MoveAsync(xFiltro, yFiltro, new MouseMoveOptions { Steps = 25 });
        await page.Mouse.ClickAsync(xFiltro, yFiltro);
        // El texto a buscar sale del primer usuario del listado (asi el clip no depende de datos fijos).
        string textoBusqueda = await page.EvaluateAsync<string>(
            "(sel) => (document.querySelector(sel + ' td:nth-child(3)')?.textContent || '').trim()", SelFilas);
        await page.Keyboard.TypeAsync(textoBusqueda, new KeyboardTypeOptions { Delay = 180 });
        await page.WaitForTimeoutAsync(1500);
        await page.EvaluateAsync("cartel('La lista se filtra mientras escribís')");
        await page.WaitForTimeoutAsync(2000);
        await page.EvaluateAsync("cartel('Con Modificar abrís la ficha del usuario')");
        var cajaBoton = await page.Locator(SelFilas + ":not(.d-none) a:has-text('Modificar')").First.BoundingBoxAsync()
            ?? throw new InvalidOperationException("No se encontro el boton Modificar.");
        await page.Mouse.MoveAsync(cajaBoton.X + cajaBoton.Width / 2, cajaBoton.Y + cajaBoton.Height / 2, new MouseMoveOptions { Steps = 30 });
        await page.WaitForTimeoutAsync(1200);
        await page.Mouse.DownAsync();
        await page.Mouse.UpAsync();
        await page.WaitForURLAsync(url => url.Contains("/Usuarios/Editar"), new PageWaitForURLOptions { Timeout = 15000 });
        await page.EvaluateAsync(Overlay.JsHelpers);
        await page.EvaluateAsync("cartel('Acá editás los datos. Si salís sin Guardar, no se cambia nada')");
        await page.WaitForTimeoutAsync(3000);

        // El .webm se cierra al cerrar el contexto; despues se copia a su nombre final.
        var video = page.Video ?? throw new InvalidOperationException("El contexto no grabo video.");
        await contextoVideo.CloseAsync();
        await video.SaveAsAsync(Path.Combine(carpetaPantalla, "buscar-y-modificar.webm"));
        try
        {
            Directory.Delete(carpetaVideoTmp, recursive: true);
        }
        catch (IOException ex)
        {
            Console.Error.WriteLine($"No se pudo borrar la carpeta temporal {carpetaVideoTmp}: {ex.Message}");
        }

        foreach (string archivo in Directory.GetFiles(carpetaPantalla))
            Console.WriteLine($"{Path.GetFileName(archivo)}  {new FileInfo(archivo).Length / 1024} KB");
    }
}
