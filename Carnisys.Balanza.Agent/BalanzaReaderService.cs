using System;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;

namespace Carnisys.Balanza.Agent
{
    internal sealed class BalanzaReaderService : IDisposable
    {
        private readonly object _sync = new object();
        private readonly BalanzaDriverRegistry _registry;
        private volatile bool _running;
        private Thread _worker;
        private AgentConfig _config;
        private SerialPort _port;
        private string _portSignature = string.Empty;
        private LecturaPeso _ultimaLectura;

        public BalanzaReaderService(AgentConfig config)
        {
            _registry = new BalanzaDriverRegistry();
            _config = ConfigStore.Normalize(config);
            _ultimaLectura = LecturaPeso.CrearError("Sin lectura disponible.", _config.Balanza);
        }

        public void Start()
        {
            if (_running)
            {
                return;
            }

            _running = true;
            _worker = new Thread(ReadLoop)
            {
                IsBackground = true,
                Name = "Carnisys.Balanza.Reader"
            };
            _worker.Start();
        }

        public void UpdateConfig(AgentConfig config)
        {
            lock (_sync)
            {
                _config = ConfigStore.Normalize(config);
                _portSignature = string.Empty;
                ClosePort();
            }
        }

        public LecturaPeso GetUltimaLectura()
        {
            lock (_sync)
            {
                return _ultimaLectura.Clone();
            }
        }

        public string[] GetPuertos()
        {
            try
            {
                return SerialPort.GetPortNames()
                    .OrderBy(p => p, StringComparer.OrdinalIgnoreCase)
                    .ToArray();
            }
            catch
            {
                return new string[0];
            }
        }

        public BalanzaDriverRegistry GetRegistry()
        {
            return _registry;
        }

        public LecturaPeso Probar(BalanzaConfig config)
        {
            var normalized = ConfigStore.NormalizeBalanza(config);
            var driver = _registry.Create(normalized.Marca);
            if (driver == null)
            {
                return LecturaPeso.CrearError("Marca de balanza no soportada.", normalized);
            }

            if (string.IsNullOrWhiteSpace(normalized.Puerto))
            {
                return LecturaPeso.CrearError("Debe seleccionar un puerto COM.", normalized);
            }

            try
            {
                using (var port = CreatePort(normalized, driver))
                {
                    port.Open();
                    SafeDiscard(port);
                    driver.SolicitarPeso(port);
                    Thread.Sleep(Math.Max(80, Math.Min(normalized.IntervaloLecturaMs, 300)));
                    string raw = port.ReadExisting();
                    var lectura = driver.ParsearRespuesta(raw, normalized);
                    lectura.Conectada = lectura.Conectada || port.IsOpen;
                    lectura.Puerto = normalized.Puerto;
                    lectura.Marca = normalized.Marca;
                    lectura.Modelo = normalized.Modelo;
                    return lectura;
                }
            }
            catch (Exception ex)
            {
                return LecturaPeso.CrearError(ex.Message, normalized);
            }
        }

        private void ReadLoop()
        {
            while (_running)
            {
                AgentConfig config;
                lock (_sync)
                {
                    config = ConfigStore.Normalize(_config);
                }

                var lectura = LeerUnaVez(config);
                lock (_sync)
                {
                    _ultimaLectura = lectura;
                }

                int espera = config.Balanza != null ? config.Balanza.IntervaloLecturaMs : 150;
                Thread.Sleep(Math.Max(100, espera));
            }
        }

        private LecturaPeso LeerUnaVez(AgentConfig config)
        {
            var balanzaConfig = ConfigStore.NormalizeBalanza(config.Balanza);
            var driver = _registry.Create(balanzaConfig.Marca);
            if (driver == null)
            {
                return LecturaPeso.CrearError("Marca de balanza no soportada.", balanzaConfig);
            }

            if (string.IsNullOrWhiteSpace(balanzaConfig.Puerto))
            {
                return LecturaPeso.CrearError("Balanza sin puerto configurado.", balanzaConfig);
            }

            try
            {
                EnsurePort(balanzaConfig, driver);
                SafeDiscard(_port);
                driver.SolicitarPeso(_port);
                Thread.Sleep(Math.Max(60, Math.Min(balanzaConfig.IntervaloLecturaMs, 250)));
                string raw = _port.ReadExisting();
                var lectura = driver.ParsearRespuesta(raw, balanzaConfig);
                lectura.Conectada = _port != null && _port.IsOpen && (lectura.Conectada || string.IsNullOrWhiteSpace(lectura.Error));
                return lectura;
            }
            catch (Exception ex)
            {
                lock (_sync)
                {
                    ClosePort();
                }
                return LecturaPeso.CrearError(ex.Message, balanzaConfig);
            }
        }

        private void EnsurePort(BalanzaConfig config, IBalanzaDriver driver)
        {
            lock (_sync)
            {
                string nextSignature = BuildSignature(config, driver);
                if (_port != null && _port.IsOpen && string.Equals(_portSignature, nextSignature, StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }

                ClosePort();
                _port = CreatePort(config, driver);
                _port.Open();
                _portSignature = nextSignature;
            }
        }

        private static string BuildSignature(BalanzaConfig config, IBalanzaDriver driver)
        {
            return string.Join("|", new[]
            {
                config.Puerto ?? string.Empty,
                config.BaudRate.ToString(),
                config.DataBits.ToString(),
                config.Parity ?? string.Empty,
                config.StopBits ?? string.Empty,
                driver.Marca
            });
        }

        private static SerialPort CreatePort(BalanzaConfig config, IBalanzaDriver driver)
        {
            var port = new SerialPort
            {
                PortName = config.Puerto,
                BaudRate = config.BaudRate,
                DataBits = config.DataBits,
                Parity = ParseParity(config.Parity),
                StopBits = ParseStopBits(config.StopBits),
                Handshake = Handshake.None,
                ReadTimeout = 250,
                WriteTimeout = 250,
                Encoding = Encoding.ASCII,
                DtrEnable = false,
                RtsEnable = false
            };

            driver.PrepararPuerto(port, config);
            return port;
        }

        private static Parity ParseParity(string parity)
        {
            return Enum.TryParse(parity ?? "None", true, out Parity value) ? value : Parity.None;
        }

        private static StopBits ParseStopBits(string stopBits)
        {
            if (Enum.TryParse(stopBits ?? "One", true, out StopBits value) && value != StopBits.None)
            {
                return value;
            }

            return StopBits.One;
        }

        private static void SafeDiscard(SerialPort port)
        {
            if (port == null || !port.IsOpen)
            {
                return;
            }

            try { port.DiscardInBuffer(); } catch { }
            try { port.DiscardOutBuffer(); } catch { }
        }

        private void ClosePort()
        {
            if (_port == null)
            {
                return;
            }

            try
            {
                if (_port.IsOpen)
                {
                    _port.Close();
                }
            }
            catch
            {
            }
            finally
            {
                _port.Dispose();
                _port = null;
                _portSignature = string.Empty;
            }
        }

        public void Dispose()
        {
            _running = false;
            if (_worker != null && _worker.IsAlive)
            {
                _worker.Join(500);
            }

            lock (_sync)
            {
                ClosePort();
            }
        }
    }
}
