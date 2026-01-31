using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Presentacion.Compras
{
    public partial class formCambiarPrecioKg : Form
    {
        Negocio.Compra oCompraN = new Negocio.Compra(FormPrincipal.EmpresaSTATIC, FormPrincipal.ParametrosCTX);
        int idCompra;
        formModificarCompra frmModificarCompra;

        public formCambiarPrecioKg(formModificarCompra formParam, int idCompraParam)
        {
            InitializeComponent(); this.Icon = Properties.Resources.CarniSys_ICONO;

            idCompra = idCompraParam;
            frmModificarCompra = formParam;
        }
        private void cambiarPrecioKg()
        {
           
            try
            {
                float precioKg;
                try
                {
                    precioKg = float.Parse(txtPrecioKg.Text.Trim(), System.Globalization.NumberStyles.Float, new System.Globalization.CultureInfo("en-US"));
                }
                catch (Exception)
                {

                    precioKg = float.Parse(txtPrecioKg.Text.Trim());

                }

                oCompraN.modificarPrecioMedia(idCompra, precioKg);
                frmModificarCompra.actualizarListas();
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void btnGuardar_Click(object sender, EventArgs e)
        {
            cambiarPrecioKg();
        }
    }
}
