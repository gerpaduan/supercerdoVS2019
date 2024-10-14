using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Presentacion.Cortes;


namespace Presentacion
{
    public partial class formNuevoCorte : formBaseColor, InterfaceCorte
    {
        public int idCorte = 0;
        Entidades.Corte oCorteMaestroE=new Entidades.Corte();
        Negocio.Corte oCorteN = new Negocio.Corte();
        Entidades.Corte oCorteE=new Entidades.Corte();
        public formCortes frmCorte;// = new formCortes();
        public formInfoCorte oFrmInfoCorte;

        string mensaje = "";

        bool modificar = false;
        bool huboModificacion = false;

        public formNuevoCorte()
        {
            InitializeComponent();            
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
        }

        #endregion

        #region Modificar

        private void cargarCampos()
        {
            this.Text = "Modificar Corte";
            modificar = true;

            txtCodigo.Text = Convert.ToString(oCorteE.codigo);
            txtDescCorte.Text = oCorteE.corte;
            txtPrecioKg.Text = Convert.ToString(oCorteE.precioKg);
            oCorteMaestroE = oCorteE.corteMaestro;
            comboTipo.Text = oCorteE.tipo;
            comboAlicuotaIva.SelectedValue = oCorteE.IdAlicuotaIva;
            txtPromedio.Text = oCorteE.Promedio.ToString("F3");
            txtIndependiente.Checked = oCorteE.independiente == 1;
            checkIngresoRapidoEmbutido.Checked = oCorteE.IngresoRapidoEmbutido;
            checkHabilitado.Checked = oCorteE.Habilitado;
            checkEnCierreStock.Checked = oCorteE.EnCierreStock;
            checkAsignarMaestro.Checked = (oCorteE.corteMaestro != null && oCorteE.corteMaestro.idCorte > 0);
            
            cargarCampoCorteMaestro();
        }

        private void cargarCampoCorteMaestro()
        {
            txtCorteMaestro.Text = (oCorteE.corteMaestro != null && oCorteE.corteMaestro.idCorte > 0) ? oCorteE.corteMaestro.corte : "-";
            txtPorcentajeCorteM.Text = (oCorteE.corteMaestro != null && oCorteE.corteMaestro.idCorte > 0) ? Convert.ToString(oCorteE.porcentaje) : "";
            txtPorcHueso.Text = (oCorteE.corteMaestro != null && oCorteE.corteMaestro.idCorte > 0) ? Convert.ToString(oCorteE.porcentajeHueso) : "";
            txtDesvioEstandar.Text = (oCorteE.corteMaestro != null && oCorteE.corteMaestro.idCorte > 0) ? oCorteE.desvioEstandar.ToString() : "";
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
                    if (oCorteE.porcentaje <= 100 && oCorteE.porcentajeHueso <= 100 && oCorteE.porcentajeHueso >= 0 && oCorteE.porcentaje >= 0)
                    {
                        if (!existeCodigoCorte())
                        {
                            oCorteN.addOrEditCorte(oCorteE);
                            if (oFrmInfoCorte != null)
                            {
                                oFrmInfoCorte.recibirCorteModificado(oCorteE);
                                this.Close();
                                return;
                            }
                            if (modificar)
                                this.Close();


                            MessageBox.Show("El corte se agregó correctamente.", "",
                                MessageBoxButtons.OK);
                            limpiarCampos();
                            txtCodigo.Focus();
                        }
                    }
                    else
                    {
                        MessageBox.Show("El porcentaje del Corte Maestro y Porcentaje en Hueso debe estar entre 0 y 100%.", "Error en ingreso de porcentaje",
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

            oCorteE.Tipo = comboTipo.Text;
            oCorteE.IdAlicuotaIva = Convert.ToInt32(comboAlicuotaIva.SelectedValue);
            oCorteE.AlicuotaIva = Utilidades.Util_Form.convertFloat(comboAlicuotaIva.Text, false);
            oCorteE.IngresoRapidoEmbutido = checkIngresoRapidoEmbutido.Checked;
            oCorteE.Pesable = checkPesable.Checked;
            oCorteE.EnCierreStock = checkEnCierreStock.Checked;
            oCorteE.Habilitado = checkHabilitado.Checked;
            oCorteE.independiente = txtIndependiente.Checked ? 1 : 0;

            oCorteE.CorteMaestro = oCorteMaestroE;

            try
            {
                oCorteE.Porcentaje = checkAsignarMaestro.Checked ? Utilidades.Util_Form.convertFloat(txtPorcentajeCorteM.Text, false) : 100;
            }
            catch (Exception)
            {
                resp = false;
                mensaje += "\n" + "- % Corte M";
            }
            try
            {
                oCorteE.porcentajeHueso = checkAsignarMaestro.Checked ? Utilidades.Util_Form.convertFloat(txtPorcHueso.Text, false) : 0;
            }
            catch (Exception)
            {
                resp = false;
                mensaje += "\n" + "- % Hueso";
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
            //Si la cantidad de filas es cero en el DataTable el codigo no existe y se la asigna al nuevo corte
            bool existeCodigo = false;
            DataTable dt = oCorteN.buscarCodigoCorte(oCorteE.codigo);
            foreach (DataRow  fila in dt.Rows)
            {
                if (!fila["idCorte"].ToString().Equals(oCorteE.idCorte.ToString()))
                {
                    existeCodigo = true;
                    MessageBox.Show("El código ingresado ya está asignado a un corte. Elija otro código o modifique el código del corte que lo tiene asignado.", "Complete los campos",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    break;
                }
            }
            return existeCodigo;
        }

        private bool validar()
        {
            //validar Corte Maestro
            if (checkAsignarMaestro.Checked && (oCorteMaestroE == null || oCorteMaestroE.idCorte == 0))
            {
                MessageBox.Show("Debe ingresar el corte maestro", "Ingrese Corte Maestro",
                      MessageBoxButtons.OK, MessageBoxIcon.Information);
                return false;
            }

            if (this.txtCodigo.Text.Equals("") || this.txtDescCorte.Text.Equals("") 
                || this.comboTipo.Text.Equals(""))
            {
                MessageBox.Show("Debe Completar todos los campos.", "Complete los campos", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return false;
            }
            else
            {
                if (checkAsignarMaestro.Checked && this.txtPorcentajeCorteM.Text.Equals("0"))
                {
                    MessageBox.Show("% en Corte M no puede ser 0. Ingrese el porcentaje entre 1 y 100 que corresponde al Corte Maestro", "Complete los campos",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return false;
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

                txtCodigo.Focus();

                if (idCorte > 0)
                {
                    oCorteE = oCorteN.getCorteById(idCorte, true);
                    cargarCampos();
                }

                if (!Usuarios.FormValidarPermiso.validarPermiso())
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
            //si cambia a unChecked y tiene corteMaestro se informa
            if (!checkAsignarMaestro.Checked && oCorteMaestroE != null && oCorteMaestroE.idCorte > 0)
            {
                DialogResult resp = MessageBox.Show("Si quita la asignación se borrará el Corte Maestro.\n\n¿Desea quitar el corte maestro?"
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
            if (frmCorte != null)
            {
                frmCorte.cargarGrilla();
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
    }
}
