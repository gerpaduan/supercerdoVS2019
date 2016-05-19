using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Utilidades;
using System.Configuration;

namespace Presentacion.Caja
{
    public partial class formIngresoBilletes : Form
    {
        public TextBox txtBoxAcargar;
        public float total = 0, cien = 0, cincuenta = 0, 
            veinte = 0, diez = 0, cinco = 0, dos = 0, monedas = 0;

        Color enableColor = ColorTranslator.FromHtml(ConfigurationManager.AppSettings["enableColor"].ToString()); //SystemColors.Window;
        Color readOnlyColor = ColorTranslator.FromHtml(ConfigurationManager.AppSettings["readOnlyColor"].ToString());//SystemColors.ScrollBar;
        Color focusColor = ColorTranslator.FromHtml(ConfigurationManager.AppSettings["focusColor"].ToString());//Color.Orange;//Color.NavajoWhite;//Color.MediumAquamarine;
        Color ultimoColor = Color.Green;

        public formIngresoBilletes()
        {
            InitializeComponent();
        }

        private void formIngresoBilletes_Load(object sender, EventArgs e)
        {

        }

        private void txtPress_Enter(object sender, KeyEventArgs e)
        {
            if (e.KeyValue == (char)(Keys.Enter))
            {
                e.Handled = true;
                SendKeys.Send("{TAB}");
                return;
            }
        }

        private void txtChangeValue_TextChanged(object sender, EventArgs e)
        {
            TextBox objectKeyDown = (TextBox)sender;
            if (objectKeyDown.Name.Equals("txtTotalMonedas"))
            {
                if (!string.IsNullOrEmpty(objectKeyDown.Text) && !Util_Form.validarCampoNumerico(objectKeyDown.Text, objectKeyDown.Name))
                {
                    objectKeyDown.Text = "";
                    return;
                }
                monedas = string.IsNullOrEmpty(objectKeyDown.Text) ? 0 : Util_Form.convertFloat(objectKeyDown.Text, true);
            }
            else
            {
                if (!Util_Form.validarCampoNumeroEntero(objectKeyDown.Text, objectKeyDown.Name))
                {
                    objectKeyDown.Text = "";
                    return;
                }
                int billete = Convert.ToInt32(objectKeyDown.Name.Replace("txt", ""));
                int totalBillete = string.IsNullOrEmpty(objectKeyDown.Text) ? 0 : Convert.ToInt32(objectKeyDown.Text) * Convert.ToInt32(billete);

                switch (billete)
                {
                    case 100:
                        cien = totalBillete;
                        txt100Total.Text = totalBillete.ToString("F2");
                        break;
                    case 50:
                        cincuenta = totalBillete;
                        txt50Total.Text = totalBillete.ToString("F2");
                        break;
                    case 20:
                        veinte = totalBillete;
                        txt20Total.Text = totalBillete.ToString("F2");
                        break;
                    case 10:
                        diez = totalBillete;
                        txt10Total.Text = totalBillete.ToString("F2");
                        break;
                    case 5:
                        cinco = totalBillete;
                        txt5Total.Text = totalBillete.ToString("F2");
                        break;
                    case 2:
                        dos = totalBillete;
                        txt2Total.Text = totalBillete.ToString("F2");
                        break;
                    default:
                        break;
                }
            }
            total = cien + cincuenta + veinte + diez + cinco + dos + monedas;
            txtTotalCambio.Text = (total - cien).ToString("F2");
            txtTotal.Text = total.ToString("F2");
        }


        private void btnAceptar_Click(object sender, EventArgs e)
        {
            txtBoxAcargar.Text = txtTotal.Text;
            this.Close();
        }

        private void control_Enter(object sender, EventArgs e)
        {
            if (sender is TextBox)
            {
                TextBox objectToChangeColor = (TextBox)sender;
                if (!objectToChangeColor.BackColor.Equals(focusColor)) ultimoColor = objectToChangeColor.BackColor;
                objectToChangeColor.BackColor = focusColor;
                return;
            }

            if (sender is MaskedTextBox)
            {
                MaskedTextBox objectToChangeColor = (MaskedTextBox)sender;
                if (!objectToChangeColor.BackColor.Equals(focusColor)) ultimoColor = objectToChangeColor.BackColor;
                objectToChangeColor.BackColor = focusColor;
                return;
            }

            if (sender is Button)
            {
                Button objectToChangeColor = (Button)sender;
                objectToChangeColor.UseVisualStyleBackColor = false;
                objectToChangeColor.BackColor = focusColor;
                return;
            }
        }

        private void control_Leave(object sender, EventArgs e)
        {
            if (sender is TextBox)
            {
                TextBox objectToChangeColor = (TextBox)sender;
                objectToChangeColor.BackColor = ultimoColor;
                return;
            }

            if (sender is MaskedTextBox)
            {
                MaskedTextBox objectToChangeColor = (MaskedTextBox)sender;
                objectToChangeColor.BackColor = ultimoColor;
                return;
            }

            if (sender is Button)
            {
                Button objectToChangeColor = (Button)sender;
                objectToChangeColor.UseVisualStyleBackColor = true;
                return;
            }
        }

    }
}
