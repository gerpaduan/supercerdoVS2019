namespace Presentacion.Embutidos
{
    partial class formInfoEmbutido
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(formInfoEmbutido));
            this.txtObservaciones = new System.Windows.Forms.TextBox();
            this.txtTotalKg = new System.Windows.Forms.TextBox();
            this.btnAceptar = new System.Windows.Forms.Button();
            this.btnCancelar = new System.Windows.Forms.Button();
            this.grillaCortesPorEmbutido = new System.Windows.Forms.DataGridView();
            this.idEmbutido = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.idCorte = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.codigo = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.corte = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.kgUtilizados = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Balanza = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.pnlBuscar = new System.Windows.Forms.Panel();
            this.txtFechaEmbutido = new System.Windows.Forms.TextBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.txtSucursal = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.txtCodigoEmbutido = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.txtEmbutido = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.barraControl = new System.Windows.Forms.ToolStrip();
            this.anular = new System.Windows.Forms.ToolStripButton();
            this.panelAnulado = new System.Windows.Forms.Panel();
            this.label10 = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            ((System.ComponentModel.ISupportInitialize)(this.grillaCortesPorEmbutido)).BeginInit();
            this.pnlBuscar.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.barraControl.SuspendLayout();
            this.panelAnulado.SuspendLayout();
            this.SuspendLayout();
            // 
            // txtObservaciones
            // 
            this.txtObservaciones.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.txtObservaciones.Location = new System.Drawing.Point(8, 562);
            this.txtObservaciones.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.txtObservaciones.Multiline = true;
            this.txtObservaciones.Name = "txtObservaciones";
            this.txtObservaciones.ReadOnly = true;
            this.txtObservaciones.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtObservaciones.Size = new System.Drawing.Size(381, 47);
            this.txtObservaciones.TabIndex = 27;
            // 
            // txtTotalKg
            // 
            this.txtTotalKg.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.txtTotalKg.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtTotalKg.Location = new System.Drawing.Point(513, 545);
            this.txtTotalKg.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.txtTotalKg.Name = "txtTotalKg";
            this.txtTotalKg.ReadOnly = true;
            this.txtTotalKg.Size = new System.Drawing.Size(119, 24);
            this.txtTotalKg.TabIndex = 26;
            this.txtTotalKg.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // btnAceptar
            // 
            this.btnAceptar.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnAceptar.Location = new System.Drawing.Point(393, 626);
            this.btnAceptar.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnAceptar.Name = "btnAceptar";
            this.btnAceptar.Size = new System.Drawing.Size(116, 33);
            this.btnAceptar.TabIndex = 25;
            this.btnAceptar.Text = "Guardar";
            this.btnAceptar.UseVisualStyleBackColor = true;
            this.btnAceptar.Visible = false;
            this.btnAceptar.Click += new System.EventHandler(this.btnAceptar_Click);
            // 
            // btnCancelar
            // 
            this.btnCancelar.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancelar.Location = new System.Drawing.Point(521, 626);
            this.btnCancelar.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnCancelar.Name = "btnCancelar";
            this.btnCancelar.Size = new System.Drawing.Size(116, 33);
            this.btnCancelar.TabIndex = 24;
            this.btnCancelar.Text = "&Salir";
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
            this.idEmbutido,
            this.idCorte,
            this.codigo,
            this.corte,
            this.kgUtilizados,
            this.Balanza});
            this.grillaCortesPorEmbutido.Location = new System.Drawing.Point(5, 225);
            this.grillaCortesPorEmbutido.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.grillaCortesPorEmbutido.MultiSelect = false;
            this.grillaCortesPorEmbutido.Name = "grillaCortesPorEmbutido";
            this.grillaCortesPorEmbutido.ReadOnly = true;
            this.grillaCortesPorEmbutido.RowHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
            this.grillaCortesPorEmbutido.RowHeadersVisible = false;
            this.grillaCortesPorEmbutido.RowHeadersWidth = 51;
            this.grillaCortesPorEmbutido.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.grillaCortesPorEmbutido.Size = new System.Drawing.Size(637, 313);
            this.grillaCortesPorEmbutido.TabIndex = 23;
            // 
            // idEmbutido
            // 
            this.idEmbutido.DataPropertyName = "idEmbutido";
            this.idEmbutido.HeaderText = "Id Embutido";
            this.idEmbutido.MinimumWidth = 6;
            this.idEmbutido.Name = "idEmbutido";
            this.idEmbutido.ReadOnly = true;
            this.idEmbutido.Visible = false;
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
            // codigo
            // 
            this.codigo.DataPropertyName = "codigo";
            this.codigo.FillWeight = 56.338F;
            this.codigo.HeaderText = "Codigo";
            this.codigo.MinimumWidth = 6;
            this.codigo.Name = "codigo";
            this.codigo.ReadOnly = true;
            // 
            // corte
            // 
            this.corte.DataPropertyName = "corte";
            this.corte.FillWeight = 82.81686F;
            this.corte.HeaderText = "Corte";
            this.corte.MinimumWidth = 6;
            this.corte.Name = "corte";
            this.corte.ReadOnly = true;
            // 
            // kgUtilizados
            // 
            this.kgUtilizados.DataPropertyName = "kgUtilizados";
            dataGridViewCellStyle1.Format = "N3";
            dataGridViewCellStyle1.NullValue = null;
            this.kgUtilizados.DefaultCellStyle = dataGridViewCellStyle1;
            this.kgUtilizados.FillWeight = 82.81686F;
            this.kgUtilizados.HeaderText = "Kgs. Utilizados";
            this.kgUtilizados.MinimumWidth = 6;
            this.kgUtilizados.Name = "kgUtilizados";
            this.kgUtilizados.ReadOnly = true;
            // 
            // Balanza
            // 
            this.Balanza.DataPropertyName = "pesoBalanza";
            this.Balanza.FillWeight = 30F;
            this.Balanza.HeaderText = "Balanza";
            this.Balanza.MinimumWidth = 6;
            this.Balanza.Name = "Balanza";
            this.Balanza.ReadOnly = true;
            // 
            // pnlBuscar
            // 
            this.pnlBuscar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pnlBuscar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(43)))), ((int)(((byte)(77)))), ((int)(((byte)(129)))));
            this.pnlBuscar.Controls.Add(this.txtFechaEmbutido);
            this.pnlBuscar.Controls.Add(this.groupBox2);
            this.pnlBuscar.Controls.Add(this.txtSucursal);
            this.pnlBuscar.Controls.Add(this.label9);
            this.pnlBuscar.Controls.Add(this.label6);
            this.pnlBuscar.Controls.Add(this.groupBox1);
            this.pnlBuscar.Location = new System.Drawing.Point(-1, 49);
            this.pnlBuscar.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.pnlBuscar.Name = "pnlBuscar";
            this.pnlBuscar.Size = new System.Drawing.Size(652, 170);
            this.pnlBuscar.TabIndex = 22;
            // 
            // txtFechaEmbutido
            // 
            this.txtFechaEmbutido.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.txtFechaEmbutido.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtFechaEmbutido.Location = new System.Drawing.Point(443, 11);
            this.txtFechaEmbutido.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.txtFechaEmbutido.Name = "txtFechaEmbutido";
            this.txtFechaEmbutido.ReadOnly = true;
            this.txtFechaEmbutido.Size = new System.Drawing.Size(191, 24);
            this.txtFechaEmbutido.TabIndex = 49;
            this.txtFechaEmbutido.TabStop = false;
            this.txtFechaEmbutido.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // groupBox2
            // 
            this.groupBox2.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.groupBox2.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.groupBox2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.groupBox2.Location = new System.Drawing.Point(17, 43);
            this.groupBox2.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.groupBox2.Size = new System.Drawing.Size(375, 9);
            this.groupBox2.TabIndex = 48;
            this.groupBox2.TabStop = false;
            // 
            // txtSucursal
            // 
            this.txtSucursal.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.txtSucursal.Location = new System.Drawing.Point(108, 14);
            this.txtSucursal.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.txtSucursal.Name = "txtSucursal";
            this.txtSucursal.ReadOnly = true;
            this.txtSucursal.Size = new System.Drawing.Size(119, 22);
            this.txtSucursal.TabIndex = 22;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label9.ForeColor = System.Drawing.Color.Cornsilk;
            this.label9.Location = new System.Drawing.Point(27, 15);
            this.label9.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(66, 18);
            this.label9.TabIndex = 21;
            this.label9.Text = "Sucursal";
            // 
            // label6
            // 
            this.label6.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.ForeColor = System.Drawing.Color.Cornsilk;
            this.label6.Location = new System.Drawing.Point(380, 15);
            this.label6.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(49, 18);
            this.label6.TabIndex = 12;
            this.label6.Text = "Fecha";
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(43)))), ((int)(((byte)(77)))), ((int)(((byte)(129)))));
            this.groupBox1.Controls.Add(this.txtCodigoEmbutido);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.txtEmbutido);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox1.ForeColor = System.Drawing.Color.Cornsilk;
            this.groupBox1.Location = new System.Drawing.Point(16, 55);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.groupBox1.Size = new System.Drawing.Size(376, 105);
            this.groupBox1.TabIndex = 9;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Embutido / Otro";
            // 
            // txtCodigoEmbutido
            // 
            this.txtCodigoEmbutido.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.txtCodigoEmbutido.Location = new System.Drawing.Point(117, 27);
            this.txtCodigoEmbutido.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.txtCodigoEmbutido.Name = "txtCodigoEmbutido";
            this.txtCodigoEmbutido.ReadOnly = true;
            this.txtCodigoEmbutido.Size = new System.Drawing.Size(93, 24);
            this.txtCodigoEmbutido.TabIndex = 11;
            // 
            // label3
            // 
            this.label3.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.ForeColor = System.Drawing.Color.Cornsilk;
            this.label3.Location = new System.Drawing.Point(48, 31);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(56, 18);
            this.label3.TabIndex = 10;
            this.label3.Text = "Código";
            // 
            // txtEmbutido
            // 
            this.txtEmbutido.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.txtEmbutido.Location = new System.Drawing.Point(117, 63);
            this.txtEmbutido.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.txtEmbutido.Name = "txtEmbutido";
            this.txtEmbutido.ReadOnly = true;
            this.txtEmbutido.Size = new System.Drawing.Size(192, 24);
            this.txtEmbutido.TabIndex = 9;
            // 
            // label4
            // 
            this.label4.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.ForeColor = System.Drawing.Color.Cornsilk;
            this.label4.Location = new System.Drawing.Point(13, 66);
            this.label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(87, 18);
            this.label4.TabIndex = 8;
            this.label4.Text = "Descripción";
            // 
            // label8
            // 
            this.label8.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.label8.AutoSize = true;
            this.label8.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label8.Location = new System.Drawing.Point(425, 550);
            this.label8.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(71, 18);
            this.label8.TabIndex = 28;
            this.label8.Text = "Total Kg";
            // 
            // label11
            // 
            this.label11.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(5, 542);
            this.label11.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(97, 16);
            this.label11.TabIndex = 29;
            this.label11.Text = "observaciones";
            // 
            // barraControl
            // 
            this.barraControl.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.barraControl.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.barraControl.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.anular});
            this.barraControl.Location = new System.Drawing.Point(0, 0);
            this.barraControl.Name = "barraControl";
            this.barraControl.Padding = new System.Windows.Forms.Padding(13, 0, 0, 0);
            this.barraControl.Size = new System.Drawing.Size(649, 49);
            this.barraControl.TabIndex = 31;
            this.barraControl.TabStop = true;
            this.barraControl.Text = "toolStrip1";
            // 
            // anular
            // 
            this.anular.Image = ((System.Drawing.Image)(resources.GetObject("anular.Image")));
            this.anular.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.anular.Margin = new System.Windows.Forms.Padding(0, 2, 0, 1);
            this.anular.Name = "anular";
            this.anular.Padding = new System.Windows.Forms.Padding(1);
            this.anular.Size = new System.Drawing.Size(58, 46);
            this.anular.Text = "Anular";
            this.anular.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.anular.Click += new System.EventHandler(this.anular_Click);
            // 
            // panelAnulado
            // 
            this.panelAnulado.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(43)))), ((int)(((byte)(77)))), ((int)(((byte)(129)))));
            this.panelAnulado.Controls.Add(this.label10);
            this.panelAnulado.Location = new System.Drawing.Point(-3, 0);
            this.panelAnulado.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.panelAnulado.Name = "panelAnulado";
            this.panelAnulado.Size = new System.Drawing.Size(652, 50);
            this.panelAnulado.TabIndex = 32;
            this.panelAnulado.Visible = false;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Underline))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label10.ForeColor = System.Drawing.Color.White;
            this.label10.Location = new System.Drawing.Point(23, 14);
            this.label10.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(183, 24);
            this.label10.TabIndex = 7;
            this.label10.Text = "Embutido Anulado";
            // 
            // panel1
            // 
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel1.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.panel1.Location = new System.Drawing.Point(5, 618);
            this.panel1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(637, 1);
            this.panel1.TabIndex = 33;
            // 
            // formInfoEmbutido
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(239)))), ((int)(((byte)(250)))));
            this.ClientSize = new System.Drawing.Size(649, 663);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.label11);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.txtObservaciones);
            this.Controls.Add(this.txtTotalKg);
            this.Controls.Add(this.btnAceptar);
            this.Controls.Add(this.btnCancelar);
            this.Controls.Add(this.grillaCortesPorEmbutido);
            this.Controls.Add(this.pnlBuscar);
            this.Controls.Add(this.barraControl);
            this.Controls.Add(this.panelAnulado);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.MaximizeBox = false;
            this.Name = "formInfoEmbutido";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Información Embutido / Otro";
            this.Load += new System.EventHandler(this.formInfoEmbutido_Load);
            ((System.ComponentModel.ISupportInitialize)(this.grillaCortesPorEmbutido)).EndInit();
            this.pnlBuscar.ResumeLayout(false);
            this.pnlBuscar.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.barraControl.ResumeLayout(false);
            this.barraControl.PerformLayout();
            this.panelAnulado.ResumeLayout(false);
            this.panelAnulado.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txtObservaciones;
        private System.Windows.Forms.TextBox txtTotalKg;
        protected System.Windows.Forms.Button btnAceptar;
        protected System.Windows.Forms.Button btnCancelar;
        private System.Windows.Forms.DataGridView grillaCortesPorEmbutido;
        protected System.Windows.Forms.Panel pnlBuscar;
        protected System.Windows.Forms.Label label9;
        protected System.Windows.Forms.Label label6;
        protected System.Windows.Forms.GroupBox groupBox1;
        protected System.Windows.Forms.TextBox txtCodigoEmbutido;
        protected System.Windows.Forms.Label label3;
        protected System.Windows.Forms.TextBox txtEmbutido;
        protected System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label11;
        protected internal System.Windows.Forms.ToolStrip barraControl;
        protected System.Windows.Forms.ToolStripButton anular;
        protected System.Windows.Forms.TextBox txtSucursal;
        private System.Windows.Forms.Panel panelAnulado;
        protected System.Windows.Forms.Label label10;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.TextBox txtFechaEmbutido;
        private System.Windows.Forms.DataGridViewTextBoxColumn idEmbutido;
        private System.Windows.Forms.DataGridViewTextBoxColumn idCorte;
        private System.Windows.Forms.DataGridViewTextBoxColumn codigo;
        private System.Windows.Forms.DataGridViewTextBoxColumn corte;
        private System.Windows.Forms.DataGridViewTextBoxColumn kgUtilizados;
        private System.Windows.Forms.DataGridViewCheckBoxColumn Balanza;
    }
}