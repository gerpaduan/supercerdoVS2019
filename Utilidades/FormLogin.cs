using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Configuration;
using System.Data.Common;

namespace Utilidades
{
    public partial class FormLogin : Form
    {
        string clave, claveSistema;

        public string Clave
        {
            get { return clave; }
            set { clave = value; }
        }
        bool existe=false;

        public bool Existe
        {
            get { return existe; }
            set { existe = value; }
        }
        public FormLogin()
        {
            InitializeComponent();
        }

        public bool Logueado()
        {
            claveSistema = ConfigurationManager.AppSettings["clave"].ToString();
            clave = txtClave.Text.Trim();
            if (clave.Equals(claveSistema))
            {
                existe = true;
            }
            return existe;
        }

        private void btnIngresar_Click(object sender, EventArgs e)
        {
            Ingresar();
        }

        private void Ingresar()
        {
            Logueado();
            if (existe)
            {
                this.Close();
            }
            else
            {
                MessageBox.Show("Clave Incorrecta.");
            }
            
        }

        private void btnSalir_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void FormLogin_FormClosing(object sender, FormClosingEventArgs e)
        {
           // Logueado();
        }

        private void txtClave_TextChanged(object sender, EventArgs e)
        {

        }

        private void txtClave_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar.Equals(Convert.ToChar(Keys.Enter)))
            {
                Ingresar();
            }
        }

        private void FormLogin_Load(object sender, EventArgs e)
        {

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
