using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Configuration;

namespace Presentacion.Ventas
{
    public partial class formVentaCaja2 : Presentacion.Ventas.formVentaCaja
    {
        public formVentaCaja2()
        {
            InitializeComponent();

            txtVendedor.Text = ConfigurationManager.AppSettings["formVentaCaja2"].ToString();
            this.pnlBuscar.BackColor = System.Drawing.Color.Teal;
            this.grupoCortes.BackColor = System.Drawing.Color.Teal;
            int x = Screen.PrimaryScreen.WorkingArea.Width / 9;
            int y = Screen.PrimaryScreen.WorkingArea.Height / 36;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Location = new System.Drawing.Point(x, y);
        }
        
    }
}
