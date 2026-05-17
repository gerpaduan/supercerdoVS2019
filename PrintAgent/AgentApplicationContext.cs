using System;
using System.Drawing;
using System.Windows.Forms;

namespace CarniSys.PrintAgent
{
    internal sealed class AgentApplicationContext : ApplicationContext
    {
        private readonly NotifyIcon _notifyIcon;
        private readonly LocalPrintServer _server;
        private AgentConfig _config;

        public AgentApplicationContext()
        {
            StartupRegistration.EnsureRegistered();
            _config = ConfigStore.Load();

            var menu = new ContextMenuStrip();
            menu.Items.Add("Configurar impresora", null, OnConfigure);
            menu.Items.Add("Estado", null, OnStatus);
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add("Salir", null, OnExit);

            _notifyIcon = new NotifyIcon
            {
                Icon = SystemIcons.Application,
                Visible = true,
                Text = "CarniSys Print Agent",
                ContextMenuStrip = menu
            };
            _notifyIcon.DoubleClick += OnConfigure;

            _server = new LocalPrintServer(ShowBalloon, () => _config, SaveConfig);
            _server.Start();

            ShowBalloon("Agente de impresión iniciado.");
        }

        private void OnConfigure(object sender, EventArgs e)
        {
            using (var form = new ConfigForm(LocalPrintServer.GetInstalledPrinters(), _config))
            {
                if (form.ShowDialog() == DialogResult.OK && form.ResultConfig != null)
                {
                    SaveConfig(form.ResultConfig);
                    ShowBalloon("Configuración guardada.");
                }
            }
        }

        private void OnStatus(object sender, EventArgs e)
        {
            string printer = !string.IsNullOrWhiteSpace(_config.PrinterName) ? _config.PrinterName : "Sin configurar";
            MessageBox.Show(
                "Agente activo en http://127.0.0.1:18777/\nImpresora: " + printer + "\nTicket: " + _config.TicketMm + " mm",
                "CarniSys Print Agent",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        private void OnExit(object sender, EventArgs e)
        {
            ExitThread();
        }

        protected override void ExitThreadCore()
        {
            _server.Dispose();
            _notifyIcon.Visible = false;
            _notifyIcon.Dispose();
            base.ExitThreadCore();
        }

        private void SaveConfig(AgentConfig config)
        {
            _config = config ?? new AgentConfig { TicketMm = 58 };
            ConfigStore.Save(_config);
        }

        private void ShowBalloon(string message)
        {
            _notifyIcon.BalloonTipTitle = "CarniSys Print Agent";
            _notifyIcon.BalloonTipText = message;
            _notifyIcon.ShowBalloonTip(2000);
        }
    }
}
