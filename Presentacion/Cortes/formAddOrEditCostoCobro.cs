using Entidades;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Utilidades;

namespace Presentacion.Cortes
{
    public partial class formAddOrEditCostoCobro : Form, InterfaceUsuario
    {
        public Entidades.Usuario oUsuario;

        Negocio.Corte oCorteN = new Negocio.Corte(FormPrincipal.EmpresaSTATIC, FormPrincipal.ParametrosCTX);
        Negocio.OtrasClases oOtrasClasesN = new Negocio.OtrasClases(FormPrincipal.EmpresaSTATIC, FormPrincipal.ParametrosCTX);
        DataTable dtParametros = new DataTable();

        public string tipoProductoSelected = "";
        public string ordenSelected = "";
        public bool esInsert = false;
        ToolTip toolTip = new ToolTip();
        bool readOnly = false;
        bool huboModificacion = false;
        public bool egresoDesdeCajaVenta = false;

        public formAddOrEditCostoCobro()
        {
            InitializeComponent(); this.Icon = Properties.Resources.CarniSys_ICONO;
        }

        private void formAddOrEditCostoCobro_Load(object sender, EventArgs e)
        {
            this.Text += Utilidades.Conexion.getSucursalConexion();
            try
            {
                //validar que sea Admin
                if (!Usuarios.FormValidarPermiso.validarPermiso(this.Name))
                {
                    this.Close();
                }

                //al guardar informa que tiene que volver a abrir la aplicacion para que se actualicen los valores
                //dtParametros = oOtrasClasesN.obtenerParametrosDt();
                //for (int fila = 0; fila < dtParametros.Rows.Count; fila++)
                //{
                //    switch (dtParametros.Rows[fila]["nombre"].ToString())
                //    {
                //        case "porcAjEfectivo":
                //            txtEfectivo.Text = ((decimal.Parse(dtParametros.Rows[fila][2].ToString()) - 1) * 100).ToString();
                //            break;
                //        case "porcAjDebito":
                //            txtDebito.Text = ((decimal.Parse(dtParametros.Rows[fila][2].ToString()) - 1) * 100).ToString();
                //            break;
                //        case "porcAjCredito":
                //            txtCredito.Text = ((decimal.Parse(dtParametros.Rows[fila][2].ToString()) - 1) * 100).ToString();
                //            break;
                //        case "porcAjQr":
                //            txtQr.Text = ((decimal.Parse(dtParametros.Rows[fila][2].ToString()) - 1) * 100).ToString();
                //            break;
                //        case "porcAjTranf":
                //            txtTransferencia.Text = ((decimal.Parse(dtParametros.Rows[fila][2].ToString()) - 1) * 100).ToString();
                //            break;
                //    }
                //}

                // Cultura actual (Argentina: coma decimal)
                var ci = CultureInfo.CurrentCulture;
                // porcAjX está guardado como factor (ej 1.05). En pantalla querés %: (1.05 - 1) * 100 = 5
                txtEfectivo.Text = ((FormPrincipal.ParametrosCTX.GetDecimal(Entidades.ParamKeys.PorcAjEfectivo, 1m) - 1m) * 100m).ToString(ci);
                txtDebito.Text = ((FormPrincipal.ParametrosCTX.GetDecimal(Entidades.ParamKeys.PorcAjDebito, 1m) - 1m) * 100m).ToString(ci);
                txtCredito.Text = ((FormPrincipal.ParametrosCTX.GetDecimal(Entidades.ParamKeys.PorcAjCredito, 1m) - 1m) * 100m).ToString(ci);
                txtQr.Text = ((FormPrincipal.ParametrosCTX.GetDecimal(Entidades.ParamKeys.PorcAjQr, 1m) - 1m) * 100m).ToString(ci);
                txtTransferencia.Text =
                                  ((FormPrincipal.ParametrosCTX.GetDecimal(Entidades.ParamKeys.PorcAjTranf, 1m) - 1m) * 100m).ToString(ci);

            }
            catch (Exception ex)
            {
                MessageBox.Show("Error en evento Load()\n" + ex.Message);
            }
        }

        public void EnviarUsuario(Entidades.Usuario usuario)
        {
            oUsuario = usuario;
        }

        private void btnAceptar_Click(object sender, EventArgs e)
        {
            addOrEdit();         
        }

        private void addOrEdit()
        {
            try
            {
               
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al guardar tipo egreso de caja.\n\n" + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnCancelar_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Escape)
            {
                this.Close();
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void btnGuardar_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(txtEfectivo.Text) || string.IsNullOrEmpty(txtDebito.Text) || string.IsNullOrEmpty(txtCredito.Text) || 
                    string.IsNullOrEmpty(txtQr.Text) || string.IsNullOrEmpty(txtTransferencia.Text) )
                {
                    MessageBox.Show("Los campos no pueden estar vacíos y deben tener un valor numérico", "",MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    return;
                }

                for (int fila = 0; fila < dtParametros.Rows.Count; fila++)
                {
                    switch (dtParametros.Rows[fila]["nombre"].ToString())
                    {
                        case "porcAjEfectivo":
                            dtParametros.Rows[fila]["valor"] = ((100 + decimal.Parse(txtEfectivo.Text)) / 100).ToString();
                            break;
                        case "porcAjDebito":
                            dtParametros.Rows[fila]["valor"] = ((100 + decimal.Parse(txtDebito.Text)) / 100).ToString();
                            break;
                        case "porcAjCredito":
                            dtParametros.Rows[fila]["valor"] = ((100 + decimal.Parse(txtCredito.Text)) / 100).ToString();
                            break;
                        case "porcAjQr":
                            dtParametros.Rows[fila]["valor"] = ((100 + decimal.Parse(txtQr.Text)) / 100).ToString();
                            break;
                        case "porcAjTranf":
                            dtParametros.Rows[fila]["valor"] = ((100 + decimal.Parse(txtTransferencia.Text)) / 100).ToString();
                            break;
                    }
                }

                //TODO: actualizar parametros
                //oOtrasClasesN.actualizarParametros(dtParametros);

                MessageBox.Show("La actualizacion de los costos por cobro se registró correctamente.\n\n"+
                    "Cierre y vuelva a abrir el Sistema para que los cambios impacten correctamente.");

                 this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al guardar el Costo por Cobro\n\n"+ ex.Message);
            }
        }

        private void btnCancelar_Click_1(object sender, EventArgs e)
        {
            this.Close();
        }
        private void txtNumerico_TextChanged(object sender, EventArgs e)
        {
            if (sender is TextBox)
            {
                TextBox txtNumerico = (TextBox)sender;
                //se reemplaza . por , para evitar error de multiplicacion precios
                int cursorPos = txtNumerico.SelectionStart;
                string originalText = txtNumerico.Text;
                txtNumerico.Text = originalText.Replace('.', ',');
                // Restaurar la posición del cursor
                txtNumerico.SelectionStart = cursorPos;
                //
                if (!validarCampoNumerico(validarSinSigno(txtNumerico.Text), txtNumerico.Name)) txtNumerico.Text = "";
                return;
            }

            if (sender is MaskedTextBox)
            {
                MaskedTextBox txtNumerico = (MaskedTextBox)sender;
                txtNumerico.Text = txtNumerico.Text.Replace('.', ',');
                if (!validarCampoNumerico(validarSinSigno(txtNumerico.Text), txtNumerico.Name)) txtNumerico.Text = "";
                return;
            }
        }

        private bool validarCampoNumerico(string valor, string nombreTextBox)
        {
            return string.IsNullOrEmpty(valor) ? true : Utilidades.Util_Form.validarCampoNumerico(valor, "El valor");
        }

        ///si el primer caracter es negativo lo saca para validar los demas caracteres <summary>
        private string validarSinSigno(string txtBox)
        {
            string valorCampo = !string.IsNullOrEmpty(txtBox) && (txtBox[0].Equals('-')) ? txtBox.Substring(1, txtBox.Length - 1) : txtBox;
            return valorCampo;
        }
    }
}
