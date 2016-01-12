namespace Utilidades
{
    partial class FormLeer_Peso
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
            this.Recibidos = new System.Windows.Forms.Label();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.btnTimer = new System.Windows.Forms.Button();
            this.txtPesoBalanza = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.txtVelocidadTimer = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // Recibidos
            // 
            this.Recibidos.AutoSize = true;
            this.Recibidos.Location = new System.Drawing.Point(110, 80);
            this.Recibidos.Name = "Recibidos";
            this.Recibidos.Size = new System.Drawing.Size(54, 13);
            this.Recibidos.TabIndex = 0;
            this.Recibidos.Text = "Recibidos";
            // 
            // timer1
            // 
            this.timer1.Interval = 700;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // btnTimer
            // 
            this.btnTimer.Location = new System.Drawing.Point(194, 19);
            this.btnTimer.Name = "btnTimer";
            this.btnTimer.Size = new System.Drawing.Size(75, 24);
            this.btnTimer.TabIndex = 1;
            this.btnTimer.Text = "Stop";
            this.btnTimer.UseVisualStyleBackColor = true;
            this.btnTimer.Click += new System.EventHandler(this.button1_Click);
            // 
            // txtPesoBalanza
            // 
            this.txtPesoBalanza.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtPesoBalanza.Location = new System.Drawing.Point(113, 48);
            this.txtPesoBalanza.Name = "txtPesoBalanza";
            this.txtPesoBalanza.Size = new System.Drawing.Size(156, 29);
            this.txtPesoBalanza.TabIndex = 2;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(36, 59);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(71, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "Peso balanza";
            // 
            // txtVelocidadTimer
            // 
            this.txtVelocidadTimer.Location = new System.Drawing.Point(113, 22);
            this.txtVelocidadTimer.Name = "txtVelocidadTimer";
            this.txtVelocidadTimer.Size = new System.Drawing.Size(75, 20);
            this.txtVelocidadTimer.TabIndex = 4;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(24, 25);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(83, 13);
            this.label2.TabIndex = 5;
            this.label2.Text = "Velocidad Timer";
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(27, 116);
            this.textBox1.Multiline = true;
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(483, 189);
            this.textBox1.TabIndex = 6;
            // 
            // FormLeer_Peso
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(522, 317);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.txtVelocidadTimer);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.txtPesoBalanza);
            this.Controls.Add(this.btnTimer);
            this.Controls.Add(this.Recibidos);
            this.Name = "FormLeer_Peso";
            this.Text = "Balanza (FormLeer_peso)";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.FormLeer_Peso_FormClosed);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label Recibidos;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.Button btnTimer;
        private System.Windows.Forms.TextBox txtPesoBalanza;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtVelocidadTimer;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBox1;
    }
}