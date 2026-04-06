using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.util;
using System.Windows.Forms;
using iTextSharp.text.html;
using Presentacion.Cortes;
using wsAFIPvs2008.WSPSA4;


namespace Presentacion
{
    public partial class formNuevoCorte : formBaseColor, InterfaceCorte, InterfacePersona
    {
        public int idCorte = 0;
        Entidades.Corte oCorteMaestroE=new Entidades.Corte();
        Negocio.Corte oCorteN = new Negocio.Corte(FormPrincipal.EmpresaSTATIC, FormPrincipal.ParametrosCTX);
        Negocio.Usuario oUsuarioN = new Negocio.Usuario(FormPrincipal.EmpresaSTATIC, FormPrincipal.ParametrosCTX);  
        Entidades.Corte oCorteE=new Entidades.Corte();
        Entidades.Corte oExisteCorte = new Entidades.Corte();
        public formCortes frmCorte;// = new formCortes();
        public formInfoCorte oFrmInfoCorte;
        Entidades.Persona oMarca;

        string mensaje = "";

        bool modificar = false;
        bool huboModificacion = false;
        bool actualizarFormCortes = false;

        string modo;
        const string AsignarMaestro = "Asignar Maestro";
        const string Presentacion = "Presentación";

        string[] nombreGroupBox = { "Prod. Maestro", "Presentación de" };
        string[] labelPorcentaje = { "% en Prod. M", "Cant.Unidades" };

        bool ignorarCheckBoxChanged = false;
        public formNuevoCorte()
        {
            InitializeComponent(); this.Icon = Properties.Resources.CarniSys_ICONO;            
        }

        #region eventos

        private void btnGuardar_Click(object sender, EventArgs e)
        {
            agregarCorte();
        }

        private void btnCancelar_Click(object sender, EventArgs e)
        {
            if (huboModificacion)
            {
                DialogResult resp = MessageBox.Show("¿Está seguro que desea salir sin guardar los datos?"
                    , "Atención", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
                if (resp.Equals(DialogResult.No))
                    return;
            }
            this.Close();
        }

        private void btnBuscarCorteM_Click(object sender, EventArgs e)
        {
            formBuscarCorte frmBuscarCorte = new formBuscarCorte();
            frmBuscarCorte.corteSinMaestro = false;
            frmBuscarCorte.Show(this);
        }

        public void EnviarCorte(Entidades.Corte corteMaestro)
        {
            oCorteMaestroE = corteMaestro;
            this.txtCorteMaestro.Text = oCorteMaestroE.corte;
            txtPorcentajeCorteM.Focus();
            huboModificacion = true;

            //Si es mayor a nivel tres se informa que puede haber errores en la contabilidad del stock de los niveles superiores +3
            int nivel = oCorteN.obtenerNivelCorte(oCorteMaestroE.idCorte);
            if (nivel > 3)
            {
                lblActualizar.Text = "Nivel: " + nivel.ToString() + " (mayor a 3)\nSegún el Maestro seleccionado\n puede generar inconsistencia\n en el stock de productos\n con niveles superiores)";
                lblActualizar.Visible = true;
            }
            else
            {
                lblActualizar.Visible = false;
            }
        }

        #endregion

        #region Modificar

        private void cargarCampos()
        {
            this.Text = "Modificar Producto";
            modificar = true;

            txtCodigo.Text = Convert.ToString(oCorteE.codigo);
            txtDescCorte.Text = oCorteE.corte;
            txtPrecioKg.Text = Convert.ToString(oCorteE.precioKg);
            oCorteMaestroE = oCorteE.corteMaestro;
            comboTipo.Text = oCorteE.tipo;
            checkPesable.Checked = oCorteE.Pesable;
            comboAlicuotaIva.SelectedValue = oCorteE.IdAlicuotaIva;
            txtPromedio.Text = oCorteE.Promedio.ToString("F3");
            txtIndependiente.Checked = oCorteE.independiente == 1;
            checkIngresoRapidoEmbutido.Checked = oCorteE.IngresoRapidoEmbutido;
            checkHabilitado.Checked = oCorteE.Habilitado;
            checkEnCierreStock.Checked = oCorteE.EnCierreStock;
            if (oCorteE.corteMaestro != null && oCorteE.corteMaestro.idCorte > 0)
            {
                if (oCorteE.Presentacion)
                {
                    checkPresentacion.Checked = true;
                    checkAsignarMaestro.Enabled = false;
                }
                else
                {
                    checkAsignarMaestro.Checked = true;
                    checkPresentacion.Enabled = false;
                }
            }

            oMarca = oCorteE.Marca;
            cargarMarca();
            cargarCampoCorteMaestro();

            ///cargar grilla proveedores
            ///
            grillaProveedores.DataSource = oCorteN.obtenerCorteProveedor(idCorte);
        }

        private void cargarCampoCorteMaestro()
        {
            txtCorteMaestro.Text = (oCorteE.corteMaestro != null && oCorteE.corteMaestro.idCorte > 0) ? oCorteE.corteMaestro.corte : "-";
            txtPorcentajeCorteM.Text = (oCorteE.corteMaestro != null && oCorteE.corteMaestro.idCorte > 0) ? Convert.ToString(oCorteE.porcentaje) : "";
            txtPorcHueso.Text = (oCorteE.corteMaestro != null && oCorteE.corteMaestro.idCorte > 0) ? Convert.ToString(oCorteE.porcentajeHueso) : "";
            txtDesvioEstandar.Text = (oCorteE.corteMaestro != null && oCorteE.corteMaestro.idCorte > 0) ? oCorteE.desvioEstandar.ToString() : "0";
        }

        #endregion

        #region métodos

        
        public void obtenerFormCorte(formCortes formCorteParam)
        {
            frmCorte = formCorteParam;
        }

        private void agregarCorte()
        {
            if (validar())
            {   
                if (cargarDatosCorte(oCorteE))	
                {
                    if (true)
                    {
                        if (!existeCodigoCorte())
                        {
                            string mensajeExito = "El Producto se " + (oCorteE.idCorte != 0 ? "modificó" : "agregó") +" correctamente.";
                            oCorteN.addOrEditCorte(oCorteE);
                            actualizarFormCortes = true;
                            if (oFrmInfoCorte != null)
                            {
                                oFrmInfoCorte.recibirCorteModificado(oCorteE);
                                this.Close();
                                return;
                            }
                            if (modificar)
                                this.Close();


                            MessageBox.Show(mensajeExito, "",
                                MessageBoxButtons.OK);
                            limpiarCampos();
                            txtCodigo.Focus();
                        }
                    }
                    else
                    {
                        MessageBox.Show("El porcentaje del Producto Maestro y Porcentaje en Hueso debe estar entre 0 y 100%.", "Error en ingreso de porcentaje",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                else
                {
                    MessageBox.Show("Los siguiente campos tienen ingresado datos erroneos:\n" + mensaje);
                }
            }
        }


        private bool cargarDatosCorte(Entidades.Corte oCorteE)
        {
            bool resp = true;
            mensaje = "";

            try
            {
                oCorteE.Codigo = Convert.ToInt64(txtCodigo.Text.Trim());
            }
            catch (Exception)
            {
                resp = false;
                mensaje += "\n" + "-Codigo";
            }
            oCorteE.CorteDesc = txtDescCorte.Text.Trim();
            try
            {
                oCorteE.precioKg = Utilidades.Util_Form.convertFloat(txtPrecioKg.Text, false);
            }
            catch (Exception)
            {
                resp = false;
                mensaje += "\n" + "-Precio Kg";
            }

            try
            {
                oCorteE.Promedio = Utilidades.Util_Form.convertFloat(txtPromedio.Text, false);
            }
            catch (Exception)
            {
                resp = false;
                mensaje += "\n" + "-Promedio";
            }

            oCorteE.PuntoStock = Convert.ToInt32(txtPuntoStock.Text);
            oCorteE.Tipo = comboTipo.Text;
            oCorteE.IdAlicuotaIva = Convert.ToInt32(comboAlicuotaIva.SelectedValue);
            oCorteE.AlicuotaIva = Utilidades.Util_Form.convertFloat(comboAlicuotaIva.Text, false);
            oCorteE.IngresoRapidoEmbutido = checkIngresoRapidoEmbutido.Checked;
            oCorteE.Pesable = checkPesable.Checked;
            oCorteE.EnCierreStock = checkEnCierreStock.Checked;
            oCorteE.Habilitado = checkHabilitado.Checked;
            oCorteE.independiente = txtIndependiente.Checked ? 1 : 0;

            oCorteE.Marca = oMarca;
            oCorteE.CorteMaestro = oCorteMaestroE;
            oCorteE.Presentacion = checkPresentacion.Checked;

            try
            {
                oCorteE.Porcentaje = (checkAsignarMaestro.Checked || checkPresentacion.Checked ) ? Utilidades.Util_Form.convertFloat(txtPorcentajeCorteM.Text, false) : 100;
            }
            catch (Exception)
            {
                resp = false;
                mensaje += "\n" + "- " + lblPorc_Pres;
            }
            try
            {
                oCorteE.porcentajeHueso = checkAsignarMaestro.Checked ? Utilidades.Util_Form.convertFloat(txtPorcHueso.Text, false) : 0;
            }
            catch (Exception)
            {
                resp = false;
                mensaje += "\n" + "- % Desperdicio";
            }

            try
            {
                oCorteE.desvioEstandar = checkAsignarMaestro.Checked ? Utilidades.Util_Form.convertFloat(txtDesvioEstandar.Text, false) : 0;
            }
            catch (Exception)
            {
                resp = false;
                mensaje += "\n" + "- Desvío Estandar";
            }
            return resp;
        }

        private bool existeCodigoCorte()
        {
            if (string.IsNullOrEmpty(txtCodigo.Text))
                return true;
            //Si la cantidad de filas es cero en el DataTable el codigo no existe y se la asigna al nuevo corte
            bool existeCodigo = false;
            DataTable dt = oCorteN.buscarCodigoCorte(Convert.ToInt64(txtCodigo.Text));
            foreach (DataRow  fila in dt.Rows)
            {
                if (!fila["idCorte"].ToString().Equals(oCorteE.idCorte.ToString()))
                {
                    existeCodigo = true;
                    MessageBox.Show("El código ingresado ya está asignado a \'"+ fila["corte"].ToString() + "\'. \nElija otro código o modifique el código del Producto que lo tiene asignado.", "Complete los campos",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    txtCodigo.Focus();
                    txtCodigo.SelectAll();
                    break;
                }
            }
            return existeCodigo;
        }

        private bool validar()
        {
            //validar Corte Maestro
            if ((checkAsignarMaestro.Checked || checkPresentacion.Checked) && (oCorteMaestroE == null || oCorteMaestroE.idCorte == 0))
            {
                MessageBox.Show("Debe ingresar "+modo, "Ingrese " + modo,
                      MessageBoxButtons.OK, MessageBoxIcon.Information);
                return false;
            }

            if (this.txtCodigo.Text.Equals("") || this.txtDescCorte.Text.Equals("") 
                || this.comboTipo.Text.Equals("") || this.txtPuntoStock.Text.Equals(""))
            {
                MessageBox.Show("Debe Completar todos los campos *.", "Complete los campos", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return false;
            }
            else
            {
                if ((checkAsignarMaestro.Checked || checkPresentacion.Checked))// && this.txtPorcentajeCorteM.Text.Equals("0"))
                {
                    if (modo.Equals(AsignarMaestro))
                    {
                        float porcentaje;
                        float desperdicio = 0;

                        bool esValido = float.TryParse(txtPorcentajeCorteM.Text,
                                                        System.Globalization.NumberStyles.Float,
                                                        System.Globalization.CultureInfo.InvariantCulture,
                                                        out porcentaje);
                        if (porcentaje <= 0 || porcentaje > 100)
                        {
                            MessageBox.Show("% en Producto M no puede ser 0. Ingrese el porcentaje entre 1 y 100 que corresponde al Producto Maestro", "Complete los campos",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                            return false;
                        }
                        if (desperdicio < 0 || desperdicio > 100)
                        {
                            MessageBox.Show("% desperdicio no puede ser menor a 0 ni mayor a 100. Ingrese el porcentaje correcto.", "Complete los campos",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                            return false;
                        }
                        return true;
                    }

                    if(modo.Equals(Presentacion) && !Utilidades.Util_Form.validarCampoNumeroEntero(txtPorcentajeCorteM.Text, lblPorc_Pres.Text))
                    {
                        MessageBox.Show(lblPorc_Pres.Text + " debe ser un número entero", "Complete los campos",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return false;
                    }

                    return true;
                } 
                else
                {
                    return true;
                }
            }
        }

        private void limpiarCampos()
        {
            idCorte = 0;
            oCorteMaestroE = new Entidades.Corte();
            oCorteE = new Entidades.Corte();

            txtCodigo.Text = "";
            txtDescCorte.Text = "";
            txtPrecioKg.Text = "";
            oCorteMaestroE = null;
            comboTipo.SelectedIndex = -1;
            txtPromedio.Text = "";
            txtPuntoStock.Text = "0";
            txtIndependiente.Checked = true;
            checkIngresoRapidoEmbutido.Checked = false;
            checkHabilitado.Checked = true;
            checkEnCierreStock.Checked = true;
            checkAsignarMaestro.Checked = false;
            txtCorteMaestro.Text = "";
            txtPorcentajeCorteM.Text = "";
            txtPorcHueso.Text = "0";
            txtDesvioEstandar.Text = "0";
            huboModificacion = false;
            oMarca = null;
            cargarMarca();
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

        private void formNuevoCorte_Load(object sender, EventArgs e)
        {
            try
            {
                this.Text += Utilidades.Conexion.getSucursalConexion();
                comboTipo.DataSource = oCorteN.obtenerTiposProducto(false);
                comboTipo.DisplayMember = "tipo";
                comboTipo.ValueMember = "tipo";
                comboTipo.SelectedIndex = -1;

                comboAlicuotaIva.DataSource = oCorteN.obtenerAlicuotasIva(false);
                comboAlicuotaIva.DisplayMember = "iva";
                comboAlicuotaIva.ValueMember = "idIva";

                checkMayuscula.Checked = FormPrincipal.ParametrosCTX.GetBool01(Entidades.ParamKeys.Mayuscula, false);;
                txtCodigo.Focus();

                checkSugerirCodigo.Visible = !(idCorte > 0);//solo visible para altas de productos
                if (idCorte > 0)
                {
                    oCorteE = oCorteN.findCorteById(idCorte, true);
                    cargarCampos();
                }

                if (!Usuarios.FormValidarPermiso.validarPermiso(this.Name))
                {
                    this.Close();
                }

                groupBox1.Select();
                huboModificacion = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void checkAsignarMaestro_CheckedChanged(object sender, EventArgs e)
        {
            checkPresentacion.Enabled = !checkAsignarMaestro.Checked;

            modo = checkAsignarMaestro.Checked ? AsignarMaestro : null;
            //si cambia a unChecked y tiene corteMaestro se informa
            if (!checkAsignarMaestro.Checked && oCorteMaestroE != null && oCorteMaestroE.idCorte > 0)
            {
                DialogResult resp = MessageBox.Show("Si quita la asignación se borrará el Producto Maestro.\n\n¿Desea quitar el Producto maestro?"
                    , "Atención", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
                if (resp.Equals(DialogResult.Yes))
                {
                    //oCorteMaestroE = oCorteE;
                    //oCorteE.CorteMaestro = oCorteMaestroE;
                    oCorteMaestroE = null;
                    oCorteE.CorteMaestro = oCorteMaestroE;
                    cargarCampoCorteMaestro();
                }
                else
                {
                    checkAsignarMaestro.Checked = !checkAsignarMaestro.Checked;
                }
            }
            groupMaestro.Enabled = checkAsignarMaestro.Checked;
            huboModificacion = true;
            if (checkAsignarMaestro.Checked)
            {
                SetearGroupBox();
            }
        }

        private void txtCodigo_TextChanged(object sender, EventArgs e)
        {
            huboModificacion = true;
        }

        private void txtIndependiente_CheckedChanged(object sender, EventArgs e)
        {
            huboModificacion = true;
        }

        private void comboTipo_SelectedIndexChanged(object sender, EventArgs e)
        {
            getCodigoSugerido();
            //Corte
            //Embutido
            //Unidad
            //Otro
            switch (comboTipo.Text)
            {
                case "Corte":
                    txtPromedio.ReadOnly = false;
                    txtPromedio.Text = oCorteE.Promedio.ToString("F3");
                    break;
                case "Unidad":
                    txtPromedio.ReadOnly = true;
                    txtPromedio.Text = "1";
                    break;
                case "Embutido":
                    txtPromedio.ReadOnly = false;
                    txtPromedio.Text = "0";
                    break;
                default:
                    txtPromedio.ReadOnly = false;
                    txtPromedio.Text = "0";
                    break;
            }
        }

        private void formNuevoCorte_FormClosed(object sender, FormClosedEventArgs e)
        {            
            if (actualizarFormCortes && frmCorte != null)
            {
                //para evitar cargar la grilla, solo se muestra lblActualizar si hubo modificaciones en los cortes
                frmCorte.actualizarForm_Mensaje();
            }
        }

        private void checkPesable_CheckedChanged(object sender, EventArgs e)
        {
            huboModificacion = true;
        }

        private void txtDescCorte_TextChanged(object sender, EventArgs e)
        {
            huboModificacion = true;

            if (checkMayuscula.Checked)
            {
                // Guardar la posición actual del cursor
                int cursorPosition = txtDescCorte.SelectionStart;

                // Convertir el texto a mayúsculas
                txtDescCorte.Text = txtDescCorte.Text.ToUpper();

                // Restaurar la posición del cursor
                txtDescCorte.SelectionStart = cursorPosition;
            }
        }

        private void txtPuntoStock_TextChanged(object sender, EventArgs e)
        {
            huboModificacion = true;
            if (!Utilidades.Util_Form.validarCampoNumeroEntero(txtPuntoStock.Text, "Punto Stock"))
            {
                txtPuntoStock.Text = "0";
                txtPuntoStock.SelectAll();
            }
        }

        private void btnMarca_Click(object sender, EventArgs e)
        {
            buscarMarca();
        }

        private void buscarMarca()
        {
            ///TODO: llamar a form marcas, tambien en addorEdit cortes y en reportes
            ///
            formMarcas frmMarcas = new formMarcas();
            frmMarcas.buscardorMarcas = true;
            frmMarcas.Show(this);
            comboTipo.Focus();
        }

        //comunicación con interface
        public void EnviarPersona(Entidades.Persona marca)
        {
            oMarca = marca;
            cargarMarca();
        }

        private void btnBorrarMarca_Click(object sender, EventArgs e)
        {
            DialogResult resp = MessageBox.Show("¿Eliminar Marca?"
                    , "Atención", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
            if (resp.Equals(DialogResult.No))
                return;

            oMarca = null;
            cargarMarca();
        }

        private void cargarMarca()
        {
            this.txtMarca.Text = oMarca != null ? oMarca.RazonSocial : ""; 
            btnBorrarMarca.Visible = string.IsNullOrEmpty(txtMarca.Text) ? false : true;
        }

        private void txtCodigo_Leave(object sender, EventArgs e)
        {
            existeCodigoCorte();
        }

        private void checkSegerirCodigo_CheckedChanged(object sender, EventArgs e)
        {
            checkSugerirCodigo.BackColor = checkSugerirCodigo.Checked ? System.Drawing.Color.DarkSeaGreen : SystemColors.ControlDark;
            if (checkSugerirCodigo.Checked)
            {
                MessageBox.Show("Ha activado la sugerencia para códigos de productos\n\n"+
                    "Al seleccionar el Tipo de Producto el sistema sugerirá el menor Código disponible para ese tipo.",
                    "Sugerir código", MessageBoxButtons.OK, MessageBoxIcon.Information);
                if (!(string.IsNullOrEmpty(comboTipo.Text) && string.IsNullOrEmpty(txtCodigo.Text)))
                {
                    getCodigoSugerido();
                }
            }
        }

        private void getCodigoSugerido()
        {
            if (!(checkSugerirCodigo.Checked && string.IsNullOrEmpty(txtCodigo.Text)))
                return;

            long codigoSugerido = oCorteN.sugerirCodigo(comboTipo.Text);
            if (codigoSugerido < 0 && !string.IsNullOrEmpty(comboTipo.Text))
            {
                MessageBox.Show("El tipo seleccionado aún no tiene asignado ningún producto\n\n" +
                    "Deberá asignar el codigo manualmente.",
                    "Sugerir código", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            txtCodigo.Text = (codigoSugerido < 0) ? "" : codigoSugerido.ToString();
        }

        private void checkedListBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void panelDesperdicio_Paint(object sender, PaintEventArgs e)
        {

        }

        private void groupMaestro_Enter(object sender, EventArgs e)
        {

        }

        private void lblPorc_Pres_Click(object sender, EventArgs e)
        {

        }

        private void checkPresentacion_CheckedChanged(object sender, EventArgs e)
        {
            checkAsignarMaestro.Enabled = !checkPresentacion.Checked;

            modo = checkPresentacion.Checked ? Presentacion : null;
            //si cambia a unChecked y tiene corteMaestro se informa
            if (!checkPresentacion.Checked && oCorteMaestroE != null && oCorteMaestroE.idCorte > 0)
            {
                DialogResult resp = MessageBox.Show("Si quita la asignación se borrará la Presentación del Producto.\n\n¿Desea quitar la Presentación?"
                    , "Atención", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
                if (resp.Equals(DialogResult.Yes))
                {
                    //oCorteMaestroE = oCorteE;
                    //oCorteE.CorteMaestro = oCorteMaestroE;
                    oCorteMaestroE = null;
                    oCorteE.CorteMaestro = oCorteMaestroE;
                    cargarCampoCorteMaestro();
                }
                else
                {
                    checkPresentacion.Checked = !checkPresentacion.Checked;
                }
            }
            groupMaestro.Enabled = checkPresentacion.Checked;
            huboModificacion = true;

            if (checkPresentacion.Checked)
            {
                SetearGroupBox();
            }
        }

        private void SetearGroupBox()
        {
            groupMaestro.Text = modo.Equals(AsignarMaestro) ? nombreGroupBox[0] : nombreGroupBox[1];
            lblPorc_Pres.Text = modo.Equals(AsignarMaestro) ? labelPorcentaje[0] : labelPorcentaje[1];
            panelDesperdicio.Visible = modo.Equals(AsignarMaestro);
        }
    }
}
