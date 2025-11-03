namespace Presentacion
{
    partial class formInfoCorte
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle11 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle12 = new System.Windows.Forms.DataGridViewCellStyle();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(formInfoCorte));
            this.btnSalir = new System.Windows.Forms.Button();
            this.pnlBuscar = new System.Windows.Forms.Panel();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.lblPresentacion = new System.Windows.Forms.Label();
            this.panelDesperdicio = new System.Windows.Forms.Panel();
            this.label12 = new System.Windows.Forms.Label();
            this.label18 = new System.Windows.Forms.Label();
            this.txtPorcHueso = new System.Windows.Forms.MaskedTextBox();
            this.txtDesvioEstandar = new System.Windows.Forms.MaskedTextBox();
            this.checkPesable = new System.Windows.Forms.CheckBox();
            this.label10 = new System.Windows.Forms.Label();
            this.txtMarca = new System.Windows.Forms.TextBox();
            this.groupProveedores = new System.Windows.Forms.GroupBox();
            this.grillaProveedores = new System.Windows.Forms.DataGridView();
            this.dataGridViewTextBoxColumn3 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ultimoPrecio = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.fechaUltimaCompra = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.txtPuntoStock = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.txtNivel = new System.Windows.Forms.TextBox();
            this.label17 = new System.Windows.Forms.Label();
            this.txtIndependiente = new System.Windows.Forms.CheckBox();
            this.checkIngresoRapidoEmbutido = new System.Windows.Forms.CheckBox();
            this.checkEnCierreStock = new System.Windows.Forms.CheckBox();
            this.txtAlicuotaIva = new System.Windows.Forms.TextBox();
            this.label14 = new System.Windows.Forms.Label();
            this.checkHabilitado = new System.Windows.Forms.CheckBox();
            this.txtPromedio = new System.Windows.Forms.TextBox();
            this.label16 = new System.Windows.Forms.Label();
            this.label15 = new System.Windows.Forms.Label();
            this.txtIdCorte = new System.Windows.Forms.TextBox();
            this.label13 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.txtPorcentajeCorte = new System.Windows.Forms.MaskedTextBox();
            this.txtPrecioKg = new System.Windows.Forms.MaskedTextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.txtTipo = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.lblPorc_Pres = new System.Windows.Forms.Label();
            this.txtCodigo = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.txtCorteMaestro = new System.Windows.Forms.TextBox();
            this.lblCorteMaestro = new System.Windows.Forms.Label();
            this.txtDescCorte = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.barraControl = new System.Windows.Forms.ToolStrip();
            this.modificar = new System.Windows.Forms.ToolStripButton();
            this.eliminar = new System.Windows.Forms.ToolStripButton();
            this.label2 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.pnlBuscar.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.panelDesperdicio.SuspendLayout();
            this.groupProveedores.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.grillaProveedores)).BeginInit();
            this.barraControl.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnSalir
            // 
            this.btnSalir.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSalir.Location = new System.Drawing.Point(543, 541);
            this.btnSalir.Name = "btnSalir";
            this.btnSalir.Size = new System.Drawing.Size(82, 27);
            this.btnSalir.TabIndex = 18;
            this.btnSalir.Text = "&Salir";
            this.btnSalir.UseVisualStyleBackColor = true;
            this.btnSalir.Click += new System.EventHandler(this.btnSalir_Click);
            // 
            // pnlBuscar
            // 
            this.pnlBuscar.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.pnlBuscar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(43)))), ((int)(((byte)(77)))), ((int)(((byte)(129)))));
            this.pnlBuscar.Controls.Add(this.groupBox1);
            this.pnlBuscar.Location = new System.Drawing.Point(0, 43);
            this.pnlBuscar.Name = "pnlBuscar";
            this.pnlBuscar.Size = new System.Drawing.Size(638, 492);
            this.pnlBuscar.TabIndex = 17;
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(43)))), ((int)(((byte)(77)))), ((int)(((byte)(129)))));
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.label7);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.lblPresentacion);
            this.groupBox1.Controls.Add(this.panelDesperdicio);
            this.groupBox1.Controls.Add(this.checkPesable);
            this.groupBox1.Controls.Add(this.label10);
            this.groupBox1.Controls.Add(this.txtMarca);
            this.groupBox1.Controls.Add(this.groupProveedores);
            this.groupBox1.Controls.Add(this.txtPuntoStock);
            this.groupBox1.Controls.Add(this.label8);
            this.groupBox1.Controls.Add(this.txtNivel);
            this.groupBox1.Controls.Add(this.label17);
            this.groupBox1.Controls.Add(this.txtIndependiente);
            this.groupBox1.Controls.Add(this.checkIngresoRapidoEmbutido);
            this.groupBox1.Controls.Add(this.checkEnCierreStock);
            this.groupBox1.Controls.Add(this.txtAlicuotaIva);
            this.groupBox1.Controls.Add(this.label14);
            this.groupBox1.Controls.Add(this.checkHabilitado);
            this.groupBox1.Controls.Add(this.txtPromedio);
            this.groupBox1.Controls.Add(this.label16);
            this.groupBox1.Controls.Add(this.label15);
            this.groupBox1.Controls.Add(this.txtIdCorte);
            this.groupBox1.Controls.Add(this.label13);
            this.groupBox1.Controls.Add(this.label9);
            this.groupBox1.Controls.Add(this.txtPorcentajeCorte);
            this.groupBox1.Controls.Add(this.txtPrecioKg);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.txtTipo);
            this.groupBox1.Controls.Add(this.label6);
            this.groupBox1.Controls.Add(this.lblPorc_Pres);
            this.groupBox1.Controls.Add(this.txtCodigo);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.txtCorteMaestro);
            this.groupBox1.Controls.Add(this.lblCorteMaestro);
            this.groupBox1.Controls.Add(this.txtDescCorte);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox1.ForeColor = System.Drawing.Color.Cornsilk;
            this.groupBox1.Location = new System.Drawing.Point(12, 3);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(613, 486);
            this.groupBox1.TabIndex = 14;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Prod.";
            // 
            // lblPresentacion
            // 
            this.lblPresentacion.AutoSize = true;
            this.lblPresentacion.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblPresentacion.ForeColor = System.Drawing.Color.Cornsilk;
            this.lblPresentacion.Location = new System.Drawing.Point(347, 134);
            this.lblPresentacion.Name = "lblPresentacion";
            this.lblPresentacion.Size = new System.Drawing.Size(96, 15);
            this.lblPresentacion.TabIndex = 64;
            this.lblPresentacion.Text = "Presentación de";
            // 
            // panelDesperdicio
            // 
            this.panelDesperdicio.Controls.Add(this.label12);
            this.panelDesperdicio.Controls.Add(this.label18);
            this.panelDesperdicio.Controls.Add(this.txtPorcHueso);
            this.panelDesperdicio.Controls.Add(this.txtDesvioEstandar);
            this.panelDesperdicio.Location = new System.Drawing.Point(339, 182);
            this.panelDesperdicio.Name = "panelDesperdicio";
            this.panelDesperdicio.Size = new System.Drawing.Size(193, 51);
            this.panelDesperdicio.TabIndex = 63;
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label12.ForeColor = System.Drawing.Color.Cornsilk;
            this.label12.Location = new System.Drawing.Point(8, 32);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(96, 15);
            this.label12.TabIndex = 41;
            this.label12.Text = "Desvío Estandar";
            // 
            // label18
            // 
            this.label18.AutoSize = true;
            this.label18.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label18.ForeColor = System.Drawing.Color.Cornsilk;
            this.label18.Location = new System.Drawing.Point(17, 5);
            this.label18.Name = "label18";
            this.label18.Size = new System.Drawing.Size(87, 15);
            this.label18.TabIndex = 40;
            this.label18.Text = "% Desperdicio";
            // 
            // txtPorcHueso
            // 
            this.txtPorcHueso.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.txtPorcHueso.Location = new System.Drawing.Point(110, 2);
            this.txtPorcHueso.Name = "txtPorcHueso";
            this.txtPorcHueso.ReadOnly = true;
            this.txtPorcHueso.Size = new System.Drawing.Size(75, 21);
            this.txtPorcHueso.TabIndex = 27;
            this.txtPorcHueso.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // txtDesvioEstandar
            // 
            this.txtDesvioEstandar.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.txtDesvioEstandar.Location = new System.Drawing.Point(110, 29);
            this.txtDesvioEstandar.Name = "txtDesvioEstandar";
            this.txtDesvioEstandar.ReadOnly = true;
            this.txtDesvioEstandar.Size = new System.Drawing.Size(75, 21);
            this.txtDesvioEstandar.TabIndex = 31;
            this.txtDesvioEstandar.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // checkPesable
            // 
            this.checkPesable.AutoSize = true;
            this.checkPesable.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.checkPesable.Checked = true;
            this.checkPesable.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkPesable.Enabled = false;
            this.checkPesable.Location = new System.Drawing.Point(80, 183);
            this.checkPesable.Name = "checkPesable";
            this.checkPesable.Size = new System.Drawing.Size(15, 14);
            this.checkPesable.TabIndex = 62;
            this.checkPesable.UseVisualStyleBackColor = true;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label10.ForeColor = System.Drawing.Color.Cornsilk;
            this.label10.Location = new System.Drawing.Point(32, 131);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(42, 15);
            this.label10.TabIndex = 61;
            this.label10.Text = "Marca";
            // 
            // txtMarca
            // 
            this.txtMarca.Location = new System.Drawing.Point(79, 128);
            this.txtMarca.Name = "txtMarca";
            this.txtMarca.ReadOnly = true;
            this.txtMarca.Size = new System.Drawing.Size(132, 21);
            this.txtMarca.TabIndex = 60;
            this.txtMarca.TabStop = false;
            // 
            // groupProveedores
            // 
            this.groupProveedores.Controls.Add(this.grillaProveedores);
            this.groupProveedores.ForeColor = System.Drawing.Color.Cornsilk;
            this.groupProveedores.Location = new System.Drawing.Point(8, 323);
            this.groupProveedores.Name = "groupProveedores";
            this.groupProveedores.Size = new System.Drawing.Size(599, 154);
            this.groupProveedores.TabIndex = 59;
            this.groupProveedores.TabStop = false;
            this.groupProveedores.Text = "Proveedores";
            // 
            // grillaProveedores
            // 
            this.grillaProveedores.AllowUserToAddRows = false;
            this.grillaProveedores.AllowUserToDeleteRows = false;
            this.grillaProveedores.AllowUserToOrderColumns = true;
            this.grillaProveedores.AllowUserToResizeRows = false;
            this.grillaProveedores.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.grillaProveedores.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.grillaProveedores.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.grillaProveedores.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.dataGridViewTextBoxColumn3,
            this.ultimoPrecio,
            this.fechaUltimaCompra});
            this.grillaProveedores.Location = new System.Drawing.Point(7, 26);
            this.grillaProveedores.MultiSelect = false;
            this.grillaProveedores.Name = "grillaProveedores";
            this.grillaProveedores.ReadOnly = true;
            this.grillaProveedores.RowHeadersVisible = false;
            this.grillaProveedores.RowHeadersWidth = 51;
            this.grillaProveedores.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.grillaProveedores.Size = new System.Drawing.Size(586, 122);
            this.grillaProveedores.TabIndex = 54;
            this.grillaProveedores.TabStop = false;
            // 
            // dataGridViewTextBoxColumn3
            // 
            this.dataGridViewTextBoxColumn3.DataPropertyName = "razonSocial";
            dataGridViewCellStyle11.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold);
            this.dataGridViewTextBoxColumn3.DefaultCellStyle = dataGridViewCellStyle11;
            this.dataGridViewTextBoxColumn3.FillWeight = 113.0288F;
            this.dataGridViewTextBoxColumn3.HeaderText = "Razon Social";
            this.dataGridViewTextBoxColumn3.MinimumWidth = 6;
            this.dataGridViewTextBoxColumn3.Name = "dataGridViewTextBoxColumn3";
            this.dataGridViewTextBoxColumn3.ReadOnly = true;
            // 
            // ultimoPrecio
            // 
            this.ultimoPrecio.DataPropertyName = "ultimoPrecio";
            dataGridViewCellStyle12.Format = "F2";
            this.ultimoPrecio.DefaultCellStyle = dataGridViewCellStyle12;
            this.ultimoPrecio.HeaderText = "Ultimo Precio";
            this.ultimoPrecio.MinimumWidth = 6;
            this.ultimoPrecio.Name = "ultimoPrecio";
            this.ultimoPrecio.ReadOnly = true;
            // 
            // fechaUltimaCompra
            // 
            this.fechaUltimaCompra.DataPropertyName = "fechaUltimaCompra";
            this.fechaUltimaCompra.HeaderText = "Fec.Compra";
            this.fechaUltimaCompra.MinimumWidth = 6;
            this.fechaUltimaCompra.Name = "fechaUltimaCompra";
            this.fechaUltimaCompra.ReadOnly = true;
            // 
            // txtPuntoStock
            // 
            this.txtPuntoStock.Location = new System.Drawing.Point(79, 260);
            this.txtPuntoStock.Name = "txtPuntoStock";
            this.txtPuntoStock.ReadOnly = true;
            this.txtPuntoStock.Size = new System.Drawing.Size(75, 21);
            this.txtPuntoStock.TabIndex = 57;
            this.txtPuntoStock.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label8.ForeColor = System.Drawing.Color.Cornsilk;
            this.label8.Location = new System.Drawing.Point(1, 262);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(72, 15);
            this.label8.TabIndex = 58;
            this.label8.Text = "Punto Stock";
            // 
            // txtNivel
            // 
            this.txtNivel.Location = new System.Drawing.Point(79, 286);
            this.txtNivel.Name = "txtNivel";
            this.txtNivel.ReadOnly = true;
            this.txtNivel.Size = new System.Drawing.Size(75, 21);
            this.txtNivel.TabIndex = 45;
            this.txtNivel.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label17
            // 
            this.label17.AutoSize = true;
            this.label17.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label17.ForeColor = System.Drawing.Color.Cornsilk;
            this.label17.Location = new System.Drawing.Point(41, 288);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(34, 15);
            this.label17.TabIndex = 46;
            this.label17.Text = "Nivel";
            // 
            // txtIndependiente
            // 
            this.txtIndependiente.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.txtIndependiente.AutoSize = true;
            this.txtIndependiente.Enabled = false;
            this.txtIndependiente.ForeColor = System.Drawing.Color.Transparent;
            this.txtIndependiente.Location = new System.Drawing.Point(446, 76);
            this.txtIndependiente.Name = "txtIndependiente";
            this.txtIndependiente.Size = new System.Drawing.Size(15, 14);
            this.txtIndependiente.TabIndex = 29;
            this.txtIndependiente.UseVisualStyleBackColor = true;
            // 
            // checkIngresoRapidoEmbutido
            // 
            this.checkIngresoRapidoEmbutido.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.checkIngresoRapidoEmbutido.AutoSize = true;
            this.checkIngresoRapidoEmbutido.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.checkIngresoRapidoEmbutido.Enabled = false;
            this.checkIngresoRapidoEmbutido.Location = new System.Drawing.Point(445, 23);
            this.checkIngresoRapidoEmbutido.Name = "checkIngresoRapidoEmbutido";
            this.checkIngresoRapidoEmbutido.Size = new System.Drawing.Size(15, 14);
            this.checkIngresoRapidoEmbutido.TabIndex = 44;
            this.checkIngresoRapidoEmbutido.TabStop = false;
            this.checkIngresoRapidoEmbutido.UseVisualStyleBackColor = true;
            this.checkIngresoRapidoEmbutido.Paint += new System.Windows.Forms.PaintEventHandler(this.checkIngresoRapidoEmbutido_Paint);
            // 
            // checkEnCierreStock
            // 
            this.checkEnCierreStock.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.checkEnCierreStock.AutoSize = true;
            this.checkEnCierreStock.BackColor = System.Drawing.Color.Transparent;
            this.checkEnCierreStock.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.checkEnCierreStock.Enabled = false;
            this.checkEnCierreStock.Location = new System.Drawing.Point(445, 50);
            this.checkEnCierreStock.Name = "checkEnCierreStock";
            this.checkEnCierreStock.Size = new System.Drawing.Size(15, 14);
            this.checkEnCierreStock.TabIndex = 36;
            this.checkEnCierreStock.UseVisualStyleBackColor = false;
            // 
            // txtAlicuotaIva
            // 
            this.txtAlicuotaIva.Location = new System.Drawing.Point(79, 207);
            this.txtAlicuotaIva.Name = "txtAlicuotaIva";
            this.txtAlicuotaIva.ReadOnly = true;
            this.txtAlicuotaIva.Size = new System.Drawing.Size(75, 21);
            this.txtAlicuotaIva.TabIndex = 43;
            this.txtAlicuotaIva.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label14.ForeColor = System.Drawing.Color.Cornsilk;
            this.label14.Location = new System.Drawing.Point(6, 210);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(68, 15);
            this.label14.TabIndex = 42;
            this.label14.Text = "Alícuota Iva";
            // 
            // checkHabilitado
            // 
            this.checkHabilitado.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.checkHabilitado.AutoSize = true;
            this.checkHabilitado.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.checkHabilitado.Enabled = false;
            this.checkHabilitado.Location = new System.Drawing.Point(445, 102);
            this.checkHabilitado.Name = "checkHabilitado";
            this.checkHabilitado.Size = new System.Drawing.Size(15, 14);
            this.checkHabilitado.TabIndex = 41;
            this.checkHabilitado.UseVisualStyleBackColor = true;
            this.checkHabilitado.Paint += new System.Windows.Forms.PaintEventHandler(this.checkIngresoRapidoEmbutido_Paint);
            // 
            // txtPromedio
            // 
            this.txtPromedio.Location = new System.Drawing.Point(79, 234);
            this.txtPromedio.Name = "txtPromedio";
            this.txtPromedio.ReadOnly = true;
            this.txtPromedio.Size = new System.Drawing.Size(75, 21);
            this.txtPromedio.TabIndex = 39;
            this.txtPromedio.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label16
            // 
            this.label16.AutoSize = true;
            this.label16.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label16.ForeColor = System.Drawing.Color.Cornsilk;
            this.label16.Location = new System.Drawing.Point(12, 236);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(61, 15);
            this.label16.TabIndex = 40;
            this.label16.Text = "Promedio";
            // 
            // label15
            // 
            this.label15.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label15.AutoSize = true;
            this.label15.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label15.ForeColor = System.Drawing.Color.Cornsilk;
            this.label15.Location = new System.Drawing.Point(352, 49);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(91, 15);
            this.label15.TabIndex = 38;
            this.label15.Text = "En Cierre Stock";
            // 
            // txtIdCorte
            // 
            this.txtIdCorte.Enabled = false;
            this.txtIdCorte.Location = new System.Drawing.Point(79, 21);
            this.txtIdCorte.Name = "txtIdCorte";
            this.txtIdCorte.ReadOnly = true;
            this.txtIdCorte.Size = new System.Drawing.Size(132, 21);
            this.txtIdCorte.TabIndex = 34;
            this.txtIdCorte.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label13.ForeColor = System.Drawing.Color.Cornsilk;
            this.label13.Location = new System.Drawing.Point(23, 24);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(49, 15);
            this.label13.TabIndex = 33;
            this.label13.Text = "Id Prod.";
            // 
            // label9
            // 
            this.label9.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label9.AutoSize = true;
            this.label9.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label9.ForeColor = System.Drawing.Color.Cornsilk;
            this.label9.Location = new System.Drawing.Point(325, 75);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(118, 15);
            this.label9.TabIndex = 30;
            this.label9.Text = "Prod. Independiente";
            // 
            // txtPorcentajeCorte
            // 
            this.txtPorcentajeCorte.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.txtPorcentajeCorte.Location = new System.Drawing.Point(449, 158);
            this.txtPorcentajeCorte.Name = "txtPorcentajeCorte";
            this.txtPorcentajeCorte.ReadOnly = true;
            this.txtPorcentajeCorte.Size = new System.Drawing.Size(75, 21);
            this.txtPorcentajeCorte.TabIndex = 26;
            this.txtPorcentajeCorte.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // txtPrecioKg
            // 
            this.txtPrecioKg.Location = new System.Drawing.Point(79, 102);
            this.txtPrecioKg.Name = "txtPrecioKg";
            this.txtPrecioKg.ReadOnly = true;
            this.txtPrecioKg.Size = new System.Drawing.Size(132, 21);
            this.txtPrecioKg.TabIndex = 25;
            this.txtPrecioKg.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.ForeColor = System.Drawing.Color.Cornsilk;
            this.label5.Location = new System.Drawing.Point(10, 105);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(63, 15);
            this.label5.TabIndex = 23;
            this.label5.Text = "Precio Kg.";
            // 
            // txtTipo
            // 
            this.txtTipo.Location = new System.Drawing.Point(79, 154);
            this.txtTipo.Name = "txtTipo";
            this.txtTipo.ReadOnly = true;
            this.txtTipo.Size = new System.Drawing.Size(132, 21);
            this.txtTipo.TabIndex = 22;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.ForeColor = System.Drawing.Color.Cornsilk;
            this.label6.Location = new System.Drawing.Point(41, 157);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(31, 15);
            this.label6.TabIndex = 21;
            this.label6.Text = "Tipo";
            // 
            // lblPorc_Pres
            // 
            this.lblPorc_Pres.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblPorc_Pres.AutoSize = true;
            this.lblPorc_Pres.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblPorc_Pres.ForeColor = System.Drawing.Color.Cornsilk;
            this.lblPorc_Pres.Location = new System.Drawing.Point(362, 161);
            this.lblPorc_Pres.Name = "lblPorc_Pres";
            this.lblPorc_Pres.Size = new System.Drawing.Size(81, 15);
            this.lblPorc_Pres.TabIndex = 15;
            this.lblPorc_Pres.Text = "% en Prod. M";
            // 
            // txtCodigo
            // 
            this.txtCodigo.Location = new System.Drawing.Point(79, 48);
            this.txtCodigo.Name = "txtCodigo";
            this.txtCodigo.ReadOnly = true;
            this.txtCodigo.Size = new System.Drawing.Size(132, 21);
            this.txtCodigo.TabIndex = 7;
            this.txtCodigo.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ForeColor = System.Drawing.Color.Cornsilk;
            this.label1.Location = new System.Drawing.Point(27, 51);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(46, 15);
            this.label1.TabIndex = 6;
            this.label1.Text = "Código";
            // 
            // txtCorteMaestro
            // 
            this.txtCorteMaestro.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.txtCorteMaestro.Location = new System.Drawing.Point(449, 131);
            this.txtCorteMaestro.Name = "txtCorteMaestro";
            this.txtCorteMaestro.ReadOnly = true;
            this.txtCorteMaestro.Size = new System.Drawing.Size(158, 21);
            this.txtCorteMaestro.TabIndex = 5;
            // 
            // lblCorteMaestro
            // 
            this.lblCorteMaestro.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblCorteMaestro.AutoSize = true;
            this.lblCorteMaestro.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblCorteMaestro.ForeColor = System.Drawing.Color.Cornsilk;
            this.lblCorteMaestro.Location = new System.Drawing.Point(359, 134);
            this.lblCorteMaestro.Name = "lblCorteMaestro";
            this.lblCorteMaestro.Size = new System.Drawing.Size(84, 15);
            this.lblCorteMaestro.TabIndex = 4;
            this.lblCorteMaestro.Text = "Prod. Maestro";
            // 
            // txtDescCorte
            // 
            this.txtDescCorte.Location = new System.Drawing.Point(79, 75);
            this.txtDescCorte.Name = "txtDescCorte";
            this.txtDescCorte.ReadOnly = true;
            this.txtDescCorte.Size = new System.Drawing.Size(204, 21);
            this.txtDescCorte.TabIndex = 3;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.ForeColor = System.Drawing.Color.Cornsilk;
            this.label4.Location = new System.Drawing.Point(36, 78);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(36, 15);
            this.label4.TabIndex = 2;
            this.label4.Text = "Prod.";
            // 
            // barraControl
            // 
            this.barraControl.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.barraControl.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.barraControl.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.modificar,
            this.eliminar});
            this.barraControl.Location = new System.Drawing.Point(0, 0);
            this.barraControl.Name = "barraControl";
            this.barraControl.Padding = new System.Windows.Forms.Padding(10, 0, 0, 0);
            this.barraControl.Size = new System.Drawing.Size(638, 44);
            this.barraControl.TabIndex = 7;
            this.barraControl.TabStop = true;
            this.barraControl.Text = "toolStrip1";
            // 
            // modificar
            // 
            this.modificar.Image = ((System.Drawing.Image)(resources.GetObject("modificar.Image")));
            this.modificar.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.modificar.Margin = new System.Windows.Forms.Padding(0, 2, 0, 1);
            this.modificar.Name = "modificar";
            this.modificar.Padding = new System.Windows.Forms.Padding(1);
            this.modificar.Size = new System.Drawing.Size(64, 41);
            this.modificar.Text = "&Modificar";
            this.modificar.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.modificar.Click += new System.EventHandler(this.modificar_Click);
            // 
            // eliminar
            // 
            this.eliminar.Image = ((System.Drawing.Image)(resources.GetObject("eliminar.Image")));
            this.eliminar.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.eliminar.Margin = new System.Windows.Forms.Padding(0, 2, 0, 1);
            this.eliminar.Name = "eliminar";
            this.eliminar.Padding = new System.Windows.Forms.Padding(1);
            this.eliminar.Size = new System.Drawing.Size(56, 41);
            this.eliminar.Text = "&Eliminar";
            this.eliminar.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.eliminar.Click += new System.EventHandler(this.eliminar_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.ForeColor = System.Drawing.Color.Cornsilk;
            this.label2.Location = new System.Drawing.Point(380, 100);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(63, 15);
            this.label2.TabIndex = 42;
            this.label2.Text = "Habilitado";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label7.ForeColor = System.Drawing.Color.Cornsilk;
            this.label7.Location = new System.Drawing.Point(292, 23);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(151, 15);
            this.label7.TabIndex = 65;
            this.label7.Text = "Ingreso Rápido Elaborado";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.ForeColor = System.Drawing.Color.Cornsilk;
            this.label3.Location = new System.Drawing.Point(22, 182);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(52, 15);
            this.label3.TabIndex = 66;
            this.label3.Text = "Pesable";
            // 
            // formInfoCorte
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(638, 572);
            this.Controls.Add(this.btnSalir);
            this.Controls.Add(this.pnlBuscar);
            this.Controls.Add(this.barraControl);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MaximizeBox = false;
            this.Name = "formInfoCorte";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Información Prod.";
            this.Load += new System.EventHandler(this.formInfoCorte_Load);
            this.pnlBuscar.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.panelDesperdicio.ResumeLayout(false);
            this.panelDesperdicio.PerformLayout();
            this.groupProveedores.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.grillaProveedores)).EndInit();
            this.barraControl.ResumeLayout(false);
            this.barraControl.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        protected internal System.Windows.Forms.ToolStrip barraControl;
        protected System.Windows.Forms.ToolStripButton modificar;
        protected System.Windows.Forms.GroupBox groupBox1;
        protected System.Windows.Forms.Label lblPorc_Pres;
        protected System.Windows.Forms.TextBox txtCodigo;
        protected System.Windows.Forms.Label label1;
        protected System.Windows.Forms.TextBox txtCorteMaestro;
        protected System.Windows.Forms.Label lblCorteMaestro;
        protected System.Windows.Forms.TextBox txtDescCorte;
        protected System.Windows.Forms.Label label4;
        protected System.Windows.Forms.Button btnSalir;
        protected System.Windows.Forms.Panel pnlBuscar;
        protected System.Windows.Forms.Label label6;
        protected System.Windows.Forms.TextBox txtTipo;
        protected System.Windows.Forms.Label label5;
        private System.Windows.Forms.MaskedTextBox txtPrecioKg;
        private System.Windows.Forms.MaskedTextBox txtPorcentajeCorte;
        private System.Windows.Forms.MaskedTextBox txtPorcHueso;
        protected System.Windows.Forms.Label label9;
        private System.Windows.Forms.CheckBox txtIndependiente;
        protected System.Windows.Forms.ToolStripButton eliminar;
        private System.Windows.Forms.MaskedTextBox txtDesvioEstandar;
        protected System.Windows.Forms.TextBox txtIdCorte;
        protected System.Windows.Forms.Label label13;
        private System.Windows.Forms.CheckBox checkEnCierreStock;
        protected System.Windows.Forms.Label label15;
        protected System.Windows.Forms.TextBox txtPromedio;
        protected System.Windows.Forms.Label label16;
        private System.Windows.Forms.CheckBox checkHabilitado;
        protected System.Windows.Forms.TextBox txtAlicuotaIva;
        protected System.Windows.Forms.Label label14;
        private System.Windows.Forms.CheckBox checkIngresoRapidoEmbutido;
        protected System.Windows.Forms.TextBox txtNivel;
        protected System.Windows.Forms.Label label17;
        protected System.Windows.Forms.TextBox txtPuntoStock;
        protected System.Windows.Forms.Label label8;
        private System.Windows.Forms.GroupBox groupProveedores;
        private System.Windows.Forms.DataGridView grillaProveedores;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn3;
        private System.Windows.Forms.DataGridViewTextBoxColumn ultimoPrecio;
        private System.Windows.Forms.DataGridViewTextBoxColumn fechaUltimaCompra;
        protected System.Windows.Forms.Label label10;
        protected System.Windows.Forms.TextBox txtMarca;
        private System.Windows.Forms.CheckBox checkPesable;
        private System.Windows.Forms.Panel panelDesperdicio;
        protected System.Windows.Forms.Label label12;
        protected System.Windows.Forms.Label label18;
        protected System.Windows.Forms.Label lblPresentacion;
        protected System.Windows.Forms.Label label3;
        protected System.Windows.Forms.Label label7;
        protected System.Windows.Forms.Label label2;
    }
}