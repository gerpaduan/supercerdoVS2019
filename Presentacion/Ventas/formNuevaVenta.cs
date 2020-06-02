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
    public partial class formNuevaVenta : Form, InterfaceCorte, InterfacePersona
    {
        Utilidades.SingletonLeerPeso Leer_Peso;
        #region variables
        formVentas frmVentas;
        DataTable dtSucursales;
        DataTable dtCortes = new DataTable();
        Negocio.Corte oCorteN = new Negocio.Corte();
        Negocio.Sucursal oSucursalN=new Negocio.Sucursal();
        Negocio.Venta oVentaN = new Negocio.Venta();
        Negocio.Usuario oUsuarioN = new Negocio.Usuario();

        Entidades.Compra oCompraE = new Entidades.Compra();
        Entidades.Persona oCliente;
        Entidades.Corte oCorteE;
        Entidades.Sucursal oSucursalE= new Entidades.Sucursal();
        Entidades.Sucursal oSucAnterior = new Entidades.Sucursal();
        Entidades.Venta oVentaE = new Entidades.Venta();
        Entidades.LineaVenta oLineaVenta;

        List<Entidades.LineaVenta> listaLineaVenta = new List<Entidades.LineaVenta>();
        List<LineaVenta> listaLineaGrilla = new List<LineaVenta>();

        bool dejarDeLeerPeso = false;
        bool aCtaCte = false;
        int sucAnterior;

        public int SucAnterior
        {
            get { return sucAnterior; }
            set { sucAnterior = value; }
        }

        bool modificar = false;
        bool cargandoDatos = false; //variable para evitar que se ejecuten funciones al cargar el form
        string fecha = "", estadoVenta="";
        float totalCorte, precioKg, cantKg;
        #endregion


        public formNuevaVenta()
        {
            InitializeComponent();
            timer1.Interval = Convert.ToInt32(ConfigurationManager.AppSettings["timerForm"].ToString());
            cargarComboVendedores();
            cargarSucursal();
            dtCortes = oCorteN.obtenerCortes();
            if (!fecha.Equals(""))
            {
                txtFechaVenta.Value = DateTime.Parse(fecha);
            }
        }

        private void cargarComboVendedores()
        {
            comboUsuario.DataSource = oUsuarioN.obtenerUsuarios();
            comboUsuario.DisplayMember = "nombre";
            comboUsuario.ValueMember = "id";
            comboUsuario.SelectedValue = 0;
        }

#region Modificar_Venta


        public void parametrosModificacion(formVentas frmVentasParam,Entidades.Venta oVentaParam, List<Entidades.LineaVenta> listaLineaVentaParam, List<LineaVenta> listaLineaGrillaParam)
        {
            modificar = true;
            cargandoDatos = true;
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
            comboUsuario.SelectedValue = oVentaE.Vendedor.Id;
            txtCliente.Text = oVentaE.Persona.razonSocial;
            comboSucursal.SelectedIndex = oVentaE.Sucursal.idSucursal - 1;
            checkCtaCte.Checked = oVentaE.EnCtaCte;
            
            txtFechaVenta.Value =oVentaE.FechaVenta;
            txtNroRemito.Text = oVentaE.NroRemito;
            txtCuit.Text = oVentaE.DiaFestivo;
            txtObservaciones.Text = oVentaE.Observaciones;
            txtCreado.Text = oVentaE.Creado.ToString();
            txtActualizado.Text = oVentaE.Actualizado >= oVentaE.Creado ? oVentaE.Actualizado.ToString() : "";

            checkEfectivo.BackColor = Utilidades.Util_Form.getBackColorCheckBox(false);
            checkDebito.BackColor = Utilidades.Util_Form.getBackColorCheckBox(false);
            checkCredito.BackColor = Utilidades.Util_Form.getBackColorCheckBox(false);
            switch (oVentaE.FormaPago)
            {
                case "Efectivo":
                    checkEfectivo.Checked = true;
                    break;
                case "Debito":
                    checkDebito.Checked = true;
                    break;
                case "Credito":
                    checkCredito.Checked = true;
                    break;
                default:
                    break;
            }
            estadoVenta = oVentaE.Estado;
        }      

        private void modificarVenta()
        {
            if (validacionFinal())
            {
                cargarVenta();
                try
                {
                    oVentaN.modificarVenta(oVentaE, SucAnterior, true);

                    foreach (Entidades.LineaVenta linea in listaLineaVenta)
                    {
                        oVentaN.agregarLineaVenta(linea);
                    }

                    aCtaCte = true;
                    frmVentas.cargarGrilla();
                    limpiarListas();
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
            if (FormPrincipal.logueado)
            {
                aCtaCte = false;
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
                    }
                    else
                    {
                        MessageBox.Show("No se ha cargado ningún corte en la venta. ", "No hay cortes cargados", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }

                try
                {
                    if (aCtaCte)
                    {
                        oVentaN.crearMovCtaCteVenta(oVentaE);

                        try
                        {                            
                            ///Se busca si la venta está asociada a un egreso de caja
                            ///si ultimo egreso de caja es negativo se lo crea
                            ///
                            Negocio.CierreCaja oCierreN = new Negocio.CierreCaja();
                            Entidades.EgresoCaja oEgresoCaja = oCierreN.findEgresoCajaByTablaYId(Entidades.EgresoCaja.tablas.Ventas.ToString(), oVentaE.IdVenta);
                            if (oEgresoCaja != null && !oEgresoCaja.Id.Equals(0) && oEgresoCaja.Monto < 0)
                            {
                                Caja.formVentaCaja frmCajaVenta = new Presentacion.Caja.formVentaCaja();
                                frmCajaVenta.egresoCajaPorCtaCte(oVentaE);
                            }
                        }
                        catch (Exception ex)
                        {

                            MessageBox.Show("Hubo un error y no se actualizó el Egreso de Caja.\n\nMas Info:" + ex.StackTrace);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Hubo un error y no se registró el movimiento en la Cta. Cta\n\n"+ex.Message);
                }
            }
            else
            {
                MessageBox.Show("No está logueado");
            }            
        }

        public void asigarFormVentas(formVentas frmVentasParam)
        {
            frmVentas = frmVentasParam;
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

                ///quitar controles según Login
                //if (frmVentas.Logueado == false)           
                if (Presentacion.FormPrincipal.logueado == false)   
                {
                    txtTotalCorte.Text = "";
                    txtTotalS.Text = "";
                    foreach (DataGridViewColumn col in grillaLineasVenta.Columns)
                    {
                        if (col.Name.Equals("totalS"))
                        {
                            col.Visible = false;
                        }
                    }
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
                    aCtaCte = true;

                    frmVentas.cargarGrilla();

                    limpiarListas();
                    txtFechaVenta.Focus();

                }
                catch (Exception ex)
                {

                    MessageBox.Show(ex.Message);
                }
            }
            
        }
        private void limpiarListas()
        {
            if (checkLimpiarCliente.Checked)
            {
                oCliente = new Entidades.Persona();
                EnviarPersona(oCliente);
            }

            txtNroRemito.Text = "";
            txtObservaciones.Text = "";
            listaLineaGrilla = new List<LineaVenta>(); 
            listaLineaVenta = new List<Entidades.LineaVenta>();
            grillaLineasVenta.DataSource = null;
            dtCortes = oCorteN.obtenerCortes();
            restablecerFormaDePago();
        }

        private void cambiarSucursal()
        {
            if (comboSucursal.SelectedIndex > -1)
            {
                  oSucursalE.idSucursal =comboSucursal.SelectedIndex +1 ;
                  cargarCorte();
            }
        }

        private void cargarVenta()
        {
            oVentaE.Persona = oCliente;

            //asigo sucursal a la venta            
            oVentaE.Sucursal = oSucursalE;

            Entidades.Usuario oUsuario = new Entidades.Usuario();
            oUsuario.Id = Convert.ToInt32(comboUsuario.SelectedValue.ToString());

            oVentaE.Vendedor = oUsuario;
            oVentaE.FechaVenta = txtFechaVenta.Value;
            oVentaE.DiaFestivo = txtCuit.Text.Trim();
            oVentaE.NroRemito = txtNroRemito.Text.Trim();
            oVentaE.Observaciones = txtObservaciones.Text.Trim();
            oVentaE.Estado = estadoVenta ;
            oVentaE.EnCtaCte = checkCtaCte.Checked;

            oVentaE.TipoComprobante = Convert.ToChar(comboTipoComprobante.SelectedItem);
            oVentaE.Cuit = txtCuit.Text;
            oVentaE.Email = txtEmail.Text;
            oVentaE.AcumRedondeoImporte = 0;//ganPesosTotRedondeo;
            oVentaE.AcumRedondeoKgs = 0;//ganKgsTotRedondeo;
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
            txtTotalKgs.Text = Convert.ToString(totalKgs);

            //si está logueado
            //if (frmVentas.Logueado)
            if (Presentacion.FormPrincipal.logueado)
            {
                txtTotalS.Text = Convert.ToString(totalPesos);
            }            
        }

        private void agregarLinea()
        {
            if (validarLinea())
            {
                try
                {
                    cargarLinea();

                    if (!existeCorte())
                    {
                        cargarGrilla();

                        limpiarCamposCorte();
                        oLineaVenta = null;

                        txtCodigo.Focus();
                    }
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
        
        private bool existeCorte()
        {
            //valida que no exista el corte
            bool existeCorte = false;
            int nroLinea=0;

            foreach (Entidades.LineaVenta linea in listaLineaVenta)
            {
                if (oLineaVenta.Corte.idCorte == linea.Corte.idCorte &&
                    oLineaVenta.Estado == linea.Estado)
                {
                    existeCorte = true;

                    nroLinea = listaLineaVenta.IndexOf(linea);
                }
            }

            if (existeCorte)
            {
                DialogResult respuesta;

                ///si el corte existe y está checkeado Sumar igual Corte
                ///sino pregunta
                if (checkSumarIgualCorte.Checked)
                {
                    respuesta = DialogResult.Yes;
                }
                else
                {
                    respuesta = MessageBox.Show("El corte ingresado ya existe.\n\n¿Desea sumar los Kg al corte ya ingresado?", "El corte ya existe", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
                }

                if (respuesta==DialogResult.Yes)
                {
                    sumarCorte(nroLinea);
                    existeCorte = false;
                    return false;
                }
                else
                {
                    limpiarCamposCorte();
                    return existeCorte;//true
                }
                
            }
            else
            {
                cargarListas();
                return existeCorte;//false
            }
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
            if (checkFijarPrecio.Checked == false)
            {
                txtPrecioKg.Text = "";
                txtTotalCorte.Text = "";
            }
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
            if (oCorteE == null || txtCodigo.Text.Trim() == "" || txtCantKgs.Text.Trim() == "" || txtPrecioKg.Text.Trim() == "")
            {
                if (oCorteE == null || txtCodigo.Text.Trim() == "")
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
                return false;
            }
            else
            {                
                if (cantKg >= 100)
                {
                    DialogResult respuesta;
                    respuesta = MessageBox.Show("¿Está seguro de ingresar una venta igual o más de 100 kg?.", "Venta mayor o igual a 100kg", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1);

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
                    return true;
                }
            }
        }

        private bool validacionFinal()
        {
            if (oVentaE.FormaPago == null)
            {
                MessageBox.Show("Seleccione una forma de pago.");
                return false;
            }
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
                if (txtCliente.Text.Trim() == "" || comboSucursal.SelectedValue == null)
                {
                    if (txtCliente.Text.Trim() == "")
                    {
                        mensaje += "\n" + "-Cliente";
                    }

                    if (comboSucursal.SelectedValue == null)
                    {
                        mensaje += "\n" + "-Sucursal";
                    }
                    MessageBox.Show(mensaje, "Completar campos", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return false;
                }
                else
                {
                    if (Utilidades.Util_Form.validarFecha(txtFechaVenta.Value, "Fecha"))
                    {
                        DialogResult respuesta;
                        respuesta = MessageBox.Show("Vefique que la Fecha, Sucursal y demás datos ingresados esté correctos.\n\n¿Están correctos?.", "Verificar datos ingresados", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);

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
                listaLineaVenta.RemoveAt(nroFila);//elimina objetos de las listas
                listaLineaGrilla.RemoveAt(nroFila);
            }
            else
            {
                MessageBox.Show("No hay ninguna fila seleccionada.", "Seleccione un fila", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            cargarGrilla();
        }

        private void cargarSucursal()
        {
            dtSucursales = new DataTable();
            oSucursalN = new Negocio.Sucursal();
            dtSucursales = oSucursalN.obtenerSucursales();
            comboSucursal.DataSource = dtSucursales;
            comboSucursal.DisplayMember = "sucursal";
            comboSucursal.ValueMember = "idSucursal";
            comboSucursal.SelectedItem = Convert.ToInt32(Utilidades.Conexion.getIdSucursalConexion());
        }


        private void cargarCorte()
        {
            oCorteE = null;
            this.txtCorte.Text = "";
            if (txtCodigo.Text.Trim() != "")
            {
                try
                {
                    if (dtCortes.Rows.Count > 0)
                    {
                        foreach (DataRow fila in dtCortes.Rows)
                        {
                            if (fila["codigo"].ToString().Equals(txtCodigo.Text))
                            {
                                //cargo el corte
                                oCorteE = new Entidades.Corte();
                                oCorteE.idCorte = Convert.ToInt32(fila["idCorte"].ToString());
                                oCorteE.codigo = Convert.ToInt32(fila["codigo"].ToString());
                                oCorteE.corte = fila["corte"].ToString();
                                oCorteE.precioKg = float.Parse(fila["precioKg"].ToString());

                                this.txtCorte.Text = oCorteE.corte;

                                //si está fijo precio kg
                                if (checkFijarPrecio.Checked)
                                {
                                    oCorteE.precioKg = float.Parse(txtPrecioKg.Text);
                                }
                                else
                                {
                                    this.txtPrecioKg.Text = Convert.ToString(oCorteE.precioKg);
                                }
                                break;
                            }
                        }
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
                if (!checkFijarPrecio.Checked)
                {
                    txtPrecioKg.Text = null;
                }                
            }
        }

        private void cargarTotalCorte()
        {
            if(!checkLeerPeso.Checked && !string.IsNullOrEmpty(txtCantKgs.Text) &&
                Utilidades.Util_Form.validarCampoNumerico(txtCantKgs.Text, "Kgs.")) //(!txtCantKgs.Text.Equals(""))
            {
                try
                {
                    cantKg = Utilidades.Util_Form.convertFloat(txtCantKgs.Text, true);

                    if (oCorteE != null)
                    {
                        if (Utilidades.Util_Form.validarCampoNumerico(txtPrecioKg.Text, "$/Kg"))
                            precioKg = Utilidades.Util_Form.convertFloat(txtPrecioKg.Text, true);
                    }
                    ///si está logueado
                    if (Presentacion.FormPrincipal.logueado)
                    {
                        totalCorte = cantKg * precioKg;
                        //cargo el txt total corte
                        txtTotalCorte.Text = totalCorte.ToString();
                    }
                    else
                    {
                        txtTotalCorte.Text = "";
                    }
                }
                catch (Exception ex)
                { 
                    if (txtCantKgs.Text.Trim() != "-")
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
                catch (Exception)
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
            DialogResult respuesta = MessageBox.Show("¿Está seguro de quitar el corte seleccionado?. ", "Quitar Corte", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
            if (respuesta == System.Windows.Forms.DialogResult.Yes)
            {
                quitarLinea();
            }            
        }

        private void salir()
        {
            if (grillaLineasVenta.SelectedRows.Count > 0)
            {
                DialogResult respuesta = MessageBox.Show("Si cierra el formulario se perderan los datos ingresados.\n¿Está seguro que desea salir?. ", "Salir de Nueva Venta", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);

                if (respuesta == System.Windows.Forms.DialogResult.Yes)
                {
                    this.Close();
                }               
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
            checkCtaCte.Visible = !oCliente.idPersona.Equals(Entidades.Parametros.idConsumidorFinal);
            checkCtaCte.Checked = oCliente.CtaCte;
            this.txtCliente.Text = oCliente.razonSocial;
            this.txtCuit.Text = oCliente.Cuit;
            this.txtTelefono.Text = oCliente.Telefono;
            this.txtEmail.Text = "";
        }

        private void txtCodigo_TextChanged(object sender, EventArgs e)
        {
            cargarCorte();
        }

        private void comboSucursal_TextChanged(object sender, EventArgs e)
        {
            cambiarSucursal();
        }

        private void TxtPruebaENTER_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)(Keys.Enter))
            {
                e.Handled = true;
                SendKeys.Send("{TAB}");
            }
        }


        const int WM_SYSCOMMAND = 0x0112;
        const int SC_CLOSE = 0xF060;

        protected override void WndProc(ref Message m)
        {
            if ((m.Msg == WM_SYSCOMMAND) && (m.WParam == (IntPtr)SC_CLOSE))
            {
                DialogResult respuesta = MessageBox.Show("Si cierra el formulario se perderan los datos ingresados.\n¿Está seguro que desea salir?. ", "Salir de Nueva Venta", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);

                if ((respuesta == System.Windows.Forms.DialogResult.No))
                {
                    return;
                }

            }

            base.WndProc(ref m);
        }

        private void formNuevaVenta_Load(object sender, EventArgs e)
        {
            this.Text += Utilidades.Conexion.getSucursalConexion();
            if (!FormPrincipal.logueado)
            {
                MessageBox.Show("No está logueado");
                this.Close();
            }

            comboTipoComprobante.SelectedIndex = 0; //Remito
            restablecerFormaDePago();
            cargandoDatos = false;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            try
            {
                if (checkLeerPeso.Checked)
                {
                    if (Convert.ToBoolean(ConfigurationManager.AppSettings["singleton"].ToString()))
                    {
                        Leer_Peso = Utilidades.SingletonLeerPeso.CrearLeerPeso();
                        txtCantKgs.Text = Leer_Peso.ObtenerPeso();
                    }
                    else
                    {
                        txtCantKgs.Text = Utilidades.Util_Form.leerPesoBalanza();
                    }
                }
            }
            catch (Exception ex)
            {
                timer1.Enabled = false;
                if (Utilidades.Util_Form.errorBalanza(ex.Message) == DialogResult.Yes)
                {
                    dejarDeLeerPeso = true;
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
                    dejarDeLeerPeso = false;
                    txtCantKgs.ReadOnly = true;
                    txtCantKgs.TabStop = false;
                    timer1.Enabled = true;
                }
                else
                {
                    txtCantKgs.Text = "";
                    txtCantKgs.ReadOnly = false;
                    txtCantKgs.TabStop = true;
                    timer1.Enabled = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void checkFijarPrecio_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                if (checkFijarPrecio.Checked)
                {
                    if (!txtPrecioKg.Text.Equals("") && Utilidades.Util_Form.validarCampoNumerico(txtPrecioKg.Text, "$/Kg"))
                    {
                        try
                        {
                            precioKg = Utilidades.Util_Form.convertFloat(txtPrecioKg.Text, true);
                            txtPrecioKg.ReadOnly = true;
                            txtTotalCorte.ReadOnly = true;

                        }
                        catch (Exception)
                        {
                            if (txtPrecioKg.Text.Trim() != "-")
                            {
                                checkFijarPrecio.Checked = false;
                                MessageBox.Show("Para fijar el Precio/Kg debe ingresar un precio válido.");
                            }
                        }
                    }
                    else
                    {
                        checkFijarPrecio.Checked = false;
                    }                    
                }
                else
                {
                    txtPrecioKg.ReadOnly = false;
                    txtPrecioKg.Focus();
                    txtTotalCorte.ReadOnly = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void checkCtaCte_CheckedChanged(object sender, EventArgs e)
        {
            checkCtaCte.BackColor = Utilidades.Util_Form.getBackColorCheckBox(checkCtaCte.Checked);
        }

        private void checkEfectivo_CheckedChanged(object sender, EventArgs e)
        {
            setFormaDePago();
            if (checkEfectivo.Checked)
            {
                checkDebito.Checked = checkCredito.Checked = false;
                oVentaE.FormaPago = Entidades.Venta.formaPagoEnum.Efectivo.ToString();
            }
        }

        private void checkDebito_CheckedChanged(object sender, EventArgs e)
        {
            setFormaDePago();

            if (checkDebito.Checked)
            {
                checkEfectivo.Checked = checkCredito.Checked = false;
                oVentaE.FormaPago = Entidades.Venta.formaPagoEnum.Debito.ToString();
            }
        }

        private void checkCredito_CheckedChanged(object sender, EventArgs e)
        {
            setFormaDePago();

            if (checkCredito.Checked)
            {
                checkEfectivo.Checked = checkDebito.Checked = false;
                oVentaE.FormaPago = Entidades.Venta.formaPagoEnum.Credito.ToString();
            }
        }

        private void setFormaDePago()
        {
            restablecerFormaDePago();
            checkEfectivo.BackColor = Utilidades.Util_Form.getBackColorCheckBox(checkEfectivo.Checked);
            checkDebito.BackColor = Utilidades.Util_Form.getBackColorCheckBox(checkDebito.Checked);
            checkCredito.BackColor = Utilidades.Util_Form.getBackColorCheckBox(checkCredito.Checked);
        }


        private void restablecerFormaDePago()
        {
            if (!cargandoDatos)
            {
                oVentaE.FormaPago = null;

                checkEfectivo.BackColor = Utilidades.Util_Form.getBackColorCheckBox(false);
                checkDebito.BackColor = Utilidades.Util_Form.getBackColorCheckBox(false);
                checkCredito.BackColor = Utilidades.Util_Form.getBackColorCheckBox(false);
            }
        }
    }
}
