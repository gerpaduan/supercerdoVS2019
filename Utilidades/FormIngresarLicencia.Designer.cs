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
            this.groupLicencias.SuspendLayout();
            this.SuspendLayout();
            // 
            // txtClave
            // 
            this.txtClave.Location = new System.Drawing.Point(67, 31);
            this.txtClave.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.txtClave.Name = "txtClave";
            this.txtClave.Size = new System.Drawing.Size(184, 22);
            this.txtClave.TabIndex = 4;
            this.txtClave.UseSystemPasswordChar = true;
            this.txtClave.TextChanged += new System.EventHandler(this.txtClave_TextChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(11, 34);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(45, 16);
            this.label1.TabIndex = 6;
            this.label1.Text = "Admin";
            // 
            // btnSalir
            // 
            this.btnSalir.Location = new System.Drawing.Point(260, 199);
            this.btnSalir.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnSalir.Name = "btnSalir";
            this.btnSalir.Size = new System.Drawing.Size(100, 28);
            this.btnSalir.TabIndex = 7;
            this.btnSalir.Text = "Salir";
            this.btnSalir.UseVisualStyleBackColor = true;
            this.btnSalir.Click += new System.EventHandler(this.btnSalir_Click);
            // 
            // btnAgregar
            // 
            this.btnAgregar.Location = new System.Drawing.Point(152, 199);
            this.btnAgregar.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnAgregar.Name = "btnAgregar";
            this.btnAgregar.Size = new System.Drawing.Size(100, 28);
            this.btnAgregar.TabIndex = 5;
            this.btnAgregar.Text = "Agregar";
            this.btnAgregar.UseVisualStyleBackColor = true;
            this.btnAgregar.Visible = false;
            this.btnAgregar.Click += new System.EventHandler(this.btnAgregar_Click);
            // 
            // groupLicencias
            // 
            this.groupLicencias.Controls.Add(this.lblErrorLicencia);
            this.groupLicencias.Controls.Add(this.serialHD);
            this.groupLicencias.Controls.Add(this.serialCPU);
            this.groupLicencias.Controls.Add(this.label2);
            this.groupLicencias.Controls.Add(this.label3);
            this.groupLicencias.Location = new System.Drawing.Point(15, 78);
            this.groupLicencias.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.groupLicencias.Name = "groupLicencias";
            this.groupLicencias.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.groupLicencias.Size = new System.Drawing.Size(345, 114);
            this.groupLicencias.TabIndex = 8;
            this.groupLicencias.TabStop = false;
            this.groupLicencias.Text = "Licencias";
            // 
            // lblErrorLicencia
            // 
            this.lblErrorLicencia.AutoSize = true;
            this.lblErrorLicencia.ForeColor = System.Drawing.Color.Maroon;
            this.lblErrorLicencia.Location = new System.Drawing.Point(29, 27);
            this.lblErrorLicencia.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblErrorLicencia.Name = "lblErrorLicencia";
            this.lblErrorLicencia.Size = new System.Drawing.Size(288, 80);
            this.lblErrorLicencia.TabIndex = 13;
            this.lblErrorLicencia.Text = "Esta copia no cuenta con la licencia habilitada. \r\n(ymd)\r\nContactar al proveedor." +
    "\r\n\r\nEmail: germanpaduan@gmail.com\r\n";
            // 
            // serialHD
            // 
            this.serialHD.AutoSize = true;
            this.serialHD.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.serialHD.Location = new System.Drawing.Point(127, 66);
            this.serialHD.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.serialHD.Name = "serialHD";
            this.serialHD.Size = new System.Drawing.Size(56, 17);
            this.serialHD.TabIndex = 12;
            this.serialHD.Text = "********";
            // 
            // serialCPU
            // 
            this.serialCPU.AutoSize = true;
            this.serialCPU.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.serialCPU.Location = new System.Drawing.Point(127, 37);
            this.serialCPU.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.serialCPU.Name = "serialCPU";
            this.serialCPU.Size = new System.Drawing.Size(56, 17);
            this.serialCPU.TabIndex = 11;
            this.serialCPU.Text = "********";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(45, 37);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(52, 16);
            this.label2.TabIndex = 9;
            this.label2.Text = "Serial 1";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(45, 66);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(52, 16);
            this.label3.TabIndex = 10;
            this.label3.Text = "Serial 2";
            // 
            // btnValidar
            // 
            this.btnValidar.Location = new System.Drawing.Point(260, 28);
            this.btnValidar.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnValidar.Name = "btnValidar";
            this.btnValidar.Size = new System.Drawing.Size(101, 28);
            this.btnValidar.TabIndex = 9;
            this.btnValidar.Text = "Ingresar";
            this.btnValidar.UseVisualStyleBackColor = true;
            this.btnValidar.Click += new System.EventHandler(this.btnValidar_Click);
            // 
            // FormIngresarLicencia
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(379, 242);
            this.Controls.Add(this.btnValidar);
            this.Controls.Add(this.btnSalir);
            this.Controls.Add(this.btnAgregar);
            this.Controls.Add(this.groupLicencias);
            this.Controls.Add(this.txtClave);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
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
    }
}