namespace Presentacion.Compras
{
    partial class formModificarCompra
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle13 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle17 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle18 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle14 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle15 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle16 = new System.Windows.Forms.DataGridViewCellStyle();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(formModificarCompra));
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle19 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle23 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle24 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle20 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle21 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle22 = new System.Windows.Forms.DataGridViewCellStyle();
            this.grillaCortePorCompra = new System.Windows.Forms.DataGridView();
            this.idCorte = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Codigo = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Corte = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.cantKgs = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.precioKgs = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.totalS = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.idSucursal = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.sucursal = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.txtObservaciones = new System.Windows.Forms.TextBox();
            this.txtTotalS = new System.Windows.Forms.TextBox();
            this.txtTotalKgs = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.btnAceptar = new System.Windows.Forms.Button();
            this.btnCancelar = new System.Windows.Forms.Button();
            this.pnlBuscar = new System.Windows.Forms.Panel();
            this.groupCantMedias = new System.Windows.Forms.GroupBox();
            this.txtCantMedias = new System.Windows.Forms.TextBox();
            this.label14 = new System.Windows.Forms.Label();
            this.txtUsuario = new System.Windows.Forms.TextBox();
            this.label13 = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.radioCorte = new System.Windows.Forms.RadioButton();
            this.radioMediaRes = new System.Windows.Forms.RadioButton();
            this.radioIngresoStock = new System.Windows.Forms.RadioButton();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.txtFechaCompra = new System.Windows.Forms.DateTimePicker();
            this.btnBuscarProv = new System.Windows.Forms.Button();
            this.txtProveedor = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.txtNroRemito = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.comboSucursal = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.grupoMediaRes = new System.Windows.Forms.GroupBox();
            this.txtPrecioKg = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.btnQuitar = new System.Windows.Forms.Button();
            this.btnAgregar = new System.Windows.Forms.Button();
            this.panelCorte = new System.Windows.Forms.Panel();
            this.btnBuscaCorte = new System.Windows.Forms.Button();
            this.txtCodigo = new System.Windows.Forms.MaskedTextBox();
            this.txtCantKgs = new System.Windows.Forms.TextBox();
            this.label18 = new System.Windows.Forms.Label();
            this.txtCorteNuevaCompra = new System.Windows.Forms.TextBox();
            this.label16 = new System.Windows.Forms.Label();
            this.label17 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.txtNroTropa = new System.Windows.Forms.TextBox();
            this.txtKgMedia = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.grillaMediaRes = new System.Windows.Forms.DataGridView();
            this.idMedia = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.nroTropa = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.kgMedia = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.precioMedia = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.totalPs = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.idSucursalMedia = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.sucursalM = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.barraControl = new System.Windows.Forms.ToolStrip();
            this.modificar = new System.Windows.Forms.ToolStripButton();
            this.quitarStock = new System.Windows.Forms.ToolStripButton();
            this.cambiarPrecio = new System.Windows.Forms.ToolStripButton();
            this.PorcentajesCorte = new System.Windows.Forms.ToolStripButton();
            this.Reporte = new System.Windows.Forms.ToolStripButton();
            this.panelCompraAnulada = new System.Windows.Forms.Panel();
            this.label10 = new System.Windows.Forms.Label();
            this.txtCantItems = new System.Windows.Forms.TextBox();
            this.label12 = new System.Windows.Forms.Label();
            this.label15 = new System.Windows.Forms.Label();
            this.txtActualizadoPor = new System.Windows.Forms.TextBox();
            this.txtActualizado = new System.Windows.Forms.TextBox();
            this.label19 = new System.Windows.Forms.Label();
            this.txtCreadoPor = new System.Windows.Forms.TextBox();
            this.txtCreado = new System.Windows.Forms.TextBox();
            this.checkCtaCte = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.grillaCortePorCompra)).BeginInit();
            this.pnlBuscar.SuspendLayout();
            this.groupCantMedias.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.grupoMediaRes.SuspendLayout();
            this.panelCorte.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.grillaMediaRes)).BeginInit();
            this.barraControl.SuspendLayout();
            this.panelCompraAnulada.SuspendLayout();
            this.SuspendLayout();
            // 
            // grillaCortePorCompra
            // 
            this.grillaCortePorCompra.AllowUserToAddRows = false;
            this.grillaCortePorCompra.AllowUserToResizeRows = false;
            this.grillaCortePorCompra.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.grillaCortePorCompra.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            dataGridViewCellStyle13.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle13.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle13.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle13.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle13.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle13.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle13.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.grillaCortePorCompra.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle13;
            this.grillaCortePorCompra.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.grillaCortePorCompra.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.idCorte,
            this.Codigo,
            this.Corte,
            this.cantKgs,
            this.precioKgs,
            this.totalS,
            this.idSucursal,
            this.sucursal});
            dataGridViewCellStyle17.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle17.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle17.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle17.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle17.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle17.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle17.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.grillaCortePorCompra.DefaultCellStyle = dataGridViewCellStyle17;
            this.grillaCortePorCompra.Location = new System.Drawing.Point(20, 189);
            this.grillaCortePorCompra.MultiSelect = false;
            this.grillaCortePorCompra.Name = "grillaCortePorCompra";
            this.grillaCortePorCompra.ReadOnly = true;
            dataGridViewCellStyle18.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle18.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle18.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle18.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle18.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle18.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle18.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.grillaCortePorCompra.RowHeadersDefaultCellStyle = dataGridViewCellStyle18;
            this.grillaCortePorCompra.RowHeadersVisible = false;
            this.grillaCortePorCompra.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.grillaCortePorCompra.Size = new System.Drawing.Size(642, 379);
            this.grillaCortePorCompra.TabIndex = 25;
            this.grillaCortePorCompra.TabStop = false;
            // 
            // idCorte
            // 
            this.idCorte.DataPropertyName = "idCorte";
            this.idCorte.HeaderText = "ID Corte";
            this.idCorte.Name = "idCorte";
            this.idCorte.ReadOnly = true;
            this.idCorte.Visible = false;
            // 
            // Codigo
            // 
            this.Codigo.DataPropertyName = "Codigo";
            this.Codigo.FillWeight = 80F;
            this.Codigo.HeaderText = "Codigo";
            this.Codigo.Name = "Codigo";
            this.Codigo.ReadOnly = true;
            // 
            // Corte
            // 
            this.Corte.DataPropertyName = "Corte";
            this.Corte.FillWeight = 120F;
            this.Corte.HeaderText = "Corte";
            this.Corte.Name = "Corte";
            this.Corte.ReadOnly = true;
            // 
            // cantKgs
            // 
            this.cantKgs.DataPropertyName = "cantKgs";
            dataGridViewCellStyle14.Format = "N3";
            dataGridViewCellStyle14.NullValue = null;
            this.cantKgs.DefaultCellStyle = dataGridViewCellStyle14;
            this.cantKgs.HeaderText = "Cant. Kgs";
            this.cantKgs.Name = "cantKgs";
            this.cantKgs.ReadOnly = true;
            // 
            // precioKgs
            // 
            this.precioKgs.DataPropertyName = "precioKg";
            dataGridViewCellStyle15.Format = "N2";
            dataGridViewCellStyle15.NullValue = null;
            this.precioKgs.DefaultCellStyle = dataGridViewCellStyle15;
            this.precioKgs.HeaderText = "Precio Kg.";
            this.precioKgs.Name = "precioKgs";
            this.precioKgs.ReadOnly = true;
            // 
            // totalS
            // 
            this.totalS.DataPropertyName = "TotalS";
            dataGridViewCellStyle16.Format = "N2";
            dataGridViewCellStyle16.NullValue = null;
            this.totalS.DefaultCellStyle = dataGridViewCellStyle16;
            this.totalS.HeaderText = "Total $";
            this.totalS.Name = "totalS";
            this.totalS.ReadOnly = true;
            // 
            // idSucursal
            // 
            this.idSucursal.DataPropertyName = "idSucursal";
            this.idSucursal.HeaderText = "ID Sucursal";
            this.idSucursal.Name = "idSucursal";
            this.idSucursal.ReadOnly = true;
            this.idSucursal.Visible = false;
            // 
            // sucursal
            // 
            this.sucursal.DataPropertyName = "Sucursal";
            this.sucursal.HeaderText = "Sucursal";
            this.sucursal.Name = "sucursal";
            this.sucursal.ReadOnly = true;
            this.sucursal.Visible = false;
            // 
            // txtObservaciones
            // 
            this.txtObservaciones.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.txtObservaciones.Location = new System.Drawing.Point(671, 315);
            this.txtObservaciones.Multiline = true;
            this.txtObservaciones.Name = "txtObservaciones";
            this.txtObservaciones.ReadOnly = true;
            this.txtObservaciones.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtObservaciones.Size = new System.Drawing.Size(134, 68);
            this.txtObservaciones.TabIndex = 8;
            this.txtObservaciones.TabStop = false;
            this.txtObservaciones.TextChanged += new System.EventHandler(this.txtObservaciones_TextChanged);
            // 
            // txtTotalS
            // 
            this.txtTotalS.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.txtTotalS.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtTotalS.Location = new System.Drawing.Point(671, 277);
            this.txtTotalS.Name = "txtTotalS";
            this.txtTotalS.ReadOnly = true;
            this.txtTotalS.Size = new System.Drawing.Size(134, 21);
            this.txtTotalS.TabIndex = 24;
            this.txtTotalS.TabStop = false;
            this.txtTotalS.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // txtTotalKgs
            // 
            this.txtTotalKgs.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.txtTotalKgs.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtTotalKgs.Location = new System.Drawing.Point(671, 237);
            this.txtTotalKgs.Name = "txtTotalKgs";
            this.txtTotalKgs.ReadOnly = true;
            this.txtTotalKgs.Size = new System.Drawing.Size(134, 21);
            this.txtTotalKgs.TabIndex = 23;
            this.txtTotalKgs.TabStop = false;
            this.txtTotalKgs.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label9
            // 
            this.label9.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.label9.AutoSize = true;
            this.label9.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label9.Location = new System.Drawing.Point(668, 261);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(51, 15);
            this.label9.TabIndex = 21;
            this.label9.Text = "Total $";
            // 
            // label8
            // 
            this.label8.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.label8.AutoSize = true;
            this.label8.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label8.Location = new System.Drawing.Point(668, 221);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(60, 15);
            this.label8.TabIndex = 18;
            this.label8.Text = "Total Kg";
            // 
            // btnAceptar
            // 
            this.btnAceptar.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnAceptar.BackColor = System.Drawing.Color.SeaGreen;
            this.btnAceptar.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnAceptar.ForeColor = System.Drawing.SystemColors.ControlLight;
            this.btnAceptar.Location = new System.Drawing.Point(671, 507);
            this.btnAceptar.Name = "btnAceptar";
            this.btnAceptar.Size = new System.Drawing.Size(134, 35);
            this.btnAceptar.TabIndex = 9;
            this.btnAceptar.TabStop = false;
            this.btnAceptar.Text = "&Guardar";
            this.btnAceptar.UseVisualStyleBackColor = false;
            this.btnAceptar.Visible = false;
            this.btnAceptar.Click += new System.EventHandler(this.btnAceptar_Click);
            // 
            // btnCancelar
            // 
            this.btnCancelar.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancelar.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.btnCancelar.Location = new System.Drawing.Point(671, 542);
            this.btnCancelar.Name = "btnCancelar";
            this.btnCancelar.Size = new System.Drawing.Size(134, 26);
            this.btnCancelar.TabIndex = 10;
            this.btnCancelar.TabStop = false;
            this.btnCancelar.Text = "&Salir";
            this.btnCancelar.UseVisualStyleBackColor = true;
            this.btnCancelar.Click += new System.EventHandler(this.btnCancelar_Click);
            // 
            // pnlBuscar
            // 
            this.pnlBuscar.BackColor = System.Drawing.Color.SteelBlue;
            this.pnlBuscar.Controls.Add(this.checkCtaCte);
            this.pnlBuscar.Controls.Add(this.groupCantMedias);
            this.pnlBuscar.Controls.Add(this.txtUsuario);
            this.pnlBuscar.Controls.Add(this.label13);
            this.pnlBuscar.Controls.Add(this.groupBox2);
            this.pnlBuscar.Controls.Add(this.groupBox1);
            this.pnlBuscar.Controls.Add(this.grupoMediaRes);
            this.pnlBuscar.Location = new System.Drawing.Point(0, 38);
            this.pnlBuscar.Name = "pnlBuscar";
            this.pnlBuscar.Size = new System.Drawing.Size(817, 145);
            this.pnlBuscar.TabIndex = 17;
            // 
            // groupCantMedias
            // 
            this.groupCantMedias.Controls.Add(this.txtCantMedias);
            this.groupCantMedias.Controls.Add(this.label14);
            this.groupCantMedias.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupCantMedias.ForeColor = System.Drawing.Color.Cornsilk;
            this.groupCantMedias.Location = new System.Drawing.Point(607, 44);
            this.groupCantMedias.Name = "groupCantMedias";
            this.groupCantMedias.Size = new System.Drawing.Size(198, 37);
            this.groupCantMedias.TabIndex = 55;
            this.groupCantMedias.TabStop = false;
            this.groupCantMedias.Visible = false;
            // 
            // txtCantMedias
            // 
            this.txtCantMedias.Location = new System.Drawing.Point(99, 12);
            this.txtCantMedias.Name = "txtCantMedias";
            this.txtCantMedias.ReadOnly = true;
            this.txtCantMedias.Size = new System.Drawing.Size(83, 21);
            this.txtCantMedias.TabIndex = 18;
            this.txtCantMedias.TabStop = false;
            this.txtCantMedias.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.txtCantMedias.TextChanged += new System.EventHandler(this.txtCantMedias_TextChanged);
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label14.ForeColor = System.Drawing.Color.Cornsilk;
            this.label14.Location = new System.Drawing.Point(14, 15);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(79, 15);
            this.label14.TabIndex = 19;
            this.label14.Text = "Cant. Medias";
            // 
            // txtUsuario
            // 
            this.txtUsuario.Location = new System.Drawing.Point(665, 7);
            this.txtUsuario.Name = "txtUsuario";
            this.txtUsuario.ReadOnly = true;
            this.txtUsuario.Size = new System.Drawing.Size(140, 20);
            this.txtUsuario.TabIndex = 53;
            this.txtUsuario.TabStop = false;
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label13.ForeColor = System.Drawing.Color.Cornsilk;
            this.label13.Location = new System.Drawing.Point(609, 8);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(50, 15);
            this.label13.TabIndex = 52;
            this.label13.Text = "Usuario";
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.groupBox2.Controls.Add(this.radioCorte);
            this.groupBox2.Controls.Add(this.radioMediaRes);
            this.groupBox2.Controls.Add(this.radioIngresoStock);
            this.groupBox2.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox2.ForeColor = System.Drawing.Color.Cornsilk;
            this.groupBox2.Location = new System.Drawing.Point(668, 81);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(137, 59);
            this.groupBox2.TabIndex = 51;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Tipo de Compra";
            // 
            // radioCorte
            // 
            this.radioCorte.AutoSize = true;
            this.radioCorte.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.radioCorte.ForeColor = System.Drawing.Color.White;
            this.radioCorte.Location = new System.Drawing.Point(36, 36);
            this.radioCorte.Name = "radioCorte";
            this.radioCorte.Size = new System.Drawing.Size(60, 19);
            this.radioCorte.TabIndex = 100;
            this.radioCorte.Text = "Cortes";
            this.radioCorte.UseVisualStyleBackColor = true;
            // 
            // radioMediaRes
            // 
            this.radioMediaRes.AutoSize = true;
            this.radioMediaRes.Checked = true;
            this.radioMediaRes.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.radioMediaRes.ForeColor = System.Drawing.Color.White;
            this.radioMediaRes.Location = new System.Drawing.Point(36, 18);
            this.radioMediaRes.Name = "radioMediaRes";
            this.radioMediaRes.Size = new System.Drawing.Size(85, 19);
            this.radioMediaRes.TabIndex = 12;
            this.radioMediaRes.TabStop = true;
            this.radioMediaRes.Text = "Media Res";
            this.radioMediaRes.UseVisualStyleBackColor = true;
            // 
            // radioIngresoStock
            // 
            this.radioIngresoStock.AutoSize = true;
            this.radioIngresoStock.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.radioIngresoStock.ForeColor = System.Drawing.Color.White;
            this.radioIngresoStock.Location = new System.Drawing.Point(140, 55);
            this.radioIngresoStock.Name = "radioIngresoStock";
            this.radioIngresoStock.Size = new System.Drawing.Size(99, 19);
            this.radioIngresoStock.TabIndex = 0;
            this.radioIngresoStock.TabStop = true;
            this.radioIngresoStock.Text = "Ingreso Stock";
            this.radioIngresoStock.UseVisualStyleBackColor = true;
            this.radioIngresoStock.Visible = false;
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.groupBox1.BackColor = System.Drawing.Color.SteelBlue;
            this.groupBox1.Controls.Add(this.txtFechaCompra);
            this.groupBox1.Controls.Add(this.btnBuscarProv);
            this.groupBox1.Controls.Add(this.txtProveedor);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.txtNroRemito);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.comboSucursal);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox1.ForeColor = System.Drawing.Color.Cornsilk;
            this.groupBox1.Location = new System.Drawing.Point(19, 2);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(491, 78);
            this.groupBox1.TabIndex = 9;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Remito";
            // 
            // txtFechaCompra
            // 
            this.txtFechaCompra.CustomFormat = "dd/MM/yyyy HH:mm";
            this.txtFechaCompra.Enabled = false;
            this.txtFechaCompra.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.txtFechaCompra.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.txtFechaCompra.Location = new System.Drawing.Point(344, 22);
            this.txtFechaCompra.Name = "txtFechaCompra";
            this.txtFechaCompra.Size = new System.Drawing.Size(137, 21);
            this.txtFechaCompra.TabIndex = 54;
            this.txtFechaCompra.ValueChanged += new System.EventHandler(this.txtFechaCompra_ValueChanged);
            // 
            // btnBuscarProv
            // 
            this.btnBuscarProv.AccessibleDescription = "";
            this.btnBuscarProv.ForeColor = System.Drawing.Color.Black;
            this.btnBuscarProv.Image = ((System.Drawing.Image)(resources.GetObject("btnBuscarProv.Image")));
            this.btnBuscarProv.Location = new System.Drawing.Point(234, 47);
            this.btnBuscarProv.Name = "btnBuscarProv";
            this.btnBuscarProv.Size = new System.Drawing.Size(28, 23);
            this.btnBuscarProv.TabIndex = 0;
            this.btnBuscarProv.TabStop = false;
            this.btnBuscarProv.UseVisualStyleBackColor = true;
            this.btnBuscarProv.Visible = false;
            this.btnBuscarProv.Click += new System.EventHandler(this.btnBuscarProv_Click);
            // 
            // txtProveedor
            // 
            this.txtProveedor.Location = new System.Drawing.Point(81, 48);
            this.txtProveedor.Name = "txtProveedor";
            this.txtProveedor.ReadOnly = true;
            this.txtProveedor.Size = new System.Drawing.Size(147, 21);
            this.txtProveedor.TabIndex = 9;
            this.txtProveedor.TabStop = false;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ForeColor = System.Drawing.Color.Cornsilk;
            this.label1.Location = new System.Drawing.Point(12, 51);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(63, 15);
            this.label1.TabIndex = 8;
            this.label1.Text = "Proveedor";
            // 
            // txtNroRemito
            // 
            this.txtNroRemito.Location = new System.Drawing.Point(344, 48);
            this.txtNroRemito.Name = "txtNroRemito";
            this.txtNroRemito.ReadOnly = true;
            this.txtNroRemito.Size = new System.Drawing.Size(137, 21);
            this.txtNroRemito.TabIndex = 2;
            this.txtNroRemito.TabStop = false;
            this.txtNroRemito.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.ForeColor = System.Drawing.Color.Cornsilk;
            this.label2.Location = new System.Drawing.Point(268, 51);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(70, 15);
            this.label2.TabIndex = 6;
            this.label2.Text = "Nro Remito";
            // 
            // comboSucursal
            // 
            this.comboSucursal.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboSucursal.Enabled = false;
            this.comboSucursal.FormattingEnabled = true;
            this.comboSucursal.Location = new System.Drawing.Point(81, 19);
            this.comboSucursal.Name = "comboSucursal";
            this.comboSucursal.Size = new System.Drawing.Size(147, 23);
            this.comboSucursal.TabIndex = 6;
            this.comboSucursal.TabStop = false;
            this.comboSucursal.SelectedIndexChanged += new System.EventHandler(this.comboSucursal_SelectedIndexChanged);
            this.comboSucursal.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.TxtPruebaENTER_KeyPress);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.ForeColor = System.Drawing.Color.Cornsilk;
            this.label4.Location = new System.Drawing.Point(20, 23);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(55, 15);
            this.label4.TabIndex = 8;
            this.label4.Text = "Sucursal";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.ForeColor = System.Drawing.Color.Cornsilk;
            this.label3.Location = new System.Drawing.Point(297, 22);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(41, 15);
            this.label3.TabIndex = 4;
            this.label3.Text = "Fecha";
            // 
            // grupoMediaRes
            // 
            this.grupoMediaRes.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.grupoMediaRes.BackColor = System.Drawing.Color.SteelBlue;
            this.grupoMediaRes.Controls.Add(this.txtPrecioKg);
            this.grupoMediaRes.Controls.Add(this.label7);
            this.grupoMediaRes.Controls.Add(this.btnQuitar);
            this.grupoMediaRes.Controls.Add(this.btnAgregar);
            this.grupoMediaRes.Controls.Add(this.panelCorte);
            this.grupoMediaRes.Controls.Add(this.label6);
            this.grupoMediaRes.Controls.Add(this.txtNroTropa);
            this.grupoMediaRes.Controls.Add(this.txtKgMedia);
            this.grupoMediaRes.Controls.Add(this.label5);
            this.grupoMediaRes.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.grupoMediaRes.ForeColor = System.Drawing.Color.Cornsilk;
            this.grupoMediaRes.Location = new System.Drawing.Point(19, 81);
            this.grupoMediaRes.Name = "grupoMediaRes";
            this.grupoMediaRes.Size = new System.Drawing.Size(643, 59);
            this.grupoMediaRes.TabIndex = 0;
            this.grupoMediaRes.TabStop = false;
            this.grupoMediaRes.Text = "Media Res";
            // 
            // txtPrecioKg
            // 
            this.txtPrecioKg.Location = new System.Drawing.Point(443, 25);
            this.txtPrecioKg.Name = "txtPrecioKg";
            this.txtPrecioKg.Size = new System.Drawing.Size(74, 21);
            this.txtPrecioKg.TabIndex = 5;
            this.txtPrecioKg.TabStop = false;
            this.txtPrecioKg.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.txtPrecioKg.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.TxtPruebaENTER_KeyPress);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label7.ForeColor = System.Drawing.Color.Cornsilk;
            this.label7.Location = new System.Drawing.Point(378, 28);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(60, 15);
            this.label7.TabIndex = 15;
            this.label7.Text = "Precio Kg";
            // 
            // btnQuitar
            // 
            this.btnQuitar.ForeColor = System.Drawing.Color.Black;
            this.btnQuitar.Image = ((System.Drawing.Image)(resources.GetObject("btnQuitar.Image")));
            this.btnQuitar.Location = new System.Drawing.Point(585, 24);
            this.btnQuitar.Name = "btnQuitar";
            this.btnQuitar.Size = new System.Drawing.Size(49, 24);
            this.btnQuitar.TabIndex = 7;
            this.btnQuitar.TabStop = false;
            this.btnQuitar.UseVisualStyleBackColor = true;
            this.btnQuitar.Visible = false;
            this.btnQuitar.Click += new System.EventHandler(this.btnQuitar_Click);
            // 
            // btnAgregar
            // 
            this.btnAgregar.AccessibleDescription = "";
            this.btnAgregar.ForeColor = System.Drawing.Color.Black;
            this.btnAgregar.Image = ((System.Drawing.Image)(resources.GetObject("btnAgregar.Image")));
            this.btnAgregar.Location = new System.Drawing.Point(530, 24);
            this.btnAgregar.Name = "btnAgregar";
            this.btnAgregar.Size = new System.Drawing.Size(49, 24);
            this.btnAgregar.TabIndex = 7;
            this.btnAgregar.UseVisualStyleBackColor = true;
            this.btnAgregar.Visible = false;
            this.btnAgregar.Click += new System.EventHandler(this.btnAgregar_Click);
            this.btnAgregar.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.TxtPruebaENTER_KeyPress);
            // 
            // panelCorte
            // 
            this.panelCorte.Controls.Add(this.btnBuscaCorte);
            this.panelCorte.Controls.Add(this.txtCodigo);
            this.panelCorte.Controls.Add(this.txtCantKgs);
            this.panelCorte.Controls.Add(this.label18);
            this.panelCorte.Controls.Add(this.txtCorteNuevaCompra);
            this.panelCorte.Controls.Add(this.label16);
            this.panelCorte.Controls.Add(this.label17);
            this.panelCorte.Location = new System.Drawing.Point(1, 20);
            this.panelCorte.Name = "panelCorte";
            this.panelCorte.Size = new System.Drawing.Size(378, 34);
            this.panelCorte.TabIndex = 17;
            // 
            // btnBuscaCorte
            // 
            this.btnBuscaCorte.AccessibleDescription = "";
            this.btnBuscaCorte.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.btnBuscaCorte.ForeColor = System.Drawing.Color.Black;
            this.btnBuscaCorte.Image = ((System.Drawing.Image)(resources.GetObject("btnBuscaCorte.Image")));
            this.btnBuscaCorte.Location = new System.Drawing.Point(106, 6);
            this.btnBuscaCorte.Name = "btnBuscaCorte";
            this.btnBuscaCorte.Size = new System.Drawing.Size(28, 23);
            this.btnBuscaCorte.TabIndex = 3;
            this.btnBuscaCorte.TabStop = false;
            this.btnBuscaCorte.UseVisualStyleBackColor = true;
            this.btnBuscaCorte.Click += new System.EventHandler(this.btnBuscaCorte_Click);
            // 
            // txtCodigo
            // 
            this.txtCodigo.Location = new System.Drawing.Point(56, 7);
            this.txtCodigo.Name = "txtCodigo";
            this.txtCodigo.Size = new System.Drawing.Size(40, 21);
            this.txtCodigo.TabIndex = 0;
            this.txtCodigo.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.txtCodigo.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.TxtPruebaENTER_KeyPress);
            this.txtCodigo.TextChanged += new System.EventHandler(this.txtCodigo_TextChanged);
            // 
            // txtCantKgs
            // 
            this.txtCantKgs.Location = new System.Drawing.Point(313, 7);
            this.txtCantKgs.Name = "txtCantKgs";
            this.txtCantKgs.Size = new System.Drawing.Size(54, 21);
            this.txtCantKgs.TabIndex = 4;
            this.txtCantKgs.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.txtCantKgs.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.TxtPruebaENTER_KeyPress);
            // 
            // label18
            // 
            this.label18.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.label18.AutoSize = true;
            this.label18.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label18.ForeColor = System.Drawing.Color.Cornsilk;
            this.label18.Location = new System.Drawing.Point(140, 8);
            this.label18.Name = "label18";
            this.label18.Size = new System.Drawing.Size(36, 15);
            this.label18.TabIndex = 43;
            this.label18.Text = "Corte";
            // 
            // txtCorteNuevaCompra
            // 
            this.txtCorteNuevaCompra.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.txtCorteNuevaCompra.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
            this.txtCorteNuevaCompra.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.CustomSource;
            this.txtCorteNuevaCompra.Location = new System.Drawing.Point(182, 7);
            this.txtCorteNuevaCompra.Name = "txtCorteNuevaCompra";
            this.txtCorteNuevaCompra.Size = new System.Drawing.Size(95, 21);
            this.txtCorteNuevaCompra.TabIndex = 3;
            this.txtCorteNuevaCompra.TabStop = false;
            this.txtCorteNuevaCompra.TextChanged += new System.EventHandler(this.txtCorteNuevaCompra_TextChanged);
            // 
            // label16
            // 
            this.label16.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.label16.AutoSize = true;
            this.label16.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label16.ForeColor = System.Drawing.Color.Cornsilk;
            this.label16.Location = new System.Drawing.Point(8, 9);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(46, 15);
            this.label16.TabIndex = 45;
            this.label16.Text = "Código";
            // 
            // label17
            // 
            this.label17.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.label17.AutoSize = true;
            this.label17.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label17.ForeColor = System.Drawing.Color.Cornsilk;
            this.label17.Location = new System.Drawing.Point(283, 8);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(28, 15);
            this.label17.TabIndex = 44;
            this.label17.Text = "Kgs";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.ForeColor = System.Drawing.Color.Cornsilk;
            this.label6.Location = new System.Drawing.Point(49, 28);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(62, 15);
            this.label6.TabIndex = 11;
            this.label6.Text = "Nro Tropa";
            // 
            // txtNroTropa
            // 
            this.txtNroTropa.Location = new System.Drawing.Point(117, 25);
            this.txtNroTropa.Name = "txtNroTropa";
            this.txtNroTropa.Size = new System.Drawing.Size(77, 21);
            this.txtNroTropa.TabIndex = 3;
            this.txtNroTropa.TabStop = false;
            this.txtNroTropa.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.txtNroTropa.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.TxtPruebaENTER_KeyPress);
            // 
            // txtKgMedia
            // 
            this.txtKgMedia.Location = new System.Drawing.Point(284, 25);
            this.txtKgMedia.Name = "txtKgMedia";
            this.txtKgMedia.Size = new System.Drawing.Size(80, 21);
            this.txtKgMedia.TabIndex = 4;
            this.txtKgMedia.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.txtKgMedia.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.TxtPruebaENTER_KeyPress);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.ForeColor = System.Drawing.Color.Cornsilk;
            this.label5.Location = new System.Drawing.Point(208, 28);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(69, 15);
            this.label5.TabIndex = 6;
            this.label5.Text = "Kgs. Media";
            // 
            // label11
            // 
            this.label11.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(671, 300);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(76, 13);
            this.label11.TabIndex = 27;
            this.label11.Text = "observaciones";
            // 
            // grillaMediaRes
            // 
            this.grillaMediaRes.AllowUserToAddRows = false;
            this.grillaMediaRes.AllowUserToDeleteRows = false;
            this.grillaMediaRes.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.grillaMediaRes.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            dataGridViewCellStyle19.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle19.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle19.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle19.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle19.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle19.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle19.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.grillaMediaRes.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle19;
            this.grillaMediaRes.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.grillaMediaRes.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.idMedia,
            this.nroTropa,
            this.kgMedia,
            this.precioMedia,
            this.totalPs,
            this.idSucursalMedia,
            this.sucursalM});
            dataGridViewCellStyle23.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle23.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle23.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle23.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle23.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle23.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle23.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.grillaMediaRes.DefaultCellStyle = dataGridViewCellStyle23;
            this.grillaMediaRes.Location = new System.Drawing.Point(20, 189);
            this.grillaMediaRes.MultiSelect = false;
            this.grillaMediaRes.Name = "grillaMediaRes";
            this.grillaMediaRes.ReadOnly = true;
            dataGridViewCellStyle24.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle24.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle24.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle24.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle24.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle24.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle24.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.grillaMediaRes.RowHeadersDefaultCellStyle = dataGridViewCellStyle24;
            this.grillaMediaRes.RowHeadersVisible = false;
            this.grillaMediaRes.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.grillaMediaRes.Size = new System.Drawing.Size(642, 379);
            this.grillaMediaRes.TabIndex = 28;
            this.grillaMediaRes.TabStop = false;
            this.grillaMediaRes.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.grillaMediaRes_CellDoubleClick);
            // 
            // idMedia
            // 
            this.idMedia.DataPropertyName = "idMedia";
            this.idMedia.HeaderText = "ID Media";
            this.idMedia.Name = "idMedia";
            this.idMedia.ReadOnly = true;
            this.idMedia.Visible = false;
            // 
            // nroTropa
            // 
            this.nroTropa.DataPropertyName = "nroTropa";
            this.nroTropa.HeaderText = "Nro Tropa";
            this.nroTropa.Name = "nroTropa";
            this.nroTropa.ReadOnly = true;
            // 
            // kgMedia
            // 
            this.kgMedia.DataPropertyName = "kgMedia";
            dataGridViewCellStyle20.Format = "N3";
            dataGridViewCellStyle20.NullValue = null;
            this.kgMedia.DefaultCellStyle = dataGridViewCellStyle20;
            this.kgMedia.HeaderText = "Kgs. Media";
            this.kgMedia.Name = "kgMedia";
            this.kgMedia.ReadOnly = true;
            // 
            // precioMedia
            // 
            this.precioMedia.DataPropertyName = "precioMedia";
            dataGridViewCellStyle21.Format = "N2";
            dataGridViewCellStyle21.NullValue = null;
            this.precioMedia.DefaultCellStyle = dataGridViewCellStyle21;
            this.precioMedia.HeaderText = "Precio Kg. ";
            this.precioMedia.Name = "precioMedia";
            this.precioMedia.ReadOnly = true;
            // 
            // totalPs
            // 
            this.totalPs.DataPropertyName = "totalS";
            dataGridViewCellStyle22.Format = "N2";
            dataGridViewCellStyle22.NullValue = null;
            this.totalPs.DefaultCellStyle = dataGridViewCellStyle22;
            this.totalPs.HeaderText = "Total $";
            this.totalPs.Name = "totalPs";
            this.totalPs.ReadOnly = true;
            // 
            // idSucursalMedia
            // 
            this.idSucursalMedia.DataPropertyName = "idSucursal";
            this.idSucursalMedia.HeaderText = "ID Sucursal";
            this.idSucursalMedia.Name = "idSucursalMedia";
            this.idSucursalMedia.ReadOnly = true;
            this.idSucursalMedia.Visible = false;
            // 
            // sucursalM
            // 
            this.sucursalM.DataPropertyName = "sucursal";
            this.sucursalM.HeaderText = "Sucursal";
            this.sucursalM.Name = "sucursalM";
            this.sucursalM.ReadOnly = true;
            this.sucursalM.Visible = false;
            // 
            // barraControl
            // 
            this.barraControl.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.barraControl.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.modificar,
            this.quitarStock,
            this.cambiarPrecio,
            this.PorcentajesCorte,
            this.Reporte});
            this.barraControl.Location = new System.Drawing.Point(0, 0);
            this.barraControl.Name = "barraControl";
            this.barraControl.Padding = new System.Windows.Forms.Padding(10, 0, 0, 0);
            this.barraControl.Size = new System.Drawing.Size(817, 38);
            this.barraControl.TabIndex = 29;
            this.barraControl.Text = "toolStrip1";
            // 
            // modificar
            // 
            this.modificar.Font = new System.Drawing.Font("Segoe UI", 8F);
            this.modificar.Image = ((System.Drawing.Image)(resources.GetObject("modificar.Image")));
            this.modificar.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.modificar.Margin = new System.Windows.Forms.Padding(0, 2, 0, 1);
            this.modificar.Name = "modificar";
            this.modificar.Padding = new System.Windows.Forms.Padding(1);
            this.modificar.Size = new System.Drawing.Size(62, 35);
            this.modificar.Text = "Modificar";
            this.modificar.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.modificar.Click += new System.EventHandler(this.modificar_Click);
            // 
            // quitarStock
            // 
            this.quitarStock.Font = new System.Drawing.Font("Segoe UI", 8F);
            this.quitarStock.Image = global::Presentacion.Properties.Resources._16__Database_remove_;
            this.quitarStock.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.quitarStock.Name = "quitarStock";
            this.quitarStock.Size = new System.Drawing.Size(74, 35);
            this.quitarStock.Text = "Quitar Stock";
            this.quitarStock.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.quitarStock.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.quitarStock.ToolTipText = "Quitar Stock Real";
            this.quitarStock.Visible = false;
            this.quitarStock.Click += new System.EventHandler(this.quitarStock_Click);
            // 
            // cambiarPrecio
            // 
            this.cambiarPrecio.Enabled = false;
            this.cambiarPrecio.Image = ((System.Drawing.Image)(resources.GetObject("cambiarPrecio.Image")));
            this.cambiarPrecio.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.cambiarPrecio.Name = "cambiarPrecio";
            this.cambiarPrecio.Size = new System.Drawing.Size(36, 35);
            this.cambiarPrecio.Text = "$/Kg";
            this.cambiarPrecio.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.cambiarPrecio.Click += new System.EventHandler(this.cambiarPrecio_Click);
            // 
            // PorcentajesCorte
            // 
            this.PorcentajesCorte.Image = ((System.Drawing.Image)(resources.GetObject("PorcentajesCorte.Image")));
            this.PorcentajesCorte.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.PorcentajesCorte.Name = "PorcentajesCorte";
            this.PorcentajesCorte.Size = new System.Drawing.Size(58, 35);
            this.PorcentajesCorte.Text = "% Cortes";
            this.PorcentajesCorte.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.PorcentajesCorte.Click += new System.EventHandler(this.PorcentajesCorte_Click);
            // 
            // Reporte
            // 
            this.Reporte.Image = ((System.Drawing.Image)(resources.GetObject("Reporte.Image")));
            this.Reporte.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.Reporte.Name = "Reporte";
            this.Reporte.Size = new System.Drawing.Size(52, 35);
            this.Reporte.Text = "Reporte";
            this.Reporte.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.Reporte.Click += new System.EventHandler(this.Reporte_Click);
            // 
            // panelCompraAnulada
            // 
            this.panelCompraAnulada.BackColor = System.Drawing.Color.SteelBlue;
            this.panelCompraAnulada.Controls.Add(this.label10);
            this.panelCompraAnulada.Location = new System.Drawing.Point(0, 0);
            this.panelCompraAnulada.Name = "panelCompraAnulada";
            this.panelCompraAnulada.Size = new System.Drawing.Size(817, 41);
            this.panelCompraAnulada.TabIndex = 30;
            this.panelCompraAnulada.Visible = false;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Underline))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label10.ForeColor = System.Drawing.SystemColors.Info;
            this.label10.Location = new System.Drawing.Point(17, 11);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(132, 18);
            this.label10.TabIndex = 7;
            this.label10.Text = "Compra Anulada";
            // 
            // txtCantItems
            // 
            this.txtCantItems.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.txtCantItems.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtCantItems.Location = new System.Drawing.Point(671, 201);
            this.txtCantItems.Name = "txtCantItems";
            this.txtCantItems.ReadOnly = true;
            this.txtCantItems.Size = new System.Drawing.Size(134, 21);
            this.txtCantItems.TabIndex = 32;
            this.txtCantItems.TabStop = false;
            this.txtCantItems.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label12
            // 
            this.label12.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.label12.AutoSize = true;
            this.label12.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label12.Location = new System.Drawing.Point(668, 185);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(79, 15);
            this.label12.TabIndex = 31;
            this.label12.Text = "Cant. Items";
            // 
            // label15
            // 
            this.label15.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label15.AutoSize = true;
            this.label15.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label15.Location = new System.Drawing.Point(670, 448);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(59, 13);
            this.label15.TabIndex = 62;
            this.label15.Text = "Modificado";
            // 
            // txtActualizadoPor
            // 
            this.txtActualizadoPor.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtActualizadoPor.Location = new System.Drawing.Point(673, 484);
            this.txtActualizadoPor.Name = "txtActualizadoPor";
            this.txtActualizadoPor.ReadOnly = true;
            this.txtActualizadoPor.Size = new System.Drawing.Size(132, 21);
            this.txtActualizadoPor.TabIndex = 61;
            this.txtActualizadoPor.TabStop = false;
            // 
            // txtActualizado
            // 
            this.txtActualizado.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtActualizado.Location = new System.Drawing.Point(673, 463);
            this.txtActualizado.Name = "txtActualizado";
            this.txtActualizado.ReadOnly = true;
            this.txtActualizado.Size = new System.Drawing.Size(132, 21);
            this.txtActualizado.TabIndex = 60;
            this.txtActualizado.TabStop = false;
            // 
            // label19
            // 
            this.label19.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label19.AutoSize = true;
            this.label19.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label19.Location = new System.Drawing.Point(670, 386);
            this.label19.Name = "label19";
            this.label19.Size = new System.Drawing.Size(41, 13);
            this.label19.TabIndex = 59;
            this.label19.Text = "Creado";
            // 
            // txtCreadoPor
            // 
            this.txtCreadoPor.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtCreadoPor.Location = new System.Drawing.Point(671, 424);
            this.txtCreadoPor.Name = "txtCreadoPor";
            this.txtCreadoPor.ReadOnly = true;
            this.txtCreadoPor.Size = new System.Drawing.Size(134, 21);
            this.txtCreadoPor.TabIndex = 58;
            this.txtCreadoPor.TabStop = false;
            // 
            // txtCreado
            // 
            this.txtCreado.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtCreado.Location = new System.Drawing.Point(671, 403);
            this.txtCreado.Name = "txtCreado";
            this.txtCreado.ReadOnly = true;
            this.txtCreado.Size = new System.Drawing.Size(134, 21);
            this.txtCreado.TabIndex = 57;
            this.txtCreado.TabStop = false;
            // 
            // checkCtaCte
            // 
            this.checkCtaCte.Appearance = System.Windows.Forms.Appearance.Button;
            this.checkCtaCte.AutoSize = true;
            this.checkCtaCte.BackColor = System.Drawing.Color.LimeGreen;
            this.checkCtaCte.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.checkCtaCte.Checked = true;
            this.checkCtaCte.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkCtaCte.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.checkCtaCte.ForeColor = System.Drawing.Color.Cornsilk;
            this.checkCtaCte.Location = new System.Drawing.Point(510, 51);
            this.checkCtaCte.Name = "checkCtaCte";
            this.checkCtaCte.Size = new System.Drawing.Size(96, 30);
            this.checkCtaCte.TabIndex = 56;
            this.checkCtaCte.TabStop = false;
            this.checkCtaCte.Text = "A &Cta. Cte.";
            this.checkCtaCte.UseVisualStyleBackColor = false;
            this.checkCtaCte.CheckedChanged += new System.EventHandler(this.checkCtaCte_CheckedChanged);
            // 
            // formModificarCompra
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Bisque;
            this.ClientSize = new System.Drawing.Size(817, 581);
            this.Controls.Add(this.label15);
            this.Controls.Add(this.txtActualizadoPor);
            this.Controls.Add(this.txtActualizado);
            this.Controls.Add(this.label19);
            this.Controls.Add(this.txtCreadoPor);
            this.Controls.Add(this.txtCreado);
            this.Controls.Add(this.pnlBuscar);
            this.Controls.Add(this.txtCantItems);
            this.Controls.Add(this.label12);
            this.Controls.Add(this.label11);
            this.Controls.Add(this.txtObservaciones);
            this.Controls.Add(this.txtTotalS);
            this.Controls.Add(this.txtTotalKgs);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.btnAceptar);
            this.Controls.Add(this.btnCancelar);
            this.Controls.Add(this.barraControl);
            this.Controls.Add(this.panelCompraAnulada);
            this.Controls.Add(this.grillaCortePorCompra);
            this.Controls.Add(this.grillaMediaRes);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "formModificarCompra";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Información de la Compra";
            this.Load += new System.EventHandler(this.formModificarCompra_Load);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.formModificarCompra_FormClosing);
            ((System.ComponentModel.ISupportInitialize)(this.grillaCortePorCompra)).EndInit();
            this.pnlBuscar.ResumeLayout(false);
            this.pnlBuscar.PerformLayout();
            this.groupCantMedias.ResumeLayout(false);
            this.groupCantMedias.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.grupoMediaRes.ResumeLayout(false);
            this.grupoMediaRes.PerformLayout();
            this.panelCorte.ResumeLayout(false);
            this.panelCorte.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.grillaMediaRes)).EndInit();
            this.barraControl.ResumeLayout(false);
            this.barraControl.PerformLayout();
            this.panelCompraAnulada.ResumeLayout(false);
            this.panelCompraAnulada.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.DataGridView grillaCortePorCompra;
        private System.Windows.Forms.TextBox txtObservaciones;
        private System.Windows.Forms.TextBox txtTotalS;
        private System.Windows.Forms.TextBox txtTotalKgs;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label8;
        protected System.Windows.Forms.Button btnAceptar;
        protected System.Windows.Forms.Button btnCancelar;
        protected System.Windows.Forms.Panel pnlBuscar;
        private System.Windows.Forms.Panel panelCorte;
        protected internal System.Windows.Forms.Button btnBuscaCorte;
        protected System.Windows.Forms.TextBox txtCorteNuevaCompra;
        protected System.Windows.Forms.GroupBox grupoMediaRes;
        private System.Windows.Forms.TextBox txtPrecioKg;
        protected System.Windows.Forms.Label label7;
        private System.Windows.Forms.Button btnQuitar;
        private System.Windows.Forms.Button btnAgregar;
        protected System.Windows.Forms.Label label6;
        private System.Windows.Forms.ComboBox comboSucursal;
        private System.Windows.Forms.TextBox txtNroTropa;
        protected System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox txtKgMedia;
        protected System.Windows.Forms.Label label5;
        protected System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button btnBuscarProv;
        private System.Windows.Forms.TextBox txtProveedor;
        protected System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtNroRemito;
        protected System.Windows.Forms.Label label2;
        protected System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.DataGridView grillaMediaRes;
        protected internal System.Windows.Forms.ToolStrip barraControl;
        protected System.Windows.Forms.ToolStripButton modificar;
        private System.Windows.Forms.Panel panelCompraAnulada;
        protected System.Windows.Forms.Label label10;
        private System.Windows.Forms.ToolStripButton quitarStock;
        private System.Windows.Forms.TextBox txtCantKgs;
        private System.Windows.Forms.ToolStripButton PorcentajesCorte;
        private System.Windows.Forms.TextBox txtCantItems;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.ToolStripButton Reporte;
        private System.Windows.Forms.MaskedTextBox txtCodigo;
        protected System.Windows.Forms.Label label18;
        protected System.Windows.Forms.Label label16;
        protected System.Windows.Forms.Label label17;
        private System.Windows.Forms.ToolStripButton cambiarPrecio;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.RadioButton radioCorte;
        private System.Windows.Forms.RadioButton radioMediaRes;
        private System.Windows.Forms.RadioButton radioIngresoStock;
        private System.Windows.Forms.DataGridViewTextBoxColumn idCorte;
        private System.Windows.Forms.DataGridViewTextBoxColumn Codigo;
        private System.Windows.Forms.DataGridViewTextBoxColumn Corte;
        private System.Windows.Forms.DataGridViewTextBoxColumn cantKgs;
        private System.Windows.Forms.DataGridViewTextBoxColumn precioKgs;
        private System.Windows.Forms.DataGridViewTextBoxColumn totalS;
        private System.Windows.Forms.DataGridViewTextBoxColumn idSucursal;
        private System.Windows.Forms.DataGridViewTextBoxColumn sucursal;
        private System.Windows.Forms.DataGridViewTextBoxColumn idMedia;
        private System.Windows.Forms.DataGridViewTextBoxColumn nroTropa;
        private System.Windows.Forms.DataGridViewTextBoxColumn kgMedia;
        private System.Windows.Forms.DataGridViewTextBoxColumn precioMedia;
        private System.Windows.Forms.DataGridViewTextBoxColumn totalPs;
        private System.Windows.Forms.DataGridViewTextBoxColumn idSucursalMedia;
        private System.Windows.Forms.DataGridViewTextBoxColumn sucursalM;
        private System.Windows.Forms.TextBox txtUsuario;
        protected System.Windows.Forms.Label label13;
        private System.Windows.Forms.DateTimePicker txtFechaCompra;
        private System.Windows.Forms.GroupBox groupCantMedias;
        private System.Windows.Forms.TextBox txtCantMedias;
        protected System.Windows.Forms.Label label14;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.TextBox txtActualizadoPor;
        private System.Windows.Forms.TextBox txtActualizado;
        private System.Windows.Forms.Label label19;
        private System.Windows.Forms.TextBox txtCreadoPor;
        private System.Windows.Forms.TextBox txtCreado;
        private System.Windows.Forms.CheckBox checkCtaCte;
    }
}