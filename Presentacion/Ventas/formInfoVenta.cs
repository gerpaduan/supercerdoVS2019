using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;


namespace Presentacion.Ventas
{
    public partial class formInfoVenta : Form
    {
        public int idVenta = 0;
        public formVentas frmVentas = new formVentas();

        Negocio.Venta oVentaN = new Negocio.Venta();

        Entidades.Venta oVentaE = new Entidades.Venta();
        List<Entidades.LineaVenta> listaLineaVenta = new List<Entidades.LineaVenta>();
        List<LineaVenta> listaLineaGrilla = new List<LineaVenta>();

        wsAFIPvs2008.formFacturaElectronica formFactElec;

        public formInfoVenta()
        {
            InitializeComponent();
        }

        private void cargarGrilla()
        {
            try
            {
                grillaLineasVenta.DataSource = null;
                grillaLineasVenta.AutoGenerateColumns = false;
                grillaLineasVenta.DataSource = listaLineaGrilla;

                cargarTotales();           
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
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
            txtTotalKgs.Text = Convert.ToString(totalKgs);
            txtTotalS.Text = Convert.ToString(totalPesos);
        }

        private void cargarListaGrilla()
        {
            foreach (Entidades.LineaVenta lineaE in oVentaE.LineasVenta)
            {
                LineaVenta lineaVentaP = new LineaVenta();

                lineaVentaP.idCorte = lineaE.Corte.idCorte;
                lineaVentaP.codigo = lineaE.Corte.codigo;
                lineaVentaP.corte = lineaE.Corte.corte;
                lineaVentaP.cantKgs = lineaE.CantKg;
                lineaVentaP.precioKg = lineaE.PrecioKg;
                lineaVentaP.bonificacion = lineaE.Bonificacion;
                lineaVentaP.totalS = lineaE.PrecioKg * lineaE.CantKg;
                lineaVentaP.PesoBalanza = lineaE.PesoBalanza;

                if (lineaE.Estado == 1)
                {
                    lineaVentaP.estado = "Anulado";
                }
                else
                {
                    lineaVentaP.estado = "";
                }

                listaLineaGrilla.Add(lineaVentaP);
                lineaVentaP = null;
            }                
        }

        private void cargarCamposVenta()
        {
            comboTipoComprobante.SelectedItem = oVentaE.TipoComprobante.ToString();
            txtIdVenta.Text = oVentaE.IdVenta.ToString();
            txtVendedor.Text = oVentaE.Vendedor.Nombre;
            txtSucursal.Text = oVentaE.Sucursal.sucursal;
            txtCliente.Text = oVentaE.Persona.razonSocial;
            txtCuit.Text = oVentaE.Persona.Cuit;
            txtTelefono.Text = oVentaE.Persona.Telefono;
            txtEmail.Text = oVentaE.Email;
            txtFechaVenta.Text = Utilidades.Util_Form.fechaFormato24Horas(oVentaE.FechaVenta);
            txtObservaciones.Text = oVentaE.Observaciones;
            txtCreado.Text = Utilidades.Util_Form.fechaFormato24Horas(oVentaE.Creado);
            txtActualizado.Text = Utilidades.Util_Form.fechaFormato24Horas(oVentaE.Actualizado);
            txtFormaPago.Text = oVentaE.FormaPago;
            cargarListaGrilla();
        }

        private void modificarVenta()
        {
            if (Application.OpenForms["formNuevaVenta"] != null)
            {
                Application.OpenForms["formNuevaVenta"].Activate();
                Application.OpenForms["formNuevaVenta"].WindowState = FormWindowState.Normal;
            }
            else
            {
                formNuevaVenta frmNuevaVenta = new formNuevaVenta();
                frmNuevaVenta.parametrosModificacion(frmVentas, oVentaE, oVentaE.LineasVenta, listaLineaGrilla);
                frmNuevaVenta.SucAnterior = oVentaE.Sucursal.idSucursal;
                this.Close();
                frmNuevaVenta.Show();
            }
        }

        private void btnSalir_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void modificar_Click(object sender, EventArgs e)
        {
            modificarVenta();
        }

        private void formInfoVenta_Load(object sender, EventArgs e)
        {
            this.Text += Utilidades.Conexion.getSucursalConexion();
            try
            {
                oVentaE = oVentaN.getVentaById(idVenta);
                cargarCamposVenta();
                cargarGrilla();
                idVentaLabel.Text = oVentaE.IdVenta.ToString();//asigno el idVenta para identificar formulario
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al obtener información de la venta\n\n" + ex.Message);
                this.Close();
            }
        }

        private void Imprimir_Click(object sender, EventArgs e)
        {
            imprimirTicket();
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
                if (oVentaE.EnCtaCte)
                    ticket.TextoCentro("A Cta. Cte.");
                ticket.TextoIzquierda("A " + oVentaE.Persona.razonSocial);
                ticket.TextoIzquierda("Nro. T. " + oVentaE.IdVenta.ToString());
                ticket.TextoExtremos("Fecha: " + oVentaE.FechaVenta.Date.ToString(), "Hora: " + oVentaE.FechaVenta.TimeOfDay.ToString());
                ticket.LineasGuion();

                foreach (Entidades.LineaVenta linea in oVentaE.LineasVenta)
                {
                    ticket.AgregaArticulo(linea.Corte.codigo.ToString() + " " + linea.Corte.corte.ToString(),
                        linea.PrecioKg, linea.CantKg, linea.PrecioKg * linea.CantKg);
                }
                ticket.TextoDerecha("-------");
                ticket.AgregaTotales("Total", Convert.ToDouble(txtTotalS.Text));
                ticket.LineasEnBlanco(1);
                ticket.TextoIzquierda("Articulos: " + txtCantItems.Text);// + "   Cajero: " + txtVendedor.Text);
                //ticket.TextoIzquierda("Cajero: " + txtVendedor.Text);
                ticket.TextoIzquierda("Cajero: " + oVentaE.Vendedor.Id);
                ticket.GraciasPorSuCompra();
                ticket.LineasEnBlanco(2);
                ticket.realizarImpresion();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Hubo un error a imprimir el ticket.\n\n" + ex.Message, "Error ticket");
            }
        }

        private void facturaElec_Click(object sender, EventArgs e)
        {
            facturaElectronica();
        }

        private void facturaElectronica()
        {
            bool formFactuElec_Abierto = false;
            foreach (Form frm in Application.OpenForms)
            {
                if (frm.GetType() == typeof(wsAFIPvs2008.formFacturaElectronica))
                {
                    formFactElec = (wsAFIPvs2008.formFacturaElectronica)frm;
                    if (formFactElec.idVenta > 0 && formFactElec.facturaPendiente)
                    {
                        foreach (Control ctrl in frm.Controls)
                        {
                            if (ctrl.Name.Equals("txtIdVenta") && ctrl.Text.Equals(oVentaE.IdVenta.ToString()))
                            {
                                formFactuElec_Abierto = true;
                                frm.BringToFront();
                                frm.Visible = false;
                                frm.ShowDialog();
                                break;
                            }
                        }

                        ///Si Form de Factura no está abierto para el idVenta se informa y se abre otro form
                        if (!formFactuElec_Abierto)
                        {
                            MessageBox.Show("Hay una factura pendiende de registrar. Se abrirá otra ventana de facturacion");
                        }
                        break;
                    }

                    if (oVentaE.IdVenta > 0)//Solo se pasa el idVenta si es nuevo
                    {
                        formFactElec.idVenta = oVentaE.IdVenta;
                        formFactElec.cargarDatosAfip = false;
                        formFactElec.cargarVenta();
                    }
                    frm.BringToFront();
                    formFactuElec_Abierto = true;
                    formFactElec.logueado = FormPrincipal.logueado;
                    this.Visible = false;
                    break;
                }
            }

            if (!formFactuElec_Abierto)
            {
                formFactElec = new wsAFIPvs2008.formFacturaElectronica();
                formFactElec.idVenta = oVentaE.IdVenta;
                formFactElec.logueado = FormPrincipal.logueado;
                formFactElec.esShowDialog = true;
                formFactElec.ShowDialog();
            }
        }
    }
}
