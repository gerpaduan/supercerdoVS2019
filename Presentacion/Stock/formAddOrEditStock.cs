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


namespace Presentacion
{
    public partial class formAddOrEditStock : Form, InterfaceCorte       
    {

        Utilidades.Leer_Peso Leer_Peso = new Utilidades.Leer_Peso();

        DataTable dtSucursales;
        DataTable dtCortes;
        Negocio.Compra oCompraN=new Negocio.Compra();
        Negocio.Sucursal oSucursalN;
        Negocio.Corte oCorteN = new Negocio.Corte();
        Entidades.Compra oCompraE = new Entidades.Compra();
        Entidades.Persona oProvNuevaCompra;
        Entidades.Corte oCorteNuevaCompra;
        Entidades.CortePorCompra oCortePorCompra;
        Entidades.Sucursal oSucursalE;
        CortesPorCompra cortesPorCompra;
        float totalKgs = 0;
        float totalPesos = 0;

        string tipoCompra = "Media Res";

        List<CortesPorCompra> listaCortesEnGrilla = new List<CortesPorCompra>();//Lista que se carga en la grilla

       List<Entidades.CortePorCompra> listaCortePorCompra = new List<Entidades.CortePorCompra>();

       formCompras oFrmCompra;

       bool ultimaValidacion = true;

        public formAddOrEditStock()
        {
            InitializeComponent();            
            cargarComboSucursal();            
        }

        #region eventos

        private void btnBuscarProv_Click(object sender, EventArgs e)
        {
            formBuscarProveedor frmBuscarProv = new formBuscarProveedor();
            frmBuscarProv.Show(this);
        }

        //comunicación con interface
        public void EnviarProveedor(Entidades.Persona proveedor)
        {
            oProvNuevaCompra = proveedor;
        }

        //comunicación con interface
        public void EnviarCorte(Entidades.Corte corte)
        {
            oCorteNuevaCompra = corte;
            this.txtCorteNuevaCompra.Text = oCorteNuevaCompra.corte;
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
            DialogResult respuesta = MessageBox.Show("Si cierra el formulario se perderan los datos ingresados.\n¿Está seguro que desea salir?. ", "Compras", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);

            if ((respuesta == System.Windows.Forms.DialogResult.Yes))
            {
                this.Close();
            }
            
        }

        private void btnAceptar_Click(object sender, EventArgs e)
        {
            agregarCompra();
        }

        #endregion

        #region Métodos

        //se asigna el form compra para recargar la grilla del formCompras
        public void asignarFormCompra(formCompras frmCompra)
        {
            oFrmCompra = frmCompra;
        }

        private void agregarCompra()
        {
            if (listaCortePorCompra.Count > 0)
            {
                if (validaciónFinal())
                {
                    cargarCompra();//se cargan datos de la compra
                    oCompraE.IdCompra = oCompraN.agregarCompra(oCompraE);

                    if (tipoCompra=="Cortes" || tipoCompra=="Ingreso Stock")
                    {
                        foreach (Entidades.CortePorCompra cortePorCompra in listaCortePorCompra)
                        {
                            oCompraN.agregarCortePorCompra(cortePorCompra);
                        }
                    }
                
                    oFrmCompra.cargarGrilla();
                    //this.Close();
                    limpiarListas();                    

                }

            }
            else
            {
                MessageBox.Show("No hay cargada ninguna linea de compra.", "No hay lineas cargadas", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
          

        }
        private void limpiarListas()
        {
            //limpio campos
            txtObservaciones.Text = "";            
            listaCortesEnGrilla = new List<CortesPorCompra>();//Lista que se carga en la grilla
            listaCortePorCompra = new List<Entidades.CortePorCompra>();
            grillaCortePorCompra.DataSource = null;                        
        }

        private void cargarCompra()
        {
            oCompraE.NroRemito = null;
            oCompraE.Proveedor = oProvNuevaCompra;
            oCompraE.FechaCompra = txtFechaCompra.Value;
            oCompraE.Estado = "";
            oCompraE.Observaciones = txtObservaciones.Text.Trim();
            oCompraE.TipoCompra = tipoCompra;
        }

        private void quitarLinea()
        {
            if (tipoCompra == "Cortes" || tipoCompra == "Ingreso Stock")
            {
                quitarCorte();
            }           
            cargarGrilla();
        }

        private void quitarCorte()
        {
            try
            {
                if (grillaCortePorCompra.SelectedRows.Count > 0 || grillaCortePorCompra.CurrentRow != null)
                {
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
                if (tipoCompra == "Cortes" || tipoCompra == "Ingreso Stock")
                {
                    agregarCorte();
                    txtCodigo.Focus();
                }
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
            totalPesos = 0;

            if (tipoCompra == "Cortes" || tipoCompra == "Ingreso Stock")
            {
                foreach (CortesPorCompra fila in listaCortesEnGrilla)
                {
                    //sumo totales
                    totalKgs = totalKgs + fila.cantKgs;

                    //cargo Totales
                    txtCantItems.Text = grillaCortePorCompra.Rows.Count.ToString();
                }
            }            
        }

        private int validarCorteEnGrilla()
        {
            int nroFila = -1;//si corte no está cargado
            foreach (Entidades.CortePorCompra corte in listaCortePorCompra)
            {
                if (corte.corte.idCorte == oCorteNuevaCompra.idCorte && corte.sucursal.IdSucursal == Convert.ToInt32(comboSucursal.SelectedValue.ToString()))
                {
                    DialogResult resp = MessageBox.Show("Ya se ha cargado el corte en la sucursal seleccionada.\nDesea sumarlo al Corte ya ingresado?.", "Corte ya cargado en la sucursal", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);

                    if (resp == DialogResult.Yes)
                    {
                        nroFila = listaCortePorCompra.IndexOf(corte);//se envía el index de la lista para sumar los kg al corte ya ingresado
                    }
                    else
                    {
                        nroFila = -2;//si está cargado y no se quiere volver a cargar
                    }
                }
            }
            return nroFila; ;
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
                try
                {
                    oCortePorCompra.cantKgs = float.Parse(txtCantKgs.Text.Trim(), System.Globalization.NumberStyles.Float, new System.Globalization.CultureInfo("en-US"));
                    oCortePorCompra.precioKg = float.Parse("0.00");
                }
                catch (Exception)
                {
                    try
                    {
                        oCortePorCompra.cantKgs = float.Parse(txtCantKgs.Text.Trim());
                        oCortePorCompra.precioKg = float.Parse("0,00");
                    }
                    catch (Exception)
                    {
                        ultimaValidacion = false;
                    }
                }

                if (ultimaValidacion)
                {
                    oSucursalE = new Entidades.Sucursal(); //creo objeto sucursal
                    oSucursalE.IdSucursal = Convert.ToInt32(comboSucursal.SelectedValue.ToString());

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
	        catch (Exception)
	        {        		
		        throw;
	        }
        }

        private bool validarCampos()
        {
            if (comboSucursal.SelectedIndex.Equals(-1) &&
                 (txtCorteNuevaCompra.Text.Equals("") || txtCantKgs.Text.Equals("") ))
            {
                MessageBox.Show("Debe Completar todos los campos.", "Complete los campos vacíos", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return false;
            }
            else
            {
                return true;
            }
        }

        private bool validaciónFinal()
        {
            DialogResult respuesta;
            respuesta = MessageBox.Show("¿ Guardar la modificación en el stock de los cortes?.", "Verificar datos ingresados", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);

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
                if (tipoCompra == "Cortes" || tipoCompra == "Ingreso Stock")
                {
                    grillaCortePorCompra.AutoGenerateColumns = false;
                    grillaCortePorCompra.DataSource = null;
                    grillaCortePorCompra.DataSource = listaCortesEnGrilla;

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
                if (txtCodigo.Text.Trim() != "")
                {
                    oCorteNuevaCompra = null;
                    oCorteNuevaCompra = new Entidades.Corte();

                    DataTable dtCorte = new DataTable();

                    dtCorte = oCorteN.buscarCodigoCorte(Convert.ToInt32(txtCodigo.Text.Trim()));

                    if (dtCorte.Rows.Count > 0)
                    {
                        foreach (DataRow fila in dtCorte.Rows)
                        {
                            oCorteNuevaCompra.idCorte = Convert.ToInt32(fila["idCorte"].ToString());
                            oCorteNuevaCompra.codigo = Convert.ToInt32(fila["codigo"].ToString());
                            oCorteNuevaCompra.corte = fila["corte"].ToString();
                        }

                        //se cargan los datos del corte
                        txtCorteNuevaCompra.Text = oCorteNuevaCompra.corte;
                    }
                    else
                    {
                        txtCorteNuevaCompra.Text = "";
                        MessageBox.Show("El código no existe");
                        txtCodigo.Focus();
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

            comboSucursal.SelectedIndex = -1;//San Martín
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
            dtCortes = oCorteN.buscarCorteSinMaestro(txtCorteNuevaCompra.Text.Trim());
            
            AutoCompleteStringCollection cortes = new AutoCompleteStringCollection();
            
            foreach (DataRow fila in dtCortes.Rows)
            {
                //cortes.Add(fila["codigo"].ToString());
                cortes.Add(fila["corte"].ToString());

                //crear un corte nuevo y lo envia al metodo EnviarCorte                
                if (txtCorteNuevaCompra.Text.Trim() == fila["corte"].ToString())
                {
                    Entidades.Corte oCorteNuevoE = new Entidades.Corte();
                    oCorteNuevoE.idCorte = Convert.ToInt32(fila["idCorte"].ToString());
                    oCorteNuevoE.codigo = Convert.ToInt32(fila["codigo"].ToString());
                    oCorteNuevoE.corte = fila["corte"].ToString();

                    EnviarCorte(oCorteNuevoE);
                }                
            }
            return cortes;
        }

        private void txtCorteNuevaCompra_TextChanged(object sender, EventArgs e)
        {
            txtCorteNuevaCompra.AutoCompleteCustomSource = LoadAutoComplete();
        }

        const int WM_SYSCOMMAND = 0x0112;
        const int SC_CLOSE = 0xF060;

        protected override void WndProc(ref Message m)
        {
            if ((m.Msg == WM_SYSCOMMAND) && (m.WParam == (IntPtr)SC_CLOSE))
            {
                DialogResult respuesta = MessageBox.Show("Si cierra el formulario se perderan los datos ingresados.\n¿Está seguro que desea salir?. ", "Compras", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);

                if ((respuesta == System.Windows.Forms.DialogResult.No))
                {
                    return;
                }
            }
            base.WndProc(ref m);
        }

        private void txtCodigo_TextChanged(object sender, EventArgs e)
        {
            cargarCorte();
        }

        private void formNuevaCompra_Load(object sender, EventArgs e)
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

        private void btnAceptar_Enter(object sender, EventArgs e)
        {
            btnAceptar.BackColor = System.Drawing.Color.FromName("LimeGreen");
        }

        private void btnAceptar_Leave(object sender, EventArgs e)
        {
            btnAceptar.BackColor = System.Drawing.Color.FromName("SeaGreen");
        }      
    }
}
