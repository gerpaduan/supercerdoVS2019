namespace Presentacion.Cortes
{
    partial class formAddOrEditTipoProducto
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
            this.txtOrden = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.txtTipoProducto = new System.Windows.Forms.TextBox();
            this.lblTipo = new System.Windows.Forms.Label();
            this.btnGuardar = new System.Windows.Forms.Button();
            this.btnCancelar = new System.Windows.Forms.Button();
            this.tipoProdLabel = new System.Windows.Forms.Label();
            this.checkMayuscula = new System.Windows.Forms.CheckBox();
            this.pnlBuscar.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlBuscar
            // 
            this.pnlBuscar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pnlBuscar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(43)))), ((int)(((byte)(77)))), ((int)(((byte)(129)))));
            this.pnlBuscar.Controls.Add(this.checkMayuscula);
            this.pnlBuscar.Controls.Add(this.txtOrden);
            this.pnlBuscar.Controls.Add(this.label1);
            this.pnlBuscar.Controls.Add(this.txtTipoProducto);
            this.pnlBuscar.Controls.Add(this.lblTipo);
            this.pnlBuscar.Location = new System.Drawing.Point(-1, -4);
            this.pnlBuscar.Margin = new System.Windows.Forms.Padding(4);
            this.pnlBuscar.Name = "pnlBuscar";
            this.pnlBuscar.Size = new System.Drawing.Size(405, 149);
            this.pnlBuscar.TabIndex = 4;
            // 
            // txtOrden
            // 
            this.txtOrden.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtOrden.Location = new System.Drawing.Point(115, 85);
            this.txtOrden.Margin = new System.Windows.Forms.Padding(4);
            this.txtOrden.Name = "txtOrden";
            this.txtOrden.Size = new System.Drawing.Size(69, 26);
            this.txtOrden.TabIndex = 2;
            this.txtOrden.Text = "100";
            this.txtOrden.MouseEnter += new System.EventHandler(this.txtOrden_MouseEnter);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ForeColor = System.Drawing.Color.Cornsilk;
            this.label1.Location = new System.Drawing.Point(52, 88);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(55, 20);
            this.label1.TabIndex = 43;
            this.label1.Text = "Orden";
            // 
            // txtTipoProducto
            // 
            this.txtTipoProducto.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtTipoProducto.Location = new System.Drawing.Point(115, 51);
            this.txtTipoProducto.Margin = new System.Windows.Forms.Padding(4);
            this.txtTipoProducto.Name = "txtTipoProducto";
            this.txtTipoProducto.Size = new System.Drawing.Size(209, 26);
            this.txtTipoProducto.TabIndex = 1;
            this.txtTipoProducto.TextChanged += new System.EventHandler(this.txtTipoProducto_TextChanged);
            // 
            // lblTipo
            // 
            this.lblTipo.AutoSize = true;
            this.lblTipo.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTipo.ForeColor = System.Drawing.Color.Cornsilk;
            this.lblTipo.Location = new System.Drawing.Point(66, 54);
            this.lblTipo.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblTipo.Name = "lblTipo";
            this.lblTipo.Size = new System.Drawing.Size(41, 20);
            this.lblTipo.TabIndex = 41;
            this.lblTipo.Text = "Tipo";
            // 
            // btnGuardar
            // 
            this.btnGuardar.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnGuardar.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnGuardar.Location = new System.Drawing.Point(139, 152);
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
            this.btnCancelar.Location = new System.Drawing.Point(268, 152);
            this.btnCancelar.Margin = new System.Windows.Forms.Padding(4);
            this.btnCancelar.Name = "btnCancelar";
            this.btnCancelar.Size = new System.Drawing.Size(121, 33);
            this.btnCancelar.TabIndex = 4;
            this.btnCancelar.TabStop = false;
            this.btnCancelar.Text = "&Cancelar";
            this.btnCancelar.UseVisualStyleBackColor = true;
            this.btnCancelar.Click += new System.EventHandler(this.btnCancelar_Click_1);
            // 
            // tipoProdLabel
            // 
            this.tipoProdLabel.AutoSize = true;
            this.tipoProdLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tipoProdLabel.ForeColor = System.Drawing.Color.DarkBlue;
            this.tipoProdLabel.Location = new System.Drawing.Point(4, 162);
            this.tipoProdLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.tipoProdLabel.Name = "tipoProdLabel";
            this.tipoProdLabel.Size = new System.Drawing.Size(96, 17);
            this.tipoProdLabel.TabIndex = 49;
            this.tipoProdLabel.Text = "tipoProdLabel";
            this.tipoProdLabel.Visible = false;
            // 
            // checkMayuscula
            // 
            this.checkMayuscula.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.checkMayuscula.AutoSize = true;
            this.checkMayuscula.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.checkMayuscula.Checked = true;
            this.checkMayuscula.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkMayuscula.ForeColor = System.Drawing.Color.Cornsilk;
            this.checkMayuscula.Location = new System.Drawing.Point(296, 125);
            this.checkMayuscula.Margin = new System.Windows.Forms.Padding(4);
            this.checkMayuscula.Name = "checkMayuscula";
            this.checkMayuscula.Size = new System.Drawing.Size(94, 20);
            this.checkMayuscula.TabIndex = 44;
            this.checkMayuscula.TabStop = false;
            this.checkMayuscula.Text = "Mayúscula";
            this.checkMayuscula.UseVisualStyleBackColor = true;
            // 
            // formAddOrEditTipoProducto
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(239)))), ((int)(((byte)(250)))));
            this.ClientSize = new System.Drawing.Size(402, 188);
            this.Controls.Add(this.tipoProdLabel);
            this.Controls.Add(this.btnGuardar);
            this.Controls.Add(this.btnCancelar);
            this.Controls.Add(this.pnlBuscar);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MaximizeBox = false;
            this.Name = "formAddOrEditTipoProducto";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Tipo Producto";
            this.Load += new System.EventHandler(this.formAddOrEditTipoProducto_Load);
            this.pnlBuscar.ResumeLayout(false);
            this.pnlBuscar.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        protected System.Windows.Forms.Panel pnlBuscar;
        protected System.Windows.Forms.Label lblTipo;
        private System.Windows.Forms.TextBox txtTipoProducto;
        protected System.Windows.Forms.Button btnGuardar;
        protected System.Windows.Forms.Button btnCancelar;
        private System.Windows.Forms.TextBox txtOrden;
        protected System.Windows.Forms.Label label1;
        protected System.Windows.Forms.Label tipoProdLabel;
        private System.Windows.Forms.CheckBox checkMayuscula;
    }
}