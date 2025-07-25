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
            this.pnlBuscar = new System.Windows.Forms.Panel();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.txtPromedio = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.checkEnCierreStock = new System.Windows.Forms.CheckBox();
            this.checkMayorista = new System.Windows.Forms.CheckBox();
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
            this.btnCancelar = new System.Windows.Forms.Button();
            this.btnGuardar = new System.Windows.Forms.Button();
            this.checkHabilitado = new System.Windows.Forms.CheckBox();
            this.pnlBuscar.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.groupMaestro.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlBuscar
            // 
            this.pnlBuscar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.pnlBuscar.BackColor = System.Drawing.Color.SteelBlue;
            this.pnlBuscar.Controls.Add(this.groupBox1);
            this.pnlBuscar.Location = new System.Drawing.Point(0, 0);
            this.pnlBuscar.Name = "pnlBuscar";
            this.pnlBuscar.Size = new System.Drawing.Size(545, 310);
            this.pnlBuscar.TabIndex = 10;
            // 
            // groupBox1
            // 
            this.groupBox1.BackColor = System.Drawing.Color.SteelBlue;
            this.groupBox1.Controls.Add(this.checkHabilitado);
            this.groupBox1.Controls.Add(this.txtPromedio);
            this.groupBox1.Controls.Add(this.label8);
            this.groupBox1.Controls.Add(this.checkEnCierreStock);
            this.groupBox1.Controls.Add(this.checkMayorista);
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
            this.groupBox1.Size = new System.Drawing.Size(521, 304);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Prod.";
            // 
            // txtPromedio
            // 
            this.txtPromedio.Location = new System.Drawing.Point(93, 139);
            this.txtPromedio.Name = "txtPromedio";
            this.txtPromedio.Size = new System.Drawing.Size(71, 21);
            this.txtPromedio.TabIndex = 33;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label8.ForeColor = System.Drawing.Color.Cornsilk;
            this.label8.Location = new System.Drawing.Point(26, 142);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(61, 15);
            this.label8.TabIndex = 34;
            this.label8.Text = "Promedio";
            // 
            // checkEnCierreStock
            // 
            this.checkEnCierreStock.AutoSize = true;
            this.checkEnCierreStock.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.checkEnCierreStock.Location = new System.Drawing.Point(267, 86);
            this.checkEnCierreStock.Name = "checkEnCierreStock";
            this.checkEnCierreStock.Size = new System.Drawing.Size(110, 19);
            this.checkEnCierreStock.TabIndex = 32;
            this.checkEnCierreStock.Text = "En Cierre Stock";
            this.checkEnCierreStock.UseVisualStyleBackColor = true;
            // 
            // checkMayorista
            // 
            this.checkMayorista.AutoSize = true;
            this.checkMayorista.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.checkMayorista.Location = new System.Drawing.Point(298, 59);
            this.checkMayorista.Name = "checkMayorista";
            this.checkMayorista.Size = new System.Drawing.Size(79, 19);
            this.checkMayorista.TabIndex = 31;
            this.checkMayorista.Text = "Mayorista";
            this.checkMayorista.UseVisualStyleBackColor = true;
            // 
            // checkAsignarMaestro
            // 
            this.checkAsignarMaestro.AutoSize = true;
            this.checkAsignarMaestro.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.checkAsignarMaestro.Location = new System.Drawing.Point(262, 142);
            this.checkAsignarMaestro.Name = "checkAsignarMaestro";
            this.checkAsignarMaestro.Size = new System.Drawing.Size(115, 19);
            this.checkAsignarMaestro.TabIndex = 30;
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
            this.groupMaestro.Location = new System.Drawing.Point(250, 170);
            this.groupMaestro.Name = "groupMaestro";
            this.groupMaestro.Size = new System.Drawing.Size(265, 128);
            this.groupMaestro.TabIndex = 29;
            this.groupMaestro.TabStop = false;
            this.groupMaestro.Text = "Prod. Maestro";
            // 
            // txtDesvioEstandar
            // 
            this.txtDesvioEstandar.Location = new System.Drawing.Point(113, 102);
            this.txtDesvioEstandar.Name = "txtDesvioEstandar";
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
            this.txtPorcentajeCorteM.Text = "100";
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
            this.btnBuscarCorteM.Location = new System.Drawing.Point(228, 20);
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
            this.txtCorteMaestro.Size = new System.Drawing.Size(106, 21);
            this.txtCorteMaestro.TabIndex = 30;
            this.txtCorteMaestro.TextChanged += new System.EventHandler(this.txtCodigo_TextChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.ForeColor = System.Drawing.Color.Cornsilk;
            this.label3.Location = new System.Drawing.Point(71, 24);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(36, 15);
            this.label3.TabIndex = 31;
            this.label3.Text = "Prod.";
            // 
            // txtIndependiente
            // 
            this.txtIndependiente.AutoSize = true;
            this.txtIndependiente.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.txtIndependiente.Checked = true;
            this.txtIndependiente.CheckState = System.Windows.Forms.CheckState.Checked;
            this.txtIndependiente.Location = new System.Drawing.Point(240, 114);
            this.txtIndependiente.Name = "txtIndependiente";
            this.txtIndependiente.Size = new System.Drawing.Size(137, 19);
            this.txtIndependiente.TabIndex = 25;
            this.txtIndependiente.Text = "Prod. Independiente";
            this.txtIndependiente.UseVisualStyleBackColor = true;
            this.txtIndependiente.CheckedChanged += new System.EventHandler(this.txtIndependiente_CheckedChanged);
            // 
            // txtPrecioKg
            // 
            this.txtPrecioKg.Location = new System.Drawing.Point(93, 83);
            this.txtPrecioKg.Name = "txtPrecioKg";
            this.txtPrecioKg.Size = new System.Drawing.Size(71, 21);
            this.txtPrecioKg.TabIndex = 2;
            this.txtPrecioKg.TextChanged += new System.EventHandler(this.txtCodigo_TextChanged);
            this.txtPrecioKg.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.TxtPruebaENTER_KeyPress);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.ForeColor = System.Drawing.Color.Cornsilk;
            this.label5.Location = new System.Drawing.Point(23, 86);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(63, 15);
            this.label5.TabIndex = 22;
            this.label5.Text = "Precio Kg.";
            // 
            // comboTipo
            // 
            this.comboTipo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboTipo.FormattingEnabled = true;
            this.comboTipo.Items.AddRange(new object[] {
            "Corte",
            "Embutido",
            "Unidad",
            "Otro"});
            this.comboTipo.Location = new System.Drawing.Point(93, 110);
            this.comboTipo.Name = "comboTipo";
            this.comboTipo.Size = new System.Drawing.Size(117, 23);
            this.comboTipo.TabIndex = 3;
            this.comboTipo.SelectedIndexChanged += new System.EventHandler(this.comboTipo_SelectedIndexChanged);
            this.comboTipo.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.TxtPruebaENTER_KeyPress);
            this.comboTipo.TextChanged += new System.EventHandler(this.txtCodigo_TextChanged);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.ForeColor = System.Drawing.Color.Cornsilk;
            this.label6.Location = new System.Drawing.Point(55, 113);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(31, 15);
            this.label6.TabIndex = 19;
            this.label6.Text = "Tipo";
            // 
            // txtCodigo
            // 
            this.txtCodigo.Location = new System.Drawing.Point(93, 29);
            this.txtCodigo.Name = "txtCodigo";
            this.txtCodigo.Size = new System.Drawing.Size(71, 21);
            this.txtCodigo.TabIndex = 0;
            this.txtCodigo.TextChanged += new System.EventHandler(this.txtCodigo_TextChanged);
            this.txtCodigo.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.TxtPruebaENTER_KeyPress);
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
            this.txtDescCorte.Size = new System.Drawing.Size(145, 21);
            this.txtDescCorte.TabIndex = 1;
            this.txtDescCorte.TextChanged += new System.EventHandler(this.txtCodigo_TextChanged);
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
            // btnCancelar
            // 
            this.btnCancelar.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancelar.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnCancelar.Location = new System.Drawing.Point(441, 316);
            this.btnCancelar.Name = "btnCancelar";
            this.btnCancelar.Size = new System.Drawing.Size(91, 27);
            this.btnCancelar.TabIndex = 9;
            this.btnCancelar.Text = "&Cancelar";
            this.btnCancelar.UseVisualStyleBackColor = true;
            this.btnCancelar.Click += new System.EventHandler(this.btnCancelar_Click);
            // 
            // btnGuardar
            // 
            this.btnGuardar.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnGuardar.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnGuardar.Location = new System.Drawing.Point(344, 316);
            this.btnGuardar.Name = "btnGuardar";
            this.btnGuardar.Size = new System.Drawing.Size(91, 27);
            this.btnGuardar.TabIndex = 8;
            this.btnGuardar.Text = "&Guardar";
            this.btnGuardar.UseVisualStyleBackColor = true;
            this.btnGuardar.Click += new System.EventHandler(this.btnGuardar_Click);
            // 
            // checkHabilitado
            // 
            this.checkHabilitado.AutoSize = true;
            this.checkHabilitado.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.checkHabilitado.Location = new System.Drawing.Point(295, 31);
            this.checkHabilitado.Name = "checkHabilitado";
            this.checkHabilitado.Size = new System.Drawing.Size(82, 19);
            this.checkHabilitado.TabIndex = 35;
            this.checkHabilitado.Text = "Habilitado";
            this.checkHabilitado.UseVisualStyleBackColor = true;
            // 
            // formNuevoCorte
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(544, 346);
            this.Controls.Add(this.btnGuardar);
            this.Controls.Add(this.btnCancelar);
            this.Controls.Add(this.pnlBuscar);
            this.MaximizeBox = false;
            this.Name = "formNuevoCorte";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Nuevo Prod.";
            this.Load += new System.EventHandler(this.formNuevoCorte_Load);
            this.pnlBuscar.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
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
        private System.Windows.Forms.CheckBox checkMayorista;
        protected System.Windows.Forms.TextBox txtPromedio;
        protected System.Windows.Forms.Label label8;
        private System.Windows.Forms.CheckBox checkHabilitado;
    }
}