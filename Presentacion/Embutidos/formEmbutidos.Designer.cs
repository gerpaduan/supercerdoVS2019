namespace Presentacion
{
    partial class formEmbutidos
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(formEmbutidos));
            this.btnSalir = new System.Windows.Forms.Button();
            this.grillaEmbutidos = new System.Windows.Forms.DataGridView();
            this.barraControl = new System.Windows.Forms.ToolStrip();
            this.pnlBuscar = new System.Windows.Forms.Panel();
            this.lblActualizar = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.comboSucursal = new System.Windows.Forms.ComboBox();
            this.label7 = new System.Windows.Forms.Label();
            this.txtDescripcion = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.fechaDesde = new System.Windows.Forms.DateTimePicker();
            this.label3 = new System.Windows.Forms.Label();
            this.fechaHasta = new System.Windows.Forms.DateTimePicker();
            this.label4 = new System.Windows.Forms.Label();
            this.txtTotalKg = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.btnSeleccionar = new System.Windows.Forms.Button();
            this.btnBuscaProd = new System.Windows.Forms.Button();
            this.nuevo = new System.Windows.Forms.ToolStripButton();
            this.LineasEmb = new System.Windows.Forms.ToolStripButton();
            this.btnIngrRapido = new System.Windows.Forms.ToolStripButton();
            this.formulas = new System.Windows.Forms.ToolStripButton();
            ((System.ComponentModel.ISupportInitialize)(this.grillaEmbutidos)).BeginInit();
            this.barraControl.SuspendLayout();
            this.pnlBuscar.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnSalir
            // 
            this.btnSalir.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSalir.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSalir.Location = new System.Drawing.Point(988, 655);
            this.btnSalir.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnSalir.Name = "btnSalir";
            this.btnSalir.Size = new System.Drawing.Size(173, 32);
            this.btnSalir.TabIndex = 12;
            this.btnSalir.Text = "&Cerrar";
            this.btnSalir.UseVisualStyleBackColor = true;
            this.btnSalir.Click += new System.EventHandler(this.btnSalir_Click);
            // 
            // grillaEmbutidos
            // 
            this.grillaEmbutidos.AllowUserToAddRows = false;
            this.grillaEmbutidos.AllowUserToResizeRows = false;
            this.grillaEmbutidos.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.grillaEmbutidos.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.grillaEmbutidos.Location = new System.Drawing.Point(13, 153);
            this.grillaEmbutidos.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.grillaEmbutidos.MultiSelect = false;
            this.grillaEmbutidos.Name = "grillaEmbutidos";
            this.grillaEmbutidos.ReadOnly = true;
            this.grillaEmbutidos.RowHeadersVisible = false;
            this.grillaEmbutidos.RowHeadersWidth = 51;
            this.grillaEmbutidos.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.grillaEmbutidos.Size = new System.Drawing.Size(1148, 453);
            this.grillaEmbutidos.TabIndex = 11;
            this.grillaEmbutidos.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.grillaEmbutidos_CellDoubleClick);
            this.grillaEmbutidos.Sorted += new System.EventHandler(this.grillaEmbutidos_Sorted);
            // 
            // barraControl
            // 
            this.barraControl.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.barraControl.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.barraControl.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.nuevo,
            this.LineasEmb,
            this.btnIngrRapido,
            this.formulas});
            this.barraControl.Location = new System.Drawing.Point(0, 0);
            this.barraControl.Name = "barraControl";
            this.barraControl.Padding = new System.Windows.Forms.Padding(13, 0, 0, 0);
            this.barraControl.Size = new System.Drawing.Size(1177, 54);
            this.barraControl.TabIndex = 13;
            this.barraControl.TabStop = true;
            this.barraControl.Text = "toolStrip1";
            // 
            // pnlBuscar
            // 
            this.pnlBuscar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pnlBuscar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(43)))), ((int)(((byte)(77)))), ((int)(((byte)(129)))));
            this.pnlBuscar.Controls.Add(this.lblActualizar);
            this.pnlBuscar.Controls.Add(this.groupBox1);
            this.pnlBuscar.Controls.Add(this.comboSucursal);
            this.pnlBuscar.Controls.Add(this.label7);
            this.pnlBuscar.Controls.Add(this.btnBuscaProd);
            this.pnlBuscar.Controls.Add(this.txtDescripcion);
            this.pnlBuscar.Controls.Add(this.label2);
            this.pnlBuscar.Controls.Add(this.label1);
            this.pnlBuscar.Controls.Add(this.fechaDesde);
            this.pnlBuscar.Controls.Add(this.label3);
            this.pnlBuscar.Controls.Add(this.fechaHasta);
            this.pnlBuscar.Controls.Add(this.label4);
            this.pnlBuscar.Location = new System.Drawing.Point(-1, 55);
            this.pnlBuscar.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.pnlBuscar.Name = "pnlBuscar";
            this.pnlBuscar.Size = new System.Drawing.Size(1179, 90);
            this.pnlBuscar.TabIndex = 14;
            // 
            // lblActualizar
            // 
            this.lblActualizar.AutoSize = true;
            this.lblActualizar.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblActualizar.ForeColor = System.Drawing.Color.LightSalmon;
            this.lblActualizar.Location = new System.Drawing.Point(380, 52);
            this.lblActualizar.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblActualizar.Name = "lblActualizar";
            this.lblActualizar.Size = new System.Drawing.Size(84, 18);
            this.lblActualizar.TabIndex = 54;
            this.lblActualizar.Text = "Actualizar...";
            this.lblActualizar.Visible = false;
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.groupBox1.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.groupBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.groupBox1.Location = new System.Drawing.Point(597, 20);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.groupBox1.Size = new System.Drawing.Size(563, 9);
            this.groupBox1.TabIndex = 41;
            this.groupBox1.TabStop = false;
            // 
            // comboSucursal
            // 
            this.comboSucursal.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboSucursal.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.comboSucursal.FormattingEnabled = true;
            this.comboSucursal.Location = new System.Drawing.Point(132, 11);
            this.comboSucursal.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.comboSucursal.Name = "comboSucursal";
            this.comboSucursal.Size = new System.Drawing.Size(195, 26);
            this.comboSucursal.TabIndex = 39;
            this.comboSucursal.TabStop = false;
            this.comboSucursal.SelectedValueChanged += new System.EventHandler(this.comboSucursal_SelectedValueChanged);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label7.ForeColor = System.Drawing.Color.Cornsilk;
            this.label7.Location = new System.Drawing.Point(51, 16);
            this.label7.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(66, 18);
            this.label7.TabIndex = 40;
            this.label7.Text = "Sucursal";
            // 
            // txtDescripcion
            // 
            this.txtDescripcion.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtDescripcion.Location = new System.Drawing.Point(132, 44);
            this.txtDescripcion.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.txtDescripcion.Name = "txtDescripcion";
            this.txtDescripcion.Size = new System.Drawing.Size(193, 24);
            this.txtDescripcion.TabIndex = 12;
            this.txtDescripcion.TextChanged += new System.EventHandler(this.txtDescripcion_TextChanged);
            this.txtDescripcion.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtDescripcion_KeyDown);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.ForeColor = System.Drawing.Color.Cornsilk;
            this.label2.Location = new System.Drawing.Point(28, 47);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(87, 18);
            this.label2.TabIndex = 11;
            this.label2.Text = "Descripción";
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ForeColor = System.Drawing.Color.Cornsilk;
            this.label1.Location = new System.Drawing.Point(535, 10);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(49, 18);
            this.label1.TabIndex = 9;
            this.label1.Text = "Fecha";
            // 
            // fechaDesde
            // 
            this.fechaDesde.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.fechaDesde.CustomFormat = "dd/MM/yyyy  HH:mm:ss";
            this.fechaDesde.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.fechaDesde.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.fechaDesde.Location = new System.Drawing.Point(659, 37);
            this.fechaDesde.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.fechaDesde.Name = "fechaDesde";
            this.fechaDesde.Size = new System.Drawing.Size(211, 24);
            this.fechaDesde.TabIndex = 5;
            this.fechaDesde.Value = new System.DateTime(2016, 3, 22, 0, 0, 0, 0);
            this.fechaDesde.ValueChanged += new System.EventHandler(this.txtDescripcion_TextChanged);
            this.fechaDesde.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtDescripcion_KeyDown);
            // 
            // label3
            // 
            this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.ForeColor = System.Drawing.Color.Cornsilk;
            this.label3.Location = new System.Drawing.Point(593, 42);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(51, 18);
            this.label3.TabIndex = 4;
            this.label3.Text = "Desde";
            // 
            // fechaHasta
            // 
            this.fechaHasta.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.fechaHasta.CustomFormat = "dd/MM/yyyy  HH:mm:ss";
            this.fechaHasta.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.fechaHasta.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.fechaHasta.Location = new System.Drawing.Point(939, 36);
            this.fechaHasta.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.fechaHasta.Name = "fechaHasta";
            this.fechaHasta.Size = new System.Drawing.Size(209, 24);
            this.fechaHasta.TabIndex = 7;
            this.fechaHasta.ValueChanged += new System.EventHandler(this.txtDescripcion_TextChanged);
            this.fechaHasta.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtDescripcion_KeyDown);
            // 
            // label4
            // 
            this.label4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.ForeColor = System.Drawing.Color.Cornsilk;
            this.label4.Location = new System.Drawing.Point(879, 41);
            this.label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(47, 18);
            this.label4.TabIndex = 6;
            this.label4.Text = "Hasta";
            // 
            // txtTotalKg
            // 
            this.txtTotalKg.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.txtTotalKg.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtTotalKg.Location = new System.Drawing.Point(1029, 613);
            this.txtTotalKg.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.txtTotalKg.Name = "txtTotalKg";
            this.txtTotalKg.ReadOnly = true;
            this.txtTotalKg.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.txtTotalKg.Size = new System.Drawing.Size(131, 24);
            this.txtTotalKg.TabIndex = 21;
            this.txtTotalKg.TabStop = false;
            // 
            // label8
            // 
            this.label8.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.label8.AutoSize = true;
            this.label8.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label8.Location = new System.Drawing.Point(941, 617);
            this.label8.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(71, 18);
            this.label8.TabIndex = 20;
            this.label8.Text = "Total Kg";
            // 
            // panel1
            // 
            this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel1.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.panel1.Location = new System.Drawing.Point(16, 646);
            this.panel1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1145, 1);
            this.panel1.TabIndex = 24;
            // 
            // btnSeleccionar
            // 
            this.btnSeleccionar.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSeleccionar.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSeleccionar.Location = new System.Drawing.Point(807, 655);
            this.btnSeleccionar.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnSeleccionar.Name = "btnSeleccionar";
            this.btnSeleccionar.Size = new System.Drawing.Size(173, 32);
            this.btnSeleccionar.TabIndex = 25;
            this.btnSeleccionar.Text = "&Seleccionar";
            this.btnSeleccionar.UseVisualStyleBackColor = true;
            this.btnSeleccionar.Click += new System.EventHandler(this.btnSeleccionar_Click);
            // 
            // btnBuscaProd
            // 
            this.btnBuscaProd.AccessibleDescription = "";
            this.btnBuscaProd.ForeColor = System.Drawing.Color.Black;
            this.btnBuscaProd.Image = ((System.Drawing.Image)(resources.GetObject("btnBuscaProd.Image")));
            this.btnBuscaProd.Location = new System.Drawing.Point(335, 43);
            this.btnBuscaProd.Margin = new System.Windows.Forms.Padding(4);
            this.btnBuscaProd.Name = "btnBuscaProd";
            this.btnBuscaProd.Size = new System.Drawing.Size(37, 30);
            this.btnBuscaProd.TabIndex = 15;
            this.btnBuscaProd.UseVisualStyleBackColor = true;
            this.btnBuscaProd.Click += new System.EventHandler(this.btnBuscaProd_Click);
            // 
            // nuevo
            // 
            this.nuevo.Image = ((System.Drawing.Image)(resources.GetObject("nuevo.Image")));
            this.nuevo.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.nuevo.Margin = new System.Windows.Forms.Padding(0, 2, 0, 1);
            this.nuevo.Name = "nuevo";
            this.nuevo.Padding = new System.Windows.Forms.Padding(1, 1, 1, 6);
            this.nuevo.Size = new System.Drawing.Size(58, 51);
            this.nuevo.Text = "&Nuevo";
            this.nuevo.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.nuevo.Click += new System.EventHandler(this.nuevo_Click);
            // 
            // LineasEmb
            // 
            this.LineasEmb.Image = ((System.Drawing.Image)(resources.GetObject("LineasEmb.Image")));
            this.LineasEmb.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.LineasEmb.Name = "LineasEmb";
            this.LineasEmb.Size = new System.Drawing.Size(91, 51);
            this.LineasEmb.Text = "Lineas Emb.";
            this.LineasEmb.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.LineasEmb.Click += new System.EventHandler(this.LineasEmb_Click);
            // 
            // btnIngrRapido
            // 
            this.btnIngrRapido.Image = ((System.Drawing.Image)(resources.GetObject("btnIngrRapido.Image")));
            this.btnIngrRapido.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnIngrRapido.Name = "btnIngrRapido";
            this.btnIngrRapido.Size = new System.Drawing.Size(94, 51);
            this.btnIngrRapido.Text = "Ingr. Rápido";
            this.btnIngrRapido.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.btnIngrRapido.Click += new System.EventHandler(this.btnIngrRapido_Click);
            // 
            // formulas
            // 
            this.formulas.Image = ((System.Drawing.Image)(resources.GetObject("formulas.Image")));
            this.formulas.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.formulas.Name = "formulas";
            this.formulas.Size = new System.Drawing.Size(73, 51);
            this.formulas.Text = "Fórmulas";
            this.formulas.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.formulas.Click += new System.EventHandler(this.formulas_Click);
            // 
            // formEmbutidos
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(239)))), ((int)(((byte)(250)))));
            this.ClientSize = new System.Drawing.Size(1177, 690);
            this.Controls.Add(this.btnSeleccionar);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.txtTotalKg);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.pnlBuscar);
            this.Controls.Add(this.barraControl);
            this.Controls.Add(this.btnSalir);
            this.Controls.Add(this.grillaEmbutidos);
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.Name = "formEmbutidos";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Embutidos | Elaborados";
            this.Load += new System.EventHandler(this.formEmbutidos_Load);
            ((System.ComponentModel.ISupportInitialize)(this.grillaEmbutidos)).EndInit();
            this.barraControl.ResumeLayout(false);
            this.barraControl.PerformLayout();
            this.pnlBuscar.ResumeLayout(false);
            this.pnlBuscar.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        protected System.Windows.Forms.Button btnSalir;
        private System.Windows.Forms.DataGridView grillaEmbutidos;
        protected System.Windows.Forms.ToolStrip barraControl;
        protected System.Windows.Forms.ToolStripButton nuevo;
        protected System.Windows.Forms.Panel pnlBuscar;
        protected internal System.Windows.Forms.Button btnBuscaProd;
        private System.Windows.Forms.TextBox txtDescripcion;
        protected System.Windows.Forms.Label label2;
        protected System.Windows.Forms.Label label1;
        protected System.Windows.Forms.DateTimePicker fechaDesde;
        protected System.Windows.Forms.Label label3;
        protected System.Windows.Forms.DateTimePicker fechaHasta;
        protected System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox txtTotalKg;
        private System.Windows.Forms.Label label8;
        
        private System.Windows.Forms.ComboBox comboSucursal;
        protected System.Windows.Forms.Label label7;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.GroupBox groupBox1;
        protected System.Windows.Forms.Button btnSeleccionar;
        protected System.Windows.Forms.Label lblActualizar;
        private System.Windows.Forms.ToolStripButton LineasEmb;
        private System.Windows.Forms.ToolStripButton btnIngrRapido;
        private System.Windows.Forms.ToolStripButton formulas;
    }
}