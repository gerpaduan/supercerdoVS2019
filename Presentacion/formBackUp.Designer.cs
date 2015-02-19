namespace Presentacion
{
    partial class formBackUp
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
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.btnGenerarBackUp = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.label8 = new System.Windows.Forms.Label();
            this.txtDestino = new System.Windows.Forms.TextBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.btnRestaurar = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.txtOrigen = new System.Windows.Forms.TextBox();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.button3 = new System.Windows.Forms.Button();
            this.panel1 = new System.Windows.Forms.Panel();
            this.label2 = new System.Windows.Forms.Label();
            this.txtBDAuxiliar = new System.Windows.Forms.TextBox();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.btnGenerarBackUp);
            this.groupBox1.Controls.Add(this.button1);
            this.groupBox1.Controls.Add(this.label8);
            this.groupBox1.Controls.Add(this.txtDestino);
            this.groupBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.groupBox1.ForeColor = System.Drawing.Color.Cornsilk;
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(407, 97);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Back Up Base de Datos";
            // 
            // btnGenerarBackUp
            // 
            this.btnGenerarBackUp.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
            this.btnGenerarBackUp.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.btnGenerarBackUp.Location = new System.Drawing.Point(245, 61);
            this.btnGenerarBackUp.Name = "btnGenerarBackUp";
            this.btnGenerarBackUp.Size = new System.Drawing.Size(98, 25);
            this.btnGenerarBackUp.TabIndex = 27;
            this.btnGenerarBackUp.Text = "Generar Back Up";
            this.btnGenerarBackUp.UseVisualStyleBackColor = true;
            this.btnGenerarBackUp.Click += new System.EventHandler(this.btnGenerarBackUp_Click);
            // 
            // button1
            // 
            this.button1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.button1.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.button1.Location = new System.Drawing.Point(348, 32);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(43, 25);
            this.button1.TabIndex = 26;
            this.button1.Text = "...";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label8.ForeColor = System.Drawing.Color.Cornsilk;
            this.label8.Location = new System.Drawing.Point(57, 37);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(49, 15);
            this.label8.TabIndex = 25;
            this.label8.Text = "Destino";
            // 
            // txtDestino
            // 
            this.txtDestino.Location = new System.Drawing.Point(112, 34);
            this.txtDestino.Name = "txtDestino";
            this.txtDestino.ReadOnly = true;
            this.txtDestino.Size = new System.Drawing.Size(231, 21);
            this.txtDestino.TabIndex = 0;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.label2);
            this.groupBox2.Controls.Add(this.txtBDAuxiliar);
            this.groupBox2.Controls.Add(this.btnRestaurar);
            this.groupBox2.Controls.Add(this.button2);
            this.groupBox2.Controls.Add(this.label1);
            this.groupBox2.Controls.Add(this.txtOrigen);
            this.groupBox2.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.groupBox2.ForeColor = System.Drawing.Color.Cornsilk;
            this.groupBox2.Location = new System.Drawing.Point(12, 113);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(407, 106);
            this.groupBox2.TabIndex = 1;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Restaurar Base de Datos";
            // 
            // btnRestaurar
            // 
            this.btnRestaurar.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
            this.btnRestaurar.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.btnRestaurar.Location = new System.Drawing.Point(245, 73);
            this.btnRestaurar.Name = "btnRestaurar";
            this.btnRestaurar.Size = new System.Drawing.Size(98, 26);
            this.btnRestaurar.TabIndex = 28;
            this.btnRestaurar.Text = "Restaurar";
            this.btnRestaurar.UseVisualStyleBackColor = true;
            this.btnRestaurar.Click += new System.EventHandler(this.btnRestaurar_Click);
            // 
            // button2
            // 
            this.button2.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.button2.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.button2.Location = new System.Drawing.Point(350, 46);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(43, 25);
            this.button2.TabIndex = 27;
            this.button2.Text = "...";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ForeColor = System.Drawing.Color.Cornsilk;
            this.label1.Location = new System.Drawing.Point(62, 51);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(44, 15);
            this.label1.TabIndex = 25;
            this.label1.Text = "Origen";
            // 
            // txtOrigen
            // 
            this.txtOrigen.Location = new System.Drawing.Point(112, 48);
            this.txtOrigen.Name = "txtOrigen";
            this.txtOrigen.ReadOnly = true;
            this.txtOrigen.Size = new System.Drawing.Size(231, 21);
            this.txtOrigen.TabIndex = 1;
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            this.openFileDialog1.FileOk += new System.ComponentModel.CancelEventHandler(this.openFileDialog1_FileOk);
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(341, 10);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(77, 28);
            this.button3.TabIndex = 28;
            this.button3.Text = "Salir";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.Bisque;
            this.panel1.Controls.Add(this.button3);
            this.panel1.Location = new System.Drawing.Point(-1, 229);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(432, 45);
            this.panel1.TabIndex = 2;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.ForeColor = System.Drawing.Color.Cornsilk;
            this.label2.Location = new System.Drawing.Point(39, 27);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(67, 15);
            this.label2.TabIndex = 30;
            this.label2.Text = "BD Auxiliar";
            // 
            // txtBDAuxiliar
            // 
            this.txtBDAuxiliar.Location = new System.Drawing.Point(112, 24);
            this.txtBDAuxiliar.Name = "txtBDAuxiliar";
            this.txtBDAuxiliar.Size = new System.Drawing.Size(121, 21);
            this.txtBDAuxiliar.TabIndex = 29;
            this.txtBDAuxiliar.Text = "base";
            // 
            // formBackUp
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.SteelBlue;
            this.ClientSize = new System.Drawing.Size(431, 273);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "formBackUp";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Tareas Base de Datos";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.TextBox txtDestino;
        private System.Windows.Forms.TextBox txtOrigen;
        protected System.Windows.Forms.Label label8;
        protected System.Windows.Forms.Label label1;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button btnGenerarBackUp;
        private System.Windows.Forms.Button btnRestaurar;
        protected System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtBDAuxiliar;

    }
}