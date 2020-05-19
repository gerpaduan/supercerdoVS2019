using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Presentacion.Reportes;

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
            checkCtaCte.Checked = oVentaE.EnCtaCte;
            checkCtaCte.BackColor = Utilidades.Util_Form.getBackColorCheckBox(checkCtaCte.Checked);
            checkCtaCte.Enabled = false;//para evitar que se cambiar el estado
            txtSucursal.Text = oVentaE.Sucursal.sucursal;
            txtCliente.Text = oVentaE.Persona.razonSocial;
            txtCuit.Text = oVentaE.Persona.Cuit;
            txtTelefono.Text = oVentaE.Persona.Telefono;
            txtEmail.Text = oVentaE.Email;
            txtFechaVenta.Text = Utilidades.Util_Form.fechaFormato24Horas(oVentaE.FechaVenta);
            txtObservaciones.Text = oVentaE.Observaciones;
            txtCreado.Text = Utilidades.Util_Form.fechaFormato24Horas(oVentaE.Creado);
            txtActualizado.Text = Utilidades.Util_Form.fechaFormato24Horas(oVentaE.Actualizado);

            checkEfectivo.BackColor = Utilidades.Util_Form.getBackColorCheckBox(false);
            checkDebito.BackColor = Utilidades.Util_Form.getBackColorCheckBox(false);
            checkCredito.BackColor = Utilidades.Util_Form.getBackColorCheckBox(false);
            switch (oVentaE.FormaPago)
            {
                case "Efectivo":
                    checkEfectivo.BackColor = Utilidades.Util_Form.getBackColorCheckBox(true);
                    break;
                case "Debito":
                    checkDebito.BackColor = Utilidades.Util_Form.getBackColorCheckBox(true);
                    break;
                case "Credito":
                    checkCredito.BackColor = Utilidades.Util_Form.getBackColorCheckBox(true);
                    break;
                default:
                    break;
            }

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
            //try
            //{
            //    string titulo = oVentaE.Persona.razonSocial;
            //    FormReportes frmReportes;

            //    Reportes.ReporteVenta reporte = new Reportes.ReporteVenta();
            //    frmReportes = new FormReportes(reporte, titulo, null, oVentaE.FechaVenta, oVentaE.FechaVenta);

            //    frmReportes.ListaLineasVenta = listaLineaGrilla;
            //    frmReportes.Objetos = true;
            //    frmReportes.ReporteVenta = true;
            //    frmReportes.Origen = oVentaE.Sucursal.SucursalNombre;
            //    frmReportes.Destino = oVentaE.Sucursal.SucursalNombre;

            //    frmReportes.Show();                
            //}
            //catch (Exception ex)
            //{
            //    MessageBox.Show(ex.Message);
            //}
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
            }
            catch (Exception ex)
            {
                MessageBox.Show("Hubo un error a imprimir el ticket.\n\n" + ex.Message, "Error ticket");
            }
        }

        private void checkCtaCte_CheckedChanged(object sender, EventArgs e)
        {
            checkCtaCte.BackColor = Utilidades.Util_Form.getBackColorCheckBox(checkCtaCte.Checked);
        }
    }
}
