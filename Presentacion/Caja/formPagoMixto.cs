using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Presentacion.Caja
{
    public partial class formPagoMixto : Form
    {
        public float totalPesos, pagoMixtoEfectivo, importe2; 
        public string formaPago, formaPago2;
        public formPOS formPOS;
        public formUltimaVenta formUltimaVenta;
        bool exito = false;
        bool cargaEfectivo = true;

        Color enableColor = ColorTranslator.FromHtml(ConfigurationManager.AppSettings["enableColor"].ToString()); //SystemColors.Window;
        Color readOnlyColor = ColorTranslator.FromHtml(ConfigurationManager.AppSettings["readOnlyColor"].ToString());//SystemColors.ScrollBar;
        Color focusColor = ColorTranslator.FromHtml(ConfigurationManager.AppSettings["focusColor"].ToString());//Color.Orange;//Color.NavajoWhite;//Color.MediumAquamarine;

        Color ultimoColor = Color.Green;

        private void btnIngresar_Click(object sender, EventArgs e)
        {
            exito = false;
            // Normalización: quitamos puntos (miles) y cambiamos la coma por punto (decimal)
            string textoNormalizado = txtImporteEfectivo.Text.Contains(",") && txtImporteEfectivo.Text.Contains(".") ? txtImporteEfectivo.Text.Replace(".", "").Replace(",", ".") : txtImporteEfectivo.Text;
            if (float.TryParse(textoNormalizado, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out pagoMixtoEfectivo) && pagoMixtoEfectivo > 0 && importe2 > 0)
            {
                //si es Cero el efectivo informar - si cierra ventana mandar cero
                if (formPOS != null)
                    formPOS.pagoMixtoEfectivo = pagoMixtoEfectivo;
                if (formUltimaVenta != null)
                    formUltimaVenta.pagoMixtoEfectivo = pagoMixtoEfectivo;

                exito = true;
                this.Close();
            }
            else
            {
                MessageBox.Show("El pago en efectivo debe ser mayor a cero y menor al monto total de la venta",
                    "Error pago efectivo", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtImporteEfectivo.Focus();
            }
        }

        private void formPagoMixto_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!exito)
            {
                if (formPOS != null)
                    formPOS.pagoMixtoEfectivo = 0;
                if (formUltimaVenta != null)
                    formUltimaVenta.pagoMixtoEfectivo = 0;
            }
        }

        public formPagoMixto()
        {
            InitializeComponent(); this.Icon = Properties.Resources.CarniSys_ICONO;
        }

        private void txtImporteEfectivo_Enter(object sender, EventArgs e)
        {
            cargaEfectivo = true;
            txtImporteEfectivo.BackColor = focusColor;
        }

        private void txtImporteEfectivo_Leave(object sender, EventArgs e)
        {
            txtImporteEfectivo.BackColor = enableColor;
        }

        private void btnIngresar_Enter(object sender, EventArgs e)
        {
            btnIngresar.BackColor = focusColor;
        }

        private void btnIngresar_Leave(object sender, EventArgs e)
        {
            btnIngresar.BackColor = enableColor;
        }

        private void txtImporte2_TextChanged(object sender, EventArgs e)
        {
            TextBox txtBox = sender as TextBox;
            if (string.IsNullOrEmpty(txtBox.Text))
                importe2 = 0;

            if (cargaEfectivo)
                return;

            if (string.IsNullOrEmpty(txtBox.Text) || float.TryParse(txtImporte2.Text, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out importe2))
            {
                pagoMixtoEfectivo = totalPesos - importe2;
                txtImporteEfectivo.Text = pagoMixtoEfectivo.ToString("N2");
            }
            else
            {
                MessageBox.Show("Importe Inválido");
                txtBox.Text = "";
            }
        }

        private void txtImporte2_Enter(object sender, EventArgs e)
        {
            cargaEfectivo = false;
            txtImporte2.BackColor = focusColor;
        }

        private void txtImporte2_Leave(object sender, EventArgs e)
        {
            txtImporte2.BackColor = enableColor;
        }

        private void formPagoMixto_Load(object sender, EventArgs e)
        {
            txtTotalS.Text = totalPesos.ToString("N2");
            lblFormaPagoTicket.Text = formaPago;
            // Ajustar posición para que crezca hacia la izquierda
            Size textSize = TextRenderer.MeasureText(formaPago, lblFormaPagoTicket.Font);
            lblFormaPagoTicket.Location = new Point(177 - textSize.Width, 146);
            txtImporteEfectivo.Text = pagoMixtoEfectivo > 0 ? pagoMixtoEfectivo.ToString("N2") : "";
            txtImporteEfectivo.Focus();

        }

        private void txtImporte1_TextChanged(object sender, EventArgs e)
        {
            TextBox txtBox = sender as TextBox;
            if (string.IsNullOrEmpty(txtBox.Text))
                pagoMixtoEfectivo = 0;

            if (!cargaEfectivo)
                return;

            if (string.IsNullOrEmpty(txtBox.Text) || float.TryParse(txtBox.Text, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out pagoMixtoEfectivo))
            {
                importe2 = totalPesos - pagoMixtoEfectivo;
                txtImporte2.Text = importe2.ToString("N2");
            }
            else
            {
                MessageBox.Show("Importe Inválido");
                txtBox.Text = "";
            }
        }


        private void TxtPruebaENTER_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)(Keys.Enter))
            {
                e.Handled = true;
                SendKeys.Send("{TAB}");
            }
        }
    }
}
