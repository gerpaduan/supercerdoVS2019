using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Presentacion.Reportes;
using System.Configuration;
using Presentacion.Caja;

namespace Presentacion.Embutidos
{
    public partial class formInfoEmbutido : Form, InterfaceUsuario
    {
        public formEmbutidos frmEmbutidos;
        public int idEmbutido_ = 0;

        Entidades.Embutido oEmbutidoE;
        Entidades.Usuario oUsuario;

        Negocio.Corte oCorteN = new Negocio.Corte();

        DataTable dtCortesPorEmbutido = new DataTable();


        public formInfoEmbutido()
        {
            InitializeComponent();
        }

        private void formInfoEmbutido_Load(object sender, EventArgs e)
        {
            this.Text += Utilidades.Conexion.getSucursalConexion();
            try
            {
                oEmbutidoE = oCorteN.findEmbutidoById(idEmbutido_);
                int sucActual = Convert.ToInt32(ConfigurationManager.AppSettings["idSucursal"].ToString());
                cargarCampos();
                cargarGrilla(); 
                if (!FormPrincipal.logueado && Convert.ToDateTime(txtFechaEmbutido.Text) < DateTime.Today ||
                    !FormPrincipal.logueado && oEmbutidoE.sucursal.idSucursal != sucActual)
                {
                    anular.Enabled = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar el formulario.(Metodo: Load())\n\n" + ex.Message);
            }  
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
            try
            {
                grillaCortesPorEmbutido.DataSource = null;
                grillaCortesPorEmbutido.AutoGenerateColumns = false;

                dtCortesPorEmbutido=oCorteN.obtenerCortesPorEmbutidos(oEmbutidoE);
                grillaCortesPorEmbutido.DataSource = dtCortesPorEmbutido;

                cargarTotalKg();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar el formulario.(Metodo: cargarGrilla())\n\n" + ex.Message);
            }  
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
            txtFechaEmbutido.Text = Utilidades.Util_Form.fechaFormato24Horas(oEmbutidoE.fechaEmbutido);
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
                FormLoginVendedor frmLogin = new FormLoginVendedor();
                frmLogin.ShowDialog(this);

                if (oUsuario != null )
                {
                    oEmbutidoE.ActualizadoPor = oUsuario;
                    oCorteN.anularEmbutido(oEmbutidoE);
                    embutidoAnulado();
                }
                oUsuario = null;            
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

        public void EnviarUsuario(Entidades.Usuario usuario)
        {
            oUsuario = usuario;
        }

        private void Imprimir_Click(object sender, EventArgs e)
        {
            try
            {
                string titulo = oEmbutidoE.corte.corte;
                FormReportes frmReportes;

                ReportesDataSet.dtCortesPorEmbutidoDataTable dtCortesEmbutido = new ReportesDataSet.dtCortesPorEmbutidoDataTable();

                foreach (DataRow fila in dtCortesPorEmbutido.Rows)
                {
                    DataRow dsFila = dtCortesEmbutido.NewRow();
                    dsFila["Codigo"] = fila["Codigo"];
                    dsFila["Corte"] = fila["Corte"];
                    dsFila["TotalKg"] = fila["kgUtilizados"];

                    dtCortesEmbutido.Rows.Add(dsFila);
                }

                Reportes.ReporteEmbutido reporte = new Reportes.ReporteEmbutido();
                frmReportes = new FormReportes(reporte, titulo, dtCortesEmbutido, oEmbutidoE.fechaEmbutido, oEmbutidoE.fechaEmbutido);

                frmReportes.Objetos = false;
                frmReportes.ReporteMovimiento = false;

                frmReportes.Show();

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
