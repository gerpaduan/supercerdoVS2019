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
    public partial class formGastosVendedor : Form
    {
        protected Negocio.CierreCaja oCierreN = new Negocio.CierreCaja();
        protected Negocio.Sucursal oSucursalN = new Negocio.Sucursal();

        protected Entidades.Gasto oGastoE = new Entidades.Gasto();
        protected Entidades.Sucursal oSucursalE = new Entidades.Sucursal();

        public Entidades.CierreCaja oCierreE;

        public formGastosVendedor()
        {
            InitializeComponent();
        }

        private void formGastosVendedor_Load(object sender, EventArgs e)
        {
            this.Text = "Gastos "+oCierreE.UsuarioInicio.Nombre;
            grillaGastos.DataSource = oCierreN.getGastosVendedor(oCierreE);
        }

        private void btnCancelar_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void grillaGastos_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                DataGridViewRow fila = grillaGastos.CurrentRow;

                if (fila != null)
                {
                    txtFechaTexto.Text = fila.Cells["Fecha"].Value.ToString();
                    txtTipoGasto.Text = fila.Cells["Tipo Gasto"].Value.ToString();
                    txtDescripcion.Text = fila.Cells["Descripción"].Value.ToString();
                    txtMonto.Text = fila.Cells["Monto"].Value.ToString();
                    txtDetalle.Text = fila.Cells["Detalle"].Value.ToString();
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Error al seleccionar fila");
            }
        }
    }
}
