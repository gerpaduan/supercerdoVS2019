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
    public partial class formAddOrEditStock : Form, InterfaceCorte, InterfaceUsuario     
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
       bool fijarPeso = Convert.ToBoolean(ConfigurationManager.AppSettings["fijarPeso"].ToString());

        public formAddOrEditStock()
        {
            InitializeComponent();
        }

        private void formNuevaCompra_Load(object sender, EventArgs e)
        {
            this.Text += Utilidades.Conexion.getSucursalConexion();        
            cargarComboSucursal();
            dtCorte = oCorteN.obtenerCortes();
            checkLeerPeso.Visible = FormPrincipal.logueado || Convert.ToBoolean(ConfigurationManager.AppSettings["leerPeso"].ToString());

            if (dtCorte.Rows.Count == 0)
            {
                MessageBox.Show("No se pudieron cargar los cortes.");
            }
            if (accion.Equals(Entidades.Compra.accion.Agregar))
            {
                logueoUsuario();
                if (oUsuario == null)
                {
                    this.Close();
                    return;
                }

                oProvNuevaCompra = new Entidades.Persona();
                oProvNuevaCompra.idPersona = Convert.ToInt32(tipoCompraEnum);
                oSucursalE.idSucursal = (int)comboSucursal.SelectedValue;
            }
            if (accion.Equals(Entidades.Compra.accion.Modificar))
            {
                oCompraE = oCompraN.findById_convertToCompra(idCompra);
                listaCortePorCompra = oCompraN.convertCortesPorCompraToList(idCompra);

                tipoCompraEnum = Entidades.Compra.tipoCompraToEnum(oCompraE.TipoCompra);
                oProvNuevaCompra = oCompraE.Proveedor;

                oSucursalE = oCompraE.Sucursal;
                comboSucursal.SelectedValue = oSucursalE.idSucursal;
                txtFechaCompra.Value = oCompraE.FechaCompra;
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
                btnAceptar.Text = "Modificar";
                txtFechaCompra.Enabled = false;
                comboSucursal.Enabled = false;
                groupBox1.Enabled = false;
                txtObservaciones.ReadOnly = true;
            }
            tipoCompra = Entidades.Compra.tipoCompraToString(tipoCompraEnum);
            txtUsuario.Text = oUsuario != null ? oUsuario.Nombre : "-";
            txtTipoAccion.Text = tipoCompra;
            this.Text = accion.ToString()+" "+tipoCompra;
            huboModificaciones = false;
            idCompraLabel.Text = idCompra.ToString();
        }

        private void logueoUsuario()
        {
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
        }

        private void btnBuscaCorte_Click(object sender, EventArgs e)
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
        }

        private void btnCancelar_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnAceptar_Click(object sender, EventArgs e)
        {
            if (btnAceptar.Text.Equals("Modificar"))
            {
                logueoUsuario();
                if (oUsuario == null)
                {
                    return;
                }
                if (Util_Form.validarFechaConAdmin(Presentacion.FormPrincipal.logueado, txtFechaCompra.Value, "Fecha") &&
                Util_Form.validarSucursal(Presentacion.FormPrincipal.logueado, Convert.ToInt32(comboSucursal.SelectedValue.ToString())))
                {
                    txtUsuario.Text = oUsuario.Nombre;
                    btnAceptar.Text = "Guardar";
                    txtFechaCompra.Enabled = true;
                    comboSucursal.Enabled = true;
                    groupBox1.Enabled = true;
                    txtObservaciones.ReadOnly = false;                    
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
                    if (Util_Form.validarFechaConAdmin(Presentacion.FormPrincipal.logueado, txtFechaCompra.Value, "Fecha") && 
                        Util_Form.validarSucursal(Presentacion.FormPrincipal.logueado, Convert.ToInt32(comboSucursal.SelectedValue.ToString()))
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
            txtObservaciones.Text = "";            
            listaCortesEnGrilla = new List<CortesPorCompra>();//Lista que se carga en la grilla
            listaCortePorCompra = new List<Entidades.CortePorCompra>();
            grillaCortePorCompra.DataSource = null;                        
        }

        private void cargarCompra()
        {
            oCompraE.NroRemito = "";
            oCompraE.Proveedor = oProvNuevaCompra;
            oCompraE.FechaCompra = txtFechaCompra.Value;
            oCompraE.Estado = "";
            oCompraE.Observaciones = txtObservaciones.Text;
            oCompraE.TipoCompra = tipoCompra;
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
            quitarCorte();
            cargarGrilla();
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
            }
        }

        private void agregarCorte()
        {
            cargarCortesPorCompra();
            cargarGrilla();            
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
                oCortePorCompra.cantKgs = Util_Form.convertFloat(txtCantKgs.Text); //float.Parse(txtCantKgs.Text.Trim(), System.Globalization.NumberStyles.Float, new System.Globalization.CultureInfo("en-US"));
                oCortePorCompra.precioKg = float.Parse("0.00");

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
                    
                    int nroFila = validarCorteEnGrilla();
                    if (nroFila == -1)
                    {
                        listaCortePorCompra.Add(oCortePorCompra);

                        //creo CortesPorCompra y cargo la lista de la grilla
                        cargarCorteEnGrilla(oCortePorCompra);
                    }

                    if (nroFila == -2)
                    {
                        oCortePorCompra = null;
                        cortesPorCompra = null;
                    }
                    if (nroFila > -1)
                    {
                        listaCortePorCompra[nroFila].cantKgs = listaCortePorCompra[nroFila].cantKgs + oCortePorCompra.cantKgs;

                        listaCortesEnGrilla[nroFila].cantKgs = listaCortePorCompra[nroFila].cantKgs;
                        listaCortesEnGrilla[nroFila].totalS = listaCortesEnGrilla[nroFila].totalS + (oCortePorCompra.cantKgs * oCortePorCompra.precioKg);

                        oCortePorCompra = null;
                        cortesPorCompra = null;
                    }
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

            cortesPorCompra.codigo = oCortePorCompra.corte.codigo;
            cortesPorCompra.corte = oCortePorCompra.corte.corte;
            cortesPorCompra.cantKgs = oCortePorCompra.cantKgs;
            cortesPorCompra.precioKg = oCortePorCompra.precioKg;
            cortesPorCompra.totalS = oCortePorCompra.precioKg * cortesPorCompra.cantKgs;
            cortesPorCompra.sucursal = oCortePorCompra.sucursal.SucursalNombre;

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
                    float cantKgs = Utilidades.Util_Form.convertFloat(txtCantKgs.Text);
                    if (cantKgs <= 0)
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
            DialogResult respuesta;
            respuesta = MessageBox.Show("¿Guardar la modificación en el stock de los cortes ingresados?.", "Verificar datos ingresados", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);

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

            comboSucursal.SelectedIndex = Convert.ToInt32(ConfigurationManager.AppSettings["idSucursal"].ToString()) - 1;
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
                if (checkLeerPeso.Checked)
                {
                    if (fijarPeso)
                    {
                        txtCantKgs.Text = "1.500";
                    }
                    else
                    {
                        Leer_Peso = Utilidades.SingletonLeerPeso.CrearLeerPeso();
                        txtCantKgs.Text = Leer_Peso.ObtenerPeso();
                    }
                }
            }
            catch (Exception ex)
            {                
                timer1.Enabled = false;
                if (Utilidades.Util_Form.errorBalanza(ex.Message) == DialogResult.Yes)
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
                }
                else
                {
                    txtCantKgs.Text = "";
                    txtCantKgs.ReadOnly = false;
                    txtCantKgs.TabStop = true;
                    txtCantKgs.Select();
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
            if (oCorteNuevaCompra != null && oCorteNuevaCompra.idCorte > 0 && oCorteNuevaCompra.tipo.Equals("Unidad") && checkLeerPeso.Checked)
            {
                checkLeerPeso.Checked = false;
                txtCantKgs.Focus();
            }
            else
            {
                if (oCorteNuevaCompra != null && oCorteNuevaCompra.idCorte > 0 && !oCorteNuevaCompra.tipo.Equals("Unidad") && !checkLeerPeso.Checked)
                {
                    checkLeerPeso.Checked = true;
                    btnAgregar.Focus();
                }
            }
        }
    }
}
