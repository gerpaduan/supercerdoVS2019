using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Presentacion
{
    public partial class formModificarCorte : Form
    {
        Entidades.Corte oCorteE;
        Negocio.Corte oCorteN;
        formInfoCorte oFrmInfoCorte;
        formCortes oFrmCortes;

        public formModificarCorte()
        {
            InitializeComponent();
        }


        public void obtenerCorteFormInfoCorte(Entidades.Corte corteParam, formInfoCorte frmInfoCorteParam)
        {
            oFrmInfoCorte = frmInfoCorteParam;
            oCorteE = corteParam;
            cargarCampos();
        }

        public void obtenerCorteFormCortes(Entidades.Corte corteParam, formCortes frmCortesParam)
        {
            oFrmCortes = frmCortesParam;
            oCorteE = corteParam;
            cargarCampos();
        }

        private void cargarCampos()
        {
            txtCodigo.Text =Convert.ToString( oCorteE.codigo);
            txtDescCorte.Text = oCorteE.corte;
            txtPrecioKg.Text = Convert.ToString(oCorteE.precioKg);
            txtTipo.Text = oCorteE.tipo;
            txtCorteMaestro.Text = oCorteE.corteMaestro.corte;
            txtPorcentajeCorteM.Text =Convert.ToString(oCorteE.porcentaje);
            
        }

        private void cargarCorte()
        {
            oCorteE.codigo = Convert.ToInt32(txtCodigo.Text.Trim());

            try
            {
                oCorteE.PrecioKg = float.Parse(txtPrecioKg.Text.Trim(), System.Globalization.NumberStyles.Float, new System.Globalization.CultureInfo("en-US"));
            }
            catch (Exception)
            {

                oCorteE.PrecioKg = float.Parse(txtPrecioKg.Text.Trim());

            }

            try
            {
                oCorteE.porcentaje = float.Parse(txtPorcentajeCorteM.Text.Trim(), System.Globalization.NumberStyles.Float, new System.Globalization.CultureInfo("en-US"));
            }
            catch (Exception)
            {

                oCorteE.porcentaje = float.Parse(txtPorcentajeCorteM.Text.Trim());
            }
        }

        private void modificarCorte()
        {
            if (validar())
            {
                cargarCorte();
                
                if (oCorteE.porcentaje <= 100)
                {
                    oCorteN = new Negocio.Corte();
                
                    oCorteN.modificarCorte(oCorteE);
                    if (oFrmCortes != null)
                    {
                        oFrmCortes.cargarGrilla();
                    }
                    else
                    {
                        oFrmInfoCorte.recibirCorteModificado(oCorteE);
                    }

                    oFrmInfoCorte.recibirCorteModificado(oCorteE);
                    this.Close();
                }
                else
                {
                    MessageBox.Show("El porcentaje del Corte Maestro debe ser igual o menor al 100%.", "Porcentaje Corte Maestro incorrecto",
                   MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private bool validar()
        {

            if (this.txtCodigo.Text.Equals("") || this.txtPorcentajeCorteM.Text.Equals(""))
            {
                MessageBox.Show("Debe Completar todos los campos.", "Complete los campos",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return false;
            }

            else
            {
                return true;
            }
        }

        private void btnGuardar_Click(object sender, EventArgs e)
        {
            modificarCorte();
        }

        private void btnCancelar_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void formModificarCorte_Load(object sender, EventArgs e)
        {

        }
    }
}
