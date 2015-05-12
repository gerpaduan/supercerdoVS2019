namespace Presentacion.Embutidos
{
    partial class formBuscarEmbutido
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(formBuscarEmbutido));
            this.grillaCortes = new System.Windows.Forms.DataGridView();
            this.idCorte = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Codigo = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.corte = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.tipo = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.idCorteMaestro = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.corteMaestro = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.porcentaje = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.idSucursalSL = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.sucursalSL = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.stockSL = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.idSucursalSM = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.sucursalSM = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.stockSM = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.panel1 = new System.Windows.Forms.Panel();
            this.btnBuscarCorte = new System.Windows.Forms.Button();
            this.txtBuscarCorte = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.btnSeleccionar = new System.Windows.Forms.Button();
            this.btnCancelar = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            ((System.ComponentModel.ISupportInitialize)(this.grillaCortes)).BeginInit();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // grillaCortes
            // 
            this.grillaCortes.AllowUserToAddRows = false;
            this.grillaCortes.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.grillaCortes.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.Raised;
            this.grillaCortes.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.grillaCortes.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.idCorte,
            this.Codigo,
            this.corte,
            this.tipo,
            this.idCorteMaestro,
            this.corteMaestro,
            this.porcentaje,
            this.idSucursalSL,
            this.sucursalSL,
            this.stockSL,
            this.idSucursalSM,
            this.sucursalSM,
            this.stockSM});
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.grillaCortes.DefaultCellStyle = dataGridViewCellStyle3;
            this.grillaCortes.Location = new System.Drawing.Point(10, 64);
            this.grillaCortes.MultiSelect = false;
            this.grillaCortes.Name = "grillaCortes";
            this.grillaCortes.ReadOnly = true;
            this.grillaCortes.RowHeadersVisible = false;
            this.grillaCortes.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.grillaCortes.Size = new System.Drawing.Size(420, 185);
            this.grillaCortes.StandardTab = true;
            this.grillaCortes.TabIndex = 19;
            this.grillaCortes.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.grillaCortes_CellDoubleClick);
            // 
            // idCorte
            // 
            this.idCorte.DataPropertyName = "idCorte";
            this.idCorte.HeaderText = "ID Corte";
            this.idCorte.Name = "idCorte";
            this.idCorte.ReadOnly = true;
            this.idCorte.Visible = false;
            // 
            // Codigo
            // 
            this.Codigo.DataPropertyName = "Codigo";
            this.Codigo.HeaderText = "Código";
            this.Codigo.Name = "Codigo";
            this.Codigo.ReadOnly = true;
            this.Codigo.Width = 70;
            // 
            // corte
            // 
            this.corte.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.corte.DataPropertyName = "corte";
            this.corte.HeaderText = "Corte";
            this.corte.Name = "corte";
            this.corte.ReadOnly = true;
            // 
            // tipo
            // 
            this.tipo.DataPropertyName = "tipo";
            this.tipo.HeaderText = "Tipo";
            this.tipo.Name = "tipo";
            this.tipo.ReadOnly = true;
            this.tipo.Visible = false;
            // 
            // idCorteMaestro
            // 
            this.idCorteMaestro.DataPropertyName = "idCorteMaestro";
            this.idCorteMaestro.HeaderText = "ID Codigo Maestro";
            this.idCorteMaestro.Name = "idCorteMaestro";
            this.idCorteMaestro.ReadOnly = true;
            this.idCorteMaestro.Visible = false;
            // 
            // corteMaestro
            // 
            this.corteMaestro.DataPropertyName = "corte";
            this.corteMaestro.HeaderText = "Codigo Maestro";
            this.corteMaestro.Name = "corteMaestro";
            this.corteMaestro.ReadOnly = true;
            this.corteMaestro.Visible = false;
            // 
            // porcentaje
            // 
            this.porcentaje.DataPropertyName = "porcentaje";
            this.porcentaje.HeaderText = "Porcentaje";
            this.porcentaje.Name = "porcentaje";
            this.porcentaje.ReadOnly = true;
            this.porcentaje.Visible = false;
            this.porcentaje.Width = 70;
            // 
            // idSucursalSL
            // 
            this.idSucursalSL.DataPropertyName = "idSucursalSL";
            this.idSucursalSL.HeaderText = "ID Sucursal SL";
            this.idSucursalSL.Name = "idSucursalSL";
            this.idSucursalSL.ReadOnly = true;
            this.idSucursalSL.Visible = false;
            // 
            // sucursalSL
            // 
            this.sucursalSL.DataPropertyName = "sucursalSL";
            this.sucursalSL.HeaderText = "Sucursal SL";
            this.sucursalSL.Name = "sucursalSL";
            this.sucursalSL.ReadOnly = true;
            this.sucursalSL.Visible = false;
            // 
            // stockSL
            // 
            this.stockSL.DataPropertyName = "stockSL";
            dataGridViewCellStyle1.Format = "N3";
            dataGridViewCellStyle1.NullValue = null;
            this.stockSL.DefaultCellStyle = dataGridViewCellStyle1;
            this.stockSL.HeaderText = "Stock S. Lorenzo";
            this.stockSL.Name = "stockSL";
            this.stockSL.ReadOnly = true;
            this.stockSL.Visible = false;
            this.stockSL.Width = 115;
            // 
            // idSucursalSM
            // 
            this.idSucursalSM.DataPropertyName = "idSucursalSM";
            this.idSucursalSM.HeaderText = "ID Sucursal SM";
            this.idSucursalSM.Name = "idSucursalSM";
            this.idSucursalSM.ReadOnly = true;
            this.idSucursalSM.Visible = false;
            // 
            // sucursalSM
            // 
            this.sucursalSM.DataPropertyName = "sucursalSM";
            this.sucursalSM.HeaderText = "Sucursal SM";
            this.sucursalSM.Name = "sucursalSM";
            this.sucursalSM.ReadOnly = true;
            this.sucursalSM.Visible = false;
            // 
            // stockSM
            // 
            this.stockSM.DataPropertyName = "stockSM";
            dataGridViewCellStyle2.Format = "N3";
            dataGridViewCellStyle2.NullValue = null;
            this.stockSM.DefaultCellStyle = dataGridViewCellStyle2;
            this.stockSM.HeaderText = "Stock S. Martín";
            this.stockSM.Name = "stockSM";
            this.stockSM.ReadOnly = true;
            this.stockSM.Visible = false;
            this.stockSM.Width = 115;
            // 
            // panel1
            // 
            this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.panel1.BackColor = System.Drawing.Color.SteelBlue;
            this.panel1.Controls.Add(this.groupBox1);
            this.panel1.Controls.Add(this.btnBuscarCorte);
            this.panel1.Controls.Add(this.txtBuscarCorte);
            this.panel1.Controls.Add(this.label9);
            this.panel1.Location = new System.Drawing.Point(-1, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(443, 58);
            this.panel1.TabIndex = 22;
            // 
            // btnBuscarCorte
            // 
            this.btnBuscarCorte.AccessibleDescription = "";
            this.btnBuscarCorte.ForeColor = System.Drawing.Color.Black;
            this.btnBuscarCorte.Image = ((System.Drawing.Image)(resources.GetObject("btnBuscarCorte.Image")));
            this.btnBuscarCorte.Location = new System.Drawing.Point(233, 20);
            this.btnBuscarCorte.Name = "btnBuscarCorte";
            this.btnBuscarCorte.Size = new System.Drawing.Size(28, 23);
            this.btnBuscarCorte.TabIndex = 15;
            this.btnBuscarCorte.TabStop = false;
            this.btnBuscarCorte.UseVisualStyleBackColor = true;
            this.btnBuscarCorte.Click += new System.EventHandler(this.btnBuscarCorte_Click);
            // 
            // txtBuscarCorte
            // 
            this.txtBuscarCorte.Location = new System.Drawing.Point(90, 21);
            this.txtBuscarCorte.Name = "txtBuscarCorte";
            this.txtBuscarCorte.Size = new System.Drawing.Size(137, 20);
            this.txtBuscarCorte.TabIndex = 0;
            this.txtBuscarCorte.TextChanged += new System.EventHandler(this.btnBuscarCorte_Click);
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label9.ForeColor = System.Drawing.Color.Cornsilk;
            this.label9.Location = new System.Drawing.Point(12, 22);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(72, 15);
            this.label9.TabIndex = 2;
            this.label9.Text = "Descripción";
            // 
            // btnSeleccionar
            // 
            this.btnSeleccionar.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSeleccionar.Location = new System.Drawing.Point(254, 255);
            this.btnSeleccionar.Name = "btnSeleccionar";
            this.btnSeleccionar.Size = new System.Drawing.Size(84, 27);
            this.btnSeleccionar.TabIndex = 20;
            this.btnSeleccionar.Text = "Seleccionar";
            this.btnSeleccionar.UseVisualStyleBackColor = true;
            this.btnSeleccionar.Click += new System.EventHandler(this.btnSeleccionar_Click);
            // 
            // btnCancelar
            // 
            this.btnCancelar.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancelar.Location = new System.Drawing.Point(344, 255);
            this.btnCancelar.Name = "btnCancelar";
            this.btnCancelar.Size = new System.Drawing.Size(84, 27);
            this.btnCancelar.TabIndex = 21;
            this.btnCancelar.Text = "Cancelar";
            this.btnCancelar.UseVisualStyleBackColor = true;
            this.btnCancelar.Click += new System.EventHandler(this.btnCancelar_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.groupBox1.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.groupBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.groupBox1.Location = new System.Drawing.Point(13, 41);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(418, 7);
            this.groupBox1.TabIndex = 26;
            this.groupBox1.TabStop = false;
            // 
            // formBuscarEmbutido
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Bisque;
            this.ClientSize = new System.Drawing.Size(440, 288);
            this.Controls.Add(this.grillaCortes);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.btnSeleccionar);
            this.Controls.Add(this.btnCancelar);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "formBuscarEmbutido";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Buscar Embutido / Otro";
            this.Load += new System.EventHandler(this.formBuscarEmbutido_Load);
            ((System.ComponentModel.ISupportInitialize)(this.grillaCortes)).EndInit();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        protected System.Windows.Forms.DataGridView grillaCortes;
        protected System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button btnBuscarCorte;
        protected System.Windows.Forms.TextBox txtBuscarCorte;
        protected System.Windows.Forms.Label label9;
        protected System.Windows.Forms.Button btnSeleccionar;
        protected System.Windows.Forms.Button btnCancelar;
        private System.Windows.Forms.DataGridViewTextBoxColumn idCorte;
        private System.Windows.Forms.DataGridViewTextBoxColumn Codigo;
        private System.Windows.Forms.DataGridViewTextBoxColumn corte;
        private System.Windows.Forms.DataGridViewTextBoxColumn tipo;
        private System.Windows.Forms.DataGridViewTextBoxColumn idCorteMaestro;
        private System.Windows.Forms.DataGridViewTextBoxColumn corteMaestro;
        private System.Windows.Forms.DataGridViewTextBoxColumn porcentaje;
        private System.Windows.Forms.DataGridViewTextBoxColumn idSucursalSL;
        private System.Windows.Forms.DataGridViewTextBoxColumn sucursalSL;
        private System.Windows.Forms.DataGridViewTextBoxColumn stockSL;
        private System.Windows.Forms.DataGridViewTextBoxColumn idSucursalSM;
        private System.Windows.Forms.DataGridViewTextBoxColumn sucursalSM;
        private System.Windows.Forms.DataGridViewTextBoxColumn stockSM;
        private System.Windows.Forms.GroupBox groupBox1;

    }
}