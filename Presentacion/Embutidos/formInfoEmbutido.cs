using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Presentacion.Embutidos
{
    public partial class formInfoEmbutido : Form
    {
        formEmbutidos frmEmbutidos;

        Entidades.Embutido oEmbutidoE = new Entidades.Embutido();

        Negocio.Corte oCorteN = new Negocio.Corte();

        DataTable dtCortesPorEmbutido = new DataTable();


        public formInfoEmbutido()
        {
            InitializeComponent();
        }

        public void obtenerParametros(Entidades.Embutido embutidoParam, formEmbutidos formEmbutidoParam)
        {
            frmEmbutidos = formEmbutidoParam;

            oEmbutidoE = embutidoParam;

            cargarCampos();
            cargarGrilla();            
        }

        private void cargarGrilla()
        {
            grillaCortesPorEmbutido.DataSource = null;
            grillaCortesPorEmbutido.AutoGenerateColumns = false;

            dtCortesPorEmbutido=oCorteN.obtenerCortesPorEmbutidos(oEmbutidoE);
            grillaCortesPorEmbutido.DataSource = dtCortesPorEmbutido;

            cargarTotalKg();
        }

        private void cargarTotalKg()
        {
            float totalKg=0;
        
            foreach (DataRow fila in dtCortesPorEmbutido.Rows)
	        {
        		 totalKg=totalKg+float.Parse(fila["kgUtilizados"].ToString());
	        }

            txtTotalKg.Text = Convert.ToString(totalKg);
        }

        private void cargarCampos()
        {
            txtSucursal.Text = oEmbutidoE.sucursal.sucursal;
            txtFechaEmbutido.Value = oEmbutidoE.fechaEmbutido;
            txtCodigoEmbutido.Text =Convert.ToString( oEmbutidoE.corte.codigo);
            txtEmbutido.Text = oEmbutidoE.corte.corte;
            txtObservaciones.Text = oEmbutidoE.observaciones;

            if (oEmbutidoE.estado=="Anulado")
            {
                barraControl.Visible = false;
                panelAnulado.Visible = true;
            }

            
        }


        private void anularEmbutido()
        {
            DialogResult respuesta=MessageBox.Show("¿Está seguro que desea anular el Embutido?. ","Anular Embutido", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);

            if (respuesta == System.Windows.Forms.DialogResult.Yes)
            {
                oCorteN.anularEmbutido(oEmbutidoE);

                foreach (DataRow cortePorEmbutido in dtCortesPorEmbutido.Rows)
                {
                    oCorteN.actualizarStockEmbutido(cortePorEmbutido, oEmbutidoE);
                }

                embutidoAnulado();
            }
        }

        private void embutidoAnulado()
        {
            barraControl.Visible = false;
            panelAnulado.Visible = true;
        }

        private void cargarGrillaFormEmbutidos()
        {
            frmEmbutidos.cargarGrilla();
            this.Close();
        }


        private void btnAceptar_Click(object sender, EventArgs e)
        {
            cargarGrillaFormEmbutidos();
        }

        private void btnCancelar_Click(object sender, EventArgs e)
        {
            cargarGrillaFormEmbutidos();
        }

        private void anular_Click(object sender, EventArgs e)
        {
            anularEmbutido();
        }
    }
}
