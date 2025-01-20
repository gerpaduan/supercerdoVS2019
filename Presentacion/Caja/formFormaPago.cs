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
    public partial class formFormaPago : Form
    {
        Entidades.Venta.formaPagoEnum formaPago = Entidades.Venta.formaPagoEnum.Nulo;

        public formFormaPago()
        {
            InitializeComponent(); this.Icon = Properties.Resources.CarniSys_ICONO;
        }

        public void enviarFormaPago()
        {
            if (formaPago.Equals(Entidades.Venta.formaPagoEnum.Nulo))
            {
                MessageBox.Show("Debe seleccionar una forma de pago");
                return;
            }
            InterfaceFormaPago formInterface = this.Owner as InterfaceFormaPago;
            if (formInterface != null)
            {
                formInterface.EnviarFormaPago(this.formaPago);
            }
            this.Close();
        }

        private void btnEfectivo_Click(object sender, EventArgs e)
        {
            formaPago = Entidades.Venta.formaPagoEnum.Efectivo;
            enviarFormaPago();
        }

        private void btnDebito_Click(object sender, EventArgs e)
        {
            formaPago = Entidades.Venta.formaPagoEnum.Debito;
            enviarFormaPago();
        }

        private void btnCredito_Click(object sender, EventArgs e)
        {
            formaPago = Entidades.Venta.formaPagoEnum.Credito;
            enviarFormaPago();
        }

        private void btnCtaCtePago_Click(object sender, EventArgs e)
        {
            formaPago = Entidades.Venta.formaPagoEnum.CtaCte;
            enviarFormaPago();
        }

        private void btnQr_Click(object sender, EventArgs e)
        {
            formaPago = Entidades.Venta.formaPagoEnum.Qr;
            enviarFormaPago();
        }

        private void btnTransf_Click(object sender, EventArgs e)
        {
            formaPago = Entidades.Venta.formaPagoEnum.Transferencia;
            enviarFormaPago();
        }
        
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {            
            unfocused();
            switch (keyData)
            {
                case Keys.NumPad1:
                    btnEfectivo.BackColor = Utilidades.Util_Form.checkedColor;
                    formaPago = Entidades.Venta.formaPagoEnum.Efectivo;
                    btnEfectivo.Focus();
                    break;
                case Keys.NumPad2:
                    btnDebito.BackColor = Utilidades.Util_Form.checkedColor;
                    formaPago = Entidades.Venta.formaPagoEnum.Debito;
                    btnDebito.Focus();
                    break;
                case Keys.NumPad3:
                    btnCredito.BackColor = Utilidades.Util_Form.checkedColor;
                    formaPago = Entidades.Venta.formaPagoEnum.Credito;
                    btnCredito.Focus();
                    break;
                case Keys.NumPad4:
                    btnCtaCtePago.BackColor = Utilidades.Util_Form.checkedColor;
                    formaPago = Entidades.Venta.formaPagoEnum.CtaCte;
                    btnCtaCtePago.Focus();
                    break;
                case Keys.NumPad5:
                    btnQr.BackColor = Utilidades.Util_Form.checkedColor;
                    formaPago = Entidades.Venta.formaPagoEnum.Qr;
                    btnQr.Focus();
                    break;
                case Keys.NumPad6:
                    btnTransf.BackColor = Utilidades.Util_Form.checkedColor;
                    formaPago = Entidades.Venta.formaPagoEnum.Transferencia;
                    btnTransf.Focus();
                    break;
                case Keys.Enter:
                    enviarFormaPago();
                    break;
                case Keys.Escape:
                    this.Close();
                    break;
                default:
                    //formaPago = Entidades.Venta.formaPagoEnum.Nulo;
                    if (!(keyData.Equals(Keys.Left) || keyData.Equals(Keys.Right) || 
                        keyData.Equals(Keys.Up) || keyData.Equals(Keys.Down)))
                    {
                        formaPago = Entidades.Venta.formaPagoEnum.Nulo;  
                        label1.Focus();
                        label1.Select();
                    }
                    unfocused();
                    break;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void formFormaPago_Load(object sender, EventArgs e)
        {
            label1.Select();
            label1.Focus();
        }

        private void btnEfectivo_Enter(object sender, EventArgs e)
        {
            unfocused();
            btnEfectivo.BackColor = Utilidades.Util_Form.checkedColor;
        }

        private void btnDebito_Enter(object sender, EventArgs e)
        {
            unfocused();
            btnDebito.BackColor = Utilidades.Util_Form.checkedColor;
        }

        private void btnCredito_Enter(object sender, EventArgs e)
        {
            unfocused();
            btnCredito.BackColor = Utilidades.Util_Form.checkedColor;
        }

        private void btnCtaCtePago_Enter(object sender, EventArgs e)
        {
            unfocused();
            btnCtaCtePago.BackColor = Utilidades.Util_Form.checkedColor;
        }

        private void btnQr_Enter(object sender, EventArgs e)
        {
            unfocused();
            btnQr.BackColor = Utilidades.Util_Form.checkedColor;
        }

        private void btnTransf_Enter(object sender, EventArgs e)
        {

            unfocused();
            btnTransf.BackColor = Utilidades.Util_Form.checkedColor;
        }

        private void unfocused()
        {
            if (formaPago == Entidades.Venta.formaPagoEnum.Nulo)
                //label1.Select();
            formaPago = Entidades.Venta.formaPagoEnum.Nulo;            
            btnEfectivo.BackColor = Utilidades.Util_Form.readOnlyColor;
            btnDebito.BackColor = Utilidades.Util_Form.readOnlyColor;
            btnCredito.BackColor = Utilidades.Util_Form.readOnlyColor;
            btnCtaCtePago.BackColor = Utilidades.Util_Form.readOnlyColor;
            btnQr.BackColor = Utilidades.Util_Form.readOnlyColor;
            btnTransf.BackColor = Utilidades.Util_Form.readOnlyColor;
        }
    }
}
