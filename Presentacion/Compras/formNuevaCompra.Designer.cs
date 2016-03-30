namespace Presentacion
{
    partial class formNuevaCompra
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(formNuevaCompra));
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle5 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle6 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle7 = new System.Windows.Forms.DataGridViewCellStyle();
            this.pnlBuscar = new System.Windows.Forms.Panel();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.radioCorte = new System.Windows.Forms.RadioButton();
            this.radioMediaRes = new System.Windows.Forms.RadioButton();
            this.radioIngresoStock = new System.Windows.Forms.RadioButton();
            this.txtUsuario = new System.Windows.Forms.TextBox();
            this.label16 = new System.Windows.Forms.Label();
            this.grupoMediaRes = new System.Windows.Forms.GroupBox();
            this.txtPrecioKg = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.btnQuitar = new System.Windows.Forms.Button();
            this.panelCorte = new System.Windows.Forms.Panel();
            this.btnBuscaCorte = new System.Windows.Forms.Button();
            this.txtCodigo = new System.Windows.Forms.MaskedTextBox();
            this.label10 = new System.Windows.Forms.Label();
            this.txtCantKgs = new System.Windows.Forms.TextBox();
            this.label13 = new System.Windows.Forms.Label();
            this.txtCorteNuevaCompra = new System.Windows.Forms.TextBox();
            this.label14 = new System.Windows.Forms.Label();
            this.btnAgregar = new System.Windows.Forms.Button();
            this.label6 = new System.Windows.Forms.Label();
            this.txtNroTropa = new System.Windows.Forms.TextBox();
            this.txtKgMedia = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.btnBuscarProv = new System.Windows.Forms.Button();
            this.txtProveedor = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.txtNroRemito = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.comboSucursal = new System.Windows.Forms.ComboBox();
            this.txtFechaCompra = new System.Windows.Forms.DateTimePicker();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.grillaMediaRes = new System.Windows.Forms.DataGridView();
            this.nroTropa = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.kgMedia = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.precioMedia = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.totalPs = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.sucursalM = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.btnAceptar = new System.Windows.Forms.Button();
            this.btnCancelar = new System.Windows.Forms.Button();
            this.label8 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.txtTotalKg = new System.Windows.Forms.TextBox();
            this.txtTotal = new System.Windows.Forms.TextBox();
            this.txtObservaciones = new System.Windows.Forms.TextBox();
            this.label11 = new System.Windows.Forms.Label();
            this.grillaCortePorCompra = new System.Windows.Forms.DataGridView();
            this.corte = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.idSucursal = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.cantKgs = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.precioKgs = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.totalS = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.sucursal = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.txtCantItems = new System.Windows.Forms.TextBox();
            this.label12 = new System.Windows.Forms.Label();
            this.pnlBuscar.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.grupoMediaRes.SuspendLayout();
            this.panelCorte.SuspendLayout();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.grillaMediaRes)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.grillaCortePorCompra)).BeginInit();
            this.SuspendLayout();
            // 
            // pnlBuscar
            // 
            this.pnlBuscar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.pnlBuscar.BackColor = System.Drawing.Color.SteelBlue;
            this.pnlBuscar.Controls.Add(this.groupBox2);
            this.pnlBuscar.Controls.Add(this.txtUsuario);
            this.pnlBuscar.Controls.Add(this.label16);
            this.pnlBuscar.Controls.Add(this.grupoMediaRes);
            this.pnlBuscar.Controls.Add(this.groupBox1);
            this.pnlBuscar.Location = new System.Drawing.Point(0, 0);
            this.pnlBuscar.Name = "pnlBuscar";
            this.pnlBuscar.Size = new System.Drawing.Size(856, 155);
            this.pnlBuscar.TabIndex = 3;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.radioCorte);
            this.groupBox2.Controls.Add(this.radioMediaRes);
            this.groupBox2.Controls.Add(this.radioIngresoStock);
            this.groupBox2.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox2.ForeColor = System.Drawing.Color.Cornsilk;
            this.groupBox2.Location = new System.Drawing.Point(707, 88);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(137, 63);
            this.groupBox2.TabIndex = 50;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Tipo de Compra";
            // 
            // radioCorte
            // 
            this.radioCorte.AutoSize = true;
            this.radioCorte.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.radioCorte.ForeColor = System.Drawing.Color.White;
            this.radioCorte.Location = new System.Drawing.Point(36, 38);
            this.radioCorte.Name = "radioCorte";
            this.radioCorte.Size = new System.Drawing.Size(60, 19);
            this.radioCorte.TabIndex = 100;
            this.radioCorte.Text = "Cortes";
            this.radioCorte.UseVisualStyleBackColor = true;
            this.radioCorte.CheckedChanged += new System.EventHandler(this.radioCorte_CheckedChanged);
            // 
            // radioMediaRes
            // 
            this.radioMediaRes.AutoSize = true;
            this.radioMediaRes.Checked = true;
            this.radioMediaRes.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.radioMediaRes.ForeColor = System.Drawing.Color.White;
            this.radioMediaRes.Location = new System.Drawing.Point(36, 20);
            this.radioMediaRes.Name = "radioMediaRes";
            this.radioMediaRes.Size = new System.Drawing.Size(85, 19);
            this.radioMediaRes.TabIndex = 12;
            this.radioMediaRes.TabStop = true;
            this.radioMediaRes.Text = "Media Res";
            this.radioMediaRes.UseVisualStyleBackColor = true;
            this.radioMediaRes.CheckedChanged += new System.EventHandler(this.radioMediaRes_CheckedChanged);
            // 
            // radioIngresoStock
            // 
            this.radioIngresoStock.AutoSize = true;
            this.radioIngresoStock.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.radioIngresoStock.ForeColor = System.Drawing.Color.White;
            this.radioIngresoStock.Location = new System.Drawing.Point(140, 55);
            this.radioIngresoStock.Name = "radioIngresoStock";
            this.radioIngresoStock.Size = new System.Drawing.Size(99, 19);
            this.radioIngresoStock.TabIndex = 0;
            this.radioIngresoStock.TabStop = true;
            this.radioIngresoStock.Text = "Ingreso Stock";
            this.radioIngresoStock.UseVisualStyleBackColor = true;
            this.radioIngresoStock.Visible = false;
            this.radioIngresoStock.CheckedChanged += new System.EventHandler(this.radioIngresoStock_CheckedChanged_1);
            // 
            // txtUsuario
            // 
            this.txtUsuario.Location = new System.Drawing.Point(707, 12);
            this.txtUsuario.Name = "txtUsuario";
            this.txtUsuario.ReadOnly = true;
            this.txtUsuario.Size = new System.Drawing.Size(140, 20);
            this.txtUsuario.TabIndex = 11;
            this.txtUsuario.TabStop = false;
            this.txtUsuario.TextChanged += new System.EventHandler(this.txtUsuario_TextChanged);
            // 
            // label16
            // 
            this.label16.AutoSize = true;
            this.label16.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label16.ForeColor = System.Drawing.Color.Cornsilk;
            this.label16.Location = new System.Drawing.Point(651, 13);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(50, 15);
            this.label16.TabIndex = 10;
            this.label16.Text = "Usuario";
            this.label16.Click += new System.EventHandler(this.label16_Click);
            // 
            // grupoMediaRes
            // 
            this.grupoMediaRes.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.grupoMediaRes.BackColor = System.Drawing.Color.SteelBlue;
            this.grupoMediaRes.Controls.Add(this.txtPrecioKg);
            this.grupoMediaRes.Controls.Add(this.label7);
            this.grupoMediaRes.Controls.Add(this.btnQuitar);
            this.grupoMediaRes.Controls.Add(this.panelCorte);
            this.grupoMediaRes.Controls.Add(this.btnAgregar);
            this.grupoMediaRes.Controls.Add(this.label6);
            this.grupoMediaRes.Controls.Add(this.txtNroTropa);
            this.grupoMediaRes.Controls.Add(this.txtKgMedia);
            this.grupoMediaRes.Controls.Add(this.label5);
            this.grupoMediaRes.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.grupoMediaRes.ForeColor = System.Drawing.Color.Cornsilk;
            this.grupoMediaRes.Location = new System.Drawing.Point(19, 90);
            this.grupoMediaRes.Name = "grupoMediaRes";
            this.grupoMediaRes.Size = new System.Drawing.Size(682, 61);
            this.grupoMediaRes.TabIndex = 10;
            this.grupoMediaRes.TabStop = false;
            this.grupoMediaRes.Text = "Media Res";
            // 
            // txtPrecioKg
            // 
            this.txtPrecioKg.Location = new System.Drawing.Point(478, 27);
            this.txtPrecioKg.Name = "txtPrecioKg";
            this.txtPrecioKg.Size = new System.Drawing.Size(74, 21);
            this.txtPrecioKg.TabIndex = 2;
            this.txtPrecioKg.TabStop = false;
            this.txtPrecioKg.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.txtPrecioKg.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.TxtPruebaENTER_KeyPress);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label7.ForeColor = System.Drawing.Color.Cornsilk;
            this.label7.Location = new System.Drawing.Point(413, 30);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(60, 15);
            this.label7.TabIndex = 15;
            this.label7.Text = "Precio Kg";
            // 
            // btnQuitar
            // 
            this.btnQuitar.ForeColor = System.Drawing.Color.Black;
            this.btnQuitar.Image = ((System.Drawing.Image)(resources.GetObject("btnQuitar.Image")));
            this.btnQuitar.Location = new System.Drawing.Point(619, 24);
            this.btnQuitar.Name = "btnQuitar";
            this.btnQuitar.Size = new System.Drawing.Size(55, 26);
            this.btnQuitar.TabIndex = 7;
            this.btnQuitar.TabStop = false;
            this.btnQuitar.UseVisualStyleBackColor = true;
            this.btnQuitar.Click += new System.EventHandler(this.btnQuitar_Click);
            // 
            // panelCorte
            // 
            this.panelCorte.Controls.Add(this.btnBuscaCorte);
            this.panelCorte.Controls.Add(this.txtCodigo);
            this.panelCorte.Controls.Add(this.label10);
            this.panelCorte.Controls.Add(this.txtCantKgs);
            this.panelCorte.Controls.Add(this.label13);
            this.panelCorte.Controls.Add(this.txtCorteNuevaCompra);
            this.panelCorte.Controls.Add(this.label14);
            this.panelCorte.Location = new System.Drawing.Point(9, 20);
            this.panelCorte.Name = "panelCorte";
            this.panelCorte.Size = new System.Drawing.Size(402, 37);
            this.panelCorte.TabIndex = 17;
            // 
            // btnBuscaCorte
            // 
            this.btnBuscaCorte.AccessibleDescription = "";
            this.btnBuscaCorte.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.btnBuscaCorte.ForeColor = System.Drawing.Color.Black;
            this.btnBuscaCorte.Image = ((System.Drawing.Image)(resources.GetObject("btnBuscaCorte.Image")));
            this.btnBuscaCorte.Location = new System.Drawing.Point(112, 6);
            this.btnBuscaCorte.Name = "btnBuscaCorte";
            this.btnBuscaCorte.Size = new System.Drawing.Size(28, 23);
            this.btnBuscaCorte.TabIndex = 0;
            this.btnBuscaCorte.TabStop = false;
            this.btnBuscaCorte.UseVisualStyleBackColor = true;
            this.btnBuscaCorte.Click += new System.EventHandler(this.btnBuscaCorte_Click);
            // 
            // txtCodigo
            // 
            this.txtCodigo.Location = new System.Drawing.Point(66, 7);
            this.txtCodigo.Name = "txtCodigo";
            this.txtCodigo.Size = new System.Drawing.Size(40, 21);
            this.txtCodigo.TabIndex = 0;
            this.txtCodigo.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.txtCodigo.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.TxtPruebaENTER_KeyPress);
            this.txtCodigo.TextChanged += new System.EventHandler(this.txtCodigo_TextChanged);
            // 
            // label10
            // 
            this.label10.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.label10.AutoSize = true;
            this.label10.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label10.ForeColor = System.Drawing.Color.Cornsilk;
            this.label10.Location = new System.Drawing.Point(14, 10);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(46, 15);
            this.label10.TabIndex = 41;
            this.label10.Text = "Código";
            // 
            // txtCantKgs
            // 
            this.txtCantKgs.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtCantKgs.Location = new System.Drawing.Point(338, 7);
            this.txtCantKgs.Name = "txtCantKgs";
            this.txtCantKgs.Size = new System.Drawing.Size(60, 21);
            this.txtCantKgs.TabIndex = 1;
            this.txtCantKgs.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.txtCantKgs.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.TxtPruebaENTER_KeyPress);
            // 
            // label13
            // 
            this.label13.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.label13.AutoSize = true;
            this.label13.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label13.ForeColor = System.Drawing.Color.Cornsilk;
            this.label13.Location = new System.Drawing.Point(305, 10);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(28, 15);
            this.label13.TabIndex = 26;
            this.label13.Text = "Kgs";
            // 
            // txtCorteNuevaCompra
            // 
            this.txtCorteNuevaCompra.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.txtCorteNuevaCompra.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
            this.txtCorteNuevaCompra.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.CustomSource;
            this.txtCorteNuevaCompra.Location = new System.Drawing.Point(185, 7);
            this.txtCorteNuevaCompra.Name = "txtCorteNuevaCompra";
            this.txtCorteNuevaCompra.Size = new System.Drawing.Size(112, 21);
            this.txtCorteNuevaCompra.TabIndex = 0;
            this.txtCorteNuevaCompra.TabStop = false;
            this.txtCorteNuevaCompra.TextChanged += new System.EventHandler(this.txtCorteNuevaCompra_TextChanged);
            // 
            // label14
            // 
            this.label14.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.label14.AutoSize = true;
            this.label14.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label14.ForeColor = System.Drawing.Color.Cornsilk;
            this.label14.Location = new System.Drawing.Point(143, 10);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(36, 15);
            this.label14.TabIndex = 24;
            this.label14.Text = "Corte";
            // 
            // btnAgregar
            // 
            this.btnAgregar.AccessibleDescription = "";
            this.btnAgregar.ForeColor = System.Drawing.Color.Black;
            this.btnAgregar.Image = ((System.Drawing.Image)(resources.GetObject("btnAgregar.Image")));
            this.btnAgregar.Location = new System.Drawing.Point(558, 24);
            this.btnAgregar.Name = "btnAgregar";
            this.btnAgregar.Size = new System.Drawing.Size(55, 26);
            this.btnAgregar.TabIndex = 3;
            this.btnAgregar.UseVisualStyleBackColor = true;
            this.btnAgregar.Click += new System.EventHandler(this.btnAgregar_Click);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.ForeColor = System.Drawing.Color.Cornsilk;
            this.label6.Location = new System.Drawing.Point(59, 30);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(62, 15);
            this.label6.TabIndex = 11;
            this.label6.Text = "Nro Tropa";
            // 
            // txtNroTropa
            // 
            this.txtNroTropa.Location = new System.Drawing.Point(127, 27);
            this.txtNroTropa.Name = "txtNroTropa";
            this.txtNroTropa.Size = new System.Drawing.Size(77, 21);
            this.txtNroTropa.TabIndex = 0;
            this.txtNroTropa.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.txtNroTropa.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.TxtPruebaENTER_KeyPress);
            // 
            // txtKgMedia
            // 
            this.txtKgMedia.Location = new System.Drawing.Point(297, 27);
            this.txtKgMedia.Name = "txtKgMedia";
            this.txtKgMedia.Size = new System.Drawing.Size(97, 21);
            this.txtKgMedia.TabIndex = 1;
            this.txtKgMedia.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.txtKgMedia.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.TxtPruebaENTER_KeyPress);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.ForeColor = System.Drawing.Color.Cornsilk;
            this.label5.Location = new System.Drawing.Point(222, 30);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(69, 15);
            this.label5.TabIndex = 6;
            this.label5.Text = "Kgs. Media";
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.groupBox1.BackColor = System.Drawing.Color.SteelBlue;
            this.groupBox1.Controls.Add(this.btnBuscarProv);
            this.groupBox1.Controls.Add(this.txtProveedor);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.txtNroRemito);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.comboSucursal);
            this.groupBox1.Controls.Add(this.txtFechaCompra);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox1.ForeColor = System.Drawing.Color.Cornsilk;
            this.groupBox1.Location = new System.Drawing.Point(19, 5);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(569, 77);
            this.groupBox1.TabIndex = 9;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Remito";
            // 
            // btnBuscarProv
            // 
            this.btnBuscarProv.AccessibleDescription = "";
            this.btnBuscarProv.ForeColor = System.Drawing.Color.Black;
            this.btnBuscarProv.Image = ((System.Drawing.Image)(resources.GetObject("btnBuscarProv.Image")));
            this.btnBuscarProv.Location = new System.Drawing.Point(259, 48);
            this.btnBuscarProv.Name = "btnBuscarProv";
            this.btnBuscarProv.Size = new System.Drawing.Size(28, 23);
            this.btnBuscarProv.TabIndex = 0;
            this.btnBuscarProv.TabStop = false;
            this.btnBuscarProv.UseVisualStyleBackColor = true;
            this.btnBuscarProv.Click += new System.EventHandler(this.btnBuscarProv_Click);
            // 
            // txtProveedor
            // 
            this.txtProveedor.Location = new System.Drawing.Point(95, 49);
            this.txtProveedor.Name = "txtProveedor";
            this.txtProveedor.ReadOnly = true;
            this.txtProveedor.Size = new System.Drawing.Size(158, 21);
            this.txtProveedor.TabIndex = 9;
            this.txtProveedor.TabStop = false;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ForeColor = System.Drawing.Color.Cornsilk;
            this.label1.Location = new System.Drawing.Point(26, 52);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(63, 15);
            this.label1.TabIndex = 8;
            this.label1.Text = "Proveedor";
            // 
            // txtNroRemito
            // 
            this.txtNroRemito.Location = new System.Drawing.Point(373, 50);
            this.txtNroRemito.Name = "txtNroRemito";
            this.txtNroRemito.Size = new System.Drawing.Size(144, 21);
            this.txtNroRemito.TabIndex = 3;
            this.txtNroRemito.TabStop = false;
            this.txtNroRemito.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.ForeColor = System.Drawing.Color.Cornsilk;
            this.label2.Location = new System.Drawing.Point(294, 53);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(70, 15);
            this.label2.TabIndex = 6;
            this.label2.Text = "Nro Remito";
            // 
            // comboSucursal
            // 
            this.comboSucursal.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboSucursal.FormattingEnabled = true;
            this.comboSucursal.Location = new System.Drawing.Point(95, 18);
            this.comboSucursal.Name = "comboSucursal";
            this.comboSucursal.Size = new System.Drawing.Size(158, 23);
            this.comboSucursal.TabIndex = 3;
            this.comboSucursal.TabStop = false;
            this.comboSucursal.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.TxtPruebaENTER_KeyPress);
            // 
            // txtFechaCompra
            // 
            this.txtFechaCompra.Checked = false;
            this.txtFechaCompra.CustomFormat = "dd/MM/yyyy  HH:mm";
            this.txtFechaCompra.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.txtFechaCompra.Location = new System.Drawing.Point(373, 23);
            this.txtFechaCompra.Name = "txtFechaCompra";
            this.txtFechaCompra.Size = new System.Drawing.Size(144, 21);
            this.txtFechaCompra.TabIndex = 1;
            this.txtFechaCompra.TabStop = false;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.ForeColor = System.Drawing.Color.Cornsilk;
            this.label4.Location = new System.Drawing.Point(34, 22);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(55, 15);
            this.label4.TabIndex = 8;
            this.label4.Text = "Sucursal";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.ForeColor = System.Drawing.Color.Cornsilk;
            this.label3.Location = new System.Drawing.Point(324, 26);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(41, 15);
            this.label3.TabIndex = 4;
            this.label3.Text = "Fecha";
            // 
            // grillaMediaRes
            // 
            this.grillaMediaRes.AllowUserToAddRows = false;
            this.grillaMediaRes.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.grillaMediaRes.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.grillaMediaRes.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.grillaMediaRes.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.nroTropa,
            this.kgMedia,
            this.precioMedia,
            this.totalPs,
            this.sucursalM});
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
            dataGridViewCellStyle3.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.grillaMediaRes.DefaultCellStyle = dataGridViewCellStyle3;
            this.grillaMediaRes.Location = new System.Drawing.Point(20, 161);
            this.grillaMediaRes.MultiSelect = false;
            this.grillaMediaRes.Name = "grillaMediaRes";
            this.grillaMediaRes.ReadOnly = true;
            this.grillaMediaRes.RowHeadersVisible = false;
            this.grillaMediaRes.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.grillaMediaRes.Size = new System.Drawing.Size(681, 336);
            this.grillaMediaRes.TabIndex = 4;
            // 
            // nroTropa
            // 
            this.nroTropa.DataPropertyName = "nroTropa";
            this.nroTropa.HeaderText = "Nro Tropa";
            this.nroTropa.Name = "nroTropa";
            this.nroTropa.ReadOnly = true;
            // 
            // kgMedia
            // 
            this.kgMedia.DataPropertyName = "kgMedia";
            this.kgMedia.HeaderText = "Kgs. Media";
            this.kgMedia.Name = "kgMedia";
            this.kgMedia.ReadOnly = true;
            // 
            // precioMedia
            // 
            this.precioMedia.DataPropertyName = "precioMedia";
            this.precioMedia.HeaderText = "Precio Kg. ";
            this.precioMedia.Name = "precioMedia";
            this.precioMedia.ReadOnly = true;
            // 
            // totalPs
            // 
            this.totalPs.DataPropertyName = "totalS";
            dataGridViewCellStyle1.Format = "N2";
            dataGridViewCellStyle1.NullValue = null;
            this.totalPs.DefaultCellStyle = dataGridViewCellStyle1;
            this.totalPs.HeaderText = "Total $";
            this.totalPs.Name = "totalPs";
            this.totalPs.ReadOnly = true;
            // 
            // sucursalM
            // 
            this.sucursalM.DataPropertyName = "sucursal";
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            this.sucursalM.DefaultCellStyle = dataGridViewCellStyle2;
            this.sucursalM.HeaderText = "Sucursal";
            this.sucursalM.Name = "sucursalM";
            this.sucursalM.ReadOnly = true;
            this.sucursalM.Visible = false;
            // 
            // btnAceptar
            // 
            this.btnAceptar.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnAceptar.BackColor = System.Drawing.Color.SeaGreen;
            this.btnAceptar.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnAceptar.ForeColor = System.Drawing.SystemColors.ControlLight;
            this.btnAceptar.Location = new System.Drawing.Point(707, 430);
            this.btnAceptar.Name = "btnAceptar";
            this.btnAceptar.Size = new System.Drawing.Size(140, 35);
            this.btnAceptar.TabIndex = 10;
            this.btnAceptar.TabStop = false;
            this.btnAceptar.Text = "Aceptar";
            this.btnAceptar.UseVisualStyleBackColor = false;
            this.btnAceptar.Click += new System.EventHandler(this.btnAceptar_Click);
            this.btnAceptar.Leave += new System.EventHandler(this.btnAceptar_Leave);
            this.btnAceptar.Enter += new System.EventHandler(this.btnAceptar_Enter);
            // 
            // btnCancelar
            // 
            this.btnCancelar.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancelar.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.btnCancelar.Location = new System.Drawing.Point(707, 471);
            this.btnCancelar.Name = "btnCancelar";
            this.btnCancelar.Size = new System.Drawing.Size(140, 26);
            this.btnCancelar.TabIndex = 11;
            this.btnCancelar.TabStop = false;
            this.btnCancelar.Text = "Cancelar";
            this.btnCancelar.UseVisualStyleBackColor = true;
            this.btnCancelar.Click += new System.EventHandler(this.btnCancelar_Click);
            // 
            // label8
            // 
            this.label8.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.label8.AutoSize = true;
            this.label8.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label8.Location = new System.Drawing.Point(707, 209);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(60, 15);
            this.label8.TabIndex = 7;
            this.label8.Text = "Total Kg";
            // 
            // label9
            // 
            this.label9.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.label9.AutoSize = true;
            this.label9.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label9.Location = new System.Drawing.Point(707, 251);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(51, 15);
            this.label9.TabIndex = 8;
            this.label9.Text = "Total $";
            // 
            // txtTotalKg
            // 
            this.txtTotalKg.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.txtTotalKg.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtTotalKg.Location = new System.Drawing.Point(707, 227);
            this.txtTotalKg.Name = "txtTotalKg";
            this.txtTotalKg.ReadOnly = true;
            this.txtTotalKg.Size = new System.Drawing.Size(137, 21);
            this.txtTotalKg.TabIndex = 9;
            this.txtTotalKg.TabStop = false;
            this.txtTotalKg.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // txtTotal
            // 
            this.txtTotal.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.txtTotal.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtTotal.Location = new System.Drawing.Point(708, 269);
            this.txtTotal.Name = "txtTotal";
            this.txtTotal.ReadOnly = true;
            this.txtTotal.Size = new System.Drawing.Size(137, 21);
            this.txtTotal.TabIndex = 10;
            this.txtTotal.TabStop = false;
            this.txtTotal.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // txtObservaciones
            // 
            this.txtObservaciones.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.txtObservaciones.Location = new System.Drawing.Point(707, 309);
            this.txtObservaciones.Multiline = true;
            this.txtObservaciones.Name = "txtObservaciones";
            this.txtObservaciones.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtObservaciones.Size = new System.Drawing.Size(140, 115);
            this.txtObservaciones.TabIndex = 9;
            this.txtObservaciones.TabStop = false;
            // 
            // label11
            // 
            this.label11.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(704, 293);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(78, 13);
            this.label11.TabIndex = 15;
            this.label11.Text = "Observaciones";
            // 
            // grillaCortePorCompra
            // 
            this.grillaCortePorCompra.AllowUserToAddRows = false;
            this.grillaCortePorCompra.AllowUserToResizeRows = false;
            this.grillaCortePorCompra.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.grillaCortePorCompra.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.grillaCortePorCompra.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.grillaCortePorCompra.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.corte,
            this.idSucursal,
            this.cantKgs,
            this.precioKgs,
            this.totalS,
            this.sucursal});
            this.grillaCortePorCompra.Location = new System.Drawing.Point(20, 161);
            this.grillaCortePorCompra.MultiSelect = false;
            this.grillaCortePorCompra.Name = "grillaCortePorCompra";
            this.grillaCortePorCompra.ReadOnly = true;
            this.grillaCortePorCompra.RowHeadersVisible = false;
            this.grillaCortePorCompra.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.grillaCortePorCompra.Size = new System.Drawing.Size(681, 336);
            this.grillaCortePorCompra.TabIndex = 16;
            this.grillaCortePorCompra.TabStop = false;
            // 
            // corte
            // 
            this.corte.DataPropertyName = "Codigo";
            dataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
            this.corte.DefaultCellStyle = dataGridViewCellStyle4;
            this.corte.HeaderText = "Codigo";
            this.corte.Name = "corte";
            this.corte.ReadOnly = true;
            // 
            // idSucursal
            // 
            this.idSucursal.DataPropertyName = "Corte";
            this.idSucursal.HeaderText = "Corte";
            this.idSucursal.Name = "idSucursal";
            this.idSucursal.ReadOnly = true;
            // 
            // cantKgs
            // 
            this.cantKgs.DataPropertyName = "cantKgs";
            dataGridViewCellStyle5.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
            dataGridViewCellStyle5.Format = "N3";
            dataGridViewCellStyle5.NullValue = null;
            this.cantKgs.DefaultCellStyle = dataGridViewCellStyle5;
            this.cantKgs.HeaderText = "Cant. Kgs";
            this.cantKgs.Name = "cantKgs";
            this.cantKgs.ReadOnly = true;
            // 
            // precioKgs
            // 
            this.precioKgs.DataPropertyName = "precioKg";
            dataGridViewCellStyle6.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
            dataGridViewCellStyle6.Format = "N2";
            dataGridViewCellStyle6.NullValue = null;
            this.precioKgs.DefaultCellStyle = dataGridViewCellStyle6;
            this.precioKgs.HeaderText = "Precio Kg.";
            this.precioKgs.Name = "precioKgs";
            this.precioKgs.ReadOnly = true;
            // 
            // totalS
            // 
            this.totalS.DataPropertyName = "TotalS";
            dataGridViewCellStyle7.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
            dataGridViewCellStyle7.Format = "N2";
            dataGridViewCellStyle7.NullValue = null;
            this.totalS.DefaultCellStyle = dataGridViewCellStyle7;
            this.totalS.HeaderText = "Total $";
            this.totalS.Name = "totalS";
            this.totalS.ReadOnly = true;
            // 
            // sucursal
            // 
            this.sucursal.DataPropertyName = "Sucursal";
            this.sucursal.HeaderText = "Sucursal";
            this.sucursal.Name = "sucursal";
            this.sucursal.ReadOnly = true;
            this.sucursal.Visible = false;
            // 
            // txtCantItems
            // 
            this.txtCantItems.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.txtCantItems.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtCantItems.Location = new System.Drawing.Point(707, 182);
            this.txtCantItems.Name = "txtCantItems";
            this.txtCantItems.ReadOnly = true;
            this.txtCantItems.Size = new System.Drawing.Size(137, 21);
            this.txtCantItems.TabIndex = 34;
            this.txtCantItems.TabStop = false;
            this.txtCantItems.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label12
            // 
            this.label12.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.label12.AutoSize = true;
            this.label12.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label12.Location = new System.Drawing.Point(707, 164);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(79, 15);
            this.label12.TabIndex = 33;
            this.label12.Text = "Cant. Items";
            // 
            // formNuevaCompra
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Bisque;
            this.ClientSize = new System.Drawing.Size(856, 509);
            this.Controls.Add(this.txtCantItems);
            this.Controls.Add(this.label12);
            this.Controls.Add(this.label11);
            this.Controls.Add(this.txtObservaciones);
            this.Controls.Add(this.txtTotal);
            this.Controls.Add(this.txtTotalKg);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.btnAceptar);
            this.Controls.Add(this.btnCancelar);
            this.Controls.Add(this.pnlBuscar);
            this.Controls.Add(this.grillaCortePorCompra);
            this.Controls.Add(this.grillaMediaRes);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "formNuevaCompra";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Nueva Compra";
            this.Load += new System.EventHandler(this.formNuevaCompra_Load);
            this.pnlBuscar.ResumeLayout(false);
            this.pnlBuscar.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.grupoMediaRes.ResumeLayout(false);
            this.grupoMediaRes.PerformLayout();
            this.panelCorte.ResumeLayout(false);
            this.panelCorte.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.grillaMediaRes)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.grillaCortePorCompra)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        protected System.Windows.Forms.Panel pnlBuscar;
        protected System.Windows.Forms.GroupBox groupBox1;
        protected System.Windows.Forms.DateTimePicker txtFechaCompra;
        protected System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtNroRemito;
        protected System.Windows.Forms.Label label2;
        protected System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtProveedor;
        protected System.Windows.Forms.GroupBox grupoMediaRes;
        protected System.Windows.Forms.Label label6;
        private System.Windows.Forms.ComboBox comboSucursal;
        private System.Windows.Forms.TextBox txtNroTropa;
        protected System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox txtKgMedia;
        protected System.Windows.Forms.Label label5;
        private System.Windows.Forms.Button btnAgregar;
        private System.Windows.Forms.Button btnQuitar;
        protected System.Windows.Forms.Label label7;
        private System.Windows.Forms.DataGridView grillaMediaRes;
        protected System.Windows.Forms.Button btnAceptar;
        protected System.Windows.Forms.Button btnCancelar;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.TextBox txtTotalKg;
        private System.Windows.Forms.TextBox txtTotal;
        private System.Windows.Forms.Button btnBuscarProv;
        private System.Windows.Forms.TextBox txtObservaciones;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.RadioButton radioCorte;
        private System.Windows.Forms.RadioButton radioMediaRes;
        private System.Windows.Forms.Panel panelCorte;
        protected internal System.Windows.Forms.Button btnBuscaCorte;
        protected System.Windows.Forms.Label label13;
        protected System.Windows.Forms.TextBox txtCorteNuevaCompra;
        protected System.Windows.Forms.Label label14;
        private System.Windows.Forms.TextBox txtPrecioKg;
        private System.Windows.Forms.DataGridView grillaCortePorCompra;
        private System.Windows.Forms.RadioButton radioIngresoStock;
        private System.Windows.Forms.TextBox txtCantKgs;
        private System.Windows.Forms.TextBox txtCantItems;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.MaskedTextBox txtCodigo;
        protected System.Windows.Forms.Label label10;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.TextBox txtUsuario;
        protected System.Windows.Forms.Label label16;
        private System.Windows.Forms.DataGridViewTextBoxColumn corte;
        private System.Windows.Forms.DataGridViewTextBoxColumn idSucursal;
        private System.Windows.Forms.DataGridViewTextBoxColumn cantKgs;
        private System.Windows.Forms.DataGridViewTextBoxColumn precioKgs;
        private System.Windows.Forms.DataGridViewTextBoxColumn totalS;
        private System.Windows.Forms.DataGridViewTextBoxColumn sucursal;
        private System.Windows.Forms.DataGridViewTextBoxColumn nroTropa;
        private System.Windows.Forms.DataGridViewTextBoxColumn kgMedia;
        private System.Windows.Forms.DataGridViewTextBoxColumn precioMedia;
        private System.Windows.Forms.DataGridViewTextBoxColumn totalPs;
        private System.Windows.Forms.DataGridViewTextBoxColumn sucursalM;
    }
}