using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Configuration;

namespace Utilidades
{
    public partial class Util_Form : Form
    {
        public Util_Form()
        {
            InitializeComponent();
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

        public static float convertFloat(string toFloat)
        {
            float? value = null;
            try 
	        {
                toFloat = toFloat.Contains(',') ? toFloat.Replace(',', '.') : toFloat;
        		value =  float.Parse( toFloat, System.Globalization.NumberStyles.Float, new System.Globalization.CultureInfo("en-US"));
	        }
	        catch (Exception ex)
	        {
        		MessageBox.Show("Error al convertir a tipo Float\n"+ex.Message, "Convertir a float");
	        }
            return (float)value;
        }

        public static bool validarNumeroMayorACero(string valor, string nombreTextBox)
        {
            bool resp = validarCampoNumerico(valor, nombreTextBox);
            if (resp)
            {
                float? value = valor.Contains("-") ? 0 : convertFloat(valor);
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

        public static DialogResult errorBalanza(string error)
        {
            DialogResult resp = MessageBox.Show("Error al leer peso de Balanza: " + error + ".\nVerifique la conexion.\n\n¿Dejar de leer el peso de la Balanza?", "Error balanza", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1);
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
            int sucActual = Convert.ToInt32(ConfigurationManager.AppSettings["idSucursal"].ToString());
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
    }
}
