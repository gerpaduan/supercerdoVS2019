namespace Presentacion.Caja
{
    partial class formUltimaVenta
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle8 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle9 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle5 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle6 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle7 = new System.Windows.Forms.DataGridViewCellStyle();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(formUltimaVenta));
            this.grillaLineasVenta = new System.Windows.Forms.DataGridView();
            this.idLineaVenta = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.idCorte = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.codigo = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.corte = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.cantKgs = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.precioKgs = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.totalS = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.estado = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.btnAnular = new System.Windows.Forms.DataGridViewButtonColumn();
            this.pnlBuscar = new System.Windows.Forms.Panel();
            this.checkPagoMixto = new System.Windows.Forms.CheckBox();
            this.txtEmail = new System.Windows.Forms.TextBox();
            this.label20 = new System.Windows.Forms.Label();
            this.txtCuit = new System.Windows.Forms.TextBox();
            this.label19 = new System.Windows.Forms.Label();
            this.comboTipoComprobante = new System.Windows.Forms.ComboBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.checkCtaCtePago = new System.Windows.Forms.CheckBox();
            this.checkTransf = new System.Windows.Forms.CheckBox();
            this.checkQr = new System.Windows.Forms.CheckBox();
            this.checkEfectivo = new System.Windows.Forms.CheckBox();
            this.checkCredito = new System.Windows.Forms.CheckBox();
            this.checkDebito = new System.Windows.Forms.CheckBox();
            this.panelInfoCtaCte = new System.Windows.Forms.Panel();
            this.label4 = new System.Windows.Forms.Label();
            this.checkCtaCte = new System.Windows.Forms.CheckBox();
            this.checkTicket = new System.Windows.Forms.CheckBox();
            this.txtFecVenta = new System.Windows.Forms.TextBox();
            this.txtVendedor = new System.Windows.Forms.TextBox();
            this.label17 = new System.Windows.Forms.Label();
            this.txtSucursal = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.txtNroTicket = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.btnBuscarCliente = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.txtCliente = new System.Windows.Forms.TextBox();
            this.label11 = new System.Windows.Forms.Label();
            this.txtObservaciones = new System.Windows.Forms.TextBox();
            this.txtCantItems = new System.Windows.Forms.TextBox();
            this.label16 = new System.Windows.Forms.Label();
            this.txtTotalS = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.btnAceptar = new System.Windows.Forms.Button();
            this.btnCancelar = new System.Windows.Forms.Button();
            this.barraControl = new System.Windows.Forms.ToolStrip();
            this.cambiarVendedor = new System.Windows.Forms.ToolStripButton();
            this.ImprimirTicket = new System.Windows.Forms.ToolStripButton();
            this.pdf = new System.Windows.Forms.ToolStripButton();
            this.facturaElec = new System.Windows.Forms.ToolStripButton();
            this.notaCredito = new System.Windows.Forms.ToolStripButton();
            this.txtTotalKgs = new System.Windows.Forms.TextBox();
            this.label12 = new System.Windows.Forms.Label();
            this.toolStripButton1 = new System.Windows.Forms.ToolStripButton();
            ((System.ComponentModel.ISupportInitialize)(this.grillaLineasVenta)).BeginInit();
            this.pnlBuscar.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.panelInfoCtaCte.SuspendLayout();
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
            this.grillaLineasVenta.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Microsoft Sans Serif", 13F);
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.grillaLineasVenta.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.grillaLineasVenta.ColumnHeadersHeight = 50;
            this.grillaLineasVenta.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.idLineaVenta,
            this.idCorte,
            this.codigo,
            this.corte,
            this.cantKgs,
            this.precioKgs,
            this.totalS,
            this.estado,
            this.btnAnular});
            dataGridViewCellStyle8.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle8.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle8.Font = new System.Drawing.Font("Microsoft Sans Serif", 13F);
            dataGridViewCellStyle8.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle8.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle8.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle8.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.grillaLineasVenta.DefaultCellStyle = dataGridViewCellStyle8;
            this.grillaLineasVenta.Location = new System.Drawing.Point(12, 184);
            this.grillaLineasVenta.MultiSelect = false;
            this.grillaLineasVenta.Name = "grillaLineasVenta";
            this.grillaLineasVenta.ReadOnly = true;
            dataGridViewCellStyle9.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle9.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle9.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle9.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle9.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle9.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle9.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.grillaLineasVenta.RowHeadersDefaultCellStyle = dataGridViewCellStyle9;
            this.grillaLineasVenta.RowHeadersVisible = false;
            this.grillaLineasVenta.RowHeadersWidth = 51;
            this.grillaLineasVenta.RowTemplate.Height = 40;
            this.grillaLineasVenta.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.grillaLineasVenta.Size = new System.Drawing.Size(671, 314);
            this.grillaLineasVenta.TabIndex = 41;
            this.grillaLineasVenta.TabStop = false;
            this.grillaLineasVenta.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.grillaLineasVenta_CellClick);
            // 
            // idLineaVenta
            // 
            this.idLineaVenta.DataPropertyName = "idLineaVenta";
            this.idLineaVenta.HeaderText = "idLineaVenta";
            this.idLineaVenta.MinimumWidth = 6;
            this.idLineaVenta.Name = "idLineaVenta";
            this.idLineaVenta.ReadOnly = true;
            this.idLineaVenta.Visible = false;
            // 
            // idCorte
            // 
            this.idCorte.DataPropertyName = "idCorte";
            this.idCorte.HeaderText = "idProd.";
            this.idCorte.MinimumWidth = 6;
            this.idCorte.Name = "idCorte";
            this.idCorte.ReadOnly = true;
            this.idCorte.Visible = false;
            // 
            // codigo
            // 
            this.codigo.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.ColumnHeader;
            this.codigo.DataPropertyName = "codigo";
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.BottomLeft;
            this.codigo.DefaultCellStyle = dataGridViewCellStyle2;
            this.codigo.HeaderText = "Código";
            this.codigo.MinimumWidth = 6;
            this.codigo.Name = "codigo";
            this.codigo.ReadOnly = true;
            this.codigo.Width = 92;
            // 
            // corte
            // 
            this.corte.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.corte.DataPropertyName = "corte";
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.BottomLeft;
            this.corte.DefaultCellStyle = dataGridViewCellStyle3;
            this.corte.HeaderText = "Prod.";
            this.corte.MinimumWidth = 6;
            this.corte.Name = "corte";
            this.corte.ReadOnly = true;
            // 
            // cantKgs
            // 
            this.cantKgs.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.ColumnHeader;
            this.cantKgs.DataPropertyName = "cantKgs";
            dataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.BottomRight;
            dataGridViewCellStyle4.Format = "N3";
            dataGridViewCellStyle4.NullValue = null;
            this.cantKgs.DefaultCellStyle = dataGridViewCellStyle4;
            this.cantKgs.HeaderText = "Cantidad";
            this.cantKgs.MinimumWidth = 6;
            this.cantKgs.Name = "cantKgs";
            this.cantKgs.ReadOnly = true;
            this.cantKgs.Width = 107;
            // 
            // precioKgs
            // 
            this.precioKgs.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.precioKgs.DataPropertyName = "precioKg";
            dataGridViewCellStyle5.Alignment = System.Windows.Forms.DataGridViewContentAlignment.BottomRight;
            dataGridViewCellStyle5.Format = "N2";
            dataGridViewCellStyle5.NullValue = null;
            this.precioKgs.DefaultCellStyle = dataGridViewCellStyle5;
            this.precioKgs.HeaderText = "Precio";
            this.precioKgs.MinimumWidth = 6;
            this.precioKgs.Name = "precioKgs";
            this.precioKgs.ReadOnly = true;
            this.precioKgs.Width = 86;
            // 
            // totalS
            // 
            this.totalS.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.totalS.DataPropertyName = "totalS";
            dataGridViewCellStyle6.Alignment = System.Windows.Forms.DataGridViewContentAlignment.BottomRight;
            dataGridViewCellStyle6.Format = "N2";
            dataGridViewCellStyle6.NullValue = null;
            this.totalS.DefaultCellStyle = dataGridViewCellStyle6;
            this.totalS.HeaderText = "Total";
            this.totalS.MinimumWidth = 6;
            this.totalS.Name = "totalS";
            this.totalS.ReadOnly = true;
            this.totalS.Width = 76;
            // 
            // estado
            // 
            this.estado.DataPropertyName = "estado";
            dataGridViewCellStyle7.ForeColor = System.Drawing.Color.Red;
            this.estado.DefaultCellStyle = dataGridViewCellStyle7;
            this.estado.HeaderText = "Estado";
            this.estado.MinimumWidth = 6;
            this.estado.Name = "estado";
            this.estado.ReadOnly = true;
            this.estado.Visible = false;
            // 
            // btnAnular
            // 
            this.btnAnular.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.ColumnHeader;
            this.btnAnular.HeaderText = "Anular";
            this.btnAnular.MinimumWidth = 6;
            this.btnAnular.Name = "btnAnular";
            this.btnAnular.ReadOnly = true;
            this.btnAnular.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.btnAnular.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            this.btnAnular.Text = "Anular";
            this.btnAnular.ToolTipText = "Anular";
            this.btnAnular.UseColumnTextForButtonValue = true;
            this.btnAnular.Width = 87;
            // 
            // pnlBuscar
            // 
            this.pnlBuscar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pnlBuscar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(43)))), ((int)(((byte)(77)))), ((int)(((byte)(129)))));
            this.pnlBuscar.Controls.Add(this.checkPagoMixto);
            this.pnlBuscar.Controls.Add(this.txtEmail);
            this.pnlBuscar.Controls.Add(this.label20);
            this.pnlBuscar.Controls.Add(this.txtCuit);
            this.pnlBuscar.Controls.Add(this.label19);
            this.pnlBuscar.Controls.Add(this.comboTipoComprobante);
            this.pnlBuscar.Controls.Add(this.groupBox1);
            this.pnlBuscar.Controls.Add(this.panelInfoCtaCte);
            this.pnlBuscar.Controls.Add(this.checkCtaCte);
            this.pnlBuscar.Controls.Add(this.checkTicket);
            this.pnlBuscar.Controls.Add(this.txtFecVenta);
            this.pnlBuscar.Controls.Add(this.txtVendedor);
            this.pnlBuscar.Controls.Add(this.label17);
            this.pnlBuscar.Controls.Add(this.txtSucursal);
            this.pnlBuscar.Controls.Add(this.label2);
            this.pnlBuscar.Controls.Add(this.txtNroTicket);
            this.pnlBuscar.Controls.Add(this.label8);
            this.pnlBuscar.Controls.Add(this.label3);
            this.pnlBuscar.Controls.Add(this.btnBuscarCliente);
            this.pnlBuscar.Controls.Add(this.label1);
            this.pnlBuscar.Controls.Add(this.txtCliente);
            this.pnlBuscar.Location = new System.Drawing.Point(0, 48);
            this.pnlBuscar.Name = "pnlBuscar";
            this.pnlBuscar.Size = new System.Drawing.Size(873, 130);
            this.pnlBuscar.TabIndex = 40;
            // 
            // checkPagoMixto
            // 
            this.checkPagoMixto.AutoSize = true;
            this.checkPagoMixto.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.checkPagoMixto.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.checkPagoMixto.ForeColor = System.Drawing.SystemColors.Window;
            this.checkPagoMixto.Location = new System.Drawing.Point(580, 97);
            this.checkPagoMixto.Margin = new System.Windows.Forms.Padding(2);
            this.checkPagoMixto.Name = "checkPagoMixto";
            this.checkPagoMixto.Size = new System.Drawing.Size(103, 24);
            this.checkPagoMixto.TabIndex = 70;
            this.checkPagoMixto.Text = "Pago mixto";
            this.checkPagoMixto.UseVisualStyleBackColor = true;
            this.checkPagoMixto.CheckedChanged += new System.EventHandler(this.checkPagoMixto_CheckedChanged);
            // 
            // txtEmail
            // 
            this.txtEmail.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.txtEmail.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtEmail.Location = new System.Drawing.Point(89, 88);
            this.txtEmail.Name = "txtEmail";
            this.txtEmail.ReadOnly = true;
            this.txtEmail.Size = new System.Drawing.Size(225, 26);
            this.txtEmail.TabIndex = 68;
            this.txtEmail.TabStop = false;
            this.txtEmail.TextChanged += new System.EventHandler(this.txtEmail_TextChanged);
            // 
            // label20
            // 
            this.label20.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label20.AutoSize = true;
            this.label20.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label20.ForeColor = System.Drawing.Color.Cornsilk;
            this.label20.Location = new System.Drawing.Point(32, 91);
            this.label20.Name = "label20";
            this.label20.Size = new System.Drawing.Size(51, 20);
            this.label20.TabIndex = 69;
            this.label20.Text = "e-mail";
            // 
            // txtCuit
            // 
            this.txtCuit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.txtCuit.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtCuit.Location = new System.Drawing.Point(89, 62);
            this.txtCuit.Name = "txtCuit";
            this.txtCuit.ReadOnly = true;
            this.txtCuit.Size = new System.Drawing.Size(225, 26);
            this.txtCuit.TabIndex = 66;
            this.txtCuit.TabStop = false;
            this.txtCuit.TextChanged += new System.EventHandler(this.txtCuit_TextChanged);
            // 
            // label19
            // 
            this.label19.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label19.AutoSize = true;
            this.label19.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label19.ForeColor = System.Drawing.Color.Cornsilk;
            this.label19.Location = new System.Drawing.Point(37, 65);
            this.label19.Name = "label19";
            this.label19.Size = new System.Drawing.Size(46, 20);
            this.label19.TabIndex = 67;
            this.label19.Text = "CUIT";
            // 
            // comboTipoComprobante
            // 
            this.comboTipoComprobante.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboTipoComprobante.DisplayMember = "R";
            this.comboTipoComprobante.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboTipoComprobante.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.comboTipoComprobante.Font = new System.Drawing.Font("Microsoft Sans Serif", 15F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.comboTipoComprobante.FormattingEnabled = true;
            this.comboTipoComprobante.Items.AddRange(new object[] {
            "X",
            "A",
            "B"});
            this.comboTipoComprobante.Location = new System.Drawing.Point(456, 3);
            this.comboTipoComprobante.Name = "comboTipoComprobante";
            this.comboTipoComprobante.Size = new System.Drawing.Size(43, 33);
            this.comboTipoComprobante.TabIndex = 65;
            this.comboTipoComprobante.TabStop = false;
            this.comboTipoComprobante.ValueMember = "R";
            this.comboTipoComprobante.SelectedIndexChanged += new System.EventHandler(this.comboTipoComprobante_SelectedIndexChanged);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.checkCtaCtePago);
            this.groupBox1.Controls.Add(this.checkTransf);
            this.groupBox1.Controls.Add(this.checkQr);
            this.groupBox1.Controls.Add(this.checkEfectivo);
            this.groupBox1.Controls.Add(this.checkCredito);
            this.groupBox1.Controls.Add(this.checkDebito);
            this.groupBox1.ForeColor = System.Drawing.Color.Cornsilk;
            this.groupBox1.Location = new System.Drawing.Point(351, 38);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(226, 83);
            this.groupBox1.TabIndex = 63;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Forma de Pago";
            // 
            // checkCtaCtePago
            // 
            this.checkCtaCtePago.Appearance = System.Windows.Forms.Appearance.Button;
            this.checkCtaCtePago.AutoSize = true;
            this.checkCtaCtePago.BackColor = System.Drawing.Color.LimeGreen;
            this.checkCtaCtePago.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.checkCtaCtePago.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.checkCtaCtePago.Location = new System.Drawing.Point(9, 46);
            this.checkCtaCtePago.Name = "checkCtaCtePago";
            this.checkCtaCtePago.Size = new System.Drawing.Size(77, 30);
            this.checkCtaCtePago.TabIndex = 65;
            this.checkCtaCtePago.TabStop = false;
            this.checkCtaCtePago.Text = "Cte.Cte ";
            this.checkCtaCtePago.UseVisualStyleBackColor = false;
            this.checkCtaCtePago.CheckedChanged += new System.EventHandler(this.checkCtaCtePago_CheckedChanged);
            // 
            // checkTransf
            // 
            this.checkTransf.Appearance = System.Windows.Forms.Appearance.Button;
            this.checkTransf.AutoSize = true;
            this.checkTransf.BackColor = System.Drawing.Color.LimeGreen;
            this.checkTransf.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.checkTransf.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.checkTransf.Location = new System.Drawing.Point(153, 46);
            this.checkTransf.Name = "checkTransf";
            this.checkTransf.Size = new System.Drawing.Size(68, 30);
            this.checkTransf.TabIndex = 67;
            this.checkTransf.TabStop = false;
            this.checkTransf.Text = "Transf ";
            this.checkTransf.UseVisualStyleBackColor = false;
            this.checkTransf.CheckedChanged += new System.EventHandler(this.checkTransf_CheckedChanged);
            // 
            // checkQr
            // 
            this.checkQr.Appearance = System.Windows.Forms.Appearance.Button;
            this.checkQr.AutoSize = true;
            this.checkQr.BackColor = System.Drawing.Color.LimeGreen;
            this.checkQr.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.checkQr.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.checkQr.Location = new System.Drawing.Point(86, 46);
            this.checkQr.Name = "checkQr";
            this.checkQr.Size = new System.Drawing.Size(67, 30);
            this.checkQr.TabIndex = 66;
            this.checkQr.TabStop = false;
            this.checkQr.Text = "QR      ";
            this.checkQr.UseVisualStyleBackColor = false;
            this.checkQr.CheckedChanged += new System.EventHandler(this.checkQr_CheckedChanged);
            // 
            // checkEfectivo
            // 
            this.checkEfectivo.Appearance = System.Windows.Forms.Appearance.Button;
            this.checkEfectivo.AutoSize = true;
            this.checkEfectivo.BackColor = System.Drawing.Color.LimeGreen;
            this.checkEfectivo.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.checkEfectivo.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.checkEfectivo.Location = new System.Drawing.Point(9, 15);
            this.checkEfectivo.Name = "checkEfectivo";
            this.checkEfectivo.Size = new System.Drawing.Size(76, 30);
            this.checkEfectivo.TabIndex = 55;
            this.checkEfectivo.TabStop = false;
            this.checkEfectivo.Text = "Efectivo";
            this.checkEfectivo.UseVisualStyleBackColor = false;
            this.checkEfectivo.CheckedChanged += new System.EventHandler(this.checkEfectivo_CheckedChanged);
            // 
            // checkCredito
            // 
            this.checkCredito.Appearance = System.Windows.Forms.Appearance.Button;
            this.checkCredito.AutoSize = true;
            this.checkCredito.BackColor = System.Drawing.Color.LimeGreen;
            this.checkCredito.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.checkCredito.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.checkCredito.Location = new System.Drawing.Point(153, 15);
            this.checkCredito.Name = "checkCredito";
            this.checkCredito.Size = new System.Drawing.Size(70, 30);
            this.checkCredito.TabIndex = 61;
            this.checkCredito.TabStop = false;
            this.checkCredito.Text = "Credito";
            this.checkCredito.UseVisualStyleBackColor = false;
            this.checkCredito.CheckedChanged += new System.EventHandler(this.checkCredito_CheckedChanged);
            // 
            // checkDebito
            // 
            this.checkDebito.Appearance = System.Windows.Forms.Appearance.Button;
            this.checkDebito.AutoSize = true;
            this.checkDebito.BackColor = System.Drawing.Color.LimeGreen;
            this.checkDebito.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.checkDebito.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.checkDebito.Location = new System.Drawing.Point(86, 15);
            this.checkDebito.Name = "checkDebito";
            this.checkDebito.Size = new System.Drawing.Size(66, 30);
            this.checkDebito.TabIndex = 60;
            this.checkDebito.TabStop = false;
            this.checkDebito.Text = "Debito";
            this.checkDebito.UseVisualStyleBackColor = false;
            this.checkDebito.CheckedChanged += new System.EventHandler(this.checkDebito_CheckedChanged);
            // 
            // panelInfoCtaCte
            // 
            this.panelInfoCtaCte.Controls.Add(this.label4);
            this.panelInfoCtaCte.Location = new System.Drawing.Point(679, 90);
            this.panelInfoCtaCte.Name = "panelInfoCtaCte";
            this.panelInfoCtaCte.Size = new System.Drawing.Size(191, 34);
            this.panelInfoCtaCte.TabIndex = 57;
            this.panelInfoCtaCte.Visible = false;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.label4.Location = new System.Drawing.Point(9, 2);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(176, 30);
            this.label4.TabIndex = 15;
            this.label4.Text = "No se pueden modificar\r\n el Cliente en Ventas a Cta. Cte.";
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
            this.checkCtaCte.Location = new System.Drawing.Point(351, 6);
            this.checkCtaCte.Name = "checkCtaCte";
            this.checkCtaCte.Size = new System.Drawing.Size(96, 30);
            this.checkCtaCte.TabIndex = 56;
            this.checkCtaCte.TabStop = false;
            this.checkCtaCte.Text = "A &Cta. Cte.";
            this.checkCtaCte.UseVisualStyleBackColor = false;
            this.checkCtaCte.Visible = false;
            this.checkCtaCte.CheckedChanged += new System.EventHandler(this.checkCtaCte_CheckedChanged);
            // 
            // checkTicket
            // 
            this.checkTicket.Appearance = System.Windows.Forms.Appearance.Button;
            this.checkTicket.AutoSize = true;
            this.checkTicket.BackColor = System.Drawing.Color.LimeGreen;
            this.checkTicket.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.checkTicket.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.checkTicket.Location = new System.Drawing.Point(514, 10);
            this.checkTicket.Name = "checkTicket";
            this.checkTicket.Size = new System.Drawing.Size(61, 30);
            this.checkTicket.TabIndex = 47;
            this.checkTicket.TabStop = false;
            this.checkTicket.Text = "&Ticket";
            this.checkTicket.UseVisualStyleBackColor = false;
            this.checkTicket.Visible = false;
            this.checkTicket.CheckedChanged += new System.EventHandler(this.checkTicket_CheckedChanged);
            // 
            // txtFecVenta
            // 
            this.txtFecVenta.BackColor = System.Drawing.SystemColors.ScrollBar;
            this.txtFecVenta.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold);
            this.txtFecVenta.Location = new System.Drawing.Point(669, 56);
            this.txtFecVenta.Name = "txtFecVenta";
            this.txtFecVenta.ReadOnly = true;
            this.txtFecVenta.Size = new System.Drawing.Size(191, 26);
            this.txtFecVenta.TabIndex = 43;
            this.txtFecVenta.TabStop = false;
            // 
            // txtVendedor
            // 
            this.txtVendedor.BackColor = System.Drawing.SystemColors.ScrollBar;
            this.txtVendedor.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtVendedor.Location = new System.Drawing.Point(89, 10);
            this.txtVendedor.Name = "txtVendedor";
            this.txtVendedor.ReadOnly = true;
            this.txtVendedor.Size = new System.Drawing.Size(185, 26);
            this.txtVendedor.TabIndex = 42;
            this.txtVendedor.TabStop = false;
            // 
            // label17
            // 
            this.label17.AutoSize = true;
            this.label17.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label17.ForeColor = System.Drawing.Color.Cornsilk;
            this.label17.Location = new System.Drawing.Point(4, 13);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(79, 20);
            this.label17.TabIndex = 41;
            this.label17.Text = "Vendedor";
            // 
            // txtSucursal
            // 
            this.txtSucursal.BackColor = System.Drawing.SystemColors.ScrollBar;
            this.txtSucursal.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtSucursal.Location = new System.Drawing.Point(669, 4);
            this.txtSucursal.Name = "txtSucursal";
            this.txtSucursal.ReadOnly = true;
            this.txtSucursal.Size = new System.Drawing.Size(191, 26);
            this.txtSucursal.TabIndex = 26;
            this.txtSucursal.TabStop = false;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.ForeColor = System.Drawing.Color.Cornsilk;
            this.label2.Location = new System.Drawing.Point(592, 7);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(71, 20);
            this.label2.TabIndex = 25;
            this.label2.Text = "Sucursal";
            // 
            // txtNroTicket
            // 
            this.txtNroTicket.BackColor = System.Drawing.SystemColors.ScrollBar;
            this.txtNroTicket.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtNroTicket.Location = new System.Drawing.Point(669, 30);
            this.txtNroTicket.Name = "txtNroTicket";
            this.txtNroTicket.ReadOnly = true;
            this.txtNroTicket.Size = new System.Drawing.Size(191, 26);
            this.txtNroTicket.TabIndex = 23;
            this.txtNroTicket.TabStop = false;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label8.ForeColor = System.Drawing.Color.Cornsilk;
            this.label8.Location = new System.Drawing.Point(583, 35);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(80, 20);
            this.label8.TabIndex = 24;
            this.label8.Text = "Nro Ticket";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.ForeColor = System.Drawing.Color.Cornsilk;
            this.label3.Location = new System.Drawing.Point(609, 59);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(54, 20);
            this.label3.TabIndex = 14;
            this.label3.Text = "Fecha";
            // 
            // btnBuscarCliente
            // 
            this.btnBuscarCliente.AccessibleDescription = "";
            this.btnBuscarCliente.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnBuscarCliente.ForeColor = System.Drawing.Color.Black;
            this.btnBuscarCliente.Image = ((System.Drawing.Image)(resources.GetObject("btnBuscarCliente.Image")));
            this.btnBuscarCliente.Location = new System.Drawing.Point(315, 36);
            this.btnBuscarCliente.Name = "btnBuscarCliente";
            this.btnBuscarCliente.Size = new System.Drawing.Size(34, 29);
            this.btnBuscarCliente.TabIndex = 12;
            this.btnBuscarCliente.TabStop = false;
            this.btnBuscarCliente.UseVisualStyleBackColor = true;
            this.btnBuscarCliente.Click += new System.EventHandler(this.btnBuscarCliente_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ForeColor = System.Drawing.Color.Cornsilk;
            this.label1.Location = new System.Drawing.Point(25, 39);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(58, 20);
            this.label1.TabIndex = 16;
            this.label1.Text = "Cliente";
            // 
            // txtCliente
            // 
            this.txtCliente.BackColor = System.Drawing.SystemColors.ScrollBar;
            this.txtCliente.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtCliente.Location = new System.Drawing.Point(89, 36);
            this.txtCliente.Name = "txtCliente";
            this.txtCliente.ReadOnly = true;
            this.txtCliente.Size = new System.Drawing.Size(225, 26);
            this.txtCliente.TabIndex = 18;
            this.txtCliente.TabStop = false;
            // 
            // label11
            // 
            this.label11.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label11.AutoSize = true;
            this.label11.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label11.Location = new System.Drawing.Point(8, 503);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(133, 16);
            this.label11.TabIndex = 43;
            this.label11.Text = "Observaciones (F11)";
            // 
            // txtObservaciones
            // 
            this.txtObservaciones.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.txtObservaciones.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtObservaciones.Location = new System.Drawing.Point(12, 522);
            this.txtObservaciones.Multiline = true;
            this.txtObservaciones.Name = "txtObservaciones";
            this.txtObservaciones.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtObservaciones.Size = new System.Drawing.Size(848, 42);
            this.txtObservaciones.TabIndex = 42;
            this.txtObservaciones.TabStop = false;
            this.txtObservaciones.TextChanged += new System.EventHandler(this.txtObservaciones_TextChanged);
            // 
            // txtCantItems
            // 
            this.txtCantItems.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.txtCantItems.BackColor = System.Drawing.SystemColors.ScrollBar;
            this.txtCantItems.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtCantItems.Location = new System.Drawing.Point(691, 208);
            this.txtCantItems.Name = "txtCantItems";
            this.txtCantItems.ReadOnly = true;
            this.txtCantItems.Size = new System.Drawing.Size(169, 24);
            this.txtCantItems.TabIndex = 47;
            this.txtCantItems.TabStop = false;
            this.txtCantItems.Text = "0";
            this.txtCantItems.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label16
            // 
            this.label16.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.label16.AutoSize = true;
            this.label16.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label16.Location = new System.Drawing.Point(688, 187);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(94, 18);
            this.label16.TabIndex = 46;
            this.label16.Text = "Cant. Items";
            // 
            // txtTotalS
            // 
            this.txtTotalS.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.txtTotalS.BackColor = System.Drawing.SystemColors.ScrollBar;
            this.txtTotalS.Font = new System.Drawing.Font("Microsoft Sans Serif", 21.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtTotalS.ForeColor = System.Drawing.Color.DarkRed;
            this.txtTotalS.Location = new System.Drawing.Point(691, 311);
            this.txtTotalS.Name = "txtTotalS";
            this.txtTotalS.ReadOnly = true;
            this.txtTotalS.Size = new System.Drawing.Size(169, 40);
            this.txtTotalS.TabIndex = 45;
            this.txtTotalS.TabStop = false;
            this.txtTotalS.Text = "000,00";
            this.txtTotalS.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label9
            // 
            this.label9.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.label9.AutoSize = true;
            this.label9.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label9.Location = new System.Drawing.Point(689, 283);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(152, 25);
            this.label9.TabIndex = 44;
            this.label9.Text = "Total a pagar";
            // 
            // btnAceptar
            // 
            this.btnAceptar.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnAceptar.BackColor = System.Drawing.SystemColors.HotTrack;
            this.btnAceptar.FlatAppearance.BorderColor = System.Drawing.Color.White;
            this.btnAceptar.FlatAppearance.BorderSize = 3;
            this.btnAceptar.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnAceptar.ForeColor = System.Drawing.Color.AliceBlue;
            this.btnAceptar.Location = new System.Drawing.Point(691, 426);
            this.btnAceptar.Name = "btnAceptar";
            this.btnAceptar.Size = new System.Drawing.Size(166, 39);
            this.btnAceptar.TabIndex = 49;
            this.btnAceptar.Text = "&Guardar";
            this.btnAceptar.UseVisualStyleBackColor = false;
            this.btnAceptar.Click += new System.EventHandler(this.btnAceptar_Click);
            // 
            // btnCancelar
            // 
            this.btnCancelar.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancelar.BackColor = System.Drawing.SystemColors.AppWorkspace;
            this.btnCancelar.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancelar.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnCancelar.Location = new System.Drawing.Point(691, 471);
            this.btnCancelar.Name = "btnCancelar";
            this.btnCancelar.Size = new System.Drawing.Size(166, 27);
            this.btnCancelar.TabIndex = 48;
            this.btnCancelar.TabStop = false;
            this.btnCancelar.Text = "&Salir";
            this.btnCancelar.UseVisualStyleBackColor = false;
            this.btnCancelar.Click += new System.EventHandler(this.btnCancelar_Click);
            // 
            // barraControl
            // 
            this.barraControl.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.barraControl.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.barraControl.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.cambiarVendedor,
            this.ImprimirTicket,
            this.pdf,
            this.facturaElec,
            this.notaCredito});
            this.barraControl.Location = new System.Drawing.Point(0, 0);
            this.barraControl.Name = "barraControl";
            this.barraControl.Padding = new System.Windows.Forms.Padding(10, 0, 0, 0);
            this.barraControl.Size = new System.Drawing.Size(872, 49);
            this.barraControl.TabIndex = 50;
            this.barraControl.Text = "toolStrip1";
            // 
            // cambiarVendedor
            // 
            this.cambiarVendedor.Image = ((System.Drawing.Image)(resources.GetObject("cambiarVendedor.Image")));
            this.cambiarVendedor.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.cambiarVendedor.Margin = new System.Windows.Forms.Padding(0, 2, 0, 1);
            this.cambiarVendedor.Name = "cambiarVendedor";
            this.cambiarVendedor.Padding = new System.Windows.Forms.Padding(1);
            this.cambiarVendedor.Size = new System.Drawing.Size(111, 46);
            this.cambiarVendedor.Text = "Cambiar vendedor";
            this.cambiarVendedor.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.cambiarVendedor.Click += new System.EventHandler(this.cambiarVendedor_Click);
            // 
            // ImprimirTicket
            // 
            this.ImprimirTicket.Image = ((System.Drawing.Image)(resources.GetObject("ImprimirTicket.Image")));
            this.ImprimirTicket.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.ImprimirTicket.Margin = new System.Windows.Forms.Padding(0, 2, 0, 1);
            this.ImprimirTicket.Name = "ImprimirTicket";
            this.ImprimirTicket.Padding = new System.Windows.Forms.Padding(1, 1, 1, 6);
            this.ImprimirTicket.Size = new System.Drawing.Size(44, 46);
            this.ImprimirTicket.Text = "Ticket";
            this.ImprimirTicket.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.ImprimirTicket.Click += new System.EventHandler(this.ImprimirTicket_Click);
            // 
            // pdf
            // 
            this.pdf.Image = ((System.Drawing.Image)(resources.GetObject("pdf.Image")));
            this.pdf.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.pdf.Name = "pdf";
            this.pdf.Size = new System.Drawing.Size(44, 46);
            this.pdf.Text = "  PDF  ";
            this.pdf.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
            this.pdf.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.pdf.Click += new System.EventHandler(this.pdf_Click);
            // 
            // facturaElec
            // 
            this.facturaElec.Image = ((System.Drawing.Image)(resources.GetObject("facturaElec.Image")));
            this.facturaElec.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.facturaElec.Name = "facturaElec";
            this.facturaElec.Size = new System.Drawing.Size(50, 46);
            this.facturaElec.Text = "Factura";
            this.facturaElec.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
            this.facturaElec.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.facturaElec.Click += new System.EventHandler(this.facturaElectronica_Click);
            // 
            // notaCredito
            // 
            this.notaCredito.Image = ((System.Drawing.Image)(resources.GetObject("notaCredito.Image")));
            this.notaCredito.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.notaCredito.Name = "notaCredito";
            this.notaCredito.Size = new System.Drawing.Size(68, 46);
            this.notaCredito.Text = "Nota Créd.";
            this.notaCredito.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
            this.notaCredito.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.notaCredito.Click += new System.EventHandler(this.notaCredito_Click);
            // 
            // txtTotalKgs
            // 
            this.txtTotalKgs.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.txtTotalKgs.BackColor = System.Drawing.SystemColors.ScrollBar;
            this.txtTotalKgs.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtTotalKgs.Location = new System.Drawing.Point(691, 256);
            this.txtTotalKgs.Name = "txtTotalKgs";
            this.txtTotalKgs.ReadOnly = true;
            this.txtTotalKgs.Size = new System.Drawing.Size(169, 24);
            this.txtTotalKgs.TabIndex = 55;
            this.txtTotalKgs.TabStop = false;
            this.txtTotalKgs.Text = "0.000";
            this.txtTotalKgs.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label12
            // 
            this.label12.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label12.AutoSize = true;
            this.label12.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label12.Location = new System.Drawing.Point(688, 235);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(87, 18);
            this.label12.TabIndex = 54;
            this.label12.Text = "Cant. Kgs.";
            // 
            // toolStripButton1
            // 
            this.toolStripButton1.Name = "toolStripButton1";
            this.toolStripButton1.Size = new System.Drawing.Size(23, 23);
            // 
            // formUltimaVenta
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(872, 576);
            this.Controls.Add(this.txtTotalKgs);
            this.Controls.Add(this.label12);
            this.Controls.Add(this.barraControl);
            this.Controls.Add(this.btnAceptar);
            this.Controls.Add(this.btnCancelar);
            this.Controls.Add(this.txtCantItems);
            this.Controls.Add(this.label16);
            this.Controls.Add(this.txtTotalS);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.label11);
            this.Controls.Add(this.txtObservaciones);
            this.Controls.Add(this.grillaLineasVenta);
            this.Controls.Add(this.pnlBuscar);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "formUltimaVenta";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Detalle Venta";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.formUltimaVenta_FormClosing);
            this.Load += new System.EventHandler(this.formUltimaVenta_Load);
            ((System.ComponentModel.ISupportInitialize)(this.grillaLineasVenta)).EndInit();
            this.pnlBuscar.ResumeLayout(false);
            this.pnlBuscar.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.panelInfoCtaCte.ResumeLayout(false);
            this.panelInfoCtaCte.PerformLayout();
            this.barraControl.ResumeLayout(false);
            this.barraControl.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.DataGridView grillaLineasVenta;
        protected System.Windows.Forms.Panel pnlBuscar;
        private System.Windows.Forms.TextBox txtFecVenta;
        public System.Windows.Forms.TextBox txtVendedor;
        protected System.Windows.Forms.Label label17;
        private System.Windows.Forms.TextBox txtSucursal;
        protected System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtNroTicket;
        protected System.Windows.Forms.Label label8;
        protected System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button btnBuscarCliente;
        protected System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtCliente;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.TextBox txtObservaciones;
        private System.Windows.Forms.TextBox txtCantItems;
        private System.Windows.Forms.Label label16;
        private System.Windows.Forms.TextBox txtTotalS;
        private System.Windows.Forms.Label label9;
        protected System.Windows.Forms.Button btnAceptar;
        protected System.Windows.Forms.Button btnCancelar;
        protected internal System.Windows.Forms.ToolStrip barraControl;
        protected System.Windows.Forms.ToolStripButton cambiarVendedor;
        protected System.Windows.Forms.ToolStripButton ImprimirTicket;
        private System.Windows.Forms.DataGridViewTextBoxColumn idLineaVenta;
        private System.Windows.Forms.DataGridViewTextBoxColumn idCorte;
        private System.Windows.Forms.DataGridViewTextBoxColumn codigo;
        private System.Windows.Forms.DataGridViewTextBoxColumn corte;
        private System.Windows.Forms.DataGridViewTextBoxColumn cantKgs;
        private System.Windows.Forms.DataGridViewTextBoxColumn precioKgs;
        private System.Windows.Forms.DataGridViewTextBoxColumn totalS;
        private System.Windows.Forms.DataGridViewTextBoxColumn estado;
        private System.Windows.Forms.DataGridViewButtonColumn btnAnular;
        private System.Windows.Forms.CheckBox checkTicket;
        private System.Windows.Forms.TextBox txtTotalKgs;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.CheckBox checkCtaCte;
        private System.Windows.Forms.Panel panelInfoCtaCte;
        protected System.Windows.Forms.Label label4;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.CheckBox checkEfectivo;
        private System.Windows.Forms.CheckBox checkCredito;
        private System.Windows.Forms.CheckBox checkDebito;
        private System.Windows.Forms.ComboBox comboTipoComprobante;
        private System.Windows.Forms.TextBox txtEmail;
        protected System.Windows.Forms.Label label20;
        private System.Windows.Forms.TextBox txtCuit;
        protected System.Windows.Forms.Label label19;
        private System.Windows.Forms.ToolStripButton facturaElec;
        private System.Windows.Forms.CheckBox checkCtaCtePago;
        private System.Windows.Forms.CheckBox checkTransf;
        private System.Windows.Forms.CheckBox checkQr;
        private System.Windows.Forms.CheckBox checkPagoMixto;
        private System.Windows.Forms.ToolStripButton notaCredito;
        private System.Windows.Forms.ToolStripButton pdf;
        private System.Windows.Forms.ToolStripButton toolStripButton1;
    }
}