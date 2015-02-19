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
    public partial class formVentas : Form
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

        public DataTable dtVentas;

        bool cargar = false;
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

                    string sucursalSelect = "";
                    if (comboSucursal.Text == "Todas")
                    {

                    }
                    else
                    {
                        sucursalSelect = comboSucursal.Text;
                    }

                    dtVentas = new DataTable();

                    dtVentas = oVentaN.obtenerVentas(sucursalSelect, fechaDesde.Value.Date, fechaHasta.Value.Date, txtDescripcion.Text.Trim());

                    grillaVentas.AutoGenerateColumns = false;
                    grillaVentas.DataSource = null;
                    grillaVentas.DataSource = dtVentas;

                    ///
                    //if (logueado==false)
                    if (Presentacion.FormPrincipal.logueado == false)
                    {
                        foreach (DataGridViewColumn col in grillaVentas.Columns)
                        {
                            if (col.Name.Equals("totalS"))
                            {
                                col.Visible = false;
                            }
                        }
                    }
                    cargarTotales();
                }                                

            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }
        }

        private void cargarTotales()
        {
            float totalKgs=0,totalS=0;

            foreach (DataRow venta in dtVentas.Rows)
            {
                totalKgs += float.Parse(venta["totalKg"].ToString());
                totalS += float.Parse(venta["totalS"].ToString());

            }
            txtTotalKgs.Text = String.Format("{0:0.00}", totalKgs);
            if (Presentacion.FormPrincipal.logueado)
            {
                txtTotalS.Text = String.Format("{0:0.00}", totalS );
            }
            
        
        }

        private void cargarSucursal()
        {
            dtSucursales = new DataTable();
            oSucursalN = new Negocio.Sucursal();
            dtSucursales = oSucursalN.obtenerSucursales();

            DataRow nuevaFila = dtSucursales.NewRow();

            nuevaFila[0] = 3;
            nuevaFila[1] = "Todas";

            dtSucursales.Rows.Add(nuevaFila);

            comboSucursal.DataSource = dtSucursales;
            comboSucursal.DisplayMember = "sucursal";
            comboSucursal.ValueMember = "idSucursal";
            comboSucursal.SelectedIndex = 2;
        }

        private void infoVenta()
        {
            int idVenta = Convert.ToInt32(grillaVentas.CurrentRow.Cells["idVenta"].Value.ToString());

            DataRow drVenta = null;

            foreach (DataRow row in dtVentas.Rows)
            {
                if (row["idVenta"].Equals(idVenta)){
                    drVenta = row;
                }
            }

            if (Application.OpenForms["formInfoVenta"] != null)
            {

                Application.OpenForms["formInfoVenta"].Activate();
                Application.OpenForms["formInfoVenta"].WindowState = FormWindowState.Normal;


            }
            else
            {
                formInfoVenta frmInfoVenta = new formInfoVenta();
                frmInfoVenta.obtenerParametro(this, drVenta);
                frmInfoVenta.Show();

            }

        }

        private void nuevaVenta()
        {
            //formNuevaVenta frmNuevaVenta = new formNuevaVenta();
            //frmNuevaVenta.asigarFormVentas(this);
            //frmNuevaVenta.ShowDialog();

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
            fechaDesde.Value = fechaHasta.Value.AddMonths(-2);
            cargarSucursal();
            cargar = true;
            cargarGrilla();
        }
    }
}
