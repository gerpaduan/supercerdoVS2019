namespace Presentacion.Caja
{
    partial class formFormaPago
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
            this.label1 = new System.Windows.Forms.Label();
            this.btnEfectivo = new System.Windows.Forms.Button();
            this.btnDebito = new System.Windows.Forms.Button();
            this.btnCredito = new System.Windows.Forms.Button();
            this.btnTransf = new System.Windows.Forms.Button();
            this.btnQr = new System.Windows.Forms.Button();
            this.btnBilleteraStaFe = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(80, 23);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(216, 20);
            this.label1.TabIndex = 0;
            this.label1.Text = "Seleccione la Forma de Pago";
            // 
            // btnEfectivo
            // 
            this.btnEfectivo.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnEfectivo.Location = new System.Drawing.Point(12, 73);
            this.btnEfectivo.Name = "btnEfectivo";
            this.btnEfectivo.Size = new System.Drawing.Size(114, 46);
            this.btnEfectivo.TabIndex = 1;
            this.btnEfectivo.Text = "1 - Efectivo";
            this.btnEfectivo.UseVisualStyleBackColor = true;
            this.btnEfectivo.Click += new System.EventHandler(this.btnEfectivo_Click);
            this.btnEfectivo.Enter += new System.EventHandler(this.btnEfectivo_Enter);
            // 
            // btnDebito
            // 
            this.btnDebito.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnDebito.Location = new System.Drawing.Point(132, 73);
            this.btnDebito.Name = "btnDebito";
            this.btnDebito.Size = new System.Drawing.Size(114, 46);
            this.btnDebito.TabIndex = 2;
            this.btnDebito.Text = "2 - Débito";
            this.btnDebito.UseVisualStyleBackColor = true;
            this.btnDebito.Click += new System.EventHandler(this.btnDebito_Click);
            this.btnDebito.Enter += new System.EventHandler(this.btnDebito_Enter);
            // 
            // btnCredito
            // 
            this.btnCredito.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnCredito.Location = new System.Drawing.Point(252, 73);
            this.btnCredito.Name = "btnCredito";
            this.btnCredito.Size = new System.Drawing.Size(114, 46);
            this.btnCredito.TabIndex = 3;
            this.btnCredito.Text = "3 - Crédito";
            this.btnCredito.UseVisualStyleBackColor = true;
            this.btnCredito.Click += new System.EventHandler(this.btnCredito_Click);
            this.btnCredito.Enter += new System.EventHandler(this.btnCredito_Enter);
            // 
            // btnTransf
            // 
            this.btnTransf.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnTransf.Location = new System.Drawing.Point(252, 137);
            this.btnTransf.Name = "btnTransf";
            this.btnTransf.Size = new System.Drawing.Size(114, 46);
            this.btnTransf.TabIndex = 6;
            this.btnTransf.Text = "6 - Transf.";
            this.btnTransf.UseVisualStyleBackColor = true;
            this.btnTransf.Click += new System.EventHandler(this.btnTransf_Click);
            this.btnTransf.Enter += new System.EventHandler(this.btnTransf_Enter);
            // 
            // btnQr
            // 
            this.btnQr.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnQr.Location = new System.Drawing.Point(132, 137);
            this.btnQr.Name = "btnQr";
            this.btnQr.Size = new System.Drawing.Size(114, 46);
            this.btnQr.TabIndex = 5;
            this.btnQr.Text = "5 - Qr      ";
            this.btnQr.UseVisualStyleBackColor = true;
            this.btnQr.Click += new System.EventHandler(this.btnQr_Click);
            this.btnQr.Enter += new System.EventHandler(this.btnQr_Enter);
            // 
            // btnBilleteraStaFe
            // 
            this.btnBilleteraStaFe.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnBilleteraStaFe.Location = new System.Drawing.Point(12, 137);
            this.btnBilleteraStaFe.Name = "btnBilleteraStaFe";
            this.btnBilleteraStaFe.Size = new System.Drawing.Size(114, 46);
            this.btnBilleteraStaFe.TabIndex = 4;
            this.btnBilleteraStaFe.Text = "4 - Billetera";
            this.btnBilleteraStaFe.UseVisualStyleBackColor = true;
            this.btnBilleteraStaFe.Click += new System.EventHandler(this.btnBilleteraStaFe_Click);
            this.btnBilleteraStaFe.Enter += new System.EventHandler(this.btnBilleteraStaFe_Enter);
            // 
            // formFormaPago
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(386, 206);
            this.Controls.Add(this.btnTransf);
            this.Controls.Add(this.btnQr);
            this.Controls.Add(this.btnBilleteraStaFe);
            this.Controls.Add(this.btnCredito);
            this.Controls.Add(this.btnDebito);
            this.Controls.Add(this.btnEfectivo);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "formFormaPago";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Forma de Pago";
            this.Load += new System.EventHandler(this.formFormaPago_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnEfectivo;
        private System.Windows.Forms.Button btnDebito;
        private System.Windows.Forms.Button btnCredito;
        private System.Windows.Forms.Button btnTransf;
        private System.Windows.Forms.Button btnQr;
        private System.Windows.Forms.Button btnBilleteraStaFe;
    }
}