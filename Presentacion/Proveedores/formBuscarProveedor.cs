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
    public partial class formBuscarProveedor : formBaseColor
    {
        Negocio.Persona oProveedorN;
        
        public formBuscarProveedor()
        {
            InitializeComponent(); this.Icon = Properties.Resources.CarniSys_ICONO;
            cargarGrilla();
        }

        public void cargarGrilla()
        {
            oProveedorN = new Negocio.Persona();
            grillaProveedores.AutoGenerateColumns = false;

            grillaProveedores.DataSource = oProveedorN.buscarPersona(txtBuscar.Text.Trim());

            oProveedorN = null;
        }

        public void buscarProveedor()
        {
            oProveedorN = new Negocio.Persona();
            string txtBusqueda = txtBuscar.Text.Trim();
            grillaProveedores.AutoGenerateColumns = false;
            grillaProveedores.DataSource = oProveedorN.buscarPersona(txtBusqueda);
            oProveedorN = null;
        }

        private void txtBuscar_TextChanged(object sender, EventArgs e)
        {
            buscarProveedor();
        }

        private void grillaProveedores_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            enviarProveedor();
        }

        private void btnSeleccionar_Click(object sender, EventArgs e)
        {
            enviarProveedor();
        }

        //public void seleccionarProveedor()
        //{ 
        //    this.ShowDialog();

        //}

        //public Entidades.Proveedor enviarProveedor()
        //{
        //    Entidades.Proveedor oProveedorE = new Entidades.Proveedor();
        //    if (activo)
        //    {
                
        //        cargarDatos(oProveedorE);
                
        //        return oProveedorE;    
        //    }
        //    else
        //    {
        //        return oProveedorE = null;
        //    }
            
        //}

        public void enviarProveedor()
        {
            Entidades.Persona oProveedorE = new Entidades.Persona();
       
            cargarDatos(oProveedorE);

            InterfaceProveedor formInterface = this.Owner as InterfaceProveedor;
            if (formInterface !=null)
            {
                formInterface.EnviarProveedor(oProveedorE);
            }
            this.Close();
        }

        private void cargarDatos(Entidades.Persona oProveedorE)
        {
            oProveedorE.idPersona = Convert.ToInt32(grillaProveedores.CurrentRow.Cells[0].Value.ToString());
            oProveedorE.razonSocial = grillaProveedores.CurrentRow.Cells[1].Value.ToString();
            oProveedorE.otrosDatos = grillaProveedores.CurrentRow.Cells[2].Value.ToString();

        }

        private void btnCancelar_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        
    }
}
