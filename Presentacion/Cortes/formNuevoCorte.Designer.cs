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
            this.txtDesvioEstandar = new System.Windows.Forms.MaskedTextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.txtIndependiente = new System.Windows.Forms.CheckBox();
            this.txtPorcHueso = new System.Windows.Forms.MaskedTextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.txtPrecioKg = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.txtPorcentajeCorteM = new System.Windows.Forms.MaskedTextBox();
            this.comboTipo = new System.Windows.Forms.ComboBox();
            this.label6 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.btnBuscarCorteM = new System.Windows.Forms.Button();
            this.txtCodigo = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.txtCorteMaestro = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.txtDescCorte = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
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
            this.pnlBuscar.Controls.Add(this.groupBox1);
            this.pnlBuscar.Location = new System.Drawing.Point(0, 0);
            this.pnlBuscar.Name = "pnlBuscar";
            this.pnlBuscar.Size = new System.Drawing.Size(545, 171);
            this.pnlBuscar.TabIndex = 10;
            // 
            // groupBox1
            // 
            this.groupBox1.BackColor = System.Drawing.Color.SteelBlue;
            this.groupBox1.Controls.Add(this.txtDesvioEstandar);
            this.groupBox1.Controls.Add(this.label9);
            this.groupBox1.Controls.Add(this.label8);
            this.groupBox1.Controls.Add(this.txtIndependiente);
            this.groupBox1.Controls.Add(this.txtPorcHueso);
            this.groupBox1.Controls.Add(this.label7);
            this.groupBox1.Controls.Add(this.txtPrecioKg);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.txtPorcentajeCorteM);
            this.groupBox1.Controls.Add(this.comboTipo);
            this.groupBox1.Controls.Add(this.label6);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.btnBuscarCorteM);
            this.groupBox1.Controls.Add(this.txtCodigo);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.txtCorteMaestro);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.txtDescCorte);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox1.ForeColor = System.Drawing.Color.Cornsilk;
            this.groupBox1.Location = new System.Drawing.Point(11, 3);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(521, 165);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Corte";
            // 
            // txtDesvioEstandar
            // 
            this.txtDesvioEstandar.Location = new System.Drawing.Point(363, 134);
            this.txtDesvioEstandar.Name = "txtDesvioEstandar";
            this.txtDesvioEstandar.Size = new System.Drawing.Size(75, 21);
            this.txtDesvioEstandar.TabIndex = 7;
            this.txtDesvioEstandar.Text = "0";
            this.txtDesvioEstandar.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.txtDesvioEstandar.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.TxtPruebaENTER_KeyPress);
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label9.ForeColor = System.Drawing.Color.Cornsilk;
            this.label9.Location = new System.Drawing.Point(264, 137);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(96, 15);
            this.label9.TabIndex = 28;
            this.label9.Text = "Desvío Estandar";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label8.ForeColor = System.Drawing.Color.Cornsilk;
            this.label8.Location = new System.Drawing.Point(239, 32);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(118, 15);
            this.label8.TabIndex = 26;
            this.label8.Text = "Corte Independiente";
            // 
            // txtIndependiente
            // 
            this.txtIndependiente.AutoSize = true;
            this.txtIndependiente.Location = new System.Drawing.Point(363, 33);
            this.txtIndependiente.Name = "txtIndependiente";
            this.txtIndependiente.Size = new System.Drawing.Size(15, 14);
            this.txtIndependiente.TabIndex = 25;
            this.txtIndependiente.UseVisualStyleBackColor = true;
            // 
            // txtPorcHueso
            // 
            this.txtPorcHueso.Location = new System.Drawing.Point(363, 107);
            this.txtPorcHueso.Name = "txtPorcHueso";
            this.txtPorcHueso.Size = new System.Drawing.Size(75, 21);
            this.txtPorcHueso.TabIndex = 6;
            this.txtPorcHueso.Text = "0";
            this.txtPorcHueso.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.txtPorcHueso.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.TxtPruebaENTER_KeyPress);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label7.ForeColor = System.Drawing.Color.Cornsilk;
            this.label7.Location = new System.Drawing.Point(273, 110);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(87, 15);
            this.label7.TabIndex = 24;
            this.label7.Text = "% Desperdicio";
            // 
            // txtPrecioKg
            // 
            this.txtPrecioKg.Location = new System.Drawing.Point(93, 83);
            this.txtPrecioKg.Name = "txtPrecioKg";
            this.txtPrecioKg.Size = new System.Drawing.Size(71, 21);
            this.txtPrecioKg.TabIndex = 2;
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
            // txtPorcentajeCorteM
            // 
            this.txtPorcentajeCorteM.Location = new System.Drawing.Point(363, 80);
            this.txtPorcentajeCorteM.Name = "txtPorcentajeCorteM";
            this.txtPorcentajeCorteM.Size = new System.Drawing.Size(75, 21);
            this.txtPorcentajeCorteM.TabIndex = 5;
            this.txtPorcentajeCorteM.Text = "0";
            this.txtPorcentajeCorteM.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.txtPorcentajeCorteM.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.TxtPruebaENTER_KeyPress);
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
            this.comboTipo.VisibleChanged += new System.EventHandler(this.comboTipo_TextChanged);
            this.comboTipo.SelectedIndexChanged += new System.EventHandler(this.comboTipo_TextChanged);
            this.comboTipo.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.TxtPruebaENTER_KeyPress);
            this.comboTipo.SelectedValueChanged += new System.EventHandler(this.comboTipo_TextChanged);
            this.comboTipo.TextChanged += new System.EventHandler(this.comboTipo_TextChanged);
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
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.ForeColor = System.Drawing.Color.Cornsilk;
            this.label2.Location = new System.Drawing.Point(276, 83);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(81, 15);
            this.label2.TabIndex = 15;
            this.label2.Text = "% en Corte M";
            // 
            // btnBuscarCorteM
            // 
            this.btnBuscarCorteM.AccessibleDescription = "";
            this.btnBuscarCorteM.ForeColor = System.Drawing.Color.Black;
            this.btnBuscarCorteM.Image = ((System.Drawing.Image)(resources.GetObject("btnBuscarCorteM.Image")));
            this.btnBuscarCorteM.Location = new System.Drawing.Point(478, 52);
            this.btnBuscarCorteM.Name = "btnBuscarCorteM";
            this.btnBuscarCorteM.Size = new System.Drawing.Size(28, 23);
            this.btnBuscarCorteM.TabIndex = 4;
            this.btnBuscarCorteM.UseVisualStyleBackColor = true;
            this.btnBuscarCorteM.Click += new System.EventHandler(this.btnBuscarCorteM_Click);
            // 
            // txtCodigo
            // 
            this.txtCodigo.Location = new System.Drawing.Point(93, 29);
            this.txtCodigo.Name = "txtCodigo";
            this.txtCodigo.Size = new System.Drawing.Size(71, 21);
            this.txtCodigo.TabIndex = 0;
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
            // txtCorteMaestro
            // 
            this.txtCorteMaestro.Location = new System.Drawing.Point(363, 53);
            this.txtCorteMaestro.Name = "txtCorteMaestro";
            this.txtCorteMaestro.ReadOnly = true;
            this.txtCorteMaestro.Size = new System.Drawing.Size(106, 21);
            this.txtCorteMaestro.TabIndex = 4;
            this.txtCorteMaestro.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.TxtPruebaENTER_KeyPress);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.ForeColor = System.Drawing.Color.Cornsilk;
            this.label3.Location = new System.Drawing.Point(273, 56);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(84, 15);
            this.label3.TabIndex = 4;
            this.label3.Text = "Corte Maestro";
            // 
            // txtDescCorte
            // 
            this.txtDescCorte.Location = new System.Drawing.Point(93, 56);
            this.txtDescCorte.Name = "txtDescCorte";
            this.txtDescCorte.Size = new System.Drawing.Size(145, 21);
            this.txtDescCorte.TabIndex = 1;
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
            this.label4.Text = "Corte";
            // 
            // btnCancelar
            // 
            this.btnCancelar.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancelar.Location = new System.Drawing.Point(450, 177);
            this.btnCancelar.Name = "btnCancelar";
            this.btnCancelar.Size = new System.Drawing.Size(82, 27);
            this.btnCancelar.TabIndex = 9;
            this.btnCancelar.Text = "Cancelar";
            this.btnCancelar.UseVisualStyleBackColor = true;
            this.btnCancelar.Click += new System.EventHandler(this.btnCancelar_Click);
            // 
            // btnGuardar
            // 
            this.btnGuardar.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnGuardar.Location = new System.Drawing.Point(362, 177);
            this.btnGuardar.Name = "btnGuardar";
            this.btnGuardar.Size = new System.Drawing.Size(82, 27);
            this.btnGuardar.TabIndex = 8;
            this.btnGuardar.Text = "Guardar";
            this.btnGuardar.UseVisualStyleBackColor = true;
            this.btnGuardar.Click += new System.EventHandler(this.btnGuardar_Click);
            // 
            // formNuevoCorte
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(544, 207);
            this.Controls.Add(this.btnGuardar);
            this.Controls.Add(this.btnCancelar);
            this.Controls.Add(this.pnlBuscar);
            this.MaximizeBox = false;
            this.Name = "formNuevoCorte";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Nuevo Corte";
            this.Load += new System.EventHandler(this.formNuevoCorte_Load);
            this.pnlBuscar.ResumeLayout(false);
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