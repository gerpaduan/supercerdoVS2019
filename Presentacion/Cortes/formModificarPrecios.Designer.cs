namespace Presentacion
{
    public partial class formModificarPrecios
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(formModificarPrecios));
            this.pnlBuscar = new System.Windows.Forms.Panel();
            this.txtDesvioEstandar = new System.Windows.Forms.MaskedTextBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.txtPrecioKg = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.txtCodigo = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.txtDescCorte = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.txtCorteMaestro = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.comboTipo = new System.Windows.Forms.ComboBox();
            this.btnBuscarCorteM = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.txtPorcHueso = new System.Windows.Forms.MaskedTextBox();
            this.txtPorcentajeCorteM = new System.Windows.Forms.MaskedTextBox();
            this.txtIndependiente = new System.Windows.Forms.CheckBox();
            this.btnCancelar = new System.Windows.Forms.Button();
            this.btnGuardar = new System.Windows.Forms.Button();
            this.pnlBuscar.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlBuscar
            // 
            this.pnlBuscar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.pnlBuscar.BackColor = System.Drawing.Color.SteelBlue;
            this.pnlBuscar.Controls.Add(this.txtDesvioEstandar);
            this.pnlBuscar.Controls.Add(this.groupBox1);
            this.pnlBuscar.Controls.Add(this.label6);
            this.pnlBuscar.Controls.Add(this.label7);
            this.pnlBuscar.Controls.Add(this.txtCorteMaestro);
            this.pnlBuscar.Controls.Add(this.label9);
            this.pnlBuscar.Controls.Add(this.label3);
            this.pnlBuscar.Controls.Add(this.comboTipo);
            this.pnlBuscar.Controls.Add(this.btnBuscarCorteM);
            this.pnlBuscar.Controls.Add(this.label2);
            this.pnlBuscar.Controls.Add(this.label8);
            this.pnlBuscar.Controls.Add(this.txtPorcHueso);
            this.pnlBuscar.Controls.Add(this.txtPorcentajeCorteM);
            this.pnlBuscar.Controls.Add(this.txtIndependiente);
            this.pnlBuscar.Location = new System.Drawing.Point(0, 0);
            this.pnlBuscar.Name = "pnlBuscar";
            this.pnlBuscar.Size = new System.Drawing.Size(479, 120);
            this.pnlBuscar.TabIndex = 10;
            // 
            // txtDesvioEstandar
            // 
            this.txtDesvioEstandar.ForeColor = System.Drawing.Color.Transparent;
            this.txtDesvioEstandar.Location = new System.Drawing.Point(846, 130);
            this.txtDesvioEstandar.Name = "txtDesvioEstandar";
            this.txtDesvioEstandar.Size = new System.Drawing.Size(75, 20);
            this.txtDesvioEstandar.TabIndex = 7;
            this.txtDesvioEstandar.Text = "0";
            this.txtDesvioEstandar.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.txtDesvioEstandar.Visible = false;
            this.txtDesvioEstandar.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.TxtPruebaENTER_KeyPress);
            // 
            // groupBox1
            // 
            this.groupBox1.BackColor = System.Drawing.Color.SteelBlue;
            this.groupBox1.Controls.Add(this.txtPrecioKg);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.txtCodigo);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.txtDescCorte);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox1.ForeColor = System.Drawing.Color.Cornsilk;
            this.groupBox1.Location = new System.Drawing.Point(8, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(458, 102);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Modificar Precio";
            // 
            // txtPrecioKg
            // 
            this.txtPrecioKg.Location = new System.Drawing.Point(319, 56);
            this.txtPrecioKg.Name = "txtPrecioKg";
            this.txtPrecioKg.Size = new System.Drawing.Size(71, 21);
            this.txtPrecioKg.TabIndex = 1;
            this.txtPrecioKg.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.TxtPruebaENTER_KeyPress);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.ForeColor = System.Drawing.Color.Cornsilk;
            this.label5.Location = new System.Drawing.Point(249, 59);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(63, 15);
            this.label5.TabIndex = 22;
            this.label5.Text = "Precio Kg.";
            // 
            // txtCodigo
            // 
            this.txtCodigo.Location = new System.Drawing.Point(98, 29);
            this.txtCodigo.Name = "txtCodigo";
            this.txtCodigo.ReadOnly = true;
            this.txtCodigo.Size = new System.Drawing.Size(71, 21);
            this.txtCodigo.TabIndex = 0;
            this.txtCodigo.TabStop = false;
            this.txtCodigo.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.TxtPruebaENTER_KeyPress);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ForeColor = System.Drawing.Color.Cornsilk;
            this.label1.Location = new System.Drawing.Point(46, 32);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(46, 15);
            this.label1.TabIndex = 6;
            this.label1.Text = "Código";
            // 
            // txtDescCorte
            // 
            this.txtDescCorte.Location = new System.Drawing.Point(98, 56);
            this.txtDescCorte.Name = "txtDescCorte";
            this.txtDescCorte.ReadOnly = true;
            this.txtDescCorte.Size = new System.Drawing.Size(145, 21);
            this.txtDescCorte.TabIndex = 1;
            this.txtDescCorte.TabStop = false;
            this.txtDescCorte.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.TxtPruebaENTER_KeyPress);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.ForeColor = System.Drawing.Color.Cornsilk;
            this.label4.Location = new System.Drawing.Point(55, 59);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(36, 15);
            this.label4.TabIndex = 2;
            this.label4.Text = "Corte";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.ForeColor = System.Drawing.Color.Transparent;
            this.label6.Location = new System.Drawing.Point(538, 109);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(31, 15);
            this.label6.TabIndex = 19;
            this.label6.Text = "Tipo";
            this.label6.Visible = false;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label7.ForeColor = System.Drawing.Color.Transparent;
            this.label7.Location = new System.Drawing.Point(756, 106);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(87, 15);
            this.label7.TabIndex = 24;
            this.label7.Text = "% Desperdicio";
            this.label7.Visible = false;
            // 
            // txtCorteMaestro
            // 
            this.txtCorteMaestro.Location = new System.Drawing.Point(628, 59);
            this.txtCorteMaestro.Name = "txtCorteMaestro";
            this.txtCorteMaestro.ReadOnly = true;
            this.txtCorteMaestro.Size = new System.Drawing.Size(106, 20);
            this.txtCorteMaestro.TabIndex = 4;
            this.txtCorteMaestro.Visible = false;
            this.txtCorteMaestro.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.TxtPruebaENTER_KeyPress);
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label9.ForeColor = System.Drawing.Color.Transparent;
            this.label9.Location = new System.Drawing.Point(747, 133);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(96, 15);
            this.label9.TabIndex = 28;
            this.label9.Text = "Desvío Estandar";
            this.label9.Visible = false;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.ForeColor = System.Drawing.Color.Cornsilk;
            this.label3.Location = new System.Drawing.Point(538, 62);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(84, 15);
            this.label3.TabIndex = 4;
            this.label3.Text = "Corte Maestro";
            this.label3.Visible = false;
            // 
            // comboTipo
            // 
            this.comboTipo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboTipo.ForeColor = System.Drawing.Color.Transparent;
            this.comboTipo.FormattingEnabled = true;
            this.comboTipo.Items.AddRange(new object[] {
            "Corte",
            "Embutido",
            "Otro"});
            this.comboTipo.Location = new System.Drawing.Point(576, 106);
            this.comboTipo.Name = "comboTipo";
            this.comboTipo.Size = new System.Drawing.Size(117, 21);
            this.comboTipo.TabIndex = 3;
            this.comboTipo.Visible = false;
            this.comboTipo.VisibleChanged += new System.EventHandler(this.comboTipo_TextChanged);
            this.comboTipo.SelectedIndexChanged += new System.EventHandler(this.comboTipo_TextChanged);
            this.comboTipo.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.TxtPruebaENTER_KeyPress);
            this.comboTipo.SelectedValueChanged += new System.EventHandler(this.comboTipo_TextChanged);
            this.comboTipo.TextChanged += new System.EventHandler(this.comboTipo_TextChanged);
            // 
            // btnBuscarCorteM
            // 
            this.btnBuscarCorteM.AccessibleDescription = "";
            this.btnBuscarCorteM.ForeColor = System.Drawing.Color.Transparent;
            this.btnBuscarCorteM.Image = ((System.Drawing.Image)(resources.GetObject("btnBuscarCorteM.Image")));
            this.btnBuscarCorteM.Location = new System.Drawing.Point(961, 48);
            this.btnBuscarCorteM.Name = "btnBuscarCorteM";
            this.btnBuscarCorteM.Size = new System.Drawing.Size(28, 23);
            this.btnBuscarCorteM.TabIndex = 4;
            this.btnBuscarCorteM.UseVisualStyleBackColor = true;
            this.btnBuscarCorteM.Visible = false;
            this.btnBuscarCorteM.Click += new System.EventHandler(this.btnBuscarCorteM_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.ForeColor = System.Drawing.Color.Transparent;
            this.label2.Location = new System.Drawing.Point(759, 79);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(81, 15);
            this.label2.TabIndex = 15;
            this.label2.Text = "% en Corte M";
            this.label2.Visible = false;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label8.ForeColor = System.Drawing.Color.Transparent;
            this.label8.Location = new System.Drawing.Point(722, 28);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(118, 15);
            this.label8.TabIndex = 26;
            this.label8.Text = "Corte Independiente";
            this.label8.Visible = false;
            // 
            // txtPorcHueso
            // 
            this.txtPorcHueso.ForeColor = System.Drawing.Color.Transparent;
            this.txtPorcHueso.Location = new System.Drawing.Point(846, 103);
            this.txtPorcHueso.Name = "txtPorcHueso";
            this.txtPorcHueso.Size = new System.Drawing.Size(75, 20);
            this.txtPorcHueso.TabIndex = 6;
            this.txtPorcHueso.Text = "0";
            this.txtPorcHueso.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.txtPorcHueso.Visible = false;
            this.txtPorcHueso.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.TxtPruebaENTER_KeyPress);
            // 
            // txtPorcentajeCorteM
            // 
            this.txtPorcentajeCorteM.ForeColor = System.Drawing.Color.Transparent;
            this.txtPorcentajeCorteM.Location = new System.Drawing.Point(846, 76);
            this.txtPorcentajeCorteM.Name = "txtPorcentajeCorteM";
            this.txtPorcentajeCorteM.Size = new System.Drawing.Size(75, 20);
            this.txtPorcentajeCorteM.TabIndex = 5;
            this.txtPorcentajeCorteM.Text = "0";
            this.txtPorcentajeCorteM.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.txtPorcentajeCorteM.Visible = false;
            this.txtPorcentajeCorteM.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.TxtPruebaENTER_KeyPress);
            // 
            // txtIndependiente
            // 
            this.txtIndependiente.AutoSize = true;
            this.txtIndependiente.ForeColor = System.Drawing.Color.Transparent;
            this.txtIndependiente.Location = new System.Drawing.Point(846, 29);
            this.txtIndependiente.Name = "txtIndependiente";
            this.txtIndependiente.Size = new System.Drawing.Size(15, 14);
            this.txtIndependiente.TabIndex = 25;
            this.txtIndependiente.UseVisualStyleBackColor = true;
            this.txtIndependiente.Visible = false;
            // 
            // btnCancelar
            // 
            this.btnCancelar.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancelar.Location = new System.Drawing.Point(384, 127);
            this.btnCancelar.Name = "btnCancelar";
            this.btnCancelar.Size = new System.Drawing.Size(82, 27);
            this.btnCancelar.TabIndex = 3;
            this.btnCancelar.Text = "Cancelar";
            this.btnCancelar.UseVisualStyleBackColor = true;
            this.btnCancelar.Click += new System.EventHandler(this.btnCancelar_Click);
            // 
            // btnGuardar
            // 
            this.btnGuardar.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnGuardar.Location = new System.Drawing.Point(296, 127);
            this.btnGuardar.Name = "btnGuardar";
            this.btnGuardar.Size = new System.Drawing.Size(82, 27);
            this.btnGuardar.TabIndex = 2;
            this.btnGuardar.Text = "Guardar";
            this.btnGuardar.UseVisualStyleBackColor = true;
            this.btnGuardar.Click += new System.EventHandler(this.btnGuardar_Click);
            // 
            // formModificarPrecios
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(478, 157);
            this.Controls.Add(this.btnGuardar);
            this.Controls.Add(this.btnCancelar);
            this.Controls.Add(this.pnlBuscar);
            this.MaximizeBox = false;
            this.Name = "formModificarPrecios";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Modificar Precio";
            this.pnlBuscar.ResumeLayout(false);
            this.pnlBuscar.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        protected System.Windows.Forms.Panel pnlBuscar;
        protected System.Windows.Forms.Button btnCancelar;
        protected System.Windows.Forms.GroupBox groupBox1;
        protected System.Windows.Forms.Label label2;
        protected internal System.Windows.Forms.Button btnBuscarCorteM;
        protected System.Windows.Forms.TextBox txtCodigo;
        protected System.Windows.Forms.Label label1;
        protected System.Windows.Forms.TextBox txtCorteMaestro;
        protected System.Windows.Forms.Label label3;
        protected System.Windows.Forms.TextBox txtDescCorte;
        protected System.Windows.Forms.Label label4;
        protected System.Windows.Forms.Label label6;
        protected System.Windows.Forms.ComboBox comboTipo;
        private System.Windows.Forms.MaskedTextBox txtPorcentajeCorteM;
        protected System.Windows.Forms.TextBox txtPrecioKg;
        protected System.Windows.Forms.Label label5;
        private System.Windows.Forms.MaskedTextBox txtPorcHueso;
        protected System.Windows.Forms.Label label7;
        private System.Windows.Forms.CheckBox txtIndependiente;
        protected System.Windows.Forms.Label label8;
        protected System.Windows.Forms.Button btnGuardar;
        private System.Windows.Forms.MaskedTextBox txtDesvioEstandar;
        protected System.Windows.Forms.Label label9;
    }
}