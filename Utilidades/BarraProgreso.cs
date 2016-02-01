using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Utilidades
{
    public partial class BarraProgreso : Form
    {
        int count = 0;
        public BarraProgreso(string nombreVendedor)
        {
            InitializeComponent();
            lblVendedor.Text = nombreVendedor;
        }

        private void BarraProgreso_Load(object sender, EventArgs e)
        {
            this.timer1.Start();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            count += timer1.Interval;
            this.progressBar1.Increment(50);
            if (count == 350)
            {
                this.timer1.Stop();
                this.Close();
            }
        }
    }
}
