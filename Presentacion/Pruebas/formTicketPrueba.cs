using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Presentacion.Pruebas
{
    public partial class formTicketPrueba : Form
    {
        public formTicketPrueba()
        {
            InitializeComponent(); this.Icon = Properties.Resources.CarniSys_ICONO;
        }

        private void btnImprimir_Click(object sender, EventArgs e)
        {
            try
            {
                Ticket.CreaTicket ticket = new Ticket.CreaTicket();
                ticket.imprimir = true;
                ticket.LineasEnBlanco(2);
                ticket.Negrita(true);
                ticket.TextoExtremos("Fecha: " + DateTime.Now.Date.ToShortDateString(), "Hora: " + DateTime.Now.ToShortTimeString());
                ticket.Negrita(false);
                ticket.LineasEnBlanco(1);
                ticket.TextoCentro(lblTitulo.Text);
                ticket.FuenteB(false);
                ticket.LineasEnBlanco(1);
                ticket.DobleTamanoA();
                ticket.TextoIzquierda(txtIngreseTexto.Text);
                ticket.DobleTamanoA(false);
                ticket.TextoCentro("Fin Prueba!");
                ticket.LineasEnBlanco(5);
                ticket.realizarImpresion();
                ticket.CortaTicket();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Sucedió un error al imprimir.\nVerifique que la impresora esté prendida y conectada" +
                    "a la computadora\n\nMas destalle del error:\n" + ex.Message);
            }
        }
    }
}
