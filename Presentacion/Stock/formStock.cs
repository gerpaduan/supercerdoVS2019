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
    public partial class formStock : formBaseColor

    {
        Negocio.Compra oCompraN;
        DataTable dtCompras = new DataTable();
        public DataTable dtSucursales;
        public Negocio.Sucursal oSucursalN = new Negocio.Sucursal();

        bool cargar = false;
        public formStock()
        {
            InitializeComponent();
            cargarSucursal();
            this.comboSucursal.SelectedIndex = 2;
            this.comboTipoCompra.SelectedIndex = 0 ;

            fechaDesde.Value = DateTime.Now.AddMonths(-2);
            cargar = true;
            cargarGrilla();
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
                dtCompras = oCompraN.obtenerCompras(idSucCombo, comboTipoCompra.Text, txtDescripcion.Text.Trim(), fechaDesde.Value.Date, fechaHasta.Value.Date);
                grillaCompras.DataSource = dtCompras;
                cargarTotales();

                oCompraN = null;
            }
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

        private void cargarCompra()
        { 
            
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
                    frmAddOrEditStock.asignarFormCompra(this);
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

        private void btnCancelar_Click(object sender, EventArgs e)
        {
            this.Close();
        }
      

        private void btnSeleccionar_Click(object sender, EventArgs e)
        {

        }
        
        private void modificar_Click(object sender, EventArgs e)
        {

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
        }

        private void nuevo_Click(object sender, EventArgs e)
        {

        }

        private void pnlBuscar_Paint(object sender, PaintEventArgs e)
        {

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
            dtSucursales = oSucursalN.obtenerSucursales();

            DataRow nuevaFila = dtSucursales.NewRow();

            nuevaFila[0] = 0;
            nuevaFila[1] = "Todas";

            dtSucursales.Rows.Add(nuevaFila);

            comboSucursal.DataSource = dtSucursales;
            comboSucursal.DisplayMember = "sucursal";
            comboSucursal.ValueMember = "idSucursal";
            comboSucursal.SelectedIndex = 2;
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
                frmAddOrEditStock.asignarFormCompra(this);
                frmAddOrEditStock.Show();
            }
        }       
    }
}
