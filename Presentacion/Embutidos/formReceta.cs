using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Presentacion.Embutidos
{
    public partial class formReceta : Form
    {

        public string recetaEditada { get; private set; }
        public bool editar = false;
        public bool observaciones= false;
        public Action<string> OnObservaciones { get; set; }
        public formReceta(string textoReceta)
        {
            InitializeComponent(); this.Icon = Properties.Resources.CarniSys_ICONO;
            txtReceta.Text = textoReceta; // Cargar texto en el TextBox grande
        }

        private void formReceta_Load(object sender, EventArgs e)
        {
            if (observaciones)
            {
                this.Text = "Observación";
                lblEtiqueta.Text = "Observación";
                btnGuardar.Text = "Cargar";
            }

            txtReceta.ReadOnly = !editar;
            if (!editar)
                btnGuardar.Text = "Cerrar";
            this.StartPosition = FormStartPosition.CenterScreen; // Centrar en la pantalla
            btnGuardar.Select();
        }

        private void btnGuardar_Click(object sender, EventArgs e)
        {
            if (editar)
            {

                recetaEditada = txtReceta.Text; // Guardar cambios
                // Llamar al método externo si está asignado
                OnObservaciones?.Invoke(txtReceta.Text);
            }

            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
