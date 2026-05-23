using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Web.Script.Serialization;

namespace Carnisys.Balanza.Agent
{
    internal sealed class LocalBalanzaServer : IDisposable
    {
        private readonly JavaScriptSerializer _serializer;
        private readonly Func<AgentConfig> _getConfig;
        private readonly Action<AgentConfig> _saveConfig;
        private readonly Func<LecturaPeso> _getLectura;
        private readonly Func<string[]> _getPuertos;
        private readonly Func<BalanzaConfig, LecturaPeso> _probarLectura;
        private readonly Action<string> _notify;
        private readonly int _port;
        private TcpListener _listener;
        private bool _running;
        private System.Threading.Thread _worker;

        public LocalBalanzaServer(
            int port,
            Func<AgentConfig> getConfig,
            Action<AgentConfig> saveConfig,
            Func<LecturaPeso> getLectura,
            Func<string[]> getPuertos,
            Func<BalanzaConfig, LecturaPeso> probarLectura,
            Action<string> notify)
        {
            _port = port;
            _getConfig = getConfig;
            _saveConfig = saveConfig;
            _getLectura = getLectura;
            _getPuertos = getPuertos;
            _probarLectura = probarLectura;
            _notify = notify;
            _serializer = new JavaScriptSerializer { MaxJsonLength = int.MaxValue };
        }

        public void Start()
        {
            _listener = new TcpListener(IPAddress.Parse("127.0.0.1"), _port);
            _listener.Start();
            _running = true;
            _worker = new System.Threading.Thread(ListenLoop)
            {
                IsBackground = true,
                Name = "Carnisys.Balanza.Http"
            };
            _worker.Start();
        }

        private void ListenLoop()
        {
            while (_running && _listener != null)
            {
                TcpClient client = null;
                try
                {
                    client = _listener.AcceptTcpClient();
                }
                catch
                {
                    if (!_running)
                    {
                        break;
                    }

                    continue;
                }

                using (client)
                {
                    try
                    {
                        HandleClient(client);
                    }
                    catch (Exception ex)
                    {
                        try
                        {
                            WriteJson(client.GetStream(), new { ok = false, error = ex.Message }, 500);
                        }
                        catch
                        {
                        }
                    }
                }
            }
        }

        private void HandleClient(TcpClient client)
        {
            using (var stream = client.GetStream())
            using (var reader = new StreamReader(stream, Encoding.UTF8, false, 8192, true))
            {
                string requestLine = reader.ReadLine();
                if (string.IsNullOrWhiteSpace(requestLine))
                {
                    WriteJson(stream, new { ok = false, error = "Solicitud inválida." }, 400);
                    return;
                }

                string[] parts = requestLine.Split(' ');
                if (parts.Length < 2)
                {
                    WriteJson(stream, new { ok = false, error = "Solicitud inválida." }, 400);
                    return;
                }

                string method = parts[0].Trim().ToUpperInvariant();
                string rawPath = parts[1].Trim();
                string path = rawPath;
                int queryIndex = rawPath.IndexOf('?');
                if (queryIndex >= 0)
                {
                    path = rawPath.Substring(0, queryIndex);
                }

                int contentLength = 0;
                string line;
                while (!string.IsNullOrEmpty(line = reader.ReadLine()))
                {
                    if (line.StartsWith("Content-Length:", StringComparison.OrdinalIgnoreCase))
                    {
                        int.TryParse(line.Substring("Content-Length:".Length).Trim(), out contentLength);
                    }
                }

                string body = string.Empty;
                if (contentLength > 0)
                {
                    char[] buffer = new char[contentLength];
                    int read = 0;
                    while (read < contentLength)
                    {
                        int chunk = reader.Read(buffer, read, contentLength - read);
                        if (chunk <= 0)
                        {
                            break;
                        }

                        read += chunk;
                    }

                    body = new string(buffer, 0, read);
                }

                HandleRequest(stream, method, path.ToLowerInvariant(), body);
            }
        }

        private void HandleRequest(Stream stream, string method, string path, string body)
        {
            if (method == "OPTIONS")
            {
                WriteJson(stream, new { ok = true });
                return;
            }

            if (path == "/status")
            {
                var config = _getConfig();
                var lectura = _getLectura();
                WriteJson(stream, new
                {
                    ok = true,
                    app = "Carnisys.Balanza.Agent",
                    host = "127.0.0.1",
                    port = config.Api.Port,
                    configurada = !string.IsNullOrWhiteSpace(config.Balanza.Puerto),
                    disponible = lectura.Ok,
                    conectada = lectura.Conectada,
                    marca = config.Balanza.Marca,
                    modelo = config.Balanza.Modelo,
                    puerto = config.Balanza.Puerto,
                    error = lectura.Error,
                    fechaHora = lectura.FechaHora.ToString("yyyy-MM-ddTHH:mm:ss")
                });
                return;
            }

            if (path == "/peso")
            {
                WriteJson(stream, ToPesoPayload(_getLectura()));
                return;
            }

            if (path == "/config" && method == "GET")
            {
                var config = _getConfig();
                WriteJson(stream, new
                {
                    ok = true,
                    balanza = config.Balanza,
                    api = config.Api
                });
                return;
            }

            if (path == "/config" && method == "POST")
            {
                var req = Deserialize<SaveConfigRequest>(body) ?? new SaveConfigRequest();
                var current = _getConfig();
                var next = new AgentConfig
                {
                    Balanza = ConfigStore.NormalizeBalanza(req.Balanza ?? new BalanzaConfig
                    {
                        Marca = req.Marca,
                        Modelo = req.Modelo,
                        Puerto = req.Puerto,
                        BaudRate = req.BaudRate,
                        DataBits = req.DataBits,
                        Parity = req.Parity,
                        StopBits = req.StopBits,
                        IntervaloLecturaMs = req.IntervaloLecturaMs
                    }),
                    Api = ConfigStore.NormalizeApi(req.Api ?? current.Api)
                };

                _saveConfig(next);
                _notify("Configuración de balanza guardada.");

                WriteJson(stream, new
                {
                    ok = true,
                    mensaje = "Configuración guardada.",
                    balanza = next.Balanza,
                    api = next.Api
                });
                return;
            }

            if (path == "/puertos")
            {
                WriteJson(stream, new
                {
                    ok = true,
                    puertos = _getPuertos()
                });
                return;
            }

            if (path == "/probar" && method == "POST")
            {
                var req = Deserialize<ProbeRequest>(body) ?? new ProbeRequest();
                var config = ConfigStore.NormalizeBalanza(new BalanzaConfig
                {
                    Marca = req.Marca,
                    Modelo = req.Modelo,
                    Puerto = req.Puerto,
                    BaudRate = req.BaudRate,
                    DataBits = req.DataBits,
                    Parity = req.Parity,
                    StopBits = req.StopBits,
                    IntervaloLecturaMs = req.IntervaloLecturaMs
                });

                var lectura = _probarLectura(config);
                WriteJson(stream, new
                {
                    ok = lectura.Ok,
                    conectada = lectura.Conectada,
                    peso = lectura.Peso,
                    pesoDisplay = lectura.PesoDisplay,
                    mensaje = lectura.Ok ? "Lectura correcta" : (lectura.Error ?? ("No se pudo leer desde " + config.Puerto))
                });
                return;
            }

            WriteJson(stream, new { ok = false, error = "Operación no soportada." }, 404);
        }

        private object ToPesoPayload(LecturaPeso lectura)
        {
            return new
            {
                ok = lectura.Ok,
                conectada = lectura.Conectada,
                peso = lectura.Peso,
                pesoTexto = lectura.PesoTexto,
                pesoDisplay = lectura.PesoDisplay,
                unidad = string.IsNullOrWhiteSpace(lectura.Unidad) ? "kg" : lectura.Unidad,
                estable = lectura.Estable,
                inestable = lectura.Inestable,
                negativo = lectura.Negativo,
                marca = lectura.Marca ?? string.Empty,
                modelo = lectura.Modelo ?? string.Empty,
                puerto = lectura.Puerto ?? string.Empty,
                raw = lectura.Raw ?? string.Empty,
                fechaHora = lectura.FechaHora.ToString("yyyy-MM-ddTHH:mm:ss"),
                error = lectura.Error
            };
        }

        private T Deserialize<T>(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
            {
                return default(T);
            }

            return _serializer.Deserialize<T>(raw);
        }

        private void WriteJson(Stream stream, object data, int statusCode = 200)
        {
            byte[] bodyBytes = Encoding.UTF8.GetBytes(_serializer.Serialize(data));
            string statusText = statusCode == 200 ? "OK" :
                statusCode == 404 ? "Not Found" :
                statusCode == 400 ? "Bad Request" :
                "Internal Server Error";

            string headers =
                "HTTP/1.1 " + statusCode + " " + statusText + "\r\n" +
                "Content-Type: application/json; charset=utf-8\r\n" +
                "Access-Control-Allow-Origin: *\r\n" +
                "Access-Control-Allow-Headers: Content-Type\r\n" +
                "Access-Control-Allow-Methods: GET, POST, OPTIONS\r\n" +
                "Content-Length: " + bodyBytes.Length + "\r\n" +
                "Connection: close\r\n\r\n";

            byte[] headerBytes = Encoding.ASCII.GetBytes(headers);
            stream.Write(headerBytes, 0, headerBytes.Length);
            stream.Write(bodyBytes, 0, bodyBytes.Length);
            stream.Flush();
        }

        public void Dispose()
        {
            _running = false;
            if (_listener != null)
            {
                try { _listener.Stop(); } catch { }
                _listener = null;
            }
        }

        private sealed class SaveConfigRequest
        {
            public BalanzaConfig Balanza { get; set; }
            public ApiConfig Api { get; set; }
            public string Marca { get; set; }
            public string Modelo { get; set; }
            public string Puerto { get; set; }
            public int BaudRate { get; set; }
            public int DataBits { get; set; }
            public string Parity { get; set; }
            public string StopBits { get; set; }
            public int IntervaloLecturaMs { get; set; }
        }

        private sealed class ProbeRequest
        {
            public string Marca { get; set; }
            public string Modelo { get; set; }
            public string Puerto { get; set; }
            public int BaudRate { get; set; }
            public int DataBits { get; set; }
            public string Parity { get; set; }
            public string StopBits { get; set; }
            public int IntervaloLecturaMs { get; set; }
        }
    }
}
