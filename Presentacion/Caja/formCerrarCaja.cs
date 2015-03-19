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
    public partial class formCerrarCaja : Form
    {
        public formCerrarCaja()
        {
            InitializeComponent();
        }

        private void formCerrarCaja_Load(object sender, EventArgs e)
        {
            txtCajaInicial.Focus();
        }

        private void btnCerrarCaja_Click(object sender, EventArgs e)
        {

        }
    }
}
