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
        public int idCorte = 0;
        Negocio.Corte oCorteN = new Negocio.Corte();
        Entidades.Corte oCorteE = new Entidades.Corte();
        public formCortes oFrmCortes;
        DataTable dtCorte;

        public formInfoCorte()
        {
            InitializeComponent();           
        }

        private void modificar_Click(object sender, EventArgs e)
        {
            formNuevoCorte frmNuevoCorte = new formNuevoCorte();
            frmNuevoCorte.idCorte = oCorteE.idCorte;
            frmNuevoCorte.oFrmInfoCorte = this;
            frmNuevoCorte.ShowDialog();
        }

        private void stock_Click(object sender, EventArgs e)
        {
            formIngresoEmbutido frmStockCorte = new formIngresoEmbutido();
            frmStockCorte.ShowDialog();
        }
        
        private void cargarCorte()
        { 
            dtCorte=new DataTable();
            dtCorte = oCorteN.obtenerInfoCorte(oCorteE.idCorte);

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
                    txtIndependiente.Checked = Convert.ToBoolean(fila["independiente"]);
                    checkMayorista.Checked = Convert.ToBoolean(fila["mayorista"]);
                    checkHabilitado.Checked = Convert.ToBoolean(fila["habilitado"]);
                    checkEnCierreStock.Checked = Convert.ToBoolean(fila["enCierreStock"]);

                    txtCorteMaestro.Text = fila["corteMaestro"].ToString();
                    txtPorcentajeCorte.Text = fila["porcentaje"].ToString();
                    txtPorcHueso.Text = fila["porcentajeHueso"].ToString();
                    txtDesvioEstandar.Text = fila["desvioEstandar"].ToString();
                    txtPromedio.Text = fila["promedio"].ToString();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public void recibirCorteModificado(Entidades.Corte oCorteMod)
        {
            formInfoCorte_Load(null, null);
        }

        private void cargarCamposCorteMod()
        {
            txtCodigo.Text =Convert.ToString( oCorteE.codigo);
            txtDescCorte.Text = oCorteE.corte;
            txtTipo.Text = oCorteE.tipo;
            txtIndependiente.Checked = oCorteE.independiente.Equals(1);
            checkMayorista.Checked = oCorteE.Mayorista.Equals(1);
            //checkEnCierreStock.Checked = oCorteE.
            txtCorteMaestro.Text = (oCorteE.corteMaestro != null && oCorteE.corteMaestro.corte != null) ? oCorteE.corteMaestro.corte : "-";
            txtPorcentajeCorte.Text =Convert.ToString( oCorteE.porcentaje);
            txtDesvioEstandar.Text = oCorteE.desvioEstandar.ToString();
            txtPorcHueso.Text = oCorteE.porcentajeHueso.ToString();
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
            if (!Usuarios.FormValidarPermiso.validarPermiso())
                return;

            eliminarCorte();
        }

        private void formInfoCorte_Load(object sender, EventArgs e)
        {
            try
            {
                this.Text += Utilidades.Conexion.getSucursalConexion();

                oCorteE = oCorteN.getCorteById(idCorte, true);
                cargarCorte();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
