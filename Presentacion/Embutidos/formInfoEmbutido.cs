using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using System.Configuration;
using Presentacion.Caja;
using Entidades;

namespace Presentacion.Embutidos
{
    public partial class formInfoEmbutido : Form, InterfaceUsuario
    {
        public formEmbutidos frmEmbutidos;
        public int idEmbutido_ = 0;

        Entidades.Embutido oEmbutidoE;
        Entidades.Usuario oUsuario;

        Negocio.Corte oCorteN = new Negocio.Corte();
        Negocio.Usuario oUsuarioN = new Negocio.Usuario();

        DataTable dtCortesPorEmbutido = new DataTable();
        private Formula oFormulaE;

        public formInfoEmbutido()
        {
            InitializeComponent(); this.Icon = Properties.Resources.CarniSys_ICONO;
        }

        private void formInfoEmbutido_Load(object sender, EventArgs e)
        {
            this.Text += Utilidades.Conexion.getSucursalConexion();
            try
            {
                oEmbutidoE = oCorteN.findEmbutidoById(idEmbutido_);
                int sucActual = Convert.ToInt32(Utilidades.Conexion.getIdSucursalConexion());
                cargarCampos();
                cargarGrilla();
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
            oFormulaE = oCorteN.findFormulaByID(0, oEmbutidoE.corte.idCorte);
            txtReceta.Text = oFormulaE.Receta;

            if (oEmbutidoE.estado=="Anulado")
            {
                barraControl.Visible = false;
                panelAnulado.Visible = true;
            }
        }

        private void anularEmbutido()
        {
            FormLoginVendedor frmLogin = new FormLoginVendedor();
            frmLogin.soloActivos = true;
            frmLogin.ShowDialog(this);

            if (oUsuario == null)
                return;

            if (!oUsuarioN.tienePermiso(oUsuario, "formIngresoEmbutido", oEmbutidoE.fechaEmbutido, oEmbutidoE.CreadoPor.Id))
            {
                Utilidades.Mensajes.ErrorPermisoEdicion();
                return;
            }

            DialogResult respuesta=MessageBox.Show("¿Está seguro que desea anular el Elaborado?. ","Anular Elaborado", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);

            if (respuesta == System.Windows.Forms.DialogResult.Yes)
            {
                oEmbutidoE.ActualizadoPor = oUsuario;
                oCorteN.anularEmbutido(oEmbutidoE);
                embutidoAnulado();
            }
            oUsuario = null;
        }

        private void embutidoAnulado()
        {
            barraControl.Visible = false;
            panelAnulado.Visible = true;
        }

        private void cargarGrillaFormEmbutidos()
        {
            //frmEmbutidos.cargarGrilla();
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

        private void btnReceta_Click(object sender, EventArgs e)
        {
            formReceta frmReceta = new formReceta(txtReceta.Text); // Pasar el texto actual
            frmReceta.editar = false;
        }
    }
}
