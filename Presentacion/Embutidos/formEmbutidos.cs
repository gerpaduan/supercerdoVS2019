using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Presentacion.Embutidos;

namespace Presentacion
{
    public partial class formEmbutidos : Form
    {
        Negocio.Corte oCorteN = new Negocio.Corte();
        DataTable dtEmbutidos = new DataTable();

        DataTable dtSucursales;
        Negocio.Sucursal oSucursalN = new Negocio.Sucursal();

        public formEmbutidos()
        {
            InitializeComponent();
            
            cargarSucursal();
            cargarGrilla();
        }

        public void cargarGrilla()
        {
            string sucursal = "";

            if (comboSucursal.Text!="Todas")
            {
                sucursal = comboSucursal.Text;
            }

            dtEmbutidos = null;
            grillaEmbutidos.DataSource = null;
            grillaEmbutidos.AutoGenerateColumns = false;
            dtEmbutidos=oCorteN.buscarEmbutido(sucursal,txtProducto.Text.Trim(), fechaDesde.Value.Date, fechaHasta.Value.Date);
            grillaEmbutidos.DataSource = dtEmbutidos;

            cargarCampoTotal();
        }

        private void cargarCampoTotal()
        {
            float totalKg = 0;
            foreach (DataRow fila in dtEmbutidos.Rows)
            {
                if (fila["estado"]==null || fila["estado"].ToString()=="")
                {
                    totalKg = totalKg + float.Parse(fila["totalKg"].ToString());
                }
            }

            txtTotalKg.Text = Convert.ToString(totalKg);
        
        }

        private void informacionEmbutido()
        {
            Entidades.Embutido oEmbutidoE = new Entidades.Embutido();

            oEmbutidoE.idEmbutido = Convert.ToInt32(grillaEmbutidos.CurrentRow.Cells["idEmbutido"].Value.ToString());
            oEmbutidoE.fechaEmbutido = Convert.ToDateTime(grillaEmbutidos.CurrentRow.Cells["fechaEmbutido"].Value.ToString());
            
            //creo Corte correspondiente al embutido y lo asigno
            Entidades.Corte oCorteE = new Entidades.Corte();
            oCorteE.idCorte = Convert.ToInt32(grillaEmbutidos.CurrentRow.Cells["idCorte"].Value.ToString());
            oCorteE.codigo = Convert.ToInt32(grillaEmbutidos.CurrentRow.Cells["codigo"].Value.ToString());
            oCorteE.corte = grillaEmbutidos.CurrentRow.Cells["corte"].Value.ToString();

            oEmbutidoE.corte = oCorteE;
            
            oEmbutidoE.estado = grillaEmbutidos.CurrentRow.Cells["estado"].Value.ToString();
            oEmbutidoE.observaciones = grillaEmbutidos.CurrentRow.Cells["observaciones"].Value.ToString();

            //creo sucursal y lo asigno al embutido
            Entidades.Sucursal oSucursalE = new Entidades.Sucursal();
            oSucursalE.idSucursal = Convert.ToInt32(grillaEmbutidos.CurrentRow.Cells["idSucursal"].Value.ToString());
            oSucursalE.sucursal = grillaEmbutidos.CurrentRow.Cells["sucursal"].Value.ToString();

            oEmbutidoE.sucursal = oSucursalE;



            if (Application.OpenForms["formInfoEmbutido"] != null)
            {

                Application.OpenForms["formInfoEmbutido"].Activate();
                Application.OpenForms["formInfoEmbutido"].WindowState = FormWindowState.Normal;


            }
            else
            {
                formInfoEmbutido frmInfoEmbutido = new formInfoEmbutido();
                frmInfoEmbutido.obtenerParametros(oEmbutidoE, this);
                frmInfoEmbutido.Show();

            }
            
        }

        private void nuevo_Click(object sender, EventArgs e)
        {

            if (Application.OpenForms["formIngresoEmbutido"] != null)
            {

                Application.OpenForms["formIngresoEmbutido"].Activate();
                Application.OpenForms["formIngresoEmbutido"].WindowState = FormWindowState.Normal;


            }
            else
            {
                formIngresoEmbutido frmIngresoEmbutido = new formIngresoEmbutido();
                frmIngresoEmbutido.obtenerParametros(this);
                frmIngresoEmbutido.Show();

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

        private void btnSalir_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnBuscaProd_Click(object sender, EventArgs e)
        {
            cargarGrilla();
        }

     
        private void fechaHasta_ValueChanged(object sender, EventArgs e)
        {

        }

        private void txtProducto_TextChanged(object sender, EventArgs e)
        {
            cargarGrilla();
        }

        private void grillaEmbutidos_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            informacionEmbutido();
        }

        private void modificar_Click(object sender, EventArgs e)
        {
            informacionEmbutido();
        }

        private void comboSucursal_TextChanged(object sender, EventArgs e)
        {
            cargarGrilla();
        }
    }
}
