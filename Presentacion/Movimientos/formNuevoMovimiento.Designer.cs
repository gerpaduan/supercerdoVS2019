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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(formNuevoMovimiento));
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle5 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
            this.btnGuardar = new System.Windows.Forms.Button();
            this.btnCancelar = new System.Windows.Forms.Button();
            this.pnlBuscar = new System.Windows.Forms.Panel();
            this.txtCreado = new System.Windows.Forms.TextBox();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.txtHora = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.txtCantUnidad = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.checkLeerPeso = new System.Windows.Forms.CheckBox();
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
            this.cantUnidad = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.cantKg = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Balanza = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.txtCantItems = new System.Windows.Forms.TextBox();
            this.label12 = new System.Windows.Forms.Label();
            this.txtTotalKg = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.txtObservaciones = new System.Windows.Forms.TextBox();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.txtCantTotUni = new System.Windows.Forms.TextBox();
            this.label10 = new System.Windows.Forms.Label();
            this.pnlBuscar.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.grillaCortesPorMovimiento)).BeginInit();
            this.SuspendLayout();
            // 
            // btnGuardar
            // 
            this.btnGuardar.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.btnGuardar.BackColor = System.Drawing.Color.SeaGreen;
            this.btnGuardar.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Bold);
            this.btnGuardar.ForeColor = System.Drawing.SystemColors.ControlLight;
            this.btnGuardar.Location = new System.Drawing.Point(484, 575);
            this.btnGuardar.Name = "btnGuardar";
            this.btnGuardar.Size = new System.Drawing.Size(130, 37);
            this.btnGuardar.TabIndex = 19;
            this.btnGuardar.TabStop = false;
            this.btnGuardar.Text = "Guardar";
            this.btnGuardar.UseVisualStyleBackColor = false;
            this.btnGuardar.Click += new System.EventHandler(this.btnGuardar_Click);
            this.btnGuardar.Leave += new System.EventHandler(this.btnGuardar_Leave);
            this.btnGuardar.Enter += new System.EventHandler(this.btnGuardar_Enter);
            // 
            // btnCancelar
            // 
            this.btnCancelar.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.btnCancelar.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.btnCancelar.Location = new System.Drawing.Point(484, 614);
            this.btnCancelar.Name = "btnCancelar";
            this.btnCancelar.Size = new System.Drawing.Size(130, 26);
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
            this.pnlBuscar.Controls.Add(this.txtCreado);
            this.pnlBuscar.Controls.Add(this.textBox1);
            this.pnlBuscar.Controls.Add(this.label7);
            this.pnlBuscar.Controls.Add(this.txtHora);
            this.pnlBuscar.Controls.Add(this.label6);
            this.pnlBuscar.Controls.Add(this.groupBox2);
            this.pnlBuscar.Controls.Add(this.groupBox1);
            this.pnlBuscar.Controls.Add(this.txtFechaMovimiento);
            this.pnlBuscar.Location = new System.Drawing.Point(0, -1);
            this.pnlBuscar.Name = "pnlBuscar";
            this.pnlBuscar.Size = new System.Drawing.Size(627, 199);
            this.pnlBuscar.TabIndex = 20;
            // 
            // txtCreado
            // 
            this.txtCreado.Location = new System.Drawing.Point(411, 44);
            this.txtCreado.Multiline = true;
            this.txtCreado.Name = "txtCreado";
            this.txtCreado.ReadOnly = true;
            this.txtCreado.Size = new System.Drawing.Size(204, 53);
            this.txtCreado.TabIndex = 55;
            this.txtCreado.TabStop = false;
            this.txtCreado.Visible = false;
            // 
            // textBox1
            // 
            this.textBox1.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.textBox1.Location = new System.Drawing.Point(309, 43);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(100, 20);
            this.textBox1.TabIndex = 19;
            this.textBox1.Visible = false;
            // 
            // label7
            // 
            this.label7.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label7.ForeColor = System.Drawing.Color.Cornsilk;
            this.label7.Location = new System.Drawing.Point(289, 18);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(34, 15);
            this.label7.TabIndex = 18;
            this.label7.Text = "Hora";
            this.label7.Visible = false;
            // 
            // txtHora
            // 
            this.txtHora.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.txtHora.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtHora.Location = new System.Drawing.Point(329, 15);
            this.txtHora.Name = "txtHora";
            this.txtHora.Size = new System.Drawing.Size(80, 22);
            this.txtHora.TabIndex = 17;
            this.txtHora.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.txtHora.Visible = false;
            // 
            // label6
            // 
            this.label6.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.ForeColor = System.Drawing.Color.Cornsilk;
            this.label6.Location = new System.Drawing.Point(408, 18);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(41, 15);
            this.label6.TabIndex = 16;
            this.label6.Text = "Fecha";
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.groupBox2.BackColor = System.Drawing.Color.SteelBlue;
            this.groupBox2.Controls.Add(this.txtCantUnidad);
            this.groupBox2.Controls.Add(this.label9);
            this.groupBox2.Controls.Add(this.checkLeerPeso);
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
            this.groupBox2.Location = new System.Drawing.Point(12, 105);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(603, 86);
            this.groupBox2.TabIndex = 14;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Corte";
            // 
            // txtCantUnidad
            // 
            this.txtCantUnidad.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.txtCantUnidad.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtCantUnidad.Location = new System.Drawing.Point(327, 52);
            this.txtCantUnidad.Name = "txtCantUnidad";
            this.txtCantUnidad.Size = new System.Drawing.Size(61, 22);
            this.txtCantUnidad.TabIndex = 2;
            this.txtCantUnidad.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.txtCantUnidad.TextChanged += new System.EventHandler(this.txtCantUnidad_TextChanged);
            this.txtCantUnidad.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtCantUnidad_KeyPress);
            // 
            // label9
            // 
            this.label9.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.label9.AutoSize = true;
            this.label9.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label9.ForeColor = System.Drawing.Color.Cornsilk;
            this.label9.Location = new System.Drawing.Point(264, 55);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(57, 15);
            this.label9.TabIndex = 22;
            this.label9.Text = "Cant. Un.";
            // 
            // checkLeerPeso
            // 
            this.checkLeerPeso.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.checkLeerPeso.AutoSize = true;
            this.checkLeerPeso.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.checkLeerPeso.Checked = true;
            this.checkLeerPeso.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkLeerPeso.Location = new System.Drawing.Point(224, 25);
            this.checkLeerPeso.Name = "checkLeerPeso";
            this.checkLeerPeso.Size = new System.Drawing.Size(130, 19);
            this.checkLeerPeso.TabIndex = 20;
            this.checkLeerPeso.TabStop = false;
            this.checkLeerPeso.Text = "Leer Peso Balanza";
            this.checkLeerPeso.UseVisualStyleBackColor = true;
            this.checkLeerPeso.CheckedChanged += new System.EventHandler(this.checkLeerPeso_CheckedChanged);
            // 
            // btnQuitar
            // 
            this.btnQuitar.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.btnQuitar.ForeColor = System.Drawing.Color.Black;
            this.btnQuitar.Image = ((System.Drawing.Image)(resources.GetObject("btnQuitar.Image")));
            this.btnQuitar.Location = new System.Drawing.Point(569, 51);
            this.btnQuitar.Name = "btnQuitar";
            this.btnQuitar.Size = new System.Drawing.Size(28, 25);
            this.btnQuitar.TabIndex = 19;
            this.btnQuitar.TabStop = false;
            this.btnQuitar.UseVisualStyleBackColor = true;
            this.btnQuitar.Click += new System.EventHandler(this.btnQuitar_Click);
            // 
            // btnAgregar
            // 
            this.btnAgregar.AccessibleDescription = "";
            this.btnAgregar.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.btnAgregar.ForeColor = System.Drawing.Color.Black;
            this.btnAgregar.Image = ((System.Drawing.Image)(resources.GetObject("btnAgregar.Image")));
            this.btnAgregar.Location = new System.Drawing.Point(532, 51);
            this.btnAgregar.Name = "btnAgregar";
            this.btnAgregar.Size = new System.Drawing.Size(35, 25);
            this.btnAgregar.TabIndex = 4;
            this.btnAgregar.UseVisualStyleBackColor = true;
            this.btnAgregar.Click += new System.EventHandler(this.btnAgregar_Click);
            // 
            // txtCantKgs
            // 
            this.txtCantKgs.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.txtCantKgs.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtCantKgs.Location = new System.Drawing.Point(458, 52);
            this.txtCantKgs.Name = "txtCantKgs";
            this.txtCantKgs.ReadOnly = true;
            this.txtCantKgs.Size = new System.Drawing.Size(68, 22);
            this.txtCantKgs.TabIndex = 3;
            this.txtCantKgs.TabStop = false;
            this.txtCantKgs.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.txtCantKgs.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.TxtPruebaENTER_KeyPress);
            // 
            // label1
            // 
            this.label1.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ForeColor = System.Drawing.Color.Cornsilk;
            this.label1.Location = new System.Drawing.Point(394, 55);
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
            this.btnBuscar.Location = new System.Drawing.Point(146, 22);
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
            this.txtCodigo.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtCodigo.Location = new System.Drawing.Point(89, 23);
            this.txtCodigo.Name = "txtCodigo";
            this.txtCodigo.Size = new System.Drawing.Size(53, 22);
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
            this.label2.Location = new System.Drawing.Point(37, 26);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(46, 15);
            this.label2.TabIndex = 10;
            this.label2.Text = "Código";
            // 
            // txtCorte
            // 
            this.txtCorte.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.txtCorte.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtCorte.Location = new System.Drawing.Point(89, 52);
            this.txtCorte.Name = "txtCorte";
            this.txtCorte.ReadOnly = true;
            this.txtCorte.Size = new System.Drawing.Size(169, 22);
            this.txtCorte.TabIndex = 9;
            // 
            // label5
            // 
            this.label5.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.ForeColor = System.Drawing.Color.Cornsilk;
            this.label5.Location = new System.Drawing.Point(11, 55);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(72, 15);
            this.label5.TabIndex = 8;
            this.label5.Text = "Descripción";
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.groupBox1.BackColor = System.Drawing.Color.SteelBlue;
            this.groupBox1.Controls.Add(this.comboSucDestino);
            this.groupBox1.Controls.Add(this.comboSucOrigen);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox1.ForeColor = System.Drawing.Color.Cornsilk;
            this.groupBox1.Location = new System.Drawing.Point(12, 8);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(327, 89);
            this.groupBox1.TabIndex = 13;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Sucursales";
            // 
            // comboSucDestino
            // 
            this.comboSucDestino.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboSucDestino.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.comboSucDestino.FormattingEnabled = true;
            this.comboSucDestino.Location = new System.Drawing.Point(92, 56);
            this.comboSucDestino.Name = "comboSucDestino";
            this.comboSucDestino.Size = new System.Drawing.Size(169, 24);
            this.comboSucDestino.TabIndex = 12;
            this.comboSucDestino.TabStop = false;
            this.comboSucDestino.SelectedValueChanged += new System.EventHandler(this.comboSucDestino_SelectedValueChanged);
            // 
            // comboSucOrigen
            // 
            this.comboSucOrigen.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboSucOrigen.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.comboSucOrigen.FormattingEnabled = true;
            this.comboSucOrigen.Location = new System.Drawing.Point(92, 24);
            this.comboSucOrigen.Name = "comboSucOrigen";
            this.comboSucOrigen.Size = new System.Drawing.Size(169, 24);
            this.comboSucOrigen.TabIndex = 11;
            this.comboSucOrigen.TabStop = false;
            this.comboSucOrigen.SelectedIndexChanged += new System.EventHandler(this.comboSucOrigen_SelectedIndexChanged);
            this.comboSucOrigen.SelectedValueChanged += new System.EventHandler(this.comboSucOrigen_SelectedValueChanged);
            // 
            // label3
            // 
            this.label3.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.ForeColor = System.Drawing.Color.Cornsilk;
            this.label3.Location = new System.Drawing.Point(30, 27);
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
            this.label4.Location = new System.Drawing.Point(25, 59);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(49, 15);
            this.label4.TabIndex = 8;
            this.label4.Text = "Destino";
            // 
            // txtFechaMovimiento
            // 
            this.txtFechaMovimiento.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.txtFechaMovimiento.CustomFormat = "dd/MM/yyyy H:mm";
            this.txtFechaMovimiento.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtFechaMovimiento.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.txtFechaMovimiento.Location = new System.Drawing.Point(455, 16);
            this.txtFechaMovimiento.Name = "txtFechaMovimiento";
            this.txtFechaMovimiento.Size = new System.Drawing.Size(160, 22);
            this.txtFechaMovimiento.TabIndex = 15;
            this.txtFechaMovimiento.TabStop = false;
            this.txtFechaMovimiento.ValueChanged += new System.EventHandler(this.txtFechaMovimiento_ValueChanged);
            // 
            // grillaCortesPorMovimiento
            // 
            this.grillaCortesPorMovimiento.AllowUserToAddRows = false;
            this.grillaCortesPorMovimiento.AllowUserToResizeColumns = false;
            this.grillaCortesPorMovimiento.AllowUserToResizeRows = false;
            this.grillaCortesPorMovimiento.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.grillaCortesPorMovimiento.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.grillaCortesPorMovimiento.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.grillaCortesPorMovimiento.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.idCorteMovimientodo,
            this.codigo,
            this.corte,
            this.cantUnidad,
            this.cantKg,
            this.Balanza});
            dataGridViewCellStyle5.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle5.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle5.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle5.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle5.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle5.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle5.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.grillaCortesPorMovimiento.DefaultCellStyle = dataGridViewCellStyle5;
            this.grillaCortesPorMovimiento.Location = new System.Drawing.Point(12, 204);
            this.grillaCortesPorMovimiento.MultiSelect = false;
            this.grillaCortesPorMovimiento.Name = "grillaCortesPorMovimiento";
            this.grillaCortesPorMovimiento.RowHeadersVisible = false;
            this.grillaCortesPorMovimiento.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.grillaCortesPorMovimiento.Size = new System.Drawing.Size(461, 437);
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
            // cantUnidad
            // 
            this.cantUnidad.DataPropertyName = "cantUnidad";
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
            this.cantUnidad.DefaultCellStyle = dataGridViewCellStyle3;
            this.cantUnidad.FillWeight = 50F;
            this.cantUnidad.HeaderText = "Cant. Un.";
            this.cantUnidad.Name = "cantUnidad";
            this.cantUnidad.ReadOnly = true;
            // 
            // cantKg
            // 
            this.cantKg.DataPropertyName = "cantKg";
            dataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
            dataGridViewCellStyle4.Format = "N3";
            dataGridViewCellStyle4.NullValue = null;
            this.cantKg.DefaultCellStyle = dataGridViewCellStyle4;
            this.cantKg.FillWeight = 55F;
            this.cantKg.HeaderText = "Cant. Kgs";
            this.cantKg.Name = "cantKg";
            this.cantKg.ReadOnly = true;
            // 
            // Balanza
            // 
            this.Balanza.DataPropertyName = "pesoBalanza";
            this.Balanza.FillWeight = 30F;
            this.Balanza.HeaderText = "Balanza";
            this.Balanza.Name = "Balanza";
            this.Balanza.ReadOnly = true;
            // 
            // txtCantItems
            // 
            this.txtCantItems.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.txtCantItems.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtCantItems.Location = new System.Drawing.Point(484, 310);
            this.txtCantItems.Name = "txtCantItems";
            this.txtCantItems.ReadOnly = true;
            this.txtCantItems.Size = new System.Drawing.Size(131, 22);
            this.txtCantItems.TabIndex = 38;
            this.txtCantItems.TabStop = false;
            this.txtCantItems.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label12
            // 
            this.label12.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.label12.AutoSize = true;
            this.label12.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label12.Location = new System.Drawing.Point(481, 292);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(79, 15);
            this.label12.TabIndex = 37;
            this.label12.Text = "Cant. Items";
            // 
            // txtTotalKg
            // 
            this.txtTotalKg.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.txtTotalKg.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtTotalKg.Location = new System.Drawing.Point(485, 267);
            this.txtTotalKg.Name = "txtTotalKg";
            this.txtTotalKg.ReadOnly = true;
            this.txtTotalKg.Size = new System.Drawing.Size(130, 22);
            this.txtTotalKg.TabIndex = 36;
            this.txtTotalKg.TabStop = false;
            this.txtTotalKg.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label8
            // 
            this.label8.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.label8.AutoSize = true;
            this.label8.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label8.Location = new System.Drawing.Point(482, 249);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(60, 15);
            this.label8.TabIndex = 35;
            this.label8.Text = "Total Kg";
            // 
            // label11
            // 
            this.label11.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(482, 349);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(78, 13);
            this.label11.TabIndex = 40;
            this.label11.Text = "Observaciones";
            // 
            // txtObservaciones
            // 
            this.txtObservaciones.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.txtObservaciones.Location = new System.Drawing.Point(485, 365);
            this.txtObservaciones.Multiline = true;
            this.txtObservaciones.Name = "txtObservaciones";
            this.txtObservaciones.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtObservaciones.Size = new System.Drawing.Size(130, 197);
            this.txtObservaciones.TabIndex = 39;
            this.txtObservaciones.TabStop = false;
            // 
            // timer1
            // 
            this.timer1.Enabled = true;
            this.timer1.Interval = 10;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // txtCantTotUni
            // 
            this.txtCantTotUni.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.txtCantTotUni.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtCantTotUni.Location = new System.Drawing.Point(484, 224);
            this.txtCantTotUni.Name = "txtCantTotUni";
            this.txtCantTotUni.ReadOnly = true;
            this.txtCantTotUni.Size = new System.Drawing.Size(131, 22);
            this.txtCantTotUni.TabIndex = 42;
            this.txtCantTotUni.TabStop = false;
            this.txtCantTotUni.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label10
            // 
            this.label10.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.label10.AutoSize = true;
            this.label10.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label10.Location = new System.Drawing.Point(481, 206);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(105, 15);
            this.label10.TabIndex = 41;
            this.label10.Text = "Cant. Unidades";
            // 
            // formNuevoMovimiento
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(627, 647);
            this.Controls.Add(this.txtCantTotUni);
            this.Controls.Add(this.label10);
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
            this.MaximizeBox = false;
            this.MinimizeBox = true;
            this.Name = "formNuevoMovimiento";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Nuevo Movimiento";
            this.Load += new System.EventHandler(this.formNuevoMovimiento_Load);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.formNuevoMovimiento_FormClosing);
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
        private System.Windows.Forms.CheckBox checkLeerPeso;
        protected System.Windows.Forms.Label label7;
        protected System.Windows.Forms.TextBox txtHora;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.TextBox textBox1;
        protected System.Windows.Forms.TextBox txtCantUnidad;
        protected System.Windows.Forms.Label label9;
        private System.Windows.Forms.TextBox txtCantTotUni;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.DataGridViewTextBoxColumn idCorteMovimientodo;
        private System.Windows.Forms.DataGridViewTextBoxColumn codigo;
        private System.Windows.Forms.DataGridViewTextBoxColumn corte;
        private System.Windows.Forms.DataGridViewTextBoxColumn cantUnidad;
        private System.Windows.Forms.DataGridViewTextBoxColumn cantKg;
        private System.Windows.Forms.DataGridViewCheckBoxColumn Balanza;
        private System.Windows.Forms.TextBox txtCreado;
    }
}