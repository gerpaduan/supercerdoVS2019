namespace Presentacion.Cortes
{
    partial class formStockActual
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle33 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle34 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle35 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle36 = new System.Windows.Forms.DataGridViewCellStyle();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(formStockActual));
            this.grillaReportes = new System.Windows.Forms.DataGridView();
            this.panel1 = new System.Windows.Forms.Panel();
            this.checkActualizacionAuto = new System.Windows.Forms.CheckBox();
            this.lblError = new System.Windows.Forms.Label();
            this.txtUltimaActualizacion = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.comboConexion = new System.Windows.Forms.ComboBox();
            this.comboInicioStock = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.comboOrdenStock = new System.Windows.Forms.ComboBox();
            this.lblActualizar = new System.Windows.Forms.Label();
            this.btnBuscar = new System.Windows.Forms.Button();
            this.label5 = new System.Windows.Forms.Label();
            this.txtDescripcion = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.barraControl = new System.Windows.Forms.ToolStrip();
            this.Imprimir = new System.Windows.Forms.ToolStripButton();
            this.menuDuplicar = new System.Windows.Forms.ToolStripButton();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.checkColStockKg = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.grillaReportes)).BeginInit();
            this.panel1.SuspendLayout();
            this.barraControl.SuspendLayout();
            this.SuspendLayout();
            // 
            // grillaReportes
            // 
            this.grillaReportes.AllowUserToAddRows = false;
            this.grillaReportes.AllowUserToResizeRows = false;
            this.grillaReportes.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.grillaReportes.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            dataGridViewCellStyle33.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle33.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle33.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle33.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle33.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle33.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle33.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.grillaReportes.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle33;
            dataGridViewCellStyle34.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle34.BackColor = System.Drawing.Color.WhiteSmoke;
            dataGridViewCellStyle34.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle34.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle34.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle34.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle34.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.grillaReportes.DefaultCellStyle = dataGridViewCellStyle34;
            this.grillaReportes.Location = new System.Drawing.Point(12, 158);
            this.grillaReportes.Name = "grillaReportes";
            this.grillaReportes.ReadOnly = true;
            dataGridViewCellStyle35.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle35.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle35.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle35.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle35.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle35.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle35.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.grillaReportes.RowHeadersDefaultCellStyle = dataGridViewCellStyle35;
            this.grillaReportes.RowHeadersVisible = false;
            this.grillaReportes.RowHeadersWidth = 300;
            dataGridViewCellStyle36.Format = "N2";
            dataGridViewCellStyle36.NullValue = null;
            this.grillaReportes.RowsDefaultCellStyle = dataGridViewCellStyle36;
            this.grillaReportes.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.grillaReportes.Size = new System.Drawing.Size(495, 415);
            this.grillaReportes.TabIndex = 14;
            this.grillaReportes.TabStop = false;
            // 
            // panel1
            // 
            this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.panel1.BackColor = System.Drawing.Color.SteelBlue;
            this.panel1.Controls.Add(this.checkColStockKg);
            this.panel1.Controls.Add(this.checkActualizacionAuto);
            this.panel1.Controls.Add(this.lblError);
            this.panel1.Controls.Add(this.txtUltimaActualizacion);
            this.panel1.Controls.Add(this.label2);
            this.panel1.Controls.Add(this.comboConexion);
            this.panel1.Controls.Add(this.comboInicioStock);
            this.panel1.Controls.Add(this.label3);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Controls.Add(this.comboOrdenStock);
            this.panel1.Controls.Add(this.lblActualizar);
            this.panel1.Controls.Add(this.btnBuscar);
            this.panel1.Controls.Add(this.label5);
            this.panel1.Controls.Add(this.txtDescripcion);
            this.panel1.Controls.Add(this.label9);
            this.panel1.Location = new System.Drawing.Point(-1, 40);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(521, 112);
            this.panel1.TabIndex = 15;
            // 
            // checkActualizacionAuto
            // 
            this.checkActualizacionAuto.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.checkActualizacionAuto.AutoSize = true;
            this.checkActualizacionAuto.Checked = true;
            this.checkActualizacionAuto.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkActualizacionAuto.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.checkActualizacionAuto.ForeColor = System.Drawing.Color.Cornsilk;
            this.checkActualizacionAuto.Location = new System.Drawing.Point(346, 93);
            this.checkActualizacionAuto.Name = "checkActualizacionAuto";
            this.checkActualizacionAuto.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.checkActualizacionAuto.Size = new System.Drawing.Size(162, 19);
            this.checkActualizacionAuto.TabIndex = 108;
            this.checkActualizacionAuto.Text = "Actualización automática";
            this.checkActualizacionAuto.UseVisualStyleBackColor = true;
            this.checkActualizacionAuto.CheckedChanged += new System.EventHandler(this.checkActualizacionAuto_CheckedChanged);
            // 
            // lblError
            // 
            this.lblError.AutoSize = true;
            this.lblError.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblError.ForeColor = System.Drawing.Color.LightSalmon;
            this.lblError.Location = new System.Drawing.Point(17, 63);
            this.lblError.Name = "lblError";
            this.lblError.Size = new System.Drawing.Size(185, 30);
            this.lblError.TabIndex = 107;
            this.lblError.Text = "Hubo un error y no se pudo \r\nactualizar el stock";
            this.lblError.Visible = false;
            // 
            // txtUltimaActualizacion
            // 
            this.txtUltimaActualizacion.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.txtUltimaActualizacion.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtUltimaActualizacion.Location = new System.Drawing.Point(415, 68);
            this.txtUltimaActualizacion.Name = "txtUltimaActualizacion";
            this.txtUltimaActualizacion.ReadOnly = true;
            this.txtUltimaActualizacion.Size = new System.Drawing.Size(93, 22);
            this.txtUltimaActualizacion.TabIndex = 105;
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.ForeColor = System.Drawing.Color.Cornsilk;
            this.label2.Location = new System.Drawing.Point(261, 71);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(148, 15);
            this.label2.TabIndex = 106;
            this.label2.Text = "Hora Última Actualización";
            // 
            // comboConexion
            // 
            this.comboConexion.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboConexion.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.comboConexion.FormattingEnabled = true;
            this.comboConexion.Items.AddRange(new object[] {
            "local",
            "sanMartin",
            "sanMartinRemoto",
            "sanLorenzo",
            "sanLorenzoRemoto"});
            this.comboConexion.Location = new System.Drawing.Point(95, 16);
            this.comboConexion.Name = "comboConexion";
            this.comboConexion.Size = new System.Drawing.Size(137, 23);
            this.comboConexion.TabIndex = 104;
            this.comboConexion.SelectedIndexChanged += new System.EventHandler(this.comboConexion_SelectedIndexChanged);
            // 
            // comboInicioStock
            // 
            this.comboInicioStock.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.comboInicioStock.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboInicioStock.FormattingEnabled = true;
            this.comboInicioStock.Location = new System.Drawing.Point(356, 15);
            this.comboInicioStock.Name = "comboInicioStock";
            this.comboInicioStock.Size = new System.Drawing.Size(152, 21);
            this.comboInicioStock.TabIndex = 22;
            this.comboInicioStock.SelectedIndexChanged += new System.EventHandler(this.comboInicioStock_SelectedIndexChanged);
            // 
            // label3
            // 
            this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.ForeColor = System.Drawing.Color.Cornsilk;
            this.label3.Location = new System.Drawing.Point(307, 19);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(43, 15);
            this.label3.TabIndex = 12;
            this.label3.Text = "Desde";
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ForeColor = System.Drawing.Color.Cornsilk;
            this.label1.Location = new System.Drawing.Point(276, 44);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(74, 15);
            this.label1.TabIndex = 55;
            this.label1.Text = "Orden Stock";
            // 
            // comboOrdenStock
            // 
            this.comboOrdenStock.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.comboOrdenStock.DisplayMember = "Sin Orden";
            this.comboOrdenStock.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboOrdenStock.FormattingEnabled = true;
            this.comboOrdenStock.Items.AddRange(new object[] {
            "Sin Orden",
            "Ascendente",
            "Descendente"});
            this.comboOrdenStock.Location = new System.Drawing.Point(356, 41);
            this.comboOrdenStock.Name = "comboOrdenStock";
            this.comboOrdenStock.Size = new System.Drawing.Size(152, 21);
            this.comboOrdenStock.TabIndex = 54;
            this.comboOrdenStock.SelectedIndexChanged += new System.EventHandler(this.comboOrdenStock_SelectedIndexChanged);
            // 
            // lblActualizar
            // 
            this.lblActualizar.AutoSize = true;
            this.lblActualizar.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblActualizar.ForeColor = System.Drawing.Color.LightSalmon;
            this.lblActualizar.Location = new System.Drawing.Point(238, 19);
            this.lblActualizar.Name = "lblActualizar";
            this.lblActualizar.Size = new System.Drawing.Size(69, 15);
            this.lblActualizar.TabIndex = 53;
            this.lblActualizar.Text = "Actualizar...";
            this.lblActualizar.Visible = false;
            // 
            // btnBuscar
            // 
            this.btnBuscar.AccessibleDescription = "";
            this.btnBuscar.ForeColor = System.Drawing.Color.Black;
            this.btnBuscar.Image = ((System.Drawing.Image)(resources.GetObject("btnBuscar.Image")));
            this.btnBuscar.Location = new System.Drawing.Point(238, 41);
            this.btnBuscar.Name = "btnBuscar";
            this.btnBuscar.Size = new System.Drawing.Size(28, 23);
            this.btnBuscar.TabIndex = 21;
            this.btnBuscar.TabStop = false;
            this.btnBuscar.UseVisualStyleBackColor = true;
            this.btnBuscar.Click += new System.EventHandler(this.btnBuscar_Click);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.ForeColor = System.Drawing.Color.Cornsilk;
            this.label5.Location = new System.Drawing.Point(34, 19);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(55, 15);
            this.label5.TabIndex = 20;
            this.label5.Text = "Sucursal";
            // 
            // txtDescripcion
            // 
            this.txtDescripcion.Location = new System.Drawing.Point(95, 42);
            this.txtDescripcion.Name = "txtDescripcion";
            this.txtDescripcion.Size = new System.Drawing.Size(137, 20);
            this.txtDescripcion.TabIndex = 0;
            this.txtDescripcion.TextChanged += new System.EventHandler(this.txtDescripcion_TextChanged);
            this.txtDescripcion.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtDescripcion_KeyDown);
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label9.ForeColor = System.Drawing.Color.Cornsilk;
            this.label9.Location = new System.Drawing.Point(17, 43);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(72, 15);
            this.label9.TabIndex = 2;
            this.label9.Text = "Descripción";
            // 
            // barraControl
            // 
            this.barraControl.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.barraControl.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.Imprimir,
            this.menuDuplicar});
            this.barraControl.Location = new System.Drawing.Point(0, 0);
            this.barraControl.Name = "barraControl";
            this.barraControl.Padding = new System.Windows.Forms.Padding(10, 0, 0, 0);
            this.barraControl.Size = new System.Drawing.Size(519, 45);
            this.barraControl.TabIndex = 16;
            this.barraControl.TabStop = true;
            this.barraControl.Text = "toolStrip1";
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
            // menuDuplicar
            // 
            this.menuDuplicar.Image = ((System.Drawing.Image)(resources.GetObject("menuDuplicar.Image")));
            this.menuDuplicar.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.menuDuplicar.Margin = new System.Windows.Forms.Padding(0, 2, 0, 1);
            this.menuDuplicar.Name = "menuDuplicar";
            this.menuDuplicar.Padding = new System.Windows.Forms.Padding(1, 1, 1, 6);
            this.menuDuplicar.Size = new System.Drawing.Size(57, 42);
            this.menuDuplicar.Text = "&Duplicar";
            this.menuDuplicar.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.menuDuplicar.Click += new System.EventHandler(this.menuDuplicar_Click);
            // 
            // timer1
            // 
            this.timer1.Interval = 120000;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // checkColStockKg
            // 
            this.checkColStockKg.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.checkColStockKg.AutoSize = true;
            this.checkColStockKg.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.checkColStockKg.ForeColor = System.Drawing.Color.Cornsilk;
            this.checkColStockKg.Location = new System.Drawing.Point(141, 93);
            this.checkColStockKg.Name = "checkColStockKg";
            this.checkColStockKg.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.checkColStockKg.Size = new System.Drawing.Size(125, 19);
            this.checkColStockKg.TabIndex = 109;
            this.checkColStockKg.Text = "Mostrar Stock.Kgs";
            this.checkColStockKg.UseVisualStyleBackColor = true;
            this.checkColStockKg.CheckedChanged += new System.EventHandler(this.checkColStockKg_CheckedChanged);
            // 
            // formStockActual
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(519, 585);
            this.Controls.Add(this.barraControl);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.grillaReportes);
            this.Name = "formStockActual";
            this.Text = "Stock Actual";
            this.Load += new System.EventHandler(this.formStockActual_Load);
            ((System.ComponentModel.ISupportInitialize)(this.grillaReportes)).EndInit();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.barraControl.ResumeLayout(false);
            this.barraControl.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.DataGridView grillaReportes;
        protected System.Windows.Forms.Panel panel1;
        protected System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox comboOrdenStock;
        protected System.Windows.Forms.Label lblActualizar;
        private System.Windows.Forms.Button btnBuscar;
        protected System.Windows.Forms.Label label5;
        protected System.Windows.Forms.TextBox txtDescripcion;
        protected System.Windows.Forms.Label label9;
        private System.Windows.Forms.ComboBox comboInicioStock;
        protected System.Windows.Forms.Label label3;
        protected System.Windows.Forms.ToolStrip barraControl;
        protected System.Windows.Forms.ToolStripButton Imprimir;
        private System.Windows.Forms.ComboBox comboConexion;
        protected System.Windows.Forms.ToolStripButton menuDuplicar;
        protected System.Windows.Forms.Label lblError;
        protected System.Windows.Forms.TextBox txtUltimaActualizacion;
        protected System.Windows.Forms.Label label2;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.CheckBox checkActualizacionAuto;
        private System.Windows.Forms.CheckBox checkColStockKg;
    }
}