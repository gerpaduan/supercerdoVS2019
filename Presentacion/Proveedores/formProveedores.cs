using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Presentacion
{
    public partial class formProveedores : formBaseColor
    {
        Negocio.Persona oProveedorN;

        public formProveedores()
        {
            InitializeComponent();
            cargarGrilla();
        }

#region Acciones

        private void nuevo_Click(object sender, EventArgs e)
        {
            agregarProveedor();
        }

       
        private void btnBuscar_Click(object sender, EventArgs e)
        {
            buscarProveedor();
        }

        private void txtBuscar_TextChanged(object sender, EventArgs e)
        {
            buscarProveedor();
        }

        private void btnCancelar_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        private void grillaProveedores_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            infoProveedor();
        }
        private void btnSeleccionar_Click(object sender, EventArgs e)
        {
            infoProveedor();
        }


#endregion

      
#region Métodos

         public void cargarGrilla()
        {
            oProveedorN = new Negocio.Persona();
            grillaProveedores.AutoGenerateColumns = false;

            grillaProveedores.DataSource = oProveedorN.buscarProveedor(txtBuscar.Text.Trim());

            oProveedorN = null;
        }

        public void buscarProveedor()
        {
            oProveedorN = new Negocio.Persona();
            string txtBusqueda = txtBuscar.Text.Trim();
            grillaProveedores.AutoGenerateColumns = false;
            grillaProveedores.DataSource = oProveedorN.buscarProveedor(txtBusqueda);
            oProveedorN = null;
        }

   

        public void agregarProveedor()
        {
            formNuevoProveedor frmNuevoProveedor = new formNuevoProveedor();
            frmNuevoProveedor.ShowDialog();
            cargarGrilla();
        }

        public void infoProveedor()
        {
          
            Entidades.Persona oProveedorE = new Entidades.Persona();
            cargarDatos(oProveedorE);

            formInfoProveedor frmInfoProveedor = new formInfoProveedor();
            frmInfoProveedor.cargarCampos(oProveedorE);
            frmInfoProveedor.ShowDialog();
            cargarGrilla();
           
            
        }

        private void cargarDatos(Entidades.Persona oProveedorE)
        {
            oProveedorE.idPersona =Convert.ToInt32( grillaProveedores.CurrentRow.Cells[0].Value.ToString());
            oProveedorE.razonSocial = grillaProveedores.CurrentRow.Cells[1].Value.ToString();
            oProveedorE.otrosDatos = grillaProveedores.CurrentRow.Cells[2].Value.ToString();

        }


#endregion

        
        

        
       

        

       
    }
}
