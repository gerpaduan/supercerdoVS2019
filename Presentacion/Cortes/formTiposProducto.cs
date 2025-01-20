using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Presentacion.Cortes;
using System.Configuration;
using Presentacion.Usuario;
using Entidades;
using Presentacion.Caja;
using System.Web.Services.Description;

namespace Presentacion.Cortes
{
    public partial class formTiposProducto : Form, InterfaceUsuario
    {
        Negocio.Corte oCorteN = new Negocio.Corte();
        DataTable dtTiposProducto = new DataTable();

        Entidades.Usuario oUsuario;
        bool cargar = false;
        bool posibleModificaciones = false;
        public formTiposProducto()
        {
            InitializeComponent(); this.Icon = Properties.Resources.CarniSys_ICONO;        
        }

        public void cargarGrilla()
        {
            if (cargar)
            {
                try
                {
                    dtTiposProducto = null;
                    grilla.DataSource = null;
                    grilla.AutoGenerateColumns = true;
                    dtTiposProducto = oCorteN.obtenerTiposProductoGrilla(txtDescripcion.Text.Trim());
                    grilla.DataSource = dtTiposProducto;

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


        private void informacionTipoProducto(bool esInsert)
        {
            addOrEditTipoProducto(esInsert);
        }

        private void nuevo_Click(object sender, EventArgs e)
        {
            addOrEditTipoProducto(true);
        }

        private void addOrEditTipoProducto(bool esInsert)
        {
            if (!esInsert && Convert.ToBoolean(grilla.CurrentRow.Cells["Reservado"].Value))
            {
                MessageBox.Show("El Tipo seleccionado es reservado por el sistema y no puede ser modificado/eliminado");
                return;
            }

            if (!Usuarios.FormValidarPermiso.validarPermiso()) return;

            formAddOrEditTipoProducto frmAddOrEditTipoProducto = new formAddOrEditTipoProducto();
            frmAddOrEditTipoProducto.tipoProductoSelected  = !esInsert ? grilla.CurrentRow.Cells["tipo"].Value.ToString() :"";
            frmAddOrEditTipoProducto.ordenSelected = !esInsert ? grilla.CurrentRow.Cells["orden"].Value.ToString() : "100";
            frmAddOrEditTipoProducto.esInsert = esInsert;
            frmAddOrEditTipoProducto.ShowDialog();
            this.cargarGrilla();

            posibleModificaciones = true;
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

        protected override bool ProcessCmdKey(ref System.Windows.Forms.Message msg, Keys keyData)
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

        private void grillaTipoProductos_Sorted(object sender, EventArgs e)
        {
            formatearGrilla();
        }

        private void btnSeleccionar_Click(object sender, EventArgs e)
        {
            informacionTipoProducto(false);//false xq no es Insert
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
                    MessageBox.Show("El Tipo Producto seleccionado es reservado por el sistema y no puede eliminarse.");
                    return;
                }

                DialogResult respuesta = MessageBox.Show("¿Está seguro que desea eliminar el Tipo Producto: "+ grilla.CurrentRow.Cells["tipo"].Value.ToString().ToUpper()+"?. ", "Eliminar TipoProducto", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);

                if (respuesta == System.Windows.Forms.DialogResult.Yes)
                {
                    FormLoginVendedor frmLogin = new FormLoginVendedor();
                    frmLogin.ShowDialog(this);

                    if (oUsuario != null && oUsuario.Admin)
                    {
                        string mensaje = oCorteN.eliminarTipoProducto(grilla.CurrentRow.Cells["tipo"].Value.ToString());

                        if (mensaje.Length > 0)
                        {
                            MessageBox.Show(mensaje, "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }

                        MessageBox.Show("El Tipo Producto se eliminó correctamente");
                        this.cargarGrilla();
                        posibleModificaciones = true;
                    }
                    else
                    {
                        MessageBox.Show("Debe tener permiso de Administrador para eliminar una TipoProducto");
                    }
                    oUsuario = null;
                }
            }
            catch (Exception ex)
            {
                string msg = ex.Message.Contains("FK") ? "No se puede eliminar porque existen Productos/Cortes con el Tipo Producto seleccionado.\n\n" : "";
                MessageBox.Show(msg + "Detalle del error: " + ex.Message);
            }
        }

        private void formTiposProducto_Load(object sender, EventArgs e)
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
                    //formTipoProductos_Load(null, null);

                this.Close();
            }
        }

        private void formTiposProducto_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (posibleModificaciones)
                MessageBox.Show("Si realizó modificaciones en los tipos de productos.\nPara una correcta visualización se recomienda cerrar y volver a abrir el formulario Cortes");
        }
    }
}
