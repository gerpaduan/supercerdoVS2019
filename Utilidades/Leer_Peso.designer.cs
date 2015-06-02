namespace Utilidades
{
    partial class Leer_Peso
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
            this.BasculaCom = new System.IO.Ports.SerialPort(this.components);
            this.Recibidos = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // BasculaCom
            // 
            this.BasculaCom.DataReceived += new System.IO.Ports.SerialDataReceivedEventHandler(this.BasculaCom_DataReceived);
            // 
            // Recibidos
            // 
            this.Recibidos.AutoSize = true;
            this.Recibidos.Location = new System.Drawing.Point(54, 55);
            this.Recibidos.Name = "Recibidos";
            this.Recibidos.Size = new System.Drawing.Size(35, 13);
            this.Recibidos.TabIndex = 0;
            this.Recibidos.Text = "label1";
            // 
            // Leer_Peso
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 262);
            this.Controls.Add(this.Recibidos);
            this.Name = "Leer_Peso";
            this.Text = "Leer_Peso";
            this.Load += new System.EventHandler(this.Leer_Peso_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.IO.Ports.SerialPort BasculaCom;
        private System.Windows.Forms.Label Recibidos;
    }
}