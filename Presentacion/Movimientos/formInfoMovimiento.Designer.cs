namespace Presentacion.Movimientos
{
    partial class formInfoMovimiento
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(formInfoMovimiento));
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            this.barraControl = new System.Windows.Forms.ToolStrip();
            this.modificar = new System.Windows.Forms.ToolStripButton();
            this.Reporte = new System.Windows.Forms.ToolStripButton();
            this.Imprimir = new System.Windows.Forms.ToolStripButton();
            this.pnlBuscar = new System.Windows.Forms.Panel();
            this.txtHoraMovimiento = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.txtSucDestino = new System.Windows.Forms.TextBox();
            this.txtSucOrigen = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.txtFechaMovimiento = new System.Windows.Forms.DateTimePicker();
            this.btnSalir = new System.Windows.Forms.Button();
            this.txtObservaciones = new System.Windows.Forms.TextBox();
            this.txtCantItems = new System.Windows.Forms.TextBox();
            this.label12 = new System.Windows.Forms.Label();
            this.txtTotalKg = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.grillaCortesPorMovimiento = new System.Windows.Forms.DataGridView();
            this.idCorteMovimientodo = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.codigo = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.corte = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.cantKg = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Balanza = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.label11 = new System.Windows.Forms.Label();
            this.lineShape1 = new Microsoft.VisualBasic.PowerPacks.LineShape();
            this.shapeContainer1 = new Microsoft.VisualBasic.PowerPacks.ShapeContainer();
            this.barraControl.SuspendLayout();
            this.pnlBuscar.SuspendLayout();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.grillaCortesPorMovimiento)).BeginInit();
            this.SuspendLayout();
            // 
            // barraControl
            // 
            this.barraControl.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.barraControl.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.modificar,
            this.Reporte,
            this.Imprimir});
            this.barraControl.Location = new System.Drawing.Point(0, 0);
            this.barraControl.Name = "barraControl";
            this.barraControl.Padding = new System.Windows.Forms.Padding(10, 0, 0, 0);
            this.barraControl.Size = new System.Drawing.Size(436, 45);
            this.barraControl.TabIndex = 7;
            this.barraControl.Text = "toolStrip1";
            // 
            // modificar
            // 
            this.modificar.Image = ((System.Drawing.Image)(resources.GetObject("modificar.Image")));
            this.modificar.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.modificar.Margin = new System.Windows.Forms.Padding(0, 2, 0, 1);
            this.modificar.Name = "modificar";
            this.modificar.Padding = new System.Windows.Forms.Padding(1);
            this.modificar.Size = new System.Drawing.Size(64, 42);
            this.modificar.Text = "Modificar";
            this.modificar.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.modificar.Click += new System.EventHandler(this.modificar_Click);
            // 
            // Reporte
            // 
            this.Reporte.Image = ((System.Drawing.Image)(resources.GetObject("Reporte.Image")));
            this.Reporte.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.Reporte.Name = "Reporte";
            this.Reporte.Size = new System.Drawing.Size(52, 42);
            this.Reporte.Text = "Reporte";
            this.Reporte.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.Reporte.Click += new System.EventHandler(this.Reporte_Click);
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
            this.Imprimir.Click += new System.EventHandler(this.Imprimir_Click);
            // 
            // pnlBuscar
            // 
            this.pnlBuscar.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.pnlBuscar.BackColor = System.Drawing.Color.SteelBlue;
            this.pnlBuscar.Controls.Add(this.txtHoraMovimiento);
            this.pnlBuscar.Controls.Add(this.label7);
            this.pnlBuscar.Controls.Add(this.label6);
            this.pnlBuscar.Controls.Add(this.groupBox1);
            this.pnlBuscar.Controls.Add(this.txtFechaMovimiento);
            this.pnlBuscar.Location = new System.Drawing.Point(-6, 39);
            this.pnlBuscar.Name = "pnlBuscar";
            this.pnlBuscar.Size = new System.Drawing.Size(448, 108);
            this.pnlBuscar.TabIndex = 23;
            // 
            // txtHoraMovimiento
            // 
            this.txtHoraMovimiento.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.txtHoraMovimiento.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtHoraMovimiento.Location = new System.Drawing.Point(357, 45);
            this.txtHoraMovimiento.Name = "txtHoraMovimiento";
            this.txtHoraMovimiento.ReadOnly = true;
            this.txtHoraMovimiento.Size = new System.Drawing.Size(78, 21);
            this.txtHoraMovimiento.TabIndex = 45;
            this.txtHoraMovimiento.TabStop = false;
            this.txtHoraMovimiento.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label7
            // 
            this.label7.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label7.ForeColor = System.Drawing.Color.Cornsilk;
            this.label7.Location = new System.Drawing.Point(317, 48);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(34, 15);
            this.label7.TabIndex = 19;
            this.label7.Text = "Hora";
            // 
            // label6
            // 
            this.label6.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.ForeColor = System.Drawing.Color.Cornsilk;
            this.label6.Location = new System.Drawing.Point(282, 19);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(41, 15);
            this.label6.TabIndex = 16;
            this.label6.Text = "Fecha";
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.BackColor = System.Drawing.Color.SteelBlue;
            this.groupBox1.Controls.Add(this.txtSucDestino);
            this.groupBox1.Controls.Add(this.txtSucOrigen);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox1.ForeColor = System.Drawing.Color.Cornsilk;
            this.groupBox1.Location = new System.Drawing.Point(15, 8);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(246, 89);
            this.groupBox1.TabIndex = 13;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Sucursales";
            // 
            // txtSucDestino
            // 
            this.txtSucDestino.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.txtSucDestino.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtSucDestino.Location = new System.Drawing.Point(89, 56);
            this.txtSucDestino.Name = "txtSucDestino";
            this.txtSucDestino.ReadOnly = true;
            this.txtSucDestino.Size = new System.Drawing.Size(121, 20);
            this.txtSucDestino.TabIndex = 12;
            // 
            // txtSucOrigen
            // 
            this.txtSucOrigen.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.txtSucOrigen.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtSucOrigen.Location = new System.Drawing.Point(89, 24);
            this.txtSucOrigen.Name = "txtSucOrigen";
            this.txtSucOrigen.ReadOnly = true;
            this.txtSucOrigen.Size = new System.Drawing.Size(121, 20);
            this.txtSucOrigen.TabIndex = 11;
            // 
            // label3
            // 
            this.label3.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.ForeColor = System.Drawing.Color.Cornsilk;
            this.label3.Location = new System.Drawing.Point(39, 27);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(44, 15);
            this.label3.TabIndex = 10;
            this.label3.Text = "Origen";
            // 
            // label4
            // 
            this.label4.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.ForeColor = System.Drawing.Color.Cornsilk;
            this.label4.Location = new System.Drawing.Point(34, 59);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(49, 15);
            this.label4.TabIndex = 8;
            this.label4.Text = "Destino";
            // 
            // txtFechaMovimiento
            // 
            this.txtFechaMovimiento.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.txtFechaMovimiento.Enabled = false;
            this.txtFechaMovimiento.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtFechaMovimiento.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.txtFechaMovimiento.Location = new System.Drawing.Point(329, 18);
            this.txtFechaMovimiento.Name = "txtFechaMovimiento";
            this.txtFechaMovimiento.Size = new System.Drawing.Size(106, 20);
            this.txtFechaMovimiento.TabIndex = 15;
            // 
            // btnSalir
            // 
            this.btnSalir.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSalir.Location = new System.Drawing.Point(351, 517);
            this.btnSalir.Name = "btnSalir";
            this.btnSalir.Size = new System.Drawing.Size(78, 27);
            this.btnSalir.TabIndex = 21;
            this.btnSalir.Text = "Salir";
            this.btnSalir.UseVisualStyleBackColor = true;
            this.btnSalir.Click += new System.EventHandler(this.btnSalir_Click);
            // 
            // txtObservaciones
            // 
            this.txtObservaciones.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.txtObservaciones.Location = new System.Drawing.Point(12, 474);
            this.txtObservaciones.Multiline = true;
            this.txtObservaciones.Name = "txtObservaciones";
            this.txtObservaciones.ReadOnly = true;
            this.txtObservaciones.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtObservaciones.Size = new System.Drawing.Size(225, 33);
            this.txtObservaciones.TabIndex = 45;
            this.txtObservaciones.TabStop = false;
            // 
            // txtCantItems
            // 
            this.txtCantItems.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.txtCantItems.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtCantItems.Location = new System.Drawing.Point(346, 461);
            this.txtCantItems.Name = "txtCantItems";
            this.txtCantItems.ReadOnly = true;
            this.txtCantItems.Size = new System.Drawing.Size(78, 21);
            this.txtCantItems.TabIndex = 44;
            this.txtCantItems.TabStop = false;
            this.txtCantItems.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label12
            // 
            this.label12.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.label12.AutoSize = true;
            this.label12.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label12.Location = new System.Drawing.Point(257, 466);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(79, 15);
            this.label12.TabIndex = 43;
            this.label12.Text = "Cant. Items";
            // 
            // txtTotalKg
            // 
            this.txtTotalKg.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.txtTotalKg.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtTotalKg.Location = new System.Drawing.Point(346, 484);
            this.txtTotalKg.Name = "txtTotalKg";
            this.txtTotalKg.ReadOnly = true;
            this.txtTotalKg.Size = new System.Drawing.Size(78, 21);
            this.txtTotalKg.TabIndex = 42;
            this.txtTotalKg.TabStop = false;
            this.txtTotalKg.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label8
            // 
            this.label8.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.label8.AutoSize = true;
            this.label8.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label8.Location = new System.Drawing.Point(276, 487);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(60, 15);
            this.label8.TabIndex = 41;
            this.label8.Text = "Total Kg";
            // 
            // grillaCortesPorMovimiento
            // 
            this.grillaCortesPorMovimiento.AllowUserToAddRows = false;
            this.grillaCortesPorMovimiento.AllowUserToOrderColumns = true;
            this.grillaCortesPorMovimiento.AllowUserToResizeRows = false;
            this.grillaCortesPorMovimiento.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.grillaCortesPorMovimiento.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.grillaCortesPorMovimiento.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.idCorteMovimientodo,
            this.codigo,
            this.corte,
            this.cantKg,
            this.Balanza});
            this.grillaCortesPorMovimiento.Location = new System.Drawing.Point(12, 154);
            this.grillaCortesPorMovimiento.Name = "grillaCortesPorMovimiento";
            this.grillaCortesPorMovimiento.RowHeadersVisible = false;
            this.grillaCortesPorMovimiento.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.grillaCortesPorMovimiento.Size = new System.Drawing.Size(412, 301);
            this.grillaCortesPorMovimiento.TabIndex = 0;
            // 
            // idCorteMovimientodo
            // 
            this.idCorteMovimientodo.DataPropertyName = "idCorteMovimiento";
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.TopRight;
            this.idCorteMovimientodo.DefaultCellStyle = dataGridViewCellStyle1;
            this.idCorteMovimientodo.HeaderText = "Id Corte Mov.";
            this.idCorteMovimientodo.MinimumWidth = 70;
            this.idCorteMovimientodo.Name = "idCorteMovimientodo";
            this.idCorteMovimientodo.ReadOnly = true;
            this.idCorteMovimientodo.Visible = false;
            // 
            // codigo
            // 
            this.codigo.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.ColumnHeader;
            this.codigo.DataPropertyName = "codigo";
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
            this.codigo.DefaultCellStyle = dataGridViewCellStyle2;
            this.codigo.FillWeight = 50F;
            this.codigo.HeaderText = "Código";
            this.codigo.MinimumWidth = 80;
            this.codigo.Name = "codigo";
            this.codigo.ReadOnly = true;
            this.codigo.Width = 80;
            // 
            // corte
            // 
            this.corte.DataPropertyName = "corte";
            this.corte.FillWeight = 89.0863F;
            this.corte.HeaderText = "Corte";
            this.corte.Name = "corte";
            this.corte.ReadOnly = true;
            // 
            // cantKg
            // 
            this.cantKg.DataPropertyName = "cantKg";
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
            dataGridViewCellStyle3.Format = "N3";
            dataGridViewCellStyle3.NullValue = null;
            this.cantKg.DefaultCellStyle = dataGridViewCellStyle3;
            this.cantKg.FillWeight = 55F;
            this.cantKg.HeaderText = "Cant. Kgs";
            this.cantKg.Name = "cantKg";
            this.cantKg.ReadOnly = true;
            // 
            // Balanza
            // 
            this.Balanza.DataPropertyName = "pesoBalanza";
            this.Balanza.FillWeight = 30F;
            this.Balanza.HeaderText = "Balanza";
            this.Balanza.Name = "Balanza";
            this.Balanza.ReadOnly = true;
            // 
            // label11
            // 
            this.label11.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(14, 458);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(78, 13);
            this.label11.TabIndex = 46;
            this.label11.Text = "Observaciones";
            // 
            // lineShape1
            // 
            this.lineShape1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.lineShape1.Name = "lineShape1";
            this.lineShape1.X1 = 7;
            this.lineShape1.X2 = 430;
            this.lineShape1.Y1 = 513;
            this.lineShape1.Y2 = 513;
            // 
            // shapeContainer1
            // 
            this.shapeContainer1.Location = new System.Drawing.Point(0, 0);
            this.shapeContainer1.Margin = new System.Windows.Forms.Padding(0);
            this.shapeContainer1.Name = "shapeContainer1";
            this.shapeContainer1.Shapes.AddRange(new Microsoft.VisualBasic.PowerPacks.Shape[] {
            this.lineShape1});
            this.shapeContainer1.Size = new System.Drawing.Size(436, 546);
            this.shapeContainer1.TabIndex = 47;
            this.shapeContainer1.TabStop = false;
            // 
            // formInfoMovimiento
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Bisque;
            this.ClientSize = new System.Drawing.Size(436, 546);
            this.Controls.Add(this.label11);
            this.Controls.Add(this.txtObservaciones);
            this.Controls.Add(this.txtCantItems);
            this.Controls.Add(this.label12);
            this.Controls.Add(this.txtTotalKg);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.grillaCortesPorMovimiento);
            this.Controls.Add(this.barraControl);
            this.Controls.Add(this.pnlBuscar);
            this.Controls.Add(this.btnSalir);
            this.Controls.Add(this.shapeContainer1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "formInfoMovimiento";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Información Movimiento";
            this.Load += new System.EventHandler(this.formInfoMovimiento_Load);
            this.barraControl.ResumeLayout(false);
            this.barraControl.PerformLayout();
            this.pnlBuscar.ResumeLayout(false);
            this.pnlBuscar.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.grillaCortesPorMovimiento)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        protected internal System.Windows.Forms.ToolStrip barraControl;
        protected System.Windows.Forms.ToolStripButton modificar;
        protected System.Windows.Forms.Panel pnlBuscar;
        protected System.Windows.Forms.Label label6;
        protected System.Windows.Forms.GroupBox groupBox1;
        protected System.Windows.Forms.TextBox txtSucDestino;
        protected System.Windows.Forms.TextBox txtSucOrigen;
        protected System.Windows.Forms.Label label3;
        protected System.Windows.Forms.Label label4;
        private System.Windows.Forms.DateTimePicker txtFechaMovimiento;
        protected System.Windows.Forms.Button btnSalir;
        private System.Windows.Forms.TextBox txtObservaciones;
        private System.Windows.Forms.TextBox txtCantItems;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.TextBox txtTotalKg;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.DataGridView grillaCortesPorMovimiento;
        private System.Windows.Forms.Label label11;
        private Microsoft.VisualBasic.PowerPacks.LineShape lineShape1;
        private Microsoft.VisualBasic.PowerPacks.ShapeContainer shapeContainer1;
        private System.Windows.Forms.ToolStripButton Reporte;
        private System.Windows.Forms.TextBox txtHoraMovimiento;
        protected System.Windows.Forms.Label label7;
        private System.Windows.Forms.DataGridViewTextBoxColumn idCorteMovimientodo;
        private System.Windows.Forms.DataGridViewTextBoxColumn codigo;
        private System.Windows.Forms.DataGridViewTextBoxColumn corte;
        private System.Windows.Forms.DataGridViewTextBoxColumn cantKg;
        private System.Windows.Forms.DataGridViewCheckBoxColumn Balanza;
        protected System.Windows.Forms.ToolStripButton Imprimir;
    }
}