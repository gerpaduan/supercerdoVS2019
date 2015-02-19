using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Presentacion.Cortes
{
    public partial class formStockCortes : Form, InterfaceCorte
    {
        Negocio.Corte oCorteN = new Negocio.Corte();
        Entidades.Corte oCorteE;
        

        DataTable dtSucursales;
        DataTable dtCortes;

        Negocio.Sucursal oSucursalN = new Negocio.Sucursal();
        Entidades.Sucursal oSucursalE ;

        Entidades.StockCorteSucursal oStockCorteSucursal;
        List<Entidades.StockCorteSucursal> listaStockCorteSucursal ;

        public formStockCortes()
        {
            InitializeComponent();
            cargarSucursal();
            cargarGrilla();
        }

        private void cargarCorte()
        {
            if (txtCodigo.Text.Trim() != "")
            {
                try
                {
                    oStockCorteSucursal = null;
                    oStockCorteSucursal = new Entidades.StockCorteSucursal();

                    oCorteE = null;
                    oCorteE = new Entidades.Corte();
                    
                    DataTable dtCorte = new DataTable();
                    dtCorte = oCorteN.buscarCodigoCorte(Convert.ToInt32(txtCodigo.Text.Trim()));
                    
                    if (dtCorte.Rows.Count > 0)
                    {
                        foreach (DataRow fila in dtCorte.Rows)
                        {
                            if (Convert.ToInt32(fila["idSucursal"].ToString()) == oSucursalE.idSucursal)
                            {
                                //cargo el corte
                                oCorteE.idCorte = Convert.ToInt32(fila["idCorte"].ToString());
                                oCorteE.codigo = Convert.ToInt32(fila["codigo"].ToString());
                                oCorteE.corte = fila["corte"].ToString();
                                
                                //cargo stock
                                oStockCorteSucursal.Corte = oCorteE;

                                Entidades.Sucursal oSucursalStock = oSucursalE;
                                oStockCorteSucursal.Sucursal = oSucursalStock;
                                

                                oStockCorteSucursal.Stock = float.Parse(fila["stock"].ToString());
                                oStockCorteSucursal.StockTeorico = float.Parse(fila["stockTeorico"].ToString()); 
                                
                            }

                        }

                        //cargo los campos
                        this.txtCodigo.Text = Convert.ToString(oCorteE.codigo);
                        this.txtCorte.Text = oCorteE.corte;
                        this.txtStock.Text = Convert.ToString(oStockCorteSucursal.Stock);
                        this.txtStockTeorico.Text = Convert.ToString(oStockCorteSucursal.StockTeorico);

                        dtCorte = null;
                        
                    }

                    else
                    {

                        oCorteE = null;
                        this.txtStockActual.Text = "";
                        this.txtCorte.Text = "";
                        this.txtStock.Text = "";
                        txtStockActual.Text = "";
                        this.txtStockTeorico.Text = "";
                        this.txtStockTeoricoActual.Text = "";
                    }
                }
                catch (Exception ex)
                {

                    MessageBox.Show(ex.Message);
                    limpiarCamposCorte();
                }

            }
            else
            {
                limpiarCamposCorte();
            }
        }

        private void limpiarCamposCorte()
        {

            txtCodigo.Text = "";
            txtCorte.Text = "";
            txtStock.Text = "";
            txtStockActual.Text = "";
            this.txtStockTeorico.Text = "";
            this.txtStockTeoricoActual.Text = "";
           
            txtCodigo.Focus();
        }

        private bool validarLinea()
        {
            string mensaje = "Complete los siguientes campos: ";
            if (txtCodigo.Text.Trim() == "" || comboSucursal.SelectedValue == null)
            {
                if (txtCodigo.Text.Trim() == "")
                {
                    mensaje += "\n" + "-Código Corte";

                    MessageBox.Show(mensaje, "Completar campos", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    txtCodigo.Focus();
                }

                else
                {
                    if (oCorteE == null)
                    {
                        MessageBox.Show("El código ingresado no pertenece a ningún corte.", "El Corte no existe", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        txtCodigo.Focus();
                    }
                    else
                    {
                        //if (txtStockActual.Text.Trim() == "")
                        //{
                        //    mensaje += "\n" + "-Stock Actual";

                        //}
                        
                        if (comboSucursal.SelectedValue == null)
                        {
                            mensaje += "\n" + "-Sucursal";
                        }

                        MessageBox.Show(mensaje, "Completar campos", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        txtStockActual.Focus();
                    }

                }


                return false;
            }



            else
            {
                return true;
            }
        }

     
        public void EnviarCorte(Entidades.Corte corte)
        {
            oCorteE = null;

            oCorteE = corte;

            this.txtCodigo.Text = Convert.ToString(oCorteE.codigo);
            this.txtCorte.Text = oCorteE.corte;
        }


        public void cargarGrilla()
        {
            grillaCortes.DataSource = null;
            grillaCortes.AutoGenerateColumns = false;
            dtCortes=oCorteN.buscarCorte("");
            
            cargarStockActualEnGrilla();
            grillaCortes.DataSource = dtCortes;
        }

        public void cargarGrillaActualizada()
        {
            grillaCortes.DataSource = null;
            grillaCortes.AutoGenerateColumns = false;
            dtCortes = oCorteN.buscarCorte("");

            //cargarStockActualEnGrilla();
            grillaCortes.DataSource = dtCortes;
        }
        

        private void cargarSucursal()
        {
            dtSucursales = new DataTable();
            oSucursalN = new Negocio.Sucursal();
            dtSucursales = oSucursalN.obtenerSucursales();
            comboSucursal.DataSource = dtSucursales;
            comboSucursal.DisplayMember = "sucursal";
            comboSucursal.ValueMember = "idSucursal";
           // comboSucursal.SelectedItem = null;
        }

        private void TxtPruebaENTER_KeyPress(object sender, KeyPressEventArgs e)
        {

            if (e.KeyChar == (char)(Keys.Enter))
            {

                e.Handled = true;

                SendKeys.Send("{TAB}");

            }

        }


        private void ingresarStockActual()
        {
            try
            {
                if (validarLinea())
                {
                    if (txtStockActual.Text.Trim() != "")
                    {
                        try
                        {

                            try
                            {
                                oStockCorteSucursal.Stock = float.Parse(txtStockActual.Text.Trim(), System.Globalization.NumberStyles.Float, new System.Globalization.CultureInfo("en-US"));
                            }
                            catch (Exception)
                            {

                                oStockCorteSucursal.Stock = float.Parse(txtStockActual.Text.Trim());
                            }


                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message);
                        }

                    }
                    

                    if (txtStockTeoricoActual.Text.Trim() != "")
                    {
                        try
                        {
                            try
                            {
                                oStockCorteSucursal.StockTeorico = float.Parse(txtStockTeoricoActual.Text.Trim(), System.Globalization.NumberStyles.Float, new System.Globalization.CultureInfo("en-US"));
                            }
                            catch (Exception)
                            {

                                oStockCorteSucursal.StockTeorico = float.Parse(txtStockTeoricoActual.Text.Trim());
                            }

                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message);
                        }



                    }
                    else
                    {
                        oStockCorteSucursal.StockTeorico = 0;
                    }


                    listaStockCorteSucursal.Add(oStockCorteSucursal);

                    cargarGrillaActualizada();

                    limpiarCamposCorte();
                }
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }

        
        }

        private void cargarStockActualEnGrilla()
        {
            if (listaStockCorteSucursal!=null)
            {
                foreach (DataRow fila in dtCortes.Rows)
                {
                    foreach (Entidades.StockCorteSucursal stockCorte in listaStockCorteSucursal)
                    {
                        if (Convert.ToInt32(fila["idCorte"].ToString()) == stockCorte.Corte.idCorte)
                        {
                            if (Convert.ToInt32(fila["idSucursalSL"].ToString()) == stockCorte.Sucursal.idSucursal)
                            {

                                fila["stockSL"] = stockCorte.Stock;
                                fila["stockTeoricoSL"] = stockCorte.StockTeorico;

                            }
                            if (Convert.ToInt32(fila["idSucursalSM"].ToString()) == stockCorte.Sucursal.idSucursal)
                            {
                                fila["stockSM"] = stockCorte.Stock;
                                fila["stockTeoricoSM"] = stockCorte.StockTeorico;
                            }
                        }
                    }
                }
            }
            else
            {
                listaStockCorteSucursal = new List<Entidades.StockCorteSucursal>();
            }

        }


        private void agregarActualizacionStock()
        {
            DialogResult resp= MessageBox.Show("Está seguro que desea actualizar el Stock.", "Reiniciar Stock", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);

            if (resp==DialogResult.Yes && listaStockCorteSucursal!=null)
            {

                int idActualizacion = oCorteN.agregarActualizacionStock(txtFechaVenta.Value, txtObservaciones.Text.Trim());

                foreach (Entidades.StockCorteSucursal stockCorte in listaStockCorteSucursal)
                {
                    oCorteN.actualizarStockPorCorte(idActualizacion, stockCorte);                
                }

                oCorteN.actualizacionStockTotal(idActualizacion);
                oCorteN.actualizacionStockTeoricoTotal(idActualizacion);
                listaStockCorteSucursal = null;

                cargarGrillaActualizada();
            }
            
        }


        private void cambiarSucursal()
        {
            if (comboSucursal.SelectedIndex > -1)
            {
                oSucursalE = new Entidades.Sucursal();
                oSucursalE.idSucursal = comboSucursal.SelectedIndex + 1;

                cargarCorte();
            }

        }

        private void reiniciarStockReal(int idSucursal)
        {
            DialogResult resp= MessageBox.Show("Está seguro que desea reiniciar el Stock?.", "Reiniciar Stock Real", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);

            if (resp==DialogResult.Yes)
            {
                oCorteN.reiniciarStockReal(idSucursal);

                cargarGrillaActualizada();
                listaStockCorteSucursal = null;
            }
        }

        private void reiniciarStockTeorico(int idSucursal)
        {
            DialogResult resp = MessageBox.Show("Está seguro que desea reiniciar el Stock?.", "Reiniciar Stock Teorico", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);

            if (resp == DialogResult.Yes)
            {
                oCorteN.reiniciarStockTeorico( idSucursal);
                cargarGrillaActualizada();
                listaStockCorteSucursal = null;
            }
            
        }

        private void btnBuscarCorte_Click(object sender, EventArgs e)
        {
            formBuscarCorte frmBuscarCorte = new formBuscarCorte();
            frmBuscarCorte.Show(this);
        }

        private void comboSucursal_TextChanged(object sender, EventArgs e)
        {
            cambiarSucursal();
        }

        private void txtCodigo_TextChanged(object sender, EventArgs e)
        {
            cargarCorte();
            cargarGrilla();
        }

        private void btnAgregar_Click(object sender, EventArgs e)
        {
            ingresarStockActual();
            txtCodigo.Focus();
        }

        private void btnAceptar_Click(object sender, EventArgs e)
        {
            agregarActualizacionStock();
        }

        private void todasLasSucursalesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            reiniciarStockReal(0);
        }

        private void sanLorenzoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            reiniciarStockReal(1);
        }

        private void sanMartinToolStripMenuItem_Click(object sender, EventArgs e)
        {
            reiniciarStockReal(2);
        }

        private void todasLasSucursalesToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            reiniciarStockTeorico(0);
        }

        private void sanLorenzoToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            reiniciarStockTeorico(1);
        }

        private void sanMartinToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            reiniciarStockTeorico(2);
        }

        private void btnCancelar_Click(object sender, EventArgs e)
        {
            this.Close();
        }

      

       

       

       
    }
}
