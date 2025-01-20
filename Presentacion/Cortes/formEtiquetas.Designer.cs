namespace Presentacion.Cortes
{
    partial class formEtiquetas
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle15 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle11 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle12 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle13 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle14 = new System.Windows.Forms.DataGridViewCellStyle();
            this.grillaCortes = new System.Windows.Forms.DataGridView();
            this.idCorte = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.seleccionar = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.codigo = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.corte = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.precioKg = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.btnBuscar = new System.Windows.Forms.Button();
            this.label9 = new System.Windows.Forms.Label();
            this.txtBuscarCorte = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.txtCantItems = new System.Windows.Forms.TextBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.btnGenerarEtiquetas = new System.Windows.Forms.Button();
            this.btnQuitarTodos = new System.Windows.Forms.Button();
            this.btnSeleccionarTodos = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.grillaCortes)).BeginInit();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // grillaCortes
            // 
            this.grillaCortes.AllowUserToAddRows = false;
            this.grillaCortes.AllowUserToDeleteRows = false;
            this.grillaCortes.AllowUserToResizeRows = false;
            this.grillaCortes.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.grillaCortes.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.grillaCortes.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.Raised;
            this.grillaCortes.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.grillaCortes.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.idCorte,
            this.seleccionar,
            this.codigo,
            this.corte,
            this.precioKg});
            dataGridViewCellStyle15.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle15.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle15.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle15.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle15.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle15.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle15.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.grillaCortes.DefaultCellStyle = dataGridViewCellStyle15;
            this.grillaCortes.Location = new System.Drawing.Point(10, 121);
            this.grillaCortes.MultiSelect = false;
            this.grillaCortes.Name = "grillaCortes";
            this.grillaCortes.ReadOnly = true;
            this.grillaCortes.RowHeadersVisible = false;
            this.grillaCortes.RowHeadersWidth = 51;
            this.grillaCortes.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.grillaCortes.Size = new System.Drawing.Size(689, 484);
            this.grillaCortes.StandardTab = true;
            this.grillaCortes.TabIndex = 8;
            this.grillaCortes.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.grillaCortes_CellContentClick);
            this.grillaCortes.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.grillaCortes_CellDoubleClick);
            // 
            // idCorte
            // 
            this.idCorte.DataPropertyName = "idCorte";
            dataGridViewCellStyle11.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            this.idCorte.DefaultCellStyle = dataGridViewCellStyle11;
            this.idCorte.HeaderText = "ID Corte";
            this.idCorte.MinimumWidth = 6;
            this.idCorte.Name = "idCorte";
            this.idCorte.ReadOnly = true;
            this.idCorte.Visible = false;
            // 
            // seleccionar
            // 
            this.seleccionar.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.ColumnHeader;
            this.seleccionar.HeaderText = "Seleccionar";
            this.seleccionar.MinimumWidth = 6;
            this.seleccionar.Name = "seleccionar";
            this.seleccionar.ReadOnly = true;
            this.seleccionar.Width = 69;
            // 
            // codigo
            // 
            this.codigo.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.ColumnHeader;
            this.codigo.DataPropertyName = "codigo";
            dataGridViewCellStyle12.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            this.codigo.DefaultCellStyle = dataGridViewCellStyle12;
            this.codigo.HeaderText = "Código";
            this.codigo.MinimumWidth = 6;
            this.codigo.Name = "codigo";
            this.codigo.ReadOnly = true;
            this.codigo.Width = 65;
            // 
            // corte
            // 
            this.corte.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.corte.DataPropertyName = "corte";
            dataGridViewCellStyle13.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.corte.DefaultCellStyle = dataGridViewCellStyle13;
            this.corte.HeaderText = "Corte";
            this.corte.MinimumWidth = 6;
            this.corte.Name = "corte";
            this.corte.ReadOnly = true;
            this.corte.Width = 57;
            // 
            // precioKg
            // 
            this.precioKg.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.precioKg.DataPropertyName = "precioKg";
            dataGridViewCellStyle14.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
            dataGridViewCellStyle14.Format = "N2";
            dataGridViewCellStyle14.NullValue = null;
            this.precioKg.DefaultCellStyle = dataGridViewCellStyle14;
            this.precioKg.HeaderText = "Precio Kg.";
            this.precioKg.MinimumWidth = 6;
            this.precioKg.Name = "precioKg";
            this.precioKg.ReadOnly = true;
            this.precioKg.Width = 81;
            // 
            // btnBuscar
            // 
            this.btnBuscar.ForeColor = System.Drawing.Color.Black;
            this.btnBuscar.Location = new System.Drawing.Point(257, 73);
            this.btnBuscar.Name = "btnBuscar";
            this.btnBuscar.Size = new System.Drawing.Size(67, 25);
            this.btnBuscar.TabIndex = 1;
            this.btnBuscar.Text = "Buscar";
            this.btnBuscar.UseVisualStyleBackColor = true;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label9.ForeColor = System.Drawing.Color.Cornsilk;
            this.label9.Location = new System.Drawing.Point(7, 76);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(84, 15);
            this.label9.TabIndex = 2;
            this.label9.Text = "Codigo / Corte";
            // 
            // txtBuscarCorte
            // 
            this.txtBuscarCorte.Location = new System.Drawing.Point(97, 76);
            this.txtBuscarCorte.Name = "txtBuscarCorte";
            this.txtBuscarCorte.Size = new System.Drawing.Size(154, 20);
            this.txtBuscarCorte.TabIndex = 0;
            this.txtBuscarCorte.TextChanged += new System.EventHandler(this.txtBuscarCorte_TextChanged);
            // 
            // label5
            // 
            this.label5.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.ForeColor = System.Drawing.Color.Cornsilk;
            this.label5.Location = new System.Drawing.Point(481, 9);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(121, 15);
            this.label5.TabIndex = 68;
            this.label5.Text = "Ítems Seleccionados";
            // 
            // txtCantItems
            // 
            this.txtCantItems.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.txtCantItems.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtCantItems.Location = new System.Drawing.Point(608, 8);
            this.txtCantItems.Name = "txtCantItems";
            this.txtCantItems.ReadOnly = true;
            this.txtCantItems.Size = new System.Drawing.Size(89, 19);
            this.txtCantItems.TabIndex = 67;
            this.txtCantItems.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // panel1
            // 
            this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(43)))), ((int)(((byte)(77)))), ((int)(((byte)(129)))));
            this.panel1.Controls.Add(this.btnGenerarEtiquetas);
            this.panel1.Controls.Add(this.btnQuitarTodos);
            this.panel1.Controls.Add(this.btnSeleccionarTodos);
            this.panel1.Controls.Add(this.txtCantItems);
            this.panel1.Controls.Add(this.label5);
            this.panel1.Controls.Add(this.txtBuscarCorte);
            this.panel1.Controls.Add(this.label9);
            this.panel1.Controls.Add(this.btnBuscar);
            this.panel1.Location = new System.Drawing.Point(2, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(707, 101);
            this.panel1.TabIndex = 9;
            // 
            // btnGenerarEtiquetas
            // 
            this.btnGenerarEtiquetas.ForeColor = System.Drawing.Color.Black;
            this.btnGenerarEtiquetas.Location = new System.Drawing.Point(10, 9);
            this.btnGenerarEtiquetas.Name = "btnGenerarEtiquetas";
            this.btnGenerarEtiquetas.Size = new System.Drawing.Size(104, 25);
            this.btnGenerarEtiquetas.TabIndex = 71;
            this.btnGenerarEtiquetas.Text = "Generar Etiquetas";
            this.btnGenerarEtiquetas.UseVisualStyleBackColor = true;
            this.btnGenerarEtiquetas.Click += new System.EventHandler(this.button1_Click);
            // 
            // btnQuitarTodos
            // 
            this.btnQuitarTodos.ForeColor = System.Drawing.Color.Black;
            this.btnQuitarTodos.Location = new System.Drawing.Point(591, 73);
            this.btnQuitarTodos.Name = "btnQuitarTodos";
            this.btnQuitarTodos.Size = new System.Drawing.Size(104, 25);
            this.btnQuitarTodos.TabIndex = 70;
            this.btnQuitarTodos.Text = "Quitar Todos";
            this.btnQuitarTodos.UseVisualStyleBackColor = true;
            this.btnQuitarTodos.Click += new System.EventHandler(this.btnQuitarTodos_Click);
            // 
            // btnSeleccionarTodos
            // 
            this.btnSeleccionarTodos.ForeColor = System.Drawing.Color.Black;
            this.btnSeleccionarTodos.Location = new System.Drawing.Point(458, 72);
            this.btnSeleccionarTodos.Name = "btnSeleccionarTodos";
            this.btnSeleccionarTodos.Size = new System.Drawing.Size(127, 25);
            this.btnSeleccionarTodos.TabIndex = 69;
            this.btnSeleccionarTodos.Text = "Selecccionar Todos";
            this.btnSeleccionarTodos.UseVisualStyleBackColor = true;
            this.btnSeleccionarTodos.Click += new System.EventHandler(this.btnSeleccionarTodos_Click);
            // 
            // formEtiquetas
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(709, 616);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.grillaCortes);
            this.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.Name = "formEtiquetas";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Etiquetas";
            this.Load += new System.EventHandler(this.formEtiquetas_Load);
            ((System.ComponentModel.ISupportInitialize)(this.grillaCortes)).EndInit();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
        protected System.Windows.Forms.DataGridView grillaCortes;
        protected System.Windows.Forms.Button btnBuscar;
        protected System.Windows.Forms.Label label9;
        protected System.Windows.Forms.TextBox txtBuscarCorte;
        protected System.Windows.Forms.Label label5;
        protected System.Windows.Forms.TextBox txtCantItems;
        protected System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.DataGridViewTextBoxColumn idCorte;
        private System.Windows.Forms.DataGridViewCheckBoxColumn seleccionar;
        private System.Windows.Forms.DataGridViewTextBoxColumn codigo;
        private System.Windows.Forms.DataGridViewTextBoxColumn corte;
        private System.Windows.Forms.DataGridViewTextBoxColumn precioKg;
        protected System.Windows.Forms.Button btnQuitarTodos;
        protected System.Windows.Forms.Button btnSeleccionarTodos;
        protected System.Windows.Forms.Button btnGenerarEtiquetas;
    }
}