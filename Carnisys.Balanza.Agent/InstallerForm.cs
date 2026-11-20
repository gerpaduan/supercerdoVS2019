using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace Carnisys.Balanza.Agent
{
    // Instalador de un solo archivo (2026-11-20): cuando Carnisys.Balanza.Agent.exe corre desde cualquier carpeta que no sea la del
    // agente (ej. Descargas), muestra esta ventana y se instala: cierra la version anterior, se copia a la carpeta del usuario
    // (sin permisos de administrador), se registra para iniciar con Windows, busca la balanza si no hay configuracion y arranca la
    // copia instalada. Reemplaza al zip + Instalar-Local.cmd. Actualizar = ejecutar el .exe nuevo (se conserva config.json).
    internal sealed class InstallerForm : Form
    {
        private enum Paso { Pendiente, EnCurso, Hecho, Aviso, Error }

        private const int CantidadPasos = 4;
        // Tiempo que se espera a que la version anterior cierre sola antes de forzarla.
        private const int EsperaCierreMs = 3000;
        // Reintentos al copiar: justo despues de cerrar el agente viejo Windows puede tardar en soltar el .exe.
        private const int ReintentosCopia = 10;
        private const int EsperaReintentoMs = 500;

        private readonly Label[] _iconos = new Label[CantidadPasos];
        private readonly Label[] _textos = new Label[CantidadPasos];
        private readonly string[] _titulos =
        {
            "Cerrar la versión anterior",
            "Copiar el agente a tu usuario",
            "Iniciar solo al prender la PC",
            "Buscar la balanza"
        };

        private ProgressBar _progreso;
        private Label _resumen;
        private Panel _bannerFinal;
        private Button _btnCerrar;
        private Button _btnRegistro;
        private bool _terminado;
        private bool _ok;
        // Actualizacion automatica pedida por el agente: al terminar bien la ventana se cierra sola (sin esperar el clic en «Listo»).
        private readonly bool _automatica;
        private const int CierreAutomaticoMs = 2500;

        public InstallerForm(bool automatica = false)
        {
            _automatica = automatica;
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            Text = "Instalar agente de balanza · CarniSys";
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
            FormClosing += (s, e) =>
            {
                // Cerrar a mitad de la instalacion dejaria el agente a medio copiar.
                if (!_terminado)
                {
                    e.Cancel = true;
                }
            };
            Shown += (s, e) =>
            {
                var hilo = new Thread(Instalar) { IsBackground = true, Name = "Carnisys.Balanza.Instalador" };
                hilo.Start();
            };
        }

        private void ConstruirUi()
        {
            var raiz = new TableLayoutPanel
            {
                ColumnCount = 1,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Margin = Padding.Empty,
                Padding = Padding.Empty,
                Dock = DockStyle.None
            };
            raiz.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

            raiz.Controls.Add(CrearEncabezado(), 0, 0);

            var cuerpo = new TableLayoutPanel
            {
                ColumnCount = 2,
                AutoSize = true,
                Padding = new Padding(24, 18, 24, 6),
                Margin = Padding.Empty,
                MinimumSize = new Size(520, 0)
            };
            cuerpo.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            cuerpo.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            var intro = new Label
            {
                Text = "Se instala en esta computadora, solo para tu usuario. No pide permisos de administrador.",
                AutoSize = true,
                MaximumSize = new Size(470, 0),
                ForeColor = BrandUi.TextoSuave,
                Margin = new Padding(0, 0, 0, 12)
            };
            cuerpo.Controls.Add(intro, 0, 0);
            cuerpo.SetColumnSpan(intro, 2);

            for (int i = 0; i < CantidadPasos; i++)
            {
                _iconos[i] = new Label
                {
                    AutoSize = false,
                    Size = new Size(26, 26),
                    TextAlign = ContentAlignment.MiddleCenter,
                    Font = new Font("Segoe UI Symbol", 12f, FontStyle.Bold),
                    Margin = new Padding(0, 2, 8, 2)
                };
                _textos[i] = new Label
                {
                    AutoSize = true,
                    MaximumSize = new Size(440, 0),
                    TextAlign = ContentAlignment.MiddleLeft,
                    Margin = new Padding(0, 6, 0, 2)
                };
                cuerpo.Controls.Add(_iconos[i], 0, i + 1);
                cuerpo.Controls.Add(_textos[i], 1, i + 1);
                PintarPaso(i, Paso.Pendiente, _titulos[i]);
            }

            _progreso = new ProgressBar
            {
                Style = ProgressBarStyle.Marquee,
                MarqueeAnimationSpeed = 30,
                Height = 6,
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 14, 0, 4)
            };
            cuerpo.Controls.Add(_progreso, 0, CantidadPasos + 1);
            cuerpo.SetColumnSpan(_progreso, 2);

            _bannerFinal = new Panel
            {
                Visible = false,
                AutoSize = true,
                Padding = new Padding(12, 10, 12, 10),
                Margin = new Padding(0, 10, 0, 4),
                Dock = DockStyle.Fill
            };
            _resumen = new Label { AutoSize = true, MaximumSize = new Size(440, 0), Font = BrandUi.Fuente(9.5f, FontStyle.Bold) };
            _bannerFinal.Controls.Add(_resumen);
            cuerpo.Controls.Add(_bannerFinal, 0, CantidadPasos + 2);
            cuerpo.SetColumnSpan(_bannerFinal, 2);

            raiz.Controls.Add(cuerpo, 0, 1);

            var pie = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.RightToLeft,
                AutoSize = true,
                Padding = new Padding(20, 6, 20, 14),
                Margin = Padding.Empty,
                Dock = DockStyle.Fill
            };
            _btnCerrar = BrandUi.Boton("Listo", true, BrandUi.Navy);
            _btnCerrar.Enabled = false;
            _btnCerrar.Click += (s, e) => Close();
            _btnRegistro = BrandUi.Boton("Ver registro", false);
            _btnRegistro.Visible = false;
            _btnRegistro.Click += (s, e) => AbrirRegistro();
            pie.Controls.Add(_btnCerrar);
            pie.Controls.Add(_btnRegistro);
            raiz.Controls.Add(pie, 0, 2);

            Controls.Add(raiz);
        }

        private Control CrearEncabezado()
        {
            var encabezado = new Panel { BackColor = BrandUi.Navy, Height = 68, Dock = DockStyle.Fill, MinimumSize = new Size(520, 68) };
            var logo = new PictureBox { Image = BrandUi.Dibujar(36, null), SizeMode = PictureBoxSizeMode.AutoSize, Left = 22, Top = 16, BackColor = Color.Transparent };
            var titulo = new Label
            {
                Text = "Instalar agente de balanza",
                ForeColor = Color.White,
                Font = BrandUi.Fuente(13f, FontStyle.Bold),
                AutoSize = true,
                Left = 70,
                Top = 10,
                BackColor = Color.Transparent
            };
            var sub = new Label
            {
                Text = "CarniSys · lee la balanza de esta PC para el sistema web",
                ForeColor = BrandUi.NaranjaClaro,
                AutoSize = true,
                Left = 71,
                Top = 38,
                BackColor = Color.Transparent
            };
            encabezado.Controls.Add(logo);
            encabezado.Controls.Add(titulo);
            encabezado.Controls.Add(sub);
            return encabezado;
        }

        // ---- Instalacion (hilo aparte) ----

        private void Instalar()
        {
            string destino = AgentPaths.RutaExeInstalado;
            bool abrirConfiguracion = false;
            try
            {
                AgentLog.Info("Instalador: inicio. Origen=" + AgentPaths.RutaExeActual() + " Destino=" + destino);

                Mostrar(0, Paso.EnCurso, "Cerrando la versión anterior…");
                int cerrados = CerrarVersionAnterior(destino);
                Mostrar(0, Paso.Hecho, cerrados > 0 ? "Se cerró la versión anterior" : "No había una versión anterior abierta");

                Mostrar(1, Paso.EnCurso, "Copiando el agente…");
                CopiarAgente(destino);
                Mostrar(1, Paso.Hecho, "Agente copiado en tu usuario");

                Mostrar(2, Paso.EnCurso, "Configurando el inicio automático…");
                StartupRegistration.Register(destino);
                Mostrar(2, Paso.Hecho, AgentPaths.ModoPrueba ? "Inicio automático omitido (modo prueba)" : "Se inicia solo al prender la PC");

                Mostrar(3, Paso.EnCurso, "Buscando la balanza…");
                abrirConfiguracion = !ResolverBalanza();

                IniciarAgente(destino, abrirConfiguracion);
                AgentLog.Info("Instalador: terminado correctamente.");
                Terminar(true, abrirConfiguracion
                    ? "Instalado. Elegí el puerto de la balanza en la ventana que se abrió."
                    : (_automatica
                        ? "Agente actualizado. Ya podés seguir trabajando."
                        : "Listo. Ya podés usar la balanza en CarniSys. El agente queda junto al reloj de Windows."));
            }
            catch (Exception ex)
            {
                AgentLog.Error("Instalador: fallo.", ex);
                // Si habia un agente instalado y esta instalacion fallo, se lo vuelve a abrir: que una actualizacion fallida no deje
                // la PC sin agente.
                ReabrirAgenteAnterior(destino);
                Terminar(false, "No se pudo instalar: " + ex.Message);
            }
        }

        // Cierra el agente anterior: primero se le pide salir (asi retira su icono de la bandeja) y, si no contesta, se lo fuerza.
        // En modo prueba solo se toca el agente de la carpeta de prueba (nunca el real de la PC).
        private int CerrarVersionAnterior(string destino)
        {
            int propio = Process.GetCurrentProcess().Id;
            int cerrados = 0;

            try
            {
                using (var salir = EventWaitHandle.OpenExisting(AgentPaths.NombreEventoSalir()))
                {
                    salir.Set();
                }
            }
            catch (WaitHandleCannotBeOpenedException)
            {
                // No hay agente nuevo escuchando (o no hay ninguno): se sigue con el cierre por proceso.
            }

            foreach (Process proceso in Process.GetProcessesByName(AgentPaths.NombreProceso))
            {
                using (proceso)
                {
                    if (proceso.Id == propio)
                    {
                        continue;
                    }

                    string ruta = null;
                    try { ruta = proceso.MainModule.FileName; }
                    catch (Exception ex) { AgentLog.Warn("No se pudo leer la ruta del proceso " + proceso.Id + ": " + ex.Message); }

                    if (AgentPaths.ModoPrueba && !string.Equals(ruta, destino, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    try
                    {
                        if (!proceso.WaitForExit(EsperaCierreMs))
                        {
                            proceso.Kill();
                            proceso.WaitForExit(EsperaCierreMs);
                        }
                        cerrados++;
                    }
                    catch (Exception ex)
                    {
                        AgentLog.Warn("No se pudo cerrar el proceso " + proceso.Id + ": " + ex.Message);
                    }
                }
            }

            return cerrados;
        }

        private static void CopiarAgente(string destino)
        {
            string origen = AgentPaths.RutaExeActual();
            if (string.IsNullOrWhiteSpace(origen) || !File.Exists(origen))
            {
                throw new InvalidOperationException("No se encontró el archivo del instalador.");
            }

            Directory.CreateDirectory(AgentPaths.Carpeta);
            CopiarConReintentos(origen, destino);

            // Si el .exe vino con su .config al lado (version en zip) se copia tambien; el .exe suelto no lo necesita.
            string configOrigen = origen + ".config";
            if (File.Exists(configOrigen))
            {
                CopiarConReintentos(configOrigen, destino + ".config");
            }
        }

        private static void CopiarConReintentos(string origen, string destino)
        {
            for (int intento = 1; ; intento++)
            {
                try
                {
                    File.Copy(origen, destino, true);
                    return;
                }
                catch (IOException ex)
                {
                    if (intento >= ReintentosCopia)
                    {
                        throw new IOException("El archivo del agente sigue en uso (" + ex.Message + "). Cerralo desde el ícono junto al reloj y volvé a intentar.");
                    }
                    Thread.Sleep(EsperaReintentoMs);
                }
            }
        }

        // true si queda una balanza configurada (la que ya habia o la encontrada); false si hay que elegirla a mano.
        private bool ResolverBalanza()
        {
            if (ConfigStore.ExisteConfig())
            {
                AgentConfig actual = ConfigStore.Load();
                if (!string.IsNullOrWhiteSpace(actual.Balanza.Puerto))
                {
                    Mostrar(3, Paso.Hecho, "Se conservó tu configuración (" + actual.Balanza.Puerto + ", " + actual.Balanza.Marca + ")");
                    return true;
                }
            }

            var registro = new BalanzaDriverRegistry();
            var resultados = BalanzaScanner.Buscar(registro, null, texto => Mostrar(3, Paso.EnCurso, texto));
            ResultadoPuerto hallado = resultados.Find(r => r != null && r.Responde);
            if (hallado == null)
            {
                Mostrar(3, Paso.Aviso, resultados.Count == 0
                    ? "No se encontró ningún puerto COM. Conectá la balanza y elegila en la ventana de configuración"
                    : "No se encontró la balanza. La elegís vos en la ventana de configuración");
                return false;
            }

            AgentConfig config = ConfigStore.Load();
            config.Balanza.Marca = hallado.Marca;
            config.Balanza.Modelo = hallado.Modelo;
            config.Balanza.Puerto = hallado.Puerto;
            config.Balanza.StopBits = string.Equals(hallado.Marca, "Kretz", StringComparison.OrdinalIgnoreCase) ? "Two" : "One";
            ConfigStore.Save(config);
            Mostrar(3, Paso.Hecho, "Balanza encontrada en " + hallado.Puerto + " (" + hallado.Marca + ")");
            return true;
        }

        private static void ReabrirAgenteAnterior(string destino)
        {
            try
            {
                if (File.Exists(destino) && !Process.GetProcessesByName(AgentPaths.NombreProceso).Any(p => EsEseExe(p, destino)))
                {
                    Process.Start(new ProcessStartInfo(destino) { WorkingDirectory = AgentPaths.Carpeta, UseShellExecute = true });
                    AgentLog.Info("Instalador: se volvio a abrir el agente anterior.");
                }
            }
            catch (Exception ex)
            {
                AgentLog.Warn("No se pudo reabrir el agente anterior: " + ex.Message);
            }
        }

        private static bool EsEseExe(Process proceso, string ruta)
        {
            try { return string.Equals(proceso.MainModule.FileName, ruta, StringComparison.OrdinalIgnoreCase); }
            catch (Exception) { return false; }
        }

        private static void IniciarAgente(string destino, bool abrirConfiguracion)
        {
            var info = new ProcessStartInfo(destino)
            {
                Arguments = abrirConfiguracion ? "--configure" : string.Empty,
                WorkingDirectory = AgentPaths.Carpeta,
                // Sin heredar handles del instalador (que a su vez pudo heredar el socket del agente que lo lanzo).
                UseShellExecute = true
            };
            Process.Start(info);
        }

        // ---- Interfaz (siempre desde el hilo de la ventana) ----

        private void Mostrar(int indice, Paso paso, string texto)
        {
            if (!IsHandleCreated) return;
            BeginInvoke(new Action(() => PintarPaso(indice, paso, texto)));
        }

        private void PintarPaso(int indice, Paso paso, string texto)
        {
            Label icono = _iconos[indice];
            Label etiqueta = _textos[indice];
            etiqueta.Text = texto;

            switch (paso)
            {
                case Paso.Hecho:
                    icono.Text = "✓";
                    icono.ForeColor = BrandUi.VerdeTexto;
                    etiqueta.ForeColor = BrandUi.Texto;
                    break;
                case Paso.Aviso:
                    icono.Text = "!";
                    icono.ForeColor = BrandUi.AmbarTexto;
                    etiqueta.ForeColor = BrandUi.AmbarTexto;
                    break;
                case Paso.Error:
                    icono.Text = "✗";
                    icono.ForeColor = BrandUi.RojoTexto;
                    etiqueta.ForeColor = BrandUi.RojoTexto;
                    break;
                case Paso.EnCurso:
                    icono.Text = "›";
                    icono.ForeColor = BrandUi.Naranja;
                    etiqueta.ForeColor = BrandUi.Texto;
                    break;
                default:
                    icono.Text = "•";
                    icono.ForeColor = BrandUi.Borde;
                    etiqueta.ForeColor = BrandUi.TextoSuave;
                    break;
            }
        }

        private void Terminar(bool ok, string mensaje)
        {
            if (!IsHandleCreated) return;
            BeginInvoke(new Action(() =>
            {
                _ok = ok;
                _terminado = true;
                _progreso.Visible = false;
                _bannerFinal.Visible = true;
                _bannerFinal.BackColor = ok ? BrandUi.VerdeFondo : BrandUi.RojoFondo;
                _resumen.ForeColor = ok ? BrandUi.VerdeTexto : BrandUi.RojoTexto;
                _resumen.Text = mensaje;
                _btnCerrar.Text = ok ? "Listo" : "Cerrar";
                _btnCerrar.Enabled = true;
                _btnRegistro.Visible = !ok;
                AcceptButton = _btnCerrar;
                if (ok && _automatica)
                {
                    var cierre = new System.Windows.Forms.Timer { Interval = CierreAutomaticoMs };
                    cierre.Tick += (s, e) => { cierre.Stop(); cierre.Dispose(); Close(); };
                    cierre.Start();
                }
            }));
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

        public bool Instalado { get { return _ok; } }
    }
}
