namespace Presentacion
{
    partial class formNuevoCorte
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(formNuevoCorte));
            this.btnGuardar = new System.Windows.Forms.Button();
            this.btnCancelar = new System.Windows.Forms.Button();
            this.pnlBuscar = new System.Windows.Forms.Panel();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.comboAlicuotaIva = new System.Windows.Forms.ComboBox();
            this.label10 = new System.Windows.Forms.Label();
            this.checkHabilitado = new System.Windows.Forms.CheckBox();
            this.txtPromedio = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.checkEnCierreStock = new System.Windows.Forms.CheckBox();
            this.checkIngresoRapidoEmbutido = new System.Windows.Forms.CheckBox();
            this.checkAsignarMaestro = new System.Windows.Forms.CheckBox();
            this.groupMaestro = new System.Windows.Forms.GroupBox();
            this.txtDesvioEstandar = new System.Windows.Forms.MaskedTextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.txtPorcHueso = new System.Windows.Forms.MaskedTextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.txtPorcentajeCorteM = new System.Windows.Forms.MaskedTextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.btnBuscarCorteM = new System.Windows.Forms.Button();
            this.txtCorteMaestro = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.txtIndependiente = new System.Windows.Forms.CheckBox();
            this.txtPrecioKg = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.comboTipo = new System.Windows.Forms.ComboBox();
            this.label6 = new System.Windows.Forms.Label();
            this.txtCodigo = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.txtDescCorte = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.pnlBuscar.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.groupMaestro.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnGuardar
            // 
            this.btnGuardar.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnGuardar.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnGuardar.Location = new System.Drawing.Point(459, 389);
            this.btnGuardar.Margin = new System.Windows.Forms.Padding(4);
            this.btnGuardar.Name = "btnGuardar";
            this.btnGuardar.Size = new System.Drawing.Size(121, 33);
            this.btnGuardar.TabIndex = 8;
            this.btnGuardar.Text = "&Guardar";
            this.btnGuardar.UseVisualStyleBackColor = true;
            this.btnGuardar.Click += new System.EventHandler(this.btnGuardar_Click);
            // 
            // btnCancelar
            // 
            this.btnCancelar.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancelar.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnCancelar.Location = new System.Drawing.Point(588, 389);
            this.btnCancelar.Margin = new System.Windows.Forms.Padding(4);
            this.btnCancelar.Name = "btnCancelar";
            this.btnCancelar.Size = new System.Drawing.Size(121, 33);
            this.btnCancelar.TabIndex = 9;
            this.btnCancelar.Text = "&Cancelar";
            this.btnCancelar.UseVisualStyleBackColor = true;
            this.btnCancelar.Click += new System.EventHandler(this.btnCancelar_Click);
            // 
            // pnlBuscar
            // 
            this.pnlBuscar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pnlBuscar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(43)))), ((int)(((byte)(77)))), ((int)(((byte)(129)))));
            this.pnlBuscar.Controls.Add(this.groupBox1);
            this.pnlBuscar.Location = new System.Drawing.Point(0, 0);
            this.pnlBuscar.Margin = new System.Windows.Forms.Padding(4);
            this.pnlBuscar.Name = "pnlBuscar";
            this.pnlBuscar.Size = new System.Drawing.Size(727, 382);
            this.pnlBuscar.TabIndex = 10;
            // 
            // groupBox1
            // 
            this.groupBox1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(43)))), ((int)(((byte)(77)))), ((int)(((byte)(129)))));
            this.groupBox1.Controls.Add(this.comboAlicuotaIva);
            this.groupBox1.Controls.Add(this.label10);
            this.groupBox1.Controls.Add(this.checkHabilitado);
            this.groupBox1.Controls.Add(this.txtPromedio);
            this.groupBox1.Controls.Add(this.label8);
            this.groupBox1.Controls.Add(this.checkEnCierreStock);
            this.groupBox1.Controls.Add(this.checkIngresoRapidoEmbutido);
            this.groupBox1.Controls.Add(this.checkAsignarMaestro);
            this.groupBox1.Controls.Add(this.groupMaestro);
            this.groupBox1.Controls.Add(this.txtIndependiente);
            this.groupBox1.Controls.Add(this.txtPrecioKg);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.comboTipo);
            this.groupBox1.Controls.Add(this.label6);
            this.groupBox1.Controls.Add(this.txtCodigo);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.txtDescCorte);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox1.ForeColor = System.Drawing.Color.Cornsilk;
            this.groupBox1.Location = new System.Drawing.Point(15, 4);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(4);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(4);
            this.groupBox1.Size = new System.Drawing.Size(695, 374);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Corte";
            // 
            // comboAlicuotaIva
            // 
            this.comboAlicuotaIva.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboAlicuotaIva.FormattingEnabled = true;
            this.comboAlicuotaIva.Items.AddRange(new object[] {
            "Pesable",
            "Unidad",
            "Elaborado",
            "Corte",
            "Embutido",
            "Otro"});
            this.comboAlicuotaIva.Location = new System.Drawing.Point(124, 203);
            this.comboAlicuotaIva.Margin = new System.Windows.Forms.Padding(4);
            this.comboAlicuotaIva.Name = "comboAlicuotaIva";
            this.comboAlicuotaIva.Size = new System.Drawing.Size(93, 26);
            this.comboAlicuotaIva.TabIndex = 5;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label10.ForeColor = System.Drawing.Color.Cornsilk;
            this.label10.Location = new System.Drawing.Point(31, 206);
            this.label10.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(82, 18);
            this.label10.TabIndex = 36;
            this.label10.Text = "Alícuota Iva";
            // 
            // checkHabilitado
            // 
            this.checkHabilitado.AutoSize = true;
            this.checkHabilitado.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.checkHabilitado.Checked = true;
            this.checkHabilitado.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkHabilitado.Location = new System.Drawing.Point(392, 71);
            this.checkHabilitado.Margin = new System.Windows.Forms.Padding(4);
            this.checkHabilitado.Name = "checkHabilitado";
            this.checkHabilitado.Size = new System.Drawing.Size(95, 22);
            this.checkHabilitado.TabIndex = 6;
            this.checkHabilitado.Text = "Habilitado";
            this.checkHabilitado.UseVisualStyleBackColor = true;
            // 
            // txtPromedio
            // 
            this.txtPromedio.Location = new System.Drawing.Point(124, 171);
            this.txtPromedio.Margin = new System.Windows.Forms.Padding(4);
            this.txtPromedio.Name = "txtPromedio";
            this.txtPromedio.Size = new System.Drawing.Size(93, 24);
            this.txtPromedio.TabIndex = 4;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label8.ForeColor = System.Drawing.Color.Cornsilk;
            this.label8.Location = new System.Drawing.Point(35, 175);
            this.label8.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(73, 18);
            this.label8.TabIndex = 34;
            this.label8.Text = "Promedio";
            // 
            // checkEnCierreStock
            // 
            this.checkEnCierreStock.AutoSize = true;
            this.checkEnCierreStock.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.checkEnCierreStock.Checked = true;
            this.checkEnCierreStock.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkEnCierreStock.Location = new System.Drawing.Point(356, 106);
            this.checkEnCierreStock.Margin = new System.Windows.Forms.Padding(4);
            this.checkEnCierreStock.Name = "checkEnCierreStock";
            this.checkEnCierreStock.Size = new System.Drawing.Size(135, 22);
            this.checkEnCierreStock.TabIndex = 7;
            this.checkEnCierreStock.Text = "En Cierre Stock";
            this.checkEnCierreStock.UseVisualStyleBackColor = true;
            // 
            // checkIngresoRapidoEmbutido
            // 
            this.checkIngresoRapidoEmbutido.AutoSize = true;
            this.checkIngresoRapidoEmbutido.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.checkIngresoRapidoEmbutido.Location = new System.Drawing.Point(284, 38);
            this.checkIngresoRapidoEmbutido.Margin = new System.Windows.Forms.Padding(4);
            this.checkIngresoRapidoEmbutido.Name = "checkIngresoRapidoEmbutido";
            this.checkIngresoRapidoEmbutido.Size = new System.Drawing.Size(189, 22);
            this.checkIngresoRapidoEmbutido.TabIndex = 6;
            this.checkIngresoRapidoEmbutido.TabStop = false;
            this.checkIngresoRapidoEmbutido.Text = "IngresoRapidoEmbutido";
            this.checkIngresoRapidoEmbutido.UseVisualStyleBackColor = true;
            // 
            // checkAsignarMaestro
            // 
            this.checkAsignarMaestro.AutoSize = true;
            this.checkAsignarMaestro.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.checkAsignarMaestro.Location = new System.Drawing.Point(350, 173);
            this.checkAsignarMaestro.Margin = new System.Windows.Forms.Padding(4);
            this.checkAsignarMaestro.Name = "checkAsignarMaestro";
            this.checkAsignarMaestro.Size = new System.Drawing.Size(138, 22);
            this.checkAsignarMaestro.TabIndex = 9;
            this.checkAsignarMaestro.Text = "Asignar maestro";
            this.checkAsignarMaestro.UseVisualStyleBackColor = true;
            this.checkAsignarMaestro.CheckedChanged += new System.EventHandler(this.checkAsignarMaestro_CheckedChanged);
            // 
            // groupMaestro
            // 
            this.groupMaestro.Controls.Add(this.txtDesvioEstandar);
            this.groupMaestro.Controls.Add(this.label9);
            this.groupMaestro.Controls.Add(this.txtPorcHueso);
            this.groupMaestro.Controls.Add(this.label7);
            this.groupMaestro.Controls.Add(this.txtPorcentajeCorteM);
            this.groupMaestro.Controls.Add(this.label2);
            this.groupMaestro.Controls.Add(this.btnBuscarCorteM);
            this.groupMaestro.Controls.Add(this.txtCorteMaestro);
            this.groupMaestro.Controls.Add(this.label3);
            this.groupMaestro.Enabled = false;
            this.groupMaestro.ForeColor = System.Drawing.Color.Cornsilk;
            this.groupMaestro.Location = new System.Drawing.Point(333, 209);
            this.groupMaestro.Margin = new System.Windows.Forms.Padding(4);
            this.groupMaestro.Name = "groupMaestro";
            this.groupMaestro.Padding = new System.Windows.Forms.Padding(4);
            this.groupMaestro.Size = new System.Drawing.Size(353, 158);
            this.groupMaestro.TabIndex = 29;
            this.groupMaestro.TabStop = false;
            this.groupMaestro.Text = "Corte Maestro";
            // 
            // txtDesvioEstandar
            // 
            this.txtDesvioEstandar.Location = new System.Drawing.Point(151, 126);
            this.txtDesvioEstandar.Margin = new System.Windows.Forms.Padding(4);
            this.txtDesvioEstandar.Name = "txtDesvioEstandar";
            this.txtDesvioEstandar.Size = new System.Drawing.Size(99, 24);
            this.txtDesvioEstandar.TabIndex = 34;
            this.txtDesvioEstandar.Text = "0";
            this.txtDesvioEstandar.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.txtDesvioEstandar.TextChanged += new System.EventHandler(this.txtCodigo_TextChanged);
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label9.ForeColor = System.Drawing.Color.Cornsilk;
            this.label9.Location = new System.Drawing.Point(19, 129);
            this.label9.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(117, 18);
            this.label9.TabIndex = 37;
            this.label9.Text = "Desvío Estandar";
            // 
            // txtPorcHueso
            // 
            this.txtPorcHueso.Location = new System.Drawing.Point(151, 92);
            this.txtPorcHueso.Margin = new System.Windows.Forms.Padding(4);
            this.txtPorcHueso.Name = "txtPorcHueso";
            this.txtPorcHueso.Size = new System.Drawing.Size(99, 24);
            this.txtPorcHueso.TabIndex = 33;
            this.txtPorcHueso.Text = "0";
            this.txtPorcHueso.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.txtPorcHueso.TextChanged += new System.EventHandler(this.txtCodigo_TextChanged);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label7.ForeColor = System.Drawing.Color.Cornsilk;
            this.label7.Location = new System.Drawing.Point(31, 96);
            this.label7.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(104, 18);
            this.label7.TabIndex = 36;
            this.label7.Text = "% Desperdicio";
            // 
            // txtPorcentajeCorteM
            // 
            this.txtPorcentajeCorteM.Location = new System.Drawing.Point(151, 59);
            this.txtPorcentajeCorteM.Margin = new System.Windows.Forms.Padding(4);
            this.txtPorcentajeCorteM.Name = "txtPorcentajeCorteM";
            this.txtPorcentajeCorteM.Size = new System.Drawing.Size(99, 24);
            this.txtPorcentajeCorteM.TabIndex = 32;
            this.txtPorcentajeCorteM.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.txtPorcentajeCorteM.TextChanged += new System.EventHandler(this.txtCodigo_TextChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.ForeColor = System.Drawing.Color.Cornsilk;
            this.label2.Location = new System.Drawing.Point(35, 63);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(99, 18);
            this.label2.TabIndex = 35;
            this.label2.Text = "% en Corte M";
            // 
            // btnBuscarCorteM
            // 
            this.btnBuscarCorteM.AccessibleDescription = "";
            this.btnBuscarCorteM.ForeColor = System.Drawing.Color.Black;
            this.btnBuscarCorteM.Image = ((System.Drawing.Image)(resources.GetObject("btnBuscarCorteM.Image")));
            this.btnBuscarCorteM.Location = new System.Drawing.Point(304, 25);
            this.btnBuscarCorteM.Margin = new System.Windows.Forms.Padding(4);
            this.btnBuscarCorteM.Name = "btnBuscarCorteM";
            this.btnBuscarCorteM.Size = new System.Drawing.Size(37, 28);
            this.btnBuscarCorteM.TabIndex = 29;
            this.btnBuscarCorteM.UseVisualStyleBackColor = true;
            this.btnBuscarCorteM.Click += new System.EventHandler(this.btnBuscarCorteM_Click);
            // 
            // txtCorteMaestro
            // 
            this.txtCorteMaestro.Location = new System.Drawing.Point(151, 26);
            this.txtCorteMaestro.Margin = new System.Windows.Forms.Padding(4);
            this.txtCorteMaestro.Name = "txtCorteMaestro";
            this.txtCorteMaestro.ReadOnly = true;
            this.txtCorteMaestro.Size = new System.Drawing.Size(140, 24);
            this.txtCorteMaestro.TabIndex = 30;
            this.txtCorteMaestro.TextChanged += new System.EventHandler(this.txtCodigo_TextChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.ForeColor = System.Drawing.Color.Cornsilk;
            this.label3.Location = new System.Drawing.Point(95, 30);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(45, 18);
            this.label3.TabIndex = 31;
            this.label3.Text = "Corte";
            // 
            // txtIndependiente
            // 
            this.txtIndependiente.AutoSize = true;
            this.txtIndependiente.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.txtIndependiente.Checked = true;
            this.txtIndependiente.CheckState = System.Windows.Forms.CheckState.Checked;
            this.txtIndependiente.Location = new System.Drawing.Point(319, 139);
            this.txtIndependiente.Margin = new System.Windows.Forms.Padding(4);
            this.txtIndependiente.Name = "txtIndependiente";
            this.txtIndependiente.Size = new System.Drawing.Size(161, 22);
            this.txtIndependiente.TabIndex = 8;
            this.txtIndependiente.Text = "Corte Independiente";
            this.txtIndependiente.UseVisualStyleBackColor = true;
            this.txtIndependiente.CheckedChanged += new System.EventHandler(this.txtIndependiente_CheckedChanged);
            // 
            // txtPrecioKg
            // 
            this.txtPrecioKg.Location = new System.Drawing.Point(124, 102);
            this.txtPrecioKg.Margin = new System.Windows.Forms.Padding(4);
            this.txtPrecioKg.Name = "txtPrecioKg";
            this.txtPrecioKg.Size = new System.Drawing.Size(93, 24);
            this.txtPrecioKg.TabIndex = 2;
            this.txtPrecioKg.TextChanged += new System.EventHandler(this.txtCodigo_TextChanged);
            this.txtPrecioKg.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.TxtPruebaENTER_KeyPress);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.ForeColor = System.Drawing.Color.Cornsilk;
            this.label5.Location = new System.Drawing.Point(31, 106);
            this.label5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(77, 18);
            this.label5.TabIndex = 22;
            this.label5.Text = "Precio Kg.";
            // 
            // comboTipo
            // 
            this.comboTipo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboTipo.FormattingEnabled = true;
            this.comboTipo.Items.AddRange(new object[] {
            "Pesable",
            "Unidad",
            "Elaborado",
            "Corte",
            "Embutido",
            "Otro"});
            this.comboTipo.Location = new System.Drawing.Point(124, 135);
            this.comboTipo.Margin = new System.Windows.Forms.Padding(4);
            this.comboTipo.Name = "comboTipo";
            this.comboTipo.Size = new System.Drawing.Size(155, 26);
            this.comboTipo.TabIndex = 3;
            this.comboTipo.SelectedIndexChanged += new System.EventHandler(this.comboTipo_SelectedIndexChanged);
            this.comboTipo.TextChanged += new System.EventHandler(this.txtCodigo_TextChanged);
            this.comboTipo.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.TxtPruebaENTER_KeyPress);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.ForeColor = System.Drawing.Color.Cornsilk;
            this.label6.Location = new System.Drawing.Point(73, 139);
            this.label6.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(37, 18);
            this.label6.TabIndex = 19;
            this.label6.Text = "Tipo";
            // 
            // txtCodigo
            // 
            this.txtCodigo.Location = new System.Drawing.Point(124, 36);
            this.txtCodigo.Margin = new System.Windows.Forms.Padding(4);
            this.txtCodigo.Name = "txtCodigo";
            this.txtCodigo.Size = new System.Drawing.Size(93, 24);
            this.txtCodigo.TabIndex = 0;
            this.txtCodigo.TextChanged += new System.EventHandler(this.txtCodigo_TextChanged);
            this.txtCodigo.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.TxtPruebaENTER_KeyPress);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ForeColor = System.Drawing.Color.Cornsilk;
            this.label1.Location = new System.Drawing.Point(55, 39);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(56, 18);
            this.label1.TabIndex = 6;
            this.label1.Text = "Código";
            // 
            // txtDescCorte
            // 
            this.txtDescCorte.Location = new System.Drawing.Point(124, 69);
            this.txtDescCorte.Margin = new System.Windows.Forms.Padding(4);
            this.txtDescCorte.Name = "txtDescCorte";
            this.txtDescCorte.Size = new System.Drawing.Size(192, 24);
            this.txtDescCorte.TabIndex = 1;
            this.txtDescCorte.TextChanged += new System.EventHandler(this.txtCodigo_TextChanged);
            this.txtDescCorte.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.TxtPruebaENTER_KeyPress);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.ForeColor = System.Drawing.Color.Cornsilk;
            this.label4.Location = new System.Drawing.Point(67, 73);
            this.label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(45, 18);
            this.label4.TabIndex = 2;
            this.label4.Text = "Corte";
            // 
            // formNuevoCorte
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(725, 426);
            this.Controls.Add(this.btnGuardar);
            this.Controls.Add(this.btnCancelar);
            this.Controls.Add(this.pnlBuscar);
            this.Margin = new System.Windows.Forms.Padding(5);
            this.MaximizeBox = false;
            this.Name = "formNuevoCorte";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Nuevo Corte";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.formNuevoCorte_FormClosed);
            this.Load += new System.EventHandler(this.formNuevoCorte_Load);
            this.pnlBuscar.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupMaestro.ResumeLayout(false);
            this.groupMaestro.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        protected System.Windows.Forms.Panel pnlBuscar;
        protected System.Windows.Forms.Button btnCancelar;
        protected System.Windows.Forms.GroupBox groupBox1;
        protected System.Windows.Forms.TextBox txtCodigo;
        protected System.Windows.Forms.Label label1;
        protected System.Windows.Forms.TextBox txtDescCorte;
        protected System.Windows.Forms.Label label4;
        protected System.Windows.Forms.Label label6;
        protected System.Windows.Forms.ComboBox comboTipo;
        protected System.Windows.Forms.TextBox txtPrecioKg;
        protected System.Windows.Forms.Label label5;
        private System.Windows.Forms.CheckBox txtIndependiente;
        protected System.Windows.Forms.Button btnGuardar;
        private System.Windows.Forms.GroupBox groupMaestro;
        private System.Windows.Forms.MaskedTextBox txtDesvioEstandar;
        protected System.Windows.Forms.Label label9;
        private System.Windows.Forms.MaskedTextBox txtPorcHueso;
        protected System.Windows.Forms.Label label7;
        private System.Windows.Forms.MaskedTextBox txtPorcentajeCorteM;
        protected System.Windows.Forms.Label label2;
        protected internal System.Windows.Forms.Button btnBuscarCorteM;
        protected System.Windows.Forms.TextBox txtCorteMaestro;
        protected System.Windows.Forms.Label label3;
        private System.Windows.Forms.CheckBox checkAsignarMaestro;
        private System.Windows.Forms.CheckBox checkEnCierreStock;
        private System.Windows.Forms.CheckBox checkIngresoRapidoEmbutido;
        protected System.Windows.Forms.TextBox txtPromedio;
        protected System.Windows.Forms.Label label8;
        private System.Windows.Forms.CheckBox checkHabilitado;
        protected System.Windows.Forms.ComboBox comboAlicuotaIva;
        protected System.Windows.Forms.Label label10;
    }
}