namespace Presentacion.Caja
{
    partial class formFinalizarVenta
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
            this.btnSinTicket = new System.Windows.Forms.Button();
            this.btnTicket = new System.Windows.Forms.Button();
            this.btnFactura = new System.Windows.Forms.Button();
            this.btnSalir = new System.Windows.Forms.Button();
            this.lblFormaPago = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.label1.Location = new System.Drawing.Point(133, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(171, 25);
            this.label1.TabIndex = 0;
            this.label1.Text = "Finalizar Venta";
            // 
            // btnSinTicket
            // 
            this.btnSinTicket.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSinTicket.Location = new System.Drawing.Point(12, 73);
            this.btnSinTicket.Name = "btnSinTicket";
            this.btnSinTicket.Size = new System.Drawing.Size(135, 46);
            this.btnSinTicket.TabIndex = 1;
            this.btnSinTicket.Text = "1 - Si (s/ticket)";
            this.btnSinTicket.UseVisualStyleBackColor = true;
            this.btnSinTicket.Click += new System.EventHandler(this.btnSinTicket_Click);
            this.btnSinTicket.Enter += new System.EventHandler(this.btnEfectivo_Enter);
            // 
            // btnTicket
            // 
            this.btnTicket.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnTicket.Location = new System.Drawing.Point(153, 73);
            this.btnTicket.Name = "btnTicket";
            this.btnTicket.Size = new System.Drawing.Size(135, 46);
            this.btnTicket.TabIndex = 2;
            this.btnTicket.Text = "2 - Ticket";
            this.btnTicket.UseVisualStyleBackColor = true;
            this.btnTicket.Click += new System.EventHandler(this.btnTicket_Click);
            this.btnTicket.Enter += new System.EventHandler(this.btnDebito_Enter);
            // 
            // btnFactura
            // 
            this.btnFactura.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnFactura.Location = new System.Drawing.Point(294, 73);
            this.btnFactura.Name = "btnFactura";
            this.btnFactura.Size = new System.Drawing.Size(135, 46);
            this.btnFactura.TabIndex = 3;
            this.btnFactura.Text = "3 - Factura";
            this.btnFactura.UseVisualStyleBackColor = true;
            this.btnFactura.Click += new System.EventHandler(this.btnFactura_Click);
            this.btnFactura.Enter += new System.EventHandler(this.btnCredito_Enter);
            // 
            // btnSalir
            // 
            this.btnSalir.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSalir.Location = new System.Drawing.Point(153, 125);
            this.btnSalir.Name = "btnSalir";
            this.btnSalir.Size = new System.Drawing.Size(135, 46);
            this.btnSalir.TabIndex = 4;
            this.btnSalir.Text = "0 - Salir";
            this.btnSalir.UseVisualStyleBackColor = true;
            this.btnSalir.Click += new System.EventHandler(this.btnSalir_Click);
            // 
            // lblFormaPago
            // 
            this.lblFormaPago.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.lblFormaPago.AutoSize = true;
            this.lblFormaPago.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblFormaPago.ForeColor = System.Drawing.Color.Olive;
            this.lblFormaPago.Location = new System.Drawing.Point(140, 39);
            this.lblFormaPago.Name = "lblFormaPago";
            this.lblFormaPago.Size = new System.Drawing.Size(155, 25);
            this.lblFormaPago.TabIndex = 5;
            this.lblFormaPago.Text = "-Forma Pago-";
            // 
            // formFinalizarVenta
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(437, 179);
            this.ControlBox = false;
            this.Controls.Add(this.lblFormaPago);
            this.Controls.Add(this.btnSalir);
            this.Controls.Add(this.btnFactura);
            this.Controls.Add(this.btnTicket);
            this.Controls.Add(this.btnSinTicket);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "formFinalizarVenta";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Finalizar Venta";
            this.Load += new System.EventHandler(this.formFinalizarVenta_Load);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.formFinalizarVenta_FormClosed);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.formFinalizarVenta_FormClosing);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnSinTicket;
        private System.Windows.Forms.Button btnTicket;
        private System.Windows.Forms.Button btnFactura;
        private System.Windows.Forms.Button btnSalir;
        private System.Windows.Forms.Label lblFormaPago;
    }
}