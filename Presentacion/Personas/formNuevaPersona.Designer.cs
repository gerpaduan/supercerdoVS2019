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
            this.lblBonificacion = new System.Windows.Forms.Label();
            this.txtBonificacion = new System.Windows.Forms.TextBox();
            this.checkCtaCte = new System.Windows.Forms.CheckBox();
            this.comboTipoPersona = new System.Windows.Forms.ComboBox();
            this.lblTipo = new System.Windows.Forms.Label();
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
            this.pnlBuscar.BackColor = System.Drawing.Color.SteelBlue;
            this.pnlBuscar.Controls.Add(this.groupBox1);
            this.pnlBuscar.Location = new System.Drawing.Point(-1, 0);
            this.pnlBuscar.Name = "pnlBuscar";
            this.pnlBuscar.Size = new System.Drawing.Size(323, 240);
            this.pnlBuscar.TabIndex = 21;
            // 
            // groupBox1
            // 
            this.groupBox1.BackColor = System.Drawing.Color.SteelBlue;
            this.groupBox1.Controls.Add(this.lblBonificacion);
            this.groupBox1.Controls.Add(this.txtBonificacion);
            this.groupBox1.Controls.Add(this.checkCtaCte);
            this.groupBox1.Controls.Add(this.comboTipoPersona);
            this.groupBox1.Controls.Add(this.lblTipo);
            this.groupBox1.Controls.Add(this.txtOtrosDatos);
            this.groupBox1.Controls.Add(this.lblOtrosDatos);
            this.groupBox1.Controls.Add(this.lblRazonSocial);
            this.groupBox1.Controls.Add(this.txtRazonSocial);
            this.groupBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox1.ForeColor = System.Drawing.Color.Cornsilk;
            this.groupBox1.Location = new System.Drawing.Point(11, 6);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(300, 231);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Persona";
            // 
            // lblBonificacion
            // 
            this.lblBonificacion.AutoSize = true;
            this.lblBonificacion.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblBonificacion.ForeColor = System.Drawing.Color.Cornsilk;
            this.lblBonificacion.Location = new System.Drawing.Point(50, 112);
            this.lblBonificacion.Name = "lblBonificacion";
            this.lblBonificacion.Size = new System.Drawing.Size(74, 15);
            this.lblBonificacion.TabIndex = 28;
            this.lblBonificacion.Text = "Bonificación";
            // 
            // txtBonificacion
            // 
            this.txtBonificacion.Location = new System.Drawing.Point(127, 109);
            this.txtBonificacion.Name = "txtBonificacion";
            this.txtBonificacion.Size = new System.Drawing.Size(150, 21);
            this.txtBonificacion.TabIndex = 27;
            this.txtBonificacion.Text = "0";
            // 
            // checkCtaCte
            // 
            this.checkCtaCte.AutoSize = true;
            this.checkCtaCte.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.checkCtaCte.Location = new System.Drawing.Point(69, 84);
            this.checkCtaCte.Name = "checkCtaCte";
            this.checkCtaCte.Size = new System.Drawing.Size(71, 19);
            this.checkCtaCte.TabIndex = 26;
            this.checkCtaCte.Text = "Cta. Cte.";
            this.checkCtaCte.UseVisualStyleBackColor = true;
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
            this.comboTipoPersona.Size = new System.Drawing.Size(151, 23);
            this.comboTipoPersona.TabIndex = 0;
            // 
            // lblTipo
            // 
            this.lblTipo.AutoSize = true;
            this.lblTipo.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTipo.ForeColor = System.Drawing.Color.Cornsilk;
            this.lblTipo.Location = new System.Drawing.Point(90, 28);
            this.lblTipo.Name = "lblTipo";
            this.lblTipo.Size = new System.Drawing.Size(31, 15);
            this.lblTipo.TabIndex = 23;
            this.lblTipo.Text = "Tipo";
            // 
            // txtOtrosDatos
            // 
            this.txtOtrosDatos.Location = new System.Drawing.Point(127, 139);
            this.txtOtrosDatos.Multiline = true;
            this.txtOtrosDatos.Name = "txtOtrosDatos";
            this.txtOtrosDatos.Size = new System.Drawing.Size(150, 66);
            this.txtOtrosDatos.TabIndex = 2;
            // 
            // lblOtrosDatos
            // 
            this.lblOtrosDatos.AutoSize = true;
            this.lblOtrosDatos.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblOtrosDatos.ForeColor = System.Drawing.Color.Cornsilk;
            this.lblOtrosDatos.Location = new System.Drawing.Point(50, 139);
            this.lblOtrosDatos.Name = "lblOtrosDatos";
            this.lblOtrosDatos.Size = new System.Drawing.Size(71, 15);
            this.lblOtrosDatos.TabIndex = 7;
            this.lblOtrosDatos.Text = "Otros Datos";
            // 
            // lblRazonSocial
            // 
            this.lblRazonSocial.AutoSize = true;
            this.lblRazonSocial.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblRazonSocial.ForeColor = System.Drawing.Color.Cornsilk;
            this.lblRazonSocial.Location = new System.Drawing.Point(42, 60);
            this.lblRazonSocial.Name = "lblRazonSocial";
            this.lblRazonSocial.Size = new System.Drawing.Size(80, 15);
            this.lblRazonSocial.TabIndex = 6;
            this.lblRazonSocial.Text = "Razon Social";
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
            this.btnGuardar.Location = new System.Drawing.Point(130, 246);
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
            this.btnCancelar.Location = new System.Drawing.Point(225, 246);
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
            this.ClientSize = new System.Drawing.Size(321, 276);
            this.Controls.Add(this.pnlBuscar);
            this.Controls.Add(this.btnGuardar);
            this.Controls.Add(this.btnCancelar);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
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
        private System.Windows.Forms.ComboBox comboTipoPersona;
        protected System.Windows.Forms.Label lblTipo;
        protected System.Windows.Forms.TextBox txtOtrosDatos;
        protected System.Windows.Forms.Label lblOtrosDatos;
        protected System.Windows.Forms.Label lblRazonSocial;
        protected System.Windows.Forms.TextBox txtRazonSocial;
        protected System.Windows.Forms.Button btnGuardar;
        protected System.Windows.Forms.Button btnCancelar;
        private System.Windows.Forms.CheckBox checkCtaCte;
        protected System.Windows.Forms.Label lblBonificacion;
        protected System.Windows.Forms.TextBox txtBonificacion;

    }
}