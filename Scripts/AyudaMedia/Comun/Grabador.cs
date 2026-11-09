// Graba un clip con voz: captura la pagina por CDP (Page.startScreencast, mejor calidad que el recordVideo de
// Playwright, ver DECISIONS 2026-10-31) y al final arma un .mp4 (H.264 + AAC) con ffmpeg, poniendo cada .wav de la
// narracion en el momento exacto en que se dijo. Solo se graba entre IniciarAsync y FinalizarAsync: el login y la
// preparacion de datos quedan afuera.
// Con franja (altoBanda > 0, pedido del usuario 2026-11-09: "que el subtitulo no tape los controles"): la app se graba
// entera arriba y el subtitulo y la tecla apretada se dibujan con ffmpeg en una franja oscura debajo, fuera de la app.
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using Microsoft.Playwright;

namespace AyudaMedia.Comun;

public sealed class Grabador : IAsyncDisposable
{
    private sealed record Cuadro(double Segundos, string Archivo);
    private sealed record Narracion(string Wav, string Texto, double DesdeSegundos, double DuracionSalida);
    private sealed record Tecla(string Rotulo, double DesdeSegundos);

    // Grabador en curso: lo usa PosComun.ApretarAsync para anotar las teclas sin pasarlo por cada llamada.
    public static Grabador? Actual { get; private set; }

    private readonly IPage _page;
    private readonly ISintetizadorVoz _voz;
    private readonly string _ffmpeg;
    private readonly string _carpetaTmp;
    private readonly ConcurrentQueue<Cuadro> _cuadros = new();
    private readonly List<Narracion> _narraciones = new();
    private readonly List<Tecla> _teclas = new();
    private readonly int _ancho;
    private readonly int _alto;
    private readonly int _altoBanda;
    private ICDPSession? _cdp;
    private double _inicioEpoch;
    private int _contadorCuadros;
    private int _contadorAudios;

    // Cuantas veces mas rapido se ven las imagenes en el .mp4 (la voz NO se acelera aca: su velocidad la da el
    // sintetizador). Para que la voz siga alineada, mientras se graba cada frase se espera su duracion multiplicada
    // por este factor, y en el .mp4 cada audio arranca en (momento real / factor).
    private readonly double _velocidadVideo;

    public bool ConFranja => _altoBanda > 0;

    // AYUDA_MEDIA_FFMPEG: ruta a ffmpeg.exe. ancho/alto: tamano de la vista del navegador (la app). altoBanda: alto de
    // la franja de subtitulos debajo (0 = sin franja: el subtitulo va como cartel dentro de la pagina).
    public Grabador(IPage page, ISintetizadorVoz voz, double velocidadVideo = 1.0,
        int ancho = Config.AnchoVista, int alto = Config.AltoVista, int altoBanda = 0)
    {
        if (velocidadVideo < 0.5 || velocidadVideo > 3.0)
            throw new ArgumentOutOfRangeException(nameof(velocidadVideo), "Velocidad de video fuera de rango (0.5 a 3).");
        if ((alto + altoBanda) % 2 != 0 || ancho % 2 != 0)
            throw new ArgumentException("H.264 necesita ancho y alto pares.");
        _page = page;
        _voz = voz;
        _velocidadVideo = velocidadVideo;
        _ancho = ancho;
        _alto = alto;
        _altoBanda = altoBanda;
        _ffmpeg = Environment.GetEnvironmentVariable("AYUDA_MEDIA_FFMPEG")
            ?? throw new InvalidOperationException("Falta la variable de entorno AYUDA_MEDIA_FFMPEG (ruta a ffmpeg.exe).");
        if (!File.Exists(_ffmpeg)) throw new FileNotFoundException("No existe ffmpeg.exe", _ffmpeg);
        _carpetaTmp = Path.Combine(Path.GetTempPath(), "ayuda-clip-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_carpetaTmp);
    }

    private static double AhoraEpoch() => DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() / 1000.0;

    // Empieza a capturar cuadros. Chromium solo manda un cuadro cuando la pantalla cambia: el tiempo de cada uno sale
    // de su timestamp, asi que una pantalla quieta igual dura lo que tiene que durar.
    public async Task IniciarAsync()
    {
        _cdp = await _page.Context.NewCDPSessionAsync(_page);
        _cdp.Event("Page.screencastFrame").OnEvent += (_, evento) =>
        {
            if (evento is not { } e) return;
            int sesion = e.GetProperty("sessionId").GetInt32();
            // Hay que confirmar cada cuadro o Chromium deja de mandar mas.
            _ = _cdp.SendAsync("Page.screencastFrameAck", new() { { "sessionId", sesion } });

            double ts = e.GetProperty("metadata").TryGetProperty("timestamp", out var t) ? t.GetDouble() : AhoraEpoch();
            int n = Interlocked.Increment(ref _contadorCuadros);
            string archivo = Path.Combine(_carpetaTmp, $"f{n:D6}.jpg");
            File.WriteAllBytes(archivo, Convert.FromBase64String(e.GetProperty("data").GetString() ?? ""));
            _cuadros.Enqueue(new Cuadro(ts, archivo));
        };
        _inicioEpoch = AhoraEpoch();
        Actual = this;
        await _cdp.SendAsync("Page.startScreencast", new()
        {
            { "format", "jpeg" },
            { "quality", 92 },
            { "maxWidth", _ancho },
            { "maxHeight", _alto },
            { "everyNthFrame", 1 }
        });
    }

    private readonly Dictionary<string, (string Wav, TimeSpan Duracion)> _voces = new();

    // Genera de antemano la voz de todas las frases del guion (antes de IniciarAsync), para que el video no tenga
    // silencios mientras Piper trabaja. Las frases repetidas se generan una sola vez.
    public async Task PrepararVocesAsync(IEnumerable<string> textos)
    {
        foreach (string texto in textos.Distinct())
            _voces[texto] = await SintetizarAsync(texto);
    }

    private async Task<(string Wav, TimeSpan Duracion)> SintetizarAsync(string texto)
    {
        string wav = Path.Combine(_carpetaTmp, $"a{Interlocked.Increment(ref _contadorAudios):D3}.wav");
        TimeSpan duracion = await _voz.SintetizarAsync(texto, wav);
        return (wav, duracion);
    }

    // Muestra el subtitulo (en la franja, o como cartel si no hay franja), lo dice en voz alta y espera a que termine
    // (+ pausa). Devuelve recien cuando la frase termino, para que la accion siguiente no se pise con la voz.
    public async Task NarrarAsync(string texto, int pausaMs = 500)
    {
        if (!_voces.TryGetValue(texto, out var voz))
        {
            // Sin preparar, la espera de Piper (1-2 s por frase) queda grabada como pantalla quieta.
            Console.Error.WriteLine($"Aviso: frase no preparada antes de grabar (queda una pausa en el video): '{texto}'");
            voz = await SintetizarAsync(texto);
        }
        var (wav, duracion) = voz;
        if (!ConFranja)
            await _page.EvaluateAsync("(t) => cartel(t)", texto);
        _narraciones.Add(new Narracion(wav, texto, AhoraEpoch() - _inicioEpoch, (duracion.TotalMilliseconds + pausaMs) / 1000.0));
        // En tiempo real se espera mas (x velocidad de video) porque despues las imagenes se comprimen y la voz no.
        await _page.WaitForTimeoutAsync((float)((duracion.TotalMilliseconds + pausaMs) * _velocidadVideo));
    }

    // Anota una tecla apretada: con franja se dibuja en la franja (a la derecha del subtitulo) durante un segundo.
    public void MarcarTecla(string rotulo) => _teclas.Add(new Tecla(rotulo, AhoraEpoch() - _inicioEpoch));

    // Corta la captura y arma el .mp4 en rutaMp4, con las imagenes aceleradas segun la velocidad de video.
    public async Task FinalizarAsync(string rutaMp4)
    {
        string v = _velocidadVideo.ToString("0.###", CultureInfo.InvariantCulture);
        if (_cdp == null) throw new InvalidOperationException("La grabacion no se inicio.");
        await _page.WaitForTimeoutAsync(300);
        await _cdp.SendAsync("Page.stopScreencast");
        double finEpoch = AhoraEpoch();
        await _cdp.DetachAsync();
        Actual = null;

        var cuadros = _cuadros.OrderBy(c => c.Segundos).ToList();
        if (cuadros.Count == 0) throw new InvalidOperationException("No se capturo ningun cuadro.");

        // Lista para el "concat demuxer" de ffmpeg: cada cuadro dura hasta el siguiente. El primer cuadro se estira
        // hacia atras hasta el inicio y el ultimo hasta el final, para que el audio quede alineado con el video.
        var lista = new StringBuilder();
        double desde = _inicioEpoch;
        for (int i = 0; i < cuadros.Count; i++)
        {
            double hasta = i + 1 < cuadros.Count ? cuadros[i + 1].Segundos : finEpoch;
            double dur = Math.Max(0.001, hasta - (i == 0 ? desde : cuadros[i].Segundos));
            lista.Append("file '").Append(cuadros[i].Archivo.Replace('\\', '/')).Append("'\n");
            lista.Append("duration ").Append(dur.ToString("0.000", CultureInfo.InvariantCulture)).Append('\n');
        }
        // El concat demuxer ignora la duracion del ultimo archivo si no se repite.
        lista.Append("file '").Append(cuadros[^1].Archivo.Replace('\\', '/')).Append("'\n");
        File.WriteAllText(Path.Combine(_carpetaTmp, "cuadros.txt"), lista.ToString(), new UTF8Encoding(false));

        var args = new List<string> { "-y", "-hide_banner", "-loglevel", "error", "-f", "concat", "-safe", "0", "-i", "cuadros.txt" };
        foreach (var n in _narraciones) { args.Add("-i"); args.Add(n.Wav); }

        // Video a 30 cuadros por segundo constantes (+ franja con subtitulos y teclas); cada audio con su retraso y
        // despues todos mezclados.
        var filtro = new StringBuilder($"[0:v]setpts=PTS/{v},fps=30,scale={_ancho}:{_alto}");
        if (ConFranja) filtro.Append(FiltroFranja());
        filtro.Append(",format=yuv420p[v]");
        for (int i = 0; i < _narraciones.Count; i++)
        {
            long ms = (long)Math.Round(_narraciones[i].DesdeSegundos / _velocidadVideo * 1000);
            filtro.Append($";[{i + 1}:a]adelay={ms}:all=1[a{i}]");
        }
        if (_narraciones.Count > 0)
        {
            filtro.Append(';');
            for (int i = 0; i < _narraciones.Count; i++) filtro.Append($"[a{i}]");
            filtro.Append($"amix=inputs={_narraciones.Count}:normalize=0[aud]");
        }
        // El filtro va en un archivo: con decenas de subtitulos la linea de comandos quedaria demasiado larga.
        File.WriteAllText(Path.Combine(_carpetaTmp, "filtro.txt"), filtro.ToString(), new UTF8Encoding(false));
        args.AddRange(new[] { "-/filter_complex", "filtro.txt", "-map", "[v]" });
        if (_narraciones.Count > 0) args.AddRange(new[] { "-map", "[aud]", "-c:a", "aac", "-b:a", "128k" });
        args.AddRange(new[] { "-c:v", "libx264", "-preset", "slow", "-crf", "22", "-movflags", "+faststart", Path.GetFullPath(rutaMp4) });

        // Se corre desde la carpeta temporal: los textfile= y la lista van por nombre relativo (sin ':' que escapar).
        var psi = new ProcessStartInfo(_ffmpeg)
        {
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            WorkingDirectory = _carpetaTmp
        };
        foreach (string a in args) psi.ArgumentList.Add(a);
        using var proceso = Process.Start(psi) ?? throw new InvalidOperationException("No se pudo iniciar ffmpeg.");
        string errores = await proceso.StandardError.ReadToEndAsync();
        await proceso.WaitForExitAsync();
        if (proceso.ExitCode != 0)
            throw new InvalidOperationException($"ffmpeg fallo (codigo {proceso.ExitCode}):\n{errores}");
    }

    // Franja oscura debajo de la app: subtitulo centrado (hasta 2 renglones, letra de 24 px) y, a la derecha, la tecla
    // apretada como un recuadro claro durante 1 segundo. Tiempos ya en el reloj del video (acelerado).
    private string FiltroFranja()
    {
        const string Fuente = "fontfile='C\\:/Windows/Fonts/arialbd.ttf'";
        const int AnchoZonaTecla = 170;
        var f = new StringBuilder($",pad={_ancho}:{_alto + _altoBanda}:0:0:color=0x111827");
        string Seg(double s) => s.ToString("0.000", CultureInfo.InvariantCulture);

        for (int i = 0; i < _narraciones.Count; i++)
        {
            var n = _narraciones[i];
            double ini = n.DesdeSegundos / _velocidadVideo;
            double fin = ini + n.DuracionSalida;
            if (i + 1 < _narraciones.Count) fin = Math.Min(fin, _narraciones[i + 1].DesdeSegundos / _velocidadVideo);
            string archivo = $"s{i:D3}.txt";
            File.WriteAllText(Path.Combine(_carpetaTmp, archivo), PartirEnRenglones(n.Texto, 78), new UTF8Encoding(false));
            f.Append($",drawtext={Fuente}:textfile={archivo}:fontsize=24:fontcolor=white:line_spacing=6")
             .Append($":x=({_ancho - AnchoZonaTecla}-text_w)/2:y={_alto}+({_altoBanda}-text_h)/2")
             .Append($":enable='between(t,{Seg(ini)},{Seg(fin)})'");
        }

        for (int i = 0; i < _teclas.Count; i++)
        {
            var k = _teclas[i];
            double ini = k.DesdeSegundos / _velocidadVideo;
            double fin = ini + 1.1;
            string archivo = $"k{i:D3}.txt";
            File.WriteAllText(Path.Combine(_carpetaTmp, archivo), k.Rotulo, new UTF8Encoding(false));
            int x = _ancho - AnchoZonaTecla + 20, y = _alto + (_altoBanda - 46) / 2;
            f.Append($",drawbox=x={x}:y={y}:w={AnchoZonaTecla - 40}:h=46:color=0xF3F4F6@1:t=fill:enable='between(t,{Seg(ini)},{Seg(fin)})'")
             .Append($",drawtext={Fuente}:textfile={archivo}:fontsize=26:fontcolor=0x111827")
             .Append($":x={x}+({AnchoZonaTecla - 40}-text_w)/2:y={y}+(46-text_h)/2:enable='between(t,{Seg(ini)},{Seg(fin)})'");
        }
        return f.ToString();
    }

    // Parte el texto en renglones de hasta "ancho" caracteres, cortando en espacios.
    internal static string PartirEnRenglones(string texto, int ancho)
    {
        var renglones = new List<string>();
        var actual = new StringBuilder();
        foreach (string palabra in texto.Split(' ', StringSplitOptions.RemoveEmptyEntries))
        {
            if (actual.Length > 0 && actual.Length + 1 + palabra.Length > ancho)
            {
                renglones.Add(actual.ToString());
                actual.Clear();
            }
            if (actual.Length > 0) actual.Append(' ');
            actual.Append(palabra);
        }
        if (actual.Length > 0) renglones.Add(actual.ToString());
        return string.Join("\n", renglones);
    }

    public ValueTask DisposeAsync()
    {
        if (Actual == this) Actual = null;
        try
        {
            Directory.Delete(_carpetaTmp, recursive: true);
        }
        catch (IOException ex)
        {
            Console.Error.WriteLine($"No se pudo borrar la carpeta temporal {_carpetaTmp}: {ex.Message}");
        }
        return ValueTask.CompletedTask;
    }
}
