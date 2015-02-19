namespace Presentacion.Pagos
{
    partial class formPagos
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(formPagos));
            this.grillaPagos = new System.Windows.Forms.DataGridView();
            this.btnSalir = new System.Windows.Forms.Button();
            this.pnlBuscar = new System.Windows.Forms.Panel();
            this.comboTipoTramite = new System.Windows.Forms.ComboBox();
            this.label7 = new System.Windows.Forms.Label();
            this.btnBuscar = new System.Windows.Forms.Button();
            this.txtDescripcion = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.txtFechaDesde = new System.Windows.Forms.DateTimePicker();
            this.label3 = new System.Windows.Forms.Label();
            this.txtFechaHasta = new System.Windows.Forms.DateTimePicker();
            this.label4 = new System.Windows.Forms.Label();
            this.shapeContainer1 = new Microsoft.VisualBasic.PowerPacks.ShapeContainer();
            this.lineShape2 = new Microsoft.VisualBasic.PowerPacks.LineShape();
            this.barraControl = new System.Windows.Forms.ToolStrip();
            this.nuevo = new System.Windows.Forms.ToolStripButton();
            this.modificar = new System.Windows.Forms.ToolStripButton();
            this.eliminar = new System.Windows.Forms.ToolStripButton();
            this.Imprimir = new System.Windows.Forms.ToolStripButton();
            this.label5 = new System.Windows.Forms.Label();
            this.txtSaldo = new System.Windows.Forms.TextBox();
            this.txtCompras = new System.Windows.Forms.TextBox();
            this.label12 = new System.Windows.Forms.Label();
            this.txtPagos = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.lineShape4 = new Microsoft.VisualBasic.PowerPacks.LineShape();
            this.shapeContainer2 = new Microsoft.VisualBasic.PowerPacks.ShapeContainer();
            this.label8 = new System.Windows.Forms.Label();
            this.txtSaldoAnterior = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.grillaPagos)).BeginInit();
            this.pnlBuscar.SuspendLayout();
            this.barraControl.SuspendLayout();
            this.SuspendLayout();
            // 
            // grillaPagos
            // 
            this.grillaPagos.AllowUserToAddRows = false;
            this.grillaPagos.AllowUserToOrderColumns = true;
            this.grillaPagos.AllowUserToResizeRows = false;
            this.grillaPagos.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
            this.grillaPagos.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle1.NullValue = null;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.grillaPagos.DefaultCellStyle = dataGridViewCellStyle1;
            this.grillaPagos.Location = new System.Drawing.Point(8, 119);
            this.grillaPagos.Name = "grillaPagos";
            this.grillaPagos.ReadOnly = true;
            this.grillaPagos.RowHeadersVisible = false;
            this.grillaPagos.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.grillaPagos.Size = new System.Drawing.Size(703, 366);
            this.grillaPagos.TabIndex = 21;
            this.grillaPagos.ColumnHeaderMouseClick += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.grillaPagos_ColumnHeaderMouseClick);
            // 
            // btnSalir
            // 
            this.btnSalir.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSalir.Location = new System.Drawing.Point(634, 584);
            this.btnSalir.Name = "btnSalir";
            this.btnSalir.Size = new System.Drawing.Size(82, 27);
            this.btnSalir.TabIndex = 19;
            this.btnSalir.Text = "Salir";
            this.btnSalir.UseVisualStyleBackColor = true;
            this.btnSalir.Click += new System.EventHandler(this.btnCancelar_Click);
            // 
            // pnlBuscar
            // 
            this.pnlBuscar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.pnlBuscar.BackColor = System.Drawing.Color.SteelBlue;
            this.pnlBuscar.Controls.Add(this.comboTipoTramite);
            this.pnlBuscar.Controls.Add(this.label7);
            this.pnlBuscar.Controls.Add(this.btnBuscar);
            this.pnlBuscar.Controls.Add(this.txtDescripcion);
            this.pnlBuscar.Controls.Add(this.label2);
            this.pnlBuscar.Controls.Add(this.label1);
            this.pnlBuscar.Controls.Add(this.txtFechaDesde);
            this.pnlBuscar.Controls.Add(this.label3);
            this.pnlBuscar.Controls.Add(this.txtFechaHasta);
            this.pnlBuscar.Controls.Add(this.label4);
            this.pnlBuscar.Controls.Add(this.shapeContainer1);
            this.pnlBuscar.Location = new System.Drawing.Point(-9, 42);
            this.pnlBuscar.Name = "pnlBuscar";
            this.pnlBuscar.Size = new System.Drawing.Size(741, 71);
            this.pnlBuscar.TabIndex = 18;
            // 
            // comboTipoTramite
            // 
            this.comboTipoTramite.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboTipoTramite.FormattingEnabled = true;
            this.comboTipoTramite.Items.AddRange(new object[] {
            "Todos",
            "Pago",
            "Compra"});
            this.comboTipoTramite.Location = new System.Drawing.Point(100, 38);
            this.comboTipoTramite.Name = "comboTipoTramite";
            this.comboTipoTramite.Size = new System.Drawing.Size(113, 21);
            this.comboTipoTramite.TabIndex = 17;
            this.comboTipoTramite.TextChanged += new System.EventHandler(this.comboTipoTramite_TextChanged);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label7.ForeColor = System.Drawing.Color.Cornsilk;
            this.label7.Location = new System.Drawing.Point(18, 41);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(76, 15);
            this.label7.TabIndex = 16;
            this.label7.Text = "Tipo Tramite";
            // 
            // btnBuscar
            // 
            this.btnBuscar.AccessibleDescription = "";
            this.btnBuscar.ForeColor = System.Drawing.Color.Black;
            this.btnBuscar.Image = ((System.Drawing.Image)(resources.GetObject("btnBuscar.Image")));
            this.btnBuscar.Location = new System.Drawing.Point(239, 11);
            this.btnBuscar.Name = "btnBuscar";
            this.btnBuscar.Size = new System.Drawing.Size(28, 23);
            this.btnBuscar.TabIndex = 15;
            this.btnBuscar.UseVisualStyleBackColor = true;
            this.btnBuscar.Click += new System.EventHandler(this.btnBuscarCorte_Click);
            // 
            // txtDescripcion
            // 
            this.txtDescripcion.Location = new System.Drawing.Point(100, 12);
            this.txtDescripcion.Name = "txtDescripcion";
            this.txtDescripcion.Size = new System.Drawing.Size(133, 20);
            this.txtDescripcion.TabIndex = 0;
            this.txtDescripcion.TextChanged += new System.EventHandler(this.txtDescripcion_TextChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.ForeColor = System.Drawing.Color.Cornsilk;
            this.label2.Location = new System.Drawing.Point(22, 13);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(72, 15);
            this.label2.TabIndex = 11;
            this.label2.Text = "Descripción";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ForeColor = System.Drawing.Color.Cornsilk;
            this.label1.Location = new System.Drawing.Point(350, 17);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(41, 15);
            this.label1.TabIndex = 9;
            this.label1.Text = "Fecha";
            // 
            // txtFechaDesde
            // 
            this.txtFechaDesde.CustomFormat = "dd/MM/yyyy";
            this.txtFechaDesde.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.txtFechaDesde.Location = new System.Drawing.Point(442, 39);
            this.txtFechaDesde.Name = "txtFechaDesde";
            this.txtFechaDesde.Size = new System.Drawing.Size(98, 20);
            this.txtFechaDesde.TabIndex = 5;
            this.txtFechaDesde.Value = new System.DateTime(2011, 9, 1, 0, 0, 0, 0);
            this.txtFechaDesde.ValueChanged += new System.EventHandler(this.txtFechaDesde_ValueChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.ForeColor = System.Drawing.Color.Cornsilk;
            this.label3.Location = new System.Drawing.Point(392, 41);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(43, 15);
            this.label3.TabIndex = 4;
            this.label3.Text = "Desde";
            // 
            // txtFechaHasta
            // 
            this.txtFechaHasta.CustomFormat = "dd/MM/yyyy";
            this.txtFechaHasta.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.txtFechaHasta.Location = new System.Drawing.Point(595, 39);
            this.txtFechaHasta.Name = "txtFechaHasta";
            this.txtFechaHasta.Size = new System.Drawing.Size(98, 20);
            this.txtFechaHasta.TabIndex = 7;
            this.txtFechaHasta.ValueChanged += new System.EventHandler(this.txtFechaHasta_ValueChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.ForeColor = System.Drawing.Color.Cornsilk;
            this.label4.Location = new System.Drawing.Point(550, 41);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(39, 15);
            this.label4.TabIndex = 6;
            this.label4.Text = "Hasta";
            // 
            // shapeContainer1
            // 
            this.shapeContainer1.Location = new System.Drawing.Point(0, 0);
            this.shapeContainer1.Margin = new System.Windows.Forms.Padding(0);
            this.shapeContainer1.Name = "shapeContainer1";
            this.shapeContainer1.Shapes.AddRange(new Microsoft.VisualBasic.PowerPacks.Shape[] {
            this.lineShape2});
            this.shapeContainer1.Size = new System.Drawing.Size(741, 71);
            this.shapeContainer1.TabIndex = 10;
            this.shapeContainer1.TabStop = false;
            // 
            // lineShape2
            // 
            this.lineShape2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.lineShape2.BorderColor = System.Drawing.Color.White;
            this.lineShape2.Name = "lineShape2";
            this.lineShape2.X1 = 393;
            this.lineShape2.X2 = 721;
            this.lineShape2.Y1 = 31;
            this.lineShape2.Y2 = 31;
            // 
            // barraControl
            // 
            this.barraControl.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.barraControl.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.nuevo,
            this.modificar,
            this.eliminar,
            this.Imprimir});
            this.barraControl.Location = new System.Drawing.Point(0, 0);
            this.barraControl.Name = "barraControl";
            this.barraControl.Padding = new System.Windows.Forms.Padding(10, 0, 0, 0);
            this.barraControl.Size = new System.Drawing.Size(723, 45);
            this.barraControl.TabIndex = 17;
            this.barraControl.Text = "toolStrip1";
            // 
            // nuevo
            // 
            this.nuevo.Image = ((System.Drawing.Image)(resources.GetObject("nuevo.Image")));
            this.nuevo.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.nuevo.Margin = new System.Windows.Forms.Padding(0, 2, 0, 1);
            this.nuevo.Name = "nuevo";
            this.nuevo.Padding = new System.Windows.Forms.Padding(1, 1, 1, 6);
            this.nuevo.Size = new System.Drawing.Size(48, 42);
            this.nuevo.Text = "Nuevo";
            this.nuevo.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.nuevo.Click += new System.EventHandler(this.nuevo_Click);
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
            // eliminar
            // 
            this.eliminar.Image = ((System.Drawing.Image)(resources.GetObject("eliminar.Image")));
            this.eliminar.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.eliminar.Margin = new System.Windows.Forms.Padding(0, 2, 0, 1);
            this.eliminar.Name = "eliminar";
            this.eliminar.Padding = new System.Windows.Forms.Padding(1);
            this.eliminar.Size = new System.Drawing.Size(56, 42);
            this.eliminar.Text = "Eliminar";
            this.eliminar.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.eliminar.Click += new System.EventHandler(this.eliminar_Click);
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
            // label5
            // 
            this.label5.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.Location = new System.Drawing.Point(562, 557);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(44, 15);
            this.label5.TabIndex = 28;
            this.label5.Text = "Saldo";
            // 
            // txtSaldo
            // 
            this.txtSaldo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.txtSaldo.BackColor = System.Drawing.SystemColors.Control;
            this.txtSaldo.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtSaldo.Location = new System.Drawing.Point(612, 554);
            this.txtSaldo.Name = "txtSaldo";
            this.txtSaldo.ReadOnly = true;
            this.txtSaldo.Size = new System.Drawing.Size(102, 21);
            this.txtSaldo.TabIndex = 27;
            this.txtSaldo.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // txtCompras
            // 
            this.txtCompras.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.txtCompras.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtCompras.Location = new System.Drawing.Point(612, 491);
            this.txtCompras.Name = "txtCompras";
            this.txtCompras.ReadOnly = true;
            this.txtCompras.Size = new System.Drawing.Size(102, 21);
            this.txtCompras.TabIndex = 26;
            this.txtCompras.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label12
            // 
            this.label12.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.label12.AutoSize = true;
            this.label12.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label12.Location = new System.Drawing.Point(542, 496);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(64, 15);
            this.label12.TabIndex = 25;
            this.label12.Text = "Compras";
            // 
            // txtPagos
            // 
            this.txtPagos.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.txtPagos.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtPagos.Location = new System.Drawing.Point(612, 512);
            this.txtPagos.Name = "txtPagos";
            this.txtPagos.ReadOnly = true;
            this.txtPagos.Size = new System.Drawing.Size(102, 21);
            this.txtPagos.TabIndex = 30;
            this.txtPagos.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label6
            // 
            this.label6.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.Location = new System.Drawing.Point(559, 516);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(47, 15);
            this.label6.TabIndex = 29;
            this.label6.Text = "Pagos";
            // 
            // lineShape4
            // 
            this.lineShape4.Name = "lineShape4";
            this.lineShape4.X1 = 6;
            this.lineShape4.X2 = 713;
            this.lineShape4.Y1 = 578;
            this.lineShape4.Y2 = 578;
            // 
            // shapeContainer2
            // 
            this.shapeContainer2.Location = new System.Drawing.Point(0, 0);
            this.shapeContainer2.Margin = new System.Windows.Forms.Padding(0);
            this.shapeContainer2.Name = "shapeContainer2";
            this.shapeContainer2.Shapes.AddRange(new Microsoft.VisualBasic.PowerPacks.Shape[] {
            this.lineShape4});
            this.shapeContainer2.Size = new System.Drawing.Size(723, 615);
            this.shapeContainer2.TabIndex = 31;
            this.shapeContainer2.TabStop = false;
            // 
            // label8
            // 
            this.label8.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.label8.AutoSize = true;
            this.label8.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label8.Location = new System.Drawing.Point(508, 536);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(98, 15);
            this.label8.TabIndex = 33;
            this.label8.Text = "Saldo Anterior";
            // 
            // txtSaldoAnterior
            // 
            this.txtSaldoAnterior.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.txtSaldoAnterior.BackColor = System.Drawing.SystemColors.Control;
            this.txtSaldoAnterior.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtSaldoAnterior.Location = new System.Drawing.Point(612, 533);
            this.txtSaldoAnterior.Name = "txtSaldoAnterior";
            this.txtSaldoAnterior.ReadOnly = true;
            this.txtSaldoAnterior.Size = new System.Drawing.Size(102, 21);
            this.txtSaldoAnterior.TabIndex = 32;
            this.txtSaldoAnterior.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // formPagos
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Bisque;
            this.ClientSize = new System.Drawing.Size(723, 615);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.txtSaldoAnterior);
            this.Controls.Add(this.txtPagos);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.txtSaldo);
            this.Controls.Add(this.txtCompras);
            this.Controls.Add(this.label12);
            this.Controls.Add(this.grillaPagos);
            this.Controls.Add(this.btnSalir);
            this.Controls.Add(this.pnlBuscar);
            this.Controls.Add(this.barraControl);
            this.Controls.Add(this.shapeContainer2);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "formPagos";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Pagos";
            this.Load += new System.EventHandler(this.formPagos_Load);
            ((System.ComponentModel.ISupportInitialize)(this.grillaPagos)).EndInit();
            this.pnlBuscar.ResumeLayout(false);
            this.pnlBuscar.PerformLayout();
            this.barraControl.ResumeLayout(false);
            this.barraControl.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.DataGridView grillaPagos;
        protected System.Windows.Forms.Button btnSalir;
        protected System.Windows.Forms.Panel pnlBuscar;
        protected internal System.Windows.Forms.Button btnBuscar;
        private System.Windows.Forms.TextBox txtDescripcion;
        protected System.Windows.Forms.Label label2;
        protected System.Windows.Forms.Label label1;
        protected System.Windows.Forms.DateTimePicker txtFechaDesde;
        protected System.Windows.Forms.Label label3;
        protected System.Windows.Forms.DateTimePicker txtFechaHasta;
        protected System.Windows.Forms.Label label4;
        private Microsoft.VisualBasic.PowerPacks.ShapeContainer shapeContainer1;
        private Microsoft.VisualBasic.PowerPacks.LineShape lineShape2;
        protected internal System.Windows.Forms.ToolStrip barraControl;
        protected System.Windows.Forms.ToolStripButton nuevo;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox txtSaldo;
        private System.Windows.Forms.TextBox txtCompras;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.TextBox txtPagos;
        private System.Windows.Forms.Label label6;
        private Microsoft.VisualBasic.PowerPacks.LineShape lineShape4;
        private Microsoft.VisualBasic.PowerPacks.ShapeContainer shapeContainer2;
        private System.Windows.Forms.ComboBox comboTipoTramite;
        protected System.Windows.Forms.Label label7;
        protected System.Windows.Forms.ToolStripButton modificar;
        protected System.Windows.Forms.ToolStripButton eliminar;
        protected System.Windows.Forms.ToolStripButton Imprimir;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox txtSaldoAnterior;
    }
}