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
    public partial class formEgresosCajaVendedor : Form
    {
        protected Negocio.CierreCaja oCierreN = new Negocio.CierreCaja();
        protected Negocio.Sucursal oSucursalN = new Negocio.Sucursal();

        protected Entidades.EgresoCaja oEgresoCajaE = new Entidades.EgresoCaja();
        protected Entidades.Sucursal oSucursalE = new Entidades.Sucursal();

        public Entidades.CierreCaja oCierreE;

        public formEgresosCajaVendedor()
        {
            InitializeComponent();
        }

        private void formEgresosCajaVendedor_Load(object sender, EventArgs e)
        {
            this.Text += Utilidades.Conexion.getSucursalConexion();
            this.Text = "Egresos Caja "+oCierreE.UsuarioInicio.Nombre;
            grillaEgresosCaja.DataSource = oCierreN.getEgresosCajaVendedor(oCierreE);
            grillaEgresosCaja.Columns["Fecha"].DefaultCellStyle.Format = "dd/MM/yyyy HH:mm:ss";
            grillaEgresosCaja.Columns["Detalle"].AutoSizeMode = DataGridViewAutoSizeColumnMode.ColumnHeader;
            grillaEgresosCaja.Columns["Monto"].DefaultCellStyle.Format = "F2";
            grillaEgresosCaja.Columns["Creado"].DefaultCellStyle.Format = "dd/MM/yyyy HH:mm:ss";
            grillaEgresosCaja.Columns["Actualizado"].DefaultCellStyle.Format = "dd/MM/yyyy HH:mm:ss";

            decimal total = 0;
            foreach (DataGridViewRow row in grillaEgresosCaja.Rows)
            {
                total = total + Convert.ToDecimal(row.Cells["monto"].Value.ToString());
            }
            txtTotalS.Text = total.ToString("F2");
        }

        private void btnCancelar_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void grillaEgresosCaja_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            seleccionarEgresoCaja();
        }

        private void seleccionarEgresoCaja()
        {
            try
            {
                DataGridViewRow fila = grillaEgresosCaja.CurrentRow;

                if (fila != null)
                {
                    txtFechaTexto.Text = fila.Cells["Fecha"].Value.ToString();
                    txtTipoEgresoCaja.Text = fila.Cells["Tipo EgresoCaja"].Value.ToString();
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

        private void grillaEgresosCaja_SelectionChanged(object sender, EventArgs e)
        {
            seleccionarEgresoCaja();
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Escape)
            {
                this.Close();
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }
    }
}
