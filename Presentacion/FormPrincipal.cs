using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Presentacion.Personas;
using Presentacion.Cortes;
using Presentacion.Pagos;
using Presentacion.Ventas;
using Presentacion.Caja;



namespace Presentacion
{
    public partial class FormPrincipal : Form, InterfaceUsuario
    {
        public static bool logueado = false;
        bool formAbierto = false;
        Entidades.Usuario oUsuario;

        public FormPrincipal()
        {
            InitializeComponent();
            
        }

        private void linkCompras_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            compras();
            

        }

        private static void compras()
        {
            //if (!logueado)
            //{
            //    Utilidades.FormLogin frmLogin = new Utilidades.FormLogin();
            //    frmLogin.ShowDialog();
            //    logueado = frmLogin.Logueado();
            //}
            if (logueado)
            {
                if (Application.OpenForms["formCompras"] != null)
                {

                    Application.OpenForms["formCompras"].Activate();
                    Application.OpenForms["formCompras"].WindowState = FormWindowState.Normal;

                }
                else
                {

                    formCompras frmCompras = new formCompras();
                    frmCompras.Show();

                }
            }
            else
            {
                MessageBox.Show("No está logueado");
            }
        }

        private void linkVentas_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            ventas();
        }

        private static void ventas()
        {
            if (!logueado)
            {
                //Utilidades.FormLogin frmLogin = new Utilidades.FormLogin();
                //frmLogin.ShowDialog();
                //logueado = frmLogin.Logueado();                
            }
            if (logueado)
            {
                if (Application.OpenForms["formVentas"] != null)
                {

                    Application.OpenForms["formVentas"].Activate();
                    Application.OpenForms["formVentas"].WindowState = FormWindowState.Normal;

                }
                else
                {

                    formVentas frmVentas = new formVentas();
                    frmVentas.Logueado = logueado;
                    frmVentas.Show();

                }
            }
            else
            {
                MessageBox.Show("No está logueado");
            }
        }

        private void linkAbrirCaja_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            formAbierto = false;

            FormLoginVendedor frmLogin = new FormLoginVendedor();
            frmLogin.ShowDialog(this);
            foreach (Form frm in Application.OpenForms)
            {
                if (frm.GetType() == typeof(formVentaCaja))
                {
                    foreach (Control ctrl in frm.Controls)
                    {
                        if (oUsuario != null && ctrl.Name.Equals("usuario") && ctrl.Text.Equals(oUsuario.User))
	                    {
                    		frm.BringToFront();
                            formAbierto = true;
                            break;
	                    }
                    }                    
                }
            }
            if (!formAbierto)
            {
                formVentaCaja frmVentaCaja = new formVentaCaja();
                frmVentaCaja.oUsuario = oUsuario;
                frmVentaCaja.Show();
            }
            oUsuario = null;
        }

        public void EnviarUsuario(Entidades.Usuario usuario)
        {
            oUsuario = usuario;
        }

        private void linkCortes_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            cortes();
        }

        private static void cortes()
        {
            if (Application.OpenForms["formCortes"] != null)
            {
                Application.OpenForms["formCortes"].Activate();
                Application.OpenForms["formCortes"].WindowState = FormWindowState.Normal;
            }
            else
            {
                formCortes frmCortes = new formCortes();
                frmCortes.Show();
            }
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            movimientos();
        }

        private static void movimientos()
        {
            if (Application.OpenForms["formMovimientos"] != null)
            {

                Application.OpenForms["formMovimientos"].Activate();
                Application.OpenForms["formMovimientos"].WindowState = FormWindowState.Normal;

            }
            else
            {

                formMovimientos frmMovimientos = new formMovimientos();
                frmMovimientos.Show();

            }
        }

        private void linkProveedores_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (Application.OpenForms["formProveedores"] != null)
            {

                Application.OpenForms["formProveedores"].Activate();
                Application.OpenForms["formProveedores"].WindowState = FormWindowState.Normal;

            }
            else
            {

                formProveedores frmProveedores = new formProveedores();
                frmProveedores.Show();

            }
        }

        private void linkMediaRes_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {

        }

        private void linkEmbutidos_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Embutidos();
        }

        private static void Embutidos()
        {
            if (Application.OpenForms["formEmbutidos"] != null)
            {

                Application.OpenForms["formEmbutidos"].Activate();
                Application.OpenForms["formEmbutidos"].WindowState = FormWindowState.Normal;

            }
            else
            {

                formEmbutidos frmEmbutidos = new formEmbutidos();
                frmEmbutidos.Show();
            }
        }

        private void linkPersonas_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            personas();
        }

        private static void personas()
        {
            if (Application.OpenForms["formPersonas"] != null)
            {

                Application.OpenForms["formPersonas"].Activate();
                Application.OpenForms["formPersonas"].WindowState = FormWindowState.Normal;

            }
            else
            {

                formPersonas frmPersonas = new formPersonas();
                frmPersonas.Show();

            }
        }


        private void linkStock_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            stockCortes();
            
        }

        private static void stockCortes()
        {
            //if (!logueado)
            //{
            //    Utilidades.FormLogin frmLogin = new Utilidades.FormLogin();
            //    frmLogin.ShowDialog();
            //    logueado = frmLogin.Logueado();
            //}
            if (logueado)
            {
                if (Application.OpenForms["formStockCortes"] != null)
                {

                    Application.OpenForms["formStockCortes"].Activate();
                    Application.OpenForms["formStockCortes"].WindowState = FormWindowState.Normal;

                }
                else
                {

                    formStockCortes frmStockCortes = new formStockCortes();
                    frmStockCortes.Show();

                }
            }
            else
            {
                MessageBox.Show("No está logueado");
            }
        }

        private void linkReportes_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            reportes();
        }

        private static void reportes()
        {
            formReporteStock frmReporteStock = new formReporteStock();
            frmReporteStock.Show();
        }

        private void linkPagos_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            pagos();
        }

        private static void pagos()
        {
            //if (!logueado)
            //{
            //    Utilidades.FormLogin frmLogin = new Utilidades.FormLogin();
            //    frmLogin.ShowDialog();
            //    logueado = frmLogin.Logueado();
            //}
            if (logueado)
            {
                if (Application.OpenForms["formPagos"] != null)
                {

                    Application.OpenForms["formPagos"].Activate();
                    Application.OpenForms["formPagos"].WindowState = FormWindowState.Normal;

                }
                else
                {

                    formPagos frmPagos = new formPagos();
                    frmPagos.Show();

                }
            }
            else
            {
                MessageBox.Show("No está logueado");
            }
        }

        

        private void linkBaseDeDatos_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            baseDeDatos();
            
        }

        private static void baseDeDatos()
        {
            //if (!logueado)
            //{
            //    Utilidades.FormLogin frmLogin = new Utilidades.FormLogin();
            //    frmLogin.ShowDialog();
            //    logueado = frmLogin.Logueado();
            //}
            if (logueado)
            {
                if (Application.OpenForms["formBackUp"] != null)
                {

                    Application.OpenForms["formBackUp"].Activate();
                    Application.OpenForms["formBackUp"].WindowState = FormWindowState.Normal;

                }
                else
                {
                    formBackUp frmBackUp = new formBackUp();
                    frmBackUp.Show();
                }
            }
            else
            {
                MessageBox.Show("No está logueado");
            }
        }

        private void FormPrincipal_Activated(object sender, EventArgs e)
        {
            if (logueado)
            {
                linkCerrarSesion.Visible = true;
            }
            else
            {
                linkCerrarSesion.Visible = false;
            }
        }

        private void linkCerrarSesion_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (logueado)
            {
                MessageBox.Show("Sesión Cerrada.");
            }
            logueado = false;
            linkLogin.Visible = true;
            linkCerrarSesion.Visible = false;
            
        }

        private void FormPrincipal_Load(object sender, EventArgs e)
        {

        }

        private void LinkVentasClientes_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            embutidos();
        }

        private static void embutidos()
        {
            if (Application.OpenForms["formEmbutidos"] != null)
            {
                Application.OpenForms["formEmbutidos"].Activate();
                Application.OpenForms["formEmbutidos"].WindowState = FormWindowState.Normal;

            }
            else
            {

                formEmbutidos frmEmbutidos = new formEmbutidos();
                frmEmbutidos.EsVentaClientes = true;
                frmEmbutidos.Show();
            }
        }

        private void linkLogin_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Utilidades.FormLogin frmLogin = new Utilidades.FormLogin();
            frmLogin.ShowDialog();
            logueado = frmLogin.Logueado();
            if (logueado)
            {
                linkLogin.Visible = false;
                linkCerrarSesion.Visible = true;
            }
            else
            {
                linkLogin.Visible = true;
                linkCerrarSesion.Visible = false;
            }
        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void verComprasToolStripMenuItem_Click(object sender, EventArgs e)
        {
            compras();
        }

        private void verVentasToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ventas();
        }

        private void cortesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            cortes();
        }

        private void personasToolStripMenuItem_Click(object sender, EventArgs e)
        {
            personas();
        }

        private void stockCortesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            stockCortes();
        }

        private void baseDeDatosToolStripMenuItem_Click(object sender, EventArgs e)
        {
            baseDeDatos();
        }

        private void FormPrincipal_FormClosing(object sender, FormClosingEventArgs e)
        { 
            if (Application.OpenForms.Count > 1)
            {
                if (MessageBox.Show("¿ Está seguro que desea salir de la aplicación?", "SuperCerdo",
               MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == DialogResult.No)
                {
                    e.Cancel = true;
                }
            }
            else
            {
                e.Cancel = false;
            }
            
        }

        private void linkCerrarCaja_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            cerrarCaja();
        }

        private void cerrarCaja()
        {
            formCajasAbiertas frmCajasAbiertas = new formCajasAbiertas();
            frmCajasAbiertas.Show();
            //FormLoginVendedor frmLogin = new FormLoginVendedor();
            //frmLogin.ShowDialog(this);

            //formCerrarCaja frmCerrarCaja = new formCerrarCaja();
            //frmCerrarCaja.oUserCierre = oUsuario;
            //frmCerrarCaja.ShowDialog();
        }

        private void linkAbrirCaja_LinkClicked_1(object sender, LinkLabelLinkClickedEventArgs e)
        {
            abrirCaja();
        }

        private void abrirCaja()
        {
            FormLoginVendedor frmLogin = new FormLoginVendedor();
            frmLogin.ShowDialog(this);
            if (oUsuario != null)
            {                
                formAbrirCaja frmAbrirCaja = new formAbrirCaja();
                frmAbrirCaja.oUserIncio = oUsuario;
                frmAbrirCaja.ShowDialog();
            }
            oUsuario = null;
        }
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            switch (keyData)
            {
                case Keys.F1:
                    formAbierto = false;
                    foreach (Form frm in Application.OpenForms)
                    {
                        if (frm.GetType() == typeof(formVentaCaja))
                        {
                            frm.BringToFront();
                            formAbierto = true;
                            break;
                        }
                    }
                    if (!formAbierto)
                    {
                        formVentaCaja frmVentaCaja = new formVentaCaja();
                        frmVentaCaja.Show();
                    }
                    break;
                case Keys.F2:
                    formAbierto = false;
                    foreach (Form frm in Application.OpenForms)
                    {
                        if (frm.GetType() == typeof(formVentaCaja2))
                        {
                            frm.BringToFront();
                            formAbierto = true;
                            break;
                        }
                    }
                    if (!formAbierto)
                    {
                        formVentaCaja2 frmVentaCaja2 = new formVentaCaja2();
                        frmVentaCaja2.Show();
                    }
                    break;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void linkCierresDeCaja_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            formCierresDeCaja frmCierresDeCaja = new formCierresDeCaja();
            frmCierresDeCaja.Show();
        }
       
    }

}
