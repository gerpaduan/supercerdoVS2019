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
            this.pnlBuscar = new System.Windows.Forms.Panel();
            this.txtSucursal = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.txtFechaEmbutido = new System.Windows.Forms.DateTimePicker();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.txtCodigoEmbutido = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.txtEmbutido = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.shapeContainer2 = new Microsoft.VisualBasic.PowerPacks.ShapeContainer();
            this.lineShape2 = new Microsoft.VisualBasic.PowerPacks.LineShape();
            this.label8 = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.lineShape1 = new Microsoft.VisualBasic.PowerPacks.LineShape();
            this.shapeContainer1 = new Microsoft.VisualBasic.PowerPacks.ShapeContainer();
            this.barraControl = new System.Windows.Forms.ToolStrip();
            this.anular = new System.Windows.Forms.ToolStripButton();
            this.panelAnulado = new System.Windows.Forms.Panel();
            this.label10 = new System.Windows.Forms.Label();
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
            this.txtObservaciones.Location = new System.Drawing.Point(6, 352);
            this.txtObservaciones.Multiline = true;
            this.txtObservaciones.Name = "txtObservaciones";
            this.txtObservaciones.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtObservaciones.Size = new System.Drawing.Size(255, 39);
            this.txtObservaciones.TabIndex = 27;
            // 
            // txtTotalKg
            // 
            this.txtTotalKg.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.txtTotalKg.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtTotalKg.Location = new System.Drawing.Point(385, 338);
            this.txtTotalKg.Name = "txtTotalKg";
            this.txtTotalKg.ReadOnly = true;
            this.txtTotalKg.Size = new System.Drawing.Size(90, 21);
            this.txtTotalKg.TabIndex = 26;
            this.txtTotalKg.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // btnAceptar
            // 
            this.btnAceptar.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnAceptar.Location = new System.Drawing.Point(295, 404);
            this.btnAceptar.Name = "btnAceptar";
            this.btnAceptar.Size = new System.Drawing.Size(87, 27);
            this.btnAceptar.TabIndex = 25;
            this.btnAceptar.Text = "Aceptar";
            this.btnAceptar.UseVisualStyleBackColor = true;
            this.btnAceptar.Click += new System.EventHandler(this.btnAceptar_Click);
            // 
            // btnCancelar
            // 
            this.btnCancelar.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancelar.Location = new System.Drawing.Point(391, 404);
            this.btnCancelar.Name = "btnCancelar";
            this.btnCancelar.Size = new System.Drawing.Size(87, 27);
            this.btnCancelar.TabIndex = 24;
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
            this.idEmbutido,
            this.idCorte,
            this.codigo,
            this.corte,
            this.kgUtilizados});
            this.grillaCortesPorEmbutido.Location = new System.Drawing.Point(4, 183);
            this.grillaCortesPorEmbutido.MultiSelect = false;
            this.grillaCortesPorEmbutido.Name = "grillaCortesPorEmbutido";
            this.grillaCortesPorEmbutido.ReadOnly = true;
            this.grillaCortesPorEmbutido.RowHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
            this.grillaCortesPorEmbutido.RowHeadersVisible = false;
            this.grillaCortesPorEmbutido.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.grillaCortesPorEmbutido.Size = new System.Drawing.Size(478, 149);
            this.grillaCortesPorEmbutido.TabIndex = 23;
            // 
            // idEmbutido
            // 
            this.idEmbutido.DataPropertyName = "idEmbutido";
            this.idEmbutido.HeaderText = "Id Embutido";
            this.idEmbutido.Name = "idEmbutido";
            this.idEmbutido.ReadOnly = true;
            this.idEmbutido.Visible = false;
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
            this.codigo.DataPropertyName = "codigo";
            this.codigo.FillWeight = 56.338F;
            this.codigo.HeaderText = "Codigo";
            this.codigo.Name = "codigo";
            this.codigo.ReadOnly = true;
            // 
            // corte
            // 
            this.corte.DataPropertyName = "corte";
            this.corte.FillWeight = 82.81686F;
            this.corte.HeaderText = "Corte";
            this.corte.Name = "corte";
            this.corte.ReadOnly = true;
            // 
            // kgUtilizados
            // 
            this.kgUtilizados.DataPropertyName = "kgUtilizados";
            dataGridViewCellStyle1.Format = "N2";
            dataGridViewCellStyle1.NullValue = null;
            this.kgUtilizados.DefaultCellStyle = dataGridViewCellStyle1;
            this.kgUtilizados.FillWeight = 82.81686F;
            this.kgUtilizados.HeaderText = "Kgs. Utilizados";
            this.kgUtilizados.Name = "kgUtilizados";
            this.kgUtilizados.ReadOnly = true;
            // 
            // pnlBuscar
            // 
            this.pnlBuscar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.pnlBuscar.BackColor = System.Drawing.Color.SteelBlue;
            this.pnlBuscar.Controls.Add(this.txtSucursal);
            this.pnlBuscar.Controls.Add(this.label9);
            this.pnlBuscar.Controls.Add(this.label6);
            this.pnlBuscar.Controls.Add(this.txtFechaEmbutido);
            this.pnlBuscar.Controls.Add(this.groupBox1);
            this.pnlBuscar.Controls.Add(this.shapeContainer2);
            this.pnlBuscar.Location = new System.Drawing.Point(-1, 40);
            this.pnlBuscar.Name = "pnlBuscar";
            this.pnlBuscar.Size = new System.Drawing.Size(489, 138);
            this.pnlBuscar.TabIndex = 22;
            // 
            // txtSucursal
            // 
            this.txtSucursal.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.txtSucursal.Location = new System.Drawing.Point(81, 11);
            this.txtSucursal.Name = "txtSucursal";
            this.txtSucursal.ReadOnly = true;
            this.txtSucursal.Size = new System.Drawing.Size(90, 20);
            this.txtSucursal.TabIndex = 22;
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
            this.label6.Location = new System.Drawing.Point(316, 10);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(41, 15);
            this.label6.TabIndex = 12;
            this.label6.Text = "Fecha";
            // 
            // txtFechaEmbutido
            // 
            this.txtFechaEmbutido.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.txtFechaEmbutido.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.txtFechaEmbutido.Location = new System.Drawing.Point(366, 8);
            this.txtFechaEmbutido.Name = "txtFechaEmbutido";
            this.txtFechaEmbutido.Size = new System.Drawing.Size(104, 20);
            this.txtFechaEmbutido.TabIndex = 11;
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.BackColor = System.Drawing.Color.SteelBlue;
            this.groupBox1.Controls.Add(this.txtCodigoEmbutido);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.txtEmbutido);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox1.ForeColor = System.Drawing.Color.Cornsilk;
            this.groupBox1.Location = new System.Drawing.Point(12, 45);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(282, 85);
            this.groupBox1.TabIndex = 9;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Embutido";
            // 
            // txtCodigoEmbutido
            // 
            this.txtCodigoEmbutido.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.txtCodigoEmbutido.Location = new System.Drawing.Point(88, 22);
            this.txtCodigoEmbutido.Name = "txtCodigoEmbutido";
            this.txtCodigoEmbutido.ReadOnly = true;
            this.txtCodigoEmbutido.Size = new System.Drawing.Size(71, 21);
            this.txtCodigoEmbutido.TabIndex = 11;
            // 
            // label3
            // 
            this.label3.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.ForeColor = System.Drawing.Color.Cornsilk;
            this.label3.Location = new System.Drawing.Point(36, 25);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(46, 15);
            this.label3.TabIndex = 10;
            this.label3.Text = "Código";
            // 
            // txtEmbutido
            // 
            this.txtEmbutido.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.txtEmbutido.Location = new System.Drawing.Point(88, 51);
            this.txtEmbutido.Name = "txtEmbutido";
            this.txtEmbutido.ReadOnly = true;
            this.txtEmbutido.Size = new System.Drawing.Size(145, 21);
            this.txtEmbutido.TabIndex = 9;
            // 
            // label4
            // 
            this.label4.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.ForeColor = System.Drawing.Color.Cornsilk;
            this.label4.Location = new System.Drawing.Point(10, 54);
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
            this.shapeContainer2.Size = new System.Drawing.Size(489, 138);
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
            this.lineShape2.X2 = 468;
            this.lineShape2.Y1 = 37;
            this.lineShape2.Y2 = 37;
            // 
            // label8
            // 
            this.label8.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.label8.AutoSize = true;
            this.label8.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label8.Location = new System.Drawing.Point(319, 342);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(60, 15);
            this.label8.TabIndex = 28;
            this.label8.Text = "Total Kg";
            // 
            // label11
            // 
            this.label11.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(4, 335);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(76, 13);
            this.label11.TabIndex = 29;
            this.label11.Text = "observaciones";
            // 
            // lineShape1
            // 
            this.lineShape1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.lineShape1.Name = "lineShape1";
            this.lineShape1.X1 = 8;
            this.lineShape1.X2 = 478;
            this.lineShape1.Y1 = 398;
            this.lineShape1.Y2 = 398;
            // 
            // shapeContainer1
            // 
            this.shapeContainer1.Location = new System.Drawing.Point(0, 0);
            this.shapeContainer1.Margin = new System.Windows.Forms.Padding(0);
            this.shapeContainer1.Name = "shapeContainer1";
            this.shapeContainer1.Shapes.AddRange(new Microsoft.VisualBasic.PowerPacks.Shape[] {
            this.lineShape1});
            this.shapeContainer1.Size = new System.Drawing.Size(487, 434);
            this.shapeContainer1.TabIndex = 30;
            this.shapeContainer1.TabStop = false;
            // 
            // barraControl
            // 
            this.barraControl.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.barraControl.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.anular});
            this.barraControl.Location = new System.Drawing.Point(0, 0);
            this.barraControl.Name = "barraControl";
            this.barraControl.Padding = new System.Windows.Forms.Padding(10, 0, 0, 0);
            this.barraControl.Size = new System.Drawing.Size(487, 40);
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
            this.anular.Size = new System.Drawing.Size(48, 37);
            this.anular.Text = "Anular";
            this.anular.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.anular.Click += new System.EventHandler(this.anular_Click);
            // 
            // panelAnulado
            // 
            this.panelAnulado.BackColor = System.Drawing.Color.SteelBlue;
            this.panelAnulado.Controls.Add(this.label10);
            this.panelAnulado.Location = new System.Drawing.Point(-2, 0);
            this.panelAnulado.Name = "panelAnulado";
            this.panelAnulado.Size = new System.Drawing.Size(489, 41);
            this.panelAnulado.TabIndex = 32;
            this.panelAnulado.Visible = false;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Underline))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label10.ForeColor = System.Drawing.Color.White;
            this.label10.Location = new System.Drawing.Point(17, 11);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(144, 18);
            this.label10.TabIndex = 7;
            this.label10.Text = "Embutido Anulado";
            // 
            // formInfoEmbutido
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Bisque;
            this.ClientSize = new System.Drawing.Size(487, 434);
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
            this.Controls.Add(this.shapeContainer1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "formInfoEmbutido";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Información Embutido";
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
        private System.Windows.Forms.DateTimePicker txtFechaEmbutido;
        protected System.Windows.Forms.GroupBox groupBox1;
        protected System.Windows.Forms.TextBox txtCodigoEmbutido;
        protected System.Windows.Forms.Label label3;
        protected System.Windows.Forms.TextBox txtEmbutido;
        protected System.Windows.Forms.Label label4;
        private Microsoft.VisualBasic.PowerPacks.ShapeContainer shapeContainer2;
        private Microsoft.VisualBasic.PowerPacks.LineShape lineShape2;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label11;
        private Microsoft.VisualBasic.PowerPacks.LineShape lineShape1;
        private Microsoft.VisualBasic.PowerPacks.ShapeContainer shapeContainer1;
        protected internal System.Windows.Forms.ToolStrip barraControl;
        protected System.Windows.Forms.ToolStripButton anular;
        protected System.Windows.Forms.TextBox txtSucursal;
        private System.Windows.Forms.Panel panelAnulado;
        protected System.Windows.Forms.Label label10;
        private System.Windows.Forms.DataGridViewTextBoxColumn idEmbutido;
        private System.Windows.Forms.DataGridViewTextBoxColumn idCorte;
        private System.Windows.Forms.DataGridViewTextBoxColumn codigo;
        private System.Windows.Forms.DataGridViewTextBoxColumn corte;
        private System.Windows.Forms.DataGridViewTextBoxColumn kgUtilizados;
    }
}