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
        Entidades.Corte oCorteMaestroE=new Entidades.Corte();
        Negocio.Corte oCorteN = new Negocio.Corte();
        Entidades.Corte oCorteE=new Entidades.Corte();
        formCortes frmCorte;// = new formCortes();
        formInfoCorte oFrmInfoCorte;

        string mensaje = "";

        bool modificar = false;

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
            this.Close();
        }

       

        private void btnBuscarCorteM_Click(object sender, EventArgs e)
        {
            formBuscarCorte frmBuscarCorte = new formBuscarCorte();
            frmBuscarCorte.Show(this);
        }

        public void EnviarCorte(Entidades.Corte corteMaestro)
        {
            oCorteMaestroE = corteMaestro;
            this.txtCorteMaestro.Text = oCorteMaestroE.corte;
            txtPorcentajeCorteM.Focus();
        }

        private void comboTipo_TextChanged(object sender, EventArgs e)
        {
            cambiaTipo();
        }

        #endregion

        #region Modificar

        public void obtenerCorteFormInfoCorte(Entidades.Corte corteParam, formInfoCorte frmInfoCorteParam)
        {
            oFrmInfoCorte = frmInfoCorteParam;
            oCorteE = corteParam;
            cargarCampos();
        }

        public void obtenerCorteFormCortes(Entidades.Corte corteParam, formCortes frmCortesParam)
        {
            frmCorte = frmCortesParam;
            oCorteE = corteParam;
            cargarCampos();
        }

        private void cargarCampos()
        {
            this.Text = "Modificar Corte";
            modificar = true;

            txtCodigo.Text = Convert.ToString(oCorteE.codigo);
            txtDescCorte.Text = oCorteE.corte;
            txtPrecioKg.Text = Convert.ToString(oCorteE.precioKg);
            oCorteMaestroE = oCorteE.corteMaestro;
            comboTipo.Text = oCorteE.tipo;

            if (oCorteE.independiente==1)
            {
                txtIndependiente.Checked = true;
            }
            
            //txtCorteMaestro.Text = oCorteE.corteMaestro.corte;
            txtPorcentajeCorteM.Text = Convert.ToString(oCorteE.porcentaje);
            txtPorcHueso.Text = Convert.ToString(oCorteE.porcentajeHueso);
            txtDesvioEstandar.Text = oCorteE.desvioEstandar.ToString();

        }

        #endregion

        #region métodos

        private void cambiaTipo()
        {
            //if (comboTipo.Text.Equals("Embutido") || comboTipo.Text.Equals("Otro"))
            //{
            //    if (comboTipo.Text.Equals("Embutido"))
            //    {
            //        txtCorteMaestro.Text = "Embutido";
            //        txtPorcentajeCorteM.Text = "100";

            //        btnBuscarCorteM.Visible = false;

            //        txtPorcentajeCorteM.ReadOnly = true;
            //    }
            //    else
            //    {
            //        txtCorteMaestro.Text = "Otro";
            //        txtPorcentajeCorteM.Text = "100";

            //        btnBuscarCorteM.Visible = false;

            //        txtPorcentajeCorteM.ReadOnly = true;
            //    }
            //}
            

            //else
            //{
            //    txtPorcentajeCorteM.Text = "";

            //    if (comboTipo.Text.Equals("Corte") )
            //    {
            //        btnBuscarCorteM.Visible = true;
                                        
            //        txtPorcentajeCorteM.ReadOnly = false;

            //        this.txtCorteMaestro.Text = oCorteMaestroE.corte;

            //        this.txtPorcentajeCorteM.Text = Convert.ToString(oCorteE.porcentaje);
                                       
            //    }
               
            //}

            this.txtCorteMaestro.Text = oCorteMaestroE.corte;

            this.txtPorcentajeCorteM.Text = Convert.ToString(oCorteE.porcentaje);
        }

        public void obtenerFormCorte(formCortes formCorteParam)
        {
            frmCorte = formCorteParam;
        }


        private void agregarCorte()
        {
            if (validar())
            {   
                //oCorteE = new Entidades.Corte();

                if (cargarDatosCorte(oCorteE))	
                {
                
                    if (oCorteE.porcentaje <= 100 && oCorteE.porcentajeHueso <= 100 && oCorteE.porcentajeHueso >= 0 && oCorteE.porcentaje >= 0)
                    {
                        if (modificar)
                        {
                            oCorteN.modificarCorte(oCorteE);
                            if (frmCorte != null)
                            {
                                frmCorte.cargarGrilla();
                                //oFrmInfoCorte.recibirCorteModificado(oCorteE);
                            }
                            else
                            {
                                oFrmInfoCorte.recibirCorteModificado(oCorteE);
                            }


                        }
                        else
                        {
                            if (existeCodigoCorte())
                            {
                                oCorteN.agregarCorte(oCorteE);
                                frmCorte.cargarGrilla();
                            }
                        }


                        this.Close();

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
            catch (Exception ex)
            {
                resp = false;
                mensaje += "\n" + "-Codigo";
            }
            
            
            
            oCorteE.CorteDesc = txtDescCorte.Text.Trim();

            try
            {
                try
                {
                    oCorteE.PrecioKg = float.Parse(txtPrecioKg.Text.Trim(), System.Globalization.NumberStyles.Float, new System.Globalization.CultureInfo("en-US"));
                }
                catch (Exception)
                {

                    oCorteE.PrecioKg = float.Parse(txtPrecioKg.Text.Trim());

                }
            }
            catch (Exception ex)
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
                try
                {
                    oCorteE.Porcentaje = float.Parse(txtPorcentajeCorteM.Text.Trim(), System.Globalization.NumberStyles.Float, new System.Globalization.CultureInfo("en-US"));
                }
                catch (Exception)
                {

                    oCorteE.Porcentaje = float.Parse(txtPorcentajeCorteM.Text.Trim());
                }
            }
            catch (Exception ex)
            {
                resp = false;
                mensaje += "\n" + "- % Corte M";
            }


            try
            {
                try
                {
                    oCorteE.porcentajeHueso = float.Parse(txtPorcHueso.Text.Trim(), System.Globalization.NumberStyles.Float, new System.Globalization.CultureInfo("en-US"));
                }
                catch (Exception)
                {

                    oCorteE.porcentajeHueso = float.Parse(txtPorcHueso.Text.Trim());
                }
            }
            catch (Exception ex)
            {
                resp = false;
                mensaje += "\n" + "- % Hueso";
            }

            try
            {
                try
                {
                    oCorteE.desvioEstandar = float.Parse(txtDesvioEstandar.Text.Trim(), System.Globalization.NumberStyles.Float, new System.Globalization.CultureInfo("en-US"));
                }
                catch (Exception)
                {

                    oCorteE.desvioEstandar = float.Parse(txtDesvioEstandar.Text.Trim());
                }
            }
            catch (Exception ex)
            {
                resp = false;
                mensaje += "\n" + "- Desvío Estandar";
            }


            return resp;
        }

        private bool existeCodigoCorte()
        {
            //Si la cantidad de filas es cero en el DataTable el codigo no existe y se la asigna al nuevo corte
            if (oCorteN.buscarCodigoCorte(oCorteE.codigo).Rows.Count == 0)
            {
                return true;
            }
            else
            {
                MessageBox.Show("El código ingresado ya está asignado a un corte. Elija otro código o modifique el código del corte que lo tiene asignado.", "Complete los campos",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return false;
            }
        }

        private bool validar()
        {

            if (this.txtCodigo.Text.Equals("") || this.txtDescCorte.Text.Equals("") 
                || this.comboTipo.Text.Equals("")|| this.txtCorteMaestro.Text.Equals("")
                || this.txtPorcentajeCorteM.Text.Equals(""))
            {
                MessageBox.Show("Debe Completar todos los campos.", "Complete los campos", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return false;
            }

            else
            {
                if (this.txtPorcentajeCorteM.Text.Equals("0"))
                {
                    MessageBox.Show("% en Corte M no puede ser 0. Ingrese el porcentaje entre 1 y 100 que corresponde al Corte Maestro", "Complete los campos",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return false;
                } else{
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

        }

        

        

    }
}
