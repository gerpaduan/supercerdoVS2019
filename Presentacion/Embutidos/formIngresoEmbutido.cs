using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Presentacion.Embutidos;
using Presentacion.Cortes;
using System.Configuration;
using Utilidades;


namespace Presentacion
{
    public partial class formIngresoEmbutido : formBaseColor, InterfaceCorte, InterfaceEmbutido, InterfaceUsuario
    {
        Utilidades.SingletonLeerPeso Leer_Peso;

        public formEmbutidos frmEmbutidos = new formEmbutidos();
        DataTable dtSucursales;
        Negocio.Sucursal oSucursalN;
        Negocio.Corte oCorteN=new Negocio.Corte();

        Entidades.Corte oCorteEmbutidoE;
        Entidades.Corte oCorteE;
        Entidades.CortePorEmbutido oCortePorEmbutidoE;
        Entidades.Embutido oEmbutidoE = new Entidades.Embutido();
        public Entidades.Usuario oUsuario;

        Entidades.Usuario oUsuarioNuevoEmbutido;

        CortePorEmbutido cortePorEmbutido;
        List<CortePorEmbutido> listaCortesEnGrilla = new List<CortePorEmbutido>();

        List<Entidades.CortePorEmbutido> listaCortePorEmbutido = new List<Entidades.CortePorEmbutido>();

        bool esDuplicado = false;
        bool saveChanges = false;
        bool dejarDeLeerPeso = false;
        bool fijarPeso = Convert.ToBoolean(ConfigurationManager.AppSettings["fijarPeso"].ToString());

        Color enableColor = ColorTranslator.FromHtml(ConfigurationManager.AppSettings["enableColor"].ToString()); //SystemColors.Window;
        Color readOnlyColor = ColorTranslator.FromHtml(ConfigurationManager.AppSettings["readOnlyColor"].ToString());//SystemColors.ScrollBar;
        Color focusColor = ColorTranslator.FromHtml(ConfigurationManager.AppSettings["focusColor"].ToString());//Color.Orange;//Color.NavajoWhite;//Color.MediumAquamarine;
        Color ultimoColor = Color.Green;

        float totalKg = 0;//totalPesoEmbutidos

        public formIngresoEmbutido()
        {
            InitializeComponent();
        }

        #region Métodos

        private void cargarGrilla()
        {
            grillaCortesPorEmbutido.AutoGenerateColumns = false;
            grillaCortesPorEmbutido.DataSource = null;
            grillaCortesPorEmbutido.DataSource = listaCortesEnGrilla;

            if (listaCortesEnGrilla.Count>0)
            {
                grillaCortesPorEmbutido.Rows[listaCortesEnGrilla.Count - 1].Selected = true;
                grillaCortesPorEmbutido.FirstDisplayedScrollingRowIndex = listaCortesEnGrilla.Count - 1;
            }           
            cargarTotalKg();            
        }

        private void agregarEmbutido()
        {
            if ( Utilidades.Util_Form.validarFechaConAdmin(Presentacion.FormPrincipal.logueado, txtFechaEmbutido.Value, "Fecha") &&
                Utilidades.Util_Form.validarSucursal(Presentacion.FormPrincipal.logueado, Convert.ToInt32(comboSucursal.SelectedValue.ToString()))
                  && validacionFinal())
            {
                cargarEmbutido();
                oEmbutidoE.idEmbutido = oCorteN.agregarEmbutido(oEmbutidoE);

                //Se carga el rebozado - 17 es el equivalente al codigo de la milanesa
                if (oEmbutidoE.corte.codigo.Equals(17))
                {
                    cargarRebozado();
                }

                foreach (Entidades.CortePorEmbutido cortePorEmbutido in listaCortePorEmbutido)
                {
                    oCorteN.agregarCortePorEmbutido(cortePorEmbutido);
                }
                saveChanges = true;
                frmEmbutidos.cargarGrilla();
                this.Close();
            }
        }

        private bool validacionFinal()
        {
            if (oCorteEmbutidoE == null || oCorteEmbutidoE.corte == null || oCorteEmbutidoE.idCorte == 0)
            {
                MessageBox.Show("Seleccione el embutido.", "Ingresar embutido", MessageBoxButtons.OK,MessageBoxIcon.Information);
                btnBuscarEmbutido.Select();
                return false;
            }
            if (grillaCortesPorEmbutido.SelectedRows.Count > 0)
            {
                if (comboSucursal.SelectedItem==null)
                {
                    MessageBox.Show("Complete el campo Sucursal.", "Completar la Sucursal", MessageBoxButtons.OK,MessageBoxIcon.Information);
                    return false;
                }
                else
                {
                    DialogResult respuesta = MessageBox.Show("Verifique si la Fecha, Sucursal y los demás los datos ingresados están correctos.\n ¿Están correctos?. ", "Verificar Datos", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);

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
                MessageBox.Show("No ingresó ningún corte correspondiente al embutido.", "No existe cortes en la grilla", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return false;
            }
        }

        private void cargarEmbutido()
        {
            oEmbutidoE.fechaEmbutido = txtFechaEmbutido.Value;
            oEmbutidoE.corte = oCorteEmbutidoE;

            //creo y asigno la sucursal seleccionada
            Entidades.Sucursal oSucursalE=new Entidades.Sucursal();
            oSucursalE.IdSucursal = Convert.ToInt32(comboSucursal.SelectedValue.ToString());
            oEmbutidoE.sucursal = oSucursalE;
            oEmbutidoE.observaciones = txtObservaciones.Text.Trim();

            Entidades.Usuario oUser = new Entidades.Usuario();
            if (oEmbutidoE.idEmbutido.Equals(0))
            {
                oEmbutidoE.CreadoPor = oUsuario;
            }
            else
            {
                oEmbutidoE.ActualizadoPor = oUsuario;
            }
        }

        private bool validarCantKgs()
        { 
            bool resp=true;

            try
            {
                //Validación para cuando es Merma
                if (oCorteE.codigo == 10000 && Utilidades.Util_Form.validarCampoNumerico(txtCantKgs.Text, "Cant. Kgs"))
                    return true;      

                if (!Utilidades.Util_Form.validarNumeroMayorACero(txtCantKgs.Text, "Cant. Kgs"))
                {
                    resp = false;
                    txtCantKgs.Focus();
                }
            }
            catch (Exception ex)
            {
                resp = false;
                MessageBox.Show("Error en método validarCantKgs()\n\n"+ex.Message);
            }
            return resp;
        }

        private void agregarCorteEnEmbutido()
        {
            if (validar() && validarCantKgs())
            {
                cargarCortePorEmbutido();
                //cargarCorteEnLista();
                listaCortesEnGrilla.Add(cortePorEmbutido);
                listaCortePorEmbutido.Add(oCortePorEmbutidoE);

                cargarGrilla();

                oCorteE = null;//libero el objeto
                oCortePorEmbutidoE = null;//libero el objeto
                limpiarCampos();

                txtCodCorteEnEmbutido.Focus();                
            }
        }

        private void quitarCortePorEmbutido()
        {
            if (grillaCortesPorEmbutido.SelectedRows.Count > 0)
            {
                int nroFila = grillaCortesPorEmbutido.Rows.GetFirstRow(DataGridViewElementStates.Selected);//obtiene nro de fila de la grilla
                listaCortePorEmbutido.RemoveAt(nroFila);//elimina objetos de las listas
                listaCortesEnGrilla.RemoveAt(nroFila);
            }
            else
            {
                MessageBox.Show("No hay ninguna fila seleccionada.", "Seleccione un fila", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            cargarGrilla();
        }

        private void cargarCortePorEmbutido()
        {
            oCortePorEmbutidoE = new Entidades.CortePorEmbutido();
            oCortePorEmbutidoE.embutido = oEmbutidoE;
            oCortePorEmbutidoE.corte = oCorteE;
            try 
	        {
                oCortePorEmbutidoE.kgUtilizado = float.Parse(txtCantKgs.Text.Trim(), System.Globalization.NumberStyles.Float, new System.Globalization.CultureInfo("en-US"));
            }
	        catch (Exception)
	        {
                oCortePorEmbutidoE.kgUtilizado = float.Parse(txtCantKgs.Text.Trim());
            }

            oCortePorEmbutidoE.PesoBalanza = checkLeerPeso.Checked;
           
            //Cargar CortePorEmbutido para grilla
            cortePorEmbutido = new CortePorEmbutido();
            cortePorEmbutido.idCorte = oCortePorEmbutidoE.corte.idCorte;
            cortePorEmbutido.codigo = oCortePorEmbutidoE.corte.codigo;
            cortePorEmbutido.corte = oCortePorEmbutidoE.corte.corte;
            cortePorEmbutido.kgUtilizado = oCortePorEmbutidoE.kgUtilizado;
            cortePorEmbutido.PesoBalanza = oCortePorEmbutidoE.PesoBalanza;
        }

        private void limpiarCampos()
        {
            txtCodCorteEnEmbutido.Text = "";
            txtCorteEnEmbutido.Text = "";
            txtCantKgs.Text = "";
        }

        private void cargarTotalKg()
        {
            totalKg = 0;

            foreach (Entidades.CortePorEmbutido corte in listaCortePorEmbutido)
            {
                totalKg = totalKg + corte.kgUtilizado;                
            }
            txtTotalKg.Text = Convert.ToString(totalKg);
        }

        private bool validar()
        {
            string mensaje="Complete los siguientes campos:";
            if (oCorteEmbutidoE == null || oCorteE == null || txtCantKgs.Text.Trim()=="")
            {                
                if (oCorteEmbutidoE == null)
                {
                    mensaje += "\n" + "- Embutido";
                    btnAgregar.Focus();
                }
                if (oCorteE==null)
                {
                    mensaje += "\n" + "- Corte en Embutido";
                    txtCodCorteEnEmbutido.Focus();
                }
                if (txtCantKgs.Text.Trim()=="")
                {
                    mensaje += "\n" + "- Cant. Kgs";
                    txtCantKgs.Focus();
                }
                MessageBox.Show(mensaje, "Complete todos los campos", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return false;                
            }
            else
            {
                return true;
            }
        }

        private void cargarComboSucursal()
        {
            dtSucursales = new DataTable();
            oSucursalN = new Negocio.Sucursal();
            dtSucursales = oSucursalN.obtenerSucursalSanMartin();
            dtSucursales = oSucursalN.obtenerSucursales();
            comboSucursal.DataSource = dtSucursales;
            comboSucursal.DisplayMember = "sucursal";
            comboSucursal.ValueMember = "idSucursal";
            comboSucursal.SelectedValue = Convert.ToInt32(Utilidades.Conexion.getIdSucursalConexion());//-1;//No muestra ninguna sucursal
        }

        #endregion

        private void btnBuscarEmbutido_Click(object sender, EventArgs e)
        {
            buscarEmbutido();
        }

        private void buscarEmbutido()
        {
            formBuscarEmbutido frmBuscarEmbutido = new formBuscarEmbutido();
            if (frmEmbutidos.EsVentaClientes)
            {
                frmBuscarEmbutido.Text = "Buscar Cliente";
            }
            frmBuscarEmbutido.Show(this);
        }

        public void EnviarEmbutido(Entidades.Corte corte)
        {
            oCorteEmbutidoE = corte;
            txtCodigoEmbutido.Text = Convert.ToString(oCorteEmbutidoE.codigo);
            txtEmbutido.Text = oCorteEmbutidoE.corte;
            //calcularFormula();
            txtCodCorteEnEmbutido.Focus();
            calcularFormula();
        }

        private void calcularFormula()
        {
            if (!oCorteEmbutidoE.tipo.Equals(Entidades.Corte.tipoCorte.Embutido.ToString()))
            {
                panelFormula.Visible = false;
                //btnCalcularForm.Visible = false;
                return;
            }

            panelFormula.Visible = true;
            //btnCalcularForm.Visible = true;
            
            //calcula total sin condimentos
            float totalKgSinCond = 0;
            foreach (Entidades.CortePorEmbutido oCortePorEmb in listaCortePorEmbutido)
            {
                totalKgSinCond += !(oCortePorEmb.corte.codigo >= 2000 && oCortePorEmb.corte.codigo < 3000) ?
                    oCortePorEmb.kgUtilizado : 0;
            }

            string sal, pimienta, nuez, bracolor, pimenton, producto;
            sal = pimienta = nuez = bracolor = pimenton = producto = "-";
            int cantDecimales = 3;

            switch (oCorteEmbutidoE.codigo)
            {
                    //chorizo
                case 4:
                    sal = Convert.ToString((Math.Round((totalKgSinCond * 
                        (Util_Form.convertFloat(ConfigurationManager.AppSettings["salChorizo"].ToString(), false))), cantDecimales)).ToString("F3"));
                    pimienta = Convert.ToString((Math.Round((totalKgSinCond *
                        (Util_Form.convertFloat(ConfigurationManager.AppSettings["pimientaChorizo"].ToString(), false))), cantDecimales)).ToString("F3"));
                    nuez = Convert.ToString((Math.Round((totalKgSinCond *
                        (Util_Form.convertFloat(ConfigurationManager.AppSettings["nuezChorizo"].ToString(), false))), cantDecimales)).ToString("F3"));
                    bracolor = Convert.ToString((Math.Round((totalKgSinCond *
                        (Util_Form.convertFloat(ConfigurationManager.AppSettings["bracolorChorizo"].ToString(), false))), cantDecimales)).ToString("F3"));
                    break;
                    //salame
                case 11:
                    sal = Convert.ToString((Math.Round((totalKgSinCond *
                        (Util_Form.convertFloat(ConfigurationManager.AppSettings["salSalame"].ToString(), false))), cantDecimales)).ToString("F3"));
                    pimienta = Convert.ToString((Math.Round((totalKgSinCond *
                        (Util_Form.convertFloat(ConfigurationManager.AppSettings["pimientaSalame"].ToString(), false))), cantDecimales)).ToString("F3"));
                    nuez = Convert.ToString((Math.Round((totalKgSinCond *
                        (Util_Form.convertFloat(ConfigurationManager.AppSettings["nuezSalame"].ToString(), false))), cantDecimales)).ToString("F3"));
                    producto = Convert.ToString((Math.Round((totalKgSinCond *
                        (Util_Form.convertFloat(ConfigurationManager.AppSettings["productoSalame"].ToString(), false))), cantDecimales)).ToString("F3"));
                    break;
                    //salchicha
                case 33:
                    sal = Convert.ToString((Math.Round((totalKgSinCond *
                        (Util_Form.convertFloat(ConfigurationManager.AppSettings["salSalchicha"].ToString(), false))), cantDecimales)).ToString("F3"));
                    pimienta = Convert.ToString((Math.Round((totalKgSinCond *
                        (Util_Form.convertFloat(ConfigurationManager.AppSettings["pimientaSalchicha"].ToString(), false))), cantDecimales)).ToString("F3"));
                    pimenton = Convert.ToString((Math.Round((totalKgSinCond *
                        (Util_Form.convertFloat(ConfigurationManager.AppSettings["pimentonSalchicha"].ToString(), false))), cantDecimales)).ToString("F3"));
                    bracolor = Convert.ToString((Math.Round((totalKgSinCond *
                        (Util_Form.convertFloat(ConfigurationManager.AppSettings["bracolorSalchicha"].ToString(), false))), cantDecimales)).ToString("F3"));
                    break;
                default:
                    break;
            }

            txtSal.Text = sal;
            txtPimienta.Text = pimienta;
            txtNuez.Text = nuez;
            txtBracolor.Text = bracolor;
            txtPimenton.Text = pimenton;
            txtProducto.Text = producto;
        }

        private void btnBuscarCorte_Click(object sender, EventArgs e)
        {
            buscarCorte();
        }

        private void buscarCorte()
        {
            formBuscarCorte frmBuscarCorte = new formBuscarCorte();
            frmBuscarCorte.Show(this);
        }

        public void EnviarCorte(Entidades.Corte corte)
        {
            oCorteE = corte;
            txtCodCorteEnEmbutido.Text = Convert.ToString(oCorteE.codigo);
            txtCorteEnEmbutido.Text = oCorteE.corte;

            txtCantKgs.Focus();
        }

        private void btnAgregar_Click(object sender, EventArgs e)
        {
            agregarCorteEnEmbutido();
            calcularFormula();
            capturarPantalla();
        }        

        private void btnGuardar_Click(object sender, EventArgs e)
        {
            agregarEmbutido();
        }

        private void btnCancelar_Click(object sender, EventArgs e)
        {
            this.Close();           
        }

        private void btnQuitar_Click(object sender, EventArgs e)
        {
            quitarCortePorEmbutido();
            calcularFormula();
            capturarPantalla();
        }

        private void capturarPantalla()
        {
            //se refresca para que se muestren los datos
            this.Refresh();
            Util_Form.capturarPantalla("Embutido", txtFechaEmbutido.Value);
        }

        private void TxtPruebaENTER_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)(Keys.Enter))
            {
                e.Handled = true;
                SendKeys.Send("{TAB}");
            }
        }
        
        //Agregando rebozado si es Milanesa
        private void cargarRebozado()
        {
            txtCodCorteEnEmbutido.Text = "301";
            float rebozado = totalKg * float.Parse("0,20");
            txtCantKgs.Text = rebozado.ToString();
            cargarCortePorEmbutido();
            agregarCorteEnEmbutido();
        }
        //Fin rebozado

        private void cargarCorteEnEmbutido()
        {
            try
            {
                if (txtCodCorteEnEmbutido.Text.Trim() != "")
                {
                    oCorteE = new Entidades.Corte();
                    DataTable dtCorte = new DataTable();
                    dtCorte = oCorteN.buscarCodigoCorte(Convert.ToInt32(txtCodCorteEnEmbutido.Text.Trim()));

                    if (dtCorte.Rows.Count > 0)
                    {
                        foreach (DataRow fila in dtCorte.Rows)
                        {
                            oCorteE.idCorte = Convert.ToInt32(fila["idCorte"].ToString());
                            oCorteE.codigo = Convert.ToInt32(fila["codigo"].ToString());
                            oCorteE.corte = fila["corte"].ToString();
                            oCorteE.tipo = fila["tipo"].ToString();
                        }
                        //se cargan los datos del corte
                        txtCorteEnEmbutido.Text = oCorteE.corte;
                    }
                    else
                    {
                        txtCorteEnEmbutido.Text = "";
                        oCorteE = null;
                    }
                }
                else
                {
                    txtCorteEnEmbutido.Text = "";
                    oCorteE = null;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error en método cargarCorteEnEmbutido().\n\n"+ex.Message);
            }
        }

        private void txtCodCorteEnEmbutido_TextChanged(object sender, EventArgs e)
        {
            cargarCorteEnEmbutido();
        }

        private void formIngresoEmbutido_Load(object sender, EventArgs e)
        {
            if (esDuplicado)
                this.Left += 50;
            
            this.Text += Utilidades.Conexion.getSucursalConexion();
            if (oUsuario == null)
            {
                this.Close();
            }
            else
            {
                if (frmEmbutidos.EsVentaClientes)
                {
                    this.Text = "Nueva Venta Cliente";
                    groupBox1.Text = "Cliente ";
                    groupBox2.Text = "Cortes ";
                }
                timer1.Interval = Convert.ToInt32(ConfigurationManager.AppSettings["timerForm"].ToString());
                checkLeerPeso.Visible = FormPrincipal.logueado || Convert.ToBoolean(ConfigurationManager.AppSettings["leerPeso"].ToString());
                cargarComboSucursal();
                txtSucursal.Text = comboSucursal.Text;
                txtUsuario.Text = oUsuario.Nombre;
                comboSucursal.Visible = FormPrincipal.logueado;
                txtSucursal.Visible = !FormPrincipal.logueado;
                btnBuscarEmbutido.Select();
            }
        }

        private void checkLeerPeso_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                checkLeerPeso.BackColor = Utilidades.Util_Form.getBackColorCheckBox(checkLeerPeso.Checked);

                if (checkLeerPeso.Checked)
                {
                    dejarDeLeerPeso = false;
                    txtCodCorteEnEmbutido.Focus();
                    txtCantKgs.ReadOnly = true;
                    txtCantKgs.TabStop = false;
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
                //timer1.Enabled = true;
                //}
            }
        }

        private void txtCodCorteEnEmbutido_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyValue.Equals(13))
            {
                if (oCorteE == null || oCorteE.idCorte.Equals(0))
                {
                    MessageBox.Show("El código no existe");
                    txtCodCorteEnEmbutido.Focus();
                }
            }
        }

        private void formIngresoEmbutido_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = salir();
        }

        private bool salir()
        {
            bool ret = false;
            if (!saveChanges && (grillaCortesPorEmbutido.SelectedRows.Count > 0 || (oEmbutidoE != null && oEmbutidoE.idEmbutido > 0)))
            {
                DialogResult respuesta = MessageBox.Show("Si cierra el formulario se perderan los datos ingresados.\n¿Está seguro que desea salir?. ", "Cerrar Formulario", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);

                if ((respuesta == DialogResult.No))
                {
                    ret = true;
                } 
            }
            saveChanges = false;//setea en false(si esta TRUE porque se presionó btnGuardar)
            return ret;
        }

        private void control_Enter(object sender, EventArgs e)
        {
            if (sender is TextBox)
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
                objectToChangeColor.UseVisualStyleBackColor = false;
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
                if (objectToChangeColor.Name.Equals("txtCodCorteEnEmbutido")) tipoDeCorte();
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

        private void tipoDeCorte()
        {
            try
            {
                if (oCorteE != null && oCorteE.idCorte > 0 && oCorteE.tipo.Equals("Unidad") && checkLeerPeso.Checked)
                {
                    checkLeerPeso.Checked = false;
                    txtCantKgs.Focus();
                }
                else
                {
                    if (!dejarDeLeerPeso && oCorteE != null && oCorteE.idCorte > 0 && !oCorteE.tipo.Equals("Unidad") && !checkLeerPeso.Checked)
                    {
                        checkLeerPeso.Checked = true;
                        txtCantKgs.BackColor = readOnlyColor;
                        btnAgregar.Focus();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Hubo un error al verificar el tipo del corte.\n\n"+ ex.Message + "\n" + ex.StackTrace);
            }
        }

        private void btnNuevo_Click(object sender, EventArgs e)
        {
            Presentacion.Caja.FormLoginVendedor frmLogin = new Presentacion.Caja.FormLoginVendedor();
            frmLogin.ShowDialog(this);
            formIngresoEmbutido frmIngresoEmbutido = new formIngresoEmbutido();
            frmIngresoEmbutido.oUsuario = oUsuarioNuevoEmbutido;
            frmIngresoEmbutido.frmEmbutidos = frmEmbutidos;
            frmIngresoEmbutido.esDuplicado = true;
            frmIngresoEmbutido.Show();
            this.Left -= 200;
        }

        public void EnviarUsuario(Entidades.Usuario usuario)
        {
            oUsuarioNuevoEmbutido = usuario;
        }

        private void btnCalcularForm_Click(object sender, EventArgs e)
        {
            if (!panelFormula.Visible && MessageBox.Show("Si ya finalizó con la carga y desea obtener la fórmula presione 'Sí'", "Calcular fórmula",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2).Equals(DialogResult.No))
                return;
            
            if (panelFormula.Visible && MessageBox.Show("¿Está seguro que desea actualizar la fórmula de los condimientos?", "Actualizar fórmula",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2).Equals(DialogResult.No))
                return;

            panelFormula.Visible = true;
            calcularFormula();
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            switch (keyData)
            {
                case Keys.Home:
                    txtCodCorteEnEmbutido.Focus();
                    break;
                case Keys.PageUp:
                    txtCodCorteEnEmbutido.Focus();
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
                case Keys.F9:
                    buscarEmbutido();
                    break;
                case Keys.F10:
                    buscarCorte();
                    break;
                case Keys.F11:
                    txtObservaciones.Focus();
                    break;
                case Keys.Escape:
                    this.Close();
                    break;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }
    }
}
