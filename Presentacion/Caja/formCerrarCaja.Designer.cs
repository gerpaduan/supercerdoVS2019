namespace Presentacion.Caja
{
    partial class formCerrarCaja
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
            this.pnlBuscar = new System.Windows.Forms.Panel();
            this.btnImprimir = new System.Windows.Forms.Button();
            this.pickerFechaHoraInicio = new System.Windows.Forms.DateTimePicker();
            this.pickerFechaHoraCierre = new System.Windows.Forms.DateTimePicker();
            this.checkTicket = new System.Windows.Forms.CheckBox();
            this.panel2 = new System.Windows.Forms.Panel();
            this.lblUsuarioCierre = new System.Windows.Forms.Label();
            this.txtUserCierre = new System.Windows.Forms.TextBox();
            this.txtSucursal = new System.Windows.Forms.TextBox();
            this.lblUsuarioInicio = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.txtUserInicio = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.txtFechaHoraCierre = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.txtFechaHoraInicio = new System.Windows.Forms.TextBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.btnIngresoBilletes = new System.Windows.Forms.Button();
            this.lblCortesAnulados = new System.Windows.Forms.Label();
            this.lblDiferenciaEntreCaja = new System.Windows.Forms.Label();
            this.btnCajaAnterior = new System.Windows.Forms.Button();
            this.btnVentas = new System.Windows.Forms.Button();
            this.controlEleccionImporte = new System.Windows.Forms.TrackBar();
            this.btnVerEgresosCaja = new System.Windows.Forms.Button();
            this.txtImporteRetirado = new System.Windows.Forms.TextBox();
            this.lblImporteRetirado = new System.Windows.Forms.Label();
            this.txtCajaInicioSiguiente = new System.Windows.Forms.TextBox();
            this.lblCaja = new System.Windows.Forms.Label();
            this.panel3 = new System.Windows.Forms.Panel();
            this.txtDiferencia = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.txtCajaCierre = new System.Windows.Forms.TextBox();
            this.lblCajaCierre = new System.Windows.Forms.Label();
            this.txtEgresosCaja = new System.Windows.Forms.TextBox();
            this.lblEgresosCaja = new System.Windows.Forms.Label();
            this.txtVentas = new System.Windows.Forms.TextBox();
            this.lblVentas = new System.Windows.Forms.Label();
            this.txtCajaInicial = new System.Windows.Forms.TextBox();
            this.lblCajaInicial = new System.Windows.Forms.Label();
            this.panelTaparCamposCierre = new System.Windows.Forms.Panel();
            this.btnCerrarCaja = new System.Windows.Forms.Button();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.pnlBuscar.SuspendLayout();
            this.panel2.SuspendLayout();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.controlEleccionImporte)).BeginInit();
            this.panel3.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlBuscar
            // 
            this.pnlBuscar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pnlBuscar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(43)))), ((int)(((byte)(77)))), ((int)(((byte)(129)))));
            this.pnlBuscar.Controls.Add(this.btnImprimir);
            this.pnlBuscar.Controls.Add(this.pickerFechaHoraInicio);
            this.pnlBuscar.Controls.Add(this.pickerFechaHoraCierre);
            this.pnlBuscar.Controls.Add(this.checkTicket);
            this.pnlBuscar.Controls.Add(this.panel2);
            this.pnlBuscar.Controls.Add(this.label3);
            this.pnlBuscar.Controls.Add(this.txtFechaHoraCierre);
            this.pnlBuscar.Controls.Add(this.label2);
            this.pnlBuscar.Controls.Add(this.txtFechaHoraInicio);
            this.pnlBuscar.Controls.Add(this.panel1);
            this.pnlBuscar.Controls.Add(this.panelTaparCamposCierre);
            this.pnlBuscar.Location = new System.Drawing.Point(0, 0);
            this.pnlBuscar.Margin = new System.Windows.Forms.Padding(4);
            this.pnlBuscar.Name = "pnlBuscar";
            this.pnlBuscar.Size = new System.Drawing.Size(520, 567);
            this.pnlBuscar.TabIndex = 24;
            // 
            // btnImprimir
            // 
            this.btnImprimir.Location = new System.Drawing.Point(416, 27);
            this.btnImprimir.Margin = new System.Windows.Forms.Padding(4);
            this.btnImprimir.Name = "btnImprimir";
            this.btnImprimir.Size = new System.Drawing.Size(88, 30);
            this.btnImprimir.TabIndex = 38;
            this.btnImprimir.Text = "&Imprimir";
            this.btnImprimir.UseVisualStyleBackColor = true;
            this.btnImprimir.Visible = false;
            this.btnImprimir.Click += new System.EventHandler(this.btnImprimir_Click);
            // 
            // pickerFechaHoraInicio
            // 
            this.pickerFechaHoraInicio.CalendarFont = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.pickerFechaHoraInicio.CustomFormat = "dd/MM/yyyy  HH:mm:ss";
            this.pickerFechaHoraInicio.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.pickerFechaHoraInicio.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.pickerFechaHoraInicio.Location = new System.Drawing.Point(20, 180);
            this.pickerFechaHoraInicio.Margin = new System.Windows.Forms.Padding(4);
            this.pickerFechaHoraInicio.Name = "pickerFechaHoraInicio";
            this.pickerFechaHoraInicio.Size = new System.Drawing.Size(239, 26);
            this.pickerFechaHoraInicio.TabIndex = 49;
            this.pickerFechaHoraInicio.Visible = false;
            this.pickerFechaHoraInicio.ValueChanged += new System.EventHandler(this.pickerDate_ValueChanged);
            // 
            // pickerFechaHoraCierre
            // 
            this.pickerFechaHoraCierre.CalendarFont = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.pickerFechaHoraCierre.CustomFormat = "dd/MM/yyyy  HH:mm:ss";
            this.pickerFechaHoraCierre.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.pickerFechaHoraCierre.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.pickerFechaHoraCierre.Location = new System.Drawing.Point(268, 180);
            this.pickerFechaHoraCierre.Margin = new System.Windows.Forms.Padding(4);
            this.pickerFechaHoraCierre.Name = "pickerFechaHoraCierre";
            this.pickerFechaHoraCierre.Size = new System.Drawing.Size(235, 26);
            this.pickerFechaHoraCierre.TabIndex = 38;
            this.pickerFechaHoraCierre.Visible = false;
            this.pickerFechaHoraCierre.ValueChanged += new System.EventHandler(this.pickerDate_ValueChanged);
            // 
            // checkTicket
            // 
            this.checkTicket.AutoSize = true;
            this.checkTicket.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.checkTicket.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.checkTicket.ForeColor = System.Drawing.Color.Cornsilk;
            this.checkTicket.Location = new System.Drawing.Point(419, 112);
            this.checkTicket.Margin = new System.Windows.Forms.Padding(4);
            this.checkTicket.Name = "checkTicket";
            this.checkTicket.Size = new System.Drawing.Size(76, 24);
            this.checkTicket.TabIndex = 47;
            this.checkTicket.TabStop = false;
            this.checkTicket.Text = "&Ticket";
            this.checkTicket.UseVisualStyleBackColor = true;
            this.checkTicket.Visible = false;
            // 
            // panel2
            // 
            this.panel2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel2.Controls.Add(this.lblUsuarioCierre);
            this.panel2.Controls.Add(this.txtUserCierre);
            this.panel2.Controls.Add(this.txtSucursal);
            this.panel2.Controls.Add(this.lblUsuarioInicio);
            this.panel2.Controls.Add(this.label1);
            this.panel2.Controls.Add(this.txtUserInicio);
            this.panel2.Location = new System.Drawing.Point(16, 15);
            this.panel2.Margin = new System.Windows.Forms.Padding(4);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(391, 121);
            this.panel2.TabIndex = 14;
            // 
            // lblUsuarioCierre
            // 
            this.lblUsuarioCierre.AutoSize = true;
            this.lblUsuarioCierre.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblUsuarioCierre.ForeColor = System.Drawing.Color.Cornsilk;
            this.lblUsuarioCierre.Location = new System.Drawing.Point(17, 86);
            this.lblUsuarioCierre.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblUsuarioCierre.Name = "lblUsuarioCierre";
            this.lblUsuarioCierre.Size = new System.Drawing.Size(132, 20);
            this.lblUsuarioCierre.TabIndex = 17;
            this.lblUsuarioCierre.Text = "Usuario Cierre";
            // 
            // txtUserCierre
            // 
            this.txtUserCierre.BackColor = System.Drawing.SystemColors.ScrollBar;
            this.txtUserCierre.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtUserCierre.Location = new System.Drawing.Point(164, 82);
            this.txtUserCierre.Margin = new System.Windows.Forms.Padding(4);
            this.txtUserCierre.Name = "txtUserCierre";
            this.txtUserCierre.ReadOnly = true;
            this.txtUserCierre.Size = new System.Drawing.Size(199, 26);
            this.txtUserCierre.TabIndex = 16;
            this.txtUserCierre.TabStop = false;
            // 
            // txtSucursal
            // 
            this.txtSucursal.BackColor = System.Drawing.SystemColors.ScrollBar;
            this.txtSucursal.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtSucursal.Location = new System.Drawing.Point(164, 12);
            this.txtSucursal.Margin = new System.Windows.Forms.Padding(4);
            this.txtSucursal.Name = "txtSucursal";
            this.txtSucursal.ReadOnly = true;
            this.txtSucursal.Size = new System.Drawing.Size(199, 26);
            this.txtSucursal.TabIndex = 7;
            this.txtSucursal.TabStop = false;
            // 
            // lblUsuarioInicio
            // 
            this.lblUsuarioInicio.AutoSize = true;
            this.lblUsuarioInicio.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblUsuarioInicio.ForeColor = System.Drawing.Color.Cornsilk;
            this.lblUsuarioInicio.Location = new System.Drawing.Point(17, 52);
            this.lblUsuarioInicio.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblUsuarioInicio.Name = "lblUsuarioInicio";
            this.lblUsuarioInicio.Size = new System.Drawing.Size(125, 20);
            this.lblUsuarioInicio.TabIndex = 15;
            this.lblUsuarioInicio.Text = "Usuario Inicio";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ForeColor = System.Drawing.Color.Cornsilk;
            this.label1.Location = new System.Drawing.Point(64, 16);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(83, 20);
            this.label1.TabIndex = 8;
            this.label1.Text = "Sucursal";
            // 
            // txtUserInicio
            // 
            this.txtUserInicio.BackColor = System.Drawing.SystemColors.ScrollBar;
            this.txtUserInicio.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtUserInicio.Location = new System.Drawing.Point(164, 48);
            this.txtUserInicio.Margin = new System.Windows.Forms.Padding(4);
            this.txtUserInicio.Name = "txtUserInicio";
            this.txtUserInicio.ReadOnly = true;
            this.txtUserInicio.Size = new System.Drawing.Size(199, 26);
            this.txtUserInicio.TabIndex = 14;
            this.txtUserInicio.TabStop = false;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.ForeColor = System.Drawing.Color.Cornsilk;
            this.label3.Location = new System.Drawing.Point(264, 159);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(165, 20);
            this.label3.TabIndex = 12;
            this.label3.Text = "Fecha Hora Cierre";
            // 
            // txtFechaHoraCierre
            // 
            this.txtFechaHoraCierre.BackColor = System.Drawing.SystemColors.ScrollBar;
            this.txtFechaHoraCierre.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtFechaHoraCierre.Location = new System.Drawing.Point(268, 181);
            this.txtFechaHoraCierre.Margin = new System.Windows.Forms.Padding(4);
            this.txtFechaHoraCierre.Name = "txtFechaHoraCierre";
            this.txtFechaHoraCierre.ReadOnly = true;
            this.txtFechaHoraCierre.Size = new System.Drawing.Size(235, 26);
            this.txtFechaHoraCierre.TabIndex = 11;
            this.txtFechaHoraCierre.TabStop = false;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.ForeColor = System.Drawing.Color.Cornsilk;
            this.label2.Location = new System.Drawing.Point(16, 159);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(158, 20);
            this.label2.TabIndex = 10;
            this.label2.Text = "Fecha Hora Inicio";
            // 
            // txtFechaHoraInicio
            // 
            this.txtFechaHoraInicio.BackColor = System.Drawing.SystemColors.ScrollBar;
            this.txtFechaHoraInicio.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtFechaHoraInicio.Location = new System.Drawing.Point(20, 181);
            this.txtFechaHoraInicio.Margin = new System.Windows.Forms.Padding(4);
            this.txtFechaHoraInicio.Name = "txtFechaHoraInicio";
            this.txtFechaHoraInicio.ReadOnly = true;
            this.txtFechaHoraInicio.Size = new System.Drawing.Size(239, 26);
            this.txtFechaHoraInicio.TabIndex = 9;
            this.txtFechaHoraInicio.TabStop = false;
            // 
            // panel1
            // 
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel1.Controls.Add(this.btnIngresoBilletes);
            this.panel1.Controls.Add(this.lblCortesAnulados);
            this.panel1.Controls.Add(this.lblDiferenciaEntreCaja);
            this.panel1.Controls.Add(this.btnCajaAnterior);
            this.panel1.Controls.Add(this.btnVentas);
            this.panel1.Controls.Add(this.controlEleccionImporte);
            this.panel1.Controls.Add(this.btnVerEgresosCaja);
            this.panel1.Controls.Add(this.txtImporteRetirado);
            this.panel1.Controls.Add(this.lblImporteRetirado);
            this.panel1.Controls.Add(this.txtCajaInicioSiguiente);
            this.panel1.Controls.Add(this.lblCaja);
            this.panel1.Controls.Add(this.panel3);
            this.panel1.Controls.Add(this.txtCajaCierre);
            this.panel1.Controls.Add(this.lblCajaCierre);
            this.panel1.Controls.Add(this.txtEgresosCaja);
            this.panel1.Controls.Add(this.lblEgresosCaja);
            this.panel1.Controls.Add(this.txtVentas);
            this.panel1.Controls.Add(this.lblVentas);
            this.panel1.Controls.Add(this.txtCajaInicial);
            this.panel1.Controls.Add(this.lblCajaInicial);
            this.panel1.Location = new System.Drawing.Point(16, 214);
            this.panel1.Margin = new System.Windows.Forms.Padding(4);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(487, 348);
            this.panel1.TabIndex = 13;
            // 
            // btnIngresoBilletes
            // 
            this.btnIngresoBilletes.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnIngresoBilletes.Location = new System.Drawing.Point(420, 164);
            this.btnIngresoBilletes.Margin = new System.Windows.Forms.Padding(4);
            this.btnIngresoBilletes.Name = "btnIngresoBilletes";
            this.btnIngresoBilletes.Size = new System.Drawing.Size(48, 30);
            this.btnIngresoBilletes.TabIndex = 37;
            this.btnIngresoBilletes.Text = "...";
            this.toolTip1.SetToolTip(this.btnIngresoBilletes, "Presione este botón para ingresar las cantidades de billetes");
            this.btnIngresoBilletes.UseVisualStyleBackColor = true;
            this.btnIngresoBilletes.Click += new System.EventHandler(this.btnIngresoBilletes_Click);
            // 
            // lblCortesAnulados
            // 
            this.lblCortesAnulados.AutoSize = true;
            this.lblCortesAnulados.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblCortesAnulados.ForeColor = System.Drawing.Color.Orange;
            this.lblCortesAnulados.Location = new System.Drawing.Point(230, 53);
            this.lblCortesAnulados.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblCortesAnulados.Name = "lblCortesAnulados";
            this.lblCortesAnulados.Size = new System.Drawing.Size(160, 15);
            this.lblCortesAnulados.TabIndex = 36;
            this.lblCortesAnulados.Text = "Existen cortes anulados";
            this.lblCortesAnulados.Visible = false;
            // 
            // lblDiferenciaEntreCaja
            // 
            this.lblDiferenciaEntreCaja.AutoSize = true;
            this.lblDiferenciaEntreCaja.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblDiferenciaEntreCaja.ForeColor = System.Drawing.Color.Orange;
            this.lblDiferenciaEntreCaja.Location = new System.Drawing.Point(212, 6);
            this.lblDiferenciaEntreCaja.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblDiferenciaEntreCaja.Name = "lblDiferenciaEntreCaja";
            this.lblDiferenciaEntreCaja.Size = new System.Drawing.Size(174, 15);
            this.lblDiferenciaEntreCaja.TabIndex = 35;
            this.lblDiferenciaEntreCaja.Text = "Hay diferencias entre caja";
            this.lblDiferenciaEntreCaja.Visible = false;
            // 
            // btnCajaAnterior
            // 
            this.btnCajaAnterior.Location = new System.Drawing.Point(420, 23);
            this.btnCajaAnterior.Margin = new System.Windows.Forms.Padding(4);
            this.btnCajaAnterior.Name = "btnCajaAnterior";
            this.btnCajaAnterior.Size = new System.Drawing.Size(48, 30);
            this.btnCajaAnterior.TabIndex = 34;
            this.btnCajaAnterior.Text = "Ver";
            this.btnCajaAnterior.UseVisualStyleBackColor = true;
            this.btnCajaAnterior.Click += new System.EventHandler(this.btnCajaAnterior_Click);
            // 
            // btnVentas
            // 
            this.btnVentas.Location = new System.Drawing.Point(420, 72);
            this.btnVentas.Margin = new System.Windows.Forms.Padding(4);
            this.btnVentas.Name = "btnVentas";
            this.btnVentas.Size = new System.Drawing.Size(48, 30);
            this.btnVentas.TabIndex = 33;
            this.btnVentas.Text = "Ver";
            this.btnVentas.UseVisualStyleBackColor = true;
            this.btnVentas.Click += new System.EventHandler(this.btnVentas_Click);
            // 
            // controlEleccionImporte
            // 
            this.controlEleccionImporte.Cursor = System.Windows.Forms.Cursors.Hand;
            this.controlEleccionImporte.LargeChange = 2;
            this.controlEleccionImporte.Location = new System.Drawing.Point(424, 255);
            this.controlEleccionImporte.Margin = new System.Windows.Forms.Padding(4);
            this.controlEleccionImporte.Maximum = 1;
            this.controlEleccionImporte.Name = "controlEleccionImporte";
            this.controlEleccionImporte.Orientation = System.Windows.Forms.Orientation.Vertical;
            this.controlEleccionImporte.Size = new System.Drawing.Size(56, 84);
            this.controlEleccionImporte.TabIndex = 0;
            this.controlEleccionImporte.TabStop = false;
            this.controlEleccionImporte.TickStyle = System.Windows.Forms.TickStyle.None;
            this.controlEleccionImporte.ValueChanged += new System.EventHandler(this.controlEleccionImporte_ValueChanged);
            // 
            // btnVerEgresosCaja
            // 
            this.btnVerEgresosCaja.Location = new System.Drawing.Point(420, 116);
            this.btnVerEgresosCaja.Margin = new System.Windows.Forms.Padding(4);
            this.btnVerEgresosCaja.Name = "btnVerEgresosCaja";
            this.btnVerEgresosCaja.Size = new System.Drawing.Size(48, 30);
            this.btnVerEgresosCaja.TabIndex = 32;
            this.btnVerEgresosCaja.Text = "Ver";
            this.btnVerEgresosCaja.UseVisualStyleBackColor = true;
            this.btnVerEgresosCaja.Click += new System.EventHandler(this.btnVerEgresosCaja_Click);
            // 
            // txtImporteRetirado
            // 
            this.txtImporteRetirado.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtImporteRetirado.Location = new System.Drawing.Point(196, 302);
            this.txtImporteRetirado.Margin = new System.Windows.Forms.Padding(4);
            this.txtImporteRetirado.Name = "txtImporteRetirado";
            this.txtImporteRetirado.Size = new System.Drawing.Size(215, 29);
            this.txtImporteRetirado.TabIndex = 31;
            this.txtImporteRetirado.Text = "0";
            this.txtImporteRetirado.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.toolTip1.SetToolTip(this.txtImporteRetirado, "Ingrese aquí la cantidad de dinero que se lleva el dueño.");
            this.txtImporteRetirado.TextChanged += new System.EventHandler(this.txtImporteRetirado_TextChanged);
            // 
            // lblImporteRetirado
            // 
            this.lblImporteRetirado.AutoSize = true;
            this.lblImporteRetirado.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblImporteRetirado.ForeColor = System.Drawing.Color.Cornsilk;
            this.lblImporteRetirado.Location = new System.Drawing.Point(23, 308);
            this.lblImporteRetirado.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblImporteRetirado.Name = "lblImporteRetirado";
            this.lblImporteRetirado.Size = new System.Drawing.Size(152, 20);
            this.lblImporteRetirado.TabIndex = 30;
            this.lblImporteRetirado.Text = "Importe a Retirar";
            this.toolTip1.SetToolTip(this.lblImporteRetirado, "Ingrese aquí la cantidad de dinero que se lleva el dueño.");
            // 
            // txtCajaInicioSiguiente
            // 
            this.txtCajaInicioSiguiente.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtCajaInicioSiguiente.Location = new System.Drawing.Point(196, 260);
            this.txtCajaInicioSiguiente.Margin = new System.Windows.Forms.Padding(4);
            this.txtCajaInicioSiguiente.Name = "txtCajaInicioSiguiente";
            this.txtCajaInicioSiguiente.ReadOnly = true;
            this.txtCajaInicioSiguiente.Size = new System.Drawing.Size(215, 29);
            this.txtCajaInicioSiguiente.TabIndex = 29;
            this.txtCajaInicioSiguiente.Text = "0";
            this.txtCajaInicioSiguiente.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.toolTip1.SetToolTip(this.txtCajaInicioSiguiente, "Aquí ingrese la cantidad de dinero que quedará en la caja para el próximo día.");
            this.txtCajaInicioSiguiente.TextChanged += new System.EventHandler(this.txtCajaInicioSiguiente_TextChanged);
            // 
            // lblCaja
            // 
            this.lblCaja.AutoSize = true;
            this.lblCaja.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblCaja.ForeColor = System.Drawing.Color.Cornsilk;
            this.lblCaja.Location = new System.Drawing.Point(133, 266);
            this.lblCaja.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblCaja.Name = "lblCaja";
            this.lblCaja.Size = new System.Drawing.Size(47, 20);
            this.lblCaja.TabIndex = 28;
            this.lblCaja.Text = "Caja";
            this.toolTip1.SetToolTip(this.lblCaja, "Aquí ingrese la cantidad de dinero que quedará en la caja.");
            // 
            // panel3
            // 
            this.panel3.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(43)))), ((int)(((byte)(77)))), ((int)(((byte)(129)))));
            this.panel3.Controls.Add(this.txtDiferencia);
            this.panel3.Controls.Add(this.label9);
            this.panel3.Location = new System.Drawing.Point(0, 199);
            this.panel3.Margin = new System.Windows.Forms.Padding(4);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(487, 52);
            this.panel3.TabIndex = 27;
            // 
            // txtDiferencia
            // 
            this.txtDiferencia.BackColor = System.Drawing.SystemColors.ScrollBar;
            this.txtDiferencia.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtDiferencia.Location = new System.Drawing.Point(196, 11);
            this.txtDiferencia.Margin = new System.Windows.Forms.Padding(4);
            this.txtDiferencia.Name = "txtDiferencia";
            this.txtDiferencia.ReadOnly = true;
            this.txtDiferencia.Size = new System.Drawing.Size(215, 29);
            this.txtDiferencia.TabIndex = 26;
            this.txtDiferencia.TabStop = false;
            this.txtDiferencia.Text = "0";
            this.txtDiferencia.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.toolTip1.SetToolTip(this.txtDiferencia, "Aquí se muestra el sobrante\r\no faltante de la caja.\r\nSi el número es negativo\r\nsi" +
        "gnifica que faltó dinero\r\nsi es positivo es porque \r\nsobró dinero.");
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label9.ForeColor = System.Drawing.Color.Cornsilk;
            this.label9.Location = new System.Drawing.Point(83, 17);
            this.label9.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(96, 20);
            this.label9.TabIndex = 25;
            this.label9.Text = "Diferencia";
            this.toolTip1.SetToolTip(this.label9, "Aquí se muestra el sobrante");
            // 
            // txtCajaCierre
            // 
            this.txtCajaCierre.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtCajaCierre.Location = new System.Drawing.Point(196, 163);
            this.txtCajaCierre.Margin = new System.Windows.Forms.Padding(4);
            this.txtCajaCierre.Name = "txtCajaCierre";
            this.txtCajaCierre.Size = new System.Drawing.Size(215, 29);
            this.txtCajaCierre.TabIndex = 24;
            this.txtCajaCierre.Text = "0";
            this.txtCajaCierre.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.toolTip1.SetToolTip(this.txtCajaCierre, "Aquí ingrese el total de dinero que hay en la caja.\r\nSe deben contar todos los bi" +
        "lletes incluyendo los de $100 y las monedas.\r\n\r\n");
            this.txtCajaCierre.TextChanged += new System.EventHandler(this.txtCajaCierre_TextChanged);
            // 
            // lblCajaCierre
            // 
            this.lblCajaCierre.AutoSize = true;
            this.lblCajaCierre.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblCajaCierre.ForeColor = System.Drawing.Color.Cornsilk;
            this.lblCajaCierre.Location = new System.Drawing.Point(72, 169);
            this.lblCajaCierre.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblCajaCierre.Name = "lblCajaCierre";
            this.lblCajaCierre.Size = new System.Drawing.Size(105, 20);
            this.lblCajaCierre.TabIndex = 23;
            this.lblCajaCierre.Text = "Caja Cierre";
            this.toolTip1.SetToolTip(this.lblCajaCierre, "Aquí ingrese el total de \r\ndinero que hay en la caja.\r\n");
            // 
            // txtEgresosCaja
            // 
            this.txtEgresosCaja.BackColor = System.Drawing.SystemColors.ScrollBar;
            this.txtEgresosCaja.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtEgresosCaja.Location = new System.Drawing.Point(196, 115);
            this.txtEgresosCaja.Margin = new System.Windows.Forms.Padding(4);
            this.txtEgresosCaja.Name = "txtEgresosCaja";
            this.txtEgresosCaja.ReadOnly = true;
            this.txtEgresosCaja.Size = new System.Drawing.Size(215, 29);
            this.txtEgresosCaja.TabIndex = 22;
            this.txtEgresosCaja.TabStop = false;
            this.txtEgresosCaja.Text = "0";
            this.txtEgresosCaja.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.txtEgresosCaja.TextChanged += new System.EventHandler(this.txtEgresosCaja_TextChanged);
            // 
            // lblEgresosCaja
            // 
            this.lblEgresosCaja.AutoSize = true;
            this.lblEgresosCaja.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblEgresosCaja.ForeColor = System.Drawing.Color.Cornsilk;
            this.lblEgresosCaja.Location = new System.Drawing.Point(52, 121);
            this.lblEgresosCaja.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblEgresosCaja.Name = "lblEgresosCaja";
            this.lblEgresosCaja.Size = new System.Drawing.Size(122, 20);
            this.lblEgresosCaja.TabIndex = 21;
            this.lblEgresosCaja.Text = "Egresos Caja";
            // 
            // txtVentas
            // 
            this.txtVentas.BackColor = System.Drawing.SystemColors.ScrollBar;
            this.txtVentas.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtVentas.Location = new System.Drawing.Point(196, 71);
            this.txtVentas.Margin = new System.Windows.Forms.Padding(4);
            this.txtVentas.Name = "txtVentas";
            this.txtVentas.ReadOnly = true;
            this.txtVentas.Size = new System.Drawing.Size(215, 29);
            this.txtVentas.TabIndex = 20;
            this.txtVentas.TabStop = false;
            this.txtVentas.Text = "0";
            this.txtVentas.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.txtVentas.TextChanged += new System.EventHandler(this.txtVentas_TextChanged);
            // 
            // lblVentas
            // 
            this.lblVentas.AutoSize = true;
            this.lblVentas.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblVentas.ForeColor = System.Drawing.Color.Cornsilk;
            this.lblVentas.Location = new System.Drawing.Point(113, 77);
            this.lblVentas.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblVentas.Name = "lblVentas";
            this.lblVentas.Size = new System.Drawing.Size(67, 20);
            this.lblVentas.TabIndex = 19;
            this.lblVentas.Text = "Ventas";
            // 
            // txtCajaInicial
            // 
            this.txtCajaInicial.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtCajaInicial.Location = new System.Drawing.Point(196, 22);
            this.txtCajaInicial.Margin = new System.Windows.Forms.Padding(4);
            this.txtCajaInicial.Name = "txtCajaInicial";
            this.txtCajaInicial.Size = new System.Drawing.Size(215, 29);
            this.txtCajaInicial.TabIndex = 16;
            this.txtCajaInicial.Text = "0";
            this.txtCajaInicial.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.txtCajaInicial.TextChanged += new System.EventHandler(this.txtCajaInicial_TextChanged);
            // 
            // lblCajaInicial
            // 
            this.lblCajaInicial.AutoSize = true;
            this.lblCajaInicial.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblCajaInicial.ForeColor = System.Drawing.Color.Cornsilk;
            this.lblCajaInicial.Location = new System.Drawing.Point(75, 28);
            this.lblCajaInicial.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblCajaInicial.Name = "lblCajaInicial";
            this.lblCajaInicial.Size = new System.Drawing.Size(103, 20);
            this.lblCajaInicial.TabIndex = 17;
            this.lblCajaInicial.Text = "Caja Inicial";
            // 
            // panelTaparCamposCierre
            // 
            this.panelTaparCamposCierre.Location = new System.Drawing.Point(17, 272);
            this.panelTaparCamposCierre.Margin = new System.Windows.Forms.Padding(4);
            this.panelTaparCamposCierre.Name = "panelTaparCamposCierre";
            this.panelTaparCamposCierre.Size = new System.Drawing.Size(485, 288);
            this.panelTaparCamposCierre.TabIndex = 33;
            // 
            // btnCerrarCaja
            // 
            this.btnCerrarCaja.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnCerrarCaja.Location = new System.Drawing.Point(324, 575);
            this.btnCerrarCaja.Margin = new System.Windows.Forms.Padding(4);
            this.btnCerrarCaja.Name = "btnCerrarCaja";
            this.btnCerrarCaja.Size = new System.Drawing.Size(180, 37);
            this.btnCerrarCaja.TabIndex = 30;
            this.btnCerrarCaja.Text = "&Cerrar Caja";
            this.btnCerrarCaja.UseVisualStyleBackColor = true;
            this.btnCerrarCaja.Click += new System.EventHandler(this.btnCerrarCaja_Click);
            // 
            // formCerrarCaja
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(239)))), ((int)(((byte)(250)))));
            this.ClientSize = new System.Drawing.Size(520, 617);
            this.Controls.Add(this.btnCerrarCaja);
            this.Controls.Add(this.pnlBuscar);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MaximizeBox = false;
            this.Name = "formCerrarCaja";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Cerrar Caja";
            this.Load += new System.EventHandler(this.formCerrarCaja_Load);
            this.pnlBuscar.ResumeLayout(false);
            this.pnlBuscar.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.controlEleccionImporte)).EndInit();
            this.panel3.ResumeLayout(false);
            this.panel3.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        protected System.Windows.Forms.Label label1;
        protected System.Windows.Forms.TextBox txtSucursal;
        protected System.Windows.Forms.Label label2;
        protected System.Windows.Forms.TextBox txtFechaHoraInicio;
        protected System.Windows.Forms.Label label3;
        protected System.Windows.Forms.TextBox txtFechaHoraCierre;
        protected System.Windows.Forms.Label lblUsuarioInicio;
        protected System.Windows.Forms.TextBox txtUserInicio;
        protected System.Windows.Forms.TextBox txtCajaInicial;
        protected System.Windows.Forms.Label lblCajaInicial;
        protected System.Windows.Forms.TextBox txtCajaCierre;
        protected System.Windows.Forms.Label lblCajaCierre;
        protected System.Windows.Forms.TextBox txtEgresosCaja;
        protected System.Windows.Forms.Label lblEgresosCaja;
        protected System.Windows.Forms.TextBox txtVentas;
        protected System.Windows.Forms.Label lblVentas;
        protected System.Windows.Forms.TextBox txtDiferencia;
        protected System.Windows.Forms.Label label9;
        protected System.Windows.Forms.TextBox txtImporteRetirado;
        protected System.Windows.Forms.Label lblImporteRetirado;
        protected System.Windows.Forms.TextBox txtCajaInicioSiguiente;
        protected System.Windows.Forms.Label lblCaja;
        protected System.Windows.Forms.Button btnCerrarCaja;
        protected System.Windows.Forms.Panel panel3;
        protected System.Windows.Forms.Label lblUsuarioCierre;
        protected System.Windows.Forms.TextBox txtUserCierre;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel pnlBuscar;
        private System.Windows.Forms.Button btnVerEgresosCaja;
        private System.Windows.Forms.CheckBox checkTicket;
        private System.Windows.Forms.Panel panelTaparCamposCierre;
        private System.Windows.Forms.TrackBar controlEleccionImporte;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.Button btnVentas;
        private System.Windows.Forms.Button btnCajaAnterior;
        protected System.Windows.Forms.Label lblDiferenciaEntreCaja;
        protected System.Windows.Forms.Label lblCortesAnulados;
        private System.Windows.Forms.Button btnIngresoBilletes;
        private System.Windows.Forms.DateTimePicker pickerFechaHoraCierre;
        private System.Windows.Forms.DateTimePicker pickerFechaHoraInicio;
        private System.Windows.Forms.Button btnImprimir;
    }
}