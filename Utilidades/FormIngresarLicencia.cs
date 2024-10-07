using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Utilidades
{
    public partial class FormIngresarLicencia : Form
    {
        string clave, claveSistema;
        public bool validacion = false;

        public string Clave
        {
            get { return clave; }
            set { clave = value; }
        }
        bool existe = false;

        public bool Existe
        {
            get { return existe; }
            set { existe = value; }
        }

        public FormIngresarLicencia()
        {
            InitializeComponent();
        }

        private void btnAgregar_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnValidar_Click(object sender, EventArgs e)
        {
            Logueado();
        }

        public void Logueado()
        {
            string day = DateTime.Now.Day > 9 ? DateTime.Now.Day.ToString() : "0" + DateTime.Now.Day.ToString();
            claveSistema = DateTime.Now.Year.ToString() + DateTime.Now.Month.ToString() + day; //ConfigurationManager.AppSettings["admin"].ToString();
            clave = txtClave.Text.Trim();
            if (clave.Equals(claveSistema))
            {
                existe = true;
                groupLicencias.Visible = true;
                lblErrorLicencia.Visible = false;
                serialCPU.Text = Utilidades.Util_Form.GetCPUId();
                //serialHD.Text = Utilidades.Util_Form.GetHDSerial();
                btnAgregar.Visible = true;
            }
            else
            {
                MessageBox.Show("Clave Incorrecta.");
                txtClave.Focus();
                lblErrorLicencia.Visible = true;
            }
            //return existe;
        }

        private void Ingresar()
        {
            //Logueado();
            //if (existe)
            //{
            //    this.Close();
            //}
            //else
            //{
            //    MessageBox.Show("Clave Incorrecta.");
            //}

        }


        public bool Licencia()
        {
            return existe;
        }

        private void btnSalir_Click(object sender, EventArgs e)
        {
            existe = false;
            this.Close();
        }

        private void FormIngresarLicencia_Load(object sender, EventArgs e)
        {
            //lblErrorLicencia.Visible = true;
            //groupLicencias.Visible = false;
        }

        private void txtClave_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar.Equals(Convert.ToChar(Keys.Enter)))
            {
                Logueado();
            }
        }
    }
}
