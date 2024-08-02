namespace Presentacion
{
    partial class formStock
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(formStock));
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle5 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            this.pnlBuscar = new System.Windows.Forms.Panel();
            this.lblActualizar = new System.Windows.Forms.Label();
            this.comboSucursal = new System.Windows.Forms.ComboBox();
            this.label7 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label1 = new System.Windows.Forms.Label();
            this.fechaDesde = new System.Windows.Forms.DateTimePicker();
            this.label3 = new System.Windows.Forms.Label();
            this.comboTipoCompra = new System.Windows.Forms.ComboBox();
            this.fechaHasta = new System.Windows.Forms.DateTimePicker();
            this.label4 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.btnBuscar = new System.Windows.Forms.Button();
            this.txtDescripcion = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.barraControl = new System.Windows.Forms.ToolStrip();
            this.btnIngreso = new System.Windows.Forms.ToolStripButton();
            this.btnEgreso = new System.Windows.Forms.ToolStripButton();
            this.btnCierre = new System.Windows.Forms.ToolStripButton();
            this.btnPesaje = new System.Windows.Forms.ToolStripButton();
            this.btnAjusteStock = new System.Windows.Forms.ToolStripButton();
            this.grillaCompras = new System.Windows.Forms.DataGridView();
            this.idCompra = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.fechaCompra = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.idPersona = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.razonSocial = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.sucursal = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.tipoCompra = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.cantKg = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.estado = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.observaciones = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.creado = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.creadoPor = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.actualizado = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.actualizadoPor = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.btnSeleccionar = new System.Windows.Forms.Button();
            this.btnCancelar = new System.Windows.Forms.Button();
            this.txtTotalKgs = new System.Windows.Forms.TextBox();
            this.label12 = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.pnlBuscar.SuspendLayout();
            this.barraControl.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.grillaCompras)).BeginInit();
            this.SuspendLayout();
            // 
            // pnlBuscar
            // 
            this.pnlBuscar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pnlBuscar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(43)))), ((int)(((byte)(77)))), ((int)(((byte)(129)))));
            this.pnlBuscar.Controls.Add(this.lblActualizar);
            this.pnlBuscar.Controls.Add(this.comboSucursal);
            this.pnlBuscar.Controls.Add(this.label7);
            this.pnlBuscar.Controls.Add(this.groupBox1);
            this.pnlBuscar.Controls.Add(this.label1);
            this.pnlBuscar.Controls.Add(this.fechaDesde);
            this.pnlBuscar.Controls.Add(this.label3);
            this.pnlBuscar.Controls.Add(this.comboTipoCompra);
            this.pnlBuscar.Controls.Add(this.fechaHasta);
            this.pnlBuscar.Controls.Add(this.label4);
            this.pnlBuscar.Controls.Add(this.label6);
            this.pnlBuscar.Controls.Add(this.btnBuscar);
            this.pnlBuscar.Controls.Add(this.txtDescripcion);
            this.pnlBuscar.Controls.Add(this.label2);
            this.pnlBuscar.Location = new System.Drawing.Point(0, 47);
            this.pnlBuscar.Margin = new System.Windows.Forms.Padding(4);
            this.pnlBuscar.Name = "pnlBuscar";
            this.pnlBuscar.Size = new System.Drawing.Size(1351, 110);
            this.pnlBuscar.TabIndex = 6;
            // 
            // lblActualizar
            // 
            this.lblActualizar.AutoSize = true;
            this.lblActualizar.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblActualizar.ForeColor = System.Drawing.Color.LightSalmon;
            this.lblActualizar.Location = new System.Drawing.Point(452, 76);
            this.lblActualizar.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblActualizar.Name = "lblActualizar";
            this.lblActualizar.Size = new System.Drawing.Size(84, 18);
            this.lblActualizar.TabIndex = 53;
            this.lblActualizar.Text = "Actualizar...";
            this.lblActualizar.Visible = false;
            // 
            // comboSucursal
            // 
            this.comboSucursal.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboSucursal.FormattingEnabled = true;
            this.comboSucursal.Location = new System.Drawing.Point(129, 7);
            this.comboSucursal.Margin = new System.Windows.Forms.Padding(4);
            this.comboSucursal.Name = "comboSucursal";
            this.comboSucursal.Size = new System.Drawing.Size(213, 24);
            this.comboSucursal.TabIndex = 41;
            this.comboSucursal.TabStop = false;
            this.comboSucursal.SelectedIndexChanged += new System.EventHandler(this.comboSucursal_SelectedIndexChanged);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label7.ForeColor = System.Drawing.Color.Cornsilk;
            this.label7.Location = new System.Drawing.Point(48, 12);
            this.label7.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(66, 18);
            this.label7.TabIndex = 42;
            this.label7.Text = "Sucursal";
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.groupBox1.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.groupBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.groupBox1.Location = new System.Drawing.Point(928, 50);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(4);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(4);
            this.groupBox1.Size = new System.Drawing.Size(387, 9);
            this.groupBox1.TabIndex = 25;
            this.groupBox1.TabStop = false;
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ForeColor = System.Drawing.Color.Cornsilk;
            this.label1.Location = new System.Drawing.Point(869, 47);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(49, 18);
            this.label1.TabIndex = 13;
            this.label1.Text = "Fecha";
            // 
            // fechaDesde
            // 
            this.fechaDesde.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.fechaDesde.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.fechaDesde.Location = new System.Drawing.Point(987, 73);
            this.fechaDesde.Margin = new System.Windows.Forms.Padding(4);
            this.fechaDesde.Name = "fechaDesde";
            this.fechaDesde.Size = new System.Drawing.Size(129, 22);
            this.fechaDesde.TabIndex = 5;
            this.fechaDesde.Value = new System.DateTime(2011, 9, 1, 0, 0, 0, 0);
            this.fechaDesde.ValueChanged += new System.EventHandler(this.fechaDesde_ValueChanged);
            this.fechaDesde.KeyDown += new System.Windows.Forms.KeyEventHandler(this.fechaDesde_KeyDown);
            // 
            // label3
            // 
            this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.ForeColor = System.Drawing.Color.Cornsilk;
            this.label3.Location = new System.Drawing.Point(923, 75);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(51, 18);
            this.label3.TabIndex = 4;
            this.label3.Text = "Desde";
            // 
            // comboTipoCompra
            // 
            this.comboTipoCompra.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboTipoCompra.FormattingEnabled = true;
            this.comboTipoCompra.Items.AddRange(new object[] {
            "Ver Todos",
            "Ingreso Stock",
            "Egreso Stock",
            "Cierre Stock",
            "Pesaje Cortes",
            "Ajuste Stock"});
            this.comboTipoCompra.Location = new System.Drawing.Point(129, 39);
            this.comboTipoCompra.Margin = new System.Windows.Forms.Padding(4);
            this.comboTipoCompra.Name = "comboTipoCompra";
            this.comboTipoCompra.Size = new System.Drawing.Size(213, 24);
            this.comboTipoCompra.TabIndex = 12;
            this.comboTipoCompra.SelectedValueChanged += new System.EventHandler(this.comboTipoCompra_SelectedValueChanged);
            // 
            // fechaHasta
            // 
            this.fechaHasta.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.fechaHasta.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.fechaHasta.Location = new System.Drawing.Point(1184, 73);
            this.fechaHasta.Margin = new System.Windows.Forms.Padding(4);
            this.fechaHasta.Name = "fechaHasta";
            this.fechaHasta.Size = new System.Drawing.Size(129, 22);
            this.fechaHasta.TabIndex = 7;
            this.fechaHasta.ValueChanged += new System.EventHandler(this.fechaHasta_ValueChanged);
            this.fechaHasta.KeyDown += new System.Windows.Forms.KeyEventHandler(this.fechaDesde_KeyDown);
            // 
            // label4
            // 
            this.label4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.ForeColor = System.Drawing.Color.Cornsilk;
            this.label4.Location = new System.Drawing.Point(1125, 75);
            this.label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(47, 18);
            this.label4.TabIndex = 6;
            this.label4.Text = "Hasta";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.ForeColor = System.Drawing.Color.Cornsilk;
            this.label6.Location = new System.Drawing.Point(64, 41);
            this.label6.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(53, 18);
            this.label6.TabIndex = 11;
            this.label6.Text = "Acción";
            // 
            // btnBuscar
            // 
            this.btnBuscar.Location = new System.Drawing.Point(352, 70);
            this.btnBuscar.Margin = new System.Windows.Forms.Padding(4);
            this.btnBuscar.Name = "btnBuscar";
            this.btnBuscar.Size = new System.Drawing.Size(92, 30);
            this.btnBuscar.TabIndex = 8;
            this.btnBuscar.Text = "&Buscar";
            this.btnBuscar.UseVisualStyleBackColor = true;
            this.btnBuscar.Click += new System.EventHandler(this.btnBuscar_Click);
            // 
            // txtDescripcion
            // 
            this.txtDescripcion.Location = new System.Drawing.Point(129, 73);
            this.txtDescripcion.Margin = new System.Windows.Forms.Padding(4);
            this.txtDescripcion.Name = "txtDescripcion";
            this.txtDescripcion.Size = new System.Drawing.Size(213, 22);
            this.txtDescripcion.TabIndex = 0;
            this.txtDescripcion.TextChanged += new System.EventHandler(this.txtDescripcion_TextChanged);
            this.txtDescripcion.KeyDown += new System.Windows.Forms.KeyEventHandler(this.fechaDesde_KeyDown);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.ForeColor = System.Drawing.Color.Cornsilk;
            this.label2.Location = new System.Drawing.Point(25, 74);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(87, 18);
            this.label2.TabIndex = 2;
            this.label2.Text = "Descripción";
            // 
            // barraControl
            // 
            this.barraControl.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.barraControl.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.barraControl.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.btnIngreso,
            this.btnEgreso,
            this.btnCierre,
            this.btnPesaje,
            this.btnAjusteStock});
            this.barraControl.Location = new System.Drawing.Point(0, 0);
            this.barraControl.Name = "barraControl";
            this.barraControl.Padding = new System.Windows.Forms.Padding(13, 0, 0, 0);
            this.barraControl.Size = new System.Drawing.Size(1349, 47);
            this.barraControl.TabIndex = 7;
            this.barraControl.TabStop = true;
            this.barraControl.Text = "toolStrip1";
            // 
            // btnIngreso
            // 
            this.btnIngreso.Image = ((System.Drawing.Image)(resources.GetObject("btnIngreso.Image")));
            this.btnIngreso.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnIngreso.Name = "btnIngreso";
            this.btnIngreso.Size = new System.Drawing.Size(62, 44);
            this.btnIngreso.Text = "&Ingreso";
            this.btnIngreso.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.btnIngreso.Click += new System.EventHandler(this.btnIngreso_Click);
            // 
            // btnEgreso
            // 
            this.btnEgreso.Image = ((System.Drawing.Image)(resources.GetObject("btnEgreso.Image")));
            this.btnEgreso.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnEgreso.Name = "btnEgreso";
            this.btnEgreso.Size = new System.Drawing.Size(58, 44);
            this.btnEgreso.Text = "&Egreso";
            this.btnEgreso.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.btnEgreso.Click += new System.EventHandler(this.btnEgreso_Click);
            // 
            // btnCierre
            // 
            this.btnCierre.Image = ((System.Drawing.Image)(resources.GetObject("btnCierre.Image")));
            this.btnCierre.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnCierre.Name = "btnCierre";
            this.btnCierre.Size = new System.Drawing.Size(52, 44);
            this.btnCierre.Text = "Cie&rre";
            this.btnCierre.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.btnCierre.Click += new System.EventHandler(this.btnCierre_Click);
            // 
            // btnPesaje
            // 
            this.btnPesaje.Image = ((System.Drawing.Image)(resources.GetObject("btnPesaje.Image")));
            this.btnPesaje.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnPesaje.Name = "btnPesaje";
            this.btnPesaje.Size = new System.Drawing.Size(54, 44);
            this.btnPesaje.Text = "&Pesaje";
            this.btnPesaje.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.btnPesaje.Click += new System.EventHandler(this.btnPesaje_Click);
            // 
            // btnAjusteStock
            // 
            this.btnAjusteStock.Image = ((System.Drawing.Image)(resources.GetObject("btnAjusteStock.Image")));
            this.btnAjusteStock.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnAjusteStock.Name = "btnAjusteStock";
            this.btnAjusteStock.Size = new System.Drawing.Size(54, 44);
            this.btnAjusteStock.Text = "&Ajuste";
            this.btnAjusteStock.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.btnAjusteStock.Click += new System.EventHandler(this.btnAjusteStock_Click);
            // 
            // grillaCompras
            // 
            this.grillaCompras.AllowDrop = true;
            this.grillaCompras.AllowUserToAddRows = false;
            this.grillaCompras.AllowUserToDeleteRows = false;
            this.grillaCompras.AllowUserToResizeRows = false;
            this.grillaCompras.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.grillaCompras.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.grillaCompras.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.grillaCompras.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.grillaCompras.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.idCompra,
            this.fechaCompra,
            this.idPersona,
            this.razonSocial,
            this.sucursal,
            this.tipoCompra,
            this.cantKg,
            this.estado,
            this.observaciones,
            this.creado,
            this.creadoPor,
            this.actualizado,
            this.actualizadoPor});
            dataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle4.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle4.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle4.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle4.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle4.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle4.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.grillaCompras.DefaultCellStyle = dataGridViewCellStyle4;
            this.grillaCompras.Location = new System.Drawing.Point(16, 165);
            this.grillaCompras.Margin = new System.Windows.Forms.Padding(4);
            this.grillaCompras.MultiSelect = false;
            this.grillaCompras.Name = "grillaCompras";
            this.grillaCompras.ReadOnly = true;
            dataGridViewCellStyle5.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle5.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle5.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle5.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle5.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle5.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle5.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.grillaCompras.RowHeadersDefaultCellStyle = dataGridViewCellStyle5;
            this.grillaCompras.RowHeadersVisible = false;
            this.grillaCompras.RowHeadersWidth = 51;
            this.grillaCompras.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.grillaCompras.Size = new System.Drawing.Size(1317, 422);
            this.grillaCompras.TabIndex = 8;
            this.grillaCompras.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.grillaCompras_CellDoubleClick);
            // 
            // idCompra
            // 
            this.idCompra.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.DisplayedCells;
            this.idCompra.DataPropertyName = "idCompra";
            this.idCompra.HeaderText = "ID";
            this.idCompra.MinimumWidth = 6;
            this.idCompra.Name = "idCompra";
            this.idCompra.ReadOnly = true;
            this.idCompra.Width = 50;
            // 
            // fechaCompra
            // 
            this.fechaCompra.DataPropertyName = "fechaCompra";
            dataGridViewCellStyle2.Format = "g";
            dataGridViewCellStyle2.NullValue = null;
            this.fechaCompra.DefaultCellStyle = dataGridViewCellStyle2;
            this.fechaCompra.FillWeight = 60F;
            this.fechaCompra.HeaderText = "Fecha";
            this.fechaCompra.MinimumWidth = 6;
            this.fechaCompra.Name = "fechaCompra";
            this.fechaCompra.ReadOnly = true;
            this.fechaCompra.Width = 84;
            // 
            // idPersona
            // 
            this.idPersona.DataPropertyName = "idProveedor";
            this.idPersona.HeaderText = "ID Proveedor";
            this.idPersona.MinimumWidth = 6;
            this.idPersona.Name = "idPersona";
            this.idPersona.ReadOnly = true;
            this.idPersona.Visible = false;
            this.idPersona.Width = 125;
            // 
            // razonSocial
            // 
            this.razonSocial.DataPropertyName = "razonSocial";
            this.razonSocial.FillWeight = 42.98663F;
            this.razonSocial.HeaderText = "Proveedor";
            this.razonSocial.MinimumWidth = 6;
            this.razonSocial.Name = "razonSocial";
            this.razonSocial.ReadOnly = true;
            this.razonSocial.Visible = false;
            this.razonSocial.Width = 125;
            // 
            // sucursal
            // 
            this.sucursal.DataPropertyName = "sucursal";
            this.sucursal.FillWeight = 42.98663F;
            this.sucursal.HeaderText = "Sucursal";
            this.sucursal.MinimumWidth = 6;
            this.sucursal.Name = "sucursal";
            this.sucursal.ReadOnly = true;
            this.sucursal.Width = 60;
            // 
            // tipoCompra
            // 
            this.tipoCompra.DataPropertyName = "tipoCompra";
            this.tipoCompra.FillWeight = 42.98663F;
            this.tipoCompra.HeaderText = "Acción";
            this.tipoCompra.MinimumWidth = 6;
            this.tipoCompra.Name = "tipoCompra";
            this.tipoCompra.ReadOnly = true;
            this.tipoCompra.Width = 60;
            // 
            // cantKg
            // 
            this.cantKg.DataPropertyName = "cantKg";
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
            dataGridViewCellStyle3.Format = "N2";
            dataGridViewCellStyle3.NullValue = null;
            this.cantKg.DefaultCellStyle = dataGridViewCellStyle3;
            this.cantKg.FillWeight = 42.98663F;
            this.cantKg.HeaderText = "Cant. Kgs";
            this.cantKg.MinimumWidth = 6;
            this.cantKg.Name = "cantKg";
            this.cantKg.ReadOnly = true;
            this.cantKg.Width = 61;
            // 
            // estado
            // 
            this.estado.DataPropertyName = "estado";
            this.estado.HeaderText = "Estado";
            this.estado.MinimumWidth = 6;
            this.estado.Name = "estado";
            this.estado.ReadOnly = true;
            this.estado.Width = 42;
            // 
            // observaciones
            // 
            this.observaciones.DataPropertyName = "observaciones";
            this.observaciones.FillWeight = 42.98663F;
            this.observaciones.HeaderText = "Observaciones";
            this.observaciones.MinimumWidth = 6;
            this.observaciones.Name = "observaciones";
            this.observaciones.ReadOnly = true;
            this.observaciones.Width = 60;
            // 
            // creado
            // 
            this.creado.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.creado.DataPropertyName = "creado";
            this.creado.HeaderText = "Creado";
            this.creado.MinimumWidth = 6;
            this.creado.Name = "creado";
            this.creado.ReadOnly = true;
            this.creado.Width = 83;
            // 
            // creadoPor
            // 
            this.creadoPor.DataPropertyName = "creadoPor";
            this.creadoPor.HeaderText = "Creado Por";
            this.creadoPor.MinimumWidth = 6;
            this.creadoPor.Name = "creadoPor";
            this.creadoPor.ReadOnly = true;
            this.creadoPor.Width = 140;
            // 
            // actualizado
            // 
            this.actualizado.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.actualizado.DataPropertyName = "actualizado";
            this.actualizado.HeaderText = "Actualizado";
            this.actualizado.MinimumWidth = 6;
            this.actualizado.Name = "actualizado";
            this.actualizado.ReadOnly = true;
            this.actualizado.Width = 110;
            // 
            // actualizadoPor
            // 
            this.actualizadoPor.DataPropertyName = "actualizadoPor";
            this.actualizadoPor.HeaderText = "ActualizadoPor";
            this.actualizadoPor.MinimumWidth = 6;
            this.actualizadoPor.Name = "actualizadoPor";
            this.actualizadoPor.ReadOnly = true;
            this.actualizadoPor.Width = 140;
            // 
            // btnSeleccionar
            // 
            this.btnSeleccionar.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSeleccionar.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSeleccionar.Location = new System.Drawing.Point(997, 638);
            this.btnSeleccionar.Margin = new System.Windows.Forms.Padding(4);
            this.btnSeleccionar.Name = "btnSeleccionar";
            this.btnSeleccionar.Size = new System.Drawing.Size(164, 33);
            this.btnSeleccionar.TabIndex = 17;
            this.btnSeleccionar.Text = "&Seleccionar";
            this.btnSeleccionar.UseVisualStyleBackColor = true;
            this.btnSeleccionar.Click += new System.EventHandler(this.btnSeleccionar_Click_1);
            // 
            // btnCancelar
            // 
            this.btnCancelar.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancelar.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnCancelar.Location = new System.Drawing.Point(1169, 638);
            this.btnCancelar.Margin = new System.Windows.Forms.Padding(4);
            this.btnCancelar.Name = "btnCancelar";
            this.btnCancelar.Size = new System.Drawing.Size(164, 33);
            this.btnCancelar.TabIndex = 16;
            this.btnCancelar.Text = "&Cerrar";
            this.btnCancelar.UseVisualStyleBackColor = true;
            this.btnCancelar.Click += new System.EventHandler(this.btnCancelar_Click);
            // 
            // txtTotalKgs
            // 
            this.txtTotalKgs.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.txtTotalKgs.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtTotalKgs.Location = new System.Drawing.Point(1195, 594);
            this.txtTotalKgs.Margin = new System.Windows.Forms.Padding(4);
            this.txtTotalKgs.Name = "txtTotalKgs";
            this.txtTotalKgs.ReadOnly = true;
            this.txtTotalKgs.Size = new System.Drawing.Size(135, 24);
            this.txtTotalKgs.TabIndex = 22;
            this.txtTotalKgs.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label12
            // 
            this.label12.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.label12.AutoSize = true;
            this.label12.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label12.Location = new System.Drawing.Point(1093, 601);
            this.label12.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(85, 18);
            this.label12.TabIndex = 21;
            this.label12.Text = "Total Kgs.";
            // 
            // panel1
            // 
            this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel1.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.panel1.Location = new System.Drawing.Point(16, 629);
            this.panel1.Margin = new System.Windows.Forms.Padding(4);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1314, 1);
            this.panel1.TabIndex = 23;
            // 
            // formStock
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(239)))), ((int)(((byte)(250)))));
            this.ClientSize = new System.Drawing.Size(1349, 676);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.txtTotalKgs);
            this.Controls.Add(this.label12);
            this.Controls.Add(this.btnSeleccionar);
            this.Controls.Add(this.btnCancelar);
            this.Controls.Add(this.grillaCompras);
            this.Controls.Add(this.barraControl);
            this.Controls.Add(this.pnlBuscar);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Sizable;
            this.KeyPreview = true;
            this.Margin = new System.Windows.Forms.Padding(5);
            this.MinimizeBox = true;
            this.Name = "formStock";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Stock";
            this.Load += new System.EventHandler(this.formStock_Load);
            this.pnlBuscar.ResumeLayout(false);
            this.pnlBuscar.PerformLayout();
            this.barraControl.ResumeLayout(false);
            this.barraControl.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.grillaCompras)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        
        
        
        protected System.Windows.Forms.Panel pnlBuscar;
        protected System.Windows.Forms.DateTimePicker fechaDesde;
        protected System.Windows.Forms.Label label3;
        protected System.Windows.Forms.DateTimePicker fechaHasta;
        protected System.Windows.Forms.Button btnBuscar;
        protected System.Windows.Forms.Label label4;
        protected System.Windows.Forms.TextBox txtDescripcion;
        protected System.Windows.Forms.Label label2;
        
        protected internal System.Windows.Forms.ToolStrip barraControl;
        private System.Windows.Forms.DataGridView grillaCompras;
        protected System.Windows.Forms.Button btnSeleccionar;
        protected System.Windows.Forms.Button btnCancelar;
        private System.Windows.Forms.TextBox txtTotalKgs;
        private System.Windows.Forms.Label label12;
        protected System.Windows.Forms.Label label6;
        private System.Windows.Forms.ComboBox comboTipoCompra;
        private System.Windows.Forms.Panel panel1;
        protected System.Windows.Forms.Label label1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.ComboBox comboSucursal;
        protected System.Windows.Forms.Label label7;
        private System.Windows.Forms.ToolStripButton btnIngreso;
        private System.Windows.Forms.ToolStripButton btnCierre;
        private System.Windows.Forms.ToolStripButton btnEgreso;
        protected System.Windows.Forms.Label lblActualizar;
        private System.Windows.Forms.ToolStripButton btnPesaje;
        private System.Windows.Forms.ToolStripButton btnAjusteStock;
        private System.Windows.Forms.DataGridViewTextBoxColumn idCompra;
        private System.Windows.Forms.DataGridViewTextBoxColumn fechaCompra;
        private System.Windows.Forms.DataGridViewTextBoxColumn idPersona;
        private System.Windows.Forms.DataGridViewTextBoxColumn razonSocial;
        private System.Windows.Forms.DataGridViewTextBoxColumn sucursal;
        private System.Windows.Forms.DataGridViewTextBoxColumn tipoCompra;
        private System.Windows.Forms.DataGridViewTextBoxColumn cantKg;
        private System.Windows.Forms.DataGridViewTextBoxColumn estado;
        private System.Windows.Forms.DataGridViewTextBoxColumn observaciones;
        private System.Windows.Forms.DataGridViewTextBoxColumn creado;
        private System.Windows.Forms.DataGridViewTextBoxColumn creadoPor;
        private System.Windows.Forms.DataGridViewTextBoxColumn actualizado;
        private System.Windows.Forms.DataGridViewTextBoxColumn actualizadoPor;

    }
}