namespace Presentacion.Pagos
{
    partial class formAddOrEditPago
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(formAddOrEditPago));
            this.pnlBuscar = new System.Windows.Forms.Panel();
            this.txtSucursal = new System.Windows.Forms.TextBox();
            this.txtUsuario = new System.Windows.Forms.TextBox();
            this.comboSucursal = new System.Windows.Forms.ComboBox();
            this.label13 = new System.Windows.Forms.Label();
            this.label16 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.txtIdPago = new System.Windows.Forms.TextBox();
            this.label12 = new System.Windows.Forms.Label();
            this.panelCheque = new System.Windows.Forms.Panel();
            this.txtTitular = new System.Windows.Forms.TextBox();
            this.label10 = new System.Windows.Forms.Label();
            this.txtNroCheque = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.txtBanco = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.txtModifPor = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.txtModificado = new System.Windows.Forms.TextBox();
            this.txtCreado = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.txtCreadoPor = new System.Windows.Forms.TextBox();
            this.checkAProveedor = new System.Windows.Forms.CheckBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.label11 = new System.Windows.Forms.Label();
            this.txtImporte = new System.Windows.Forms.TextBox();
            this.txtObservaciones = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.btnBuscarProv = new System.Windows.Forms.Button();
            this.txtPersona = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.txtNroRecibo = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.txtFechaPago = new System.Windows.Forms.DateTimePicker();
            this.label3 = new System.Windows.Forms.Label();
            this.comboTipoPago = new System.Windows.Forms.ComboBox();
            this.label9 = new System.Windows.Forms.Label();
            this.btnGuardar = new System.Windows.Forms.Button();
            this.btnCancelar = new System.Windows.Forms.Button();
            this.idPagoLabel = new System.Windows.Forms.Label();
            this.pnlBuscar.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.panelCheque.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlBuscar
            // 
            this.pnlBuscar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.pnlBuscar.BackColor = System.Drawing.Color.SteelBlue;
            this.pnlBuscar.Controls.Add(this.txtSucursal);
            this.pnlBuscar.Controls.Add(this.txtUsuario);
            this.pnlBuscar.Controls.Add(this.comboSucursal);
            this.pnlBuscar.Controls.Add(this.label13);
            this.pnlBuscar.Controls.Add(this.label16);
            this.pnlBuscar.Controls.Add(this.groupBox1);
            this.pnlBuscar.Location = new System.Drawing.Point(-2, 0);
            this.pnlBuscar.Name = "pnlBuscar";
            this.pnlBuscar.Size = new System.Drawing.Size(479, 422);
            this.pnlBuscar.TabIndex = 24;
            // 
            // txtSucursal
            // 
            this.txtSucursal.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtSucursal.Location = new System.Drawing.Point(76, 9);
            this.txtSucursal.Name = "txtSucursal";
            this.txtSucursal.ReadOnly = true;
            this.txtSucursal.Size = new System.Drawing.Size(158, 22);
            this.txtSucursal.TabIndex = 57;
            this.txtSucursal.TabStop = false;
            this.txtSucursal.Visible = false;
            // 
            // txtUsuario
            // 
            this.txtUsuario.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtUsuario.Location = new System.Drawing.Point(304, 9);
            this.txtUsuario.Name = "txtUsuario";
            this.txtUsuario.ReadOnly = true;
            this.txtUsuario.Size = new System.Drawing.Size(158, 22);
            this.txtUsuario.TabIndex = 56;
            this.txtUsuario.TabStop = false;
            // 
            // comboSucursal
            // 
            this.comboSucursal.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboSucursal.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.comboSucursal.FormattingEnabled = true;
            this.comboSucursal.Location = new System.Drawing.Point(76, 8);
            this.comboSucursal.Name = "comboSucursal";
            this.comboSucursal.Size = new System.Drawing.Size(158, 24);
            this.comboSucursal.TabIndex = 52;
            this.comboSucursal.TabStop = false;
            this.comboSucursal.SelectedValueChanged += new System.EventHandler(this.comboSucursal_SelectedValueChanged);
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label13.ForeColor = System.Drawing.Color.Cornsilk;
            this.label13.Location = new System.Drawing.Point(15, 12);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(60, 16);
            this.label13.TabIndex = 54;
            this.label13.Text = "Sucursal";
            // 
            // label16
            // 
            this.label16.AutoSize = true;
            this.label16.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label16.ForeColor = System.Drawing.Color.Cornsilk;
            this.label16.Location = new System.Drawing.Point(248, 10);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(55, 16);
            this.label16.TabIndex = 55;
            this.label16.Text = "Usuario";
            // 
            // groupBox1
            // 
            this.groupBox1.BackColor = System.Drawing.Color.SteelBlue;
            this.groupBox1.Controls.Add(this.txtIdPago);
            this.groupBox1.Controls.Add(this.label12);
            this.groupBox1.Controls.Add(this.panelCheque);
            this.groupBox1.Controls.Add(this.txtModifPor);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.txtModificado);
            this.groupBox1.Controls.Add(this.txtCreado);
            this.groupBox1.Controls.Add(this.label6);
            this.groupBox1.Controls.Add(this.txtCreadoPor);
            this.groupBox1.Controls.Add(this.checkAProveedor);
            this.groupBox1.Controls.Add(this.groupBox2);
            this.groupBox1.Controls.Add(this.label11);
            this.groupBox1.Controls.Add(this.txtImporte);
            this.groupBox1.Controls.Add(this.txtObservaciones);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.btnBuscarProv);
            this.groupBox1.Controls.Add(this.txtPersona);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.txtNroRecibo);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.txtFechaPago);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.comboTipoPago);
            this.groupBox1.Controls.Add(this.label9);
            this.groupBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox1.ForeColor = System.Drawing.Color.Cornsilk;
            this.groupBox1.Location = new System.Drawing.Point(11, 38);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(458, 374);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Recibo";
            // 
            // txtIdPago
            // 
            this.txtIdPago.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtIdPago.Location = new System.Drawing.Point(98, 26);
            this.txtIdPago.Name = "txtIdPago";
            this.txtIdPago.ReadOnly = true;
            this.txtIdPago.Size = new System.Drawing.Size(158, 22);
            this.txtIdPago.TabIndex = 62;
            this.txtIdPago.TabStop = false;
            this.txtIdPago.Text = "-";
            this.txtIdPago.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label12.ForeColor = System.Drawing.Color.Cornsilk;
            this.label12.Location = new System.Drawing.Point(71, 29);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(21, 16);
            this.label12.TabIndex = 61;
            this.label12.Text = "ID";
            // 
            // panelCheque
            // 
            this.panelCheque.Controls.Add(this.txtTitular);
            this.panelCheque.Controls.Add(this.label10);
            this.panelCheque.Controls.Add(this.txtNroCheque);
            this.panelCheque.Controls.Add(this.label8);
            this.panelCheque.Controls.Add(this.txtBanco);
            this.panelCheque.Controls.Add(this.label7);
            this.panelCheque.Location = new System.Drawing.Point(6, 170);
            this.panelCheque.Name = "panelCheque";
            this.panelCheque.Size = new System.Drawing.Size(445, 61);
            this.panelCheque.TabIndex = 60;
            this.panelCheque.Visible = false;
            // 
            // txtTitular
            // 
            this.txtTitular.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtTitular.Location = new System.Drawing.Point(92, 31);
            this.txtTitular.Name = "txtTitular";
            this.txtTitular.Size = new System.Drawing.Size(125, 22);
            this.txtTitular.TabIndex = 33;
            this.txtTitular.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label10.ForeColor = System.Drawing.Color.Cornsilk;
            this.label10.Location = new System.Drawing.Point(38, 34);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(45, 16);
            this.label10.TabIndex = 34;
            this.label10.Text = "Titular";
            // 
            // txtNroCheque
            // 
            this.txtNroCheque.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtNroCheque.Location = new System.Drawing.Point(314, 3);
            this.txtNroCheque.Name = "txtNroCheque";
            this.txtNroCheque.Size = new System.Drawing.Size(125, 22);
            this.txtNroCheque.TabIndex = 31;
            this.txtNroCheque.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label8.ForeColor = System.Drawing.Color.Cornsilk;
            this.label8.Location = new System.Drawing.Point(243, 6);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(69, 16);
            this.label8.TabIndex = 32;
            this.label8.Text = "N°Cheque";
            // 
            // txtBanco
            // 
            this.txtBanco.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtBanco.Location = new System.Drawing.Point(92, 3);
            this.txtBanco.Name = "txtBanco";
            this.txtBanco.Size = new System.Drawing.Size(125, 22);
            this.txtBanco.TabIndex = 29;
            this.txtBanco.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label7.ForeColor = System.Drawing.Color.Cornsilk;
            this.label7.Location = new System.Drawing.Point(38, 6);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(47, 16);
            this.label7.TabIndex = 30;
            this.label7.Text = "Banco";
            // 
            // txtModifPor
            // 
            this.txtModifPor.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
            this.txtModifPor.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.CustomSource;
            this.txtModifPor.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtModifPor.Location = new System.Drawing.Point(303, 348);
            this.txtModifPor.Name = "txtModifPor";
            this.txtModifPor.ReadOnly = true;
            this.txtModifPor.Size = new System.Drawing.Size(142, 21);
            this.txtModifPor.TabIndex = 59;
            this.txtModifPor.TabStop = false;
            // 
            // label5
            // 
            this.label5.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.Location = new System.Drawing.Point(300, 243);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(47, 15);
            this.label5.TabIndex = 54;
            this.label5.Text = "Creado";
            // 
            // txtModificado
            // 
            this.txtModificado.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
            this.txtModificado.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.CustomSource;
            this.txtModificado.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtModificado.Location = new System.Drawing.Point(303, 327);
            this.txtModificado.Name = "txtModificado";
            this.txtModificado.ReadOnly = true;
            this.txtModificado.Size = new System.Drawing.Size(142, 21);
            this.txtModificado.TabIndex = 58;
            this.txtModificado.TabStop = false;
            // 
            // txtCreado
            // 
            this.txtCreado.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
            this.txtCreado.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.CustomSource;
            this.txtCreado.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtCreado.Location = new System.Drawing.Point(303, 259);
            this.txtCreado.Name = "txtCreado";
            this.txtCreado.ReadOnly = true;
            this.txtCreado.Size = new System.Drawing.Size(142, 21);
            this.txtCreado.TabIndex = 55;
            this.txtCreado.TabStop = false;
            // 
            // label6
            // 
            this.label6.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.Location = new System.Drawing.Point(300, 311);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(68, 15);
            this.label6.TabIndex = 56;
            this.label6.Text = "Modificado";
            // 
            // txtCreadoPor
            // 
            this.txtCreadoPor.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
            this.txtCreadoPor.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.CustomSource;
            this.txtCreadoPor.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtCreadoPor.Location = new System.Drawing.Point(303, 280);
            this.txtCreadoPor.Name = "txtCreadoPor";
            this.txtCreadoPor.ReadOnly = true;
            this.txtCreadoPor.Size = new System.Drawing.Size(142, 21);
            this.txtCreadoPor.TabIndex = 57;
            this.txtCreadoPor.TabStop = false;
            // 
            // checkAProveedor
            // 
            this.checkAProveedor.Appearance = System.Windows.Forms.Appearance.Button;
            this.checkAProveedor.AutoSize = true;
            this.checkAProveedor.BackColor = System.Drawing.Color.LimeGreen;
            this.checkAProveedor.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.checkAProveedor.Checked = true;
            this.checkAProveedor.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkAProveedor.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.checkAProveedor.ForeColor = System.Drawing.Color.Cornsilk;
            this.checkAProveedor.Location = new System.Drawing.Point(98, 54);
            this.checkAProveedor.Name = "checkAProveedor";
            this.checkAProveedor.Size = new System.Drawing.Size(78, 26);
            this.checkAProveedor.TabIndex = 53;
            this.checkAProveedor.TabStop = false;
            this.checkAProveedor.Text = "Pagar a ...";
            this.checkAProveedor.UseVisualStyleBackColor = false;
            this.checkAProveedor.CheckedChanged += new System.EventHandler(this.checkAProveedor_CheckedChanged);
            // 
            // groupBox2
            // 
            this.groupBox2.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.groupBox2.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.groupBox2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.groupBox2.Location = new System.Drawing.Point(16, 230);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(429, 7);
            this.groupBox2.TabIndex = 47;
            this.groupBox2.TabStop = false;
            // 
            // label11
            // 
            this.label11.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(14, 242);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(100, 16);
            this.label11.TabIndex = 26;
            this.label11.Text = "Observaciones";
            // 
            // txtImporte
            // 
            this.txtImporte.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtImporte.Location = new System.Drawing.Point(320, 140);
            this.txtImporte.Name = "txtImporte";
            this.txtImporte.Size = new System.Drawing.Size(125, 22);
            this.txtImporte.TabIndex = 3;
            this.txtImporte.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // txtObservaciones
            // 
            this.txtObservaciones.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.txtObservaciones.Location = new System.Drawing.Point(16, 259);
            this.txtObservaciones.Multiline = true;
            this.txtObservaciones.Name = "txtObservaciones";
            this.txtObservaciones.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtObservaciones.Size = new System.Drawing.Size(270, 110);
            this.txtObservaciones.TabIndex = 25;
            this.txtObservaciones.TabStop = false;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.ForeColor = System.Drawing.Color.Cornsilk;
            this.label4.Location = new System.Drawing.Point(265, 143);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(53, 16);
            this.label4.TabIndex = 32;
            this.label4.Text = "Importe";
            // 
            // btnBuscarProv
            // 
            this.btnBuscarProv.AccessibleDescription = "";
            this.btnBuscarProv.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnBuscarProv.ForeColor = System.Drawing.Color.Black;
            this.btnBuscarProv.Image = ((System.Drawing.Image)(resources.GetObject("btnBuscarProv.Image")));
            this.btnBuscarProv.Location = new System.Drawing.Point(228, 85);
            this.btnBuscarProv.Name = "btnBuscarProv";
            this.btnBuscarProv.Size = new System.Drawing.Size(28, 23);
            this.btnBuscarProv.TabIndex = 24;
            this.btnBuscarProv.TabStop = false;
            this.btnBuscarProv.UseVisualStyleBackColor = true;
            this.btnBuscarProv.Click += new System.EventHandler(this.btnBuscarProv_Click);
            // 
            // txtPersona
            // 
            this.txtPersona.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtPersona.Location = new System.Drawing.Point(98, 86);
            this.txtPersona.Name = "txtPersona";
            this.txtPersona.ReadOnly = true;
            this.txtPersona.Size = new System.Drawing.Size(125, 22);
            this.txtPersona.TabIndex = 0;
            this.txtPersona.TabStop = false;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ForeColor = System.Drawing.Color.Cornsilk;
            this.label1.Location = new System.Drawing.Point(30, 89);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(59, 16);
            this.label1.TabIndex = 29;
            this.label1.Text = "Persona";
            // 
            // txtNroRecibo
            // 
            this.txtNroRecibo.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtNroRecibo.Location = new System.Drawing.Point(98, 113);
            this.txtNroRecibo.Name = "txtNroRecibo";
            this.txtNroRecibo.Size = new System.Drawing.Size(125, 22);
            this.txtNroRecibo.TabIndex = 1;
            this.txtNroRecibo.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.ForeColor = System.Drawing.Color.Cornsilk;
            this.label2.Location = new System.Drawing.Point(14, 116);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(77, 16);
            this.label2.TabIndex = 28;
            this.label2.Text = "Nro Recibo";
            // 
            // txtFechaPago
            // 
            this.txtFechaPago.Checked = false;
            this.txtFechaPago.CustomFormat = "dd/MM/yy  HH:mm";
            this.txtFechaPago.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtFechaPago.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.txtFechaPago.Location = new System.Drawing.Point(321, 85);
            this.txtFechaPago.Name = "txtFechaPago";
            this.txtFechaPago.Size = new System.Drawing.Size(124, 22);
            this.txtFechaPago.TabIndex = 25;
            this.txtFechaPago.TabStop = false;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.ForeColor = System.Drawing.Color.Cornsilk;
            this.label3.Location = new System.Drawing.Point(272, 88);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(46, 16);
            this.label3.TabIndex = 27;
            this.label3.Text = "Fecha";
            // 
            // comboTipoPago
            // 
            this.comboTipoPago.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboTipoPago.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.comboTipoPago.FormattingEnabled = true;
            this.comboTipoPago.Items.AddRange(new object[] {
            "Efectivo",
            "Cheque",
            "Eftvo+Cheque",
            "Trasferencia",
            "Otro"});
            this.comboTipoPago.Location = new System.Drawing.Point(98, 140);
            this.comboTipoPago.Name = "comboTipoPago";
            this.comboTipoPago.Size = new System.Drawing.Size(125, 24);
            this.comboTipoPago.TabIndex = 2;
            this.comboTipoPago.SelectedIndexChanged += new System.EventHandler(this.comboTipoPago_SelectedIndexChanged);
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label9.ForeColor = System.Drawing.Color.Cornsilk;
            this.label9.Location = new System.Drawing.Point(9, 143);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(83, 16);
            this.label9.TabIndex = 23;
            this.label9.Text = "Forma Pago";
            // 
            // btnGuardar
            // 
            this.btnGuardar.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnGuardar.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnGuardar.Location = new System.Drawing.Point(222, 426);
            this.btnGuardar.Name = "btnGuardar";
            this.btnGuardar.Size = new System.Drawing.Size(115, 27);
            this.btnGuardar.TabIndex = 4;
            this.btnGuardar.TabStop = false;
            this.btnGuardar.Text = "&Guardar";
            this.btnGuardar.UseVisualStyleBackColor = true;
            this.btnGuardar.Click += new System.EventHandler(this.btnGuardar_Click);
            // 
            // btnCancelar
            // 
            this.btnCancelar.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancelar.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnCancelar.Location = new System.Drawing.Point(349, 426);
            this.btnCancelar.Name = "btnCancelar";
            this.btnCancelar.Size = new System.Drawing.Size(111, 27);
            this.btnCancelar.TabIndex = 5;
            this.btnCancelar.TabStop = false;
            this.btnCancelar.Text = "&Cancelar";
            this.btnCancelar.UseVisualStyleBackColor = true;
            this.btnCancelar.Click += new System.EventHandler(this.btnCancelar_Click);
            // 
            // idPagoLabel
            // 
            this.idPagoLabel.AutoSize = true;
            this.idPagoLabel.Location = new System.Drawing.Point(9, 429);
            this.idPagoLabel.Name = "idPagoLabel";
            this.idPagoLabel.Size = new System.Drawing.Size(66, 13);
            this.idPagoLabel.TabIndex = 25;
            this.idPagoLabel.Text = "idPagoLabel";
            this.idPagoLabel.Visible = false;
            // 
            // formAddOrEditPago
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Bisque;
            this.ClientSize = new System.Drawing.Size(475, 456);
            this.Controls.Add(this.idPagoLabel);
            this.Controls.Add(this.pnlBuscar);
            this.Controls.Add(this.btnGuardar);
            this.Controls.Add(this.btnCancelar);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "formAddOrEditPago";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Nuevo Pago";
            this.Load += new System.EventHandler(this.formNuevoPago_Load);
            this.pnlBuscar.ResumeLayout(false);
            this.pnlBuscar.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.panelCheque.ResumeLayout(false);
            this.panelCheque.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        protected System.Windows.Forms.Panel pnlBuscar;
        protected System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.ComboBox comboTipoPago;
        protected System.Windows.Forms.Label label9;
        protected System.Windows.Forms.Button btnGuardar;
        protected System.Windows.Forms.Button btnCancelar;
        private System.Windows.Forms.TextBox txtImporte;
        protected System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button btnBuscarProv;
        private System.Windows.Forms.TextBox txtPersona;
        protected System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtNroRecibo;
        protected System.Windows.Forms.Label label2;
        protected System.Windows.Forms.DateTimePicker txtFechaPago;
        protected System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.TextBox txtObservaciones;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.CheckBox checkAProveedor;
        protected System.Windows.Forms.TextBox txtModifPor;
        private System.Windows.Forms.Label label5;
        protected System.Windows.Forms.TextBox txtModificado;
        protected System.Windows.Forms.TextBox txtCreado;
        private System.Windows.Forms.Label label6;
        protected System.Windows.Forms.TextBox txtCreadoPor;
        private System.Windows.Forms.Panel panelCheque;
        private System.Windows.Forms.TextBox txtTitular;
        protected System.Windows.Forms.Label label10;
        private System.Windows.Forms.TextBox txtNroCheque;
        protected System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox txtBanco;
        protected System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox txtIdPago;
        protected System.Windows.Forms.Label label12;
        private System.Windows.Forms.TextBox txtSucursal;
        private System.Windows.Forms.TextBox txtUsuario;
        private System.Windows.Forms.ComboBox comboSucursal;
        protected System.Windows.Forms.Label label13;
        protected System.Windows.Forms.Label label16;
        private System.Windows.Forms.Label idPagoLabel;
    }
}