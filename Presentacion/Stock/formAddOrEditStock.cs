using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Presentacion.Cortes;
using Utilidades;
using System.Configuration;


namespace Presentacion
{
    public partial class formAddOrEditStock : Form, InterfaceCorte, InterfaceUsuario, InterfacePersona     
    {
        public formStock frmStock;
        Utilidades.SingletonLeerPeso Leer_Peso;
        Utilidades.Util_Form Util_Form = new Utilidades.Util_Form();
        
        DataTable dtCorte = new DataTable();
        DataTable dtSucursales;
        Negocio.Compra oCompraN=new Negocio.Compra();
        Negocio.Sucursal oSucursalN;
        Negocio.Corte oCorteN = new Negocio.Corte();
        Entidades.Compra oCompraE = new Entidades.Compra();
        Entidades.Persona oProvNuevaCompra;
        public Entidades.Usuario oUsuario;
        Entidades.Corte oCorteNuevaCompra;
        Entidades.CortePorCompra oCortePorCompra;
        Entidades.Sucursal oSucursalE = new Entidades.Sucursal();
        CortesPorCompra cortesPorCompra;
        float totalKgs = 0;

        string tipoCompra = "";

        public int idCompra = 0;
        public Entidades.Compra.tipoCompraEnum tipoCompraEnum;
        public Entidades.Compra.accion accion = Entidades.Compra.accion.Agregar;

        List<CortesPorCompra> listaCortesEnGrilla = new List<CortesPorCompra>();//Lista que se carga en la grilla

       List<Entidades.CortePorCompra> listaCortePorCompra = new List<Entidades.CortePorCompra>();

       bool ultimaValidacion = true;
       bool huboModificaciones = false;
       bool dejarDeLeerPeso = false;
       bool fijarPeso = Convert.ToBoolean(ConfigurationManager.AppSettings["fijarPeso"].ToString());

       Color enableColor = ColorTranslator.FromHtml(ConfigurationManager.AppSettings["enableColor"].ToString()); //SystemColors.Window;
       Color readOnlyColor = ColorTranslator.FromHtml(ConfigurationManager.AppSettings["readOnlyColor"].ToString());//SystemColors.ScrollBar;
       Color focusColor = ColorTranslator.FromHtml(ConfigurationManager.AppSettings["focusColor"].ToString());//Color.Orange;//Color.NavajoWhite;//Color.MediumAquamarine;
       Color ultimoColor = Color.Green;

       Entidades.Compra.tipoCompraEnum[] arrayTipo = {Entidades.Compra.tipoCompraEnum.IngresoStock, Entidades.Compra.tipoCompraEnum.EgresoStock, Entidades.Compra.tipoCompraEnum.CierreStock, Entidades.Compra.tipoCompraEnum.PesajeCortes, Entidades.Compra.tipoCompraEnum.AjusteStock};

        public formAddOrEditStock()
        {
            InitializeComponent();
        }

        private void formNuevaCompra_Load(object sender, EventArgs e)
        {       
            cargarComboSucursal();
            dtCorte = oCorteN.obtenerCortes();
            checkLeerPeso.Visible = FormPrincipal.logueado || Convert.ToBoolean(ConfigurationManager.AppSettings["leerPeso"].ToString());
            huboModificaciones = false;

            if (dtCorte.Rows.Count == 0)
            {
                MessageBox.Show("No se pudieron cargar los cortes.");
            }
            if (accion.Equals(Entidades.Compra.accion.Agregar))
            {
                timer1.Stop();
                logueoUsuario();
                if (oUsuario == null)
                {
                    this.Close();
                    return;
                }

                if (!validarAjusteStock())
                {
                    this.Close();
                    return;
                }

                oProvNuevaCompra = new Entidades.Persona();
                oProvNuevaCompra.idPersona = Entidades.Parametros.idIndefinido;
                oSucursalE.idSucursal = (int)comboSucursal.SelectedValue;

                btnVerNoCargados.Visible = tipoCompraEnum.Equals(Entidades.Compra.tipoCompraEnum.CierreStock);

                timer1.Start();
            }
            if (accion.Equals(Entidades.Compra.accion.Modificar))
            {
                timer1.Stop();
                oCompraE = oCompraN.findById_convertToCompra(idCompra);
                listaCortePorCompra = oCompraN.convertCortesPorCompraToList(idCompra);

                tipoCompraEnum = Entidades.Compra.tipoCompraToEnum(oCompraE.TipoCompra);
                oProvNuevaCompra = oCompraE.Proveedor;

                oSucursalE = oCompraE.Sucursal;
                comboSucursal.SelectedValue = oSucursalE.idSucursal;
                txtFechaCompra.Value = oCompraE.FechaCompra;
                txtProveedor.Text = oCompraE.Proveedor.razonSocial;
                txtKgsMedias.Text = oCompraE.KgsMedias.ToString();
                txtCantMedias.Text = oCompraE.CantMedias.ToString();
                txtObservaciones.Text = oCompraE.Observaciones;
                txtCreado.Text = Util_Form.fechaFormato24Horas(oCompraE.Creado);
                txtCreadoPor.Text = oCompraE.CreadoPor != null ? oCompraE.CreadoPor.Nombre : "-";
                txtActualizado.Text = oCompraE.Actualizado != null ? Util_Form.fechaFormato24Horas(oCompraE.Actualizado): "-";
                txtActualizadoPor.Text = oCompraE.ActualizadoPor != null ? oCompraE.ActualizadoPor.Nombre : "-";

                foreach (Entidades.CortePorCompra corte in listaCortePorCompra)
                {
                    cargarCorteEnGrilla(corte);
                }
                cargarGrilla();
                Sort("creado", SortOrder.Ascending);
                btnAceptar.Text = "&Modificar";
                btnCambiarAccion.Visible = false;
                txtFechaCompra.Enabled = false;
                comboSucursal.Enabled = false;
                groupBox1.Enabled = false;
                panelPesaje.Enabled = false;
                panelProveedor.Enabled = false;
                txtObservaciones.ReadOnly = true;
            }
            tipoCompra = Entidades.Compra.tipoCompraToString(tipoCompraEnum);
            panelPesaje.Visible = tipoCompraEnum.Equals(Entidades.Compra.tipoCompraEnum.PesajeCortes);
            panelProveedor.Visible = tipoCompraEnum.Equals(Entidades.Compra.tipoCompraEnum.PesajeCortes);
            btnVerPorcentaje.Visible = tipoCompraEnum.Equals(Entidades.Compra.tipoCompraEnum.PesajeCortes);
            panelEstadoAjusteStock.Visible = tipoCompraEnum.Equals(Entidades.Compra.tipoCompraEnum.PesajeCortes) && oCompraE.IdCompra > 0 && oCompraE.KgsMedias != null && oCompraE.CantMedias != null;
            cargarEstadoAjuste(oCompraE.Estado);
            txtUsuario.Text = oUsuario != null ? oUsuario.Nombre : "-";
            txtTipoAccion.Text = tipoCompra;
            huboModificaciones = false;
            idCompraLabel.Text = idCompra.ToString();
            setTituloForm();
        }

        public void cargarEstadoAjuste(string estadoAjStock)
        {
            lblEstadoAjuste.Text = estadoAjStock;
            switch (Entidades.Compra.estadoAjStockToEnum(estadoAjStock))
            {
                case Entidades.Compra.estadoAjusteStock.Actualizado:
                    lblEstadoAjuste.ForeColor = Color.Green;
                    break;
                case Entidades.Compra.estadoAjusteStock.NoActualizado:
                    lblEstadoAjuste.ForeColor = Color.Red;
                    break;
                case Entidades.Compra.estadoAjusteStock.NoRealizado:
                    lblEstadoAjuste.ForeColor = Color.Red;
                    break;
            }    
        }

        private bool validarAjusteStock()
        {
            bool resp = true;
            if (tipoCompraEnum.Equals(Entidades.Compra.tipoCompraEnum.AjusteStock))
            {
                resp = oUsuario != null && oUsuario.Admin;
                if (!resp)
                    MessageBox.Show("No tienes permiso para realizar Ajuste de Stock.", "Sin Permiso", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            return resp;
        }

        private void setTituloForm()
        {
            this.Text = (btnAceptar.Text.Contains("Guardar") ? accion.ToString() : "Info") + " " + tipoCompra;
            this.Text += Utilidades.Conexion.getSucursalConexion(); 
        }

        private void logueoUsuario()
        {
            this.BringToFront();
            Presentacion.Caja.FormLoginVendedor frmLogin = new Presentacion.Caja.FormLoginVendedor();
            frmLogin.ShowDialog(this);
        }

        public void EnviarUsuario(Entidades.Usuario usuario)
        {
            oUsuario = usuario;
        } 

        #region eventos

        //comunicación con interface
        public void EnviarCorte(Entidades.Corte corte)
        {
            oCorteNuevaCompra = corte;
            //this.txtCorteNuevaCompra.Text = oCorteNuevaCompra.corte;
            this.txtCodigo.Text = oCorteNuevaCompra.codigo.ToString();
            this.txtCodigo.Focus();
            btnBuscaCorte.UseVisualStyleBackColor = true;
        }

        private void btnBuscaCorte_Click(object sender, EventArgs e)
        {
            buscarCorte();
        }

        private void buscarCorte()
        {
            formBuscarCorte frmBuscarCorte = new formBuscarCorte();
            frmBuscarCorte.Show(this);
        }

        private void btnQuitar_Click(object sender, EventArgs e)
        {            
            quitarLinea();
        }

        private void btnAgregar_Click(object sender, EventArgs e)
        {
            agregarLinea();
            capturarPantalla();
        }

        private void btnCancelar_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnAceptar_Click(object sender, EventArgs e)
        {
            if (btnAceptar.Text.Contains("Modificar"))
            {
                logueoUsuario();
                if (oUsuario == null)
                {
                    return;
                }

                if (!validarAjusteStock())
                    return;
                
                if (Util_Form.validarFechaConAdmin(Presentacion.FormPrincipal.logueado || oUsuario.Admin, txtFechaCompra.Value, "Fecha") &&
                Util_Form.validarSucursal(Presentacion.FormPrincipal.logueado || oUsuario.Admin, Convert.ToInt32(comboSucursal.SelectedValue.ToString())))
                {
                    txtUsuario.Text = oUsuario.Nombre;
                    btnAceptar.Text = "&Guardar";
                    btnCambiarAccion.Visible = true;
                    btnVerNoCargados.Visible = tipoCompraEnum.Equals(Entidades.Compra.tipoCompraEnum.CierreStock);
                    setTituloForm();//se setea el titulo luego de cambiar el text a BtnAceptar 
                    txtFechaCompra.Enabled = true;
                    comboSucursal.Enabled = true;
                    groupBox1.Enabled = true;
                    panelPesaje.Enabled = true;
                    panelProveedor.Enabled = true;
                    txtObservaciones.ReadOnly = false;
                    timer1.Start();               
                }
            }
            else
            {
                agregarCompra();
            }
        }

        #endregion

        #region Métodos

        private void agregarCompra()
        {
            try
            {
                if (listaCortePorCompra.Count > 0 || accion.Equals(Entidades.Compra.accion.Modificar))
                {
                    if (validarAjusteStock() && Util_Form.validarFechaConAdmin(Presentacion.FormPrincipal.logueado || oUsuario.Admin, txtFechaCompra.Value, "Fecha") &&
                        Util_Form.validarSucursal(Presentacion.FormPrincipal.logueado || oUsuario.Admin, Convert.ToInt32(comboSucursal.SelectedValue.ToString()))
                        && Util_Form.validarFecha(txtFechaCompra.Value, "Fecha") && validaciónFinal())
                    {
                        cargarCompra();//se cargan datos de la compra
                        if (accion.Equals(Entidades.Compra.accion.Modificar))
                        {
                            oCompraN.modificarCompra(oCompraE);
                        }
                        if (accion.Equals(Entidades.Compra.accion.Agregar))
                        {
                            oCompraE.IdCompra = oCompraN.agregarCompra(oCompraE);
                        }
                        foreach (Entidades.CortePorCompra cortePorCompra in listaCortePorCompra)
                        {
                            cortePorCompra.Sucursal = oSucursalE;
                            oCompraN.agregarCortePorCompra(cortePorCompra);
                        }

                        //Se actualiza el estado del Pesaje
                        if (oCompraE.TipoCompra.Equals(Entidades.Compra.tipoCompraToString(Entidades.Compra.tipoCompraEnum.PesajeCortes)))
                        {
                            //se verifica que el pesaje sea de medias
                            if ( (!oCompraE.KgsMedias.Equals(null) && oCompraE.KgsMedias > 0) && 
                                (!oCompraE.CantMedias.Equals(null) && oCompraE.CantMedias > 0))
                            {
                                oCompraN.actualizarEstadoPesaje(oCompraE.IdCompra, oCompraN.estadoAjusteStock(oCompraE.IdCompra, 0));
                                    //(accion.Equals(Entidades.Compra.accion.Agregar) ? 
                                    //Entidades.Compra.estadoAjusteStock.NoRealizado : Entidades.Compra.estadoAjusteStock.NoActualizado));
                            }
                        }

                        if (frmStock != null)
                        {
                            frmStock.cargarGrilla();
                        }

                        huboModificaciones = false;
                        this.Close();
                        //limpiarListas();
                    }
                }
                else
                {
                    MessageBox.Show("No hay cargado ningún registro.", "No hay registros cargados", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void limpiarListas()
        {
            //limpio campos
            txtCantItems.Text = "0";
            txtTotalKgs.Text = "0";
            txtObservaciones.Text = "";            
            listaCortesEnGrilla = new List<CortesPorCompra>();//Lista que se carga en la grilla
            listaCortePorCompra = new List<Entidades.CortePorCompra>();
            grillaCortePorCompra.DataSource = null;                        
        }

        private void cargarCompra()
        {
            //oCompraE.NroRemito = "";
            oCompraE.Proveedor = oProvNuevaCompra;
            oCompraE.FechaCompra = txtFechaCompra.Value;
            oCompraE.Estado = "";
            oCompraE.Observaciones = txtObservaciones.Text;
            oCompraE.TipoCompra = tipoCompra;
            oCompraE.CantMedias = string.IsNullOrEmpty(txtCantMedias.Text) ? null : (int?)Convert.ToInt32(txtCantMedias.Text);
            oCompraE.KgsMedias = string.IsNullOrEmpty(txtKgsMedias.Text) ? null : (int?)Convert.ToInt32(txtKgsMedias.Text);
            oCompraE.Sucursal = oSucursalE;
            switch (oCompraE.IdCompra)
            {
                case 0:
                    oCompraE.CreadoPor = oUsuario;
                    break;
	            default:
                    oCompraE.ActualizadoPor = oUsuario;
                    break;
            }
        }

        private void quitarLinea()
        {
            DialogResult resp = MessageBox.Show("¿Quitar el ítem seleccionado?", "Quitar", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);

            if (resp == DialogResult.Yes)
            {
                quitarCorte();
                cargarGrilla();
                capturarPantalla();
            }
        }

        private void quitarCorte()
        {
            try
            {
                if (grillaCortePorCompra.SelectedRows.Count > 0 || grillaCortePorCompra.CurrentRow != null)
                {
                    huboModificaciones = true;
                    int nroFila = grillaCortePorCompra.Rows.GetFirstRow(DataGridViewElementStates.Selected);//obtiene nro de fila de la grilla
                    listaCortePorCompra.RemoveAt(nroFila);//elimina objetos de las listas
                    listaCortesEnGrilla.RemoveAt(nroFila);
                }
                else
                {
                    MessageBox.Show("No hay ninguna fila seleccionada.", "Seleccione un fila", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }            
        }
        private void agregarLinea()
        {
            if (validarCampos())
            {
                huboModificaciones = true;
                agregarCorte();
                txtCodigo.Focus();
                limpiarCampos();
                Sort("creado", SortOrder.Ascending);
            }
        }

        private void agregarCorte()
        {
            cargarCortesPorCompra();
            cargarGrilla();            
        }

        private void capturarPantalla()
        {
            //se refresca para que se muestren los datos
            this.Refresh();
            Util_Form.capturarPantalla(txtTipoAccion.Text, txtFechaCompra.Value);        
        }

        //carga textBox de totales
        private void cargarTotales()
        {
            totalKgs = 0;
            foreach (CortesPorCompra fila in listaCortesEnGrilla)
            {
                //sumo totales
                totalKgs = totalKgs + fila.cantKgs;
            }
            //cargo Totales
            txtCantItems.Text = grillaCortePorCompra.Rows.Count.ToString();
            txtTotalKgs.Text = totalKgs.ToString("F3");
        }

        private int validarCorteEnGrilla()
        {
            int nroFila = -1;//si corte no está cargado
            foreach (Entidades.CortePorCompra corte in listaCortePorCompra)
            {
                if (corte.corte.idCorte == oCorteNuevaCompra.idCorte)
                {
                    string mensaje = "Ya se han cargado " + corte.CantKgs.ToString() + "Kgs a " + corte.corte.corte + "\n\n¿Desea sumar los " + oCortePorCompra.CantKgs.ToString() + " Kgs ingresados?";
                    DialogResult resp = MessageBox.Show(mensaje, "Corte ya cargado", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1);

                    if (resp == DialogResult.Yes)
                    {
                        MessageBox.Show("Se sumaron correctamente los " + oCortePorCompra.CantKgs.ToString() + " Kgs a "+ corte.corte.corte);
                        nroFila = listaCortePorCompra.IndexOf(corte);//se envía el index de la lista para sumar los kg al corte ya ingresado
                    }
                    else
                    {
                        nroFila = -2;//si está cargado y no se quiere volver a cargar
                    }
                }
            }
            return nroFila;
        }


        //cargar Cortes y Grilla
        private void cargarCortesPorCompra()
        {
            try 
	        {
                ultimaValidacion = true;
                //creo y Cargar la Entidad CortePorCompra
                oCortePorCompra = new Entidades.CortePorCompra();

                oCortePorCompra.corte = oCorteNuevaCompra;
                cargarCompra();//cargo datos en oCompraE
                oCortePorCompra.compra = oCompraE;
                oCortePorCompra.cantKgs = Util_Form.convertFloat(txtCantKgs.Text, true); //float.Parse(txtCantKgs.Text.Trim(), System.Globalization.NumberStyles.Float, new System.Globalization.CultureInfo("en-US"));
                oCortePorCompra.precioKg = float.Parse("0.00");
                oCortePorCompra.Creado = DateTime.Now;
                oCortePorCompra.CreadoPor = oUsuario;

                if (ultimaValidacion)
                {
                    if (tipoCompraEnum.Equals(Entidades.Compra.tipoCompraEnum.EgresoStock))
                    {
                        oCortePorCompra.CantKgs = oCortePorCompra.CantKgs * -1;
                    }

                    //oSucursalE = new Entidades.Sucursal(); //creo objeto sucursal
                    //oSucursalE.IdSucursal = Convert.ToInt32(comboSucursal.SelectedValue.ToString());

                    if (oSucursalE.IdSucursal == 1)
                    {
                        oSucursalE.SucursalNombre = "San Lorenzo";
                    }
                    else
                    {
                        oSucursalE.SucursalNombre = "San Martín";
                    }

                    oCortePorCompra.sucursal = oSucursalE;

                    listaCortePorCompra.Add(oCortePorCompra);

                    //creo CortesPorCompra y cargo la lista de la grilla
                    cargarCorteEnGrilla(oCortePorCompra);
                }
	        }
	        catch (Exception ex)
	        {
                MessageBox.Show("Hubo un error al cargar el corte.\n\nMensaje de exception: " + ex.Message);
	        }
        }

        private void cargarCorteEnGrilla(Entidades.CortePorCompra oCortePorCompra)
        {
            cortesPorCompra = new CortesPorCompra();

            cortesPorCompra.Index = oCortePorCompra.IdCortePorCompra;
            cortesPorCompra.codigo = oCortePorCompra.corte.codigo;
            cortesPorCompra.corte = oCortePorCompra.corte.corte;
            cortesPorCompra.cantKgs = oCortePorCompra.cantKgs;
            cortesPorCompra.precioKg = oCortePorCompra.precioKg;
            cortesPorCompra.totalS = oCortePorCompra.precioKg * cortesPorCompra.cantKgs;
            cortesPorCompra.sucursal = oCortePorCompra.sucursal.SucursalNombre;
            cortesPorCompra.Creado = oCortePorCompra.Creado;

            listaCortesEnGrilla.Add(cortesPorCompra);

            oCortePorCompra = null;
            cortesPorCompra = null;
        }

        private bool validarCampos()
        {
            if (txtCorteNuevaCompra.Text.Equals(""))
            {
                MessageBox.Show("El corte ingresado no existe.", "Ingrese un producto", MessageBoxButtons.OK, MessageBoxIcon.Information);
                txtCodigo.Focus();
                return false;
            }
            else
            {
                if (!Utilidades.Util_Form.validarCampoNumerico(txtCantKgs.Text, "Kgs."))
                {
                    txtCantKgs.Focus();
                    return false;
                }
                else
                {
                    float cantKgs = Utilidades.Util_Form.convertFloat(txtCantKgs.Text, true);
                    if (!tipoCompraEnum.Equals(Entidades.Compra.tipoCompraEnum.AjusteStock) && cantKgs <= 0)
                    {
                        MessageBox.Show("Ingrese una cantidad de Kgs mayor a 0 (cero).", "Ingrese una cantidad", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        txtCantKgs.Focus();
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }
            }
        }

        private bool validaciónFinal()
        {
            //valida si se modifico CantKgs y CantMedias
            huboModificaciones = (!huboModificaciones && txtKgsMedias.Text.Equals(oCompraE.KgsMedias.ToString()) && 
                txtCantMedias.Text.Equals(oCompraE.CantMedias.ToString()) && txtObservaciones.Text.Equals(oCompraE.Observaciones)) ? false : true;

            if (!huboModificaciones)
            {
                MessageBox.Show("No se realizaron modificaciones.\n\nPresione el boton Cancelar para salir sin realizar modificaciones");
                return false;
            }
            DialogResult respuesta;
            string pregunta = oCompraE != null && oCompraE.IdCompra > 1 ? " las modificaciones realizadas en" : " el nuevo ";
            respuesta = MessageBox.Show("¿Guardar "+pregunta+" "+ Entidades.Compra.tipoCompraToString(tipoCompraEnum)+" ?", "Guardar datos", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);

            if (respuesta == DialogResult.Yes)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private void limpiarCampos()
        {
            oCorteNuevaCompra=null;
            txtCodigo.Text = "";
            txtCorteNuevaCompra.Text = "";
            txtCantKgs.Text = "";        
        }

        private void cargarGrilla()
        {
            try
            {                
                grillaCortePorCompra.AutoGenerateColumns = false;
                grillaCortePorCompra.DataSource = null;
                grillaCortePorCompra.DataSource = listaCortesEnGrilla;

                if (listaCortesEnGrilla.Count > 0)
                {
                    grillaCortePorCompra.Rows[listaCortesEnGrilla.Count - 1].Selected = true;
                    grillaCortePorCompra.FirstDisplayedScrollingRowIndex = listaCortesEnGrilla.Count - 1;
                }

                cargarTotales();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
      }

        private void cargarCorte()
        {
            try
            {
                oCorteNuevaCompra = null;
                txtCorteNuevaCompra.Text = "";

                if (txtCodigo.Text.Trim() != "")
                {
                    oCorteNuevaCompra = new Entidades.Corte();

                    if (dtCorte.Rows.Count > 0)
                    {
                        foreach (DataRow fila in dtCorte.Rows)
                        {
                            if (fila["codigo"].ToString().Equals(txtCodigo.Text))
                            {
                                oCorteNuevaCompra.idCorte = Convert.ToInt32(fila["idCorte"].ToString());
                                oCorteNuevaCompra.codigo = Convert.ToInt32(fila["codigo"].ToString());
                                oCorteNuevaCompra.corte = fila["corte"].ToString();
                                oCorteNuevaCompra.tipo = fila["tipo"].ToString();
                                break;
                            }
                        }
                        //se cargan los datos del corte
                        txtCorteNuevaCompra.Text = oCorteNuevaCompra.corte;
                    }
                }
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }
        }

        private void cargarComboSucursal()
        {
            dtSucursales = new DataTable();
            oSucursalN=new Negocio.Sucursal();
            dtSucursales=oSucursalN.obtenerSucursales();
            comboSucursal.DataSource = dtSucursales;
            comboSucursal.DisplayMember = "sucursal";
            comboSucursal.ValueMember = "idSucursal";

            comboSucursal.SelectedIndex = Convert.ToInt32(Utilidades.Conexion.getIdSucursalConexion()) - 1;
        }

        #endregion

        private void TxtPruebaENTER_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)(Keys.Enter))
            {
                e.Handled = true;
                SendKeys.Send("{TAB}");
            }
        }

        //Métodos autocompletar
        public  AutoCompleteStringCollection LoadAutoComplete()
        {
            AutoCompleteStringCollection cortes = new AutoCompleteStringCollection();
            
            foreach (DataRow fila in dtCorte.Rows)
            {
                //cortes.Add(fila["codigo"].ToString());
                cortes.Add(fila["corte"].ToString());

                //crear un corte nuevo y lo envia al metodo EnviarCorte                
                if (txtCorteNuevaCompra.Text.Trim() == fila["corte"].ToString())
                {
                    if (!txtCodigo.Text.Equals(fila["codigo"].ToString()))
                    {
                        txtCodigo.Text = fila["codigo"].ToString();                        
                    }
                    break;
                }
            }
            return cortes;
        }

        private void txtCorteNuevaCompra_TextChanged(object sender, EventArgs e)
        {
            txtCorteNuevaCompra.AutoCompleteCustomSource = LoadAutoComplete();
        }

        private void txtCodigo_TextChanged(object sender, EventArgs e)
        {
            cargarCorte();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            try
            {
                if (!FormPrincipal.leerBalanza) checkLeerPeso.Checked = false;

                if (checkLeerPeso.Checked)
                {
                    if (fijarPeso)
                    {
                        txtCantKgs.Text = "1.500";
                    }
                    else
                    {
                        if (Convert.ToBoolean(ConfigurationManager.AppSettings["singleton"].ToString()))
                        {
                            Leer_Peso = Utilidades.SingletonLeerPeso.CrearLeerPeso();
                            txtCantKgs.Text = Leer_Peso.ObtenerPeso();
                        }
                        else
                        {
                            txtCantKgs.Text = Utilidades.Util_Form.leerPesoBalanza();
                            lblErrorBalanza.Visible = false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                txtCantKgs.Text = "Error balanza";
                lblErrorBalanza.Text = ex.Message;
                lblErrorBalanza.Visible = true;          
                //timer1.Enabled = false;
                //if (Utilidades.Util_Form.errorBalanza(ex.Message) == DialogResult.Yes)
                //{
                //    dejarDeLeerPeso = true;
                //    checkLeerPeso.Checked = false;
                //}
                //else
                //{
                //    timer1.Enabled = true;
                //}
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
                    txtCantKgs.BackColor = readOnlyColor;
                    txtCantKgs.TabStop = false;
                    txtCodigo.Focus();
                    timer1.Enabled = true;
                }
                else
                {
                    txtCantKgs.Text = "";
                    txtCantKgs.ReadOnly = false;
                    txtCantKgs.TabStop = true;
                    txtCantKgs.Focus();
                    lblErrorBalanza.Visible = false;
                    timer1.Enabled = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnAceptar_Enter(object sender, EventArgs e)
        {
            btnAceptar.BackColor = System.Drawing.Color.FromName("LimeGreen");
        }

        private void btnAceptar_Leave(object sender, EventArgs e)
        {
            btnAceptar.BackColor = System.Drawing.Color.FromName("SeaGreen");
        }

        private void comboSucursal_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!comboSucursal.ValueMember.Equals(""))
            {
                oSucursalE.idSucursal = Convert.ToInt32(comboSucursal.SelectedValue);
                huboModificaciones = true;
            }
        }

        private void txtFechaCompra_ValueChanged(object sender, EventArgs e)
        {
            huboModificaciones = true;
        }

        private void txtObservaciones_TextChanged(object sender, EventArgs e)
        {
            huboModificaciones = true;
        }

        private void formAddOrEditStock_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = salir();
        }

        private bool salir()
        {
            if (huboModificaciones)
            {
                DialogResult respuesta = MessageBox.Show("Si cierra el formulario se perderan las modificaciones realizadas.\n¿Está seguro que desea salir?. ", "Stock", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);

                if ((respuesta == System.Windows.Forms.DialogResult.Yes))
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            else
            {
                return false;
            }
        }

        private void txtCodigo_Leave(object sender, EventArgs e)
        {
            txtCodigo.BackColor = enableColor;
            if (oCorteNuevaCompra != null && oCorteNuevaCompra.idCorte > 0 && oCorteNuevaCompra.tipo.Equals("Unidad") && checkLeerPeso.Checked)
            {
                checkLeerPeso.Checked = false;
                txtCantKgs.Focus();
            }
            else
            {
                if (!dejarDeLeerPeso && oCorteNuevaCompra != null && oCorteNuevaCompra.idCorte > 0 && !oCorteNuevaCompra.tipo.Equals("Unidad") && !checkLeerPeso.Checked)
                {
                    checkLeerPeso.Checked = true;
                    btnAgregar.Focus();
                }
            }
        }

        private void txtCodigo_Enter(object sender, EventArgs e)
        {
            if (sender is TextBox )
            {
                TextBox objectToChangeColor = (TextBox)sender;
                if (!objectToChangeColor.BackColor.Equals(focusColor)) ultimoColor = objectToChangeColor.BackColor;
                objectToChangeColor.BackColor = focusColor;
                return;
            } 

            if (sender is MaskedTextBox)
            {
                MaskedTextBox objectToChangeColor = (MaskedTextBox)sender;
                if (!objectToChangeColor.BackColor.Equals(focusColor)) ultimoColor = objectToChangeColor.BackColor;
                objectToChangeColor.BackColor = focusColor;
                return;
            }

            if (sender is Button)
            {
                Button objectToChangeColor = (Button)sender;
                objectToChangeColor.BackColor = focusColor;
                return;
            }
        }

        private void control_Leave(object sender, EventArgs e)
        {
            if (sender is TextBox)
            {
                TextBox objectToChangeColor = (TextBox)sender;
                objectToChangeColor.BackColor = ultimoColor;
                return;
            }

            if (sender is MaskedTextBox)
            {
                MaskedTextBox objectToChangeColor = (MaskedTextBox)sender;
                objectToChangeColor.BackColor = ultimoColor;
                return;
            }

            if (sender is Button)
            {
                Button objectToChangeColor = (Button)sender;
                objectToChangeColor.UseVisualStyleBackColor = true;
                return;
            }
        }

        private void grillaCortePorCompra_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            DataGridView grid = (DataGridView)sender;
            SortOrder so = SortOrder.None;
            if (grid.Columns[e.ColumnIndex].HeaderCell.SortGlyphDirection == SortOrder.None ||
                grid.Columns[e.ColumnIndex].HeaderCell.SortGlyphDirection == SortOrder.Ascending)
            {
                so = SortOrder.Descending;
            }
            else
            {
                so = SortOrder.Ascending;
            }
            //set SortGlyphDirection after databinding otherwise will always be none 
            Sort(grid.Columns[e.ColumnIndex].Name, so);
            //listaCortesEnGrilla.Clear();
            //foreach (Entidades.CortePorCompra corteOrdenado in listaCortePorCompra)
            //{
            //    cargarCorteEnGrilla(corteOrdenado);
            //}
            //cargarGrilla();
            //grid.Columns[e.ColumnIndex].HeaderCell.SortGlyphDirection = so;
        }
        /// <summary>
        /// Sort the DataGridView
        /// </summary>
        /// <param name="column"></param>
        /// <param name="sortOrder"></param>
        private void Sort(string column, SortOrder sortOrder)
        {
            switch (column)
            {
                case "Codigo":
                {
                    if (sortOrder == SortOrder.Ascending)
                    {
                        listaCortePorCompra = listaCortePorCompra.OrderBy(x => x.Corte.codigo).ToList();
                        //grillaCortePorCompra.DataSource = listaCortesEnGrilla.OrderBy(x => x.codigo).ToList();
                    }
                    else
                    {
                        listaCortePorCompra = listaCortePorCompra.OrderByDescending(x => x.Corte.codigo).ToList();
                        //grillaCortePorCompra.DataSource = listaCortesEnGrilla.OrderByDescending(x => x.codigo).ToList();
                    }
                    break;
                }
                case "Corte":
                {
                    if (sortOrder == SortOrder.Ascending)
                    {
                        listaCortePorCompra = listaCortePorCompra.OrderBy(x => x.Corte.corte).ToList();
                    }
                    else
                    {
                        listaCortePorCompra = listaCortePorCompra.OrderByDescending(x => x.Corte.corte).ToList();
                    }
                    break;
                }
                case "cantKgs":
                {
                    if (sortOrder == SortOrder.Ascending)
                    {
                        listaCortePorCompra = listaCortePorCompra.OrderBy(x => x.cantKgs).ToList();
                    }
                    else
                    {
                        listaCortePorCompra = listaCortePorCompra.OrderByDescending(x => x.cantKgs).ToList();
                    }
                    break;
                }
                case "creado":
                {
                    if (sortOrder == SortOrder.Ascending)
                    {
                        listaCortePorCompra = listaCortePorCompra.OrderBy(x => x.Creado).ToList();
                    }
                    else
                    {
                        listaCortePorCompra = listaCortePorCompra.OrderByDescending(x => x.Creado).ToList();
                    }
                    break;
                }
            }
            listaCortesEnGrilla.Clear();
            foreach (Entidades.CortePorCompra corteOrdenado in listaCortePorCompra)
            {
                cargarCorteEnGrilla(corteOrdenado);
            }
            cargarGrilla();
            grillaCortePorCompra.Columns[column].HeaderCell.SortGlyphDirection = sortOrder;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            comboTipo.DataSource = arrayTipo;
            for (int index = 0; index < arrayTipo.Length; index++)
            {
                if (arrayTipo[index] == tipoCompraEnum)
                {
                    comboTipo.SelectedIndex = index;
                    break;
                }
            }
            //comboTipo.SelectedIndex = arrayTipo.Select(element => element.Equals(tipoCompraEnum));
            //comboTipo.Text = Entidades.Compra.tipoCompraToString(tipoCompraEnum);
            comboTipo.Visible = !comboTipo.Visible;
        }

        private void comboTipo_TextChanged(object sender, EventArgs e)
        {
            if (!comboTipo.Visible) return;
            if (arrayTipo[comboTipo.SelectedIndex] != tipoCompraEnum)
            {
                DialogResult resp = MessageBox.Show("¿Desea cambiar de /" + 
                    Entidades.Compra.tipoCompraToString(tipoCompraEnum) + "/ a /" +
                    Entidades.Compra.tipoCompraToString(arrayTipo[comboTipo.SelectedIndex]) + "/?", "Cambiar acción", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);

                if (resp == DialogResult.Yes)
                {
                    tipoCompraEnum = arrayTipo[comboTipo.SelectedIndex];
                    tipoCompra = Entidades.Compra.tipoCompraToString(tipoCompraEnum);
                    txtTipoAccion.Text = tipoCompra;
                    btnVerNoCargados.Visible = tipoCompraEnum.Equals(Entidades.Compra.tipoCompraEnum.CierreStock);
                    //si no hubo modificaciones si comprueba si hubo cambio en la accion
                    if (!huboModificaciones) huboModificaciones = oCompraE != null && oCompraE.IdCompra > 0 && !oCompraE.TipoCompra.Equals(tipoCompra);
                    setTituloForm();

                    //panel de pesaje para ingresar Kg y Cant Medias
                    panelPesaje.Visible = tipoCompraEnum.Equals(Entidades.Compra.tipoCompraEnum.PesajeCortes);
                    panelPesaje.Enabled = tipoCompraEnum.Equals(Entidades.Compra.tipoCompraEnum.PesajeCortes);
                    btnVerPorcentaje.Visible = tipoCompraEnum.Equals(Entidades.Compra.tipoCompraEnum.PesajeCortes);

                    panelProveedor.Visible = tipoCompraEnum.Equals(Entidades.Compra.tipoCompraEnum.PesajeCortes);
                    panelProveedor.Enabled = tipoCompraEnum.Equals(Entidades.Compra.tipoCompraEnum.PesajeCortes);

                    for (int index = 0; index < listaCortePorCompra.Count; index++)
                    {
                        switch (tipoCompraEnum)
                        {
                            case Entidades.Compra.tipoCompraEnum.EgresoStock:
                                if(listaCortePorCompra[index].cantKgs > 0)
                                    listaCortePorCompra[index].cantKgs = listaCortePorCompra[index].cantKgs * -1;
                                if (listaCortesEnGrilla[index].cantKgs > 0)
                                    listaCortesEnGrilla[index].cantKgs = listaCortesEnGrilla[index].cantKgs * -1;
                                break;
                            default:
                                if (listaCortePorCompra[index].cantKgs < 0)
                                    listaCortePorCompra[index].cantKgs = listaCortePorCompra[index].cantKgs * -1;
                                if (listaCortesEnGrilla[index].cantKgs < 0)
                                    listaCortesEnGrilla[index].cantKgs = listaCortesEnGrilla[index].cantKgs * -1;
                                break;
                        }
                    }
                    cargarGrilla();
                }
            }
            comboTipo.Visible = false;
        }

        private void btnComprobar_Click(object sender, EventArgs e)
        {
            panelGrillaFaltantes.Visible = !panelGrillaFaltantes.Visible;
            this.panelGrillaFaltantes.BringToFront();
            btnVerNoCargados.Visible = !panelGrillaFaltantes.Visible;
            comprobarStock();
        }

        //se muestran aquellos cortes que no han sido cargados aún en el cierre stock
        private void comprobarStock()
        {
            if (grillaSinStock.Visible)
            {
                grillaSinStock.AutoGenerateColumns = false;
                DataTable dtCortesSinStock = dtCorte.Clone();
                foreach (DataRow corte in dtCorte.Rows)
                {
                    int codigoSelect = Convert.ToInt32(corte["codigo"].ToString());
                    if (Convert.ToBoolean(corte["enCierreStock"]))
                    {
                        var selected = listaCortePorCompra.Where(c => c.corte.codigo.Equals(codigoSelect));

                        List<Entidades.CortePorCompra> selectedCollection = selected.ToList();
                        if (selectedCollection.Count == 0)
                        {
                            dtCortesSinStock.ImportRow(corte);
                        }
                    }
                }
                grillaSinStock.DataSource = dtCortesSinStock;
            }
        }

        private void grillaSinStock_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            // Ignore clicks that are not on button cells.  
            if (e.RowIndex < 0 || e.ColumnIndex !=
                grillaSinStock.Columns["btnSinStock"].Index) return;

            // Retrieve the Employee object from the "Assigned To" cell.
            string codigoCorteSelect = grillaSinStock.Rows[e.RowIndex].Cells[0].Value.ToString();

            cargarCorteSinStock(codigoCorteSelect);

            //se establece la seleccion de la fila
            int selectRow = e.RowIndex;

            switch (grillaSinStock.Rows.Count)
	        {
                case 0:
                    break;
                case 1:
                    grillaSinStock.Rows[0].Selected = true;
                    grillaSinStock.FirstDisplayedScrollingRowIndex = 0;
                    break;
		        default:
                    selectRow = e.RowIndex == grillaSinStock.Rows.Count ? e.RowIndex - 1 : e.RowIndex;
                    grillaSinStock.Rows[selectRow].Selected = true;
                    grillaSinStock.FirstDisplayedScrollingRowIndex = selectRow == 0 ? 0 : (selectRow-1); 
                    break;
	        }
        }

        private void cargarCorteSinStock(string codigo)
        {
            txtCodigo.Text = codigo;
            txtCantKgs.Text = "-0.0051";//se pone este resultado por se el menor para q se pueda ver en reporte            
            agregarCorte();
            comprobarStock();
            txtCodigo.Text = "";
            huboModificaciones = true;
        }

        private void btnCerrarPanel_Click(object sender, EventArgs e)
        {
            panelGrillaFaltantes.Visible = false;
            btnVerNoCargados.Visible = !panelGrillaFaltantes.Visible;
        }

        private void btnVerAcum_Click(object sender, EventArgs e)
        {
            ///Se crea nueva lista porque usando la lista que carga la grilla acumula los kgs
            ///de los cortes que se repiten.
            List<CortesPorCompra> listEnGrilla = new List<CortesPorCompra>();
            foreach (Entidades.CortePorCompra corte in listaCortePorCompra)
            {
                cortesPorCompra = new CortesPorCompra();

                cortesPorCompra.Index = corte.IdCortePorCompra;
                cortesPorCompra.codigo = corte.corte.codigo;
                cortesPorCompra.corte = corte.corte.corte;
                cortesPorCompra.cantKgs = corte.cantKgs;
                cortesPorCompra.precioKg = corte.precioKg;
                cortesPorCompra.totalS = corte.precioKg * corte.cantKgs;
                cortesPorCompra.sucursal = corte.sucursal.SucursalNombre;
                cortesPorCompra.Creado = corte.Creado;

                listEnGrilla.Add(cortesPorCompra);
            }

            Movimientos.formVerAcumulados formVerAcum = new Presentacion.Movimientos.formVerAcumulados();
            formVerAcum.verAcumulados(null, listEnGrilla, Presentacion.Movimientos.formVerAcumulados.tipoAcum.stock);// (listaCortesPorMovimiento);
            formVerAcum.ShowDialog();
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            switch (keyData)
            {
                case Keys.Home:
                    txtCodigo.Focus();
                    break;
                case Keys.PageUp:
                    txtCodigo.Focus();
                    break;
                case Keys.F2:
                    foreach (Form frm in Application.OpenForms)
                    {
                        if (frm.GetType() == typeof(FormPrincipal))
                        {
                            frm.BringToFront();
                            break;
                        }
                    }
                    break;
                case Keys.F10:
                    buscarCorte();
                    break;
                case Keys.F11:
                    txtObservaciones.Focus();
                    break;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void btnVerPorcentaje_Click(object sender, EventArgs e)
        {
            if (oCompraE.CantMedias == null || oCompraE.KgsMedias == null)
            {
                MessageBox.Show("El pesaje no tiene registrado KgsMedias y CantMedias.\n\nIngrese KgsMedias y CantMedias presione Guardar y vuelva a intentarlo.",
                    "Datos faltantes", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            Stock.FormVerPorcCortes frmVerPorcCorte = new Presentacion.Stock.FormVerPorcCortes();
            frmVerPorcCorte.idPesaje = idCompra;
            frmVerPorcCorte.frmPesaje = this;
            frmVerPorcCorte.Show();
        }

        private void btnBuscarProv_Click(object sender, EventArgs e)
        {
            buscarPersona();
        }

        private void buscarPersona()
        {
            Personas.formBuscarPersona frmBuscarPersona = new Personas.formBuscarPersona();
            frmBuscarPersona.ShowDialog(this);
        }

        //comunicación con interface
        public void EnviarPersona(Entidades.Persona proveedor)
        {
            oProvNuevaCompra = proveedor;
            this.txtProveedor.Text = oProvNuevaCompra.razonSocial;
        }
    }
}
