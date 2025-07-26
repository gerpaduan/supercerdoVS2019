namespace Presentacion.Cheques
{
    partial class formCheques
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle14 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle15 = new System.Windows.Forms.DataGridViewCellStyle();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(formCheques));
            this.grilla = new System.Windows.Forms.DataGridView();
            this.pnlBuscar = new System.Windows.Forms.Panel();
            this.checkPropioFiltro = new System.Windows.Forms.CheckBox();
            this.btnBuscar = new System.Windows.Forms.Button();
            this.txtUsuario = new System.Windows.Forms.TextBox();
            this.label16 = new System.Windows.Forms.Label();
            this.comboEstadosFiltro = new System.Windows.Forms.ComboBox();
            this.label8 = new System.Windows.Forms.Label();
            this.groupCheque = new System.Windows.Forms.GroupBox();
            this.btnObservaciones = new System.Windows.Forms.Button();
            this.label18 = new System.Windows.Forms.Label();
            this.label17 = new System.Windows.Forms.Label();
            this.btnCancelar = new System.Windows.Forms.Button();
            this.btnGuardar = new System.Windows.Forms.Button();
            this.comboEstado = new System.Windows.Forms.ComboBox();
            this.label15 = new System.Windows.Forms.Label();
            this.txtEntregadoA = new System.Windows.Forms.TextBox();
            this.label14 = new System.Windows.Forms.Label();
            this.txtRecibidoDe = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.txtTitular = new System.Windows.Forms.TextBox();
            this.label13 = new System.Windows.Forms.Label();
            this.txtFechaEmision = new System.Windows.Forms.MaskedTextBox();
            this.label12 = new System.Windows.Forms.Label();
            this.txtFechaPago = new System.Windows.Forms.DateTimePicker();
            this.label5 = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.txtObservaciones = new System.Windows.Forms.TextBox();
            this.txtImporteCheque = new System.Windows.Forms.TextBox();
            this.label10 = new System.Windows.Forms.Label();
            this.checkPropio = new System.Windows.Forms.CheckBox();
            this.comboBanco = new System.Windows.Forms.ComboBox();
            this.label9 = new System.Windows.Forms.Label();
            this.txtNroCheque = new System.Windows.Forms.TextBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.txtBuscarNroCheque = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.txtFechaDesde = new System.Windows.Forms.DateTimePicker();
            this.btnEliminar = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.btnModificar = new System.Windows.Forms.Button();
            this.txtFechaHasta = new System.Windows.Forms.DateTimePicker();
            this.btnNuevo = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.lblChequeVence = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.grilla)).BeginInit();
            this.pnlBuscar.SuspendLayout();
            this.groupCheque.SuspendLayout();
            this.SuspendLayout();
            // 
            // grilla
            // 
            this.grilla.AllowUserToAddRows = false;
            this.grilla.AllowUserToOrderColumns = true;
            this.grilla.AllowUserToResizeRows = false;
            this.grilla.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.grilla.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
            dataGridViewCellStyle13.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle13.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle13.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle13.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle13.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle13.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle13.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.grilla.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle13;
            this.grilla.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewCellStyle14.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle14.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle14.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle14.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle14.NullValue = null;
            dataGridViewCellStyle14.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle14.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle14.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.grilla.DefaultCellStyle = dataGridViewCellStyle14;
            this.grilla.Location = new System.Drawing.Point(8, 245);
            this.grilla.Name = "grilla";
            this.grilla.ReadOnly = true;
            dataGridViewCellStyle15.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle15.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle15.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle15.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle15.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle15.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle15.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.grilla.RowHeadersDefaultCellStyle = dataGridViewCellStyle15;
            this.grilla.RowHeadersVisible = false;
            this.grilla.RowHeadersWidth = 51;
            this.grilla.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.grilla.Size = new System.Drawing.Size(905, 329);
            this.grilla.TabIndex = 21;
            this.grilla.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.grilla_CellDoubleClick);
            this.grilla.CellFormatting += new System.Windows.Forms.DataGridViewCellFormattingEventHandler(this.grilla_CellFormatting);
            // 
            // pnlBuscar
            // 
            this.pnlBuscar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pnlBuscar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(43)))), ((int)(((byte)(77)))), ((int)(((byte)(129)))));
            this.pnlBuscar.Controls.Add(this.lblChequeVence);
            this.pnlBuscar.Controls.Add(this.checkPropioFiltro);
            this.pnlBuscar.Controls.Add(this.btnBuscar);
            this.pnlBuscar.Controls.Add(this.txtUsuario);
            this.pnlBuscar.Controls.Add(this.label16);
            this.pnlBuscar.Controls.Add(this.comboEstadosFiltro);
            this.pnlBuscar.Controls.Add(this.label8);
            this.pnlBuscar.Controls.Add(this.groupCheque);
            this.pnlBuscar.Controls.Add(this.groupBox2);
            this.pnlBuscar.Controls.Add(this.txtBuscarNroCheque);
            this.pnlBuscar.Controls.Add(this.label2);
            this.pnlBuscar.Controls.Add(this.label1);
            this.pnlBuscar.Controls.Add(this.txtFechaDesde);
            this.pnlBuscar.Controls.Add(this.btnEliminar);
            this.pnlBuscar.Controls.Add(this.label3);
            this.pnlBuscar.Controls.Add(this.btnModificar);
            this.pnlBuscar.Controls.Add(this.txtFechaHasta);
            this.pnlBuscar.Controls.Add(this.btnNuevo);
            this.pnlBuscar.Controls.Add(this.label4);
            this.pnlBuscar.Location = new System.Drawing.Point(-9, 2);
            this.pnlBuscar.Name = "pnlBuscar";
            this.pnlBuscar.Size = new System.Drawing.Size(943, 237);
            this.pnlBuscar.TabIndex = 18;
            // 
            // checkPropioFiltro
            // 
            this.checkPropioFiltro.AutoSize = true;
            this.checkPropioFiltro.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.checkPropioFiltro.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.checkPropioFiltro.ForeColor = System.Drawing.Color.Cornsilk;
            this.checkPropioFiltro.Location = new System.Drawing.Point(145, 61);
            this.checkPropioFiltro.Name = "checkPropioFiltro";
            this.checkPropioFiltro.Size = new System.Drawing.Size(96, 19);
            this.checkPropioFiltro.TabIndex = 3;
            this.checkPropioFiltro.TabStop = false;
            this.checkPropioFiltro.Text = "Solo Propios";
            this.checkPropioFiltro.UseVisualStyleBackColor = true;
            this.checkPropioFiltro.CheckedChanged += new System.EventHandler(this.checkPropioFiltro_CheckedChanged);
            // 
            // btnBuscar
            // 
            this.btnBuscar.AccessibleDescription = "";
            this.btnBuscar.ForeColor = System.Drawing.Color.Black;
            this.btnBuscar.Image = ((System.Drawing.Image)(resources.GetObject("btnBuscar.Image")));
            this.btnBuscar.Location = new System.Drawing.Point(247, 37);
            this.btnBuscar.Name = "btnBuscar";
            this.btnBuscar.Size = new System.Drawing.Size(24, 21);
            this.btnBuscar.TabIndex = 62;
            this.btnBuscar.UseVisualStyleBackColor = true;
            this.btnBuscar.Click += new System.EventHandler(this.btnBuscar_Click);
            // 
            // txtUsuario
            // 
            this.txtUsuario.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.txtUsuario.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtUsuario.Location = new System.Drawing.Point(764, 7);
            this.txtUsuario.Name = "txtUsuario";
            this.txtUsuario.ReadOnly = true;
            this.txtUsuario.Size = new System.Drawing.Size(158, 22);
            this.txtUsuario.TabIndex = 61;
            this.txtUsuario.TabStop = false;
            // 
            // label16
            // 
            this.label16.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label16.AutoSize = true;
            this.label16.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label16.ForeColor = System.Drawing.Color.Cornsilk;
            this.label16.Location = new System.Drawing.Point(708, 10);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(54, 16);
            this.label16.TabIndex = 60;
            this.label16.Text = "Usuario";
            // 
            // comboEstadosFiltro
            // 
            this.comboEstadosFiltro.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboEstadosFiltro.FormattingEnabled = true;
            this.comboEstadosFiltro.Items.AddRange(new object[] {
            "TODOS",
            "PENDIENTE",
            "ENTREGADO",
            "DEPOSITADO",
            "ACREDITADO",
            "RECHAZADO",
            "VENCIDO"});
            this.comboEstadosFiltro.Location = new System.Drawing.Point(100, 9);
            this.comboEstadosFiltro.Name = "comboEstadosFiltro";
            this.comboEstadosFiltro.Size = new System.Drawing.Size(141, 21);
            this.comboEstadosFiltro.TabIndex = 1;
            this.comboEstadosFiltro.SelectedValueChanged += new System.EventHandler(this.btnBuscar_Click);
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label8.ForeColor = System.Drawing.Color.Cornsilk;
            this.label8.Location = new System.Drawing.Point(43, 10);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(51, 15);
            this.label8.TabIndex = 49;
            this.label8.Text = "Estados";
            // 
            // groupCheque
            // 
            this.groupCheque.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupCheque.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(43)))), ((int)(((byte)(77)))), ((int)(((byte)(129)))));
            this.groupCheque.Controls.Add(this.btnObservaciones);
            this.groupCheque.Controls.Add(this.label18);
            this.groupCheque.Controls.Add(this.label17);
            this.groupCheque.Controls.Add(this.btnCancelar);
            this.groupCheque.Controls.Add(this.btnGuardar);
            this.groupCheque.Controls.Add(this.comboEstado);
            this.groupCheque.Controls.Add(this.label15);
            this.groupCheque.Controls.Add(this.txtEntregadoA);
            this.groupCheque.Controls.Add(this.label14);
            this.groupCheque.Controls.Add(this.txtRecibidoDe);
            this.groupCheque.Controls.Add(this.label7);
            this.groupCheque.Controls.Add(this.label6);
            this.groupCheque.Controls.Add(this.txtTitular);
            this.groupCheque.Controls.Add(this.label13);
            this.groupCheque.Controls.Add(this.txtFechaEmision);
            this.groupCheque.Controls.Add(this.label12);
            this.groupCheque.Controls.Add(this.txtFechaPago);
            this.groupCheque.Controls.Add(this.label5);
            this.groupCheque.Controls.Add(this.label11);
            this.groupCheque.Controls.Add(this.txtObservaciones);
            this.groupCheque.Controls.Add(this.txtImporteCheque);
            this.groupCheque.Controls.Add(this.label10);
            this.groupCheque.Controls.Add(this.checkPropio);
            this.groupCheque.Controls.Add(this.comboBanco);
            this.groupCheque.Controls.Add(this.label9);
            this.groupCheque.Controls.Add(this.txtNroCheque);
            this.groupCheque.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupCheque.ForeColor = System.Drawing.Color.Cornsilk;
            this.groupCheque.Location = new System.Drawing.Point(21, 74);
            this.groupCheque.Name = "groupCheque";
            this.groupCheque.Size = new System.Drawing.Size(901, 160);
            this.groupCheque.TabIndex = 48;
            this.groupCheque.TabStop = false;
            this.groupCheque.Text = "Datos Cheque";
            // 
            // btnObservaciones
            // 
            this.btnObservaciones.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnObservaciones.ForeColor = System.Drawing.Color.Black;
            this.btnObservaciones.Location = new System.Drawing.Point(445, 124);
            this.btnObservaciones.Name = "btnObservaciones";
            this.btnObservaciones.Size = new System.Drawing.Size(17, 30);
            this.btnObservaciones.TabIndex = 77;
            this.btnObservaciones.Text = ">";
            this.btnObservaciones.UseVisualStyleBackColor = true;
            this.btnObservaciones.Click += new System.EventHandler(this.btnObservaciones_Click);
            // 
            // label18
            // 
            this.label18.AutoSize = true;
            this.label18.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label18.ForeColor = System.Drawing.Color.Cornsilk;
            this.label18.Location = new System.Drawing.Point(612, 44);
            this.label18.Name = "label18";
            this.label18.Size = new System.Drawing.Size(12, 15);
            this.label18.TabIndex = 76;
            this.label18.Text = "*";
            // 
            // label17
            // 
            this.label17.AutoSize = true;
            this.label17.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label17.ForeColor = System.Drawing.Color.Cornsilk;
            this.label17.Location = new System.Drawing.Point(442, 43);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(12, 15);
            this.label17.TabIndex = 75;
            this.label17.Text = "*";
            // 
            // btnCancelar
            // 
            this.btnCancelar.AccessibleDescription = "";
            this.btnCancelar.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancelar.ForeColor = System.Drawing.Color.Black;
            this.btnCancelar.Location = new System.Drawing.Point(770, 128);
            this.btnCancelar.Name = "btnCancelar";
            this.btnCancelar.Size = new System.Drawing.Size(113, 24);
            this.btnCancelar.TabIndex = 29;
            this.btnCancelar.Text = "&Cancelar";
            this.btnCancelar.UseVisualStyleBackColor = true;
            this.btnCancelar.Click += new System.EventHandler(this.btnCancelar_Click_1);
            // 
            // btnGuardar
            // 
            this.btnGuardar.AccessibleDescription = "";
            this.btnGuardar.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnGuardar.ForeColor = System.Drawing.Color.Black;
            this.btnGuardar.Location = new System.Drawing.Point(650, 128);
            this.btnGuardar.Name = "btnGuardar";
            this.btnGuardar.Size = new System.Drawing.Size(113, 24);
            this.btnGuardar.TabIndex = 28;
            this.btnGuardar.Text = "&Guardar";
            this.btnGuardar.UseVisualStyleBackColor = true;
            this.btnGuardar.Click += new System.EventHandler(this.btnGuardar_Click);
            // 
            // comboEstado
            // 
            this.comboEstado.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboEstado.FormattingEnabled = true;
            this.comboEstado.Items.AddRange(new object[] {
            "PENDIENTE",
            "ENTREGADO",
            "DEPOSITADO",
            "ACREDITADO",
            "RECHAZADO",
            "VENCIDO"});
            this.comboEstado.Location = new System.Drawing.Point(470, 91);
            this.comboEstado.Name = "comboEstado";
            this.comboEstado.Size = new System.Drawing.Size(141, 23);
            this.comboEstado.TabIndex = 26;
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label15.ForeColor = System.Drawing.Color.Cornsilk;
            this.label15.Location = new System.Drawing.Point(467, 74);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(51, 15);
            this.label15.TabIndex = 72;
            this.label15.Text = "Estados";
            // 
            // txtEntregadoA
            // 
            this.txtEntregadoA.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.txtEntregadoA.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtEntregadoA.Location = new System.Drawing.Point(650, 92);
            this.txtEntregadoA.Name = "txtEntregadoA";
            this.txtEntregadoA.ReadOnly = true;
            this.txtEntregadoA.Size = new System.Drawing.Size(233, 22);
            this.txtEntregadoA.TabIndex = 70;
            this.txtEntregadoA.TabStop = false;
            this.txtEntregadoA.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label14
            // 
            this.label14.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label14.AutoSize = true;
            this.label14.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.label14.ForeColor = System.Drawing.Color.Cornsilk;
            this.label14.Location = new System.Drawing.Point(647, 72);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(74, 15);
            this.label14.TabIndex = 71;
            this.label14.Text = "Entregado a";
            // 
            // txtRecibidoDe
            // 
            this.txtRecibidoDe.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.txtRecibidoDe.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtRecibidoDe.Location = new System.Drawing.Point(650, 44);
            this.txtRecibidoDe.Name = "txtRecibidoDe";
            this.txtRecibidoDe.ReadOnly = true;
            this.txtRecibidoDe.Size = new System.Drawing.Size(233, 22);
            this.txtRecibidoDe.TabIndex = 68;
            this.txtRecibidoDe.TabStop = false;
            this.txtRecibidoDe.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label7
            // 
            this.label7.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.label7.ForeColor = System.Drawing.Color.Cornsilk;
            this.label7.Location = new System.Drawing.Point(647, 26);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(73, 15);
            this.label7.TabIndex = 69;
            this.label7.Text = "Recibido de";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.label6.ForeColor = System.Drawing.Color.Cornsilk;
            this.label6.Location = new System.Drawing.Point(230, 47);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(64, 15);
            this.label6.TabIndex = 67;
            this.label6.Text = "N°Cheque";
            // 
            // txtTitular
            // 
            this.txtTitular.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtTitular.Location = new System.Drawing.Point(79, 98);
            this.txtTitular.Name = "txtTitular";
            this.txtTitular.Size = new System.Drawing.Size(362, 22);
            this.txtTitular.TabIndex = 25;
            this.txtTitular.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.label13.ForeColor = System.Drawing.Color.Cornsilk;
            this.label13.Location = new System.Drawing.Point(32, 99);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(41, 15);
            this.label13.TabIndex = 66;
            this.label13.Text = "Titular";
            // 
            // txtFechaEmision
            // 
            this.txtFechaEmision.Location = new System.Drawing.Point(79, 71);
            this.txtFechaEmision.Mask = "00/00/0000";
            this.txtFechaEmision.Name = "txtFechaEmision";
            this.txtFechaEmision.Size = new System.Drawing.Size(141, 21);
            this.txtFechaEmision.TabIndex = 23;
            this.txtFechaEmision.ValidatingType = typeof(System.DateTime);
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label12.ForeColor = System.Drawing.Color.Cornsilk;
            this.label12.Location = new System.Drawing.Point(1, 75);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(75, 15);
            this.label12.TabIndex = 63;
            this.label12.Text = "Fec.Emisión";
            // 
            // txtFechaPago
            // 
            this.txtFechaPago.CalendarFont = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtFechaPago.CustomFormat = "dd/MM/yyyy";
            this.txtFechaPago.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.txtFechaPago.Location = new System.Drawing.Point(300, 72);
            this.txtFechaPago.Name = "txtFechaPago";
            this.txtFechaPago.Size = new System.Drawing.Size(141, 21);
            this.txtFechaPago.TabIndex = 24;
            this.txtFechaPago.Value = new System.DateTime(2025, 7, 18, 0, 0, 0, 0);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.ForeColor = System.Drawing.Color.Cornsilk;
            this.label5.Location = new System.Drawing.Point(235, 75);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(59, 15);
            this.label5.TabIndex = 54;
            this.label5.Text = "Fec.Pago";
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(41, 125);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(32, 15);
            this.label11.TabIndex = 61;
            this.label11.Text = "Obs.";
            // 
            // txtObservaciones
            // 
            this.txtObservaciones.Location = new System.Drawing.Point(79, 125);
            this.txtObservaciones.Multiline = true;
            this.txtObservaciones.Name = "txtObservaciones";
            this.txtObservaciones.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtObservaciones.Size = new System.Drawing.Size(362, 29);
            this.txtObservaciones.TabIndex = 27;
            this.txtObservaciones.TabStop = false;
            // 
            // txtImporteCheque
            // 
            this.txtImporteCheque.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtImporteCheque.Location = new System.Drawing.Point(470, 44);
            this.txtImporteCheque.Name = "txtImporteCheque";
            this.txtImporteCheque.Size = new System.Drawing.Size(141, 22);
            this.txtImporteCheque.TabIndex = 22;
            this.txtImporteCheque.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label10.ForeColor = System.Drawing.Color.Cornsilk;
            this.label10.Location = new System.Drawing.Point(450, 47);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(14, 15);
            this.label10.TabIndex = 56;
            this.label10.Text = "$";
            // 
            // checkPropio
            // 
            this.checkPropio.AutoSize = true;
            this.checkPropio.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.checkPropio.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.checkPropio.ForeColor = System.Drawing.Color.Cornsilk;
            this.checkPropio.Location = new System.Drawing.Point(31, 23);
            this.checkPropio.Name = "checkPropio";
            this.checkPropio.Size = new System.Drawing.Size(62, 19);
            this.checkPropio.TabIndex = 54;
            this.checkPropio.TabStop = false;
            this.checkPropio.Text = "Propio";
            this.checkPropio.UseVisualStyleBackColor = true;
            // 
            // comboBanco
            // 
            this.comboBanco.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBanco.FormattingEnabled = true;
            this.comboBanco.Items.AddRange(new object[] {
            "Ver Todos",
            "Ingreso Stock",
            "Egreso Stock",
            "Cierre Stock",
            "Pesaje Cortes",
            "Ajuste Stock"});
            this.comboBanco.Location = new System.Drawing.Point(79, 44);
            this.comboBanco.Name = "comboBanco";
            this.comboBanco.Size = new System.Drawing.Size(141, 23);
            this.comboBanco.TabIndex = 20;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label9.ForeColor = System.Drawing.Color.Cornsilk;
            this.label9.Location = new System.Drawing.Point(34, 47);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(42, 15);
            this.label9.TabIndex = 51;
            this.label9.Text = "Banco";
            // 
            // txtNroCheque
            // 
            this.txtNroCheque.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtNroCheque.Location = new System.Drawing.Point(300, 44);
            this.txtNroCheque.Name = "txtNroCheque";
            this.txtNroCheque.Size = new System.Drawing.Size(141, 22);
            this.txtNroCheque.TabIndex = 21;
            this.txtNroCheque.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // groupBox2
            // 
            this.groupBox2.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.groupBox2.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.groupBox2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.groupBox2.Location = new System.Drawing.Point(412, 18);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(258, 7);
            this.groupBox2.TabIndex = 47;
            this.groupBox2.TabStop = false;
            // 
            // txtBuscarNroCheque
            // 
            this.txtBuscarNroCheque.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtBuscarNroCheque.Location = new System.Drawing.Point(100, 36);
            this.txtBuscarNroCheque.Name = "txtBuscarNroCheque";
            this.txtBuscarNroCheque.Size = new System.Drawing.Size(141, 21);
            this.txtBuscarNroCheque.TabIndex = 2;
            this.txtBuscarNroCheque.TextChanged += new System.EventHandler(this.txtDescripcion_TextChanged);
            this.txtBuscarNroCheque.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtDescripcion_KeyPress);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.ForeColor = System.Drawing.Color.Cornsilk;
            this.label2.Location = new System.Drawing.Point(30, 39);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(64, 15);
            this.label2.TabIndex = 11;
            this.label2.Text = "N°Cheque";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ForeColor = System.Drawing.Color.Cornsilk;
            this.label1.Location = new System.Drawing.Point(335, 10);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(77, 15);
            this.label1.TabIndex = 9;
            this.label1.Text = "Fecha Cobro";
            // 
            // txtFechaDesde
            // 
            this.txtFechaDesde.CalendarFont = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtFechaDesde.CustomFormat = "dd/MM/yyyy";
            this.txtFechaDesde.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.txtFechaDesde.Location = new System.Drawing.Point(419, 31);
            this.txtFechaDesde.Name = "txtFechaDesde";
            this.txtFechaDesde.Size = new System.Drawing.Size(98, 20);
            this.txtFechaDesde.TabIndex = 4;
            this.txtFechaDesde.Value = new System.DateTime(2025, 7, 16, 0, 0, 0, 0);
            this.txtFechaDesde.ValueChanged += new System.EventHandler(this.txtFechaDesde_ValueChanged);
            // 
            // btnEliminar
            // 
            this.btnEliminar.AccessibleDescription = "";
            this.btnEliminar.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnEliminar.ForeColor = System.Drawing.Color.Black;
            this.btnEliminar.Location = new System.Drawing.Point(854, 44);
            this.btnEliminar.Name = "btnEliminar";
            this.btnEliminar.Size = new System.Drawing.Size(68, 24);
            this.btnEliminar.TabIndex = 59;
            this.btnEliminar.Text = "&Eliminar";
            this.btnEliminar.UseVisualStyleBackColor = true;
            this.btnEliminar.Click += new System.EventHandler(this.btnEliminar_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.ForeColor = System.Drawing.Color.Cornsilk;
            this.label3.Location = new System.Drawing.Point(369, 33);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(43, 15);
            this.label3.TabIndex = 4;
            this.label3.Text = "Desde";
            // 
            // btnModificar
            // 
            this.btnModificar.AccessibleDescription = "";
            this.btnModificar.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnModificar.ForeColor = System.Drawing.Color.Black;
            this.btnModificar.Location = new System.Drawing.Point(781, 44);
            this.btnModificar.Name = "btnModificar";
            this.btnModificar.Size = new System.Drawing.Size(68, 24);
            this.btnModificar.TabIndex = 58;
            this.btnModificar.Text = "&Modificar";
            this.btnModificar.UseVisualStyleBackColor = true;
            this.btnModificar.Click += new System.EventHandler(this.btnModificar_Click);
            // 
            // txtFechaHasta
            // 
            this.txtFechaHasta.CalendarFont = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtFechaHasta.CustomFormat = "dd/MM/yyyy";
            this.txtFechaHasta.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.txtFechaHasta.Location = new System.Drawing.Point(572, 31);
            this.txtFechaHasta.Name = "txtFechaHasta";
            this.txtFechaHasta.Size = new System.Drawing.Size(98, 20);
            this.txtFechaHasta.TabIndex = 5;
            this.txtFechaHasta.ValueChanged += new System.EventHandler(this.txtFechaHasta_ValueChanged);
            // 
            // btnNuevo
            // 
            this.btnNuevo.AccessibleDescription = "";
            this.btnNuevo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnNuevo.ForeColor = System.Drawing.Color.Black;
            this.btnNuevo.Location = new System.Drawing.Point(710, 44);
            this.btnNuevo.Name = "btnNuevo";
            this.btnNuevo.Size = new System.Drawing.Size(65, 24);
            this.btnNuevo.TabIndex = 57;
            this.btnNuevo.Text = "&Nuevo";
            this.btnNuevo.UseVisualStyleBackColor = true;
            this.btnNuevo.Click += new System.EventHandler(this.btnNuevo_Click);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.ForeColor = System.Drawing.Color.Cornsilk;
            this.label4.Location = new System.Drawing.Point(527, 33);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(39, 15);
            this.label4.TabIndex = 6;
            this.label4.Text = "Hasta";
            // 
            // lblChequeVence
            // 
            this.lblChequeVence.AutoSize = true;
            this.lblChequeVence.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.lblChequeVence.ForeColor = System.Drawing.Color.DarkSalmon;
            this.lblChequeVence.Location = new System.Drawing.Point(335, 62);
            this.lblChequeVence.Name = "lblChequeVence";
            this.lblChequeVence.Size = new System.Drawing.Size(284, 15);
            this.lblChequeVence.TabIndex = 78;
            this.lblChequeVence.Text = "Hay Cheques Vencidos(rojo) | Por Vencer (naranja)";
            this.lblChequeVence.Visible = false;
            // 
            // formCheques
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(239)))), ((int)(((byte)(250)))));
            this.ClientSize = new System.Drawing.Size(925, 586);
            this.Controls.Add(this.grilla);
            this.Controls.Add(this.pnlBuscar);
            this.Name = "formCheques";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Cheques";
            this.Load += new System.EventHandler(this.formCheques_Load);
            ((System.ComponentModel.ISupportInitialize)(this.grilla)).EndInit();
            this.pnlBuscar.ResumeLayout(false);
            this.pnlBuscar.PerformLayout();
            this.groupCheque.ResumeLayout(false);
            this.groupCheque.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView grilla;
        protected System.Windows.Forms.Panel pnlBuscar;
        private System.Windows.Forms.TextBox txtBuscarNroCheque;
        protected System.Windows.Forms.Label label2;
        protected System.Windows.Forms.Label label1;
        protected System.Windows.Forms.DateTimePicker txtFechaDesde;
        protected System.Windows.Forms.Label label3;
        protected System.Windows.Forms.DateTimePicker txtFechaHasta;
        protected System.Windows.Forms.Label label4;
        private System.Windows.Forms.GroupBox groupBox2;
        protected System.Windows.Forms.GroupBox groupCheque;
        protected System.Windows.Forms.TextBox txtNroCheque;
        private System.Windows.Forms.ComboBox comboEstadosFiltro;
        protected System.Windows.Forms.Label label8;
        private System.Windows.Forms.ComboBox comboBanco;
        protected System.Windows.Forms.Label label9;
        private System.Windows.Forms.CheckBox checkPropio;
        private System.Windows.Forms.CheckBox checkPropioFiltro;
        protected System.Windows.Forms.TextBox txtImporteCheque;
        protected System.Windows.Forms.Label label10;
        private System.Windows.Forms.Button btnNuevo;
        private System.Windows.Forms.Button btnEliminar;
        private System.Windows.Forms.Button btnModificar;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.TextBox txtObservaciones;
        private System.Windows.Forms.MaskedTextBox txtFechaEmision;
        protected System.Windows.Forms.Label label12;
        protected System.Windows.Forms.DateTimePicker txtFechaPago;
        protected System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox txtTitular;
        protected System.Windows.Forms.Label label13;
        protected System.Windows.Forms.Label label6;
        private System.Windows.Forms.Button btnGuardar;
        private System.Windows.Forms.ComboBox comboEstado;
        protected System.Windows.Forms.Label label15;
        private System.Windows.Forms.TextBox txtEntregadoA;
        protected System.Windows.Forms.Label label14;
        private System.Windows.Forms.TextBox txtRecibidoDe;
        protected System.Windows.Forms.Label label7;
        private System.Windows.Forms.Button btnCancelar;
        private System.Windows.Forms.TextBox txtUsuario;
        protected System.Windows.Forms.Label label16;
        protected System.Windows.Forms.Label label18;
        protected System.Windows.Forms.Label label17;
        protected internal System.Windows.Forms.Button btnBuscar;
        private System.Windows.Forms.Button btnObservaciones;
        protected System.Windows.Forms.Label lblChequeVence;
    }
}