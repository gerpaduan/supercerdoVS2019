using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Configuration;
using Utilidades;

namespace Presentacion.Caja
{
    public partial class formAddOrEditTipoEgreso : Form, InterfaceUsuario
    {
        public Entidades.Usuario oUsuario;

        Negocio.CierreCaja oCierreN = new Negocio.CierreCaja();
        DataTable dtTiposEgreso = new DataTable();

        public int idTipoEgreso = 0;
        bool readOnly = false;
        bool huboModificacion = false;
        public bool egresoDesdeCajaVenta = false;

        public formAddOrEditTipoEgreso()
        {
            InitializeComponent(); this.Icon = Properties.Resources.CarniSys_ICONO;
        }

        private void formAddOrEditTipoEgreso_Load(object sender, EventArgs e)
        {
            this.Text += Utilidades.Conexion.getSucursalConexion();
            try
            {
                bool closeForm = false;
                if (idTipoEgreso == 0 && oUsuario == null) closeForm = true;

                if (!closeForm)
                {
                    if (idTipoEgreso > 0)
                    {
                        dtTiposEgreso = oCierreN.obtenerTiposEgresoCaja("", idTipoEgreso);
                        cargarCampos();
                        idEgresoCajaLabel.Text = idTipoEgreso.ToString();//asigno id para identificar el formulario al llamar
                    }
                }
                else
                {
                    this.Close();
                }
                txtTipoEgresoCaja.Select();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error en evento Load()\n" + ex.Message);
            }
        }

        private void validarAperturaCaja()
        {
            Negocio.CierreCaja oCierreN = new Negocio.CierreCaja();
            Entidades.CierreCaja oCierreE = new Entidades.CierreCaja();
            //oCierreE.Sucursal = oSucursalE;
            //oCierreE.UsuarioInicio = oUsuario;
            //oCierreE = oCierreN.findByIdOrLast(oCierreE, Entidades.CierreCaja.tipoBusqueda.FindLast, "");
            //if (oCierreE == null || !oCierreE.UsuarioCierre.Id.Equals(0))
            //{
            //    MessageBox.Show(oUsuario.Nombre + ":\nDebes Abrir Caja para poder registrar gastos.", "Abrir Caja", MessageBoxButtons.OK, MessageBoxIcon.Information);
            //    oUsuario = null;
            //    if (idTipoEgreso == 0)this.Close();
            //}
        }

        public void EnviarUsuario(Entidades.Usuario usuario)
        {
            oUsuario = usuario;
        }

        private void cargarCampos()
        {
            //cargar campos en pantalla
            txtIdTipoEgreso.Text = dtTiposEgreso.Rows[0]["id"].ToString();
            txtTipoEgresoCaja.Text = dtTiposEgreso.Rows[0]["tipoEgresoCaja"].ToString();
            checkEsGasto.Checked = Convert.ToBoolean(dtTiposEgreso.Rows[0]["Es_Gasto"]);
        }

        public void asignarForm(formEgresosCaja form)
        {
            //frmEgresosCaja = form;
        }

        private void btnAceptar_Click(object sender, EventArgs e)
        {
            addOrEdit();         
        }

        private void addOrEdit()
        {
            try
            {
               
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al guardar tipo egreso de caja.\n\n" + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnCancelar_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Escape)
            {
                this.Close();
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void btnGuardar_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(txtTipoEgresoCaja.Text))
                {
                    MessageBox.Show("El campo Tipo no puede ser vacío.");
                    txtTipoEgresoCaja.Focus();  
                    return;
                }

                int id = string.IsNullOrEmpty(txtIdTipoEgreso.Text) ? 0 : idTipoEgreso;
                oCierreN.addOrEditTipoEgreso(id, txtTipoEgresoCaja.Text, checkEsGasto.Checked);
                MessageBox.Show("El Tipo Egreso se registró correctamente");

                ///Si es Nuevo registro se limpian campos
                ///sino se cierra la ventana
                if (id == -1)
                {
                    txtTipoEgresoCaja.Text = "";
                    checkEsGasto.Checked = false;
                    txtTipoEgresoCaja.Focus();
                }
                else
                {
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al guardar el Tipo Egreso", ex.Message);
            }
        }

        private void btnCancelar_Click_1(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
