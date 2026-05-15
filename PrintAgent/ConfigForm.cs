using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace CarniSys.PrintAgent
{
    internal sealed class ConfigForm : Form
    {
        private readonly ComboBox _cmbPrinters;
        private readonly ComboBox _cmbMm;
        private readonly Button _btnSave;
        private readonly Button _btnCancel;

        public AgentConfig ResultConfig { get; private set; }

        public ConfigForm(IEnumerable<PrinterInfo> printers, AgentConfig current)
        {
            Text = "CarniSys - Configurar impresora";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterScreen;
            MaximizeBox = false;
            MinimizeBox = false;
            ClientSize = new Size(420, 180);

            var lblPrinter = new Label
            {
                Left = 20,
                Top = 20,
                Width = 150,
                Text = "Impresora ticket"
            };

            _cmbPrinters = new ComboBox
            {
                Left = 20,
                Top = 42,
                Width = 370,
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            var printerList = (printers ?? new List<PrinterInfo>()).ToList();
            _cmbPrinters.DataSource = printerList;
            _cmbPrinters.DisplayMember = "Name";
            _cmbPrinters.ValueMember = "Name";

            var lblMm = new Label
            {
                Left = 20,
                Top = 82,
                Width = 150,
                Text = "Tamaño ticket"
            };

            _cmbMm = new ComboBox
            {
                Left = 20,
                Top = 104,
                Width = 120,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            _cmbMm.Items.Add("58");
            _cmbMm.Items.Add("80");

            _btnSave = new Button
            {
                Left = 214,
                Top = 132,
                Width = 85,
                Text = "Guardar"
            };
            _btnSave.Click += SaveClick;

            _btnCancel = new Button
            {
                Left = 305,
                Top = 132,
                Width = 85,
                Text = "Cancelar"
            };
            _btnCancel.Click += (s, e) => Close();

            Controls.Add(lblPrinter);
            Controls.Add(_cmbPrinters);
            Controls.Add(lblMm);
            Controls.Add(_cmbMm);
            Controls.Add(_btnSave);
            Controls.Add(_btnCancel);

            current = current ?? new AgentConfig();
            if (!string.IsNullOrWhiteSpace(current.PrinterName))
            {
                _cmbPrinters.SelectedValue = current.PrinterName;
            }
            else
            {
                var defaultPrinter = printerList.FirstOrDefault(x => x.IsDefault);
                if (defaultPrinter != null)
                {
                    _cmbPrinters.SelectedValue = defaultPrinter.Name;
                }
            }

            _cmbMm.SelectedItem = (current.TicketMm == 80 ? 80 : 58).ToString();
        }

        private void SaveClick(object sender, EventArgs e)
        {
            var printerName = Convert.ToString(_cmbPrinters.SelectedValue ?? "");
            if (string.IsNullOrWhiteSpace(printerName))
            {
                MessageBox.Show(this, "Debe seleccionar una impresora.", "Impresión", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int mm;
            if (!int.TryParse(Convert.ToString(_cmbMm.SelectedItem ?? "58"), out mm))
            {
                mm = 58;
            }

            ResultConfig = new AgentConfig
            {
                PrinterName = printerName,
                TicketMm = mm == 80 ? 80 : 58
            };

            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
