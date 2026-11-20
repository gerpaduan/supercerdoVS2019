using System;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;

namespace Carnisys.Balanza.Agent
{
    // Hilo lector de la balanza: abre el puerto COM, pide el peso cada IntervaloLecturaMs y deja la ultima lectura para el HTTP.
    // 2026-11-20 (peso "pegado"): la lectura se guarda con su hora monotona y numero de secuencia; si el hilo se traba en el
    // puerto serie, GetUltimaLectura informa "lectura congelada" (no conectada) en vez de seguir sirviendo el ultimo peso como
    // bueno, e intenta destrabarlo cerrando el puerto. Ver docs/06-datos-e-integraciones/balanzas-e-impresoras.md.
    internal sealed class BalanzaReaderService : IDisposable
    {
        // Una lectura sana se renueva cada ~300 ms. Pasado este tiempo sin lectura nueva el hilo esta trabado (o abriendo el puerto).
        private const int LecturaCongeladaMs = 3000;
        // Con la lectura congelada este tiempo se cierra el puerto desde otro hilo para destrabar la llamada bloqueada.
        private const int RecuperacionPuertoMs = 10000;
        // Tiempo que se espera al pausar el lector para que termine la vuelta en curso (una vuelta dura ~300 ms).
        private const int LectorPausaEsperaMs = 450;

        // _sync protege puerto y config (lo usa el hilo lector); _lecturaSync solo la ultima lectura, para que el HTTP nunca
        // espere a una operacion del puerto serie (que puede colgarse con adaptadores USB-serie).
        private readonly object _sync = new object();
        private readonly object _lecturaSync = new object();
        private readonly BalanzaDriverRegistry _registry;
        private volatile bool _running;
        private Thread _worker;
        private AgentConfig _config;
        private SerialPort _port;
        private string _portSignature = string.Empty;
        private LecturaPeso _ultimaLectura;
        private long _ultimaLecturaTimestamp;
        private long _secuencia;
        private string _ultimoErrorLogueado;
        private bool _congeladaLogueada;
        private int _recuperacionEnCurso;
        // Pausas pedidas por quien prueba puertos (ventana de configuracion, instalador): con _pausas > 0 el lector suelta el COM.
        private int _pausas;

        public BalanzaReaderService(AgentConfig config)
        {
            _registry = new BalanzaDriverRegistry();
            _config = ConfigStore.Normalize(config);
            GuardarLectura(LecturaPeso.CrearError("Sin lectura disponible.", _config.Balanza));
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
            AgentLog.Info("Lector de balanza iniciado. Log en " + AgentLog.Directorio);
        }

        public void UpdateConfig(AgentConfig config)
        {
            lock (_sync)
            {
                _config = ConfigStore.Normalize(config);
                _portSignature = string.Empty;
                ClosePort();
            }
            AgentLog.Info("Configuracion actualizada: " + DescribirConfig(_config.Balanza));
        }

        // Ultima lectura con su antiguedad y secuencia. Si no se renovo en LecturaCongeladaMs se devuelve como error no
        // conectado ("lectura congelada"): un peso viejo nunca sale como bueno.
        public LecturaPeso GetUltimaLectura()
        {
            LecturaPeso lectura;
            long timestamp;
            lock (_lecturaSync)
            {
                lectura = _ultimaLectura.Clone();
                timestamp = _ultimaLecturaTimestamp;
            }

            long edadMs = (Stopwatch.GetTimestamp() - timestamp) * 1000 / Stopwatch.Frequency;
            lectura.EdadMs = edadMs;
            if (edadMs <= UmbralCongeladaMs() || !_running)
            {
                return lectura;
            }

            RegistrarCongelada(edadMs);
            var congelada = LecturaPeso.CrearError(
                "Lectura congelada: la balanza no actualiza el peso hace " + (edadMs / 1000) + " s.",
                null,
                lectura.Raw,
                false);
            congelada.Marca = lectura.Marca;
            congelada.Modelo = lectura.Modelo;
            congelada.Puerto = lectura.Puerto;
            congelada.Secuencia = lectura.Secuencia;
            congelada.EdadMs = edadMs;
            return congelada;
        }

        // Umbral de lectura congelada: LecturaCongeladaMs, o mas si el intervalo configurado es largo (una vuelta del hilo lector
        // dura ~ min(intervalo, 250) + intervalo ms; con 2 vueltas perdidas recien se considera trabado).
        private int UmbralCongeladaMs()
        {
            var config = _config;
            int intervalo = config != null && config.Balanza != null ? config.Balanza.IntervaloLecturaMs : 150;
            int vuelta = Math.Min(intervalo, 250) + Math.Max(100, intervalo);
            return Math.Max(LecturaCongeladaMs, vuelta * 2 + 1000);
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

            PausarLector();
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
            finally
            {
                ReanudarLector();
            }
        }

        // Prueba cada puerto con cada marca para encontrar la balanza (ver BalanzaScanner). Pausa el lector mientras dura.
        public System.Collections.Generic.List<ResultadoPuerto> Buscar(BalanzaConfig plantilla, Action<string> avance)
        {
            PausarLector();
            try
            {
                return BalanzaScanner.Buscar(_registry, plantilla, avance);
            }
            finally
            {
                ReanudarLector();
            }
        }

        // Hace que el lector suelte el puerto COM (para que otro lo pueda abrir y probar). Cada llamada se cancela con
        // ReanudarLector. Mientras dura la pausa la ultima lectura queda como "Buscando la balanza..." (no es una lectura vieja).
        public void PausarLector()
        {
            Interlocked.Increment(ref _pausas);
            lock (_sync)
            {
                ClosePort();
            }

            // El lector puede estar a mitad de una vuelta: se le da tiempo a salir antes de que el otro abra el puerto.
            Thread.Sleep(LectorPausaEsperaMs);
            AgentLog.Info("Lector pausado para probar puertos.");
        }

        public void ReanudarLector()
        {
            if (Interlocked.Decrement(ref _pausas) < 0)
            {
                Interlocked.Exchange(ref _pausas, 0);
            }

            AgentLog.Info("Lector reanudado.");
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

                if (Volatile.Read(ref _pausas) > 0)
                {
                    // Pausa por una prueba de puertos: no se lee, y la lectura no queda vieja (el watchdog no debe saltar).
                    GuardarLectura(LecturaPeso.CrearError("Buscando la balanza…", config.Balanza));
                    Thread.Sleep(150);
                    continue;
                }

                var lectura = LeerUnaVez(config);
                GuardarLectura(lectura);
                LoguearCambioDeEstado(lectura);

                int espera = config.Balanza != null ? config.Balanza.IntervaloLecturaMs : 150;
                Thread.Sleep(Math.Max(100, espera));
            }
        }

        // Guarda la lectura nueva con su hora monotona (no depende del reloj de Windows) y su numero de secuencia.
        private void GuardarLectura(LecturaPeso lectura)
        {
            lock (_lecturaSync)
            {
                lectura.Secuencia = ++_secuencia;
                _ultimaLectura = lectura;
                _ultimaLecturaTimestamp = Stopwatch.GetTimestamp();
                if (_congeladaLogueada)
                {
                    _congeladaLogueada = false;
                    AgentLog.Info("La lectura volvio a actualizarse (secuencia " + lectura.Secuencia + ").");
                }
            }
        }

        // Loguea solo los cambios (error nuevo o recuperacion), no cada lectura: el hilo lee ~3 veces por segundo.
        private void LoguearCambioDeEstado(LecturaPeso lectura)
        {
            string error = lectura.Ok ? null : (lectura.Error ?? "Error de lectura.");
            if (string.Equals(error, _ultimoErrorLogueado, StringComparison.Ordinal))
            {
                return;
            }

            if (error == null)
            {
                AgentLog.Info("Lectura OK en " + (lectura.Puerto ?? string.Empty) + " (antes: " + _ultimoErrorLogueado + ").");
            }
            else
            {
                AgentLog.Warn("Lectura con error en " + (lectura.Puerto ?? string.Empty) + ": " + error + " | raw=" + Recortar(lectura.Raw));
            }

            _ultimoErrorLogueado = error;
        }

        // Lectura congelada: se loguea una vez por episodio y, si dura RecuperacionPuertoMs, se intenta destrabar el hilo lector.
        private void RegistrarCongelada(long edadMs)
        {
            lock (_lecturaSync)
            {
                if (!_congeladaLogueada)
                {
                    _congeladaLogueada = true;
                    AgentLog.Warn("Lectura congelada: el hilo lector no renueva la lectura hace " + edadMs + " ms.");
                }
            }

            if (edadMs >= RecuperacionPuertoMs)
            {
                IntentarDestrabarPuerto(edadMs);
            }
        }

        // El hilo lector quedo bloqueado dentro de una llamada al puerto serie. Cerrar el puerto desde otro hilo hace que esa
        // llamada falle con excepcion: LeerUnaVez la captura, cierra y reabre el puerto en la vuelta siguiente. Se hace en un hilo
        // aparte porque el cierre tambien puede colgarse, y nunca hay dos intentos a la vez.
        private void IntentarDestrabarPuerto(long edadMs)
        {
            if (Interlocked.CompareExchange(ref _recuperacionEnCurso, 1, 0) != 0)
            {
                return;
            }

            SerialPort trabado = _port;
            AgentLog.Warn("Intentando destrabar el puerto " + (trabado != null ? trabado.PortName : "(sin puerto)") + " tras " + edadMs + " ms sin lectura.");
            var hilo = new Thread(() =>
            {
                try
                {
                    if (trabado != null)
                    {
                        trabado.Dispose();
                    }
                    AgentLog.Info("Puerto cerrado para destrabar el lector.");
                }
                catch (Exception ex)
                {
                    AgentLog.Error("No se pudo cerrar el puerto trabado.", ex);
                }
                finally
                {
                    // Siguiente intento recien despues de otra ventana completa, para no martillar el driver.
                    Thread.Sleep(RecuperacionPuertoMs);
                    Interlocked.Exchange(ref _recuperacionEnCurso, 0);
                }
            })
            {
                IsBackground = true,
                Name = "Carnisys.Balanza.Recuperacion"
            };
            hilo.Start();
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
                var port = EnsurePort(balanzaConfig, driver);
                SafeDiscard(port);
                driver.SolicitarPeso(port);
                Thread.Sleep(Math.Max(60, Math.Min(balanzaConfig.IntervaloLecturaMs, 250)));
                string raw = port.ReadExisting();
                var lectura = driver.ParsearRespuesta(raw, balanzaConfig);
                lectura.Conectada = port.IsOpen && (lectura.Conectada || string.IsNullOrWhiteSpace(lectura.Error));
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

        // Devuelve el puerto abierto para la config actual; si cambio la config (o se cerro) lo reabre. El Open() va FUERA del
        // lock: si el driver se cuelga abriendo, no bloquea a UpdateConfig ni a nadie mas.
        private SerialPort EnsurePort(BalanzaConfig config, IBalanzaDriver driver)
        {
            string nextSignature = BuildSignature(config, driver);
            lock (_sync)
            {
                if (Volatile.Read(ref _pausas) > 0)
                {
                    throw new InvalidOperationException("Lector en pausa.");
                }

                if (_port != null && _port.IsOpen && string.Equals(_portSignature, nextSignature, StringComparison.OrdinalIgnoreCase))
                {
                    return _port;
                }

                ClosePort();
            }

            AgentLog.Info("Abriendo puerto: " + DescribirConfig(config));
            var nuevo = CreatePort(config, driver);
            try
            {
                nuevo.Open();
            }
            catch
            {
                nuevo.Dispose();
                throw;
            }

            lock (_sync)
            {
                _port = nuevo;
                _portSignature = nextSignature;
            }
            return nuevo;
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

        private static string DescribirConfig(BalanzaConfig config)
        {
            if (config == null) return "(sin config)";
            return (config.Marca ?? string.Empty) + " " + (config.Puerto ?? "(sin puerto)") + " " + config.BaudRate + " baud";
        }

        private static string Recortar(string texto)
        {
            if (string.IsNullOrEmpty(texto)) return string.Empty;
            string limpio = texto.Replace("\r", "\\r").Replace("\n", "\\n").Replace("\0", "\\0");
            return limpio.Length > 80 ? limpio.Substring(0, 80) + "..." : limpio;
        }

        internal static SerialPort CreatePort(BalanzaConfig config, IBalanzaDriver driver)
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
