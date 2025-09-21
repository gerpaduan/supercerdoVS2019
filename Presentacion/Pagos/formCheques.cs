using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;

using System.Configuration;
using Entidades;
using Presentacion.Embutidos;
using System.Web.UI.WebControls;
using System.Windows;

namespace Presentacion.Cheques
{
    public partial class formCheques : Form, InterfaceUsuario
    {
        Negocio.CuentaCorriente oCtaCteN = new Negocio.CuentaCorriente();
        protected Negocio.Usuario oUsuarioN = new Negocio.Usuario();
        Entidades.Pago oPagoE = new Entidades.Pago();
        Entidades.Persona oPersonaE;
        public Entidades.Usuario oUsuario;
        Entidades.Cheque oCheque;

        DataTable dtCheques = new DataTable();

        DataGridViewRow fila;
        string tramite;
        bool cargar = false;
        public bool llamadoDesdePago = false;
        public string nroChequeDesdePago = "";
        bool chequesVencidos = false;
        bool chequesPorVencer = false;
        public Action<string> OnChequeDobleClick { get; set; }



        public formCheques()
        {
            InitializeComponent(); this.Icon = Properties.Resources.CarniSys_ICONO;
        }

        public void EnviarUsuario(Entidades.Usuario usuario)
        {
            oUsuario = usuario;
        }

        private void btnBuscarCorte_Click(object sender, EventArgs e)
        {
            cargarGrilla();
        }

        private void btnSeleccionar_Click(object sender, EventArgs e)
        {

        }

        private void btnCancelar_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        #region Metodos
        public void cargarGrilla()
        {
            if (cargar)
            {

                if (!(FormPrincipal.logueado || oUsuarioN.tienePermiso(oUsuario, this.Name, txtFechaDesde.Value,
                        Utilidades.ValoresParametrosMetodos.IdCreadorNulo())))
                {
                    Utilidades.Mensajes.ErrorPermisoAcceso();
                    return;
                }

                string descripcion = txtBuscarNroCheque.Text.Trim();
                string estado = comboEstadosFiltro.Text.Trim().Equals("TODOS") ? "" : comboEstadosFiltro.Text.Trim();

                dtCheques = oCtaCteN.obtenerCheques(descripcion, txtFechaDesde.Value.Date, txtFechaHasta.Value.Date, checkPropioFiltro.Checked, estado);
                grilla.DataSource = null;
                grilla.DataSource = dtCheques;
                grilla.Columns["importe"].DefaultCellStyle.Format = "N2";
                grilla.Columns["propio"].Visible = false;
                grilla.Columns["recibidoDe"].Visible = false;
                grilla.Columns["entregadoA"].Visible = false;

                lblChequeVence.Visible = chequesVencidos || chequesPorVencer;
            }
        }

        private void modificarPago()
        {
            try
            {
                
            }
            catch (Exception)
            {
                throw;
            }           
        }

        private void eliminarPago()
        {
            cargarFilaSeleccionada();
            if (tramite == "Pago")
            {
                DialogResult resp = MessageBox.Show("Está seguro que desea eliminar el Pago?.", "Eliminar Pago", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
                if (resp == DialogResult.Yes)
                {
                    oPagoE.Id = Convert.ToInt32(fila.Cells["Id"].Value.ToString());
                    oPagoE = oCtaCteN.getPagoById(oPagoE.Id);

                    oCtaCteN.eliminarPago(oPagoE);
                    cargarGrilla();
                }
            }
            else
            {
                MessageBox.Show("Sólo se pueden eliminar Cheques. Asegúrese de seleccionar un Pago.");
            }            
        }

        private void cargarFilaSeleccionada()
        {
            if (grilla.CurrentRow != null && grilla.Rows.Count >0)
            {
                fila = grilla.CurrentRow;
            }
            else
            {
                MessageBox.Show("Asegurese de seleccionar una fila de la grilla.");
            }            
        }

        private void formatearGrilla()
        {
            if (dtCheques.Rows.Count > 0)
            {
                
            }

        }
        #endregion

        private void comboTipoTramite_TextChanged(object sender, EventArgs e)
        {
            cargarGrilla();
        }

        private void txtDescripcion_TextChanged(object sender, EventArgs e)
        {
            //cargarGrilla();
        }

        private void txtFechaDesde_ValueChanged(object sender, EventArgs e)
        {
            cargarGrilla();
        }

        private void txtFechaHasta_ValueChanged(object sender, EventArgs e)
        {
            cargarGrilla();
        }

        private void modificar_Click(object sender, EventArgs e)
        {
            modificarPago();
        }

        private void eliminar_Click(object sender, EventArgs e)
        {
            eliminarPago();
        }


        private void formCheques_Load(object sender, EventArgs e)
        {
            this.Text += Utilidades.Conexion.getSucursalConexion();
            txtTitular.CharacterCasing = CharacterCasing.Upper;
            comboEstadosFiltro.SelectedIndex = 0;
            txtFechaDesde.Value = DateTime.Now.AddDays(-30);
            txtFechaHasta.Value = DateTime.Now.AddMonths(12); 
            txtUsuario.Text = oUsuario.Nombre;
            groupChequeEstado(false || llamadoDesdePago);

            // Limpia y carga el combo igual que antes
            comboBanco.Items.Clear();
            comboBanco.Items.AddRange(oCtaCteN.getBancos().ToArray());

            cargar = true;
            cargarGrilla();
        }

        private void grillaCheques_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            formatearGrilla();
        }

        private void txtDescripcion_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar==Convert.ToChar(Keys.Enter))
            {
                cargarGrilla();
            }
        }

        private void btnNuevo_Click(object sender, EventArgs e)
        {
            NuevoCheque();
        }

        public void NuevoCheque()
        {
            //groupCheque.Enabled = true;
            groupChequeEstado(true);

            oCheque = new Cheque();
            LimpiarTextBoxes(this);
            comboEstado.SelectedIndex = 0;

            if (llamadoDesdePago)
            {
                txtNroCheque.Text = nroChequeDesdePago;
                comboBanco.Focus();
            }
        }

        private void btnGuardar_Click(object sender, EventArgs e)
        {
            addOrEditCheque();
        }

        private void addOrEditCheque()
        {
            try
            {
                if (!(oUsuarioN.tienePermiso(oUsuario, this.Name, DateTime.Today,
                           oCheque != null && oCheque.Id > 0 ? oCheque.CreadoPor.Id : oUsuario.Id)))
                {
                    Utilidades.Mensajes.ErrorPermisoEdicion();
                    return;
                }

                if (string.IsNullOrWhiteSpace(txtNroCheque.Text))
                {
                    MessageBox.Show("Debe ingresar el número de cheque.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtNroCheque.Focus();
                    return;
                }

                if (string.IsNullOrWhiteSpace(txtImporteCheque.Text))
                {
                    MessageBox.Show("Debe ingresar el importe del cheque.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtImporteCheque.Focus();
                    return;
                }


                if (!Utilidades.Util_Form.validarNumeroMayorACero(txtImporteCheque.Text,"Importe"))// !float.TryParse(txtImporteCheque.Text, out float importe))
                {
                    MessageBox.Show("El importe ingresado no es válido.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtImporteCheque.Focus();
                    return;
                }

                oCheque.NroCheque = txtNroCheque.Text.Trim();
                oCheque.Banco = comboBanco.Text.Trim();
                oCheque.Propio = checkPropio.Checked;
                oCheque.FechaEmision = txtFechaEmision.Text.Trim();
                oCheque.FechaPago = txtFechaPago.Value;
                oCheque.Importe = Utilidades.Util_Form.convertFloat(txtImporteCheque.Text,false);
                oCheque.Estado = comboEstado.Text.Trim();
                oCheque.Titular = txtTitular.Text.Trim();
                oCheque.Observaciones = txtObservaciones.Text.Trim();

                if (oCheque.Id == 0)
                {
                    oCheque.Creado = DateTime.Now;
                    oCheque.CreadoPor = oUsuario;
                }
                else
                {
                    oCheque.Actualizado = DateTime.Now;
                    oCheque.ActualizadoPor = oUsuario;
                }

                oCtaCteN.AddOrEditCheque(oCheque);

                MessageBox.Show("El Cheque se ha guardado correctamente.", "Guardar", MessageBoxButtons.OK, MessageBoxIcon.Information);

                if (llamadoDesdePago)
                {
                    this.Close();
                    return;
                }

                oCheque = null;
                LimpiarTextBoxes(this.groupCheque);
                cargarGrilla();
                groupChequeEstado(false);
            }
            catch (Exception)
            {

                throw;
            }
        }

        private void LimpiarTextBoxes(Control parent)
        {
            checkPropio.Checked = false;
            comboEstado.SelectedIndex = -1;
            comboBanco.SelectedIndex = -1;
            txtFechaEmision.Text = "";
            txtFechaPago.Value = DateTime.Now;
            foreach (Control control in parent.Controls)
            {
                if (control is System.Windows.Forms.TextBox txt)
                    txt.Text = "";
                else if (control.HasChildren)
                    LimpiarTextBoxes(control); // Recorre niveles anidados
            }
        }

        private void btnModificar_Click(object sender, EventArgs e)
        {
            CargarChequeSeleccionado();
            //groupCheque.Enabled = true;
            groupChequeEstado(true);
        }

        private void groupChequeEstado(bool estado)
        {
            foreach (Control c in groupCheque.Controls)
            {
                if (c != btnObservaciones)
                {
                    c.Enabled = estado;
                }
            }
        }

        private void CargarChequeSeleccionado()
        {
            try
            {
                if (grilla.CurrentRow != null)
                {
                    int idCheque = Convert.ToInt32(grilla.CurrentRow.Cells["id"].Value);

                    // Llamar a función que busca el cheque y carga los datos
                    oCheque = oCtaCteN.getChequePorIDorNro(idCheque, "");
                    CargarCheque();
                }

            }
            catch (Exception)
            {

                throw;
            }
        }

        private void CargarCheque()
        {
            if (oCheque != null)
            {
                txtNroCheque.Text = oCheque.NroCheque;
                comboBanco.Text  = oCheque.Banco;
                checkPropio.Checked = oCheque.Propio;
                txtFechaEmision.Text = oCheque.FechaEmision;
                txtFechaPago.Value = oCheque.FechaPago;
                txtImporteCheque.Text = oCheque.Importe.ToString("N2");
                comboEstado.Text = oCheque.Estado;
                txtTitular.Text = oCheque.Titular;
                txtObservaciones.Text = oCheque.Observaciones;
                txtRecibidoDe.Text  = oCheque.PagoDe != null ? oCheque.PagoDe.Persona.Identificacion :"";
                txtEntregadoA.Text = oCheque.PagoA != null ? oCheque.PagoA.Persona.Identificacion : "";

                // Podés guardar el ID en una variable global o en una propiedad oculta del formulario
                //txtIdCheque.Text = oCheque.Id.ToString(); // o usar una variable interna
            }
        }

        private void btnCancelar_Click_1(object sender, EventArgs e)
        {
            LimpiarTextBoxes(this);
            //groupCheque.Enabled = false;
            groupChequeEstado(false);
        }

        private void btnEliminar_Click(object sender, EventArgs e)
        {
            CargarChequeSeleccionado();
            try
            {
                if (!oUsuarioN.tienePermiso(FormPrincipal.oUserLogueado, this.Name, DateTime.Today,
                           oCheque != null && oCheque.Id > 0 ? oCheque.CreadoPor.Id : FormPrincipal.oUserLogueado.Id))
                {
                    Utilidades.Mensajes.ErrorPermisoEdicion();
                    return;
                }

                if ((oCheque.PagoA != null && oCheque.PagoA.Id > 0) || (oCheque.PagoDe != null && oCheque.PagoDe.Id > 0))
                {
                    string detallePagos = oCheque.PagoA != null && oCheque.PagoA.Id > 0 ? "Pago realizado a: " + oCheque.PagoA.Persona.Identificacion + " - Fecha: " + oCheque.PagoA.Fecha.ToString() + "\n" : "";
                    detallePagos += oCheque.PagoDe != null && oCheque.PagoDe.Id > 0 ? "Pago recibido de: " + oCheque.PagoDe.Persona.Identificacion + " - Fecha: " + oCheque.PagoDe.Fecha.ToString() : "";
                    MessageBox.Show("El Cheque seleccionado no puede eliminarse porque ha sido asígnado a los siguientes Pagos:\n\n" + detallePagos);

                    return;
                }

                DialogResult resp = MessageBox.Show("Está seguro que desea eliminar el Cheque N° "+ oCheque.NroCheque +"?.", "Eliminar Cheque", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
                if (resp == DialogResult.Yes)
                {
                    if (oCtaCteN.EliminarCheque(oCheque.Id))
                    {
                        MessageBox.Show("El Cheque " + oCheque.NroCheque + " ha sido eliminado");
                        cargarGrilla();
                        oCheque = null;
                        LimpiarTextBoxes(this);
                    }
                    else
                    {
                        MessageBox.Show("No se pudo eliminar el cheque.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            catch (Exception)
            {

                throw;
            }
        }


        private void btnBuscar_Click(object sender, EventArgs e)
        {
            cargarGrilla();
        }

        private void checkPropioFiltro_CheckedChanged(object sender, EventArgs e)
        {
            cargarGrilla();
        }

        private void grilla_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                if (llamadoDesdePago)
                {
                    string nroCheque = grilla.Rows[e.RowIndex].Cells["nroCheque"].Value.ToString();

                    // Llamar al método externo si está asignado
                    OnChequeDobleClick?.Invoke(nroCheque);

                    // Cerrar el form si es necesario
                    this.Close();
                }
                else
                {
                    CargarChequeSeleccionado();
                }
            }
        }

        private void grilla_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {

            if (grilla.Columns[e.ColumnIndex].Name == "fechaPago")
            {
                if (e.Value != null && DateTime.TryParse(e.Value.ToString(), out DateTime fecha))
                {
                    // Obtener el valor de la columna "estado" de la misma fila
                    var estado = grilla.Rows[e.RowIndex].Cells["estado"].Value?.ToString();

                    //Si fecha pago + 23 dias es a hoy, entonces pinta en rojo por vencido
                    if (fecha.Date.AddDays(23) < DateTime.Today && estado == Entidades.Cheque.EstadoEnum.PENDIENTE.ToString())
                    {
                        e.CellStyle.ForeColor = Color.Orange;
                        e.CellStyle.Font = new Font(grilla.Font, System.Drawing.FontStyle.Bold);
                        chequesPorVencer = true;
                    }
                    //Si fecha pago + 30 dias es a hoy, entonces pinta en rojo por vencido
                    if (fecha.Date.AddDays(30) < DateTime.Today && estado == Entidades.Cheque.EstadoEnum.PENDIENTE.ToString())
                    {
                        e.CellStyle.ForeColor = Color.OrangeRed;
                        e.CellStyle.Font = new Font(grilla.Font, System.Drawing.FontStyle.Bold);
                        chequesVencidos = true;
                        
                    }
                }
            }
        }

        private void btnObservaciones_Click(object sender, EventArgs e)
        {
            formReceta frmReceta = new formReceta(txtObservaciones.Text); // Pasar el texto actual
            frmReceta.editar = txtObservaciones.Enabled;
            frmReceta.observaciones = true;
            frmReceta.OnObservaciones = CargarObservaciones;
            frmReceta.ShowDialog();
        }

        public void CargarObservaciones(string obs)
        { 
            txtObservaciones.Text = obs;
        }
    }
}
