using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Configuration;

namespace Presentacion.Caja
{
    public partial class formEgresosCaja : Form, InterfaceUsuario
    {
        protected Negocio.CierreCaja oCierreN = new Negocio.CierreCaja();
        protected Negocio.Sucursal oSucursalN = new Negocio.Sucursal();
        public Negocio.Usuario oUsuarioN = new Negocio.Usuario();

        protected Entidades.EgresoCaja oEgresoCajaE = new Entidades.EgresoCaja();
        protected Entidades.Sucursal oSucursalE = new Entidades.Sucursal();
        public Entidades.Usuario oUsuario;
        string[] arrayRowFilter = new string[] { "1 = 1", "1 = 1", "1 = 1", "1 = 1" };
        string consultaRowFilter = "";

        DataTable dtEgresosCaja = null;

        ///se establece true cuando se esta cargando el forma para evitar actualizaciones en la grilla
        ///al finalizar el Load se establece a false
        bool loadingForm = true;

        public formEgresosCaja()
        {
            InitializeComponent(); this.Icon = Properties.Resources.CarniSys_ICONO;
        }

        private void formEgresosCaja_Load(object sender, EventArgs e)
        {

            if (!oUsuarioN.tienePermiso(FormPrincipal.oUserLogueado, this.Name, DateTime.Today, Utilidades.ValoresParametrosMetodos.IdCreadorNulo()))
            {
                Utilidades.Mensajes.ErrorPermisoAcceso();
                this.Close();
                return;
            }

            this.Text += Utilidades.Conexion.getSucursalConexion();
            DateTime today = DateTime.Today;
            fechaHasta.Value = today.AddDays(1).AddSeconds(-1);
            fechaDesde.Value = today;
            cargarSucursal();
            cargarComboUsuario();
            cargarTiposEgresoCaja();
            loadingForm = false;
            oUsuario = FormPrincipal.oUserLogueado;
            cargarGrilla();            
        }


        private void cargarComboUsuario()
        {
            comboUsuario.DataSource = oUsuarioN.obtenerUsuariosConTodos(true);
            comboUsuario.DisplayMember = "nombre";
            comboUsuario.ValueMember = "id";
            comboUsuario.SelectedIndex = 0;
        }

        private void comboSucursal_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!comboSucursal.ValueMember.Equals(""))
            {
                int idSucursal = (int)comboSucursal.SelectedValue;
                oSucursalE = oSucursalN.findById(idSucursal);
                oEgresoCajaE.Sucursal = oSucursalE;
                cargarGrilla();
            }
        }

        public void cargarGrilla()
        {
            if (loadingForm) return;
            lblActualizar.Visible = false;

            if (!oUsuarioN.tienePermiso(FormPrincipal.oUserLogueado, this.Name, fechaDesde.Value.Date, Utilidades.ValoresParametrosMetodos.IdCreadorNulo()))
            {
                Utilidades.Mensajes.ErrorPermisoAcceso();
                return;
            }

            dtEgresosCaja = oCierreN.obtenerEgresosCaja(oEgresoCajaE.Sucursal.idSucursal,
                Convert.ToInt32(comboUsuario.SelectedValue.ToString()), oEgresoCajaE.IdTipoEgresoCaja, txtDescripcion.Text, fechaDesde.Value, fechaHasta.Value);
            grillaEgresosCaja.DataSource = dtEgresosCaja;
            grillaEgresosCaja.Columns["Fecha"].DefaultCellStyle.Format = "dd/MM/yyyy HH:mm:ss";
            grillaEgresosCaja.Columns["Detalle"].AutoSizeMode = DataGridViewAutoSizeColumnMode.ColumnHeader;
            grillaEgresosCaja.Columns["Monto"].DefaultCellStyle.Format = "F2";
            grillaEgresosCaja.Columns["Creado"].DefaultCellStyle.Format = "dd/MM/yyyy HH:mm:ss";
            grillaEgresosCaja.Columns["Actualizado"].DefaultCellStyle.Format = "dd/MM/yyyy HH:mm:ss";

            // Obtener el índice de la columna "Gasto" original
            int index = grillaEgresosCaja.Columns["Gasto"].Index;
            grillaEgresosCaja.Columns.Remove("Gasto");
            // Crear una columna de tipo DataGridViewCheckBoxColumn
            DataGridViewCheckBoxColumn checkBoxColumn = new DataGridViewCheckBoxColumn();
            checkBoxColumn.Name = "Gasto";
            checkBoxColumn.HeaderText = "Gasto";
            checkBoxColumn.DataPropertyName = "Gasto"; // Nombre de la propiedad en el origen de datos, si corresponde
            // Añadir la columna CheckBox a la grilla
            grillaEgresosCaja.Columns.Insert(index, checkBoxColumn);
            cargarTotales();
            buscarSoloGastos();
        }

        private void cargarTotales()
        {
            int cantItems = 0;
            decimal total = 0;
            foreach (DataGridViewRow row in grillaEgresosCaja.Rows)
            {
                cantItems++;
                total = total + Convert.ToDecimal(row.Cells["monto"].Value.ToString());
            }
            txtItems.Text = cantItems.ToString();
            txtTotalS.Text = total.ToString("F2");
        }

        private void cargarSucursal()
        {
            int idSucursal = Convert.ToInt32(Utilidades.Conexion.getIdSucursalConexion());
            oSucursalE = oSucursalN.findById(idSucursal);
            oEgresoCajaE.Sucursal = oSucursalE;

            comboSucursal.DataSource = oSucursalN.obtenerSucursales();
            comboSucursal.DisplayMember = "sucursal";
            comboSucursal.ValueMember = "idSucursal";
            comboSucursal.SelectedIndex = idSucursal - 1;
        }

        private void cargarTiposEgresoCaja()
        {
            comboTipoEgresoCaja.DataSource = oCierreN.obtenerTiposEgresoCaja("", 0);
            comboTipoEgresoCaja.DisplayMember = "tipoEgresoCaja";
            comboTipoEgresoCaja.ValueMember = "id";
            //comboSucursal.SelectedIndex = idSucursal - 1;
        }

        private void btnBuscar_Click(object sender, EventArgs e)
        {
            cargarGrilla();
        }

        private void comboTipoEgresoCaja_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!comboTipoEgresoCaja.ValueMember.Equals(""))
            {
                int idTipoEgresoCaja = (int)comboTipoEgresoCaja.SelectedValue;
                oEgresoCajaE.IdTipoEgresoCaja = idTipoEgresoCaja;
                cargarGrilla();
            }
        }

        private void nuevo_Click(object sender, EventArgs e)
        {
            FormLoginVendedor frmLogin = new FormLoginVendedor();
            frmLogin.ShowDialog(this);

            if (oUsuario == null) return;

            if (oUsuarioN.tienePermiso(oUsuario, "formAddOrEditEgresoCaja", DateTime.Today, oUsuario.Id))
            {
                formAddOrEditEgresoCaja frmAddOrEditEgresoCaja = new formAddOrEditEgresoCaja();
                frmAddOrEditEgresoCaja.oUsuario = oUsuario;
                frmAddOrEditEgresoCaja.asignarForm(this);
                frmAddOrEditEgresoCaja.Show();                 
            }
            else
            {
                MessageBox.Show("Debe agregar sus gastos desde la pantalla de Punto de Venta (POS).\n");
            }
            oUsuario = null;
        }

        private void btnCancelar_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnSeleccionar_Click(object sender, EventArgs e)
        {
            try
            {                
                int idEgresoCaja = Convert.ToInt32(grillaEgresosCaja.CurrentRow.Cells["id"].Value.ToString());
                bool formAbierto = false;
                foreach (Form frm in Application.OpenForms)
                {
                    if (frm.GetType() == typeof(formAddOrEditEgresoCaja))
                    {
                        foreach (Control ctrl in frm.Controls)
                        {
                            if (ctrl.Name.Equals("idEgresoCajaLabel") && ctrl.Text.Equals(idEgresoCaja.ToString())) //(oUsuario != null && ctrl.Name.Equals("usuario") && ctrl.Text.Equals(oUsuario.User))
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
                    formAddOrEditEgresoCaja frmAddOrEditEgresoCaja = new formAddOrEditEgresoCaja();
                    frmAddOrEditEgresoCaja.idEgresoCaja = idEgresoCaja;
                    frmAddOrEditEgresoCaja.asignarForm(this);
                    frmAddOrEditEgresoCaja.Show();
                }
                oUsuario = null;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public void EnviarUsuario(Entidades.Usuario usuario)
        {
            oUsuario = usuario;
        }

        private void txtDescripcion_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyValue.Equals(13))
            {
                cargarGrilla();
            }
        }

        private void txtDescripcion_TextChanged(object sender, EventArgs e)
        {
            lblActualizar.Visible = true;
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Escape)
            {
                this.Close();
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void tiposEgresos_Click(object sender, EventArgs e)
        {
            if (Application.OpenForms["formTiposEgresos"] != null)
            {

                Application.OpenForms["formTiposEgresos"].Activate();
                Application.OpenForms["formTiposEgresos"].WindowState = FormWindowState.Normal;

            }
            else
            {
                formTiposEgresos frmTiposEgresos = new formTiposEgresos();
                frmTiposEgresos.EnviarUsuario(oUsuario);
                frmTiposEgresos.Show();
            }
        }

        private void checkSoloGastos_CheckedChanged(object sender, EventArgs e)
        {
            buscarSoloGastos();
        }

        private void buscarSoloGastos()
        {
            string nombreCol = "Gasto";
            string consulta = "1 <> 1";
            if (checkSoloGastos.Checked)
                consulta += " OR " + nombreCol + " = true";
            else
            {
                consulta = "1 = 1";
            }

            arrayRowFilter[2] = consulta;
            aplicarRowFilter();
            cargarTotales();
        }

        private void aplicarRowFilter()
        {
            consultaRowFilter = "";

            for (int i = 0; i < arrayRowFilter.Length; i++)
            {
                string and = (i != arrayRowFilter.Length - 1) ? " AND " : "";
                consultaRowFilter += "( " + arrayRowFilter[i] + " )" + and;
            }

            (grillaEgresosCaja.DataSource as DataTable).DefaultView.RowFilter = string.Format(consultaRowFilter);
        }
    }
}
