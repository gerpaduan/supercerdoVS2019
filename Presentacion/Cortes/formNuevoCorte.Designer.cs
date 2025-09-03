namespace Presentacion
{
    partial class formNuevoCorte
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(formNuevoCorte));
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle9 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle10 = new System.Windows.Forms.DataGridViewCellStyle();
            this.btnGuardar = new System.Windows.Forms.Button();
            this.btnCancelar = new System.Windows.Forms.Button();
            this.pnlBuscar = new System.Windows.Forms.Panel();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.checkSugerirCodigo = new System.Windows.Forms.CheckBox();
            this.label20 = new System.Windows.Forms.Label();
            this.label19 = new System.Windows.Forms.Label();
            this.label18 = new System.Windows.Forms.Label();
            this.label17 = new System.Windows.Forms.Label();
            this.label16 = new System.Windows.Forms.Label();
            this.label15 = new System.Windows.Forms.Label();
            this.label14 = new System.Windows.Forms.Label();
            this.btnBorrarMarca = new System.Windows.Forms.Button();
            this.label13 = new System.Windows.Forms.Label();
            this.btnMarca = new System.Windows.Forms.Button();
            this.groupProveedores = new System.Windows.Forms.GroupBox();
            this.grillaProveedores = new System.Windows.Forms.DataGridView();
            this.dataGridViewTextBoxColumn3 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ultimoPrecio = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.fechaUltimaCompra = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.label12 = new System.Windows.Forms.Label();
            this.txtMarca = new System.Windows.Forms.TextBox();
            this.txtPuntoStock = new System.Windows.Forms.TextBox();
            this.label11 = new System.Windows.Forms.Label();
            this.lblActualizar = new System.Windows.Forms.Label();
            this.checkMayuscula = new System.Windows.Forms.CheckBox();
            this.checkPesable = new System.Windows.Forms.CheckBox();
            this.comboAlicuotaIva = new System.Windows.Forms.ComboBox();
            this.label10 = new System.Windows.Forms.Label();
            this.checkHabilitado = new System.Windows.Forms.CheckBox();
            this.txtPromedio = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.checkEnCierreStock = new System.Windows.Forms.CheckBox();
            this.checkIngresoRapidoEmbutido = new System.Windows.Forms.CheckBox();
            this.checkAsignarMaestro = new System.Windows.Forms.CheckBox();
            this.groupMaestro = new System.Windows.Forms.GroupBox();
            this.txtDesvioEstandar = new System.Windows.Forms.MaskedTextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.txtPorcHueso = new System.Windows.Forms.MaskedTextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.txtPorcentajeCorteM = new System.Windows.Forms.MaskedTextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.btnBuscarCorteM = new System.Windows.Forms.Button();
            this.txtCorteMaestro = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.txtIndependiente = new System.Windows.Forms.CheckBox();
            this.txtPrecioKg = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.comboTipo = new System.Windows.Forms.ComboBox();
            this.label6 = new System.Windows.Forms.Label();
            this.txtCodigo = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.txtDescCorte = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.pnlBuscar.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.groupProveedores.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.grillaProveedores)).BeginInit();
            this.groupMaestro.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnGuardar
            // 
            this.btnGuardar.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnGuardar.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnGuardar.Location = new System.Drawing.Point(398, 511);
            this.btnGuardar.Name = "btnGuardar";
            this.btnGuardar.Size = new System.Drawing.Size(91, 27);
            this.btnGuardar.TabIndex = 8;
            this.btnGuardar.Text = "&Guardar";
            this.btnGuardar.UseVisualStyleBackColor = true;
            this.btnGuardar.Click += new System.EventHandler(this.btnGuardar_Click);
            // 
            // btnCancelar
            // 
            this.btnCancelar.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancelar.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnCancelar.Location = new System.Drawing.Point(494, 511);
            this.btnCancelar.Name = "btnCancelar";
            this.btnCancelar.Size = new System.Drawing.Size(91, 27);
            this.btnCancelar.TabIndex = 9;
            this.btnCancelar.Text = "&Cancelar";
            this.btnCancelar.UseVisualStyleBackColor = true;
            this.btnCancelar.Click += new System.EventHandler(this.btnCancelar_Click);
            // 
            // pnlBuscar
            // 
            this.pnlBuscar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pnlBuscar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(43)))), ((int)(((byte)(77)))), ((int)(((byte)(129)))));
            this.pnlBuscar.Controls.Add(this.groupBox1);
            this.pnlBuscar.Location = new System.Drawing.Point(0, 0);
            this.pnlBuscar.Name = "pnlBuscar";
            this.pnlBuscar.Size = new System.Drawing.Size(598, 505);
            this.pnlBuscar.TabIndex = 10;
            // 
            // groupBox1
            // 
            this.groupBox1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(43)))), ((int)(((byte)(77)))), ((int)(((byte)(129)))));
            this.groupBox1.Controls.Add(this.checkSugerirCodigo);
            this.groupBox1.Controls.Add(this.label20);
            this.groupBox1.Controls.Add(this.label19);
            this.groupBox1.Controls.Add(this.label18);
            this.groupBox1.Controls.Add(this.label17);
            this.groupBox1.Controls.Add(this.label16);
            this.groupBox1.Controls.Add(this.label15);
            this.groupBox1.Controls.Add(this.label14);
            this.groupBox1.Controls.Add(this.btnBorrarMarca);
            this.groupBox1.Controls.Add(this.label13);
            this.groupBox1.Controls.Add(this.btnMarca);
            this.groupBox1.Controls.Add(this.groupProveedores);
            this.groupBox1.Controls.Add(this.txtMarca);
            this.groupBox1.Controls.Add(this.txtPuntoStock);
            this.groupBox1.Controls.Add(this.label11);
            this.groupBox1.Controls.Add(this.lblActualizar);
            this.groupBox1.Controls.Add(this.checkMayuscula);
            this.groupBox1.Controls.Add(this.checkPesable);
            this.groupBox1.Controls.Add(this.comboAlicuotaIva);
            this.groupBox1.Controls.Add(this.label10);
            this.groupBox1.Controls.Add(this.checkHabilitado);
            this.groupBox1.Controls.Add(this.txtPromedio);
            this.groupBox1.Controls.Add(this.label8);
            this.groupBox1.Controls.Add(this.checkEnCierreStock);
            this.groupBox1.Controls.Add(this.checkIngresoRapidoEmbutido);
            this.groupBox1.Controls.Add(this.checkAsignarMaestro);
            this.groupBox1.Controls.Add(this.groupMaestro);
            this.groupBox1.Controls.Add(this.txtIndependiente);
            this.groupBox1.Controls.Add(this.txtPrecioKg);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.comboTipo);
            this.groupBox1.Controls.Add(this.label6);
            this.groupBox1.Controls.Add(this.txtCodigo);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.txtDescCorte);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox1.ForeColor = System.Drawing.Color.Cornsilk;
            this.groupBox1.Location = new System.Drawing.Point(11, 3);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(576, 498);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Prod.";
            // 
            // checkSugerirCodigo
            // 
            this.checkSugerirCodigo.Appearance = System.Windows.Forms.Appearance.Button;
            this.checkSugerirCodigo.AutoSize = true;
            this.checkSugerirCodigo.BackColor = System.Drawing.SystemColors.ControlDark;
            this.checkSugerirCodigo.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.checkSugerirCodigo.FlatAppearance.CheckedBackColor = System.Drawing.Color.LimeGreen;
            this.checkSugerirCodigo.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.checkSugerirCodigo.Location = new System.Drawing.Point(243, 27);
            this.checkSugerirCodigo.Name = "checkSugerirCodigo";
            this.checkSugerirCodigo.Size = new System.Drawing.Size(57, 25);
            this.checkSugerirCodigo.TabIndex = 66;
            this.checkSugerirCodigo.TabStop = false;
            this.checkSugerirCodigo.Text = "&Sugerir";
            this.checkSugerirCodigo.UseVisualStyleBackColor = false;
            this.checkSugerirCodigo.CheckedChanged += new System.EventHandler(this.checkSegerirCodigo_CheckedChanged);
            // 
            // label20
            // 
            this.label20.AutoSize = true;
            this.label20.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label20.ForeColor = System.Drawing.Color.Cornsilk;
            this.label20.Location = new System.Drawing.Point(170, 245);
            this.label20.Name = "label20";
            this.label20.Size = new System.Drawing.Size(12, 15);
            this.label20.TabIndex = 65;
            this.label20.Text = "*";
            // 
            // label19
            // 
            this.label19.AutoSize = true;
            this.label19.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label19.ForeColor = System.Drawing.Color.Cornsilk;
            this.label19.Location = new System.Drawing.Point(170, 218);
            this.label19.Name = "label19";
            this.label19.Size = new System.Drawing.Size(12, 15);
            this.label19.TabIndex = 64;
            this.label19.Text = "*";
            // 
            // label18
            // 
            this.label18.AutoSize = true;
            this.label18.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label18.ForeColor = System.Drawing.Color.Cornsilk;
            this.label18.Location = new System.Drawing.Point(170, 193);
            this.label18.Name = "label18";
            this.label18.Size = new System.Drawing.Size(12, 15);
            this.label18.TabIndex = 63;
            this.label18.Text = "*";
            // 
            // label17
            // 
            this.label17.AutoSize = true;
            this.label17.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label17.ForeColor = System.Drawing.Color.Cornsilk;
            this.label17.Location = new System.Drawing.Point(231, 141);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(12, 15);
            this.label17.TabIndex = 62;
            this.label17.Text = "*";
            // 
            // label16
            // 
            this.label16.AutoSize = true;
            this.label16.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label16.ForeColor = System.Drawing.Color.Cornsilk;
            this.label16.Location = new System.Drawing.Point(231, 87);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(12, 15);
            this.label16.TabIndex = 61;
            this.label16.Text = "*";
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label15.ForeColor = System.Drawing.Color.Cornsilk;
            this.label15.Location = new System.Drawing.Point(325, 57);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(12, 15);
            this.label15.TabIndex = 60;
            this.label15.Text = "*";
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label14.ForeColor = System.Drawing.Color.Cornsilk;
            this.label14.Location = new System.Drawing.Point(231, 32);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(12, 15);
            this.label14.TabIndex = 59;
            this.label14.Text = "*";
            // 
            // btnBorrarMarca
            // 
            this.btnBorrarMarca.AccessibleDescription = "";
            this.btnBorrarMarca.ForeColor = System.Drawing.Color.Black;
            this.btnBorrarMarca.Location = new System.Drawing.Point(254, 109);
            this.btnBorrarMarca.Name = "btnBorrarMarca";
            this.btnBorrarMarca.Size = new System.Drawing.Size(67, 23);
            this.btnBorrarMarca.TabIndex = 58;
            this.btnBorrarMarca.Text = "Borrar Marca";
            this.btnBorrarMarca.UseVisualStyleBackColor = true;
            this.btnBorrarMarca.Visible = false;
            this.btnBorrarMarca.Click += new System.EventHandler(this.btnBorrarMarca_Click);
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label13.ForeColor = System.Drawing.Color.Cornsilk;
            this.label13.Location = new System.Drawing.Point(46, 113);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(42, 15);
            this.label13.TabIndex = 57;
            this.label13.Text = "Marca";
            // 
            // btnMarca
            // 
            this.btnMarca.AccessibleDescription = "";
            this.btnMarca.ForeColor = System.Drawing.Color.Black;
            this.btnMarca.Image = ((System.Drawing.Image)(resources.GetObject("btnMarca.Image")));
            this.btnMarca.Location = new System.Drawing.Point(226, 109);
            this.btnMarca.Name = "btnMarca";
            this.btnMarca.Size = new System.Drawing.Size(28, 23);
            this.btnMarca.TabIndex = 3;
            this.btnMarca.UseVisualStyleBackColor = true;
            this.btnMarca.Click += new System.EventHandler(this.btnMarca_Click);
            // 
            // groupProveedores
            // 
            this.groupProveedores.Controls.Add(this.grillaProveedores);
            this.groupProveedores.Controls.Add(this.label12);
            this.groupProveedores.ForeColor = System.Drawing.Color.Cornsilk;
            this.groupProveedores.Location = new System.Drawing.Point(6, 305);
            this.groupProveedores.Name = "groupProveedores";
            this.groupProveedores.Size = new System.Drawing.Size(564, 162);
            this.groupProveedores.TabIndex = 38;
            this.groupProveedores.TabStop = false;
            this.groupProveedores.Text = "Proveedores";
            // 
            // grillaProveedores
            // 
            this.grillaProveedores.AllowUserToAddRows = false;
            this.grillaProveedores.AllowUserToDeleteRows = false;
            this.grillaProveedores.AllowUserToOrderColumns = true;
            this.grillaProveedores.AllowUserToResizeRows = false;
            this.grillaProveedores.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.grillaProveedores.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.grillaProveedores.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.grillaProveedores.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.dataGridViewTextBoxColumn3,
            this.ultimoPrecio,
            this.fechaUltimaCompra});
            this.grillaProveedores.Location = new System.Drawing.Point(6, 27);
            this.grillaProveedores.MultiSelect = false;
            this.grillaProveedores.Name = "grillaProveedores";
            this.grillaProveedores.ReadOnly = true;
            this.grillaProveedores.RowHeadersVisible = false;
            this.grillaProveedores.RowHeadersWidth = 51;
            this.grillaProveedores.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.grillaProveedores.Size = new System.Drawing.Size(552, 114);
            this.grillaProveedores.TabIndex = 56;
            this.grillaProveedores.TabStop = false;
            // 
            // dataGridViewTextBoxColumn3
            // 
            this.dataGridViewTextBoxColumn3.DataPropertyName = "razonSocial";
            dataGridViewCellStyle9.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold);
            this.dataGridViewTextBoxColumn3.DefaultCellStyle = dataGridViewCellStyle9;
            this.dataGridViewTextBoxColumn3.FillWeight = 113.0288F;
            this.dataGridViewTextBoxColumn3.HeaderText = "Razon Social";
            this.dataGridViewTextBoxColumn3.MinimumWidth = 6;
            this.dataGridViewTextBoxColumn3.Name = "dataGridViewTextBoxColumn3";
            this.dataGridViewTextBoxColumn3.ReadOnly = true;
            // 
            // ultimoPrecio
            // 
            this.ultimoPrecio.DataPropertyName = "ultimoPrecio";
            dataGridViewCellStyle10.Format = "F2";
            this.ultimoPrecio.DefaultCellStyle = dataGridViewCellStyle10;
            this.ultimoPrecio.HeaderText = "Ultimo Precio";
            this.ultimoPrecio.MinimumWidth = 6;
            this.ultimoPrecio.Name = "ultimoPrecio";
            this.ultimoPrecio.ReadOnly = true;
            // 
            // fechaUltimaCompra
            // 
            this.fechaUltimaCompra.DataPropertyName = "fechaUltimaCompra";
            this.fechaUltimaCompra.HeaderText = "Fec.Compra";
            this.fechaUltimaCompra.MinimumWidth = 6;
            this.fechaUltimaCompra.Name = "fechaUltimaCompra";
            this.fechaUltimaCompra.ReadOnly = true;
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label12.ForeColor = System.Drawing.Color.Cornsilk;
            this.label12.Location = new System.Drawing.Point(8, 144);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(410, 15);
            this.label12.TabIndex = 55;
            this.label12.Text = "-Se agregarán automáticamente al realizar las compras a cada proveedor";
            // 
            // txtMarca
            // 
            this.txtMarca.Location = new System.Drawing.Point(93, 110);
            this.txtMarca.Name = "txtMarca";
            this.txtMarca.ReadOnly = true;
            this.txtMarca.Size = new System.Drawing.Size(132, 21);
            this.txtMarca.TabIndex = 39;
            this.txtMarca.TabStop = false;
            // 
            // txtPuntoStock
            // 
            this.txtPuntoStock.Location = new System.Drawing.Point(93, 216);
            this.txtPuntoStock.Name = "txtPuntoStock";
            this.txtPuntoStock.Size = new System.Drawing.Size(71, 21);
            this.txtPuntoStock.TabIndex = 6;
            this.txtPuntoStock.Text = "0";
            this.txtPuntoStock.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.txtPuntoStock.TextChanged += new System.EventHandler(this.txtPuntoStock_TextChanged);
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label11.ForeColor = System.Drawing.Color.Cornsilk;
            this.label11.Location = new System.Drawing.Point(14, 219);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(72, 15);
            this.label11.TabIndex = 56;
            this.label11.Text = "Punto Stock";
            // 
            // lblActualizar
            // 
            this.lblActualizar.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblActualizar.AutoSize = true;
            this.lblActualizar.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblActualizar.ForeColor = System.Drawing.Color.LightSalmon;
            this.lblActualizar.Location = new System.Drawing.Point(91, 266);
            this.lblActualizar.Name = "lblActualizar";
            this.lblActualizar.Size = new System.Drawing.Size(111, 15);
            this.lblActualizar.TabIndex = 54;
            this.lblActualizar.Text = "**Nivel mayor a 3**";
            this.lblActualizar.Visible = false;
            // 
            // checkMayuscula
            // 
            this.checkMayuscula.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.checkMayuscula.AutoSize = true;
            this.checkMayuscula.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.checkMayuscula.Checked = true;
            this.checkMayuscula.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkMayuscula.Location = new System.Drawing.Point(9, 474);
            this.checkMayuscula.Name = "checkMayuscula";
            this.checkMayuscula.Size = new System.Drawing.Size(85, 19);
            this.checkMayuscula.TabIndex = 37;
            this.checkMayuscula.TabStop = false;
            this.checkMayuscula.Text = "Mayúscula";
            this.checkMayuscula.UseVisualStyleBackColor = true;
            // 
            // checkPesable
            // 
            this.checkPesable.AutoSize = true;
            this.checkPesable.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.checkPesable.Checked = true;
            this.checkPesable.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkPesable.Location = new System.Drawing.Point(35, 166);
            this.checkPesable.Name = "checkPesable";
            this.checkPesable.Size = new System.Drawing.Size(71, 19);
            this.checkPesable.TabIndex = 4;
            this.checkPesable.Text = "Pesable";
            this.checkPesable.UseVisualStyleBackColor = true;
            this.checkPesable.CheckedChanged += new System.EventHandler(this.checkPesable_CheckedChanged);
            // 
            // comboAlicuotaIva
            // 
            this.comboAlicuotaIva.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboAlicuotaIva.FormattingEnabled = true;
            this.comboAlicuotaIva.Items.AddRange(new object[] {
            "Pesable",
            "Unidad",
            "Elaborado",
            "Corte",
            "Embutido",
            "Otro"});
            this.comboAlicuotaIva.Location = new System.Drawing.Point(93, 242);
            this.comboAlicuotaIva.Name = "comboAlicuotaIva";
            this.comboAlicuotaIva.Size = new System.Drawing.Size(71, 23);
            this.comboAlicuotaIva.TabIndex = 7;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label10.ForeColor = System.Drawing.Color.Cornsilk;
            this.label10.Location = new System.Drawing.Point(23, 245);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(68, 15);
            this.label10.TabIndex = 36;
            this.label10.Text = "Alícuota Iva";
            // 
            // checkHabilitado
            // 
            this.checkHabilitado.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.checkHabilitado.AutoSize = true;
            this.checkHabilitado.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.checkHabilitado.Checked = true;
            this.checkHabilitado.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkHabilitado.Location = new System.Drawing.Point(410, 56);
            this.checkHabilitado.Name = "checkHabilitado";
            this.checkHabilitado.Size = new System.Drawing.Size(82, 19);
            this.checkHabilitado.TabIndex = 9;
            this.checkHabilitado.Text = "Habilitado";
            this.checkHabilitado.UseVisualStyleBackColor = true;
            // 
            // txtPromedio
            // 
            this.txtPromedio.Location = new System.Drawing.Point(93, 190);
            this.txtPromedio.Name = "txtPromedio";
            this.txtPromedio.Size = new System.Drawing.Size(71, 21);
            this.txtPromedio.TabIndex = 5;
            this.txtPromedio.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label8.ForeColor = System.Drawing.Color.Cornsilk;
            this.label8.Location = new System.Drawing.Point(26, 193);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(61, 15);
            this.label8.TabIndex = 34;
            this.label8.Text = "Promedio";
            // 
            // checkEnCierreStock
            // 
            this.checkEnCierreStock.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.checkEnCierreStock.AutoSize = true;
            this.checkEnCierreStock.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.checkEnCierreStock.Checked = true;
            this.checkEnCierreStock.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkEnCierreStock.Location = new System.Drawing.Point(382, 81);
            this.checkEnCierreStock.Name = "checkEnCierreStock";
            this.checkEnCierreStock.Size = new System.Drawing.Size(110, 19);
            this.checkEnCierreStock.TabIndex = 10;
            this.checkEnCierreStock.Text = "En Cierre Stock";
            this.checkEnCierreStock.UseVisualStyleBackColor = true;
            // 
            // checkIngresoRapidoEmbutido
            // 
            this.checkIngresoRapidoEmbutido.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.checkIngresoRapidoEmbutido.AutoSize = true;
            this.checkIngresoRapidoEmbutido.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.checkIngresoRapidoEmbutido.Location = new System.Drawing.Point(328, 32);
            this.checkIngresoRapidoEmbutido.Name = "checkIngresoRapidoEmbutido";
            this.checkIngresoRapidoEmbutido.Size = new System.Drawing.Size(164, 19);
            this.checkIngresoRapidoEmbutido.TabIndex = 8;
            this.checkIngresoRapidoEmbutido.Text = "IngresoRapidoElaborado";
            this.checkIngresoRapidoEmbutido.UseVisualStyleBackColor = true;
            // 
            // checkAsignarMaestro
            // 
            this.checkAsignarMaestro.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.checkAsignarMaestro.AutoSize = true;
            this.checkAsignarMaestro.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.checkAsignarMaestro.Location = new System.Drawing.Point(377, 137);
            this.checkAsignarMaestro.Name = "checkAsignarMaestro";
            this.checkAsignarMaestro.Size = new System.Drawing.Size(115, 19);
            this.checkAsignarMaestro.TabIndex = 12;
            this.checkAsignarMaestro.Text = "Asignar maestro";
            this.checkAsignarMaestro.UseVisualStyleBackColor = true;
            this.checkAsignarMaestro.CheckedChanged += new System.EventHandler(this.checkAsignarMaestro_CheckedChanged);
            // 
            // groupMaestro
            // 
            this.groupMaestro.Controls.Add(this.txtDesvioEstandar);
            this.groupMaestro.Controls.Add(this.label9);
            this.groupMaestro.Controls.Add(this.txtPorcHueso);
            this.groupMaestro.Controls.Add(this.label7);
            this.groupMaestro.Controls.Add(this.txtPorcentajeCorteM);
            this.groupMaestro.Controls.Add(this.label2);
            this.groupMaestro.Controls.Add(this.btnBuscarCorteM);
            this.groupMaestro.Controls.Add(this.txtCorteMaestro);
            this.groupMaestro.Controls.Add(this.label3);
            this.groupMaestro.Enabled = false;
            this.groupMaestro.ForeColor = System.Drawing.Color.Cornsilk;
            this.groupMaestro.Location = new System.Drawing.Point(260, 170);
            this.groupMaestro.Name = "groupMaestro";
            this.groupMaestro.Size = new System.Drawing.Size(310, 128);
            this.groupMaestro.TabIndex = 29;
            this.groupMaestro.TabStop = false;
            this.groupMaestro.Text = "Prod. Maestro";
            // 
            // txtDesvioEstandar
            // 
            this.txtDesvioEstandar.Enabled = false;
            this.txtDesvioEstandar.Location = new System.Drawing.Point(113, 102);
            this.txtDesvioEstandar.Name = "txtDesvioEstandar";
            this.txtDesvioEstandar.ReadOnly = true;
            this.txtDesvioEstandar.Size = new System.Drawing.Size(75, 21);
            this.txtDesvioEstandar.TabIndex = 34;
            this.txtDesvioEstandar.Text = "0";
            this.txtDesvioEstandar.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.txtDesvioEstandar.TextChanged += new System.EventHandler(this.txtCodigo_TextChanged);
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label9.ForeColor = System.Drawing.Color.Cornsilk;
            this.label9.Location = new System.Drawing.Point(14, 105);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(96, 15);
            this.label9.TabIndex = 37;
            this.label9.Text = "Desvío Estandar";
            // 
            // txtPorcHueso
            // 
            this.txtPorcHueso.Location = new System.Drawing.Point(113, 75);
            this.txtPorcHueso.Name = "txtPorcHueso";
            this.txtPorcHueso.Size = new System.Drawing.Size(75, 21);
            this.txtPorcHueso.TabIndex = 33;
            this.txtPorcHueso.Text = "0";
            this.txtPorcHueso.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.txtPorcHueso.TextChanged += new System.EventHandler(this.txtCodigo_TextChanged);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label7.ForeColor = System.Drawing.Color.Cornsilk;
            this.label7.Location = new System.Drawing.Point(23, 78);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(87, 15);
            this.label7.TabIndex = 36;
            this.label7.Text = "% Desperdicio";
            // 
            // txtPorcentajeCorteM
            // 
            this.txtPorcentajeCorteM.Location = new System.Drawing.Point(113, 48);
            this.txtPorcentajeCorteM.Name = "txtPorcentajeCorteM";
            this.txtPorcentajeCorteM.Size = new System.Drawing.Size(75, 21);
            this.txtPorcentajeCorteM.TabIndex = 32;
            this.txtPorcentajeCorteM.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.txtPorcentajeCorteM.TextChanged += new System.EventHandler(this.txtCodigo_TextChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.ForeColor = System.Drawing.Color.Cornsilk;
            this.label2.Location = new System.Drawing.Point(26, 51);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(81, 15);
            this.label2.TabIndex = 35;
            this.label2.Text = "% en Prod. M";
            // 
            // btnBuscarCorteM
            // 
            this.btnBuscarCorteM.AccessibleDescription = "";
            this.btnBuscarCorteM.ForeColor = System.Drawing.Color.Black;
            this.btnBuscarCorteM.Image = ((System.Drawing.Image)(resources.GetObject("btnBuscarCorteM.Image")));
            this.btnBuscarCorteM.Location = new System.Drawing.Point(270, 20);
            this.btnBuscarCorteM.Name = "btnBuscarCorteM";
            this.btnBuscarCorteM.Size = new System.Drawing.Size(28, 23);
            this.btnBuscarCorteM.TabIndex = 29;
            this.btnBuscarCorteM.UseVisualStyleBackColor = true;
            this.btnBuscarCorteM.Click += new System.EventHandler(this.btnBuscarCorteM_Click);
            // 
            // txtCorteMaestro
            // 
            this.txtCorteMaestro.Location = new System.Drawing.Point(113, 21);
            this.txtCorteMaestro.Name = "txtCorteMaestro";
            this.txtCorteMaestro.ReadOnly = true;
            this.txtCorteMaestro.Size = new System.Drawing.Size(157, 21);
            this.txtCorteMaestro.TabIndex = 30;
            this.txtCorteMaestro.TextChanged += new System.EventHandler(this.txtCodigo_TextChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.ForeColor = System.Drawing.Color.Cornsilk;
            this.label3.Location = new System.Drawing.Point(51, 24);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(56, 15);
            this.label3.TabIndex = 31;
            this.label3.Text = "Producto";
            // 
            // txtIndependiente
            // 
            this.txtIndependiente.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.txtIndependiente.AutoSize = true;
            this.txtIndependiente.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.txtIndependiente.Checked = true;
            this.txtIndependiente.CheckState = System.Windows.Forms.CheckState.Checked;
            this.txtIndependiente.Location = new System.Drawing.Point(355, 113);
            this.txtIndependiente.Name = "txtIndependiente";
            this.txtIndependiente.Size = new System.Drawing.Size(137, 19);
            this.txtIndependiente.TabIndex = 11;
            this.txtIndependiente.Text = "Prod. Independiente";
            this.txtIndependiente.UseVisualStyleBackColor = true;
            this.txtIndependiente.CheckedChanged += new System.EventHandler(this.txtIndependiente_CheckedChanged);
            // 
            // txtPrecioKg
            // 
            this.txtPrecioKg.Location = new System.Drawing.Point(93, 83);
            this.txtPrecioKg.Name = "txtPrecioKg";
            this.txtPrecioKg.Size = new System.Drawing.Size(132, 21);
            this.txtPrecioKg.TabIndex = 2;
            this.txtPrecioKg.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.txtPrecioKg.TextChanged += new System.EventHandler(this.txtCodigo_TextChanged);
            this.txtPrecioKg.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.TxtPruebaENTER_KeyPress);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.ForeColor = System.Drawing.Color.Cornsilk;
            this.label5.Location = new System.Drawing.Point(7, 86);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(85, 15);
            this.label5.TabIndex = 22;
            this.label5.Text = "Precio Kg./Un.";
            // 
            // comboTipo
            // 
            this.comboTipo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboTipo.FormattingEnabled = true;
            this.comboTipo.Location = new System.Drawing.Point(93, 138);
            this.comboTipo.Name = "comboTipo";
            this.comboTipo.Size = new System.Drawing.Size(132, 23);
            this.comboTipo.TabIndex = 3;
            this.comboTipo.SelectedIndexChanged += new System.EventHandler(this.comboTipo_SelectedIndexChanged);
            this.comboTipo.TextChanged += new System.EventHandler(this.txtCodigo_TextChanged);
            this.comboTipo.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.TxtPruebaENTER_KeyPress);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.ForeColor = System.Drawing.Color.Cornsilk;
            this.label6.Location = new System.Drawing.Point(55, 141);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(31, 15);
            this.label6.TabIndex = 19;
            this.label6.Text = "Tipo";
            // 
            // txtCodigo
            // 
            this.txtCodigo.Location = new System.Drawing.Point(93, 29);
            this.txtCodigo.Name = "txtCodigo";
            this.txtCodigo.Size = new System.Drawing.Size(132, 21);
            this.txtCodigo.TabIndex = 0;
            this.txtCodigo.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.txtCodigo.TextChanged += new System.EventHandler(this.txtCodigo_TextChanged);
            this.txtCodigo.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.TxtPruebaENTER_KeyPress);
            this.txtCodigo.Leave += new System.EventHandler(this.txtCodigo_Leave);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ForeColor = System.Drawing.Color.Cornsilk;
            this.label1.Location = new System.Drawing.Point(41, 32);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(46, 15);
            this.label1.TabIndex = 6;
            this.label1.Text = "Código";
            // 
            // txtDescCorte
            // 
            this.txtDescCorte.Location = new System.Drawing.Point(93, 56);
            this.txtDescCorte.Name = "txtDescCorte";
            this.txtDescCorte.Size = new System.Drawing.Size(228, 21);
            this.txtDescCorte.TabIndex = 1;
            this.txtDescCorte.TextChanged += new System.EventHandler(this.txtDescCorte_TextChanged);
            this.txtDescCorte.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.TxtPruebaENTER_KeyPress);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.ForeColor = System.Drawing.Color.Cornsilk;
            this.label4.Location = new System.Drawing.Point(50, 59);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(36, 15);
            this.label4.TabIndex = 2;
            this.label4.Text = "Prod.";
            // 
            // formNuevoCorte
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(597, 541);
            this.Controls.Add(this.btnGuardar);
            this.Controls.Add(this.btnCancelar);
            this.Controls.Add(this.pnlBuscar);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MaximizeBox = false;
            this.MinimizeBox = true;
            this.Name = "formNuevoCorte";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Nuevo Prod.";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.formNuevoCorte_FormClosed);
            this.Load += new System.EventHandler(this.formNuevoCorte_Load);
            this.pnlBuscar.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupProveedores.ResumeLayout(false);
            this.groupProveedores.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.grillaProveedores)).EndInit();
            this.groupMaestro.ResumeLayout(false);
            this.groupMaestro.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        protected System.Windows.Forms.Panel pnlBuscar;
        protected System.Windows.Forms.Button btnCancelar;
        protected System.Windows.Forms.GroupBox groupBox1;
        protected System.Windows.Forms.TextBox txtCodigo;
        protected System.Windows.Forms.Label label1;
        protected System.Windows.Forms.TextBox txtDescCorte;
        protected System.Windows.Forms.Label label4;
        protected System.Windows.Forms.Label label6;
        protected System.Windows.Forms.ComboBox comboTipo;
        protected System.Windows.Forms.TextBox txtPrecioKg;
        protected System.Windows.Forms.Label label5;
        private System.Windows.Forms.CheckBox txtIndependiente;
        protected System.Windows.Forms.Button btnGuardar;
        private System.Windows.Forms.GroupBox groupMaestro;
        private System.Windows.Forms.MaskedTextBox txtDesvioEstandar;
        protected System.Windows.Forms.Label label9;
        private System.Windows.Forms.MaskedTextBox txtPorcHueso;
        protected System.Windows.Forms.Label label7;
        private System.Windows.Forms.MaskedTextBox txtPorcentajeCorteM;
        protected System.Windows.Forms.Label label2;
        protected internal System.Windows.Forms.Button btnBuscarCorteM;
        protected System.Windows.Forms.TextBox txtCorteMaestro;
        protected System.Windows.Forms.Label label3;
        private System.Windows.Forms.CheckBox checkAsignarMaestro;
        private System.Windows.Forms.CheckBox checkEnCierreStock;
        private System.Windows.Forms.CheckBox checkIngresoRapidoEmbutido;
        protected System.Windows.Forms.TextBox txtPromedio;
        protected System.Windows.Forms.Label label8;
        private System.Windows.Forms.CheckBox checkHabilitado;
        protected System.Windows.Forms.ComboBox comboAlicuotaIva;
        protected System.Windows.Forms.Label label10;
        private System.Windows.Forms.CheckBox checkPesable;
        private System.Windows.Forms.CheckBox checkMayuscula;
        protected System.Windows.Forms.Label lblActualizar;
        protected System.Windows.Forms.TextBox txtPuntoStock;
        protected System.Windows.Forms.Label label11;
        private System.Windows.Forms.GroupBox groupProveedores;
        protected System.Windows.Forms.Label label12;
        private System.Windows.Forms.DataGridView grillaProveedores;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn3;
        private System.Windows.Forms.DataGridViewTextBoxColumn ultimoPrecio;
        private System.Windows.Forms.DataGridViewTextBoxColumn fechaUltimaCompra;
        protected System.Windows.Forms.Label label13;
        protected internal System.Windows.Forms.Button btnMarca;
        protected System.Windows.Forms.TextBox txtMarca;
        protected internal System.Windows.Forms.Button btnBorrarMarca;
        protected System.Windows.Forms.Label label20;
        protected System.Windows.Forms.Label label19;
        protected System.Windows.Forms.Label label18;
        protected System.Windows.Forms.Label label17;
        protected System.Windows.Forms.Label label16;
        protected System.Windows.Forms.Label label15;
        protected System.Windows.Forms.Label label14;
        private System.Windows.Forms.CheckBox checkSugerirCodigo;
    }
}