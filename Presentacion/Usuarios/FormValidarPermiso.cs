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
    public partial class FormValidarPermiso : Form, InterfaceUsuario
    {
        Entidades.Usuario oUsuario;

        public FormValidarPermiso()
        {
            InitializeComponent();
        }

        private void FormValidarPermiso_Load(object sender, EventArgs e)
        {
        }

        public void EnviarUsuario(Entidades.Usuario usuario)
        {
            oUsuario = usuario;
        }

        public static bool validarPermiso()
        {
            FormValidarPermiso thisForm = new FormValidarPermiso();
            return thisForm.tienePermiso();       
        }

        private bool tienePermiso()
        {
            bool resp = true;

            if (!FormPrincipal.logueado)
            {
                Presentacion.Caja.FormLoginVendedor frmLogin = new Presentacion.Caja.FormLoginVendedor();
                frmLogin.ShowDialog(this);
                if (oUsuario == null) return false;
                if (!oUsuario.Admin)
                {
                    resp = false;
                    MessageBox.Show("No tienes permiso para acceder al area seleccionada.");
                }
            }
            return resp;
        }
    }
}
