using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Presentacion.Movimientos;

using Presentacion.Cortes;
using System.Configuration;

namespace Presentacion
{
    public partial class formMovimientos : formBaseColor
    {
        Negocio.Corte oCorteN = new Negocio.Corte(FormPrincipal.EmpresaSTATIC, FormPrincipal.ParametrosCTX);

        Entidades.Corte oCorteE = new Entidades.Corte();
        Entidades.Sucursal oSucursalOrigen = new Entidades.Sucursal();
        Entidades.Sucursal oSucursalDestino = new Entidades.Sucursal();
        Entidades.Movimiento oMovimientoE = new Entidades.Movimiento();

        DataTable dtMovimientos = new DataTable();
        DataTable dtSucursalOrigen = new DataTable();
        DataTable dtSucursalDestino = new DataTable();
 
        Negocio.Sucursal oSucursalN = new Negocio.Sucursal(FormPrincipal.EmpresaSTATIC, FormPrincipal.ParametrosCTX);
        Negocio.Usuario oUsuarioN = new Negocio.Usuario(FormPrincipal.EmpresaSTATIC, FormPrincipal.ParametrosCTX);

        int cantServidores = Convert.ToInt32(ConfigurationManager.AppSettings["cantServidores"].ToString());
        DateTime limitFechaDesde = DateTime.Today.AddDays(-FormPrincipal.ParametrosCTX.GetInt(Entidades.Parametros.DiasLimitFechaDesde, 0));
        DateTime ultimaFechaDesde; //guarda la ultima fecha de la busqueda exitosa

        bool cargar = false;

        public formMovimientos()
        {
            InitializeComponent(); this.Icon = Properties.Resources.CarniSys_ICONO;
        }
        
        private void formMovimientos_Load(object sender, EventArgs e)
        {
            try
            {
                this.Text += Utilidades.Conexion.getSucursalConexion();
                cargarSucursales();
                txtFechaDesde.Value = ultimaFechaDesde = DateTime.Today.AddDays(-FormPrincipal.ParametrosCTX.GetInt(Entidades.Parametros.DiasLimitFechaDesde, 0));
                cargar = true;
                cargarGrilla();
                actualizar.Visible = FormPrincipal.soyYo;
            }
            catch (Exception ex)
            {
                if (Utilidades.Util_Form.errorConexionBD_Return(ex.Message))
                    formMovimientos_Load(null, null);

                this.Close();
            }
        }

        private void cargarSucursales()
        {
            //Suc. Origen
            dtSucursalOrigen = oSucursalN.obtenerSucursalesConTodas();
            comboSucOrigen.DataSource = dtSucursalOrigen;
            comboSucOrigen.DisplayMember = "sucursal";
            comboSucOrigen.ValueMember = "idSucursal";
            comboSucOrigen.SelectedIndex = 0;//todas            

            //Suc. destino
            dtSucursalDestino = oSucursalN.obtenerSucursalesConTodas();
            comboSucDestino.DataSource = dtSucursalDestino;
            comboSucDestino.DisplayMember = "sucursal";
            comboSucDestino.ValueMember = "idSucursal";
            comboSucDestino.SelectedIndex = 0;//Todas
        }

        public void cargarGrilla()
        {
            try
            {
                if (cargar)
                {
                    if (txtFechaDesde.Value < limitFechaDesde && (FormPrincipal.oUserLogueado == null ||
                       !oUsuarioN.tienePermiso(FormPrincipal.oUserLogueado, this.Name, txtFechaDesde.Value,
                       Utilidades.ValoresParametrosMetodos.IdCreadorNulo())))
                    {
                        ///si ultimaFechaDesde es menor al limitFechaDesde significa que el usuario tiene permiso para fecha anterior
                        ///
                        if (ultimaFechaDesde < limitFechaDesde)
                        {
                            Utilidades.Mensajes.ErrorPermisoAcceso();
                            txtFechaDesde.Value = ultimaFechaDesde;
                            return;
                        }
                        else
                        {
                            MessageBox.Show("No tiene permiso para ingresar una fecha desde menor a " + limitFechaDesde.ToShortDateString());
                            txtFechaDesde.Value = limitFechaDesde;
                        }
                    }

                    grillaMovimientos.DataSource = null;

                    string sucOrigen, SucDestino;

                    sucOrigen = (Convert.ToInt32(comboSucOrigen.SelectedValue.ToString()) > 0) ? comboSucOrigen.Text : "";
                    SucDestino = (Convert.ToInt32(comboSucDestino.SelectedValue.ToString()) > 0) ? comboSucDestino.Text : "";

                    dtMovimientos = oCorteN.obtenerMovimientos(sucOrigen, SucDestino, txtFechaDesde.Value.Date, txtFechaHasta.Value.Date, txtDescripcion.Text.Trim());
                    grillaMovimientos.DataSource = dtMovimientos;
                    formatearGrilla();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void formatearGrilla()
        {
            grillaMovimientos.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
            grillaMovimientos.Columns["observaciones"].AutoSizeMode = DataGridViewAutoSizeColumnMode.ColumnHeader;

            //formato para columna de fechas
            grillaMovimientos.Columns["Fecha Movimiento"].DefaultCellStyle.Format = "dd/MM/yyyy HH:mm:ss";
            //grillaCompras.Columns["fechaCompra"].DefaultCellStyle.Format = "ddd dd MMM HH:mm:ss";
            grillaMovimientos.Columns["creado"].DefaultCellStyle.Format = "dd/MM/yyyy HH:mm:ss";
            grillaMovimientos.Columns["actualizado"].DefaultCellStyle.Format = "dd/MM/yyyy HH:mm:ss";

            grillaMovimientos.Columns["Id Origen"].Visible = cantServidores > 1;
            grillaMovimientos.Columns["Estado"].Visible = cantServidores > 1;
        }

        private void infoMovimiento()
        {
            try
            {
                int idMovimiento = Convert.ToInt32(grillaMovimientos.CurrentRow.Cells["Id Movimiento"].Value.ToString());
                bool formAbierto = false;
                foreach (Form frm in Application.OpenForms)
                {
                    if (frm.GetType() == typeof(formInfoMovimiento))
                    {
                        foreach (Control ctrl in frm.Controls)
                        {
                            if (ctrl.Name.Equals("idMovimientoLabel") && ctrl.Text.Equals(idMovimiento.ToString()))
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
                    formInfoMovimiento frmInfoMovimiento = new formInfoMovimiento();
                    frmInfoMovimiento.idMovimiento = idMovimiento;
                    frmInfoMovimiento.frmMovimiento = this;
                    frmInfoMovimiento.Show();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void nuevo_Click(object sender, EventArgs e)
        {
            try
            {
                int idMovimiento = 0;
                bool formAbierto = false;
                foreach (Form frm in Application.OpenForms)
                {
                    if (frm.GetType() == typeof(formNuevoMovimiento))
                    {
                        foreach (Control ctrl in frm.Controls)
                        {
                            if (ctrl.Name.Equals("idMovimientoLabel") && ctrl.Text.Equals(idMovimiento.ToString()))
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
                    formNuevoMovimiento frmNuevoMovimiento = new formNuevoMovimiento();
                    frmNuevoMovimiento.obtenerForm(this);
                    frmNuevoMovimiento.Show();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnBuscarCorte_Click(object sender, EventArgs e)
        {
            cargarGrilla();
        }

        private void btnSeleccionar_Click(object sender, EventArgs e)
        {
            infoMovimiento();
        }
        
        private void btnCancelar_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void grillaMovimientos_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            infoMovimiento();
        }

        private void comboSucOrigen_TextChanged(object sender, EventArgs e)
        {
            cargarGrilla();
        }

        private void comboSucDestino_TextChanged(object sender, EventArgs e)
        {
            cargarGrilla();
        }

        private void actualizar_Click(object sender, EventArgs e)
        {
            try
            {
                string ruta = ConfigurationManager.AppSettings["rutaActualizarMovimientos"].ToString();
                System.Diagnostics.Process.Start(ruta);
                cargarGrilla();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ocurrió un error al actualizar los movimientos.\n\n" + ex.Message);
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

        private void LineasMov_Click(object sender, EventArgs e)
        {
            if (Application.OpenForms["formLineasMov"] != null)
            {
                Application.OpenForms["formLineasMov"].Activate();
                Application.OpenForms["formLineasMov"].WindowState = FormWindowState.Normal;
            }
            else
            {
                formLineasMov frmLineasMov = new formLineasMov();
                frmLineasMov.Show();
            }
        }

        private void menuDuplicar_Click(object sender, EventArgs e)
        {
            formMovimientos frmMovimientosDuplicar = new formMovimientos();
            frmMovimientosDuplicar.Show();
        }     
    }
}
