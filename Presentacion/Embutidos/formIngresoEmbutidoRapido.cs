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
using Entidades;


namespace Presentacion
{
    public partial class formIngresoEmbutidoRapido : formBaseColor//, InterfaceCorte, InterfaceEmbutido, InterfaceUsuario
    {
        Utilidades.SingletonLeerPeso Leer_Peso;

        public formEmbutidos frmEmbutidos = new formEmbutidos();
        DataTable dtSucursales;
        Negocio.Sucursal oSucursalN;
        Negocio.Corte oCorteN=new Negocio.Corte();
        DataTable dtFormula;

        public Entidades.Corte oCorteEmbutidoE;
        public Entidades.Corte oCorteE;
        public Entidades.Corte oCorteE2;
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
        int nroErrorBalanza = 0;

        Color enableColor = ColorTranslator.FromHtml(ConfigurationManager.AppSettings["enableColor"].ToString()); //SystemColors.Window;
        Color readOnlyColor = ColorTranslator.FromHtml(ConfigurationManager.AppSettings["readOnlyColor"].ToString());//SystemColors.ScrollBar;
        Color focusColor = ColorTranslator.FromHtml(ConfigurationManager.AppSettings["focusColor"].ToString());//Color.Orange;//Color.NavajoWhite;//Color.MediumAquamarine;
        Color ultimoColor = Color.Green;

        float totalKg = 0;//totalPesoEmbutidos
        private Formula oFormulaE;

        public formIngresoEmbutidoRapido()
        {
            InitializeComponent(); this.Icon = Properties.Resources.CarniSys_ICONO;
        }

        #region Métodos

        private bool validarCantKgs()
        { 
            bool resp=true;

            try
            {
                ////Validación para cuando es Merma
                //if (oCorteE.codigo == 10000 && Utilidades.Util_Form.validarCampoNumerico(txtCantKgs.Text, "Cant. Kgs"))
                //    return true;      

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
        
        private void cargarEmbutido()
        {
            oEmbutidoE.fechaEmbutido = txtFechaEmbutido.Value;
            oEmbutidoE.corte = oCorteEmbutidoE;

            //creo y asigno la sucursal seleccionada
            Entidades.Sucursal oSucursalE = new Entidades.Sucursal();
            oSucursalE.IdSucursal = Convert.ToInt32(comboSucursal.SelectedValue.ToString());
            oEmbutidoE.sucursal = oSucursalE;
            oEmbutidoE.observaciones = "";

            Entidades.Usuario oUser = new Entidades.Usuario();
            if (oEmbutidoE.idEmbutido.Equals(0))
            {
                oEmbutidoE.CreadoPor = oUsuario;
            }
            else
            {
                oEmbutidoE.ActualizadoPor = oUsuario;
            }

            calcularFormula();
        }

        private void calcularFormula()
        {
            try
            {
                ///se valida para que no tire carteles de error en peso balanza
                double validacionNumerica;
                if (!double.TryParse(txtCantKgs.Text, out validacionNumerica))
                    return;

                dtFormula = oCorteN.getFormulaEmbutido(oCorteEmbutidoE.idCorte);

                //calcula total sin condimentos
                float totalKgSinCond = string.IsNullOrEmpty(txtCantKgs.Text) ? 0 : Util_Form.convertFloat(txtCantKgs.Text, false);

                int cantDecimales = 3;
                float porcentaje;
                for (int i = 0; i < dtFormula.Rows.Count; i++)
                {
                    porcentaje = (Util_Form.convertFloat(dtFormula.Rows[i]["porcentaje"].ToString(), false));
                    dtFormula.Rows[i]["kgs"] = Convert.ToString((Math.Round((0.01 * totalKgSinCond *
                            (porcentaje)), cantDecimales)).ToString("F3"));
                }

                grillaFormula.DataSource = dtFormula;
                // Deshabilitar la ordenación en todas las columnas
                foreach (DataGridViewColumn column in grillaFormula.Columns)
                {
                    column.SortMode = DataGridViewColumnSortMode.NotSortable;
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        private void agregarEmbutido()
        {
            try
            {
                if (dtFormula.Rows.Count == 0)
                {
                    MessageBox.Show("El Elaborado seleccionado no tiene ingresada una fórmula.","",MessageBoxButtons.OK,MessageBoxIcon.Error);
                    return;
                }

                if (Utilidades.Util_Form.validarFechaConAdmin(Presentacion.FormPrincipal.logueado, txtFechaEmbutido.Value, "Fecha") &&
                    Utilidades.Util_Form.validarSucursal(Presentacion.FormPrincipal.logueado, Convert.ToInt32(comboSucursal.SelectedValue.ToString()))
                    && validarCantKgs())
                {
                    cargarEmbutido();
                    oEmbutidoE.idEmbutido = oCorteN.agregarEmbutido(oEmbutidoE);

                    //se cargan los ingredientes seatado como Agregar Automaticamente
                    cargarIngredientesFormula();

                    foreach (Entidades.CortePorEmbutido cortePorEmbutido in listaCortePorEmbutido)
                    {
                        oCorteN.agregarCortePorEmbutido(cortePorEmbutido);
                    }

                    saveChanges = true;
                    frmEmbutidos.cargarGrilla();
                    MessageBox.Show("Los datos se guardaron correctamente!", "Mensaje");
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void cargarIngredientesFormula()
        {
            ///Se cargan todos los ingredientes de la Formula
            ///
            for (int i = 0; i < dtFormula.Rows.Count; i++)
            {
                    oCortePorEmbutidoE = new Entidades.CortePorEmbutido();
                    oCortePorEmbutidoE.embutido = oEmbutidoE;
                    oCortePorEmbutidoE.corte = oCorteN.getCorteById(Convert.ToInt32(dtFormula.Rows[i]["idCorte"]), false);
                    oCortePorEmbutidoE.kgUtilizado = Util_Form.convertFloat(dtFormula.Rows[i]["kgs"].ToString(), false);
                    oCortePorEmbutidoE.PesoBalanza = false;

                    listaCortePorEmbutido.Add(oCortePorEmbutidoE);
            }
        }


        private void cargarCortePorEmbutido(Entidades.Corte oCorte, string cantKgs)
        {
            oCortePorEmbutidoE = new Entidades.CortePorEmbutido();
            oCortePorEmbutidoE.embutido = oEmbutidoE;
            oCortePorEmbutidoE.corte = oCorte;
            try 
	        {
                oCortePorEmbutidoE.kgUtilizado = float.Parse(cantKgs.Trim(), System.Globalization.NumberStyles.Float, new System.Globalization.CultureInfo("en-US"));
            }
	        catch (Exception)
	        {
                oCortePorEmbutidoE.kgUtilizado = float.Parse(cantKgs.Trim());
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


        private void btnGuardar_Click(object sender, EventArgs e)
        {
            agregarEmbutido();
        }

        private void btnCancelar_Click(object sender, EventArgs e)
        {
            this.Close();           
        }


        private void TxtPruebaENTER_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '*')// (char)(Keys.Multiply))
            {
                e.Handled = true;
                return;
            }

            if (e.KeyChar == (char)(Keys.Enter))
            {
                e.Handled = true;
                SendKeys.Send("{TAB}");
            }
        }
    
        private void formIngresoEmbutidoRapido_Load(object sender, EventArgs e)
        {            
            this.Text += Utilidades.Conexion.getSucursalConexion();
            if (oUsuario == null)
            {
                this.Close();
            }
            else
            {
                timer1.Interval = Convert.ToInt32(ConfigurationManager.AppSettings["timerForm"].ToString());
                checkLeerPeso.Visible = FormPrincipal.logueado || Convert.ToBoolean(ConfigurationManager.AppSettings["leerPeso"].ToString());
                cargarComboSucursal();
                txtSucursal.Text = comboSucursal.Text;
                txtUsuario.Text = oUsuario.Nombre;
                comboSucursal.Visible = FormPrincipal.logueado;
                txtSucursal.Visible = !FormPrincipal.logueado;

                txtCodigoEmbutido.Text = oCorteEmbutidoE.codigo.ToString();
                txtEmbutido.Text = oCorteEmbutidoE.CorteDesc;
                oFormulaE = oCorteN.findFormulaByID(0, oCorteEmbutidoE.idCorte);
                txtReceta.Text = oFormulaE.Receta;
                txtCantKgs.Focus();

                tipoDeCorte();
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
                   // txtCodCorteEnEmbutido.Focus();
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
               // MessageBox.Show(ex.Message);
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

                nroErrorBalanza++;
                //si tira error mas de 5 veces se desactiva balanza automaticamente y se pone contador en serio
                if (nroErrorBalanza > 20)
                {
                    timer1.Stop();
                    nroErrorBalanza = 0;
                    MessageBox.Show("Balanza desactivada automaticamente");
                }
            }
        }

        private void formIngresoEmbutidoRapido_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = salir();
        }

        private bool salir()
        {
            bool ret = false;
            if (!saveChanges)
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
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Hubo un error al verificar el tipo del corte.\n\n"+ ex.Message + "\n" + ex.StackTrace);
            }
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            switch (keyData)
            {

                case Keys.Multiply:
                    dejarDeLeerPeso = checkLeerPeso.Checked;
                    checkLeerPeso.Checked = FormPrincipal.leerBalanza ? !checkLeerPeso.Checked : checkLeerPeso.Checked;
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
                case Keys.Escape:
                    this.Close();
                    break;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void button7_Click(object sender, EventArgs e)
        {
            Button boton = (Button)sender;
            txtCantKgs.Text += boton.Text;
        }

        private void btnLimpiar_Click(object sender, EventArgs e)
        {
            txtCantKgs.Text = "";
        }

        private void txtCantKgs_TextChanged(object sender, EventArgs e)
        {
            calcularFormula();
        }

        private void btnReceta_Click(object sender, EventArgs e)
        {
            formReceta frmReceta = new formReceta(txtReceta.Text); // Pasar el texto actual
            frmReceta.editar = false;
        }
    }
}
