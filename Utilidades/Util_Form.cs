using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

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

        public static bool validarCampoNumerico(string texto, string nombreTextBox)
        {
            bool resp = true;
            int index = 0;
            int cantPuntosDecimal = 0;
            
            foreach (char letra in texto)
            {
                //if ((!char.IsNumber(letra) && !char.IsPunctuation('.')) ||  char.IsLetter(letra) || !char.IsSeparator(' '))
                //{
                //    resp = false;
                //}
                //if (char.IsSymbol(letra))
                //{
                //    resp = false;
                //}&& (index>0 && letra == '-')

                ///
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

        private void Util_Form_Load(object sender, EventArgs e)
        {

        }
    }
}
