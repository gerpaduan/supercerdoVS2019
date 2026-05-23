using System;
using System.Drawing;
using System.Windows.Forms;

namespace Carnisys.Balanza.Agent
{
    internal sealed class AgentApplicationContext : ApplicationContext
    {
        private readonly NotifyIcon _notifyIcon;
        private readonly BalanzaReaderService _reader;
        private readonly bool _abrirConfiguracionAlInicio;
        private LocalBalanzaServer _server;
        private AgentConfig _config;

        public AgentApplicationContext(bool abrirConfiguracionAlInicio)
        {
            _abrirConfiguracionAlInicio = abrirConfiguracionAlInicio;
            StartupRegistration.EnsureRegistered();
            _config = ConfigStore.Load();
            _reader = new BalanzaReaderService(_config);
            _reader.Start();

            var menu = new ContextMenuStrip();
            menu.Items.Add("Configurar balanza", null, OnConfigure);
            menu.Items.Add("Estado", null, OnStatus);
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add("Salir", null, OnExit);

            _notifyIcon = new NotifyIcon
            {
                Icon = SystemIcons.Application,
                Visible = true,
                Text = "Carnisys Balanza Agent",
                ContextMenuStrip = menu
            };
            _notifyIcon.DoubleClick += OnConfigure;

            StartServer();
            ShowBalloon("Agente de balanza iniciado en http://127.0.0.1:" + _config.Api.Port + "/");

            if (_abrirConfiguracionAlInicio || string.IsNullOrWhiteSpace(_config.Balanza.Puerto))
            {
                ShowConfigDialog();
            }
        }

        private void StartServer()
        {
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

            _server.Dispose();
            StartServer();
        }

        private void OnConfigure(object sender, EventArgs e)
        {
            ShowConfigDialog();
        }

        private void ShowConfigDialog()
        {
            using (var form = new ConfigForm(_config, _reader.GetRegistry(), () => _reader.GetPuertos(), cfg => _reader.Probar(cfg)))
            {
                if (form.ShowDialog() == DialogResult.OK && form.ResultConfig != null)
                {
                    SaveConfig(form.ResultConfig);
                    ShowBalloon("Configuración de balanza guardada.");
                }
            }
        }

        private void OnStatus(object sender, EventArgs e)
        {
            var lectura = _reader.GetUltimaLectura();
            MessageBox.Show(
                "Agente activo en http://127.0.0.1:" + _config.Api.Port + "/\n" +
                "Marca: " + (_config.Balanza.Marca ?? "Sin configurar") + "\n" +
                "Puerto: " + (!string.IsNullOrWhiteSpace(_config.Balanza.Puerto) ? _config.Balanza.Puerto : "Sin configurar") + "\n" +
                "Última lectura: " + (lectura != null ? lectura.PesoDisplay : "Sin lectura"),
                "Carnisys Balanza Agent",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
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
            if (_server != null)
            {
                _server.Dispose();
            }

            _reader.Dispose();
            _notifyIcon.Visible = false;
            _notifyIcon.Dispose();
            base.ExitThreadCore();
        }

        private void ShowBalloon(string message)
        {
            _notifyIcon.BalloonTipTitle = "Carnisys Balanza Agent";
            _notifyIcon.BalloonTipText = message;
            _notifyIcon.ShowBalloonTip(2000);
        }
    }
}
