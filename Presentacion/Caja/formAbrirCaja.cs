using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Presentacion.Caja
{
    public partial class formAbrirCaja : Presentacion.Caja.formCerrarCaja
    {     
        public formAbrirCaja()
        {
            InitializeComponent();
            tipoCierreActual = tipoCierre.AbrirCaja;
        }

        private void formAbrirCaja_Load(object sender, EventArgs e)
        {
            txtFechaHoraInicio.Text = DateTime.Now.ToString();
            txtFechaHoraCierre.Text = "";
            txtDiferencia.Text = "";
            txtImporteRetirado.Text = "";
        }

    }
}
