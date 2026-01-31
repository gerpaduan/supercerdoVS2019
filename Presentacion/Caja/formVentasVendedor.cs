using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Presentacion.Ventas;

namespace Presentacion
{
    public partial class formVentasVendedor : Form
    {
        public Entidades.CierreCaja oCierreE;
        public Negocio.Venta oVentaN = new Negocio.Venta(FormPrincipal.EmpresaSTATIC, FormPrincipal.ParametrosCTX);
        Negocio.Usuario oUsuarioN = new Negocio.Usuario(FormPrincipal.EmpresaSTATIC, FormPrincipal.ParametrosCTX);

        public DataTable dtVentas;
        bool soloAnulados = false;
        public bool desdeCajaVenta = false;

        public formVentasVendedor()
        {
            InitializeComponent(); this.Icon = Properties.Resources.CarniSys_ICONO;            
        }

        public void cargarGrilla()  
        {
            try
            {
                if (oCierreE == null && !(oUsuarioN.tienePermiso(oCierreE.UsuarioInicio, this.Name, DateTime.Today, Utilidades.ValoresParametrosMetodos.IdCreadorNulo())))
                {
                    Utilidades.Mensajes.ErrorPermisoAcceso();
                    this.Close();
                    return;
                }

                dtVentas = oVentaN.getVentasVendedorCierreCaja(oCierreE, soloAnulados);

                grillaVentas.AutoGenerateColumns = false;
                grillaVentas.DataSource = null;
                grillaVentas.DataSource = dtVentas;
                grillaVentas.Columns["totalKg"].Visible = !soloAnulados;
                grillaVentas.Columns["totalS"].Visible = !soloAnulados;
                foreach (DataGridViewRow row in grillaVentas.Rows)
                {
                    if (Convert.ToDecimal(row.Cells["totalS"].Value) == 0)
                    {
                        row.DefaultCellStyle.BackColor = Color.LightCoral;// Color.Orange;
                    }
                }
                cargarTotales();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar la grilla.\n\n"+ex.Message);
            }
        }

        private void cargarTotales()
        {
            //Si es de caja venta la llamada al form, no se muestra el Total
            if (desdeCajaVenta)
                return;

            float totalS=0;

            foreach (DataRow venta in dtVentas.Rows)
            {
                totalS += float.Parse(venta["totalS"].ToString());
            }
            txtTotalS.Text = soloAnulados ? "-" : String.Format("{0:0.00}", totalS );
        }

        private void infoVenta()
        {
            int idVenta = Convert.ToInt32(grillaVentas.CurrentRow.Cells["idVenta"].Value.ToString());

            if (Application.OpenForms["formInfoVenta"] != null)
            {
                Application.OpenForms["formInfoVenta"].Activate();
                Application.OpenForms["formInfoVenta"].WindowState = FormWindowState.Normal;
            }
            else
            {                
                if (desdeCajaVenta)
                {
                    Caja.formUltimaVenta frmUltimaVenta = new Caja.formUltimaVenta();
                    frmUltimaVenta.oUltimaVenta = oVentaN.getVentaById(idVenta);
                    frmUltimaVenta.oCierreE = oCierreE;
                    frmUltimaVenta.ShowDialog();

                    if (grillaVentas.CurrentRow != null)
                    {
                        // 1. Guardar el IdVenta de la fila actual
                        int idVentaSeleccionada = Convert.ToInt32(grillaVentas.CurrentRow.Cells["IdVenta"].Value);

                        // 2. Refrescar la grilla
                        cargarGrilla();

                        // 3. Buscar la fila y seleccionarla
                        foreach (DataGridViewRow fila in grillaVentas.Rows)
                        {
                            if (Convert.ToInt32(fila.Cells["IdVenta"].Value) == idVentaSeleccionada)
                            {
                                fila.Selected = true;                                // La pinta
                                grillaVentas.CurrentCell = fila.Cells[0];            // Le da foco
                                grillaVentas.FirstDisplayedScrollingRowIndex = fila.Index; // Hace scroll hasta mostrarla
                                break;
                            }
                        }
                    }
                }
                else
                {
                    formInfoVenta frmInfoVenta = new formInfoVenta();
                    frmInfoVenta.idVenta = idVenta;
                    frmInfoVenta.ShowDialog();
                }
            }
        }

        private void btnBuscar_Click(object sender, EventArgs e)
        {
            cargarGrilla();
        }

        private void btnSeleccionar_Click(object sender, EventArgs e)
        {
            infoVenta();
        }

        private void btnCancelar_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void grillaVentas_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            infoVenta();
        }

        private void formVentasVendedor_Load(object sender, EventArgs e)
        {
            soloAnulados = true;
            verSoloAnulados();
            this.Text = this.Text + " || " + oCierreE.UsuarioInicio.Nombre;
            this.Text += Utilidades.Conexion.getSucursalConexion();
            txtSucursal.Text = oCierreE.Sucursal.sucursal;
            txtVendedor.Text = oCierreE.UsuarioInicio.Nombre;
            cargarGrilla();
        }

        private void btnVerTodas_Click(object sender, EventArgs e)
        {
            verSoloAnulados();
            cargarGrilla();
        }

        private void verSoloAnulados()
        {
            if (soloAnulados)
            {
                soloAnulados = false;
                btnVerTodas.Text = "Ver &anulados";
            }
            else
            {
                soloAnulados = true;
                btnVerTodas.Text = "Ver &todas";
            }
        }

        private void grillaVentas_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyValue.Equals(13))
            {
                e.SuppressKeyPress = true;
                infoVenta();
            }
        }

        private void btnLineasVenta_Click(object sender, EventArgs e)
        {
            formLineasVendedor frmLineasVendedor = new formLineasVendedor();
            frmLineasVendedor.oCierreE = oCierreE;
            frmLineasVendedor.Show();
        }
    }
}
