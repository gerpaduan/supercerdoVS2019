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
            txtIndependiente.Checked = oCorteE.independiente == 1;

            if (oCorteE.idCorte != oCorteE.corteMaestro.idCorte)
            {
                checkAsignarMaestro.Checked = true;
            }
            cargarCampoCorteMaestro();
        }

        private void cargarCampoCorteMaestro()
        {
            txtCorteMaestro.Text = oCorteE.corteMaestro.corte;
            txtPorcentajeCorteM.Text = Convert.ToString(oCorteE.porcentaje);
            txtPorcHueso.Text = Convert.ToString(oCorteE.porcentajeHueso);
            txtDesvioEstandar.Text = oCorteE.desvioEstandar.ToString();
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
                        bool cerrarForm = false;

                        if (!existeCodigoCorte())
                        {
                            oCorteN.addOrEditCorte(oCorteE);
                            cerrarForm = true;
                            if (frmCorte != null)
                            {
                                frmCorte.cargarGrilla();
                            }
                            else
                            {
                                if(oFrmInfoCorte != null) oFrmInfoCorte.recibirCorteModificado(oCorteE);
                            }
                        }
                        
                        if(cerrarForm) this.Close();
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
                oCorteE.Codigo = Convert.ToInt32(txtCodigo.Text.Trim());
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

            oCorteE.Tipo = comboTipo.Text;

            if (txtIndependiente.Checked.Equals(true))
            {
                oCorteE.independiente = 1;
                
            }
            else
            {
                oCorteE.independiente = 0;
            }

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
            if (this.txtCodigo.Text.Equals("") || this.txtDescCorte.Text.Equals("") 
                || this.comboTipo.Text.Equals("")|| (checkAsignarMaestro.Checked && 
                ( this.txtCorteMaestro.Text.Equals("") || this.txtPorcentajeCorteM.Text.Equals(""))))
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

                if (idCorte > 0)
                {
                    oCorteE = oCorteN.getCorteById(idCorte, true);
                    cargarCampos();
                }

                if (Presentacion.FormPrincipal.logueado == false)
                {
                    MessageBox.Show("No está logueado!.\nInicie sesión y vuelva a intentar.");
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
                    oCorteMaestroE = oCorteE;
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
    }
}
