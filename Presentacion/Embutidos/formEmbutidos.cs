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

namespace Presentacion
{
    public partial class formEmbutidos : Form, InterfaceUsuario
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
        DateTime ultimaFechaDesde; //guarda la ultima fecha de la busqueda exitosa
        bool cargar = false;
        public formEmbutidos()
        {
            InitializeComponent(); this.Icon = Properties.Resources.CarniSys_ICONO;        
        }

        public void cargarGrilla()
        {
            if (cargar)
            {
                try
                {
                    if (fechaDesde.Value < limitFechaDesde && (FormPrincipal.oUserLogueado == null ||
                       !oUsuarioN.tienePermiso(FormPrincipal.oUserLogueado, this.Name, fechaDesde.Value,
                       Utilidades.ValoresParametrosMetodos.IdCreadorNulo())))
                    {
                        ///si ultimaFechaDesde es menor al limitFechaDesde significa que el usuario tiene permiso para fecha anterior
                        ///
                        if (ultimaFechaDesde < limitFechaDesde)
                        {
                            Utilidades.Mensajes.ErrorPermisoAcceso();
                            fechaDesde.Value = ultimaFechaDesde;
                            return;
                        }
                        else
                        {
                            MessageBox.Show("No tiene permiso para ingresar una fecha desde menor a " + limitFechaDesde.ToShortDateString());
                            fechaDesde.Value = limitFechaDesde;
                        }
                    }

                    dtEmbutidos = null;
                    grillaEmbutidos.DataSource = null;
                    grillaEmbutidos.AutoGenerateColumns = true;
                    dtEmbutidos = oCorteN.buscarEmbutido(Convert.ToInt32(comboSucursal.SelectedValue), txtDescripcion.Text.Trim(), fechaDesde.Value, fechaHasta.Value);
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
                else if (grillaEmbutidos["Observaciones", index].Value.ToString().ToLower().Contains("desarme") &&
                    grillaEmbutidos["Kgs", index].Value.ToString().Contains("-"))
                {
                    grillaEmbutidos.Rows[index].DefaultCellStyle.BackColor = Color.LightBlue;
                }
            }
            grillaEmbutidos.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;

            grillaEmbutidos.Columns["Kgs"].DefaultCellStyle.Format = "F3";
            grillaEmbutidos.Columns["Kgs"].HeaderText = "Cant.";
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
                frmInfoEmbutido.frmEmbutidos = this;
                frmInfoEmbutido.idEmbutido_ = Convert.ToInt32(grillaEmbutidos.CurrentRow.Cells["Id"].Value.ToString());
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
                frmIngresoEmbutido.oUsuario = oUsuario;
                frmIngresoEmbutido.frmEmbutidos = this;
                frmIngresoEmbutido.Show();
            }
            oUsuario = null;
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

        private void formEmbutidos_Load(object sender, EventArgs e)
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
                fechaDesde.Value = ultimaFechaDesde = limitFechaDesde;
                cargarSucursal();
                cargar = true;
                cargarGrilla();  
            }
            catch (Exception ex)
            {
                if (Utilidades.Util_Form.errorConexionBD_Return(ex.Message))
                    formEmbutidos_Load(null, null);

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
            if (fechaDesde.Value < limitFechaDesde &&  (FormPrincipal.oUserLogueado == null || 
                !oUsuarioN.tienePermiso(FormPrincipal.oUserLogueado, this.Name, fechaDesde.Value,
                Utilidades.ValoresParametrosMetodos.IdCreadorNulo())))
            {
                MessageBox.Show("No tiene permiso para ingresar una fecha desde menor a " + limitFechaDesde.ToShortDateString());
                fechaDesde.Value = limitFechaDesde;
            }
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
            ingresoRapido();
        }

        private void ingresoRapido(bool esDesarmeElaborado = false)
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
                frmElegirEmbutido.esDesarmeElaborado = esDesarmeElaborado;
                frmElegirEmbutido.frmEmbutidos = this;
                frmElegirEmbutido.Show();
            }
            oUsuario = null;
        }

        private void formulas_Click(object sender, EventArgs e)
        {
            if (Application.OpenForms["formFormulas"] != null)
            {
                Application.OpenForms["formFormulas"].Activate();
                Application.OpenForms["formFormulas"].WindowState = FormWindowState.Normal;
            }
            else
            {
                if (FormPrincipal.oUserLogueado == null)
                {
                    Utilidades.Mensajes.MensajeInicioSesion();
                    return;
                }

                formFormulas frmmFormulas = new formFormulas();
                frmmFormulas.Show();
            }
        }

        private void desarmeElaborado_Click(object sender, EventArgs e)
        {
            ingresoRapido(true);
        }
    }
}
