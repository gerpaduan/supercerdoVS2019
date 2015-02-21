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
        formCortes frmCorte;
        formInfoCorte oFrmInfoCorte;

        string mensaje = "";

        bool modificar = true;

        public bool finalizarMod = false;

        public formModificarPrecios()
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
            if (cargarDatosCorte(oCorteE))	
            {   
                oCorteN.modificarCorte(oCorteE);
                if (frmCorte != null)
                {
                    frmCorte.cargarGrilla();
                } 
                this.Close();               
            }
            else
            {
                MessageBox.Show("Los siguiente campos tienen ingresado datos erroneos:\n" + mensaje);
            }            
        }

        private bool cargarDatosCorte(Entidades.Corte oCorteE)
        {
            bool resp = true;
            mensaje = "";

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

        }
    }
}
