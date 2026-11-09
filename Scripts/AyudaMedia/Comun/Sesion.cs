// Inicio de sesion en WebCore para los escenarios. Se hace en un contexto SIN video, asi ningun clip muestra al
// usuario escribiendo sus datos; despues el estado de la sesion (cookies) se reusa en el contexto que graba.
using Microsoft.Playwright;

namespace AyudaMedia.Comun;

public static class Sesion
{
    // Abre /Login, completa usuario y clave y espera salir del login. Si no sale en 20 s, falla mostrando lo que dice
    // la pantalla (clave vencida, bloqueo, dispositivo no autorizado...). Deja la pagina ya logueada.
    public static async Task IniciarAsync(IPage page, Config cfg)
    {
        await page.GotoAsync($"{cfg.BaseUrl}/Login", new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });
        await page.FillAsync("input[name='Usuario']", cfg.Usuario);
        await page.FillAsync("input[name='Clave']", cfg.Clave);
        await page.ClickAsync("button[type='submit']");
        try
        {
            await page.WaitForURLAsync(url => !url.Contains("/Login"), new PageWaitForURLOptions { Timeout = 20000 });
        }
        catch (TimeoutException)
        {
            string textoPantalla = await page.InnerTextAsync("body");
            throw new InvalidOperationException($"No se pudo iniciar sesion. Pantalla en {page.Url}:\n{textoPantalla}");
        }
    }
}
