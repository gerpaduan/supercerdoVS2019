using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Presentacion.Balanza
{
    public partial class formBalanza : Form
    {

        Utilidades.FormLeer_Peso Leer_Peso = Utilidades.FormLeer_Peso.CrearLeerPeso();

        public formBalanza()
        {
            InitializeComponent();
        }

        private void formBalanza_Load(object sender, EventArgs e)
        {
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
            txtPesoBalanza.Text = Leer_Peso.ObtenerPeso();
        }

        private void btnVerBalanza_Click(object sender, EventArgs e)
        {

            
            //Leer_Peso.Show();
            try
            {
                Leer_Peso.Show();
            }
            catch (Exception)
            {
                //Leer_Peso.Dispose();
                Leer_Peso = Utilidades.FormLeer_Peso.CrearLeerPeso();
                Leer_Peso.Show();
            }
        }
    }
}
