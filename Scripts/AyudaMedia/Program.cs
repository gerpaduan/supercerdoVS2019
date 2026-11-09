// Genera las imagenes y videos del manual de uso (ver docs/11-manual-de-uso/README.md, seccion "Imagenes y videos").
// Abre un Chromium headless contra un WebCore YA corriendo, inicia sesion con un usuario de la base de DESARROLLO y
// corre el escenario pedido (uno por pantalla, en Escenarios/). Salida: WebCore/wwwroot/ayuda/media/<Controller.Action>/.
// Variables de entorno:
//   AYUDA_MEDIA_USER / AYUDA_MEDIA_PASSWORD   (obligatorias) credenciales de un usuario de la base de dev.
//   AYUDA_MEDIA_BASE_URL                      (opcional, default http://localhost:5270)
//   AYUDA_MEDIA_OUT                           (opcional) carpeta raiz de salida; default: WebCore/wwwroot/ayuda/media del repo.
// Uso:  dotnet run --project scripts/AyudaMedia -- <escenario> [argumentos del escenario]
//       (sin escenario, por compatibilidad con el uso anterior, corre "usuarios")
using AyudaMedia.Comun;
using AyudaMedia.Escenarios;
using Microsoft.Playwright;

IEscenario[] escenarios =
{
    new UsuariosIndex(),
    new Tutorial()
};

string nombre = args.Length > 0 ? args[0] : "usuarios";
var escenario = escenarios.FirstOrDefault(e => string.Equals(e.Nombre, nombre, StringComparison.OrdinalIgnoreCase));
if (escenario == null)
{
    Console.Error.WriteLine($"Escenario desconocido '{nombre}'. Disponibles:");
    foreach (var e in escenarios)
        Console.Error.WriteLine($"  {e.Nombre,-12} {e.Descripcion}");
    return 2;
}

var cfg = Config.DesdeEntorno();
using var playwright = await Playwright.CreateAsync();
await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = true });
await escenario.EjecutarAsync(browser, cfg, args.Skip(1).ToList());
return 0;
