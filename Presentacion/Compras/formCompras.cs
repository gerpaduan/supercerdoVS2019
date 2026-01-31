using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Presentacion.Caja;
using Presentacion.Compras;

namespace Presentacion
{
    public partial class formCompras : formBaseColor, InterfaceUsuario
    {
        private bool logueado = false;

        public bool Logueado
        {
            get { return logueado; }
            set { logueado = value; }
        }

        Negocio.Compra oCompraN;
        DataTable dtCompras = new DataTable();

        public DataTable dtSucursales;
        public Negocio.Sucursal oSucursalN = new Negocio.Sucursal(FormPrincipal.EmpresaSTATIC, FormPrincipal.ParametrosCTX);
        Entidades.Usuario oUsuario;
        Negocio.Usuario oUsuarioN = new Negocio.Usuario(FormPrincipal.EmpresaSTATIC, FormPrincipal.ParametrosCTX);  

        bool cargar = false;
        public formCompras()
        {
            InitializeComponent(); this.Icon = Properties.Resources.CarniSys_ICONO;
        }
        
        private void formCompras_Load(object sender, EventArgs e)
        {
            try
            {
                if (!oUsuarioN.tienePermiso(FormPrincipal.oUserLogueado, this.Name, DateTime.Today, Utilidades.ValoresParametrosMetodos.IdCreadorNulo()))
                {
                    Utilidades.Mensajes.ErrorPermisoAcceso();
                    this.Close();
                    return;
                }

                this.Text += Utilidades.Conexion.getSucursalConexion();
                cargarSucursal();
                this.comboTipoCompra.SelectedIndex = 0;
                this.comboTipoCompra.Enabled = FormPrincipal.soyYo;
                fechaDesde.Value = DateTime.Today.AddDays(0);                

                cargar = true;
                cargarGrilla();
            }
            catch (Exception ex)
            {
                if (Utilidades.Util_Form.errorConexionBD_Return(ex.Message))
                    formCompras_Load(null, null);

                this.Close();
            }
        }
      
        #region metodos

        public void cargarGrilla()
        {
            

            if (cargar)
            {
                int idSucCombo = 0;
                if (comboSucursal.SelectedValue != null)
	            {
                    idSucCombo = Convert.ToInt32(comboSucursal.SelectedValue);
	            }

                if (!oUsuarioN.tienePermiso(FormPrincipal.oUserLogueado, this.Name, fechaDesde.Value.Date, Utilidades.ValoresParametrosMetodos.IdCreadorNulo()))
                {
                    Utilidades.Mensajes.ErrorPermisoAcceso();
                    return;
                }

                oCompraN = new Negocio.Compra(FormPrincipal.EmpresaSTATIC, FormPrincipal.ParametrosCTX);

                grillaCompras.AutoGenerateColumns = false;

                dtCompras = null;
                dtCompras = oCompraN.obtenerCompras(idSucCombo, comboTipoCompra.Text, txtDescripcion.Text.Trim(), fechaDesde.Value.Date, fechaHasta.Value.Date, null);
                grillaCompras.DataSource = dtCompras;
                grillaCompras.Columns["tipoCompra"].Visible = FormPrincipal.soyYo;
                cargarTotales();
                oCompraN = null;
            }
        }

        private void cargarTotales()
        {
            float totalKg = 0, totalS = 0;
            int cantMedias = 0;
            foreach (DataRow fila in dtCompras.Rows)
            {
                cantMedias += string.IsNullOrEmpty(fila["cantMedias"].ToString()) ? 0 : Convert.ToInt32(fila["cantMedias"]);
                totalKg = totalKg + float.Parse(fila["cantKg"].ToString());
                totalS = totalS + float.Parse(fila["totalS"].ToString());
            }
            txtCantMedias.Text = cantMedias.ToString();
            txtTotalKgs.Text = totalKg.ToString("F3");
            txtTotalS.Text = totalS.ToString("F2");
        }

        private void modificarCompra()
        {
            try
            {
                int idCompra = Convert.ToInt32(grillaCompras.CurrentRow.Cells["idCompra"].Value.ToString());
                bool formAbierto = false;

                foreach (Form frm in Application.OpenForms)
                {
                    if (frm.GetType() == typeof(formModificarCompra))
                    {
                        foreach (Control ctrl in frm.Controls)
                        {
                            if (ctrl.Name.Equals("txtIdCompra") && ctrl.Text.Equals(idCompra.ToString()))
                            {
                                //Application.OpenForms["formModificarCompra"].Activate();
                                //Application.OpenForms["formModificarCompra"].WindowState = FormWindowState.Normal;
                                frm.BringToFront();
                                formAbierto = true;
                                break;
                            }
                        }
                    }
                }
                if (!formAbierto)
                {
                    formModificarCompra frmModificarCompra = new formModificarCompra();
                    frmModificarCompra.cargarParametros(this, idCompra);
                    frmModificarCompra.Show();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void nuevaCompra()
        {
            if (Application.OpenForms["formNuevaCompra"] != null)
            {

                Application.OpenForms["formNuevaCompra"].Activate();
                Application.OpenForms["formNuevaCompra"].WindowState = FormWindowState.Normal;
            }
            else
            {
                formNuevaCompra frmNuevaCompra = new formNuevaCompra();
                frmNuevaCompra.asignarFormCompra(this);
                frmNuevaCompra.oUsuario = oUsuario;
                frmNuevaCompra.Show();
            }
        }

        public void EnviarUsuario(Entidades.Usuario usuario)
        {
            oUsuario = usuario;
        }

        #endregion

        #region eventos

        private void btnCancelar_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void nuevo_Click(object sender, EventArgs e)
        {
            nuevaCompra();
        }

        private void fechaDesde_ValueChanged(object sender, EventArgs e)
        {
            cargarGrilla();
        }

        private void fechaHasta_ValueChanged(object sender, EventArgs e)
        {
            cargarGrilla();
        }
        private void txtDescripcion_TextChanged(object sender, EventArgs e)
        {
            cargarGrilla();
        }
        
        private void btnSeleccionar_Click_1(object sender, EventArgs e)
        {
            modificarCompra();
        }
        private void grillaCompras_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            modificarCompra();
        }
        #endregion

        private void btnBuscar_Click(object sender, EventArgs e)
        {
            cargarGrilla();
        }

        private void comboTipoCompra_SelectedValueChanged(object sender, EventArgs e)
        {
            cargarGrilla();
        }

        private void formCompras_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode==Keys.N)
            {
                nuevaCompra();
            }
        }

        private void comboSucursal_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!comboSucursal.ValueMember.Equals(""))
            {
                cargarGrilla();
            }           
        }

        private void cargarSucursal()
        {
            dtSucursales = new DataTable();
            oSucursalN = new Negocio.Sucursal(FormPrincipal.EmpresaSTATIC, FormPrincipal.ParametrosCTX);
            dtSucursales = oSucursalN.obtenerSucursalesConTodas();

            comboSucursal.DataSource = dtSucursales;
            comboSucursal.DisplayMember = "sucursal";
            comboSucursal.ValueMember = "idSucursal";
            comboSucursal.SelectedValue = -1;
        }

        private void menuDuplicar_Click(object sender, EventArgs e)
        {
            formCompras frmCompras = new formCompras();
            frmCompras.Show();
        }

        private void LineasCompras_Click(object sender, EventArgs e)
        {
            if (FormPrincipal.logueado)
            {
                if (Application.OpenForms["formLineasCompras"] != null)
                {
                    Application.OpenForms["formLineasCompras"].Activate();
                    Application.OpenForms["formLineasCompras"].WindowState = FormWindowState.Normal;

                }
                else
                {
                    formLineasCompras frmLineasCompras = new formLineasCompras();
                    frmLineasCompras.Show();
                }
            }
            else
            {
                MessageBox.Show("No está logueado");
            }
        }
    }
}
