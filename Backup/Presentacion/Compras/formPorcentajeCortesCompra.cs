using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Presentacion.Compras
{
    public partial class formPorcentajeCortesCompra : Form
    {
        Negocio.Compra oCompraN = new Negocio.Compra();
        public formPorcentajeCortesCompra(int idCompra)
        {
            InitializeComponent();
            cargarGrilla(idCompra);
        }

        private void cargarGrilla(int idCompra)
        {
            grillaPorcentajePorCorte.DataSource = null;
            grillaPorcentajePorCorte.DataSource = oCompraN.porcentajeCortesPorCompra(idCompra);
        }

        private void btnSalir_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        
    }
}
