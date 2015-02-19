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
        public FormReportes(CrystalDecisions.CrystalReports.Engine.ReportDocument reporte, string tituloParam, DataTable dtReporte, DateTime fechaDesdeParam, DateTime fechaHastaParam)
        {
            InitializeComponent();
            
            reporte.SetDataSource(dtReporte);
            reporte.SetParameterValue("Titulo", tituloParam);
            reporte.SetParameterValue("FechaDesde", fechaDesdeParam);
            reporte.SetParameterValue("FechaHasta", fechaHastaParam);

            crystalReportes.ReportSource = reporte;

        }

        private void crystalReportViewer1_Load(object sender, EventArgs e)
        {

        }
    }
}
