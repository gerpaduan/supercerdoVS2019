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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(formInfoCorte));
            this.btnSalir = new System.Windows.Forms.Button();
            this.pnlBuscar = new System.Windows.Forms.Panel();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
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
            this.txtDesvioEstandar = new System.Windows.Forms.MaskedTextBox();
            this.label11 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.txtPorcHueso = new System.Windows.Forms.MaskedTextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.txtPorcentajeCorte = new System.Windows.Forms.MaskedTextBox();
            this.txtPrecioKg = new System.Windows.Forms.MaskedTextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.txtTipo = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.txtCodigo = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.txtCorteMaestro = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.txtDescCorte = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.barraControl = new System.Windows.Forms.ToolStrip();
            this.modificar = new System.Windows.Forms.ToolStripButton();
            this.eliminar = new System.Windows.Forms.ToolStripButton();
            this.pnlBuscar.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.barraControl.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnSalir
            // 
            this.btnSalir.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSalir.Location = new System.Drawing.Point(724, 362);
            this.btnSalir.Margin = new System.Windows.Forms.Padding(4);
            this.btnSalir.Name = "btnSalir";
            this.btnSalir.Size = new System.Drawing.Size(109, 33);
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
            this.pnlBuscar.Location = new System.Drawing.Point(0, 49);
            this.pnlBuscar.Margin = new System.Windows.Forms.Padding(4);
            this.pnlBuscar.Name = "pnlBuscar";
            this.pnlBuscar.Size = new System.Drawing.Size(850, 305);
            this.pnlBuscar.TabIndex = 17;
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(43)))), ((int)(((byte)(77)))), ((int)(((byte)(129)))));
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
            this.groupBox1.Controls.Add(this.txtDesvioEstandar);
            this.groupBox1.Controls.Add(this.label11);
            this.groupBox1.Controls.Add(this.label9);
            this.groupBox1.Controls.Add(this.txtPorcHueso);
            this.groupBox1.Controls.Add(this.label7);
            this.groupBox1.Controls.Add(this.txtPorcentajeCorte);
            this.groupBox1.Controls.Add(this.txtPrecioKg);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.txtTipo);
            this.groupBox1.Controls.Add(this.label6);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.txtCodigo);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.txtCorteMaestro);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.txtDescCorte);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox1.ForeColor = System.Drawing.Color.Cornsilk;
            this.groupBox1.Location = new System.Drawing.Point(16, 4);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(4);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(4);
            this.groupBox1.Size = new System.Drawing.Size(817, 297);
            this.groupBox1.TabIndex = 14;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Corte";
            // 
            // txtNivel
            // 
            this.txtNivel.Location = new System.Drawing.Point(105, 259);
            this.txtNivel.Margin = new System.Windows.Forms.Padding(4);
            this.txtNivel.Name = "txtNivel";
            this.txtNivel.ReadOnly = true;
            this.txtNivel.Size = new System.Drawing.Size(99, 24);
            this.txtNivel.TabIndex = 45;
            this.txtNivel.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label17
            // 
            this.label17.AutoSize = true;
            this.label17.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label17.ForeColor = System.Drawing.Color.Cornsilk;
            this.label17.Location = new System.Drawing.Point(57, 262);
            this.label17.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(40, 18);
            this.label17.TabIndex = 46;
            this.label17.Text = "Nivel";
            // 
            // txtIndependiente
            // 
            this.txtIndependiente.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.txtIndependiente.AutoSize = true;
            this.txtIndependiente.Enabled = false;
            this.txtIndependiente.Location = new System.Drawing.Point(597, 95);
            this.txtIndependiente.Margin = new System.Windows.Forms.Padding(4);
            this.txtIndependiente.Name = "txtIndependiente";
            this.txtIndependiente.Size = new System.Drawing.Size(18, 17);
            this.txtIndependiente.TabIndex = 29;
            this.txtIndependiente.UseVisualStyleBackColor = true;
            // 
            // checkIngresoRapidoEmbutido
            // 
            this.checkIngresoRapidoEmbutido.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.checkIngresoRapidoEmbutido.AutoSize = true;
            this.checkIngresoRapidoEmbutido.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.checkIngresoRapidoEmbutido.Enabled = false;
            this.checkIngresoRapidoEmbutido.Location = new System.Drawing.Point(420, 28);
            this.checkIngresoRapidoEmbutido.Margin = new System.Windows.Forms.Padding(4);
            this.checkIngresoRapidoEmbutido.Name = "checkIngresoRapidoEmbutido";
            this.checkIngresoRapidoEmbutido.Size = new System.Drawing.Size(194, 22);
            this.checkIngresoRapidoEmbutido.TabIndex = 44;
            this.checkIngresoRapidoEmbutido.TabStop = false;
            this.checkIngresoRapidoEmbutido.Text = "IngresoRapidoElaborado";
            this.checkIngresoRapidoEmbutido.UseVisualStyleBackColor = true;
            this.checkIngresoRapidoEmbutido.Paint += new System.Windows.Forms.PaintEventHandler(this.checkIngresoRapidoEmbutido_Paint);
            // 
            // checkEnCierreStock
            // 
            this.checkEnCierreStock.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.checkEnCierreStock.AutoSize = true;
            this.checkEnCierreStock.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.checkEnCierreStock.Enabled = false;
            this.checkEnCierreStock.Location = new System.Drawing.Point(596, 63);
            this.checkEnCierreStock.Margin = new System.Windows.Forms.Padding(4);
            this.checkEnCierreStock.Name = "checkEnCierreStock";
            this.checkEnCierreStock.Size = new System.Drawing.Size(18, 17);
            this.checkEnCierreStock.TabIndex = 36;
            this.checkEnCierreStock.UseVisualStyleBackColor = true;
            // 
            // txtAlicuotaIva
            // 
            this.txtAlicuotaIva.Location = new System.Drawing.Point(105, 191);
            this.txtAlicuotaIva.Margin = new System.Windows.Forms.Padding(4);
            this.txtAlicuotaIva.Name = "txtAlicuotaIva";
            this.txtAlicuotaIva.ReadOnly = true;
            this.txtAlicuotaIva.Size = new System.Drawing.Size(93, 24);
            this.txtAlicuotaIva.TabIndex = 43;
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label14.ForeColor = System.Drawing.Color.Cornsilk;
            this.label14.Location = new System.Drawing.Point(15, 195);
            this.label14.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(82, 18);
            this.label14.TabIndex = 42;
            this.label14.Text = "Alícuota Iva";
            // 
            // checkHabilitado
            // 
            this.checkHabilitado.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.checkHabilitado.AutoSize = true;
            this.checkHabilitado.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.checkHabilitado.Enabled = false;
            this.checkHabilitado.Location = new System.Drawing.Point(519, 125);
            this.checkHabilitado.Margin = new System.Windows.Forms.Padding(4);
            this.checkHabilitado.Name = "checkHabilitado";
            this.checkHabilitado.Size = new System.Drawing.Size(95, 22);
            this.checkHabilitado.TabIndex = 41;
            this.checkHabilitado.Text = "Habilitado";
            this.checkHabilitado.UseVisualStyleBackColor = true;
            this.checkHabilitado.Paint += new System.Windows.Forms.PaintEventHandler(this.checkIngresoRapidoEmbutido_Paint);
            // 
            // txtPromedio
            // 
            this.txtPromedio.Location = new System.Drawing.Point(105, 227);
            this.txtPromedio.Margin = new System.Windows.Forms.Padding(4);
            this.txtPromedio.Name = "txtPromedio";
            this.txtPromedio.ReadOnly = true;
            this.txtPromedio.Size = new System.Drawing.Size(99, 24);
            this.txtPromedio.TabIndex = 39;
            this.txtPromedio.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label16
            // 
            this.label16.AutoSize = true;
            this.label16.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label16.ForeColor = System.Drawing.Color.Cornsilk;
            this.label16.Location = new System.Drawing.Point(16, 230);
            this.label16.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(73, 18);
            this.label16.TabIndex = 40;
            this.label16.Text = "Promedio";
            // 
            // label15
            // 
            this.label15.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label15.AutoSize = true;
            this.label15.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label15.ForeColor = System.Drawing.Color.Cornsilk;
            this.label15.Location = new System.Drawing.Point(466, 62);
            this.label15.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(113, 18);
            this.label15.TabIndex = 38;
            this.label15.Text = "En Cierre Stock";
            // 
            // txtIdCorte
            // 
            this.txtIdCorte.Enabled = false;
            this.txtIdCorte.Location = new System.Drawing.Point(105, 26);
            this.txtIdCorte.Margin = new System.Windows.Forms.Padding(4);
            this.txtIdCorte.Name = "txtIdCorte";
            this.txtIdCorte.ReadOnly = true;
            this.txtIdCorte.Size = new System.Drawing.Size(93, 24);
            this.txtIdCorte.TabIndex = 34;
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label13.ForeColor = System.Drawing.Color.Cornsilk;
            this.label13.Location = new System.Drawing.Point(31, 30);
            this.label13.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(60, 18);
            this.label13.TabIndex = 33;
            this.label13.Text = "Id Corte";
            // 
            // txtDesvioEstandar
            // 
            this.txtDesvioEstandar.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.txtDesvioEstandar.Location = new System.Drawing.Point(599, 261);
            this.txtDesvioEstandar.Margin = new System.Windows.Forms.Padding(4);
            this.txtDesvioEstandar.Name = "txtDesvioEstandar";
            this.txtDesvioEstandar.ReadOnly = true;
            this.txtDesvioEstandar.Size = new System.Drawing.Size(99, 24);
            this.txtDesvioEstandar.TabIndex = 31;
            this.txtDesvioEstandar.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label11
            // 
            this.label11.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label11.AutoSize = true;
            this.label11.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label11.ForeColor = System.Drawing.Color.Cornsilk;
            this.label11.Location = new System.Drawing.Point(463, 265);
            this.label11.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(117, 18);
            this.label11.TabIndex = 32;
            this.label11.Text = "Desvío Estandar";
            // 
            // label9
            // 
            this.label9.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label9.AutoSize = true;
            this.label9.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label9.ForeColor = System.Drawing.Color.Cornsilk;
            this.label9.Location = new System.Drawing.Point(440, 94);
            this.label9.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(139, 18);
            this.label9.TabIndex = 30;
            this.label9.Text = "Corte Independiente";
            // 
            // txtPorcHueso
            // 
            this.txtPorcHueso.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.txtPorcHueso.Location = new System.Drawing.Point(599, 228);
            this.txtPorcHueso.Margin = new System.Windows.Forms.Padding(4);
            this.txtPorcHueso.Name = "txtPorcHueso";
            this.txtPorcHueso.ReadOnly = true;
            this.txtPorcHueso.Size = new System.Drawing.Size(99, 24);
            this.txtPorcHueso.TabIndex = 27;
            this.txtPorcHueso.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label7
            // 
            this.label7.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label7.ForeColor = System.Drawing.Color.Cornsilk;
            this.label7.Location = new System.Drawing.Point(475, 232);
            this.label7.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(104, 18);
            this.label7.TabIndex = 28;
            this.label7.Text = "% Desperdicio";
            // 
            // txtPorcentajeCorte
            // 
            this.txtPorcentajeCorte.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.txtPorcentajeCorte.Location = new System.Drawing.Point(599, 195);
            this.txtPorcentajeCorte.Margin = new System.Windows.Forms.Padding(4);
            this.txtPorcentajeCorte.Name = "txtPorcentajeCorte";
            this.txtPorcentajeCorte.ReadOnly = true;
            this.txtPorcentajeCorte.Size = new System.Drawing.Size(99, 24);
            this.txtPorcentajeCorte.TabIndex = 26;
            this.txtPorcentajeCorte.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // txtPrecioKg
            // 
            this.txtPrecioKg.Location = new System.Drawing.Point(105, 126);
            this.txtPrecioKg.Margin = new System.Windows.Forms.Padding(4);
            this.txtPrecioKg.Name = "txtPrecioKg";
            this.txtPrecioKg.ReadOnly = true;
            this.txtPrecioKg.Size = new System.Drawing.Size(93, 24);
            this.txtPrecioKg.TabIndex = 25;
            this.txtPrecioKg.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.ForeColor = System.Drawing.Color.Cornsilk;
            this.label5.Location = new System.Drawing.Point(13, 129);
            this.label5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(77, 18);
            this.label5.TabIndex = 23;
            this.label5.Text = "Precio Kg.";
            // 
            // txtTipo
            // 
            this.txtTipo.Location = new System.Drawing.Point(105, 159);
            this.txtTipo.Margin = new System.Windows.Forms.Padding(4);
            this.txtTipo.Name = "txtTipo";
            this.txtTipo.ReadOnly = true;
            this.txtTipo.Size = new System.Drawing.Size(156, 24);
            this.txtTipo.TabIndex = 22;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.ForeColor = System.Drawing.Color.Cornsilk;
            this.label6.Location = new System.Drawing.Point(55, 162);
            this.label6.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(37, 18);
            this.label6.TabIndex = 21;
            this.label6.Text = "Tipo";
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.ForeColor = System.Drawing.Color.Cornsilk;
            this.label2.Location = new System.Drawing.Point(483, 199);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(99, 18);
            this.label2.TabIndex = 15;
            this.label2.Text = "% en Corte M";
            // 
            // txtCodigo
            // 
            this.txtCodigo.Location = new System.Drawing.Point(105, 59);
            this.txtCodigo.Margin = new System.Windows.Forms.Padding(4);
            this.txtCodigo.Name = "txtCodigo";
            this.txtCodigo.ReadOnly = true;
            this.txtCodigo.Size = new System.Drawing.Size(93, 24);
            this.txtCodigo.TabIndex = 7;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ForeColor = System.Drawing.Color.Cornsilk;
            this.label1.Location = new System.Drawing.Point(36, 63);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(56, 18);
            this.label1.TabIndex = 6;
            this.label1.Text = "Código";
            // 
            // txtCorteMaestro
            // 
            this.txtCorteMaestro.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.txtCorteMaestro.Location = new System.Drawing.Point(599, 162);
            this.txtCorteMaestro.Margin = new System.Windows.Forms.Padding(4);
            this.txtCorteMaestro.Name = "txtCorteMaestro";
            this.txtCorteMaestro.ReadOnly = true;
            this.txtCorteMaestro.Size = new System.Drawing.Size(210, 24);
            this.txtCorteMaestro.TabIndex = 5;
            // 
            // label3
            // 
            this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.ForeColor = System.Drawing.Color.Cornsilk;
            this.label3.Location = new System.Drawing.Point(479, 165);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(104, 18);
            this.label3.TabIndex = 4;
            this.label3.Text = "Corte Maestro";
            // 
            // txtDescCorte
            // 
            this.txtDescCorte.Location = new System.Drawing.Point(105, 92);
            this.txtDescCorte.Margin = new System.Windows.Forms.Padding(4);
            this.txtDescCorte.Name = "txtDescCorte";
            this.txtDescCorte.ReadOnly = true;
            this.txtDescCorte.Size = new System.Drawing.Size(270, 24);
            this.txtDescCorte.TabIndex = 3;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.ForeColor = System.Drawing.Color.Cornsilk;
            this.label4.Location = new System.Drawing.Point(48, 96);
            this.label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(45, 18);
            this.label4.TabIndex = 2;
            this.label4.Text = "Corte";
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
            this.barraControl.Padding = new System.Windows.Forms.Padding(13, 0, 0, 0);
            this.barraControl.Size = new System.Drawing.Size(850, 49);
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
            this.modificar.Size = new System.Drawing.Size(79, 46);
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
            this.eliminar.Size = new System.Drawing.Size(69, 46);
            this.eliminar.Text = "&Eliminar";
            this.eliminar.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.eliminar.Click += new System.EventHandler(this.eliminar_Click);
            // 
            // formInfoCorte
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(850, 400);
            this.Controls.Add(this.btnSalir);
            this.Controls.Add(this.pnlBuscar);
            this.Controls.Add(this.barraControl);
            this.Margin = new System.Windows.Forms.Padding(5);
            this.MaximizeBox = false;
            this.Name = "formInfoCorte";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Información Corte";
            this.Load += new System.EventHandler(this.formInfoCorte_Load);
            this.pnlBuscar.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.barraControl.ResumeLayout(false);
            this.barraControl.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        protected internal System.Windows.Forms.ToolStrip barraControl;
        protected System.Windows.Forms.ToolStripButton modificar;
        protected System.Windows.Forms.GroupBox groupBox1;
        protected System.Windows.Forms.Label label2;
        protected System.Windows.Forms.TextBox txtCodigo;
        protected System.Windows.Forms.Label label1;
        protected System.Windows.Forms.TextBox txtCorteMaestro;
        protected System.Windows.Forms.Label label3;
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
        protected System.Windows.Forms.Label label7;
        protected System.Windows.Forms.Label label9;
        private System.Windows.Forms.CheckBox txtIndependiente;
        protected System.Windows.Forms.ToolStripButton eliminar;
        private System.Windows.Forms.MaskedTextBox txtDesvioEstandar;
        protected System.Windows.Forms.Label label11;
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
    }
}