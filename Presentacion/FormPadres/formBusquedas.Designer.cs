namespace Presentacion
{
    partial class formBusquedas
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
            this.panel1 = new System.Windows.Forms.Panel();
            this.txtBuscar = new System.Windows.Forms.TextBox();
            this.Proveedor = new System.Windows.Forms.Label();
            this.button3 = new System.Windows.Forms.Button();
            
            this.btnSeleccionar = new System.Windows.Forms.Button();
            this.btnCancelar = new System.Windows.Forms.Button();
            this.grillaProveedores = new System.Windows.Forms.DataGridView();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.grillaProveedores)).BeginInit();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.panel1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(43)))), ((int)(((byte)(77)))), ((int)(((byte)(129)))));
            this.panel1.Controls.Add(this.txtBuscar);
            this.panel1.Controls.Add(this.Proveedor);
            this.panel1.Controls.Add(this.button3);
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(492, 62);
            this.panel1.TabIndex = 20;
            // 
            // txtBuscar
            // 
            this.txtBuscar.Location = new System.Drawing.Point(81, 21);
            this.txtBuscar.Name = "txtBuscar";
            this.txtBuscar.Size = new System.Drawing.Size(137, 20);
            this.txtBuscar.TabIndex = 3;
            // 
            // Proveedor
            // 
            this.Proveedor.AutoSize = true;
            this.Proveedor.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Proveedor.ForeColor = System.Drawing.Color.Cornsilk;
            this.Proveedor.Location = new System.Drawing.Point(12, 22);
            this.Proveedor.Name = "Proveedor";
            this.Proveedor.Size = new System.Drawing.Size(44, 15);
            this.Proveedor.TabIndex = 2;
            this.Proveedor.Text = "buscar";
            // 
            // button3
            // 
            this.button3.ForeColor = System.Drawing.Color.Black;
            this.button3.Location = new System.Drawing.Point(230, 18);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(67, 25);
            this.button3.TabIndex = 8;
            this.button3.Text = "Buscar";
            this.button3.UseVisualStyleBackColor = true;
        
            // 
            // btnSeleccionar
            // 
            this.btnSeleccionar.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSeleccionar.Location = new System.Drawing.Point(307, 318);
            this.btnSeleccionar.Name = "btnSeleccionar";
            this.btnSeleccionar.Size = new System.Drawing.Size(84, 28);
            this.btnSeleccionar.TabIndex = 23;
            this.btnSeleccionar.Text = "Seleccionar";
            this.btnSeleccionar.UseVisualStyleBackColor = true;
            // 
            // btnCancelar
            // 
            this.btnCancelar.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancelar.Location = new System.Drawing.Point(397, 318);
            this.btnCancelar.Name = "btnCancelar";
            this.btnCancelar.Size = new System.Drawing.Size(84, 27);
            this.btnCancelar.TabIndex = 22;
            this.btnCancelar.Text = "Cancelar";
            this.btnCancelar.UseVisualStyleBackColor = true;
            // 
            // grillaProveedores
            // 
            this.grillaProveedores.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.grillaProveedores.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.grillaProveedores.Location = new System.Drawing.Point(12, 69);
            this.grillaProveedores.Name = "grillaProveedores";
            this.grillaProveedores.Size = new System.Drawing.Size(469, 243);
            this.grillaProveedores.TabIndex = 21;
            // 
            // formBusquedas
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(239)))), ((int)(((byte)(250)))));
            this.ClientSize = new System.Drawing.Size(492, 349);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.btnSeleccionar);
            this.Controls.Add(this.btnCancelar);
            this.Controls.Add(this.grillaProveedores);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MinimizeBox = false;
            this.Name = "formBusquedas";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "formBusquedas";
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.grillaProveedores)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        protected System.Windows.Forms.Panel panel1;
        protected System.Windows.Forms.TextBox txtBuscar;
        protected System.Windows.Forms.Label Proveedor;
        protected System.Windows.Forms.Button button3;
        
        protected System.Windows.Forms.Button btnSeleccionar;
        protected System.Windows.Forms.Button btnCancelar;
        protected System.Windows.Forms.DataGridView grillaProveedores;
    }

    //partial class formBuscarProveedor
    //{
    //    /// <summary>
    //    /// Required designer variable.
    //    /// </summary>
    //    private System.ComponentModel.IContainer components = null;

    //    /// <summary>
    //    /// Clean up any resources being used.
    //    /// </summary>
    //    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    //    protected override void Dispose(bool disposing)
    //    {
    //        if (disposing && (components != null))
    //        {
    //            components.Dispose();
    //        }
    //        base.Dispose(disposing);
    //    }

    //    #region Windows Form Designer generated code

    //    /// <summary>
    //    /// Required method for Designer support - do not modify
    //    /// the contents of this method with the code editor.
    //    /// </summary>
    //    private void InitializeComponent()
    //    {
    //        this.panel1 = new System.Windows.Forms.Panel();
    //        this.txtBuscar = new System.Windows.Forms.TextBox();
    //        this.Proveedor = new System.Windows.Forms.Label();
    //        this.button3 = new System.Windows.Forms.Button();
    //        this.shapeContainer2 = new Microsoft.VisualBasic.PowerPacks.ShapeContainer();
    //        
    //        this.btnSeleccionar = new System.Windows.Forms.Button();
    //        this.btnCancelar = new System.Windows.Forms.Button();
    //        this.grillaProveedores = new System.Windows.Forms.DataGridView();
    //        this.panel1.SuspendLayout();
    //        ((System.ComponentModel.ISupportInitialize)(this.grillaProveedores)).BeginInit();
    //        this.SuspendLayout();
    //        // 
    //        // panel1
    //        // 
    //        this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
    //                    | System.Windows.Forms.AnchorStyles.Right)));
    //        this.panel1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(43)))), ((int)(((byte)(77)))), ((int)(((byte)(129)))));
    //        this.panel1.Controls.Add(this.txtBuscar);
    //        this.panel1.Controls.Add(this.Proveedor);
    //        this.panel1.Controls.Add(this.button3);
    //        this.panel1.Controls.Add(this.shapeContainer2);
    //        this.panel1.Location = new System.Drawing.Point(0, 0);
    //        this.panel1.Name = "panel1";
    //        this.panel1.Size = new System.Drawing.Size(498, 62);
    //        this.panel1.TabIndex = 20;
    //        // 
    //        // txtBuscar
    //        // 
    //        this.txtBuscar.Location = new System.Drawing.Point(81, 21);
    //        this.txtBuscar.Name = "txtBuscar";
    //        this.txtBuscar.Size = new System.Drawing.Size(137, 20);
    //        this.txtBuscar.TabIndex = 3;
    //        // 
    //        // Proveedor
    //        // 
    //        this.Proveedor.AutoSize = true;
    //        this.Proveedor.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
    //        this.Proveedor.ForeColor = System.Drawing.Color.Cornsilk;
    //        this.Proveedor.Location = new System.Drawing.Point(12, 22);
    //        this.Proveedor.Name = "Proveedor";
    //        this.Proveedor.Size = new System.Drawing.Size(44, 15);
    //        this.Proveedor.TabIndex = 2;
    //        this.Proveedor.Text = "buscar";
    //        // 
    //        // button3
    //        // 
    //        this.button3.ForeColor = System.Drawing.Color.Black;
    //        this.button3.Location = new System.Drawing.Point(230, 18);
    //        this.button3.Name = "button3";
    //        this.button3.Size = new System.Drawing.Size(67, 25);
    //        this.button3.TabIndex = 8;
    //        this.button3.Text = "Buscar";
    //        this.button3.UseVisualStyleBackColor = true;
    //        // 
    //        // shapeContainer2
    //        // 
    //        this.shapeContainer2.Location = new System.Drawing.Point(0, 0);
    //        this.shapeContainer2.Margin = new System.Windows.Forms.Padding(0);
    //        this.shapeContainer2.Name = "shapeContainer2";
    //        this.shapeContainer2.Shapes.AddRange(new Microsoft.VisualBasic.PowerPacks.Shape[] {
    //        this.lineShape2});
    //        this.shapeContainer2.Size = new System.Drawing.Size(498, 62);
    //        this.shapeContainer2.TabIndex = 11;
    //        this.shapeContainer2.TabStop = false;
    //        // 
    //        // lineShape2
    //        // 
    //        this.lineShape2.BorderColor = System.Drawing.Color.White;
    //        this.lineShape2.Name = "lineShape2";
    //        this.lineShape2.X1 = 13;
    //        this.lineShape2.X2 = 483;
    //        this.lineShape2.Y1 = 49;
    //        this.lineShape2.Y2 = 49;
    //        // 
    //        // btnSeleccionar
    //        // 
    //        this.btnSeleccionar.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
    //        this.btnSeleccionar.Location = new System.Drawing.Point(307, 318);
    //        this.btnSeleccionar.Name = "btnSeleccionar";
    //        this.btnSeleccionar.Size = new System.Drawing.Size(84, 28);
    //        this.btnSeleccionar.TabIndex = 23;
    //        this.btnSeleccionar.Text = "Seleccionar";
    //        this.btnSeleccionar.UseVisualStyleBackColor = true;
    //        // 
    //        // btnCancelar
    //        // 
    //        this.btnCancelar.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
    //        this.btnCancelar.Location = new System.Drawing.Point(403, 318);
    //        this.btnCancelar.Name = "btnCancelar";
    //        this.btnCancelar.Size = new System.Drawing.Size(84, 27);
    //        this.btnCancelar.TabIndex = 22;
    //        this.btnCancelar.Text = "Cancelar";
    //        this.btnCancelar.UseVisualStyleBackColor = true;
    //        // 
    //        // grillaProveedores
    //        // 
    //        this.grillaProveedores.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
    //                    | System.Windows.Forms.AnchorStyles.Left)
    //                    | System.Windows.Forms.AnchorStyles.Right)));
    //        this.grillaProveedores.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
    //        this.grillaProveedores.Location = new System.Drawing.Point(12, 69);
    //        this.grillaProveedores.Name = "grillaProveedores";
    //        this.grillaProveedores.Size = new System.Drawing.Size(475, 243);
    //        this.grillaProveedores.TabIndex = 21;
    //        // 
    //        //// formBuscarProveedor
    //        //// 
    //        //this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
    //        //this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
    //        //this.ClientSize = new System.Drawing.Size(498, 349);
    //        //this.Controls.Add(this.panel1);
    //        //this.Controls.Add(this.btnSeleccionar);
    //        //this.Controls.Add(this.btnCancelar);
    //        //this.Controls.Add(this.grillaProveedores);
    //        //this.Name = "formBuscarProveedor";
    //        //this.Text = "formBuscarProveedor";
    //        //this.panel1.ResumeLayout(false);
    //        //this.panel1.PerformLayout();
    //        //((System.ComponentModel.ISupportInitialize)(this.grillaProveedores)).EndInit();
    //        //this.ResumeLayout(false);

    //    }

    //    #endregion

    //    protected System.Windows.Forms.Panel panel1;
    //    protected System.Windows.Forms.TextBox txtBuscar;
    //    protected System.Windows.Forms.Label Proveedor;
    //    protected System.Windows.Forms.Button button3;
    //    private Microsoft.VisualBasic.PowerPacks.ShapeContainer shapeContainer2;
    //    
    //    protected System.Windows.Forms.Button btnSeleccionar;
    //    protected System.Windows.Forms.Button btnCancelar;
    //    protected System.Windows.Forms.DataGridView grillaProveedores;
    //}
}