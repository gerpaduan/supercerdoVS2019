namespace Presentacion.Balanza
{
    partial class formBalanza
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
            this.label2 = new System.Windows.Forms.Label();
            this.txtVelocidadTimer = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.txtPesoBalanza = new System.Windows.Forms.TextBox();
            this.btnTimer = new System.Windows.Forms.Button();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.btnVerBalanza = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(35, 28);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(83, 13);
            this.label2.TabIndex = 10;
            this.label2.Text = "Velocidad Timer";
            // 
            // txtVelocidadTimer
            // 
            this.txtVelocidadTimer.Location = new System.Drawing.Point(124, 25);
            this.txtVelocidadTimer.Name = "txtVelocidadTimer";
            this.txtVelocidadTimer.Size = new System.Drawing.Size(75, 20);
            this.txtVelocidadTimer.TabIndex = 9;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(47, 62);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(71, 13);
            this.label1.TabIndex = 8;
            this.label1.Text = "Peso balanza";
            // 
            // txtPesoBalanza
            // 
            this.txtPesoBalanza.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtPesoBalanza.Location = new System.Drawing.Point(124, 51);
            this.txtPesoBalanza.Name = "txtPesoBalanza";
            this.txtPesoBalanza.Size = new System.Drawing.Size(156, 29);
            this.txtPesoBalanza.TabIndex = 7;
            // 
            // btnTimer
            // 
            this.btnTimer.Location = new System.Drawing.Point(205, 22);
            this.btnTimer.Name = "btnTimer";
            this.btnTimer.Size = new System.Drawing.Size(75, 24);
            this.btnTimer.TabIndex = 6;
            this.btnTimer.Text = "Timer";
            this.btnTimer.UseVisualStyleBackColor = true;
            this.btnTimer.Click += new System.EventHandler(this.btnTimer_Click);
            // 
            // timer1
            // 
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // btnVerBalanza
            // 
            this.btnVerBalanza.Location = new System.Drawing.Point(285, 106);
            this.btnVerBalanza.Name = "btnVerBalanza";
            this.btnVerBalanza.Size = new System.Drawing.Size(75, 24);
            this.btnVerBalanza.TabIndex = 11;
            this.btnVerBalanza.Text = "Ver balanza";
            this.btnVerBalanza.UseVisualStyleBackColor = true;
            this.btnVerBalanza.Click += new System.EventHandler(this.btnVerBalanza_Click);
            // 
            // formBalanza
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(372, 142);
            this.Controls.Add(this.btnVerBalanza);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.txtVelocidadTimer);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.txtPesoBalanza);
            this.Controls.Add(this.btnTimer);
            this.Name = "formBalanza";
            this.Text = "Balanza";
            this.Load += new System.EventHandler(this.formBalanza_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtVelocidadTimer;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtPesoBalanza;
        private System.Windows.Forms.Button btnTimer;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.Button btnVerBalanza;
    }
}