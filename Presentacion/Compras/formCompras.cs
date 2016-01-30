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
        Entidades.Compra oCompraE;
        DataTable dtCompras = new DataTable();

        public DataTable dtSucursales;
        public Negocio.Sucursal oSucursalN = new Negocio.Sucursal();

        bool cargar = false;
        public formCompras()
        {
            InitializeComponent();
            cargarSucursal();
            this.comboSucursal.SelectedIndex = 2;
            this.comboTipoCompra.SelectedIndex = 0;
            fechaDesde.Value = DateTime.Today.AddMonths(-2);
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
            txtTotalS.Text = Convert.ToString( totalS);
        }

        private void cargarCompra()
        { 
            
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
            //formNuevaCompra frmNuevaCompra = new formNuevaCompra();
            //frmNuevaCompra.asignarFormCompra(this);
            //frmNuevaCompra.ShowDialog();

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

        private void formCompras_Load(object sender, EventArgs e)
        {
            //cargarSucursal();
        }

      
        //cargar grilla del formModificarCorte
        

      

        

        

       

        

        

      

        

       
    }
}
