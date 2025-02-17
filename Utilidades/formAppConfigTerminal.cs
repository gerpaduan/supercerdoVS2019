using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Configuration;
using System.Drawing.Printing;


namespace Utilidades
{
    public partial class formAppConfigTerminal : Form
    {
        const string si = "SI";
        const string no = "NO";
        const string true_ = "true";
        const string false_ = "false";

        public formAppConfigTerminal()
        {
            InitializeComponent();
        }

        private void formAppConfigTerminal_Load(object sender, EventArgs e)
        {
            foreach (string printer in PrinterSettings.InstalledPrinters)
            {
                cbImpresoras.Items.Add(printer);
            }

            // Seleccionar la impresora predeterminada por defecto
            cbImpresoras.SelectedIndex = cbImpresoras.Items.IndexOf(new PrintDocument().PrinterSettings.PrinterName);


            this.Text += Utilidades.Conexion.getSucursalConexion();
            Configuration config =
                ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

            var result = (from configKey in ConfigurationManager.AppSettings.Keys.Cast<string>()
                          let configValue = ConfigurationManager.AppSettings[configKey]
                          select new
                          {
                              key = configKey,
                              value = configValue
                          }).ToList();

            foreach (var key in ConfigurationManager.AppSettings.AllKeys)
            {
                Control[] controls = this.Controls.Find(key, true);
                if (controls.Length > 0)
                {
                    if (controls[0] is TextBox textBox)
                    {
                        textBox.Text = ConfigurationManager.AppSettings[key];
                    }
                    else if (controls[0] is ComboBox comboBox)
                    {
                        string valorCombo = ConfigurationManager.AppSettings[key];
                        if (valorCombo.Equals(true_) || valorCombo.Equals(false_))
                            valorCombo = valorCombo.Equals(true_) ? si : no;

                            comboBox.SelectedItem = valorCombo;
                    }
                }
            }
        }

        private void btnGuardar_Click(object sender, EventArgs e)
        {
            try
            {
                    DialogResult respuesta = MessageBox.Show("¿Está seguro que desea modificar la configuración?. ", "App.config", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);

                    if ((respuesta == System.Windows.Forms.DialogResult.Yes))
                    {
                        Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

                        foreach (var key in ConfigurationManager.AppSettings.AllKeys)
                        {
                            Control[] controls = this.Controls.Find(key, true);
                            if (controls.Length > 0)
                            {
                                if (controls[0] is TextBox textBox)
                                {
                                    config.AppSettings.Settings[key].Value = textBox.Text;
                                }
                                else if (controls[0] is ComboBox comboBox && comboBox.SelectedItem != null)
                                {
                                    string valorCombo = comboBox.SelectedItem.ToString();
                                    if (valorCombo.Equals(si) || valorCombo.Equals(no))
                                        valorCombo = valorCombo.Equals(si) ? true_ : false_;

                                    config.AppSettings.Settings[key].Value = valorCombo;
                                }
                            }
                        }

                        config.Save(ConfigurationSaveMode.Modified);
                        ConfigurationManager.RefreshSection("appSettings");

                        respuesta = MessageBox.Show("Los cambios en la configuración se registraron correctamente.\n" +
                                "Para que los cambios se apliquen debe reiniciar la aplicación.\n\n¿Reinicar ahora la aplicación?",
                                "Archivo de configuaración", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);

                        if ((respuesta == System.Windows.Forms.DialogResult.Yes))
                        {
                            Application.Restart();
                        }
                    }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al actualizar App.config" + ex.Message);
            }
        }

        private void label16_Click(object sender, EventArgs e)
        {

        }

        private void btnBuscarAfip_Click(object sender, EventArgs e)
        {
            try
            {
                //string cuitSinGuiones = txtCuit.Text.Replace("-", "");
                //wsAFIPvs2008.formFacturaElectronica formFactElec = new wsAFIPvs2008.formFacturaElectronica();
                //formFactElec.loadForm();
                //formFactElec.ConsultarDatosContribuyente(cuitSinGuiones);
                //txtRazonSocial.Text = txtIdentificacion.Text = formFactElec.razonSocialAfip;
                //txtDomicilio.Text = formFactElec.domicilioFiscalAfip;
                //txtCiudad.Text = formFactElec.localidadAfip + ", " + formFactElec.provinciaAfip;
                //comboIva.SelectedIndex = -1;
            }
            catch (Exception ex)
            {
                MessageBox.Show("No se pudo obtener los datos desde Afip. " + ex.Message);
            }
        }

        private void cuitCliente_TextChanged(object sender, EventArgs e)
        {
            cuit.Text = cuitCliente.Text;
            NegocioAgregado4.Text = "CUIT: " + cuitCliente.Text;

        }

        private void cliente_TextChanged(object sender, EventArgs e)
        {
            Negocio.Text = cliente.Text;
        }

        private void cbImpresoras_SelectedIndexChanged(object sender, EventArgs e)
        {
            string nombreEquipo = Environment.MachineName;

            Impresora.Text = "\\\\" + nombreEquipo + "\\" + cbImpresoras.Text;
            ImpresoraEtiqueta.Text = "\\\\" + nombreEquipo + "\\" + cbImpresoras.Text;
        }
    }
}
