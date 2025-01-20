using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Presentacion.Usuarios
{
    public partial class formSelectUser : Form
    {

        Negocio.Usuario oUsuarioN = new Negocio.Usuario();
        Entidades.Usuario oUsuarioE = new Entidades.Usuario();

        public formSelectUser()
        {
            InitializeComponent(); this.Icon = Properties.Resources.CarniSys_ICONO;
        }

        private void formSelectUser_Load(object sender, EventArgs e)
        {
            try
            {
                this.Text += Utilidades.Conexion.getSucursalConexion();

                grillaUsuarios.DataSource = oUsuarioN.getUsuarioActivos();

                grillaUsuarios.Columns["nombre"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                grillaUsuarios.Columns["nombre"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                grillaUsuarios.Columns["usuario"].Visible = false;
                grillaUsuarios.Columns["clave"].Visible = false;

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public void enviarUsuario()
        {
            InterfaceUsuario formInterface = this.Owner as InterfaceUsuario;
            if (formInterface != null)
            {
                formInterface.EnviarUsuario(oUsuarioE);
            }
            this.Close();
        }

        private void ingresar()
        {
            oUsuarioE = oUsuarioN.validarUsuario(grillaUsuarios.CurrentRow.Cells["usuario"].Value.ToString(),
                grillaUsuarios.CurrentRow.Cells["clave"].Value.ToString(), false);
            if (oUsuarioE != null)
            {
                enviarUsuario();
            }
            else
            {
                MessageBox.Show("Contraseña incorrecta.", "Error Login", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void grillaUsuarios_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            // Ignore clicks that are not on button cells.  
            //if (e.RowIndex < 0 || e.ColumnIndex !=
            //    grillaUsuarios.Columns["nombre"].Index) return;

            // Retrieve the Employee object from the "Assigned To" cell.
            //string usuario = grillaUsuarios.Rows[e.RowIndex].Cells["usuario"].Value.ToString();

            ingresar();
        }
    }
}
