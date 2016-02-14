namespace Presentacion.Pruebas
{
    partial class formTicketPrueba
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
            this.btnImprimir = new System.Windows.Forms.Button();
            this.txtIngreseTexto = new System.Windows.Forms.TextBox();
            this.lblTitulo = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // btnImprimir
            // 
            this.btnImprimir.Location = new System.Drawing.Point(67, 93);
            this.btnImprimir.Name = "btnImprimir";
            this.btnImprimir.Size = new System.Drawing.Size(165, 39);
            this.btnImprimir.TabIndex = 0;
            this.btnImprimir.Text = "Imprimir prueba";
            this.btnImprimir.UseVisualStyleBackColor = true;
            this.btnImprimir.Click += new System.EventHandler(this.btnImprimir_Click);
            // 
            // txtIngreseTexto
            // 
            this.txtIngreseTexto.Location = new System.Drawing.Point(24, 56);
            this.txtIngreseTexto.Name = "txtIngreseTexto";
            this.txtIngreseTexto.Size = new System.Drawing.Size(250, 20);
            this.txtIngreseTexto.TabIndex = 2;
            this.txtIngreseTexto.Text = "Ingrese Texto";
            // 
            // lblTitulo
            // 
            this.lblTitulo.AutoSize = true;
            this.lblTitulo.Location = new System.Drawing.Point(21, 17);
            this.lblTitulo.Name = "lblTitulo";
            this.lblTitulo.Size = new System.Drawing.Size(102, 13);
            this.lblTitulo.TabIndex = 3;
            this.lblTitulo.Text = "Esto es una prueba!";
            // 
            // formTicketPrueba
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(293, 144);
            this.Controls.Add(this.lblTitulo);
            this.Controls.Add(this.txtIngreseTexto);
            this.Controls.Add(this.btnImprimir);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.MinimizeBox = false;
            this.Name = "formTicketPrueba";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Probando impresion ticket..";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnImprimir;
        private System.Windows.Forms.TextBox txtIngreseTexto;
        private System.Windows.Forms.Label lblTitulo;
    }
}