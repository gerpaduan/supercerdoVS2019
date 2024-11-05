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
    public partial class formCortes : formBaseColor
    {
        Negocio.Corte oCorteN;
        Entidades.Corte oCorteE;
        Entidades.Corte oCorteMaestroE;

        DataTable dtCortes;
        DataTable dtCortesFiltrado;

        bool comboCargado = false;
        long codigoDesde, codigoHasta;
        public formCortes()
        {
            InitializeComponent();
        }

        #region eventos
        private void nuevo_Click(object sender, EventArgs e)
        {
            nuevoCorte();
        }

        private void nuevoCorte()
        {
            //formNuevoCorte frmNuevoCorte = new formNuevoCorte();
            //frmNuevoCorte.frmCorte = this;
            //frmNuevoCorte.ShowDialog(this);

            if (Application.OpenForms["formNuevoCorte"] != null)
            {

                Application.OpenForms["formNuevoCorte"].Activate();
                Application.OpenForms["formNuevoCorte"].WindowState = FormWindowState.Normal;
            }
            else
            {
                //Presentacion.Caja.FormLoginVendedor frmLogin = new Presentacion.Caja.FormLoginVendedor();
                //frmLogin.soloActivos = true;
                //frmLogin.ShowDialog(this);

                //if (oUsuario == null)
                //    return;

                formNuevoCorte frmNuevoCorte = new formNuevoCorte();
                frmNuevoCorte.frmCorte = this;
                frmNuevoCorte.Show(this);
            }
        }

        private void modificar_Click(object sender, EventArgs e)
        {
            modificarCorte();

            /////Actualizando Nivel 
            /////
            //for (int i = 0; i < grillaCortes.Rows.Count; i++)
            //{
            //    cargarCorte(i);
            //    oCorteN.addOrEditCorte(oCorteE);
            //}
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
            //buscarCorte();
            //lblActualizar.Visible = true;
        }
        #endregion

        #region metodos

        public void cargarGrilla()
        {
            if (!comboCargado)
                return;

            lblActualizar.Visible = false;

            string txtBusqueda = this.txtBuscarCorte.Text.Trim();

            grillaCortes.AutoGenerateColumns = false;

            dtCortes = oCorteN.buscarCorte(txtBusqueda);
            grillaCortes.DataSource = dtCortes;
            filtarGrilla();
        }
        
        public void buscarCorte()
        {
            lblActualizar.Visible = false;
            oCorteN = new Negocio.Corte();

            string txtBusqueda = this.txtBuscarCorte.Text.Trim();

            grillaCortes.AutoGenerateColumns = false;

            dtCortes = oCorteN.buscarCorte(txtBusqueda);
            grillaCortes.DataSource = dtCortes;
            filtarGrilla();
        }

        private void modificarCorte()
        {
            try
            {
                int idCorte = Convert.ToInt32(grillaCortes.CurrentRow.Cells["idCorte"].Value.ToString());
                formNuevoCorte frmNuevoCorte = new formNuevoCorte();
                frmNuevoCorte.idCorte = idCorte;
                frmNuevoCorte.frmCorte = this;
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
                if (!Usuarios.FormValidarPermiso.validarPermiso())
                {
                    this.Close();
                }
                else
                {
                    List<Entidades.Corte> listCortes = new List<Entidades.Corte>();

                    foreach (DataGridViewRow filaCorte in grillaCortes.Rows)
                    {
                        cargarCorte(filaCorte.Index);
                        listCortes.Add(oCorteE);
                    }

                    formModificarPrecios frmModificarPrecios = new formModificarPrecios();

                    foreach (Entidades.Corte filaCorte in listCortes)
                    {
                        if (!frmModificarPrecios.finalizarMod)
                        {
                            //CargarCorte(filaCorte.Index);
                            frmModificarPrecios.obtenerCorteFormCortes(filaCorte, listCortes, this);
                            frmModificarPrecios.ShowDialog();

                            //si se modificó por porcentaje que cierra una vez q finalizó la modificacion en lotes
                            if (frmModificarPrecios.precioPorPorc)
                                return;
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
            try
            {
                int idCorte = Convert.ToInt32(grillaCortes.CurrentRow.Cells["idCorte"].Value.ToString());
                formInfoCorte frmInfoCorte = new formInfoCorte();
                frmInfoCorte.idCorte = idCorte;
                frmInfoCorte.oFrmCortes = this;
                frmInfoCorte.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void cargarCorte(int fila)
        {
            oCorteE = new Entidades.Corte();
            oCorteMaestroE = new Entidades.Corte();

            oCorteE.idCorte = Convert.ToInt32(grillaCortes.Rows[fila].Cells["idCorte"].Value.ToString());
            oCorteE = oCorteN.getCorteById(oCorteE.idCorte, true);
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
            Ticket.formTipoTicket tipoTicket = new Presentacion.Ticket.formTipoTicket();
            tipoTicket.cortesConPrecios(dtCortesFiltrado);
            //imprimirReporte();
        }

        private void formCortes_Load(object sender, EventArgs e)
        {
            oCorteN = new Negocio.Corte();
            this.Text += Utilidades.Conexion.getSucursalConexion();
            comboTipo.DataSource = oCorteN.obtenerTiposProducto(true);
            comboTipo.DisplayMember = "tipo";
            comboTipo.ValueMember = "tipo";
            comboTipo.SelectedIndex = 0;
            comboCargado = true;
            cargarGrilla();
            this.txtBuscarCorte.Select();
        }

        private void txtCodigoDesde_TextChanged(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(txtCodigoDesde.Text) && Utilidades.Util_Form.validarCampoNumeroEntero(txtCodigoDesde.Text, "Desde"))
            {
                codigoDesde = Convert.ToInt64(txtCodigoDesde.Text);
            }
            filtarGrilla();
        }

        private void txtCodigohasta_TextChanged(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(txtCodigohasta.Text) && Utilidades.Util_Form.validarCampoNumeroEntero(txtCodigohasta.Text, "Hasta"))
            {
                codigoHasta = Convert.ToInt64(txtCodigohasta.Text);
            }
            filtarGrilla();
        }

        public void filtarGrilla()
        {
            if (!comboCargado)
                return;

            dtCortesFiltrado = dtCortes.Clone();
            // Presuming the DataTable has a column named Date.
            string expresion = !string.IsNullOrEmpty(txtCodigoDesde.Text) ? "codigo >= " + codigoDesde : "true";
            expresion+= " and ";
            expresion += !string.IsNullOrEmpty(txtCodigohasta.Text) ? "codigo <= " + codigoHasta :  "true";
            if (!string.IsNullOrEmpty(comboTipo.Text) && !comboTipo.Text.Equals("Todos"))
            {
                expresion += " and ";
                expresion += !string.IsNullOrEmpty(comboTipo.Text) ? "tipo = \'" + comboTipo.Text + "\'" : "true";
            }
            if (!string.IsNullOrEmpty(txtBuscarCorte.Text))
            {
                string buscaPorCodigo = (long.TryParse(txtBuscarCorte.Text, out long numero)) ? "codigo = " + numero : "true";

                expresion += " and ";
                expresion += " ( corte like \'" + txtBuscarCorte.Text + "%\' or " + buscaPorCodigo +" ) ";
            }
            if (!string.IsNullOrEmpty(txtBuscarMaestro.Text))
            {
                expresion += " and ";
                expresion += " corteMaestro like \'%" + txtBuscarMaestro.Text + "%\'";
            }

            DataRow[] foundRows;
            // Use the Select method to find all rows matching the filter.
            foundRows = dtCortes.Select(expresion);//, "codigo");

            foreach (DataRow row in foundRows)
            {
                dtCortesFiltrado.ImportRow(row);
            }
            grillaCortes.DataSource = dtCortesFiltrado;
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Escape)
            {
                this.Close();
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void tipos_Click(object sender, EventArgs e)
        {
            if (Application.OpenForms["formTiposProducto"] != null)
            {

                Application.OpenForms["formTiposProducto"].Activate();
                Application.OpenForms["formTiposProducto"].WindowState = FormWindowState.Normal;

            }
            else
            {
                formTiposProducto frmTiposProducto = new formTiposProducto();
                frmTiposProducto.Show();
            }
        }

        private void btnCostoPorCobro_Click(object sender, EventArgs e)
        {
            formAddOrEditCostoCobro frmCostoPorCobre = new formAddOrEditCostoCobro();
            frmCostoPorCobre.ShowDialog();
        }

        private void txtBuscarCorte_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyValue.Equals(13))
            {
                cargarGrilla();
            }
        }   
    }
}
