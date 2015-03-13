using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Presentacion.Ventas
{
    public partial class formVentaCaja2 : Presentacion.Ventas.formVentaCaja
    {
        public formVentaCaja2()
        {
            InitializeComponent();
            this.pnlBuscar.BackColor = System.Drawing.Color.Teal;
            this.grupoCortes.BackColor = System.Drawing.Color.Teal;
            //this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            int x = Screen.PrimaryScreen.WorkingArea.Width / 9;
            int y = Screen.PrimaryScreen.WorkingArea.Height / 36;
            //MessageBox.Show("x:"+x.ToString()+"\nX:" +this.Location.X.ToString() + "   Y: " + this.Location.Y.ToString());
            
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Location = new System.Drawing.Point(x, y);
        }
        
    }
}
