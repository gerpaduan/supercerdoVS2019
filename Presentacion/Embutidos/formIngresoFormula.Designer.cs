namespace Presentacion
{
    partial class formIngresoFormula
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle26 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle29 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle30 = new System.Windows.Forms.DataGridViewCellStyle();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(formIngresoFormula));
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle28 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle27 = new System.Windows.Forms.DataGridViewCellStyle();
            this.btnGuardar = new System.Windows.Forms.Button();
            this.btnCancelar = new System.Windows.Forms.Button();
            this.grillaCortesPorEmbutido = new System.Windows.Forms.DataGridView();
            this.pnlBuscar = new System.Windows.Forms.Panel();
            this.txtUsuario = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.groupBoxCortesFormula = new System.Windows.Forms.GroupBox();
            this.checkAgregarAuto = new System.Windows.Forms.CheckBox();
            this.label8 = new System.Windows.Forms.Label();
            this.label18 = new System.Windows.Forms.Label();
            this.btnQuitar = new System.Windows.Forms.Button();
            this.btnAgregar = new System.Windows.Forms.Button();
            this.txtPorcentaje = new System.Windows.Forms.TextBox();
            this.btnBuscarCorte = new System.Windows.Forms.Button();
            this.txtCodCorteEnFormula = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.txtCorteEnFormula = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.groupBoxFormula = new System.Windows.Forms.GroupBox();
            this.label17 = new System.Windows.Forms.Label();
            this.btnBuscarEmbutido = new System.Windows.Forms.Button();
            this.txtCodigoEmbutido = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.txtEmbutido = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.label6 = new System.Windows.Forms.Label();
            this.txtActualizadoPor = new System.Windows.Forms.TextBox();
            this.txtActualizado = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.txtCreadoPor = new System.Windows.Forms.TextBox();
            this.txtCreado = new System.Windows.Forms.TextBox();
            this.lblError = new System.Windows.Forms.Label();
            this.agregarAuto = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.porcentaje = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.corte = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.codigo = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.idCorte = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.grillaCortesPorEmbutido)).BeginInit();
            this.pnlBuscar.SuspendLayout();
            this.groupBoxCortesFormula.SuspendLayout();
            this.groupBoxFormula.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnGuardar
            // 
            this.btnGuardar.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnGuardar.BackColor = System.Drawing.Color.SeaGreen;
            this.btnGuardar.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold);
            this.btnGuardar.ForeColor = System.Drawing.SystemColors.ControlLight;
            this.btnGuardar.Location = new System.Drawing.Point(539, 489);
            this.btnGuardar.Margin = new System.Windows.Forms.Padding(4);
            this.btnGuardar.Name = "btnGuardar";
            this.btnGuardar.Size = new System.Drawing.Size(195, 50);
            this.btnGuardar.TabIndex = 6;
            this.btnGuardar.TabStop = false;
            this.btnGuardar.Text = "&Guardar";
            this.btnGuardar.UseVisualStyleBackColor = false;
            this.btnGuardar.Click += new System.EventHandler(this.btnGuardar_Click);
            // 
            // btnCancelar
            // 
            this.btnCancelar.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancelar.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnCancelar.Location = new System.Drawing.Point(539, 547);
            this.btnCancelar.Margin = new System.Windows.Forms.Padding(4);
            this.btnCancelar.Name = "btnCancelar";
            this.btnCancelar.Size = new System.Drawing.Size(195, 33);
            this.btnCancelar.TabIndex = 7;
            this.btnCancelar.TabStop = false;
            this.btnCancelar.Text = "&Cancelar";
            this.btnCancelar.UseVisualStyleBackColor = true;
            this.btnCancelar.Click += new System.EventHandler(this.btnCancelar_Click);
            // 
            // grillaCortesPorEmbutido
            // 
            this.grillaCortesPorEmbutido.AllowUserToAddRows = false;
            this.grillaCortesPorEmbutido.AllowUserToResizeRows = false;
            this.grillaCortesPorEmbutido.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.grillaCortesPorEmbutido.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            dataGridViewCellStyle26.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle26.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle26.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle26.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle26.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle26.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle26.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.grillaCortesPorEmbutido.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle26;
            this.grillaCortesPorEmbutido.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.grillaCortesPorEmbutido.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.idCorte,
            this.codigo,
            this.corte,
            this.porcentaje,
            this.agregarAuto});
            dataGridViewCellStyle29.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle29.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle29.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle29.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle29.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle29.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle29.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.grillaCortesPorEmbutido.DefaultCellStyle = dataGridViewCellStyle29;
            this.grillaCortesPorEmbutido.Location = new System.Drawing.Point(15, 315);
            this.grillaCortesPorEmbutido.Margin = new System.Windows.Forms.Padding(4);
            this.grillaCortesPorEmbutido.MultiSelect = false;
            this.grillaCortesPorEmbutido.Name = "grillaCortesPorEmbutido";
            this.grillaCortesPorEmbutido.ReadOnly = true;
            this.grillaCortesPorEmbutido.RowHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
            dataGridViewCellStyle30.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle30.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle30.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle30.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle30.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle30.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle30.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.grillaCortesPorEmbutido.RowHeadersDefaultCellStyle = dataGridViewCellStyle30;
            this.grillaCortesPorEmbutido.RowHeadersVisible = false;
            this.grillaCortesPorEmbutido.RowHeadersWidth = 51;
            this.grillaCortesPorEmbutido.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.grillaCortesPorEmbutido.Size = new System.Drawing.Size(516, 265);
            this.grillaCortesPorEmbutido.StandardTab = true;
            this.grillaCortesPorEmbutido.TabIndex = 15;
            this.grillaCortesPorEmbutido.TabStop = false;
            // 
            // pnlBuscar
            // 
            this.pnlBuscar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pnlBuscar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(43)))), ((int)(((byte)(77)))), ((int)(((byte)(129)))));
            this.pnlBuscar.Controls.Add(this.txtUsuario);
            this.pnlBuscar.Controls.Add(this.label7);
            this.pnlBuscar.Controls.Add(this.groupBox3);
            this.pnlBuscar.Controls.Add(this.groupBoxCortesFormula);
            this.pnlBuscar.Controls.Add(this.groupBoxFormula);
            this.pnlBuscar.Location = new System.Drawing.Point(0, 0);
            this.pnlBuscar.Margin = new System.Windows.Forms.Padding(4);
            this.pnlBuscar.Name = "pnlBuscar";
            this.pnlBuscar.Size = new System.Drawing.Size(749, 307);
            this.pnlBuscar.TabIndex = 14;
            // 
            // txtUsuario
            // 
            this.txtUsuario.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.txtUsuario.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtUsuario.Location = new System.Drawing.Point(548, 13);
            this.txtUsuario.Margin = new System.Windows.Forms.Padding(4);
            this.txtUsuario.Name = "txtUsuario";
            this.txtUsuario.ReadOnly = true;
            this.txtUsuario.Size = new System.Drawing.Size(186, 26);
            this.txtUsuario.TabIndex = 28;
            this.txtUsuario.TabStop = false;
            // 
            // label7
            // 
            this.label7.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label7.ForeColor = System.Drawing.Color.Cornsilk;
            this.label7.Location = new System.Drawing.Point(480, 17);
            this.label7.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(60, 18);
            this.label7.TabIndex = 27;
            this.label7.Text = "Usuario";
            // 
            // groupBox3
            // 
            this.groupBox3.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.groupBox3.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.groupBox3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.groupBox3.Location = new System.Drawing.Point(16, 39);
            this.groupBox3.Margin = new System.Windows.Forms.Padding(4);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Padding = new System.Windows.Forms.Padding(4);
            this.groupBox3.Size = new System.Drawing.Size(849, 9);
            this.groupBox3.TabIndex = 26;
            this.groupBox3.TabStop = false;
            // 
            // groupBoxCortesFormula
            // 
            this.groupBoxCortesFormula.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.groupBoxCortesFormula.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(43)))), ((int)(((byte)(77)))), ((int)(((byte)(129)))));
            this.groupBoxCortesFormula.Controls.Add(this.checkAgregarAuto);
            this.groupBoxCortesFormula.Controls.Add(this.label8);
            this.groupBoxCortesFormula.Controls.Add(this.label18);
            this.groupBoxCortesFormula.Controls.Add(this.btnQuitar);
            this.groupBoxCortesFormula.Controls.Add(this.btnAgregar);
            this.groupBoxCortesFormula.Controls.Add(this.txtPorcentaje);
            this.groupBoxCortesFormula.Controls.Add(this.btnBuscarCorte);
            this.groupBoxCortesFormula.Controls.Add(this.txtCodCorteEnFormula);
            this.groupBoxCortesFormula.Controls.Add(this.label2);
            this.groupBoxCortesFormula.Controls.Add(this.txtCorteEnFormula);
            this.groupBoxCortesFormula.Controls.Add(this.label5);
            this.groupBoxCortesFormula.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBoxCortesFormula.ForeColor = System.Drawing.Color.Cornsilk;
            this.groupBoxCortesFormula.Location = new System.Drawing.Point(21, 176);
            this.groupBoxCortesFormula.Margin = new System.Windows.Forms.Padding(4);
            this.groupBoxCortesFormula.Name = "groupBoxCortesFormula";
            this.groupBoxCortesFormula.Padding = new System.Windows.Forms.Padding(4);
            this.groupBoxCortesFormula.Size = new System.Drawing.Size(715, 106);
            this.groupBoxCortesFormula.TabIndex = 10;
            this.groupBoxCortesFormula.TabStop = false;
            this.groupBoxCortesFormula.Text = "Cortes de Fórmula";
            // 
            // checkAgregarAuto
            // 
            this.checkAgregarAuto.AutoSize = true;
            this.checkAgregarAuto.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.checkAgregarAuto.Checked = true;
            this.checkAgregarAuto.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkAgregarAuto.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.checkAgregarAuto.Location = new System.Drawing.Point(466, 67);
            this.checkAgregarAuto.Margin = new System.Windows.Forms.Padding(4);
            this.checkAgregarAuto.Name = "checkAgregarAuto";
            this.checkAgregarAuto.Size = new System.Drawing.Size(119, 22);
            this.checkAgregarAuto.TabIndex = 4;
            this.checkAgregarAuto.Text = "Agregar Auto.";
            this.checkAgregarAuto.UseVisualStyleBackColor = true;
            this.checkAgregarAuto.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.TxtPruebaENTER_KeyPress);
            this.checkAgregarAuto.Leave += new System.EventHandler(this.control_Leave);
            // 
            // label8
            // 
            this.label8.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.label8.AutoSize = true;
            this.label8.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label8.ForeColor = System.Drawing.Color.Cornsilk;
            this.label8.Location = new System.Drawing.Point(435, 68);
            this.label8.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(21, 18);
            this.label8.TabIndex = 25;
            this.label8.Text = "%";
            // 
            // label18
            // 
            this.label18.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.label18.AutoSize = true;
            this.label18.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label18.ForeColor = System.Drawing.Color.Cornsilk;
            this.label18.Location = new System.Drawing.Point(262, 32);
            this.label18.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label18.Name = "label18";
            this.label18.Size = new System.Drawing.Size(41, 18);
            this.label18.TabIndex = 24;
            this.label18.Text = "[F10]";
            // 
            // btnQuitar
            // 
            this.btnQuitar.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.btnQuitar.ForeColor = System.Drawing.Color.Black;
            this.btnQuitar.Image = ((System.Drawing.Image)(resources.GetObject("btnQuitar.Image")));
            this.btnQuitar.Location = new System.Drawing.Point(670, 62);
            this.btnQuitar.Margin = new System.Windows.Forms.Padding(4);
            this.btnQuitar.Name = "btnQuitar";
            this.btnQuitar.Size = new System.Drawing.Size(37, 30);
            this.btnQuitar.TabIndex = 6;
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
            this.btnAgregar.Location = new System.Drawing.Point(625, 62);
            this.btnAgregar.Margin = new System.Windows.Forms.Padding(4);
            this.btnAgregar.Name = "btnAgregar";
            this.btnAgregar.Size = new System.Drawing.Size(37, 30);
            this.btnAgregar.TabIndex = 5;
            this.btnAgregar.UseVisualStyleBackColor = true;
            this.btnAgregar.Click += new System.EventHandler(this.btnAgregar_Click);
            this.btnAgregar.Enter += new System.EventHandler(this.control_Enter);
            this.btnAgregar.Leave += new System.EventHandler(this.control_Leave);
            // 
            // txtPorcentaje
            // 
            this.txtPorcentaje.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.txtPorcentaje.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtPorcentaje.Location = new System.Drawing.Point(337, 64);
            this.txtPorcentaje.Margin = new System.Windows.Forms.Padding(4);
            this.txtPorcentaje.Name = "txtPorcentaje";
            this.txtPorcentaje.Size = new System.Drawing.Size(93, 26);
            this.txtPorcentaje.TabIndex = 3;
            this.txtPorcentaje.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.txtPorcentaje.Enter += new System.EventHandler(this.control_Enter);
            this.txtPorcentaje.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.TxtPruebaENTER_KeyPress);
            this.txtPorcentaje.Leave += new System.EventHandler(this.control_Leave);
            // 
            // btnBuscarCorte
            // 
            this.btnBuscarCorte.AccessibleDescription = "";
            this.btnBuscarCorte.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.btnBuscarCorte.ForeColor = System.Drawing.Color.Black;
            this.btnBuscarCorte.Image = ((System.Drawing.Image)(resources.GetObject("btnBuscarCorte.Image")));
            this.btnBuscarCorte.Location = new System.Drawing.Point(226, 27);
            this.btnBuscarCorte.Margin = new System.Windows.Forms.Padding(4);
            this.btnBuscarCorte.Name = "btnBuscarCorte";
            this.btnBuscarCorte.Size = new System.Drawing.Size(37, 30);
            this.btnBuscarCorte.TabIndex = 2;
            this.btnBuscarCorte.TabStop = false;
            this.btnBuscarCorte.UseVisualStyleBackColor = true;
            this.btnBuscarCorte.Click += new System.EventHandler(this.btnBuscarCorte_Click);
            this.btnBuscarCorte.Enter += new System.EventHandler(this.control_Enter);
            this.btnBuscarCorte.Leave += new System.EventHandler(this.control_Leave);
            // 
            // txtCodCorteEnFormula
            // 
            this.txtCodCorteEnFormula.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.txtCodCorteEnFormula.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtCodCorteEnFormula.Location = new System.Drawing.Point(122, 29);
            this.txtCodCorteEnFormula.Margin = new System.Windows.Forms.Padding(4);
            this.txtCodCorteEnFormula.Name = "txtCodCorteEnFormula";
            this.txtCodCorteEnFormula.Size = new System.Drawing.Size(93, 26);
            this.txtCodCorteEnFormula.TabIndex = 2;
            this.txtCodCorteEnFormula.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.txtCodCorteEnFormula.TextChanged += new System.EventHandler(this.txtCodCorteEnFormula_TextChanged);
            this.txtCodCorteEnFormula.Enter += new System.EventHandler(this.control_Enter);
            this.txtCodCorteEnFormula.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtCodCorteEnFormula_KeyDown);
            this.txtCodCorteEnFormula.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.TxtPruebaENTER_KeyPress);
            this.txtCodCorteEnFormula.Leave += new System.EventHandler(this.control_Leave);
            // 
            // label2
            // 
            this.label2.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.ForeColor = System.Drawing.Color.Cornsilk;
            this.label2.Location = new System.Drawing.Point(52, 32);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(56, 18);
            this.label2.TabIndex = 10;
            this.label2.Text = "Código";
            // 
            // txtCorteEnFormula
            // 
            this.txtCorteEnFormula.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.txtCorteEnFormula.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtCorteEnFormula.Location = new System.Drawing.Point(122, 64);
            this.txtCorteEnFormula.Margin = new System.Windows.Forms.Padding(4);
            this.txtCorteEnFormula.Name = "txtCorteEnFormula";
            this.txtCorteEnFormula.ReadOnly = true;
            this.txtCorteEnFormula.Size = new System.Drawing.Size(192, 26);
            this.txtCorteEnFormula.TabIndex = 9;
            this.txtCorteEnFormula.TabStop = false;
            // 
            // label5
            // 
            this.label5.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.ForeColor = System.Drawing.Color.Cornsilk;
            this.label5.Location = new System.Drawing.Point(18, 68);
            this.label5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(87, 18);
            this.label5.TabIndex = 8;
            this.label5.Text = "Descripción";
            // 
            // groupBoxFormula
            // 
            this.groupBoxFormula.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxFormula.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(43)))), ((int)(((byte)(77)))), ((int)(((byte)(129)))));
            this.groupBoxFormula.Controls.Add(this.lblError);
            this.groupBoxFormula.Controls.Add(this.label17);
            this.groupBoxFormula.Controls.Add(this.btnBuscarEmbutido);
            this.groupBoxFormula.Controls.Add(this.txtCodigoEmbutido);
            this.groupBoxFormula.Controls.Add(this.label3);
            this.groupBoxFormula.Controls.Add(this.txtEmbutido);
            this.groupBoxFormula.Controls.Add(this.label4);
            this.groupBoxFormula.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBoxFormula.ForeColor = System.Drawing.Color.Cornsilk;
            this.groupBoxFormula.Location = new System.Drawing.Point(21, 63);
            this.groupBoxFormula.Margin = new System.Windows.Forms.Padding(4);
            this.groupBoxFormula.Name = "groupBoxFormula";
            this.groupBoxFormula.Padding = new System.Windows.Forms.Padding(4);
            this.groupBoxFormula.Size = new System.Drawing.Size(457, 105);
            this.groupBoxFormula.TabIndex = 9;
            this.groupBoxFormula.TabStop = false;
            this.groupBoxFormula.Text = "Fórmula";
            // 
            // label17
            // 
            this.label17.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.label17.AutoSize = true;
            this.label17.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label17.ForeColor = System.Drawing.Color.Cornsilk;
            this.label17.Location = new System.Drawing.Point(262, 31);
            this.label17.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(33, 18);
            this.label17.TabIndex = 12;
            this.label17.Text = "[F9]";
            // 
            // btnBuscarEmbutido
            // 
            this.btnBuscarEmbutido.AccessibleDescription = "";
            this.btnBuscarEmbutido.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.btnBuscarEmbutido.ForeColor = System.Drawing.Color.Black;
            this.btnBuscarEmbutido.Image = ((System.Drawing.Image)(resources.GetObject("btnBuscarEmbutido.Image")));
            this.btnBuscarEmbutido.Location = new System.Drawing.Point(226, 26);
            this.btnBuscarEmbutido.Margin = new System.Windows.Forms.Padding(4);
            this.btnBuscarEmbutido.Name = "btnBuscarEmbutido";
            this.btnBuscarEmbutido.Size = new System.Drawing.Size(37, 30);
            this.btnBuscarEmbutido.TabIndex = 1;
            this.btnBuscarEmbutido.TabStop = false;
            this.btnBuscarEmbutido.UseVisualStyleBackColor = true;
            this.btnBuscarEmbutido.Click += new System.EventHandler(this.btnBuscarEmbutido_Click);
            this.btnBuscarEmbutido.Enter += new System.EventHandler(this.control_Enter);
            this.btnBuscarEmbutido.Leave += new System.EventHandler(this.control_Leave);
            // 
            // txtCodigoEmbutido
            // 
            this.txtCodigoEmbutido.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.txtCodigoEmbutido.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtCodigoEmbutido.Location = new System.Drawing.Point(123, 27);
            this.txtCodigoEmbutido.Margin = new System.Windows.Forms.Padding(4);
            this.txtCodigoEmbutido.Name = "txtCodigoEmbutido";
            this.txtCodigoEmbutido.ReadOnly = true;
            this.txtCodigoEmbutido.Size = new System.Drawing.Size(93, 26);
            this.txtCodigoEmbutido.TabIndex = 11;
            this.txtCodigoEmbutido.TabStop = false;
            this.txtCodigoEmbutido.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label3
            // 
            this.label3.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.ForeColor = System.Drawing.Color.Cornsilk;
            this.label3.Location = new System.Drawing.Point(54, 31);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(56, 18);
            this.label3.TabIndex = 10;
            this.label3.Text = "Código";
            // 
            // txtEmbutido
            // 
            this.txtEmbutido.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.txtEmbutido.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtEmbutido.Location = new System.Drawing.Point(123, 63);
            this.txtEmbutido.Margin = new System.Windows.Forms.Padding(4);
            this.txtEmbutido.Name = "txtEmbutido";
            this.txtEmbutido.ReadOnly = true;
            this.txtEmbutido.Size = new System.Drawing.Size(192, 26);
            this.txtEmbutido.TabIndex = 9;
            this.txtEmbutido.TabStop = false;
            // 
            // label4
            // 
            this.label4.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.ForeColor = System.Drawing.Color.Cornsilk;
            this.label4.Location = new System.Drawing.Point(19, 66);
            this.label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(87, 18);
            this.label4.TabIndex = 8;
            this.label4.Text = "Descripción";
            // 
            // label6
            // 
            this.label6.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.Location = new System.Drawing.Point(539, 413);
            this.label6.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(76, 17);
            this.label6.TabIndex = 56;
            this.label6.Text = "Modificado";
            // 
            // txtActualizadoPor
            // 
            this.txtActualizadoPor.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtActualizadoPor.Location = new System.Drawing.Point(542, 456);
            this.txtActualizadoPor.Margin = new System.Windows.Forms.Padding(4);
            this.txtActualizadoPor.Name = "txtActualizadoPor";
            this.txtActualizadoPor.ReadOnly = true;
            this.txtActualizadoPor.Size = new System.Drawing.Size(192, 24);
            this.txtActualizadoPor.TabIndex = 55;
            this.txtActualizadoPor.TabStop = false;
            // 
            // txtActualizado
            // 
            this.txtActualizado.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtActualizado.Location = new System.Drawing.Point(542, 431);
            this.txtActualizado.Margin = new System.Windows.Forms.Padding(4);
            this.txtActualizado.Name = "txtActualizado";
            this.txtActualizado.ReadOnly = true;
            this.txtActualizado.Size = new System.Drawing.Size(192, 24);
            this.txtActualizado.TabIndex = 54;
            this.txtActualizado.TabStop = false;
            // 
            // label9
            // 
            this.label9.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label9.AutoSize = true;
            this.label9.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label9.Location = new System.Drawing.Point(538, 337);
            this.label9.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(54, 17);
            this.label9.TabIndex = 53;
            this.label9.Text = "Creado";
            // 
            // txtCreadoPor
            // 
            this.txtCreadoPor.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtCreadoPor.Location = new System.Drawing.Point(542, 383);
            this.txtCreadoPor.Margin = new System.Windows.Forms.Padding(4);
            this.txtCreadoPor.Name = "txtCreadoPor";
            this.txtCreadoPor.ReadOnly = true;
            this.txtCreadoPor.Size = new System.Drawing.Size(192, 24);
            this.txtCreadoPor.TabIndex = 52;
            this.txtCreadoPor.TabStop = false;
            // 
            // txtCreado
            // 
            this.txtCreado.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtCreado.Location = new System.Drawing.Point(542, 357);
            this.txtCreado.Margin = new System.Windows.Forms.Padding(4);
            this.txtCreado.Name = "txtCreado";
            this.txtCreado.ReadOnly = true;
            this.txtCreado.Size = new System.Drawing.Size(192, 24);
            this.txtCreado.TabIndex = 51;
            this.txtCreado.TabStop = false;
            // 
            // lblError
            // 
            this.lblError.AutoSize = true;
            this.lblError.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblError.ForeColor = System.Drawing.Color.LightSalmon;
            this.lblError.Location = new System.Drawing.Point(291, 18);
            this.lblError.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblError.Name = "lblError";
            this.lblError.Size = new System.Drawing.Size(165, 36);
            this.lblError.TabIndex = 53;
            this.lblError.Text = "El Código seleccionado\r\n ya posee fórmula";
            this.lblError.Visible = false;
            // 
            // agregarAuto
            // 
            this.agregarAuto.DataPropertyName = "agregarAuto";
            this.agregarAuto.FillWeight = 51.28205F;
            this.agregarAuto.HeaderText = "Agregar Auto.";
            this.agregarAuto.MinimumWidth = 6;
            this.agregarAuto.Name = "agregarAuto";
            this.agregarAuto.ReadOnly = true;
            // 
            // porcentaje
            // 
            this.porcentaje.DataPropertyName = "Porcentaje";
            dataGridViewCellStyle28.Format = "N3";
            this.porcentaje.DefaultCellStyle = dataGridViewCellStyle28;
            this.porcentaje.FillWeight = 88.16828F;
            this.porcentaje.HeaderText = "Porcentaje";
            this.porcentaje.MinimumWidth = 6;
            this.porcentaje.Name = "porcentaje";
            this.porcentaje.ReadOnly = true;
            // 
            // corte
            // 
            this.corte.DataPropertyName = "corte";
            this.corte.FillWeight = 160.5497F;
            this.corte.HeaderText = "Corte";
            this.corte.MinimumWidth = 6;
            this.corte.Name = "corte";
            this.corte.ReadOnly = true;
            // 
            // codigo
            // 
            this.codigo.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.codigo.DataPropertyName = "codigo";
            dataGridViewCellStyle27.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
            this.codigo.DefaultCellStyle = dataGridViewCellStyle27;
            this.codigo.FillWeight = 80F;
            this.codigo.HeaderText = "Codigo";
            this.codigo.MinimumWidth = 6;
            this.codigo.Name = "codigo";
            this.codigo.ReadOnly = true;
            this.codigo.Width = 70;
            // 
            // idCorte
            // 
            this.idCorte.DataPropertyName = "idCorte";
            this.idCorte.HeaderText = "ID Corte";
            this.idCorte.MinimumWidth = 6;
            this.idCorte.Name = "idCorte";
            this.idCorte.ReadOnly = true;
            this.idCorte.Visible = false;
            // 
            // formIngresoFormula
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(749, 593);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.txtActualizadoPor);
            this.Controls.Add(this.txtActualizado);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.txtCreadoPor);
            this.Controls.Add(this.txtCreado);
            this.Controls.Add(this.btnGuardar);
            this.Controls.Add(this.btnCancelar);
            this.Controls.Add(this.grillaCortesPorEmbutido);
            this.Controls.Add(this.pnlBuscar);
            this.Margin = new System.Windows.Forms.Padding(5);
            this.MaximizeBox = false;
            this.Name = "formIngresoFormula";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Nueva Fórmula";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.formIngresoFormula_FormClosing);
            this.Load += new System.EventHandler(this.formIngresoFormula_Load);
            ((System.ComponentModel.ISupportInitialize)(this.grillaCortesPorEmbutido)).EndInit();
            this.pnlBuscar.ResumeLayout(false);
            this.pnlBuscar.PerformLayout();
            this.groupBoxCortesFormula.ResumeLayout(false);
            this.groupBoxCortesFormula.PerformLayout();
            this.groupBoxFormula.ResumeLayout(false);
            this.groupBoxFormula.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        protected System.Windows.Forms.Button btnGuardar;
        protected System.Windows.Forms.Button btnCancelar;
        private System.Windows.Forms.DataGridView grillaCortesPorEmbutido;
        protected System.Windows.Forms.Panel pnlBuscar;
        protected System.Windows.Forms.GroupBox groupBoxFormula;
        protected System.Windows.Forms.TextBox txtCodigoEmbutido;
        protected System.Windows.Forms.Label label3;
        protected System.Windows.Forms.TextBox txtEmbutido;
        protected System.Windows.Forms.Label label4;
        protected System.Windows.Forms.GroupBox groupBoxCortesFormula;
        protected System.Windows.Forms.TextBox txtCodCorteEnFormula;
        protected System.Windows.Forms.Label label2;
        protected System.Windows.Forms.TextBox txtCorteEnFormula;
        protected System.Windows.Forms.Label label5;
        protected internal System.Windows.Forms.Button btnBuscarCorte;
        protected System.Windows.Forms.TextBox txtPorcentaje;
        private System.Windows.Forms.Button btnQuitar;
        private System.Windows.Forms.Button btnAgregar;
        protected internal System.Windows.Forms.Button btnBuscarEmbutido;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.GroupBox groupBox3;
        protected System.Windows.Forms.TextBox txtUsuario;
        protected System.Windows.Forms.Label label7;
        protected System.Windows.Forms.Label label18;
        protected System.Windows.Forms.Label label17;
        protected System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox txtActualizadoPor;
        private System.Windows.Forms.TextBox txtActualizado;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.TextBox txtCreadoPor;
        private System.Windows.Forms.TextBox txtCreado;
        private System.Windows.Forms.CheckBox checkAgregarAuto;
        protected System.Windows.Forms.Label lblError;
        private System.Windows.Forms.DataGridViewTextBoxColumn idCorte;
        private System.Windows.Forms.DataGridViewTextBoxColumn codigo;
        private System.Windows.Forms.DataGridViewTextBoxColumn corte;
        private System.Windows.Forms.DataGridViewTextBoxColumn porcentaje;
        private System.Windows.Forms.DataGridViewCheckBoxColumn agregarAuto;
    }
}