using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Presentacion.Ventas;
using Presentacion.Caja;
using iTextSharp.text.pdf;
using iTextSharp.text;
using System.IO;

namespace Presentacion.Licencia
{
    public partial class formVencimientoCuotas : Form, InterfaceUsuario
    {
        Entidades.Usuario oUsuario;

        public int idPersona;
        DataTable dtMov;
        Entidades.Persona oPersonaE;
        DateTime fechaDesde = DateTime.Now;
        Negocio.OtrasClases otrasClasesN = new Negocio.OtrasClases();   

        public formVencimientoCuotas()
        {
            InitializeComponent(); this.Icon = Properties.Resources.CarniSys_ICONO;
        }

        private void formVencimientoCuotas_Load(object sender, EventArgs e)
        {
            try
            {
                fechaDesdePick.Value = fechaDesde;
                cargarGrilla();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void cargarGrilla()
        {
            try
            {
                grilla.DataSource = otrasClasesN.obtenerVencimientoLicencia(fechaDesdePick.Value);

                foreach (DataGridViewColumn column in grilla.Columns)
                {
                    column.SortMode = DataGridViewColumnSortMode.NotSortable;
                }

                menuGenerarCuotas.Enabled = grilla.Rows.Count == 0;

                // Asegúrate de que ninguna fila quede seleccionada al inicio
                grilla.ClearSelection();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnSeleccionar_Click(object sender, EventArgs e)
        {
            try
            {
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }

        private void btnCancelar_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnBuscar_Click(object sender, EventArgs e)
        {
            cargarGrilla();
        }
       
        public void EnviarUsuario(Entidades.Usuario usuario)
        {
            oUsuario = usuario;
        }

        private void checkSinRegRepetidos_CheckedChanged(object sender, EventArgs e)
        {
            cargarGrilla();
        }

        private void fechaDesdePick_KeyDown(object sender, KeyEventArgs e)
        {
                cargarGrilla();
        }

        private void fechaDesdePick_ValueChanged(object sender, EventArgs e)
        {
        }

        private void menuGenerarCuotas_Click(object sender, EventArgs e)
        {
            Presentacion.Caja.FormLoginVendedor frmLogin = new Presentacion.Caja.FormLoginVendedor();
            frmLogin.ShowDialog(this);
            if (oUsuario == null) return;
            if (!(oUsuario.User.ToLower() ==  "admin"))
            {
                MessageBox.Show("No tienes permiso para generar la cuotas");
                return;
            }

            DialogResult resp = MessageBox.Show("¿Generar cuotas mensuales a partir del "+ fechaDesdePick.Value.ToString("dd/MM/yyyy")+"?", "Generar Cuotas", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
            if (resp == DialogResult.Yes)
            {
                otrasClasesN.agregaVencimientosLicencia(fechaDesdePick.Value);
                cargarGrilla();
            }
        }

        private void btnAgregar_Click(object sender, EventArgs e)
        {

            if (string.IsNullOrEmpty(txtFechaVencimiento.Text))
                return;

            //Valido que no haya un pago en la fecha actual porque solo se permite registrar un pago por fecha por el tema de la validacion
            DateTime menorFecha = DateTime.MaxValue;

            foreach (DataGridViewRow fila in grilla.Rows)
            {
                if (fila.Cells["fechaVencimiento"].Value != null)
                {
                    DateTime fechaActual = Convert.ToDateTime(fila.Cells["fechaVencimiento"].Value);

                    // Compara si la fecha actual es menor que la menor fecha encontrada
                    if (fechaActual < menorFecha && fila.Cells["pagado"].Value.ToString().Equals("PENDIENTE"))
                    {
                        menorFecha = fechaActual;
                    }
                }
            }

            if (menorFecha != Convert.ToDateTime(txtFechaVencimiento.Text))
            {
                MessageBox.Show("Debe seleccionar el registro con la menor fecha para registrar el pago. Fecha: "+menorFecha.ToString("dd/MM/yyyy"));
                return;
            }

            // Obtener el nombre completo del día de la semana actual
            string nombreDia = DateTime.Now.DayOfWeek.ToString();
            string letraDia = nombreDia[0].ToString();
            int ultimosTresCuit = Convert.ToInt32(FormPrincipal.cuitCliente.Substring(FormPrincipal.cuitCliente.Length - 3));
            int ultimosDosCuit = Convert.ToInt32(FormPrincipal.cuitCliente.Substring(FormPrincipal.cuitCliente.Length - 2));
            string day = DateTime.Now.Day > 9 ? DateTime.Now.Day.ToString() : "0" + DateTime.Now.Day.ToString();
            DateTime fechaVenc = Convert.ToDateTime(txtFechaVencimiento.Text);
            string por7 = ((DateTime.Now.Year + DateTime.Now.Day + DateTime.Now.Month + fechaVenc.Year + fechaVenc.Month + ultimosTresCuit) * 1007).ToString();

            //string claveSistema = letraDia + DateTime.Now.Month.ToString() + day + por7; //ConfigurationManager.AppSettings["admin"].ToString();

            int multiplicarFecha_Div_ultimosCuit = ((DateTime.Now.Year * DateTime.Now.Month * DateTime.Now.Day) / ultimosDosCuit);
            string claveSistema = letraDia + multiplicarFecha_Div_ultimosCuit + DateTime.Now.Day.ToString();
            string clave = txtClave.Text.Trim();
            if (clave.ToUpper().Equals(claveSistema.ToUpper()))
            {
                if (otrasClasesN.existePagoLicenciaHoy())
                {
                    MessageBox.Show("Sólo puede realizar un solo pago por fecha", "",MessageBoxButtons.OK,MessageBoxIcon.Information);
                    return;
                }

                otrasClasesN.agregarPagoCuota(Convert.ToDateTime(txtFechaVencimiento.Text));
                MessageBox.Show("Pago registrado.");
                cargarGrilla();
            }
            else
            {
                //MessageBox.Show("Código Incorrecto.\n\nDing-mA-dA-(3lastcuit+y+m+d+yV+mV*1007)\nCuit Cliente:"+ FormPrincipal.cuitCliente);
                MessageBox.Show("Código Incorrecto.\n\nletterday_concat_[trun((y*m*d)/2cuit)]_concat_d\nCuit Cliente:" + FormPrincipal.cuitCliente);
                txtClave.Focus();
            }
        }

        private void grilla_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                DataGridViewRow fila = grilla.CurrentRow;

                if (fila != null)
                {
                    txtFechaVencimiento.Text = fila.Cells["fechaVencimiento"].Value.ToString();
                    txtEstado.Text = fila.Cells["pagado"].Value.ToString() ;
                    
                    btnAgregar.Enabled = txtEstado.Text.Equals("PENDIENTE");                    
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Error al seleccionar fila");
            }
        }

        private void txtFechaVencimiento_TextChanged(object sender, EventArgs e)
        {
        }
    }
}
