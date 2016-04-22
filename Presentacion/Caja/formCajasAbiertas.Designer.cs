namespace Presentacion.Caja
{
    partial class formCajasAbiertas
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
            this.grillaCajasAbiertas = new System.Windows.Forms.DataGridView();
            this.id = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.usuarioInicio = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.vendedor = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.fechaHoraInicio = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.cajaInicio = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.cerrarCaja = new System.Windows.Forms.DataGridViewButtonColumn();
            this.cajero = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.panel1 = new System.Windows.Forms.Panel();
            this.checkCajasMultiple = new System.Windows.Forms.CheckBox();
            this.comboSucursal = new System.Windows.Forms.ComboBox();
            this.label7 = new System.Windows.Forms.Label();
            this.txtBuscar = new System.Windows.Forms.TextBox();
            this.Proveedor = new System.Windows.Forms.Label();
            this.btnBuscar = new System.Windows.Forms.Button();
            this.btnCancelar = new System.Windows.Forms.Button();
            this.btnCerrarMultipleCajas = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.grillaCajasAbiertas)).BeginInit();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // grillaCajasAbiertas
            // 
            this.grillaCajasAbiertas.AllowUserToAddRows = false;
            this.grillaCajasAbiertas.AllowUserToDeleteRows = false;
            this.grillaCajasAbiertas.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.grillaCajasAbiertas.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.grillaCajasAbiertas.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.grillaCajasAbiertas.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.id,
            this.usuarioInicio,
            this.vendedor,
            this.fechaHoraInicio,
            this.cajaInicio,
            this.cerrarCaja,
            this.cajero});
            this.grillaCajasAbiertas.Location = new System.Drawing.Point(12, 71);
            this.grillaCajasAbiertas.MultiSelect = false;
            this.grillaCajasAbiertas.Name = "grillaCajasAbiertas";
            this.grillaCajasAbiertas.ReadOnly = true;
            this.grillaCajasAbiertas.RowHeadersVisible = false;
            this.grillaCajasAbiertas.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.AutoSizeToAllHeaders;
            this.grillaCajasAbiertas.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.grillaCajasAbiertas.Size = new System.Drawing.Size(667, 262);
            this.grillaCajasAbiertas.TabIndex = 1;
            this.grillaCajasAbiertas.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.grillaCajasAbiertas_CellClick);
            this.grillaCajasAbiertas.KeyDown += new System.Windows.Forms.KeyEventHandler(this.grillaCajasAbiertas_KeyDown);
            // 
            // id
            // 
            this.id.DataPropertyName = "id";
            this.id.HeaderText = "Id";
            this.id.Name = "id";
            this.id.ReadOnly = true;
            // 
            // usuarioInicio
            // 
            this.usuarioInicio.DataPropertyName = "usuarioInicio";
            this.usuarioInicio.HeaderText = "usuarioInicio";
            this.usuarioInicio.Name = "usuarioInicio";
            this.usuarioInicio.ReadOnly = true;
            this.usuarioInicio.Visible = false;
            // 
            // vendedor
            // 
            this.vendedor.DataPropertyName = "vendedor";
            this.vendedor.HeaderText = "Vendedor";
            this.vendedor.Name = "vendedor";
            this.vendedor.ReadOnly = true;
            // 
            // fechaHoraInicio
            // 
            this.fechaHoraInicio.DataPropertyName = "fechaHoraInicio";
            this.fechaHoraInicio.HeaderText = "Fecha Apertura Caja";
            this.fechaHoraInicio.Name = "fechaHoraInicio";
            this.fechaHoraInicio.ReadOnly = true;
            // 
            // cajaInicio
            // 
            this.cajaInicio.DataPropertyName = "cajaInicio";
            this.cajaInicio.HeaderText = "Caja Inicial";
            this.cajaInicio.Name = "cajaInicio";
            this.cajaInicio.ReadOnly = true;
            // 
            // cerrarCaja
            // 
            this.cerrarCaja.HeaderText = "Cerrar Caja";
            this.cerrarCaja.Name = "cerrarCaja";
            this.cerrarCaja.ReadOnly = true;
            this.cerrarCaja.Text = "Cerrar Caja";
            this.cerrarCaja.ToolTipText = "Cerrar Caja";
            this.cerrarCaja.UseColumnTextForButtonValue = true;
            // 
            // cajero
            // 
            this.cajero.FalseValue = "false";
            this.cajero.HeaderText = "Cajero";
            this.cajero.Name = "cajero";
            this.cajero.ReadOnly = true;
            this.cajero.TrueValue = "true";
            this.cajero.Visible = false;
            // 
            // panel1
            // 
            this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.panel1.BackColor = System.Drawing.Color.SteelBlue;
            this.panel1.Controls.Add(this.checkCajasMultiple);
            this.panel1.Controls.Add(this.comboSucursal);
            this.panel1.Controls.Add(this.label7);
            this.panel1.Controls.Add(this.txtBuscar);
            this.panel1.Controls.Add(this.Proveedor);
            this.panel1.Controls.Add(this.btnBuscar);
            this.panel1.Location = new System.Drawing.Point(-1, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(692, 67);
            this.panel1.TabIndex = 23;
            // 
            // checkCajasMultiple
            // 
            this.checkCajasMultiple.AutoSize = true;
            this.checkCajasMultiple.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.checkCajasMultiple.ForeColor = System.Drawing.Color.Cornsilk;
            this.checkCajasMultiple.Location = new System.Drawing.Point(535, 39);
            this.checkCajasMultiple.Name = "checkCajasMultiple";
            this.checkCajasMultiple.Size = new System.Drawing.Size(145, 19);
            this.checkCajasMultiple.TabIndex = 41;
            this.checkCajasMultiple.Text = "Cerrar multiples cajas";
            this.checkCajasMultiple.UseVisualStyleBackColor = true;
            this.checkCajasMultiple.CheckedChanged += new System.EventHandler(this.checkCajasMultiple_CheckedChanged);
            // 
            // comboSucursal
            // 
            this.comboSucursal.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboSucursal.FormattingEnabled = true;
            this.comboSucursal.Location = new System.Drawing.Point(72, 11);
            this.comboSucursal.Name = "comboSucursal";
            this.comboSucursal.Size = new System.Drawing.Size(128, 21);
            this.comboSucursal.TabIndex = 3;
            this.comboSucursal.TabStop = false;
            this.comboSucursal.SelectedIndexChanged += new System.EventHandler(this.comboSucursal_SelectedIndexChanged);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label7.ForeColor = System.Drawing.Color.Cornsilk;
            this.label7.Location = new System.Drawing.Point(11, 15);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(55, 15);
            this.label7.TabIndex = 40;
            this.label7.Text = "Sucursal";
            // 
            // txtBuscar
            // 
            this.txtBuscar.Location = new System.Drawing.Point(72, 38);
            this.txtBuscar.Name = "txtBuscar";
            this.txtBuscar.Size = new System.Drawing.Size(174, 20);
            this.txtBuscar.TabIndex = 4;
            this.txtBuscar.TextChanged += new System.EventHandler(this.txtBuscar_TextChanged);
            // 
            // Proveedor
            // 
            this.Proveedor.AutoSize = true;
            this.Proveedor.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Proveedor.ForeColor = System.Drawing.Color.Cornsilk;
            this.Proveedor.Location = new System.Drawing.Point(22, 39);
            this.Proveedor.Name = "Proveedor";
            this.Proveedor.Size = new System.Drawing.Size(45, 15);
            this.Proveedor.TabIndex = 2;
            this.Proveedor.Text = "Buscar";
            // 
            // btnBuscar
            // 
            this.btnBuscar.ForeColor = System.Drawing.Color.Black;
            this.btnBuscar.Location = new System.Drawing.Point(256, 35);
            this.btnBuscar.Name = "btnBuscar";
            this.btnBuscar.Size = new System.Drawing.Size(67, 25);
            this.btnBuscar.TabIndex = 5;
            this.btnBuscar.Text = "&Buscar";
            this.btnBuscar.UseVisualStyleBackColor = true;
            this.btnBuscar.Click += new System.EventHandler(this.btnBuscar_Click);
            // 
            // btnCancelar
            // 
            this.btnCancelar.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancelar.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnCancelar.Location = new System.Drawing.Point(561, 341);
            this.btnCancelar.Name = "btnCancelar";
            this.btnCancelar.Size = new System.Drawing.Size(118, 28);
            this.btnCancelar.TabIndex = 2;
            this.btnCancelar.Text = "&Salir";
            this.btnCancelar.UseVisualStyleBackColor = true;
            this.btnCancelar.Click += new System.EventHandler(this.btnCancelar_Click);
            // 
            // btnCerrarMultipleCajas
            // 
            this.btnCerrarMultipleCajas.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCerrarMultipleCajas.Enabled = false;
            this.btnCerrarMultipleCajas.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnCerrarMultipleCajas.Location = new System.Drawing.Point(386, 341);
            this.btnCerrarMultipleCajas.Name = "btnCerrarMultipleCajas";
            this.btnCerrarMultipleCajas.Size = new System.Drawing.Size(169, 28);
            this.btnCerrarMultipleCajas.TabIndex = 24;
            this.btnCerrarMultipleCajas.Text = "&Cerrar Cajas Múltiples";
            this.btnCerrarMultipleCajas.UseVisualStyleBackColor = true;
            this.btnCerrarMultipleCajas.Click += new System.EventHandler(this.btnCerrarMultipleCajas_Click);
            // 
            // formCajasAbiertas
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Bisque;
            this.ClientSize = new System.Drawing.Size(691, 374);
            this.Controls.Add(this.btnCerrarMultipleCajas);
            this.Controls.Add(this.btnCancelar);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.grillaCajasAbiertas);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "formCajasAbiertas";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Cajas Abiertas";
            this.Load += new System.EventHandler(this.formCajasAbiertas_Load);
            ((System.ComponentModel.ISupportInitialize)(this.grillaCajasAbiertas)).EndInit();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        protected System.Windows.Forms.DataGridView grillaCajasAbiertas;
        protected System.Windows.Forms.Panel panel1;
        protected System.Windows.Forms.TextBox txtBuscar;
        protected System.Windows.Forms.Label Proveedor;
        protected System.Windows.Forms.Button btnBuscar;
        protected System.Windows.Forms.Button btnCancelar;
        private System.Windows.Forms.ComboBox comboSucursal;
        protected System.Windows.Forms.Label label7;
        private System.Windows.Forms.CheckBox checkCajasMultiple;
        protected System.Windows.Forms.Button btnCerrarMultipleCajas;
        private System.Windows.Forms.DataGridViewTextBoxColumn id;
        private System.Windows.Forms.DataGridViewTextBoxColumn usuarioInicio;
        private System.Windows.Forms.DataGridViewTextBoxColumn vendedor;
        private System.Windows.Forms.DataGridViewTextBoxColumn fechaHoraInicio;
        private System.Windows.Forms.DataGridViewTextBoxColumn cajaInicio;
        private System.Windows.Forms.DataGridViewButtonColumn cerrarCaja;
        private System.Windows.Forms.DataGridViewCheckBoxColumn cajero;
    }
}