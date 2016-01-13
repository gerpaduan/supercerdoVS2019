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
    public partial class formInfoCorte : formBaseColor
    {
        Negocio.Corte oCorteN = new Negocio.Corte();
        Entidades.Corte oCorteE = new Entidades.Corte();
        formCortes oFrmCortes=new formCortes();
        DataTable dtCorte;

        int idCorte;

        public formInfoCorte()
        {
            InitializeComponent();
           
        }

        private void modificar_Click(object sender, EventArgs e)
        {
            formNuevoCorte frmNuevoCorte = new formNuevoCorte();
            frmNuevoCorte.obtenerCorteFormInfoCorte(oCorteE, this);
            frmNuevoCorte.ShowDialog();
        }

        private void stock_Click(object sender, EventArgs e)
        {
            formIngresoEmbutido frmStockCorte = new formIngresoEmbutido();
            frmStockCorte.ShowDialog();
        }


        public void obtenerParametros(Entidades.Corte corteParam, formCortes frmCortesParam)
        {
            oFrmCortes = frmCortesParam;
            oCorteE = corteParam;
            idCorte = oCorteE.idCorte;
            cargarCorte();
        }

        private void cargarCorte()
        { 
            dtCorte=new DataTable();
            dtCorte = oCorteN.obtenerInfoCorte(idCorte);

            cargarCampos();
        }

        private void cargarCampos()
        {
            try
            {
                foreach (DataRow fila in dtCorte.Rows)
                {
                    txtIdCorte.Text = fila["idCorte"].ToString();
                    txtCodigo.Text = fila["codigo"].ToString();
                    txtDescCorte.Text = fila["corte"].ToString();
                    txtPrecioKg.Text = fila["precioKg"].ToString();
                    txtTipo.Text = fila["tipo"].ToString();
                    if (oCorteE.independiente == 1)
                    {
                        txtIndependiente.Checked = true;
                    }

                    txtCorteMaestro.Text = fila["corteMaestro"].ToString();
                    txtPorcentajeCorte.Text = fila["porcentaje"].ToString();
                    txtPorcHueso.Text = fila["porcentajeHueso"].ToString();
                    txtDesvioEstandar.Text = fila["desvioEstandar"].ToString();
                    txtStockSanLorenzo.Text = fila["stockSL"].ToString();
                    txtStockSanMartin.Text = fila["stockSM"].ToString();

                    float stockSL, stockSM, total;
                    stockSL = float.Parse(fila["stockSL"].ToString());
                    stockSM = float.Parse(fila["stockSM"].ToString());
                    total = stockSL + stockSM;

                    txtTotalStock.Text = Convert.ToString(total);
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

           
        }

        public void recibirCorteModificado(Entidades.Corte oCorteMod)
        {
            oCorteE = oCorteMod;

            cargarCamposCorteMod();

            oFrmCortes.cargarGrilla();
        }

        private void cargarCamposCorteMod()
        {
            txtCodigo.Text =Convert.ToString( oCorteE.codigo);
            txtDescCorte.Text = oCorteE.corte;
            txtTipo.Text = oCorteE.tipo;
            txtCorteMaestro.Text = oCorteE.corteMaestro.corte;
            txtPorcentajeCorte.Text =Convert.ToString( oCorteE.porcentaje);
            txtDesvioEstandar.Text = oCorteE.desvioEstandar.ToString();
        }

        private void eliminarCorte()
        {
            DialogResult resp = MessageBox.Show("Está seguro que desea eliminar el Corte?.", "Eliminar Corte", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
            if (resp == DialogResult.Yes)
            {
                oCorteN.eliminarCorte(oCorteE);

                oFrmCortes.cargarGrilla();
                this.Close();
            }
        }

        private void btnSalir_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void eliminar_Click(object sender, EventArgs e)
        {
            if (Presentacion.FormPrincipal.logueado == false)
            {
                MessageBox.Show("No está logueado!.\nInicie sesión y vuelva a intentar.");
            }
            else
            {
                eliminarCorte();
            }
        }

    }
}
