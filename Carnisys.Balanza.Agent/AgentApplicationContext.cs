using System;
using System.Globalization;
using System.Threading;
using System.Windows.Forms;

namespace Carnisys.Balanza.Agent
{
    // Contexto de la app de bandeja: arranca el lector y el servidor HTTP, muestra el icono de balanza (cambia de color segun el
    // estado y su globo muestra el peso) y abre la ventana de configuracion. Tambien atiende a otras instancias: si el usuario vuelve
    // a abrir el .exe se muestra la configuracion; si el instalador pide "salir" el agente se cierra solo (retira su icono).
    internal sealed class AgentApplicationContext : ApplicationContext
    {
        private const int RefrescoIconoMs = 1000;

        private readonly NotifyIcon _notifyIcon;
        private readonly BalanzaReaderService _reader;
        private readonly bool _abrirConfiguracionAlInicio;
        private readonly System.Windows.Forms.Timer _timerIcono = new System.Windows.Forms.Timer { Interval = RefrescoIconoMs };
        private readonly Control _hiloUi = new Control();
        private readonly EventWaitHandle _eventoConfigurar;
        private readonly EventWaitHandle _eventoSalir;
        private AgentConfig _config;
        private LocalBalanzaServer _server;
        private EstadoBalanza? _estadoMostrado;
        private bool _configAbierta;
        private bool _servidorActivo;
        private bool _restosPendientes;
        private int _servidorFallosLogueados;

        public AgentApplicationContext(bool abrirConfiguracionAlInicio)
        {
            _abrirConfiguracionAlInicio = abrirConfiguracionAlInicio;
            StartupRegistration.EnsureRegistered();
            _restosPendientes = !Actualizador.LimpiarRestos();
            _config = ConfigStore.Load();
            _reader = new BalanzaReaderService(_config);
            _reader.Start();

            // Handle creado ya, para poder hacer BeginInvoke desde los hilos que esperan los eventos.
            var handle = _hiloUi.Handle;

            var menu = new ContextMenuStrip();
            menu.Items.Add("Configurar balanza", null, OnConfigure);
            menu.Items.Add("Estado", null, OnStatus);
            menu.Items.Add("Ver registro", null, (s, e) => AbrirRegistro());
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add("Salir", null, OnExit);

            _notifyIcon = new NotifyIcon
            {
                Icon = BrandUi.IconoBandeja(null),
                Visible = true,
                Text = "CarniSys Balanza",
                ContextMenuStrip = menu
            };
            _notifyIcon.DoubleClick += OnConfigure;

            _eventoConfigurar = new EventWaitHandle(false, EventResetMode.AutoReset, AgentPaths.NombreEventoConfigurar());
            _eventoSalir = new EventWaitHandle(false, EventResetMode.AutoReset, AgentPaths.NombreEventoSalir());
            new Thread(EsperarSenales) { IsBackground = true, Name = "Carnisys.Balanza.Senales" }.Start();

            IntentarIniciarServidor();

            _timerIcono.Tick += (s, e) =>
            {
                ActualizarIcono();
                // Si el puerto estaba ocupado al arrancar (ej. el agente anterior todavia lo liberaba al actualizar) se reintenta.
                if (!_servidorActivo) IntentarIniciarServidor();
                if (_restosPendientes) _restosPendientes = !Actualizador.LimpiarRestos();
            };
            _timerIcono.Start();
            ActualizarIcono();

            if (_abrirConfiguracionAlInicio || string.IsNullOrWhiteSpace(_config.Balanza.Puerto))
            {
                // Despues de que arranque el bucle de mensajes: abrir un dialogo modal dentro del constructor bloquearia el inicio.
                _hiloUi.BeginInvoke(new Action(ShowConfigDialog));
            }
        }

        // Espera las senales de otras instancias: abrir configuracion (se volvio a ejecutar el .exe) o salir (lo pide el instalador).
        private void EsperarSenales()
        {
            WaitHandle[] esperas = { _eventoConfigurar, _eventoSalir };
            while (true)
            {
                int cual;
                try { cual = WaitHandle.WaitAny(esperas); }
                catch (ObjectDisposedException) { return; }

                try
                {
                    if (cual == 0)
                    {
                        _hiloUi.BeginInvoke(new Action(ShowConfigDialog));
                    }
                    else
                    {
                        AgentLog.Info("Cierre pedido por otra instancia (instalador).");
                        _hiloUi.BeginInvoke(new Action(ExitThread));
                        return;
                    }
                }
                catch (InvalidOperationException)
                {
                    return; // la app ya se estaba cerrando
                }
            }
        }

        // Color del icono segun la ultima lectura y peso en el globo (maximo 63 caracteres que admite Windows).
        private void ActualizarIcono()
        {
            LecturaPeso lectura = _reader.GetUltimaLectura();
            bool configurada = !string.IsNullOrWhiteSpace(_config.Balanza.Puerto);
            EstadoBalanza estado = BrandUi.Clasificar(lectura, configurada);

            if (_estadoMostrado != estado)
            {
                _notifyIcon.Icon = BrandUi.IconoBandeja(estado);
                if (_estadoMostrado == EstadoBalanza.Conectada && estado == EstadoBalanza.Error)
                {
                    ShowBalloon("Se perdió la lectura de la balanza. Revisá el cable y que esté prendida.");
                }
                _estadoMostrado = estado;
            }

            string texto;
            switch (estado)
            {
                case EstadoBalanza.Conectada:
                case EstadoBalanza.Atencion when lectura.Ok:
                    texto = "CarniSys Balanza · " + lectura.Peso.ToString("0.000", CultureInfo.GetCultureInfo("es-AR")) + " kg" + (lectura.Inestable ? " (inestable)" : string.Empty);
                    break;
                case EstadoBalanza.SinConfigurar:
                    texto = "CarniSys Balanza · sin configurar";
                    break;
                case EstadoBalanza.Atencion:
                    texto = "CarniSys Balanza · buscando…";
                    break;
                default:
                    texto = "CarniSys Balanza · sin lectura (" + _config.Balanza.Puerto + ")";
                    break;
            }

            _notifyIcon.Text = texto.Length > 63 ? texto.Substring(0, 63) : texto;
        }

        // Arranca el servidor HTTP. Un puerto ocupado no debe tumbar el agente (ni la bandeja ni el lector): se loguea y se reintenta.
        private void IntentarIniciarServidor()
        {
            try
            {
                StartServer();
                _servidorActivo = true;
                _servidorFallosLogueados = 0;
            }
            catch (Exception ex)
            {
                _servidorActivo = false;
                if (_servidorFallosLogueados++ == 0)
                {
                    AgentLog.Error("No se pudo abrir el servidor local en el puerto " + _config.Api.Port + " (se reintenta cada segundo).", ex);
                }
            }
        }

        private void StartServer()
        {
            if (_server != null)
            {
                _server.Dispose();
                _server = null;
            }

            _server = new LocalBalanzaServer(
                _config.Api.Port,
                () => _config,
                SaveConfig,
                () => _reader.GetUltimaLectura(),
                () => _reader.GetPuertos(),
                cfg => _reader.Probar(cfg),
                ShowBalloon);
            _server.Start();
        }

        private void RestartServerIfNeeded(AgentConfig previous, AgentConfig next)
        {
            bool changed = previous == null
                || previous.Api == null
                || next == null
                || next.Api == null
                || previous.Api.Port != next.Api.Port;

            if (!changed)
            {
                return;
            }

            _servidorActivo = false;
            IntentarIniciarServidor();
        }

        private void OnConfigure(object sender, EventArgs e)
        {
            ShowConfigDialog();
        }

        private void ShowConfigDialog()
        {
            // Una sola ventana de configuracion a la vez (doble clic repetido o .exe ejecutado de nuevo con la ventana abierta).
            if (_configAbierta)
            {
                return;
            }

            _configAbierta = true;
            try
            {
                var host = new ConfigHost
                {
                    Registro = _reader.GetRegistry(),
                    GetLectura = () => _reader.GetUltimaLectura(),
                    ListarPuertos = BalanzaScanner.ListarPuertos,
                    Probar = cfg => _reader.Probar(cfg),
                    Buscar = (plantilla, avance) => _reader.Buscar(plantilla, avance)
                };

                using (var form = new ConfigForm(_config, host))
                {
                    if (form.ShowDialog() == DialogResult.OK && form.ResultConfig != null)
                    {
                        SaveConfig(form.ResultConfig);
                        ShowBalloon("Configuración de balanza guardada.");
                    }
                }
            }
            finally
            {
                _configAbierta = false;
            }
        }

        private void OnStatus(object sender, EventArgs e)
        {
            var lectura = _reader.GetUltimaLectura();
            MessageBox.Show(
                "Agente activo en http://127.0.0.1:" + _config.Api.Port + "/\n" +
                "Versión: " + AgentPaths.Version() + "\n" +
                "Marca: " + (_config.Balanza.Marca ?? "Sin configurar") + "\n" +
                "Puerto: " + (!string.IsNullOrWhiteSpace(_config.Balanza.Puerto) ? _config.Balanza.Puerto : "Sin configurar") + "\n" +
                "Última lectura: " + (lectura != null ? lectura.PesoDisplay : "Sin lectura"),
                "CarniSys Balanza",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        private static void AbrirRegistro()
        {
            try
            {
                System.IO.Directory.CreateDirectory(AgentPaths.CarpetaLogs);
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(AgentPaths.CarpetaLogs) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                AgentLog.Warn("No se pudo abrir la carpeta de registros: " + ex.Message);
            }
        }

        private void SaveConfig(AgentConfig config)
        {
            var previous = _config;
            _config = ConfigStore.Normalize(config);
            ConfigStore.Save(_config);
            _reader.UpdateConfig(_config);
            RestartServerIfNeeded(previous, _config);
        }

        private void OnExit(object sender, EventArgs e)
        {
            ExitThread();
        }

        protected override void ExitThreadCore()
        {
            _timerIcono.Stop();
            _timerIcono.Dispose();

            if (_server != null)
            {
                _server.Dispose();
            }

            _reader.Dispose();
            _notifyIcon.Visible = false;
            _notifyIcon.Dispose();
            _eventoConfigurar.Dispose();
            _eventoSalir.Dispose();
            _hiloUi.Dispose();
            base.ExitThreadCore();
        }

        private void ShowBalloon(string message)
        {
            _notifyIcon.BalloonTipTitle = "CarniSys Balanza";
            _notifyIcon.BalloonTipText = message;
            _notifyIcon.ShowBalloonTip(2000);
        }
    }
}
