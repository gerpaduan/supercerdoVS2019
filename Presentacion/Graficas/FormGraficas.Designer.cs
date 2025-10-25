
namespace Presentacion.Graficas
{
    partial class FormGraficas
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea1 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend1 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.Series series1 = new System.Windows.Forms.DataVisualization.Charting.Series();
            this.chartVentasDiarias = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.comboBoxDias = new System.Windows.Forms.ComboBox();
            this.txtFecha = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.panelSuperior = new System.Windows.Forms.Panel();
            this.txtSucursal = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.chartVentasDiarias)).BeginInit();
            this.panelSuperior.SuspendLayout();
            this.SuspendLayout();
            // 
            // chartVentasDiarias
            // 
            this.chartVentasDiarias.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            chartArea1.Name = "ChartArea1";
            this.chartVentasDiarias.ChartAreas.Add(chartArea1);
            legend1.Name = "Legend1";
            this.chartVentasDiarias.Legends.Add(legend1);
            this.chartVentasDiarias.Location = new System.Drawing.Point(12, 77);
            this.chartVentasDiarias.Name = "chartVentasDiarias";
            series1.ChartArea = "ChartArea1";
            series1.Legend = "Legend1";
            series1.Name = "Series1";
            this.chartVentasDiarias.Series.Add(series1);
            this.chartVentasDiarias.Size = new System.Drawing.Size(776, 361);
            this.chartVentasDiarias.TabIndex = 0;
            this.chartVentasDiarias.Text = "chart1";
            // 
            // comboBoxDias
            // 
            this.comboBoxDias.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.comboBoxDias.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.comboBoxDias.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.comboBoxDias.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.comboBoxDias.Location = new System.Drawing.Point(12, 3);
            this.comboBoxDias.Name = "comboBoxDias";
            this.comboBoxDias.Size = new System.Drawing.Size(197, 28);
            this.comboBoxDias.TabIndex = 1;
            this.comboBoxDias.SelectedIndexChanged += new System.EventHandler(this.comboBoxDias_SelectedIndexChanged);
            // 
            // txtFecha
            // 
            this.txtFecha.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.txtFecha.Font = new System.Drawing.Font("Microsoft Sans Serif", 13.75F, System.Drawing.FontStyle.Bold);
            this.txtFecha.Location = new System.Drawing.Point(420, 40);
            this.txtFecha.Name = "txtFecha";
            this.txtFecha.ReadOnly = true;
            this.txtFecha.Size = new System.Drawing.Size(376, 28);
            this.txtFecha.TabIndex = 30;
            this.txtFecha.TabStop = false;
            // 
            // label7
            // 
            this.label7.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label7.ForeColor = System.Drawing.Color.Cornsilk;
            this.label7.Location = new System.Drawing.Point(360, 45);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(54, 20);
            this.label7.TabIndex = 29;
            this.label7.Text = "Fecha";
            // 
            // panelSuperior
            // 
            this.panelSuperior.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelSuperior.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(33)))), ((int)(((byte)(150)))), ((int)(((byte)(243)))));
            this.panelSuperior.Controls.Add(this.label1);
            this.panelSuperior.Controls.Add(this.txtSucursal);
            this.panelSuperior.Controls.Add(this.comboBoxDias);
            this.panelSuperior.Controls.Add(this.txtFecha);
            this.panelSuperior.Controls.Add(this.label7);
            this.panelSuperior.Location = new System.Drawing.Point(0, 0);
            this.panelSuperior.Name = "panelSuperior";
            this.panelSuperior.Size = new System.Drawing.Size(799, 71);
            this.panelSuperior.TabIndex = 71;
            // 
            // txtSucursal
            // 
            this.txtSucursal.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.txtSucursal.Font = new System.Drawing.Font("Microsoft Sans Serif", 13.75F, System.Drawing.FontStyle.Bold);
            this.txtSucursal.Location = new System.Drawing.Point(606, 6);
            this.txtSucursal.Name = "txtSucursal";
            this.txtSucursal.ReadOnly = true;
            this.txtSucursal.Size = new System.Drawing.Size(190, 28);
            this.txtSucursal.TabIndex = 69;
            this.txtSucursal.TabStop = false;
            // 
            // label1
            // 
            this.label1.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ForeColor = System.Drawing.Color.Cornsilk;
            this.label1.Location = new System.Drawing.Point(529, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(71, 20);
            this.label1.TabIndex = 70;
            this.label1.Text = "Sucursal";
            // 
            // FormGraficas
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.panelSuperior);
            this.Controls.Add(this.chartVentasDiarias);
            this.Name = "FormGraficas";
            this.Text = "Graficas";
            this.Load += new System.EventHandler(this.FormGraficas_Load);
            ((System.ComponentModel.ISupportInitialize)(this.chartVentasDiarias)).EndInit();
            this.panelSuperior.ResumeLayout(false);
            this.panelSuperior.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataVisualization.Charting.Chart chartVentasDiarias;
        private System.Windows.Forms.ComboBox comboBoxDias;
        protected System.Windows.Forms.TextBox txtFecha;
        protected System.Windows.Forms.Label label7;
        private System.Windows.Forms.Panel panelSuperior;
        protected System.Windows.Forms.Label label1;
        protected System.Windows.Forms.TextBox txtSucursal;
    }
}