using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Configuration;
using System.IO;
using IWshRuntimeLibrary;
using System.Drawing.Imaging;
using System.Management;
using System.Diagnostics;

namespace Utilidades
{
    public partial class Util_Form : Form
    {
        public static Color enableColor = ColorTranslator.FromHtml(ConfigurationManager.AppSettings["enableColor"].ToString()); //SystemColors.Window;
        public static Color readOnlyColor = ColorTranslator.FromHtml(ConfigurationManager.AppSettings["readOnlyColor"].ToString());//SystemColors.ScrollBar;
        public static Color focusColor = ColorTranslator.FromHtml(ConfigurationManager.AppSettings["focusColor"].ToString());//Color.Orange;//Color.NavajoWhite;//Color.MediumAquamarine;
        public static Color checkedColor = Color.DarkSeaGreen;
        public static float escalaPantalla = convertFloat(ConfigurationManager.AppSettings["escalaPantalla"].ToString(), false);

        private static bool _enEdicion = false;

        public Util_Form()
        {
            InitializeComponent();
        }

        private void Util_Form_Load(object sender, EventArgs e)
        {

        }


        /// <summary>
        /// Formatea un TextBox de importe mientras se escribe:
        /// - convierte punto a coma
        /// - agrega separador de miles automáticamente
        /// - mantiene el cursor en la posición correcta
        /// </summary>
        /// 
        public static void ValidarImporte(TextBox txt, bool aceptarNegativo = false)
        {
            if (txt == null) return;

            int cursor = txt.SelectionStart;
            string input = txt.Text ?? string.Empty;

            if (input.Length == 0) return;

            bool negativo = false;

            // -------------------------------
            // 1) Procesar signo negativo
            // -------------------------------
            if (aceptarNegativo)
            {
                if (input == "-")
                {
                    // dejar exactamente "-" y posicionar cursor al final
                    txt.Text = "-";
                    txt.SelectionStart = txt.Text.Length;
                    return;
                }

                if (input.StartsWith("-"))
                {
                    negativo = true;
                    input = input.Substring(1); // quitar signo para procesar números
                }

                // quitar "-" en cualquier otra posición
                input = input.Replace("-", "");
            }
            else
            {
                // eliminar cualquier '-'
                input = input.Replace("-", "");
            }

            // -------------------------------
            // 2) Normalizar puntos/decimales
            // -------------------------------
            if (input.EndsWith(".")) input = input.Substring(0, input.Length - 1) + ",";
            input = input.Replace(".", "");

            // -------------------------------
            // 3) Mantener solo números y UNA coma
            // -------------------------------
            var sb = new System.Text.StringBuilder();
            bool coma = false;

            foreach (char c in input)
            {
                if (char.IsDigit(c)) sb.Append(c);
                else if (c == ',' && !coma)
                {
                    sb.Append(',');
                    coma = true;
                }
            }

            string limpio = sb.ToString();

            if (string.IsNullOrEmpty(limpio))
            {
                // si no quedó nada, permitimos "-" si corresponde, o dejamos vacío
                txt.Text = negativo ? "-" : "";
                txt.SelectionStart = txt.Text.Length;
                return;
            }

            // -------------------------------
            // 4) Separar entero / decimal
            // -------------------------------
            string ent = limpio;
            string dec = "";
            int idx = limpio.IndexOf(',');
            if (idx >= 0)
            {
                ent = limpio.Substring(0, idx);
                if (idx + 1 < limpio.Length) dec = limpio.Substring(idx + 1);
            }

            // -------------------------------
            // 5) Formatear miles (con fallback si el número es muy grande)
            // -------------------------------
            string entFmt;
            if (string.IsNullOrEmpty(ent))
            {
                entFmt = "";
            }
            else if (long.TryParse(ent, out long entVal))
            {
                // "N0" respeta la cultura actual (separador de miles)
                entFmt = entVal.ToString("N0");
            }
            else
            {
                // fallback: si no puede parsear (número muy grande o raro), usar tal cual
                entFmt = ent;
            }

            string result = entFmt + (idx >= 0 ? "," + dec : "");

            if (negativo && aceptarNegativo)
                result = "-" + result;

            // -------------------------------
            // 6) Aplicar resultado + cursor
            // -------------------------------
            int diff = result.Length - txt.Text.Length;
            txt.Text = result;

            int newPos = cursor + diff;
            if (newPos < 0) newPos = 0;
            if (newPos > txt.Text.Length) newPos = txt.Text.Length;
            txt.SelectionStart = newPos;
        }

        //public static void ValidarImporte(TextBox txt)
        //{
        //    if (txt == null) return;

        //    // recordar posición del cursor
        //    int sel = txt.SelectionStart;

        //    string texto = txt.Text;
        //    if (string.IsNullOrWhiteSpace(texto)) return;

        //    // ----- Si el último carácter es '.' convertirlo en ',' -----
        //    if (texto.Length > 0 && texto[texto.Length - 1] == '.')
        //    {
        //        texto = texto.Substring(0, texto.Length - 1) + ",";
        //    }

        //    // Normalizar: quitar puntos (ingresados manualmente) pero mantener coma
        //    texto = texto.Replace(".", ""); // eliminamos puntos previos
        //                                    // (los puntos de miles se volverán a agregar)
        //                                    // Nota: no reemplazamos la coma aquí

        //    // Mantener sólo dígitos y una coma (si existe)
        //    var sb = new System.Text.StringBuilder();
        //    bool comaEncontrada = false;
        //    foreach (char c in texto)
        //    {
        //        if (char.IsDigit(c)) sb.Append(c);
        //        else if (c == ',' && !comaEncontrada) { sb.Append(','); comaEncontrada = true; }
        //    }
        //    string limpio = sb.ToString();

        //    // Separar parte entera y decimal
        //    string entera, dec = "";
        //    int pos = limpio.IndexOf(',');
        //    if (pos >= 0)
        //    {
        //        entera = limpio.Substring(0, pos);
        //        if (limpio.Length > pos + 1) dec = limpio.Substring(pos + 1);
        //    }
        //    else
        //    {
        //        entera = limpio;
        //    }

        //    // Formatear parte entera con miles (si está vacía dejar vacío)
        //    string entFmt = string.IsNullOrEmpty(entera) ? "" : long.Parse(entera).ToString("N0");

        //    // Reconstruir resultado
        //    string nuevo = entFmt + (pos >= 0 ? "," + dec : "");

        //    // Asignar y restaurar cursor (ajustando por cambio de longitud)
        //    int diff = nuevo.Length - txt.Text.Length;
        //    txt.Text = nuevo;
        //    int nuevaPos = sel + diff;
        //    if (nuevaPos < 0) nuevaPos = 0;
        //    if (nuevaPos > txt.Text.Length) nuevaPos = txt.Text.Length;
        //    txt.SelectionStart = nuevaPos;
        //}

        public static bool validarCampoVacio(string texto, string nombreTextBox)
        {
            bool resp = true;

            if (String.IsNullOrEmpty(texto))
            {
                resp = false;
                MessageBox.Show("-" + nombreTextBox + " está vacío. Ingrese un valor.", "Error ingreso", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return resp;
        }

        /// <summary>
        /// //Cargando TextBox para validar
        /// int nroFilas = 0;
        /// int nombreTextBox = 1;
        /// int valorTextBox = 0;
        /// 
        /// string[,] textBoxes = new string[CantidadTexbox, 2];
        /// 
        /// string[valor_campo][nombre_textBox] 
        /// 
        /// textBoxes[nroFilas, valorTextBox] = txtDescripcion.Text;
        /// textBoxes[nroFilas++, nombreTextBox] = lblDescripcion.Text;
        /// </summary>
        public static bool validarArrayCamposVacios(string[,] textBoxes)
        {
            int nombreTextBox = 1;
            int valorTextBox = 0;
            bool resp = true;
            string mensaje = "Complete los siguientes campos";
            for (int fila = 0; fila < textBoxes.GetLength(0); fila++)
            {
                if (String.IsNullOrEmpty(textBoxes[fila, valorTextBox]))
                {
                    resp = false;
                    mensaje += "\n- " + textBoxes[fila, nombreTextBox];
                }
            }
            if (!resp)
            {
                MessageBox.Show(mensaje, "Error ingreso", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return resp;
        }

        public static bool validarCampoNumerico(string texto, string nombreTextBox)
        {
            bool resp = texto.Length > 0 ? double.TryParse(texto, out _) : false;
            if (!resp)
            {
                MessageBox.Show("-" + nombreTextBox + " debe ser un número.", "Error ingreso", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return resp;

        }

        public static bool validarCampoNumeroEntero(string texto, string nombreTextBox)
        {
            bool resp = true;
            foreach (char letra in texto)
            {
                if (!char.IsNumber(letra))
                {
                    resp = false;
                }
            }
            if (!resp)
            {
                MessageBox.Show("-" + nombreTextBox + " debe ser un número entero.", "Error ingreso", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return resp;

        }

        public static float convertFloat(string toFloat, bool messageBox, bool esPesaje = true)
        {
            float? value = null;
            try
            {
                //Si contiene '.' y ',' 
                toFloat = !toFloat.Contains("..") && toFloat.Contains('.') && toFloat.Contains(',') ? toFloat.Replace(".", "") : toFloat;

                //si no es Pesaje -> es importe
                if (!esPesaje)
                {
                    toFloat = toFloat.Replace(".", "");
                }
                toFloat = toFloat.Contains(',') ? toFloat.Replace(',', '.') : toFloat;
                value = float.Parse(toFloat, System.Globalization.NumberStyles.Float, new System.Globalization.CultureInfo("en-US"));
            }
            catch (Exception ex)
            {
                if (messageBox) MessageBox.Show("Error al convertir a tipo Float\n" + ex.Message, "Convertir a float");
            }
            return (float)value;
        }

        public static bool validarNumeroMayorACero(string valor, string nombreTextBox)
        {
            bool resp = validarCampoNumerico(valor, nombreTextBox);
            if (resp)
            {
                float? value = valor.Contains("-") ? 0 : convertFloat(valor, true);
                if (value.Equals(null) || value <= 0)
                {
                    resp = false;
                    MessageBox.Show("-" + nombreTextBox + " debe ser un valor mayor a cero.", "Error ingreso", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            return resp;
        }

        public static bool validarNumeroMayorUnGramo(string texto, string nombreTextBox)
        {
            double limit = double.Parse("0,001");
            bool resp = double.TryParse(texto.Replace('.', ','), out double peso) && peso >= limit;
            if (!resp)
            {
                MessageBox.Show("-" + nombreTextBox + " debe ser un número mayor o igual a Un Gramo (mayor o igual 0,001)", "Error ingreso", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return resp;

        }

        public static bool validarFecha(DateTime fecha, string nombreTextBox)
        {
            bool resp = fecha > DateTime.Now ? false : true;

            if (!resp)
            {
                MessageBox.Show("-" + nombreTextBox + " debe ser menor o igual a la fecha de hoy.", "Error fecha", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return resp;
        }

        public static string fechaFormato24Horas(DateTime? fechaParaFormatear)
        {
            string fechaFormateada = "";
            try
            {
                fechaFormateada = fechaParaFormatear.Equals(null) ? "" : DateTime.Parse(fechaParaFormatear.ToString()).ToString("dd/MM/yyyy  HH:mm:ss");
            }
            catch (Exception)
            {
                fechaFormateada = "Error formato";
            }
            return fechaFormateada;
        }

        public static DialogResult errorBalanza(string error)
        {
            DialogResult resp = MessageBox.Show("Error al leer peso de Balanza: " + error + ".\nVerifique la conexion.\n\n¿Dejar de leer el peso de la Balanza?", "Error balanza", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
            return resp;
        }

        public static bool validarPermisoModif(bool esAdmin, DateTime fechaRegistro)
        {
            bool resp = true;
            if (!esAdmin && fechaRegistro < DateTime.Today)
            {
                resp = false;
                MessageBox.Show("No está logueado!.\n\nDebe iniciar sesión como administrador para " +
                "poder agregar o modificar registros con fecha anterior al día de hoy.\n\nInicie sesión y vuelva a intentar.", "No tiene permiso para cambios", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return resp;
        }

        public static bool validarSucursal(bool esAdmin, int idSucursal)
        {
            bool resp = true;
            int sucActual = Convert.ToInt32(Utilidades.Conexion.getIdSucursalConexion());
            if (!esAdmin && idSucursal != sucActual)
            {
                resp = false;
                MessageBox.Show("No está logueado!.\n\nDebe iniciar sesión como administrador para " +
                "poder agregar o modificar registros que pertenecen a una sucursal diferente a la que Ud. se encuentra.\n\nInicie sesión y vuelva a intentar.", "Sucursal diferente", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return resp;
        }

        public static bool validarFechaConAdmin(bool esAdmin, DateTime fechaRegistro, string nombreTextBox)
        {
            bool resp = fechaRegistro.Date != DateTime.Today && !esAdmin ? false : true;

            if (!resp)
            {
                MessageBox.Show("-" + nombreTextBox + " debe ser igual a la fecha de hoy -" + DateTime.Now.ToShortDateString() + "-.\n\n" +
                    "Debe iniciar sesión como administrador para " +
                    "poder agregar o modificar registros con fecha diferente al día de hoy.\n\nInicie sesión y vuelva a intentar.", "Fecha distinta a hoy", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return resp;
        }

        public static int idSucursalAppConfig()
        {
            return Convert.ToInt32(Utilidades.Conexion.getIdSucursalConexion());
        }

        public static string errorConexionBD(string exception)
        {
            string lineaDivisoria = "\n------------------\n";
            string mensaje = "No se pudo conectar a la base de datos. Verifique que haya elegido la conexión correcta.\n" +
                "--Si no se conecta posiblemente no haya INTERNET.--\n";
            mensaje = exception.Contains("Error relacionado con la red") ||
                exception.Contains("Proveedor de TCP") ? mensaje + lineaDivisoria + exception : exception;
            return mensaje;
        }

        public static bool errorConexionBD_Return(string exception)
        {
            string lineaDivisoria = "\n------------------\n";
            string mensaje = "No se pudo conectar a la base de datos. Verifique que haya elegido la conexión correcta.\n" +
                "--Si no se conecta posiblemente no haya INTERNET.--\n";
            mensaje = exception.Contains("Error relacionado con la red") ||
                exception.Contains("Proveedor de TCP") ? mensaje + lineaDivisoria + exception : exception;

            DialogResult resp = MessageBox.Show(mensaje, "Mensaje", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1);
            return resp.Equals(DialogResult.Yes) ? true : false;
        }

        public static string leerPesoBalanza()
        {
            string peso = "";
            bool formAbierto = false;
            foreach (Form frm in Application.OpenForms)
            {
                int d = Application.OpenForms.Count;
                if (frm.GetType() == typeof(FormPesoBalanza))
                {
                    foreach (Control ctrl in frm.Controls)
                    {
                        if (ctrl.Name.Equals("pesoBalanzaLabel"))
                        {
                            peso = ctrl.Text;
                            if (peso.Contains("error"))
                            {
                                frm.Close();
                                throw new Exception(peso);
                            }
                            if (string.IsNullOrEmpty(peso))
                            {
                                peso = "Peso nulo";
                                //throw new Exception("Peso nulo\n\n.Verifique que la balanza esté conectada correctamente");
                            }
                            formAbierto = true;
                            break;
                        }
                    }
                }
            }
            if (!formAbierto)
            {
                FormPesoBalanza frmBalanza = new FormPesoBalanza();
                frmBalanza.MinimizeBox = true;
                frmBalanza.Show();
                frmBalanza.Visible = false;
            }
            return peso;
        }

        public static void capturarPantalla(string nameCaptura, DateTime fechaRegistro)
        {
            try
            {
                string nombreCarpeta = "Capturas";
                string fullNameCaptura = DateTime.Now.ToString("dd-MM-yyyy HHmmss") + " - " + nameCaptura + ".jpg";
                string folderCaptura = fechaRegistro.ToString("dd-MM-yyyy HHmmss") + " - " + nameCaptura;
                string escritorio = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                string capturasPath = Path.GetFullPath(escritorio + "\\" + nombreCarpeta);// (@"\" + nombreCarpeta);
                if (!Directory.Exists(capturasPath))
                {
                    Directory.CreateDirectory(capturasPath);
                }

                string fullPath = Path.GetFullPath(capturasPath + "\\" + folderCaptura);// (@"\" + nombreCarpeta);

                if (!Directory.Exists(fullPath))
                {
                    Directory.CreateDirectory(fullPath);
                }

                // Obtener la resolución de la pantalla
                int screenWidth = (int)Math.Round((Screen.PrimaryScreen.Bounds.Width * escalaPantalla));
                int screenHeight = (int)Math.Round(Screen.PrimaryScreen.Bounds.Height * escalaPantalla);

                // Crear un bitmap con el tamaño de la pantalla
                using (Bitmap bitmap = new Bitmap(screenWidth, screenHeight))
                {
                    // Crear un objeto gráfico desde el bitmap
                    using (Graphics g = Graphics.FromImage(bitmap))
                    {
                        // Copiar la pantalla en el bitmap
                        g.CopyFromScreen(0, 0, 0, 0, bitmap.Size);
                    }

                    // Guardar el bitmap en un archivo
                    bitmap.Save(fullPath + "\\" + fullNameCaptura, System.Drawing.Imaging.ImageFormat.Png);
                }
            }
            catch (Exception)
            {
            }
        }

        public static Color getBackColorTextBox(bool readOnly)
        {
            try
            {
                Color color = readOnly ? ColorTranslator.FromHtml(ConfigurationManager.AppSettings["readOnlyColor"].ToString()) :
                    ColorTranslator.FromHtml(ConfigurationManager.AppSettings["enableColor"].ToString());

                return color;
            }
            catch (Exception)
            {
                return Color.White;
            }
        }
        public static Color getBackColorCheckBox(bool isChecked)
        {
            try
            {
                Color color = isChecked ? Color.DarkSeaGreen : ColorTranslator.FromHtml(ConfigurationManager.AppSettings["readOnlyColor"].ToString());

                return color;
            }
            catch (Exception)
            {
                return Color.White;
            }
        }

        public static void enviarWhatsApp(string telefono)
        {
            //telefono = "5493413396372"; // con código de país sin '+'
            string mensaje = Uri.EscapeDataString("Hola!");
            string url = $"https://wa.me/{telefono}?text={mensaje}";

            System.Diagnostics.Process.Start(new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
            });
        }

        public static string GetCPUId()
        {
            string cpuInfo = String.Empty;
            string temp = String.Empty;
            ManagementClass mc = new ManagementClass("Win32_Processor");
            ManagementObjectCollection moc = mc.GetInstances();
            foreach (ManagementObject mo in moc)
            {
                if ((cpuInfo == String.Empty))
                { cpuInfo = mo.Properties["ProcessorId"].Value.ToString(); }
            }
            return cpuInfo;
        }

        public static string GetHDSerial()
        {
            ManagementObject disk = new ManagementObject("Win32_LogicalDisk.DeviceID=\"C:\"");
            PropertyData diskPropertyA = disk.Properties["VolumeSerialNumber"];
            return diskPropertyA.Value.ToString();
        }

    }
}
