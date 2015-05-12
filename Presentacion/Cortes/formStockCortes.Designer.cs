namespace Presentacion.Cortes
{
    partial class formStockCortes
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle13 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle14 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle15 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle16 = new System.Windows.Forms.DataGridViewCellStyle();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(formStockCortes));
            this.btnCancelar = new System.Windows.Forms.Button();
            this.grillaCortes = new System.Windows.Forms.DataGridView();
            this.idCorte = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.codigo = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.corte = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.precioKg = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.tipo = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.idCorteMaestro = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.corteMaestro = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.porcentaje = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.independiente = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.idSucursalSL = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.porcentajeHueso = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.sucursalSL = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.stockSL = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.stockTeoricoSL = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.idSucursalSM = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.sucursalSM = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.stockSM = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.stockTeoricoSM = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.pnlBuscar = new System.Windows.Forms.Panel();
            this.txtFechaVenta = new System.Windows.Forms.DateTimePicker();
            this.label3 = new System.Windows.Forms.Label();
            this.grupoCortes = new System.Windows.Forms.GroupBox();
            this.txtStockTeoricoActual = new System.Windows.Forms.MaskedTextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.txtStockTeorico = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.txtStockActual = new System.Windows.Forms.MaskedTextBox();
            this.comboSucursal = new System.Windows.Forms.ComboBox();
            this.label13 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.txtStock = new System.Windows.Forms.TextBox();
            this.label12 = new System.Windows.Forms.Label();
            this.txtCodigo = new System.Windows.Forms.MaskedTextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.btnBuscarCorte = new System.Windows.Forms.Button();
            this.txtCorte = new System.Windows.Forms.TextBox();
            this.btnAgregar = new System.Windows.Forms.Button();
            this.label14 = new System.Windows.Forms.Label();
            this.btnAceptar = new System.Windows.Forms.Button();
            this.label11 = new System.Windows.Forms.Label();
            this.txtObservaciones = new System.Windows.Forms.TextBox();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.accionesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.reiniciarStockRealToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.todasLasSucursalesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.sanLorenzoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.sanMartinToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.reiniciarStockTeoricoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.todasLasSucursalesToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.sanLorenzoToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.sanMartinToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.panel1 = new System.Windows.Forms.Panel();
            ((System.ComponentModel.ISupportInitialize)(this.grillaCortes)).BeginInit();
            this.pnlBuscar.SuspendLayout();
            this.grupoCortes.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnCancelar
            // 
            this.btnCancelar.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancelar.Location = new System.Drawing.Point(754, 648);
            this.btnCancelar.Name = "btnCancelar";
            this.btnCancelar.Size = new System.Drawing.Size(84, 27);
            this.btnCancelar.TabIndex = 8;
            this.btnCancelar.TabStop = false;
            this.btnCancelar.Text = "Cancelar";
            this.btnCancelar.UseVisualStyleBackColor = true;
            this.btnCancelar.Click += new System.EventHandler(this.btnCancelar_Click);
            // 
            // grillaCortes
            // 
            this.grillaCortes.AllowUserToAddRows = false;
            this.grillaCortes.AllowUserToResizeRows = false;
            this.grillaCortes.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.grillaCortes.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.Raised;
            this.grillaCortes.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.grillaCortes.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.idCorte,
            this.codigo,
            this.corte,
            this.precioKg,
            this.tipo,
            this.idCorteMaestro,
            this.corteMaestro,
            this.porcentaje,
            this.independiente,
            this.idSucursalSL,
            this.porcentajeHueso,
            this.sucursalSL,
            this.stockSL,
            this.stockTeoricoSL,
            this.idSucursalSM,
            this.sucursalSM,
            this.stockSM,
            this.stockTeoricoSM});
            this.grillaCortes.Location = new System.Drawing.Point(6, 128);
            this.grillaCortes.MultiSelect = false;
            this.grillaCortes.Name = "grillaCortes";
            this.grillaCortes.ReadOnly = true;
            this.grillaCortes.RowHeadersVisible = false;
            this.grillaCortes.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.grillaCortes.Size = new System.Drawing.Size(832, 446);
            this.grillaCortes.StandardTab = true;
            this.grillaCortes.TabIndex = 6;
            this.grillaCortes.TabStop = false;
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
            dataGridViewCellStyle9.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.corte.DefaultCellStyle = dataGridViewCellStyle9;
            this.corte.HeaderText = "Corte";
            this.corte.Name = "corte";
            this.corte.ReadOnly = true;
            this.corte.Width = 150;
            // 
            // precioKg
            // 
            this.precioKg.DataPropertyName = "precioKg";
            dataGridViewCellStyle10.Format = "N2";
            dataGridViewCellStyle10.NullValue = null;
            this.precioKg.DefaultCellStyle = dataGridViewCellStyle10;
            this.precioKg.HeaderText = "Precio Kg.";
            this.precioKg.Name = "precioKg";
            this.precioKg.ReadOnly = true;
            this.precioKg.Visible = false;
            this.precioKg.Width = 80;
            // 
            // tipo
            // 
            this.tipo.DataPropertyName = "tipo";
            this.tipo.HeaderText = "Tipo";
            this.tipo.Name = "tipo";
            this.tipo.ReadOnly = true;
            this.tipo.Visible = false;
            // 
            // idCorteMaestro
            // 
            this.idCorteMaestro.DataPropertyName = "idCorteMaestro";
            this.idCorteMaestro.HeaderText = "ID Codigo Maestro";
            this.idCorteMaestro.Name = "idCorteMaestro";
            this.idCorteMaestro.ReadOnly = true;
            this.idCorteMaestro.Visible = false;
            // 
            // corteMaestro
            // 
            this.corteMaestro.DataPropertyName = "corteMaestro";
            this.corteMaestro.HeaderText = "Corte Maestro";
            this.corteMaestro.Name = "corteMaestro";
            this.corteMaestro.ReadOnly = true;
            this.corteMaestro.Width = 120;
            // 
            // porcentaje
            // 
            this.porcentaje.DataPropertyName = "porcentaje";
            dataGridViewCellStyle11.Format = "N3";
            dataGridViewCellStyle11.NullValue = null;
            this.porcentaje.DefaultCellStyle = dataGridViewCellStyle11;
            this.porcentaje.HeaderText = "Porcentaje";
            this.porcentaje.Name = "porcentaje";
            this.porcentaje.ReadOnly = true;
            this.porcentaje.Visible = false;
            this.porcentaje.Width = 70;
            // 
            // independiente
            // 
            this.independiente.DataPropertyName = "independiente";
            this.independiente.HeaderText = "Independiente";
            this.independiente.Name = "independiente";
            this.independiente.ReadOnly = true;
            this.independiente.Visible = false;
            // 
            // idSucursalSL
            // 
            this.idSucursalSL.DataPropertyName = "idSucursalSL";
            this.idSucursalSL.HeaderText = "ID Sucursal SL";
            this.idSucursalSL.Name = "idSucursalSL";
            this.idSucursalSL.ReadOnly = true;
            this.idSucursalSL.Visible = false;
            // 
            // porcentajeHueso
            // 
            this.porcentajeHueso.DataPropertyName = "porcentajeHueso";
            dataGridViewCellStyle12.Format = "N2";
            dataGridViewCellStyle12.NullValue = null;
            this.porcentajeHueso.DefaultCellStyle = dataGridViewCellStyle12;
            this.porcentajeHueso.HeaderText = "% Hueso";
            this.porcentajeHueso.Name = "porcentajeHueso";
            this.porcentajeHueso.ReadOnly = true;
            this.porcentajeHueso.Visible = false;
            this.porcentajeHueso.Width = 80;
            // 
            // sucursalSL
            // 
            this.sucursalSL.DataPropertyName = "sucursalSL";
            this.sucursalSL.HeaderText = "Sucursal SL";
            this.sucursalSL.Name = "sucursalSL";
            this.sucursalSL.ReadOnly = true;
            this.sucursalSL.Visible = false;
            // 
            // stockSL
            // 
            this.stockSL.DataPropertyName = "stockSL";
            dataGridViewCellStyle13.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle13.Format = "N2";
            dataGridViewCellStyle13.NullValue = null;
            this.stockSL.DefaultCellStyle = dataGridViewCellStyle13;
            this.stockSL.HeaderText = "Stock S. Lorenzo";
            this.stockSL.Name = "stockSL";
            this.stockSL.ReadOnly = true;
            this.stockSL.Width = 115;
            // 
            // stockTeoricoSL
            // 
            this.stockTeoricoSL.DataPropertyName = "stockTeoricoSL";
            dataGridViewCellStyle14.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle14.ForeColor = System.Drawing.Color.Black;
            dataGridViewCellStyle14.Format = "N2";
            dataGridViewCellStyle14.NullValue = null;
            this.stockTeoricoSL.DefaultCellStyle = dataGridViewCellStyle14;
            this.stockTeoricoSL.HeaderText = "Stock Teorico SL";
            this.stockTeoricoSL.Name = "stockTeoricoSL";
            this.stockTeoricoSL.ReadOnly = true;
            this.stockTeoricoSL.Width = 125;
            // 
            // idSucursalSM
            // 
            this.idSucursalSM.DataPropertyName = "idSucursalSM";
            this.idSucursalSM.HeaderText = "ID Sucursal SM";
            this.idSucursalSM.Name = "idSucursalSM";
            this.idSucursalSM.ReadOnly = true;
            this.idSucursalSM.Visible = false;
            // 
            // sucursalSM
            // 
            this.sucursalSM.DataPropertyName = "sucursalSM";
            this.sucursalSM.HeaderText = "Sucursal SM";
            this.sucursalSM.Name = "sucursalSM";
            this.sucursalSM.ReadOnly = true;
            this.sucursalSM.Visible = false;
            // 
            // stockSM
            // 
            this.stockSM.DataPropertyName = "stockSM";
            dataGridViewCellStyle15.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle15.ForeColor = System.Drawing.Color.Black;
            dataGridViewCellStyle15.Format = "N2";
            dataGridViewCellStyle15.NullValue = null;
            this.stockSM.DefaultCellStyle = dataGridViewCellStyle15;
            this.stockSM.HeaderText = "Stock S. Martín";
            this.stockSM.Name = "stockSM";
            this.stockSM.ReadOnly = true;
            this.stockSM.Width = 115;
            // 
            // stockTeoricoSM
            // 
            this.stockTeoricoSM.DataPropertyName = "stockTeoricoSM";
            dataGridViewCellStyle16.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle16.ForeColor = System.Drawing.Color.Black;
            dataGridViewCellStyle16.Format = "N2";
            dataGridViewCellStyle16.NullValue = null;
            this.stockTeoricoSM.DefaultCellStyle = dataGridViewCellStyle16;
            this.stockTeoricoSM.HeaderText = "Stock Teorico SM";
            this.stockTeoricoSM.Name = "stockTeoricoSM";
            this.stockTeoricoSM.ReadOnly = true;
            this.stockTeoricoSM.Width = 125;
            // 
            // pnlBuscar
            // 
            this.pnlBuscar.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.pnlBuscar.BackColor = System.Drawing.Color.SteelBlue;
            this.pnlBuscar.Controls.Add(this.txtFechaVenta);
            this.pnlBuscar.Controls.Add(this.label3);
            this.pnlBuscar.Controls.Add(this.grupoCortes);
            this.pnlBuscar.Location = new System.Drawing.Point(-2, 24);
            this.pnlBuscar.Name = "pnlBuscar";
            this.pnlBuscar.Size = new System.Drawing.Size(849, 98);
            this.pnlBuscar.TabIndex = 10;
            // 
            // txtFechaVenta
            // 
            this.txtFechaVenta.Checked = false;
            this.txtFechaVenta.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.txtFechaVenta.Location = new System.Drawing.Point(717, 9);
            this.txtFechaVenta.Name = "txtFechaVenta";
            this.txtFechaVenta.Size = new System.Drawing.Size(116, 20);
            this.txtFechaVenta.TabIndex = 26;
            this.txtFechaVenta.TabStop = false;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.ForeColor = System.Drawing.Color.Cornsilk;
            this.label3.Location = new System.Drawing.Point(670, 12);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(41, 15);
            this.label3.TabIndex = 27;
            this.label3.Text = "Fecha";
            // 
            // grupoCortes
            // 
            this.grupoCortes.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.grupoCortes.BackColor = System.Drawing.Color.SteelBlue;
            this.grupoCortes.Controls.Add(this.txtStockTeoricoActual);
            this.grupoCortes.Controls.Add(this.label1);
            this.grupoCortes.Controls.Add(this.txtStockTeorico);
            this.grupoCortes.Controls.Add(this.label2);
            this.grupoCortes.Controls.Add(this.txtStockActual);
            this.grupoCortes.Controls.Add(this.comboSucursal);
            this.grupoCortes.Controls.Add(this.label13);
            this.grupoCortes.Controls.Add(this.label4);
            this.grupoCortes.Controls.Add(this.txtStock);
            this.grupoCortes.Controls.Add(this.label12);
            this.grupoCortes.Controls.Add(this.txtCodigo);
            this.grupoCortes.Controls.Add(this.label5);
            this.grupoCortes.Controls.Add(this.btnBuscarCorte);
            this.grupoCortes.Controls.Add(this.txtCorte);
            this.grupoCortes.Controls.Add(this.btnAgregar);
            this.grupoCortes.Controls.Add(this.label14);
            this.grupoCortes.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.grupoCortes.ForeColor = System.Drawing.Color.Cornsilk;
            this.grupoCortes.Location = new System.Drawing.Point(8, 30);
            this.grupoCortes.Name = "grupoCortes";
            this.grupoCortes.Size = new System.Drawing.Size(832, 63);
            this.grupoCortes.TabIndex = 25;
            this.grupoCortes.TabStop = false;
            this.grupoCortes.Text = "Corte";
            // 
            // txtStockTeoricoActual
            // 
            this.txtStockTeoricoActual.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.txtStockTeoricoActual.Location = new System.Drawing.Point(564, 34);
            this.txtStockTeoricoActual.Name = "txtStockTeoricoActual";
            this.txtStockTeoricoActual.Size = new System.Drawing.Size(92, 21);
            this.txtStockTeoricoActual.TabIndex = 2;
            this.txtStockTeoricoActual.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.txtStockTeoricoActual.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.TxtPruebaENTER_KeyPress);
            // 
            // label1
            // 
            this.label1.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ForeColor = System.Drawing.Color.Cornsilk;
            this.label1.Location = new System.Drawing.Point(561, 16);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(86, 15);
            this.label1.TabIndex = 47;
            this.label1.Text = "Stock T. Actual";
            // 
            // txtStockTeorico
            // 
            this.txtStockTeorico.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.txtStockTeorico.Location = new System.Drawing.Point(461, 34);
            this.txtStockTeorico.Name = "txtStockTeorico";
            this.txtStockTeorico.ReadOnly = true;
            this.txtStockTeorico.Size = new System.Drawing.Size(85, 21);
            this.txtStockTeorico.TabIndex = 46;
            this.txtStockTeorico.TabStop = false;
            // 
            // label2
            // 
            this.label2.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.ForeColor = System.Drawing.Color.Cornsilk;
            this.label2.Location = new System.Drawing.Point(458, 16);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(81, 15);
            this.label2.TabIndex = 45;
            this.label2.Text = "Stock Teorico";
            // 
            // txtStockActual
            // 
            this.txtStockActual.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.txtStockActual.Location = new System.Drawing.Point(361, 34);
            this.txtStockActual.Name = "txtStockActual";
            this.txtStockActual.Size = new System.Drawing.Size(84, 21);
            this.txtStockActual.TabIndex = 1;
            this.txtStockActual.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.txtStockActual.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.TxtPruebaENTER_KeyPress);
            // 
            // comboSucursal
            // 
            this.comboSucursal.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.comboSucursal.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboSucursal.FormattingEnabled = true;
            this.comboSucursal.Location = new System.Drawing.Point(673, 33);
            this.comboSucursal.Name = "comboSucursal";
            this.comboSucursal.Size = new System.Drawing.Size(104, 23);
            this.comboSucursal.TabIndex = 3;
            this.comboSucursal.SelectedIndexChanged += new System.EventHandler(this.comboSucursal_TextChanged);
            this.comboSucursal.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.TxtPruebaENTER_KeyPress);
            this.comboSucursal.TextChanged += new System.EventHandler(this.comboSucursal_TextChanged);
            // 
            // label13
            // 
            this.label13.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.label13.AutoSize = true;
            this.label13.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label13.ForeColor = System.Drawing.Color.Cornsilk;
            this.label13.Location = new System.Drawing.Point(358, 16);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(73, 15);
            this.label13.TabIndex = 43;
            this.label13.Text = "Stock Actual";
            // 
            // label4
            // 
            this.label4.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.ForeColor = System.Drawing.Color.Cornsilk;
            this.label4.Location = new System.Drawing.Point(670, 16);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(55, 15);
            this.label4.TabIndex = 17;
            this.label4.Text = "Sucursal";
            // 
            // txtStock
            // 
            this.txtStock.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.txtStock.Location = new System.Drawing.Point(273, 34);
            this.txtStock.Name = "txtStock";
            this.txtStock.ReadOnly = true;
            this.txtStock.Size = new System.Drawing.Size(74, 21);
            this.txtStock.TabIndex = 41;
            this.txtStock.TabStop = false;
            // 
            // label12
            // 
            this.label12.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.label12.AutoSize = true;
            this.label12.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label12.ForeColor = System.Drawing.Color.Cornsilk;
            this.label12.Location = new System.Drawing.Point(270, 16);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(37, 15);
            this.label12.TabIndex = 40;
            this.label12.Text = "Stock";
            // 
            // txtCodigo
            // 
            this.txtCodigo.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.txtCodigo.Location = new System.Drawing.Point(41, 34);
            this.txtCodigo.Name = "txtCodigo";
            this.txtCodigo.Size = new System.Drawing.Size(53, 21);
            this.txtCodigo.TabIndex = 0;
            this.txtCodigo.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.txtCodigo.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.TxtPruebaENTER_KeyPress);
            this.txtCodigo.TextChanged += new System.EventHandler(this.txtCodigo_TextChanged);
            // 
            // label5
            // 
            this.label5.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.ForeColor = System.Drawing.Color.Cornsilk;
            this.label5.Location = new System.Drawing.Point(39, 15);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(46, 15);
            this.label5.TabIndex = 39;
            this.label5.Text = "Código";
            // 
            // btnBuscarCorte
            // 
            this.btnBuscarCorte.AccessibleDescription = "";
            this.btnBuscarCorte.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.btnBuscarCorte.ForeColor = System.Drawing.Color.Black;
            this.btnBuscarCorte.Image = ((System.Drawing.Image)(resources.GetObject("btnBuscarCorte.Image")));
            this.btnBuscarCorte.Location = new System.Drawing.Point(98, 33);
            this.btnBuscarCorte.Name = "btnBuscarCorte";
            this.btnBuscarCorte.Size = new System.Drawing.Size(28, 24);
            this.btnBuscarCorte.TabIndex = 29;
            this.btnBuscarCorte.TabStop = false;
            this.btnBuscarCorte.UseVisualStyleBackColor = true;
            this.btnBuscarCorte.Click += new System.EventHandler(this.btnBuscarCorte_Click);
            // 
            // txtCorte
            // 
            this.txtCorte.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.txtCorte.Location = new System.Drawing.Point(135, 34);
            this.txtCorte.Name = "txtCorte";
            this.txtCorte.ReadOnly = true;
            this.txtCorte.Size = new System.Drawing.Size(124, 21);
            this.txtCorte.TabIndex = 36;
            this.txtCorte.TabStop = false;
            // 
            // btnAgregar
            // 
            this.btnAgregar.AccessibleDescription = "";
            this.btnAgregar.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.btnAgregar.ForeColor = System.Drawing.Color.Black;
            this.btnAgregar.Image = ((System.Drawing.Image)(resources.GetObject("btnAgregar.Image")));
            this.btnAgregar.Location = new System.Drawing.Point(790, 31);
            this.btnAgregar.Name = "btnAgregar";
            this.btnAgregar.Size = new System.Drawing.Size(29, 25);
            this.btnAgregar.TabIndex = 4;
            this.btnAgregar.UseVisualStyleBackColor = true;
            this.btnAgregar.Click += new System.EventHandler(this.btnAgregar_Click);
            this.btnAgregar.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.TxtPruebaENTER_KeyPress);
            // 
            // label14
            // 
            this.label14.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.label14.AutoSize = true;
            this.label14.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label14.ForeColor = System.Drawing.Color.Cornsilk;
            this.label14.Location = new System.Drawing.Point(132, 16);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(72, 15);
            this.label14.TabIndex = 35;
            this.label14.Text = "Descripción";
            // 
            // btnAceptar
            // 
            this.btnAceptar.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnAceptar.Location = new System.Drawing.Point(659, 648);
            this.btnAceptar.Name = "btnAceptar";
            this.btnAceptar.Size = new System.Drawing.Size(89, 27);
            this.btnAceptar.TabIndex = 38;
            this.btnAceptar.TabStop = false;
            this.btnAceptar.Text = "Aceptar";
            this.btnAceptar.UseVisualStyleBackColor = true;
            this.btnAceptar.Click += new System.EventHandler(this.btnAceptar_Click);
            // 
            // label11
            // 
            this.label11.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(8, 576);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(76, 13);
            this.label11.TabIndex = 40;
            this.label11.Text = "observaciones";
            // 
            // txtObservaciones
            // 
            this.txtObservaciones.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.txtObservaciones.Location = new System.Drawing.Point(11, 591);
            this.txtObservaciones.Multiline = true;
            this.txtObservaciones.Name = "txtObservaciones";
            this.txtObservaciones.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtObservaciones.Size = new System.Drawing.Size(279, 43);
            this.txtObservaciones.TabIndex = 39;
            this.txtObservaciones.TabStop = false;
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.accionesToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(845, 24);
            this.menuStrip1.TabIndex = 42;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // accionesToolStripMenuItem
            // 
            this.accionesToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.reiniciarStockRealToolStripMenuItem,
            this.reiniciarStockTeoricoToolStripMenuItem});
            this.accionesToolStripMenuItem.Name = "accionesToolStripMenuItem";
            this.accionesToolStripMenuItem.Size = new System.Drawing.Size(67, 20);
            this.accionesToolStripMenuItem.Text = "Acciones";
            // 
            // reiniciarStockRealToolStripMenuItem
            // 
            this.reiniciarStockRealToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.todasLasSucursalesToolStripMenuItem,
            this.sanLorenzoToolStripMenuItem,
            this.sanMartinToolStripMenuItem});
            this.reiniciarStockRealToolStripMenuItem.Name = "reiniciarStockRealToolStripMenuItem";
            this.reiniciarStockRealToolStripMenuItem.Size = new System.Drawing.Size(194, 22);
            this.reiniciarStockRealToolStripMenuItem.Text = "Reiniciar Stock Real";
            // 
            // todasLasSucursalesToolStripMenuItem
            // 
            this.todasLasSucursalesToolStripMenuItem.Name = "todasLasSucursalesToolStripMenuItem";
            this.todasLasSucursalesToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.todasLasSucursalesToolStripMenuItem.Text = "Todas las sucursales";
            this.todasLasSucursalesToolStripMenuItem.Click += new System.EventHandler(this.todasLasSucursalesToolStripMenuItem_Click);
            // 
            // sanLorenzoToolStripMenuItem
            // 
            this.sanLorenzoToolStripMenuItem.Name = "sanLorenzoToolStripMenuItem";
            this.sanLorenzoToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.sanLorenzoToolStripMenuItem.Text = "San Lorenzo";
            this.sanLorenzoToolStripMenuItem.Click += new System.EventHandler(this.sanLorenzoToolStripMenuItem_Click);
            // 
            // sanMartinToolStripMenuItem
            // 
            this.sanMartinToolStripMenuItem.Name = "sanMartinToolStripMenuItem";
            this.sanMartinToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.sanMartinToolStripMenuItem.Text = "San Martin";
            this.sanMartinToolStripMenuItem.Click += new System.EventHandler(this.sanMartinToolStripMenuItem_Click);
            // 
            // reiniciarStockTeoricoToolStripMenuItem
            // 
            this.reiniciarStockTeoricoToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.todasLasSucursalesToolStripMenuItem1,
            this.sanLorenzoToolStripMenuItem1,
            this.sanMartinToolStripMenuItem1});
            this.reiniciarStockTeoricoToolStripMenuItem.Name = "reiniciarStockTeoricoToolStripMenuItem";
            this.reiniciarStockTeoricoToolStripMenuItem.Size = new System.Drawing.Size(194, 22);
            this.reiniciarStockTeoricoToolStripMenuItem.Text = "Reiniciar Stock Teorico";
            // 
            // todasLasSucursalesToolStripMenuItem1
            // 
            this.todasLasSucursalesToolStripMenuItem1.Name = "todasLasSucursalesToolStripMenuItem1";
            this.todasLasSucursalesToolStripMenuItem1.Size = new System.Drawing.Size(181, 22);
            this.todasLasSucursalesToolStripMenuItem1.Text = "Todas las Sucursales";
            this.todasLasSucursalesToolStripMenuItem1.Click += new System.EventHandler(this.todasLasSucursalesToolStripMenuItem1_Click);
            // 
            // sanLorenzoToolStripMenuItem1
            // 
            this.sanLorenzoToolStripMenuItem1.Name = "sanLorenzoToolStripMenuItem1";
            this.sanLorenzoToolStripMenuItem1.Size = new System.Drawing.Size(181, 22);
            this.sanLorenzoToolStripMenuItem1.Text = "San Lorenzo";
            this.sanLorenzoToolStripMenuItem1.Click += new System.EventHandler(this.sanLorenzoToolStripMenuItem1_Click);
            // 
            // sanMartinToolStripMenuItem1
            // 
            this.sanMartinToolStripMenuItem1.Name = "sanMartinToolStripMenuItem1";
            this.sanMartinToolStripMenuItem1.Size = new System.Drawing.Size(181, 22);
            this.sanMartinToolStripMenuItem1.Text = "San Martin";
            this.sanMartinToolStripMenuItem1.Click += new System.EventHandler(this.sanMartinToolStripMenuItem1_Click);
            // 
            // panel1
            // 
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel1.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.panel1.Location = new System.Drawing.Point(11, 641);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(826, 1);
            this.panel1.TabIndex = 43;
            // 
            // formStockCortes
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Bisque;
            this.ClientSize = new System.Drawing.Size(845, 679);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.label11);
            this.Controls.Add(this.txtObservaciones);
            this.Controls.Add(this.btnAceptar);
            this.Controls.Add(this.pnlBuscar);
            this.Controls.Add(this.btnCancelar);
            this.Controls.Add(this.grillaCortes);
            this.Controls.Add(this.menuStrip1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MainMenuStrip = this.menuStrip1;
            this.MaximizeBox = false;
            this.Name = "formStockCortes";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Stock Cortes";
            ((System.ComponentModel.ISupportInitialize)(this.grillaCortes)).EndInit();
            this.pnlBuscar.ResumeLayout(false);
            this.pnlBuscar.PerformLayout();
            this.grupoCortes.ResumeLayout(false);
            this.grupoCortes.PerformLayout();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        protected System.Windows.Forms.Button btnCancelar;
        protected System.Windows.Forms.DataGridView grillaCortes;
        protected System.Windows.Forms.Panel pnlBuscar;
        protected System.Windows.Forms.Button btnAceptar;
        protected System.Windows.Forms.DateTimePicker txtFechaVenta;
        protected System.Windows.Forms.Label label3;
        protected System.Windows.Forms.GroupBox grupoCortes;
        private System.Windows.Forms.MaskedTextBox txtStockTeoricoActual;
        protected System.Windows.Forms.Label label1;
        protected System.Windows.Forms.TextBox txtStockTeorico;
        protected System.Windows.Forms.Label label2;
        private System.Windows.Forms.MaskedTextBox txtStockActual;
        private System.Windows.Forms.ComboBox comboSucursal;
        protected System.Windows.Forms.Label label13;
        protected System.Windows.Forms.Label label4;
        protected System.Windows.Forms.TextBox txtStock;
        protected System.Windows.Forms.Label label12;
        private System.Windows.Forms.MaskedTextBox txtCodigo;
        protected System.Windows.Forms.Label label5;
        protected internal System.Windows.Forms.Button btnBuscarCorte;
        protected System.Windows.Forms.TextBox txtCorte;
        private System.Windows.Forms.Button btnAgregar;
        protected System.Windows.Forms.Label label14;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.TextBox txtObservaciones;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem accionesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem reiniciarStockRealToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem reiniciarStockTeoricoToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem todasLasSucursalesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem sanLorenzoToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem sanMartinToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem todasLasSucursalesToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem sanLorenzoToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem sanMartinToolStripMenuItem1;
        private System.Windows.Forms.DataGridViewTextBoxColumn idCorte;
        private System.Windows.Forms.DataGridViewTextBoxColumn codigo;
        private System.Windows.Forms.DataGridViewTextBoxColumn corte;
        private System.Windows.Forms.DataGridViewTextBoxColumn precioKg;
        private System.Windows.Forms.DataGridViewTextBoxColumn tipo;
        private System.Windows.Forms.DataGridViewTextBoxColumn idCorteMaestro;
        private System.Windows.Forms.DataGridViewTextBoxColumn corteMaestro;
        private System.Windows.Forms.DataGridViewTextBoxColumn porcentaje;
        private System.Windows.Forms.DataGridViewTextBoxColumn independiente;
        private System.Windows.Forms.DataGridViewTextBoxColumn idSucursalSL;
        private System.Windows.Forms.DataGridViewTextBoxColumn porcentajeHueso;
        private System.Windows.Forms.DataGridViewTextBoxColumn sucursalSL;
        private System.Windows.Forms.DataGridViewTextBoxColumn stockSL;
        private System.Windows.Forms.DataGridViewTextBoxColumn stockTeoricoSL;
        private System.Windows.Forms.DataGridViewTextBoxColumn idSucursalSM;
        private System.Windows.Forms.DataGridViewTextBoxColumn sucursalSM;
        private System.Windows.Forms.DataGridViewTextBoxColumn stockSM;
        private System.Windows.Forms.DataGridViewTextBoxColumn stockTeoricoSM;
        private System.Windows.Forms.Panel panel1;
    }
}