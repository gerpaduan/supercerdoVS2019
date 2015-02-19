using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Presentacion.Cortes;

namespace Presentacion.Movimientos
{
    public partial class formInfoMovimiento : Form
    {
        formMovimientos frmMovimiento;

        Entidades.Movimiento oMovimientoE = new Entidades.Movimiento();
        Entidades.CortePorMovimiento oCortePorMovimientoE;

        List<Entidades.CortePorMovimiento> listaCortesPorMovimiento = new List<Entidades.CortePorMovimiento>();

        CortesPorMovimiento cortePorMovimiento;
        List<CortesPorMovimiento> listaEnGrilla;

        Negocio.Corte oCorteN = new Negocio.Corte();

        string estado;

        public formInfoMovimiento()
        {
            InitializeComponent();

        }

        public void obtenerParametros(formMovimientos frmMovimientosParam, Entidades.Movimiento movimientoParam)
        {
            frmMovimiento = frmMovimientosParam;
            oMovimientoE = movimientoParam;

            cargarCampos();
        }

        private void cargarCampos()
        {
            txtSucOrigen.Text = oMovimientoE.SucursalOrigen.sucursal;
            txtSucDestino.Text = oMovimientoE.SucursalDestino.sucursal;
            txtFechaMovimiento.Value = oMovimientoE.FechaMovimiento;
            txtObservaciones.Text = oMovimientoE.Observaciones;

            cargarListaCortesPorMovimiento();
        }

        private void cargarListaCortesPorMovimiento()
        {
            listaCortesPorMovimiento= oCorteN.cargarCortesPorMovimiento(oMovimientoE.IdMovimiento);

            cargarGrilla();
        
        }


        private void cargarListaEnGrilla()
        {
            listaEnGrilla = new List<CortesPorMovimiento>();

            foreach (Entidades.CortePorMovimiento lineaCorte in listaCortesPorMovimiento)
            {
                cortePorMovimiento = new CortesPorMovimiento();

                cortePorMovimiento.IdCortePorMovimiento = lineaCorte.IdCorteMovimiento;
                cortePorMovimiento.IdCorte = lineaCorte.Corte.idCorte;
                cortePorMovimiento.Codigo = lineaCorte.Corte.codigo;
                cortePorMovimiento.Corte = lineaCorte.Corte.corte;
                cortePorMovimiento.CantKg = lineaCorte.CantKg;

                listaEnGrilla.Add(cortePorMovimiento);
            }

        }

        public void cargarGrilla()
        {
            cargarListaEnGrilla();

            grillaCortesPorMovimiento.DataSource = null;
            grillaCortesPorMovimiento.AutoGenerateColumns = false;

            grillaCortesPorMovimiento.DataSource = listaEnGrilla;

            cargarTotales();
        }

        private void cargarTotales()
        {
            float totalKg = 0;
            foreach (Entidades.CortePorMovimiento filaCorte in listaCortesPorMovimiento)
            {
                totalKg += filaCorte.CantKg;
            }

            txtCantItems.Text = Convert.ToString(grillaCortesPorMovimiento.Rows.Count);
            txtTotalKg.Text = Convert.ToString(totalKg);

        }



        private void btnSalir_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void modificar_Click(object sender, EventArgs e)
        {
            if (Application.OpenForms["formNuevoMovimiento"] != null)
            {

                Application.OpenForms["formNuevoMovimiento"].Activate();


            }
            else
            {

                formNuevoMovimiento frmNuevoMovimiento = new formNuevoMovimiento();
                frmNuevoMovimiento.obtenerParametros(frmMovimiento,oMovimientoE,listaCortesPorMovimiento);
                this.Close();
                frmNuevoMovimiento.Show();

            }
        }

        private void cargarReporte()
        {
            int tipoReporte = 4;//nro perteneciente al reporte de los movimientos
            formReporteStock frmReporte = new formReporteStock();
            frmReporte.obtenerParametros(oMovimientoE.SucursalDestino.idSucursal, oMovimientoE.FechaMovimiento, oMovimientoE.FechaMovimiento, tipoReporte, oMovimientoE.IdMovimiento.ToString());
            frmReporte.Show();
        
        }

        private void Reporte_Click(object sender, EventArgs e)
        {
            cargarReporte();
        }

       

       
        
    }
}
