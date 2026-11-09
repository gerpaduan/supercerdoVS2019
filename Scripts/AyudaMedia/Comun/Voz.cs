// Voz de los tutoriales: convierte cada texto del guion en un .wav con un motor TTS local. Hoy Piper
// (OSS, gratis, sin servicios externos; decision 2026-11-09 en docs/DECISIONS.md). Detras de una interfaz para poder
// cambiar de motor sin tocar los escenarios.
using System.Diagnostics;
using System.Text;

namespace AyudaMedia.Comun;

public interface ISintetizadorVoz
{
    // Genera el .wav del texto en rutaWav y devuelve su duracion.
    Task<TimeSpan> SintetizarAsync(string texto, string rutaWav);
}

public sealed class PiperSintetizador : ISintetizadorVoz
{
    private readonly string _exe;
    private readonly string _modelo;
    private readonly double _velocidad;

    // AYUDA_MEDIA_PIPER: ruta a piper.exe. AYUDA_MEDIA_VOZ: ruta al modelo .onnx (con su .onnx.json al lado).
    // velocidad: 1 = normal, 1.1 = 10% mas rapido. Piper la aplica al generar (--length_scale = 1/velocidad), asi
    // la voz suena natural, sin estirar el audio despues.
    public static PiperSintetizador DesdeEntorno(double velocidad = 1.0)
    {
        if (velocidad < 0.5 || velocidad > 2.0)
            throw new ArgumentOutOfRangeException(nameof(velocidad), "Velocidad de voz fuera de rango (0.5 a 2).");
        static string Requerida(string nombre) =>
            Environment.GetEnvironmentVariable(nombre)
            ?? throw new InvalidOperationException($"Falta la variable de entorno {nombre} (ver docs/11-manual-de-uso/README.md).");

        string exe = Requerida("AYUDA_MEDIA_PIPER");
        string modelo = Requerida("AYUDA_MEDIA_VOZ");
        if (!File.Exists(exe)) throw new FileNotFoundException("No existe piper.exe", exe);
        if (!File.Exists(modelo)) throw new FileNotFoundException("No existe el modelo de voz", modelo);
        return new PiperSintetizador(exe, modelo, velocidad);
    }

    private PiperSintetizador(string exe, string modelo, double velocidad)
    {
        _exe = exe;
        _modelo = modelo;
        _velocidad = velocidad;
    }

    public async Task<TimeSpan> SintetizarAsync(string texto, string rutaWav)
    {
        var psi = new ProcessStartInfo(_exe)
        {
            RedirectStandardInput = true,
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            // Piper lee el texto en UTF-8 (sin BOM: un BOM lo pronunciaria como un caracter mas).
            StandardInputEncoding = new UTF8Encoding(false)
        };
        psi.ArgumentList.Add("--model");
        psi.ArgumentList.Add(_modelo);
        psi.ArgumentList.Add("--output_file");
        psi.ArgumentList.Add(rutaWav);
        psi.ArgumentList.Add("--length_scale");
        psi.ArgumentList.Add((1.0 / _velocidad).ToString("0.###", System.Globalization.CultureInfo.InvariantCulture));

        using var proceso = Process.Start(psi) ?? throw new InvalidOperationException("No se pudo iniciar Piper.");
        await proceso.StandardInput.WriteAsync(texto);
        proceso.StandardInput.Close();
        string errores = await proceso.StandardError.ReadToEndAsync();
        await proceso.StandardOutput.ReadToEndAsync();
        await proceso.WaitForExitAsync();
        if (proceso.ExitCode != 0 || !File.Exists(rutaWav))
            throw new InvalidOperationException($"Piper fallo (codigo {proceso.ExitCode}) con el texto '{texto}':\n{errores}");

        return DuracionWav(rutaWav);
    }

    // Duracion de un .wav PCM: tamano del bloque "data" / bytes por segundo del bloque "fmt ".
    internal static TimeSpan DuracionWav(string ruta)
    {
        byte[] bytes = File.ReadAllBytes(ruta);
        int bytesPorSegundo = 0;
        int i = 12; // salta "RIFF" + tamano + "WAVE"
        while (i + 8 <= bytes.Length)
        {
            string id = Encoding.ASCII.GetString(bytes, i, 4);
            int largo = BitConverter.ToInt32(bytes, i + 4);
            if (id == "fmt ")
                bytesPorSegundo = BitConverter.ToInt32(bytes, i + 8 + 8);
            else if (id == "data" && bytesPorSegundo > 0)
                return TimeSpan.FromSeconds((double)largo / bytesPorSegundo);
            i += 8 + largo + (largo % 2);
        }
        throw new InvalidOperationException($"No se pudo leer la duracion de {ruta}.");
    }
}
