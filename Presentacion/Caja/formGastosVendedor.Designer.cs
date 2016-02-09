namespace Presentacion.Caja
{
    partial class formGastosVendedor
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
            this.grillaGastos = new System.Windows.Forms.DataGridView();
            this.btnCancelar = new System.Windows.Forms.Button();
            this.txtDetalle = new System.Windows.Forms.TextBox();
            this.lblMonto = new System.Windows.Forms.Label();
            this.txtMonto = new System.Windows.Forms.TextBox();
            this.lblDescripcion = new System.Windows.Forms.Label();
            this.txtDescripcion = new System.Windows.Forms.TextBox();
            this.lblDetalle = new System.Windows.Forms.Label();
            this.txtTipoGasto = new System.Windows.Forms.TextBox();
            this.lblTipo = new System.Windows.Forms.Label();
            this.txtFechaTexto = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.grillaGastos)).BeginInit();
            this.SuspendLayout();
            // 
            // grillaGastos
            // 
            this.grillaGastos.AllowUserToAddRows = false;
            this.grillaGastos.AllowUserToDeleteRows = false;
            this.grillaGastos.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.grillaGastos.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
            this.grillaGastos.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.grillaGastos.Location = new System.Drawing.Point(2, 176);
            this.grillaGastos.Name = "grillaGastos";
            this.grillaGastos.ReadOnly = true;
            this.grillaGastos.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.AutoSizeToAllHeaders;
            this.grillaGastos.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.grillaGastos.Size = new System.Drawing.Size(539, 100);
            this.grillaGastos.TabIndex = 44;
            this.grillaGastos.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.grillaGastos_CellClick);
            // 
            // btnCancelar
            // 
            this.btnCancelar.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancelar.Location = new System.Drawing.Point(445, 282);
            this.btnCancelar.Name = "btnCancelar";
            this.btnCancelar.Size = new System.Drawing.Size(84, 26);
            this.btnCancelar.TabIndex = 45;
            this.btnCancelar.Text = "&Salir";
            this.btnCancelar.UseVisualStyleBackColor = true;
            this.btnCancelar.Click += new System.EventHandler(this.btnCancelar_Click);
            // 
            // txtDetalle
            // 
            this.txtDetalle.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.txtDetalle.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtDetalle.Location = new System.Drawing.Point(103, 106);
            this.txtDetalle.Multiline = true;
            this.txtDetalle.Name = "txtDetalle";
            this.txtDetalle.ReadOnly = true;
            this.txtDetalle.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtDetalle.Size = new System.Drawing.Size(401, 64);
            this.txtDetalle.TabIndex = 46;
            this.txtDetalle.TabStop = false;
            // 
            // lblMonto
            // 
            this.lblMonto.AutoSize = true;
            this.lblMonto.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblMonto.ForeColor = System.Drawing.Color.Cornsilk;
            this.lblMonto.Location = new System.Drawing.Point(52, 81);
            this.lblMonto.Name = "lblMonto";
            this.lblMonto.Size = new System.Drawing.Size(45, 16);
            this.lblMonto.TabIndex = 50;
            this.lblMonto.Text = "Monto";
            // 
            // txtMonto
            // 
            this.txtMonto.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
            this.txtMonto.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.CustomSource;
            this.txtMonto.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtMonto.Location = new System.Drawing.Point(103, 78);
            this.txtMonto.Name = "txtMonto";
            this.txtMonto.ReadOnly = true;
            this.txtMonto.Size = new System.Drawing.Size(158, 22);
            this.txtMonto.TabIndex = 48;
            this.txtMonto.TabStop = false;
            // 
            // lblDescripcion
            // 
            this.lblDescripcion.AutoSize = true;
            this.lblDescripcion.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblDescripcion.ForeColor = System.Drawing.Color.Cornsilk;
            this.lblDescripcion.Location = new System.Drawing.Point(17, 53);
            this.lblDescripcion.Name = "lblDescripcion";
            this.lblDescripcion.Size = new System.Drawing.Size(80, 16);
            this.lblDescripcion.TabIndex = 49;
            this.lblDescripcion.Text = "Descripción";
            // 
            // txtDescripcion
            // 
            this.txtDescripcion.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
            this.txtDescripcion.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.CustomSource;
            this.txtDescripcion.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtDescripcion.Location = new System.Drawing.Point(103, 50);
            this.txtDescripcion.Name = "txtDescripcion";
            this.txtDescripcion.ReadOnly = true;
            this.txtDescripcion.Size = new System.Drawing.Size(259, 22);
            this.txtDescripcion.TabIndex = 47;
            this.txtDescripcion.TabStop = false;
            // 
            // lblDetalle
            // 
            this.lblDetalle.AutoSize = true;
            this.lblDetalle.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblDetalle.ForeColor = System.Drawing.Color.Cornsilk;
            this.lblDetalle.Location = new System.Drawing.Point(46, 106);
            this.lblDetalle.Name = "lblDetalle";
            this.lblDetalle.Size = new System.Drawing.Size(51, 16);
            this.lblDetalle.TabIndex = 51;
            this.lblDetalle.Text = "Detalle";
            // 
            // txtTipoGasto
            // 
            this.txtTipoGasto.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtTipoGasto.Location = new System.Drawing.Point(103, 22);
            this.txtTipoGasto.Name = "txtTipoGasto";
            this.txtTipoGasto.ReadOnly = true;
            this.txtTipoGasto.Size = new System.Drawing.Size(158, 22);
            this.txtTipoGasto.TabIndex = 53;
            this.txtTipoGasto.TabStop = false;
            // 
            // lblTipo
            // 
            this.lblTipo.AutoSize = true;
            this.lblTipo.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTipo.ForeColor = System.Drawing.Color.Cornsilk;
            this.lblTipo.Location = new System.Drawing.Point(61, 26);
            this.lblTipo.Name = "lblTipo";
            this.lblTipo.Size = new System.Drawing.Size(36, 16);
            this.lblTipo.TabIndex = 52;
            this.lblTipo.Text = "Tipo";
            // 
            // txtFechaTexto
            // 
            this.txtFechaTexto.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtFechaTexto.Location = new System.Drawing.Point(368, 22);
            this.txtFechaTexto.Name = "txtFechaTexto";
            this.txtFechaTexto.ReadOnly = true;
            this.txtFechaTexto.Size = new System.Drawing.Size(161, 22);
            this.txtFechaTexto.TabIndex = 55;
            this.txtFechaTexto.TabStop = false;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.ForeColor = System.Drawing.Color.Cornsilk;
            this.label3.Location = new System.Drawing.Point(316, 24);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(46, 16);
            this.label3.TabIndex = 54;
            this.label3.Text = "Fecha";
            // 
            // formGastosVendedor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.SteelBlue;
            this.ClientSize = new System.Drawing.Size(541, 316);
            this.Controls.Add(this.txtFechaTexto);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.txtTipoGasto);
            this.Controls.Add(this.lblTipo);
            this.Controls.Add(this.lblDetalle);
            this.Controls.Add(this.lblMonto);
            this.Controls.Add(this.txtMonto);
            this.Controls.Add(this.lblDescripcion);
            this.Controls.Add(this.txtDescripcion);
            this.Controls.Add(this.txtDetalle);
            this.Controls.Add(this.btnCancelar);
            this.Controls.Add(this.grillaGastos);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.MinimizeBox = false;
            this.Name = "formGastosVendedor";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "formGastosVendedor";
            this.Load += new System.EventHandler(this.formGastosVendedor_Load);
            ((System.ComponentModel.ISupportInitialize)(this.grillaGastos)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        protected System.Windows.Forms.DataGridView grillaGastos;
        protected System.Windows.Forms.Button btnCancelar;
        private System.Windows.Forms.TextBox txtDetalle;
        protected System.Windows.Forms.Label lblMonto;
        protected System.Windows.Forms.TextBox txtMonto;
        protected System.Windows.Forms.Label lblDescripcion;
        protected System.Windows.Forms.TextBox txtDescripcion;
        protected System.Windows.Forms.Label lblDetalle;
        private System.Windows.Forms.TextBox txtTipoGasto;
        protected System.Windows.Forms.Label lblTipo;
        private System.Windows.Forms.TextBox txtFechaTexto;
        protected System.Windows.Forms.Label label3;
    }
}