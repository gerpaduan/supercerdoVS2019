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
        List<CortesPorMovimiento> listaGrillaAcum;
        List<CortesPorCompra> listaGrillaStockAcum;

        public enum tipoAcum
        {
            movimiento,
            stock,
        }

        public formVerAcumulados()
        {
            InitializeComponent(); this.Icon = Properties.Resources.CarniSys_ICONO;
        }

        private void formVerAcumulados_Load(object sender, EventArgs e)
        {
        }

        public void verAcumulados(List<CortesPorMovimiento> listaEnGrilla, List<CortesPorCompra> listaEnGrillaStock, tipoAcum tipo)//List<Entidades.CortePorMovimiento> listaCortesPorMovimiento)
        {
            this.grillaAcum.DataSource = null;

            switch (tipo)
            {
                case tipoAcum.movimiento:
                    if (listaEnGrilla.Equals(null)) return;
                    listaGrillaAcum = new List<CortesPorMovimiento>();
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
                    this.grillaAcum.Columns["permitirIngreso"].Visible = false;

                    this.grillaAcum.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

                    grillaAcum.Columns["Codigo"].DisplayIndex = 0;
                    grillaAcum.Columns["Corte"].DisplayIndex = 1;
                    grillaAcum.Columns["CantUnidad"].DisplayIndex = 2;
                    grillaAcum.Columns["CantKg"].DisplayIndex = 3;
                    break;
                case tipoAcum.stock:
                    if (listaEnGrillaStock.Equals(null)) return;
                    this.Text = "Acum. de Stock";
                    listaGrillaStockAcum = new List<CortesPorCompra>();

                    foreach (Presentacion.CortesPorCompra lineaCorte in listaEnGrillaStock)
                    {
                        bool encontro = false;
                        for (int nroFila = 0; nroFila < listaGrillaStockAcum.Count; nroFila++)
                        {
                            if (listaGrillaStockAcum[nroFila].codigo.Equals(lineaCorte.codigo))
                            {
                                listaGrillaStockAcum[nroFila].CantKgs += lineaCorte.CantKgs;
                                encontro = true;
                                break;
                            }
                        }
                        if (!encontro)
                        {
                            listaGrillaStockAcum.Add(lineaCorte);
                        }
                    }

                    this.grillaAcum.DataSource = listaGrillaStockAcum.OrderBy(order => order.Codigo).ToList();

                    for (int nroCol = 0; nroCol < grillaAcum.Columns.Count; nroCol++)
                    {
                        this.grillaAcum.Columns[nroCol].Visible = false;
                    }
                    this.grillaAcum.Columns["Codigo"].Visible = true;
                    this.grillaAcum.Columns["Corte"].Visible = true;
                    this.grillaAcum.Columns["CantKgs"].Visible = true;

                    this.grillaAcum.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

                    grillaAcum.Columns["Codigo"].DisplayIndex = 0;
                    grillaAcum.Columns["Corte"].DisplayIndex = 1;
                    break;
            }
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
