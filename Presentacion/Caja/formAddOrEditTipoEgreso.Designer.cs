namespace Presentacion.Caja
{
    partial class formAddOrEditTipoEgreso
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
            this.pnlBuscar = new System.Windows.Forms.Panel();
            this.checkEsGasto = new System.Windows.Forms.CheckBox();
            this.txtIdTipoEgreso = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.txtTipoEgresoCaja = new System.Windows.Forms.TextBox();
            this.lblTipo = new System.Windows.Forms.Label();
            this.idEgresoCajaLabel = new System.Windows.Forms.Label();
            this.btnGuardar = new System.Windows.Forms.Button();
            this.btnCancelar = new System.Windows.Forms.Button();
            this.pnlBuscar.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlBuscar
            // 
            this.pnlBuscar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pnlBuscar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(43)))), ((int)(((byte)(77)))), ((int)(((byte)(129)))));
            this.pnlBuscar.Controls.Add(this.checkEsGasto);
            this.pnlBuscar.Controls.Add(this.txtIdTipoEgreso);
            this.pnlBuscar.Controls.Add(this.txtTipoEgresoCaja);
            this.pnlBuscar.Controls.Add(this.label1);
            this.pnlBuscar.Controls.Add(this.lblTipo);
            this.pnlBuscar.Location = new System.Drawing.Point(-1, -4);
            this.pnlBuscar.Margin = new System.Windows.Forms.Padding(4);
            this.pnlBuscar.Name = "pnlBuscar";
            this.pnlBuscar.Size = new System.Drawing.Size(405, 177);
            this.pnlBuscar.TabIndex = 4;
            // 
            // checkEsGasto
            // 
            this.checkEsGasto.AutoSize = true;
            this.checkEsGasto.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.checkEsGasto.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.checkEsGasto.ForeColor = System.Drawing.Color.Cornsilk;
            this.checkEsGasto.Location = new System.Drawing.Point(30, 115);
            this.checkEsGasto.Margin = new System.Windows.Forms.Padding(4);
            this.checkEsGasto.Name = "checkEsGasto";
            this.checkEsGasto.Size = new System.Drawing.Size(101, 24);
            this.checkEsGasto.TabIndex = 2;
            this.checkEsGasto.Text = "&Es Gasto";
            this.checkEsGasto.UseVisualStyleBackColor = true;
            // 
            // txtIdTipoEgreso
            // 
            this.txtIdTipoEgreso.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtIdTipoEgreso.Location = new System.Drawing.Point(116, 44);
            this.txtIdTipoEgreso.Margin = new System.Windows.Forms.Padding(4);
            this.txtIdTipoEgreso.Name = "txtIdTipoEgreso";
            this.txtIdTipoEgreso.ReadOnly = true;
            this.txtIdTipoEgreso.Size = new System.Drawing.Size(209, 26);
            this.txtIdTipoEgreso.TabIndex = 53;
            this.txtIdTipoEgreso.TabStop = false;
            this.txtIdTipoEgreso.Text = "-";
            this.txtIdTipoEgreso.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ForeColor = System.Drawing.Color.Cornsilk;
            this.label1.Location = new System.Drawing.Point(80, 47);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(26, 20);
            this.label1.TabIndex = 51;
            this.label1.Text = "ID";
            // 
            // txtTipoEgresoCaja
            // 
            this.txtTipoEgresoCaja.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtTipoEgresoCaja.Location = new System.Drawing.Point(116, 81);
            this.txtTipoEgresoCaja.Margin = new System.Windows.Forms.Padding(4);
            this.txtTipoEgresoCaja.Name = "txtTipoEgresoCaja";
            this.txtTipoEgresoCaja.Size = new System.Drawing.Size(209, 26);
            this.txtTipoEgresoCaja.TabIndex = 1;
            // 
            // lblTipo
            // 
            this.lblTipo.AutoSize = true;
            this.lblTipo.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTipo.ForeColor = System.Drawing.Color.Cornsilk;
            this.lblTipo.Location = new System.Drawing.Point(60, 84);
            this.lblTipo.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblTipo.Name = "lblTipo";
            this.lblTipo.Size = new System.Drawing.Size(41, 20);
            this.lblTipo.TabIndex = 41;
            this.lblTipo.Text = "Tipo";
            // 
            // idEgresoCajaLabel
            // 
            this.idEgresoCajaLabel.AutoSize = true;
            this.idEgresoCajaLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.idEgresoCajaLabel.ForeColor = System.Drawing.Color.DarkBlue;
            this.idEgresoCajaLabel.Location = new System.Drawing.Point(3, 177);
            this.idEgresoCajaLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.idEgresoCajaLabel.Name = "idEgresoCajaLabel";
            this.idEgresoCajaLabel.Size = new System.Drawing.Size(127, 17);
            this.idEgresoCajaLabel.TabIndex = 48;
            this.idEgresoCajaLabel.Text = "idEgresoCajaLabel";
            this.idEgresoCajaLabel.Visible = false;
            // 
            // btnGuardar
            // 
            this.btnGuardar.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnGuardar.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnGuardar.Location = new System.Drawing.Point(139, 179);
            this.btnGuardar.Margin = new System.Windows.Forms.Padding(4);
            this.btnGuardar.Name = "btnGuardar";
            this.btnGuardar.Size = new System.Drawing.Size(121, 33);
            this.btnGuardar.TabIndex = 3;
            this.btnGuardar.Text = "&Guardar";
            this.btnGuardar.UseVisualStyleBackColor = true;
            this.btnGuardar.Click += new System.EventHandler(this.btnGuardar_Click);
            // 
            // btnCancelar
            // 
            this.btnCancelar.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancelar.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnCancelar.Location = new System.Drawing.Point(268, 179);
            this.btnCancelar.Margin = new System.Windows.Forms.Padding(4);
            this.btnCancelar.Name = "btnCancelar";
            this.btnCancelar.Size = new System.Drawing.Size(121, 33);
            this.btnCancelar.TabIndex = 50;
            this.btnCancelar.TabStop = false;
            this.btnCancelar.Text = "&Cancelar";
            this.btnCancelar.UseVisualStyleBackColor = true;
            this.btnCancelar.Click += new System.EventHandler(this.btnCancelar_Click_1);
            // 
            // formAddOrEditTipoEgreso
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(239)))), ((int)(((byte)(250)))));
            this.ClientSize = new System.Drawing.Size(402, 215);
            this.Controls.Add(this.btnGuardar);
            this.Controls.Add(this.btnCancelar);
            this.Controls.Add(this.idEgresoCajaLabel);
            this.Controls.Add(this.pnlBuscar);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MaximizeBox = false;
            this.Name = "formAddOrEditTipoEgreso";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Tipo Egreso Caja";
            this.Load += new System.EventHandler(this.formAddOrEditTipoEgreso_Load);
            this.pnlBuscar.ResumeLayout(false);
            this.pnlBuscar.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        protected System.Windows.Forms.Panel pnlBuscar;
        protected System.Windows.Forms.Label lblTipo;
        protected System.Windows.Forms.Label idEgresoCajaLabel;
        private System.Windows.Forms.TextBox txtTipoEgresoCaja;
        private System.Windows.Forms.CheckBox checkEsGasto;
        private System.Windows.Forms.TextBox txtIdTipoEgreso;
        protected System.Windows.Forms.Label label1;
        protected System.Windows.Forms.Button btnGuardar;
        protected System.Windows.Forms.Button btnCancelar;
    }
}