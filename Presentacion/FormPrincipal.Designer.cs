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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormPrincipal));
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.checkAutoDesconectar = new System.Windows.Forms.CheckBox();
            this.btnLogin = new System.Windows.Forms.Button();
            this.btnCerrarSesion = new System.Windows.Forms.Button();
            this.btnEgresosCaja = new System.Windows.Forms.Button();
            this.btnUsuarios = new System.Windows.Forms.Button();
            this.btnReportes = new System.Windows.Forms.Button();
            this.btnPersonas = new System.Windows.Forms.Button();
            this.btnCortes = new System.Windows.Forms.Button();
            this.btnEmbutidos = new System.Windows.Forms.Button();
            this.btnStock = new System.Windows.Forms.Button();
            this.btnMovimientos = new System.Windows.Forms.Button();
            this.btnCerrarCaja = new System.Windows.Forms.Button();
            this.btnCajaVentas = new System.Windows.Forms.Button();
            this.btnCompras = new System.Windows.Forms.Button();
            this.btnVentas = new System.Windows.Forms.Button();
            this.comboConexion = new System.Windows.Forms.ComboBox();
            this.lblConectadoA = new System.Windows.Forms.Label();
            this.btnTipoConexioin = new System.Windows.Forms.Button();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.comprasToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.verComprasToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ventasToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.verVentasToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.verToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.lineasVentaToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.cajaToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.cierresCajaToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.mantenimientoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.cortesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.personasToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.baseDeDatosToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.balanzaToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.verBalanzaToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.leerPesoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.configuraciónToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ctasCtesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pagosToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.stockToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.stockActualToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.probarToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.imprimirTicketToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.timerInactividadAdmin = new System.Windows.Forms.Timer(this.components);
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Margin = new System.Windows.Forms.Padding(4);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(43)))), ((int)(((byte)(77)))), ((int)(((byte)(129)))));
            this.splitContainer1.Panel1.Controls.Add(this.pictureBox1);
            this.splitContainer1.Panel1.Controls.Add(this.checkAutoDesconectar);
            this.splitContainer1.Panel1.Controls.Add(this.btnLogin);
            this.splitContainer1.Panel1.Controls.Add(this.btnCerrarSesion);
            this.splitContainer1.Panel1.Controls.Add(this.btnEgresosCaja);
            this.splitContainer1.Panel1.Controls.Add(this.btnUsuarios);
            this.splitContainer1.Panel1.Controls.Add(this.btnReportes);
            this.splitContainer1.Panel1.Controls.Add(this.btnPersonas);
            this.splitContainer1.Panel1.Controls.Add(this.btnCortes);
            this.splitContainer1.Panel1.Controls.Add(this.btnEmbutidos);
            this.splitContainer1.Panel1.Controls.Add(this.btnStock);
            this.splitContainer1.Panel1.Controls.Add(this.btnMovimientos);
            this.splitContainer1.Panel1.Controls.Add(this.btnCerrarCaja);
            this.splitContainer1.Panel1.Controls.Add(this.btnCajaVentas);
            this.splitContainer1.Panel1.Controls.Add(this.btnCompras);
            this.splitContainer1.Panel1.Controls.Add(this.btnVentas);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.BackColor = System.Drawing.SystemColors.Window;
            this.splitContainer1.Panel2.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("splitContainer1.Panel2.BackgroundImage")));
            this.splitContainer1.Panel2.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.splitContainer1.Panel2.Controls.Add(this.comboConexion);
            this.splitContainer1.Panel2.Controls.Add(this.lblConectadoA);
            this.splitContainer1.Panel2.Controls.Add(this.btnTipoConexioin);
            this.splitContainer1.Panel2.Controls.Add(this.menuStrip1);
            this.splitContainer1.Size = new System.Drawing.Size(1329, 727);
            this.splitContainer1.SplitterDistance = 270;
            this.splitContainer1.SplitterWidth = 5;
            this.splitContainer1.TabIndex = 0;
            // 
            // checkAutoDesconectar
            // 
            this.checkAutoDesconectar.AutoSize = true;
            this.checkAutoDesconectar.Checked = true;
            this.checkAutoDesconectar.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkAutoDesconectar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.checkAutoDesconectar.Location = new System.Drawing.Point(123, 701);
            this.checkAutoDesconectar.Margin = new System.Windows.Forms.Padding(4);
            this.checkAutoDesconectar.Name = "checkAutoDesconectar";
            this.checkAutoDesconectar.Size = new System.Drawing.Size(121, 20);
            this.checkAutoDesconectar.TabIndex = 31;
            this.checkAutoDesconectar.TabStop = false;
            this.checkAutoDesconectar.Text = "Auto-Desconec.";
            this.checkAutoDesconectar.UseVisualStyleBackColor = true;
            this.checkAutoDesconectar.Visible = false;
            // 
            // btnLogin
            // 
            this.btnLogin.FlatAppearance.BorderSize = 0;
            this.btnLogin.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnLogin.ForeColor = System.Drawing.Color.White;
            this.btnLogin.Location = new System.Drawing.Point(13, 697);
            this.btnLogin.Margin = new System.Windows.Forms.Padding(4);
            this.btnLogin.Name = "btnLogin";
            this.btnLogin.Size = new System.Drawing.Size(105, 28);
            this.btnLogin.TabIndex = 32;
            this.btnLogin.TabStop = false;
            this.btnLogin.Text = "&Iniciar Sesión";
            this.btnLogin.UseVisualStyleBackColor = true;
            this.btnLogin.Click += new System.EventHandler(this.btnLogin_Click);
            // 
            // btnCerrarSesion
            // 
            this.btnCerrarSesion.FlatAppearance.BorderSize = 0;
            this.btnCerrarSesion.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCerrarSesion.ForeColor = System.Drawing.Color.White;
            this.btnCerrarSesion.Location = new System.Drawing.Point(16, 697);
            this.btnCerrarSesion.Margin = new System.Windows.Forms.Padding(4);
            this.btnCerrarSesion.Name = "btnCerrarSesion";
            this.btnCerrarSesion.Size = new System.Drawing.Size(104, 28);
            this.btnCerrarSesion.TabIndex = 33;
            this.btnCerrarSesion.TabStop = false;
            this.btnCerrarSesion.Text = "&Cerrar Ses&ión";
            this.btnCerrarSesion.UseVisualStyleBackColor = true;
            this.btnCerrarSesion.Click += new System.EventHandler(this.btnCerrarSesion_Click);
            // 
            // btnEgresosCaja
            // 
            this.btnEgresosCaja.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(43)))), ((int)(((byte)(77)))), ((int)(((byte)(129)))));
            this.btnEgresosCaja.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(64)))));
            this.btnEgresosCaja.FlatAppearance.MouseDownBackColor = System.Drawing.SystemColors.ActiveCaption;
            this.btnEgresosCaja.FlatAppearance.MouseOverBackColor = System.Drawing.SystemColors.ActiveCaption;
            this.btnEgresosCaja.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnEgresosCaja.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.5F);
            this.btnEgresosCaja.ForeColor = System.Drawing.Color.White;
            this.btnEgresosCaja.Location = new System.Drawing.Point(-7, 249);
            this.btnEgresosCaja.Margin = new System.Windows.Forms.Padding(4);
            this.btnEgresosCaja.Name = "btnEgresosCaja";
            this.btnEgresosCaja.Size = new System.Drawing.Size(280, 50);
            this.btnEgresosCaja.TabIndex = 30;
            this.btnEgresosCaja.TabStop = false;
            this.btnEgresosCaja.Text = "E&gresos Caja";
            this.btnEgresosCaja.UseVisualStyleBackColor = false;
            this.btnEgresosCaja.Click += new System.EventHandler(this.btnEgresosCaja_Click);
            // 
            // btnUsuarios
            // 
            this.btnUsuarios.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(43)))), ((int)(((byte)(77)))), ((int)(((byte)(129)))));
            this.btnUsuarios.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(64)))));
            this.btnUsuarios.FlatAppearance.MouseDownBackColor = System.Drawing.SystemColors.ActiveCaption;
            this.btnUsuarios.FlatAppearance.MouseOverBackColor = System.Drawing.SystemColors.ActiveCaption;
            this.btnUsuarios.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnUsuarios.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.5F);
            this.btnUsuarios.ForeColor = System.Drawing.Color.White;
            this.btnUsuarios.Location = new System.Drawing.Point(-7, 532);
            this.btnUsuarios.Margin = new System.Windows.Forms.Padding(4);
            this.btnUsuarios.Name = "btnUsuarios";
            this.btnUsuarios.Size = new System.Drawing.Size(280, 50);
            this.btnUsuarios.TabIndex = 28;
            this.btnUsuarios.TabStop = false;
            this.btnUsuarios.Text = "&Usuarios";
            this.btnUsuarios.UseVisualStyleBackColor = false;
            this.btnUsuarios.Click += new System.EventHandler(this.btnUsuarios_Click);
            // 
            // btnReportes
            // 
            this.btnReportes.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(43)))), ((int)(((byte)(77)))), ((int)(((byte)(129)))));
            this.btnReportes.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(64)))));
            this.btnReportes.FlatAppearance.MouseDownBackColor = System.Drawing.SystemColors.ActiveCaption;
            this.btnReportes.FlatAppearance.MouseOverBackColor = System.Drawing.SystemColors.ActiveCaption;
            this.btnReportes.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnReportes.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.5F);
            this.btnReportes.ForeColor = System.Drawing.Color.White;
            this.btnReportes.Location = new System.Drawing.Point(-7, 580);
            this.btnReportes.Margin = new System.Windows.Forms.Padding(4);
            this.btnReportes.Name = "btnReportes";
            this.btnReportes.Size = new System.Drawing.Size(279, 50);
            this.btnReportes.TabIndex = 27;
            this.btnReportes.TabStop = false;
            this.btnReportes.Text = "Re&portes";
            this.btnReportes.UseVisualStyleBackColor = false;
            this.btnReportes.Click += new System.EventHandler(this.btnReportes_Click);
            // 
            // btnPersonas
            // 
            this.btnPersonas.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(43)))), ((int)(((byte)(77)))), ((int)(((byte)(129)))));
            this.btnPersonas.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(64)))));
            this.btnPersonas.FlatAppearance.MouseDownBackColor = System.Drawing.SystemColors.ActiveCaption;
            this.btnPersonas.FlatAppearance.MouseOverBackColor = System.Drawing.SystemColors.ActiveCaption;
            this.btnPersonas.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnPersonas.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.5F);
            this.btnPersonas.ForeColor = System.Drawing.Color.White;
            this.btnPersonas.Location = new System.Drawing.Point(-7, 487);
            this.btnPersonas.Margin = new System.Windows.Forms.Padding(4);
            this.btnPersonas.Name = "btnPersonas";
            this.btnPersonas.Size = new System.Drawing.Size(279, 50);
            this.btnPersonas.TabIndex = 25;
            this.btnPersonas.TabStop = false;
            this.btnPersonas.Text = "&Personas";
            this.btnPersonas.UseVisualStyleBackColor = false;
            this.btnPersonas.Click += new System.EventHandler(this.btnPersonas_Click);
            // 
            // btnCortes
            // 
            this.btnCortes.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(43)))), ((int)(((byte)(77)))), ((int)(((byte)(129)))));
            this.btnCortes.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(64)))));
            this.btnCortes.FlatAppearance.MouseDownBackColor = System.Drawing.SystemColors.ActiveCaption;
            this.btnCortes.FlatAppearance.MouseOverBackColor = System.Drawing.SystemColors.ActiveCaption;
            this.btnCortes.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCortes.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.5F);
            this.btnCortes.ForeColor = System.Drawing.Color.White;
            this.btnCortes.Location = new System.Drawing.Point(-7, 442);
            this.btnCortes.Margin = new System.Windows.Forms.Padding(4);
            this.btnCortes.Name = "btnCortes";
            this.btnCortes.Size = new System.Drawing.Size(279, 50);
            this.btnCortes.TabIndex = 24;
            this.btnCortes.TabStop = false;
            this.btnCortes.Text = "Cor&tes";
            this.btnCortes.UseVisualStyleBackColor = false;
            this.btnCortes.Click += new System.EventHandler(this.btnCortes_Click);
            // 
            // btnEmbutidos
            // 
            this.btnEmbutidos.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(43)))), ((int)(((byte)(77)))), ((int)(((byte)(129)))));
            this.btnEmbutidos.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(64)))));
            this.btnEmbutidos.FlatAppearance.MouseDownBackColor = System.Drawing.SystemColors.ActiveCaption;
            this.btnEmbutidos.FlatAppearance.MouseOverBackColor = System.Drawing.SystemColors.ActiveCaption;
            this.btnEmbutidos.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnEmbutidos.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.5F);
            this.btnEmbutidos.ForeColor = System.Drawing.Color.White;
            this.btnEmbutidos.Location = new System.Drawing.Point(-7, 347);
            this.btnEmbutidos.Margin = new System.Windows.Forms.Padding(4);
            this.btnEmbutidos.Name = "btnEmbutidos";
            this.btnEmbutidos.Size = new System.Drawing.Size(280, 50);
            this.btnEmbutidos.TabIndex = 22;
            this.btnEmbutidos.TabStop = false;
            this.btnEmbutidos.Text = "&Embutidos y Otros";
            this.btnEmbutidos.UseVisualStyleBackColor = false;
            this.btnEmbutidos.Click += new System.EventHandler(this.btnEmbutidos_Click);
            // 
            // btnStock
            // 
            this.btnStock.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(43)))), ((int)(((byte)(77)))), ((int)(((byte)(129)))));
            this.btnStock.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(64)))));
            this.btnStock.FlatAppearance.MouseDownBackColor = System.Drawing.SystemColors.ActiveCaption;
            this.btnStock.FlatAppearance.MouseOverBackColor = System.Drawing.SystemColors.ActiveCaption;
            this.btnStock.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnStock.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.5F);
            this.btnStock.ForeColor = System.Drawing.Color.White;
            this.btnStock.Location = new System.Drawing.Point(-7, 393);
            this.btnStock.Margin = new System.Windows.Forms.Padding(4);
            this.btnStock.Name = "btnStock";
            this.btnStock.Size = new System.Drawing.Size(280, 50);
            this.btnStock.TabIndex = 21;
            this.btnStock.TabStop = false;
            this.btnStock.Text = "&Stock";
            this.btnStock.UseVisualStyleBackColor = false;
            this.btnStock.Click += new System.EventHandler(this.btnStock_Click);
            // 
            // btnMovimientos
            // 
            this.btnMovimientos.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(43)))), ((int)(((byte)(77)))), ((int)(((byte)(129)))));
            this.btnMovimientos.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(64)))));
            this.btnMovimientos.FlatAppearance.MouseDownBackColor = System.Drawing.SystemColors.ActiveCaption;
            this.btnMovimientos.FlatAppearance.MouseOverBackColor = System.Drawing.SystemColors.ActiveCaption;
            this.btnMovimientos.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnMovimientos.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.5F);
            this.btnMovimientos.ForeColor = System.Drawing.Color.White;
            this.btnMovimientos.Location = new System.Drawing.Point(-7, 298);
            this.btnMovimientos.Margin = new System.Windows.Forms.Padding(4);
            this.btnMovimientos.Name = "btnMovimientos";
            this.btnMovimientos.Size = new System.Drawing.Size(279, 50);
            this.btnMovimientos.TabIndex = 19;
            this.btnMovimientos.TabStop = false;
            this.btnMovimientos.Text = "&Movimientos";
            this.btnMovimientos.UseVisualStyleBackColor = false;
            this.btnMovimientos.Click += new System.EventHandler(this.btnMovimientos_Click);
            // 
            // btnCerrarCaja
            // 
            this.btnCerrarCaja.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(43)))), ((int)(((byte)(77)))), ((int)(((byte)(129)))));
            this.btnCerrarCaja.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(64)))));
            this.btnCerrarCaja.FlatAppearance.MouseDownBackColor = System.Drawing.SystemColors.ActiveCaption;
            this.btnCerrarCaja.FlatAppearance.MouseOverBackColor = System.Drawing.SystemColors.ActiveCaption;
            this.btnCerrarCaja.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCerrarCaja.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.5F);
            this.btnCerrarCaja.ForeColor = System.Drawing.Color.White;
            this.btnCerrarCaja.Location = new System.Drawing.Point(-7, 200);
            this.btnCerrarCaja.Margin = new System.Windows.Forms.Padding(4);
            this.btnCerrarCaja.Name = "btnCerrarCaja";
            this.btnCerrarCaja.Size = new System.Drawing.Size(280, 50);
            this.btnCerrarCaja.TabIndex = 17;
            this.btnCerrarCaja.TabStop = false;
            this.btnCerrarCaja.Text = "Ce&rrar Caja";
            this.btnCerrarCaja.UseVisualStyleBackColor = false;
            this.btnCerrarCaja.Click += new System.EventHandler(this.btnCerrarCaja_Click);
            // 
            // btnCajaVentas
            // 
            this.btnCajaVentas.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(43)))), ((int)(((byte)(77)))), ((int)(((byte)(129)))));
            this.btnCajaVentas.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(64)))));
            this.btnCajaVentas.FlatAppearance.MouseDownBackColor = System.Drawing.SystemColors.ActiveCaption;
            this.btnCajaVentas.FlatAppearance.MouseOverBackColor = System.Drawing.SystemColors.ActiveCaption;
            this.btnCajaVentas.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCajaVentas.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.5F);
            this.btnCajaVentas.ForeColor = System.Drawing.Color.White;
            this.btnCajaVentas.Location = new System.Drawing.Point(-7, 151);
            this.btnCajaVentas.Margin = new System.Windows.Forms.Padding(4);
            this.btnCajaVentas.Name = "btnCajaVentas";
            this.btnCajaVentas.Size = new System.Drawing.Size(280, 50);
            this.btnCajaVentas.TabIndex = 15;
            this.btnCajaVentas.TabStop = false;
            this.btnCajaVentas.Text = "&Caja Ventas";
            this.btnCajaVentas.UseVisualStyleBackColor = false;
            this.btnCajaVentas.Click += new System.EventHandler(this.btnCajaVentas_Click);
            // 
            // btnCompras
            // 
            this.btnCompras.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(43)))), ((int)(((byte)(77)))), ((int)(((byte)(129)))));
            this.btnCompras.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(64)))));
            this.btnCompras.FlatAppearance.MouseDownBackColor = System.Drawing.SystemColors.ActiveCaption;
            this.btnCompras.FlatAppearance.MouseOverBackColor = System.Drawing.SystemColors.ActiveCaption;
            this.btnCompras.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCompras.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.5F);
            this.btnCompras.ForeColor = System.Drawing.Color.White;
            this.btnCompras.Location = new System.Drawing.Point(-7, 53);
            this.btnCompras.Margin = new System.Windows.Forms.Padding(4);
            this.btnCompras.Name = "btnCompras";
            this.btnCompras.Size = new System.Drawing.Size(280, 50);
            this.btnCompras.TabIndex = 14;
            this.btnCompras.TabStop = false;
            this.btnCompras.Text = "C&ompras";
            this.btnCompras.UseVisualStyleBackColor = false;
            this.btnCompras.Click += new System.EventHandler(this.btnCompras_Click);
            // 
            // btnVentas
            // 
            this.btnVentas.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(43)))), ((int)(((byte)(77)))), ((int)(((byte)(129)))));
            this.btnVentas.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(64)))));
            this.btnVentas.FlatAppearance.MouseDownBackColor = System.Drawing.SystemColors.ActiveCaption;
            this.btnVentas.FlatAppearance.MouseOverBackColor = System.Drawing.SystemColors.ActiveCaption;
            this.btnVentas.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnVentas.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.5F);
            this.btnVentas.ForeColor = System.Drawing.Color.White;
            this.btnVentas.Location = new System.Drawing.Point(-7, 102);
            this.btnVentas.Margin = new System.Windows.Forms.Padding(4);
            this.btnVentas.Name = "btnVentas";
            this.btnVentas.Size = new System.Drawing.Size(280, 50);
            this.btnVentas.TabIndex = 3;
            this.btnVentas.TabStop = false;
            this.btnVentas.Text = "Ve&ntas";
            this.btnVentas.UseVisualStyleBackColor = false;
            this.btnVentas.Click += new System.EventHandler(this.btnVentas_Click);
            // 
            // comboConexion
            // 
            this.comboConexion.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboConexion.Enabled = false;
            this.comboConexion.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.comboConexion.FormattingEnabled = true;
            this.comboConexion.Items.AddRange(new object[] {
            "local",
            "sanMartin",
            "sanMartinRemoto",
            "sanLorenzo",
            "sanLorenzoRemoto"});
            this.comboConexion.Location = new System.Drawing.Point(840, 1);
            this.comboConexion.Margin = new System.Windows.Forms.Padding(4);
            this.comboConexion.Name = "comboConexion";
            this.comboConexion.Size = new System.Drawing.Size(208, 26);
            this.comboConexion.TabIndex = 103;
            this.comboConexion.SelectedIndexChanged += new System.EventHandler(this.comboConexion_SelectedIndexChanged);
            // 
            // lblConectadoA
            // 
            this.lblConectadoA.AutoSize = true;
            this.lblConectadoA.Location = new System.Drawing.Point(731, 6);
            this.lblConectadoA.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblConectadoA.Name = "lblConectadoA";
            this.lblConectadoA.Size = new System.Drawing.Size(93, 16);
            this.lblConectadoA.TabIndex = 102;
            this.lblConectadoA.Text = "|  Conectado a";
            // 
            // btnTipoConexioin
            // 
            this.btnTipoConexioin.BackColor = System.Drawing.SystemColors.Control;
            this.btnTipoConexioin.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnTipoConexioin.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnTipoConexioin.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.btnTipoConexioin.Location = new System.Drawing.Point(509, 0);
            this.btnTipoConexioin.Margin = new System.Windows.Forms.Padding(4);
            this.btnTipoConexioin.Name = "btnTipoConexioin";
            this.btnTipoConexioin.Size = new System.Drawing.Size(152, 28);
            this.btnTipoConexioin.TabIndex = 100;
            this.btnTipoConexioin.TabStop = false;
            this.btnTipoConexioin.Text = "Local";
            this.btnTipoConexioin.UseVisualStyleBackColor = false;
            this.btnTipoConexioin.Visible = false;
            this.btnTipoConexioin.Click += new System.EventHandler(this.btnTipoConexioin_Click);
            // 
            // menuStrip1
            // 
            this.menuStrip1.BackColor = System.Drawing.Color.WhiteSmoke;
            this.menuStrip1.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.comprasToolStripMenuItem,
            this.ventasToolStripMenuItem,
            this.cajaToolStripMenuItem,
            this.mantenimientoToolStripMenuItem,
            this.stockToolStripMenuItem,
            this.probarToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(1054, 30);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // comprasToolStripMenuItem
            // 
            this.comprasToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.verComprasToolStripMenuItem});
            this.comprasToolStripMenuItem.Name = "comprasToolStripMenuItem";
            this.comprasToolStripMenuItem.Size = new System.Drawing.Size(82, 26);
            this.comprasToolStripMenuItem.Text = "Compras";
            // 
            // verComprasToolStripMenuItem
            // 
            this.verComprasToolStripMenuItem.Name = "verComprasToolStripMenuItem";
            this.verComprasToolStripMenuItem.Size = new System.Drawing.Size(176, 26);
            this.verComprasToolStripMenuItem.Text = "Ver Compras";
            this.verComprasToolStripMenuItem.Click += new System.EventHandler(this.verComprasToolStripMenuItem_Click);
            // 
            // ventasToolStripMenuItem
            // 
            this.ventasToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.verVentasToolStripMenuItem,
            this.verToolStripMenuItem,
            this.lineasVentaToolStripMenuItem});
            this.ventasToolStripMenuItem.Name = "ventasToolStripMenuItem";
            this.ventasToolStripMenuItem.Size = new System.Drawing.Size(66, 26);
            this.ventasToolStripMenuItem.Text = "Ventas";
            // 
            // verVentasToolStripMenuItem
            // 
            this.verVentasToolStripMenuItem.Name = "verVentasToolStripMenuItem";
            this.verVentasToolStripMenuItem.Size = new System.Drawing.Size(235, 26);
            this.verVentasToolStripMenuItem.Text = "Ver Ventas";
            this.verVentasToolStripMenuItem.Click += new System.EventHandler(this.verVentasToolStripMenuItem_Click);
            // 
            // verToolStripMenuItem
            // 
            this.verToolStripMenuItem.Name = "verToolStripMenuItem";
            this.verToolStripMenuItem.Size = new System.Drawing.Size(235, 26);
            this.verToolStripMenuItem.Text = "Temporal Linea Venta";
            this.verToolStripMenuItem.Click += new System.EventHandler(this.verToolStripMenuItem_Click);
            // 
            // lineasVentaToolStripMenuItem
            // 
            this.lineasVentaToolStripMenuItem.Name = "lineasVentaToolStripMenuItem";
            this.lineasVentaToolStripMenuItem.Size = new System.Drawing.Size(235, 26);
            this.lineasVentaToolStripMenuItem.Text = "Lineas Venta ";
            this.lineasVentaToolStripMenuItem.Click += new System.EventHandler(this.lineasVentaToolStripMenuItem_Click);
            // 
            // cajaToolStripMenuItem
            // 
            this.cajaToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.cierresCajaToolStripMenuItem});
            this.cajaToolStripMenuItem.Name = "cajaToolStripMenuItem";
            this.cajaToolStripMenuItem.Size = new System.Drawing.Size(52, 26);
            this.cajaToolStripMenuItem.Text = "Caja";
            // 
            // cierresCajaToolStripMenuItem
            // 
            this.cierresCajaToolStripMenuItem.Name = "cierresCajaToolStripMenuItem";
            this.cierresCajaToolStripMenuItem.Size = new System.Drawing.Size(170, 26);
            this.cierresCajaToolStripMenuItem.Text = "Cierres Caja";
            this.cierresCajaToolStripMenuItem.Click += new System.EventHandler(this.cierresCajaToolStripMenuItem_Click);
            // 
            // mantenimientoToolStripMenuItem
            // 
            this.mantenimientoToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.cortesToolStripMenuItem,
            this.personasToolStripMenuItem,
            this.baseDeDatosToolStripMenuItem,
            this.balanzaToolStripMenuItem,
            this.configuraciónToolStripMenuItem,
            this.ctasCtesToolStripMenuItem,
            this.pagosToolStripMenuItem});
            this.mantenimientoToolStripMenuItem.Name = "mantenimientoToolStripMenuItem";
            this.mantenimientoToolStripMenuItem.Size = new System.Drawing.Size(124, 26);
            this.mantenimientoToolStripMenuItem.Text = "Mantenimiento";
            // 
            // cortesToolStripMenuItem
            // 
            this.cortesToolStripMenuItem.Name = "cortesToolStripMenuItem";
            this.cortesToolStripMenuItem.Size = new System.Drawing.Size(187, 26);
            this.cortesToolStripMenuItem.Text = "Cortes";
            this.cortesToolStripMenuItem.Click += new System.EventHandler(this.cortesToolStripMenuItem_Click);
            // 
            // personasToolStripMenuItem
            // 
            this.personasToolStripMenuItem.Name = "personasToolStripMenuItem";
            this.personasToolStripMenuItem.Size = new System.Drawing.Size(187, 26);
            this.personasToolStripMenuItem.Text = "Personas";
            this.personasToolStripMenuItem.Click += new System.EventHandler(this.personasToolStripMenuItem_Click);
            // 
            // baseDeDatosToolStripMenuItem
            // 
            this.baseDeDatosToolStripMenuItem.Name = "baseDeDatosToolStripMenuItem";
            this.baseDeDatosToolStripMenuItem.Size = new System.Drawing.Size(187, 26);
            this.baseDeDatosToolStripMenuItem.Text = "Base de Datos";
            this.baseDeDatosToolStripMenuItem.Click += new System.EventHandler(this.baseDeDatosToolStripMenuItem_Click);
            // 
            // balanzaToolStripMenuItem
            // 
            this.balanzaToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.verBalanzaToolStripMenuItem,
            this.leerPesoToolStripMenuItem});
            this.balanzaToolStripMenuItem.Name = "balanzaToolStripMenuItem";
            this.balanzaToolStripMenuItem.Size = new System.Drawing.Size(187, 26);
            this.balanzaToolStripMenuItem.Text = "Balanza";
            this.balanzaToolStripMenuItem.Click += new System.EventHandler(this.balanzaToolStripMenuItem_Click);
            // 
            // verBalanzaToolStripMenuItem
            // 
            this.verBalanzaToolStripMenuItem.Name = "verBalanzaToolStripMenuItem";
            this.verBalanzaToolStripMenuItem.Size = new System.Drawing.Size(169, 26);
            this.verBalanzaToolStripMenuItem.Text = "Ver Balanza";
            this.verBalanzaToolStripMenuItem.Click += new System.EventHandler(this.verBalanzaToolStripMenuItem_Click);
            // 
            // leerPesoToolStripMenuItem
            // 
            this.leerPesoToolStripMenuItem.Name = "leerPesoToolStripMenuItem";
            this.leerPesoToolStripMenuItem.Size = new System.Drawing.Size(169, 26);
            this.leerPesoToolStripMenuItem.Text = "Leer Peso";
            this.leerPesoToolStripMenuItem.Click += new System.EventHandler(this.leerPesoToolStripMenuItem_Click);
            // 
            // configuraciónToolStripMenuItem
            // 
            this.configuraciónToolStripMenuItem.Name = "configuraciónToolStripMenuItem";
            this.configuraciónToolStripMenuItem.Size = new System.Drawing.Size(187, 26);
            this.configuraciónToolStripMenuItem.Text = "Configuración";
            this.configuraciónToolStripMenuItem.Click += new System.EventHandler(this.configuraciónToolStripMenuItem_Click);
            // 
            // ctasCtesToolStripMenuItem
            // 
            this.ctasCtesToolStripMenuItem.Name = "ctasCtesToolStripMenuItem";
            this.ctasCtesToolStripMenuItem.Size = new System.Drawing.Size(187, 26);
            this.ctasCtesToolStripMenuItem.Text = "Ctas. Ctes.";
            this.ctasCtesToolStripMenuItem.Click += new System.EventHandler(this.ctasCtesToolStripMenuItem_Click);
            // 
            // pagosToolStripMenuItem
            // 
            this.pagosToolStripMenuItem.Name = "pagosToolStripMenuItem";
            this.pagosToolStripMenuItem.Size = new System.Drawing.Size(187, 26);
            this.pagosToolStripMenuItem.Text = "Pagos";
            this.pagosToolStripMenuItem.Click += new System.EventHandler(this.pagosToolStripMenuItem_Click);
            // 
            // stockToolStripMenuItem
            // 
            this.stockToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.stockActualToolStripMenuItem});
            this.stockToolStripMenuItem.Name = "stockToolStripMenuItem";
            this.stockToolStripMenuItem.Size = new System.Drawing.Size(59, 26);
            this.stockToolStripMenuItem.Text = "Stock";
            // 
            // stockActualToolStripMenuItem
            // 
            this.stockActualToolStripMenuItem.Name = "stockActualToolStripMenuItem";
            this.stockActualToolStripMenuItem.Size = new System.Drawing.Size(174, 26);
            this.stockActualToolStripMenuItem.Text = "Stock Actual";
            this.stockActualToolStripMenuItem.Click += new System.EventHandler(this.stockActualToolStripMenuItem_Click);
            // 
            // probarToolStripMenuItem
            // 
            this.probarToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.imprimirTicketToolStripMenuItem});
            this.probarToolStripMenuItem.Name = "probarToolStripMenuItem";
            this.probarToolStripMenuItem.Size = new System.Drawing.Size(67, 26);
            this.probarToolStripMenuItem.Text = "Probar";
            // 
            // imprimirTicketToolStripMenuItem
            // 
            this.imprimirTicketToolStripMenuItem.Name = "imprimirTicketToolStripMenuItem";
            this.imprimirTicketToolStripMenuItem.Size = new System.Drawing.Size(192, 26);
            this.imprimirTicketToolStripMenuItem.Text = "Imprimir Ticket";
            this.imprimirTicketToolStripMenuItem.Click += new System.EventHandler(this.imprimirTicketToolStripMenuItem_Click);
            // 
            // timerInactividadAdmin
            // 
            this.timerInactividadAdmin.Interval = 3000000;
            this.timerInactividadAdmin.Tick += new System.EventHandler(this.timerInactividadAdmin_Tick);
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
            this.pictureBox1.Location = new System.Drawing.Point(42, 4);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(177, 42);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox1.TabIndex = 34;
            this.pictureBox1.TabStop = false;
            // 
            // FormPrincipal
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Black;
            this.ClientSize = new System.Drawing.Size(1329, 727);
            this.Controls.Add(this.splitContainer1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MainMenuStrip = this.menuStrip1;
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MaximizeBox = false;
            this.Name = "FormPrincipal";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Nombre Negocio";
            this.Activated += new System.EventHandler(this.FormPrincipal_Activated);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormPrincipal_FormClosing);
            this.Load += new System.EventHandler(this.FormPrincipal_Load);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem comprasToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem verComprasToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem ventasToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem verVentasToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem mantenimientoToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem cortesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem personasToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem baseDeDatosToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem balanzaToolStripMenuItem;
        private System.Windows.Forms.Button btnTipoConexioin;
        private System.Windows.Forms.ToolStripMenuItem verBalanzaToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem leerPesoToolStripMenuItem;
        private System.Windows.Forms.Button btnVentas;
        private System.Windows.Forms.Button btnCerrarCaja;
        private System.Windows.Forms.Button btnCajaVentas;
        private System.Windows.Forms.Button btnCompras;
        private System.Windows.Forms.Button btnEmbutidos;
        private System.Windows.Forms.Button btnStock;
        private System.Windows.Forms.Button btnMovimientos;
        private System.Windows.Forms.Button btnReportes;
        private System.Windows.Forms.Button btnPersonas;
        private System.Windows.Forms.Button btnCortes;
        private System.Windows.Forms.Button btnUsuarios;
        private System.Windows.Forms.ToolStripMenuItem cajaToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem cierresCajaToolStripMenuItem;
        private System.Windows.Forms.Button btnEgresosCaja;
        private System.Windows.Forms.ToolStripMenuItem probarToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem imprimirTicketToolStripMenuItem;
        private System.Windows.Forms.Label lblConectadoA;
        private System.Windows.Forms.ComboBox comboConexion;
        private System.Windows.Forms.ToolStripMenuItem verToolStripMenuItem;
        private System.Windows.Forms.Timer timerInactividadAdmin;
        private System.Windows.Forms.CheckBox checkAutoDesconectar;
        private System.Windows.Forms.ToolStripMenuItem configuraciónToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem lineasVentaToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem ctasCtesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem pagosToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem stockToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem stockActualToolStripMenuItem;
        private System.Windows.Forms.Button btnLogin;
        private System.Windows.Forms.Button btnCerrarSesion;
        private System.Windows.Forms.PictureBox pictureBox1;
    }
}

