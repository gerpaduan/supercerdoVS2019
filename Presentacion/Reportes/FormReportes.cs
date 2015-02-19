using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;


namespace Presentacion.Reportes
{
    public partial class FormReportes : Form
    {
        bool objetos = false;
        
        public bool Objetos
        {
            get { return objetos; }
            set { objetos = value; }
        }
        bool reporteVenta = false;

        public bool ReporteVenta
        {
            get { return reporteVenta; }
            set { reporteVenta = value; }
        }
        List<Presentacion.LineaVenta> listaLineasVenta;

        public List<Presentacion.LineaVenta> ListaLineasVenta
        {
            get { return listaLineasVenta; }
            set { listaLineasVenta = value; }
        }

        bool reporteMovimiento = false;

        public bool ReporteMovimiento
        {
            get { return reporteMovimiento; }
            set { reporteMovimiento = value; }
        }
        List<Presentacion.CortesPorMovimiento> listaCortesPorMov;

        public List<Presentacion.CortesPorMovimiento> ListaCortesPorMov
        {
            get { return listaCortesPorMov; }
            set { listaCortesPorMov = value; }
        }
        string origen;

        public string Origen
        {
            get { return origen; }
            set { origen = value; }
        }
        string destino;

        public string Destino
        {
            get { return destino; }
            set { destino = value; }
        }

        //parámetros
        CrystalDecisions.CrystalReports.Engine.ReportDocument reporte;
        string tituloParam;
        DataTable dtReporte;
        DateTime fechaDesdeParam, fechaHastaParam;

        public FormReportes(CrystalDecisions.CrystalReports.Engine.ReportDocument reporte, string tituloParam, DataTable dtReporte, DateTime fechaDesdeParam, DateTime fechaHastaParam)
        {
            InitializeComponent();

            this.reporte=reporte;
            this.tituloParam=tituloParam;
            this.dtReporte=dtReporte;
            this.fechaDesdeParam = fechaDesdeParam;
            this.fechaHastaParam=fechaHastaParam;            

        }

        private void crystalReportViewer1_Load(object sender, EventArgs e)
        {
            if (objetos)
            {
                if (reporteMovimiento)
                {
                    reporte.SetDataSource(ListaCortesPorMov);
                    reporte.SetParameterValue("Origen", Origen);
                    reporte.SetParameterValue("Destino", Destino);

                }
                if (reporteVenta)
                {
                    reporte.SetDataSource(ListaLineasVenta);
                    reporte.SetParameterValue("Origen", Origen);
                    reporte.SetParameterValue("Destino", Destino);

                }
            }
            else
            {
                reporte.SetDataSource(dtReporte);
            }

            reporte.SetParameterValue("Titulo", tituloParam);
            reporte.SetParameterValue("FechaDesde", fechaDesdeParam);
            reporte.SetParameterValue("FechaHasta", fechaHastaParam);

            crystalReportes.ReportSource = reporte;
        }

        private void FormReportes_Load(object sender, EventArgs e)
        {

        }
    }
}
