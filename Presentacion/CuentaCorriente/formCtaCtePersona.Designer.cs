namespace Presentacion.CuentaCorriente
{
    partial class formCtaCtePersona
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle13 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle14 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle15 = new System.Windows.Forms.DataGridViewCellStyle();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(formCtaCtePersona));
            this.panel1 = new System.Windows.Forms.Panel();
            this.btnSeleccionar = new System.Windows.Forms.Button();
            this.btnCancelar = new System.Windows.Forms.Button();
            this.grillaMovCtaCte = new System.Windows.Forms.DataGridView();
            this.pnlBuscar = new System.Windows.Forms.Panel();
            this.fechaDesdePick = new System.Windows.Forms.DateTimePicker();
            this.label3 = new System.Windows.Forms.Label();
            this.lblActualizar = new System.Windows.Forms.Label();
            this.btnBuscar = new System.Windows.Forms.Button();
            this.txtPersona = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.barraControl = new System.Windows.Forms.ToolStrip();
            this.menuNuevoPago = new System.Windows.Forms.ToolStripButton();
            this.nuevo = new System.Windows.Forms.ToolStripButton();
            this.checkSinRegRepetidos = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.grillaMovCtaCte)).BeginInit();
            this.pnlBuscar.SuspendLayout();
            this.barraControl.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel1.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.panel1.Location = new System.Drawing.Point(12, 462);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(784, 1);
            this.panel1.TabIndex = 26;
            // 
            // btnSeleccionar
            // 
            this.btnSeleccionar.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSeleccionar.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSeleccionar.Location = new System.Drawing.Point(547, 469);
            this.btnSeleccionar.Name = "btnSeleccionar";
            this.btnSeleccionar.Size = new System.Drawing.Size(123, 27);
            this.btnSeleccionar.TabIndex = 25;
            this.btnSeleccionar.Text = "&Seleccionar";
            this.btnSeleccionar.UseVisualStyleBackColor = true;
            this.btnSeleccionar.Click += new System.EventHandler(this.btnSeleccionar_Click);
            // 
            // btnCancelar
            // 
            this.btnCancelar.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancelar.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnCancelar.Location = new System.Drawing.Point(676, 469);
            this.btnCancelar.Name = "btnCancelar";
            this.btnCancelar.Size = new System.Drawing.Size(123, 27);
            this.btnCancelar.TabIndex = 24;
            this.btnCancelar.Text = "&Cerrar";
            this.btnCancelar.UseVisualStyleBackColor = true;
            this.btnCancelar.Click += new System.EventHandler(this.btnCancelar_Click);
            // 
            // grillaMovCtaCte
            // 
            this.grillaMovCtaCte.AllowDrop = true;
            this.grillaMovCtaCte.AllowUserToAddRows = false;
            this.grillaMovCtaCte.AllowUserToDeleteRows = false;
            this.grillaMovCtaCte.AllowUserToResizeRows = false;
            this.grillaMovCtaCte.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.grillaMovCtaCte.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
            dataGridViewCellStyle13.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle13.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle13.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle13.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle13.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle13.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle13.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.grillaMovCtaCte.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle13;
            this.grillaMovCtaCte.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewCellStyle14.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle14.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle14.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle14.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle14.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle14.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle14.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.grillaMovCtaCte.DefaultCellStyle = dataGridViewCellStyle14;
            this.grillaMovCtaCte.Location = new System.Drawing.Point(12, 127);
            this.grillaMovCtaCte.MultiSelect = false;
            this.grillaMovCtaCte.Name = "grillaMovCtaCte";
            this.grillaMovCtaCte.ReadOnly = true;
            dataGridViewCellStyle15.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle15.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle15.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle15.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle15.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle15.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle15.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.grillaMovCtaCte.RowHeadersDefaultCellStyle = dataGridViewCellStyle15;
            this.grillaMovCtaCte.RowHeadersVisible = false;
            this.grillaMovCtaCte.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.grillaMovCtaCte.Size = new System.Drawing.Size(787, 329);
            this.grillaMovCtaCte.TabIndex = 28;
            // 
            // pnlBuscar
            // 
            this.pnlBuscar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.pnlBuscar.BackColor = System.Drawing.Color.SteelBlue;
            this.pnlBuscar.Controls.Add(this.checkSinRegRepetidos);
            this.pnlBuscar.Controls.Add(this.fechaDesdePick);
            this.pnlBuscar.Controls.Add(this.label3);
            this.pnlBuscar.Controls.Add(this.lblActualizar);
            this.pnlBuscar.Controls.Add(this.btnBuscar);
            this.pnlBuscar.Controls.Add(this.txtPersona);
            this.pnlBuscar.Controls.Add(this.label2);
            this.pnlBuscar.Location = new System.Drawing.Point(1, 47);
            this.pnlBuscar.Name = "pnlBuscar";
            this.pnlBuscar.Size = new System.Drawing.Size(807, 74);
            this.pnlBuscar.TabIndex = 29;
            // 
            // fechaDesdePick
            // 
            this.fechaDesdePick.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.fechaDesdePick.CustomFormat = "dd/MM/yyyy  HH:mm";
            this.fechaDesdePick.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.fechaDesdePick.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.fechaDesdePick.Location = new System.Drawing.Point(512, 38);
            this.fechaDesdePick.Name = "fechaDesdePick";
            this.fechaDesdePick.Size = new System.Drawing.Size(128, 20);
            this.fechaDesdePick.TabIndex = 53;
            this.fechaDesdePick.Value = new System.DateTime(2011, 7, 1, 0, 0, 0, 0);
            this.fechaDesdePick.ValueChanged += new System.EventHandler(this.fechaDesdePick_ValueChanged);
            this.fechaDesdePick.KeyDown += new System.Windows.Forms.KeyEventHandler(this.fechaDesdePick_KeyDown);
            // 
            // label3
            // 
            this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.ForeColor = System.Drawing.Color.Cornsilk;
            this.label3.Location = new System.Drawing.Point(463, 40);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(43, 15);
            this.label3.TabIndex = 54;
            this.label3.Text = "Desde";
            // 
            // lblActualizar
            // 
            this.lblActualizar.AutoSize = true;
            this.lblActualizar.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblActualizar.ForeColor = System.Drawing.Color.LightSalmon;
            this.lblActualizar.Location = new System.Drawing.Point(326, 39);
            this.lblActualizar.Name = "lblActualizar";
            this.lblActualizar.Size = new System.Drawing.Size(69, 15);
            this.lblActualizar.TabIndex = 52;
            this.lblActualizar.Text = "Actualizar...";
            this.lblActualizar.Visible = false;
            // 
            // btnBuscar
            // 
            this.btnBuscar.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnBuscar.Location = new System.Drawing.Point(646, 36);
            this.btnBuscar.Name = "btnBuscar";
            this.btnBuscar.Size = new System.Drawing.Size(74, 24);
            this.btnBuscar.TabIndex = 8;
            this.btnBuscar.TabStop = false;
            this.btnBuscar.Text = "Actualizar";
            this.btnBuscar.UseVisualStyleBackColor = true;
            this.btnBuscar.Click += new System.EventHandler(this.btnBuscar_Click);
            // 
            // txtPersona
            // 
            this.txtPersona.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtPersona.Location = new System.Drawing.Point(16, 33);
            this.txtPersona.Name = "txtPersona";
            this.txtPersona.ReadOnly = true;
            this.txtPersona.Size = new System.Drawing.Size(213, 26);
            this.txtPersona.TabIndex = 0;
            this.txtPersona.Text = "Nombre Persona";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.ForeColor = System.Drawing.Color.Cornsilk;
            this.label2.Location = new System.Drawing.Point(13, 15);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(52, 15);
            this.label2.TabIndex = 2;
            this.label2.Text = "Nombre";
            // 
            // barraControl
            // 
            this.barraControl.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.barraControl.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuNuevoPago,
            this.nuevo});
            this.barraControl.Location = new System.Drawing.Point(0, 0);
            this.barraControl.Name = "barraControl";
            this.barraControl.Padding = new System.Windows.Forms.Padding(10, 0, 0, 0);
            this.barraControl.Size = new System.Drawing.Size(808, 45);
            this.barraControl.TabIndex = 30;
            this.barraControl.Text = "toolStrip1";
            // 
            // menuNuevoPago
            // 
            this.menuNuevoPago.Image = ((System.Drawing.Image)(resources.GetObject("menuNuevoPago.Image")));
            this.menuNuevoPago.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.menuNuevoPago.Margin = new System.Windows.Forms.Padding(0, 2, 0, 1);
            this.menuNuevoPago.Name = "menuNuevoPago";
            this.menuNuevoPago.Padding = new System.Windows.Forms.Padding(1, 1, 1, 6);
            this.menuNuevoPago.Size = new System.Drawing.Size(78, 42);
            this.menuNuevoPago.Text = "&Nuevo Pago";
            this.menuNuevoPago.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.menuNuevoPago.Click += new System.EventHandler(this.menuNuevoPago_Click);
            // 
            // nuevo
            // 
            this.nuevo.Image = ((System.Drawing.Image)(resources.GetObject("nuevo.Image")));
            this.nuevo.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.nuevo.Margin = new System.Windows.Forms.Padding(0, 2, 0, 1);
            this.nuevo.Name = "nuevo";
            this.nuevo.Padding = new System.Windows.Forms.Padding(1, 1, 1, 6);
            this.nuevo.Size = new System.Drawing.Size(78, 42);
            this.nuevo.Text = "&Nuevo Mov.";
            this.nuevo.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            // 
            // checkSinRegRepetidos
            // 
            this.checkSinRegRepetidos.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.checkSinRegRepetidos.AutoSize = true;
            this.checkSinRegRepetidos.Checked = true;
            this.checkSinRegRepetidos.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkSinRegRepetidos.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.checkSinRegRepetidos.ForeColor = System.Drawing.SystemColors.Window;
            this.checkSinRegRepetidos.Location = new System.Drawing.Point(651, 3);
            this.checkSinRegRepetidos.Name = "checkSinRegRepetidos";
            this.checkSinRegRepetidos.Size = new System.Drawing.Size(144, 19);
            this.checkSinRegRepetidos.TabIndex = 55;
            this.checkSinRegRepetidos.Text = "Mostrar Repetidos";
            this.checkSinRegRepetidos.UseVisualStyleBackColor = true;
            this.checkSinRegRepetidos.CheckedChanged += new System.EventHandler(this.checkSinRegRepetidos_CheckedChanged);
            // 
            // formCtaCtePersona
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(808, 501);
            this.Controls.Add(this.barraControl);
            this.Controls.Add(this.pnlBuscar);
            this.Controls.Add(this.grillaMovCtaCte);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.btnSeleccionar);
            this.Controls.Add(this.btnCancelar);
            this.Name = "formCtaCtePersona";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Cuenta Corriente";
            this.Load += new System.EventHandler(this.formCtaCtePersona_Load);
            ((System.ComponentModel.ISupportInitialize)(this.grillaMovCtaCte)).EndInit();
            this.pnlBuscar.ResumeLayout(false);
            this.pnlBuscar.PerformLayout();
            this.barraControl.ResumeLayout(false);
            this.barraControl.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        protected System.Windows.Forms.Button btnSeleccionar;
        protected System.Windows.Forms.Button btnCancelar;
        private System.Windows.Forms.DataGridView grillaMovCtaCte;
        protected System.Windows.Forms.Panel pnlBuscar;
        protected System.Windows.Forms.Label lblActualizar;
        protected System.Windows.Forms.Button btnBuscar;
        protected System.Windows.Forms.TextBox txtPersona;
        protected System.Windows.Forms.Label label2;
        protected internal System.Windows.Forms.ToolStrip barraControl;
        protected System.Windows.Forms.ToolStripButton nuevo;
        protected System.Windows.Forms.DateTimePicker fechaDesdePick;
        protected System.Windows.Forms.Label label3;
        protected System.Windows.Forms.ToolStripButton menuNuevoPago;
        private System.Windows.Forms.CheckBox checkSinRegRepetidos;
    }
}