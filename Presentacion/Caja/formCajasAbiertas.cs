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
    public partial class formCajasAbiertas : Form, InterfaceUsuario
    {
        protected Negocio.CierreCaja oCierreN = new Negocio.CierreCaja();
        protected Negocio.Sucursal oSucursalN = new Negocio.Sucursal();

        protected Entidades.CierreCaja oCierreE = new Entidades.CierreCaja();
        protected Entidades.Sucursal oSucursalE = new Entidades.Sucursal();
        Entidades.Usuario oUsuario;

        protected enum tipoCierre { AbrirCaja, CerrarCaja };
        protected tipoCierre tipoCierreActual = tipoCierre.CerrarCaja;

        DataTable dtCajasAbiertas = null;

        public formCajasAbiertas()
        {
            InitializeComponent();
        }

        private void formCajasAbiertas_Load(object sender, EventArgs e)
        {
            //codigo para permitir la selección del checkBox
            grillaCajasAbiertas.ReadOnly = false;
            for (int col = 0; col < grillaCajasAbiertas.Columns.Count; col++)
            {
                grillaCajasAbiertas.Columns[col].ReadOnly = !grillaCajasAbiertas.Columns[col].Name.Equals("cajero");
            }
            this.Text += Utilidades.Conexion.getSucursalConexion();
            cargarSucursal();
            cargarGrilla();
        }

        private void cargarGrilla()
        {
            dtCajasAbiertas = oCierreN.findCierreCaja(oCierreE, Entidades.CierreCaja.tipoBusqueda.FindOpen, txtBuscar.Text);
            grillaCajasAbiertas.AutoGenerateColumns = false;
            grillaCajasAbiertas.DataSource = dtCajasAbiertas;
        }

        private void cargarSucursal()
        {
            int idSucursal = Utilidades.Conexion.getIdSucursalConexion();
            oSucursalE = oSucursalN.findById(idSucursal);
            oCierreE.Sucursal = oSucursalE;

            comboSucursal.DataSource = oSucursalN.obtenerSucursales();
            comboSucursal.DisplayMember = "sucursal";
            comboSucursal.ValueMember = "idSucursal";
            comboSucursal.SelectedIndex = idSucursal-1;
        }

        private void comboSucursal_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!comboSucursal.ValueMember.Equals(""))
            {
                int idSucursal = (int)comboSucursal.SelectedValue;
                oSucursalE = oSucursalN.findById(idSucursal);
                oCierreE.Sucursal = oSucursalE;
                cargarGrilla();
            }            
        }

        private void btnCancelar_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnBuscar_Click(object sender, EventArgs e)
        {
            cargarGrilla();
        }

        private void txtBuscar_TextChanged(object sender, EventArgs e)
        {
            cargarGrilla();
        }

        private void grillaCajasAbiertas_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex ==
                grillaCajasAbiertas.Columns["cajero"].Index)
            {
                for (int row = 0; row < grillaCajasAbiertas.Rows.Count; row++)
                {
                    grillaCajasAbiertas.Rows[row].Cells["cajero"].Value = false;
                }
                grillaCajasAbiertas.Rows[e.RowIndex].Cells["cajero"].Value = true;
                return;
            }

            // Ignore clicks that are not on button cells.  
            if (e.RowIndex < 0 || e.ColumnIndex !=
                grillaCajasAbiertas.Columns["CerrarCaja"].Index) return;

            // Retrieve the Employee object from the "Assigned To" cell.
            int? cierreCajaId = Convert.ToInt32(grillaCajasAbiertas.Rows[e.RowIndex].Cells["id"].Value.ToString());

            cerrarCajaVendedor(cierreCajaId);
        }

        private void cerrarCajaVendedor(int? cierreCajaId)
        {
            // Request status through the Employee object if present.  
            if (cierreCajaId != null)
            {
                FormLoginVendedor frmLogin = new FormLoginVendedor();
                frmLogin.ShowDialog(this);

                if (oUsuario != null)
                {
                    formCerrarCaja frmCerrarCaja = new formCerrarCaja();
                    frmCerrarCaja.oUserCierre = oUsuario;
                    frmCerrarCaja.oCierreE.Id = cierreCajaId.Value;
                    frmCerrarCaja.ShowDialog();
                    cargarGrilla();
                }
                oUsuario = null;
            }
        }

        public void EnviarUsuario(Entidades.Usuario usuario)
        {
            oUsuario = usuario;
        }

        private void grillaCajasAbiertas_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyValue.Equals(13))
            {
                e.SuppressKeyPress = true;
                int? cierreCajaId = Convert.ToInt32(grillaCajasAbiertas.SelectedRows[0].Cells["id"].Value.ToString());
                cerrarCajaVendedor(cierreCajaId);
            }
        }

        private void checkCajasMultiple_CheckedChanged(object sender, EventArgs e)
        {
            //MessageBox.Show("Para cerrar múltiples cajas debe seleccionar primero la caja que corresponde al cajero.");

            grillaCajasAbiertas.MultiSelect = checkCajasMultiple.Checked;
            btnCerrarMultipleCajas.Visible = checkCajasMultiple.Checked;
            grillaCajasAbiertas.Columns["cerrarCaja"].Visible = !checkCajasMultiple.Checked;
            grillaCajasAbiertas.Columns["cajero"].Visible = checkCajasMultiple.Checked;
        }

        private void btnCerrarMultipleCajas_Click(object sender, EventArgs e)
        {
            if (grillaCajasAbiertas.SelectedRows.Count > 1)
            {
                Entidades.CierreCaja oCierreCajero = new Entidades.CierreCaja();
                List<Entidades.CierreCaja> listaCerrarCaja = new List<Entidades.CierreCaja>();
                foreach (DataGridViewRow row in grillaCajasAbiertas.SelectedRows)
                {
                    Entidades.CierreCaja oCierreSelect = new Entidades.CierreCaja();
                    oCierreSelect.Id = Convert.ToInt32(row.Cells["Id"].Value);

                    DataGridViewCheckBoxCell cellSelecion = row.Cells["Cajero"] as DataGridViewCheckBoxCell;
                    if (Convert.ToBoolean(cellSelecion.Value))
                    {
                        oCierreCajero.Id = oCierreSelect.Id;
                        //seteo la fecha de apertura de caja para posterior validacion que sea la menor 
                        oCierreCajero.FechaHoraInicio = Convert.ToDateTime(row.Cells["fechaHoraInicio"].Value);
                    }

                    listaCerrarCaja.Add(oCierreSelect);
                }

                //se verifica que se haya seleccionado una caje
                if (oCierreCajero.Id.Equals(0))
                {
                    MessageBox.Show("Debe selecionar un cajero.", "Seleccionar cajero");
                    return;
                }

                //se verifica que la fecha de apertura del cajero sea la menor
                foreach (DataGridViewRow row in grillaCajasAbiertas.SelectedRows)
                {
                    if (Convert.ToDateTime(row.Cells["fechaHoraInicio"].Value) < oCierreCajero.FechaHoraInicio)
                    {
                        DialogResult resp = MessageBox.Show("Se recomienda que la fecha y hora de apertura de caja del cajero seleccionado sea la"+
                            " menor entre todos los vendedores seleccionados para evitar errores entre las cajas.\n\n"+
                        "Si está seguro presione 'Si' para continuar con el cierre de cajas.","Mensaje error",
                            MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
                        if(resp.Equals(DialogResult.No))return;
                        break;
                    }
                }

                FormLoginVendedor frmLogin = new FormLoginVendedor();
                frmLogin.ShowDialog(this);

                if (oUsuario != null)
                {
                    formCerrarCajaMultiple frmCerrarCajaMultiple = new formCerrarCajaMultiple();
                    frmCerrarCajaMultiple.oUserCierre = oUsuario;
                    frmCerrarCajaMultiple.oCierreCajero = oCierreCajero;
                    frmCerrarCajaMultiple.ListCierreE = listaCerrarCaja;
                    frmCerrarCajaMultiple.ShowDialog();
                    cargarGrilla();
                }
                oUsuario = null;
            }
            else
            {
                MessageBox.Show("Se deben seleccionar más de un vendedor para poder cerrar cajas múltiples","Cajas múltiples", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

    }
}
