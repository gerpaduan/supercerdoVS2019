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
        Negocio.Usuario oUsuarioN = new Negocio.Usuario(FormPrincipal.EmpresaSTATIC, FormPrincipal.ParametrosCTX);
        Entidades.Usuario oUsuarioE = new Entidades.Usuario();
        public bool soloActivos = false;
        public bool usuarioConPermiso = false;
        public bool soloAdmin = false;

        public FormLoginVendedor()
        {
            InitializeComponent(); this.Icon = Properties.Resources.CarniSys_ICONO;
        }

        private void FormLoginVendedor_Load(object sender, EventArgs e)
        {
            this.Text += Utilidades.Conexion.getSucursalConexion();
            cargarCombo();
        }

        private void cargarCombo()
        {
            comboUsuario.DataSource = oUsuarioN.obtenerUsuarios(soloActivos, true, soloAdmin);
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
        public void enviarUsuarioConPermiso()
        {
            InterfaceUsuarioConPermiso formInterface = this.Owner as InterfaceUsuarioConPermiso;
            if (formInterface != null)
            {
                formInterface.EnviarUsuarioConPermiso(oUsuarioE);
            }
            this.Close();
        }

        private void btnIngresar_Click(object sender, EventArgs e)
        {
            ingresar();          
        }

        private void ingresar()
        {
            oUsuarioE = oUsuarioN.validarUsuario(comboUsuario.Text, txtClave.Text, false);
            if (oUsuarioE != null)
            {
                if (usuarioConPermiso)
                    enviarUsuarioConPermiso();
                else
                    enviarUsuario();
            }
            else
            {
                MessageBox.Show("Contraseña incorrecta.", "Error Login", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void txtClave_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)(Keys.Enter))
            {
                ingresar();
            }
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Escape)
            {
                this.Close();
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }
    }
}
