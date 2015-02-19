namespace Presentacion
{
    partial class formIngresoEmbutido
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(formIngresoEmbutido));
            this.btnGuardar = new System.Windows.Forms.Button();
            this.btnCancelar = new System.Windows.Forms.Button();
            this.grillaCortesPorEmbutido = new System.Windows.Forms.DataGridView();
            this.idCorte = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.codigo = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.corte = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.kgUtilizados = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.pnlBuscar = new System.Windows.Forms.Panel();
            this.comboSucursal = new System.Windows.Forms.ComboBox();
            this.label9 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.txtFechaEmbutido = new System.Windows.Forms.DateTimePicker();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.btnQuitar = new System.Windows.Forms.Button();
            this.btnAgregar = new System.Windows.Forms.Button();
            this.txtCantKgs = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.btnBuscarCorte = new System.Windows.Forms.Button();
            this.txtCodCorteEnEmbutido = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.txtCorteEnEmbutido = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.btnBuscarEmbutido = new System.Windows.Forms.Button();
            this.txtCodigoEmbutido = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.txtEmbutido = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.shapeContainer2 = new Microsoft.VisualBasic.PowerPacks.ShapeContainer();
            this.lineShape2 = new Microsoft.VisualBasic.PowerPacks.LineShape();
            this.txtTotalKg = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.lineShape1 = new Microsoft.VisualBasic.PowerPacks.LineShape();
            this.shapeContainer1 = new Microsoft.VisualBasic.PowerPacks.ShapeContainer();
            this.label11 = new System.Windows.Forms.Label();
            this.txtObservaciones = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.grillaCortesPorEmbutido)).BeginInit();
            this.pnlBuscar.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnGuardar
            // 
            this.btnGuardar.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnGuardar.Location = new System.Drawing.Point(305, 495);
            this.btnGuardar.Name = "btnGuardar";
            this.btnGuardar.Size = new System.Drawing.Size(87, 27);
            this.btnGuardar.TabIndex = 6;
            this.btnGuardar.TabStop = false;
            this.btnGuardar.Text = "Guardar";
            this.btnGuardar.UseVisualStyleBackColor = true;
            this.btnGuardar.Click += new System.EventHandler(this.btnGuardar_Click);
            // 
            // btnCancelar
            // 
            this.btnCancelar.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancelar.Location = new System.Drawing.Point(401, 495);
            this.btnCancelar.Name = "btnCancelar";
            this.btnCancelar.Size = new System.Drawing.Size(87, 27);
            this.btnCancelar.TabIndex = 7;
            this.btnCancelar.TabStop = false;
            this.btnCancelar.Text = "Cancelar";
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
            this.grillaCortesPorEmbutido.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.grillaCortesPorEmbutido.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.idCorte,
            this.codigo,
            this.corte,
            this.kgUtilizados});
            this.grillaCortesPorEmbutido.Location = new System.Drawing.Point(9, 240);
            this.grillaCortesPorEmbutido.MultiSelect = false;
            this.grillaCortesPorEmbutido.Name = "grillaCortesPorEmbutido";
            this.grillaCortesPorEmbutido.ReadOnly = true;
            this.grillaCortesPorEmbutido.RowHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
            this.grillaCortesPorEmbutido.RowHeadersVisible = false;
            this.grillaCortesPorEmbutido.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.grillaCortesPorEmbutido.Size = new System.Drawing.Size(478, 182);
            this.grillaCortesPorEmbutido.StandardTab = true;
            this.grillaCortesPorEmbutido.TabIndex = 15;
            this.grillaCortesPorEmbutido.TabStop = false;
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
            this.codigo.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.codigo.DataPropertyName = "codigo";
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
            this.codigo.DefaultCellStyle = dataGridViewCellStyle1;
            this.codigo.FillWeight = 80F;
            this.codigo.HeaderText = "Codigo";
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
            // 
            // kgUtilizados
            // 
            this.kgUtilizados.DataPropertyName = "kgUtilizado";
            this.kgUtilizados.HeaderText = "Kgs. Utilizados";
            this.kgUtilizados.Name = "kgUtilizados";
            this.kgUtilizados.ReadOnly = true;
            // 
            // pnlBuscar
            // 
            this.pnlBuscar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.pnlBuscar.BackColor = System.Drawing.Color.SteelBlue;
            this.pnlBuscar.Controls.Add(this.comboSucursal);
            this.pnlBuscar.Controls.Add(this.label9);
            this.pnlBuscar.Controls.Add(this.label6);
            this.pnlBuscar.Controls.Add(this.txtFechaEmbutido);
            this.pnlBuscar.Controls.Add(this.groupBox2);
            this.pnlBuscar.Controls.Add(this.groupBox1);
            this.pnlBuscar.Controls.Add(this.shapeContainer2);
            this.pnlBuscar.Location = new System.Drawing.Point(0, 0);
            this.pnlBuscar.Name = "pnlBuscar";
            this.pnlBuscar.Size = new System.Drawing.Size(497, 234);
            this.pnlBuscar.TabIndex = 14;
            // 
            // comboSucursal
            // 
            this.comboSucursal.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboSucursal.FormattingEnabled = true;
            this.comboSucursal.Location = new System.Drawing.Point(80, 9);
            this.comboSucursal.Name = "comboSucursal";
            this.comboSucursal.Size = new System.Drawing.Size(116, 21);
            this.comboSucursal.TabIndex = 0;
            this.comboSucursal.TabStop = false;
            this.comboSucursal.SelectedIndexChanged += new System.EventHandler(this.comboSucursal_SelectedIndexChanged);
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label9.ForeColor = System.Drawing.Color.Cornsilk;
            this.label9.Location = new System.Drawing.Point(20, 12);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(55, 15);
            this.label9.TabIndex = 21;
            this.label9.Text = "Sucursal";
            // 
            // label6
            // 
            this.label6.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.ForeColor = System.Drawing.Color.Cornsilk;
            this.label6.Location = new System.Drawing.Point(324, 10);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(41, 15);
            this.label6.TabIndex = 12;
            this.label6.Text = "Fecha";
            // 
            // txtFechaEmbutido
            // 
            this.txtFechaEmbutido.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.txtFechaEmbutido.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.txtFechaEmbutido.Location = new System.Drawing.Point(374, 8);
            this.txtFechaEmbutido.Name = "txtFechaEmbutido";
            this.txtFechaEmbutido.Size = new System.Drawing.Size(104, 20);
            this.txtFechaEmbutido.TabIndex = 11;
            this.txtFechaEmbutido.TabStop = false;
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.groupBox2.BackColor = System.Drawing.Color.SteelBlue;
            this.groupBox2.Controls.Add(this.btnQuitar);
            this.groupBox2.Controls.Add(this.btnAgregar);
            this.groupBox2.Controls.Add(this.txtCantKgs);
            this.groupBox2.Controls.Add(this.label1);
            this.groupBox2.Controls.Add(this.btnBuscarCorte);
            this.groupBox2.Controls.Add(this.txtCodCorteEnEmbutido);
            this.groupBox2.Controls.Add(this.label2);
            this.groupBox2.Controls.Add(this.txtCorteEnEmbutido);
            this.groupBox2.Controls.Add(this.label5);
            this.groupBox2.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox2.ForeColor = System.Drawing.Color.Cornsilk;
            this.groupBox2.Location = new System.Drawing.Point(11, 137);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(477, 86);
            this.groupBox2.TabIndex = 10;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Corte en Embutido";
            // 
            // btnQuitar
            // 
            this.btnQuitar.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.btnQuitar.ForeColor = System.Drawing.Color.Black;
            this.btnQuitar.Image = ((System.Drawing.Image)(resources.GetObject("btnQuitar.Image")));
            this.btnQuitar.Location = new System.Drawing.Point(436, 52);
            this.btnQuitar.Name = "btnQuitar";
            this.btnQuitar.Size = new System.Drawing.Size(28, 24);
            this.btnQuitar.TabIndex = 5;
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
            this.btnAgregar.Location = new System.Drawing.Point(402, 52);
            this.btnAgregar.Name = "btnAgregar";
            this.btnAgregar.Size = new System.Drawing.Size(28, 24);
            this.btnAgregar.TabIndex = 4;
            this.btnAgregar.UseVisualStyleBackColor = true;
            this.btnAgregar.Click += new System.EventHandler(this.btnAgregar_Click);
            // 
            // txtCantKgs
            // 
            this.txtCantKgs.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.txtCantKgs.Location = new System.Drawing.Point(316, 53);
            this.txtCantKgs.Name = "txtCantKgs";
            this.txtCantKgs.Size = new System.Drawing.Size(71, 21);
            this.txtCantKgs.TabIndex = 3;
            this.txtCantKgs.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.txtCantKgs.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.TxtPruebaENTER_KeyPress);
            // 
            // label1
            // 
            this.label1.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ForeColor = System.Drawing.Color.Cornsilk;
            this.label1.Location = new System.Drawing.Point(251, 56);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(59, 15);
            this.label1.TabIndex = 16;
            this.label1.Text = "Cant. Kgs";
            // 
            // btnBuscarCorte
            // 
            this.btnBuscarCorte.AccessibleDescription = "";
            this.btnBuscarCorte.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.btnBuscarCorte.ForeColor = System.Drawing.Color.Black;
            this.btnBuscarCorte.Image = ((System.Drawing.Image)(resources.GetObject("btnBuscarCorte.Image")));
            this.btnBuscarCorte.Location = new System.Drawing.Point(170, 23);
            this.btnBuscarCorte.Name = "btnBuscarCorte";
            this.btnBuscarCorte.Size = new System.Drawing.Size(28, 24);
            this.btnBuscarCorte.TabIndex = 2;
            this.btnBuscarCorte.TabStop = false;
            this.btnBuscarCorte.UseVisualStyleBackColor = true;
            this.btnBuscarCorte.Click += new System.EventHandler(this.btnBuscarCorte_Click);
            // 
            // txtCodCorteEnEmbutido
            // 
            this.txtCodCorteEnEmbutido.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.txtCodCorteEnEmbutido.Location = new System.Drawing.Point(92, 24);
            this.txtCodCorteEnEmbutido.Name = "txtCodCorteEnEmbutido";
            this.txtCodCorteEnEmbutido.Size = new System.Drawing.Size(71, 21);
            this.txtCodCorteEnEmbutido.TabIndex = 2;
            this.txtCodCorteEnEmbutido.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.txtCodCorteEnEmbutido.TextChanged += new System.EventHandler(this.txtCodCorteEnEmbutido_TextChanged);
            this.txtCodCorteEnEmbutido.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.TxtPruebaENTER_KeyPress);
            // 
            // label2
            // 
            this.label2.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.ForeColor = System.Drawing.Color.Cornsilk;
            this.label2.Location = new System.Drawing.Point(40, 27);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(46, 15);
            this.label2.TabIndex = 10;
            this.label2.Text = "Código";
            // 
            // txtCorteEnEmbutido
            // 
            this.txtCorteEnEmbutido.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.txtCorteEnEmbutido.Location = new System.Drawing.Point(92, 53);
            this.txtCorteEnEmbutido.Name = "txtCorteEnEmbutido";
            this.txtCorteEnEmbutido.ReadOnly = true;
            this.txtCorteEnEmbutido.Size = new System.Drawing.Size(145, 21);
            this.txtCorteEnEmbutido.TabIndex = 9;
            this.txtCorteEnEmbutido.TabStop = false;
            // 
            // label5
            // 
            this.label5.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.ForeColor = System.Drawing.Color.Cornsilk;
            this.label5.Location = new System.Drawing.Point(14, 56);
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
            this.groupBox1.Controls.Add(this.btnBuscarEmbutido);
            this.groupBox1.Controls.Add(this.txtCodigoEmbutido);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.txtEmbutido);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox1.ForeColor = System.Drawing.Color.Cornsilk;
            this.groupBox1.Location = new System.Drawing.Point(12, 45);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(290, 85);
            this.groupBox1.TabIndex = 9;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Embutido";
            // 
            // btnBuscarEmbutido
            // 
            this.btnBuscarEmbutido.AccessibleDescription = "";
            this.btnBuscarEmbutido.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.btnBuscarEmbutido.ForeColor = System.Drawing.Color.Black;
            this.btnBuscarEmbutido.Image = ((System.Drawing.Image)(resources.GetObject("btnBuscarEmbutido.Image")));
            this.btnBuscarEmbutido.Location = new System.Drawing.Point(169, 21);
            this.btnBuscarEmbutido.Name = "btnBuscarEmbutido";
            this.btnBuscarEmbutido.Size = new System.Drawing.Size(28, 24);
            this.btnBuscarEmbutido.TabIndex = 1;
            this.btnBuscarEmbutido.TabStop = false;
            this.btnBuscarEmbutido.UseVisualStyleBackColor = true;
            this.btnBuscarEmbutido.Click += new System.EventHandler(this.btnBuscarEmbutido_Click);
            // 
            // txtCodigoEmbutido
            // 
            this.txtCodigoEmbutido.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.txtCodigoEmbutido.Location = new System.Drawing.Point(92, 22);
            this.txtCodigoEmbutido.Name = "txtCodigoEmbutido";
            this.txtCodigoEmbutido.ReadOnly = true;
            this.txtCodigoEmbutido.Size = new System.Drawing.Size(71, 21);
            this.txtCodigoEmbutido.TabIndex = 11;
            this.txtCodigoEmbutido.TabStop = false;
            this.txtCodigoEmbutido.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.txtCodigoEmbutido.TextChanged += new System.EventHandler(this.txtCodigoEmbutido_TextChanged);
            // 
            // label3
            // 
            this.label3.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.ForeColor = System.Drawing.Color.Cornsilk;
            this.label3.Location = new System.Drawing.Point(40, 25);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(46, 15);
            this.label3.TabIndex = 10;
            this.label3.Text = "Código";
            // 
            // txtEmbutido
            // 
            this.txtEmbutido.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.txtEmbutido.Location = new System.Drawing.Point(92, 51);
            this.txtEmbutido.Name = "txtEmbutido";
            this.txtEmbutido.ReadOnly = true;
            this.txtEmbutido.Size = new System.Drawing.Size(145, 21);
            this.txtEmbutido.TabIndex = 9;
            this.txtEmbutido.TabStop = false;
            // 
            // label4
            // 
            this.label4.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.ForeColor = System.Drawing.Color.Cornsilk;
            this.label4.Location = new System.Drawing.Point(14, 54);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(72, 15);
            this.label4.TabIndex = 8;
            this.label4.Text = "Descripción";
            // 
            // shapeContainer2
            // 
            this.shapeContainer2.Location = new System.Drawing.Point(0, 0);
            this.shapeContainer2.Margin = new System.Windows.Forms.Padding(0);
            this.shapeContainer2.Name = "shapeContainer2";
            this.shapeContainer2.Shapes.AddRange(new Microsoft.VisualBasic.PowerPacks.Shape[] {
            this.lineShape2});
            this.shapeContainer2.Size = new System.Drawing.Size(497, 234);
            this.shapeContainer2.TabIndex = 19;
            this.shapeContainer2.TabStop = false;
            // 
            // lineShape2
            // 
            this.lineShape2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.lineShape2.BorderColor = System.Drawing.Color.White;
            this.lineShape2.Name = "lineShape2";
            this.lineShape2.X1 = 17;
            this.lineShape2.X2 = 476;
            this.lineShape2.Y1 = 37;
            this.lineShape2.Y2 = 37;
            // 
            // txtTotalKg
            // 
            this.txtTotalKg.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.txtTotalKg.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtTotalKg.Location = new System.Drawing.Point(383, 428);
            this.txtTotalKg.Name = "txtTotalKg";
            this.txtTotalKg.ReadOnly = true;
            this.txtTotalKg.Size = new System.Drawing.Size(102, 21);
            this.txtTotalKg.TabIndex = 19;
            this.txtTotalKg.TabStop = false;
            this.txtTotalKg.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label8
            // 
            this.label8.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.label8.AutoSize = true;
            this.label8.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label8.Location = new System.Drawing.Point(317, 433);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(60, 15);
            this.label8.TabIndex = 18;
            this.label8.Text = "Total Kg";
            // 
            // lineShape1
            // 
            this.lineShape1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.lineShape1.Name = "lineShape1";
            this.lineShape1.X1 = 8;
            this.lineShape1.X2 = 489;
            this.lineShape1.Y1 = 488;
            this.lineShape1.Y2 = 488;
            // 
            // shapeContainer1
            // 
            this.shapeContainer1.Location = new System.Drawing.Point(0, 0);
            this.shapeContainer1.Margin = new System.Windows.Forms.Padding(0);
            this.shapeContainer1.Name = "shapeContainer1";
            this.shapeContainer1.Shapes.AddRange(new Microsoft.VisualBasic.PowerPacks.Shape[] {
            this.lineShape1});
            this.shapeContainer1.Size = new System.Drawing.Size(497, 528);
            this.shapeContainer1.TabIndex = 20;
            this.shapeContainer1.TabStop = false;
            // 
            // label11
            // 
            this.label11.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(8, 427);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(76, 13);
            this.label11.TabIndex = 22;
            this.label11.Text = "observaciones";
            // 
            // txtObservaciones
            // 
            this.txtObservaciones.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.txtObservaciones.Location = new System.Drawing.Point(11, 442);
            this.txtObservaciones.Multiline = true;
            this.txtObservaciones.Name = "txtObservaciones";
            this.txtObservaciones.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtObservaciones.Size = new System.Drawing.Size(255, 39);
            this.txtObservaciones.TabIndex = 5;
            this.txtObservaciones.TabStop = false;
            // 
            // formIngresoEmbutido
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(497, 528);
            this.Controls.Add(this.label11);
            this.Controls.Add(this.txtObservaciones);
            this.Controls.Add(this.txtTotalKg);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.btnGuardar);
            this.Controls.Add(this.btnCancelar);
            this.Controls.Add(this.grillaCortesPorEmbutido);
            this.Controls.Add(this.pnlBuscar);
            this.Controls.Add(this.shapeContainer1);
            this.MaximizeBox = false;
            this.Name = "formIngresoEmbutido";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Ingreso de Embutidos";
            ((System.ComponentModel.ISupportInitialize)(this.grillaCortesPorEmbutido)).EndInit();
            this.pnlBuscar.ResumeLayout(false);
            this.pnlBuscar.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        protected System.Windows.Forms.Button btnGuardar;
        protected System.Windows.Forms.Button btnCancelar;
        private System.Windows.Forms.DataGridView grillaCortesPorEmbutido;
        protected System.Windows.Forms.Panel pnlBuscar;
        protected System.Windows.Forms.GroupBox groupBox1;
        protected System.Windows.Forms.TextBox txtCodigoEmbutido;
        protected System.Windows.Forms.Label label3;
        protected System.Windows.Forms.TextBox txtEmbutido;
        protected System.Windows.Forms.Label label4;
        protected System.Windows.Forms.GroupBox groupBox2;
        protected System.Windows.Forms.TextBox txtCodCorteEnEmbutido;
        protected System.Windows.Forms.Label label2;
        protected System.Windows.Forms.TextBox txtCorteEnEmbutido;
        protected System.Windows.Forms.Label label5;
        protected internal System.Windows.Forms.Button btnBuscarCorte;
        protected System.Windows.Forms.TextBox txtCantKgs;
        protected System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnQuitar;
        private System.Windows.Forms.Button btnAgregar;
        protected System.Windows.Forms.Label label6;
        private System.Windows.Forms.DateTimePicker txtFechaEmbutido;
        private System.Windows.Forms.TextBox txtTotalKg;
        private System.Windows.Forms.Label label8;
        private Microsoft.VisualBasic.PowerPacks.LineShape lineShape1;
        private Microsoft.VisualBasic.PowerPacks.ShapeContainer shapeContainer1;
        private Microsoft.VisualBasic.PowerPacks.ShapeContainer shapeContainer2;
        private Microsoft.VisualBasic.PowerPacks.LineShape lineShape2;
        protected internal System.Windows.Forms.Button btnBuscarEmbutido;
        private System.Windows.Forms.ComboBox comboSucursal;
        protected System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.TextBox txtObservaciones;
        private System.Windows.Forms.DataGridViewTextBoxColumn idCorte;
        private System.Windows.Forms.DataGridViewTextBoxColumn codigo;
        private System.Windows.Forms.DataGridViewTextBoxColumn corte;
        private System.Windows.Forms.DataGridViewTextBoxColumn kgUtilizados;
    }
}