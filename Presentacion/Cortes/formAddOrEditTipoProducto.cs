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

namespace Presentacion.Cortes
{
    public partial class formAddOrEditTipoProducto : Form, InterfaceUsuario
    {
        public Entidades.Usuario oUsuario;

        Negocio.Corte oCorteN = new Negocio.Corte();
        DataTable dtTipoProducto = new DataTable();

        public string tipoProductoSelected = "";
        public string ordenSelected = "";
        public bool esInsert = false;
        ToolTip toolTip = new ToolTip();
        bool readOnly = false;
        bool huboModificacion = false;
        public bool egresoDesdeCajaVenta = false;

        public formAddOrEditTipoProducto()
        {
            InitializeComponent(); this.Icon = Properties.Resources.CarniSys_ICONO;
        }

        private void formAddOrEditTipoProducto_Load(object sender, EventArgs e)
        {
            //validar que sea Admin
            if (!Usuarios.FormValidarPermiso.validarPermiso(this.Name))
            {
                this.Close();
            }

            this.Text += Utilidades.Conexion.getSucursalConexion();
            try
            {
                if (!esInsert)
                {
                    txtTipoProducto.Text = tipoProductoSelected;
                    txtOrden.Text = ordenSelected;
                    tipoProdLabel.Text = tipoProductoSelected.ToString();//asigno id para identificar el formulario al llamar
                }
                txtTipoProducto.Select();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error en evento Load()\n" + ex.Message);
            }
        }

        public void EnviarUsuario(Entidades.Usuario usuario)
        {
            oUsuario = usuario;
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
                if (string.IsNullOrEmpty(txtTipoProducto.Text))
                {
                    MessageBox.Show("El campo Tipo no puede ser vacío.");
                    txtTipoProducto.Focus();  
                    return;
                }

                if (!(int.TryParse(txtOrden.Text, out int ord) && (ord > 0)))
                {
                    MessageBox.Show("El campo Orden debe ser un numero entero mayor a Cero");
                    txtOrden.Focus();
                    return;
                }

                string mensaje = oCorteN.addOrEditTipoProducto(txtTipoProducto.Text, txtOrden.Text, esInsert, tipoProductoSelected);
                if (mensaje.Length > 0)
                {
                    MessageBox.Show(mensaje, "",MessageBoxButtons.OK,MessageBoxIcon.Error);
                    txtTipoProducto.Focus();
                    return;
                }

                MessageBox.Show("El Tipo Egreso se registró correctamente");

                ///Si es Nuevo registro se limpian campos
                ///sino se cierra la ventana
                if (esInsert)
                {
                    txtTipoProducto.Text = "";
                    txtOrden.Text = "100";
                    txtTipoProducto.Focus();
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

        private void txtTipoProducto_TextChanged(object sender, EventArgs e)
        {
            if (checkMayuscula.Checked)
            {
                // Guardar la posición actual del cursor
                int cursorPosition = txtTipoProducto.SelectionStart;

                // Convertir el texto a mayúsculas
                txtTipoProducto.Text = txtTipoProducto.Text.ToUpper();

                // Restaurar la posición del cursor
                txtTipoProducto.SelectionStart = cursorPosition;
            }
        }

        private void txtOrden_MouseEnter(object sender, EventArgs e)
        {
            toolTip.SetToolTip(txtOrden, "Orden que tendrá en la lista el Tipo Producto.\nPara igual número se ordenará alfabéticamente");
        }
    }
}
