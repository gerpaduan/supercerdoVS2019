// Configuracion comun de todos los escenarios del generador de medios del manual: URL del WebCore ya corriendo,
// credenciales de un usuario de la base de DESARROLLO y carpetas. Todo sale de variables de entorno (nada en el repo).
namespace AyudaMedia.Comun;

public sealed class Config
{
    public const int AnchoVista = 1280;
    public const int AltoVista = 720;

    public required string BaseUrl { get; init; }
    public required string Usuario { get; init; }
    public required string Clave { get; init; }
    // Raiz del repo (donde esta CarniSys.sln).
    public required string RaizRepo { get; init; }
    // Raiz de salida: WebCore/wwwroot/ayuda/media del repo salvo AYUDA_MEDIA_OUT.
    public required string SalidaRaiz { get; init; }

    // Carpeta de salida de una pantalla (clave Controller.Action), creada si no existe.
    public string CarpetaPantalla(string clave)
    {
        string carpeta = Path.Combine(SalidaRaiz, clave);
        Directory.CreateDirectory(carpeta);
        return carpeta;
    }

    // Lee las variables de entorno. Falla con un mensaje claro si falta una obligatoria.
    public static Config DesdeEntorno()
    {
        static string Requerida(string nombre) =>
            Environment.GetEnvironmentVariable(nombre)
            ?? throw new InvalidOperationException($"Falta la variable de entorno {nombre}.");

        // Raiz del repo: se sube desde el directorio actual hasta encontrar CarniSys.sln.
        string raiz = Directory.GetCurrentDirectory();
        while (!File.Exists(Path.Combine(raiz, "CarniSys.sln")))
        {
            string? padre = Directory.GetParent(raiz)?.FullName;
            raiz = padre ?? throw new InvalidOperationException("No se encontro CarniSys.sln: correr desde dentro del repo.");
        }

        return new Config
        {
            BaseUrl = (Environment.GetEnvironmentVariable("AYUDA_MEDIA_BASE_URL") ?? "http://localhost:5270").TrimEnd('/'),
            Usuario = Requerida("AYUDA_MEDIA_USER"),
            Clave = Requerida("AYUDA_MEDIA_PASSWORD"),
            RaizRepo = raiz,
            SalidaRaiz = Environment.GetEnvironmentVariable("AYUDA_MEDIA_OUT") ?? Path.Combine(raiz, "WebCore", "wwwroot", "ayuda", "media")
        };
    }
}
