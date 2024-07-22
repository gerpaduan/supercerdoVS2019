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

namespace Utilidades
{
    public partial class Util_Form : Form
    {
        public static Color enableColor = ColorTranslator.FromHtml(ConfigurationManager.AppSettings["enableColor"].ToString()); //SystemColors.Window;
        public static Color readOnlyColor = ColorTranslator.FromHtml(ConfigurationManager.AppSettings["readOnlyColor"].ToString());//SystemColors.ScrollBar;
        public static Color focusColor = ColorTranslator.FromHtml(ConfigurationManager.AppSettings["focusColor"].ToString());//Color.Orange;//Color.NavajoWhite;//Color.MediumAquamarine;
        public static Color checkedColor = Color.LimeGreen;
        public static float escalaPantalla = convertFloat(ConfigurationManager.AppSettings["escalaPantalla"].ToString(), false);

        public Util_Form()
        {
            InitializeComponent();
        }

        private void Util_Form_Load(object sender, EventArgs e)
        {

        }

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
                if (String.IsNullOrEmpty(textBoxes[fila,valorTextBox]))
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
            bool resp = texto.Length > 0 ? true : false;
            int index = 0;
            int cantPuntosDecimal = 0;
            
            foreach (char letra in texto)
            {
                bool esNro = true;
                if (!char.IsNumber(letra))
                {
                    esNro = false;
                }
                if ((!esNro && letra != '.' && letra != ',' && !(index==0 && letra == '-')))
                {                    
                    resp = false;
                }
                if (resp && (letra.Equals('.') || letra.Equals(',')))
	            {
                    cantPuntosDecimal++;
                    if (cantPuntosDecimal > 1)
                    {
                        resp = false;
                    }
	            }
                index++;
            }
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

        public static float convertFloat(string toFloat, bool messageBox)
        {
            float? value = null;
            try 
	        {
                toFloat = !toFloat.Contains("..") && toFloat.Contains('.') && toFloat.Contains(',') ? toFloat.Replace(".", "") : toFloat;
                toFloat = toFloat.Contains(',') ? toFloat.Replace(',', '.') : toFloat;
        		value =  float.Parse( toFloat, System.Globalization.NumberStyles.Float, new System.Globalization.CultureInfo("en-US"));
	        }
	        catch (Exception ex)
	        {
                if(messageBox) MessageBox.Show("Error al convertir a tipo Float\n"+ex.Message, "Convertir a float");
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
                MessageBox.Show("-" + nombreTextBox + " debe ser igual a la fecha de hoy -" + DateTime.Now.ToShortDateString() + "-.\n\n"  +
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
            string mensaje = "No se pudo conectar a la base de datos. Verifique que haya elegido la conexión correcta.\n"+
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
            string peso="";
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
                Color color = isChecked ? Color.LimeGreen :ColorTranslator.FromHtml(ConfigurationManager.AppSettings["readOnlyColor"].ToString());

                return color;
            }
            catch (Exception)
            {
                return Color.White;
            }
        }
    }
}
