using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Presentacion.Caja
{
    public partial class formBonificar : Form
    {
        public Entidades.LineaVenta oLineaVenta;
        public formVentaCaja frmVentaCaja;
        float precio, total;
        bool validado = true;

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
            frmVentaCaja.precioBonificado = oLineaVenta.PrecioKg.ToString("F2");
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
                frmVentaCaja.precioBonificado = txtPrecioKg.Text;
                MessageBox.Show("La bonificación se realizó correctamente.");
                this.Close();
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
        }
    }
}
