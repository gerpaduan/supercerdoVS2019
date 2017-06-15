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
using Presentacion.Pruebas;
using Presentacion.CuentaCorriente;
using System.Configuration;
using Utilidades;


namespace Presentacion
{
    public partial class FormPrincipal : Form, InterfaceUsuario
    {        
        public static bool logueado = false;
        bool formAbierto = false;
        Entidades.Usuario oUsuario;

        string ultimaConnSelect;

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

            Presentacion.Caja.FormLoginVendedor frmLogin = new Presentacion.Caja.FormLoginVendedor();
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

        private void reportes()
        {            
            if (!logueado)
            {
                Presentacion.Caja.FormLoginVendedor frmLogin = new Presentacion.Caja.FormLoginVendedor();
                frmLogin.ShowDialog(this);
                if (oUsuario == null) return;
                if (!oUsuario.Admin)
                {
                    MessageBox.Show("No tienes permiso para ver reportes");
                    return;
                }                
            }
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
                btnCerrarSesion.Visible = true;
            }
            else
            {
                btnCerrarSesion.Visible = false;
            }
        }

        private void linkCerrarSesion_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            cerrarSesion();
        }

        private void cerrarSesion()
        {
            logueado = false;
            btnLogin.Visible = true;
            btnCerrarSesion.Visible = false;
            checkAutoDesconectar.Visible = false;
            comboConexion.Enabled = false;
            timerInactividadAdmin.Stop();
            MessageBox.Show("Sesión Cerrada.");
        }

        private void FormPrincipal_Load(object sender, EventArgs e)
        {
            timerInactividadAdmin.Interval = Convert.ToInt32(ConfigurationManager.AppSettings["tiempoInactivoAdmin"].ToString());
            comboConexion.Text = Utilidades.Conexion.connStringActual;
            ultimaConnSelect = comboConexion.Text;
            Utilidades.Conexion.tipoConn = Utilidades.Conexion.getTipoConexion();
            this.Text += Utilidades.Conexion.getSucursalConexion();
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
            login();
        }

        private void login()
        {
            Utilidades.FormLogin frmLogin = new Utilidades.FormLogin();
            frmLogin.ShowDialog();
            logueado = frmLogin.Logueado();
            checkAutoDesconectar.Visible = logueado;
            if (logueado)
            {
                btnLogin.Visible = false;
                btnCerrarSesion.Visible = true;
                //btnTipoConexioin.Visible = true;
                comboConexion.Enabled = true;
                timerInactividadAdmin.Start();
            }
            else
            {
                btnLogin.Visible = true;
                btnCerrarSesion.Visible = false;
                //btnTipoConexioin.Visible = false;
                comboConexion.Enabled = false;
                timerInactividadAdmin.Stop();
            }
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
            if (e.CloseReason == CloseReason.WindowsShutDown) return;

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
            if (Application.OpenForms["formCajasAbiertas"] != null)
            {
                Application.OpenForms["formCajasAbiertas"].Activate();
                Application.OpenForms["formCajasAbiertas"].WindowState = FormWindowState.Normal;
            }
            else
            {
                formCajasAbiertas frmCajasAbiertas = new formCajasAbiertas();
                frmCajasAbiertas.Show();
            }
        }

        private void linkAbrirCaja_LinkClicked_1(object sender, LinkLabelLinkClickedEventArgs e)
        {
            abrirCaja();
        }

        private void abrirCaja()
        {
            Presentacion.Caja.FormLoginVendedor frmLogin = new Presentacion.Caja.FormLoginVendedor();
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
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void linkCierresDeCaja_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            cierresCaja();
        }

        private static void cierresCaja()
        {
            if (Usuarios.FormValidarPermiso.validarPermiso())
            {
                if (Application.OpenForms["formCierresDeCaja"] != null)
                {
                    Application.OpenForms["formCierresDeCaja"].Activate();
                    Application.OpenForms["formCierresDeCaja"].WindowState = FormWindowState.Normal;
                }
                else
                {
                    formCierresDeCaja frmCierresDeCaja = new formCierresDeCaja();
                    frmCierresDeCaja.Show();
                }
            }
        }

        private void linkStock_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            stock();
        }

        private static void stock()
        {
            if (Application.OpenForms["formStock"] != null)
            {

                Application.OpenForms["formStock"].Activate();
                Application.OpenForms["formStock"].WindowState = FormWindowState.Normal;

            }
            else
            {
                formStock frm = new formStock();
                frm.Show();
            }
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
                        Utilidades.Conexion.tipoConn = Utilidades.Conexion.tipoConexion.sanLorenzo;
                        btnTipoConexioin.Text = "San Lorenzo";
                    }
                    else
                    {
                        if (Utilidades.Conexion.tipoConn == Utilidades.Conexion.tipoConexion.sanLorenzo)
                        {
                            Utilidades.Conexion.tipoConn = Utilidades.Conexion.tipoConexion.sanMartin;
                            btnTipoConexioin.Text = "San Martín";
                        }
                        else //(Utilidades.Conexion.tipoConn == Utilidades.Conexion.tipoConexion.sanMartin)
                        {
                            Utilidades.Conexion.tipoConn = Utilidades.Conexion.tipoConexion.local;
                            btnTipoConexioin.Text = "Local";
                        }

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
            bool formAbierto = false;
            foreach (Form frm in Application.OpenForms)
            {
                int d = Application.OpenForms.Count;
                if (frm.GetType() == typeof(FormPesoBalanza))
                {
                    frm.BringToFront();
                    formAbierto = true;
                    break;                    
                }
            }
            if (!formAbierto)
            {
                Utilidades.FormPesoBalanza frmBalanza = new Utilidades.FormPesoBalanza();
                frmBalanza.Show();
            }
           
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
            if (Application.OpenForms["formEgresosCaja"] != null)
            {

                Application.OpenForms["formEgresosCaja"].Activate();
                Application.OpenForms["formEgresosCaja"].WindowState = FormWindowState.Normal;

            }
            else
            {
                if (!Usuarios.FormValidarPermiso.validarPermiso()) return;
                formEgresosCaja frmEgresosCaja = new formEgresosCaja();
                frmEgresosCaja.Show();
            }
        }

        private void cierresCajaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            cierresCaja();
        }

        private void btnEgresosCaja_Click(object sender, EventArgs e)
        {
            gastos();
        }

        private void imprimirTicketToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Application.OpenForms["formTicketPrueba"] != null)
            {

                Application.OpenForms["formTicketPrueba"].Activate();
                Application.OpenForms["formTicketPrueba"].WindowState = FormWindowState.Normal;

            }
            else
            {
                formTicketPrueba frmTicketPrueba = new formTicketPrueba();
                frmTicketPrueba.Show();
            }
        }

        private void comboConexion_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (logueado)
            {
                //si verifica si está abierto el formulario desde donde se obtiene el peso de la balanza
                Form existe = Application.OpenForms.OfType<Form>().Where(pre => pre.Name == "FormPesoBalanza").SingleOrDefault<Form>();
                
                if (Application.OpenForms.Count == 1 || (Application.OpenForms.Count.Equals(2) && existe!=null))
                {
                    ultimaConnSelect = comboConexion.Text;
                    Utilidades.Conexion.connStringActual = comboConexion.Text;
                    Utilidades.Conexion.tipoConn = Utilidades.Conexion.getTipoConexion();
                    MessageBox.Show("Ud. se ha conectado correctamente a la siguiente Base de Datos:\n\n" + Utilidades.Conexion.getConnString(), "Cambio de conexion", MessageBoxButtons.OK);
                    this.Text = Utilidades.Conexion.getSucursalConexion();
                }
                else
                {
                    if (!comboConexion.Text.Equals(ultimaConnSelect))
                    {
                        comboConexion.Text = ultimaConnSelect;
                        MessageBox.Show("Debe cerrar todas las ventanas para poder conectarse a otra base de datos");
                    }
                }                
            }
        }

        private void verToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (logueado)
            {
                if (Application.OpenForms["formTemporalLineaVenta"] != null)
                {
                    Application.OpenForms["formTemporalLineaVenta"].Activate();
                    Application.OpenForms["formTemporalLineaVenta"].WindowState = FormWindowState.Normal;

                }
                else
                {
                    formTemporalLineaVenta frmTemporalLineaVenta = new formTemporalLineaVenta();
                    frmTemporalLineaVenta.Show();
                }
            }
            else
            {
                MessageBox.Show("No está logueado");
            }
        }

        private void timerInactividadAdmin_Tick(object sender, EventArgs e)
        {
            if (logueado && checkAutoDesconectar.Checked)
            {
                cerrarSesion();
            }
        }

        private void configuraciónToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (logueado)
            {
                Utilidades.FormAppConfig formAppConfig = new Utilidades.FormAppConfig();
                formAppConfig.Show();
            }
            else
            {
                MessageBox.Show("No está logueado");
            }
        }

        private void lineasVentaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (logueado)
            {
                if (Application.OpenForms["formGetAllLineaVenta"] != null)
                {
                    Application.OpenForms["formGetAllLineaVenta"].Activate();
                    Application.OpenForms["formGetAllLineaVenta"].WindowState = FormWindowState.Normal;

                }
                else
                {
                    formGetAllLineaVenta frmTemporalLineaVenta = new formGetAllLineaVenta();
                    frmTemporalLineaVenta.Show();
                }
            }
            else
            {
                MessageBox.Show("No está logueado");
            }
        }

        private void ctasCtesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (logueado)
            {
                if (Application.OpenForms["formCtasCtes"] != null)
                {
                    Application.OpenForms["formCtasCtes"].Activate();
                    Application.OpenForms["formCtasCtes"].WindowState = FormWindowState.Normal;

                }
                else
                {
                    formCtasCtes frmCtasCtes = new formCtasCtes();
                    frmCtasCtes.Show();
                }
            }
            else
            {
                MessageBox.Show("No está logueado");
            }
        }

        private void pagosToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (logueado)
            {
                if (Application.OpenForms["formPagos"] != null)
                {
                    Application.OpenForms["formPagos"].Activate();
                    Application.OpenForms["formPagos"].WindowState = FormWindowState.Normal;

                }
                else
                {
                    Pagos.formPagos frmPagos = new formPagos();
                    frmPagos.Show();
                }
            }
            else
            {
                MessageBox.Show("No está logueado");
            }
        }

        private void stockActualToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Application.OpenForms["formStockActual"] != null)
            {
                Application.OpenForms["formStockActual"].Activate();
                Application.OpenForms["formStockActual"].WindowState = FormWindowState.Normal;

            }
            else
            {
                formStockActual frmStockActual = new formStockActual();
                frmStockActual.Show();
            }
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            login();
        }

        private void btnCerrarSesion_Click(object sender, EventArgs e)
        {
            cerrarSesion();
        }
    }
}
