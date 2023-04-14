using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Configuration;

namespace Presentacion.Balanza
{
    public partial class formBalanza : Form
    {

        Utilidades.SingletonLeerPeso Leer_Peso = Utilidades.SingletonLeerPeso.CrearLeerPeso();
        public formBalanza()
        {
            InitializeComponent();
        }

        private void formBalanza_Load(object sender, EventArgs e)
        {
            this.Text += Utilidades.Conexion.getSucursalConexion();
            txtVelocidadTimer.Text = timer1.Interval.ToString();
        }

        private void btnTimer_Click(object sender, EventArgs e)
        {
            if (timer1.Enabled)
            {
                timer1.Stop();
                txtVelocidadTimer.ReadOnly = false;
                btnTimer.Text = "Start";
            }
            else
            {
                timer1.Interval = Convert.ToInt32(txtVelocidadTimer.Text);
                timer1.Start();
                txtVelocidadTimer.ReadOnly = true;
                btnTimer.Text = "Stop";
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            try
            {
                if (Convert.ToBoolean(ConfigurationManager.AppSettings["singleton"].ToString()))
                {
                    Leer_Peso = Utilidades.SingletonLeerPeso.CrearLeerPeso();
                    txtPesoBalanza.Text = Leer_Peso.ObtenerPeso();
                }
                else
                {
                    txtPesoBalanza.Text = Utilidades.Util_Form.leerPesoBalanza();
                }
            }
            catch (Exception ex)
            {
                timer1.Enabled = false;
                if (Utilidades.Util_Form.errorBalanza(ex.Message) == DialogResult.Yes)
                {
                    
                }
                else
                {
                }
            }
        }

        private void btnVerBalanza_Click(object sender, EventArgs e)
        {            
            //Leer_Peso.Show();
            try
            {
                Leer_Peso = Utilidades.SingletonLeerPeso.CrearLeerPeso();
            }
            catch (Exception)
            {
            }
        }

        private void txtNuevo_Click(object sender, EventArgs e)
        {
            formBalanza frm = new formBalanza();
            frm.Show();
        }
    }
}
