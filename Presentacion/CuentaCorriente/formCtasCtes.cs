using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Presentacion.CuentaCorriente
{
    public partial class formCtasCtes : Form
    {
        public formCtasCtes()
        {
            InitializeComponent();
        }

        private void formCtasCtes_Load(object sender, EventArgs e)
        {

        }

        private void cargarGrilla()
        { 
        }

        private void txtDescripcion_TextChanged(object sender, EventArgs e)
        {

        }

        private void btnBuscar_Click(object sender, EventArgs e)
        {

        }

        private void btnSeleccionar_Click(object sender, EventArgs e)
        {

        }

        private void btnCancelar_Click(object sender, EventArgs e)
        {

        }

        private void txtDescripcion_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyValue.Equals(13))
            {
                cargarGrilla();
            }
        }
    }
}
