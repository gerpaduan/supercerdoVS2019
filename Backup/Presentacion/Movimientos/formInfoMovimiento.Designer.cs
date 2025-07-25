namespace Presentacion.Movimientos
{
    partial class formInfoMovimiento
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(formInfoMovimiento));
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
            this.barraControl = new System.Windows.Forms.ToolStrip();
            this.modificar = new System.Windows.Forms.ToolStripButton();
            this.verAcum = new System.Windows.Forms.ToolStripButton();
            this.Reporte = new System.Windows.Forms.ToolStripButton();
            this.Imprimir = new System.Windows.Forms.ToolStripButton();
            this.eliminar = new System.Windows.Forms.ToolStripButton();
            this.pnlBuscar = new System.Windows.Forms.Panel();
            this.txtFechaMovimiento = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label10 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.lblIdDestino = new System.Windows.Forms.Label();
            this.lblIdOrigen = new System.Windows.Forms.Label();
            this.txtSucDestino = new System.Windows.Forms.TextBox();
            this.txtSucOrigen = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.btnSalir = new System.Windows.Forms.Button();
            this.txtObservaciones = new System.Windows.Forms.TextBox();
            this.txtCantItems = new System.Windows.Forms.TextBox();
            this.label12 = new System.Windows.Forms.Label();
            this.txtTotalKg = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.grillaCortesPorMovimiento = new System.Windows.Forms.DataGridView();
            this.label11 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.pnlEliminado = new System.Windows.Forms.Panel();
            this.label7 = new System.Windows.Forms.Label();
            this.txtActualizadoPor = new System.Windows.Forms.TextBox();
            this.txtActualizado = new System.Windows.Forms.TextBox();
            this.label13 = new System.Windows.Forms.Label();
            this.txtCreadoPor = new System.Windows.Forms.TextBox();
            this.txtCreado = new System.Windows.Forms.TextBox();
            this.idMovimientoLabel = new System.Windows.Forms.Label();
            this.idCorteMovimientodo = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.codigo = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.corte = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.cantUnidad = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.cantKg = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Balanza = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.permitirIngreso = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.barraControl.SuspendLayout();
            this.pnlBuscar.SuspendLayout();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.grillaCortesPorMovimiento)).BeginInit();
            this.pnlEliminado.SuspendLayout();
            this.SuspendLayout();
            // 
            // barraControl
            // 
            this.barraControl.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.barraControl.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.modificar,
            this.verAcum,
            this.Reporte,
            this.Imprimir,
            this.eliminar});
            this.barraControl.Location = new System.Drawing.Point(0, 0);
            this.barraControl.Name = "barraControl";
            this.barraControl.Padding = new System.Windows.Forms.Padding(10, 0, 0, 0);
            this.barraControl.Size = new System.Drawing.Size(605, 45);
            this.barraControl.TabIndex = 7;
            this.barraControl.Text = "toolStrip1";
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
            // verAcum
            // 
            this.verAcum.Image = ((System.Drawing.Image)(resources.GetObject("verAcum.Image")));
            this.verAcum.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.verAcum.Name = "verAcum";
            this.verAcum.Size = new System.Drawing.Size(63, 42);
            this.verAcum.Text = "Ver acum.";
            this.verAcum.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.verAcum.Click += new System.EventHandler(this.verAcum_Click);
            // 
            // Reporte
            // 
            this.Reporte.Image = ((System.Drawing.Image)(resources.GetObject("Reporte.Image")));
            this.Reporte.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.Reporte.Name = "Reporte";
            this.Reporte.Size = new System.Drawing.Size(52, 42);
            this.Reporte.Text = "Reporte";
            this.Reporte.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.Reporte.Visible = false;
            this.Reporte.Click += new System.EventHandler(this.Reporte_Click);
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
            // eliminar
            // 
            this.eliminar.Image = ((System.Drawing.Image)(resources.GetObject("eliminar.Image")));
            this.eliminar.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.eliminar.Margin = new System.Windows.Forms.Padding(0, 2, 0, 1);
            this.eliminar.Name = "eliminar";
            this.eliminar.Padding = new System.Windows.Forms.Padding(1);
            this.eliminar.Size = new System.Drawing.Size(56, 42);
            this.eliminar.Text = "Eliminar";
            this.eliminar.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.eliminar.Click += new System.EventHandler(this.eliminar_Click);
            // 
            // pnlBuscar
            // 
            this.pnlBuscar.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.pnlBuscar.BackColor = System.Drawing.Color.SteelBlue;
            this.pnlBuscar.Controls.Add(this.txtFechaMovimiento);
            this.pnlBuscar.Controls.Add(this.label6);
            this.pnlBuscar.Controls.Add(this.groupBox1);
            this.pnlBuscar.Location = new System.Drawing.Point(-24, 45);
            this.pnlBuscar.Name = "pnlBuscar";
            this.pnlBuscar.Size = new System.Drawing.Size(629, 93);
            this.pnlBuscar.TabIndex = 23;
            // 
            // txtFechaMovimiento
            // 
            this.txtFechaMovimiento.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.txtFechaMovimiento.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtFechaMovimiento.Location = new System.Drawing.Point(472, 6);
            this.txtFechaMovimiento.Name = "txtFechaMovimiento";
            this.txtFechaMovimiento.ReadOnly = true;
            this.txtFechaMovimiento.Size = new System.Drawing.Size(154, 21);
            this.txtFechaMovimiento.TabIndex = 45;
            this.txtFechaMovimiento.TabStop = false;
            this.txtFechaMovimiento.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label6
            // 
            this.label6.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.ForeColor = System.Drawing.Color.Cornsilk;
            this.label6.Location = new System.Drawing.Point(425, 9);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(41, 15);
            this.label6.TabIndex = 16;
            this.label6.Text = "Fecha";
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.groupBox1.BackColor = System.Drawing.Color.SteelBlue;
            this.groupBox1.Controls.Add(this.label10);
            this.groupBox1.Controls.Add(this.label9);
            this.groupBox1.Controls.Add(this.lblIdDestino);
            this.groupBox1.Controls.Add(this.lblIdOrigen);
            this.groupBox1.Controls.Add(this.txtSucDestino);
            this.groupBox1.Controls.Add(this.txtSucOrigen);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox1.ForeColor = System.Drawing.Color.Cornsilk;
            this.groupBox1.Location = new System.Drawing.Point(36, 3);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(280, 79);
            this.groupBox1.TabIndex = 13;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Sucursales";
            // 
            // label10
            // 
            this.label10.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.label10.AutoSize = true;
            this.label10.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label10.ForeColor = System.Drawing.Color.Cornsilk;
            this.label10.Location = new System.Drawing.Point(50, 49);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(10, 15);
            this.label10.TabIndex = 16;
            this.label10.Text = "|";
            // 
            // label9
            // 
            this.label9.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.label9.AutoSize = true;
            this.label9.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label9.ForeColor = System.Drawing.Color.Cornsilk;
            this.label9.Location = new System.Drawing.Point(50, 23);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(10, 15);
            this.label9.TabIndex = 15;
            this.label9.Text = "|";
            // 
            // lblIdDestino
            // 
            this.lblIdDestino.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.lblIdDestino.AutoSize = true;
            this.lblIdDestino.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblIdDestino.ForeColor = System.Drawing.Color.Cornsilk;
            this.lblIdDestino.Location = new System.Drawing.Point(6, 49);
            this.lblIdDestino.Name = "lblIdDestino";
            this.lblIdDestino.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.lblIdDestino.Size = new System.Drawing.Size(44, 15);
            this.lblIdDestino.TabIndex = 14;
            this.lblIdDestino.Text = "IdDes";
            // 
            // lblIdOrigen
            // 
            this.lblIdOrigen.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.lblIdOrigen.AutoSize = true;
            this.lblIdOrigen.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblIdOrigen.ForeColor = System.Drawing.Color.Cornsilk;
            this.lblIdOrigen.Location = new System.Drawing.Point(6, 23);
            this.lblIdOrigen.Name = "lblIdOrigen";
            this.lblIdOrigen.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.lblIdOrigen.Size = new System.Drawing.Size(38, 15);
            this.lblIdOrigen.TabIndex = 13;
            this.lblIdOrigen.Text = "IdOri";
            // 
            // txtSucDestino
            // 
            this.txtSucDestino.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.txtSucDestino.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtSucDestino.Location = new System.Drawing.Point(116, 46);
            this.txtSucDestino.Name = "txtSucDestino";
            this.txtSucDestino.ReadOnly = true;
            this.txtSucDestino.Size = new System.Drawing.Size(152, 20);
            this.txtSucDestino.TabIndex = 12;
            // 
            // txtSucOrigen
            // 
            this.txtSucOrigen.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.txtSucOrigen.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtSucOrigen.Location = new System.Drawing.Point(116, 20);
            this.txtSucOrigen.Name = "txtSucOrigen";
            this.txtSucOrigen.ReadOnly = true;
            this.txtSucOrigen.Size = new System.Drawing.Size(152, 20);
            this.txtSucOrigen.TabIndex = 11;
            // 
            // label3
            // 
            this.label3.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.ForeColor = System.Drawing.Color.Cornsilk;
            this.label3.Location = new System.Drawing.Point(66, 23);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(44, 15);
            this.label3.TabIndex = 10;
            this.label3.Text = "Origen";
            // 
            // label4
            // 
            this.label4.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.ForeColor = System.Drawing.Color.Cornsilk;
            this.label4.Location = new System.Drawing.Point(61, 49);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(49, 15);
            this.label4.TabIndex = 8;
            this.label4.Text = "Destino";
            // 
            // btnSalir
            // 
            this.btnSalir.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSalir.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSalir.Location = new System.Drawing.Point(470, 542);
            this.btnSalir.Name = "btnSalir";
            this.btnSalir.Size = new System.Drawing.Size(123, 37);
            this.btnSalir.TabIndex = 21;
            this.btnSalir.Text = "Salir";
            this.btnSalir.UseVisualStyleBackColor = true;
            this.btnSalir.Click += new System.EventHandler(this.btnSalir_Click);
            // 
            // txtObservaciones
            // 
            this.txtObservaciones.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.txtObservaciones.Location = new System.Drawing.Point(470, 246);
            this.txtObservaciones.Multiline = true;
            this.txtObservaciones.Name = "txtObservaciones";
            this.txtObservaciones.ReadOnly = true;
            this.txtObservaciones.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtObservaciones.Size = new System.Drawing.Size(123, 142);
            this.txtObservaciones.TabIndex = 45;
            this.txtObservaciones.TabStop = false;
            // 
            // txtCantItems
            // 
            this.txtCantItems.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.txtCantItems.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtCantItems.Location = new System.Drawing.Point(470, 206);
            this.txtCantItems.Name = "txtCantItems";
            this.txtCantItems.ReadOnly = true;
            this.txtCantItems.Size = new System.Drawing.Size(123, 21);
            this.txtCantItems.TabIndex = 44;
            this.txtCantItems.TabStop = false;
            this.txtCantItems.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label12
            // 
            this.label12.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.label12.AutoSize = true;
            this.label12.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label12.Location = new System.Drawing.Point(467, 188);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(79, 15);
            this.label12.TabIndex = 43;
            this.label12.Text = "Cant. Items";
            // 
            // txtTotalKg
            // 
            this.txtTotalKg.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.txtTotalKg.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtTotalKg.Location = new System.Drawing.Point(470, 164);
            this.txtTotalKg.Name = "txtTotalKg";
            this.txtTotalKg.ReadOnly = true;
            this.txtTotalKg.Size = new System.Drawing.Size(123, 21);
            this.txtTotalKg.TabIndex = 42;
            this.txtTotalKg.TabStop = false;
            this.txtTotalKg.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label8
            // 
            this.label8.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.label8.AutoSize = true;
            this.label8.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label8.Location = new System.Drawing.Point(467, 146);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(60, 15);
            this.label8.TabIndex = 41;
            this.label8.Text = "Total Kg";
            // 
            // grillaCortesPorMovimiento
            // 
            this.grillaCortesPorMovimiento.AllowUserToAddRows = false;
            this.grillaCortesPorMovimiento.AllowUserToOrderColumns = true;
            this.grillaCortesPorMovimiento.AllowUserToResizeRows = false;
            this.grillaCortesPorMovimiento.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.grillaCortesPorMovimiento.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.grillaCortesPorMovimiento.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.idCorteMovimientodo,
            this.codigo,
            this.corte,
            this.cantUnidad,
            this.cantKg,
            this.Balanza,
            this.permitirIngreso});
            this.grillaCortesPorMovimiento.Location = new System.Drawing.Point(12, 145);
            this.grillaCortesPorMovimiento.Name = "grillaCortesPorMovimiento";
            this.grillaCortesPorMovimiento.RowHeadersVisible = false;
            this.grillaCortesPorMovimiento.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.grillaCortesPorMovimiento.Size = new System.Drawing.Size(449, 434);
            this.grillaCortesPorMovimiento.TabIndex = 0;
            // 
            // label11
            // 
            this.label11.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(467, 230);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(78, 13);
            this.label11.TabIndex = 46;
            this.label11.Text = "Observaciones";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.BackColor = System.Drawing.Color.Transparent;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 15F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(15, 10);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(106, 25);
            this.label1.TabIndex = 47;
            this.label1.Text = "Eliminado";
            // 
            // pnlEliminado
            // 
            this.pnlEliminado.BackColor = System.Drawing.SystemColors.ActiveBorder;
            this.pnlEliminado.Controls.Add(this.label1);
            this.pnlEliminado.Location = new System.Drawing.Point(0, -1);
            this.pnlEliminado.Name = "pnlEliminado";
            this.pnlEliminado.Size = new System.Drawing.Size(605, 46);
            this.pnlEliminado.TabIndex = 48;
            this.pnlEliminado.Visible = false;
            // 
            // label7
            // 
            this.label7.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label7.Location = new System.Drawing.Point(469, 457);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(59, 13);
            this.label7.TabIndex = 62;
            this.label7.Text = "Modificado";
            // 
            // txtActualizadoPor
            // 
            this.txtActualizadoPor.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtActualizadoPor.Location = new System.Drawing.Point(470, 494);
            this.txtActualizadoPor.Name = "txtActualizadoPor";
            this.txtActualizadoPor.ReadOnly = true;
            this.txtActualizadoPor.Size = new System.Drawing.Size(123, 21);
            this.txtActualizadoPor.TabIndex = 61;
            this.txtActualizadoPor.TabStop = false;
            // 
            // txtActualizado
            // 
            this.txtActualizado.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtActualizado.Location = new System.Drawing.Point(470, 473);
            this.txtActualizado.Name = "txtActualizado";
            this.txtActualizado.ReadOnly = true;
            this.txtActualizado.Size = new System.Drawing.Size(123, 21);
            this.txtActualizado.TabIndex = 60;
            this.txtActualizado.TabStop = false;
            // 
            // label13
            // 
            this.label13.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label13.AutoSize = true;
            this.label13.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label13.Location = new System.Drawing.Point(470, 396);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(41, 13);
            this.label13.TabIndex = 59;
            this.label13.Text = "Creado";
            // 
            // txtCreadoPor
            // 
            this.txtCreadoPor.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtCreadoPor.Location = new System.Drawing.Point(470, 433);
            this.txtCreadoPor.Name = "txtCreadoPor";
            this.txtCreadoPor.ReadOnly = true;
            this.txtCreadoPor.Size = new System.Drawing.Size(123, 21);
            this.txtCreadoPor.TabIndex = 58;
            this.txtCreadoPor.TabStop = false;
            // 
            // txtCreado
            // 
            this.txtCreado.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtCreado.Location = new System.Drawing.Point(470, 412);
            this.txtCreado.Name = "txtCreado";
            this.txtCreado.ReadOnly = true;
            this.txtCreado.Size = new System.Drawing.Size(123, 21);
            this.txtCreado.TabIndex = 57;
            this.txtCreado.TabStop = false;
            // 
            // idMovimientoLabel
            // 
            this.idMovimientoLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.idMovimientoLabel.AutoSize = true;
            this.idMovimientoLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.idMovimientoLabel.Location = new System.Drawing.Point(498, 518);
            this.idMovimientoLabel.Name = "idMovimientoLabel";
            this.idMovimientoLabel.Size = new System.Drawing.Size(95, 13);
            this.idMovimientoLabel.TabIndex = 63;
            this.idMovimientoLabel.Text = "idMovimientoLabel";
            this.idMovimientoLabel.Visible = false;
            // 
            // idCorteMovimientodo
            // 
            this.idCorteMovimientodo.DataPropertyName = "idCorteMovimiento";
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.TopRight;
            this.idCorteMovimientodo.DefaultCellStyle = dataGridViewCellStyle1;
            this.idCorteMovimientodo.HeaderText = "Id Prod. Mov.";
            this.idCorteMovimientodo.MinimumWidth = 70;
            this.idCorteMovimientodo.Name = "idCorteMovimientodo";
            this.idCorteMovimientodo.ReadOnly = true;
            this.idCorteMovimientodo.Visible = false;
            // 
            // codigo
            // 
            this.codigo.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.ColumnHeader;
            this.codigo.DataPropertyName = "codigo";
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
            this.codigo.DefaultCellStyle = dataGridViewCellStyle2;
            this.codigo.FillWeight = 40F;
            this.codigo.HeaderText = "Código";
            this.codigo.MinimumWidth = 80;
            this.codigo.Name = "codigo";
            this.codigo.ReadOnly = true;
            this.codigo.Width = 80;
            // 
            // corte
            // 
            this.corte.DataPropertyName = "corte";
            this.corte.FillWeight = 28.74658F;
            this.corte.HeaderText = "Prod.";
            this.corte.Name = "corte";
            this.corte.ReadOnly = true;
            // 
            // cantUnidad
            // 
            this.cantUnidad.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.ColumnHeader;
            this.cantUnidad.DataPropertyName = "cantUnidad";
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
            this.cantUnidad.DefaultCellStyle = dataGridViewCellStyle3;
            this.cantUnidad.FillWeight = 12.66835F;
            this.cantUnidad.HeaderText = "Cant. Un.";
            this.cantUnidad.Name = "cantUnidad";
            this.cantUnidad.ReadOnly = true;
            this.cantUnidad.Width = 77;
            // 
            // cantKg
            // 
            this.cantKg.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.ColumnHeader;
            this.cantKg.DataPropertyName = "cantKg";
            dataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
            dataGridViewCellStyle4.Format = "N3";
            dataGridViewCellStyle4.NullValue = null;
            this.cantKg.DefaultCellStyle = dataGridViewCellStyle4;
            this.cantKg.FillWeight = 17.74753F;
            this.cantKg.HeaderText = "Cant. Kgs";
            this.cantKg.Name = "cantKg";
            this.cantKg.ReadOnly = true;
            this.cantKg.Width = 78;
            // 
            // Balanza
            // 
            this.Balanza.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.ColumnHeader;
            this.Balanza.DataPropertyName = "pesoBalanza";
            this.Balanza.FillWeight = 9.68047F;
            this.Balanza.HeaderText = "Balanza";
            this.Balanza.Name = "Balanza";
            this.Balanza.ReadOnly = true;
            this.Balanza.Width = 51;
            // 
            // permitirIngreso
            // 
            this.permitirIngreso.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.ColumnHeader;
            this.permitirIngreso.DataPropertyName = "permitirIngreso";
            this.permitirIngreso.FillWeight = 25F;
            this.permitirIngreso.HeaderText = "Permitir";
            this.permitirIngreso.Name = "permitirIngreso";
            this.permitirIngreso.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.permitirIngreso.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            this.permitirIngreso.Width = 66;
            // 
            // formInfoMovimiento
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Bisque;
            this.ClientSize = new System.Drawing.Size(605, 591);
            this.Controls.Add(this.idMovimientoLabel);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.txtActualizadoPor);
            this.Controls.Add(this.txtActualizado);
            this.Controls.Add(this.label13);
            this.Controls.Add(this.txtCreadoPor);
            this.Controls.Add(this.txtCreado);
            this.Controls.Add(this.label11);
            this.Controls.Add(this.txtObservaciones);
            this.Controls.Add(this.txtCantItems);
            this.Controls.Add(this.label12);
            this.Controls.Add(this.txtTotalKg);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.grillaCortesPorMovimiento);
            this.Controls.Add(this.barraControl);
            this.Controls.Add(this.pnlBuscar);
            this.Controls.Add(this.btnSalir);
            this.Controls.Add(this.pnlEliminado);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "formInfoMovimiento";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Información Movimiento";
            this.Load += new System.EventHandler(this.formInfoMovimiento_Load);
            this.barraControl.ResumeLayout(false);
            this.barraControl.PerformLayout();
            this.pnlBuscar.ResumeLayout(false);
            this.pnlBuscar.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.grillaCortesPorMovimiento)).EndInit();
            this.pnlEliminado.ResumeLayout(false);
            this.pnlEliminado.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        protected internal System.Windows.Forms.ToolStrip barraControl;
        protected System.Windows.Forms.ToolStripButton modificar;
        protected System.Windows.Forms.Panel pnlBuscar;
        protected System.Windows.Forms.Label label6;
        protected System.Windows.Forms.TextBox txtSucDestino;
        protected System.Windows.Forms.TextBox txtSucOrigen;
        protected System.Windows.Forms.Label label3;
        protected System.Windows.Forms.Label label4;
        protected System.Windows.Forms.Button btnSalir;
        private System.Windows.Forms.TextBox txtObservaciones;
        private System.Windows.Forms.TextBox txtCantItems;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.TextBox txtTotalKg;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.DataGridView grillaCortesPorMovimiento;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.ToolStripButton Reporte;
        private System.Windows.Forms.TextBox txtFechaMovimiento;
        protected System.Windows.Forms.ToolStripButton Imprimir;
        protected System.Windows.Forms.GroupBox groupBox1;
        protected System.Windows.Forms.ToolStripButton eliminar;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Panel pnlEliminado;
        protected System.Windows.Forms.Label label10;
        protected System.Windows.Forms.Label label9;
        protected System.Windows.Forms.Label lblIdDestino;
        protected System.Windows.Forms.Label lblIdOrigen;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox txtActualizadoPor;
        private System.Windows.Forms.TextBox txtActualizado;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.TextBox txtCreadoPor;
        private System.Windows.Forms.TextBox txtCreado;
        private System.Windows.Forms.Label idMovimientoLabel;
        private System.Windows.Forms.ToolStripButton verAcum;
        private System.Windows.Forms.DataGridViewTextBoxColumn idCorteMovimientodo;
        private System.Windows.Forms.DataGridViewTextBoxColumn codigo;
        private System.Windows.Forms.DataGridViewTextBoxColumn corte;
        private System.Windows.Forms.DataGridViewTextBoxColumn cantUnidad;
        private System.Windows.Forms.DataGridViewTextBoxColumn cantKg;
        private System.Windows.Forms.DataGridViewCheckBoxColumn Balanza;
        private System.Windows.Forms.DataGridViewCheckBoxColumn permitirIngreso;
    }
}