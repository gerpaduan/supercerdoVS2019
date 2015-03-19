using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Configuration;

namespace Presentacion.Caja
{
    public partial class formAbrirCaja : Presentacion.Caja.formCerrarCaja
    {
        

        public formAbrirCaja()
        {
            InitializeComponent();
        }

        private void formAbrirCaja_Load(object sender, EventArgs e)
        {
            int idSucursal = Convert.ToInt32(ConfigurationManager.AppSettings["idSucursal"].ToString());
            oSucursalE = oSucursalN.findById(idSucursal);
            
            txtSucursal.Text = oSucursalE.sucursal;
            txtFechaHoraInicio.Text = DateTime.Now.ToString();
        }

        private void btnCerrarCaja_Click(object sender, EventArgs e)
        {
            abrirCaja();
        }

        private void abrirCaja()
        {
            oCierreE = new Entidades.CierreCaja();
            oCierreE.Sucursal = oSucursalE;            
        }
    }
}
