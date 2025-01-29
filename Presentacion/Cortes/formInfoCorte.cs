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
            InitializeComponent(); this.Icon = Properties.Resources.CarniSys_ICONO;           
        }

        private void modificar_Click(object sender, EventArgs e)
        {
            formNuevoCorte frmNuevoCorte = new formNuevoCorte();
            frmNuevoCorte.idCorte = oCorteE.idCorte;
            frmNuevoCorte.oFrmInfoCorte = this;
            frmNuevoCorte.ShowDialog();
            this.Close();
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
                txtIdCorte.Text = oCorteE.idCorte.ToString();
                txtCodigo.Text = Convert.ToString(oCorteE.codigo);
                txtDescCorte.Text = oCorteE.corte;
                txtPrecioKg.Text = oCorteE.precioKg.ToString("F2"); 
                txtTipo.Text = oCorteE.tipo;
                txtMarca.Text = oCorteE.Marca != null ? oCorteE.Marca.Identificacion : "";
                checkPesable.Checked = oCorteE.Pesable;
                txtAlicuotaIva.Text = oCorteE.AlicuotaIva.ToString();
                txtPromedio.Text = oCorteE.Promedio.ToString("F2");
                txtPuntoStock.Text = oCorteE.PuntoStock.ToString();
                txtNivel.Text = oCorteE.Nivel.ToString();
                checkIngresoRapidoEmbutido.Checked = oCorteE.IngresoRapidoEmbutido;
                checkEnCierreStock.Checked = oCorteE.EnCierreStock;
                txtIndependiente.Checked = oCorteE.independiente.Equals(1);
                checkHabilitado.Checked = oCorteE.Habilitado;
                txtCorteMaestro.Text = (oCorteE.corteMaestro != null && oCorteE.corteMaestro.corte != null) ? oCorteE.corteMaestro.corte : "-";
                txtPorcentajeCorte.Text = Convert.ToString(oCorteE.porcentaje);
                txtDesvioEstandar.Text = oCorteE.desvioEstandar.ToString();
                txtPorcHueso.Text = oCorteE.porcentajeHueso.ToString();

                grillaProveedores.DataSource = oCorteN.obtenerCorteProveedor(idCorte);
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
            txtAlicuotaIva.Text = oCorteE.AlicuotaIva.ToString();
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
                //cargarCorte();
                cargarCampos();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void checkIngresoRapidoEmbutido_Paint(object sender, PaintEventArgs e)
        {

            CheckBox cb = sender as CheckBox;
            TextRenderer.DrawText(e.Graphics, cb.Text, cb.Font, cb.ClientRectangle,
                                  cb.Enabled ? cb.ForeColor : cb.ForeColor);
        }
    }
}
