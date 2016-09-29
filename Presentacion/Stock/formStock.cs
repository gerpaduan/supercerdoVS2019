using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Presentacion.Compras;
using Presentacion.Caja;

namespace Presentacion
{
    public partial class formStock : formBaseColor
    {
        Negocio.Compra oCompraN = new Negocio.Compra();
        DataTable dtCompras = new DataTable();
        public DataTable dtSucursales;
        public Negocio.Sucursal oSucursalN = new Negocio.Sucursal();

        bool cargar = false;
        public formStock()
        {
            InitializeComponent();
        }

        private void formStock_Load(object sender, EventArgs e)
        {
            this.Text += Utilidades.Conexion.getSucursalConexion();
            cargarSucursal();
            this.comboTipoCompra.SelectedIndex = 0;

            fechaDesde.Value = DateTime.Now.AddMonths(-2);
            cargar = true;
            cargarGrilla();
        }       

        #region metodos

        public void cargarGrilla()
        {
            if (cargar)
            {
                lblActualizar.Visible = false;
                grillaCompras.AutoGenerateColumns = false;

                dtCompras = null;
                dtCompras = oCompraN.obtenerCompras(Convert.ToInt32(comboSucursal.SelectedValue.ToString()), comboTipoCompra.Text, txtDescripcion.Text.Trim(), fechaDesde.Value.Date, fechaHasta.Value.Date, null);
                grillaCompras.DataSource = dtCompras;
                formatearGrilla();
                cargarTotales();
            }
        }

        private void formatearGrilla()
        {
            grillaCompras.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
            grillaCompras.Columns["observaciones"].AutoSizeMode = DataGridViewAutoSizeColumnMode.ColumnHeader;

            //formato para columna de fechas
            grillaCompras.Columns["fechaCompra"].DefaultCellStyle.Format = "dd/MM/yyyy HH:mm:ss";
            //grillaCompras.Columns["fechaCompra"].DefaultCellStyle.Format = "ddd dd MMM HH:mm:ss";
            grillaCompras.Columns["creado"].DefaultCellStyle.Format = "dd/MM/yyyy HH:mm:ss";
            grillaCompras.Columns["actualizado"].DefaultCellStyle.Format = "dd/MM/yyyy HH:mm:ss";
        }

        private void cargarTotales()
        {
            float totalKg=0, totalS=0;
            foreach (DataRow fila in dtCompras.Rows)
            {
                totalKg = totalKg + float.Parse( fila[6].ToString());
                totalS = totalS + float.Parse(fila["totalS"].ToString());
            }

            txtTotalKgs.Text = Convert.ToString( totalKg);
        }

        private void modificarCompra()
        {
            try
            {
                int idCompra = Convert.ToInt32(grillaCompras.CurrentRow.Cells["idCompra"].Value.ToString());
                bool formAbierto = false;
                foreach (Form frm in Application.OpenForms)
                {
                    if (frm.GetType() == typeof(formAddOrEditStock))
                    {
                        foreach (Control ctrl in frm.Controls)
                        {
                            if(ctrl.Name.Equals("idCompraLabel") && ctrl.Text.Equals(idCompra.ToString())) //(oUsuario != null && ctrl.Name.Equals("usuario") && ctrl.Text.Equals(oUsuario.User))
                            {
                                frm.BringToFront();
                                formAbierto = true;
                                break;
                            }
                        }
                    }
                }
                if (!formAbierto)
                {
                    formAddOrEditStock frmAddOrEditStock = new formAddOrEditStock();
                    frmAddOrEditStock.accion = Entidades.Compra.accion.Modificar;
                    frmAddOrEditStock.idCompra = idCompra;
                    frmAddOrEditStock.frmStock = this;
                    frmAddOrEditStock.Show();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        #endregion

        
        #region eventos

        private void actualizarLabel()
        {
            lblActualizar.Visible = true;
        }

        private void btnCancelar_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        
        private void fechaDesde_ValueChanged(object sender, EventArgs e)
        {
            actualizarLabel();
        }

        private void fechaHasta_ValueChanged(object sender, EventArgs e)
        {
            actualizarLabel();
        }
        private void txtDescripcion_TextChanged(object sender, EventArgs e)
        {
            actualizarLabel();
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
            comboSucursal.SelectedValue = Utilidades.Util_Form.idSucursalAppConfig();
        }

        private void btnIngreso_Click(object sender, EventArgs e)
        {
            nuevoStock(Entidades.Compra.tipoCompraEnum.IngresoStock);
        }

        private void btnEgreso_Click(object sender, EventArgs e)
        {
            nuevoStock(Entidades.Compra.tipoCompraEnum.EgresoStock);
        }

        private void btnCierre_Click(object sender, EventArgs e)
        {
            nuevoStock(Entidades.Compra.tipoCompraEnum.CierreStock);
        }

        private void nuevoStock(Entidades.Compra.tipoCompraEnum tipoCompra)
        {
            if (Application.OpenForms["formAddOrEditStock"] != null)
            {
                Application.OpenForms["formAddOrEditStock"].Activate();
                Application.OpenForms["formAddOrEditStock"].WindowState = FormWindowState.Normal;
                MessageBox.Show("Si desea modificar un registro diferente debe cerrar este formulario y volver a seleccionar el que desea modificar");
            }
            else
            {
                formAddOrEditStock frmAddOrEditStock = new formAddOrEditStock();
                frmAddOrEditStock.tipoCompraEnum = tipoCompra;
                frmAddOrEditStock.frmStock = this;
                frmAddOrEditStock.Show();
            }
        }

        private void fechaDesde_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyValue.Equals(13))
            {
                cargarGrilla();
            }
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
