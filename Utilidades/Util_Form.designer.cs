namespace Utilidades
{
    partial class Util_Form
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
            this.pesoBalanza = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // pesoBalanza
            // 
            this.pesoBalanza.AutoSize = true;
            this.pesoBalanza.Location = new System.Drawing.Point(84, 51);
            this.pesoBalanza.Name = "pesoBalanza";
            this.pesoBalanza.Size = new System.Drawing.Size(0, 13);
            this.pesoBalanza.TabIndex = 0;
            // 
            // Util_Form
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 262);
            this.Controls.Add(this.pesoBalanza);
            this.Name = "Util_Form";
            this.Text = "Util_Form";
            this.Load += new System.EventHandler(this.Util_Form_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label pesoBalanza;
    }
}