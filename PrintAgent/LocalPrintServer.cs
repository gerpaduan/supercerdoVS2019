using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Management;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading;
using System.Web.Script.Serialization;

namespace CarniSys.PrintAgent
{
    internal sealed class LocalPrintServer : IDisposable
    {
        private readonly Action<string> _notify;
        private readonly Func<AgentConfig> _getConfig;
        private readonly Action<AgentConfig> _saveConfig;
        private readonly JavaScriptSerializer _serializer;
        private TcpListener _listener;
        private bool _running;
        private Thread _worker;

        public LocalPrintServer(Action<string> notify, Func<AgentConfig> getConfig, Action<AgentConfig> saveConfig)
        {
            _notify = notify;
            _getConfig = getConfig;
            _saveConfig = saveConfig;
            _serializer = new JavaScriptSerializer { MaxJsonLength = int.MaxValue };
        }

        public void Start()
        {
            _listener = new TcpListener(IPAddress.Loopback, 18777);
            _listener.Start();
            _running = true;
            _worker = new Thread(ListenLoop);
            _worker.IsBackground = true;
            _worker.Start();
        }

        public static List<PrinterInfo> GetInstalledPrinters()
        {
            string defaultPrinter = new PrinterSettings().PrinterName;
            return PrinterSettings.InstalledPrinters
                .Cast<string>()
                .Select(name => new PrinterInfo
                {
                    Name = name,
                    IsDefault = string.Equals(name, defaultPrinter, StringComparison.OrdinalIgnoreCase)
                })
                .OrderByDescending(x => x.IsDefault)
                .ThenBy(x => x.Name)
                .ToList();
        }

        // Mismo query WMI que Utilidades/Util_Form.cs (GetCPUId()) -- duplicado a proposito en vez
        // de referenciar el proyecto Utilidades completo, para mantener este agente minimo (unica
        // referencia extra hoy es System.Web.Extensions). Usado para "dispositivos seguros"
        // (Web/Controllers/DispositivosSegurosController.cs): identifica la PC por el ProcessorId,
        // no por IP (la IP cambia; el CPU ID no).
        private static string GetCpuId()
        {
            try
            {
                var mc = new ManagementClass("Win32_Processor");
                foreach (ManagementObject mo in mc.GetInstances())
                {
                    var value = mo.Properties["ProcessorId"].Value;
                    if (value != null && !string.IsNullOrWhiteSpace(value.ToString()))
                        return value.ToString();
                }

                return null;
            }
            catch
            {
                return null;
            }
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
                    if (!_running) break;
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
                            WriteJson(client.GetStream(), new { ok = false, message = ex.Message }, 500);
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
                    WriteJson(stream, new { ok = false, message = "Solicitud inválida." }, 400);
                    return;
                }

                string[] parts = requestLine.Split(' ');
                if (parts.Length < 2)
                {
                    WriteJson(stream, new { ok = false, message = "Solicitud inválida." }, 400);
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

                string body = "";
                if (contentLength > 0)
                {
                    char[] buffer = new char[contentLength];
                    int read = 0;
                    while (read < contentLength)
                    {
                        int chunk = reader.Read(buffer, read, contentLength - read);
                        if (chunk <= 0) break;
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

            if (path == "/health")
            {
                var config = _getConfig();
                WriteJson(stream, new
                {
                    ok = true,
                    app = "CarniSys.PrintAgent",
                    printerName = config != null ? config.PrinterName : "",
                    ticketMm = config != null ? config.TicketMm : 58
                });
                return;
            }

            if (path == "/device-id")
            {
                string deviceId = GetCpuId();
                WriteJson(stream, new
                {
                    ok = !string.IsNullOrEmpty(deviceId),
                    deviceId = deviceId ?? ""
                });
                return;
            }

            if (path == "/printers")
            {
                WriteJson(stream, new
                {
                    ok = true,
                    items = GetInstalledPrinters()
                });
                return;
            }

            if (path == "/config" && method == "GET")
            {
                var config = _getConfig() ?? new AgentConfig();
                WriteJson(stream, new
                {
                    ok = true,
                    printerName = config.PrinterName ?? "",
                    ticketMm = config.TicketMm
                });
                return;
            }

            if (path == "/config" && method == "POST")
            {
                var req = Deserialize<SaveConfigRequest>(body);
                var config = new AgentConfig
                {
                    PrinterName = req != null ? req.PrinterName : "",
                    TicketMm = req != null && req.TicketMm == 80 ? 80 : 58
                };
                _saveConfig(config);
                WriteJson(stream, new { ok = true });
                return;
            }

            if (path == "/print/expendio" && method == "POST")
            {
                var req = Deserialize<TicketPrintRequest>(body);
                var config = _getConfig();
                EscPosTicketPrinter.Print(req, config);
                _notify("Ticket impreso correctamente.");
                WriteJson(stream, new { ok = true });
                return;
            }

            WriteJson(stream, new { ok = false, message = "Operación no soportada." }, 404);
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
                statusCode == 400 ? "Bad Request" : "Internal Server Error";

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
    }
}
