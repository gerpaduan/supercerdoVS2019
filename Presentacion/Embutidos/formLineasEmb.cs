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

namespace Presentacion
{
    public partial class formLineasEmb : Form, InterfaceUsuario
    {
        bool esVentaClientes=false;

        public bool EsVentaClientes
        {
            get { return esVentaClientes; }
            set { esVentaClientes = value; }
        }
        Negocio.Corte oCorteN = new Negocio.Corte(FormPrincipal.EmpresaSTATIC, FormPrincipal.ParametrosCTX);
        DataTable dtEmbutidos = new DataTable();

        DataTable dtSucursales;
        Negocio.Sucursal oSucursalN = new Negocio.Sucursal(FormPrincipal.EmpresaSTATIC, FormPrincipal.ParametrosCTX);
        Negocio.Usuario oUsuarioN = new Negocio.Usuario(FormPrincipal.EmpresaSTATIC, FormPrincipal.ParametrosCTX);

        Entidades.Usuario oUsuario;

        int cantDiasLimitFechaDesde = FormPrincipal.ParametrosCTX.GetInt(Entidades.ParamKeys.DiasLimitFechaDesde, 0);
        DateTime limitFechaDesde;
        bool cargar = false;

        public formLineasEmb()
        {
            InitializeComponent(); this.Icon = Properties.Resources.CarniSys_ICONO;        
        }

        private void formLineasEmb_Load(object sender, EventArgs e)
        {
            try
            {
                this.Text += Utilidades.Conexion.getSucursalConexion();
                if (EsVentaClientes)
                {
                    this.Text = "Embutidos/Ventas Clientes/Otros";
                }
                DateTime today = DateTime.Today;
                fechaHasta.Value = today.AddDays(1).AddSeconds(-1);
                limitFechaDesde = today.AddDays(-cantDiasLimitFechaDesde);
                fechaDesde.Value = limitFechaDesde;
                cargarSucursal();
                cargar = true;
                cargarGrilla();
            }
            catch (Exception ex)
            {
                if (Utilidades.Util_Form.errorConexionBD_Return(ex.Message))
                    formLineasEmb_Load(null, null);

                this.Close();
            }

        }

        public void cargarGrilla()
        {
            if (cargar)
            {
                try
                {
                    dtEmbutidos = null;
                    grillaEmbutidos.DataSource = null;
                    grillaEmbutidos.AutoGenerateColumns = true;
                    dtEmbutidos = oCorteN.obtenerLineasEmb(Convert.ToInt32(comboSucursal.SelectedValue), txtDescripcion.Text.Trim(), fechaDesde.Value, fechaHasta.Value);
                    grillaEmbutidos.DataSource = dtEmbutidos;

                    formatearGrilla();
                    cargarCampoTotal();
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
            for (int index = 0; index < grillaEmbutidos.Rows.Count; index++)
            {
                if (!string.IsNullOrEmpty(grillaEmbutidos["Estado", index].Value.ToString().ToString()))
                {
                    grillaEmbutidos.Rows[index].DefaultCellStyle.BackColor = Color.SandyBrown;
                }
            }
            grillaEmbutidos.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;

            grillaEmbutidos.Columns["Cod.Emb"].HeaderText = "Cod.Elab";
            grillaEmbutidos.Columns["Embutido"].HeaderText = "Elaborado";
            grillaEmbutidos.Columns["Kgs"].DefaultCellStyle.Format = "F3";
            grillaEmbutidos.Columns["Kgs"].HeaderText = "Cant";
            grillaEmbutidos.Columns["Corte"].HeaderText = "Ingrediente";
            //formato para columna de fechas
            grillaEmbutidos.Columns["Fecha"].DefaultCellStyle.Format = "dd/MM/yyyy HH:mm:ss";
            grillaEmbutidos.Columns["Creado"].DefaultCellStyle.Format = "dd/MM/yyyy HH:mm:ss";
            grillaEmbutidos.Columns["Actualizado"].DefaultCellStyle.Format = "dd/MM/yyyy HH:mm:ss";
        }

        private void cargarCampoTotal()
        {
            float totalKg = 0;
            foreach (DataRow fila in dtEmbutidos.Rows)
            {
                if (fila["Estado"] == null || fila["Estado"].ToString() == "")
                {
                    totalKg = totalKg + float.Parse(fila["Kgs"].ToString());
                }
            }
            txtTotalKg.Text = Convert.ToString(totalKg);        
        }

        private void informacionEmbutido()
        {
            if (Application.OpenForms["formInfoEmbutido"] != null)
            {
                Application.OpenForms["formInfoEmbutido"].Activate();
                Application.OpenForms["formInfoEmbutido"].WindowState = FormWindowState.Normal;
            }
            else
            {
                formInfoEmbutido frmInfoEmbutido = new formInfoEmbutido();
                //frmInfoEmbutido.frmEmbutidos = this;
                frmInfoEmbutido.idEmbutido_ = Convert.ToInt32(grillaEmbutidos.CurrentRow.Cells["Id"].Value.ToString());
                frmInfoEmbutido.Show();
            }            
        }

        public void EnviarUsuario(Entidades.Usuario usuario)
        {
            oUsuario = usuario;
        }

        private void cargarSucursal()
        {
            oSucursalN = new Negocio.Sucursal(FormPrincipal.EmpresaSTATIC, FormPrincipal.ParametrosCTX);
            dtSucursales = oSucursalN.obtenerSucursalesConTodas();
            comboSucursal.DataSource = dtSucursales;
            comboSucursal.DisplayMember = "sucursal";
            comboSucursal.ValueMember = "idSucursal";
            int idSucursalActual = FormPrincipal.idSucursal; // o Conexion.getIdSucursalConexion()

            // Seleccionar por valor (no por índice)
            comboSucursal.SelectedValue = idSucursalActual;

            // Si no existe en la lista, dejar vacío
            if (comboSucursal.SelectedValue == null ||
                Convert.ToInt32(comboSucursal.SelectedValue) != idSucursalActual)
            {
                comboSucursal.SelectedIndex = -1;
            }
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
            if (fechaDesde.Value < limitFechaDesde && (FormPrincipal.oUserLogueado == null ||
                !oUsuarioN.tienePermiso(FormPrincipal.oUserLogueado, this.Name, fechaDesde.Value,
                Utilidades.ValoresParametrosMetodos.IdCreadorNulo())))
            {
                MessageBox.Show("No tiene permiso para ingresar una fecha desde menor a " + limitFechaDesde.ToShortDateString());
                fechaDesde.Value = limitFechaDesde;
            }
            lblActualizar.Visible = true;
        }
    }
}
