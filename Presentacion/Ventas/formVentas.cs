using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Presentacion.Ventas;
using Presentacion.Personas;

namespace Presentacion
{
    public partial class formVentas : Form, InterfacePersona
    {
        private bool logueado = false;

        public bool Logueado
        {
            get { return logueado; }
            set { logueado = value; }
        }

        public DataTable dtSucursales;

        public Negocio.Sucursal oSucursalN = new Negocio.Sucursal();
        public Negocio.Venta oVentaN = new Negocio.Venta();
        public Negocio.Usuario oUsuarioN = new Negocio.Usuario();

        public DataTable dtVentas;

        bool cargar = false;
        bool soloAnulados = false;

        int idCliente = -1;//-1 busca a todos
        string[] arrayRowFilter = new string[] {"1 = 1", "1 = 1", "1 = 1", "1 = 1"};
        string consultaRowFilter = "";

        public formVentas()
        {
            InitializeComponent();
        }

        public void cargarGrilla()
        {
            try
            {
                if (cargar)
                {
                    lblActualizar.Visible = false;
                    dtVentas = new DataTable();
                    dtVentas = oVentaN.obtenerVentas(Convert.ToInt32(comboSucursal.SelectedValue.ToString()), idCliente, 
                        Convert.ToInt32(comboUsuario.SelectedValue.ToString()), fechaDesde.Value, fechaHasta.Value, 
                        txtDescripcion.Text.Trim(), soloAnulados);

                    grillaVentas.AutoGenerateColumns = false;
                    grillaVentas.DataSource = null;
                    grillaVentas.DataSource = dtVentas;
                    grillaVentas.Columns["totalKg"].Visible = !soloAnulados;
                    grillaVentas.Columns["totalS"].Visible = !soloAnulados;
                    cargarTotales();
                    aplicarRowFilter();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Se produjo un error al cargar las ventas.\n\n"+ex.Message);
            }
        }

        private void cargarTotales()
        {
            float totalKgs = 0, totalS = 0, totComisionTarj = 0, totalKgsAj = 0, totalImpAj = 0;

            //foreach (DataRow venta in dtVentas.Rows)
            foreach (DataGridViewRow row in grillaVentas.Rows)
            {
                totComisionTarj += float.Parse(row.Cells["totComTarj"].Value.ToString());
                totalImpAj += (float.Parse(row.Cells["totAjuste"].Value.ToString()) + 
                    float.Parse(row.Cells["totalImpAj"].Value.ToString()));
                totalKgsAj += float.Parse(row.Cells["totalKgAj"].Value.ToString());
                totalKgs += float.Parse(row.Cells["totalKg"].Value.ToString());
                totalS += float.Parse(row.Cells["totalS"].Value.ToString());
            }
            txtCantItems.Text = grillaVentas.Rows.Count.ToString();
            txtTotComisionTarj.Text = String.Format("{0:0.00}", totComisionTarj);
            txtKgsAj.Text = String.Format("{0:0.000}", totalKgsAj);
            txtTotalSAj.Text = String.Format("{0:0.00}", totalImpAj);
            totalKgs = totalKgs - totalKgsAj;//resta los kgs del ajuste
            txtTotalKgs.Text = String.Format("{0:0.000}", totalKgs);
            if (Presentacion.FormPrincipal.logueado)
            {
                txtTotalS.Text = String.Format("{0:0.00}", totalS );
            }
        }

        private void cargarSucursal()
        {
            dtSucursales = new DataTable();
            oSucursalN = new Negocio.Sucursal();
            dtSucursales = oSucursalN.obtenerSucursalesConTodas();
            comboSucursal.DataSource = dtSucursales;
            comboSucursal.DisplayMember = "sucursal";
            comboSucursal.ValueMember = "idSucursal";
        }

        private void infoVenta()
        {
            int idVenta = Convert.ToInt32(grillaVentas.CurrentRow.Cells["idVenta"].Value.ToString());
            
            bool formAbierto = false;
            foreach (Form frm in Application.OpenForms)
            {
                if (frm.GetType() == typeof(formInfoVenta))
                {
                    foreach (Control ctrl in frm.Controls)
                    {
                        if (ctrl.Name.Equals("idVentaLabel") && ctrl.Text.Equals(idVenta.ToString()))
                        {
                            frm.BringToFront();
                            formAbierto = true;
                            break;
                        }
                    }
                }
            }
            if (!formAbierto)
            {
                formInfoVenta frmInfoVenta = new formInfoVenta();
                frmInfoVenta.idVenta = idVenta;
                frmInfoVenta.frmVentas = this;
                frmInfoVenta.Show();
            }
        }

        private void nuevaVenta()
        {
            if (Application.OpenForms["formNuevaVenta"] != null)
            {
                Application.OpenForms["formNuevaVenta"].Activate();
                Application.OpenForms["formNuevaVenta"].WindowState = FormWindowState.Normal;
            }
            else
            {
                formNuevaVenta frmNuevaVenta = new formNuevaVenta();
                frmNuevaVenta.asigarFormVentas(this);
                frmNuevaVenta.Show();
            }
        }

        private void nuevo_Click(object sender, EventArgs e)
        {
            nuevaVenta();
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

        private void formVentas_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode==Keys.N)
            {
                nuevaVenta();
            }
        }

        private void comboSucursal_SelectedIndexChanged(object sender, EventArgs e)
        {
            cargarGrilla();
        }

        private void formVentas_Load(object sender, EventArgs e)
        {
            try
            {
                this.Text += Utilidades.Conexion.getSucursalConexion();
                DateTime today = DateTime.Today.Date.AddHours(24);
                fechaHasta.Value = today.AddMilliseconds(-1);
                fechaDesde.Value = today.AddDays(-1);
                cargarSucursal();
                cargarComboVendedor();
                cargar = true;
                cargarGrilla();

                //Se establecen a checked todos los componentes
                for (int i = 0; i < checkListFormaPago.Items.Count; i++)
                {
                    checkListFormaPago.SetItemChecked(i, true);
                }
                for (int i = 0; i < checkListTipoComprobante.Items.Count; i++)
                {
                    checkListTipoComprobante.SetItemChecked(i, true);
                } 
                for (int i = 0; i < checkListCondVenta.Items.Count; i++)
                {
                    checkListCondVenta.SetItemChecked(i, true);
                }
            }
            catch (Exception ex)
            {
                if (Utilidades.Util_Form.errorConexionBD_Return(ex.Message)) 
                    formVentas_Load(null, null);

                this.Close();
            }
        }

        private void cargarComboVendedor()
        {
            comboUsuario.DataSource = oUsuarioN.obtenerUsuariosConTodos(true);
            comboUsuario.DisplayMember = "nombre";
            comboUsuario.ValueMember = "id";
            comboUsuario.SelectedIndex = 0; 
        }

        private void btnVerTodas_Click(object sender, EventArgs e)
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
            cargarGrilla();
        }

        private void txtDescripcion_KeyDown(object sender, KeyEventArgs e)
        {
            lblActualizar.Visible = true;
            if (e.KeyValue.Equals(13))
            {
                cargarGrilla();
            }
        }

        private void fechaDesde_ValueChanged(object sender, EventArgs e)
        {
            lblActualizar.Visible = true;
        }

        private void menuDuplicar_Click(object sender, EventArgs e)
        {
            formVentas frmVentaDuplicar = new formVentas();
            frmVentaDuplicar.Show();
        }

        private void LineasVtas_Click(object sender, EventArgs e)
        {
            if (logueado)
            {
                if (Application.OpenForms["formGetAllLineaVenta"] != null)
                {
                    Application.OpenForms["formGetAllLineaVenta"].Activate();
                    Application.OpenForms["formGetAllLineaVenta"].WindowState = FormWindowState.Normal;

                }
                else
                {
                    formGetAllLineaVenta frmTemporalLineaVenta = new formGetAllLineaVenta();
                    frmTemporalLineaVenta.Show();
                }
            }
            else
            {
                MessageBox.Show("No está logueado");
            }
        }

        private void btnBuscarCliente_Click(object sender, EventArgs e)
        {
            buscarCliente();
        }

        private void buscarCliente()
        {
            formBuscarPersona frmBuscarPersona = new formBuscarPersona();
            frmBuscarPersona.Show(this);
        }

        public void EnviarPersona(Entidades.Persona persona)
        {
            this.txtCliente.Text = persona.razonSocial;
            idCliente = persona.idPersona;
            cargarGrilla();
        }

        private void btnLimpiar_Click(object sender, EventArgs e)
        {
            if (idCliente != -1)//validacion para evitar conexion a la BD
            {
                idCliente = -1;
                cargarGrilla();
                txtCliente.Text = "todos";
            }
        }

        private void checkListFormaPago_SelectedIndexChanged(object sender, EventArgs e)
        {
            string nombreCol = "formaPago";
            string consulta = "1 <> 1"; //checkListFormaPago.CheckedItems.Count == 0 ? "1 = 1" : "1 <> 1";
            foreach (string item in checkListFormaPago.CheckedItems)
            {
                if (item == "Efectivo")
                    consulta += " OR " + nombreCol + " = 'Efectivo'";
                if (item == "Debito")
                    consulta += " OR " + nombreCol + " = 'Debito'";
                if (item == "Credito")
                    consulta += " OR " + nombreCol + " = 'Credito'";
                if (item == "Qr")
                    consulta += " OR " + nombreCol + " = 'Qr'";
                if (item == "Transferencia")
                    consulta += " OR " + nombreCol + " = 'Transferencia'";
                if (item == "Cta.Cte")
                    consulta += " OR " + nombreCol + " = 'CtaCte'";
                //if (item == "Otras")
                //    consulta += " OR (" + nombreCol + " <> 'Efectivo'" + " AND " + nombreCol + " <> 'Debito'" + 
                //        " AND " + nombreCol + " <> 'Credito')";
            }
            arrayRowFilter[1] = consulta;
            aplicarRowFilter();

        }

        private void checkListTipoComprobante_SelectedIndexChanged(object sender, EventArgs e)
        {
            string nombreCol = "tipoComprobante";
            string consulta = "1 <> 1"; //checkListTipoComprobante.CheckedItems.Count == 0 ? "1 = 1" : "1 <> 1";
            foreach (string item in checkListTipoComprobante.CheckedItems)
            {
                if (item == "Remito X")
                    consulta += " OR " + nombreCol + " = 'X'";
                if (item == "Factura A")
                    consulta += " OR " + nombreCol + " = 'A'";
                if (item == "Factura B")
                    consulta += " OR " + nombreCol + " = 'B'";
            }
            arrayRowFilter[2] = consulta;
            aplicarRowFilter();
        }

        private void checkListCondVenta_SelectedIndexChanged(object sender, EventArgs e)
        {
            //string nombreCol = "enCtaCte";
            //string consulta = "1 <> 1";// checkListCondVenta.CheckedItems.Count == 0 ? "1 = 1" : "1 <> 1";
            //foreach (string item in checkListCondVenta.CheckedItems)
            //{
            //    if (item == "Contado")
            //        consulta += " OR " + nombreCol + " = '0'";

            //    if (item == "Cta.Cte")
            //        consulta += " OR " + nombreCol + " = '1'";
            //}
            //arrayRowFilter[3] = consulta;
            //aplicarRowFilter();
        }

        private void aplicarRowFilter()
        {
            consultaRowFilter = "";

            for (int i = 0; i < arrayRowFilter.Length; i++)
            {
                string and = (i != arrayRowFilter.Length - 1) ? " AND " : "";
                consultaRowFilter += "( " + arrayRowFilter[i] + " )" + and;
            }

            (grillaVentas.DataSource as DataTable).DefaultView.RowFilter = string.Format(consultaRowFilter);

            cargarTotales();
        
        }
    }
}
