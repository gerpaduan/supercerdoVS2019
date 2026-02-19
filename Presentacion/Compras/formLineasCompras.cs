using iTextSharp.text;
using Presentacion.Compras;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace Presentacion
{
    public partial class formLineasCompras : formBaseColor
    {
        Negocio.Compra oCompraN;
        DataTable dtCompras = new DataTable();

        public DataTable dtSucursales;
        public Negocio.Sucursal oSucursalN = new Negocio.Sucursal(FormPrincipal.EmpresaSTATIC, FormPrincipal.ParametrosCTX);

        bool cargar = false;
        string descripcion, codigo, corte, tipoCompra;
        public formLineasCompras()
        {
            InitializeComponent(); this.Icon = Properties.Resources.CarniSys_ICONO;
        }
        
        private void formLineasCompras_Load(object sender, EventArgs e)
        {
            try
            {
                this.Text += Utilidades.Conexion.getSucursalConexion();
                cargarSucursal();
                this.comboTipoCompra.SelectedIndex = 0;
                //ocultar item media res si no soy yo la empresa
                if (!FormPrincipal.soyYo)
                {
                    comboTipoCompra.Items.Remove("Media Res");
                }
                fechaDesde.Value = DateTime.Today.AddMonths(-2);
                cargar = true;
                cargarGrilla();
            }
            catch (Exception ex)
            {
                if (Utilidades.Util_Form.errorConexionBD_Return(ex.Message))
                    formLineasCompras_Load(null, null);

                this.Close();
            }
        }
      
        #region metodos

        public void cargarGrilla()
        {            
            if (cargar)
            {

                lblActualizar.Visible = false;
                splitDescription();
                int idSucCombo = 0;
                if (comboSucursal.SelectedValue != null)
	            {
                    idSucCombo = Convert.ToInt32(comboSucursal.SelectedValue);
	            }
                oCompraN = new Negocio.Compra(FormPrincipal.EmpresaSTATIC, FormPrincipal.ParametrosCTX);

                grillaLineasCompras.AutoGenerateColumns = true;

                dtCompras = null;
                dtCompras = oCompraN.getLineasCompras(idSucCombo, tipoCompra, descripcion, codigo, corte, fechaDesde.Value.Date, fechaHasta.Value.Date, null);
                grillaLineasCompras.DataSource = dtCompras;

                formatearGrilla();
                cargarTotales();
                oCompraN = null;
            }
        }

        private void splitDescription()
        {
            string[] words = txtDescripcion.Text.ToString().Split('+');

            descripcion = words[0] != null ? words[0] : "";
            codigo = words.Count() > 1 &&  words[1] != null ? words[1] : "";
            corte = words.Count() > 2 && words[2] != null ? words[2] : "";
        }

        private void formatearGrilla()
        {
            grillaLineasCompras.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;


            grillaLineasCompras.Columns["idProveedor"].Visible = false;
            grillaLineasCompras.Columns["idSucursal"].Visible = false;

            grillaLineasCompras.Columns["cantKg"].DefaultCellStyle.Format = "F3";
            grillaLineasCompras.Columns["cantKg"].HeaderText = "Cantidad";
            grillaLineasCompras.Columns["precioKg"].DefaultCellStyle.Format = "F2";
            grillaLineasCompras.Columns["totalS"].DefaultCellStyle.Format = "F2";
            //formato para columna de fechas
            grillaLineasCompras.Columns["fechaCompra"].DefaultCellStyle.Format = "dd/MM/yyyy HH:mm:ss";
            grillaLineasCompras.Columns["creado"].DefaultCellStyle.Format = "dd/MM/yyyy HH:mm:ss";
            grillaLineasCompras.Columns["actualizado"].DefaultCellStyle.Format = "dd/MM/yyyy HH:mm:ss";
        }

        private void cargarTotales()
        {
            float totalKg = 0, totalS = 0;
            foreach (DataRow fila in dtCompras.Rows)
            {
                totalKg = totalKg + float.Parse(fila["cantKg"].ToString());
                totalS = totalS + float.Parse(fila["totalS"].ToString());
            }
            txtCantItems.Text = dtCompras.Rows.Count.ToString();
            txtTotalKgs.Text = totalKg.ToString("F3");
            txtTotalS.Text = totalS.ToString("F2");
        }

        private void modificarCompra()
        {
            try
            {
                int idCompra = Convert.ToInt32(grillaLineasCompras.CurrentRow.Cells["idCompra"].Value.ToString());

                if (Application.OpenForms["formModificarCompra"] != null)
                {
                    Application.OpenForms["formModificarCompra"].Activate();
                    Application.OpenForms["formModificarCompra"].WindowState = FormWindowState.Normal;
                }
                else
                {
                    formModificarCompra frmModificarCompra = new formModificarCompra();
                    frmModificarCompra.cargarParametros(null, idCompra);
                    frmModificarCompra.Show();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        #endregion
        
        #region eventos

        private void btnCancelar_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void fechaDesde_ValueChanged(object sender, EventArgs e)
        {
            //cargarGrilla();
        }

        private void fechaHasta_ValueChanged(object sender, EventArgs e)
        {
            //cargarGrilla();
        }
        private void txtDescripcion_TextChanged(object sender, EventArgs e)
        {
            lblActualizar.Visible = true; //cargarGrilla();
        }
        
        private void btnSeleccionar_Click_1(object sender, EventArgs e)
        {
            modificarCompra();
        }
        private void grillaLineasCompras_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            modificarCompra();
        }
        #endregion

        private void btnBuscar_Click(object sender, EventArgs e)
        {
            cargarGrilla();
        }

        private void comboTipoCompra_SelectedValueChanged(object sender, EventArgs e)
        {
            ///Todos
            //Media Res
            //Cortes
            txtDescripcion.Text = "";
            checkBusquedaMultiple.Visible = false;
            if (comboTipoCompra.Text == "Productos")
            {
                tipoCompra = "Cortes";
                //checkBusquedaMultiple.Visible = true; Comentado el 09/02/2026 xq no le encontré utilidad
                txtDescripcion.Text = "NroRem_RazonSoc+Codigo+Producto";
            }
            else
            {
                tipoCompra = comboTipoCompra.Text;
                checkBusquedaMultiple.Visible = false;
            }

            descripcion = codigo = corte = "";
            lblActualizar.Visible = true;
        }

        private void comboSucursal_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!comboSucursal.ValueMember.Equals(""))
            {
                cargarGrilla();
            }           
        }

        private void cargarSucursal()
        {
            dtSucursales = new DataTable();
            oSucursalN = new Negocio.Sucursal(FormPrincipal.EmpresaSTATIC, FormPrincipal.ParametrosCTX);
            dtSucursales = oSucursalN.obtenerSucursalesConTodas();

            comboSucursal.DataSource = dtSucursales;
            comboSucursal.DisplayMember = "sucursal";
            comboSucursal.ValueMember = "idSucursal";
            comboSucursal.SelectedValue = -1;
        }

        private void menuDuplicar_Click(object sender, EventArgs e)
        {
            formLineasCompras frmLineasCompras = new formLineasCompras();
            frmLineasCompras.Show();
        }

        private void txtDescripcion_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyValue.Equals(13))
            {
                cargarGrilla();
            }
        }
    }
}
