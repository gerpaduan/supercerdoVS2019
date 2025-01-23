namespace Utilidades
{
    partial class FormIngresarLicencia
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
            this.txtClave = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.btnSalir = new System.Windows.Forms.Button();
            this.btnAgregar = new System.Windows.Forms.Button();
            this.groupLicencias = new System.Windows.Forms.GroupBox();
            this.lblErrorLicencia = new System.Windows.Forms.Label();
            this.serialHD = new System.Windows.Forms.Label();
            this.serialCPU = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.btnValidar = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.txtIdentificacion = new System.Windows.Forms.TextBox();
            this.groupLicencias.SuspendLayout();
            this.SuspendLayout();
            // 
            // txtClave
            // 
            this.txtClave.Location = new System.Drawing.Point(50, 25);
            this.txtClave.Name = "txtClave";
            this.txtClave.Size = new System.Drawing.Size(139, 20);
            this.txtClave.TabIndex = 4;
            this.txtClave.UseSystemPasswordChar = true;
            this.txtClave.TextChanged += new System.EventHandler(this.txtClave_TextChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(8, 28);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(36, 13);
            this.label1.TabIndex = 6;
            this.label1.Text = "Admin";
            // 
            // btnSalir
            // 
            this.btnSalir.Location = new System.Drawing.Point(195, 174);
            this.btnSalir.Name = "btnSalir";
            this.btnSalir.Size = new System.Drawing.Size(75, 23);
            this.btnSalir.TabIndex = 7;
            this.btnSalir.Text = "Salir";
            this.btnSalir.UseVisualStyleBackColor = true;
            this.btnSalir.Click += new System.EventHandler(this.btnSalir_Click);
            // 
            // btnAgregar
            // 
            this.btnAgregar.Location = new System.Drawing.Point(114, 174);
            this.btnAgregar.Name = "btnAgregar";
            this.btnAgregar.Size = new System.Drawing.Size(75, 23);
            this.btnAgregar.TabIndex = 5;
            this.btnAgregar.Text = "Agregar";
            this.btnAgregar.UseVisualStyleBackColor = true;
            this.btnAgregar.Visible = false;
            this.btnAgregar.Click += new System.EventHandler(this.btnAgregar_Click);
            // 
            // groupLicencias
            // 
            this.groupLicencias.Controls.Add(this.lblErrorLicencia);
            this.groupLicencias.Controls.Add(this.txtIdentificacion);
            this.groupLicencias.Controls.Add(this.label4);
            this.groupLicencias.Controls.Add(this.serialHD);
            this.groupLicencias.Controls.Add(this.serialCPU);
            this.groupLicencias.Controls.Add(this.label2);
            this.groupLicencias.Controls.Add(this.label3);
            this.groupLicencias.Location = new System.Drawing.Point(11, 52);
            this.groupLicencias.Name = "groupLicencias";
            this.groupLicencias.Size = new System.Drawing.Size(259, 116);
            this.groupLicencias.TabIndex = 8;
            this.groupLicencias.TabStop = false;
            this.groupLicencias.Text = "Licencias";
            // 
            // lblErrorLicencia
            // 
            this.lblErrorLicencia.AutoSize = true;
            this.lblErrorLicencia.ForeColor = System.Drawing.Color.Maroon;
            this.lblErrorLicencia.Location = new System.Drawing.Point(8, 16);
            this.lblErrorLicencia.Name = "lblErrorLicencia";
            this.lblErrorLicencia.Size = new System.Drawing.Size(233, 91);
            this.lblErrorLicencia.TabIndex = 13;
            this.lblErrorLicencia.Text = "\r\nEsta copia no cuenta con la licencia habilitada. \r\n(ymd)\r\nContactar al proveedo" +
    "r.\r\n\r\n\r\nEmail: germanpaduan@gmail.com\r\n";
            // 
            // serialHD
            // 
            this.serialHD.AutoSize = true;
            this.serialHD.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.serialHD.Location = new System.Drawing.Point(95, 73);
            this.serialHD.Name = "serialHD";
            this.serialHD.Size = new System.Drawing.Size(47, 13);
            this.serialHD.TabIndex = 12;
            this.serialHD.Text = "********";
            // 
            // serialCPU
            // 
            this.serialCPU.AutoSize = true;
            this.serialCPU.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.serialCPU.Location = new System.Drawing.Point(95, 49);
            this.serialCPU.Name = "serialCPU";
            this.serialCPU.Size = new System.Drawing.Size(47, 13);
            this.serialCPU.TabIndex = 11;
            this.serialCPU.Text = "********";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(34, 49);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(42, 13);
            this.label2.TabIndex = 9;
            this.label2.Text = "Serial 1";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(34, 73);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(42, 13);
            this.label3.TabIndex = 10;
            this.label3.Text = "Serial 2";
            // 
            // btnValidar
            // 
            this.btnValidar.Location = new System.Drawing.Point(195, 23);
            this.btnValidar.Name = "btnValidar";
            this.btnValidar.Size = new System.Drawing.Size(76, 23);
            this.btnValidar.TabIndex = 9;
            this.btnValidar.Text = "Ingresar";
            this.btnValidar.UseVisualStyleBackColor = true;
            this.btnValidar.Click += new System.EventHandler(this.btnValidar_Click);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(6, 22);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(70, 13);
            this.label4.TabIndex = 15;
            this.label4.Text = "Identificación";
            // 
            // txtIdentificacion
            // 
            this.txtIdentificacion.Location = new System.Drawing.Point(98, 19);
            this.txtIdentificacion.Name = "txtIdentificacion";
            this.txtIdentificacion.Size = new System.Drawing.Size(139, 20);
            this.txtIdentificacion.TabIndex = 14;
            // 
            // FormIngresarLicencia
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 204);
            this.Controls.Add(this.btnValidar);
            this.Controls.Add(this.btnSalir);
            this.Controls.Add(this.btnAgregar);
            this.Controls.Add(this.groupLicencias);
            this.Controls.Add(this.txtClave);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "FormIngresarLicencia";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Ingresar Licencia";
            this.Load += new System.EventHandler(this.FormIngresarLicencia_Load);
            this.groupLicencias.ResumeLayout(false);
            this.groupLicencias.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txtClave;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnSalir;
        private System.Windows.Forms.Button btnAgregar;
        private System.Windows.Forms.GroupBox groupLicencias;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label serialCPU;
        private System.Windows.Forms.Label serialHD;
        private System.Windows.Forms.Button btnValidar;
        private System.Windows.Forms.Label lblErrorLicencia;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox txtIdentificacion;
    }
}