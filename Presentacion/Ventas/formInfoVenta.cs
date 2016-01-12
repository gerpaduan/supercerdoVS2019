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
        formVentas frmVentas = new formVentas();
        
        DataRow drVenta;

        Negocio.Venta oVentaN = new Negocio.Venta();

        Entidades.Venta oVentaE = new Entidades.Venta();
        Entidades.LineaVenta oLineaVenta;

        List<Entidades.LineaVenta> listaLineaVenta = new List<Entidades.LineaVenta>();
        List<LineaVenta> listaLineaGrilla = new List<LineaVenta>();

        public formInfoVenta()
        {
            InitializeComponent();

        }

        public void obtenerParametro(formVentas frmVentasParam, DataRow drVentaParam)
        {
            frmVentas = frmVentasParam;
            drVenta = drVentaParam;

            cargarCamposVenta();
            obtenerLineasVenta();
            cargarGrilla();

        }

        private void cargarGrilla()
        {
            try
            {
                grillaLineasVenta.DataSource = null;
                grillaLineasVenta.AutoGenerateColumns = false;
                grillaLineasVenta.DataSource = listaLineaGrilla;

                cargarTotales();

                ///quitar controles según Login
                if (Presentacion.FormPrincipal.logueado==false)
                {
                    txtTotalS.Text = "";
                    modificar.Enabled = false;
                    agregaStock.Enabled = false;
                    foreach (DataGridViewColumn col in grillaLineasVenta.Columns)
                    {
                        if (col.Name.Equals("totalS"))
                        {
                            col.Visible = false;
                        }
                    }
                }
           
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


        private void obtenerLineasVenta()
        {
            listaLineaVenta = oVentaN.obtenerLineasVenta(Convert.ToInt32(drVenta["idVenta"].ToString()));
            cargarListaGrilla();

        }

        private void cargarListaGrilla()
        {
            foreach (Entidades.LineaVenta lineaE in listaLineaVenta)
            {
                LineaVenta lineaVentaP = new LineaVenta();

                lineaVentaP.idCorte = lineaE.Corte.idCorte;
                lineaVentaP.codigo = lineaE.Corte.codigo;
                lineaVentaP.corte = lineaE.Corte.corte;
                lineaVentaP.cantKgs = lineaE.CantKg;
                lineaVentaP.precioKg = lineaE.PrecioKg;
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
            oVentaE.IdVenta = Convert.ToInt32(drVenta["idVenta"].ToString());
            oVentaE.NroRemito = drVenta["nroRemito"].ToString();
            oVentaE.FechaVenta = Convert.ToDateTime(drVenta["fechaVenta"].ToString());
            oVentaE.DiaFestivo = drVenta["diaFestivo"].ToString();
            oVentaE.Turno = drVenta["turno"].ToString();
            oVentaE.Estado = drVenta["estado"].ToString();
            oVentaE.Observaciones = drVenta["observaciones"].ToString();
            DateTime fechaNull = Convert.ToDateTime("01/01/1990");
           // oVentaE.Creado = Convert.ToDateTime(drVenta["creado"].ToString());
            oVentaE.Creado = !String.IsNullOrEmpty(drVenta["creado"].ToString()) ? (Convert.ToDateTime(drVenta["creado"].ToString())) : fechaNull;
        
            oVentaE.Actualizado = !String.IsNullOrEmpty(drVenta["actualizado"].ToString()) ? (Convert.ToDateTime(drVenta["actualizado"].ToString())) : fechaNull ;

            Entidades.Persona oPersona = new Entidades.Persona();
            oPersona.idPersona = Convert.ToInt32(drVenta["idPersona"].ToString());
            oPersona.razonSocial = drVenta["razonSocial"].ToString();

            oVentaE.Persona = oPersona;

            Entidades.Sucursal oSucursal = new Entidades.Sucursal();
            oSucursal.idSucursal = Convert.ToInt32(drVenta["idSucursal"].ToString());
            oSucursal.sucursal = drVenta["sucursal"].ToString();

            oVentaE.Sucursal = oSucursal;


            //carga los campos
            txtCliente.Text = drVenta["razonSocial"].ToString();
            txtDiaFestivo.Text = drVenta["diaFestivo"].ToString();
            txtTurno.Text = drVenta["turno"].ToString();

            DateTime fecha = new DateTime();
            fecha = Convert.ToDateTime(drVenta["fechaVenta"].ToString());
            txtFechaVenta.Value = DateTime.Parse( fecha.ToShortDateString());
            

            txtFechaVenta.Value = fecha;
            txtNroRemito.Text = drVenta["nroRemito"].ToString();
            txtObservaciones.Text = drVenta["observaciones"].ToString();
            txtCreado.Text = drVenta["creado"].ToString();
            txtActualizado.Text = drVenta["actualizado"].ToString();
            txtSucursal.Text = drVenta["sucursal"].ToString();

            if (!oVentaE.Estado.Equals(""))
            {
                agregaStock.Enabled = false;
            }

        }
        private void agregarStock()
        {
            DialogResult resp = MessageBox.Show("Los Kgs. de los cortes correspondiente a la venta se ingresarán al stock.\n¿Está seguro de realizar esta acción?", "Agregar stock", MessageBoxButtons.YesNo, MessageBoxIcon.Information, MessageBoxDefaultButton.Button2);
            if (resp==DialogResult.Yes)
            {
                oVentaE.Estado = "Stock Agregado";
                oVentaN.agregarStockVenta(oVentaE);

                agregaStock.Enabled = false;
                frmVentas.cargarGrilla();
            }
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
                frmNuevaVenta.parametrosModificacion(frmVentas, oVentaE, listaLineaVenta, listaLineaGrilla);
                frmNuevaVenta.SucAnterior = oVentaE.Sucursal.idSucursal;
                this.Close();
                frmNuevaVenta.Show();

            }
        }

        private void btnSalir_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void anular_Click(object sender, EventArgs e)
        {
          //  anularVenta();
        }

        private void modificar_Click(object sender, EventArgs e)
        {
            modificarVenta();
        }

       

        private void agregaStock_Click(object sender, EventArgs e)
        {
            agregarStock();
        }

        private void formInfoVenta_Load(object sender, EventArgs e)
        {

        }

        private void Imprimir_Click(object sender, EventArgs e)
        {
            try
            {
                string titulo = oVentaE.Persona.razonSocial;
                FormReportes frmReportes;

                Reportes.ReporteVenta reporte = new Reportes.ReporteVenta();
                frmReportes = new FormReportes(reporte, titulo, null, oVentaE.FechaVenta, oVentaE.FechaVenta);

                frmReportes.ListaLineasVenta = listaLineaGrilla;
                frmReportes.Objetos = true;
                frmReportes.ReporteVenta = true;
                frmReportes.Origen = oVentaE.Sucursal.SucursalNombre;
                frmReportes.Destino = oVentaE.Sucursal.SucursalNombre;

                frmReportes.Show();
                
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

       

       
    }
}
