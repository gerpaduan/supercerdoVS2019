namespace Presentacion.Caja
{
    partial class formPagoMixto
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(formPagoMixto));
            this.btnIngresar = new System.Windows.Forms.Button();
            this.lbl = new System.Windows.Forms.Label();
            this.txtImporteEfectivo = new System.Windows.Forms.TextBox();
            this.txtImporte2 = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.txtTotalS = new System.Windows.Forms.TextBox();
            this.lblFormaPagoTicket = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // btnIngresar
            // 
            this.btnIngresar.Font = new System.Drawing.Font("Microsoft Sans Serif", 13.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnIngresar.Location = new System.Drawing.Point(10, 203);
            this.btnIngresar.Name = "btnIngresar";
            this.btnIngresar.Size = new System.Drawing.Size(336, 46);
            this.btnIngresar.TabIndex = 3;
            this.btnIngresar.Text = "Ingresar";
            this.btnIngresar.UseVisualStyleBackColor = true;
            this.btnIngresar.Click += new System.EventHandler(this.btnIngresar_Click);
            this.btnIngresar.Enter += new System.EventHandler(this.btnIngresar_Enter);
            this.btnIngresar.Leave += new System.EventHandler(this.btnIngresar_Leave);
            // 
            // lbl
            // 
            this.lbl.AutoSize = true;
            this.lbl.Font = new System.Drawing.Font("Microsoft Sans Serif", 13.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbl.Location = new System.Drawing.Point(103, 95);
            this.lbl.Name = "lbl";
            this.lbl.Size = new System.Drawing.Size(76, 24);
            this.lbl.TabIndex = 5;
            this.lbl.Text = "Efectivo";
            // 
            // txtImporteEfectivo
            // 
            this.txtImporteEfectivo.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.txtImporteEfectivo.BackColor = System.Drawing.SystemColors.Window;
            this.txtImporteEfectivo.Font = new System.Drawing.Font("Microsoft Sans Serif", 16.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtImporteEfectivo.Location = new System.Drawing.Point(185, 90);
            this.txtImporteEfectivo.Name = "txtImporteEfectivo";
            this.txtImporteEfectivo.Size = new System.Drawing.Size(162, 32);
            this.txtImporteEfectivo.TabIndex = 1;
            this.txtImporteEfectivo.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.txtImporteEfectivo.TextChanged += new System.EventHandler(this.txtImporte1_TextChanged);
            this.txtImporteEfectivo.Enter += new System.EventHandler(this.txtImporteEfectivo_Enter);
            this.txtImporteEfectivo.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.TxtPruebaENTER_KeyPress);
            this.txtImporteEfectivo.Leave += new System.EventHandler(this.txtImporteEfectivo_Leave);
            // 
            // txtImporte2
            // 
            this.txtImporte2.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.txtImporte2.BackColor = System.Drawing.SystemColors.Window;
            this.txtImporte2.Font = new System.Drawing.Font("Microsoft Sans Serif", 16.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtImporte2.Location = new System.Drawing.Point(185, 141);
            this.txtImporte2.Name = "txtImporte2";
            this.txtImporte2.ReadOnly = true;
            this.txtImporte2.Size = new System.Drawing.Size(162, 32);
            this.txtImporte2.TabIndex = 20;
            this.txtImporte2.TabStop = false;
            this.txtImporte2.Text = "0,00";
            this.txtImporte2.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label9
            // 
            this.label9.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.label9.AutoSize = true;
            this.label9.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label9.Location = new System.Drawing.Point(33, 33);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(152, 25);
            this.label9.TabIndex = 36;
            this.label9.Text = "Total a pagar";
            // 
            // txtTotalS
            // 
            this.txtTotalS.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.txtTotalS.BackColor = System.Drawing.SystemColors.ScrollBar;
            this.txtTotalS.Font = new System.Drawing.Font("Microsoft Sans Serif", 21.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtTotalS.ForeColor = System.Drawing.Color.DarkRed;
            this.txtTotalS.Location = new System.Drawing.Point(185, 24);
            this.txtTotalS.Name = "txtTotalS";
            this.txtTotalS.ReadOnly = true;
            this.txtTotalS.Size = new System.Drawing.Size(162, 40);
            this.txtTotalS.TabIndex = 37;
            this.txtTotalS.TabStop = false;
            this.txtTotalS.Text = "000,00";
            this.txtTotalS.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // lblFormaPagoTicket
            // 
            this.lblFormaPagoTicket.AutoSize = true;
            this.lblFormaPagoTicket.Font = new System.Drawing.Font("Microsoft Sans Serif", 13.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblFormaPagoTicket.Location = new System.Drawing.Point(18, 146);
            this.lblFormaPagoTicket.Name = "lblFormaPagoTicket";
            this.lblFormaPagoTicket.Size = new System.Drawing.Size(161, 24);
            this.lblFormaPagoTicket.TabIndex = 38;
            this.lblFormaPagoTicket.Text = "Forma Pago ticket";
            this.lblFormaPagoTicket.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // formPagoMixto
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(368, 266);
            this.Controls.Add(this.lblFormaPagoTicket);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.txtTotalS);
            this.Controls.Add(this.txtImporte2);
            this.Controls.Add(this.txtImporteEfectivo);
            this.Controls.Add(this.btnIngresar);
            this.Controls.Add(this.lbl);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "formPagoMixto";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Pago Mixto";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.formPagoMixto_FormClosing);
            this.Load += new System.EventHandler(this.formPagoMixto_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button btnIngresar;
        private System.Windows.Forms.Label lbl;
        private System.Windows.Forms.TextBox txtImporteEfectivo;
        private System.Windows.Forms.TextBox txtImporte2;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.TextBox txtTotalS;
        private System.Windows.Forms.Label lblFormaPagoTicket;
    }
}