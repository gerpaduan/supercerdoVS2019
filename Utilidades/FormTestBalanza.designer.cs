namespace Utilidades
{
    partial class FormTestBalanza
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
            this.txtPesoBalanza = new System.Windows.Forms.TextBox();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.btnStart = new System.Windows.Forms.Button();
            this.btnStop = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.txtPesoFormat = new System.Windows.Forms.TextBox();
            this.lbl_error5 = new System.Windows.Forms.Label();
            this.pesoBalanzaLabel = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.txtCantBuffer = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.txtBuffer1 = new System.Windows.Forms.TextBox();
            this.txtBuffer2 = new System.Windows.Forms.TextBox();
            this.comboBalanzas = new System.Windows.Forms.ComboBox();
            this.label6 = new System.Windows.Forms.Label();
            this.txtDetalleConfiBalanza = new System.Windows.Forms.TextBox();
            this.comboPuertos = new System.Windows.Forms.ComboBox();
            this.txtErrores = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // BalanzaCom
            // 
            this.BalanzaCom.PortName = "COM8";
            // 
            // txtPesoBalanza
            // 
            this.txtPesoBalanza.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtPesoBalanza.Location = new System.Drawing.Point(166, 157);
            this.txtPesoBalanza.Name = "txtPesoBalanza";
            this.txtPesoBalanza.ReadOnly = true;
            this.txtPesoBalanza.Size = new System.Drawing.Size(156, 29);
            this.txtPesoBalanza.TabIndex = 7;
            // 
            // timer1
            // 
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(40, 164);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(120, 18);
            this.label1.TabIndex = 9;
            this.label1.Text = "Peso Obtenido";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(102, 111);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(58, 18);
            this.label2.TabIndex = 11;
            this.label2.Text = "Puerto";
            // 
            // btnStart
            // 
            this.btnStart.Location = new System.Drawing.Point(334, 106);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(75, 30);
            this.btnStart.TabIndex = 12;
            this.btnStart.Text = "Start";
            this.btnStart.UseVisualStyleBackColor = true;
            this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
            // 
            // btnStop
            // 
            this.btnStop.Location = new System.Drawing.Point(422, 106);
            this.btnStop.Name = "btnStop";
            this.btnStop.Size = new System.Drawing.Size(75, 30);
            this.btnStop.TabIndex = 13;
            this.btnStop.Text = "Stop";
            this.btnStop.UseVisualStyleBackColor = true;
            this.btnStop.Click += new System.EventHandler(this.btnStop_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(17, 217);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(143, 18);
            this.label3.TabIndex = 15;
            this.label3.Text = "Peso Formateado";
            // 
            // txtPesoFormat
            // 
            this.txtPesoFormat.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtPesoFormat.Location = new System.Drawing.Point(166, 210);
            this.txtPesoFormat.Name = "txtPesoFormat";
            this.txtPesoFormat.ReadOnly = true;
            this.txtPesoFormat.Size = new System.Drawing.Size(156, 29);
            this.txtPesoFormat.TabIndex = 14;
            // 
            // lbl_error5
            // 
            this.lbl_error5.AutoSize = true;
            this.lbl_error5.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbl_error5.Location = new System.Drawing.Point(163, 260);
            this.lbl_error5.Name = "lbl_error5";
            this.lbl_error5.Size = new System.Drawing.Size(20, 18);
            this.lbl_error5.TabIndex = 16;
            this.lbl_error5.Text = "\"\"";
            // 
            // pesoBalanzaLabel
            // 
            this.pesoBalanzaLabel.AutoSize = true;
            this.pesoBalanzaLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.pesoBalanzaLabel.Location = new System.Drawing.Point(331, 164);
            this.pesoBalanzaLabel.Name = "pesoBalanzaLabel";
            this.pesoBalanzaLabel.Size = new System.Drawing.Size(237, 18);
            this.pesoBalanzaLabel.TabIndex = 8;
            this.pesoBalanzaLabel.Text = "Error balanza (Acá va el peso)";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Enabled = false;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(67, 68);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(93, 18);
            this.label4.TabIndex = 18;
            this.label4.Text = "Cant.Buffer";
            // 
            // txtCantBuffer
            // 
            this.txtCantBuffer.Enabled = false;
            this.txtCantBuffer.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtCantBuffer.Location = new System.Drawing.Point(166, 61);
            this.txtCantBuffer.Name = "txtCantBuffer";
            this.txtCantBuffer.Size = new System.Drawing.Size(58, 29);
            this.txtCantBuffer.TabIndex = 17;
            this.txtCantBuffer.Text = "1 ó 2";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Enabled = false;
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.Location = new System.Drawing.Point(230, 68);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(97, 18);
            this.label5.TabIndex = 20;
            this.label5.Text = "Valor Buffer";
            // 
            // txtBuffer1
            // 
            this.txtBuffer1.Enabled = false;
            this.txtBuffer1.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtBuffer1.Location = new System.Drawing.Point(334, 61);
            this.txtBuffer1.Name = "txtBuffer1";
            this.txtBuffer1.Size = new System.Drawing.Size(30, 29);
            this.txtBuffer1.TabIndex = 19;
            this.txtBuffer1.Text = "5";
            // 
            // txtBuffer2
            // 
            this.txtBuffer2.Enabled = false;
            this.txtBuffer2.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtBuffer2.Location = new System.Drawing.Point(379, 61);
            this.txtBuffer2.Name = "txtBuffer2";
            this.txtBuffer2.Size = new System.Drawing.Size(30, 29);
            this.txtBuffer2.TabIndex = 21;
            this.txtBuffer2.Text = "5";
            // 
            // comboBalanzas
            // 
            this.comboBalanzas.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Bold);
            this.comboBalanzas.FormattingEnabled = true;
            this.comboBalanzas.Items.AddRange(new object[] {
            "Systel",
            "Kretz"});
            this.comboBalanzas.Location = new System.Drawing.Point(166, 13);
            this.comboBalanzas.Name = "comboBalanzas";
            this.comboBalanzas.Size = new System.Drawing.Size(243, 32);
            this.comboBalanzas.TabIndex = 22;
            this.comboBalanzas.SelectedIndexChanged += new System.EventHandler(this.comboBalanzas_SelectedIndexChanged);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.Location = new System.Drawing.Point(92, 20);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(68, 18);
            this.label6.TabIndex = 23;
            this.label6.Text = "Balanza";
            // 
            // txtDetalleConfiBalanza
            // 
            this.txtDetalleConfiBalanza.Location = new System.Drawing.Point(574, 62);
            this.txtDetalleConfiBalanza.Multiline = true;
            this.txtDetalleConfiBalanza.Name = "txtDetalleConfiBalanza";
            this.txtDetalleConfiBalanza.Size = new System.Drawing.Size(167, 217);
            this.txtDetalleConfiBalanza.TabIndex = 24;
            // 
            // comboPuertos
            // 
            this.comboPuertos.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Bold);
            this.comboPuertos.FormattingEnabled = true;
            this.comboPuertos.Items.AddRange(new object[] {
            "Systel",
            "Kretz"});
            this.comboPuertos.Location = new System.Drawing.Point(166, 104);
            this.comboPuertos.Name = "comboPuertos";
            this.comboPuertos.Size = new System.Drawing.Size(156, 32);
            this.comboPuertos.TabIndex = 25;
            // 
            // txtErrores
            // 
            this.txtErrores.Location = new System.Drawing.Point(747, 62);
            this.txtErrores.Multiline = true;
            this.txtErrores.Name = "txtErrores";
            this.txtErrores.Size = new System.Drawing.Size(142, 217);
            this.txtErrores.TabIndex = 26;
            // 
            // FormTestBalanza
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(894, 291);
            this.Controls.Add(this.txtErrores);
            this.Controls.Add(this.comboPuertos);
            this.Controls.Add(this.txtDetalleConfiBalanza);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.comboBalanzas);
            this.Controls.Add(this.txtBuffer2);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.txtBuffer1);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.txtCantBuffer);
            this.Controls.Add(this.lbl_error5);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.txtPesoFormat);
            this.Controls.Add(this.btnStop);
            this.Controls.Add(this.btnStart);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.pesoBalanzaLabel);
            this.Controls.Add(this.txtPesoBalanza);
            this.Name = "FormTestBalanza";
            this.Text = "Balanza";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormTestBalanza_FormClosing);
            this.Load += new System.EventHandler(this.FormTestBalanza_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.IO.Ports.SerialPort BalanzaCom;
        private System.Windows.Forms.TextBox txtPesoBalanza;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btnStart;
        private System.Windows.Forms.Button btnStop;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtPesoFormat;
        private System.Windows.Forms.Label lbl_error5;
        private System.Windows.Forms.Label pesoBalanzaLabel;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox txtCantBuffer;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox txtBuffer1;
        private System.Windows.Forms.TextBox txtBuffer2;
        private System.Windows.Forms.ComboBox comboBalanzas;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox txtDetalleConfiBalanza;
        private System.Windows.Forms.ComboBox comboPuertos;
        private System.Windows.Forms.TextBox txtErrores;
    }
}