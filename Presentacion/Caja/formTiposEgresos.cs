using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Presentacion.Caja;
using System.Configuration;
using Presentacion.Usuario;
using Entidades;

namespace Presentacion.Caja
{
    public partial class formTiposEgresos : Form, InterfaceUsuario
    {
        Negocio.CierreCaja oCierreCajaN = new Negocio.CierreCaja();
        DataTable dtTiposEgreso = new DataTable();

        Entidades.Usuario oUsuario;
        bool cargar = false;
        int idTipoEgreso = -1;
        public formTiposEgresos()
        {
            InitializeComponent(); this.Icon = Properties.Resources.CarniSys_ICONO;        
        }

        public void cargarGrilla()
        {
            if (cargar)
            {
                try
                {
                    dtTiposEgreso = null;
                    grilla.DataSource = null;
                    grilla.AutoGenerateColumns = true;
                    dtTiposEgreso = oCierreCajaN.obtenerTiposEgresoCaja(txtDescripcion.Text.Trim(), 0);
                    grilla.DataSource = dtTiposEgreso;

                    formatearGrilla();
                    lblActualizar.Visible = false;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error al cargar la grilla.(Método: cargarGrilla()).\n\n" + ex.Message);
                }
            }
        }

        private void formatearGrilla()
        {
            grilla.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        }


        private void informacionTipoEgreso()
        {
            idTipoEgreso = (Convert.ToInt32(grilla.CurrentRow.Cells["ID"].Value.ToString()));
            addOrEditTipoEgreso(idTipoEgreso);
        }

        private void nuevo_Click(object sender, EventArgs e)
        {
            idTipoEgreso = -1;
            addOrEditTipoEgreso(idTipoEgreso);
        }

        private void addOrEditTipoEgreso(int idTipoEgreso)
        {
            if (idTipoEgreso != -1 && Convert.ToBoolean(grilla.CurrentRow.Cells["Reservado"].Value))
            {
                MessageBox.Show("El Tipo Egreso seleccionado es reservado por el sistema y no puede ser modificado/eliminado");
                return;
            }


            if (!Usuarios.FormValidarPermiso.validarPermiso()) return;

            if (Application.OpenForms["formAddOrEditTipoEgreso"] != null)
            {

                Application.OpenForms["formAddOrEditTipoEgreso"].Activate();
                Application.OpenForms["formAddOrEditTipoEgreso"].WindowState = FormWindowState.Normal;
            }
            else
            {
                formAddOrEditTipoEgreso formAddOrEditTipoEgreso = new formAddOrEditTipoEgreso();
                //formAddOrEditTipoEgreso.oUsuario = oUsuario; 
                formAddOrEditTipoEgreso.idTipoEgreso = idTipoEgreso;
                //formAddOrEditTipoEgreso.frmTipoEgresos = this;
                formAddOrEditTipoEgreso.ShowDialog();
                this.cargarGrilla();
            }
            oUsuario = null;
        }

        public void EnviarUsuario(Entidades.Usuario usuario)
        {
            oUsuario = usuario;
        }

        private void btnSalir_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnBuscaProd_Click(object sender, EventArgs e)
        {
            cargarGrilla();
        }

        private void grillaTipoEgresos_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            informacionTipoEgreso();
        }

        private void modificar_Click(object sender, EventArgs e)
        {
            informacionTipoEgreso();
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Escape)
            {
                this.Close();
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void txtDescripcion_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyValue.Equals(13))
            {
                cargarGrilla();
            }
        }

        private void comboSucursal_SelectedValueChanged(object sender, EventArgs e)
        {
            cargarGrilla();
        }

        private void grillaTipoEgresos_Sorted(object sender, EventArgs e)
        {
            formatearGrilla();
        }

        private void btnSeleccionar_Click(object sender, EventArgs e)
        {
            informacionTipoEgreso();
        }

        private void txtDescripcion_TextChanged(object sender, EventArgs e)
        {
            lblActualizar.Visible = true;
        }

        private void eliminar_Click(object sender, EventArgs e)
        {
            try
            {
                if (Convert.ToBoolean(grilla.CurrentRow.Cells["Reservado"].Value))
                {
                    MessageBox.Show("El Tipo Egreso seleccionado es reservado por el sistema y no puede eliminarse.");
                    return;
                }

                DialogResult respuesta = MessageBox.Show("¿Está seguro que desea eliminar el tipo egreso: "+ grilla.CurrentRow.Cells["tipoEgresoCaja"].Value.ToString().ToUpper()+"?. ", "Eliminar TipoEgreso", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);

                if (respuesta == System.Windows.Forms.DialogResult.Yes)
                {
                    FormLoginVendedor frmLogin = new FormLoginVendedor();
                    frmLogin.ShowDialog(this);

                    if (oUsuario != null && oUsuario.Admin)
                    {
                        oCierreCajaN.eliminarTipoEgreso(Convert.ToInt32(grilla.CurrentRow.Cells["id"].Value.ToString()));
                        MessageBox.Show("El Tipo Egreso se eliminó correctamente");
                        this.cargarGrilla();
                    }
                    else
                    {
                        MessageBox.Show("Debe tener permiso de Administrador para eliminar una TipoEgreso");
                    }
                    oUsuario = null;
                }
            }
            catch (Exception ex)
            {
                string msg = ex.Message.Contains("FK") ? "No se puede eliminar porque existen egresos de caja con el Tipo Egreso seleccionado.\n\n" : "";
                MessageBox.Show(msg + "Detalle del error: " + ex.Message);
            }
        }

        private void formTiposEgresos_Load(object sender, EventArgs e)
        {
            try
            {
                this.Text += Utilidades.Conexion.getSucursalConexion();
                cargar = true;
                cargarGrilla();
            }
            catch (Exception ex)
            {
                if (Utilidades.Util_Form.errorConexionBD_Return(ex.Message))
                    //formTipoEgresos_Load(null, null);

                this.Close();
            }
        }
    }
}
