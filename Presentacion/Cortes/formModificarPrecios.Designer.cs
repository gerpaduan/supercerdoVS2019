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
            this.label11 = new System.Windows.Forms.Label();
            this.txtPorcentaje = new System.Windows.Forms.TextBox();
            this.label10 = new System.Windows.Forms.Label();
            this.checkBoxPorcPrecio = new System.Windows.Forms.CheckBox();
            this.boxModificarPrecio = new System.Windows.Forms.GroupBox();
            this.txtPrecioKg = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.txtCodigo = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.txtDescCorte = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.btnBuscarCorteM = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.btnCancelar = new System.Windows.Forms.Button();
            this.btnGuardar = new System.Windows.Forms.Button();
            this.pnlBuscar.SuspendLayout();
            this.boxModificarPrecio.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlBuscar
            // 
            this.pnlBuscar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pnlBuscar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(43)))), ((int)(((byte)(77)))), ((int)(((byte)(129)))));
            this.pnlBuscar.Controls.Add(this.label11);
            this.pnlBuscar.Controls.Add(this.txtPorcentaje);
            this.pnlBuscar.Controls.Add(this.label10);
            this.pnlBuscar.Controls.Add(this.checkBoxPorcPrecio);
            this.pnlBuscar.Controls.Add(this.boxModificarPrecio);
            this.pnlBuscar.Controls.Add(this.label6);
            this.pnlBuscar.Controls.Add(this.label7);
            this.pnlBuscar.Controls.Add(this.label9);
            this.pnlBuscar.Controls.Add(this.label3);
            this.pnlBuscar.Controls.Add(this.btnBuscarCorteM);
            this.pnlBuscar.Controls.Add(this.label2);
            this.pnlBuscar.Controls.Add(this.label8);
            this.pnlBuscar.Location = new System.Drawing.Point(0, 0);
            this.pnlBuscar.Name = "pnlBuscar";
            this.pnlBuscar.Size = new System.Drawing.Size(479, 173);
            this.pnlBuscar.TabIndex = 10;
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label11.ForeColor = System.Drawing.Color.Cornsilk;
            this.label11.Location = new System.Drawing.Point(156, 42);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(273, 15);
            this.label11.TabIndex = 30;
            this.label11.Text = "( Ej: escriba 5 para aplicar 5%  | valores > a -100)";
            // 
            // txtPorcentaje
            // 
            this.txtPorcentaje.Enabled = false;
            this.txtPorcentaje.Location = new System.Drawing.Point(79, 41);
            this.txtPorcentaje.Name = "txtPorcentaje";
            this.txtPorcentaje.Size = new System.Drawing.Size(71, 20);
            this.txtPorcentaje.TabIndex = 23;
            this.txtPorcentaje.TabStop = false;
            this.txtPorcentaje.TextChanged += new System.EventHandler(this.txtPorcentaje_TextChanged);
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label10.ForeColor = System.Drawing.Color.Cornsilk;
            this.label10.Location = new System.Drawing.Point(57, 42);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(18, 15);
            this.label10.TabIndex = 24;
            this.label10.Text = "%";
            // 
            // checkBoxPorcPrecio
            // 
            this.checkBoxPorcPrecio.AutoSize = true;
            this.checkBoxPorcPrecio.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.checkBoxPorcPrecio.ForeColor = System.Drawing.Color.Cornsilk;
            this.checkBoxPorcPrecio.Location = new System.Drawing.Point(26, 10);
            this.checkBoxPorcPrecio.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.checkBoxPorcPrecio.Name = "checkBoxPorcPrecio";
            this.checkBoxPorcPrecio.Size = new System.Drawing.Size(139, 19);
            this.checkBoxPorcPrecio.TabIndex = 29;
            this.checkBoxPorcPrecio.TabStop = false;
            this.checkBoxPorcPrecio.Text = "Aplicar Porcentaje %";
            this.checkBoxPorcPrecio.UseVisualStyleBackColor = true;
            this.checkBoxPorcPrecio.CheckedChanged += new System.EventHandler(this.checkBoxPorcPrecio_CheckedChanged);
            // 
            // boxModificarPrecio
            // 
            this.boxModificarPrecio.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(43)))), ((int)(((byte)(77)))), ((int)(((byte)(129)))));
            this.boxModificarPrecio.Controls.Add(this.txtPrecioKg);
            this.boxModificarPrecio.Controls.Add(this.label5);
            this.boxModificarPrecio.Controls.Add(this.txtCodigo);
            this.boxModificarPrecio.Controls.Add(this.label1);
            this.boxModificarPrecio.Controls.Add(this.txtDescCorte);
            this.boxModificarPrecio.Controls.Add(this.label4);
            this.boxModificarPrecio.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.boxModificarPrecio.ForeColor = System.Drawing.Color.Cornsilk;
            this.boxModificarPrecio.Location = new System.Drawing.Point(10, 67);
            this.boxModificarPrecio.Name = "boxModificarPrecio";
            this.boxModificarPrecio.Size = new System.Drawing.Size(458, 92);
            this.boxModificarPrecio.TabIndex = 0;
            this.boxModificarPrecio.TabStop = false;
            this.boxModificarPrecio.Text = "Modificar Precio";
            // 
            // txtPrecioKg
            // 
            this.txtPrecioKg.Location = new System.Drawing.Point(349, 58);
            this.txtPrecioKg.Name = "txtPrecioKg";
            this.txtPrecioKg.Size = new System.Drawing.Size(85, 21);
            this.txtPrecioKg.TabIndex = 1;
            this.txtPrecioKg.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.TxtPruebaENTER_KeyPress);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.ForeColor = System.Drawing.Color.Cornsilk;
            this.label5.Location = new System.Drawing.Point(287, 60);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(63, 15);
            this.label5.TabIndex = 22;
            this.label5.Text = "Precio Kg.";
            // 
            // txtCodigo
            // 
            this.txtCodigo.Location = new System.Drawing.Point(67, 31);
            this.txtCodigo.Name = "txtCodigo";
            this.txtCodigo.ReadOnly = true;
            this.txtCodigo.Size = new System.Drawing.Size(135, 21);
            this.txtCodigo.TabIndex = 0;
            this.txtCodigo.TabStop = false;
            this.txtCodigo.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.TxtPruebaENTER_KeyPress);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ForeColor = System.Drawing.Color.Cornsilk;
            this.label1.Location = new System.Drawing.Point(19, 34);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(46, 15);
            this.label1.TabIndex = 6;
            this.label1.Text = "Código";
            // 
            // txtDescCorte
            // 
            this.txtDescCorte.Location = new System.Drawing.Point(67, 58);
            this.txtDescCorte.Name = "txtDescCorte";
            this.txtDescCorte.ReadOnly = true;
            this.txtDescCorte.Size = new System.Drawing.Size(214, 21);
            this.txtDescCorte.TabIndex = 1;
            this.txtDescCorte.TabStop = false;
            this.txtDescCorte.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.TxtPruebaENTER_KeyPress);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.ForeColor = System.Drawing.Color.Cornsilk;
            this.label4.Location = new System.Drawing.Point(27, 60);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(36, 15);
            this.label4.TabIndex = 2;
            this.label4.Text = "Prod.";
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
            this.label3.Text = "Prod. Maestro";
            this.label3.Visible = false;
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
            this.label2.Text = "% en Prod. M";
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
            this.label8.Text = "Prod. Independiente";
            this.label8.Visible = false;
            // 
            // btnCancelar
            // 
            this.btnCancelar.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancelar.Location = new System.Drawing.Point(384, 180);
            this.btnCancelar.Name = "btnCancelar";
            this.btnCancelar.Size = new System.Drawing.Size(82, 27);
            this.btnCancelar.TabIndex = 3;
            this.btnCancelar.Text = "&Salir";
            this.btnCancelar.UseVisualStyleBackColor = true;
            this.btnCancelar.Click += new System.EventHandler(this.btnCancelar_Click);
            // 
            // btnGuardar
            // 
            this.btnGuardar.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnGuardar.Location = new System.Drawing.Point(296, 180);
            this.btnGuardar.Name = "btnGuardar";
            this.btnGuardar.Size = new System.Drawing.Size(82, 27);
            this.btnGuardar.TabIndex = 2;
            this.btnGuardar.Text = "&Guardar";
            this.btnGuardar.UseVisualStyleBackColor = true;
            this.btnGuardar.Click += new System.EventHandler(this.btnGuardar_Click);
            // 
            // formModificarPrecios
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(478, 210);
            this.Controls.Add(this.btnGuardar);
            this.Controls.Add(this.btnCancelar);
            this.Controls.Add(this.pnlBuscar);
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.MaximizeBox = false;
            this.Name = "formModificarPrecios";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Modificar Precio";
            this.Load += new System.EventHandler(this.formModificarPrecios_Load_1);
            this.pnlBuscar.ResumeLayout(false);
            this.pnlBuscar.PerformLayout();
            this.boxModificarPrecio.ResumeLayout(false);
            this.boxModificarPrecio.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        protected System.Windows.Forms.Panel pnlBuscar;
        protected System.Windows.Forms.Button btnCancelar;
        protected System.Windows.Forms.GroupBox boxModificarPrecio;
        protected System.Windows.Forms.Label label2;
        protected internal System.Windows.Forms.Button btnBuscarCorteM;
        protected System.Windows.Forms.TextBox txtCodigo;
        protected System.Windows.Forms.Label label1;
        protected System.Windows.Forms.Label label3;
        protected System.Windows.Forms.TextBox txtDescCorte;
        protected System.Windows.Forms.Label label4;
        protected System.Windows.Forms.Label label6;
        protected System.Windows.Forms.TextBox txtPrecioKg;
        protected System.Windows.Forms.Label label5;
        protected System.Windows.Forms.Label label7;
        protected System.Windows.Forms.Label label8;
        protected System.Windows.Forms.Button btnGuardar;
        protected System.Windows.Forms.Label label9;
        protected System.Windows.Forms.TextBox txtPorcentaje;
        protected System.Windows.Forms.Label label10;
        private System.Windows.Forms.CheckBox checkBoxPorcPrecio;
        protected System.Windows.Forms.Label label11;
    }
}