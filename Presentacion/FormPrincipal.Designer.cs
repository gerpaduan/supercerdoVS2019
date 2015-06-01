namespace Presentacion
{
    partial class FormPrincipal
    {
        /// <summary>
        /// Variable del diseñador requerida.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Limpiar los recursos que se estén utilizando.
        /// </summary>
        /// <param name="disposing">true si los recursos administrados se deben eliminar; false en caso contrario, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Código generado por el Diseñador de Windows Forms

        /// <summary>
        /// Método necesario para admitir el Diseñador. No se puede modificar
        /// el contenido del método con el editor de código.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormPrincipal));
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.linkLogin = new System.Windows.Forms.LinkLabel();
            this.linkCerrarSesion = new System.Windows.Forms.LinkLabel();
            this.pnlMantenimientos = new System.Windows.Forms.Panel();
            this.panel4 = new System.Windows.Forms.Panel();
            this.linkBaseDeDatos = new System.Windows.Forms.LinkLabel();
            this.linkPagos = new System.Windows.Forms.LinkLabel();
            this.linkReportes = new System.Windows.Forms.LinkLabel();
            this.linkStock = new System.Windows.Forms.LinkLabel();
            this.linkEmbutidos = new System.Windows.Forms.LinkLabel();
            this.linkLabel1 = new System.Windows.Forms.LinkLabel();
            this.linkPersonas = new System.Windows.Forms.LinkLabel();
            this.linkCortes = new System.Windows.Forms.LinkLabel();
            this.label3 = new System.Windows.Forms.Label();
            this.pnlVentas = new System.Windows.Forms.Panel();
            this.panel1 = new System.Windows.Forms.Panel();
            this.linkVentas = new System.Windows.Forms.LinkLabel();
            this.label1 = new System.Windows.Forms.Label();
            this.pnlCompras = new System.Windows.Forms.Panel();
            this.panel2 = new System.Windows.Forms.Panel();
            this.linkCompras = new System.Windows.Forms.LinkLabel();
            this.label2 = new System.Windows.Forms.Label();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.pnlMantenimientos.SuspendLayout();
            this.panel4.SuspendLayout();
            this.pnlVentas.SuspendLayout();
            this.panel1.SuspendLayout();
            this.pnlCompras.SuspendLayout();
            this.panel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.BackColor = System.Drawing.Color.PeachPuff;
            this.splitContainer1.Panel1.Controls.Add(this.linkLogin);
            this.splitContainer1.Panel1.Controls.Add(this.linkCerrarSesion);
            this.splitContainer1.Panel1.Controls.Add(this.pnlMantenimientos);
            this.splitContainer1.Panel1.Controls.Add(this.pnlVentas);
            this.splitContainer1.Panel1.Controls.Add(this.pnlCompras);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.BackColor = System.Drawing.SystemColors.HotTrack;
            this.splitContainer1.Panel2.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("splitContainer1.Panel2.BackgroundImage")));
            this.splitContainer1.Panel2.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.splitContainer1.Size = new System.Drawing.Size(886, 540);
            this.splitContainer1.SplitterDistance = 241;
            this.splitContainer1.TabIndex = 0;
            // 
            // linkLogin
            // 
            this.linkLogin.AutoSize = true;
            this.linkLogin.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.linkLogin.LinkBehavior = System.Windows.Forms.LinkBehavior.NeverUnderline;
            this.linkLogin.Location = new System.Drawing.Point(12, 518);
            this.linkLogin.Name = "linkLogin";
            this.linkLogin.Size = new System.Drawing.Size(70, 13);
            this.linkLogin.TabIndex = 13;
            this.linkLogin.TabStop = true;
            this.linkLogin.Text = "Iniciar Sesión";
            this.linkLogin.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLogin_LinkClicked);
            // 
            // linkCerrarSesion
            // 
            this.linkCerrarSesion.AutoSize = true;
            this.linkCerrarSesion.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.linkCerrarSesion.LinkBehavior = System.Windows.Forms.LinkBehavior.NeverUnderline;
            this.linkCerrarSesion.Location = new System.Drawing.Point(12, 518);
            this.linkCerrarSesion.Name = "linkCerrarSesion";
            this.linkCerrarSesion.Size = new System.Drawing.Size(70, 13);
            this.linkCerrarSesion.TabIndex = 12;
            this.linkCerrarSesion.TabStop = true;
            this.linkCerrarSesion.Text = "Cerrar Sesión";
            this.linkCerrarSesion.Visible = false;
            this.linkCerrarSesion.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkCerrarSesion_LinkClicked);
            // 
            // pnlMantenimientos
            // 
            this.pnlMantenimientos.BackColor = System.Drawing.Color.SandyBrown;
            this.pnlMantenimientos.Controls.Add(this.panel4);
            this.pnlMantenimientos.Controls.Add(this.label3);
            this.pnlMantenimientos.Location = new System.Drawing.Point(9, 216);
            this.pnlMantenimientos.Name = "pnlMantenimientos";
            this.pnlMantenimientos.Size = new System.Drawing.Size(210, 265);
            this.pnlMantenimientos.TabIndex = 1;
            // 
            // panel4
            // 
            this.panel4.BackColor = System.Drawing.Color.PeachPuff;
            this.panel4.Controls.Add(this.linkBaseDeDatos);
            this.panel4.Controls.Add(this.linkPagos);
            this.panel4.Controls.Add(this.linkReportes);
            this.panel4.Controls.Add(this.linkStock);
            this.panel4.Controls.Add(this.linkEmbutidos);
            this.panel4.Controls.Add(this.linkLabel1);
            this.panel4.Controls.Add(this.linkPersonas);
            this.panel4.Controls.Add(this.linkCortes);
            this.panel4.Location = new System.Drawing.Point(2, 28);
            this.panel4.Name = "panel4";
            this.panel4.Size = new System.Drawing.Size(205, 223);
            this.panel4.TabIndex = 1;
            // 
            // linkBaseDeDatos
            // 
            this.linkBaseDeDatos.AutoSize = true;
            this.linkBaseDeDatos.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.linkBaseDeDatos.LinkBehavior = System.Windows.Forms.LinkBehavior.NeverUnderline;
            this.linkBaseDeDatos.Location = new System.Drawing.Point(23, 183);
            this.linkBaseDeDatos.Name = "linkBaseDeDatos";
            this.linkBaseDeDatos.Size = new System.Drawing.Size(98, 16);
            this.linkBaseDeDatos.TabIndex = 12;
            this.linkBaseDeDatos.TabStop = true;
            this.linkBaseDeDatos.Text = "Base de Datos";
            this.linkBaseDeDatos.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkBaseDeDatos_LinkClicked);
            // 
            // linkPagos
            // 
            this.linkPagos.AutoSize = true;
            this.linkPagos.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.linkPagos.LinkBehavior = System.Windows.Forms.LinkBehavior.NeverUnderline;
            this.linkPagos.Location = new System.Drawing.Point(23, 160);
            this.linkPagos.Name = "linkPagos";
            this.linkPagos.Size = new System.Drawing.Size(48, 16);
            this.linkPagos.TabIndex = 11;
            this.linkPagos.TabStop = true;
            this.linkPagos.Text = "Pagos";
            this.linkPagos.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkPagos_LinkClicked);
            // 
            // linkReportes
            // 
            this.linkReportes.AutoSize = true;
            this.linkReportes.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.linkReportes.LinkBehavior = System.Windows.Forms.LinkBehavior.NeverUnderline;
            this.linkReportes.Location = new System.Drawing.Point(23, 110);
            this.linkReportes.Name = "linkReportes";
            this.linkReportes.Size = new System.Drawing.Size(64, 16);
            this.linkReportes.TabIndex = 10;
            this.linkReportes.TabStop = true;
            this.linkReportes.Text = "Reportes";
            this.linkReportes.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkReportes_LinkClicked);
            // 
            // linkStock
            // 
            this.linkStock.AutoSize = true;
            this.linkStock.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.linkStock.LinkBehavior = System.Windows.Forms.LinkBehavior.NeverUnderline;
            this.linkStock.Location = new System.Drawing.Point(23, 86);
            this.linkStock.Name = "linkStock";
            this.linkStock.Size = new System.Drawing.Size(42, 16);
            this.linkStock.TabIndex = 9;
            this.linkStock.TabStop = true;
            this.linkStock.Text = "Stock";
            this.linkStock.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkStock_LinkClicked);
            // 
            // linkEmbutidos
            // 
            this.linkEmbutidos.AutoSize = true;
            this.linkEmbutidos.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.linkEmbutidos.LinkBehavior = System.Windows.Forms.LinkBehavior.NeverUnderline;
            this.linkEmbutidos.Location = new System.Drawing.Point(23, 62);
            this.linkEmbutidos.Name = "linkEmbutidos";
            this.linkEmbutidos.Size = new System.Drawing.Size(117, 16);
            this.linkEmbutidos.TabIndex = 8;
            this.linkEmbutidos.TabStop = true;
            this.linkEmbutidos.Text = "Embutidos y Otros";
            this.linkEmbutidos.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkEmbutidos_LinkClicked);
            // 
            // linkLabel1
            // 
            this.linkLabel1.AutoSize = true;
            this.linkLabel1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.linkLabel1.LinkBehavior = System.Windows.Forms.LinkBehavior.NeverUnderline;
            this.linkLabel1.Location = new System.Drawing.Point(23, 38);
            this.linkLabel1.Name = "linkLabel1";
            this.linkLabel1.Size = new System.Drawing.Size(84, 16);
            this.linkLabel1.TabIndex = 7;
            this.linkLabel1.TabStop = true;
            this.linkLabel1.Text = "Movimientos";
            this.linkLabel1.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel1_LinkClicked);
            // 
            // linkPersonas
            // 
            this.linkPersonas.AutoSize = true;
            this.linkPersonas.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.linkPersonas.LinkBehavior = System.Windows.Forms.LinkBehavior.NeverUnderline;
            this.linkPersonas.Location = new System.Drawing.Point(23, 135);
            this.linkPersonas.Name = "linkPersonas";
            this.linkPersonas.Size = new System.Drawing.Size(66, 16);
            this.linkPersonas.TabIndex = 4;
            this.linkPersonas.TabStop = true;
            this.linkPersonas.Text = "Personas";
            this.linkPersonas.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkPersonas_LinkClicked);
            // 
            // linkCortes
            // 
            this.linkCortes.AutoSize = true;
            this.linkCortes.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.linkCortes.LinkBehavior = System.Windows.Forms.LinkBehavior.NeverUnderline;
            this.linkCortes.Location = new System.Drawing.Point(23, 15);
            this.linkCortes.Name = "linkCortes";
            this.linkCortes.Size = new System.Drawing.Size(47, 16);
            this.linkCortes.TabIndex = 2;
            this.linkCortes.TabStop = true;
            this.linkCortes.Text = "Cortes";
            this.linkCortes.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkCortes_LinkClicked);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.ForeColor = System.Drawing.SystemColors.ButtonHighlight;
            this.label3.Location = new System.Drawing.Point(44, 7);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(127, 18);
            this.label3.TabIndex = 0;
            this.label3.Text = "Mantenimientos";
            // 
            // pnlVentas
            // 
            this.pnlVentas.BackColor = System.Drawing.Color.SandyBrown;
            this.pnlVentas.Controls.Add(this.panel1);
            this.pnlVentas.Controls.Add(this.label1);
            this.pnlVentas.Location = new System.Drawing.Point(9, 110);
            this.pnlVentas.Name = "pnlVentas";
            this.pnlVentas.Size = new System.Drawing.Size(212, 86);
            this.pnlVentas.TabIndex = 0;
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.PeachPuff;
            this.panel1.Controls.Add(this.linkVentas);
            this.panel1.Location = new System.Drawing.Point(2, 30);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(208, 46);
            this.panel1.TabIndex = 1;
            // 
            // linkVentas
            // 
            this.linkVentas.AutoSize = true;
            this.linkVentas.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.linkVentas.LinkBehavior = System.Windows.Forms.LinkBehavior.NeverUnderline;
            this.linkVentas.Location = new System.Drawing.Point(23, 16);
            this.linkVentas.Name = "linkVentas";
            this.linkVentas.Size = new System.Drawing.Size(108, 16);
            this.linkVentas.TabIndex = 1;
            this.linkVentas.TabStop = true;
            this.linkVentas.Text = "Registrar Ventas";
            this.linkVentas.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkVentas_LinkClicked);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ForeColor = System.Drawing.SystemColors.ButtonHighlight;
            this.label1.Location = new System.Drawing.Point(74, 7);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(59, 18);
            this.label1.TabIndex = 0;
            this.label1.Text = "Ventas";
            // 
            // pnlCompras
            // 
            this.pnlCompras.BackColor = System.Drawing.Color.SandyBrown;
            this.pnlCompras.Controls.Add(this.panel2);
            this.pnlCompras.Controls.Add(this.label2);
            this.pnlCompras.Location = new System.Drawing.Point(9, 12);
            this.pnlCompras.Name = "pnlCompras";
            this.pnlCompras.Size = new System.Drawing.Size(212, 82);
            this.pnlCompras.TabIndex = 0;
            // 
            // panel2
            // 
            this.panel2.BackColor = System.Drawing.Color.PeachPuff;
            this.panel2.Controls.Add(this.linkCompras);
            this.panel2.Location = new System.Drawing.Point(2, 28);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(208, 42);
            this.panel2.TabIndex = 1;
            // 
            // linkCompras
            // 
            this.linkCompras.AutoSize = true;
            this.linkCompras.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.linkCompras.LinkBehavior = System.Windows.Forms.LinkBehavior.NeverUnderline;
            this.linkCompras.Location = new System.Drawing.Point(23, 14);
            this.linkCompras.Name = "linkCompras";
            this.linkCompras.Size = new System.Drawing.Size(121, 16);
            this.linkCompras.TabIndex = 1;
            this.linkCompras.TabStop = true;
            this.linkCompras.Text = "Registrar Compras";
            this.linkCompras.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkCompras_LinkClicked);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.ForeColor = System.Drawing.SystemColors.ButtonHighlight;
            this.label2.Location = new System.Drawing.Point(59, 6);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(77, 18);
            this.label2.TabIndex = 0;
            this.label2.Text = "Compras";
            // 
            // FormPrincipal
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Black;
            this.ClientSize = new System.Drawing.Size(886, 540);
            this.Controls.Add(this.splitContainer1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "FormPrincipal";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Super Cerdo";
            this.Load += new System.EventHandler(this.FormPrincipal_Load);
            this.Activated += new System.EventHandler(this.FormPrincipal_Activated);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.ResumeLayout(false);
            this.pnlMantenimientos.ResumeLayout(false);
            this.pnlMantenimientos.PerformLayout();
            this.panel4.ResumeLayout(false);
            this.panel4.PerformLayout();
            this.pnlVentas.ResumeLayout(false);
            this.pnlVentas.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.pnlCompras.ResumeLayout(false);
            this.pnlCompras.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.Panel pnlVentas;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.LinkLabel linkVentas;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel pnlCompras;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.LinkLabel linkCompras;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Panel pnlMantenimientos;
        private System.Windows.Forms.Panel panel4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.LinkLabel linkPersonas;
        private System.Windows.Forms.LinkLabel linkCortes;
        private System.Windows.Forms.LinkLabel linkLabel1;
        private System.Windows.Forms.LinkLabel linkEmbutidos;
        private System.Windows.Forms.LinkLabel linkStock;
        private System.Windows.Forms.LinkLabel linkReportes;
        private System.Windows.Forms.LinkLabel linkPagos;
        private System.Windows.Forms.LinkLabel linkBaseDeDatos;
        private System.Windows.Forms.LinkLabel linkCerrarSesion;
        private System.Windows.Forms.LinkLabel linkLogin;
    }
}

