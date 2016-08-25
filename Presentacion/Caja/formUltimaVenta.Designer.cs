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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle10 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle17 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle18 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle11 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle12 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle13 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle14 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle15 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle16 = new System.Windows.Forms.DataGridViewCellStyle();
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
            this.anularVenta = new System.Windows.Forms.ToolStripButton();
            this.ImprimirTicket = new System.Windows.Forms.ToolStripButton();
            this.txtTotalKgs = new System.Windows.Forms.TextBox();
            this.label12 = new System.Windows.Forms.Label();
            this.panelInfoCtaCte = new System.Windows.Forms.Panel();
            this.label4 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.grillaLineasVenta)).BeginInit();
            this.pnlBuscar.SuspendLayout();
            this.barraControl.SuspendLayout();
            this.panelInfoCtaCte.SuspendLayout();
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
            dataGridViewCellStyle10.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle10.Font = new System.Drawing.Font("Microsoft Sans Serif", 13F);
            dataGridViewCellStyle10.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle10.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle10.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle10.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.grillaLineasVenta.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle10;
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
            dataGridViewCellStyle17.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle17.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle17.Font = new System.Drawing.Font("Microsoft Sans Serif", 13F);
            dataGridViewCellStyle17.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle17.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle17.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle17.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.grillaLineasVenta.DefaultCellStyle = dataGridViewCellStyle17;
            this.grillaLineasVenta.Location = new System.Drawing.Point(12, 163);
            this.grillaLineasVenta.MultiSelect = false;
            this.grillaLineasVenta.Name = "grillaLineasVenta";
            this.grillaLineasVenta.ReadOnly = true;
            dataGridViewCellStyle18.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle18.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle18.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle18.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle18.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle18.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle18.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.grillaLineasVenta.RowHeadersDefaultCellStyle = dataGridViewCellStyle18;
            this.grillaLineasVenta.RowHeadersVisible = false;
            this.grillaLineasVenta.RowTemplate.Height = 40;
            this.grillaLineasVenta.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.grillaLineasVenta.Size = new System.Drawing.Size(692, 335);
            this.grillaLineasVenta.TabIndex = 41;
            this.grillaLineasVenta.TabStop = false;
            this.grillaLineasVenta.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.grillaLineasVenta_CellClick);
            // 
            // idLineaVenta
            // 
            this.idLineaVenta.DataPropertyName = "idLineaVenta";
            this.idLineaVenta.HeaderText = "idLineaVenta";
            this.idLineaVenta.Name = "idLineaVenta";
            this.idLineaVenta.ReadOnly = true;
            this.idLineaVenta.Visible = false;
            // 
            // idCorte
            // 
            this.idCorte.DataPropertyName = "idCorte";
            this.idCorte.HeaderText = "idCorte";
            this.idCorte.Name = "idCorte";
            this.idCorte.ReadOnly = true;
            this.idCorte.Visible = false;
            // 
            // codigo
            // 
            this.codigo.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.ColumnHeader;
            this.codigo.DataPropertyName = "codigo";
            dataGridViewCellStyle11.Alignment = System.Windows.Forms.DataGridViewContentAlignment.BottomLeft;
            this.codigo.DefaultCellStyle = dataGridViewCellStyle11;
            this.codigo.HeaderText = "Código";
            this.codigo.Name = "codigo";
            this.codigo.ReadOnly = true;
            this.codigo.Width = 92;
            // 
            // corte
            // 
            this.corte.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.corte.DataPropertyName = "corte";
            dataGridViewCellStyle12.Alignment = System.Windows.Forms.DataGridViewContentAlignment.BottomLeft;
            this.corte.DefaultCellStyle = dataGridViewCellStyle12;
            this.corte.HeaderText = "Corte";
            this.corte.Name = "corte";
            this.corte.ReadOnly = true;
            // 
            // cantKgs
            // 
            this.cantKgs.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.ColumnHeader;
            this.cantKgs.DataPropertyName = "cantKgs";
            dataGridViewCellStyle13.Alignment = System.Windows.Forms.DataGridViewContentAlignment.BottomRight;
            dataGridViewCellStyle13.Format = "N3";
            dataGridViewCellStyle13.NullValue = null;
            this.cantKgs.DefaultCellStyle = dataGridViewCellStyle13;
            this.cantKgs.HeaderText = "Cantidad";
            this.cantKgs.Name = "cantKgs";
            this.cantKgs.ReadOnly = true;
            this.cantKgs.Width = 107;
            // 
            // precioKgs
            // 
            this.precioKgs.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.precioKgs.DataPropertyName = "precioKg";
            dataGridViewCellStyle14.Alignment = System.Windows.Forms.DataGridViewContentAlignment.BottomRight;
            dataGridViewCellStyle14.Format = "N2";
            dataGridViewCellStyle14.NullValue = null;
            this.precioKgs.DefaultCellStyle = dataGridViewCellStyle14;
            this.precioKgs.HeaderText = "Precio";
            this.precioKgs.Name = "precioKgs";
            this.precioKgs.ReadOnly = true;
            this.precioKgs.Width = 86;
            // 
            // totalS
            // 
            this.totalS.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.totalS.DataPropertyName = "totalS";
            dataGridViewCellStyle15.Alignment = System.Windows.Forms.DataGridViewContentAlignment.BottomRight;
            dataGridViewCellStyle15.Format = "N2";
            dataGridViewCellStyle15.NullValue = null;
            this.totalS.DefaultCellStyle = dataGridViewCellStyle15;
            this.totalS.HeaderText = "Total";
            this.totalS.Name = "totalS";
            this.totalS.ReadOnly = true;
            this.totalS.Width = 76;
            // 
            // estado
            // 
            this.estado.DataPropertyName = "estado";
            dataGridViewCellStyle16.ForeColor = System.Drawing.Color.Red;
            this.estado.DefaultCellStyle = dataGridViewCellStyle16;
            this.estado.HeaderText = "Estado";
            this.estado.Name = "estado";
            this.estado.ReadOnly = true;
            this.estado.Visible = false;
            // 
            // btnAnular
            // 
            this.btnAnular.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.ColumnHeader;
            this.btnAnular.HeaderText = "Anular";
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
            this.pnlBuscar.BackColor = System.Drawing.Color.SteelBlue;
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
            this.pnlBuscar.Size = new System.Drawing.Size(873, 110);
            this.pnlBuscar.TabIndex = 40;
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
            this.checkCtaCte.Location = new System.Drawing.Point(372, 75);
            this.checkCtaCte.Name = "checkCtaCte";
            this.checkCtaCte.Size = new System.Drawing.Size(96, 30);
            this.checkCtaCte.TabIndex = 56;
            this.checkCtaCte.TabStop = false;
            this.checkCtaCte.Text = "A &Cta. Cte.";
            this.checkCtaCte.UseVisualStyleBackColor = false;
            this.checkCtaCte.CheckedChanged += new System.EventHandler(this.checkCtaCte_CheckedChanged);
            // 
            // checkTicket
            // 
            this.checkTicket.Appearance = System.Windows.Forms.Appearance.Button;
            this.checkTicket.AutoSize = true;
            this.checkTicket.BackColor = System.Drawing.Color.LimeGreen;
            this.checkTicket.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.checkTicket.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.checkTicket.Location = new System.Drawing.Point(474, 75);
            this.checkTicket.Name = "checkTicket";
            this.checkTicket.Size = new System.Drawing.Size(61, 30);
            this.checkTicket.TabIndex = 47;
            this.checkTicket.TabStop = false;
            this.checkTicket.Text = "&Ticket";
            this.checkTicket.UseVisualStyleBackColor = false;
            this.checkTicket.CheckedChanged += new System.EventHandler(this.checkTicket_CheckedChanged);
            // 
            // txtFecVenta
            // 
            this.txtFecVenta.BackColor = System.Drawing.SystemColors.ScrollBar;
            this.txtFecVenta.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold);
            this.txtFecVenta.Location = new System.Drawing.Point(634, 45);
            this.txtFecVenta.Name = "txtFecVenta";
            this.txtFecVenta.ReadOnly = true;
            this.txtFecVenta.Size = new System.Drawing.Size(226, 26);
            this.txtFecVenta.TabIndex = 43;
            this.txtFecVenta.TabStop = false;
            // 
            // txtVendedor
            // 
            this.txtVendedor.BackColor = System.Drawing.SystemColors.ScrollBar;
            this.txtVendedor.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtVendedor.Location = new System.Drawing.Point(89, 45);
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
            this.label17.Location = new System.Drawing.Point(4, 48);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(79, 20);
            this.label17.TabIndex = 41;
            this.label17.Text = "Vendedor";
            // 
            // txtSucursal
            // 
            this.txtSucursal.BackColor = System.Drawing.SystemColors.ScrollBar;
            this.txtSucursal.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtSucursal.Location = new System.Drawing.Point(89, 13);
            this.txtSucursal.Name = "txtSucursal";
            this.txtSucursal.ReadOnly = true;
            this.txtSucursal.Size = new System.Drawing.Size(185, 26);
            this.txtSucursal.TabIndex = 26;
            this.txtSucursal.TabStop = false;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.ForeColor = System.Drawing.Color.Cornsilk;
            this.label2.Location = new System.Drawing.Point(12, 16);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(71, 20);
            this.label2.TabIndex = 25;
            this.label2.Text = "Sucursal";
            // 
            // txtNroTicket
            // 
            this.txtNroTicket.BackColor = System.Drawing.SystemColors.ScrollBar;
            this.txtNroTicket.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtNroTicket.Location = new System.Drawing.Point(634, 11);
            this.txtNroTicket.Name = "txtNroTicket";
            this.txtNroTicket.ReadOnly = true;
            this.txtNroTicket.Size = new System.Drawing.Size(226, 26);
            this.txtNroTicket.TabIndex = 23;
            this.txtNroTicket.TabStop = false;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label8.ForeColor = System.Drawing.Color.Cornsilk;
            this.label8.Location = new System.Drawing.Point(548, 16);
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
            this.label3.Location = new System.Drawing.Point(574, 48);
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
            this.btnBuscarCliente.Location = new System.Drawing.Point(320, 76);
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
            this.label1.Location = new System.Drawing.Point(25, 80);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(58, 20);
            this.label1.TabIndex = 16;
            this.label1.Text = "Cliente";
            // 
            // txtCliente
            // 
            this.txtCliente.BackColor = System.Drawing.SystemColors.ScrollBar;
            this.txtCliente.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtCliente.Location = new System.Drawing.Point(89, 77);
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
            this.txtCantItems.Location = new System.Drawing.Point(710, 184);
            this.txtCantItems.Name = "txtCantItems";
            this.txtCantItems.ReadOnly = true;
            this.txtCantItems.Size = new System.Drawing.Size(150, 24);
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
            this.label16.Location = new System.Drawing.Point(707, 163);
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
            this.txtTotalS.Location = new System.Drawing.Point(710, 287);
            this.txtTotalS.Name = "txtTotalS";
            this.txtTotalS.ReadOnly = true;
            this.txtTotalS.Size = new System.Drawing.Size(150, 40);
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
            this.label9.Location = new System.Drawing.Point(708, 259);
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
            this.btnAceptar.Location = new System.Drawing.Point(710, 426);
            this.btnAceptar.Name = "btnAceptar";
            this.btnAceptar.Size = new System.Drawing.Size(147, 39);
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
            this.btnCancelar.Location = new System.Drawing.Point(710, 471);
            this.btnCancelar.Name = "btnCancelar";
            this.btnCancelar.Size = new System.Drawing.Size(147, 27);
            this.btnCancelar.TabIndex = 48;
            this.btnCancelar.TabStop = false;
            this.btnCancelar.Text = "&Salir";
            this.btnCancelar.UseVisualStyleBackColor = false;
            this.btnCancelar.Click += new System.EventHandler(this.btnCancelar_Click);
            // 
            // barraControl
            // 
            this.barraControl.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.barraControl.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.cambiarVendedor,
            this.anularVenta,
            this.ImprimirTicket});
            this.barraControl.Location = new System.Drawing.Point(0, 0);
            this.barraControl.Name = "barraControl";
            this.barraControl.Padding = new System.Windows.Forms.Padding(10, 0, 0, 0);
            this.barraControl.Size = new System.Drawing.Size(872, 45);
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
            this.cambiarVendedor.Size = new System.Drawing.Size(111, 42);
            this.cambiarVendedor.Text = "Cambiar vendedor";
            this.cambiarVendedor.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.cambiarVendedor.Click += new System.EventHandler(this.cambiarVendedor_Click);
            // 
            // anularVenta
            // 
            this.anularVenta.Image = ((System.Drawing.Image)(resources.GetObject("anularVenta.Image")));
            this.anularVenta.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.anularVenta.Margin = new System.Windows.Forms.Padding(0, 2, 0, 1);
            this.anularVenta.Name = "anularVenta";
            this.anularVenta.Padding = new System.Windows.Forms.Padding(1);
            this.anularVenta.Size = new System.Drawing.Size(80, 42);
            this.anularVenta.Text = "Anular venta";
            this.anularVenta.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.anularVenta.Visible = false;
            this.anularVenta.Click += new System.EventHandler(this.anularVenta_Click);
            // 
            // ImprimirTicket
            // 
            this.ImprimirTicket.Image = ((System.Drawing.Image)(resources.GetObject("ImprimirTicket.Image")));
            this.ImprimirTicket.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.ImprimirTicket.Margin = new System.Windows.Forms.Padding(0, 2, 0, 1);
            this.ImprimirTicket.Name = "ImprimirTicket";
            this.ImprimirTicket.Padding = new System.Windows.Forms.Padding(1, 1, 1, 6);
            this.ImprimirTicket.Size = new System.Drawing.Size(45, 42);
            this.ImprimirTicket.Text = "Ticket";
            this.ImprimirTicket.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.ImprimirTicket.Click += new System.EventHandler(this.ImprimirTicket_Click);
            // 
            // txtTotalKgs
            // 
            this.txtTotalKgs.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.txtTotalKgs.BackColor = System.Drawing.SystemColors.ScrollBar;
            this.txtTotalKgs.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtTotalKgs.Location = new System.Drawing.Point(710, 232);
            this.txtTotalKgs.Name = "txtTotalKgs";
            this.txtTotalKgs.ReadOnly = true;
            this.txtTotalKgs.Size = new System.Drawing.Size(150, 24);
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
            this.label12.Location = new System.Drawing.Point(707, 211);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(87, 18);
            this.label12.TabIndex = 54;
            this.label12.Text = "Cant. Kgs.";
            // 
            // panelInfoCtaCte
            // 
            this.panelInfoCtaCte.Controls.Add(this.label4);
            this.panelInfoCtaCte.Location = new System.Drawing.Point(320, 37);
            this.panelInfoCtaCte.Name = "panelInfoCtaCte";
            this.panelInfoCtaCte.Size = new System.Drawing.Size(191, 34);
            this.panelInfoCtaCte.TabIndex = 57;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.label4.Location = new System.Drawing.Point(3, 2);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(176, 30);
            this.label4.TabIndex = 15;
            this.label4.Text = "No se pueden modificar\r\n el Cliente en Ventas a Cta. Cte.";
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
            this.Text = "Ultima venta";
            this.Load += new System.EventHandler(this.formUltimaVenta_Load);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.formUltimaVenta_FormClosing);
            ((System.ComponentModel.ISupportInitialize)(this.grillaLineasVenta)).EndInit();
            this.pnlBuscar.ResumeLayout(false);
            this.pnlBuscar.PerformLayout();
            this.barraControl.ResumeLayout(false);
            this.barraControl.PerformLayout();
            this.panelInfoCtaCte.ResumeLayout(false);
            this.panelInfoCtaCte.PerformLayout();
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
        protected System.Windows.Forms.ToolStripButton anularVenta;
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
    }
}