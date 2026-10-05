// Genera las imagenes y videos del manual de uso (ver docs/11-manual-de-uso/README.md, seccion "Imagenes y videos").
// Abre un Chromium headless contra un WebCore YA corriendo, inicia sesion con un usuario de la base de DESARROLLO,
// marca con recuadros numerados los controles de la pantalla, saca las capturas y graba un clip (.webm, sin audio).
// Salida: WebCore/wwwroot/ayuda/media/<Controller.Action>/ (los .md los referencian como media:<Controller.Action>/<archivo>).
// Variables de entorno:
//   AYUDA_MEDIA_USER / AYUDA_MEDIA_PASSWORD   (obligatorias) credenciales de un administrador de la base de dev.
//   AYUDA_MEDIA_BASE_URL                      (opcional, default http://localhost:5270)
//   AYUDA_MEDIA_OUT                           (opcional) carpeta raiz de salida; default: WebCore/wwwroot/ayuda/media del repo.
// Uso:  dotnet run --project scripts/AyudaMedia
using Microsoft.Playwright;

const int AnchoVista = 1280;
const int AltoVista = 720;

string Requerida(string nombre) =>
    Environment.GetEnvironmentVariable(nombre)
    ?? throw new InvalidOperationException($"Falta la variable de entorno {nombre}.");

string baseUrl = (Environment.GetEnvironmentVariable("AYUDA_MEDIA_BASE_URL") ?? "http://localhost:5270").TrimEnd('/');
string usuario = Requerida("AYUDA_MEDIA_USER");
string clave = Requerida("AYUDA_MEDIA_PASSWORD");

// Raiz del repo: se sube desde el directorio actual hasta encontrar CarniSys.sln.
string raiz = Directory.GetCurrentDirectory();
while (!File.Exists(Path.Combine(raiz, "CarniSys.sln")))
{
    string? padre = Directory.GetParent(raiz)?.FullName;
    raiz = padre ?? throw new InvalidOperationException("No se encontro CarniSys.sln: correr desde dentro del repo.");
}
string salidaRaiz = Environment.GetEnvironmentVariable("AYUDA_MEDIA_OUT") ?? Path.Combine(raiz, "WebCore", "wwwroot", "ayuda", "media");
string carpetaPantalla = Path.Combine(salidaRaiz, "Usuarios.Index");
Directory.CreateDirectory(carpetaPantalla);
string carpetaVideoTmp = Path.Combine(Path.GetTempPath(), "ayuda-media-" + Guid.NewGuid().ToString("N"));

// Scripts que se inyectan en la pagina. Recuadro numerado: position:fixed para que coincida con la captura de la vista.
const string JsHelpers = @"
window.marcar = function (el, numero) {
    if (!el) return false;
    var r = el.getBoundingClientRect();
    var caja = document.createElement('div');
    caja.setAttribute('data-ayuda-marca', '1');
    caja.style.cssText = 'position:fixed;z-index:2147483000;pointer-events:none;border:3px solid #e11d48;border-radius:6px;box-shadow:0 0 0 2px rgba(255,255,255,.85);'
        + 'left:' + (r.left - 4) + 'px;top:' + (r.top - 4) + 'px;width:' + (r.width + 8) + 'px;height:' + (r.height + 8) + 'px;';
    var chapa = document.createElement('div');
    chapa.textContent = String(numero);
    chapa.style.cssText = 'position:absolute;left:' + (r.left < 24 ? 2 : -14) + 'px;top:-14px;width:26px;height:26px;border-radius:50%;background:#e11d48;color:#fff;font:700 15px/26px Arial,sans-serif;text-align:center;box-shadow:0 1px 4px rgba(0,0,0,.4);';
    caja.appendChild(chapa);
    document.body.appendChild(caja);
    return true;
};
window.limpiarMarcas = function () {
    document.querySelectorAll('[data-ayuda-marca]').forEach(function (n) { n.remove(); });
};
// Cartel con el paso en curso (para los videos, que no tienen audio).
window.cartel = function (texto) {
    var c = document.getElementById('__cartel');
    if (!c) {
        c = document.createElement('div');
        c.id = '__cartel';
        c.style.cssText = 'position:fixed;z-index:2147483600;left:50%;bottom:64px;transform:translateX(-50%);background:rgba(17,24,39,.92);color:#fff;'
            + 'font:700 34px Arial,sans-serif;padding:12px 28px;border-radius:10px;box-shadow:0 4px 14px rgba(0,0,0,.4);max-width:80%;text-align:center;';
        document.body.appendChild(c);
    }
    c.textContent = texto;
    c.style.display = texto ? 'block' : 'none';
};
";

// El video de Playwright no dibuja el puntero: se simula con un circulo que sigue al mouse.
const string JsPuntero = @"
(function () {
    function crear() {
        if (document.getElementById('__puntero')) return;
        var p = document.createElement('div');
        p.id = '__puntero';
        p.style.cssText = 'position:fixed;z-index:2147483647;pointer-events:none;width:34px;height:34px;margin:-6px 0 0 -6px;border-radius:50%;'
            + 'background:rgba(225,29,72,.55);border:2px solid #e11d48;left:-50px;top:-50px;transition:transform .08s;';
        document.documentElement.appendChild(p);
        document.addEventListener('mousemove', function (e) { p.style.left = e.clientX + 'px'; p.style.top = e.clientY + 'px'; }, true);
        document.addEventListener('mousedown', function () { p.style.transform = 'scale(.6)'; }, true);
        document.addEventListener('mouseup', function () { p.style.transform = 'scale(1)'; }, true);
    }
    if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', crear); else crear();
})();
";

// Selectores (en constantes para no repetirlos entre la captura y el clip).
const string SelCrearUsuario = "a.btn-primary[href*='/Usuarios/Editar']";
const string SelBuscador = "#filtroUsuarios";
const string SelFilas = "#tablaUsuarios tbody tr.js-usuario-row";
const string SelMenuUsuarios = "a.collapse-item[href$='/Usuarios']";

using var playwright = await Playwright.CreateAsync();
await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = true });

// Contexto 1: login y capturas, SIN video (asi el clip no muestra el usuario escribiendo sus datos).
await using var contextoCapturas = await browser.NewContextAsync(new BrowserNewContextOptions
{
    ViewportSize = new ViewportSize { Width = AnchoVista, Height = AltoVista }
});
var page = await contextoCapturas.NewPageAsync();

// ---- Login ----
await page.GotoAsync($"{baseUrl}/Login", new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });
await page.FillAsync("input[name='Usuario']", usuario);
await page.FillAsync("input[name='Clave']", clave);
await page.ClickAsync("button[type='submit']");
try
{
    await page.WaitForURLAsync(url => !url.Contains("/Login"), new PageWaitForURLOptions { Timeout = 20000 });
}
catch (TimeoutException)
{
    // Si el login falla, se muestra lo que dice la pantalla (clave vencida, bloqueo, dispositivo no autorizado...).
    string textoPantalla = await page.InnerTextAsync("body");
    throw new InvalidOperationException($"No se pudo iniciar sesion. Pantalla en {page.Url}:\n{textoPantalla}");
}

// ---- Captura 0: donde esta el menu (Configuracion -> Usuarios), desde el inicio, con el submenu desplegado ----
// El inicio hace polling (notificaciones) y nunca queda "idle": se espera el menu, no la red.
await page.GotoAsync($"{baseUrl}/", new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });
await page.WaitForSelectorAsync("a[data-bs-target='#collapseConfiguracion']");
await page.EvaluateAsync(JsHelpers);
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
await page.EvaluateAsync(JsHelpers);

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
await contextoVideo.AddInitScriptAsync(JsPuntero);
page = await contextoVideo.NewPageAsync();
await page.GotoAsync($"{baseUrl}/Usuarios", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
await page.EvaluateAsync(JsHelpers);
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
await page.EvaluateAsync(JsHelpers);
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
