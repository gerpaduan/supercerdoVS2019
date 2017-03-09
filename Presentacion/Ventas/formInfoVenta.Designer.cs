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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(formInfoVenta));
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle5 = new System.Windows.Forms.DataGridViewCellStyle();
            this.grillaLineasVenta = new System.Windows.Forms.DataGridView();
            this.btnSalir = new System.Windows.Forms.Button();
            this.txtTotalS = new System.Windows.Forms.TextBox();
            this.txtTotalKgs = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.txtObservaciones = new System.Windows.Forms.TextBox();
            this.pnlBuscar = new System.Windows.Forms.Panel();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.txtFechaVenta = new System.Windows.Forms.TextBox();
            this.txtVendedor = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.txtIdVenta = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.txtSucursal = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.txtDiaFestivo = new System.Windows.Forms.TextBox();
            this.txtCliente = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.barraControl = new System.Windows.Forms.ToolStrip();
            this.modificar = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.Imprimir = new System.Windows.Forms.ToolStripButton();
            this.label11 = new System.Windows.Forms.Label();
            this.txtCantItems = new System.Windows.Forms.TextBox();
            this.label16 = new System.Windows.Forms.Label();
            this.label18 = new System.Windows.Forms.Label();
            this.txtActualizado = new System.Windows.Forms.TextBox();
            this.label17 = new System.Windows.Forms.Label();
            this.txtCreado = new System.Windows.Forms.TextBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.idVentaLabel = new System.Windows.Forms.Label();
            this.idCorte = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.codigo = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.corte = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.cantKgs = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.precioKgs = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.totalS = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.bonificacion = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.PesoBalanza = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.estado = new System.Windows.Forms.DataGridViewTextBoxColumn();
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
            this.bonificacion,
            this.PesoBalanza,
            this.estado});
            this.grillaLineasVenta.Location = new System.Drawing.Point(10, 174);
            this.grillaLineasVenta.Name = "grillaLineasVenta";
            this.grillaLineasVenta.ReadOnly = true;
            this.grillaLineasVenta.RowHeadersVisible = false;
            this.grillaLineasVenta.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.grillaLineasVenta.Size = new System.Drawing.Size(719, 304);
            this.grillaLineasVenta.TabIndex = 48;
            this.grillaLineasVenta.TabStop = false;
            // 
            // btnSalir
            // 
            this.btnSalir.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSalir.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.25F, System.Drawing.FontStyle.Bold);
            this.btnSalir.Location = new System.Drawing.Point(572, 550);
            this.btnSalir.Name = "btnSalir";
            this.btnSalir.Size = new System.Drawing.Size(159, 27);
            this.btnSalir.TabIndex = 47;
            this.btnSalir.TabStop = false;
            this.btnSalir.Text = "&Salir";
            this.btnSalir.UseVisualStyleBackColor = true;
            this.btnSalir.Click += new System.EventHandler(this.btnSalir_Click);
            // 
            // txtTotalS
            // 
            this.txtTotalS.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.txtTotalS.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtTotalS.Location = new System.Drawing.Point(626, 524);
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
            this.txtTotalKgs.Location = new System.Drawing.Point(626, 502);
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
            this.label9.Location = new System.Drawing.Point(569, 529);
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
            this.label10.Location = new System.Drawing.Point(560, 507);
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
            this.pnlBuscar.Location = new System.Drawing.Point(0, 45);
            this.pnlBuscar.Name = "pnlBuscar";
            this.pnlBuscar.Size = new System.Drawing.Size(740, 123);
            this.pnlBuscar.TabIndex = 40;
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.BackColor = System.Drawing.Color.SteelBlue;
            this.groupBox1.Controls.Add(this.txtFechaVenta);
            this.groupBox1.Controls.Add(this.txtVendedor);
            this.groupBox1.Controls.Add(this.label6);
            this.groupBox1.Controls.Add(this.txtIdVenta);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.txtSucursal);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.txtDiaFestivo);
            this.groupBox1.Controls.Add(this.txtCliente);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox1.ForeColor = System.Drawing.Color.Cornsilk;
            this.groupBox1.Location = new System.Drawing.Point(12, 5);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(717, 108);
            this.groupBox1.TabIndex = 19;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Datos Venta";
            // 
            // txtFechaVenta
            // 
            this.txtFechaVenta.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtFechaVenta.Location = new System.Drawing.Point(507, 50);
            this.txtFechaVenta.Name = "txtFechaVenta";
            this.txtFechaVenta.ReadOnly = true;
            this.txtFechaVenta.Size = new System.Drawing.Size(179, 22);
            this.txtFechaVenta.TabIndex = 32;
            this.txtFechaVenta.TabStop = false;
            // 
            // txtVendedor
            // 
            this.txtVendedor.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtVendedor.Location = new System.Drawing.Point(86, 49);
            this.txtVendedor.Name = "txtVendedor";
            this.txtVendedor.ReadOnly = true;
            this.txtVendedor.Size = new System.Drawing.Size(206, 22);
            this.txtVendedor.TabIndex = 31;
            this.txtVendedor.TabStop = false;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.ForeColor = System.Drawing.Color.Cornsilk;
            this.label6.Location = new System.Drawing.Point(20, 52);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(60, 15);
            this.label6.TabIndex = 30;
            this.label6.Text = "Vendedor";
            // 
            // txtIdVenta
            // 
            this.txtIdVenta.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtIdVenta.Location = new System.Drawing.Point(86, 22);
            this.txtIdVenta.Name = "txtIdVenta";
            this.txtIdVenta.ReadOnly = true;
            this.txtIdVenta.Size = new System.Drawing.Size(206, 22);
            this.txtIdVenta.TabIndex = 29;
            this.txtIdVenta.TabStop = false;
            this.txtIdVenta.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.ForeColor = System.Drawing.Color.Cornsilk;
            this.label5.Location = new System.Drawing.Point(29, 25);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(51, 15);
            this.label5.TabIndex = 28;
            this.label5.Text = "Id Venta";
            // 
            // txtSucursal
            // 
            this.txtSucursal.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtSucursal.Location = new System.Drawing.Point(507, 22);
            this.txtSucursal.Name = "txtSucursal";
            this.txtSucursal.ReadOnly = true;
            this.txtSucursal.Size = new System.Drawing.Size(179, 22);
            this.txtSucursal.TabIndex = 25;
            this.txtSucursal.TabStop = false;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.ForeColor = System.Drawing.Color.Cornsilk;
            this.label4.Location = new System.Drawing.Point(446, 25);
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
            this.label2.Location = new System.Drawing.Point(434, 80);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(67, 15);
            this.label2.TabIndex = 22;
            this.label2.Text = "Día Festivo";
            // 
            // txtDiaFestivo
            // 
            this.txtDiaFestivo.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtDiaFestivo.Location = new System.Drawing.Point(507, 77);
            this.txtDiaFestivo.Name = "txtDiaFestivo";
            this.txtDiaFestivo.ReadOnly = true;
            this.txtDiaFestivo.Size = new System.Drawing.Size(179, 22);
            this.txtDiaFestivo.TabIndex = 21;
            this.txtDiaFestivo.TabStop = false;
            // 
            // txtCliente
            // 
            this.txtCliente.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtCliente.Location = new System.Drawing.Point(86, 76);
            this.txtCliente.Name = "txtCliente";
            this.txtCliente.ReadOnly = true;
            this.txtCliente.Size = new System.Drawing.Size(206, 22);
            this.txtCliente.TabIndex = 18;
            this.txtCliente.TabStop = false;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ForeColor = System.Drawing.Color.Cornsilk;
            this.label1.Location = new System.Drawing.Point(35, 79);
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
            this.label3.Location = new System.Drawing.Point(460, 52);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(41, 15);
            this.label3.TabIndex = 14;
            this.label3.Text = "Fecha";
            // 
            // barraControl
            // 
            this.barraControl.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.barraControl.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.modificar,
            this.toolStripSeparator1,
            this.Imprimir});
            this.barraControl.Location = new System.Drawing.Point(0, 0);
            this.barraControl.Name = "barraControl";
            this.barraControl.Padding = new System.Windows.Forms.Padding(10, 0, 0, 0);
            this.barraControl.Size = new System.Drawing.Size(740, 45);
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
            this.modificar.Text = "&Modificar";
            this.modificar.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.modificar.Click += new System.EventHandler(this.modificar_Click);
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
            this.txtCantItems.Location = new System.Drawing.Point(626, 480);
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
            this.label16.Location = new System.Drawing.Point(541, 485);
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
            this.txtActualizado.Location = new System.Drawing.Point(367, 524);
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
            this.txtCreado.Location = new System.Drawing.Point(367, 497);
            this.txtCreado.Name = "txtCreado";
            this.txtCreado.ReadOnly = true;
            this.txtCreado.Size = new System.Drawing.Size(148, 21);
            this.txtCreado.TabIndex = 54;
            this.txtCreado.TabStop = false;
            this.txtCreado.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // panel1
            // 
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel1.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.panel1.Location = new System.Drawing.Point(9, 548);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(719, 1);
            this.panel1.TabIndex = 58;
            // 
            // idVentaLabel
            // 
            this.idVentaLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.idVentaLabel.AutoSize = true;
            this.idVentaLabel.Location = new System.Drawing.Point(12, 558);
            this.idVentaLabel.Name = "idVentaLabel";
            this.idVentaLabel.Size = new System.Drawing.Size(69, 13);
            this.idVentaLabel.TabIndex = 59;
            this.idVentaLabel.Text = "idVentaLabel";
            this.idVentaLabel.Visible = false;
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
            this.codigo.Width = 80;
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
            dataGridViewCellStyle1.Format = "N3";
            dataGridViewCellStyle1.NullValue = null;
            this.cantKgs.DefaultCellStyle = dataGridViewCellStyle1;
            this.cantKgs.HeaderText = "Cant. Kgs";
            this.cantKgs.Name = "cantKgs";
            this.cantKgs.ReadOnly = true;
            this.cantKgs.Width = 110;
            // 
            // precioKgs
            // 
            this.precioKgs.DataPropertyName = "precioKg";
            dataGridViewCellStyle2.Format = "N2";
            dataGridViewCellStyle2.NullValue = null;
            this.precioKgs.DefaultCellStyle = dataGridViewCellStyle2;
            this.precioKgs.HeaderText = "Precio Kg.";
            this.precioKgs.Name = "precioKgs";
            this.precioKgs.ReadOnly = true;
            this.precioKgs.Width = 80;
            // 
            // totalS
            // 
            this.totalS.DataPropertyName = "totalS";
            dataGridViewCellStyle3.Format = "N2";
            dataGridViewCellStyle3.NullValue = null;
            this.totalS.DefaultCellStyle = dataGridViewCellStyle3;
            this.totalS.HeaderText = "Total $";
            this.totalS.Name = "totalS";
            this.totalS.ReadOnly = true;
            this.totalS.Width = 110;
            // 
            // bonificacion
            // 
            this.bonificacion.DataPropertyName = "bonificacion";
            dataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
            dataGridViewCellStyle4.Format = "N2";
            this.bonificacion.DefaultCellStyle = dataGridViewCellStyle4;
            this.bonificacion.HeaderText = "Bonif.";
            this.bonificacion.Name = "bonificacion";
            this.bonificacion.ReadOnly = true;
            this.bonificacion.Width = 60;
            // 
            // PesoBalanza
            // 
            this.PesoBalanza.DataPropertyName = "pesoBalanza";
            this.PesoBalanza.HeaderText = "Balanza";
            this.PesoBalanza.Name = "PesoBalanza";
            this.PesoBalanza.ReadOnly = true;
            this.PesoBalanza.Width = 50;
            // 
            // estado
            // 
            this.estado.DataPropertyName = "estado";
            dataGridViewCellStyle5.ForeColor = System.Drawing.Color.Red;
            this.estado.DefaultCellStyle = dataGridViewCellStyle5;
            this.estado.HeaderText = "Estado";
            this.estado.Name = "estado";
            this.estado.ReadOnly = true;
            this.estado.Width = 70;
            // 
            // formInfoVenta
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Bisque;
            this.ClientSize = new System.Drawing.Size(740, 580);
            this.Controls.Add(this.idVentaLabel);
            this.Controls.Add(this.panel1);
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
        protected System.Windows.Forms.GroupBox groupBox1;
        protected System.Windows.Forms.Label label4;
        protected System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtDiaFestivo;
        private System.Windows.Forms.TextBox txtCliente;
        protected System.Windows.Forms.Label label1;
        protected System.Windows.Forms.Label label3;
        protected internal System.Windows.Forms.ToolStrip barraControl;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.TextBox txtSucursal;
        protected System.Windows.Forms.ToolStripButton modificar;
        private System.Windows.Forms.TextBox txtCantItems;
        private System.Windows.Forms.Label label16;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        protected System.Windows.Forms.ToolStripButton Imprimir;
        private System.Windows.Forms.Label label18;
        private System.Windows.Forms.TextBox txtActualizado;
        private System.Windows.Forms.Label label17;
        private System.Windows.Forms.TextBox txtCreado;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.TextBox txtIdVenta;
        protected System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox txtVendedor;
        protected System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox txtFechaVenta;
        private System.Windows.Forms.Label idVentaLabel;
        private System.Windows.Forms.DataGridViewTextBoxColumn idCorte;
        private System.Windows.Forms.DataGridViewTextBoxColumn codigo;
        private System.Windows.Forms.DataGridViewTextBoxColumn corte;
        private System.Windows.Forms.DataGridViewTextBoxColumn cantKgs;
        private System.Windows.Forms.DataGridViewTextBoxColumn precioKgs;
        private System.Windows.Forms.DataGridViewTextBoxColumn totalS;
        private System.Windows.Forms.DataGridViewTextBoxColumn bonificacion;
        private System.Windows.Forms.DataGridViewCheckBoxColumn PesoBalanza;
        private System.Windows.Forms.DataGridViewTextBoxColumn estado;
    }
}