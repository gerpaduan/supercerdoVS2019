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
    public partial class FormNuevoUsuario : Form
    {
        Negocio.Usuario oUsuarioN = new Negocio.Usuario();
        Entidades.Usuario oUsuarioE = new Entidades.Usuario();

        public FormNuevoUsuario()
        {
            InitializeComponent();
        }

        private void FormLoginVendedor_Load(object sender, EventArgs e)
        {
            this.Text += Utilidades.Conexion.getSucursalConexion();
            if (FormPrincipal.logueado)
	        {
                txtNombre.ReadOnly = false;
                checkAdmin.Enabled = true;
                checkActivo.Enabled = true;
                btnGuardarDatos.Enabled = true;
	        }
        }

        private void btnGuardarDatos_Click(object sender, EventArgs e)
        {
            string mensaje = "Errores:\n\n";
            bool errores = false;
            oUsuarioE = oUsuarioN.validarUsuario(txtUsuario.Text, txtClave.Text, true);
            if (oUsuarioE != null && oUsuarioE.Id > 0)
            {
                errores = true;
                mensaje += "-El usuario ya existe.\n";
            }
            if (string.IsNullOrEmpty(txtUsuario.Text) || string.IsNullOrEmpty(txtNombre.Text) || string.IsNullOrEmpty(txtClave.Text))
            {
                errores = true;
                mensaje += "-Complete todos los campos.\n";
            }

            if (errores)
            {
                MessageBox.Show(mensaje, "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            oUsuarioE = new Entidades.Usuario();
            oUsuarioE.User = txtUsuario.Text;
            oUsuarioE.Nombre = txtNombre.Text;
            oUsuarioE.Admin = checkAdmin.Checked;
            oUsuarioE.Activo = checkActivo.Checked;
            oUsuarioE.Clave = txtClave.Text;
            oUsuarioE.ColorForm = "43; 77; 129";

            //TODO - guarda datos en BD
            addOrEditUser();
        }

        private void addOrEditUser()
        {
            try
            {
                oUsuarioN.addOrEditUser(oUsuarioE);
                MessageBox.Show("Los datos se guardaron correctamente!", "Datos guardados");
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error al guardar usuario", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

    }
}
