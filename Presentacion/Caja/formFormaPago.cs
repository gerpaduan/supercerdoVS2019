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
            InitializeComponent();
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
        
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            switch (keyData)
            {
                case Keys.NumPad1:
                    formaPago = Entidades.Venta.formaPagoEnum.Efectivo;
                    enviarFormaPago();
                    break;
                case Keys.NumPad2:
                    formaPago = Entidades.Venta.formaPagoEnum.Debito;
                    enviarFormaPago();
                    break;
                case Keys.NumPad3:
                    formaPago = Entidades.Venta.formaPagoEnum.Credito;
                    enviarFormaPago();
                    break;  
                case Keys.Escape:
                    this.Close();
                    break;
            }
            return base.ProcessCmdKey(ref msg, keyData);
            enviarFormaPago();
        }

        private void formFormaPago_Load(object sender, EventArgs e)
        {
            label1.Select();
            label1.Focus();
        }
    }
}
