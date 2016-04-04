using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Presentacion.Movimientos
{
    public partial class formVerAcumulados : Form
    {
        public formVerAcumulados()
        {
            InitializeComponent();
        }

        private void formVerAcumulados_Load(object sender, EventArgs e)
        {
        }

        public void verAcumulados(List<CortesPorMovimiento> listaEnGrilla)//List<Entidades.CortePorMovimiento> listaCortesPorMovimiento)
        {
            this.grillaAcum.DataSource = null;
            List<CortesPorMovimiento> listaGrillaAcum = new List<CortesPorMovimiento>();
            if (listaEnGrilla.Equals(null)) return;
            foreach (Presentacion.CortesPorMovimiento lineaCorte in listaEnGrilla)
            {
                bool encontro = false;
                for (int nroFila = 0; nroFila < listaGrillaAcum.Count; nroFila++)
                {
                    if (listaGrillaAcum[nroFila].IdCorte.Equals(lineaCorte.IdCorte))
                    {
                        listaGrillaAcum[nroFila].CantKg += lineaCorte.CantKg;
                        listaGrillaAcum[nroFila].CantUnidad += lineaCorte.CantUnidad;
                        encontro = true;
                        break;
                    }
                }
                if (!encontro)
                {
                    listaGrillaAcum.Add(lineaCorte);
                }
            }

            this.grillaAcum.DataSource = listaGrillaAcum.OrderBy(order => order.Codigo).ToList();

            this.grillaAcum.Columns["IdCortePorMovimiento"].Visible = false;
            this.grillaAcum.Columns["IdCorte"].Visible = false;
            this.grillaAcum.Columns["PesoBalanza"].Visible = false;

            this.grillaAcum.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Escape)
            {
                this.Close();
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }   
    }
}
