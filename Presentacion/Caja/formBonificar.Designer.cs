namespace Presentacion.Caja
{
    partial class formBonificar
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
            this.txtCorte = new System.Windows.Forms.TextBox();
            this.label14 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.txtTotalCorte = new System.Windows.Forms.MaskedTextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.txtPrecioKg = new System.Windows.Forms.MaskedTextBox();
            this.label13 = new System.Windows.Forms.Label();
            this.txtCantKgs = new System.Windows.Forms.MaskedTextBox();
            this.label15 = new System.Windows.Forms.Label();
            this.btnBonificar = new System.Windows.Forms.Button();
            this.btnPrecioReal = new System.Windows.Forms.Button();
            this.checkPorcentaje = new System.Windows.Forms.CheckBox();
            this.txtPorcentaje = new System.Windows.Forms.MaskedTextBox();
            this.checkBonificarTodos = new System.Windows.Forms.CheckBox();
            this.txtCodigo = new System.Windows.Forms.MaskedTextBox();
            this.SuspendLayout();
            // 
            // txtCorte
            // 
            this.txtCorte.BackColor = System.Drawing.SystemColors.ScrollBar;
            this.txtCorte.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtCorte.Location = new System.Drawing.Point(108, 76);
            this.txtCorte.Margin = new System.Windows.Forms.Padding(4);
            this.txtCorte.Name = "txtCorte";
            this.txtCorte.ReadOnly = true;
            this.txtCorte.Size = new System.Drawing.Size(404, 34);
            this.txtCorte.TabIndex = 51;
            this.txtCorte.TabStop = false;
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label14.ForeColor = System.Drawing.Color.Black;
            this.label14.Location = new System.Drawing.Point(36, 84);
            this.label14.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(60, 25);
            this.label14.TabIndex = 50;
            this.label14.Text = "Corte";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label7.ForeColor = System.Drawing.Color.Black;
            this.label7.Location = new System.Drawing.Point(41, 213);
            this.label7.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(56, 25);
            this.label7.TabIndex = 49;
            this.label7.Text = "Total";
            // 
            // txtTotalCorte
            // 
            this.txtTotalCorte.BackColor = System.Drawing.SystemColors.ScrollBar;
            this.txtTotalCorte.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtTotalCorte.Location = new System.Drawing.Point(108, 206);
            this.txtTotalCorte.Margin = new System.Windows.Forms.Padding(4);
            this.txtTotalCorte.Name = "txtTotalCorte";
            this.txtTotalCorte.ReadOnly = true;
            this.txtTotalCorte.Size = new System.Drawing.Size(210, 34);
            this.txtTotalCorte.TabIndex = 48;
            this.txtTotalCorte.TabStop = false;
            this.txtTotalCorte.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.ForeColor = System.Drawing.Color.Black;
            this.label5.Location = new System.Drawing.Point(21, 41);
            this.label5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(75, 25);
            this.label5.TabIndex = 52;
            this.label5.Text = "Código";
            // 
            // txtPrecioKg
            // 
            this.txtPrecioKg.BackColor = System.Drawing.SystemColors.Window;
            this.txtPrecioKg.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtPrecioKg.Location = new System.Drawing.Point(108, 119);
            this.txtPrecioKg.Margin = new System.Windows.Forms.Padding(4);
            this.txtPrecioKg.Name = "txtPrecioKg";
            this.txtPrecioKg.Size = new System.Drawing.Size(210, 34);
            this.txtPrecioKg.TabIndex = 1;
            this.txtPrecioKg.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.txtPrecioKg.TextChanged += new System.EventHandler(this.txtPrecioKg_TextChanged);
            this.txtPrecioKg.Enter += new System.EventHandler(this.txtPrecioKg_Enter);
            this.txtPrecioKg.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.TxtPruebaENTER_KeyPress);
            this.txtPrecioKg.Leave += new System.EventHandler(this.txtPrecioKg_Leave);
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label13.ForeColor = System.Drawing.Color.Black;
            this.label13.Location = new System.Drawing.Point(47, 170);
            this.label13.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(52, 25);
            this.label13.TabIndex = 53;
            this.label13.Text = "Kgs.";
            // 
            // txtCantKgs
            // 
            this.txtCantKgs.BackColor = System.Drawing.SystemColors.ScrollBar;
            this.txtCantKgs.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtCantKgs.Location = new System.Drawing.Point(108, 162);
            this.txtCantKgs.Margin = new System.Windows.Forms.Padding(4);
            this.txtCantKgs.Name = "txtCantKgs";
            this.txtCantKgs.ReadOnly = true;
            this.txtCantKgs.Size = new System.Drawing.Size(210, 34);
            this.txtCantKgs.TabIndex = 46;
            this.txtCantKgs.TabStop = false;
            this.txtCantKgs.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label15.ForeColor = System.Drawing.Color.Black;
            this.label15.Location = new System.Drawing.Point(29, 127);
            this.label15.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(67, 25);
            this.label15.TabIndex = 54;
            this.label15.Text = "Precio";
            // 
            // btnBonificar
            // 
            this.btnBonificar.BackColor = System.Drawing.Color.MediumSeaGreen;
            this.btnBonificar.Font = new System.Drawing.Font("Microsoft Sans Serif", 13F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnBonificar.Location = new System.Drawing.Point(108, 249);
            this.btnBonificar.Margin = new System.Windows.Forms.Padding(4);
            this.btnBonificar.Name = "btnBonificar";
            this.btnBonificar.Size = new System.Drawing.Size(367, 52);
            this.btnBonificar.TabIndex = 2;
            this.btnBonificar.Text = "&Bonificar";
            this.btnBonificar.UseVisualStyleBackColor = false;
            this.btnBonificar.Click += new System.EventHandler(this.btnBonificar_Click);
            this.btnBonificar.Enter += new System.EventHandler(this.btnBonificar_Enter);
            this.btnBonificar.Leave += new System.EventHandler(this.btnBonificar_Leave);
            // 
            // btnPrecioReal
            // 
            this.btnPrecioReal.Location = new System.Drawing.Point(326, 162);
            this.btnPrecioReal.Margin = new System.Windows.Forms.Padding(4);
            this.btnPrecioReal.Name = "btnPrecioReal";
            this.btnPrecioReal.Size = new System.Drawing.Size(149, 36);
            this.btnPrecioReal.TabIndex = 55;
            this.btnPrecioReal.Text = "Precio &Real";
            this.btnPrecioReal.UseVisualStyleBackColor = true;
            this.btnPrecioReal.Click += new System.EventHandler(this.btnPrecioReal_Click);
            // 
            // checkPorcentaje
            // 
            this.checkPorcentaje.Appearance = System.Windows.Forms.Appearance.Button;
            this.checkPorcentaje.AutoSize = true;
            this.checkPorcentaje.BackColor = System.Drawing.Color.LimeGreen;
            this.checkPorcentaje.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.checkPorcentaje.Checked = true;
            this.checkPorcentaje.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkPorcentaje.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.checkPorcentaje.ForeColor = System.Drawing.Color.Cornsilk;
            this.checkPorcentaje.Location = new System.Drawing.Point(434, 119);
            this.checkPorcentaje.Margin = new System.Windows.Forms.Padding(4);
            this.checkPorcentaje.Name = "checkPorcentaje";
            this.checkPorcentaje.Size = new System.Drawing.Size(41, 35);
            this.checkPorcentaje.TabIndex = 56;
            this.checkPorcentaje.TabStop = false;
            this.checkPorcentaje.Text = "%";
            this.checkPorcentaje.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.checkPorcentaje.UseVisualStyleBackColor = false;
            this.checkPorcentaje.CheckedChanged += new System.EventHandler(this.checkPorcentaje_CheckedChanged);
            // 
            // txtPorcentaje
            // 
            this.txtPorcentaje.BackColor = System.Drawing.SystemColors.Window;
            this.txtPorcentaje.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtPorcentaje.Location = new System.Drawing.Point(326, 119);
            this.txtPorcentaje.Margin = new System.Windows.Forms.Padding(4);
            this.txtPorcentaje.Name = "txtPorcentaje";
            this.txtPorcentaje.Size = new System.Drawing.Size(100, 34);
            this.txtPorcentaje.TabIndex = 57;
            this.txtPorcentaje.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.txtPorcentaje.TextChanged += new System.EventHandler(this.txtPorcentaje_TextChanged);
            this.txtPorcentaje.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.TxtPruebaENTER_KeyPress);
            // 
            // checkBonificarTodos
            // 
            this.checkBonificarTodos.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.checkBonificarTodos.Appearance = System.Windows.Forms.Appearance.Button;
            this.checkBonificarTodos.AutoSize = true;
            this.checkBonificarTodos.BackColor = System.Drawing.Color.LimeGreen;
            this.checkBonificarTodos.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.checkBonificarTodos.Checked = true;
            this.checkBonificarTodos.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBonificarTodos.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.checkBonificarTodos.ForeColor = System.Drawing.Color.Cornsilk;
            this.checkBonificarTodos.Location = new System.Drawing.Point(360, 40);
            this.checkBonificarTodos.Margin = new System.Windows.Forms.Padding(4);
            this.checkBonificarTodos.Name = "checkBonificarTodos";
            this.checkBonificarTodos.Size = new System.Drawing.Size(152, 30);
            this.checkBonificarTodos.TabIndex = 58;
            this.checkBonificarTodos.TabStop = false;
            this.checkBonificarTodos.Text = "Bonificar &Todos";
            this.checkBonificarTodos.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.checkBonificarTodos.UseVisualStyleBackColor = false;
            this.checkBonificarTodos.CheckedChanged += new System.EventHandler(this.checkBonificarTodos_CheckedChanged);
            // 
            // txtCodigo
            // 
            this.txtCodigo.BackColor = System.Drawing.SystemColors.ScrollBar;
            this.txtCodigo.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtCodigo.Location = new System.Drawing.Point(108, 36);
            this.txtCodigo.Margin = new System.Windows.Forms.Padding(4);
            this.txtCodigo.Name = "txtCodigo";
            this.txtCodigo.ReadOnly = true;
            this.txtCodigo.Size = new System.Drawing.Size(231, 34);
            this.txtCodigo.TabIndex = 59;
            this.txtCodigo.TabStop = false;
            this.txtCodigo.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // formBonificar
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(525, 315);
            this.Controls.Add(this.txtCodigo);
            this.Controls.Add(this.checkBonificarTodos);
            this.Controls.Add(this.txtPorcentaje);
            this.Controls.Add(this.checkPorcentaje);
            this.Controls.Add(this.btnPrecioReal);
            this.Controls.Add(this.btnBonificar);
            this.Controls.Add(this.txtCorte);
            this.Controls.Add(this.label14);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.txtTotalCorte);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.txtPrecioKg);
            this.Controls.Add(this.label13);
            this.Controls.Add(this.txtCantKgs);
            this.Controls.Add(this.label15);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "formBonificar";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Bonificar";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.formBonificar_FormClosing);
            this.Load += new System.EventHandler(this.formBonificar_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        protected System.Windows.Forms.TextBox txtCorte;
        protected System.Windows.Forms.Label label14;
        protected System.Windows.Forms.Label label7;
        private System.Windows.Forms.MaskedTextBox txtTotalCorte;
        protected System.Windows.Forms.Label label5;
        private System.Windows.Forms.MaskedTextBox txtPrecioKg;
        protected System.Windows.Forms.Label label13;
        private System.Windows.Forms.MaskedTextBox txtCantKgs;
        protected System.Windows.Forms.Label label15;
        private System.Windows.Forms.Button btnBonificar;
        private System.Windows.Forms.Button btnPrecioReal;
        private System.Windows.Forms.CheckBox checkPorcentaje;
        private System.Windows.Forms.MaskedTextBox txtPorcentaje;
        private System.Windows.Forms.CheckBox checkBonificarTodos;
        private System.Windows.Forms.MaskedTextBox txtCodigo;
    }
}