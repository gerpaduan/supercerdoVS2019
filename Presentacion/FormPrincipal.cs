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
using Presentacion.Balanza;
using Presentacion.Usuario;
using System.Configuration;


namespace Presentacion
{
    public partial class FormPrincipal : Form, InterfaceUsuario
    {
        
        public static bool logueado = false;
        //public enum tipoConexion { local, remota }
        //public static tipoConexion tipoConn  = tipoConexion.local;
        bool formAbierto = false;
        Entidades.Usuario oUsuario;

        Entidades.Sucursal oSucursalE = new Entidades.Sucursal();
        Negocio.Sucursal oSucursalN = new Negocio.Sucursal();

        public FormPrincipal()
        {
            InitializeComponent();
            
        }

        private static void compras()
        {
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

        private static void ventas()
        {
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

        private void cajaVentas()
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

        private static void reportes()
        {
            formReporteStock frmReporteStock = new formReporteStock();
            frmReporteStock.Show();
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
            Utilidades.Conexion.tipoConn = Utilidades.Conexion.tipoConexion.local;

            //asigo sucursal al título  
            int idSucursal = Convert.ToInt32(ConfigurationManager.AppSettings["idSucursal"].ToString());
            oSucursalE = oSucursalN.findById(idSucursal);
            this.Text = this.Text + " | Suc. " + oSucursalE.sucursal;
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
                btnTipoConexioin.Visible = true;
            }
            else
            {
                linkLogin.Visible = true;
                linkCerrarSesion.Visible = false;
                btnTipoConexioin.Visible = false;
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
            bool permitirCerrar = true;
            e.Cancel = true;
            foreach (Form frm in Application.OpenForms)
            {
                if (frm.GetType() == typeof(formVentaCaja))
                {
                    MessageBox.Show("Para salir de la aplicación debe cerrar las ventanas de ventas");
                    permitirCerrar = false;
                    break;
                }
            }
            if (permitirCerrar && MessageBox.Show("¿ Está seguro que desea salir de la aplicación?", "SuperCerdo",
           MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == DialogResult.Yes)
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
            cierresCaja();
        }

        private static void cierresCaja()
        {
            formCierresDeCaja frmCierresDeCaja = new formCierresDeCaja();
            frmCierresDeCaja.Show();
        }

        private void linkStock_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            stock();
        }

        private static void stock()
        {
            formStock frm = new formStock();
            frm.Show();
        }

        private void balanzaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            
        }

        private void btnTipoConexioin_Click(object sender, EventArgs e)
        {
            if (Application.OpenForms.Count == 1)
            {
                if (MessageBox.Show("¿ Está seguro que desea conectarse a otra base de datos?", "Cambiar de conexión",
                 MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == DialogResult.Yes)
                {
                    if (Utilidades.Conexion.tipoConn == Utilidades.Conexion.tipoConexion.local)
                    {
                        Utilidades.Conexion.tipoConn = Utilidades.Conexion.tipoConexion.remota;
                        btnTipoConexioin.Text = "Con. Remota";
                        btnTipoConexioin.BackColor = Color.DarkSalmon;
                    }
                    else
                    {
                        Utilidades.Conexion.tipoConn = Utilidades.Conexion.tipoConexion.local;
                        btnTipoConexioin.Text = "Con. Local";
                        btnTipoConexioin.BackColor = Color.SteelBlue;
                    }
                    MessageBox.Show("Ud. se ha conectado correctamente a la siguiente Base de Datos:\n\n"+Utilidades.Conexion.getConnString(),"Cambio de conexion",MessageBoxButtons.OK);                    
                }
            }
            else
            {
                MessageBox.Show("Debe cerrar todas las ventanas para poder conectarse a otra base de datos");
            }
        }

        private void verBalanzaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Utilidades.FormLeer_Peso frm = Utilidades.FormLeer_Peso.CrearLeerPeso();
            frm.Show();
        }

        private void leerPesoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            formBalanza frm = new formBalanza();
            frm.Show();
        }

        private void btnCompras_Click(object sender, EventArgs e)
        {
            compras();
        }

        private void btnVentas_Click(object sender, EventArgs e)
        {
            ventas();
        }

        private void btnCajaVentas_Click(object sender, EventArgs e)
        {
            cajaVentas();
        }

        private void btnAbrirCaja_Click(object sender, EventArgs e)
        {
            abrirCaja();
        }

        private void btnCerrarCaja_Click(object sender, EventArgs e)
        {
            cerrarCaja();
        }

        private void btnCierresCaja_Click(object sender, EventArgs e)
        {
            cierresCaja();
        }

        private void btnMovimientos_Click(object sender, EventArgs e)
        {
            movimientos();
        }

        private void btnEmbutidos_Click(object sender, EventArgs e)
        {
            Embutidos();
        }

        private void btnStock_Click(object sender, EventArgs e)
        {
            stock();
        }

        private void btnCortes_Click(object sender, EventArgs e)
        {
            cortes();
        }

        private void btnPersonas_Click(object sender, EventArgs e)
        {
            personas();
        }

        private void btnReportes_Click(object sender, EventArgs e)
        {
            reportes();
        }

        private void btnUsuarios_Click(object sender, EventArgs e)
        {
            FormUsuarios frmUsuario = new FormUsuarios();
            frmUsuario.Show();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            gastos();
        }

        private static void gastos()
        {
            if (Application.OpenForms["formGastos"] != null)
            {

                Application.OpenForms["formGastos"].Activate();
                Application.OpenForms["formGastos"].WindowState = FormWindowState.Normal;

            }
            else
            {
                formGastos frmGastos = new formGastos();
                frmGastos.Show();
            }
        }     
    }
}
