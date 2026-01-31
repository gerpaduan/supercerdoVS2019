using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Presentacion.Personas;

namespace Presentacion.Cortes
{
    public partial class formMarcas : Form
    {
        Negocio.Persona oPersonaN;
        public bool buscardorMarcas = false;
        public formMarcas()
        {
            InitializeComponent(); this.Icon = Properties.Resources.CarniSys_ICONO;
            cargarGrilla();
        }


        #region Métodos

        public void cargarGrilla()
        {
            oPersonaN = new Negocio.Persona(FormPrincipal.EmpresaSTATIC, FormPrincipal.ParametrosCTX);
            string txtBusqueda = txtBuscar.Text.Trim();
            grillaPersonas.DataSource = null;
            grillaPersonas.AutoGenerateColumns = true;
            grillaPersonas.DataSource = oPersonaN.buscarPersona(txtBusqueda, true);
            grillaPersonas.Columns["idPersona"].AutoSizeMode = DataGridViewAutoSizeColumnMode.ColumnHeader;
            grillaPersonas.Columns["Marca"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            grillaPersonas.Columns["otrosDatos"].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            grillaPersonas.Columns["Propietario"].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            grillaPersonas.Columns["cuit"].AutoSizeMode = DataGridViewAutoSizeColumnMode.ColumnHeader;
            grillaPersonas.Columns["telefono"].AutoSizeMode = DataGridViewAutoSizeColumnMode.ColumnHeader;
            grillaPersonas.Columns["domicilio"].AutoSizeMode = DataGridViewAutoSizeColumnMode.ColumnHeader;
            grillaPersonas.Columns["ciudad"].AutoSizeMode = DataGridViewAutoSizeColumnMode.ColumnHeader;
            oPersonaN = null;

            //p.idPersona,
            //p.razonSocial as Marca,
            //p.otrosDatos AS otrosDatos,
            //prop.razonSocial AS Propietario,
            //prop.cuit AS cuit,
            //prop.telefono AS telefono,
            //prop.domicilio AS domicilio,
            //prop.ciudad AS ciudad

            grillaPersonas.Columns["otrosDatos"].Visible = !buscardorMarcas;
            grillaPersonas.Columns["Propietario"].Visible = !buscardorMarcas;
            grillaPersonas.Columns["idPersona"].Visible = false;
            grillaPersonas.Columns["cuit"].Visible = false;
            grillaPersonas.Columns["telefono"].Visible = false;
            grillaPersonas.Columns["domicilio"].Visible = false;
            grillaPersonas.Columns["ciudad"].Visible = false;

            grillaPersonas.ClearSelection();
        }


        public void agregarPersona()
        {
            formMarcaAddOrEdit frmMarcaAddOrEdit = new formMarcaAddOrEdit();
            //frmNuevaPersona.obtenerParametros(this);
            frmMarcaAddOrEdit.ShowDialog();            
        }

        public void infoPersona()
        {
            try
            {
                int idPersona = Convert.ToInt32(grillaPersonas.CurrentRow.Cells["idPersona"].Value.ToString());
                formMarcaAddOrEdit frmMarcaAddOrEdit = new formMarcaAddOrEdit();
                frmMarcaAddOrEdit.idPersona = idPersona;
                frmMarcaAddOrEdit.frmMarcas = this;
                frmMarcaAddOrEdit.ShowDialog();
                cargarGrilla();
            }
            catch (Exception)
            {
                MessageBox.Show("Hubo un error al seleccionar la fila");
            }
        }

        private void cargarDatos(Entidades.Persona oPersonaE)
        {
            oPersonaE.idPersona = Convert.ToInt32(grillaPersonas.CurrentRow.Cells["idPersona"].Value.ToString());
            oPersonaE.razonSocial = grillaPersonas.CurrentRow.Cells["razonSocial"].Value.ToString();
            oPersonaE.otrosDatos = grillaPersonas.CurrentRow.Cells["otrosDatos"].Value.ToString();
            oPersonaE.tipo = grillaPersonas.CurrentRow.Cells["tipo"].Value.ToString();
        }


        #endregion


        #region Acciones

        private void nuevo_Click(object sender, EventArgs e)
        {
            agregarPersona();
        }


        private void btnBuscar_Click(object sender, EventArgs e)
        {
            cargarGrilla();
        }

        private void txtBuscar_TextChanged(object sender, EventArgs e)
        {
            cargarGrilla();
        }

        private void btnCancelar_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnBuscar_Click_1(object sender, EventArgs e)
        {
            cargarGrilla();
        }
        private void btnSeleccionar_Click(object sender, EventArgs e)
        {
            if (buscardorMarcas)
                enviarPersona();
            else
                infoPersona();
        }

        //enviar Marca
        public void enviarPersona()
        {
            Entidades.Persona oMarcaE = new Entidades.Persona();
            try
            {
                if (grillaPersonas.Rows.Count == 0)
                    return;

                int idPersona = Convert.ToInt32(grillaPersonas.CurrentRow.Cells[0].Value.ToString());
                oPersonaN = new Negocio.Persona(FormPrincipal.EmpresaSTATIC, FormPrincipal.ParametrosCTX);
                oMarcaE = oPersonaN.findById(idPersona);

                InterfacePersona formInterface = this.Owner as InterfacePersona;
                if (formInterface != null)
                {
                    formInterface.EnviarPersona(oMarcaE);
                }
                this.Close();
            }
            catch (Exception)
            {
                MessageBox.Show("No se seleccionó ninguna Marca.\n\nSeleccione una Marca o presione Cerrar para no seleccionar.");
            }
        }

        private void grillaPersonas_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (buscardorMarcas)
                enviarPersona();
            else
                infoPersona();
        }
        #endregion

        private void btnCancelar_Click_1(object sender, EventArgs e)
        {
            this.Close();
        }

        private void formPersonas_Load(object sender, EventArgs e)
        {
            this.Text += Utilidades.Conexion.getSucursalConexion();

        }
    }
}
