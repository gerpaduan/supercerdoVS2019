namespace Presentacion.Personas
{
    partial class formBuscarPersona
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(formBuscarPersona));
            this.grillaPersonas = new System.Windows.Forms.DataGridView();
            this.idPersona = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.razonSocial = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.otroDatos = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.btnSeleccionar = new System.Windows.Forms.Button();
            this.btnCancelar = new System.Windows.Forms.Button();
            this.panel1 = new System.Windows.Forms.Panel();
            this.btnAgregar = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.btnBuscarProv = new System.Windows.Forms.Button();
            this.txtBuscar = new System.Windows.Forms.TextBox();
            this.Proveedor = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.grillaPersonas)).BeginInit();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // grillaPersonas
            // 
            this.grillaPersonas.AllowUserToAddRows = false;
            this.grillaPersonas.AllowUserToResizeRows = false;
            this.grillaPersonas.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.grillaPersonas.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.Raised;
            this.grillaPersonas.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.grillaPersonas.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.idPersona,
            this.razonSocial,
            this.otroDatos});
            this.grillaPersonas.Location = new System.Drawing.Point(10, 68);
            this.grillaPersonas.MultiSelect = false;
            this.grillaPersonas.Name = "grillaPersonas";
            this.grillaPersonas.ReadOnly = true;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.grillaPersonas.RowHeadersDefaultCellStyle = dataGridViewCellStyle2;
            this.grillaPersonas.RowHeadersVisible = false;
            this.grillaPersonas.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.grillaPersonas.Size = new System.Drawing.Size(316, 183);
            this.grillaPersonas.StandardTab = true;
            this.grillaPersonas.TabIndex = 4;
            this.grillaPersonas.TabStop = false;
            this.grillaPersonas.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.grillaPersonas_CellDoubleClick);
            // 
            // idPersona
            // 
            this.idPersona.DataPropertyName = "idPersona";
            this.idPersona.HeaderText = "ID Proveedor";
            this.idPersona.Name = "idPersona";
            this.idPersona.ReadOnly = true;
            this.idPersona.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.idPersona.Visible = false;
            this.idPersona.Width = 80;
            // 
            // razonSocial
            // 
            this.razonSocial.DataPropertyName = "razonSocial";
            this.razonSocial.HeaderText = "Razon Social";
            this.razonSocial.Name = "razonSocial";
            this.razonSocial.ReadOnly = true;
            this.razonSocial.Width = 314;
            // 
            // otroDatos
            // 
            this.otroDatos.DataPropertyName = "otrosDatos";
            this.otroDatos.HeaderText = "Otros Datos";
            this.otroDatos.Name = "otroDatos";
            this.otroDatos.ReadOnly = true;
            this.otroDatos.Visible = false;
            this.otroDatos.Width = 240;
            // 
            // btnSeleccionar
            // 
            this.btnSeleccionar.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.btnSeleccionar.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSeleccionar.Location = new System.Drawing.Point(139, 254);
            this.btnSeleccionar.Name = "btnSeleccionar";
            this.btnSeleccionar.Size = new System.Drawing.Size(91, 28);
            this.btnSeleccionar.TabIndex = 2;
            this.btnSeleccionar.TabStop = false;
            this.btnSeleccionar.Text = "&Seleccionar";
            this.btnSeleccionar.UseVisualStyleBackColor = true;
            this.btnSeleccionar.Click += new System.EventHandler(this.btnSeleccionar_Click);
            // 
            // btnCancelar
            // 
            this.btnCancelar.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.btnCancelar.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancelar.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnCancelar.Location = new System.Drawing.Point(236, 255);
            this.btnCancelar.Name = "btnCancelar";
            this.btnCancelar.Size = new System.Drawing.Size(91, 27);
            this.btnCancelar.TabIndex = 3;
            this.btnCancelar.TabStop = false;
            this.btnCancelar.Text = "&Cerrar";
            this.btnCancelar.UseVisualStyleBackColor = true;
            this.btnCancelar.Click += new System.EventHandler(this.btnCancelar_Click);
            // 
            // panel1
            // 
            this.panel1.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.panel1.BackColor = System.Drawing.Color.SteelBlue;
            this.panel1.Controls.Add(this.btnAgregar);
            this.panel1.Controls.Add(this.groupBox2);
            this.panel1.Controls.Add(this.btnBuscarProv);
            this.panel1.Controls.Add(this.txtBuscar);
            this.panel1.Controls.Add(this.Proveedor);
            this.panel1.Location = new System.Drawing.Point(-6, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(349, 62);
            this.panel1.TabIndex = 40;
            // 
            // btnAgregar
            // 
            this.btnAgregar.Image = ((System.Drawing.Image)(resources.GetObject("btnAgregar.Image")));
            this.btnAgregar.Location = new System.Drawing.Point(297, 18);
            this.btnAgregar.Name = "btnAgregar";
            this.btnAgregar.Size = new System.Drawing.Size(33, 24);
            this.btnAgregar.TabIndex = 48;
            this.btnAgregar.UseVisualStyleBackColor = true;
            this.btnAgregar.Click += new System.EventHandler(this.btnAgregar_Click);
            // 
            // groupBox2
            // 
            this.groupBox2.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.groupBox2.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.groupBox2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.groupBox2.Location = new System.Drawing.Point(16, 46);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(314, 7);
            this.groupBox2.TabIndex = 47;
            this.groupBox2.TabStop = false;
            // 
            // btnBuscarProv
            // 
            this.btnBuscarProv.AccessibleDescription = "";
            this.btnBuscarProv.ForeColor = System.Drawing.Color.Black;
            this.btnBuscarProv.Image = ((System.Drawing.Image)(resources.GetObject("btnBuscarProv.Image")));
            this.btnBuscarProv.Location = new System.Drawing.Point(232, 19);
            this.btnBuscarProv.Name = "btnBuscarProv";
            this.btnBuscarProv.Size = new System.Drawing.Size(28, 23);
            this.btnBuscarProv.TabIndex = 14;
            this.btnBuscarProv.TabStop = false;
            this.btnBuscarProv.UseVisualStyleBackColor = true;
            this.btnBuscarProv.Click += new System.EventHandler(this.btnBuscarProv_Click);
            // 
            // txtBuscar
            // 
            this.txtBuscar.Location = new System.Drawing.Point(89, 20);
            this.txtBuscar.Name = "txtBuscar";
            this.txtBuscar.Size = new System.Drawing.Size(137, 20);
            this.txtBuscar.TabIndex = 1;
            this.txtBuscar.TextChanged += new System.EventHandler(this.btnBuscarProv_Click);
            this.txtBuscar.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.TxtPruebaENTER_KeyPress);
            // 
            // Proveedor
            // 
            this.Proveedor.AutoSize = true;
            this.Proveedor.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Proveedor.ForeColor = System.Drawing.Color.Cornsilk;
            this.Proveedor.Location = new System.Drawing.Point(13, 22);
            this.Proveedor.Name = "Proveedor";
            this.Proveedor.Size = new System.Drawing.Size(72, 15);
            this.Proveedor.TabIndex = 2;
            this.Proveedor.Text = "Descripción";
            // 
            // formBuscarPersona
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Bisque;
            this.CancelButton = this.btnCancelar;
            this.ClientSize = new System.Drawing.Size(336, 286);
            this.Controls.Add(this.grillaPersonas);
            this.Controls.Add(this.btnSeleccionar);
            this.Controls.Add(this.btnCancelar);
            this.Controls.Add(this.panel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "formBuscarPersona";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Buscar Personas";
            this.Load += new System.EventHandler(this.formBuscarPersona_Load);
            this.Activated += new System.EventHandler(this.formBuscarPersona_Activated);
            ((System.ComponentModel.ISupportInitialize)(this.grillaPersonas)).EndInit();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        protected System.Windows.Forms.DataGridView grillaPersonas;
        private System.Windows.Forms.DataGridViewTextBoxColumn idPersona;
        private System.Windows.Forms.DataGridViewTextBoxColumn razonSocial;
        private System.Windows.Forms.DataGridViewTextBoxColumn otroDatos;
        protected System.Windows.Forms.Button btnSeleccionar;
        protected System.Windows.Forms.Button btnCancelar;
        protected System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button btnBuscarProv;
        protected System.Windows.Forms.TextBox txtBuscar;
        protected System.Windows.Forms.Label Proveedor;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button btnAgregar;
    }
}