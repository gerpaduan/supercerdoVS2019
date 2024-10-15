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
using System.IO;
using wsAFIPvs2008;
using Negocio;
using Presentacion.Licencia;


namespace Presentacion
{
    public partial class FormPrincipal : Form, InterfaceUsuario
    {
        bool cerrarFormPorError = true;
        public static bool logueado = false;
        public static bool leerBalanza = ConfigurationManager.AppSettings["puerto"].ToString().Equals("0") ? false : true;
        public static string connStringActual = ConfigurationManager.AppSettings["connString"].ToString();
        public static int idSucursal = Convert.ToInt32(ConfigurationManager.AppSettings["idSucursal"].ToString());
        public static string nombreSucursal = ConfigurationManager.AppSettings["nombreSucursal"].ToString();
        public static string cliente = ConfigurationManager.AppSettings["cliente"].ToString();
        public static string cuitCliente = ConfigurationManager.AppSettings["cuitCliente"].ToString();
        public static bool soyYo = ConfigurationManager.AppSettings["cuitCliente"].ToString().Equals("20306210786") ? true : false;
        public static string textForm = cliente + " | Suc. " + nombreSucursal;
        //codigo de barras
        public static int cantDigitosProdEnCodBarra = Convert.ToInt32(ConfigurationManager.AppSettings["cantDigitosProdEnCodBarra"].ToString());
        public static bool esCodBarraPorCantidad = ConfigurationManager.AppSettings["codBarraPorCantidad"].ToString().Equals("0") ? true : false;

        bool formAbierto = false;
        Entidades.Usuario oUsuario;
        Entidades.Usuario oUserAdmin;

        string ultimaConnSelect;

        public FormPrincipal()
        {
            InitializeComponent();
        }

        private static void compras()
        {
            if (Application.OpenForms["formCompras"] != null)
            {
                Application.OpenForms["formCompras"].Activate();
                Application.OpenForms["formCompras"].WindowState = FormWindowState.Normal;
            }
            else
            {
                if (!Usuarios.FormValidarPermiso.validarPermiso()) return;

                formCompras frmCompras = new formCompras();
                frmCompras.Logueado = true;
                frmCompras.Show();
            }   
        }

        private static void ventas()
        {
            if (Application.OpenForms["formVentas"] != null)
            {
                Application.OpenForms["formVentas"].Activate();
                Application.OpenForms["formVentas"].WindowState = FormWindowState.Normal;
            }
            else
            {
                if (!Usuarios.FormValidarPermiso.validarPermiso()) return;

                formVentas frmVentas = new formVentas();
                frmVentas.Logueado = true;
                frmVentas.Show();
            }
        }

        private void cajaVentas()
        {
            formAbierto = false;

            Presentacion.Caja.FormLoginVendedor frmLogin = new Presentacion.Caja.FormLoginVendedor();
            frmLogin.soloActivos = true;
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
            try
            {
                Negocio.OtrasClases otrasClasesN = new OtrasClases();

                #region validarLicenciaCuotas
                ///se valida que la licencia no esté vencida
                ///
                DateTime fechaVencimiento = otrasClasesN.fechaVencimientoLicencia();

                // Calcular la diferencia de fechas
                TimeSpan diferencia = fechaVencimiento - DateTime.Now;

                // Obtener el resultado en días
                int diasDiferencia = diferencia.Days;

                if (diasDiferencia < 5 && diasDiferencia >= 0)
                {
                    MessageBox.Show("Su licencia vence en "+diasDiferencia +" dias.", "Vencimiento",MessageBoxButtons.OK,MessageBoxIcon.Information);
                }
                else if (diasDiferencia < 0 && diasDiferencia >= -30)
                {
                    int caduca = 30 + diasDiferencia;
                    MessageBox.Show("Su licencia está vencida.\nEl Sistema se bloqueará en "+caduca+" dias.", "Vencimiento", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                else if (diasDiferencia < -31)
                {
                    MessageBox.Show("Su licencia ha caducado.\nIngrese el código de pago y vuelva a abrir el sistema.", "Vencimiento", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    verVencimientoCuotas();
                    Application.Exit();
                }

                #endregion



                string CPU = Utilidades.Util_Form.GetCPUId();
                //string HD = Utilidades.Util_Form.GetHDSerial();

                if (otrasClasesN.existeLicencia(CPU))
                {
                    //se ingresa al sistema
                }
                else
                {
                    //DialogResult resp = MessageBox.Show("Esta copia no cuenta con la licencia habilitada. Contactar al proveedor.", "Licencia no habilitada", MessageBoxButtons.OK, MessageBoxIcon.Error);

                    Utilidades.FormIngresarLicencia frmLicencia = new Utilidades.FormIngresarLicencia();
                    frmLicencia.ShowDialog();

                    if (frmLicencia.Licencia())
                    {
                        if (CPU != "")
                        {
                            otrasClasesN.agregarLicencia(CPU);
                        }
                        //if (HD != "")
                        //{
                        //    Utilidades.Util_Form.agregarLicencia(HD);
                        //}
                    }
                    else
                    {
                        Application.Exit();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al validar licencia. El Sistema se cerrará..\n\n"+ex.Message);
                Application.Exit();
            }


            this.Text = textForm;
            timerInactividadAdmin.Interval = Convert.ToInt32(ConfigurationManager.AppSettings["tiempoInactivoAdmin"].ToString());

            //si no soy yo vacio items de comboConexion
            if (soyYo)
            {
                comboConexion.Text = Utilidades.Conexion.connStringActual;
                ultimaConnSelect = comboConexion.Text;
                Utilidades.Conexion.tipoConn = Utilidades.Conexion.getTipoConexion();
                this.Text += Utilidades.Conexion.getSucursalConexion();
            }
            else
            {
                comboConexion.Text = connStringActual.ToString();
            }
            //Se obtienen los parametros
            Negocio.OtrasClases oOtrasClasesN = new Negocio.OtrasClases();
            oOtrasClasesN.obtenerParametros();

            valorTextoMenuEncriptarDesencriptar();
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
            Presentacion.Caja.FormLoginVendedor frmLogin = new Presentacion.Caja.FormLoginVendedor();
            frmLogin.soloActivos = true;
            frmLogin.ShowDialog(this);

            if (oUsuario == null) return;
            if (!oUsuario.Admin)
            {
                MessageBox.Show("No tienes permiso para acceder al area seleccionada.");
            }

            logueado = true;
            checkAutoDesconectar.Visible = logueado;
            if (logueado)
            {
                oUserAdmin = oUsuario.User.Equals("admin") ? oUsuario : null;
                btnLogin.Visible = false;
                btnCerrarSesion.Visible = true;
                //btnTipoConexioin.Visible = true;
                //solo muestra cambiar combo conexion si es cuit German Paduan
                comboConexion.Enabled = true && cuitCliente == "20306210786";
                timerInactividadAdmin.Start();
            }
            else
            {
                oUserAdmin = null;
                btnLogin.Visible = true;
                btnCerrarSesion.Visible = false;
                //btnTipoConexioin.Visible = false;
                comboConexion.Enabled = false;
                timerInactividadAdmin.Stop();
            }
            oUsuario = null;
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

        private void baseDeDatosToolStripMenuItem_Click(object sender, EventArgs e)
        {
            baseDeDatos();
        }

        private void FormPrincipal_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (cerrarFormPorError) return;

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

        //private void btnTipoConexioin_Click(object sender, EventArgs e)
        //{
        //    NewMethod();
        //}

        //private void NewMethod()
        //{
        //    if (Application.OpenForms.Count == 1)
        //    {
        //        if (MessageBox.Show("¿ Está seguro que desea conectarse a otra base de datos?", "Cambiar de conexión",
        //         MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == DialogResult.Yes)
        //        {
        //            if (Utilidades.Conexion.tipoConn == Utilidades.Conexion.tipoConexion.local)
        //            {
        //                Utilidades.Conexion.tipoConn = Utilidades.Conexion.tipoConexion.sanLorenzo;
        //                btnTipoConexioin.Text = "San Lorenzo";
        //            }
        //            else
        //            {
        //                if (Utilidades.Conexion.tipoConn == Utilidades.Conexion.tipoConexion.sanLorenzo)
        //                {
        //                    Utilidades.Conexion.tipoConn = Utilidades.Conexion.tipoConexion.sanMartin;
        //                    btnTipoConexioin.Text = "San Martín";
        //                }
        //                else //(Utilidades.Conexion.tipoConn == Utilidades.Conexion.tipoConexion.sanMartin)
        //                {
        //                    Utilidades.Conexion.tipoConn = Utilidades.Conexion.tipoConexion.local;
        //                    btnTipoConexioin.Text = "Local";
        //                }

        //            }
        //            MessageBox.Show("Ud. se ha conectado correctamente a la siguiente Base de Datos:\n\n" + Utilidades.Conexion.getConnString(), "Cambio de conexion", MessageBoxButtons.OK);
        //        }
        //    }
        //    else
        //    {
        //        MessageBox.Show("Debe cerrar todas las ventanas para poder conectarse a otra base de datos");
        //    }
        //}

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
            try
            {
                string abrirStockActualExterno = ConfigurationManager.AppSettings["abrirStockActualExterno"].ToString();
                switch (abrirStockActualExterno)
                {
                    //se llama la aplicacion para que inicie formStockActual independiente a la aplicacion
                    case "1":
                        string ruta = Directory.GetCurrentDirectory();
                        ruta = ruta + "\\StockActual\\CarniSys.exe";
                        System.Diagnostics.Process.Start(ruta);
                        break;
                    default:
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
                        break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error en abrir Stock Actual.\n\n" + ex.Message);
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

        private void button1_Click_1(object sender, EventArgs e)
        {

        }

        private void button1_Click_2(object sender, EventArgs e)
        {
            formFacturaElectronica factElectr = new formFacturaElectronica();
            //factElectr.Show();//comenté xq me tiraba error en la depuracion
        }

        private void verBalanzaToolStripMenuItem1_Click(object sender, EventArgs e)
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

        private void leerPesoToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            formBalanza frm = new formBalanza();
            frm.Show();
        }

        private void fórmulasToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Application.OpenForms["formFormulas"] != null)
            {
                Application.OpenForms["formFormulas"].Activate();
                Application.OpenForms["formFormulas"].WindowState = FormWindowState.Normal;
            }
            else
            {
                formFormulas frmmFormulas = new formFormulas();
                frmmFormulas.Show();
            }
        }

        private void verVencimientosToolStripMenuItem_Click(object sender, EventArgs e)
        {
        }

        private void vencimientosLicenciaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            verVencimientoCuotas();
        }

        private void verVencimientoCuotas()
        {
            if (Application.OpenForms["formVencimientoCuotas"] != null)
            {
                Application.OpenForms["formVencimientoCuotas"].Activate();
                Application.OpenForms["formVencimientoCuotas"].WindowState = FormWindowState.Normal;
            }
            else
            {
                formVencimientoCuotas frmVencimientoCuotas = new formVencimientoCuotas();
                frmVencimientoCuotas.ShowDialog();
            }
        }

        private void valorTextoMenuEncriptarDesencriptar()
        {
            // Obtener la configuración del archivo app.config
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

            // Especificar la sección que deseas encriptar
            ConfigurationSection section = config.GetSection("connectionStrings");
            encriptarToolStripMenuItem.Text = (section != null && !section.SectionInformation.IsProtected) ? "Encriptar" :
                (section != null && section.SectionInformation.IsProtected) ? "Desencriptar" : "Error en App.config";
        }
        private void encriptarToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (oUserAdmin == null)
            {
                MessageBox.Show("No tienes permiso para acceder al area seleccionada.");
                return;
            }

            // Obtener la configuración del archivo app.config
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

            // Especificar la sección que deseas encriptar
            ConfigurationSection section = config.GetSection("connectionStrings");

            if (section != null && !section.SectionInformation.IsProtected)
            {
                // Encriptar la sección si no está encriptada
                section.SectionInformation.ProtectSection("DataProtectionConfigurationProvider");

                // Guardar los cambios
                config.Save(ConfigurationSaveMode.Full);

                MessageBox.Show("La sección ha sido encriptada.");
            }
            else if(section != null && section.SectionInformation.IsProtected)
            {
                // Desencriptar la sección si está encriptada
                section.SectionInformation.UnprotectSection();

                // Guardar los cambios
                config.Save(ConfigurationSaveMode.Full);

                MessageBox.Show("La sección ha sido desencriptada.");
            }
            else
            {
                MessageBox.Show("La sección ya está encriptada o no se encontró.");
            }
            valorTextoMenuEncriptarDesencriptar();
        }

        private void appConfigToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (oUserAdmin == null)
            {
                MessageBox.Show("No tienes permiso para acceder al area seleccionada.");
                return;
            }
            Utilidades.FormAppConfig formAppConfig = new Utilidades.FormAppConfig();
            formAppConfig.Show();
        }
    }
}
