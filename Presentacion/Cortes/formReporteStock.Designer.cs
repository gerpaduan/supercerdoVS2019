namespace Presentacion.Cortes
{
    partial class formReporteStock
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(formReporteStock));
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
            this.panel1 = new System.Windows.Forms.Panel();
            this.label1 = new System.Windows.Forms.Label();
            this.comboOrdenStock = new System.Windows.Forms.ComboBox();
            this.lblActualizar = new System.Windows.Forms.Label();
            this.btnExportar = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.fechaDesdeProgresivo = new System.Windows.Forms.DateTimePicker();
            this.txtFechaHastaProgresivo = new System.Windows.Forms.DateTimePicker();
            this.comboCierreStock = new System.Windows.Forms.ComboBox();
            this.comboInicioStock = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.btnBuscar = new System.Windows.Forms.Button();
            this.comboSucursal = new System.Windows.Forms.ComboBox();
            this.label5 = new System.Windows.Forms.Label();
            this.comboTipoReporte = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.txtDescripcion = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.btnSalir = new System.Windows.Forms.Button();
            this.barraControl = new System.Windows.Forms.ToolStrip();
            this.Imprimir = new System.Windows.Forms.ToolStripButton();
            this.grillaReportes = new System.Windows.Forms.DataGridView();
            this.panel1.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.barraControl.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.grillaReportes)).BeginInit();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(43)))), ((int)(((byte)(77)))), ((int)(((byte)(129)))));
            this.panel1.Controls.Add(this.label1);
            this.panel1.Controls.Add(this.comboOrdenStock);
            this.panel1.Controls.Add(this.lblActualizar);
            this.panel1.Controls.Add(this.btnExportar);
            this.panel1.Controls.Add(this.groupBox1);
            this.panel1.Controls.Add(this.btnBuscar);
            this.panel1.Controls.Add(this.comboSucursal);
            this.panel1.Controls.Add(this.label5);
            this.panel1.Controls.Add(this.comboTipoReporte);
            this.panel1.Controls.Add(this.label2);
            this.panel1.Controls.Add(this.txtDescripcion);
            this.panel1.Controls.Add(this.label9);
            this.panel1.Location = new System.Drawing.Point(-5, 44);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1007, 91);
            this.panel1.TabIndex = 12;
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ForeColor = System.Drawing.Color.Cornsilk;
            this.label1.Location = new System.Drawing.Point(888, 41);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(74, 15);
            this.label1.TabIndex = 55;
            this.label1.Text = "Orden Stock";
            // 
            // comboOrdenStock
            // 
            this.comboOrdenStock.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.comboOrdenStock.DisplayMember = "Sin Orden";
            this.comboOrdenStock.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboOrdenStock.FormattingEnabled = true;
            this.comboOrdenStock.Items.AddRange(new object[] {
            "Sin Orden",
            "Ascendente",
            "Descendente"});
            this.comboOrdenStock.Location = new System.Drawing.Point(891, 59);
            this.comboOrdenStock.Name = "comboOrdenStock";
            this.comboOrdenStock.Size = new System.Drawing.Size(99, 21);
            this.comboOrdenStock.TabIndex = 54;
            this.comboOrdenStock.SelectedValueChanged += new System.EventHandler(this.btnBuscar_Click);
            // 
            // lblActualizar
            // 
            this.lblActualizar.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblActualizar.AutoSize = true;
            this.lblActualizar.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblActualizar.ForeColor = System.Drawing.Color.LightSalmon;
            this.lblActualizar.Location = new System.Drawing.Point(335, 16);
            this.lblActualizar.Name = "lblActualizar";
            this.lblActualizar.Size = new System.Drawing.Size(69, 15);
            this.lblActualizar.TabIndex = 53;
            this.lblActualizar.Text = "Actualizar...";
            this.lblActualizar.Visible = false;
            // 
            // btnExportar
            // 
            this.btnExportar.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnExportar.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnExportar.Image = ((System.Drawing.Image)(resources.GetObject("btnExportar.Image")));
            this.btnExportar.Location = new System.Drawing.Point(789, 53);
            this.btnExportar.Name = "btnExportar";
            this.btnExportar.Size = new System.Drawing.Size(82, 31);
            this.btnExportar.TabIndex = 25;
            this.btnExportar.Text = "Exportar";
            this.btnExportar.TextImageRelation = System.Windows.Forms.TextImageRelation.TextBeforeImage;
            this.btnExportar.UseVisualStyleBackColor = true;
            this.btnExportar.Click += new System.EventHandler(this.btnExportar_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.fechaDesdeProgresivo);
            this.groupBox1.Controls.Add(this.txtFechaHastaProgresivo);
            this.groupBox1.Controls.Add(this.comboCierreStock);
            this.groupBox1.Controls.Add(this.comboInicioStock);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.ForeColor = System.Drawing.Color.Cornsilk;
            this.groupBox1.Location = new System.Drawing.Point(333, 37);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(450, 51);
            this.groupBox1.TabIndex = 24;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Fechas";
            // 
            // fechaDesdeProgresivo
            // 
            this.fechaDesdeProgresivo.CustomFormat = "dd/MM/yyyy  HH:mm:ss";
            this.fechaDesdeProgresivo.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.fechaDesdeProgresivo.Location = new System.Drawing.Point(77, 19);
            this.fechaDesdeProgresivo.Name = "fechaDesdeProgresivo";
            this.fechaDesdeProgresivo.Size = new System.Drawing.Size(151, 20);
            this.fechaDesdeProgresivo.TabIndex = 25;
            this.fechaDesdeProgresivo.ValueChanged += new System.EventHandler(this.txtFechaHastaProgresivo_ValueChanged);
            this.fechaDesdeProgresivo.KeyDown += new System.Windows.Forms.KeyEventHandler(this.fechaDesdeProgresivo_KeyDown);
            // 
            // txtFechaHastaProgresivo
            // 
            this.txtFechaHastaProgresivo.CustomFormat = "dd/MM/yyyy  HH:mm:ss";
            this.txtFechaHastaProgresivo.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.txtFechaHastaProgresivo.Location = new System.Drawing.Point(281, 20);
            this.txtFechaHastaProgresivo.Name = "txtFechaHastaProgresivo";
            this.txtFechaHastaProgresivo.Size = new System.Drawing.Size(151, 20);
            this.txtFechaHastaProgresivo.TabIndex = 24;
            this.txtFechaHastaProgresivo.ValueChanged += new System.EventHandler(this.txtFechaHastaProgresivo_ValueChanged);
            this.txtFechaHastaProgresivo.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtFechaHastaProgresivo_KeyDown);
            // 
            // comboCierreStock
            // 
            this.comboCierreStock.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboCierreStock.FormattingEnabled = true;
            this.comboCierreStock.Location = new System.Drawing.Point(280, 20);
            this.comboCierreStock.Name = "comboCierreStock";
            this.comboCierreStock.Size = new System.Drawing.Size(152, 21);
            this.comboCierreStock.TabIndex = 23;
            this.comboCierreStock.Visible = false;
            this.comboCierreStock.SelectedValueChanged += new System.EventHandler(this.comboCierreStock_SelectedValueChanged);
            // 
            // comboInicioStock
            // 
            this.comboInicioStock.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboInicioStock.FormattingEnabled = true;
            this.comboInicioStock.Location = new System.Drawing.Point(77, 20);
            this.comboInicioStock.Name = "comboInicioStock";
            this.comboInicioStock.Size = new System.Drawing.Size(152, 21);
            this.comboInicioStock.TabIndex = 22;
            this.comboInicioStock.Visible = false;
            this.comboInicioStock.SelectedValueChanged += new System.EventHandler(this.comboInicioStock_SelectedValueChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.ForeColor = System.Drawing.Color.Cornsilk;
            this.label4.Location = new System.Drawing.Point(235, 24);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(39, 15);
            this.label4.TabIndex = 14;
            this.label4.Text = "Hasta";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.ForeColor = System.Drawing.Color.Cornsilk;
            this.label3.Location = new System.Drawing.Point(28, 24);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(43, 15);
            this.label3.TabIndex = 12;
            this.label3.Text = "Desde";
            // 
            // btnBuscar
            // 
            this.btnBuscar.AccessibleDescription = "";
            this.btnBuscar.ForeColor = System.Drawing.Color.Black;
            this.btnBuscar.Image = ((System.Drawing.Image)(resources.GetObject("btnBuscar.Image")));
            this.btnBuscar.Location = new System.Drawing.Point(238, 52);
            this.btnBuscar.Name = "btnBuscar";
            this.btnBuscar.Size = new System.Drawing.Size(28, 23);
            this.btnBuscar.TabIndex = 21;
            this.btnBuscar.TabStop = false;
            this.btnBuscar.UseVisualStyleBackColor = true;
            this.btnBuscar.Click += new System.EventHandler(this.btnBuscar_Click);
            // 
            // comboSucursal
            // 
            this.comboSucursal.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboSucursal.FormattingEnabled = true;
            this.comboSucursal.Location = new System.Drawing.Point(95, 12);
            this.comboSucursal.Name = "comboSucursal";
            this.comboSucursal.Size = new System.Drawing.Size(137, 21);
            this.comboSucursal.TabIndex = 19;
            this.comboSucursal.SelectedValueChanged += new System.EventHandler(this.btnBuscar_Click);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.ForeColor = System.Drawing.Color.Cornsilk;
            this.label5.Location = new System.Drawing.Point(34, 16);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(55, 15);
            this.label5.TabIndex = 20;
            this.label5.Text = "Sucursal";
            // 
            // comboTipoReporte
            // 
            this.comboTipoReporte.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.comboTipoReporte.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboTipoReporte.FormattingEnabled = true;
            this.comboTipoReporte.Location = new System.Drawing.Point(831, 10);
            this.comboTipoReporte.Name = "comboTipoReporte";
            this.comboTipoReporte.Size = new System.Drawing.Size(159, 21);
            this.comboTipoReporte.TabIndex = 18;
            this.comboTipoReporte.SelectedValueChanged += new System.EventHandler(this.comboTipoReporte_SelectedValueChanged);
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.ForeColor = System.Drawing.Color.Cornsilk;
            this.label2.Location = new System.Drawing.Point(774, 12);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(51, 15);
            this.label2.TabIndex = 17;
            this.label2.Text = "Reporte";
            // 
            // txtDescripcion
            // 
            this.txtDescripcion.Location = new System.Drawing.Point(95, 53);
            this.txtDescripcion.Name = "txtDescripcion";
            this.txtDescripcion.Size = new System.Drawing.Size(137, 20);
            this.txtDescripcion.TabIndex = 0;
            this.txtDescripcion.TextChanged += new System.EventHandler(this.txtDescripcion_TextChanged);
            this.txtDescripcion.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtDescripcion_KeyDown);
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label9.ForeColor = System.Drawing.Color.Cornsilk;
            this.label9.Location = new System.Drawing.Point(17, 54);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(72, 15);
            this.label9.TabIndex = 2;
            this.label9.Text = "Descripción";
            // 
            // btnSalir
            // 
            this.btnSalir.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSalir.Location = new System.Drawing.Point(844, 576);
            this.btnSalir.Name = "btnSalir";
            this.btnSalir.Size = new System.Drawing.Size(141, 27);
            this.btnSalir.TabIndex = 10;
            this.btnSalir.Text = "&Salir";
            this.btnSalir.UseVisualStyleBackColor = true;
            this.btnSalir.Click += new System.EventHandler(this.btnSalir_Click);
            // 
            // barraControl
            // 
            this.barraControl.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.barraControl.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.barraControl.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.Imprimir});
            this.barraControl.Location = new System.Drawing.Point(0, 0);
            this.barraControl.Name = "barraControl";
            this.barraControl.Padding = new System.Windows.Forms.Padding(10, 0, 0, 0);
            this.barraControl.Size = new System.Drawing.Size(997, 49);
            this.barraControl.TabIndex = 11;
            this.barraControl.TabStop = true;
            this.barraControl.Text = "toolStrip1";
            // 
            // Imprimir
            // 
            this.Imprimir.Image = ((System.Drawing.Image)(resources.GetObject("Imprimir.Image")));
            this.Imprimir.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.Imprimir.Margin = new System.Windows.Forms.Padding(0, 2, 0, 1);
            this.Imprimir.Name = "Imprimir";
            this.Imprimir.Padding = new System.Windows.Forms.Padding(1, 1, 1, 6);
            this.Imprimir.Size = new System.Drawing.Size(59, 46);
            this.Imprimir.Text = "Imprimir";
            this.Imprimir.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.Imprimir.Click += new System.EventHandler(this.nuevo_Click);
            // 
            // grillaReportes
            // 
            this.grillaReportes.AllowUserToAddRows = false;
            this.grillaReportes.AllowUserToResizeRows = false;
            this.grillaReportes.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.grillaReportes.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.grillaReportes.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.grillaReportes.ColumnHeadersHeight = 29;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.Color.WhiteSmoke;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.grillaReportes.DefaultCellStyle = dataGridViewCellStyle2;
            this.grillaReportes.Location = new System.Drawing.Point(12, 142);
            this.grillaReportes.Name = "grillaReportes";
            this.grillaReportes.ReadOnly = true;
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.grillaReportes.RowHeadersDefaultCellStyle = dataGridViewCellStyle3;
            this.grillaReportes.RowHeadersVisible = false;
            this.grillaReportes.RowHeadersWidth = 300;
            dataGridViewCellStyle4.Format = "N2";
            dataGridViewCellStyle4.NullValue = null;
            this.grillaReportes.RowsDefaultCellStyle = dataGridViewCellStyle4;
            this.grillaReportes.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.grillaReportes.Size = new System.Drawing.Size(973, 428);
            this.grillaReportes.TabIndex = 13;
            this.grillaReportes.TabStop = false;
            // 
            // formReporteStock
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(239)))), ((int)(((byte)(250)))));
            this.ClientSize = new System.Drawing.Size(997, 609);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.grillaReportes);
            this.Controls.Add(this.btnSalir);
            this.Controls.Add(this.barraControl);
            this.Name = "formReporteStock";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Reporte Stock";
            this.Load += new System.EventHandler(this.formReporteStock_Load);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.barraControl.ResumeLayout(false);
            this.barraControl.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.grillaReportes)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        protected System.Windows.Forms.Panel panel1;
        protected System.Windows.Forms.TextBox txtDescripcion;
        protected System.Windows.Forms.Label label9;
        protected System.Windows.Forms.Button btnSalir;
        protected System.Windows.Forms.ToolStrip barraControl;
        protected System.Windows.Forms.ToolStripButton Imprimir;
        protected System.Windows.Forms.Label label3;
        protected System.Windows.Forms.Label label4;
        private System.Windows.Forms.ComboBox comboTipoReporte;
        protected System.Windows.Forms.Label label2;
        private System.Windows.Forms.DataGridView grillaReportes;
        private System.Windows.Forms.ComboBox comboSucursal;
        protected System.Windows.Forms.Label label5;
        private System.Windows.Forms.Button btnBuscar;
        private System.Windows.Forms.ComboBox comboCierreStock;
        private System.Windows.Forms.ComboBox comboInicioStock;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button btnExportar;
        private System.Windows.Forms.DateTimePicker txtFechaHastaProgresivo;
        protected System.Windows.Forms.Label lblActualizar;
        protected System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox comboOrdenStock;
        private System.Windows.Forms.DateTimePicker fechaDesdeProgresivo;
    }
}