using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Windows.Forms;
using Utilidades;
using Presentacion.Ventas;

namespace Presentacion.Caja
{
    public partial class formCerrarCajaMultiple : Form
    {
        protected Negocio.CierreCaja oCierreN = new Negocio.CierreCaja();
        protected Negocio.Sucursal oSucursalN = new Negocio.Sucursal();

        public Entidades.CierreCaja oCierreCajero = new Entidades.CierreCaja();
        public List<Entidades.CierreCaja> ListCierreE = new List<Entidades.CierreCaja>();
        protected Entidades.Sucursal oSucursalE = new Entidades.Sucursal();
        public Entidades.Usuario oUserCierre = new Entidades.Usuario();
        Entidades.CierreCaja oCierreAnterior;               

        protected enum tipoCierre { AbrirCaja, CerrarCaja };
        protected tipoCierre tipoCierreActual = tipoCierre.CerrarCaja;

        public formCerrarCajaMultiple()
        {
            InitializeComponent();
        }

        private void formCerrarCajaMultiple_Load(object sender, EventArgs e)
        {
            this.Text += Utilidades.Conexion.getSucursalConexion();
            int idSucursal = Utilidades.Conexion.getIdSucursalConexion();
            oSucursalE = oSucursalN.findById(idSucursal);
            grillaCajasACerrar.AutoGenerateColumns = false;
            grillaCajasACerrar.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells;
            grillaCajasACerrar.DataSource = oCierreN.findCierreCajaMultiples(ListCierreE);
            validarAperturaForm();
            txtSucursal.Text = oSucursalE.sucursal;
            txtFechaHoraCierre.Text = DateTime.Now.ToString();
        }

        private void btnCerrarCaja_Click(object sender, EventArgs e)
        {
            cargarCierreCaja();
        }

        protected void cargarCierreCaja()
        {
            try
            {
                if (validaciones())
                {
                    for (int index = 0; index < ListCierreE.Count; index++)
                    {
                        ListCierreE[index].UsuarioCierre = oUserCierre;
                        ListCierreE[index].FechaHoraCierre = string.IsNullOrEmpty(txtFechaHoraCierre.Text) ? 
                            (DateTime?)null : Convert.ToDateTime(txtFechaHoraCierre.Text.ToString());

                        if (ListCierreE[index].Id.Equals(oCierreCajero.Id))
                        {
                            ListCierreE[index].CajaInicio = Util_Form.convertFloat(txtCajaInicial.Text, true);
                            ListCierreE[index].Ventas = string.IsNullOrEmpty(txtVentas.Text) ? (float?)null : Util_Form.convertFloat(txtVentas.Text, true);
                            ListCierreE[index].EgresosCaja = string.IsNullOrEmpty(txtEgresosCaja.Text) ? (float?)null : Util_Form.convertFloat(txtEgresosCaja.Text, true);
                            ListCierreE[index].CajaCierre = string.IsNullOrEmpty(txtCajaCierre.Text) ? (float?)null : Util_Form.convertFloat(txtCajaCierre.Text, true);
                            ListCierreE[index].Diferencia = string.IsNullOrEmpty(txtDiferencia.Text) ? (float?)null : Util_Form.convertFloat(txtDiferencia.Text, true);
                            ListCierreE[index].CajaInicioSiguiente = string.IsNullOrEmpty(txtCajaInicioSiguiente.Text) ? (float?)null : Util_Form.convertFloat(txtCajaInicioSiguiente.Text, true);
                            ListCierreE[index].ImporteRetirado = string.IsNullOrEmpty(txtImporteRetirado.Text) ? (float?)null : Util_Form.convertFloat(txtImporteRetirado.Text, true);
                            //se actualiza los datos del cajero
                            oCierreCajero = ListCierreE[index];
                        }
                    }

                    DialogResult respuesta = MessageBox.Show("¿Está seguro que desea cerrar caja?."
                                , "Cerrar Caja", MessageBoxButtons.YesNo, MessageBoxIcon.Question,
                                MessageBoxDefaultButton.Button2);

                    grillaCajasACerrar.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells;
                    if (respuesta == DialogResult.Yes)
                    {
                        for (int index = 0; index < ListCierreE.Count; index++)
                        {
                            oCierreN.addOrEditCierreCaja(ListCierreE[index]);
                        }

                        //imprimir ticket
                        Ticket.CreaTicket ticket = new Ticket.CreaTicket();
                        ticket.imprimir = checkTicket.Checked;
                        ticket.TextoCentro("Cierre Caja");
                        ticket.TextoCentro("--Copia Cajero--");
                        ticket.LineasEnBlanco(1);
                        //ticket.TextoIzquierda("123456789*123456789*123456789*123456789*123456789*");
                        ticket.TextoIzquierda("Vendedor: " + oCierreCajero.UsuarioInicio.Nombre);
                        ticket.TextoIzquierda("Desde: " + oCierreCajero.FechaHoraInicio.Value.ToString());
                        ticket.TextoIzquierda("Hasta: " + oCierreCajero.FechaHoraCierre.Value.ToString());
                        ticket.LineasGuion();
                        ticket.AgregaTotales("Caja Inicial", Convert.ToDouble(oCierreCajero.CajaInicio));
                        ticket.LineasEnBlanco(1);
                        ticket.AgregaTotales("Diferencia", Convert.ToDouble(oCierreCajero.Diferencia));
                        ticket.AgregaTotales("Queda en Caja:", Convert.ToDouble(oCierreCajero.CajaInicioSiguiente));
                        ticket.LineasEnBlanco(3);
                        ticket.realizarImpresion();

                        if (MessageBox.Show("¿Imprimir copia para administrador?", "",
                            MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1).Equals(DialogResult.Yes))
                        {
                            ticket.TextoCentro("Cierre Caja");
                            ticket.TextoCentro("--Copia Admin--");
                            ticket.LineasEnBlanco(1);
                            //ticket.TextoIzquierda("123456789*123456789*123456789*123456789*123456789*");
                            ticket.TextoIzquierda("Vendedor: " + oCierreCajero.UsuarioInicio.Nombre);
                            ticket.TextoIzquierda("Desde: " + oCierreCajero.FechaHoraInicio.Value.ToString());
                            ticket.TextoIzquierda("Hasta: " + oCierreCajero.FechaHoraCierre.Value.ToString());
                            ticket.LineasGuion();
                            ticket.AgregaTotales("Caja Inicial", Convert.ToDouble(oCierreCajero.CajaInicio));
                            ticket.AgregaTotales("Ventas", Convert.ToDouble(oCierreCajero.Ventas));
                            ticket.AgregaTotales("EgresosCaja", Convert.ToDouble(oCierreCajero.EgresosCaja));
                            ticket.AgregaTotales("Caja Cierre", Convert.ToDouble(oCierreCajero.CajaCierre));
                            ticket.AgregaTotales("Diferencia", Convert.ToDouble(oCierreCajero.Diferencia));
                            ticket.AgregaTotales("Prox. Caja", Convert.ToDouble(oCierreCajero.CajaInicioSiguiente));
                            ticket.AgregaTotales("Retira", Convert.ToDouble(oCierreCajero.ImporteRetirado));
                            ticket.LineasEnBlanco(3);
                            ticket.realizarImpresion();
                        }

                        this.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Exception cargarCierreCaja");
            }
            
        }

        protected bool validaciones()
        {
            bool resp = true;

            if (oSucursalE == null || txtCajaInicial.Text == "" )
            {
                resp = false;
                string mensaje = "Se deben completar los siguientes campos\n";
                if (oSucursalE == null)
                {                    
                    mensaje += "\n- Sucursal";
                }
                if (txtCajaInicial.Text == "")
                {
                    mensaje += "\n- Caja Inicial";
                }
                if (tipoCierreActual.Equals(tipoCierre.CerrarCaja))
                {
                    mensaje += "\n\nSi no puede completar los mensajes comuníquese con el adminitrador del sistema";
                }
                MessageBox.Show(mensaje);
            }
            if (resp && tipoCierreActual.Equals(tipoCierre.CerrarCaja))
            {
                int nroFilas = 0;
                int nombreTextBox = 1;
                int valorTextBox = 0;
                //string[valor_campo][nombre_textBox]
                string[,] textBoxes = new string[5, 2];
                textBoxes[nroFilas, valorTextBox] = txtUserCierre.Text;
                textBoxes[nroFilas++, nombreTextBox] = lblUsuarioCierre.Text;

                textBoxes[nroFilas, valorTextBox] = txtCajaInicial.Text;
                textBoxes[nroFilas++, nombreTextBox] = lblCajaInicial.Text;

                textBoxes[nroFilas, valorTextBox] = txtVentas.Text;
                textBoxes[nroFilas++, nombreTextBox] = lblVentas.Text;

                textBoxes[nroFilas, valorTextBox] = txtEgresosCaja.Text;
                textBoxes[nroFilas++, nombreTextBox] = lblEgresosCaja.Text;

                textBoxes[nroFilas, valorTextBox] = txtCajaCierre.Text;
                textBoxes[nroFilas++, nombreTextBox] = lblCajaCierre.Text;

                if (!Utilidades.Util_Form.validarArrayCamposVacios(textBoxes))
                {
                    resp = false;
                }
            }
            return resp;
        }

        private void calcularCierreCaja()
        {
            float cero = Util_Form.convertFloat("0", true),
            cajaInicial = txtCajaInicial.Text.Equals("") ? cero : Util_Form.convertFloat(txtCajaInicial.Text, true),
            ventas = txtVentas.Text.Equals("") ? cero : Util_Form.convertFloat(txtVentas.Text, true),
            gastos = txtEgresosCaja.Text.Equals("") ? cero : Util_Form.convertFloat(txtEgresosCaja.Text, true),
            cajaCierre = txtCajaCierre.Text.Equals("") ? cero : Util_Form.convertFloat(txtCajaCierre.Text, true),
            cajaInicioSiguiente = txtCajaInicioSiguiente.Text.Equals("") ? cero : Util_Form.convertFloat(txtCajaInicioSiguiente.Text, true),
            importeRetirado = txtImporteRetirado.Text.Equals("") ? cero : Util_Form.convertFloat(txtImporteRetirado.Text, true),
            diferencia = 0;

            diferencia = (gastos + cajaCierre) - (cajaInicial + ventas);
            oCierreCajero.Diferencia = diferencia;
            txtDiferencia.Text = txtCajaCierre.Text.Length > 0 ? diferencia.ToString("F2") : "";
            if (txtCajaInicioSiguiente.ReadOnly)
            {
                txtCajaInicioSiguiente.Text = (cajaCierre - importeRetirado).ToString();
            }
            else
            {
                importeRetirado = cajaCierre - cajaInicioSiguiente;
                txtImporteRetirado.Text = importeRetirado.ToString();
            }   
        }

        private void txtCajaInicial_TextChanged(object sender, EventArgs e)
        {
            if (!(txtCajaInicial.Text != "" && Utilidades.Util_Form.validarCampoNumerico(txtCajaInicial.Text, "Caja Inicial")))
            {
                txtCajaInicial.Text = "";
            }
            calcularCierreCaja();
        }

        private void txtVentas_TextChanged(object sender, EventArgs e)
        {
            if (!(txtVentas.Text != "" && Utilidades.Util_Form.validarCampoNumerico(txtVentas.Text, "Ventas")))
            {
                txtVentas.Text = "";
            }
            calcularCierreCaja();
        }

        private void txtEgresosCaja_TextChanged(object sender, EventArgs e)
        {
            if (!(txtEgresosCaja.Text != "" && Utilidades.Util_Form.validarCampoNumerico(txtEgresosCaja.Text, "EgresosCaja")))
            {
                txtEgresosCaja.Text = "";
            }
            calcularCierreCaja();
        }

        private void txtCajaCierre_TextChanged(object sender, EventArgs e)
        {
            if (!(txtCajaCierre.Text != "" && Utilidades.Util_Form.validarCampoNumerico(txtCajaCierre.Text, "Caja Cierre")))
            {
                txtCajaCierre.Text = "";
            }
            calcularCierreCaja();
        }

        private void txtCajaInicioSiguiente_TextChanged(object sender, EventArgs e)
        {
            if (!txtCajaInicioSiguiente.ReadOnly)
            {
                if (!(txtCajaInicioSiguiente.Text != "" && Utilidades.Util_Form.validarCampoNumerico(txtCajaInicioSiguiente.Text, "Caja")))
                {
                    txtCajaInicioSiguiente.Text = "";
                }
                calcularCierreCaja();                
            }
        }

        private void txtImporteRetirado_TextChanged(object sender, EventArgs e)
        {
            if (!txtImporteRetirado.ReadOnly)
            {
                if (!(txtImporteRetirado.Text != "" && Utilidades.Util_Form.validarCampoNumerico(txtImporteRetirado.Text, "Importe a Retirar")))
                {
                    txtImporteRetirado.Text = "";
                }
                calcularCierreCaja();                
            }
        }

        private void validarAperturaForm()
        {
            try
            {
                txtCajaInicial.ReadOnly = true;
                txtCajaInicial.TabStop = false;
                txtCajaInicial.BackColor = SystemColors.ScrollBar;
                checkTicket.Visible = true;
                checkTicket.Checked = true;
                controlEleccionImporte.Value = 1;

                oCierreCajero = oCierreN.findByIdOrLast(oCierreCajero, Entidades.CierreCaja.tipoBusqueda.FindById, "");
                oCierreAnterior = oCierreN.findByIdOrLast(oCierreCajero, Entidades.CierreCaja.tipoBusqueda.FindLastOpen, "");
                lblDiferenciaEntreCaja.Visible = oCierreAnterior != null && !oCierreAnterior.CajaInicioSiguiente.Equals(oCierreCajero.CajaInicio);

                if (!oUserCierre.Admin)
                {
                    MessageBox.Show(oUserCierre.Nombre + "\nNo tienes permiso para los cierres de caja.","Cerrar Caja", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.Close();
                }
                if (!oCierreCajero.FechaHoraCierre.Equals(null))
                {
                    MessageBox.Show("No puede Cerrar Caja porque no se ha iniciado caja anteriormente.\n" + "Fecha Ultimo Cierre: " + oCierreCajero.FechaHoraCierre.ToString(),
                        "Cerrar Caja", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.Close();
                }

                foreach (Form frm in Application.OpenForms)
                {
                    if (frm.GetType() == typeof(formVentaCaja))
                    {
                        foreach (Control ctrl in frm.Controls)
                        {
                            if (ctrl.Name.Equals("usuario") && ctrl.Text.Equals(oCierreCajero.UsuarioInicio.User))
                            {
                                MessageBox.Show("No puedes cerrar la caja de "+ oCierreCajero.UsuarioInicio.Nombre +" porque tiene una venta en curso." +
                                    "\n\nCierre la pantalla de ventas correspondiente al vendedor e intente cerrar caja nuevamente",
                                    "Cerrar Caja", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                this.Close();
                                break;
                            }
                        }
                    }
                }
                txtUserCierre.Text = oUserCierre.Nombre;
                txtFechaHoraCierre.Text = oCierreCajero.FechaHoraCierre.ToString();
                txtCajaInicial.Text = oCierreCajero.CajaInicio.ToString();

                decimal totalventas = 0;
                decimal totalEgresosCaja = 0;

                for (int index = 0; index < ListCierreE.Count; index++)
                {
                    ListCierreE[index] = oCierreN.findByIdOrLast(ListCierreE[index], Entidades.CierreCaja.tipoBusqueda.FindById, "");
                    ListCierreE[index].Ventas = oCierreN.obtenerTotalVentas(ListCierreE[index].UsuarioInicio.Id, ListCierreE[index].Sucursal.idSucursal, ListCierreE[index].FechaHoraInicio, DateTime.Now);
                    ListCierreE[index].EgresosCaja = oCierreN.getMontoEgresosCajaVendedor(ListCierreE[index]);

                    for (int idx = 0; idx < grillaCajasACerrar.Rows.Count; idx++)
                    {
                        if (ListCierreE[index].Id == Convert.ToInt32(grillaCajasACerrar.Rows[idx].Cells["id"].Value))
                        {
                            if (oCierreCajero.Id.Equals(ListCierreE[index].Id))
                            {
                                grillaCajasACerrar.Rows[idx].DefaultCellStyle.Font = new Font(grillaCajasACerrar.Font, FontStyle.Bold);
                                grillaCajasACerrar.Rows[idx].Cells["vendedor"].Value += " (C)";
                            }
                            grillaCajasACerrar.Rows[idx].Cells["ventas"].Value = ListCierreE[index].Ventas;
                            grillaCajasACerrar.Rows[idx].Cells["gastos"].Value = ListCierreE[index].EgresosCaja;
                            break;
                        }
                    }

                    totalventas += Convert.ToDecimal(ListCierreE[index].Ventas);
                    totalEgresosCaja += Convert.ToDecimal(ListCierreE[index].EgresosCaja);
                }

                txtVentas.Text = totalventas.ToString("F2");
                txtEgresosCaja.Text = totalEgresosCaja.ToString("F2");

                txtCajaCierre.Text = oCierreCajero.CajaCierre.ToString();
                txtDiferencia.Text = oCierreCajero.Diferencia.ToString();
                txtCajaInicioSiguiente.Text = oCierreCajero.CajaInicioSiguiente.ToString();
                txtImporteRetirado.Text = oCierreCajero.ImporteRetirado.ToString();                
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error en validarAperturaForm() \n" + ex.Message);
            }            
        }

        private void btnVerEgresosCaja_Click(object sender, EventArgs e)
        {
            formEgresosCajaVendedor frmEgresosCajaVendedor = new formEgresosCajaVendedor();
            frmEgresosCajaVendedor.oCierreE = oCierreCajero;
            frmEgresosCajaVendedor.ShowDialog();
        }

        private void controlEleccionImporte_ValueChanged(object sender, EventArgs e)
        {
            if (controlEleccionImporte.Value.Equals(0))
            {
                txtCajaInicioSiguiente.ReadOnly = true;
                txtCajaInicioSiguiente.BackColor = SystemColors.ScrollBar;
                txtImporteRetirado.ReadOnly = false;
                txtImporteRetirado.BackColor = SystemColors.Window;
            }
            else
            {
                txtCajaInicioSiguiente.ReadOnly = false;
                txtCajaInicioSiguiente.BackColor = SystemColors.Window;
                txtImporteRetirado.ReadOnly = true;
                txtImporteRetirado.BackColor = SystemColors.ScrollBar;
            }

        }

        private void btnVentas_Click(object sender, EventArgs e)
        {
            formVentasVendedor frmVentasVendedor = new formVentasVendedor();
            frmVentasVendedor.oCierreE = oCierreCajero;
            frmVentasVendedor.Show();
        }

        private void btnCajaAnterior_Click(object sender, EventArgs e)
        {
            try
            {
                if (oCierreAnterior != null)
                {
                    string mensaje = "Ultimo cierre de " + oCierreAnterior.UsuarioInicio.Nombre + "\n\n" +
                        "Apertura: " + oCierreAnterior.FechaHoraInicio.ToString() + "\nCierre: " + oCierreAnterior.FechaHoraCierre.ToString() +
                        "\n-------------\nQuedó en Caja anterior: " + oCierreAnterior.CajaInicioSiguiente.ToString() + "\nCaja inicio actual: "+oCierreCajero.CajaInicio+
                        "\n-------------\nDiferencia: " + (oCierreAnterior.CajaInicioSiguiente-oCierreCajero.CajaInicio).ToString();
                    MessageBox.Show(mensaje, "Caja Anterior");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al obtener el cierre de caja anterior.\n\n" + ex.Message);
            }
        }

        private void grillaCajasACerrar_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            int idSelected = Convert.ToInt32(grillaCajasACerrar.Rows[e.RowIndex].Cells["id"].Value);
            Entidades.CierreCaja oCierreSelected = ListCierreE.Find(x => x.Id == idSelected);
            if (e.ColumnIndex == grillaCajasACerrar.Columns["verEgresosCaja"].Index)
            {                
                formEgresosCajaVendedor frmEgresosCajaVendedor = new formEgresosCajaVendedor();
                frmEgresosCajaVendedor.oCierreE = oCierreSelected;
                frmEgresosCajaVendedor.ShowDialog();
            }

            if (e.ColumnIndex == grillaCajasACerrar.Columns["verVentas"].Index)
            {
                formVentasVendedor frmVentasVendedor = new formVentasVendedor();
                frmVentasVendedor.oCierreE = oCierreSelected;
                frmVentasVendedor.Show();
            }
        }

        private void btnIngresoBilletes_Click(object sender, EventArgs e)
        {
            formIngresoBilletes frmIngresoBilletes = new formIngresoBilletes();
            frmIngresoBilletes.txtBoxAcargar = txtCajaCierre;
            frmIngresoBilletes.ShowDialog();
        }

    }
}
