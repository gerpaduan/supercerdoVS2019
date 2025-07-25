namespace Presentacion
{
    partial class formLineasMov
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(formLineasMov));
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            this.pnlBuscar = new System.Windows.Forms.Panel();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label6 = new System.Windows.Forms.Label();
            this.comboSucDestino = new System.Windows.Forms.ComboBox();
            this.label7 = new System.Windows.Forms.Label();
            this.comboSucOrigen = new System.Windows.Forms.ComboBox();
            this.label5 = new System.Windows.Forms.Label();
            this.btnBuscarCorte = new System.Windows.Forms.Button();
            this.txtDescripcion = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.txtFechaDesde = new System.Windows.Forms.DateTimePicker();
            this.label3 = new System.Windows.Forms.Label();
            this.txtFechaHasta = new System.Windows.Forms.DateTimePicker();
            this.label4 = new System.Windows.Forms.Label();
            this.movimiento = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.fechaMovimiento = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.corte = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.idSucursalOrigen = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.sucursalOrigen = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.idSucursalDestino = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.sucursalDestino = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.kgCorte = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.estado = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.btnSeleccionar = new System.Windows.Forms.Button();
            this.btnCancelar = new System.Windows.Forms.Button();
            this.grillaMovimientos = new System.Windows.Forms.DataGridView();
            this.lblActualizar = new System.Windows.Forms.Label();
            this.lblCargando = new System.Windows.Forms.Label();
            this.pnlBuscar.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.grillaMovimientos)).BeginInit();
            this.SuspendLayout();
            // 
            // pnlBuscar
            // 
            this.pnlBuscar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.pnlBuscar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(43)))), ((int)(((byte)(77)))), ((int)(((byte)(129)))));
            this.pnlBuscar.Controls.Add(this.lblCargando);
            this.pnlBuscar.Controls.Add(this.lblActualizar);
            this.pnlBuscar.Controls.Add(this.groupBox3);
            this.pnlBuscar.Controls.Add(this.groupBox2);
            this.pnlBuscar.Controls.Add(this.groupBox1);
            this.pnlBuscar.Controls.Add(this.label6);
            this.pnlBuscar.Controls.Add(this.comboSucDestino);
            this.pnlBuscar.Controls.Add(this.label7);
            this.pnlBuscar.Controls.Add(this.comboSucOrigen);
            this.pnlBuscar.Controls.Add(this.label5);
            this.pnlBuscar.Controls.Add(this.btnBuscarCorte);
            this.pnlBuscar.Controls.Add(this.txtDescripcion);
            this.pnlBuscar.Controls.Add(this.label2);
            this.pnlBuscar.Controls.Add(this.label1);
            this.pnlBuscar.Controls.Add(this.txtFechaDesde);
            this.pnlBuscar.Controls.Add(this.label3);
            this.pnlBuscar.Controls.Add(this.txtFechaHasta);
            this.pnlBuscar.Controls.Add(this.label4);
            this.pnlBuscar.Location = new System.Drawing.Point(-4, 0);
            this.pnlBuscar.Name = "pnlBuscar";
            this.pnlBuscar.Size = new System.Drawing.Size(995, 108);
            this.pnlBuscar.TabIndex = 8;
            // 
            // groupBox3
            // 
            this.groupBox3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox3.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.groupBox3.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.groupBox3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.groupBox3.Location = new System.Drawing.Point(16, 67);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(963, 7);
            this.groupBox3.TabIndex = 47;
            this.groupBox3.TabStop = false;
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox2.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.groupBox2.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.groupBox2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.groupBox2.Location = new System.Drawing.Point(690, 34);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(289, 7);
            this.groupBox2.TabIndex = 46;
            this.groupBox2.TabStop = false;
            // 
            // groupBox1
            // 
            this.groupBox1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.groupBox1.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.groupBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.groupBox1.Location = new System.Drawing.Point(77, 14);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(175, 7);
            this.groupBox1.TabIndex = 45;
            this.groupBox1.TabStop = false;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.ForeColor = System.Drawing.Color.Cornsilk;
            this.label6.Location = new System.Drawing.Point(16, 6);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(55, 15);
            this.label6.TabIndex = 44;
            this.label6.Text = "Sucursal";
            // 
            // comboSucDestino
            // 
            this.comboSucDestino.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboSucDestino.FormattingEnabled = true;
            this.comboSucDestino.Location = new System.Drawing.Point(89, 47);
            this.comboSucDestino.Name = "comboSucDestino";
            this.comboSucDestino.Size = new System.Drawing.Size(133, 21);
            this.comboSucDestino.TabIndex = 43;
            this.comboSucDestino.TabStop = false;
            this.comboSucDestino.TextChanged += new System.EventHandler(this.comboSucDestino_TextChanged);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label7.ForeColor = System.Drawing.Color.Cornsilk;
            this.label7.Location = new System.Drawing.Point(39, 28);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(44, 15);
            this.label7.TabIndex = 40;
            this.label7.Text = "Origen";
            // 
            // comboSucOrigen
            // 
            this.comboSucOrigen.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboSucOrigen.FormattingEnabled = true;
            this.comboSucOrigen.Location = new System.Drawing.Point(89, 25);
            this.comboSucOrigen.Name = "comboSucOrigen";
            this.comboSucOrigen.Size = new System.Drawing.Size(133, 21);
            this.comboSucOrigen.TabIndex = 42;
            this.comboSucOrigen.TabStop = false;
            this.comboSucOrigen.TextChanged += new System.EventHandler(this.comboSucOrigen_TextChanged);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.ForeColor = System.Drawing.Color.Cornsilk;
            this.label5.Location = new System.Drawing.Point(34, 48);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(49, 15);
            this.label5.TabIndex = 41;
            this.label5.Text = "Destino";
            // 
            // btnBuscarCorte
            // 
            this.btnBuscarCorte.AccessibleDescription = "";
            this.btnBuscarCorte.ForeColor = System.Drawing.Color.Black;
            this.btnBuscarCorte.Image = ((System.Drawing.Image)(resources.GetObject("btnBuscarCorte.Image")));
            this.btnBuscarCorte.Location = new System.Drawing.Point(224, 80);
            this.btnBuscarCorte.Name = "btnBuscarCorte";
            this.btnBuscarCorte.Size = new System.Drawing.Size(28, 23);
            this.btnBuscarCorte.TabIndex = 15;
            this.btnBuscarCorte.UseVisualStyleBackColor = true;
            this.btnBuscarCorte.Click += new System.EventHandler(this.btnBuscarCorte_Click);
            // 
            // txtDescripcion
            // 
            this.txtDescripcion.Location = new System.Drawing.Point(89, 81);
            this.txtDescripcion.Name = "txtDescripcion";
            this.txtDescripcion.Size = new System.Drawing.Size(129, 20);
            this.txtDescripcion.TabIndex = 0;
            this.txtDescripcion.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtDescripcion_KeyDown);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.ForeColor = System.Drawing.Color.Cornsilk;
            this.label2.Location = new System.Drawing.Point(16, 83);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(72, 15);
            this.label2.TabIndex = 11;
            this.label2.Text = "Descripción";
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ForeColor = System.Drawing.Color.Cornsilk;
            this.label1.Location = new System.Drawing.Point(643, 28);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(41, 15);
            this.label1.TabIndex = 9;
            this.label1.Text = "Fecha";
            // 
            // txtFechaDesde
            // 
            this.txtFechaDesde.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.txtFechaDesde.CustomFormat = "dd/MM/yyyy";
            this.txtFechaDesde.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.txtFechaDesde.Location = new System.Drawing.Point(737, 47);
            this.txtFechaDesde.Name = "txtFechaDesde";
            this.txtFechaDesde.Size = new System.Drawing.Size(95, 20);
            this.txtFechaDesde.TabIndex = 5;
            this.txtFechaDesde.Value = new System.DateTime(2012, 5, 24, 0, 0, 0, 0);
            this.txtFechaDesde.ValueChanged += new System.EventHandler(this.txtFechaDesde_ValueChanged);
            this.txtFechaDesde.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtDescripcion_KeyDown);
            // 
            // label3
            // 
            this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.ForeColor = System.Drawing.Color.Cornsilk;
            this.label3.Location = new System.Drawing.Point(687, 49);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(43, 15);
            this.label3.TabIndex = 4;
            this.label3.Text = "Desde";
            // 
            // txtFechaHasta
            // 
            this.txtFechaHasta.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.txtFechaHasta.CustomFormat = "dd/MM/yyyy";
            this.txtFechaHasta.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.txtFechaHasta.Location = new System.Drawing.Point(883, 48);
            this.txtFechaHasta.Name = "txtFechaHasta";
            this.txtFechaHasta.Size = new System.Drawing.Size(96, 20);
            this.txtFechaHasta.TabIndex = 7;
            this.txtFechaHasta.ValueChanged += new System.EventHandler(this.txtFechaDesde_ValueChanged);
            this.txtFechaHasta.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtDescripcion_KeyDown);
            // 
            // label4
            // 
            this.label4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.ForeColor = System.Drawing.Color.Cornsilk;
            this.label4.Location = new System.Drawing.Point(838, 50);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(39, 15);
            this.label4.TabIndex = 6;
            this.label4.Text = "Hasta";
            // 
            // movimiento
            // 
            this.movimiento.DataPropertyName = "movimiento";
            this.movimiento.HeaderText = "movimiento";
            this.movimiento.Name = "movimiento";
            this.movimiento.ReadOnly = true;
            this.movimiento.Visible = false;
            // 
            // fechaMovimiento
            // 
            this.fechaMovimiento.DataPropertyName = "fechaMovimiento";
            dataGridViewCellStyle1.Format = "d";
            dataGridViewCellStyle1.NullValue = null;
            this.fechaMovimiento.DefaultCellStyle = dataGridViewCellStyle1;
            this.fechaMovimiento.HeaderText = "Fecha Movimiento";
            this.fechaMovimiento.Name = "fechaMovimiento";
            this.fechaMovimiento.ReadOnly = true;
            this.fechaMovimiento.Width = 80;
            // 
            // corte
            // 
            this.corte.DataPropertyName = "corte";
            this.corte.HeaderText = "Prod.";
            this.corte.Name = "corte";
            this.corte.ReadOnly = true;
            this.corte.Visible = false;
            // 
            // idSucursalOrigen
            // 
            this.idSucursalOrigen.DataPropertyName = "idSucursalOrigen";
            this.idSucursalOrigen.HeaderText = "idSucursalOrigen";
            this.idSucursalOrigen.Name = "idSucursalOrigen";
            this.idSucursalOrigen.ReadOnly = true;
            this.idSucursalOrigen.Visible = false;
            // 
            // sucursalOrigen
            // 
            this.sucursalOrigen.DataPropertyName = "sucursalOrigen";
            this.sucursalOrigen.HeaderText = "Origen";
            this.sucursalOrigen.Name = "sucursalOrigen";
            this.sucursalOrigen.ReadOnly = true;
            this.sucursalOrigen.Width = 70;
            // 
            // idSucursalDestino
            // 
            this.idSucursalDestino.DataPropertyName = "idSucursalDestino";
            this.idSucursalDestino.HeaderText = "idSucursalDestino";
            this.idSucursalDestino.Name = "idSucursalDestino";
            this.idSucursalDestino.ReadOnly = true;
            this.idSucursalDestino.Visible = false;
            // 
            // sucursalDestino
            // 
            this.sucursalDestino.DataPropertyName = "sucursalDestino";
            this.sucursalDestino.HeaderText = "Destino";
            this.sucursalDestino.Name = "sucursalDestino";
            this.sucursalDestino.ReadOnly = true;
            this.sucursalDestino.Width = 70;
            // 
            // kgCorte
            // 
            this.kgCorte.DataPropertyName = "kgCorte";
            this.kgCorte.HeaderText = "Kgs. Prod.";
            this.kgCorte.Name = "kgCorte";
            this.kgCorte.ReadOnly = true;
            this.kgCorte.Width = 70;
            // 
            // estado
            // 
            this.estado.DataPropertyName = "estado";
            dataGridViewCellStyle2.ForeColor = System.Drawing.Color.Red;
            this.estado.DefaultCellStyle = dataGridViewCellStyle2;
            this.estado.HeaderText = "Estado";
            this.estado.Name = "estado";
            this.estado.ReadOnly = true;
            // 
            // btnSeleccionar
            // 
            this.btnSeleccionar.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSeleccionar.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSeleccionar.Location = new System.Drawing.Point(749, 486);
            this.btnSeleccionar.Name = "btnSeleccionar";
            this.btnSeleccionar.Size = new System.Drawing.Size(114, 27);
            this.btnSeleccionar.TabIndex = 15;
            this.btnSeleccionar.Text = "&Seleccionar";
            this.btnSeleccionar.UseVisualStyleBackColor = true;
            this.btnSeleccionar.Click += new System.EventHandler(this.btnSeleccionar_Click);
            // 
            // btnCancelar
            // 
            this.btnCancelar.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancelar.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnCancelar.Location = new System.Drawing.Point(869, 486);
            this.btnCancelar.Name = "btnCancelar";
            this.btnCancelar.Size = new System.Drawing.Size(106, 27);
            this.btnCancelar.TabIndex = 14;
            this.btnCancelar.Text = "&Cerrar";
            this.btnCancelar.UseVisualStyleBackColor = true;
            this.btnCancelar.Click += new System.EventHandler(this.btnCancelar_Click);
            // 
            // grillaMovimientos
            // 
            this.grillaMovimientos.AllowUserToAddRows = false;
            this.grillaMovimientos.AllowUserToOrderColumns = true;
            this.grillaMovimientos.AllowUserToResizeRows = false;
            this.grillaMovimientos.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.grillaMovimientos.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
            this.grillaMovimientos.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle3.NullValue = null;
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.grillaMovimientos.DefaultCellStyle = dataGridViewCellStyle3;
            this.grillaMovimientos.Location = new System.Drawing.Point(12, 114);
            this.grillaMovimientos.Name = "grillaMovimientos";
            this.grillaMovimientos.ReadOnly = true;
            this.grillaMovimientos.RowHeadersVisible = false;
            this.grillaMovimientos.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.grillaMovimientos.Size = new System.Drawing.Size(963, 366);
            this.grillaMovimientos.TabIndex = 16;
            this.grillaMovimientos.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.grillaMovimientos_CellDoubleClick);
            // 
            // lblActualizar
            // 
            this.lblActualizar.AutoSize = true;
            this.lblActualizar.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblActualizar.ForeColor = System.Drawing.Color.LightSalmon;
            this.lblActualizar.Location = new System.Drawing.Point(258, 83);
            this.lblActualizar.Name = "lblActualizar";
            this.lblActualizar.Size = new System.Drawing.Size(69, 15);
            this.lblActualizar.TabIndex = 53;
            this.lblActualizar.Text = "Actualizar...";
            this.lblActualizar.Visible = false;
            // 
            // lblCargando
            // 
            this.lblCargando.AutoSize = true;
            this.lblCargando.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblCargando.ForeColor = System.Drawing.Color.LightSalmon;
            this.lblCargando.Location = new System.Drawing.Point(333, 83);
            this.lblCargando.Name = "lblCargando";
            this.lblCargando.Size = new System.Drawing.Size(81, 15);
            this.lblCargando.TabIndex = 54;
            this.lblCargando.Text = "Cargando...";
            this.lblCargando.Visible = false;
            // 
            // formLineasMov
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(987, 517);
            this.Controls.Add(this.grillaMovimientos);
            this.Controls.Add(this.btnSeleccionar);
            this.Controls.Add(this.btnCancelar);
            this.Controls.Add(this.pnlBuscar);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Sizable;
            this.MinimizeBox = true;
            this.Name = "formLineasMov";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Lineas Movimientos";
            this.Load += new System.EventHandler(this.formLineasMov_Load);
            this.pnlBuscar.ResumeLayout(false);
            this.pnlBuscar.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.grillaMovimientos)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        protected System.Windows.Forms.Panel pnlBuscar;
        protected internal System.Windows.Forms.Button btnBuscarCorte;
        private System.Windows.Forms.TextBox txtDescripcion;
        protected System.Windows.Forms.Label label2;
        protected System.Windows.Forms.Label label1;
        protected System.Windows.Forms.DateTimePicker txtFechaDesde;
        protected System.Windows.Forms.Label label3;
        protected System.Windows.Forms.DateTimePicker txtFechaHasta;
        protected System.Windows.Forms.Label label4;
        protected System.Windows.Forms.Button btnSeleccionar;
        protected System.Windows.Forms.Button btnCancelar;
        private System.Windows.Forms.DataGridViewTextBoxColumn fechaMovimiento;
        private System.Windows.Forms.DataGridViewTextBoxColumn corte;
        private System.Windows.Forms.DataGridViewTextBoxColumn idSucursalOrigen;
        private System.Windows.Forms.DataGridViewTextBoxColumn sucursalOrigen;
        private System.Windows.Forms.DataGridViewTextBoxColumn idSucursalDestino;
        private System.Windows.Forms.DataGridViewTextBoxColumn sucursalDestino;
        private System.Windows.Forms.DataGridViewTextBoxColumn kgCorte;
        private System.Windows.Forms.DataGridViewTextBoxColumn estado;
        private System.Windows.Forms.DataGridViewTextBoxColumn movimiento;
        private System.Windows.Forms.DataGridView grillaMovimientos;
        protected System.Windows.Forms.Label label7;
        private System.Windows.Forms.ComboBox comboSucDestino;
        private System.Windows.Forms.ComboBox comboSucOrigen;
        protected System.Windows.Forms.Label label5;
        protected System.Windows.Forms.Label label6;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.GroupBox groupBox2;
        protected System.Windows.Forms.Label lblActualizar;
        protected System.Windows.Forms.Label lblCargando;
    }
}