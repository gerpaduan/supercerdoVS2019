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
            this.btnBuscarAfip = new System.Windows.Forms.Button();
            this.btnCopiarRS = new System.Windows.Forms.Button();
            this.lblNombreIdentif = new System.Windows.Forms.Label();
            this.txtIdentificacion = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.comboIva = new System.Windows.Forms.ComboBox();
            this.txtCuit = new System.Windows.Forms.MaskedTextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.txtCiudad = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.txtDomicilio = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.txtTelefono = new System.Windows.Forms.TextBox();
            this.lblBonificacion = new System.Windows.Forms.Label();
            this.txtBonificacion = new System.Windows.Forms.TextBox();
            this.checkCtaCte = new System.Windows.Forms.CheckBox();
            this.txtOtrosDatos = new System.Windows.Forms.TextBox();
            this.lblOtrosDatos = new System.Windows.Forms.Label();
            this.lblRazonSocial = new System.Windows.Forms.Label();
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
            this.pnlBuscar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(43)))), ((int)(((byte)(77)))), ((int)(((byte)(129)))));
            this.pnlBuscar.Controls.Add(this.groupBox1);
            this.pnlBuscar.Location = new System.Drawing.Point(-1, 0);
            this.pnlBuscar.Margin = new System.Windows.Forms.Padding(4);
            this.pnlBuscar.Name = "pnlBuscar";
            this.pnlBuscar.Size = new System.Drawing.Size(633, 346);
            this.pnlBuscar.TabIndex = 21;
            // 
            // groupBox1
            // 
            this.groupBox1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(43)))), ((int)(((byte)(77)))), ((int)(((byte)(129)))));
            this.groupBox1.Controls.Add(this.btnBuscarAfip);
            this.groupBox1.Controls.Add(this.btnCopiarRS);
            this.groupBox1.Controls.Add(this.lblNombreIdentif);
            this.groupBox1.Controls.Add(this.txtIdentificacion);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.comboIva);
            this.groupBox1.Controls.Add(this.txtCuit);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.txtCiudad);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.txtDomicilio);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.txtTelefono);
            this.groupBox1.Controls.Add(this.lblBonificacion);
            this.groupBox1.Controls.Add(this.txtBonificacion);
            this.groupBox1.Controls.Add(this.checkCtaCte);
            this.groupBox1.Controls.Add(this.txtOtrosDatos);
            this.groupBox1.Controls.Add(this.lblOtrosDatos);
            this.groupBox1.Controls.Add(this.lblRazonSocial);
            this.groupBox1.Controls.Add(this.txtRazonSocial);
            this.groupBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox1.ForeColor = System.Drawing.Color.Cornsilk;
            this.groupBox1.Location = new System.Drawing.Point(15, 5);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(4);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(4);
            this.groupBox1.Size = new System.Drawing.Size(601, 322);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Persona";
            // 
            // btnBuscarAfip
            // 
            this.btnBuscarAfip.ForeColor = System.Drawing.Color.Black;
            this.btnBuscarAfip.Location = new System.Drawing.Point(355, 110);
            this.btnBuscarAfip.Margin = new System.Windows.Forms.Padding(4);
            this.btnBuscarAfip.Name = "btnBuscarAfip";
            this.btnBuscarAfip.Size = new System.Drawing.Size(126, 28);
            this.btnBuscarAfip.TabIndex = 55;
            this.btnBuscarAfip.Text = "Buscar Cuit Afip";
            this.btnBuscarAfip.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            this.btnBuscarAfip.UseVisualStyleBackColor = true;
            this.btnBuscarAfip.Click += new System.EventHandler(this.btnBuscarAfip_Click);
            // 
            // btnCopiarRS
            // 
            this.btnCopiarRS.ForeColor = System.Drawing.Color.Black;
            this.btnCopiarRS.Location = new System.Drawing.Point(491, 28);
            this.btnCopiarRS.Margin = new System.Windows.Forms.Padding(4);
            this.btnCopiarRS.Name = "btnCopiarRS";
            this.btnCopiarRS.Size = new System.Drawing.Size(97, 28);
            this.btnCopiarRS.TabIndex = 54;
            this.btnCopiarRS.Text = "Copiar RS";
            this.btnCopiarRS.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            this.btnCopiarRS.UseVisualStyleBackColor = true;
            this.btnCopiarRS.Click += new System.EventHandler(this.btnCopiarRS_Click);
            // 
            // lblNombreIdentif
            // 
            this.lblNombreIdentif.AutoSize = true;
            this.lblNombreIdentif.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblNombreIdentif.ForeColor = System.Drawing.Color.Cornsilk;
            this.lblNombreIdentif.Location = new System.Drawing.Point(20, 32);
            this.lblNombreIdentif.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblNombreIdentif.Name = "lblNombreIdentif";
            this.lblNombreIdentif.Size = new System.Drawing.Size(108, 18);
            this.lblNombreIdentif.TabIndex = 53;
            this.lblNombreIdentif.Text = "Nombre Identif.";
            // 
            // txtIdentificacion
            // 
            this.txtIdentificacion.Location = new System.Drawing.Point(148, 28);
            this.txtIdentificacion.Margin = new System.Windows.Forms.Padding(4);
            this.txtIdentificacion.Name = "txtIdentificacion";
            this.txtIdentificacion.Size = new System.Drawing.Size(333, 24);
            this.txtIdentificacion.TabIndex = 0;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.ForeColor = System.Drawing.Color.Cornsilk;
            this.label5.Location = new System.Drawing.Point(109, 85);
            this.label5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(29, 18);
            this.label5.TabIndex = 51;
            this.label5.Text = "IVA";
            // 
            // comboIva
            // 
            this.comboIva.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboIva.FormattingEnabled = true;
            this.comboIva.Location = new System.Drawing.Point(149, 81);
            this.comboIva.Margin = new System.Windows.Forms.Padding(4);
            this.comboIva.Name = "comboIva";
            this.comboIva.Size = new System.Drawing.Size(199, 26);
            this.comboIva.TabIndex = 2;
            // 
            // txtCuit
            // 
            this.txtCuit.Location = new System.Drawing.Point(148, 111);
            this.txtCuit.Margin = new System.Windows.Forms.Padding(4);
            this.txtCuit.Mask = "00-00000000-0";
            this.txtCuit.Name = "txtCuit";
            this.txtCuit.Size = new System.Drawing.Size(199, 24);
            this.txtCuit.TabIndex = 3;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.ForeColor = System.Drawing.Color.Cornsilk;
            this.label4.Location = new System.Drawing.Point(65, 140);
            this.label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(66, 18);
            this.label4.TabIndex = 36;
            this.label4.Text = "Telefono";
            // 
            // txtCiudad
            // 
            this.txtCiudad.Location = new System.Drawing.Point(148, 188);
            this.txtCiudad.Margin = new System.Windows.Forms.Padding(4);
            this.txtCiudad.Name = "txtCiudad";
            this.txtCiudad.Size = new System.Drawing.Size(199, 24);
            this.txtCiudad.TabIndex = 6;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.ForeColor = System.Drawing.Color.Cornsilk;
            this.label3.Location = new System.Drawing.Point(79, 192);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(54, 18);
            this.label3.TabIndex = 34;
            this.label3.Text = "Ciudad";
            // 
            // txtDomicilio
            // 
            this.txtDomicilio.Location = new System.Drawing.Point(148, 162);
            this.txtDomicilio.Margin = new System.Windows.Forms.Padding(4);
            this.txtDomicilio.Name = "txtDomicilio";
            this.txtDomicilio.Size = new System.Drawing.Size(199, 24);
            this.txtDomicilio.TabIndex = 5;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.ForeColor = System.Drawing.Color.Cornsilk;
            this.label2.Location = new System.Drawing.Point(60, 166);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(70, 18);
            this.label2.TabIndex = 32;
            this.label2.Text = "Domicilio";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ForeColor = System.Drawing.Color.Cornsilk;
            this.label1.Location = new System.Drawing.Point(95, 114);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(42, 18);
            this.label1.TabIndex = 30;
            this.label1.Text = "CUIT";
            // 
            // txtTelefono
            // 
            this.txtTelefono.Location = new System.Drawing.Point(148, 137);
            this.txtTelefono.Margin = new System.Windows.Forms.Padding(4);
            this.txtTelefono.Name = "txtTelefono";
            this.txtTelefono.Size = new System.Drawing.Size(199, 24);
            this.txtTelefono.TabIndex = 4;
            // 
            // lblBonificacion
            // 
            this.lblBonificacion.AutoSize = true;
            this.lblBonificacion.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblBonificacion.ForeColor = System.Drawing.Color.Cornsilk;
            this.lblBonificacion.Location = new System.Drawing.Point(363, 192);
            this.lblBonificacion.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblBonificacion.Name = "lblBonificacion";
            this.lblBonificacion.Size = new System.Drawing.Size(89, 18);
            this.lblBonificacion.TabIndex = 28;
            this.lblBonificacion.Text = "Bonificación";
            // 
            // txtBonificacion
            // 
            this.txtBonificacion.Location = new System.Drawing.Point(465, 188);
            this.txtBonificacion.Margin = new System.Windows.Forms.Padding(4);
            this.txtBonificacion.Name = "txtBonificacion";
            this.txtBonificacion.Size = new System.Drawing.Size(100, 24);
            this.txtBonificacion.TabIndex = 27;
            this.txtBonificacion.TabStop = false;
            this.txtBonificacion.Text = "0";
            // 
            // checkCtaCte
            // 
            this.checkCtaCte.AutoSize = true;
            this.checkCtaCte.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.checkCtaCte.Location = new System.Drawing.Point(389, 165);
            this.checkCtaCte.Margin = new System.Windows.Forms.Padding(4);
            this.checkCtaCte.Name = "checkCtaCte";
            this.checkCtaCte.Size = new System.Drawing.Size(88, 22);
            this.checkCtaCte.TabIndex = 26;
            this.checkCtaCte.TabStop = false;
            this.checkCtaCte.Text = "Cta. Cte.";
            this.checkCtaCte.UseVisualStyleBackColor = true;
            this.checkCtaCte.Visible = false;
            // 
            // txtOtrosDatos
            // 
            this.txtOtrosDatos.Location = new System.Drawing.Point(148, 214);
            this.txtOtrosDatos.Margin = new System.Windows.Forms.Padding(4);
            this.txtOtrosDatos.Multiline = true;
            this.txtOtrosDatos.Name = "txtOtrosDatos";
            this.txtOtrosDatos.Size = new System.Drawing.Size(417, 99);
            this.txtOtrosDatos.TabIndex = 7;
            // 
            // lblOtrosDatos
            // 
            this.lblOtrosDatos.AutoSize = true;
            this.lblOtrosDatos.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblOtrosDatos.ForeColor = System.Drawing.Color.Cornsilk;
            this.lblOtrosDatos.Location = new System.Drawing.Point(47, 214);
            this.lblOtrosDatos.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblOtrosDatos.Name = "lblOtrosDatos";
            this.lblOtrosDatos.Size = new System.Drawing.Size(90, 18);
            this.lblOtrosDatos.TabIndex = 7;
            this.lblOtrosDatos.Text = "Otros Datos";
            // 
            // lblRazonSocial
            // 
            this.lblRazonSocial.AutoSize = true;
            this.lblRazonSocial.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblRazonSocial.ForeColor = System.Drawing.Color.Cornsilk;
            this.lblRazonSocial.Location = new System.Drawing.Point(33, 58);
            this.lblRazonSocial.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblRazonSocial.Name = "lblRazonSocial";
            this.lblRazonSocial.Size = new System.Drawing.Size(97, 18);
            this.lblRazonSocial.TabIndex = 6;
            this.lblRazonSocial.Text = "Razon Social";
            // 
            // txtRazonSocial
            // 
            this.txtRazonSocial.Location = new System.Drawing.Point(148, 54);
            this.txtRazonSocial.Margin = new System.Windows.Forms.Padding(4);
            this.txtRazonSocial.Name = "txtRazonSocial";
            this.txtRazonSocial.Size = new System.Drawing.Size(333, 24);
            this.txtRazonSocial.TabIndex = 1;
            // 
            // btnGuardar
            // 
            this.btnGuardar.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnGuardar.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnGuardar.Location = new System.Drawing.Point(376, 356);
            this.btnGuardar.Margin = new System.Windows.Forms.Padding(4);
            this.btnGuardar.Name = "btnGuardar";
            this.btnGuardar.Size = new System.Drawing.Size(119, 33);
            this.btnGuardar.TabIndex = 8;
            this.btnGuardar.Text = "&Guardar";
            this.btnGuardar.UseVisualStyleBackColor = true;
            this.btnGuardar.Click += new System.EventHandler(this.btnGuardar_Click);
            // 
            // btnCancelar
            // 
            this.btnCancelar.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancelar.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnCancelar.Location = new System.Drawing.Point(503, 356);
            this.btnCancelar.Margin = new System.Windows.Forms.Padding(4);
            this.btnCancelar.Name = "btnCancelar";
            this.btnCancelar.Size = new System.Drawing.Size(112, 33);
            this.btnCancelar.TabIndex = 9;
            this.btnCancelar.Text = "&Cancelar";
            this.btnCancelar.UseVisualStyleBackColor = true;
            this.btnCancelar.Click += new System.EventHandler(this.btnCancelar_Click);
            // 
            // formNuevaPersona
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(239)))), ((int)(((byte)(250)))));
            this.ClientSize = new System.Drawing.Size(631, 393);
            this.Controls.Add(this.pnlBuscar);
            this.Controls.Add(this.btnGuardar);
            this.Controls.Add(this.btnCancelar);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MaximizeBox = false;
            this.Name = "formNuevaPersona";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Nueva Persona";
            this.Load += new System.EventHandler(this.formNuevaPersona_Load);
            this.pnlBuscar.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        protected System.Windows.Forms.Panel pnlBuscar;
        protected System.Windows.Forms.GroupBox groupBox1;
        protected System.Windows.Forms.TextBox txtOtrosDatos;
        protected System.Windows.Forms.Label lblOtrosDatos;
        protected System.Windows.Forms.Label lblRazonSocial;
        protected System.Windows.Forms.TextBox txtRazonSocial;
        protected System.Windows.Forms.Button btnGuardar;
        protected System.Windows.Forms.Button btnCancelar;
        private System.Windows.Forms.CheckBox checkCtaCte;
        protected System.Windows.Forms.Label lblBonificacion;
        protected System.Windows.Forms.TextBox txtBonificacion;
        protected System.Windows.Forms.Label label4;
        protected System.Windows.Forms.TextBox txtCiudad;
        protected System.Windows.Forms.Label label3;
        protected System.Windows.Forms.TextBox txtDomicilio;
        protected System.Windows.Forms.Label label2;
        protected System.Windows.Forms.Label label1;
        protected System.Windows.Forms.TextBox txtTelefono;
        private System.Windows.Forms.MaskedTextBox txtCuit;
        protected System.Windows.Forms.Label label5;
        private System.Windows.Forms.ComboBox comboIva;
        protected System.Windows.Forms.Label lblNombreIdentif;
        protected System.Windows.Forms.TextBox txtIdentificacion;
        private System.Windows.Forms.Button btnCopiarRS;
        private System.Windows.Forms.Button btnBuscarAfip;
    }
}