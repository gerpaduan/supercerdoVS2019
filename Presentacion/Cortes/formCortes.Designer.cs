namespace Presentacion
{
    partial class formCortes
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(formCortes));
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle5 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle6 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle7 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle8 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle9 = new System.Windows.Forms.DataGridViewCellStyle();
            this.barraControl = new System.Windows.Forms.ToolStrip();
            this.nuevo = new System.Windows.Forms.ToolStripButton();
            this.modificar = new System.Windows.Forms.ToolStripButton();
            this.modificarPrecios = new System.Windows.Forms.ToolStripButton();
            this.Imprimir = new System.Windows.Forms.ToolStripButton();
            this.grillaCortes = new System.Windows.Forms.DataGridView();
            this.idCorte = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.codigo = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.corte = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.precioKg = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.tipo = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.idCorteMaestro = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.corteMaestro = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.porcentaje = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.independiente = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.porcentajeHueso = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.desvioEstandar = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.idSucursalSL = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.sucursalSL = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.stockSL = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.idSucursalSM = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.sucursalSM = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.stockSM = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.btnSeleccionar = new System.Windows.Forms.Button();
            this.btnCancelar = new System.Windows.Forms.Button();
            this.panel1 = new System.Windows.Forms.Panel();
            this.txtBuscarCorte = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.btnBuscar = new System.Windows.Forms.Button();
            this.shapeContainer2 = new Microsoft.VisualBasic.PowerPacks.ShapeContainer();
            this.lineShape2 = new Microsoft.VisualBasic.PowerPacks.LineShape();
            this.barraControl.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.grillaCortes)).BeginInit();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // barraControl
            // 
            this.barraControl.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.barraControl.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.nuevo,
            this.modificar,
            this.modificarPrecios,
            this.Imprimir});
            this.barraControl.Location = new System.Drawing.Point(0, 0);
            this.barraControl.Name = "barraControl";
            this.barraControl.Padding = new System.Windows.Forms.Padding(10, 0, 0, 0);
            this.barraControl.Size = new System.Drawing.Size(855, 45);
            this.barraControl.TabIndex = 5;
            this.barraControl.TabStop = true;
            this.barraControl.Text = "toolStrip1";
            // 
            // nuevo
            // 
            this.nuevo.Image = ((System.Drawing.Image)(resources.GetObject("nuevo.Image")));
            this.nuevo.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.nuevo.Margin = new System.Windows.Forms.Padding(0, 2, 0, 1);
            this.nuevo.Name = "nuevo";
            this.nuevo.Padding = new System.Windows.Forms.Padding(1, 1, 1, 6);
            this.nuevo.Size = new System.Drawing.Size(48, 42);
            this.nuevo.Text = "Nuevo";
            this.nuevo.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.nuevo.Click += new System.EventHandler(this.nuevo_Click);
            // 
            // modificar
            // 
            this.modificar.Image = ((System.Drawing.Image)(resources.GetObject("modificar.Image")));
            this.modificar.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.modificar.Margin = new System.Windows.Forms.Padding(0, 2, 0, 1);
            this.modificar.Name = "modificar";
            this.modificar.Padding = new System.Windows.Forms.Padding(1);
            this.modificar.Size = new System.Drawing.Size(64, 42);
            this.modificar.Text = "Modificar";
            this.modificar.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.modificar.Click += new System.EventHandler(this.modificar_Click);
            // 
            // modificarPrecios
            // 
            this.modificarPrecios.Image = ((System.Drawing.Image)(resources.GetObject("modificarPrecios.Image")));
            this.modificarPrecios.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.modificarPrecios.Margin = new System.Windows.Forms.Padding(0, 2, 0, 1);
            this.modificarPrecios.Name = "modificarPrecios";
            this.modificarPrecios.Padding = new System.Windows.Forms.Padding(1);
            this.modificarPrecios.Size = new System.Drawing.Size(82, 42);
            this.modificarPrecios.Text = "Mod. Precios";
            this.modificarPrecios.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.modificarPrecios.Click += new System.EventHandler(this.modificarPrecios_Click);
            // 
            // Imprimir
            // 
            this.Imprimir.Image = ((System.Drawing.Image)(resources.GetObject("Imprimir.Image")));
            this.Imprimir.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.Imprimir.Margin = new System.Windows.Forms.Padding(0, 2, 0, 1);
            this.Imprimir.Name = "Imprimir";
            this.Imprimir.Padding = new System.Windows.Forms.Padding(1, 1, 1, 6);
            this.Imprimir.Size = new System.Drawing.Size(59, 42);
            this.Imprimir.Text = "Imprimir";
            this.Imprimir.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.Imprimir.Click += new System.EventHandler(this.Imprimir_Click);
            // 
            // grillaCortes
            // 
            this.grillaCortes.AllowUserToAddRows = false;
            this.grillaCortes.AllowUserToResizeRows = false;
            this.grillaCortes.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.grillaCortes.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.grillaCortes.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.Raised;
            this.grillaCortes.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.grillaCortes.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.idCorte,
            this.codigo,
            this.corte,
            this.precioKg,
            this.tipo,
            this.idCorteMaestro,
            this.corteMaestro,
            this.porcentaje,
            this.independiente,
            this.porcentajeHueso,
            this.desvioEstandar,
            this.idSucursalSL,
            this.sucursalSL,
            this.stockSL,
            this.idSucursalSM,
            this.sucursalSM,
            this.stockSM});
            this.grillaCortes.Location = new System.Drawing.Point(12, 114);
            this.grillaCortes.MultiSelect = false;
            this.grillaCortes.Name = "grillaCortes";
            this.grillaCortes.ReadOnly = true;
            this.grillaCortes.RowHeadersVisible = false;
            this.grillaCortes.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.grillaCortes.Size = new System.Drawing.Size(832, 445);
            this.grillaCortes.StandardTab = true;
            this.grillaCortes.TabIndex = 2;
            this.grillaCortes.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.grillaCortes_CellDoubleClick);
            // 
            // idCorte
            // 
            this.idCorte.DataPropertyName = "idCorte";
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            this.idCorte.DefaultCellStyle = dataGridViewCellStyle1;
            this.idCorte.HeaderText = "ID Corte";
            this.idCorte.Name = "idCorte";
            this.idCorte.ReadOnly = true;
            this.idCorte.Visible = false;
            // 
            // codigo
            // 
            this.codigo.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.ColumnHeader;
            this.codigo.DataPropertyName = "codigo";
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            this.codigo.DefaultCellStyle = dataGridViewCellStyle2;
            this.codigo.HeaderText = "Código";
            this.codigo.Name = "codigo";
            this.codigo.ReadOnly = true;
            this.codigo.Width = 65;
            // 
            // corte
            // 
            this.corte.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.DisplayedCells;
            this.corte.DataPropertyName = "corte";
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.corte.DefaultCellStyle = dataGridViewCellStyle3;
            this.corte.HeaderText = "Corte";
            this.corte.Name = "corte";
            this.corte.ReadOnly = true;
            this.corte.Width = 57;
            // 
            // precioKg
            // 
            this.precioKg.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.ColumnHeader;
            this.precioKg.DataPropertyName = "precioKg";
            dataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
            dataGridViewCellStyle4.Format = "N2";
            dataGridViewCellStyle4.NullValue = null;
            this.precioKg.DefaultCellStyle = dataGridViewCellStyle4;
            this.precioKg.HeaderText = "Precio Kg.";
            this.precioKg.Name = "precioKg";
            this.precioKg.ReadOnly = true;
            this.precioKg.Width = 75;
            // 
            // tipo
            // 
            this.tipo.DataPropertyName = "tipo";
            this.tipo.HeaderText = "Tipo";
            this.tipo.Name = "tipo";
            this.tipo.ReadOnly = true;
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
            this.corteMaestro.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.corteMaestro.DataPropertyName = "corteMaestro";
            this.corteMaestro.HeaderText = "Corte Maestro";
            this.corteMaestro.Name = "corteMaestro";
            this.corteMaestro.ReadOnly = true;
            // 
            // porcentaje
            // 
            this.porcentaje.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.ColumnHeader;
            this.porcentaje.DataPropertyName = "porcentaje";
            dataGridViewCellStyle5.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
            dataGridViewCellStyle5.Format = "N3";
            dataGridViewCellStyle5.NullValue = null;
            this.porcentaje.DefaultCellStyle = dataGridViewCellStyle5;
            this.porcentaje.HeaderText = "Porcentaje";
            this.porcentaje.Name = "porcentaje";
            this.porcentaje.ReadOnly = true;
            this.porcentaje.Width = 83;
            // 
            // independiente
            // 
            this.independiente.DataPropertyName = "independiente";
            this.independiente.HeaderText = "Independiente";
            this.independiente.Name = "independiente";
            this.independiente.ReadOnly = true;
            this.independiente.Visible = false;
            // 
            // porcentajeHueso
            // 
            this.porcentajeHueso.DataPropertyName = "porcentajeHueso";
            dataGridViewCellStyle6.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
            dataGridViewCellStyle6.Format = "N2";
            dataGridViewCellStyle6.NullValue = null;
            this.porcentajeHueso.DefaultCellStyle = dataGridViewCellStyle6;
            this.porcentajeHueso.HeaderText = "% Desperdicio";
            this.porcentajeHueso.Name = "porcentajeHueso";
            this.porcentajeHueso.ReadOnly = true;
            // 
            // desvioEstandar
            // 
            this.desvioEstandar.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.desvioEstandar.DataPropertyName = "desvioEstandar";
            dataGridViewCellStyle7.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
            dataGridViewCellStyle7.Format = "N2";
            dataGridViewCellStyle7.NullValue = null;
            this.desvioEstandar.DefaultCellStyle = dataGridViewCellStyle7;
            this.desvioEstandar.FillWeight = 90F;
            this.desvioEstandar.HeaderText = "Desvío Estandar";
            this.desvioEstandar.Name = "desvioEstandar";
            this.desvioEstandar.ReadOnly = true;
            this.desvioEstandar.Width = 60;
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
            this.stockSL.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.stockSL.DataPropertyName = "stockSL";
            dataGridViewCellStyle8.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
            dataGridViewCellStyle8.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle8.Format = "N3";
            dataGridViewCellStyle8.NullValue = null;
            this.stockSL.DefaultCellStyle = dataGridViewCellStyle8;
            this.stockSL.FillWeight = 50F;
            this.stockSL.HeaderText = "Stock S. Lorenzo";
            this.stockSL.Name = "stockSL";
            this.stockSL.ReadOnly = true;
            this.stockSL.Width = 90;
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
            this.stockSM.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.stockSM.DataPropertyName = "stockSM";
            dataGridViewCellStyle9.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
            dataGridViewCellStyle9.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle9.Format = "N3";
            dataGridViewCellStyle9.NullValue = null;
            this.stockSM.DefaultCellStyle = dataGridViewCellStyle9;
            this.stockSM.FillWeight = 50F;
            this.stockSM.HeaderText = "Stock S. Martín";
            this.stockSM.Name = "stockSM";
            this.stockSM.ReadOnly = true;
            this.stockSM.Width = 90;
            // 
            // btnSeleccionar
            // 
            this.btnSeleccionar.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSeleccionar.Location = new System.Drawing.Point(664, 567);
            this.btnSeleccionar.Name = "btnSeleccionar";
            this.btnSeleccionar.Size = new System.Drawing.Size(84, 28);
            this.btnSeleccionar.TabIndex = 3;
            this.btnSeleccionar.Text = "Seleccionar";
            this.btnSeleccionar.UseVisualStyleBackColor = true;
            this.btnSeleccionar.Click += new System.EventHandler(this.btnSeleccionar_Click);
            // 
            // btnCancelar
            // 
            this.btnCancelar.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancelar.Location = new System.Drawing.Point(760, 567);
            this.btnCancelar.Name = "btnCancelar";
            this.btnCancelar.Size = new System.Drawing.Size(84, 27);
            this.btnCancelar.TabIndex = 4;
            this.btnCancelar.Text = "Cancelar";
            this.btnCancelar.UseVisualStyleBackColor = true;
            this.btnCancelar.Click += new System.EventHandler(this.btnCancelar_Click);
            // 
            // panel1
            // 
            this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.panel1.BackColor = System.Drawing.Color.SteelBlue;
            this.panel1.Controls.Add(this.txtBuscarCorte);
            this.panel1.Controls.Add(this.label9);
            this.panel1.Controls.Add(this.btnBuscar);
            this.panel1.Controls.Add(this.shapeContainer2);
            this.panel1.Location = new System.Drawing.Point(0, 45);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(855, 62);
            this.panel1.TabIndex = 7;
            // 
            // txtBuscarCorte
            // 
            this.txtBuscarCorte.Location = new System.Drawing.Point(90, 21);
            this.txtBuscarCorte.Name = "txtBuscarCorte";
            this.txtBuscarCorte.Size = new System.Drawing.Size(137, 20);
            this.txtBuscarCorte.TabIndex = 0;
            this.txtBuscarCorte.TextChanged += new System.EventHandler(this.txtBuscarCorte_TextChanged);
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
            // btnBuscar
            // 
            this.btnBuscar.ForeColor = System.Drawing.Color.Black;
            this.btnBuscar.Location = new System.Drawing.Point(233, 19);
            this.btnBuscar.Name = "btnBuscar";
            this.btnBuscar.Size = new System.Drawing.Size(67, 25);
            this.btnBuscar.TabIndex = 1;
            this.btnBuscar.Text = "Buscar";
            this.btnBuscar.UseVisualStyleBackColor = true;
            this.btnBuscar.Click += new System.EventHandler(this.btnBuscar_Click);
            // 
            // shapeContainer2
            // 
            this.shapeContainer2.Location = new System.Drawing.Point(0, 0);
            this.shapeContainer2.Margin = new System.Windows.Forms.Padding(0);
            this.shapeContainer2.Name = "shapeContainer2";
            this.shapeContainer2.Shapes.AddRange(new Microsoft.VisualBasic.PowerPacks.Shape[] {
            this.lineShape2});
            this.shapeContainer2.Size = new System.Drawing.Size(855, 62);
            this.shapeContainer2.TabIndex = 11;
            this.shapeContainer2.TabStop = false;
            // 
            // lineShape2
            // 
            this.lineShape2.BorderColor = System.Drawing.Color.White;
            this.lineShape2.Name = "lineShape2";
            this.lineShape2.X1 = 13;
            this.lineShape2.X2 = 843;
            this.lineShape2.Y1 = 49;
            this.lineShape2.Y2 = 49;
            // 
            // formCortes
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(855, 602);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.btnSeleccionar);
            this.Controls.Add(this.btnCancelar);
            this.Controls.Add(this.grillaCortes);
            this.Controls.Add(this.barraControl);
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MinimizeBox = true;
            this.Name = "formCortes";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Cortes";
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.formCortes_KeyDown);
            this.barraControl.ResumeLayout(false);
            this.barraControl.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.grillaCortes)).EndInit();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        protected System.Windows.Forms.ToolStripButton nuevo;
        protected System.Windows.Forms.ToolStripButton modificarPrecios;
        protected System.Windows.Forms.Button btnSeleccionar;
        protected System.Windows.Forms.Button btnCancelar;
        protected System.Windows.Forms.Panel panel1;
        protected System.Windows.Forms.Button btnBuscar;
        protected System.Windows.Forms.Label label9;
        protected System.Windows.Forms.TextBox txtBuscarCorte;
        private Microsoft.VisualBasic.PowerPacks.ShapeContainer shapeContainer2;
        private Microsoft.VisualBasic.PowerPacks.LineShape lineShape2;
        protected System.Windows.Forms.ToolStrip barraControl;
        protected System.Windows.Forms.DataGridView grillaCortes;
        private System.Windows.Forms.DataGridViewTextBoxColumn idCorte;
        private System.Windows.Forms.DataGridViewTextBoxColumn codigo;
        private System.Windows.Forms.DataGridViewTextBoxColumn corte;
        private System.Windows.Forms.DataGridViewTextBoxColumn precioKg;
        private System.Windows.Forms.DataGridViewTextBoxColumn tipo;
        private System.Windows.Forms.DataGridViewTextBoxColumn idCorteMaestro;
        private System.Windows.Forms.DataGridViewTextBoxColumn corteMaestro;
        private System.Windows.Forms.DataGridViewTextBoxColumn porcentaje;
        private System.Windows.Forms.DataGridViewTextBoxColumn independiente;
        private System.Windows.Forms.DataGridViewTextBoxColumn porcentajeHueso;
        private System.Windows.Forms.DataGridViewTextBoxColumn desvioEstandar;
        private System.Windows.Forms.DataGridViewTextBoxColumn idSucursalSL;
        private System.Windows.Forms.DataGridViewTextBoxColumn sucursalSL;
        private System.Windows.Forms.DataGridViewTextBoxColumn stockSL;
        private System.Windows.Forms.DataGridViewTextBoxColumn idSucursalSM;
        private System.Windows.Forms.DataGridViewTextBoxColumn sucursalSM;
        private System.Windows.Forms.DataGridViewTextBoxColumn stockSM;
        protected System.Windows.Forms.ToolStripButton Imprimir;
        protected System.Windows.Forms.ToolStripButton modificar;
    }
}