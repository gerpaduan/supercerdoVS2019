namespace Presentacion.Personas
{
    partial class formNuevaPersona
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
            this.pnlBuscar = new System.Windows.Forms.Panel();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.comboTipoPersona = new System.Windows.Forms.ComboBox();
            this.label9 = new System.Windows.Forms.Label();
            this.txtOtrosDatos = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.txtRazonSocial = new System.Windows.Forms.TextBox();
            this.btnGuardar = new System.Windows.Forms.Button();
            this.btnCancelar = new System.Windows.Forms.Button();
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
            this.pnlBuscar.Location = new System.Drawing.Point(-1, 0);
            this.pnlBuscar.Name = "pnlBuscar";
            this.pnlBuscar.Size = new System.Drawing.Size(323, 194);
            this.pnlBuscar.TabIndex = 21;
            // 
            // groupBox1
            // 
            this.groupBox1.BackColor = System.Drawing.Color.SteelBlue;
            this.groupBox1.Controls.Add(this.comboTipoPersona);
            this.groupBox1.Controls.Add(this.label9);
            this.groupBox1.Controls.Add(this.txtOtrosDatos);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.txtRazonSocial);
            this.groupBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox1.ForeColor = System.Drawing.Color.Cornsilk;
            this.groupBox1.Location = new System.Drawing.Point(11, 6);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(300, 178);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Persona";
            // 
            // comboTipoPersona
            // 
            this.comboTipoPersona.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboTipoPersona.FormattingEnabled = true;
            this.comboTipoPersona.Items.AddRange(new object[] {
            "Proveedor",
            "Empleado",
            "Cliente",
            "Otro"});
            this.comboTipoPersona.Location = new System.Drawing.Point(127, 25);
            this.comboTipoPersona.Name = "comboTipoPersona";
            this.comboTipoPersona.Size = new System.Drawing.Size(116, 23);
            this.comboTipoPersona.TabIndex = 0;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label9.ForeColor = System.Drawing.Color.Cornsilk;
            this.label9.Location = new System.Drawing.Point(90, 28);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(31, 15);
            this.label9.TabIndex = 23;
            this.label9.Text = "Tipo";
            // 
            // txtOtrosDatos
            // 
            this.txtOtrosDatos.Location = new System.Drawing.Point(128, 95);
            this.txtOtrosDatos.Multiline = true;
            this.txtOtrosDatos.Name = "txtOtrosDatos";
            this.txtOtrosDatos.Size = new System.Drawing.Size(150, 66);
            this.txtOtrosDatos.TabIndex = 2;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.ForeColor = System.Drawing.Color.Cornsilk;
            this.label2.Location = new System.Drawing.Point(51, 95);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(71, 15);
            this.label2.TabIndex = 7;
            this.label2.Text = "Otros Datos";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ForeColor = System.Drawing.Color.Cornsilk;
            this.label1.Location = new System.Drawing.Point(42, 60);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(80, 15);
            this.label1.TabIndex = 6;
            this.label1.Text = "Razon Social";
            // 
            // txtRazonSocial
            // 
            this.txtRazonSocial.Location = new System.Drawing.Point(128, 57);
            this.txtRazonSocial.Name = "txtRazonSocial";
            this.txtRazonSocial.Size = new System.Drawing.Size(150, 21);
            this.txtRazonSocial.TabIndex = 1;
            // 
            // btnGuardar
            // 
            this.btnGuardar.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnGuardar.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnGuardar.Location = new System.Drawing.Point(130, 199);
            this.btnGuardar.Name = "btnGuardar";
            this.btnGuardar.Size = new System.Drawing.Size(89, 27);
            this.btnGuardar.TabIndex = 3;
            this.btnGuardar.Text = "&Guardar";
            this.btnGuardar.UseVisualStyleBackColor = true;
            this.btnGuardar.Click += new System.EventHandler(this.btnGuardar_Click);
            // 
            // btnCancelar
            // 
            this.btnCancelar.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancelar.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnCancelar.Location = new System.Drawing.Point(225, 199);
            this.btnCancelar.Name = "btnCancelar";
            this.btnCancelar.Size = new System.Drawing.Size(84, 27);
            this.btnCancelar.TabIndex = 4;
            this.btnCancelar.Text = "&Cancelar";
            this.btnCancelar.UseVisualStyleBackColor = true;
            this.btnCancelar.Click += new System.EventHandler(this.btnCancelar_Click);
            // 
            // formNuevaPersona
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Bisque;
            this.ClientSize = new System.Drawing.Size(321, 229);
            this.Controls.Add(this.pnlBuscar);
            this.Controls.Add(this.btnGuardar);
            this.Controls.Add(this.btnCancelar);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "formNuevaPersona";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Nueva Persona";
            this.pnlBuscar.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        protected System.Windows.Forms.Panel pnlBuscar;
        protected System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.ComboBox comboTipoPersona;
        protected System.Windows.Forms.Label label9;
        protected System.Windows.Forms.TextBox txtOtrosDatos;
        protected System.Windows.Forms.Label label2;
        protected System.Windows.Forms.Label label1;
        protected System.Windows.Forms.TextBox txtRazonSocial;
        protected System.Windows.Forms.Button btnGuardar;
        protected System.Windows.Forms.Button btnCancelar;

    }
}