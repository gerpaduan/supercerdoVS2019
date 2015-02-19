namespace Presentacion.Ventas
{
    partial class formInfoVenta
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle9 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle10 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle11 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle12 = new System.Windows.Forms.DataGridViewCellStyle();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(formInfoVenta));
            this.grillaLineasVenta = new System.Windows.Forms.DataGridView();
            this.idCorte = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.codigo = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.corte = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.cantKgs = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.precioKgs = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.totalS = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.estado = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.btnSalir = new System.Windows.Forms.Button();
            this.txtTotalS = new System.Windows.Forms.TextBox();
            this.txtTotalKgs = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.txtObservaciones = new System.Windows.Forms.TextBox();
            this.pnlBuscar = new System.Windows.Forms.Panel();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.txtFechaVenta = new System.Windows.Forms.DateTimePicker();
            this.txtTurno = new System.Windows.Forms.TextBox();
            this.txtSucursal = new System.Windows.Forms.TextBox();
            this.txtNroRemito = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.txtDiaFestivo = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.txtCliente = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.lineShape1 = new Microsoft.VisualBasic.PowerPacks.LineShape();
            this.shapeContainer1 = new Microsoft.VisualBasic.PowerPacks.ShapeContainer();
            this.barraControl = new System.Windows.Forms.ToolStrip();
            this.modificar = new System.Windows.Forms.ToolStripButton();
            this.agregaStock = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.Imprimir = new System.Windows.Forms.ToolStripButton();
            this.label11 = new System.Windows.Forms.Label();
            this.txtCantItems = new System.Windows.Forms.TextBox();
            this.label16 = new System.Windows.Forms.Label();
            this.label18 = new System.Windows.Forms.Label();
            this.txtActualizado = new System.Windows.Forms.TextBox();
            this.label17 = new System.Windows.Forms.Label();
            this.txtCreado = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.grillaLineasVenta)).BeginInit();
            this.pnlBuscar.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.barraControl.SuspendLayout();
            this.SuspendLayout();
            // 
            // grillaLineasVenta
            // 
            this.grillaLineasVenta.AllowUserToAddRows = false;
            this.grillaLineasVenta.AllowUserToResizeRows = false;
            this.grillaLineasVenta.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.grillaLineasVenta.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.grillaLineasVenta.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.idCorte,
            this.codigo,
            this.corte,
            this.cantKgs,
            this.precioKgs,
            this.totalS,
            this.estado});
            this.grillaLineasVenta.Location = new System.Drawing.Point(10, 159);
            this.grillaLineasVenta.Name = "grillaLineasVenta";
            this.grillaLineasVenta.ReadOnly = true;
            this.grillaLineasVenta.RowHeadersVisible = false;
            this.grillaLineasVenta.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.grillaLineasVenta.Size = new System.Drawing.Size(716, 319);
            this.grillaLineasVenta.TabIndex = 48;
            this.grillaLineasVenta.TabStop = false;
            // 
            // idCorte
            // 
            this.idCorte.DataPropertyName = "idCorte";
            this.idCorte.HeaderText = "ID Corte";
            this.idCorte.Name = "idCorte";
            this.idCorte.ReadOnly = true;
            this.idCorte.Visible = false;
            // 
            // codigo
            // 
            this.codigo.DataPropertyName = "codigo";
            this.codigo.HeaderText = "Código";
            this.codigo.Name = "codigo";
            this.codigo.ReadOnly = true;
            // 
            // corte
            // 
            this.corte.DataPropertyName = "corte";
            this.corte.HeaderText = "Corte";
            this.corte.Name = "corte";
            this.corte.ReadOnly = true;
            this.corte.Width = 150;
            // 
            // cantKgs
            // 
            this.cantKgs.DataPropertyName = "cantKgs";
            dataGridViewCellStyle9.Format = "N3";
            dataGridViewCellStyle9.NullValue = null;
            this.cantKgs.DefaultCellStyle = dataGridViewCellStyle9;
            this.cantKgs.HeaderText = "Cant. Kgs";
            this.cantKgs.Name = "cantKgs";
            this.cantKgs.ReadOnly = true;
            this.cantKgs.Width = 120;
            // 
            // precioKgs
            // 
            this.precioKgs.DataPropertyName = "precioKg";
            dataGridViewCellStyle10.Format = "N2";
            dataGridViewCellStyle10.NullValue = null;
            this.precioKgs.DefaultCellStyle = dataGridViewCellStyle10;
            this.precioKgs.HeaderText = "Precio Kg.";
            this.precioKgs.Name = "precioKgs";
            this.precioKgs.ReadOnly = true;
            this.precioKgs.Width = 120;
            // 
            // totalS
            // 
            this.totalS.DataPropertyName = "totalS";
            dataGridViewCellStyle11.Format = "N2";
            dataGridViewCellStyle11.NullValue = null;
            this.totalS.DefaultCellStyle = dataGridViewCellStyle11;
            this.totalS.HeaderText = "Total $";
            this.totalS.Name = "totalS";
            this.totalS.ReadOnly = true;
            this.totalS.Width = 120;
            // 
            // estado
            // 
            this.estado.DataPropertyName = "estado";
            dataGridViewCellStyle12.ForeColor = System.Drawing.Color.Red;
            this.estado.DefaultCellStyle = dataGridViewCellStyle12;
            this.estado.HeaderText = "Estado";
            this.estado.Name = "estado";
            this.estado.ReadOnly = true;
            // 
            // btnSalir
            // 
            this.btnSalir.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSalir.Location = new System.Drawing.Point(644, 550);
            this.btnSalir.Name = "btnSalir";
            this.btnSalir.Size = new System.Drawing.Size(84, 27);
            this.btnSalir.TabIndex = 47;
            this.btnSalir.TabStop = false;
            this.btnSalir.Text = "Salir";
            this.btnSalir.UseVisualStyleBackColor = true;
            this.btnSalir.Click += new System.EventHandler(this.btnSalir_Click);
            // 
            // txtTotalS
            // 
            this.txtTotalS.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.txtTotalS.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtTotalS.Location = new System.Drawing.Point(623, 524);
            this.txtTotalS.Name = "txtTotalS";
            this.txtTotalS.ReadOnly = true;
            this.txtTotalS.Size = new System.Drawing.Size(102, 21);
            this.txtTotalS.TabIndex = 45;
            this.txtTotalS.TabStop = false;
            this.txtTotalS.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // txtTotalKgs
            // 
            this.txtTotalKgs.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.txtTotalKgs.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtTotalKgs.Location = new System.Drawing.Point(623, 502);
            this.txtTotalKgs.Name = "txtTotalKgs";
            this.txtTotalKgs.ReadOnly = true;
            this.txtTotalKgs.Size = new System.Drawing.Size(102, 21);
            this.txtTotalKgs.TabIndex = 44;
            this.txtTotalKgs.TabStop = false;
            this.txtTotalKgs.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label9
            // 
            this.label9.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.label9.AutoSize = true;
            this.label9.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label9.Location = new System.Drawing.Point(566, 529);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(51, 15);
            this.label9.TabIndex = 43;
            this.label9.Text = "Total $";
            // 
            // label10
            // 
            this.label10.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.label10.AutoSize = true;
            this.label10.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label10.Location = new System.Drawing.Point(557, 507);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(60, 15);
            this.label10.TabIndex = 42;
            this.label10.Text = "Total Kg";
            // 
            // txtObservaciones
            // 
            this.txtObservaciones.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.txtObservaciones.Location = new System.Drawing.Point(9, 499);
            this.txtObservaciones.Multiline = true;
            this.txtObservaciones.Name = "txtObservaciones";
            this.txtObservaciones.ReadOnly = true;
            this.txtObservaciones.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtObservaciones.Size = new System.Drawing.Size(279, 43);
            this.txtObservaciones.TabIndex = 41;
            this.txtObservaciones.TabStop = false;
            // 
            // pnlBuscar
            // 
            this.pnlBuscar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.pnlBuscar.BackColor = System.Drawing.Color.SteelBlue;
            this.pnlBuscar.Controls.Add(this.groupBox1);
            this.pnlBuscar.Location = new System.Drawing.Point(0, 39);
            this.pnlBuscar.Name = "pnlBuscar";
            this.pnlBuscar.Size = new System.Drawing.Size(737, 114);
            this.pnlBuscar.TabIndex = 40;
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.BackColor = System.Drawing.Color.SteelBlue;
            this.groupBox1.Controls.Add(this.txtTurno);
            this.groupBox1.Controls.Add(this.txtSucursal);
            this.groupBox1.Controls.Add(this.txtNroRemito);
            this.groupBox1.Controls.Add(this.label8);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.txtDiaFestivo);
            this.groupBox1.Controls.Add(this.label6);
            this.groupBox1.Controls.Add(this.txtCliente);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.txtFechaVenta);
            this.groupBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox1.ForeColor = System.Drawing.Color.Cornsilk;
            this.groupBox1.Location = new System.Drawing.Point(12, 7);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(687, 93);
            this.groupBox1.TabIndex = 19;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Datos Venta";
            // 
            // txtFechaVenta
            // 
            this.txtFechaVenta.CustomFormat = "dd/MM/yyyy  HH:mm";
            this.txtFechaVenta.Enabled = false;
            this.txtFechaVenta.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.txtFechaVenta.Location = new System.Drawing.Point(506, 27);
            this.txtFechaVenta.Name = "txtFechaVenta";
            this.txtFechaVenta.Size = new System.Drawing.Size(145, 21);
            this.txtFechaVenta.TabIndex = 27;
            this.txtFechaVenta.TabStop = false;
            // 
            // txtTurno
            // 
            this.txtTurno.Location = new System.Drawing.Point(506, 61);
            this.txtTurno.Name = "txtTurno";
            this.txtTurno.ReadOnly = true;
            this.txtTurno.Size = new System.Drawing.Size(145, 21);
            this.txtTurno.TabIndex = 26;
            this.txtTurno.TabStop = false;
            // 
            // txtSucursal
            // 
            this.txtSucursal.Location = new System.Drawing.Point(333, 27);
            this.txtSucursal.Name = "txtSucursal";
            this.txtSucursal.ReadOnly = true;
            this.txtSucursal.Size = new System.Drawing.Size(105, 21);
            this.txtSucursal.TabIndex = 25;
            this.txtSucursal.TabStop = false;
            // 
            // txtNroRemito
            // 
            this.txtNroRemito.Location = new System.Drawing.Point(94, 59);
            this.txtNroRemito.Name = "txtNroRemito";
            this.txtNroRemito.ReadOnly = true;
            this.txtNroRemito.Size = new System.Drawing.Size(125, 21);
            this.txtNroRemito.TabIndex = 23;
            this.txtNroRemito.TabStop = false;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label8.ForeColor = System.Drawing.Color.Cornsilk;
            this.label8.Location = new System.Drawing.Point(18, 62);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(70, 15);
            this.label8.TabIndex = 24;
            this.label8.Text = "Nro Remito";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.ForeColor = System.Drawing.Color.Cornsilk;
            this.label4.Location = new System.Drawing.Point(272, 30);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(55, 15);
            this.label4.TabIndex = 17;
            this.label4.Text = "Sucursal";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.ForeColor = System.Drawing.Color.Cornsilk;
            this.label2.Location = new System.Drawing.Point(260, 64);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(67, 15);
            this.label2.TabIndex = 22;
            this.label2.Text = "Día Festivo";
            // 
            // txtDiaFestivo
            // 
            this.txtDiaFestivo.Location = new System.Drawing.Point(333, 61);
            this.txtDiaFestivo.Name = "txtDiaFestivo";
            this.txtDiaFestivo.ReadOnly = true;
            this.txtDiaFestivo.Size = new System.Drawing.Size(105, 21);
            this.txtDiaFestivo.TabIndex = 21;
            this.txtDiaFestivo.TabStop = false;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.ForeColor = System.Drawing.Color.Cornsilk;
            this.label6.Location = new System.Drawing.Point(461, 64);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(39, 15);
            this.label6.TabIndex = 20;
            this.label6.Text = "Turno";
            // 
            // txtCliente
            // 
            this.txtCliente.Location = new System.Drawing.Point(94, 27);
            this.txtCliente.Name = "txtCliente";
            this.txtCliente.ReadOnly = true;
            this.txtCliente.Size = new System.Drawing.Size(172, 21);
            this.txtCliente.TabIndex = 18;
            this.txtCliente.TabStop = false;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ForeColor = System.Drawing.Color.Cornsilk;
            this.label1.Location = new System.Drawing.Point(43, 30);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(45, 15);
            this.label1.TabIndex = 16;
            this.label1.Text = "Cliente";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.ForeColor = System.Drawing.Color.Cornsilk;
            this.label3.Location = new System.Drawing.Point(459, 30);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(41, 15);
            this.label3.TabIndex = 14;
            this.label3.Text = "Fecha";
            // 
            // lineShape1
            // 
            this.lineShape1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.lineShape1.Name = "lineShape1";
            this.lineShape1.X1 = 10;
            this.lineShape1.X2 = 727;
            this.lineShape1.Y1 = 546;
            this.lineShape1.Y2 = 546;
            // 
            // shapeContainer1
            // 
            this.shapeContainer1.Location = new System.Drawing.Point(0, 0);
            this.shapeContainer1.Margin = new System.Windows.Forms.Padding(0);
            this.shapeContainer1.Name = "shapeContainer1";
            this.shapeContainer1.Shapes.AddRange(new Microsoft.VisualBasic.PowerPacks.Shape[] {
            this.lineShape1});
            this.shapeContainer1.Size = new System.Drawing.Size(737, 580);
            this.shapeContainer1.TabIndex = 49;
            this.shapeContainer1.TabStop = false;
            // 
            // barraControl
            // 
            this.barraControl.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.barraControl.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.modificar,
            this.agregaStock,
            this.toolStripSeparator1,
            this.Imprimir});
            this.barraControl.Location = new System.Drawing.Point(0, 0);
            this.barraControl.Name = "barraControl";
            this.barraControl.Padding = new System.Windows.Forms.Padding(10, 0, 0, 0);
            this.barraControl.Size = new System.Drawing.Size(737, 45);
            this.barraControl.TabIndex = 50;
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
            // agregaStock
            // 
            this.agregaStock.Image = global::Presentacion.Properties.Resources._16__Database_remove_;
            this.agregaStock.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.agregaStock.Name = "agregaStock";
            this.agregaStock.Size = new System.Drawing.Size(85, 42);
            this.agregaStock.Text = "Agregar Stock";
            this.agregaStock.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.agregaStock.Click += new System.EventHandler(this.agregaStock_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 45);
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
            // label11
            // 
            this.label11.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(9, 483);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(76, 13);
            this.label11.TabIndex = 51;
            this.label11.Text = "observaciones";
            // 
            // txtCantItems
            // 
            this.txtCantItems.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.txtCantItems.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtCantItems.Location = new System.Drawing.Point(623, 480);
            this.txtCantItems.Name = "txtCantItems";
            this.txtCantItems.ReadOnly = true;
            this.txtCantItems.Size = new System.Drawing.Size(102, 21);
            this.txtCantItems.TabIndex = 53;
            this.txtCantItems.TabStop = false;
            this.txtCantItems.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label16
            // 
            this.label16.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.label16.AutoSize = true;
            this.label16.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label16.Location = new System.Drawing.Point(538, 485);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(79, 15);
            this.label16.TabIndex = 52;
            this.label16.Text = "Cant. Items";
            // 
            // label18
            // 
            this.label18.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label18.AutoSize = true;
            this.label18.Location = new System.Drawing.Point(296, 526);
            this.label18.Name = "label18";
            this.label18.Size = new System.Drawing.Size(62, 13);
            this.label18.TabIndex = 57;
            this.label18.Text = "Actualizado";
            // 
            // txtActualizado
            // 
            this.txtActualizado.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.txtActualizado.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtActualizado.Location = new System.Drawing.Point(364, 524);
            this.txtActualizado.Name = "txtActualizado";
            this.txtActualizado.ReadOnly = true;
            this.txtActualizado.Size = new System.Drawing.Size(148, 21);
            this.txtActualizado.TabIndex = 56;
            this.txtActualizado.TabStop = false;
            this.txtActualizado.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label17
            // 
            this.label17.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label17.AutoSize = true;
            this.label17.Location = new System.Drawing.Point(317, 503);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(41, 13);
            this.label17.TabIndex = 55;
            this.label17.Text = "Creado";
            // 
            // txtCreado
            // 
            this.txtCreado.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.txtCreado.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtCreado.Location = new System.Drawing.Point(364, 497);
            this.txtCreado.Name = "txtCreado";
            this.txtCreado.ReadOnly = true;
            this.txtCreado.Size = new System.Drawing.Size(148, 21);
            this.txtCreado.TabIndex = 54;
            this.txtCreado.TabStop = false;
            this.txtCreado.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // formInfoVenta
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Bisque;
            this.ClientSize = new System.Drawing.Size(737, 580);
            this.Controls.Add(this.label18);
            this.Controls.Add(this.txtActualizado);
            this.Controls.Add(this.label17);
            this.Controls.Add(this.txtCreado);
            this.Controls.Add(this.txtCantItems);
            this.Controls.Add(this.label16);
            this.Controls.Add(this.label11);
            this.Controls.Add(this.barraControl);
            this.Controls.Add(this.grillaLineasVenta);
            this.Controls.Add(this.btnSalir);
            this.Controls.Add(this.txtTotalS);
            this.Controls.Add(this.txtTotalKgs);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.label10);
            this.Controls.Add(this.txtObservaciones);
            this.Controls.Add(this.pnlBuscar);
            this.Controls.Add(this.shapeContainer1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "formInfoVenta";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Detalles de Venta";
            this.Load += new System.EventHandler(this.formInfoVenta_Load);
            ((System.ComponentModel.ISupportInitialize)(this.grillaLineasVenta)).EndInit();
            this.pnlBuscar.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.barraControl.ResumeLayout(false);
            this.barraControl.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.DataGridView grillaLineasVenta;
        protected System.Windows.Forms.Button btnSalir;
        private System.Windows.Forms.TextBox txtTotalS;
        private System.Windows.Forms.TextBox txtTotalKgs;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.TextBox txtObservaciones;
        protected System.Windows.Forms.Panel pnlBuscar;
        private Microsoft.VisualBasic.PowerPacks.LineShape lineShape1;
        private Microsoft.VisualBasic.PowerPacks.ShapeContainer shapeContainer1;
        protected System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TextBox txtNroRemito;
        protected System.Windows.Forms.Label label8;
        protected System.Windows.Forms.Label label4;
        protected System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtDiaFestivo;
        protected System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox txtCliente;
        protected System.Windows.Forms.Label label1;
        protected System.Windows.Forms.Label label3;
        protected internal System.Windows.Forms.ToolStrip barraControl;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.TextBox txtTurno;
        private System.Windows.Forms.TextBox txtSucursal;
        protected System.Windows.Forms.ToolStripButton modificar;
        private System.Windows.Forms.DateTimePicker txtFechaVenta;
        private System.Windows.Forms.ToolStripButton agregaStock;
        private System.Windows.Forms.TextBox txtCantItems;
        private System.Windows.Forms.Label label16;
        private System.Windows.Forms.DataGridViewTextBoxColumn idCorte;
        private System.Windows.Forms.DataGridViewTextBoxColumn codigo;
        private System.Windows.Forms.DataGridViewTextBoxColumn corte;
        private System.Windows.Forms.DataGridViewTextBoxColumn cantKgs;
        private System.Windows.Forms.DataGridViewTextBoxColumn precioKgs;
        private System.Windows.Forms.DataGridViewTextBoxColumn totalS;
        private System.Windows.Forms.DataGridViewTextBoxColumn estado;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        protected System.Windows.Forms.ToolStripButton Imprimir;
        private System.Windows.Forms.Label label18;
        private System.Windows.Forms.TextBox txtActualizado;
        private System.Windows.Forms.Label label17;
        private System.Windows.Forms.TextBox txtCreado;

    }
}