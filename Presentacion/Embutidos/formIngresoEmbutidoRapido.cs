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
    public partial class formIngresoEmbutidoRapido : formBaseColor//, InterfaceCorte, InterfaceEmbutido, InterfaceUsuario
    {
        Utilidades.SingletonLeerPeso Leer_Peso;

        public formEmbutidos frmEmbutidos = new formEmbutidos();
        DataTable dtSucursales;
        Negocio.Sucursal oSucursalN;
        Negocio.Corte oCorteN=new Negocio.Corte();

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

        Color enableColor = ColorTranslator.FromHtml(ConfigurationManager.AppSettings["enableColor"].ToString()); //SystemColors.Window;
        Color readOnlyColor = ColorTranslator.FromHtml(ConfigurationManager.AppSettings["readOnlyColor"].ToString());//SystemColors.ScrollBar;
        Color focusColor = ColorTranslator.FromHtml(ConfigurationManager.AppSettings["focusColor"].ToString());//Color.Orange;//Color.NavajoWhite;//Color.MediumAquamarine;
        Color ultimoColor = Color.Green;

        float totalKg = 0;//totalPesoEmbutidos

        public formIngresoEmbutidoRapido()
        {
            InitializeComponent();
        }

        #region Métodos

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
        }

        private void agregarEmbutido()
        {
            try
            {                
                if (Utilidades.Util_Form.validarFechaConAdmin(Presentacion.FormPrincipal.logueado, txtFechaEmbutido.Value, "Fecha") &&
                    Utilidades.Util_Form.validarSucursal(Presentacion.FormPrincipal.logueado, Convert.ToInt32(comboSucursal.SelectedValue.ToString()))
                    && validarCantKgs())
                {
                    cargarEmbutido();
                    oEmbutidoE.idEmbutido = oCorteN.agregarEmbutido(oEmbutidoE);

                    //Si CorteEn es igual a CorteEn2 cargar como rebozado
                    if(oCorteE.codigo != oCorteE2.codigo)
                    {
                        cargarRebozado();
                    }
                    cargarCortePorEmbutido(oCorteE, txtCantKgs.Text);
                    oCorteN.agregarCortePorEmbutido(oCortePorEmbutidoE);
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


        //private void agregarCorteEnEmbutido()
        //{
        //    if (validarCantKgs())
        //    {
        //        cargarCortePorEmbutido();            
        //    }
        //}


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
            if (e.KeyChar == (char)(Keys.Enter))
            {
                e.Handled = true;
                SendKeys.Send("{TAB}");
            }
        }
        
        //Agregando rebozado si es Milanesa
        private void cargarRebozado()
        {
            //txtCodCorteEnEmbutido.Text = "301";

            float rebozado = 0;
            switch (oEmbutidoE.Corte.codigo)
            {
                //pote
                case 13:
                    //El pote es de 700 grms y la mema entonces es del 300 grms en la Unidad
                    rebozado = float.Parse(txtCantKgs.Text) * (1-Entidades.Parametros.porcGrasaEnPote);
                    txtCantKgs.Text = (float.Parse(txtCantKgs.Text) * Entidades.Parametros.porcGrasaEnPote).ToString();
                    break;
                //milanesa
                case 17:
                    rebozado = float.Parse(txtCantKgs.Text) * Entidades.Parametros.porcPanRayadoMilanesa;
                    break;
                //grasa liquida
                case 59:
                    //Grasa liquidad tiene merma en el kilo cocinado por chicharron y evaporacion
                    rebozado = -1 * float.Parse(txtCantKgs.Text) * (1 - Entidades.Parametros.porcGrasaLiquida);
                    break;
                default:
                    break;
            } 
            cargarCortePorEmbutido(oCorteE2, rebozado.ToString());
            oCorteN.agregarCortePorEmbutido(oCortePorEmbutidoE);
        }
        //Fin rebozado


        private void agregarCorteEnEmbutido()
        {
            //if (validar() && validarCantKgs())
            //{
            //    cargarCortePorEmbutido();
            //    //cargarCorteEnLista();
            //    listaCortesEnGrilla.Add(cortePorEmbutido);
            //    listaCortePorEmbutido.Add(oCortePorEmbutidoE);

            //    cargarGrilla();

            //    oCorteE = null;//libero el objeto
            //    oCortePorEmbutidoE = null;//libero el objeto
            //    limpiarCampos();

            //    txtCodCorteEnEmbutido.Focus();
            //}
        }

        //private void cargarCorteEnEmbutido()
        //{
        //    try
        //    {
        //        if (txtCodCorteEnEmbutido.Text.Trim() != "")
        //        {
        //            oCorteE = new Entidades.Corte();
        //            DataTable dtCorte = new DataTable();
        //            dtCorte = oCorteN.buscarCodigoCorte(Convert.ToInt32(txtCodCorteEnEmbutido.Text.Trim()));

        //            if (dtCorte.Rows.Count > 0)
        //            {
        //                foreach (DataRow fila in dtCorte.Rows)
        //                {
        //                    oCorteE.idCorte = Convert.ToInt32(fila["idCorte"].ToString());
        //                    oCorteE.codigo = Convert.ToInt32(fila["codigo"].ToString());
        //                    oCorteE.corte = fila["corte"].ToString();
        //                    oCorteE.tipo = fila["tipo"].ToString();
        //                }
        //                //se cargan los datos del corte
        //                txtCorteEnEmbutido.Text = oCorteE.corte;
        //            }
        //            else
        //            {
        //                txtCorteEnEmbutido.Text = "";
        //                oCorteE = null;
        //            }
        //        }
        //        else
        //        {
        //            txtCorteEnEmbutido.Text = "";
        //            oCorteE = null;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show("Error en método cargarCorteEnEmbutido().\n\n"+ex.Message);
        //    }
        //}

        private void txtCodCorteEnEmbutido_TextChanged(object sender, EventArgs e)
        {
            //cargarCorteEnEmbutido();
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

                //txtCorteEnEmbutido.Text = oCorteE.codigo.ToString();
                //txtCorteEnEmbutido.Text = oCorteE.corte;

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
    }
}
