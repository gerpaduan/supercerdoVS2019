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

namespace Presentacion.Ventas
{
    public partial class formNuevaVenta : Form, InterfaceCorte, InterfacePersona
    {
        bool checkAnterior = false;
        Utilidades.Leer_Peso Leer_Peso = new Utilidades.Leer_Peso();
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
        #endregion


        public formNuevaVenta()
        {
            InitializeComponent();
            cargarSucursal();
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
            comboSucursal.SelectedIndex = oVentaE.Sucursal.idSucursal - 1;
            
            txtFechaVenta.Value =oVentaE.FechaVenta;
            txtNroRemito.Text = oVentaE.NroRemito;
            txtDiaFestivo.Text = oVentaE.DiaFestivo;
            comboTurno.SelectedItem= oVentaE.Turno;
            txtObservaciones.Text = oVentaE.Observaciones;
            txtCreado.Text = oVentaE.Creado.ToString();
            txtActualizado.Text = oVentaE.Actualizado >= oVentaE.Creado ? oVentaE.Actualizado.ToString() : "";

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
                }
                else
                {
                    MessageBox.Show("No se ha cargado ningún corte en la venta. ", "No hay cortes cargados", MessageBoxButtons.OK, MessageBoxIcon.Information);

                }
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

                    frmVentas.cargarGrilla();

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
            txtNroRemito.Text = "";
            txtDiaFestivo.Text = "";
            txtObservaciones.Text = "";

            listaLineaGrilla = new List<LineaVenta>(); 
            listaLineaVenta = new List<Entidades.LineaVenta>();
            grillaLineasVenta.DataSource = null;
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
            oVentaE.Turno = comboTurno.SelectedItem.ToString();

            //asigo sucursal a la venta
            
            oVentaE.Sucursal = oSucursalE;

            oVentaE.FechaVenta = txtFechaVenta.Value;
            oVentaE.DiaFestivo = txtDiaFestivo.Text.Trim();
            oVentaE.NroRemito = txtNroRemito.Text.Trim();
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
            txtStock.Text = "";
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

            //try
            //{
            //    oLineaVenta.CantKg = float.Parse(txtCantKgs.Text.Trim(), System.Globalization.NumberStyles.Float, new System.Globalization.CultureInfo("en-US"));
            //}
            //catch (Exception)
            //{

            //    oLineaVenta.CantKg = float.Parse(txtCantKgs.Text.Trim());
            //}

            //try
            //{
            //    oLineaVenta.PrecioKg = float.Parse(txtPrecioKg.Text.Trim(), System.Globalization.NumberStyles.Float, new System.Globalization.CultureInfo("en-US"));
            //}
            //catch (Exception)
            //{

            //    oLineaVenta.PrecioKg = float.Parse(txtPrecioKg.Text.Trim());
            //}

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
                if (txtCliente.Text.Trim() == "" || comboSucursal.SelectedValue == null || comboTurno.SelectedIndex.Equals(-1))
                {
                    if (txtCliente.Text.Trim() == "")
                    {
                        mensaje += "\n" + "-Cliente";
                    }

                    if (comboSucursal.SelectedValue == null)
                    {
                        mensaje += "\n" + "-Sucursal";
                    }
                    if (comboTurno.SelectedIndex.Equals(-1))
                    {
                        mensaje += "\n" + "-Turno";
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
            comboSucursal.SelectedItem = null;
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
                        this.txtStock.Text =Convert.ToString(oStockCorteSucursal.Stock);

                        //si está fijo precio kg
                        if (checkFijarPrecio.Checked)
                        {
                            oCorteE.precioKg = float.Parse(txtPrecioKg.Text);
                        }
                        else
                        {
                            this.txtPrecioKg.Text = Convert.ToString(oCorteE.precioKg);
                        }
                    }

                    else
                    {

                        oCorteE = null;
                        this.txtTotalCorte.Text = "";
                        this.txtPrecioKg.Text = "";
                        this.txtCorte.Text = "";
                        this.txtStock.Text = "";
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
            if (!txtCantKgs.Text.Equals(""))
            {
                try
                {
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

                    ///si está logueado
                    //if (frmVentas.Logueado)
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
                    if (txtCantKgs.Text.Trim() != "-" )
                    {
                        MessageBox.Show(ex.Message);
                    }
                    //if (!txtCantKgs.Text.Equals("."))
                    //{
                    //    MessageBox.Show(ex.Message);
                    //}

                    //MessageBox.Show(ex.Message);
                    
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

        private void txtTotalCorte_TextChanged(object sender, EventArgs e)
        {
            //if (!checkFijarPrecio.Checked)
            //{
            //    establecerPrecioKg();
            //}
            //if (checkLeerPeso.Checked)
            //{
            //    establecerPrecioKg();
            //}
            
        }

        private void txtPrecioKg_TextChanged(object sender, EventArgs e)
        {
            establecerTotalCorte();
        }

        //private void txtTotalCorte_TextChanged(object sender, EventArgs e)
        //{
        //    establecerPrecioKg();
        //}

        //private void txtPrecioKg_TextChanged(object sender, EventArgs e)
        //{
        //    establecerTotalCorte();
        //}


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
            this.txtCliente.Text = oCliente.razonSocial;
            
        }

        private void txtCodigo_TextChanged(object sender, EventArgs e)
        {
            cargarCorte();
        }

        private void label5_Click(object sender, EventArgs e)
        {

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
            
            if (!frmVentas.Logueado)
            {
                
            }
        }

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
                    //Leer_Peso.AbrirPuerto();
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

        private void checkFijarPrecio_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                if (checkFijarPrecio.Checked)
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
                            txtPrecioKg.ReadOnly = true;
                            txtTotalCorte.ReadOnly = true;

                        }
                        catch (Exception ex)
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
                        MessageBox.Show("Para fijar el Precio/Kg debe ingresar un precio válido.");
                    }
                    
                }
                else
                {
                    //txtCantKgs.Text = "";
                    txtPrecioKg.ReadOnly = false;
                    txtTotalCorte.ReadOnly = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void label17_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {

        }

        

        
       

       
        

       

        
        
      
        
    }
}
