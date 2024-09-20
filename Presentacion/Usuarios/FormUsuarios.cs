using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Presentacion.Usuario
{
    public partial class FormUsuarios : Form
    {
        Negocio.Usuario oUsuarioN = new Negocio.Usuario();
        Entidades.Usuario oUsuarioE = new Entidades.Usuario();

        public FormUsuarios()
        {
            InitializeComponent();
        }

        private void FormLoginVendedor_Load(object sender, EventArgs e)
        {
            this.Text += Utilidades.Conexion.getSucursalConexion();
            cargarCombo();
            if (FormPrincipal.logueado)
	        {
                txtNombre.ReadOnly = false;
                checkAdmin.Enabled = true;
                checkActivo.Enabled = true;
                btnGuardarDatos.Enabled = true;
                btnNuevoUsuario.Enabled = true; 
                checkOlvidoClave.Enabled = true;
	        }
        }

        private void cargarCombo()
        {
            comboUsuario.DataSource = oUsuarioN.obtenerUsuarios(false);
            comboUsuario.DisplayMember = "usuario";
            comboUsuario.ValueMember = "usuario";
        }

        private void comboUsuario_SelectedValueChanged(object sender, EventArgs e)
        {
            oUsuarioE = comboUsuario.SelectedValue != null ? oUsuarioN.getUser(comboUsuario.SelectedValue.ToString()) : null;
            if (oUsuarioE != null)
            {
                txtNombre.Text = oUsuarioE.Nombre;
                checkAdmin.Checked = oUsuarioE.Admin;
                checkActivo.Checked = oUsuarioE.Activo;
                txtClave.Text = txtNueva.Text = txtRepetir.Text = "";
            }
            else
            {
                txtNombre.Text = "";
                checkAdmin.Checked = false;
                checkActivo.Checked = false;
            }
            checkOlvidoClave.Checked = false;
        }

        private void btnGuardarDatos_Click(object sender, EventArgs e)
        {
            if (oUsuarioE != null)
            {
                oUsuarioE.Nombre = txtNombre.Text;
                oUsuarioE.Admin = checkAdmin.Checked;
                oUsuarioE.Activo = checkActivo.Checked;

                addOrEditUser();
            }
            else
            {
                MessageBox.Show("No seleccionó ningún usuario.", "El usuario no existe", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void addOrEditUser()
        {
            try
            {
                oUsuarioN.addOrEditUser(oUsuarioE);
                MessageBox.Show("Los datos se guardaron correctamente!", "Datos guardados");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error al guardar usuario", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnGuardarContra_Click(object sender, EventArgs e)
        {
            string mensaje = "Errores:\n\n";
            bool errores = false;
            oUsuarioE = checkOlvidoClave.Checked ? oUsuarioE : oUsuarioN.validarUsuario(comboUsuario.Text, txtClave.Text, false);            
            if (oUsuarioE == null)
            {
                errores = true;
                mensaje += "-Contraseña incorrecta\n";
            } 
            if (!txtNueva.Text.Equals(txtRepetir.Text))
            {
                errores = true;
                mensaje += "-La nueva contraseña no coinciden.\n";
            }
            if (txtNueva.Text.Equals("") || txtRepetir.Text.Equals(""))
            {
                errores = true;
                mensaje += "-La nueva contraseña no puede ser vacia.\n";
            }
            if (txtNueva.Text.Contains(" ") || txtRepetir.Text.Contains(" "))
            {
                errores = true;
                mensaje += "-La nueva contraseña no puede contener espacios en blanco.\n";
            }

            if (!errores)
            {
                oUsuarioE.Clave = txtNueva.Text;
                addOrEditUser();

                //limpio campos
                txtClave.Text = txtNueva.Text = txtRepetir.Text = "";
                checkOlvidoClave.Checked = false;
            }
            else
            {
                MessageBox.Show(mensaje, "Cambiar contraseña", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void checkOlvidoClave_CheckedChanged(object sender, EventArgs e)
        {
            txtClave.Text = checkOlvidoClave.Checked && oUsuarioE != null ? oUsuarioE.Clave : "";
            txtClave.ReadOnly = checkOlvidoClave.Checked;
            txtRepetir.ReadOnly = checkOlvidoClave.Checked;
        }

        private void txtNueva_TextChanged(object sender, EventArgs e)
        {
            if (checkOlvidoClave.Checked)
            {
                txtRepetir.Text = txtNueva.Text;
            }
        }

        private void btnNuevoUsuario_Click(object sender, EventArgs e)
        {
            FormNuevoUsuario formNuevoUsuario1 = new FormNuevoUsuario();
            formNuevoUsuario1.ShowDialog();
            cargarCombo();
        }
    }
}
