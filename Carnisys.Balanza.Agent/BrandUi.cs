using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Carnisys.Balanza.Agent
{
    // Estado de la balanza para colorear el icono de la bandeja y el panel de peso.
    internal enum EstadoBalanza
    {
        SinConfigurar,   // gris: todavia no se eligio puerto
        Conectada,       // verde: lectura fresca y estable
        Atencion,        // ambar: peso inestable o buscando
        Error            // rojo: sin lectura / desconectada / congelada
    }

    // Colores y fuente de la marca CarniSys (los mismos del login web: --login-navy y --login-orange de site.css) y
    // dibujo del icono de balanza. Un solo lugar para que instalador, ventana de configuracion y bandeja se vean iguales.
    internal static class BrandUi
    {
        public static readonly Color Navy = ColorTranslator.FromHtml("#062d63");
        public static readonly Color NavyOscuro = ColorTranslator.FromHtml("#031d42");
        public static readonly Color Naranja = ColorTranslator.FromHtml("#ee5c0b");
        public static readonly Color NaranjaClaro = ColorTranslator.FromHtml("#ff7c2a");
        public static readonly Color Texto = ColorTranslator.FromHtml("#1b2433");
        public static readonly Color TextoSuave = ColorTranslator.FromHtml("#5b6676");
        public static readonly Color Borde = ColorTranslator.FromHtml("#cfd6e0");
        public static readonly Color Fondo = Color.White;

        public static readonly Color VerdeFondo = ColorTranslator.FromHtml("#e6f4ea");
        public static readonly Color VerdeTexto = ColorTranslator.FromHtml("#146c35");
        public static readonly Color AmbarFondo = ColorTranslator.FromHtml("#fff3dc");
        public static readonly Color AmbarTexto = ColorTranslator.FromHtml("#8a5200");
        public static readonly Color RojoFondo = ColorTranslator.FromHtml("#fdeceb");
        public static readonly Color RojoTexto = ColorTranslator.FromHtml("#a8261d");
        public static readonly Color GrisFondo = ColorTranslator.FromHtml("#eef1f5");
        public static readonly Color GrisTexto = ColorTranslator.FromHtml("#4b5563");

        // Estado visible de una lectura del agente (para el panel de peso y el icono de la bandeja).
        public static EstadoBalanza Clasificar(LecturaPeso lectura, bool configurada)
        {
            if (!configurada)
            {
                return EstadoBalanza.SinConfigurar;
            }

            if (lectura == null)
            {
                return EstadoBalanza.Error;
            }

            if (lectura.Ok && lectura.Conectada)
            {
                return lectura.Inestable ? EstadoBalanza.Atencion : EstadoBalanza.Conectada;
            }

            if (lectura.Error != null && lectura.Error.StartsWith("Buscando", StringComparison.OrdinalIgnoreCase))
            {
                return EstadoBalanza.Atencion;
            }

            return EstadoBalanza.Error;
        }

        public static Font Fuente(float puntos, FontStyle estilo = FontStyle.Regular)
        {
            return new Font("Segoe UI", puntos, estilo, GraphicsUnit.Point);
        }

        public static Color ColorPunto(EstadoBalanza estado)
        {
            switch (estado)
            {
                case EstadoBalanza.Conectada: return ColorTranslator.FromHtml("#2eb85c");
                case EstadoBalanza.Atencion: return ColorTranslator.FromHtml("#f5a623");
                case EstadoBalanza.Error: return ColorTranslator.FromHtml("#e0443a");
                default: return ColorTranslator.FromHtml("#9aa5b4");
            }
        }

        public static Color FondoEstado(EstadoBalanza estado)
        {
            switch (estado)
            {
                case EstadoBalanza.Conectada: return VerdeFondo;
                case EstadoBalanza.Atencion: return AmbarFondo;
                case EstadoBalanza.Error: return RojoFondo;
                default: return GrisFondo;
            }
        }

        public static Color TextoEstado(EstadoBalanza estado)
        {
            switch (estado)
            {
                case EstadoBalanza.Conectada: return VerdeTexto;
                case EstadoBalanza.Atencion: return AmbarTexto;
                case EstadoBalanza.Error: return RojoTexto;
                default: return GrisTexto;
            }
        }

        // Boton con la estetica de la marca: primario = relleno (naranja/navy), secundario = borde.
        public static Button Boton(string texto, bool primario, Color? relleno = null)
        {
            var boton = new Button
            {
                Text = texto,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                MinimumSize = new Size(96, 34),
                Padding = new Padding(10, 2, 10, 2),
                FlatStyle = FlatStyle.Flat,
                Font = Fuente(9.5f, primario ? FontStyle.Bold : FontStyle.Regular),
                Cursor = Cursors.Hand,
                UseVisualStyleBackColor = false,
                Margin = new Padding(4, 4, 0, 4)
            };

            boton.FlatAppearance.BorderSize = primario ? 0 : 1;
            boton.FlatAppearance.BorderColor = Borde;
            if (primario)
            {
                Color fondo = relleno ?? Naranja;
                boton.BackColor = fondo;
                boton.ForeColor = Color.White;
                boton.FlatAppearance.MouseOverBackColor = ControlPaint.Light(fondo, 0.15f);
                boton.FlatAppearance.MouseDownBackColor = ControlPaint.Dark(fondo, 0.1f);
            }
            else
            {
                boton.BackColor = Color.White;
                boton.ForeColor = Texto;
                boton.FlatAppearance.MouseOverBackColor = GrisFondo;
            }

            return boton;
        }

        // ---- Iconos ----

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern bool DestroyIcon(IntPtr handle);

        private static readonly Dictionary<string, Icon> Cache = new Dictionary<string, Icon>();

        // Icono de balanza para la bandeja con un punto de estado abajo a la derecha. Se cachea por (estado, tamano): el
        // icono se crea una vez y vive todo el proceso (sin fuga de handles de GDI).
        public static Icon IconoBandeja(EstadoBalanza? estado)
        {
            int lado = Math.Max(16, SystemInformation.SmallIconSize.Width);
            string clave = (estado.HasValue ? estado.Value.ToString() : "sin") + lado;
            Icon icono;
            lock (Cache)
            {
                if (!Cache.TryGetValue(clave, out icono))
                {
                    using (Bitmap bitmap = Dibujar(lado, estado))
                    {
                        IntPtr handle = bitmap.GetHicon();
                        try
                        {
                            // Clone: el Icon de GetHicon no es dueno del handle; se copia y se libera el original.
                            icono = (Icon)Icon.FromHandle(handle).Clone();
                        }
                        finally
                        {
                            DestroyIcon(handle);
                        }
                    }

                    Cache[clave] = icono;
                }
            }

            return icono;
        }

        // Dibujo base: cuadrado redondeado navy + balanza blanca (barra, apoyo y platillos) + punto de estado opcional.
        public static Bitmap Dibujar(int lado, EstadoBalanza? estado)
        {
            var bitmap = new Bitmap(lado, lado);
            using (Graphics g = Graphics.FromImage(bitmap))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.Clear(Color.Transparent);

                float u = lado / 16f;
                using (GraphicsPath fondo = Redondeado(new RectangleF(0, 0, lado, lado), 3.2f * u))
                using (var pincel = new SolidBrush(Navy))
                {
                    g.FillPath(pincel, fondo);
                }

                // Sin punto de estado (icono de ventana) la balanza va centrada; con punto se corre arriba a la izquierda.
                if (!estado.HasValue)
                {
                    g.TranslateTransform(1.6f * u, 1.2f * u);
                }

                using (var blanco = new Pen(Color.White, Math.Max(1.2f, 1.2f * u)) { StartCap = LineCap.Round, EndCap = LineCap.Round })
                {
                    // Poste y base
                    g.DrawLine(blanco, 7 * u, 3 * u, 7 * u, 10.2f * u);
                    g.DrawLine(blanco, 4.2f * u, 11 * u, 9.8f * u, 11 * u);
                    // Barra y colgantes de los platillos
                    g.DrawLine(blanco, 2.5f * u, 5 * u, 11.5f * u, 5 * u);
                    g.DrawLine(blanco, 2.5f * u, 5 * u, 2.5f * u, 7.4f * u);
                    g.DrawLine(blanco, 11.5f * u, 5 * u, 11.5f * u, 7.4f * u);
                }

                using (var naranja = new SolidBrush(NaranjaClaro))
                {
                    g.FillPie(naranja, 0.8f * u, 6.2f * u, 3.4f * u, 3.4f * u, 0, 180);
                    g.FillPie(naranja, 9.8f * u, 6.2f * u, 3.4f * u, 3.4f * u, 0, 180);
                }

                g.ResetTransform();

                if (estado.HasValue)
                {
                    float d = 5.4f * u;
                    float x = lado - d - 0.6f * u;
                    float y = lado - d - 0.6f * u;
                    using (var aro = new SolidBrush(Color.White))
                    using (var punto = new SolidBrush(ColorPunto(estado.Value)))
                    {
                        g.FillEllipse(aro, x - 0.9f * u, y - 0.9f * u, d + 1.8f * u, d + 1.8f * u);
                        g.FillEllipse(punto, x, y, d, d);
                    }
                }
            }

            return bitmap;
        }

        // Icono de ventana (barra de titulo): el dibujo de la bandeja a 32 px, sin punto.
        public static Icon IconoVentana()
        {
            using (Bitmap bitmap = Dibujar(32, null))
            {
                IntPtr handle = bitmap.GetHicon();
                try
                {
                    return (Icon)Icon.FromHandle(handle).Clone();
                }
                finally
                {
                    DestroyIcon(handle);
                }
            }
        }

        public static GraphicsPath Redondeado(RectangleF r, float radio)
        {
            float d = radio * 2;
            var ruta = new GraphicsPath();
            ruta.AddArc(r.X, r.Y, d, d, 180, 90);
            ruta.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            ruta.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            ruta.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            ruta.CloseFigure();
            return ruta;
        }
    }
}
