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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle5 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle6 = new System.Windows.Forms.DataGridViewCellStyle();
            this.panel1 = new System.Windows.Forms.Panel();
            this.btnBuscar = new System.Windows.Forms.Button();
            this.comboSucursal = new System.Windows.Forms.ComboBox();
            this.label5 = new System.Windows.Forms.Label();
            this.comboTipoReporte = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.fechaDesde = new System.Windows.Forms.DateTimePicker();
            this.label3 = new System.Windows.Forms.Label();
            this.fechaHasta = new System.Windows.Forms.DateTimePicker();
            this.label4 = new System.Windows.Forms.Label();
            this.txtDescripcion = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.shapeContainer2 = new Microsoft.VisualBasic.PowerPacks.ShapeContainer();
            this.lineShape1 = new Microsoft.VisualBasic.PowerPacks.LineShape();
            this.lineShape3 = new Microsoft.VisualBasic.PowerPacks.LineShape();
            this.lineShape2 = new Microsoft.VisualBasic.PowerPacks.LineShape();
            this.btnSalir = new System.Windows.Forms.Button();
            this.barraControl = new System.Windows.Forms.ToolStrip();
            this.Imprimir = new System.Windows.Forms.ToolStripButton();
            this.grillaReportes = new System.Windows.Forms.DataGridView();
            this.panel1.SuspendLayout();
            this.barraControl.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.grillaReportes)).BeginInit();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.panel1.BackColor = System.Drawing.Color.SteelBlue;
            this.panel1.Controls.Add(this.btnBuscar);
            this.panel1.Controls.Add(this.comboSucursal);
            this.panel1.Controls.Add(this.label5);
            this.panel1.Controls.Add(this.comboTipoReporte);
            this.panel1.Controls.Add(this.label2);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Controls.Add(this.fechaDesde);
            this.panel1.Controls.Add(this.label3);
            this.panel1.Controls.Add(this.fechaHasta);
            this.panel1.Controls.Add(this.label4);
            this.panel1.Controls.Add(this.txtDescripcion);
            this.panel1.Controls.Add(this.label9);
            this.panel1.Controls.Add(this.shapeContainer2);
            this.panel1.Location = new System.Drawing.Point(-5, 44);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(855, 91);
            this.panel1.TabIndex = 12;
            this.panel1.Paint += new System.Windows.Forms.PaintEventHandler(this.panel1_Paint);
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
            this.comboSucursal.Size = new System.Drawing.Size(113, 21);
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
            this.comboTipoReporte.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboTipoReporte.FormattingEnabled = true;
            this.comboTipoReporte.Items.AddRange(new object[] {
            "Cierre Stock",
            "Cierre Stock 2",
            "Ingreso - Egreso",
            "Total Cortes Vendidos",
            "Total Kgs Corte/Compra",
            "Movimiento/Corte",
            "Teorico - Real"});
            this.comboTipoReporte.Location = new System.Drawing.Point(708, 10);
            this.comboTipoReporte.Name = "comboTipoReporte";
            this.comboTipoReporte.Size = new System.Drawing.Size(122, 21);
            this.comboTipoReporte.TabIndex = 18;
            this.comboTipoReporte.SelectedIndexChanged += new System.EventHandler(this.comboTipoReporte_SelectedIndexChanged);
            this.comboTipoReporte.SelectedValueChanged += new System.EventHandler(this.btnBuscar_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.ForeColor = System.Drawing.Color.Cornsilk;
            this.label2.Location = new System.Drawing.Point(651, 12);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(51, 15);
            this.label2.TabIndex = 17;
            this.label2.Text = "Reporte";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ForeColor = System.Drawing.Color.Cornsilk;
            this.label1.Location = new System.Drawing.Point(331, 35);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(47, 15);
            this.label1.TabIndex = 16;
            this.label1.Text = "Fechas";
            // 
            // fechaDesde
            // 
            this.fechaDesde.Checked = false;
            this.fechaDesde.CustomFormat = "dd/MM/yyyy ";
            this.fechaDesde.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.fechaDesde.Location = new System.Drawing.Point(422, 53);
            this.fechaDesde.Name = "fechaDesde";
            this.fechaDesde.Size = new System.Drawing.Size(101, 20);
            this.fechaDesde.TabIndex = 13;
            this.fechaDesde.ValueChanged += new System.EventHandler(this.btnBuscar_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.ForeColor = System.Drawing.Color.Cornsilk;
            this.label3.Location = new System.Drawing.Point(374, 55);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(43, 15);
            this.label3.TabIndex = 12;
            this.label3.Text = "Desde";
            // 
            // fechaHasta
            // 
            this.fechaHasta.CustomFormat = "dd/MM/yyyy";
            this.fechaHasta.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.fechaHasta.Location = new System.Drawing.Point(586, 53);
            this.fechaHasta.Name = "fechaHasta";
            this.fechaHasta.Size = new System.Drawing.Size(101, 20);
            this.fechaHasta.TabIndex = 15;
            this.fechaHasta.ValueChanged += new System.EventHandler(this.btnBuscar_Click);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.ForeColor = System.Drawing.Color.Cornsilk;
            this.label4.Location = new System.Drawing.Point(542, 55);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(39, 15);
            this.label4.TabIndex = 14;
            this.label4.Text = "Hasta";
            // 
            // txtDescripcion
            // 
            this.txtDescripcion.Location = new System.Drawing.Point(95, 53);
            this.txtDescripcion.Name = "txtDescripcion";
            this.txtDescripcion.Size = new System.Drawing.Size(137, 20);
            this.txtDescripcion.TabIndex = 0;
            this.txtDescripcion.TextChanged += new System.EventHandler(this.btnBuscar_Click);
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
            // shapeContainer2
            // 
            this.shapeContainer2.Location = new System.Drawing.Point(0, 0);
            this.shapeContainer2.Margin = new System.Windows.Forms.Padding(0);
            this.shapeContainer2.Name = "shapeContainer2";
            this.shapeContainer2.Shapes.AddRange(new Microsoft.VisualBasic.PowerPacks.Shape[] {
            this.lineShape1,
            this.lineShape3,
            this.lineShape2});
            this.shapeContainer2.Size = new System.Drawing.Size(855, 91);
            this.shapeContainer2.TabIndex = 11;
            this.shapeContainer2.TabStop = false;
            // 
            // lineShape1
            // 
            this.lineShape1.BorderColor = System.Drawing.Color.White;
            this.lineShape1.Name = "lineShape1";
            this.lineShape1.X1 = 13;
            this.lineShape1.X2 = 321;
            this.lineShape1.Y1 = 38;
            this.lineShape1.Y2 = 38;
            // 
            // lineShape3
            // 
            this.lineShape3.BorderColor = System.Drawing.Color.White;
            this.lineShape3.Name = "lineShape3";
            this.lineShape3.X1 = 375;
            this.lineShape3.X2 = 697;
            this.lineShape3.Y1 = 49;
            this.lineShape3.Y2 = 49;
            // 
            // lineShape2
            // 
            this.lineShape2.BorderColor = System.Drawing.Color.White;
            this.lineShape2.Name = "lineShape2";
            this.lineShape2.X1 = 13;
            this.lineShape2.X2 = 843;
            this.lineShape2.Y1 = 81;
            this.lineShape2.Y2 = 81;
            // 
            // btnSalir
            // 
            this.btnSalir.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSalir.Location = new System.Drawing.Point(745, 616);
            this.btnSalir.Name = "btnSalir";
            this.btnSalir.Size = new System.Drawing.Size(84, 27);
            this.btnSalir.TabIndex = 10;
            this.btnSalir.Text = "Salir";
            this.btnSalir.UseVisualStyleBackColor = true;
            this.btnSalir.Click += new System.EventHandler(this.btnSalir_Click);
            // 
            // barraControl
            // 
            this.barraControl.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.barraControl.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.Imprimir});
            this.barraControl.Location = new System.Drawing.Point(0, 0);
            this.barraControl.Name = "barraControl";
            this.barraControl.Padding = new System.Windows.Forms.Padding(10, 0, 0, 0);
            this.barraControl.Size = new System.Drawing.Size(845, 45);
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
            this.Imprimir.Size = new System.Drawing.Size(59, 42);
            this.Imprimir.Text = "Imprimir";
            this.Imprimir.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.Imprimir.Click += new System.EventHandler(this.nuevo_Click);
            // 
            // grillaReportes
            // 
            this.grillaReportes.AllowUserToAddRows = false;
            this.grillaReportes.AllowUserToDeleteRows = false;
            this.grillaReportes.AllowUserToResizeRows = false;
            dataGridViewCellStyle4.BackColor = System.Drawing.Color.Gainsboro;
            this.grillaReportes.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle4;
            this.grillaReportes.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.grillaReportes.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            dataGridViewCellStyle5.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle5.BackColor = System.Drawing.Color.WhiteSmoke;
            dataGridViewCellStyle5.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle5.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle5.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle5.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle5.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.grillaReportes.DefaultCellStyle = dataGridViewCellStyle5;
            this.grillaReportes.Location = new System.Drawing.Point(13, 142);
            this.grillaReportes.Name = "grillaReportes";
            this.grillaReportes.ReadOnly = true;
            this.grillaReportes.RowHeadersVisible = false;
            this.grillaReportes.RowHeadersWidth = 300;
            dataGridViewCellStyle6.Format = "N2";
            dataGridViewCellStyle6.NullValue = null;
            this.grillaReportes.RowsDefaultCellStyle = dataGridViewCellStyle6;
            this.grillaReportes.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.grillaReportes.Size = new System.Drawing.Size(816, 468);
            this.grillaReportes.TabIndex = 13;
            this.grillaReportes.TabStop = false;
            // 
            // formReporteStock
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Bisque;
            this.ClientSize = new System.Drawing.Size(845, 646);
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
        private Microsoft.VisualBasic.PowerPacks.ShapeContainer shapeContainer2;
        private Microsoft.VisualBasic.PowerPacks.LineShape lineShape2;
        protected System.Windows.Forms.Button btnSalir;
        protected System.Windows.Forms.ToolStrip barraControl;
        protected System.Windows.Forms.ToolStripButton Imprimir;
        protected System.Windows.Forms.Label label1;
        protected System.Windows.Forms.DateTimePicker fechaDesde;
        protected System.Windows.Forms.Label label3;
        protected System.Windows.Forms.DateTimePicker fechaHasta;
        protected System.Windows.Forms.Label label4;
        private Microsoft.VisualBasic.PowerPacks.LineShape lineShape3;
        private System.Windows.Forms.ComboBox comboTipoReporte;
        protected System.Windows.Forms.Label label2;
        private Microsoft.VisualBasic.PowerPacks.LineShape lineShape1;
        private System.Windows.Forms.DataGridView grillaReportes;
        private System.Windows.Forms.ComboBox comboSucursal;
        protected System.Windows.Forms.Label label5;
        private System.Windows.Forms.Button btnBuscar;
    }
}