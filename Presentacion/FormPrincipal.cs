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



namespace Presentacion
{
    public partial class FormPrincipal : Form
    {
        public static bool logueado = false;
        public FormPrincipal()
        {
            InitializeComponent();
            
        }

        private void linkCompras_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
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
            //if (!logueado)
            //{
            //    Utilidades.FormLogin frmLogin = new Utilidades.FormLogin();
            //    frmLogin.ShowDialog();
            //    logueado = frmLogin.Logueado();
            //}
            //if (logueado)
            //{
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
            //}
            //else
            //{
            //    MessageBox.Show("No está logueado");
            //}


        }

      

        private void linkCortes_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
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
            formReporteStock frmReporteStock = new formReporteStock();
            frmReporteStock.Show();

            //if (Application.OpenForms["formReporteStock"] != null)
            //{

            //    Application.OpenForms["formReporteStock"].Activate();
            //    Application.OpenForms["formReporteStock"].WindowState = FormWindowState.Normal;

            //}
            //else
            //{

            //    formReporteStock frmReporteStock = new formReporteStock();
            //    frmReporteStock.Show();

            //}
        }

        private void linkPagos_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
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
        
       
    }

}
