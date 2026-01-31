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
        Negocio.Usuario oUsuarioN = new Negocio.Usuario(FormPrincipal.EmpresaSTATIC, FormPrincipal.ParametrosCTX);

        public FormValidarPermiso()
        {
            InitializeComponent(); this.Icon = Properties.Resources.CarniSys_ICONO;
        }

        private void FormValidarPermiso_Load(object sender, EventArgs e)
        {
        }

        public void EnviarUsuario(Entidades.Usuario usuario)
        {
            oUsuario = usuario;
        }

        public static bool validarPermiso(string nombreForm)
        {
            FormValidarPermiso thisForm = new FormValidarPermiso();
            return thisForm.tienePermiso(nombreForm);       
        }

        private bool tienePermiso(string nombreForm)
        {
            bool resp = true;

            if (!FormPrincipal.logueado)
            {
                Presentacion.Caja.FormLoginVendedor frmLogin = new Presentacion.Caja.FormLoginVendedor();
                frmLogin.soloActivos = true;
                frmLogin.ShowDialog(this);
                if (oUsuario == null) return false;
                //FormPrincipal.oUserLogueado = oUsuario;
            }
            else
                oUsuario = FormPrincipal.oUserLogueado;
            if (!oUsuario.Admin && !oUsuarioN.tienePermiso(oUsuario, nombreForm, DateTime.Today, Utilidades.ValoresParametrosMetodos.IdCreadorNulo()))
            {
                resp = false;
                Utilidades.Mensajes.ErrorPermisoAcceso();
            }

            return resp;
        }
    }
}
