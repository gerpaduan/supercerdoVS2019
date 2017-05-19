using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Presentacion.Stock
{
    public partial class FormVerPorcCortes : Form
    {
        public int idCompra = 0;

        Negocio.Compra oCompraN = new Negocio.Compra();

        public FormVerPorcCortes()
        {
            InitializeComponent();
        }

        private void FormVerPorcCortes_Load(object sender, EventArgs e)
        {
            try
            {
                grillaPromMedias.DataSource = oCompraN.getPromMedias(idCompra);
                grillaPorcCortes.DataSource = oCompraN.getPorcCortesEnMedias(idCompra);

                for (int colum = 2; colum < grillaPorcCortes.Columns.Count; colum++)
                {
                    grillaPorcCortes.Columns[colum].DefaultCellStyle.Format = "F3";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
