using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Presentacion.Reportes;
using Presentacion.Cortes;

namespace Presentacion
{
    public partial class formCortes : formBaseColor
    {
        Negocio.Corte oCorteN;
        Entidades.Corte oCorteE;
        Entidades.Corte oCorteMaestroE;

        DataTable dtCortes;
       
        public formCortes()
        {
            InitializeComponent();
            cargarGrilla();
        }

        #region eventos
        private void nuevo_Click(object sender, EventArgs e)
        {
            formNuevoCorte frmNuevoCorte = new formNuevoCorte();
            frmNuevoCorte.obtenerFormCorte(this);
            frmNuevoCorte.ShowDialog();
        }

        private void modificar_Click(object sender, EventArgs e)
        {
            modificarCorte();
        }
    
        private void stock_Click(object sender, EventArgs e)
        {
            formIngresoEmbutido frmIngresoEmbutido = new formIngresoEmbutido();
            frmIngresoEmbutido.ShowDialog();
        }
        
        private void grillaCortes_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            infoCorte();
        }
        private void btnBuscar_Click(object sender, EventArgs e)
        {
            buscarCorte();
        }
        private void txtBuscarCorte_TextChanged(object sender, EventArgs e)
        {
            buscarCorte();
        }
        #endregion

        #region metodos

        public void cargarGrilla()
        {
            oCorteN = new Negocio.Corte();

            string txtBusqueda = this.txtBuscarCorte.Text.Trim();

            grillaCortes.AutoGenerateColumns = false;

            dtCortes = oCorteN.buscarCorte(txtBusqueda);
            grillaCortes.DataSource = dtCortes;
        }
        
        public void buscarCorte()
        {
            oCorteN = new Negocio.Corte();

            string txtBusqueda = this.txtBuscarCorte.Text.Trim();

            grillaCortes.AutoGenerateColumns = false;

            dtCortes = oCorteN.buscarCorte(txtBusqueda);
            grillaCortes.DataSource = dtCortes;            
        }

        private void modificarCorte()
        {
            try
            {
                formNuevoCorte frmNuevoCorte = new formNuevoCorte();
                cargarCorte();
                frmNuevoCorte.obtenerCorteFormCortes(oCorteE, this);
                frmNuevoCorte.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void modificarPrecios_Click(object sender, EventArgs e)
        {
            try
            {
                if (Presentacion.FormPrincipal.logueado == false)
                {
                    MessageBox.Show("No está logueado!.\nInicie sesión y vuelva a intentar.");
                }
                else
                {
                    formModificarPrecios frmModificarPrecios = new formModificarPrecios();

                    foreach (DataGridViewRow filaCorte in grillaCortes.Rows)
                    {
                        if (!frmModificarPrecios.finalizarMod)
                        {
                            cargarCorte(filaCorte.Index);
                            frmModificarPrecios.obtenerCorteFormCortes(oCorteE, this);
                            frmModificarPrecios.ShowDialog();
                        }
                        else
                        {
                            break;
                        }
                    }
                }      
            }
            catch (Exception ex)
            {                
                MessageBox.Show(ex.Message);
            }
            
        }

        private void infoCorte()
        {
            cargarCorte();            

            formInfoCorte frmInfoCorte=new formInfoCorte();
            frmInfoCorte.obtenerParametros(oCorteE, this);
            frmInfoCorte.ShowDialog();

        }

        private void cargarCorte()
        {
            oCorteE = new Entidades.Corte();
            oCorteMaestroE=new Entidades.Corte();

            oCorteE.idCorte =Convert.ToInt32(grillaCortes.CurrentRow.Cells["idCorte"].Value.ToString());
            oCorteE.codigo =Convert.ToInt32(grillaCortes.CurrentRow.Cells["codigo"].Value.ToString());
            oCorteE.corte = grillaCortes.CurrentRow.Cells["corte"].Value.ToString();
            oCorteE.precioKg = float.Parse(grillaCortes.CurrentRow.Cells["precioKg"].Value.ToString());
            oCorteE.independiente = Convert.ToInt32(grillaCortes.CurrentRow.Cells["independiente"].Value.ToString());
            oCorteE.tipo = grillaCortes.CurrentRow.Cells["tipo"].Value.ToString();
            oCorteE.corteMaestro=oCorteMaestroE;
            oCorteE.corteMaestro.idCorte=Convert.ToInt32(grillaCortes.CurrentRow.Cells["idCorteMaestro"].Value.ToString());
            oCorteE.corteMaestro.corte = grillaCortes.CurrentRow.Cells["corteMaestro"].Value.ToString();
            oCorteE.porcentaje = float.Parse(grillaCortes.CurrentRow.Cells["porcentaje"].Value.ToString());
            oCorteE.desvioEstandar = float.Parse(grillaCortes.CurrentRow.Cells["desvioEstandar"].Value.ToString());
            oCorteE.porcentajeHueso = float.Parse(grillaCortes.CurrentRow.Cells["porcentajeHueso"].Value.ToString());
        }

        private void cargarCorte(int fila)
        {
            oCorteE = new Entidades.Corte();
            oCorteMaestroE = new Entidades.Corte();

            oCorteE.idCorte = Convert.ToInt32(grillaCortes.Rows[fila].Cells["idCorte"].Value.ToString());
            oCorteE.codigo = Convert.ToInt32(grillaCortes.Rows[fila].Cells["codigo"].Value.ToString());
            oCorteE.corte = grillaCortes.Rows[fila].Cells["corte"].Value.ToString();
            oCorteE.precioKg = float.Parse(grillaCortes.Rows[fila].Cells["precioKg"].Value.ToString());
            oCorteE.independiente = Convert.ToInt32(grillaCortes.Rows[fila].Cells["independiente"].Value.ToString());
            oCorteE.tipo = grillaCortes.Rows[fila].Cells["tipo"].Value.ToString();
            oCorteE.corteMaestro = oCorteMaestroE;
            oCorteE.corteMaestro.idCorte = Convert.ToInt32(grillaCortes.Rows[fila].Cells["idCorteMaestro"].Value.ToString());
            oCorteE.corteMaestro.corte = grillaCortes.Rows[fila].Cells["corteMaestro"].Value.ToString();
            oCorteE.porcentaje = float.Parse(grillaCortes.Rows[fila].Cells["porcentaje"].Value.ToString());
            oCorteE.desvioEstandar = float.Parse(grillaCortes.Rows[fila].Cells["desvioEstandar"].Value.ToString());
            oCorteE.porcentajeHueso = float.Parse(grillaCortes.Rows[fila].Cells["porcentajeHueso"].Value.ToString());
        }        

        #endregion

        private void btnSeleccionar_Click(object sender, EventArgs e)
        {
            infoCorte();
        }

        private void btnCancelar_Click(object sender, EventArgs e)
        {
            this.Close();            
        }

        private void imprimirReporte()
        {
            Reportes.ReportesDataSet.dtStockCortesDataTable dtStockCortes = new ReportesDataSet.dtStockCortesDataTable();
            string titulo = "Reporte Stock Cortes";

            foreach (DataRow fila in dtCortes.Rows)
            {
                DataRow dsFila = dtStockCortes.NewRow();

                dsFila["Codigo"] = fila["codigo"];
                dsFila["Corte"] = fila["corte"];
                dsFila["StockSanLorenzo"] = fila["stockSL"];
                dsFila["StockSanMartin"] = fila["stockSM"];

                dtStockCortes.Rows.Add(dsFila);
            }

            Reportes.ReporteStockCortes reporte = new Reportes.ReporteStockCortes();
            FormReportes frmReportes = new FormReportes(reporte, titulo, dtStockCortes , DateTime.Now , DateTime.Now);

            frmReportes.Show();            
        }

        private void formCortes_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode== Keys.M)
            {
                modificarCorte();
            }

            if (e.Control && e.KeyCode==Keys.B)
            {
                txtBuscarCorte.Focus();                
            }
        }

        private void Imprimir_Click(object sender, EventArgs e)
        {
            imprimirReporte();
        }      
    }
}
