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
        public formVentaCajaConExpendio frmVentaCajaConExp;
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
            checkPorcentaje.Checked = false;
            checkBonificarTodos.Checked = false;
            CargarCampos();
            btnPrecioReal.Text = "&Quitar Bonif.";
            btnPrecioReal.Visible = !oLineaVenta.PrecioKg.Equals(oLineaVenta.Corte.precioKg);
            txtPrecioKg.SelectAll();
        }

        private void CargarCampos()
        {
            txtCodigo.Text = checkBonificarTodos.Checked ? "" : oLineaVenta.Corte.codigo.ToString();
            txtCorte.Text = checkBonificarTodos.Checked ? "" : oLineaVenta.Corte.CorteDesc;
            txtPrecioKg.Text = checkBonificarTodos.Checked ? "" : oLineaVenta.PrecioKg.ToString("F2");
            txtCantKgs.Text = checkBonificarTodos.Checked ? "" : oLineaVenta.CantKg.ToString("F3");
            txtTotalCorte.Text = checkBonificarTodos.Checked ? "" : (oLineaVenta.PrecioKg * oLineaVenta.CantKg).ToString("F2");
        }

        private void txtPrecioKg_TextChanged(object sender, EventArgs e)
        {
            try
            {
                if (checkBonificarTodos.Checked)
                    return;

                validado = false;
                if (txtPrecioKg.Text.Equals("")) return;

                if (!txtPrecioKg.Text.Contains("-") && Utilidades.Util_Form.validarCampoNumerico(txtPrecioKg.Text, "Precio"))
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
                if (checkBonificarTodos.Checked)
                {
                    frmVentaCaja.bonificarTodos = checkBonificarTodos.Checked;
                    this.Close();
                    return;
                }

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
                if (checkPorcentaje.Checked)
                    txtPorcentaje.Focus();
                else
                    txtPrecioKg.Focus();
            }
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            
            switch (keyData)
            {
                case Keys.Escape:
                    this.Close();
                    break;
                case Keys.F4:
                    checkPorcentaje.Checked = !checkPorcentaje.Checked;
                    break;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void btnPrecioReal_Click(object sender, EventArgs e)
        {
            if (checkBonificarTodos.Checked)
            {
                txtPorcentaje.Text = "0";
                frmVentaCaja.porcentajeBonif_String = "0";
                MessageBox.Show("Presione el botón Bonificar para terminar el proceso de la quita de bonificación");
            }
            else
            {
                txtPrecioKg.Text = oLineaVenta.Corte.precioKg.ToString("F2");
                txtPorcentaje.Text = "";
            }
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
            txtPrecioKg.BackColor = checkPorcentaje.Checked ? readOnlyColor : enableColor;
        }

        private void btnBonificar_Enter(object sender, EventArgs e)
        {
            this.btnBonificar.UseVisualStyleBackColor = false;
            this.btnBonificar.BackColor = focusColor;

        }

        private void checkPorcentaje_CheckedChanged(object sender, EventArgs e)
        {
            checkPorcentaje.BackColor = Utilidades.Util_Form.getBackColorCheckBox(checkPorcentaje.Checked);

            txtPorcentaje.Enabled = checkPorcentaje.Checked;
            txtPrecioKg.ReadOnly = checkPorcentaje.Checked;
            txtPrecioKg.BackColor = checkPorcentaje.Checked ? readOnlyColor : enableColor;
            if (checkPorcentaje.Checked)
                txtPorcentaje.Select();
            else
            {
                checkBonificarTodos.Checked = false;
                txtPorcentaje.Text = "";
                txtPrecioKg.Focus();
                txtPrecioKg.SelectAll();
            }
        }

        private void txtPorcentaje_TextChanged(object sender, EventArgs e)
        {
            try
            {
                //Si es vacio el porcentaje recupera el precio de lista del corte
                if (txtPorcentaje.Text.Equals(""))
                {
                    if (!checkBonificarTodos.Checked)
                        txtPrecioKg.Text = oLineaVenta.Corte.precioKg.ToString("F2");
                    return;
                }

                if (Utilidades.Util_Form.validarCampoNumerico(txtPorcentaje.Text, "Porcentaje"))
                {
                    float porcentaje = (100 - Utilidades.Util_Form.convertFloat(txtPorcentaje.Text, false)) / 100;
                    frmVentaCaja.porcentajeBonif_String = txtPorcentaje.Text;
                    if (!checkBonificarTodos.Checked)
                        txtPrecioKg.Text  = (oLineaVenta.Corte.PrecioKg * porcentaje ).ToString("F2");
                    validado = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ingrese un % válido\n\n" + ex.Message, "Error en el precio");
                validado = false;
            }
        }

        private void checkBonificarTodos_CheckedChanged(object sender, EventArgs e)
        {
            checkBonificarTodos.BackColor = Utilidades.Util_Form.getBackColorCheckBox(checkBonificarTodos.Checked);
            checkPorcentaje.Checked = checkBonificarTodos.Checked;
            CargarCampos();
        }

        private void formBonificar_FormClosing(object sender, FormClosingEventArgs e)
        {
        }

        private void btnBonificar_Leave(object sender, EventArgs e)
        {
            this.btnBonificar.UseVisualStyleBackColor = true;
        }

    }
}
