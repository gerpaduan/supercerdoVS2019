using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Presentacion.Caja
{
    public partial class FormLoginVendedor : Form
    {
        Negocio.Usuario oUsuarioN = new Negocio.Usuario();
        Entidades.Usuario oUsuarioE = new Entidades.Usuario();

        public FormLoginVendedor()
        {
            InitializeComponent();
        }

        private void FormLoginVendedor_Load(object sender, EventArgs e)
        {
            cargarCombo();
        }

        private void cargarCombo()
        {
            comboUsuario.DataSource = oUsuarioN.obtenerUsuarios();
            comboUsuario.DisplayMember = "usuario";
            comboUsuario.ValueMember = "usuario";
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

        private void btnIngresar_Click(object sender, EventArgs e)
        {
            oUsuarioE = oUsuarioN.validarUsuario(comboUsuario.SelectedValue.ToString(), txtClave.Text);
            if (oUsuarioE != null)
            {
                enviarUsuario();
            }
            else
            {
                MessageBox.Show("Contraseña incorrecta.", "Error Login", MessageBoxButtons.OK ,MessageBoxIcon.Error);
            }
            
        }
    }
}
