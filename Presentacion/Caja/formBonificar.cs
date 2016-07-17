using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Configuration;

namespace Presentacion.Caja
{
    public partial class formBonificar : Form
    {
        public Entidades.LineaVenta oLineaVenta;
        public formVentaCaja frmVentaCaja;
        float precio, total;
        bool validado = true;

        Color enableColor = ColorTranslator.FromHtml(ConfigurationManager.AppSettings["enableColor"].ToString()); //SystemColors.Window;
        Color readOnlyColor = ColorTranslator.FromHtml(ConfigurationManager.AppSettings["readOnlyColor"].ToString());//SystemColors.ScrollBar;
        Color focusColor = ColorTranslator.FromHtml(ConfigurationManager.AppSettings["focusColor"].ToString());//Color.Orange;//Color.NavajoWhite;//Color.MediumAquamarine;

        Color ultimoColor = Color.Green;

        public formBonificar()
        {
            InitializeComponent();
        }

        private void formBonificar_Load(object sender, EventArgs e)
        {
            txtCodigo.Text = oLineaVenta.Corte.codigo.ToString();
            txtCorte.Text = oLineaVenta.Corte.CorteDesc;
            txtPrecioKg.Text = oLineaVenta.PrecioKg.ToString("F2");
            txtCantKgs.Text = oLineaVenta.CantKg.ToString("F3");
            txtTotalCorte.Text = (oLineaVenta.PrecioKg * oLineaVenta.CantKg).ToString("F2");
            btnPrecioReal.Text = "Quitar Bonif.";
            btnPrecioReal.Visible = !oLineaVenta.PrecioKg.Equals(oLineaVenta.Corte.precioKg);

            txtPrecioKg.SelectAll();
        }

        private void txtPrecioKg_TextChanged(object sender, EventArgs e)
        {
            try
            {                
                validado = false;
                if (txtPrecioKg.Text.Equals("")) return;

                if (Utilidades.Util_Form.validarCampoNumerico(txtPrecioKg.Text, "Precio"))
                {
                    precio = Utilidades.Util_Form.convertFloat(txtPrecioKg.Text, false);
                    total = precio * oLineaVenta.CantKg;
                    txtTotalCorte.Text = total.ToString("F2");
                    validado = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ingrese un precio válido\n\n"+ex.Message, "Error en el precio");
            }
        }

        private void btnBonificar_Click(object sender, EventArgs e)
        {
            if (validado)
            {
                if (precio.Equals(oLineaVenta.Corte.precioKg) && oLineaVenta.Bonificacion == 0)
                {
                    MessageBox.Show("No se puede bonificar porque no se realizó cambios en el precio.\n"+
                        "Para salir presione la tecla Esc","No se realizó bonificación");
                }
                else
                {
                    frmVentaCaja.precioBonificado = txtPrecioKg.Text;
                    MessageBox.Show("La bonificación se realizó correctamente.");
                    this.Close();
                }
            }
            else
            {
                MessageBox.Show("Ingrese un precio válido", "Error en el precio");
                txtPrecioKg.Focus();
            }
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Escape)
            {
                this.Close();
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void btnPrecioReal_Click(object sender, EventArgs e)
        {
            txtPrecioKg.Text = oLineaVenta.Corte.precioKg.ToString("F2");
            btnBonificar.Focus();
        }

        private void TxtPruebaENTER_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)(Keys.Enter))
            {
                btnBonificar.Focus();
            }
        }

        private void txtPrecioKg_Enter(object sender, EventArgs e)
        {
            this.txtPrecioKg.BackColor = focusColor;
        }

        private void txtPrecioKg_Leave(object sender, EventArgs e)
        {
            this.txtPrecioKg.BackColor = enableColor;
        }

        private void btnBonificar_Enter(object sender, EventArgs e)
        {
            this.btnBonificar.UseVisualStyleBackColor = false;
            this.btnBonificar.BackColor = focusColor;

        }

        private void btnBonificar_Leave(object sender, EventArgs e)
        {
            this.btnBonificar.UseVisualStyleBackColor = true;
        }

    }
}
