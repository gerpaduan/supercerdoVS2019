using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Presentacion.Embutidos;
using Presentacion.Caja;
using System.Configuration;
using Presentacion.Usuario;
using Entidades;

namespace Presentacion
{
    public partial class formFormulas : Form, InterfaceUsuario
    {
        bool esVentaClientes=false;

        public bool EsVentaClientes
        {
            get { return esVentaClientes; }
            set { esVentaClientes = value; }
        }
        Negocio.Corte oCorteN = new Negocio.Corte();
        DataTable dtEmbutidos = new DataTable();

        DataTable dtSucursales;
        Negocio.Sucursal oSucursalN = new Negocio.Sucursal();
        public Negocio.Usuario oUsuarioN = new Negocio.Usuario();

        Entidades.Usuario oUsuario;
        bool cargar = false;
        public formFormulas()
        {
            InitializeComponent(); this.Icon = Properties.Resources.CarniSys_ICONO;        
        }

        public void cargarGrilla()
        {
            if (cargar)
            {
                try
                {
                    dtEmbutidos = null;
                    grilla.DataSource = null;
                    grilla.AutoGenerateColumns = true;
                    dtEmbutidos = oCorteN.buscarFormula(txtDescripcion.Text.Trim());
                    grilla.DataSource = dtEmbutidos;

                    formatearGrilla();
                    lblActualizar.Visible = false;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error al cargar la grilla.(Método: cargarGrilla()).\n\n" + ex.Message);
                }
            }
        }

        private void formatearGrilla()
        {
            grilla.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        }


        private void informacionEmbutido()
        {
            int idFormula = (Convert.ToInt32(grilla.CurrentRow.Cells["idFormula"].Value.ToString()));
            addOrEditFormula(idFormula);
        }

        private void nuevo_Click(object sender, EventArgs e)
        {
            addOrEditFormula(0);
        }

        private void addOrEditFormula(int idFormula)
        {
            if (Application.OpenForms["formIngresoFormula"] != null)
            {

                Application.OpenForms["formIngresoFormula"].Activate();
                Application.OpenForms["formIngresoFormula"].WindowState = FormWindowState.Normal;
            }
            else
            {
                formIngresoFormula formIngresoFormula = new formIngresoFormula();
                formIngresoFormula.oUsuario = FormPrincipal.oUserLogueado; 
                formIngresoFormula.idFormula = idFormula;
                formIngresoFormula.frmFormulas = this;
                formIngresoFormula.ShowDialog();
            }
            oUsuario = null;
        }

        public void EnviarUsuario(Entidades.Usuario usuario)
        {
            oUsuario = usuario;
        }

        private void btnSalir_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnBuscaProd_Click(object sender, EventArgs e)
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

        private void formFormulas_Load(object sender, EventArgs e)
        {

            if (!oUsuarioN.tienePermiso(FormPrincipal.oUserLogueado, this.Name, DateTime.Today, Utilidades.ValoresParametrosMetodos.IdCreadorNulo()))
            {
                Utilidades.Mensajes.ErrorPermisoAcceso();
                this.Close();
                return;
            }

            try
            {
                this.Text += Utilidades.Conexion.getSucursalConexion();
                cargar = true;
                cargarGrilla();  
            }
            catch (Exception ex)
            {
                if (Utilidades.Util_Form.errorConexionBD_Return(ex.Message))
                    formFormulas_Load(null, null);

                this.Close();
            }             
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Escape)
            {
                this.Close();
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void txtDescripcion_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyValue.Equals(13))
            {
                cargarGrilla();
            }
        }

        private void comboSucursal_SelectedValueChanged(object sender, EventArgs e)
        {
            cargarGrilla();
        }

        private void grillaEmbutidos_Sorted(object sender, EventArgs e)
        {
            formatearGrilla();
        }

        private void btnSeleccionar_Click(object sender, EventArgs e)
        {
            informacionEmbutido();
        }

        private void txtDescripcion_TextChanged(object sender, EventArgs e)
        {
            lblActualizar.Visible = true;
        }

        private void LineasEmb_Click(object sender, EventArgs e)
        {
            if (Application.OpenForms["formLineasEmb"] != null)
            {
                Application.OpenForms["formLineasEmb"].Activate();
                Application.OpenForms["formLineasEmb"].WindowState = FormWindowState.Normal;
            }
            else
            {
                formLineasEmb frmLineasEmb = new formLineasEmb();
                frmLineasEmb.Show();
            }
        }

        private void btnIngrRapido_Click(object sender, EventArgs e)
        {
            if (Application.OpenForms["formElegirEmbutido"] != null)
            {

                Application.OpenForms["formElegirEmbutido"].Activate();
                Application.OpenForms["formElegirEmbutido"].WindowState = FormWindowState.Normal;
            }
            else
            {
                Usuarios.formSelectUser frmSelectUser = new Presentacion.Usuarios.formSelectUser();
                frmSelectUser.ShowDialog(this);
                Presentacion.Embutidos.formElegirEmbutido frmElegirEmbutido = new Presentacion.Embutidos.formElegirEmbutido();
                frmElegirEmbutido.oUsuario = oUsuario;
                //frmElegirEmbutido.frmEmbutidos = this;
                frmElegirEmbutido.Show();
            }
            oUsuario = null;
        }

        private void eliminar_Click(object sender, EventArgs e)
        {
            try
            {
                DialogResult respuesta = MessageBox.Show("¿Está seguro que desea eliminar la formula de "+ grilla.CurrentRow.Cells["corte"].Value.ToString().ToUpper()+"?. ", "Eliminar Formula", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);

                if (respuesta == System.Windows.Forms.DialogResult.Yes)
                {
                    FormLoginVendedor frmLogin = new FormLoginVendedor();
                    frmLogin.ShowDialog(this);

                    if (oUsuario == null) return;

                    Entidades.Formula formulaEliminar = oCorteN.findFormulaByID(Convert.ToInt32(grilla.CurrentRow.Cells["idFormula"].Value.ToString()), 0);

                    if (!oUsuarioN.tienePermiso(oUsuario, "formIngresoFormula", DateTime.Today, formulaEliminar.CreadoPor.Id))
                    {
                        Utilidades.Mensajes.ErrorPermisoEdicion();
                        oUsuario = null;
                        return;
                    }

                    oCorteN.eliminarFormula(Convert.ToInt32(grilla.CurrentRow.Cells["idFormula"].Value.ToString()));
                    MessageBox.Show("La Formula se eliminó correctamente");
                    this.cargarGrilla();

                    oUsuario = null;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("La Formula no se pudo eliminar.\n" + ex.Message);
            }
        }
    }
}
