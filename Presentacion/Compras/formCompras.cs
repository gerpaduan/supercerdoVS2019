using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Presentacion.Compras;

namespace Presentacion
{
    public partial class formCompras : formBaseColor
    {
        Negocio.Compra oCompraN;
        DataTable dtCompras = new DataTable();

        public DataTable dtSucursales;
        public Negocio.Sucursal oSucursalN = new Negocio.Sucursal();

        bool cargar = false;
        public formCompras()
        {
            InitializeComponent();
        }
        
        private void formCompras_Load(object sender, EventArgs e)
        {
            try
            {
                this.Text += Utilidades.Conexion.getSucursalConexion();
                cargarSucursal();
                this.comboTipoCompra.SelectedIndex = 0;
                fechaDesde.Value = DateTime.Today.AddMonths(-2);
                cargar = true;
                cargarGrilla();
            }
            catch (Exception ex)
            {
                if (Utilidades.Util_Form.errorConexionBD_Return(ex.Message))
                    formCompras_Load(null, null);

                this.Close();
            }
        }
      
        #region metodos

        public void cargarGrilla()
        {            
            if (cargar)
            {
                int idSucCombo = 0;
                if (comboSucursal.SelectedValue != null)
	            {
                    idSucCombo = Convert.ToInt32(comboSucursal.SelectedValue);
	            }
                oCompraN = new Negocio.Compra();

                grillaCompras.AutoGenerateColumns = false;

                dtCompras = null;
                dtCompras = oCompraN.obtenerCompras(idSucCombo, comboTipoCompra.Text, txtDescripcion.Text.Trim(), fechaDesde.Value.Date, fechaHasta.Value.Date, null);
                grillaCompras.DataSource = dtCompras;

                cargarTotales();
                oCompraN = null;
            }
        }

        private void cargarTotales()
        {
            float totalKg = 0, totalS = 0;
            int cantMedias = 0;
            foreach (DataRow fila in dtCompras.Rows)
            {
                cantMedias += string.IsNullOrEmpty(fila["cantMedias"].ToString()) ? 0 : Convert.ToInt32(fila["cantMedias"]);
                totalKg = totalKg + float.Parse(fila["cantKg"].ToString());
                totalS = totalS + float.Parse(fila["totalS"].ToString());
            }
            txtCantMedias.Text = cantMedias.ToString();
            txtTotalKgs.Text = totalKg.ToString("F3");
            txtTotalS.Text = totalS.ToString("F2");
        }

        private void modificarCompra()
        {
            try
            {
                int idCompra = Convert.ToInt32(grillaCompras.CurrentRow.Cells["idCompra"].Value.ToString());

                if (Application.OpenForms["formModificarCompra"] != null)
                {
                    Application.OpenForms["formModificarCompra"].Activate();
                    Application.OpenForms["formModificarCompra"].WindowState = FormWindowState.Normal;
                }
                else
                {
                    formModificarCompra frmModificarCompra = new formModificarCompra();
                    frmModificarCompra.cargarParametros(this, idCompra);
                    frmModificarCompra.Show();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void nuevaCompra()
        {
            if (Application.OpenForms["formNuevaCompra"] != null)
            {

                Application.OpenForms["formNuevaCompra"].Activate();
                Application.OpenForms["formNuevaCompra"].WindowState = FormWindowState.Normal;
            }
            else
            {
                formNuevaCompra frmNuevaCompra = new formNuevaCompra();
                frmNuevaCompra.asignarFormCompra(this);
                frmNuevaCompra.Show();
            }        
        }

        #endregion
        
        #region eventos

        private void btnCancelar_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void nuevo_Click(object sender, EventArgs e)
        {
            nuevaCompra();
        }

        private void fechaDesde_ValueChanged(object sender, EventArgs e)
        {
            cargarGrilla();
        }

        private void fechaHasta_ValueChanged(object sender, EventArgs e)
        {
            cargarGrilla();
        }
        private void txtDescripcion_TextChanged(object sender, EventArgs e)
        {
            cargarGrilla();
        }
        
        private void btnSeleccionar_Click_1(object sender, EventArgs e)
        {
            modificarCompra();
        }
        private void grillaCompras_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            modificarCompra();
        }
        #endregion

        private void btnBuscar_Click(object sender, EventArgs e)
        {
            cargarGrilla();
        }

        private void comboTipoCompra_SelectedValueChanged(object sender, EventArgs e)
        {
            cargarGrilla();
        }

        private void formCompras_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode==Keys.N)
            {
                nuevaCompra();
            }
        }

        private void comboSucursal_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!comboSucursal.ValueMember.Equals(""))
            {
                cargarGrilla();
            }           
        }

        private void cargarSucursal()
        {
            dtSucursales = new DataTable();
            oSucursalN = new Negocio.Sucursal();
            dtSucursales = oSucursalN.obtenerSucursalesConTodas();

            comboSucursal.DataSource = dtSucursales;
            comboSucursal.DisplayMember = "sucursal";
            comboSucursal.ValueMember = "idSucursal";
            comboSucursal.SelectedValue = -1;
        }
    }
}
