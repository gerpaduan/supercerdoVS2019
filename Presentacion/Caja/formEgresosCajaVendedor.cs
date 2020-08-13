using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Presentacion.Caja
{
    public partial class formEgresosCajaVendedor : Form
    {
        protected Negocio.CierreCaja oCierreN = new Negocio.CierreCaja();
        protected Negocio.Sucursal oSucursalN = new Negocio.Sucursal();

        protected Entidades.EgresoCaja oEgresoCajaE = new Entidades.EgresoCaja();
        protected Entidades.Sucursal oSucursalE = new Entidades.Sucursal();

        public Entidades.CierreCaja oCierreE;
        int idVentaSelected = 0;//Obtiene el IdVenta correspondiente al EgresoCaja ( 0 si no es egreso por Venta)

        public formEgresosCajaVendedor()
        {
            InitializeComponent();
        }

        private void formEgresosCajaVendedor_Load(object sender, EventArgs e)
        {
            form_Load();
            comboFiltro.SelectedIndex = 0;
        }

        private void form_Load()
        {
            this.Text += Utilidades.Conexion.getSucursalConexion();
            this.Text = "Egresos Caja " + oCierreE.UsuarioInicio.Nombre;
            grillaEgresosCaja.DataSource = oCierreN.getEgresosCajaVendedor(oCierreE);
            grillaEgresosCaja.Columns["Fecha"].DefaultCellStyle.Format = "dd/MM/yyyy HH:mm:ss";
            grillaEgresosCaja.Columns["Detalle"].AutoSizeMode = DataGridViewAutoSizeColumnMode.ColumnHeader;
            grillaEgresosCaja.Columns["Monto"].DefaultCellStyle.Format = "F2";
            grillaEgresosCaja.Columns["Creado"].DefaultCellStyle.Format = "dd/MM/yyyy HH:mm:ss";
            grillaEgresosCaja.Columns["Actualizado"].DefaultCellStyle.Format = "dd/MM/yyyy HH:mm:ss";

            CargarTotal();
        }

        private void CargarTotal()
        {
            decimal total = 0;
            foreach (DataGridViewRow row in grillaEgresosCaja.Rows)
            {
                total = total + Convert.ToDecimal(row.Cells["monto"].Value.ToString());
            }
            txtTotalS.Text = total.ToString("F2");
        }

        private void btnCancelar_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void grillaEgresosCaja_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            seleccionarEgresoCaja();
        }

        private void seleccionarEgresoCaja()
        {
            try
            {
                DataGridViewRow fila = grillaEgresosCaja.CurrentRow;

                if (fila != null)
                {
                    txtFechaTexto.Text = fila.Cells["Fecha"].Value.ToString();
                    txtTipoEgresoCaja.Text = fila.Cells["TipoEgresoCaja"].Value.ToString();
                    txtDescripcion.Text = fila.Cells["Descripción"].Value.ToString();
                    txtMonto.Text = fila.Cells["Monto"].Value.ToString();
                    txtDetalle.Text = fila.Cells["Detalle"].Value.ToString();
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Error al seleccionar fila");
            }
        }

        private void grillaEgresosCaja_SelectionChanged(object sender, EventArgs e)
        {
            seleccionarEgresoCaja();
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Escape)
            {
                this.Close();
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void btnNuevoGasto_Click(object sender, EventArgs e)
        {
            formAddOrEditEgresoCaja frmAddOrEditEgresoCaja = new formAddOrEditEgresoCaja();
            frmAddOrEditEgresoCaja.oUsuario = oCierreE.UsuarioInicio;
            frmAddOrEditEgresoCaja.egresoDesdeCajaVenta = true;
            frmAddOrEditEgresoCaja.ShowDialog();

            //formEgresosCajaVendedor_Load(null, null);
            form_Load();
        }

        private void btnVerGasto_Click(object sender, EventArgs e)
        {
            try
            {
                int idEgresoCaja = Convert.ToInt32(grillaEgresosCaja.CurrentRow.Cells["id"].Value.ToString());
                oEgresoCajaE = oCierreN.getEgresoCajaById(idEgresoCaja);

                ///si es Venta con tarjeta o Cta Cte se muestra la venta infoVenta
                if (oEgresoCajaE.Monto > 0 && (oEgresoCajaE.IdTipoEgresoCaja.Equals(Entidades.EgresoCaja.idPagoTarjeta) ||
                    oEgresoCajaE.esEgresoCtaCte(oEgresoCajaE.IdTipoEgresoCaja)))
                {
                    //Obtiene los Numeros del String
                    string resultString = System.Text.RegularExpressions.Regex.Match(oEgresoCajaE.Descripcion, @"\d+").Value;

                    idVentaSelected = Convert.ToInt32(resultString);
                    Ventas.formInfoVenta frmInfoVenta = new Ventas.formInfoVenta();
                    frmInfoVenta.idVenta = idVentaSelected;
                    frmInfoVenta.ShowDialog();
                    return;
                }

                formAddOrEditEgresoCaja frmAddOrEditEgresoCaja = new formAddOrEditEgresoCaja();
                frmAddOrEditEgresoCaja.oUsuario = oCierreE.UsuarioInicio;
                frmAddOrEditEgresoCaja.idEgresoCaja = idEgresoCaja;
                frmAddOrEditEgresoCaja.ShowDialog();

                idVentaSelected = 0;
                formEgresosCajaVendedor_Load(null, null);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void comboFiltro_SelectedIndexChanged(object sender, EventArgs e)
        {

            string nombreCol = grillaEgresosCaja.Columns["TipoEgresoCaja"].Name;

            switch (comboFiltro.Text)
            {
                case "Todos":
                        form_Load();
                        break;
                case "Tarjeta":
                        (grillaEgresosCaja.DataSource as DataTable).DefaultView.RowFilter = string.Format(nombreCol + "= 'Tarjeta Pago'");// comboFiltro.SelectedItem.ToString());
                        break;
                case "Egresos":
                        (grillaEgresosCaja.DataSource as DataTable).DefaultView.RowFilter = string.Format(nombreCol + "<> 'Tarjeta Pago'");// comboFiltro.SelectedItem.ToString());
                        break;
                default:
                    break;
            }
            CargarTotal();
        }

        private void formEgresosCajaVendedor_Activated(object sender, EventArgs e)
        {
            //se cargar el form si se selecciona un egreso correspondiente a venta
            if (idVentaSelected > 0)
            {
                formEgresosCajaVendedor_Load(null, null);
            }
        }
    }
}
