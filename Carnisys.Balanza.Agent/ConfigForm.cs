using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace Carnisys.Balanza.Agent
{
    // Lo que la ventana necesita del agente (se pasa como delegados para poder abrirla en pruebas sin el agente completo).
    internal sealed class ConfigHost
    {
        public BalanzaDriverRegistry Registro;
        public Func<LecturaPeso> GetLectura;
        public Func<List<PuertoInfo>> ListarPuertos;
        public Func<BalanzaConfig, LecturaPeso> Probar;
        public Func<BalanzaConfig, Action<string>, List<ResultadoPuerto>> Buscar;
    }

    // Ventana de configuracion del agente (rediseno 2026-11-20): peso en vivo arriba, marca y puerto (con el nombre que le pone
    // Windows) en el medio, "Buscar balanza automaticamente", y lo tecnico (modelo, baudios, intervalo, puerto de la API) plegado en
    // "Opciones avanzadas". Los colores son los de la marca (BrandUi).
    internal sealed class ConfigForm : Form
    {
        // Tope del intervalo de lectura: mas alto y la lectura sana pasa los 1500 ms de vigencia de la web (ver DECISIONS 2026-11-20).
        private const int IntervaloMaximoMs = 800;
        private const int RefrescoPanelMs = 300;

        private readonly ConfigHost _host;
        private readonly AgentConfig _inicial;
        private readonly Dictionary<string, string> _estadosPuertos = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, bool> _respondePuerto = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
        private readonly System.Windows.Forms.Timer _timer = new System.Windows.Forms.Timer { Interval = RefrescoPanelMs };

        private Panel _panelPeso;
        private Label _lblPeso;
        private Label _lblUnidad;
        private Label _lblEstado;
        private Label _lblEdad;
        private FlowLayoutPanel _marcas;
        private ListBox _lstPuertos;
        private ComboBox _cmbModelo;
        private TextBox _txtBaud;
        private TextBox _txtIntervalo;
        private NumericUpDown _numApi;
        private Panel _avanzado;
        private LinkLabel _lnkAvanzado;
        private Label _lblResultado;
        private ProgressBar _progreso;
        private Button _btnBuscar;
        private Button _btnProbar;
        private Button _btnGuardar;
        private bool _ocupado;

        public AgentConfig ResultConfig { get; private set; }

        public ConfigForm(AgentConfig config, ConfigHost host)
        {
            _host = host;
            _inicial = ConfigStore.Normalize(config);

            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            Text = "Balanza · CarniSys";
            Icon = BrandUi.IconoVentana();
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            BackColor = BrandUi.Fondo;
            Font = BrandUi.Fuente(9.5f);
            AutoSize = true;
            AutoSizeMode = AutoSizeMode.GrowAndShrink;

            ConstruirUi();
            CargarConfig();

            _timer.Tick += (s, e) => RefrescarPanel();
            _timer.Start();
            RefrescarPanel();
            Shown += (s, e) => CargarPuertosAsync(_inicial.Balanza.Puerto);
            FormClosed += (s, e) => _timer.Dispose();
        }

        // ---- Construccion de la interfaz ----

        private void ConstruirUi()
        {
            var raiz = new TableLayoutPanel { ColumnCount = 1, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, Margin = Padding.Empty, Padding = Padding.Empty };
            raiz.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

            raiz.Controls.Add(CrearEncabezado());

            var cuerpo = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoSize = true,
                Padding = new Padding(22, 16, 22, 4),
                Margin = Padding.Empty,
                MinimumSize = new Size(560, 0)
            };

            cuerpo.Controls.Add(CrearPanelPeso());
            cuerpo.Controls.Add(Texto("Poné algo sobre la balanza: el número tiene que cambiar.", BrandUi.TextoSuave, 0, 6, 0, 14));

            cuerpo.Controls.Add(Etiqueta("Marca"));
            _marcas = new FlowLayoutPanel { AutoSize = true, Margin = new Padding(0, 0, 0, 12), WrapContents = false };
            cuerpo.Controls.Add(_marcas);

            cuerpo.Controls.Add(Etiqueta("Puerto donde está conectada"));
            _lstPuertos = new ListBox
            {
                DrawMode = DrawMode.OwnerDrawFixed,
                ItemHeight = LogicalToDeviceUnits(30),
                IntegralHeight = false,
                Height = LogicalToDeviceUnits(30) * 4 + 4,
                Width = LogicalToDeviceUnits(516),
                BorderStyle = BorderStyle.FixedSingle,
                Margin = new Padding(0, 0, 0, 8),
                Font = BrandUi.Fuente(9.5f)
            };
            _lstPuertos.DrawItem += DibujarPuerto;
            _lstPuertos.SelectedIndexChanged += (s, e) => _lstPuertos.Invalidate();
            cuerpo.Controls.Add(_lstPuertos);

            var acciones = new FlowLayoutPanel { AutoSize = true, Margin = new Padding(0, 0, 0, 0), WrapContents = false };
            _btnBuscar = BrandUi.Boton("Buscar balanza automáticamente", true);
            _btnBuscar.Click += (s, e) => BuscarAsync();
            _btnProbar = BrandUi.Boton("Probar selección", false);
            _btnProbar.Click += (s, e) => ProbarAsync();
            acciones.Controls.Add(_btnBuscar);
            acciones.Controls.Add(_btnProbar);
            cuerpo.Controls.Add(acciones);

            _progreso = new ProgressBar { Style = ProgressBarStyle.Marquee, MarqueeAnimationSpeed = 30, Height = 4, Width = LogicalToDeviceUnits(516), Visible = false, Margin = new Padding(0, 6, 0, 0) };
            cuerpo.Controls.Add(_progreso);

            _lblResultado = Texto(string.Empty, BrandUi.TextoSuave, 0, 6, 0, 6);
            _lblResultado.MaximumSize = new Size(LogicalToDeviceUnits(516), 0);
            // Sin texto no ocupa lugar (evita un hueco entre los botones y "Opciones avanzadas").
            _lblResultado.Visible = false;
            _lblResultado.TextChanged += (s, e) => _lblResultado.Visible = !string.IsNullOrEmpty(_lblResultado.Text);
            cuerpo.Controls.Add(_lblResultado);

            _lnkAvanzado = new LinkLabel
            {
                Text = "▸ Opciones avanzadas (modelo, velocidad, intervalo, puerto local)",
                AutoSize = true,
                LinkColor = BrandUi.Navy,
                ActiveLinkColor = BrandUi.Naranja,
                LinkBehavior = LinkBehavior.HoverUnderline,
                Margin = new Padding(0, 4, 0, 4)
            };
            _lnkAvanzado.LinkClicked += (s, e) => AlternarAvanzado();
            cuerpo.Controls.Add(_lnkAvanzado);

            _avanzado = CrearAvanzado();
            _avanzado.Visible = false;
            cuerpo.Controls.Add(_avanzado);

            raiz.Controls.Add(cuerpo);
            raiz.Controls.Add(CrearPie());
            Controls.Add(raiz);
        }

        private Control CrearEncabezado()
        {
            var encabezado = new Panel { BackColor = BrandUi.Navy, Height = 64, MinimumSize = new Size(560, 64), Margin = Padding.Empty };
            encabezado.Controls.Add(new PictureBox { Image = BrandUi.Dibujar(34, null), SizeMode = PictureBoxSizeMode.AutoSize, Left = 22, Top = 15, BackColor = Color.Transparent });
            encabezado.Controls.Add(new Label { Text = "Balanza", ForeColor = Color.White, Font = BrandUi.Fuente(13f, FontStyle.Bold), AutoSize = true, Left = 68, Top = 9, BackColor = Color.Transparent });
            encabezado.Controls.Add(new Label { Text = "CarniSys · configuración del agente", ForeColor = BrandUi.NaranjaClaro, AutoSize = true, Left = 69, Top = 36, BackColor = Color.Transparent });
            return encabezado;
        }

        private Control CrearPanelPeso()
        {
            _panelPeso = new Panel { Width = LogicalToDeviceUnits(516), Height = LogicalToDeviceUnits(92), BackColor = BrandUi.GrisFondo, Margin = new Padding(0) };
            var titulo = new Label { Text = "Peso en vivo", AutoSize = true, Left = 16, Top = 8, BackColor = Color.Transparent, ForeColor = BrandUi.GrisTexto };
            _lblPeso = new Label { Text = "—", AutoSize = true, Left = 14, Top = 26, BackColor = Color.Transparent, Font = new Font("Consolas", 28f, FontStyle.Bold), ForeColor = BrandUi.GrisTexto };
            _lblUnidad = new Label { Text = "kg", AutoSize = true, BackColor = Color.Transparent, Font = BrandUi.Fuente(13f, FontStyle.Bold), ForeColor = BrandUi.GrisTexto };
            _lblEstado = new Label { Text = "Sin balanza configurada", AutoSize = true, BackColor = Color.Transparent, Font = BrandUi.Fuente(9.5f, FontStyle.Bold), ForeColor = BrandUi.GrisTexto, TextAlign = ContentAlignment.MiddleRight };
            _lblEdad = new Label { Text = string.Empty, AutoSize = true, BackColor = Color.Transparent, ForeColor = BrandUi.GrisTexto, TextAlign = ContentAlignment.MiddleRight };
            _panelPeso.Controls.AddRange(new Control[] { titulo, _lblPeso, _lblUnidad, _lblEstado, _lblEdad });
            _panelPeso.Resize += (s, e) => PosicionarPanelPeso();
            PosicionarPanelPeso();
            return _panelPeso;
        }

        // El peso va a la izquierda y el estado a la derecha; se reacomoda cuando cambia el texto.
        private void PosicionarPanelPeso()
        {
            _lblUnidad.Left = _lblPeso.Right - 2;
            _lblUnidad.Top = _lblPeso.Top + _lblPeso.Height - _lblUnidad.Height - 8;
            _lblEstado.Left = _panelPeso.Width - _lblEstado.Width - 16;
            _lblEstado.Top = 12;
            _lblEdad.Left = _panelPeso.Width - _lblEdad.Width - 16;
            _lblEdad.Top = 38;
        }

        private Panel CrearAvanzado()
        {
            var panel = new TableLayoutPanel { ColumnCount = 2, AutoSize = true, Margin = new Padding(0, 2, 0, 6), Padding = new Padding(10, 6, 10, 6), BackColor = BrandUi.GrisFondo };
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

            _cmbModelo = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = LogicalToDeviceUnits(170) };
            _txtBaud = new TextBox { Width = LogicalToDeviceUnits(90) };
            _txtIntervalo = new TextBox { Width = LogicalToDeviceUnits(90) };
            _numApi = new NumericUpDown { Width = LogicalToDeviceUnits(90), Minimum = 1, Maximum = 65535, Value = 5100 };

            AgregarFila(panel, "Modelo", _cmbModelo, null);
            AgregarFila(panel, "Velocidad (baudios)", _txtBaud, "Casi siempre 9600.");
            AgregarFila(panel, "Intervalo de lectura (ms)", _txtIntervalo, "100 a " + IntervaloMaximoMs + ". Dejá 150 salvo indicación de soporte.");
            AgregarFila(panel, "Puerto local de la API", _numApi, "El sistema web lo busca en 127.0.0.1:5100. No lo cambies.");
            return panel;
        }

        private void AgregarFila(TableLayoutPanel panel, string titulo, Control control, string ayuda)
        {
            int fila = panel.RowCount;
            panel.RowCount = fila + 1;
            panel.Controls.Add(new Label { Text = titulo, AutoSize = true, Anchor = AnchorStyles.Left, Margin = new Padding(0, 6, 12, 6) }, 0, fila);

            var celda = new FlowLayoutPanel { FlowDirection = FlowDirection.TopDown, AutoSize = true, WrapContents = false, Margin = new Padding(0, 2, 0, 2) };
            celda.Controls.Add(control);
            if (!string.IsNullOrEmpty(ayuda))
            {
                celda.Controls.Add(new Label { Text = ayuda, AutoSize = true, MaximumSize = new Size(LogicalToDeviceUnits(250), 0), ForeColor = BrandUi.TextoSuave, Font = BrandUi.Fuente(8.5f), Margin = new Padding(0, 2, 0, 0) });
            }
            panel.Controls.Add(celda, 1, fila);
        }

        private Control CrearPie()
        {
            var pie = new TableLayoutPanel { ColumnCount = 2, AutoSize = true, Margin = Padding.Empty, Padding = new Padding(18, 4, 18, 8), MinimumSize = new Size(560, 0) };
            pie.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            pie.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

            var izquierda = BrandUi.Boton("Ver registro", false);
            izquierda.Click += (s, e) => AbrirRegistro();
            izquierda.Anchor = AnchorStyles.Left;
            pie.Controls.Add(izquierda, 0, 0);

            var derecha = new FlowLayoutPanel { AutoSize = true, WrapContents = false, Margin = Padding.Empty };
            var cancelar = BrandUi.Boton("Cancelar", false);
            cancelar.DialogResult = DialogResult.Cancel;
            _btnGuardar = BrandUi.Boton("Guardar", true, BrandUi.Navy);
            _btnGuardar.Click += (s, e) => Guardar();
            derecha.Controls.Add(cancelar);
            derecha.Controls.Add(_btnGuardar);
            pie.Controls.Add(derecha, 1, 0);

            var version = new Label
            {
                Text = "Versión " + AgentPaths.Version() + " · http://127.0.0.1:" + _inicial.Api.Port + "/",
                AutoSize = true,
                ForeColor = BrandUi.TextoSuave,
                Font = BrandUi.Fuente(8.5f),
                Margin = new Padding(6, 0, 0, 0)
            };
            pie.Controls.Add(version, 0, 1);
            pie.SetColumnSpan(version, 2);

            AcceptButton = _btnGuardar;
            CancelButton = cancelar;
            return pie;
        }

        private static Label Etiqueta(string texto)
        {
            return Texto(texto, BrandUi.TextoSuave, 0, 0, 0, 4);
        }

        private static Label Texto(string texto, Color color, int izq, int arr, int der, int abj)
        {
            return new Label { Text = texto, AutoSize = true, ForeColor = color, Margin = new Padding(izq, arr, der, abj) };
        }

        // ---- Datos ----

        private void CargarConfig()
        {
            // Marcas como botones segmentados (una por driver distinto).
            foreach (string marca in _host.Registro.GetAll().Select(d => d.Marca).Distinct())
            {
                var radio = new RadioButton
                {
                    Text = marca,
                    Appearance = Appearance.Button,
                    FlatStyle = FlatStyle.Flat,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Width = LogicalToDeviceUnits(120),
                    Height = LogicalToDeviceUnits(34),
                    Margin = new Padding(0, 0, 6, 0),
                    Cursor = Cursors.Hand,
                    UseVisualStyleBackColor = false
                };
                radio.FlatAppearance.BorderColor = BrandUi.Borde;
                radio.FlatAppearance.CheckedBackColor = BrandUi.Navy;
                radio.CheckedChanged += (s, e) => { EstilizarMarca((RadioButton)s); if (((RadioButton)s).Checked) SincronizarModelos(); };
                _marcas.Controls.Add(radio);
                EstilizarMarca(radio);
            }

            RadioButton elegida = _marcas.Controls.OfType<RadioButton>()
                .FirstOrDefault(r => string.Equals(r.Text, _inicial.Balanza.Marca, StringComparison.OrdinalIgnoreCase))
                ?? _marcas.Controls.OfType<RadioButton>().FirstOrDefault();
            if (elegida != null) elegida.Checked = true;

            SincronizarModelos();
            if (_cmbModelo.Items.Contains(_inicial.Balanza.Modelo)) _cmbModelo.SelectedItem = _inicial.Balanza.Modelo;

            _txtBaud.Text = _inicial.Balanza.BaudRate.ToString(CultureInfo.InvariantCulture);
            _txtIntervalo.Text = _inicial.Balanza.IntervaloLecturaMs.ToString(CultureInfo.InvariantCulture);
            _numApi.Value = _inicial.Api.Port;
        }

        private static void EstilizarMarca(RadioButton radio)
        {
            radio.BackColor = radio.Checked ? BrandUi.Navy : Color.White;
            radio.ForeColor = radio.Checked ? Color.White : BrandUi.Texto;
            radio.Font = BrandUi.Fuente(9.5f, radio.Checked ? FontStyle.Bold : FontStyle.Regular);
        }

        private string MarcaElegida()
        {
            RadioButton r = _marcas.Controls.OfType<RadioButton>().FirstOrDefault(x => x.Checked);
            return r != null ? r.Text : "Systel";
        }

        private void SincronizarModelos()
        {
            string marca = MarcaElegida();
            _cmbModelo.Items.Clear();
            foreach (IBalanzaDriver d in _host.Registro.GetAll().Where(d => string.Equals(d.Marca, marca, StringComparison.OrdinalIgnoreCase)))
            {
                _cmbModelo.Items.Add(d.Modelo);
            }
            if (_cmbModelo.Items.Count > 0) _cmbModelo.SelectedIndex = 0;
        }

        // El listado de puertos consulta WMI (~1 s): se carga en segundo plano para que la ventana abra al instante.
        private void CargarPuertosAsync(string seleccionado)
        {
            _lblResultado.Text = "Leyendo los puertos de la PC…";
            var hilo = new Thread(() =>
            {
                List<PuertoInfo> puertos;
                try { puertos = _host.ListarPuertos(); }
                catch (Exception ex) { AgentLog.Error("No se pudieron listar los puertos.", ex); puertos = new List<PuertoInfo>(); }
                Ui(() =>
                {
                    _lstPuertos.Items.Clear();
                    foreach (PuertoInfo p in puertos) _lstPuertos.Items.Add(p);
                    PuertoInfo sel = puertos.FirstOrDefault(p => string.Equals(p.Puerto, seleccionado, StringComparison.OrdinalIgnoreCase));
                    if (sel != null) _lstPuertos.SelectedItem = sel;
                    else if (puertos.Count > 0) _lstPuertos.SelectedIndex = 0;
                    _lblResultado.Text = puertos.Count == 0
                        ? "No hay puertos COM en esta PC. Conectá la balanza (o su cable USB) y tocá «Buscar balanza automáticamente»."
                        : string.Empty;
                });
            }) { IsBackground = true, Name = "Carnisys.Balanza.Puertos" };
            hilo.Start();
        }

        private void DibujarPuerto(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0) return;
            var puerto = (PuertoInfo)_lstPuertos.Items[e.Index];
            bool elegido = (e.State & DrawItemState.Selected) == DrawItemState.Selected;

            using (var fondo = new SolidBrush(elegido ? Color.FromArgb(255, 241, 232) : Color.White))
            {
                e.Graphics.FillRectangle(fondo, e.Bounds);
            }
            if (elegido)
            {
                using (var borde = new Pen(BrandUi.Naranja, 2))
                {
                    e.Graphics.DrawRectangle(borde, e.Bounds.X + 1, e.Bounds.Y + 1, e.Bounds.Width - 3, e.Bounds.Height - 3);
                }
            }

            string estado;
            bool responde = _respondePuerto.TryGetValue(puerto.Puerto, out bool r) && r;
            _estadosPuertos.TryGetValue(puerto.Puerto, out estado);

            var rectTitulo = new Rectangle(e.Bounds.X + 28, e.Bounds.Y, e.Bounds.Width - 190, e.Bounds.Height);
            var formato = new StringFormat { LineAlignment = StringAlignment.Center, Trimming = StringTrimming.EllipsisCharacter, FormatFlags = StringFormatFlags.NoWrap };
            using (var pincel = new SolidBrush(BrandUi.Texto))
            {
                e.Graphics.DrawString(puerto.Titulo, e.Font, pincel, rectTitulo, formato);
            }

            // Punto de seleccion
            using (var contorno = new Pen(elegido ? BrandUi.Naranja : BrandUi.Borde, 1.5f))
            {
                int cy = e.Bounds.Y + e.Bounds.Height / 2;
                e.Graphics.DrawEllipse(contorno, e.Bounds.X + 8, cy - 6, 12, 12);
                if (elegido)
                {
                    using (var lleno = new SolidBrush(BrandUi.Naranja))
                    {
                        e.Graphics.FillEllipse(lleno, e.Bounds.X + 11, cy - 3, 6, 6);
                    }
                }
            }

            if (!string.IsNullOrEmpty(estado))
            {
                var rectEstado = new Rectangle(e.Bounds.Right - 170, e.Bounds.Y, 162, e.Bounds.Height);
                var derecha = new StringFormat { LineAlignment = StringAlignment.Center, Alignment = StringAlignment.Far, Trimming = StringTrimming.EllipsisCharacter, FormatFlags = StringFormatFlags.NoWrap };
                using (var pincel = new SolidBrush(responde ? BrandUi.VerdeTexto : BrandUi.TextoSuave))
                using (var fuente = BrandUi.Fuente(8.5f, responde ? FontStyle.Bold : FontStyle.Regular))
                {
                    e.Graphics.DrawString(estado, fuente, pincel, rectEstado, derecha);
                }
            }

            using (var linea = new Pen(Color.FromArgb(232, 236, 241)))
            {
                e.Graphics.DrawLine(linea, e.Bounds.X, e.Bounds.Bottom - 1, e.Bounds.Right, e.Bounds.Bottom - 1);
            }
        }

        // ---- Panel de peso en vivo ----

        private void RefrescarPanel()
        {
            LecturaPeso lectura = null;
            try { lectura = _host.GetLectura(); }
            catch (Exception ex) { AgentLog.Warn("No se pudo leer el estado para la ventana: " + ex.Message); }

            bool configurada = !string.IsNullOrWhiteSpace(_inicial.Balanza.Puerto);
            EstadoBalanza estado = BrandUi.Clasificar(lectura, configurada);

            Color fondo = BrandUi.FondoEstado(estado);
            Color texto = BrandUi.TextoEstado(estado);
            _panelPeso.BackColor = fondo;
            foreach (Control c in _panelPeso.Controls) c.ForeColor = texto;

            switch (estado)
            {
                case EstadoBalanza.Conectada:
                case EstadoBalanza.Atencion when lectura != null && lectura.Ok:
                    _lblPeso.Text = lectura.Peso.ToString("0.000", CultureInfo.GetCultureInfo("es-AR"));
                    _lblEstado.Text = lectura.Inestable ? "● Conectada · peso inestable" : "● Conectada · estable";
                    _lblEdad.Text = "Lectura de hace " + (lectura.EdadMs / 1000.0).ToString("0.0", CultureInfo.GetCultureInfo("es-AR")) + " s";
                    break;
                case EstadoBalanza.Atencion:
                    _lblPeso.Text = "…";
                    _lblEstado.Text = "● Buscando la balanza";
                    _lblEdad.Text = string.Empty;
                    break;
                case EstadoBalanza.Error:
                    _lblPeso.Text = "—";
                    _lblEstado.Text = "● Sin lectura";
                    _lblEdad.Text = lectura != null && !string.IsNullOrEmpty(lectura.Error) ? Recortar(lectura.Error, 46) : "Sin respuesta del agente";
                    break;
                default:
                    _lblPeso.Text = "—";
                    _lblEstado.Text = "● Sin balanza configurada";
                    _lblEdad.Text = "Elegí el puerto y guardá";
                    break;
            }

            PosicionarPanelPeso();
        }

        // El agente manda el peso con punto ("2.345"); en pantalla va con coma, como en el resto de CarniSys.
        private static string FormatearPeso(string pesoDisplay)
        {
            return (pesoDisplay ?? string.Empty).Replace('.', ',');
        }

        private static string Recortar(string texto, int max)
        {
            return texto.Length <= max ? texto : texto.Substring(0, max - 1) + "…";
        }

        // ---- Acciones ----

        private void AlternarAvanzado()
        {
            _avanzado.Visible = !_avanzado.Visible;
            _lnkAvanzado.Text = (_avanzado.Visible ? "▾ " : "▸ ") + "Opciones avanzadas (modelo, velocidad, intervalo, puerto local)";
        }

        private void Ocupar(bool ocupado, string texto)
        {
            _ocupado = ocupado;
            _progreso.Visible = ocupado;
            _btnBuscar.Enabled = !ocupado;
            _btnProbar.Enabled = !ocupado;
            _btnGuardar.Enabled = !ocupado;
            _lstPuertos.Enabled = !ocupado;
            _lblResultado.ForeColor = BrandUi.TextoSuave;
            if (texto != null) _lblResultado.Text = texto;
        }

        private void ProbarAsync()
        {
            if (_ocupado) return;
            var puerto = _lstPuertos.SelectedItem as PuertoInfo;
            if (puerto == null)
            {
                MostrarResultado("Elegí primero un puerto de la lista.", false);
                return;
            }

            BalanzaConfig config = ConstruirConfig().Balanza;
            Ocupar(true, "Probando " + puerto.Puerto + " con " + config.Marca + "…");
            new Thread(() =>
            {
                LecturaPeso lectura;
                try { lectura = _host.Probar(config); }
                catch (Exception ex) { lectura = LecturaPeso.CrearError(ex.Message, config); }
                Ui(() =>
                {
                    Ocupar(false, null);
                    _respondePuerto[puerto.Puerto] = lectura.Ok;
                    _estadosPuertos[puerto.Puerto] = lectura.Ok ? "Responde" : "Sin respuesta";
                    _lstPuertos.Invalidate();
                    MostrarResultado(lectura.Ok
                            ? "Lectura correcta: " + FormatearPeso(lectura.PesoDisplay) + " kg en " + puerto.Puerto + ". Guardá para usarla."
                            : "No se pudo leer la balanza en " + puerto.Puerto + ": " + (lectura.Error ?? "sin detalle") + ".",
                        lectura.Ok);
                });
            }) { IsBackground = true, Name = "Carnisys.Balanza.Probar" }.Start();
        }

        private void BuscarAsync()
        {
            if (_ocupado) return;
            BalanzaConfig plantilla = ConstruirConfig().Balanza;
            Ocupar(true, "Buscando la balanza en los puertos de la PC…");
            new Thread(() =>
            {
                List<ResultadoPuerto> resultados;
                try { resultados = _host.Buscar(plantilla, texto => Ui(() => _lblResultado.Text = texto)); }
                catch (Exception ex) { AgentLog.Error("Fallo la busqueda de la balanza.", ex); resultados = new List<ResultadoPuerto>(); }

                List<PuertoInfo> puertos;
                try { puertos = _host.ListarPuertos(); } catch (Exception) { puertos = new List<PuertoInfo>(); }

                Ui(() =>
                {
                    Ocupar(false, null);
                    _lstPuertos.Items.Clear();
                    foreach (PuertoInfo p in puertos) _lstPuertos.Items.Add(p);
                    foreach (ResultadoPuerto r in resultados)
                    {
                        _respondePuerto[r.Puerto] = r.Responde;
                        _estadosPuertos[r.Puerto] = r.Responde ? "Responde (" + r.Marca + ")" : (r.Detalle ?? "Sin respuesta");
                    }

                    ResultadoPuerto hallado = resultados.FirstOrDefault(r => r != null && r.Responde);
                    if (hallado != null)
                    {
                        PuertoInfo sel = puertos.FirstOrDefault(p => string.Equals(p.Puerto, hallado.Puerto, StringComparison.OrdinalIgnoreCase));
                        if (sel != null) _lstPuertos.SelectedItem = sel;
                        RadioButton marca = _marcas.Controls.OfType<RadioButton>().FirstOrDefault(m => string.Equals(m.Text, hallado.Marca, StringComparison.OrdinalIgnoreCase));
                        if (marca != null) marca.Checked = true;
                        if (_cmbModelo.Items.Contains(hallado.Modelo)) _cmbModelo.SelectedItem = hallado.Modelo;
                        MostrarResultado("Balanza encontrada en " + hallado.Puerto + " (" + hallado.Marca + "): lee " + FormatearPeso(hallado.PesoDisplay) + " kg. Guardá para usarla.", true);
                    }
                    else
                    {
                        _lstPuertos.Invalidate();
                        MostrarResultado(puertos.Count == 0
                                ? "No hay puertos COM en esta PC. Conectá la balanza (o su cable USB) y volvé a buscar."
                                : "No se encontró la balanza. Revisá que esté prendida y con el cable conectado, y volvé a buscar.",
                            false);
                    }
                });
            }) { IsBackground = true, Name = "Carnisys.Balanza.Buscar" }.Start();
        }

        private void MostrarResultado(string texto, bool ok)
        {
            _lblResultado.Text = texto;
            _lblResultado.ForeColor = ok ? BrandUi.VerdeTexto : BrandUi.RojoTexto;
        }

        private void Guardar()
        {
            if (_ocupado) return;
            if (!(_lstPuertos.SelectedItem is PuertoInfo))
            {
                MostrarResultado("Elegí el puerto donde está conectada la balanza.", false);
                return;
            }

            ResultConfig = ConstruirConfig();
            DialogResult = DialogResult.OK;
            Close();
        }

        private static void AbrirRegistro()
        {
            try
            {
                Directory.CreateDirectory(AgentPaths.CarpetaLogs);
                Process.Start(new ProcessStartInfo(AgentPaths.CarpetaLogs) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                AgentLog.Warn("No se pudo abrir la carpeta de registros: " + ex.Message);
            }
        }

        private AgentConfig ConstruirConfig()
        {
            string marca = MarcaElegida();
            var puerto = _lstPuertos.SelectedItem as PuertoInfo;
            return ConfigStore.Normalize(new AgentConfig
            {
                Balanza = new BalanzaConfig
                {
                    Marca = marca,
                    Modelo = _cmbModelo.SelectedItem as string,
                    Puerto = puerto != null ? puerto.Puerto : null,
                    BaudRate = ParseInt(_txtBaud.Text, 9600),
                    DataBits = 8,
                    Parity = "None",
                    StopBits = string.Equals(marca, "Kretz", StringComparison.OrdinalIgnoreCase) ? "Two" : "One",
                    IntervaloLecturaMs = Math.Min(ParseInt(_txtIntervalo.Text, 150), IntervaloMaximoMs)
                },
                Api = new ApiConfig { Host = "127.0.0.1", Port = (int)_numApi.Value },
                // La ventana no edita los origenes de actualizacion: se conservan los de config.json.
                Actualizacion = _inicial.Actualizacion
            });
        }

        private static int ParseInt(string raw, int fallback)
        {
            return int.TryParse(raw, out int valor) && valor > 0 ? valor : fallback;
        }

        // Ejecuta en el hilo de la ventana, salvo que ya se haya cerrado (los hilos de prueba pueden terminar despues).
        private void Ui(Action accion)
        {
            if (IsDisposed || !IsHandleCreated) return;
            try { BeginInvoke(accion); }
            catch (InvalidOperationException) { /* la ventana se cerro justo ahora */ }
        }
    }
}
