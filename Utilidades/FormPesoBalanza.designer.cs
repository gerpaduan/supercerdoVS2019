namespace Utilidades
{
    partial class FormPesoBalanza
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
            this.components = new System.ComponentModel.Container();
            this.BalanzaCom = new System.IO.Ports.SerialPort(this.components);
            this.pesoBalanzaLabel = new System.Windows.Forms.Label();
            this.txtPesoBalanza = new System.Windows.Forms.TextBox();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.SuspendLayout();
            // 
            // BalanzaCom
            // 
            this.BalanzaCom.PortName = "COM8";
            // 
            // pesoBalanzaLabel
            // 
            this.pesoBalanzaLabel.AutoSize = true;
            this.pesoBalanzaLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 13F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.pesoBalanzaLabel.Location = new System.Drawing.Point(221, 18);
            this.pesoBalanzaLabel.Name = "pesoBalanzaLabel";
            this.pesoBalanzaLabel.Size = new System.Drawing.Size(130, 22);
            this.pesoBalanzaLabel.TabIndex = 8;
            this.pesoBalanzaLabel.Text = "Peso balanza";
            // 
            // txtPesoBalanza
            // 
            this.txtPesoBalanza.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtPesoBalanza.Location = new System.Drawing.Point(12, 12);
            this.txtPesoBalanza.Name = "txtPesoBalanza";
            this.txtPesoBalanza.Size = new System.Drawing.Size(156, 29);
            this.txtPesoBalanza.TabIndex = 7;
            // 
            // timer1
            // 
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // FormPesoBalanza
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(413, 83);
            this.Controls.Add(this.pesoBalanzaLabel);
            this.Controls.Add(this.txtPesoBalanza);
            this.Name = "FormPesoBalanza";
            this.Text = "Balanza";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.IO.Ports.SerialPort BalanzaCom;
        private System.Windows.Forms.Label pesoBalanzaLabel;
        private System.Windows.Forms.TextBox txtPesoBalanza;
        private System.Windows.Forms.Timer timer1;
    }
}