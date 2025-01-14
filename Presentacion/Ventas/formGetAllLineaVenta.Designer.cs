namespace Presentacion
{
    partial class formGetAllLineaVenta
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(formGetAllLineaVenta));
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            this.label5 = new System.Windows.Forms.Label();
            this.txtTotalS = new System.Windows.Forms.TextBox();
            this.btnSeleccionar = new System.Windows.Forms.Button();
            this.btnCancelar = new System.Windows.Forms.Button();
            this.barraControl = new System.Windows.Forms.ToolStrip();
            this.menuDuplicar = new System.Windows.Forms.ToolStripButton();
            this.pnlBuscar = new System.Windows.Forms.Panel();
            this.lblActualizar = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.comboUsuario = new System.Windows.Forms.ComboBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.comboSucursal = new System.Windows.Forms.ComboBox();
            this.label7 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.fechaDesde = new System.Windows.Forms.DateTimePicker();
            this.label3 = new System.Windows.Forms.Label();
            this.fechaHasta = new System.Windows.Forms.DateTimePicker();
            this.btnBuscar = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.txtDescripcion = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.grillaVentas = new System.Windows.Forms.DataGridView();
            this.label6 = new System.Windows.Forms.Label();
            this.txtTotalKgs = new System.Windows.Forms.TextBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.exportExcel = new System.Windows.Forms.ToolStripButton();
            this.label9 = new System.Windows.Forms.Label();
            this.txtCantItems = new System.Windows.Forms.TextBox();
            this.barraControl.SuspendLayout();
            this.pnlBuscar.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.grillaVentas)).BeginInit();
            this.SuspendLayout();
            // 
            // label5
            // 
            this.label5.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.Location = new System.Drawing.Point(832, 596);
            this.label5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(60, 18);
            this.label5.TabIndex = 33;
            this.label5.Text = "Total $";
            // 
            // txtTotalS
            // 
            this.txtTotalS.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.txtTotalS.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtTotalS.Location = new System.Drawing.Point(900, 593);
            this.txtTotalS.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.txtTotalS.Name = "txtTotalS";
            this.txtTotalS.ReadOnly = true;
            this.txtTotalS.Size = new System.Drawing.Size(167, 24);
            this.txtTotalS.TabIndex = 32;
            this.txtTotalS.TabStop = false;
            this.txtTotalS.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // btnSeleccionar
            // 
            this.btnSeleccionar.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSeleccionar.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.25F, System.Drawing.FontStyle.Bold);
            this.btnSeleccionar.Location = new System.Drawing.Point(753, 635);
            this.btnSeleccionar.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnSeleccionar.Name = "btnSeleccionar";
            this.btnSeleccionar.Size = new System.Drawing.Size(156, 33);
            this.btnSeleccionar.TabIndex = 29;
            this.btnSeleccionar.Text = "Seleccionar";
            this.btnSeleccionar.UseVisualStyleBackColor = true;
            this.btnSeleccionar.Click += new System.EventHandler(this.btnSeleccionar_Click);
            // 
            // btnCancelar
            // 
            this.btnCancelar.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancelar.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.25F, System.Drawing.FontStyle.Bold);
            this.btnCancelar.Location = new System.Drawing.Point(917, 635);
            this.btnCancelar.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnCancelar.Name = "btnCancelar";
            this.btnCancelar.Size = new System.Drawing.Size(151, 32);
            this.btnCancelar.TabIndex = 28;
            this.btnCancelar.Text = "&Cerrar";
            this.btnCancelar.UseVisualStyleBackColor = true;
            this.btnCancelar.Click += new System.EventHandler(this.btnCancelar_Click);
            // 
            // barraControl
            // 
            this.barraControl.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.barraControl.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.barraControl.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuDuplicar,
            this.exportExcel});
            this.barraControl.Location = new System.Drawing.Point(0, 0);
            this.barraControl.Name = "barraControl";
            this.barraControl.Padding = new System.Windows.Forms.Padding(13, 0, 0, 0);
            this.barraControl.Size = new System.Drawing.Size(1088, 54);
            this.barraControl.TabIndex = 26;
            this.barraControl.TabStop = true;
            this.barraControl.Text = "toolStrip1";
            // 
            // menuDuplicar
            // 
            this.menuDuplicar.Image = ((System.Drawing.Image)(resources.GetObject("menuDuplicar.Image")));
            this.menuDuplicar.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.menuDuplicar.Margin = new System.Windows.Forms.Padding(0, 2, 0, 1);
            this.menuDuplicar.Name = "menuDuplicar";
            this.menuDuplicar.Padding = new System.Windows.Forms.Padding(1, 1, 1, 6);
            this.menuDuplicar.Size = new System.Drawing.Size(71, 51);
            this.menuDuplicar.Text = "Duplicar";
            this.menuDuplicar.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.menuDuplicar.Click += new System.EventHandler(this.menuDuplicar_Click);
            // 
            // pnlBuscar
            // 
            this.pnlBuscar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pnlBuscar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(43)))), ((int)(((byte)(77)))), ((int)(((byte)(129)))));
            this.pnlBuscar.Controls.Add(this.lblActualizar);
            this.pnlBuscar.Controls.Add(this.label8);
            this.pnlBuscar.Controls.Add(this.comboUsuario);
            this.pnlBuscar.Controls.Add(this.groupBox2);
            this.pnlBuscar.Controls.Add(this.comboSucursal);
            this.pnlBuscar.Controls.Add(this.label7);
            this.pnlBuscar.Controls.Add(this.label1);
            this.pnlBuscar.Controls.Add(this.fechaDesde);
            this.pnlBuscar.Controls.Add(this.label3);
            this.pnlBuscar.Controls.Add(this.fechaHasta);
            this.pnlBuscar.Controls.Add(this.btnBuscar);
            this.pnlBuscar.Controls.Add(this.label4);
            this.pnlBuscar.Controls.Add(this.txtDescripcion);
            this.pnlBuscar.Controls.Add(this.label2);
            this.pnlBuscar.Location = new System.Drawing.Point(-1, 55);
            this.pnlBuscar.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.pnlBuscar.Name = "pnlBuscar";
            this.pnlBuscar.Size = new System.Drawing.Size(1089, 102);
            this.pnlBuscar.TabIndex = 25;
            // 
            // lblActualizar
            // 
            this.lblActualizar.AutoSize = true;
            this.lblActualizar.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblActualizar.ForeColor = System.Drawing.Color.LightSalmon;
            this.lblActualizar.Location = new System.Drawing.Point(429, 73);
            this.lblActualizar.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblActualizar.Name = "lblActualizar";
            this.lblActualizar.Size = new System.Drawing.Size(84, 18);
            this.lblActualizar.TabIndex = 52;
            this.lblActualizar.Text = "Actualizar...";
            this.lblActualizar.Visible = false;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label8.ForeColor = System.Drawing.Color.Cornsilk;
            this.label8.Location = new System.Drawing.Point(33, 42);
            this.label8.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(71, 18);
            this.label8.TabIndex = 50;
            this.label8.Text = "Vendedor";
            // 
            // comboUsuario
            // 
            this.comboUsuario.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboUsuario.FormattingEnabled = true;
            this.comboUsuario.Location = new System.Drawing.Point(121, 41);
            this.comboUsuario.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.comboUsuario.Name = "comboUsuario";
            this.comboUsuario.Size = new System.Drawing.Size(213, 24);
            this.comboUsuario.TabIndex = 49;
            this.comboUsuario.TabStop = false;
            this.comboUsuario.SelectedValueChanged += new System.EventHandler(this.btnBuscar_Click);
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox2.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.groupBox2.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.groupBox2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.groupBox2.Location = new System.Drawing.Point(603, 11);
            this.groupBox2.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.groupBox2.Size = new System.Drawing.Size(471, 9);
            this.groupBox2.TabIndex = 47;
            this.groupBox2.TabStop = false;
            // 
            // comboSucursal
            // 
            this.comboSucursal.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboSucursal.FormattingEnabled = true;
            this.comboSucursal.Location = new System.Drawing.Point(121, 9);
            this.comboSucursal.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.comboSucursal.Name = "comboSucursal";
            this.comboSucursal.Size = new System.Drawing.Size(213, 24);
            this.comboSucursal.TabIndex = 37;
            this.comboSucursal.TabStop = false;
            this.comboSucursal.SelectedIndexChanged += new System.EventHandler(this.comboSucursal_SelectedIndexChanged);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label7.ForeColor = System.Drawing.Color.Cornsilk;
            this.label7.Location = new System.Drawing.Point(40, 14);
            this.label7.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(66, 18);
            this.label7.TabIndex = 38;
            this.label7.Text = "Sucursal";
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ForeColor = System.Drawing.Color.Cornsilk;
            this.label1.Location = new System.Drawing.Point(540, 1);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(49, 18);
            this.label1.TabIndex = 9;
            this.label1.Text = "Fecha";
            // 
            // fechaDesde
            // 
            this.fechaDesde.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.fechaDesde.CustomFormat = "dd/MM/yyyy  HH:mm";
            this.fechaDesde.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.fechaDesde.Location = new System.Drawing.Point(664, 26);
            this.fechaDesde.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.fechaDesde.Name = "fechaDesde";
            this.fechaDesde.Size = new System.Drawing.Size(169, 22);
            this.fechaDesde.TabIndex = 5;
            this.fechaDesde.Value = new System.DateTime(2011, 7, 1, 0, 0, 0, 0);
            this.fechaDesde.ValueChanged += new System.EventHandler(this.fechaDesde_ValueChanged);
            this.fechaDesde.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtDescripcion_KeyDown);
            // 
            // label3
            // 
            this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.ForeColor = System.Drawing.Color.Cornsilk;
            this.label3.Location = new System.Drawing.Point(599, 28);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(51, 18);
            this.label3.TabIndex = 4;
            this.label3.Text = "Desde";
            // 
            // fechaHasta
            // 
            this.fechaHasta.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.fechaHasta.CustomFormat = "dd/MM/yyyy  HH:mm";
            this.fechaHasta.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.fechaHasta.Location = new System.Drawing.Point(903, 26);
            this.fechaHasta.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.fechaHasta.Name = "fechaHasta";
            this.fechaHasta.Size = new System.Drawing.Size(169, 22);
            this.fechaHasta.TabIndex = 7;
            this.fechaHasta.Value = new System.DateTime(2016, 3, 2, 23, 27, 0, 0);
            this.fechaHasta.ValueChanged += new System.EventHandler(this.fechaDesde_ValueChanged);
            this.fechaHasta.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtDescripcion_KeyDown);
            // 
            // btnBuscar
            // 
            this.btnBuscar.Location = new System.Drawing.Point(344, 68);
            this.btnBuscar.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnBuscar.Name = "btnBuscar";
            this.btnBuscar.Size = new System.Drawing.Size(77, 30);
            this.btnBuscar.TabIndex = 8;
            this.btnBuscar.TabStop = false;
            this.btnBuscar.Text = "Buscar";
            this.btnBuscar.UseVisualStyleBackColor = true;
            this.btnBuscar.Click += new System.EventHandler(this.btnBuscar_Click);
            // 
            // label4
            // 
            this.label4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.ForeColor = System.Drawing.Color.Cornsilk;
            this.label4.Location = new System.Drawing.Point(843, 28);
            this.label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(47, 18);
            this.label4.TabIndex = 6;
            this.label4.Text = "Hasta";
            // 
            // txtDescripcion
            // 
            this.txtDescripcion.Location = new System.Drawing.Point(121, 70);
            this.txtDescripcion.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.txtDescripcion.Name = "txtDescripcion";
            this.txtDescripcion.Size = new System.Drawing.Size(213, 22);
            this.txtDescripcion.TabIndex = 0;
            this.txtDescripcion.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtDescripcion_KeyDown);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.ForeColor = System.Drawing.Color.Cornsilk;
            this.label2.Location = new System.Drawing.Point(17, 71);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(87, 18);
            this.label2.TabIndex = 2;
            this.label2.Text = "Descripción";
            // 
            // grillaVentas
            // 
            this.grillaVentas.AllowDrop = true;
            this.grillaVentas.AllowUserToAddRows = false;
            this.grillaVentas.AllowUserToDeleteRows = false;
            this.grillaVentas.AllowUserToResizeRows = false;
            this.grillaVentas.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.grillaVentas.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.grillaVentas.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.grillaVentas.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.grillaVentas.DefaultCellStyle = dataGridViewCellStyle2;
            this.grillaVentas.Location = new System.Drawing.Point(15, 165);
            this.grillaVentas.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.grillaVentas.MultiSelect = false;
            this.grillaVentas.Name = "grillaVentas";
            this.grillaVentas.ReadOnly = true;
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.grillaVentas.RowHeadersDefaultCellStyle = dataGridViewCellStyle3;
            this.grillaVentas.RowHeadersVisible = false;
            this.grillaVentas.RowHeadersWidth = 51;
            this.grillaVentas.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.grillaVentas.Size = new System.Drawing.Size(1053, 368);
            this.grillaVentas.TabIndex = 27;
            this.grillaVentas.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.grillaVentas_CellDoubleClick);
            // 
            // label6
            // 
            this.label6.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.Location = new System.Drawing.Point(812, 569);
            this.label6.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(80, 18);
            this.label6.TabIndex = 36;
            this.label6.Text = "Total Kgs";
            // 
            // txtTotalKgs
            // 
            this.txtTotalKgs.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.txtTotalKgs.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtTotalKgs.Location = new System.Drawing.Point(900, 566);
            this.txtTotalKgs.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.txtTotalKgs.Name = "txtTotalKgs";
            this.txtTotalKgs.ReadOnly = true;
            this.txtTotalKgs.Size = new System.Drawing.Size(167, 24);
            this.txtTotalKgs.TabIndex = 35;
            this.txtTotalKgs.TabStop = false;
            this.txtTotalKgs.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // panel1
            // 
            this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel1.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.panel1.Location = new System.Drawing.Point(16, 626);
            this.panel1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1055, 1);
            this.panel1.TabIndex = 37;
            // 
            // exportExcel
            // 
            this.exportExcel.Image = ((System.Drawing.Image)(resources.GetObject("exportExcel.Image")));
            this.exportExcel.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.exportExcel.Name = "exportExcel";
            this.exportExcel.Size = new System.Drawing.Size(77, 51);
            this.exportExcel.Text = "  &Exportar";
            this.exportExcel.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.exportExcel.ToolTipText = "Exportar lista a Excel\r\n\r\n\r\n";
            this.exportExcel.Click += new System.EventHandler(this.exportExcel_Click);
            // 
            // label9
            // 
            this.label9.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.label9.AutoSize = true;
            this.label9.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label9.Location = new System.Drawing.Point(798, 544);
            this.label9.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(94, 18);
            this.label9.TabIndex = 41;
            this.label9.Text = "Cant. Ítems";
            // 
            // txtCantItems
            // 
            this.txtCantItems.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.txtCantItems.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtCantItems.Location = new System.Drawing.Point(900, 541);
            this.txtCantItems.Margin = new System.Windows.Forms.Padding(4);
            this.txtCantItems.Name = "txtCantItems";
            this.txtCantItems.ReadOnly = true;
            this.txtCantItems.Size = new System.Drawing.Size(167, 24);
            this.txtCantItems.TabIndex = 40;
            this.txtCantItems.TabStop = false;
            this.txtCantItems.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // formGetAllLineaVenta
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(239)))), ((int)(((byte)(250)))));
            this.ClientSize = new System.Drawing.Size(1088, 676);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.txtCantItems);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.txtTotalKgs);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.txtTotalS);
            this.Controls.Add(this.btnSeleccionar);
            this.Controls.Add(this.btnCancelar);
            this.Controls.Add(this.grillaVentas);
            this.Controls.Add(this.barraControl);
            this.Controls.Add(this.pnlBuscar);
            this.KeyPreview = true;
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.Name = "formGetAllLineaVenta";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Lineas venta";
            this.Load += new System.EventHandler(this.formGetAllLineaVenta_Load);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.formGetAllLineaVenta_KeyDown);
            this.barraControl.ResumeLayout(false);
            this.barraControl.PerformLayout();
            this.pnlBuscar.ResumeLayout(false);
            this.pnlBuscar.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.grillaVentas)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox txtTotalS;
        protected System.Windows.Forms.Button btnSeleccionar;
        protected System.Windows.Forms.Button btnCancelar;
        protected internal System.Windows.Forms.ToolStrip barraControl;
        protected System.Windows.Forms.Panel pnlBuscar;
        protected System.Windows.Forms.Label label1;
        protected System.Windows.Forms.DateTimePicker fechaDesde;
        protected System.Windows.Forms.Label label3;
        protected System.Windows.Forms.DateTimePicker fechaHasta;
        protected System.Windows.Forms.Button btnBuscar;
        protected System.Windows.Forms.Label label4;
        protected System.Windows.Forms.TextBox txtDescripcion;
        protected System.Windows.Forms.Label label2;
        private System.Windows.Forms.DataGridView grillaVentas;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox txtTotalKgs;
        private System.Windows.Forms.ComboBox comboSucursal;
        protected System.Windows.Forms.Label label7;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.GroupBox groupBox2;
        protected System.Windows.Forms.Label label8;
        private System.Windows.Forms.ComboBox comboUsuario;
        protected System.Windows.Forms.Label lblActualizar;
        protected System.Windows.Forms.ToolStripButton menuDuplicar;
        private System.Windows.Forms.ToolStripButton exportExcel;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.TextBox txtCantItems;
    }
}