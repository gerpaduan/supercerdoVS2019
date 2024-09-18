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
            this.components = new System.ComponentModel.Container();
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle10 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle11 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle12 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle13 = new System.Windows.Forms.DataGridViewCellStyle();
            this.barraControl = new System.Windows.Forms.ToolStrip();
            this.nuevo = new System.Windows.Forms.ToolStripButton();
            this.modificar = new System.Windows.Forms.ToolStripButton();
            this.modificarPrecios = new System.Windows.Forms.ToolStripButton();
            this.Imprimir = new System.Windows.Forms.ToolStripButton();
            this.grillaCortes = new System.Windows.Forms.DataGridView();
            this.btnSeleccionar = new System.Windows.Forms.Button();
            this.btnCancelar = new System.Windows.Forms.Button();
            this.panel1 = new System.Windows.Forms.Panel();
            this.txtBuscarMaestro = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.comboTipo = new System.Windows.Forms.ComboBox();
            this.label6 = new System.Windows.Forms.Label();
            this.lblActualizar = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.txtCodigohasta = new System.Windows.Forms.TextBox();
            this.txtCodigoDesde = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.txtBuscarCorte = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.btnBuscar = new System.Windows.Forms.Button();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.idCorte = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.codigo = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.corte = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.precioKg = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.efectivo = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.debito = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.credito = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.alicuotaIva = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.tipo = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.promedio = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.idCorteMaestro = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.corteMaestro = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.independiente = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.porcentaje = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.porcentajeHueso = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.desvioEstandar = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.habilitado = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.enCierreStock = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.mayorista = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.idSucursalSL = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.sucursalSL = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.stockSL = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.idSucursalSM = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.sucursalSM = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.stockSM = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.barraControl.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.grillaCortes)).BeginInit();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // barraControl
            // 
            this.barraControl.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.barraControl.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.barraControl.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.nuevo,
            this.modificar,
            this.modificarPrecios,
            this.Imprimir});
            this.barraControl.Location = new System.Drawing.Point(0, 0);
            this.barraControl.Name = "barraControl";
            this.barraControl.Padding = new System.Windows.Forms.Padding(13, 0, 0, 0);
            this.barraControl.Size = new System.Drawing.Size(1380, 54);
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
            this.nuevo.Size = new System.Drawing.Size(58, 51);
            this.nuevo.Text = "&Nuevo";
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
            this.modificar.Size = new System.Drawing.Size(79, 51);
            this.modificar.Text = "&Modificar";
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
            this.modificarPrecios.Size = new System.Drawing.Size(100, 51);
            this.modificarPrecios.Text = "Mod. &Precios";
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
            this.Imprimir.Size = new System.Drawing.Size(72, 51);
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
            this.efectivo,
            this.debito,
            this.credito,
            this.alicuotaIva,
            this.tipo,
            this.promedio,
            this.idCorteMaestro,
            this.corteMaestro,
            this.independiente,
            this.porcentaje,
            this.porcentajeHueso,
            this.desvioEstandar,
            this.habilitado,
            this.enCierreStock,
            this.mayorista,
            this.idSucursalSL,
            this.sucursalSL,
            this.stockSL,
            this.idSucursalSM,
            this.sucursalSM,
            this.stockSM});
            this.grillaCortes.Location = new System.Drawing.Point(16, 163);
            this.grillaCortes.Margin = new System.Windows.Forms.Padding(4);
            this.grillaCortes.MultiSelect = false;
            this.grillaCortes.Name = "grillaCortes";
            this.grillaCortes.ReadOnly = true;
            this.grillaCortes.RowHeadersVisible = false;
            this.grillaCortes.RowHeadersWidth = 51;
            this.grillaCortes.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.grillaCortes.Size = new System.Drawing.Size(1349, 525);
            this.grillaCortes.StandardTab = true;
            this.grillaCortes.TabIndex = 2;
            this.grillaCortes.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.grillaCortes_CellDoubleClick);
            // 
            // btnSeleccionar
            // 
            this.btnSeleccionar.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSeleccionar.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSeleccionar.Location = new System.Drawing.Point(1028, 698);
            this.btnSeleccionar.Margin = new System.Windows.Forms.Padding(4);
            this.btnSeleccionar.Name = "btnSeleccionar";
            this.btnSeleccionar.Size = new System.Drawing.Size(164, 34);
            this.btnSeleccionar.TabIndex = 3;
            this.btnSeleccionar.Text = "&Seleccionar";
            this.btnSeleccionar.UseVisualStyleBackColor = true;
            this.btnSeleccionar.Click += new System.EventHandler(this.btnSeleccionar_Click);
            // 
            // btnCancelar
            // 
            this.btnCancelar.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancelar.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnCancelar.Location = new System.Drawing.Point(1200, 698);
            this.btnCancelar.Margin = new System.Windows.Forms.Padding(4);
            this.btnCancelar.Name = "btnCancelar";
            this.btnCancelar.Size = new System.Drawing.Size(164, 34);
            this.btnCancelar.TabIndex = 4;
            this.btnCancelar.Text = "&Cerrar";
            this.btnCancelar.UseVisualStyleBackColor = true;
            this.btnCancelar.Click += new System.EventHandler(this.btnCancelar_Click);
            // 
            // panel1
            // 
            this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(43)))), ((int)(((byte)(77)))), ((int)(((byte)(129)))));
            this.panel1.Controls.Add(this.txtBuscarMaestro);
            this.panel1.Controls.Add(this.label4);
            this.panel1.Controls.Add(this.comboTipo);
            this.panel1.Controls.Add(this.label6);
            this.panel1.Controls.Add(this.lblActualizar);
            this.panel1.Controls.Add(this.label3);
            this.panel1.Controls.Add(this.label2);
            this.panel1.Controls.Add(this.txtCodigohasta);
            this.panel1.Controls.Add(this.txtCodigoDesde);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Controls.Add(this.groupBox1);
            this.panel1.Controls.Add(this.txtBuscarCorte);
            this.panel1.Controls.Add(this.label9);
            this.panel1.Controls.Add(this.btnBuscar);
            this.panel1.Location = new System.Drawing.Point(0, 55);
            this.panel1.Margin = new System.Windows.Forms.Padding(4);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1380, 100);
            this.panel1.TabIndex = 7;
            // 
            // txtBuscarMaestro
            // 
            this.txtBuscarMaestro.Location = new System.Drawing.Point(124, 42);
            this.txtBuscarMaestro.Margin = new System.Windows.Forms.Padding(4);
            this.txtBuscarMaestro.Name = "txtBuscarMaestro";
            this.txtBuscarMaestro.Size = new System.Drawing.Size(181, 22);
            this.txtBuscarMaestro.TabIndex = 56;
            this.txtBuscarMaestro.TextChanged += new System.EventHandler(this.txtCodigoDesde_TextChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.ForeColor = System.Drawing.Color.Cornsilk;
            this.label4.Location = new System.Drawing.Point(13, 43);
            this.label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(104, 18);
            this.label4.TabIndex = 57;
            this.label4.Text = "Corte Maestro";
            // 
            // comboTipo
            // 
            this.comboTipo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboTipo.FormattingEnabled = true;
            this.comboTipo.Items.AddRange(new object[] {
            "Todos",
            "Pesable",
            "Unidad",
            "Elaborado",
            "Corte",
            "Embutido",
            "Otro"});
            this.comboTipo.Location = new System.Drawing.Point(124, 12);
            this.comboTipo.Margin = new System.Windows.Forms.Padding(4);
            this.comboTipo.Name = "comboTipo";
            this.comboTipo.Size = new System.Drawing.Size(181, 24);
            this.comboTipo.TabIndex = 54;
            this.comboTipo.TextChanged += new System.EventHandler(this.txtCodigoDesde_TextChanged);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.ForeColor = System.Drawing.Color.Cornsilk;
            this.label6.Location = new System.Drawing.Point(79, 13);
            this.label6.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(37, 18);
            this.label6.TabIndex = 55;
            this.label6.Text = "Tipo";
            // 
            // lblActualizar
            // 
            this.lblActualizar.AutoSize = true;
            this.lblActualizar.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblActualizar.ForeColor = System.Drawing.Color.LightSalmon;
            this.lblActualizar.Location = new System.Drawing.Point(410, 72);
            this.lblActualizar.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblActualizar.Name = "lblActualizar";
            this.lblActualizar.Size = new System.Drawing.Size(84, 18);
            this.lblActualizar.TabIndex = 53;
            this.lblActualizar.Text = "Actualizar...";
            this.lblActualizar.Visible = false;
            // 
            // label3
            // 
            this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.ForeColor = System.Drawing.Color.Cornsilk;
            this.label3.Location = new System.Drawing.Point(1187, 70);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(47, 18);
            this.label3.TabIndex = 31;
            this.label3.Text = "Hasta";
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.ForeColor = System.Drawing.Color.Cornsilk;
            this.label2.Location = new System.Drawing.Point(997, 71);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(51, 18);
            this.label2.TabIndex = 30;
            this.label2.Text = "Desde";
            // 
            // txtCodigohasta
            // 
            this.txtCodigohasta.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.txtCodigohasta.Location = new System.Drawing.Point(1247, 68);
            this.txtCodigohasta.Margin = new System.Windows.Forms.Padding(4);
            this.txtCodigohasta.Name = "txtCodigohasta";
            this.txtCodigohasta.Size = new System.Drawing.Size(117, 22);
            this.txtCodigohasta.TabIndex = 29;
            this.toolTip1.SetToolTip(this.txtCodigohasta, "Ingrese aquí el Código hasta donde se filtrará.\r\n(Deje vacío el campo para no fil" +
        "trar por este campo)");
            this.txtCodigohasta.TextChanged += new System.EventHandler(this.txtCodigohasta_TextChanged);
            // 
            // txtCodigoDesde
            // 
            this.txtCodigoDesde.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.txtCodigoDesde.Location = new System.Drawing.Point(1062, 68);
            this.txtCodigoDesde.Margin = new System.Windows.Forms.Padding(4);
            this.txtCodigoDesde.Name = "txtCodigoDesde";
            this.txtCodigoDesde.Size = new System.Drawing.Size(116, 22);
            this.txtCodigoDesde.TabIndex = 28;
            this.toolTip1.SetToolTip(this.txtCodigoDesde, "Ingrese aquí el Código a partir del cual se empezará a filtrar.\r\n(Deje vacío el c" +
        "ampo para no filtrar por este campo)");
            this.txtCodigoDesde.TextChanged += new System.EventHandler(this.txtCodigoDesde_TextChanged);
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ForeColor = System.Drawing.Color.Cornsilk;
            this.label1.Location = new System.Drawing.Point(843, 41);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(120, 18);
            this.label1.TabIndex = 27;
            this.label1.Text = "Filtrar por codigo";
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.groupBox1.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.groupBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.groupBox1.Location = new System.Drawing.Point(974, 51);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(4);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(4);
            this.groupBox1.Size = new System.Drawing.Size(393, 9);
            this.groupBox1.TabIndex = 26;
            this.groupBox1.TabStop = false;
            // 
            // txtBuscarCorte
            // 
            this.txtBuscarCorte.Location = new System.Drawing.Point(124, 69);
            this.txtBuscarCorte.Margin = new System.Windows.Forms.Padding(4);
            this.txtBuscarCorte.Name = "txtBuscarCorte";
            this.txtBuscarCorte.Size = new System.Drawing.Size(181, 22);
            this.txtBuscarCorte.TabIndex = 0;
            this.txtBuscarCorte.TextChanged += new System.EventHandler(this.txtCodigoDesde_TextChanged);
            this.txtBuscarCorte.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtBuscarCorte_KeyDown);
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label9.ForeColor = System.Drawing.Color.Cornsilk;
            this.label9.Location = new System.Drawing.Point(71, 70);
            this.label9.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(45, 18);
            this.label9.TabIndex = 2;
            this.label9.Text = "Corte";
            // 
            // btnBuscar
            // 
            this.btnBuscar.ForeColor = System.Drawing.Color.Black;
            this.btnBuscar.Location = new System.Drawing.Point(313, 65);
            this.btnBuscar.Margin = new System.Windows.Forms.Padding(4);
            this.btnBuscar.Name = "btnBuscar";
            this.btnBuscar.Size = new System.Drawing.Size(89, 31);
            this.btnBuscar.TabIndex = 1;
            this.btnBuscar.Text = "Buscar";
            this.btnBuscar.UseVisualStyleBackColor = true;
            this.btnBuscar.Click += new System.EventHandler(this.btnBuscar_Click);
            // 
            // idCorte
            // 
            this.idCorte.DataPropertyName = "idCorte";
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            this.idCorte.DefaultCellStyle = dataGridViewCellStyle1;
            this.idCorte.HeaderText = "ID Corte";
            this.idCorte.MinimumWidth = 6;
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
            this.codigo.MinimumWidth = 6;
            this.codigo.Name = "codigo";
            this.codigo.ReadOnly = true;
            this.codigo.Width = 80;
            // 
            // corte
            // 
            this.corte.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.corte.DataPropertyName = "corte";
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.corte.DefaultCellStyle = dataGridViewCellStyle3;
            this.corte.HeaderText = "Corte";
            this.corte.MinimumWidth = 6;
            this.corte.Name = "corte";
            this.corte.ReadOnly = true;
            this.corte.Width = 68;
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
            this.precioKg.MinimumWidth = 6;
            this.precioKg.Name = "precioKg";
            this.precioKg.ReadOnly = true;
            this.precioKg.Width = 97;
            // 
            // efectivo
            // 
            this.efectivo.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.ColumnHeader;
            this.efectivo.DataPropertyName = "efectivo";
            dataGridViewCellStyle5.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
            dataGridViewCellStyle5.Format = "N2";
            this.efectivo.DefaultCellStyle = dataGridViewCellStyle5;
            this.efectivo.HeaderText = "Efectivo";
            this.efectivo.MinimumWidth = 6;
            this.efectivo.Name = "efectivo";
            this.efectivo.ReadOnly = true;
            this.efectivo.Width = 84;
            // 
            // debito
            // 
            this.debito.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.ColumnHeader;
            this.debito.DataPropertyName = "debito";
            dataGridViewCellStyle6.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
            dataGridViewCellStyle6.Format = "N2";
            this.debito.DefaultCellStyle = dataGridViewCellStyle6;
            this.debito.HeaderText = "Debito";
            this.debito.MinimumWidth = 6;
            this.debito.Name = "debito";
            this.debito.ReadOnly = true;
            this.debito.Width = 76;
            // 
            // credito
            // 
            this.credito.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.ColumnHeader;
            this.credito.DataPropertyName = "credito";
            dataGridViewCellStyle7.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
            dataGridViewCellStyle7.Format = "N2";
            this.credito.DefaultCellStyle = dataGridViewCellStyle7;
            this.credito.HeaderText = "Credito";
            this.credito.MinimumWidth = 6;
            this.credito.Name = "credito";
            this.credito.ReadOnly = true;
            this.credito.Width = 79;
            // 
            // alicuotaIva
            // 
            this.alicuotaIva.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.ColumnHeader;
            this.alicuotaIva.DataPropertyName = "alicuotaIva";
            this.alicuotaIva.HeaderText = "Alicuota Iva";
            this.alicuotaIva.MinimumWidth = 6;
            this.alicuotaIva.Name = "alicuotaIva";
            this.alicuotaIva.ReadOnly = true;
            this.alicuotaIva.Width = 105;
            // 
            // tipo
            // 
            this.tipo.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.tipo.DataPropertyName = "tipo";
            this.tipo.HeaderText = "Tipo";
            this.tipo.MinimumWidth = 6;
            this.tipo.Name = "tipo";
            this.tipo.ReadOnly = true;
            this.tipo.Width = 80;
            // 
            // promedio
            // 
            this.promedio.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.ColumnHeader;
            this.promedio.DataPropertyName = "promedio";
            dataGridViewCellStyle8.Format = "F3";
            this.promedio.DefaultCellStyle = dataGridViewCellStyle8;
            this.promedio.HeaderText = "Promedio";
            this.promedio.MinimumWidth = 6;
            this.promedio.Name = "promedio";
            this.promedio.ReadOnly = true;
            this.promedio.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.promedio.Width = 95;
            // 
            // idCorteMaestro
            // 
            this.idCorteMaestro.DataPropertyName = "idCorteMaestro";
            this.idCorteMaestro.HeaderText = "ID Codigo Maestro";
            this.idCorteMaestro.MinimumWidth = 6;
            this.idCorteMaestro.Name = "idCorteMaestro";
            this.idCorteMaestro.ReadOnly = true;
            this.idCorteMaestro.Visible = false;
            // 
            // corteMaestro
            // 
            this.corteMaestro.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.corteMaestro.DataPropertyName = "corteMaestro";
            this.corteMaestro.HeaderText = "Corte Maestro";
            this.corteMaestro.MinimumWidth = 6;
            this.corteMaestro.Name = "corteMaestro";
            this.corteMaestro.ReadOnly = true;
            this.corteMaestro.Width = 120;
            // 
            // independiente
            // 
            this.independiente.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.independiente.DataPropertyName = "independiente";
            this.independiente.HeaderText = "Independiente";
            this.independiente.MinimumWidth = 6;
            this.independiente.Name = "independiente";
            this.independiente.ReadOnly = true;
            this.independiente.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.independiente.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            this.independiente.Width = 85;
            // 
            // porcentaje
            // 
            this.porcentaje.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.porcentaje.DataPropertyName = "porcentaje";
            dataGridViewCellStyle9.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
            dataGridViewCellStyle9.Format = "N3";
            dataGridViewCellStyle9.NullValue = null;
            this.porcentaje.DefaultCellStyle = dataGridViewCellStyle9;
            this.porcentaje.HeaderText = "Porcentaje";
            this.porcentaje.MinimumWidth = 6;
            this.porcentaje.Name = "porcentaje";
            this.porcentaje.ReadOnly = true;
            this.porcentaje.Width = 80;
            // 
            // porcentajeHueso
            // 
            this.porcentajeHueso.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.ColumnHeader;
            this.porcentajeHueso.DataPropertyName = "porcentajeHueso";
            dataGridViewCellStyle10.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
            dataGridViewCellStyle10.Format = "N2";
            dataGridViewCellStyle10.NullValue = null;
            this.porcentajeHueso.DefaultCellStyle = dataGridViewCellStyle10;
            this.porcentajeHueso.HeaderText = "% Desperdicio";
            this.porcentajeHueso.MinimumWidth = 6;
            this.porcentajeHueso.Name = "porcentajeHueso";
            this.porcentajeHueso.ReadOnly = true;
            this.porcentajeHueso.Width = 115;
            // 
            // desvioEstandar
            // 
            this.desvioEstandar.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.desvioEstandar.DataPropertyName = "desvioEstandar";
            dataGridViewCellStyle11.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
            dataGridViewCellStyle11.Format = "N2";
            dataGridViewCellStyle11.NullValue = null;
            this.desvioEstandar.DefaultCellStyle = dataGridViewCellStyle11;
            this.desvioEstandar.FillWeight = 90F;
            this.desvioEstandar.HeaderText = "Desvío Estandar";
            this.desvioEstandar.MinimumWidth = 6;
            this.desvioEstandar.Name = "desvioEstandar";
            this.desvioEstandar.ReadOnly = true;
            this.desvioEstandar.Visible = false;
            this.desvioEstandar.Width = 60;
            // 
            // habilitado
            // 
            this.habilitado.DataPropertyName = "habilitado";
            this.habilitado.HeaderText = "Habilitado";
            this.habilitado.MinimumWidth = 6;
            this.habilitado.Name = "habilitado";
            this.habilitado.ReadOnly = true;
            // 
            // enCierreStock
            // 
            this.enCierreStock.DataPropertyName = "enCierreStock";
            this.enCierreStock.HeaderText = "En Cierre Stock";
            this.enCierreStock.MinimumWidth = 6;
            this.enCierreStock.Name = "enCierreStock";
            this.enCierreStock.ReadOnly = true;
            // 
            // mayorista
            // 
            this.mayorista.DataPropertyName = "mayorista";
            this.mayorista.HeaderText = "Mayorista";
            this.mayorista.MinimumWidth = 6;
            this.mayorista.Name = "mayorista";
            this.mayorista.ReadOnly = true;
            this.mayorista.Visible = false;
            // 
            // idSucursalSL
            // 
            this.idSucursalSL.DataPropertyName = "idSucursalSL";
            this.idSucursalSL.HeaderText = "ID Sucursal SL";
            this.idSucursalSL.MinimumWidth = 6;
            this.idSucursalSL.Name = "idSucursalSL";
            this.idSucursalSL.ReadOnly = true;
            this.idSucursalSL.Visible = false;
            // 
            // sucursalSL
            // 
            this.sucursalSL.DataPropertyName = "sucursalSL";
            this.sucursalSL.HeaderText = "Sucursal SL";
            this.sucursalSL.MinimumWidth = 6;
            this.sucursalSL.Name = "sucursalSL";
            this.sucursalSL.ReadOnly = true;
            this.sucursalSL.Visible = false;
            // 
            // stockSL
            // 
            this.stockSL.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.stockSL.DataPropertyName = "stockSL";
            dataGridViewCellStyle12.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
            dataGridViewCellStyle12.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle12.Format = "N3";
            dataGridViewCellStyle12.NullValue = null;
            this.stockSL.DefaultCellStyle = dataGridViewCellStyle12;
            this.stockSL.FillWeight = 50F;
            this.stockSL.HeaderText = "Stock S. Lorenzo";
            this.stockSL.MinimumWidth = 6;
            this.stockSL.Name = "stockSL";
            this.stockSL.ReadOnly = true;
            this.stockSL.Visible = false;
            this.stockSL.Width = 90;
            // 
            // idSucursalSM
            // 
            this.idSucursalSM.DataPropertyName = "idSucursalSM";
            this.idSucursalSM.HeaderText = "ID Sucursal SM";
            this.idSucursalSM.MinimumWidth = 6;
            this.idSucursalSM.Name = "idSucursalSM";
            this.idSucursalSM.ReadOnly = true;
            this.idSucursalSM.Visible = false;
            // 
            // sucursalSM
            // 
            this.sucursalSM.DataPropertyName = "sucursalSM";
            this.sucursalSM.HeaderText = "Sucursal SM";
            this.sucursalSM.MinimumWidth = 6;
            this.sucursalSM.Name = "sucursalSM";
            this.sucursalSM.ReadOnly = true;
            this.sucursalSM.Visible = false;
            // 
            // stockSM
            // 
            this.stockSM.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.stockSM.DataPropertyName = "stockSM";
            dataGridViewCellStyle13.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
            dataGridViewCellStyle13.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle13.Format = "N3";
            dataGridViewCellStyle13.NullValue = null;
            this.stockSM.DefaultCellStyle = dataGridViewCellStyle13;
            this.stockSM.FillWeight = 50F;
            this.stockSM.HeaderText = "Stock S. Martín";
            this.stockSM.MinimumWidth = 6;
            this.stockSM.Name = "stockSM";
            this.stockSM.ReadOnly = true;
            this.stockSM.Visible = false;
            this.stockSM.Width = 90;
            // 
            // formCortes
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1380, 741);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.btnSeleccionar);
            this.Controls.Add(this.btnCancelar);
            this.Controls.Add(this.grillaCortes);
            this.Controls.Add(this.barraControl);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Sizable;
            this.KeyPreview = true;
            this.Margin = new System.Windows.Forms.Padding(5);
            this.MinimizeBox = true;
            this.Name = "formCortes";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Cortes";
            this.Load += new System.EventHandler(this.formCortes_Load);
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
        protected System.Windows.Forms.ToolStrip barraControl;
        protected System.Windows.Forms.DataGridView grillaCortes;
        protected System.Windows.Forms.ToolStripButton Imprimir;
        protected System.Windows.Forms.ToolStripButton modificar;
        private System.Windows.Forms.GroupBox groupBox1;
        protected System.Windows.Forms.Label label1;
        protected System.Windows.Forms.Label label3;
        protected System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtCodigohasta;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.TextBox txtCodigoDesde;
        protected System.Windows.Forms.Label lblActualizar;
        protected System.Windows.Forms.ComboBox comboTipo;
        protected System.Windows.Forms.Label label6;
        protected System.Windows.Forms.TextBox txtBuscarMaestro;
        protected System.Windows.Forms.Label label4;
        private System.Windows.Forms.DataGridViewTextBoxColumn idCorte;
        private System.Windows.Forms.DataGridViewTextBoxColumn codigo;
        private System.Windows.Forms.DataGridViewTextBoxColumn corte;
        private System.Windows.Forms.DataGridViewTextBoxColumn precioKg;
        private System.Windows.Forms.DataGridViewTextBoxColumn efectivo;
        private System.Windows.Forms.DataGridViewTextBoxColumn debito;
        private System.Windows.Forms.DataGridViewTextBoxColumn credito;
        private System.Windows.Forms.DataGridViewTextBoxColumn alicuotaIva;
        private System.Windows.Forms.DataGridViewTextBoxColumn tipo;
        private System.Windows.Forms.DataGridViewTextBoxColumn promedio;
        private System.Windows.Forms.DataGridViewTextBoxColumn idCorteMaestro;
        private System.Windows.Forms.DataGridViewTextBoxColumn corteMaestro;
        private System.Windows.Forms.DataGridViewCheckBoxColumn independiente;
        private System.Windows.Forms.DataGridViewTextBoxColumn porcentaje;
        private System.Windows.Forms.DataGridViewTextBoxColumn porcentajeHueso;
        private System.Windows.Forms.DataGridViewTextBoxColumn desvioEstandar;
        private System.Windows.Forms.DataGridViewCheckBoxColumn habilitado;
        private System.Windows.Forms.DataGridViewCheckBoxColumn enCierreStock;
        private System.Windows.Forms.DataGridViewCheckBoxColumn mayorista;
        private System.Windows.Forms.DataGridViewTextBoxColumn idSucursalSL;
        private System.Windows.Forms.DataGridViewTextBoxColumn sucursalSL;
        private System.Windows.Forms.DataGridViewTextBoxColumn stockSL;
        private System.Windows.Forms.DataGridViewTextBoxColumn idSucursalSM;
        private System.Windows.Forms.DataGridViewTextBoxColumn sucursalSM;
        private System.Windows.Forms.DataGridViewTextBoxColumn stockSM;
    }
}