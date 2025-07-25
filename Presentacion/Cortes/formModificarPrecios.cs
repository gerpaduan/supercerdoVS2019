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
    public partial class formModificarPrecios: formBaseColor
    {
        Entidades.Corte oCorteMaestroE=new Entidades.Corte();
        Negocio.Corte oCorteN = new Negocio.Corte();
        Entidades.Corte oCorteE=new Entidades.Corte();
        List<Entidades.Corte> listCortes = new List<Entidades.Corte>();
        formCortes frmCorte;
        formInfoCorte oFrmInfoCorte;

        string mensaje = "";
        bool modificar = true;
        public bool precioPorPorc = false;   
        public bool finalizarMod = false;

        public formModificarPrecios()
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
            finalizarMod = true;
            this.Close();
        }

        #endregion

        #region Modificar

        public void obtenerCorteFormInfoCorte(Entidades.Corte corteParam, formInfoCorte frmInfoCorteParam)
        {
            oFrmInfoCorte = frmInfoCorteParam;
            oCorteE = corteParam;
            cargarCampos();
        }

        public void obtenerCorteFormCortes(Entidades.Corte corteParam, List<Entidades.Corte> listCortesParam, formCortes frmCortesParam)
        {
            frmCorte = frmCortesParam;
            oCorteE = corteParam;
            listCortes = listCortesParam;
            cargarCampos();
        }

        private void cargarCampos()
        {
            this.Text = "Modificar Corte";
            modificar = true;

            txtCodigo.Text = Convert.ToString(oCorteE.codigo);
            txtDescCorte.Text = oCorteE.corte;
            txtPrecioKg.Text = Convert.ToString(oCorteE.precioKg);

            txtPrecioKg.SelectAll();
            txtPrecioKg.Focus();
        }

        #endregion

        #region métodos

        public void obtenerFormCorte(formCortes formCorteParam)
        {
            frmCorte = formCorteParam;
        }

        private void agregarCorte()
        {
            if (precioPorPorc)
            {
                if (!Utilidades.Util_Form.validarCampoNumerico(txtPorcentaje.Text, "%"))
                    return;
                
                DialogResult resp = MessageBox.Show("Esta acción modificará los precios de toda la lista de Productos que se muestra en el formulario y no tiene vuelta atrás. ¿Deseas continuar?",
                    "Advertencia", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);
                if (resp == DialogResult.No)
                    return;

                float porcentaje = Utilidades.Util_Form.convertFloat(txtPorcentaje.Text, false);
                if (porcentaje < -100)
                {
                    MessageBox.Show("El porcentaje debe ser mayor a -100", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                float porcentajeDescuento = (1 + porcentaje / 100);


                foreach (Entidades.Corte filaCorte in listCortes)
                {
                    filaCorte.precioKg = filaCorte.precioKg * porcentajeDescuento;
                    oCorteN.editPrecioCorte(filaCorte);
                }

                if (frmCorte != null)
                {
                    //para evitar cargar la grilla, solo se muestra lblActualizar si hubo modificaciones en los cortes
                    frmCorte.actualizarForm_Mensaje();
                    //frmCorte.cargarGrilla();

                }
                this.Close();
            }
            else
            {
                if (cargarDatosCorte(oCorteE))
                {
                    oCorteN.editPrecioCorte(oCorteE);
                    if (frmCorte != null)
                    {
                        //para evitar cargar la grilla, solo se muestra lblActualizar si hubo modificaciones en los cortes
                        frmCorte.actualizarForm_Mensaje();
                        //frmCorte.cargarGrilla();
                    }
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Los siguiente campos tienen ingresado datos erroneos:\n" + mensaje);
                    txtPrecioKg.Focus();
                }
            }         
        }

        private bool cargarDatosCorte(Entidades.Corte oCorteE)
        {
            bool resp = true;
            mensaje = "";

            try
            {
                oCorteE.precioKg = Utilidades.Util_Form.convertFloat(txtPrecioKg.Text, false);
            }
            catch (Exception)
            {
                resp = false;
                mensaje += "\n" + "-Precio Kg";
            }

            return resp;
        }

        #endregion


        private void TxtPruebaENTER_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)(Keys.Enter))
            {
                e.Handled = true;

                agregarCorte();
            }
        }

        private void formModificarPrecios_Load(object sender, EventArgs e)
        {
            this.Text += Utilidades.Conexion.getSucursalConexion();
        }

        private void formModificarPrecios_Load_1(object sender, EventArgs e)
        {

        }

        private void checkBoxPorcPrecio_CheckedChanged(object sender, EventArgs e)
        {
            precioPorPorc = checkBoxPorcPrecio.Checked;
            boxModificarPrecio.Enabled = !precioPorPorc;
            txtPorcentaje.Enabled = precioPorPorc;

            if (precioPorPorc)
                txtPorcentaje.Focus();  
        }

        private void txtPorcentaje_TextChanged(object sender, EventArgs e)
        {
            //Utilidades.Util_Form.validarCampoNumerico(txtPorcentaje.Text, "txtPorcentaje");
        }
    }
}
