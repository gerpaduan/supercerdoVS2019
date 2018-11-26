using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Presentacion.Ventas;
using System.Configuration;
using iTextSharp.text.pdf;
using iTextSharp.text;
using System.IO;

namespace Presentacion
{
    public partial class formUltimasVentasCliente : Form
    {
        private bool logueado = false;

        public bool Logueado
        {
            get { return logueado; }
            set { logueado = value; }
        }

        public DataTable dtSucursales;

        public Negocio.Sucursal oSucursalN = new Negocio.Sucursal();
        public Negocio.Venta oVentaN = new Negocio.Venta();
        public Negocio.Usuario oUsuarioN = new Negocio.Usuario();

        public DataTable dtVentas;
        public int idSucursal;
        public int idPersona;
        public string nombrePresona;

        public formUltimasVentasCliente()
        {
            InitializeComponent();

            this.Text += Utilidades.Conexion.getSucursalConexion();
            cargarGrilla();
        }

        public void cargarGrilla()
        {
            //try
            //{
            //    dtVentas = new DataTable();
            //    dtVentas = oVentaN.ultimasVentasCliente(idSucursal, idPersona);
            //    grillaVentas.DataSource = dtVentas;
            //    grillaVentas.Columns["fechaVenta"].DefaultCellStyle.Format = "dd/MM/yyyy HH:mm:ss";
            //    grillaVentas.Columns["fechaVenta"].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCellsExceptHeader;
            //    grillaVentas.Columns["cantKg"].DefaultCellStyle.Format = "F3";
            //    grillaVentas.Columns["precioKg"].DefaultCellStyle.Format = "F2";
            //    grillaVentas.Columns["bonificacion"].DefaultCellStyle.Format = "F2";
            //    grillaVentas.Columns["totalCorte"].DefaultCellStyle.Format = "F2";
            
            //    cargarTotales();
            //}
            //catch (Exception ex)
            //{
            //    MessageBox.Show(ex.Message);
            //}
        }

        private void btnCancelar_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        
    }
}
