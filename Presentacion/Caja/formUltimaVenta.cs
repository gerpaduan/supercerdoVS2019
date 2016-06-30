using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Presentacion.Personas;

namespace Presentacion.Caja
{
    public partial class formUltimaVenta : Form, InterfaceUsuario, InterfacePersona
    {
        public Entidades.Venta oUltimaVenta = new Entidades.Venta();
        List<Entidades.LineaVenta> lineaNuevosAnulados = new List<Entidades.LineaVenta>();
        List<LineaVenta> listaLineaGrilla = new List<LineaVenta>();
        Entidades.Usuario oVendedorNuevo;

        Negocio.Venta oVentaN = new Negocio.Venta();
        Entidades.LineaVenta oLineaVenta;

        bool huboModificaciones = false;//se establece true cuando se modificó algo

        public formUltimaVenta()
        {
            InitializeComponent();
        }

        private void formUltimaVenta_Load(object sender, EventArgs e)
        {
            this.Text += Utilidades.Conexion.getSucursalConexion();

            cargarGrilla();
            changeCheckTicket();
            this.txtCliente.Text = oUltimaVenta.Persona.razonSocial;
            txtFecVenta.Text = oUltimaVenta.FechaVenta.ToString();
            txtVendedor.Text = oUltimaVenta.Vendedor.Nombre;
            txtSucursal.Text = oUltimaVenta.Sucursal.sucursal;
            txtNroTicket.Text = oUltimaVenta.IdVenta.ToString();
            txtObservaciones.Text = oUltimaVenta.Observaciones;
            huboModificaciones = false;
        }

        public void cargarGrilla()
        {
            try
            {
                grillaLineasVenta.AutoGenerateColumns = false;
                grillaLineasVenta.DataSource = null;
                cargarListaGrilla();
                grillaLineasVenta.DataSource = listaLineaGrilla;
                if (listaLineaGrilla.Count > 0)
                {
                    grillaLineasVenta.Rows[listaLineaGrilla.Count - 1].Selected = true;
                    grillaLineasVenta.FirstDisplayedScrollingRowIndex = listaLineaGrilla.Count - 1;

                    for (int nroFila = 0; nroFila < grillaLineasVenta.Rows.Count; nroFila++)
                    {
                        foreach (Entidades.LineaVenta linea in oUltimaVenta.LineasVenta)
                        {
                            if (grillaLineasVenta.Rows[nroFila].Cells["Corte"].Value.ToString().Length > 22)
                            {
                                grillaLineasVenta.Rows[nroFila].Cells["Corte"].Style.Font = new Font(grillaLineasVenta.Font.ToString(), 13);
                            }

                            if (Convert.ToInt32(grillaLineasVenta.Rows[nroFila].Cells["Codigo"].Value) == linea.Corte.codigo && 
                                Convert.ToInt32(grillaLineasVenta.Rows[nroFila].Cells["idLineaVenta"].Value) == linea.IndexAnulado)
                            {
                                grillaLineasVenta.Rows[nroFila].DefaultCellStyle.ForeColor = Color.Red;
                            }
                        }
                    }
                }
                cargarTotales();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void cargarListaGrilla()
        {
            LineaVenta lineaVentaP;
            listaLineaGrilla.Clear();

            foreach (Entidades.LineaVenta  lineaE in oUltimaVenta.LineasVenta)
            {
                lineaVentaP = new LineaVenta();
                lineaVentaP.IdLineaVenta = lineaE.IdLineaVenta;
                lineaVentaP.idCorte = lineaE.Corte.idCorte;
                lineaVentaP.codigo = lineaE.Corte.codigo;
                lineaVentaP.corte = lineaE.Corte.corte;
                lineaVentaP.cantKgs = lineaE.CantKg;
                lineaVentaP.precioKg = lineaE.PrecioKg;
                lineaVentaP.totalS = lineaE.PrecioKg * lineaE.CantKg;
                lineaVentaP.IndexAnulado = lineaE.IndexAnulado;

                if (lineaE.Estado == 1)
                {
                    lineaVentaP.estado = "Anulado";
                    lineaVentaP.corte += "(Anulado)";
                }
                else
                {
                    lineaVentaP.estado = "";
                }

                listaLineaGrilla.Add(lineaVentaP);
                lineaVentaP = null;
            }
        }

        private void cargarTotales()
        {
            float totalKgs = 0;
            float totalPesos = 0;

            foreach (LineaVenta linea in listaLineaGrilla)
            {
                totalKgs += linea.cantKgs;
                totalPesos += linea.totalS;
            }

            txtCantItems.Text = grillaLineasVenta.Rows.Count.ToString();
            txtTotalKgs.Text = totalKgs.ToString("F3");
            txtTotalS.Text = totalPesos.ToString("N2");
            anularVenta.Enabled = totalPesos > 0 ? true : false;
            //totalVenta = float.Parse(txtTotalS.Text.Trim());
            //abonar();
        }

        public void EnviarUsuario(Entidades.Usuario usuario)
        {
            oVendedorNuevo = usuario;
        }

        private void cambiarVendedor_Click(object sender, EventArgs e)
        {
            FormLoginVendedor frmLogin = new FormLoginVendedor();
            frmLogin.ShowDialog(this);
            validarAperturaCaja();
            if ( oVendedorNuevo != null && oVendedorNuevo.Id > 0)
            {
                huboModificaciones = true;
                oUltimaVenta.Vendedor = oVendedorNuevo;
                txtVendedor.Text = oVendedorNuevo.Nombre;
            }
            oVendedorNuevo = null;
        }

        private void validarAperturaCaja()
        {
            Negocio.CierreCaja oCierreN = new Negocio.CierreCaja();
            Entidades.CierreCaja oCierreE = new Entidades.CierreCaja();
            oCierreE.Sucursal = oUltimaVenta.Sucursal;
            oCierreE.UsuarioInicio = oVendedorNuevo;
            oCierreE = oCierreN.findByIdOrLast(oCierreE, Entidades.CierreCaja.tipoBusqueda.FindLast, "");
            if (oCierreE == null || !oCierreE.UsuarioCierre.Id.Equals(0))
            {
                MessageBox.Show(oVendedorNuevo.Nombre + ":\nDebes Abrir Caja para poder registrar ventas.", "Abrir Caja", MessageBoxButtons.OK, MessageBoxIcon.Information);
                oVendedorNuevo = null;
            }
        }

        private void anularVenta_Click(object sender, EventArgs e)
        {
        //    DialogResult respuesta = MessageBox.Show("¿Está seguro que desea anular la venta?", "Anular venta", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
        //    if (respuesta.Equals(DialogResult.Yes))
        //    {
        //        huboModificaciones = true;
        //        Entidades.LineaVenta oLineaVenta;
        //        for (int nroFila = 0; nroFila < oUltimaVenta.LineasVenta.Count; nroFila++)
        //        {
        //            if (oUltimaVenta.LineasVenta[nroFila].CantKg > 0)
        //            {
        //                Entidades.LineaVenta oLineaVentaSelect = new Entidades.LineaVenta();
        //                oLineaVentaSelect = oUltimaVenta.LineasVenta[nroFila];

        //                bool existeAnulado = false;
        //                foreach (Entidades.LineaVenta linea in oUltimaVenta.LineasVenta)
        //                {
        //                    if (oLineaVentaSelect.Corte.codigo == linea.Corte.codigo &&
        //                        (linea.IndexAnulado == nroFila ||
        //                        (oLineaVentaSelect.CantKg > 0 && oLineaVentaSelect.CantKg.Equals(-linea.CantKg))))
        //                    {
        //                        existeAnulado = true;
        //                        break;
        //                    }
        //                }

        //                if (oLineaVentaSelect.Estado == 0 && !existeAnulado)
        //                {
        //                    oLineaVenta = new Entidades.LineaVenta();

        //                    oLineaVenta = new Entidades.LineaVenta();
        //                    oLineaVenta.Corte = oLineaVentaSelect.Corte;
        //                    oLineaVenta.Venta = oLineaVentaSelect.Venta;
        //                    oLineaVenta.CantKg = oLineaVentaSelect.CantKg * -1;
        //                    oLineaVenta.PrecioKg = oLineaVentaSelect.PrecioKg;
        //                    oLineaVenta.Estado = 1;//anulado
        //                    oLineaVenta.IndexAnulado = nroFila;
        //                    lineaNuevosAnulados.Add(oLineaVenta);
        //                }
        //            }
        //        }
        //        foreach (Entidades.LineaVenta linea in lineaNuevosAnulados)
        //        {
        //            oUltimaVenta.LineasVenta.Add(linea);                    
        //        }
        //        cargarListaGrilla();
        //        cargarGrilla();
        //        cargarTotales();
        //        anularVenta.Enabled = false;
        //    }
        }

        private void ImprimirTicket_Click(object sender, EventArgs e)
        {
            imprimirTicket();
        }

        private void btnAceptar_Click(object sender, EventArgs e)
        {
            try
            {
                if (huboModificaciones)
                {
                    DialogResult respuesta = MessageBox.Show("¿Está seguro que desea modificar los datos de la venta?", "Modificar venta", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
                    if (respuesta.Equals(DialogResult.Yes))
                    {
                        oVentaN.modificarVenta(oUltimaVenta, oUltimaVenta.Sucursal.IdSucursal, false);

                        foreach (Entidades.LineaVenta lineaNuevoAnulado in lineaNuevosAnulados)
                        {
                            oVentaN.agregarLineaVenta(lineaNuevoAnulado);
                        }

                        if(checkTicket.Checked)
                            imprimirTicket();

                        huboModificaciones = false;//se establece FALSE para evitar mensaje de salida del form
                        this.Close();
                    }
                }
                else
                {
                    MessageBox.Show("No se realizaron cambios en la venta para guardar.\nPara cerrar la ventana presione el botón Salir");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al guardar la modificación en la venta.\n\n"+ex.Message);
            }
        }

        private void btnBuscarCliente_Click(object sender, EventArgs e)
        {
            buscarCliente();
        }

        private void buscarCliente()
        {
            formBuscarPersona frmBuscarPersona = new formBuscarPersona();
            frmBuscarPersona.Show(this);
        }

        public void EnviarPersona(Entidades.Persona persona)
        {
            huboModificaciones = true;
            oUltimaVenta.Persona = persona;
            this.txtCliente.Text = oUltimaVenta.Persona.razonSocial;
        }

        private void txtObservaciones_TextChanged(object sender, EventArgs e)
        {
            huboModificaciones = true;            
            oUltimaVenta.Observaciones = txtObservaciones.Text;
        }

        private void btnCancelar_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void formUltimaVenta_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = salir();
        }

        private bool salir()
        {
            bool resp = false;
            if (huboModificaciones)
            {
                string mensaje = "Si sale no se guardarán los datos modificados.\n\n¿Está seguro que desea salir?.";
                if (MessageBox.Show(mensaje, "Salir", MessageBoxButtons.YesNo, MessageBoxIcon.Question,
                    MessageBoxDefaultButton.Button2).Equals(DialogResult.No))
                {
                    resp = true;
                }
            }
            return resp;
        }
        
        private void imprimirTicket()
        {
            try
            {
                Ticket.CreaTicket ticket = new Ticket.CreaTicket();
                ticket.imprimir = true;
                ticket.TextoCentro("x");
                ticket.NoValidoComoFactura();
                ticket.LineasEnBlanco(1);
                ticket.TextoIzquierda("A " + oUltimaVenta.Persona.razonSocial);
                ticket.TextoIzquierda("Nro. T. " + oUltimaVenta.IdVenta.ToString());
                ticket.TextoExtremos("Fecha: " + oUltimaVenta.FechaVenta.Date.ToString(), "Hora: " + oUltimaVenta.FechaVenta.TimeOfDay.ToString());
                ticket.LineasGuion();

                foreach (Entidades.LineaVenta linea in oUltimaVenta.LineasVenta)
                {
                    ticket.AgregaArticulo(linea.Corte.codigo.ToString() + " " + linea.Corte.corte.ToString(),
                        linea.PrecioKg, linea.CantKg, linea.PrecioKg * linea.CantKg);
                }
                ticket.TextoDerecha("-------");
                ticket.AgregaTotales("Total", Convert.ToDouble(txtTotalS.Text));
                ticket.LineasEnBlanco(1);
                ticket.TextoIzquierda("Articulos: " + txtCantItems.Text);// + "   Cajero: " + txtVendedor.Text);
                //ticket.TextoIzquierda("Cajero: " + txtVendedor.Text);
                ticket.TextoIzquierda("Cajero: " + oUltimaVenta.Vendedor.Id);
                ticket.GraciasPorSuCompra();
                ticket.LineasEnBlanco(2);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Hubo un error a imprimir el ticket.\n\n" + ex.Message, "Error ticket");
            }
        }

        private void anularCorte()
        {
            if (grillaLineasVenta.SelectedRows.Count > 0)
            {
                int nroFila = grillaLineasVenta.Rows.GetFirstRow(DataGridViewElementStates.Selected);//obtiene nro de fila de la grilla

                Entidades.LineaVenta oLineaVentaSelect = new Entidades.LineaVenta();
                oLineaVentaSelect = oUltimaVenta.LineasVenta[nroFila];

                bool existeAnulado = false;
                foreach (Entidades.LineaVenta linea in oUltimaVenta.LineasVenta)
                {
                    if (Entidades.LineaVenta.esAnulado(oLineaVentaSelect.Estado) || oLineaVentaSelect.IdLineaVenta == linea.IndexAnulado)
                    {
                        existeAnulado = true;
                        break;
                    }
                }

                if (!existeAnulado)
                {
                    string datosLinea = "\n\n Datos del Corte \n-----------------------------------------\n " +
                        oLineaVentaSelect.Corte.corte +
                        "    |   Cantidad:  " + oLineaVentaSelect.CantKg + "    |    Total:  $ " + oLineaVentaSelect.CantKg * oLineaVentaSelect.PrecioKg;
                    string mensaje = "¿Está seguro de anular el corte seleccionado?" + datosLinea;
                    DialogResult respuesta = MessageBox.Show(mensaje, "Anular Corte", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
                    if (respuesta == System.Windows.Forms.DialogResult.Yes)
                    {
                        oLineaVenta = new Entidades.LineaVenta();
                        oLineaVenta.Corte = oLineaVentaSelect.Corte;
                        oLineaVenta.Venta = oLineaVentaSelect.Venta;
                        oLineaVenta.CantKg = oLineaVentaSelect.CantKg * -1;
                        oLineaVenta.PrecioKg = oLineaVentaSelect.PrecioKg;
                        oLineaVenta.Estado = 1;//anulado
                        oLineaVenta.IndexAnulado = oLineaVentaSelect.IdLineaVenta;

                        lineaNuevosAnulados.Add(oLineaVenta);
                        oUltimaVenta.LineasVenta.Add(oLineaVenta);
                        cargarListaGrilla();
                        cargarGrilla();
                        cargarTotales();
                        huboModificaciones = true;
                    }
                }
                else
                {
                    MessageBox.Show("El corte seleccionado ya ha sido anulado.", "Anular corte", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            else
            {
                MessageBox.Show("No hay ninguna fila seleccionada.", "Seleccione un fila", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            oLineaVenta = null;
        }

        private void grillaLineasVenta_CellClick(object sender, DataGridViewCellEventArgs e)
        {

            // Ignore clicks that are not on button cells.  
            if (e.RowIndex < 0 || e.ColumnIndex !=
                grillaLineasVenta.Columns["btnAnular"].Index) return;

            anularCorte();
        }

        private void checkTicket_CheckedChanged(object sender, EventArgs e)
        {
            changeCheckTicket();
        }

        private void changeCheckTicket()
        {

            checkTicket.BackColor = Utilidades.Util_Form.getBackColorCheckBox(checkTicket.Checked);
        }
    }
}
