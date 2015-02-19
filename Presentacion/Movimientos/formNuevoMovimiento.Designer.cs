namespace Presentacion
{
    partial class formNuevoMovimiento
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(formNuevoMovimiento));
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            this.btnGuardar = new System.Windows.Forms.Button();
            this.btnCancelar = new System.Windows.Forms.Button();
            this.pnlBuscar = new System.Windows.Forms.Panel();
            this.label6 = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.checkMantenerCodigo = new System.Windows.Forms.CheckBox();
            this.btnQuitar = new System.Windows.Forms.Button();
            this.btnAgregar = new System.Windows.Forms.Button();
            this.txtCantKgs = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.btnBuscar = new System.Windows.Forms.Button();
            this.txtCodigo = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.txtCorte = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.comboSucDestino = new System.Windows.Forms.ComboBox();
            this.comboSucOrigen = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.txtFechaMovimiento = new System.Windows.Forms.DateTimePicker();
            this.grillaCortesPorMovimiento = new System.Windows.Forms.DataGridView();
            this.idCorteMovimientodo = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.codigo = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.corte = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.cantKg = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.txtCantItems = new System.Windows.Forms.TextBox();
            this.label12 = new System.Windows.Forms.Label();
            this.txtTotalKg = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.txtObservaciones = new System.Windows.Forms.TextBox();
            this.lineShape1 = new Microsoft.VisualBasic.PowerPacks.LineShape();
            this.shapeContainer1 = new Microsoft.VisualBasic.PowerPacks.ShapeContainer();
            this.pnlBuscar.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.grillaCortesPorMovimiento)).BeginInit();
            this.SuspendLayout();
            // 
            // btnGuardar
            // 
            this.btnGuardar.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnGuardar.Location = new System.Drawing.Point(303, 498);
            this.btnGuardar.Name = "btnGuardar";
            this.btnGuardar.Size = new System.Drawing.Size(80, 28);
            this.btnGuardar.TabIndex = 19;
            this.btnGuardar.TabStop = false;
            this.btnGuardar.Text = "Guardar";
            this.btnGuardar.UseVisualStyleBackColor = true;
            this.btnGuardar.Click += new System.EventHandler(this.btnGuardar_Click);
            // 
            // btnCancelar
            // 
            this.btnCancelar.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancelar.Location = new System.Drawing.Point(389, 498);
            this.btnCancelar.Name = "btnCancelar";
            this.btnCancelar.Size = new System.Drawing.Size(78, 27);
            this.btnCancelar.TabIndex = 18;
            this.btnCancelar.TabStop = false;
            this.btnCancelar.Text = "Cancelar";
            this.btnCancelar.UseVisualStyleBackColor = true;
            this.btnCancelar.Click += new System.EventHandler(this.btnCancelar_Click);
            // 
            // pnlBuscar
            // 
            this.pnlBuscar.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.pnlBuscar.BackColor = System.Drawing.Color.SteelBlue;
            this.pnlBuscar.Controls.Add(this.label6);
            this.pnlBuscar.Controls.Add(this.groupBox2);
            this.pnlBuscar.Controls.Add(this.groupBox1);
            this.pnlBuscar.Controls.Add(this.txtFechaMovimiento);
            this.pnlBuscar.Location = new System.Drawing.Point(-1, 0);
            this.pnlBuscar.Name = "pnlBuscar";
            this.pnlBuscar.Size = new System.Drawing.Size(480, 199);
            this.pnlBuscar.TabIndex = 20;
            // 
            // label6
            // 
            this.label6.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.ForeColor = System.Drawing.Color.Cornsilk;
            this.label6.Location = new System.Drawing.Point(301, 17);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(41, 15);
            this.label6.TabIndex = 16;
            this.label6.Text = "Fecha";
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.groupBox2.BackColor = System.Drawing.Color.SteelBlue;
            this.groupBox2.Controls.Add(this.checkMantenerCodigo);
            this.groupBox2.Controls.Add(this.btnQuitar);
            this.groupBox2.Controls.Add(this.btnAgregar);
            this.groupBox2.Controls.Add(this.txtCantKgs);
            this.groupBox2.Controls.Add(this.label1);
            this.groupBox2.Controls.Add(this.btnBuscar);
            this.groupBox2.Controls.Add(this.txtCodigo);
            this.groupBox2.Controls.Add(this.label2);
            this.groupBox2.Controls.Add(this.txtCorte);
            this.groupBox2.Controls.Add(this.label5);
            this.groupBox2.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox2.ForeColor = System.Drawing.Color.Cornsilk;
            this.groupBox2.Location = new System.Drawing.Point(13, 105);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(455, 86);
            this.groupBox2.TabIndex = 14;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Corte";
            // 
            // checkMantenerCodigo
            // 
            this.checkMantenerCodigo.AutoSize = true;
            this.checkMantenerCodigo.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.checkMantenerCodigo.Location = new System.Drawing.Point(202, 26);
            this.checkMantenerCodigo.Name = "checkMantenerCodigo";
            this.checkMantenerCodigo.Size = new System.Drawing.Size(121, 19);
            this.checkMantenerCodigo.TabIndex = 20;
            this.checkMantenerCodigo.TabStop = false;
            this.checkMantenerCodigo.Text = "Mantener Código";
            this.checkMantenerCodigo.UseVisualStyleBackColor = true;
            this.checkMantenerCodigo.CheckedChanged += new System.EventHandler(this.checkMantenerCodigo_CheckedChanged);
            // 
            // btnQuitar
            // 
            this.btnQuitar.ForeColor = System.Drawing.Color.Black;
            this.btnQuitar.Image = ((System.Drawing.Image)(resources.GetObject("btnQuitar.Image")));
            this.btnQuitar.Location = new System.Drawing.Point(418, 50);
            this.btnQuitar.Name = "btnQuitar";
            this.btnQuitar.Size = new System.Drawing.Size(29, 24);
            this.btnQuitar.TabIndex = 19;
            this.btnQuitar.TabStop = false;
            this.btnQuitar.UseVisualStyleBackColor = true;
            this.btnQuitar.Click += new System.EventHandler(this.btnQuitar_Click);
            // 
            // btnAgregar
            // 
            this.btnAgregar.AccessibleDescription = "";
            this.btnAgregar.ForeColor = System.Drawing.Color.Black;
            this.btnAgregar.Image = ((System.Drawing.Image)(resources.GetObject("btnAgregar.Image")));
            this.btnAgregar.Location = new System.Drawing.Point(386, 50);
            this.btnAgregar.Name = "btnAgregar";
            this.btnAgregar.Size = new System.Drawing.Size(29, 24);
            this.btnAgregar.TabIndex = 3;
            this.btnAgregar.UseVisualStyleBackColor = true;
            this.btnAgregar.Click += new System.EventHandler(this.btnAgregar_Click);
            // 
            // txtCantKgs
            // 
            this.txtCantKgs.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.txtCantKgs.Location = new System.Drawing.Point(310, 52);
            this.txtCantKgs.Name = "txtCantKgs";
            this.txtCantKgs.Size = new System.Drawing.Size(68, 21);
            this.txtCantKgs.TabIndex = 2;
            this.txtCantKgs.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.txtCantKgs.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.TxtPruebaENTER_KeyPress);
            // 
            // label1
            // 
            this.label1.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ForeColor = System.Drawing.Color.Cornsilk;
            this.label1.Location = new System.Drawing.Point(246, 55);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(59, 15);
            this.label1.TabIndex = 16;
            this.label1.Text = "Cant. Kgs";
            // 
            // btnBuscar
            // 
            this.btnBuscar.AccessibleDescription = "";
            this.btnBuscar.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.btnBuscar.ForeColor = System.Drawing.Color.Black;
            this.btnBuscar.Image = ((System.Drawing.Image)(resources.GetObject("btnBuscar.Image")));
            this.btnBuscar.Location = new System.Drawing.Point(151, 22);
            this.btnBuscar.Name = "btnBuscar";
            this.btnBuscar.Size = new System.Drawing.Size(28, 24);
            this.btnBuscar.TabIndex = 15;
            this.btnBuscar.TabStop = false;
            this.btnBuscar.UseVisualStyleBackColor = true;
            this.btnBuscar.Click += new System.EventHandler(this.btnBuscar_Click);
            // 
            // txtCodigo
            // 
            this.txtCodigo.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.txtCodigo.Location = new System.Drawing.Point(94, 23);
            this.txtCodigo.Name = "txtCodigo";
            this.txtCodigo.Size = new System.Drawing.Size(53, 21);
            this.txtCodigo.TabIndex = 1;
            this.txtCodigo.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.txtCodigo.TextChanged += new System.EventHandler(this.txtCodigo_TextChanged);
            this.txtCodigo.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.TxtPruebaENTER_KeyPress);
            // 
            // label2
            // 
            this.label2.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.ForeColor = System.Drawing.Color.Cornsilk;
            this.label2.Location = new System.Drawing.Point(42, 26);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(46, 15);
            this.label2.TabIndex = 10;
            this.label2.Text = "Código";
            // 
            // txtCorte
            // 
            this.txtCorte.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.txtCorte.Location = new System.Drawing.Point(94, 52);
            this.txtCorte.Name = "txtCorte";
            this.txtCorte.ReadOnly = true;
            this.txtCorte.Size = new System.Drawing.Size(134, 21);
            this.txtCorte.TabIndex = 9;
            // 
            // label5
            // 
            this.label5.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.ForeColor = System.Drawing.Color.Cornsilk;
            this.label5.Location = new System.Drawing.Point(16, 55);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(72, 15);
            this.label5.TabIndex = 8;
            this.label5.Text = "Descripción";
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.BackColor = System.Drawing.Color.SteelBlue;
            this.groupBox1.Controls.Add(this.comboSucDestino);
            this.groupBox1.Controls.Add(this.comboSucOrigen);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox1.ForeColor = System.Drawing.Color.Cornsilk;
            this.groupBox1.Location = new System.Drawing.Point(13, 8);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(259, 89);
            this.groupBox1.TabIndex = 13;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Sucursales";
            // 
            // comboSucDestino
            // 
            this.comboSucDestino.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboSucDestino.FormattingEnabled = true;
            this.comboSucDestino.Location = new System.Drawing.Point(94, 56);
            this.comboSucDestino.Name = "comboSucDestino";
            this.comboSucDestino.Size = new System.Drawing.Size(132, 23);
            this.comboSucDestino.TabIndex = 12;
            this.comboSucDestino.TabStop = false;
            this.comboSucDestino.SelectedValueChanged += new System.EventHandler(this.comboSucDestino_SelectedValueChanged);
            // 
            // comboSucOrigen
            // 
            this.comboSucOrigen.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboSucOrigen.FormattingEnabled = true;
            this.comboSucOrigen.Location = new System.Drawing.Point(94, 24);
            this.comboSucOrigen.Name = "comboSucOrigen";
            this.comboSucOrigen.Size = new System.Drawing.Size(132, 23);
            this.comboSucOrigen.TabIndex = 11;
            this.comboSucOrigen.TabStop = false;
            this.comboSucOrigen.SelectedValueChanged += new System.EventHandler(this.comboSucOrigen_SelectedValueChanged);
            // 
            // label3
            // 
            this.label3.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.ForeColor = System.Drawing.Color.Cornsilk;
            this.label3.Location = new System.Drawing.Point(44, 27);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(44, 15);
            this.label3.TabIndex = 10;
            this.label3.Text = "Origen";
            // 
            // label4
            // 
            this.label4.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.ForeColor = System.Drawing.Color.Cornsilk;
            this.label4.Location = new System.Drawing.Point(39, 59);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(49, 15);
            this.label4.TabIndex = 8;
            this.label4.Text = "Destino";
            // 
            // txtFechaMovimiento
            // 
            this.txtFechaMovimiento.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.txtFechaMovimiento.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.txtFechaMovimiento.Location = new System.Drawing.Point(348, 16);
            this.txtFechaMovimiento.Name = "txtFechaMovimiento";
            this.txtFechaMovimiento.Size = new System.Drawing.Size(111, 20);
            this.txtFechaMovimiento.TabIndex = 15;
            this.txtFechaMovimiento.TabStop = false;
            // 
            // grillaCortesPorMovimiento
            // 
            this.grillaCortesPorMovimiento.AllowUserToAddRows = false;
            this.grillaCortesPorMovimiento.AllowUserToResizeColumns = false;
            this.grillaCortesPorMovimiento.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.grillaCortesPorMovimiento.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.grillaCortesPorMovimiento.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.idCorteMovimientodo,
            this.codigo,
            this.corte,
            this.cantKg});
            this.grillaCortesPorMovimiento.Location = new System.Drawing.Point(12, 205);
            this.grillaCortesPorMovimiento.MultiSelect = false;
            this.grillaCortesPorMovimiento.Name = "grillaCortesPorMovimiento";
            this.grillaCortesPorMovimiento.RowHeadersVisible = false;
            this.grillaCortesPorMovimiento.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.grillaCortesPorMovimiento.Size = new System.Drawing.Size(455, 233);
            this.grillaCortesPorMovimiento.TabIndex = 21;
            this.grillaCortesPorMovimiento.TabStop = false;
            // 
            // idCorteMovimientodo
            // 
            this.idCorteMovimientodo.DataPropertyName = "idCorteMovimiento";
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.TopRight;
            this.idCorteMovimientodo.DefaultCellStyle = dataGridViewCellStyle1;
            this.idCorteMovimientodo.HeaderText = "Id Corte Mov.";
            this.idCorteMovimientodo.MinimumWidth = 70;
            this.idCorteMovimientodo.Name = "idCorteMovimientodo";
            this.idCorteMovimientodo.ReadOnly = true;
            this.idCorteMovimientodo.Visible = false;
            // 
            // codigo
            // 
            this.codigo.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.ColumnHeader;
            this.codigo.DataPropertyName = "codigo";
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
            this.codigo.DefaultCellStyle = dataGridViewCellStyle2;
            this.codigo.FillWeight = 50F;
            this.codigo.HeaderText = "Código";
            this.codigo.MinimumWidth = 80;
            this.codigo.Name = "codigo";
            this.codigo.ReadOnly = true;
            this.codigo.Width = 80;
            // 
            // corte
            // 
            this.corte.DataPropertyName = "corte";
            this.corte.FillWeight = 89.0863F;
            this.corte.HeaderText = "Corte";
            this.corte.Name = "corte";
            this.corte.ReadOnly = true;
            // 
            // cantKg
            // 
            this.cantKg.DataPropertyName = "cantKg";
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
            dataGridViewCellStyle3.Format = "N3";
            dataGridViewCellStyle3.NullValue = null;
            this.cantKg.DefaultCellStyle = dataGridViewCellStyle3;
            this.cantKg.FillWeight = 55F;
            this.cantKg.HeaderText = "Cant. Kgs";
            this.cantKg.Name = "cantKg";
            this.cantKg.ReadOnly = true;
            // 
            // txtCantItems
            // 
            this.txtCantItems.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.txtCantItems.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtCantItems.Location = new System.Drawing.Point(389, 444);
            this.txtCantItems.Name = "txtCantItems";
            this.txtCantItems.ReadOnly = true;
            this.txtCantItems.Size = new System.Drawing.Size(78, 21);
            this.txtCantItems.TabIndex = 38;
            this.txtCantItems.TabStop = false;
            this.txtCantItems.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label12
            // 
            this.label12.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.label12.AutoSize = true;
            this.label12.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label12.Location = new System.Drawing.Point(300, 449);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(79, 15);
            this.label12.TabIndex = 37;
            this.label12.Text = "Cant. Items";
            // 
            // txtTotalKg
            // 
            this.txtTotalKg.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.txtTotalKg.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtTotalKg.Location = new System.Drawing.Point(389, 467);
            this.txtTotalKg.Name = "txtTotalKg";
            this.txtTotalKg.ReadOnly = true;
            this.txtTotalKg.Size = new System.Drawing.Size(78, 21);
            this.txtTotalKg.TabIndex = 36;
            this.txtTotalKg.TabStop = false;
            this.txtTotalKg.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label8
            // 
            this.label8.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.label8.AutoSize = true;
            this.label8.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label8.Location = new System.Drawing.Point(319, 470);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(60, 15);
            this.label8.TabIndex = 35;
            this.label8.Text = "Total Kg";
            // 
            // label11
            // 
            this.label11.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(10, 440);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(78, 13);
            this.label11.TabIndex = 40;
            this.label11.Text = "Observaciones";
            // 
            // txtObservaciones
            // 
            this.txtObservaciones.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.txtObservaciones.Location = new System.Drawing.Point(13, 455);
            this.txtObservaciones.Multiline = true;
            this.txtObservaciones.Name = "txtObservaciones";
            this.txtObservaciones.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtObservaciones.Size = new System.Drawing.Size(225, 33);
            this.txtObservaciones.TabIndex = 39;
            this.txtObservaciones.TabStop = false;
            // 
            // lineShape1
            // 
            this.lineShape1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.lineShape1.Name = "lineShape1";
            this.lineShape1.X1 = 6;
            this.lineShape1.X2 = 469;
            this.lineShape1.Y1 = 493;
            this.lineShape1.Y2 = 493;
            // 
            // shapeContainer1
            // 
            this.shapeContainer1.Location = new System.Drawing.Point(0, 0);
            this.shapeContainer1.Margin = new System.Windows.Forms.Padding(0);
            this.shapeContainer1.Name = "shapeContainer1";
            this.shapeContainer1.Shapes.AddRange(new Microsoft.VisualBasic.PowerPacks.Shape[] {
            this.lineShape1});
            this.shapeContainer1.Size = new System.Drawing.Size(479, 529);
            this.shapeContainer1.TabIndex = 41;
            this.shapeContainer1.TabStop = false;
            // 
            // formNuevoMovimiento
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(479, 529);
            this.Controls.Add(this.label11);
            this.Controls.Add(this.txtObservaciones);
            this.Controls.Add(this.txtCantItems);
            this.Controls.Add(this.label12);
            this.Controls.Add(this.txtTotalKg);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.grillaCortesPorMovimiento);
            this.Controls.Add(this.pnlBuscar);
            this.Controls.Add(this.btnGuardar);
            this.Controls.Add(this.btnCancelar);
            this.Controls.Add(this.shapeContainer1);
            this.MaximizeBox = false;
            this.MinimizeBox = true;
            this.Name = "formNuevoMovimiento";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Nuevo Movimiento";
            this.pnlBuscar.ResumeLayout(false);
            this.pnlBuscar.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.grillaCortesPorMovimiento)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        protected System.Windows.Forms.Button btnGuardar;
        protected System.Windows.Forms.Button btnCancelar;
        protected System.Windows.Forms.Panel pnlBuscar;
        protected System.Windows.Forms.Label label6;
        protected System.Windows.Forms.GroupBox groupBox2;
        protected System.Windows.Forms.TextBox txtCantKgs;
        protected System.Windows.Forms.Label label1;
        protected internal System.Windows.Forms.Button btnBuscar;
        protected System.Windows.Forms.TextBox txtCodigo;
        protected System.Windows.Forms.Label label2;
        protected System.Windows.Forms.TextBox txtCorte;
        protected System.Windows.Forms.Label label5;
        protected System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.ComboBox comboSucDestino;
        private System.Windows.Forms.ComboBox comboSucOrigen;
        protected System.Windows.Forms.Label label3;
        protected System.Windows.Forms.Label label4;
        private System.Windows.Forms.DateTimePicker txtFechaMovimiento;
        private System.Windows.Forms.Button btnQuitar;
        private System.Windows.Forms.Button btnAgregar;
        private System.Windows.Forms.DataGridView grillaCortesPorMovimiento;
        private System.Windows.Forms.TextBox txtCantItems;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.TextBox txtTotalKg;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.TextBox txtObservaciones;
        private Microsoft.VisualBasic.PowerPacks.LineShape lineShape1;
        private Microsoft.VisualBasic.PowerPacks.ShapeContainer shapeContainer1;
        private System.Windows.Forms.DataGridViewTextBoxColumn idCorteMovimientodo;
        private System.Windows.Forms.DataGridViewTextBoxColumn codigo;
        private System.Windows.Forms.DataGridViewTextBoxColumn corte;
        private System.Windows.Forms.DataGridViewTextBoxColumn cantKg;
        private System.Windows.Forms.CheckBox checkMantenerCodigo;
    }
}