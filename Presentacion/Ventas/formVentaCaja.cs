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
using System.Configuration;

namespace Presentacion.Ventas
{
    public partial class formVentaCaja : Form, InterfaceCorte, InterfacePersona
    {
        string cambiar = "";
        bool checkAnterior = false;
        Utilidades.Leer_Peso Leer_Peso = new Utilidades.Leer_Peso();
        Utilidades.Util_Form Util_Form = new Utilidades.Util_Form();
        #region variables
        formVentas frmVentas;
        DataTable dtSucursales;
        Negocio.Corte oCorteN = new Negocio.Corte();
        Negocio.Sucursal oSucursalN=new Negocio.Sucursal();
        Negocio.Venta oVentaN = new Negocio.Venta();

        Entidades.Compra oCompraE = new Entidades.Compra();
        Entidades.Persona oCliente;
        Entidades.Corte oCorteE;
        Entidades.Sucursal oSucursalE= new Entidades.Sucursal();
        Entidades.Sucursal oSucAnterior = new Entidades.Sucursal();
        Entidades.Venta oVentaE = new Entidades.Venta();
        Entidades.LineaVenta oLineaVenta;
        Entidades.StockCorteSucursal oStockCorteSucursal;

        List<Entidades.LineaVenta> listaLineaVenta = new List<Entidades.LineaVenta>();
        List<LineaVenta> listaLineaGrilla = new List<LineaVenta>();

        int sucAnterior;

        public int SucAnterior
        {
            get { return sucAnterior; }
            set { sucAnterior = value; }
        }

        bool modificar = false;
        string fecha = "", estadoVenta="";
        float totalCorte, precioKg, cantKg;
        float totalVenta = 0, abona = 0, cambio = 0;
        #endregion


        public formVentaCaja()
        {
            InitializeComponent();
            this.KeyPreview = true;
            //asigo sucursal a la venta  
            int idSucursal = Convert.ToInt32(ConfigurationManager.AppSettings["idSucursal"].ToString());
            oSucursalE = oSucursalN.findById(idSucursal);
            oVentaE.Sucursal = oSucursalE;
            this.txtSucursal.Text = oVentaE.Sucursal.sucursal;
            Negocio.Persona oPersonaN = new Negocio.Persona();
            int idConsumidorFinal = Convert.ToInt32(ConfigurationManager.AppSettings["idConsumidorFinal"].ToString());
            oCliente = oPersonaN.findById(idConsumidorFinal);
            this.txtCliente.Text = oCliente.razonSocial;
            if (!fecha.Equals(""))
            {
                txtFechaVenta.Value = DateTime.Parse(fecha);
            }
            
        }


#region Modificar_Venta


        public void parametrosModificacion(formVentas frmVentasParam,Entidades.Venta oVentaParam, List<Entidades.LineaVenta> listaLineaVentaParam, List<LineaVenta> listaLineaGrillaParam)
        {
            modificar = true;
            this.Text = "Modificar Venta";

            frmVentas = frmVentasParam;
            oVentaE = oVentaParam;
            oCliente = oVentaE.Persona;
            oSucursalE = oVentaE.Sucursal;
            oSucAnterior = oVentaParam.Sucursal;

            listaLineaVenta = listaLineaVentaParam;
            listaLineaGrilla = listaLineaGrillaParam;

            cargarCamposVenta();
            cargarGrilla();

        }

        private void cargarCamposVenta()
        {
            txtCliente.Text = oVentaE.Persona.razonSocial;
            txtSucursal.Text = oVentaE.Sucursal.sucursal;
            txtFechaVenta.Value =oVentaE.FechaVenta;
            txtNroRemito.Text = oVentaE.NroRemito;
            txtObservaciones.Text = oVentaE.Observaciones;

            estadoVenta = oVentaE.Estado;
        }

      

        private void modificarVenta()
        {
            if (validacionFinal())
            {
                cargarVenta();
                try
                {
                    
                    oVentaN.modificarVenta(oVentaE, SucAnterior);

                    foreach (Entidades.LineaVenta linea in listaLineaVenta)
                    {
                        oVentaN.agregarLineaVenta(linea);
                    }

                    frmVentas.cargarGrilla();

                    this.Close();
                    
                }
                catch (Exception ex)
                {
                    string g = ex.Message;

                    MessageBox.Show(ex.Message);
                }
            }

        }

#endregion

        private void esModificacion()
        {
            //si es modificacion o agregacion
            if (modificar)
            {
                modificarVenta();
            }
            else
            {
                if (grillaLineasVenta.SelectedRows.Count > 0)
                {
                    agregarVenta();
                    txtCodigo.Focus();
                }
                else
                {
                    MessageBox.Show("No se ha cargado ningún corte en la venta. ", "No hay cortes cargados", MessageBoxButtons.OK, MessageBoxIcon.Information);

                }
            }
            
        }

        public void cargarGrilla()
        {
            try
            {
                grillaLineasVenta.AutoGenerateColumns = false;
                grillaLineasVenta.DataSource = null;

                grillaLineasVenta.DataSource = listaLineaGrilla;

                if (listaLineaGrilla.Count > 0)
                {
                    grillaLineasVenta.Rows[listaLineaGrilla.Count - 1].Selected = true;
                    grillaLineasVenta.FirstDisplayedScrollingRowIndex = listaLineaGrilla.Count - 1;
                }

                cargarTotales();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            
        }

        private void agregarVenta()
        {
            if (validacionFinal())
            {
                cargarVenta();

                try
                {
                    oVentaE.IdVenta = oVentaN.agregarVenta(oVentaE);

                    foreach (Entidades.LineaVenta linea in listaLineaVenta)
                    {
                        oVentaN.agregarLineaVenta(linea);
                    }

                    limpiarListas();
                    //this.Close();
                }
                catch (Exception ex)
                {

                    MessageBox.Show(ex.Message);
                }
            }
            
        }
        private void limpiarListas()
        {
            txtFechaVenta.Value = DateTime.Now;
            txtNroRemito.Text = "";
            txtObservaciones.Text = "";
            txtCantItems.Text = "0";
            txtTotalS.Text = "000,00";
            txtAbona.Text = "";
            txtCambio.Text = "";
            panelPago.Visible = false;

            listaLineaGrilla = new List<LineaVenta>(); 
            listaLineaVenta = new List<Entidades.LineaVenta>();
            grillaLineasVenta.DataSource = null;
        }

        private void cargarVenta()
        {
            oVentaE.Persona = oCliente;
            oVentaE.Sucursal = oSucursalE;
            oVentaE.FechaVenta = txtFechaVenta.Value;
            oVentaE.NroRemito = txtNroRemito.Text.Trim();
            oVentaE.Turno = "";
            oVentaE.DiaFestivo = "";
            oVentaE.Observaciones = txtObservaciones.Text.Trim();
            oVentaE.Estado = estadoVenta ;


        }

        private void cargarTotales()
        {
            float totalKgs = 0;
            float totalPesos = 0;

            foreach (LineaVenta linea in listaLineaGrilla)
            {
                totalKgs += linea.cantKgs;
                totalPesos += linea.totalS;
            }

            txtCantItems.Text = grillaLineasVenta.Rows.Count.ToString();
            txtTotalS.Text = totalPesos.ToString("N2");        
        }

        private void agregarLinea()
        {
            if (validarLinea())
            {
                try
                {
                    if (grillaLineasVenta.Rows.Count == 0)
                    {
                        txtFechaVenta.Value = DateTime.Now;
                    }
                    cargarLinea();

                    cargarListas();
                    cargarGrilla();

                    limpiarCamposCorte();
                    oLineaVenta = null;

                    txtCodigo.Focus();
                }
                catch (Exception ex)
                {                    
                    MessageBox.Show(ex.Message);
                }                    
            }
        }

        private void cargarListas()
        {
            listaLineaVenta.Add(oLineaVenta);
            cargarListaGrilla(oLineaVenta);
        }
        
        

        private void sumarCorte(int nroLinea)
        {
            listaLineaVenta[nroLinea].CantKg = listaLineaVenta[nroLinea].CantKg + oLineaVenta.CantKg;
            listaLineaGrilla[nroLinea].cantKgs = listaLineaGrilla[nroLinea].cantKgs + oLineaVenta.CantKg;

            listaLineaGrilla[nroLinea].totalS = listaLineaGrilla[nroLinea].totalS + (oLineaVenta.CantKg * oLineaVenta.PrecioKg);
        }

        private void limpiarCamposCorte()
        {
            
            txtCodigo.Text = "";
            txtCorte.Text = "";
            txtCantKgs.Text = "";
            txtPrecioKg.Text = "";
            txtTotalCorte.Text = "";

            txtCodigo.Focus();
        }

        private void cargarListaGrilla(Entidades.LineaVenta lineaE)
        {
            LineaVenta lineaVentaP = new LineaVenta();

            lineaVentaP.idCorte = lineaE.Corte.idCorte;
            lineaVentaP.codigo = lineaE.Corte.codigo;
            lineaVentaP.corte = lineaE.Corte.corte;
            lineaVentaP.cantKgs = lineaE.CantKg;
            lineaVentaP.precioKg = lineaE.PrecioKg;
            lineaVentaP.totalS = lineaE.PrecioKg * lineaE.CantKg;

            if (lineaE.Estado==1)
            {
                lineaVentaP.estado = "Anulado";
            }
            else
            {
                lineaVentaP.estado = "";
            }

            listaLineaGrilla.Add(lineaVentaP);
            lineaVentaP = null;

        }

        private void cargarLinea()
        {
            oLineaVenta = new Entidades.LineaVenta();

            oLineaVenta.Corte = oCorteE;
            oLineaVenta.Venta = oVentaE;

            oLineaVenta.CantKg = cantKg;
            oLineaVenta.PrecioKg = precioKg;
            
             if (oLineaVenta.CantKg < 0)
             {
                 oLineaVenta.Estado = 1;//Anulado
             }
             else
             {
                 oLineaVenta.Estado = 0;//Activo
             }                    
        }

        
        private bool validarLinea()
        {
            string mensaje = "Complete los siguientes campos: ";
            if (txtCodigo.Text.Trim() == "" || txtCantKgs.Text.Trim() == "" || txtPrecioKg.Text.Trim() == "")
            {
                if (txtCodigo.Text.Trim() == "")
                {
                    mensaje += "\n" + "-Código Corte";
                    
                    MessageBox.Show(mensaje, "Completar campos", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    txtCodigo.Focus();
                }

                else
                {
                    if (oCorteE==null)
                    {
                          MessageBox.Show("El código ingresado no pertenece a ningún corte.", "El Corte no existe", MessageBoxButtons.OK, MessageBoxIcon.Information);
                          txtCodigo.Focus();  
                    }
                    else
                    {
                        if (txtCantKgs.Text.Trim() == "")
                        {
                            mensaje += "\n" + "-Cant. Kgs";
                            
                        }
                        if (txtPrecioKg.Text.Trim() == "")
                        {
                            mensaje += "\n" + "-Precio Kg";
                        }

                        MessageBox.Show(mensaje, "Completar campos", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        txtCantKgs.Focus();
                    }
                
                }
                return false;
            }
            if (!Utilidades.Util_Form.validarCampoNumerico(txtCantKgs.Text, "Kgs."))
            {
                txtCantKgs.Focus();
                return false;
            }
            else
            {
                return true;
            }
        }

        private bool validacionFinal()
        {
            //si es una modificacion y no hay datos en la grilla no valida porque se eliminar la venta
            if (modificar && grillaLineasVenta.Rows.Count==0)
            {
                DialogResult respuesta;
                respuesta = MessageBox.Show("¿Está seguro que desea eliminar todos los datos de la venta?.", "Eliminar venta", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);

                if (respuesta == DialogResult.Yes)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                string mensaje = "Complete los siguientes campos: ";
                if (txtCliente.Text.Trim() == "")
                {
                    if (txtCliente.Text.Trim() == "")
                    {
                        mensaje += "\n" + "-Cliente";
                    }

                    MessageBox.Show(mensaje, "Completar campos", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return false;
                }
                else
                {
                    if (totalVenta >= 0)
                    {
                        if (abona > 0 && cambio < 0)
                        {
                            mensaje = "El pago del cliente es menor al total de la venta";
                            MessageBox.Show(mensaje, "Error en el pago", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            txtAbona.Focus();
                            return false;
                        }
                        else
                        {
                            DialogResult respuesta = MessageBox.Show("¿Finalizar la venta?. ", "Finalizar Venta", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1);

                            if (respuesta == System.Windows.Forms.DialogResult.Yes)
                            {
                                return true;
                            }
                            else
                            {
                                return false;
                            }
                        }
                    }
                    else
                    {
                        mensaje = "No se puede finalizar la venta porque el total a pagar es negativo (menor a cero).\n\nTotal a pagar:  $ "+totalVenta;
                        MessageBox.Show(mensaje, "Error en la venta", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return false;
                    }
                }
            }
        }

        private void quitarLinea()
        {
            if (grillaLineasVenta.SelectedRows.Count > 0)
            {
                int nroFila = grillaLineasVenta.Rows.GetFirstRow(DataGridViewElementStates.Selected);//obtiene nro de fila de la grilla
                
                Entidades.LineaVenta oLineaVentaSelect = new Entidades.LineaVenta();
                oLineaVentaSelect = listaLineaVenta[nroFila];

                bool existeAnulado = false;
                foreach (Entidades.LineaVenta linea in listaLineaVenta)
                  {
                    if (oLineaVentaSelect.Corte.codigo == linea.Corte.codigo &&
                        linea.IndexAnulado == nroFila)
                    {
                        existeAnulado = true;
                    }
                }

                if (oLineaVentaSelect.Estado == 0 && !existeAnulado)
                {
                    string datosLinea = "\n\n Datos del Corte \n-----------------------------------------\n " + 
                        oLineaVentaSelect.Corte.corte +
                        "    |   Cantidad:  " + oLineaVentaSelect.CantKg + "    |    Total:  $ " + oLineaVentaSelect.CantKg * oLineaVentaSelect.Corte.precioKg;
                    string mensaje = "¿Está seguro de anular el corte seleccionado?" + datosLinea;
                    DialogResult respuesta = MessageBox.Show(mensaje, "Anular Corte", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
                    if (respuesta == System.Windows.Forms.DialogResult.Yes)
                    {
                        oLineaVenta = new Entidades.LineaVenta();

                        oLineaVenta = new Entidades.LineaVenta();
                        oLineaVenta.Corte = oLineaVentaSelect.Corte;
                        oLineaVenta.Venta = oLineaVentaSelect.Venta;
                        oLineaVenta.CantKg = oLineaVentaSelect.CantKg * -1;
                        oLineaVenta.PrecioKg = oLineaVentaSelect.PrecioKg;
                        oLineaVenta.Estado = 1;//anulado
                        oLineaVenta.IndexAnulado = nroFila;
                        cargarListas();
                        cargarGrilla();

                        txtCodigo.Focus();
                    }
                }
                else
                {
                    MessageBox.Show("El corte seleccionado ya ha sido anulado.", "Anular corte", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }                
            }
            else
            {
                MessageBox.Show("No hay ninguna fila seleccionada.", "Seleccione un fila", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            oLineaVenta = null;
        }

        private void cargarCorte()
        {
            if (txtCodigo.Text.Trim() != "")
            {
                try
                {
                    oStockCorteSucursal = null;
                    oStockCorteSucursal = new Entidades.StockCorteSucursal();

                    oCorteE = null;
                    oCorteE = new Entidades.Corte();

                    DataTable dtCortes = new DataTable();
                    dtCortes = oCorteN.buscarCodigoCorte(Convert.ToInt32(txtCodigo.Text.Trim()));

                    if (dtCortes.Rows.Count > 0 )
                    {
                        foreach (DataRow fila in dtCortes.Rows)
                        {
                            if (Convert.ToInt32(fila["idSucursal"].ToString()) == oSucursalE.idSucursal)
	                        {
                                //cargo el corte
                                oCorteE.idCorte = Convert.ToInt32(fila["idCorte"].ToString());
                                oCorteE.codigo = Convert.ToInt32(fila["codigo"].ToString());
                                oCorteE.corte = fila["corte"].ToString();                                
                                oCorteE.precioKg = float.Parse(fila["precioKg"].ToString());

                                //cargo stock
                                oStockCorteSucursal.Corte = oCorteE;
                                oStockCorteSucursal.Sucursal = oSucursalE;

                                oStockCorteSucursal.Stock = float.Parse(fila["stock"].ToString());
	                           
                              }
                            
                        }
                        //cargo los campos
                        this.txtCodigo.Text = Convert.ToString(oCorteE.codigo);
                        this.txtCorte.Text = oCorteE.corte;
                        this.txtPrecioKg.Text = oCorteE.precioKg.ToString("N");
                    }
                    else
                    {
                        oCorteE = null;
                        this.txtTotalCorte.Text = "";
                        this.txtPrecioKg.Text = "";
                        this.txtCorte.Text = "";
                    }
                }
                catch (Exception ex)
                {

                    MessageBox.Show(ex.Message);
                    limpiarCamposCorte();
                }

            }
            else
            {
                precioKg = 0;
                totalCorte = 0;
                txtTotalCorte.Text = null;
                txtPrecioKg.Text = null;
            }
        }

        private void cargarTotalCorte()
        {
            if (!txtCantKgs.Text.Equals("") && Utilidades.Util_Form.validarCampoNumerico(txtCantKgs.Text, "Kgs"))
            {
                try
                {
                    //bool d = Utilidades.Util_Form.validarCampoNumerico(txtCantKgs.Text, "Kgs");
                    try
                    {
                        cantKg = float.Parse(txtCantKgs.Text.Trim(), System.Globalization.NumberStyles.Float, new System.Globalization.CultureInfo("en-US"));
                    }
                    catch (Exception)
                    {

                        cantKg = float.Parse(txtCantKgs.Text.Trim());
                    }

                    if (oCorteE != null)
                    {
                        try
                        {
                            precioKg = float.Parse(txtPrecioKg.Text.Trim(), System.Globalization.NumberStyles.Float, new System.Globalization.CultureInfo("en-US"));
                        }
                        catch (Exception)
                        {
                            try
                            {                                
                                precioKg = float.Parse(txtPrecioKg.Text.Trim());
                            }
                            catch (Exception)
                            {

                                if (checkLeerPeso.Checked)
                                {
                                    precioKg = 0;
                                }
                            }
                            
                        }
                    }
                    totalCorte = cantKg * precioKg;
                    //cargo el txt total corte
                    txtTotalCorte.Text = totalCorte.ToString("N");
                }
                catch (Exception ex)
                { 
                    if (txtCantKgs.Text.Trim() != "-" )
                    {
                        MessageBox.Show(ex.Message);
                    }                    
                }
            }

        }

        private void establecerPrecioKg()
        {
            if (!txtTotalCorte.Text.Equals(""))
            {
                try
                {
                    try
                    {
                        totalCorte = float.Parse(txtTotalCorte.Text.Trim(), System.Globalization.NumberStyles.Float, new System.Globalization.CultureInfo("en-US"));
                    }
                    catch (Exception)
                    {

                        totalCorte = float.Parse(txtTotalCorte.Text.Trim());
                    }

                    if (cantKg>0)
                    {
                        precioKg = totalCorte / cantKg;
                        txtPrecioKg.Text = precioKg.ToString();
                    }
                     
                    

                }
                catch (Exception ex)
                {

                    MessageBox.Show(ex.Message);
                }
            }


        }

        private void establecerTotalCorte()
        {
            if (!txtPrecioKg.Text.Equals(""))
            {
                try
                {
                    try
                    {
                        precioKg = float.Parse(txtPrecioKg.Text.Trim(), System.Globalization.NumberStyles.Float, new System.Globalization.CultureInfo("en-US"));
                    }
                    catch (Exception)
                    {

                        precioKg = float.Parse(txtPrecioKg.Text.Trim());
                    }
                    totalCorte = precioKg * cantKg;

                    if (Presentacion.FormPrincipal.logueado)
                    {
                        txtTotalCorte.Text = totalCorte.ToString();
                    }                   
  
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Para fijar el Precio/Kg debe ingresar un precio válido.");
                    txtPrecioKg.Text = "";
                }
            }
      }


        private void txtCantKgs_TextChanged(object sender, EventArgs e)
        {
            cargarTotalCorte();
        }

        private void txtPrecioKg_TextChanged(object sender, EventArgs e)
        {
            establecerTotalCorte();
        }

        private void btnAgregar_Click(object sender, EventArgs e)
        {
            agregarLinea();
        }

        private void btnQuitar_Click(object sender, EventArgs e)
        {
            quitarLinea();
        }

        private void salir()
        {
            if (grillaLineasVenta.SelectedRows.Count > 0)
            {
                string mensaje = "No se puede salir porque hay un venta en curso.\n\nFinalice la venta e inténtelo nuevamente.";
                MessageBox.Show(mensaje, "Salir", MessageBoxButtons.OK, MessageBoxIcon.Information);               
            }

            else
            {
                this.Close();
            }
        }

        private void btnBuscaCorte_Click(object sender, EventArgs e)
        {
            formBuscarCorte frmBuscarCorte = new formBuscarCorte();
            frmBuscarCorte.Show(this);
        }

        public void EnviarCorte(Entidades.Corte corte)
        {
            oCorteE = null;

            oCorteE = corte;

            this.txtCodigo.Text =Convert.ToString( oCorteE.codigo);
            this.txtCorte.Text = oCorteE.corte;
        }

        private void btnAceptar_Click(object sender, EventArgs e)
        {
            esModificacion();
        }

        private void btnCancelar_Click(object sender, EventArgs e)
        {
            salir();

        }        

        private void btnBuscarCliente_Click(object sender, EventArgs e)
        {
            formBuscarPersona frmBuscarPersona = new formBuscarPersona();
            frmBuscarPersona.Show(this);
        }

        public void EnviarPersona(Entidades.Persona persona)
        {
            oCliente = persona;
            this.txtCliente.Text = oCliente.razonSocial;
            
        }

        private void txtCodigo_TextChanged(object sender, EventArgs e)
        {
            cargarCorte();
        }

        private void label5_Click(object sender, EventArgs e)
        {

        }

        private void TxtPruebaENTER_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)(Keys.Enter))
            {
                e.Handled = true;

                SendKeys.Send("{TAB}");

            }

        }


        //const int WM_SYSCOMMAND = 0x0112;
        //const int SC_CLOSE = 0xF060;

        //protected override void WndProc(ref Message m)
        //{
        //    if ((m.Msg == WM_SYSCOMMAND) && (m.WParam == (IntPtr)SC_CLOSE))
        //    {
        //        DialogResult respuesta = MessageBox.Show("Si cierra el formulario se perderan los datos ingresados.\n¿Está seguro que desea salir?. ", "Salir de Nueva Venta", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);

        //        if ((respuesta == System.Windows.Forms.DialogResult.No))
        //        {
        //            return;
        //        } 
                
        //    }

        //    base.WndProc(ref m);
        //}

        private void grupoCortes_Enter(object sender, EventArgs e)
        {

        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            try
            {
                if (checkLeerPeso.Checked)
                {
                    txtCantKgs.Text = Leer_Peso.ObtenerPeso(); //"000.568";
                }
            }
            catch (Exception ex)
            {
                timer1.Enabled = false;
                DialogResult resp = MessageBox.Show("Error al leer peso de Balanza: " + ex.Message + ".\nVerifique la conexion.\n\n¿Dejar de leer el peso de la Balanza?", "Error balanza", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1);
                if (resp == DialogResult.Yes)
                {
                    checkLeerPeso.Checked = false;
                }
                else
                {
                    timer1.Enabled = true;
                }
            }
        }

        private void checkLeerPeso_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                if (checkLeerPeso.Checked)
                {
                    txtCantKgs.ReadOnly = true;
                    txtCantKgs.TabStop = false;
                    timer1.Enabled = true;
                    Leer_Peso.AbrirPuerto();
                }
                else
                {
                    txtCantKgs.Text = "";
                    txtCantKgs.ReadOnly = false;
                    txtCantKgs.TabStop = true;
                    timer1.Enabled = false;
                    Leer_Peso.CerrarPuerto();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void formNuevaVenta_Leave(object sender, EventArgs e)
        {
            Leer_Peso.CerrarPuerto();
        }

        private void formNuevaVenta_Deactivate(object sender, EventArgs e)
        {
            checkAnterior = checkLeerPeso.Checked;
            checkLeerPeso.Checked = false;
            Leer_Peso.CerrarPuerto();
        }

        private void formNuevaVenta_Activated(object sender, EventArgs e)
        {
            if (checkAnterior)
            {
                checkLeerPeso.Checked = true;
                Leer_Peso.AbrirPuerto();
            }
        }

        private void formVentaCaja_Load(object sender, EventArgs e)
        {
            
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            switch (keyData)
            {
                case Keys.End:
                    panelPago.Visible = true;
                    txtAbona.ReadOnly = false;
                    txtAbona.Focus();
                    break;
                case Keys.PageUp:
                    txtCodigo.Focus();
                    break;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void txtAbona_TextChanged(object sender, EventArgs e)
        {
            if (Utilidades.Util_Form.validarCampoNumerico(txtAbona.Text, "Abona") && txtAbona.Text != "")
            {
                totalVenta = float.Parse(txtTotalS.Text.Trim());
                abona = float.Parse(txtAbona.Text.Replace('.', ','));
                cambio = abona - totalVenta;         
            }
            else
            {
                txtAbona.Text = "";
                txtCambio.Text = "";
                abona = 0;
                cambio = 0;
            }
            txtCambio.Text = cambio.ToString("N2");
        }

       
        

       

        
        
      
        
    }
}
