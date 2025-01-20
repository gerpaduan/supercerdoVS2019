using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Presentacion.Caja
{
    public partial class FormSelectPuntoExpendio : Form
    {
        Negocio.Venta oVentaN = new Negocio.Venta();
        public bool soloActivos = false;

        public FormSelectPuntoExpendio()
        {
            InitializeComponent(); this.Icon = Properties.Resources.CarniSys_ICONO;
        }

        private void FormSelectPuntoExpendio_Load(object sender, EventArgs e)
        {
            this.Text += Utilidades.Conexion.getSucursalConexion();
            cargarCombo();
        }

        private void cargarCombo()
        {
            //al cargar combo se selecciona el último que se seleccionó en esta terminal
            //obteniendo este valor desde la tabla Licencias donde se guarda el serial cpu y el sector
            string ultimoSectorSelect = oVentaN.getUltimoSectorSelect(Utilidades.Util_Form.GetCPUId());

            comboSector.DataSource = oVentaN.obtenerSectores();
            comboSector.DisplayMember = "sector";
            comboSector.ValueMember = "sector";
            comboSector.Text = ultimoSectorSelect;

            //si existe sector se hace foco en boton, sino en combo
            if (!string.IsNullOrEmpty(ultimoSectorSelect))
                btnIngresar.Select();
            else
                comboSector.Select();
        }


        public void EnviarSector(string sector)
        {
            if (this.Owner is InterfaceSector formInterface)
            {
                formInterface.EnviarSector(sector);
            }
            this.Close();
        }

        private void btnIngresar_Click(object sender, EventArgs e)
        {
            ingresar();          
        }

        private void ingresar()
        {
            if (!string.IsNullOrEmpty(comboSector.Text))
            {
                EnviarSector(comboSector.Text);
            }
            else
            {
                MessageBox.Show("Seleccione un sector para el punto de expendio.", "Seleccione un sector", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void txtClave_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)(Keys.Enter))
            {
                ingresar();
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
