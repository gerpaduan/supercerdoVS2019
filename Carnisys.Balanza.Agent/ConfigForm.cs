using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Carnisys.Balanza.Agent
{
    internal sealed class ConfigForm : Form
    {
        private readonly Func<string[]> _getPuertos;
        private readonly Func<BalanzaConfig, LecturaPeso> _probarLectura;
        private readonly BalanzaDriverRegistry _registry;

        private ComboBox _cmbMarca;
        private ComboBox _cmbModelo;
        private ComboBox _cmbPuerto;
        private TextBox _txtBaudRate;
        private TextBox _txtIntervalo;
        private NumericUpDown _numPortApi;
        private Label _lblResultado;

        public AgentConfig ResultConfig { get; private set; }

        public ConfigForm(AgentConfig config, BalanzaDriverRegistry registry, Func<string[]> getPuertos, Func<BalanzaConfig, LecturaPeso> probarLectura)
        {
            _registry = registry;
            _getPuertos = getPuertos;
            _probarLectura = probarLectura;

            Text = "Configurar balanza local";
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ClientSize = new Size(520, 360);

            BuildUi();
            LoadConfig(ConfigStore.Normalize(config));
        }

        private void BuildUi()
        {
            var y = 18;
            Controls.Add(CreateLabel("Marca", 18, y));
            _cmbMarca = CreateCombo(160, y - 3, 220);
            _cmbMarca.SelectedIndexChanged += (s, e) => SyncModelo();
            Controls.Add(_cmbMarca);
            y += 38;

            Controls.Add(CreateLabel("Modelo", 18, y));
            _cmbModelo = CreateCombo(160, y - 3, 220);
            Controls.Add(_cmbModelo);
            y += 38;

            Controls.Add(CreateLabel("Puerto COM", 18, y));
            _cmbPuerto = CreateCombo(160, y - 3, 150);
            Controls.Add(_cmbPuerto);
            var btnActualizar = new Button
            {
                Text = "Actualizar puertos",
                Left = 322,
                Top = y - 4,
                Width = 120,
                Height = 28
            };
            btnActualizar.Click += (s, e) => LoadPuertos(null);
            Controls.Add(btnActualizar);
            y += 38;

            Controls.Add(CreateLabel("Baud rate", 18, y));
            _txtBaudRate = CreateTextBox(160, y - 3, 90);
            Controls.Add(_txtBaudRate);
            y += 38;

            Controls.Add(CreateLabel("Intervalo lectura (ms)", 18, y));
            _txtIntervalo = CreateTextBox(160, y - 3, 90);
            Controls.Add(_txtIntervalo);
            y += 38;

            Controls.Add(CreateLabel("API local", 18, y));
            var txtHost = CreateTextBox(160, y - 3, 120);
            txtHost.Text = "127.0.0.1";
            txtHost.ReadOnly = true;
            Controls.Add(txtHost);
            _numPortApi = new NumericUpDown
            {
                Left = 292,
                Top = y - 3,
                Width = 90,
                Minimum = 1,
                Maximum = 65535,
                Value = 5100
            };
            Controls.Add(_numPortApi);
            y += 46;

            var btnProbar = new Button
            {
                Text = "Probar lectura",
                Left = 160,
                Top = y,
                Width = 120,
                Height = 32
            };
            btnProbar.Click += OnProbarClick;
            Controls.Add(btnProbar);

            var btnGuardar = new Button
            {
                Text = "Guardar",
                Left = 292,
                Top = y,
                Width = 90,
                Height = 32
            };
            btnGuardar.Click += OnGuardarClick;
            Controls.Add(btnGuardar);

            var btnCancelar = new Button
            {
                Text = "Cancelar",
                Left = 392,
                Top = y,
                Width = 90,
                Height = 32,
                DialogResult = DialogResult.Cancel
            };
            Controls.Add(btnCancelar);
            y += 50;

            _lblResultado = new Label
            {
                Left = 18,
                Top = y,
                Width = 470,
                Height = 90,
                BorderStyle = BorderStyle.FixedSingle,
                Padding = new Padding(8),
                Text = "Seleccione una balanza y pruebe la lectura."
            };
            Controls.Add(_lblResultado);

            AcceptButton = btnGuardar;
            CancelButton = btnCancelar;
        }

        private void LoadConfig(AgentConfig config)
        {
            foreach (var driver in _registry.GetAll())
            {
                _cmbMarca.Items.Add(driver.Marca);
            }

            _cmbMarca.SelectedItem = config.Balanza.Marca;
            if (_cmbMarca.SelectedIndex < 0 && _cmbMarca.Items.Count > 0)
            {
                _cmbMarca.SelectedIndex = 0;
            }

            SyncModelo();
            _cmbModelo.SelectedItem = config.Balanza.Modelo;
            if (_cmbModelo.SelectedIndex < 0 && _cmbModelo.Items.Count > 0)
            {
                _cmbModelo.SelectedIndex = 0;
            }

            LoadPuertos(config.Balanza.Puerto);
            _txtBaudRate.Text = config.Balanza.BaudRate.ToString();
            _txtIntervalo.Text = config.Balanza.IntervaloLecturaMs.ToString();
            _numPortApi.Value = config.Api.Port;
        }

        private void SyncModelo()
        {
            string marca = _cmbMarca.SelectedItem as string ?? "Systel";
            _cmbModelo.Items.Clear();

            foreach (var driver in _registry.GetAll().Where(d => string.Equals(d.Marca, marca, StringComparison.OrdinalIgnoreCase)))
            {
                _cmbModelo.Items.Add(driver.Modelo);
            }

            if (_cmbModelo.Items.Count > 0)
            {
                _cmbModelo.SelectedIndex = 0;
            }
        }

        private void LoadPuertos(string selectedPort)
        {
            var puertos = _getPuertos();
            _cmbPuerto.Items.Clear();

            foreach (var puerto in puertos)
            {
                _cmbPuerto.Items.Add(puerto);
            }

            if (!string.IsNullOrWhiteSpace(selectedPort) && _cmbPuerto.Items.Contains(selectedPort))
            {
                _cmbPuerto.SelectedItem = selectedPort;
            }
            else if (_cmbPuerto.Items.Count > 0)
            {
                _cmbPuerto.SelectedIndex = 0;
            }
        }

        private void OnProbarClick(object sender, EventArgs e)
        {
            var config = BuildConfig();
            var lectura = _probarLectura(config.Balanza);
            _lblResultado.Text = lectura.Ok
                ? "Lectura correcta\r\nPeso: " + lectura.PesoDisplay + "\r\nPuerto: " + lectura.Puerto
                : "No se pudo leer la balanza.\r\n" + (lectura.Error ?? "Sin detalle.");
        }

        private void OnGuardarClick(object sender, EventArgs e)
        {
            ResultConfig = BuildConfig();
            DialogResult = DialogResult.OK;
            Close();
        }

        private AgentConfig BuildConfig()
        {
            return ConfigStore.Normalize(new AgentConfig
            {
                Balanza = new BalanzaConfig
                {
                    Marca = _cmbMarca.SelectedItem as string,
                    Modelo = _cmbModelo.SelectedItem as string,
                    Puerto = _cmbPuerto.SelectedItem as string,
                    BaudRate = ParseInt(_txtBaudRate.Text, 9600),
                    DataBits = 8,
                    Parity = "None",
                    StopBits = string.Equals(_cmbMarca.SelectedItem as string, "Kretz", StringComparison.OrdinalIgnoreCase) ? "Two" : "One",
                    IntervaloLecturaMs = ParseInt(_txtIntervalo.Text, 150)
                },
                Api = new ApiConfig
                {
                    Host = "127.0.0.1",
                    Port = (int)_numPortApi.Value
                }
            });
        }

        private static int ParseInt(string raw, int fallback)
        {
            return int.TryParse(raw, out int value) && value > 0 ? value : fallback;
        }

        private static Label CreateLabel(string text, int left, int top)
        {
            return new Label
            {
                Text = text,
                Left = left,
                Top = top + 4,
                Width = 130
            };
        }

        private static ComboBox CreateCombo(int left, int top, int width)
        {
            return new ComboBox
            {
                Left = left,
                Top = top,
                Width = width,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
        }

        private static TextBox CreateTextBox(int left, int top, int width)
        {
            return new TextBox
            {
                Left = left,
                Top = top,
                Width = width
            };
        }
    }
}
