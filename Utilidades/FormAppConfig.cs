using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Configuration;

namespace Utilidades
{
    public partial class FormAppConfig : Form
    {

        Label lbl = new Label();
        TextBox txtBox = new TextBox();
        public FormAppConfig()
        {
            InitializeComponent();
        }

        private void FormAppConfig_Load(object sender, EventArgs e)
        {
            Configuration config =
                ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

            var result = (from configKey in ConfigurationManager.AppSettings.Keys.Cast<string>()
                          let configValue = ConfigurationManager.AppSettings[configKey]
                          select new
                          {
                              key = configKey,
                              value = configValue
                          }).ToList();

            int horizontal = 10;
            int verticalInicio = 70;
            int vertical = verticalInicio;
            int count = 0;
            int cambiarColumn = 20;
            foreach (var setting in result)
            {
                count++;                
                if ((count % 2) != 0)
                {
                    lbl = new Label();
                    lbl.Name = setting.key.ToString();
                    lbl.Text = setting.value.ToString();
                    lbl.Size = new Size(400, 20);
                    lbl.Location = new Point(horizontal, vertical);
                    lbl.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, FontStyle.Bold);
                    this.Controls.Add(lbl);                    
                }
                else
                {
                    lbl = new Label();
                    lbl.Name = setting.key.ToString();
                    lbl.Text = setting.key.ToString();
                    lbl.Location = new Point(horizontal, vertical);
                    lbl.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
                    this.Controls.Add(lbl);

                    txtBox = new TextBox();
                    txtBox.Name = setting.key.ToString();
                    txtBox.Text = setting.value.ToString();
                    txtBox.Size = new Size(150, 20);
                    txtBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
                    txtBox.Location = new System.Drawing.Point(horizontal+120, vertical);
                    this.Controls.Add(txtBox);
                }
                vertical += 25; 
                if (count.Equals(cambiarColumn))
                {
                    cambiarColumn += cambiarColumn;
                    horizontal += 400;
                    vertical = verticalInicio;
                }
            }
        }

        private void btnGuardar_Click(object sender, EventArgs e)
        {
            try
            {
                if (Application.OpenForms.Count == 2)
                {
                    DialogResult respuesta = MessageBox.Show("¿Está seguro que desea modificar la configuración?. ", "App.config", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);

                    if ((respuesta == System.Windows.Forms.DialogResult.Yes))
                    {
                        Configuration config =
                               ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

                        var result = (from configKey in ConfigurationManager.AppSettings.Keys.Cast<string>()
                                      let configValue = ConfigurationManager.AppSettings[configKey]
                                      select new
                                      {
                                          key = configKey,
                                          value = configValue
                                      }).ToList();

                        foreach (Control control in this.Controls)
                        {
                            if (control.GetType().Equals(typeof(TextBox)))
                            {
                                foreach (var setting in result)
                                {
                                    if (control.Name.Equals(setting.key))
                                    {
                                        config.AppSettings.Settings[setting.key].Value = control.Text;
                                        config.Save(ConfigurationSaveMode.Modified);
                                        //MessageBox.Show("Valores anteriores:\nKey: " + control.Name + "  - Texto: " + control.Text +
                                        //    "\n\nNuevos valores: \nKey: " + setting.key.ToString() + "  - Texto: " + setting.value.ToString());
                                        break;
                                    }
                                }
                            }
                        }

                        respuesta = MessageBox.Show("Los cambios en la configuración se registraron correctamente.\n" +
                            "Para que los cambios se apliquen debe reiniciar la aplicación.\n\n¿Reinicar ahora la aplicación?",
                            "Archivo de configuaración", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);

                        if ((respuesta == System.Windows.Forms.DialogResult.Yes))
                        {
                            Application.Restart();
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Debe cerrar las demás ventanas para modificar el archivo de configuración");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al actualizar App.config" + ex.Message);
            }
        }
    }
}
