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
            this.linkReportes = new System.Windows.Forms.LinkLabel();
            this.linkEmbutidos = new System.Windows.Forms.LinkLabel();
            this.linkLabel1 = new System.Windows.Forms.LinkLabel();
            this.linkPersonas = new System.Windows.Forms.LinkLabel();
            this.linkCortes = new System.Windows.Forms.LinkLabel();
            this.label3 = new System.Windows.Forms.Label();
            this.pnlCompras = new System.Windows.Forms.Panel();
            this.panel2 = new System.Windows.Forms.Panel();
            this.linkCompras = new System.Windows.Forms.LinkLabel();
            this.label2 = new System.Windows.Forms.Label();
            this.pnlVentas = new System.Windows.Forms.Panel();
            this.panel5 = new System.Windows.Forms.Panel();
            this.linkVentas = new System.Windows.Forms.LinkLabel();
            this.linkCierresDeCaja = new System.Windows.Forms.LinkLabel();
            this.linkCajaVentas = new System.Windows.Forms.LinkLabel();
            this.linkCerrarCaja = new System.Windows.Forms.LinkLabel();
            this.linkAbrirCaja = new System.Windows.Forms.LinkLabel();
            this.label1 = new System.Windows.Forms.Label();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.comprasToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.verComprasToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ventasToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.verVentasToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.mantenimientoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.cortesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.personasToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.stockCortesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.baseDeDatosToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.pnlMantenimientos.SuspendLayout();
            this.panel4.SuspendLayout();
            this.pnlCompras.SuspendLayout();
            this.panel2.SuspendLayout();
            this.pnlVentas.SuspendLayout();
            this.panel5.SuspendLayout();
            this.menuStrip1.SuspendLayout();
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
            this.splitContainer1.Panel1.Controls.Add(this.pnlCompras);
            this.splitContainer1.Panel1.Controls.Add(this.pnlVentas);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.BackColor = System.Drawing.SystemColors.Window;
            this.splitContainer1.Panel2.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("splitContainer1.Panel2.BackgroundImage")));
            this.splitContainer1.Panel2.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.splitContainer1.Panel2.Controls.Add(this.menuStrip1);
            this.splitContainer1.Panel2.Controls.Add(this.linkCierresDeCaja);
            this.splitContainer1.Panel2.Controls.Add(this.linkCajaVentas);
            this.splitContainer1.Panel2.Controls.Add(this.linkAbrirCaja);
            this.splitContainer1.Panel2.Controls.Add(this.linkCerrarCaja);
            this.splitContainer1.Size = new System.Drawing.Size(997, 581);
            this.splitContainer1.SplitterDistance = 203;
            this.splitContainer1.TabIndex = 0;
            // 
            // linkLogin
            // 
            this.linkLogin.AutoSize = true;
            this.linkLogin.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.linkLogin.LinkBehavior = System.Windows.Forms.LinkBehavior.NeverUnderline;
            this.linkLogin.Location = new System.Drawing.Point(12, 559);
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
            this.linkCerrarSesion.Location = new System.Drawing.Point(12, 559);
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
            this.pnlMantenimientos.Location = new System.Drawing.Point(9, 215);
            this.pnlMantenimientos.Name = "pnlMantenimientos";
            this.pnlMantenimientos.Size = new System.Drawing.Size(175, 341);
            this.pnlMantenimientos.TabIndex = 1;
            // 
            // panel4
            // 
            this.panel4.BackColor = System.Drawing.Color.PeachPuff;
            this.panel4.Controls.Add(this.linkReportes);
            this.panel4.Controls.Add(this.linkEmbutidos);
            this.panel4.Controls.Add(this.linkLabel1);
            this.panel4.Controls.Add(this.linkPersonas);
            this.panel4.Controls.Add(this.linkCortes);
            this.panel4.Location = new System.Drawing.Point(2, 28);
            this.panel4.Name = "panel4";
            this.panel4.Size = new System.Drawing.Size(170, 302);
            this.panel4.TabIndex = 1;
            // 
            // linkReportes
            // 
            this.linkReportes.AutoSize = true;
            this.linkReportes.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.linkReportes.LinkBehavior = System.Windows.Forms.LinkBehavior.NeverUnderline;
            this.linkReportes.Location = new System.Drawing.Point(23, 111);
            this.linkReportes.Name = "linkReportes";
            this.linkReportes.Size = new System.Drawing.Size(64, 16);
            this.linkReportes.TabIndex = 10;
            this.linkReportes.TabStop = true;
            this.linkReportes.Text = "Reportes";
            this.linkReportes.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkReportes_LinkClicked);
            // 
            // linkEmbutidos
            // 
            this.linkEmbutidos.AutoSize = true;
            this.linkEmbutidos.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.linkEmbutidos.LinkBehavior = System.Windows.Forms.LinkBehavior.NeverUnderline;
            this.linkEmbutidos.Location = new System.Drawing.Point(23, 86);
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
            this.linkLabel1.Location = new System.Drawing.Point(23, 62);
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
            this.linkPersonas.Location = new System.Drawing.Point(23, 38);
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
            this.label3.Location = new System.Drawing.Point(25, 7);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(127, 18);
            this.label3.TabIndex = 0;
            this.label3.Text = "Mantenimientos";
            // 
            // pnlCompras
            // 
            this.pnlCompras.BackColor = System.Drawing.Color.SandyBrown;
            this.pnlCompras.Controls.Add(this.panel2);
            this.pnlCompras.Controls.Add(this.label2);
            this.pnlCompras.Location = new System.Drawing.Point(9, 12);
            this.pnlCompras.Name = "pnlCompras";
            this.pnlCompras.Size = new System.Drawing.Size(177, 82);
            this.pnlCompras.TabIndex = 0;
            // 
            // panel2
            // 
            this.panel2.BackColor = System.Drawing.Color.PeachPuff;
            this.panel2.Controls.Add(this.linkCompras);
            this.panel2.Location = new System.Drawing.Point(2, 28);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(173, 42);
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
            this.label2.Location = new System.Drawing.Point(44, 7);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(77, 18);
            this.label2.TabIndex = 0;
            this.label2.Text = "Compras";
            // 
            // pnlVentas
            // 
            this.pnlVentas.BackColor = System.Drawing.Color.SandyBrown;
            this.pnlVentas.Controls.Add(this.panel5);
            this.pnlVentas.Controls.Add(this.label1);
            this.pnlVentas.Location = new System.Drawing.Point(9, 100);
            this.pnlVentas.Name = "pnlVentas";
            this.pnlVentas.Size = new System.Drawing.Size(177, 95);
            this.pnlVentas.TabIndex = 0;
            // 
            // panel5
            // 
            this.panel5.BackColor = System.Drawing.Color.PeachPuff;
            this.panel5.Controls.Add(this.linkVentas);
            this.panel5.Location = new System.Drawing.Point(2, 32);
            this.panel5.Name = "panel5";
            this.panel5.Size = new System.Drawing.Size(173, 52);
            this.panel5.TabIndex = 1;
            // 
            // linkVentas
            // 
            this.linkVentas.AutoSize = true;
            this.linkVentas.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.linkVentas.LinkBehavior = System.Windows.Forms.LinkBehavior.NeverUnderline;
            this.linkVentas.Location = new System.Drawing.Point(21, 13);
            this.linkVentas.Name = "linkVentas";
            this.linkVentas.Size = new System.Drawing.Size(50, 16);
            this.linkVentas.TabIndex = 1;
            this.linkVentas.TabStop = true;
            this.linkVentas.Text = "Ventas";
            this.linkVentas.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkVentas_LinkClicked);
            // 
            // linkCierresDeCaja
            // 
            this.linkCierresDeCaja.AutoSize = true;
            this.linkCierresDeCaja.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.linkCierresDeCaja.LinkBehavior = System.Windows.Forms.LinkBehavior.NeverUnderline;
            this.linkCierresDeCaja.Location = new System.Drawing.Point(103, 215);
            this.linkCierresDeCaja.Name = "linkCierresDeCaja";
            this.linkCierresDeCaja.Size = new System.Drawing.Size(101, 16);
            this.linkCierresDeCaja.TabIndex = 4;
            this.linkCierresDeCaja.TabStop = true;
            this.linkCierresDeCaja.Text = "Cierres de Caja";
            this.linkCierresDeCaja.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkCierresDeCaja_LinkClicked);
            // 
            // linkCajaVentas
            // 
            this.linkCajaVentas.AutoSize = true;
            this.linkCajaVentas.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.linkCajaVentas.LinkBehavior = System.Windows.Forms.LinkBehavior.NeverUnderline;
            this.linkCajaVentas.Location = new System.Drawing.Point(102, 144);
            this.linkCajaVentas.Name = "linkCajaVentas";
            this.linkCajaVentas.Size = new System.Drawing.Size(81, 16);
            this.linkCajaVentas.TabIndex = 1;
            this.linkCajaVentas.TabStop = true;
            this.linkCajaVentas.Text = "Caja Ventas";
            this.linkCajaVentas.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkAbrirCaja_LinkClicked);
            // 
            // linkCerrarCaja
            // 
            this.linkCerrarCaja.AutoSize = true;
            this.linkCerrarCaja.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.linkCerrarCaja.LinkBehavior = System.Windows.Forms.LinkBehavior.NeverUnderline;
            this.linkCerrarCaja.Location = new System.Drawing.Point(103, 194);
            this.linkCerrarCaja.Name = "linkCerrarCaja";
            this.linkCerrarCaja.Size = new System.Drawing.Size(76, 16);
            this.linkCerrarCaja.TabIndex = 2;
            this.linkCerrarCaja.TabStop = true;
            this.linkCerrarCaja.Text = "Cerrar Caja";
            this.linkCerrarCaja.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkCerrarCaja_LinkClicked);
            // 
            // linkAbrirCaja
            // 
            this.linkAbrirCaja.AutoSize = true;
            this.linkAbrirCaja.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.linkAbrirCaja.LinkBehavior = System.Windows.Forms.LinkBehavior.NeverUnderline;
            this.linkAbrirCaja.Location = new System.Drawing.Point(104, 168);
            this.linkAbrirCaja.Name = "linkAbrirCaja";
            this.linkAbrirCaja.Size = new System.Drawing.Size(67, 16);
            this.linkAbrirCaja.TabIndex = 3;
            this.linkAbrirCaja.TabStop = true;
            this.linkAbrirCaja.Text = "Abrir Caja";
            this.linkAbrirCaja.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkAbrirCaja_LinkClicked_1);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ForeColor = System.Drawing.SystemColors.ButtonHighlight;
            this.label1.Location = new System.Drawing.Point(50, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(59, 18);
            this.label1.TabIndex = 0;
            this.label1.Text = "Ventas";
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.comprasToolStripMenuItem,
            this.ventasToolStripMenuItem,
            this.mantenimientoToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(790, 24);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // comprasToolStripMenuItem
            // 
            this.comprasToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.verComprasToolStripMenuItem});
            this.comprasToolStripMenuItem.Name = "comprasToolStripMenuItem";
            this.comprasToolStripMenuItem.Size = new System.Drawing.Size(67, 20);
            this.comprasToolStripMenuItem.Text = "Compras";
            // 
            // verComprasToolStripMenuItem
            // 
            this.verComprasToolStripMenuItem.Name = "verComprasToolStripMenuItem";
            this.verComprasToolStripMenuItem.Size = new System.Drawing.Size(142, 22);
            this.verComprasToolStripMenuItem.Text = "Ver Compras";
            this.verComprasToolStripMenuItem.Click += new System.EventHandler(this.verComprasToolStripMenuItem_Click);
            // 
            // ventasToolStripMenuItem
            // 
            this.ventasToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.verVentasToolStripMenuItem});
            this.ventasToolStripMenuItem.Name = "ventasToolStripMenuItem";
            this.ventasToolStripMenuItem.Size = new System.Drawing.Size(54, 20);
            this.ventasToolStripMenuItem.Text = "Ventas";
            // 
            // verVentasToolStripMenuItem
            // 
            this.verVentasToolStripMenuItem.Name = "verVentasToolStripMenuItem";
            this.verVentasToolStripMenuItem.Size = new System.Drawing.Size(129, 22);
            this.verVentasToolStripMenuItem.Text = "Ver Ventas";
            this.verVentasToolStripMenuItem.Click += new System.EventHandler(this.verVentasToolStripMenuItem_Click);
            // 
            // mantenimientoToolStripMenuItem
            // 
            this.mantenimientoToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.cortesToolStripMenuItem,
            this.personasToolStripMenuItem,
            this.stockCortesToolStripMenuItem,
            this.baseDeDatosToolStripMenuItem});
            this.mantenimientoToolStripMenuItem.Name = "mantenimientoToolStripMenuItem";
            this.mantenimientoToolStripMenuItem.Size = new System.Drawing.Size(101, 20);
            this.mantenimientoToolStripMenuItem.Text = "Mantenimiento";
            // 
            // cortesToolStripMenuItem
            // 
            this.cortesToolStripMenuItem.Name = "cortesToolStripMenuItem";
            this.cortesToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.cortesToolStripMenuItem.Text = "Cortes";
            this.cortesToolStripMenuItem.Click += new System.EventHandler(this.cortesToolStripMenuItem_Click);
            // 
            // personasToolStripMenuItem
            // 
            this.personasToolStripMenuItem.Name = "personasToolStripMenuItem";
            this.personasToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.personasToolStripMenuItem.Text = "Personas";
            this.personasToolStripMenuItem.Click += new System.EventHandler(this.personasToolStripMenuItem_Click);
            // 
            // stockCortesToolStripMenuItem
            // 
            this.stockCortesToolStripMenuItem.Name = "stockCortesToolStripMenuItem";
            this.stockCortesToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.stockCortesToolStripMenuItem.Text = "Stock Cortes";
            this.stockCortesToolStripMenuItem.Click += new System.EventHandler(this.stockCortesToolStripMenuItem_Click);
            // 
            // baseDeDatosToolStripMenuItem
            // 
            this.baseDeDatosToolStripMenuItem.Name = "baseDeDatosToolStripMenuItem";
            this.baseDeDatosToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.baseDeDatosToolStripMenuItem.Text = "Base de Datos";
            this.baseDeDatosToolStripMenuItem.Click += new System.EventHandler(this.baseDeDatosToolStripMenuItem_Click);
            // 
            // FormPrincipal
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Black;
            this.ClientSize = new System.Drawing.Size(997, 581);
            this.Controls.Add(this.splitContainer1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MainMenuStrip = this.menuStrip1;
            this.MaximizeBox = false;
            this.Name = "FormPrincipal";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Super Cerdo";
            this.Load += new System.EventHandler(this.FormPrincipal_Load);
            this.Activated += new System.EventHandler(this.FormPrincipal_Activated);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormPrincipal_FormClosing);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.Panel2.PerformLayout();
            this.splitContainer1.ResumeLayout(false);
            this.pnlMantenimientos.ResumeLayout(false);
            this.pnlMantenimientos.PerformLayout();
            this.panel4.ResumeLayout(false);
            this.panel4.PerformLayout();
            this.pnlCompras.ResumeLayout(false);
            this.pnlCompras.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.pnlVentas.ResumeLayout(false);
            this.pnlVentas.PerformLayout();
            this.panel5.ResumeLayout(false);
            this.panel5.PerformLayout();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.Panel pnlVentas;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.LinkLabel linkVentas;
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
        private System.Windows.Forms.LinkLabel linkReportes;
        private System.Windows.Forms.LinkLabel linkCerrarSesion;
        private System.Windows.Forms.LinkLabel linkLogin;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem comprasToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem verComprasToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem ventasToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem verVentasToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem mantenimientoToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem cortesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem personasToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem stockCortesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem baseDeDatosToolStripMenuItem;
        private System.Windows.Forms.Panel panel5;
        private System.Windows.Forms.LinkLabel linkCierresDeCaja;
        private System.Windows.Forms.LinkLabel linkCajaVentas;
        private System.Windows.Forms.LinkLabel linkCerrarCaja;
        private System.Windows.Forms.LinkLabel linkAbrirCaja;
    }
}

