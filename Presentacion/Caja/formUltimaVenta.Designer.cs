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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle33 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle40 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle34 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle35 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle36 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle37 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle38 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle39 = new System.Windows.Forms.DataGridViewCellStyle();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(formUltimaVenta));
            this.grillaLineasVenta = new System.Windows.Forms.DataGridView();
            this.idCorte = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.codigo = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.corte = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.cantKgs = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.precioKgs = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.totalS = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.estado = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.pnlBuscar = new System.Windows.Forms.Panel();
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
            ((System.ComponentModel.ISupportInitialize)(this.grillaLineasVenta)).BeginInit();
            this.pnlBuscar.SuspendLayout();
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
            dataGridViewCellStyle33.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle33.Font = new System.Drawing.Font("Microsoft Sans Serif", 15F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle33.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle33.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle33.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle33.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.grillaLineasVenta.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle33;
            this.grillaLineasVenta.ColumnHeadersHeight = 50;
            this.grillaLineasVenta.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.idCorte,
            this.codigo,
            this.corte,
            this.cantKgs,
            this.precioKgs,
            this.totalS,
            this.estado});
            dataGridViewCellStyle40.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle40.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle40.Font = new System.Drawing.Font("Microsoft Sans Serif", 15F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle40.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle40.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle40.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle40.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.grillaLineasVenta.DefaultCellStyle = dataGridViewCellStyle40;
            this.grillaLineasVenta.Location = new System.Drawing.Point(12, 163);
            this.grillaLineasVenta.MultiSelect = false;
            this.grillaLineasVenta.Name = "grillaLineasVenta";
            this.grillaLineasVenta.ReadOnly = true;
            this.grillaLineasVenta.RowHeadersVisible = false;
            this.grillaLineasVenta.RowTemplate.Height = 40;
            this.grillaLineasVenta.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.grillaLineasVenta.Size = new System.Drawing.Size(692, 283);
            this.grillaLineasVenta.TabIndex = 41;
            this.grillaLineasVenta.TabStop = false;
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
            dataGridViewCellStyle34.Alignment = System.Windows.Forms.DataGridViewContentAlignment.BottomLeft;
            this.codigo.DefaultCellStyle = dataGridViewCellStyle34;
            this.codigo.HeaderText = "Código";
            this.codigo.Name = "codigo";
            this.codigo.ReadOnly = true;
            // 
            // corte
            // 
            this.corte.DataPropertyName = "corte";
            dataGridViewCellStyle35.Alignment = System.Windows.Forms.DataGridViewContentAlignment.BottomLeft;
            this.corte.DefaultCellStyle = dataGridViewCellStyle35;
            this.corte.HeaderText = "Corte";
            this.corte.Name = "corte";
            this.corte.ReadOnly = true;
            // 
            // cantKgs
            // 
            this.cantKgs.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.cantKgs.DataPropertyName = "cantKgs";
            dataGridViewCellStyle36.Alignment = System.Windows.Forms.DataGridViewContentAlignment.BottomRight;
            dataGridViewCellStyle36.Format = "N3";
            dataGridViewCellStyle36.NullValue = null;
            this.cantKgs.DefaultCellStyle = dataGridViewCellStyle36;
            this.cantKgs.HeaderText = "Cantidad";
            this.cantKgs.Name = "cantKgs";
            this.cantKgs.ReadOnly = true;
            this.cantKgs.Width = 150;
            // 
            // precioKgs
            // 
            this.precioKgs.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.precioKgs.DataPropertyName = "precioKg";
            dataGridViewCellStyle37.Alignment = System.Windows.Forms.DataGridViewContentAlignment.BottomRight;
            dataGridViewCellStyle37.Format = "N2";
            dataGridViewCellStyle37.NullValue = null;
            this.precioKgs.DefaultCellStyle = dataGridViewCellStyle37;
            this.precioKgs.HeaderText = "Precio";
            this.precioKgs.Name = "precioKgs";
            this.precioKgs.ReadOnly = true;
            this.precioKgs.Width = 150;
            // 
            // totalS
            // 
            this.totalS.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.totalS.DataPropertyName = "totalS";
            dataGridViewCellStyle38.Alignment = System.Windows.Forms.DataGridViewContentAlignment.BottomRight;
            dataGridViewCellStyle38.Format = "N2";
            dataGridViewCellStyle38.NullValue = null;
            this.totalS.DefaultCellStyle = dataGridViewCellStyle38;
            this.totalS.HeaderText = "Total";
            this.totalS.Name = "totalS";
            this.totalS.ReadOnly = true;
            this.totalS.Width = 150;
            // 
            // estado
            // 
            this.estado.DataPropertyName = "estado";
            dataGridViewCellStyle39.ForeColor = System.Drawing.Color.Red;
            this.estado.DefaultCellStyle = dataGridViewCellStyle39;
            this.estado.HeaderText = "Estado";
            this.estado.Name = "estado";
            this.estado.ReadOnly = true;
            this.estado.Visible = false;
            // 
            // pnlBuscar
            // 
            this.pnlBuscar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.pnlBuscar.BackColor = System.Drawing.Color.SteelBlue;
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
            this.label11.Location = new System.Drawing.Point(8, 451);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(133, 16);
            this.label11.TabIndex = 43;
            this.label11.Text = "Observaciones (F11)";
            // 
            // txtObservaciones
            // 
            this.txtObservaciones.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.txtObservaciones.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtObservaciones.Location = new System.Drawing.Point(12, 470);
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
            this.txtCantItems.Location = new System.Drawing.Point(710, 183);
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
            this.label16.Location = new System.Drawing.Point(707, 162);
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
            this.txtTotalS.Location = new System.Drawing.Point(710, 244);
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
            this.label9.Location = new System.Drawing.Point(708, 216);
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
            this.btnAceptar.Location = new System.Drawing.Point(710, 374);
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
            this.btnCancelar.Location = new System.Drawing.Point(710, 419);
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
            // formUltimaVenta
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(872, 524);
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
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.DataGridView grillaLineasVenta;
        private System.Windows.Forms.DataGridViewTextBoxColumn idCorte;
        private System.Windows.Forms.DataGridViewTextBoxColumn codigo;
        private System.Windows.Forms.DataGridViewTextBoxColumn corte;
        private System.Windows.Forms.DataGridViewTextBoxColumn cantKgs;
        private System.Windows.Forms.DataGridViewTextBoxColumn precioKgs;
        private System.Windows.Forms.DataGridViewTextBoxColumn totalS;
        private System.Windows.Forms.DataGridViewTextBoxColumn estado;
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
    }
}