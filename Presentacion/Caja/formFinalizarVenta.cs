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
    public partial class formFinalizarVenta : Form
    {
        public Entidades.Venta oVentaE;
        Entidades.Venta.imprimirCbteEnum imprimirCbte = Entidades.Venta.imprimirCbteEnum.Nulo;
        bool esEfectivo;
        
        public formFinalizarVenta()
        {
            InitializeComponent();
        }

        public void enviarImprimirCbte()
        {
            InterfaceImprimirCbte formInterface = this.Owner as InterfaceImprimirCbte;
            if (formInterface != null)
            {
                formInterface.EnviarImprimirCbte(this.imprimirCbte);
            }
            this.Close();
        }
        
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            unfocused();            
            imprimirCbte = Entidades.Venta.imprimirCbteEnum.Nulo;
            switch (keyData)
            {
                    //Salir
                case Keys.NumPad0:
                    this.Close();
                    break;
                    //Sin Ticket
                case Keys.NumPad1:
                    if (!esEfectivo) break;//Si no es efectivo no se permite hacer foco en el boton
                    btnSinTicket.BackColor = Utilidades.Util_Form.checkedColor;
                    imprimirCbte = Entidades.Venta.imprimirCbteEnum.SinTicket;
                    btnSinTicket.Focus();
                    break;
                    //Ticket
                case Keys.NumPad2:
                    if (!esEfectivo) break;//Si no es efectivo no se permite hacer foco en el boton
                    btnTicket.BackColor = Utilidades.Util_Form.checkedColor;
                    imprimirCbte = Entidades.Venta.imprimirCbteEnum.Ticket;
                    btnTicket.Focus();
                    break;
                    //Factura
                case Keys.NumPad3:
                    btnFactura.BackColor = Utilidades.Util_Form.checkedColor;
                    imprimirCbte = Entidades.Venta.imprimirCbteEnum.Factura;
                    btnFactura.Focus();
                    break;
                case Keys.Enter:
                    if (!(imprimirCbte == Entidades.Venta.imprimirCbteEnum.Nulo))
                        enviarImprimirCbte();
                    break;
                case Keys.Escape:
                    this.Close();
                    break;
                default:
                    //imprimirCbte = Entidades.Venta.imprimirCbteEnum.Nulo;
                    if (!(keyData.Equals(Keys.Left) || keyData.Equals(Keys.Right) || 
                        keyData.Equals(Keys.Up) || keyData.Equals(Keys.Down)))
                    {
                        imprimirCbte = Entidades.Venta.imprimirCbteEnum.Nulo;
                        label1.Focus();
                        label1.Select();
                    }
                    unfocused();
                    break;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void formFinalizarVenta_Load(object sender, EventArgs e)
        {
            esEfectivo = oVentaE.FormaPago.Equals(Entidades.Venta.formaPagoEnum.Efectivo.ToString());

            if (!esEfectivo)
	        {
                btnSinTicket.Enabled = false;
                btnTicket.Enabled = false;                
	        }
            lblFormaPago.Text = "-" + oVentaE.FormaPago +"-";
            lblFormaPago.AutoSize = false;
            lblFormaPago.Left = (this.ClientSize.Width / 2) - (lblFormaPago.Width / 2);
            label1.Select();
            label1.Focus();
        }
        //Sin-Ticket
        private void btnEfectivo_Enter(object sender, EventArgs e)
        {
            if (!esEfectivo) return;//Si no es efectivo no se permite hacer foco en el boton
            unfocused();
            btnSinTicket.BackColor = Utilidades.Util_Form.checkedColor;
        }
        //Ticket
        private void btnDebito_Enter(object sender, EventArgs e)
        {
            if (!esEfectivo) return;//Si no es efectivo no se permite hacer foco en el boton
            unfocused();
            btnTicket.BackColor = Utilidades.Util_Form.checkedColor;
        }
        //Factura
        private void btnCredito_Enter(object sender, EventArgs e)
        {
            unfocused();
            btnFactura.BackColor = Utilidades.Util_Form.checkedColor;
        }

        private void unfocused()
        {            
            btnSinTicket.BackColor = Utilidades.Util_Form.readOnlyColor;
            btnTicket.BackColor = Utilidades.Util_Form.readOnlyColor;
            btnFactura.BackColor = Utilidades.Util_Form.readOnlyColor;
        }

        private void btnSinTicket_Click(object sender, EventArgs e)
        {
            if (!esEfectivo) return;//Si no es efectivo no se permite hacer foco en el boton
            imprimirCbte = Entidades.Venta.imprimirCbteEnum.SinTicket;
            enviarImprimirCbte();
        }

        private void btnTicket_Click(object sender, EventArgs e)
        {
            if (!esEfectivo) return;//Si no es efectivo no se permite hacer foco en el boton
            imprimirCbte = Entidades.Venta.imprimirCbteEnum.Ticket;
            enviarImprimirCbte();
        }

        private void btnFactura_Click(object sender, EventArgs e)
        {
            imprimirCbte = Entidades.Venta.imprimirCbteEnum.Factura;
            enviarImprimirCbte();
        }

        private void btnSalir_Click(object sender, EventArgs e)
        {
            imprimirCbte = Entidades.Venta.imprimirCbteEnum.Nulo;
            this.Close();
        }
    }
}
