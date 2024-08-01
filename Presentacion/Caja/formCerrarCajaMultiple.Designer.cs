namespace Presentacion.Caja
{
    partial class formCerrarCajaMultiple
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle29 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle30 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle31 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle32 = new System.Windows.Forms.DataGridViewCellStyle();
            this.pnlBuscar = new System.Windows.Forms.Panel();
            this.grillaCajasACerrar = new System.Windows.Forms.DataGridView();
            this.id = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.usuarioInicio = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.vendedor = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.fechaHoraInicio = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.cajaInicio = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.gastos = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ventas = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.verEgresosCaja = new System.Windows.Forms.DataGridViewButtonColumn();
            this.verVentas = new System.Windows.Forms.DataGridViewButtonColumn();
            this.checkTicket = new System.Windows.Forms.CheckBox();
            this.panel2 = new System.Windows.Forms.Panel();
            this.lblUsuarioCierre = new System.Windows.Forms.Label();
            this.txtUserCierre = new System.Windows.Forms.TextBox();
            this.txtFechaHoraCierre = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.txtSucursal = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.lblDiferenciaEntreCaja = new System.Windows.Forms.Label();
            this.btnCajaAnterior = new System.Windows.Forms.Button();
            this.controlEleccionImporte = new System.Windows.Forms.TrackBar();
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
            this.btnCerrarCaja = new System.Windows.Forms.Button();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.btnIngresoBilletes = new System.Windows.Forms.Button();
            this.pnlBuscar.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.grillaCajasACerrar)).BeginInit();
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
            this.pnlBuscar.Controls.Add(this.grillaCajasACerrar);
            this.pnlBuscar.Controls.Add(this.checkTicket);
            this.pnlBuscar.Controls.Add(this.panel2);
            this.pnlBuscar.Controls.Add(this.panel1);
            this.pnlBuscar.Location = new System.Drawing.Point(0, 0);
            this.pnlBuscar.Name = "pnlBuscar";
            this.pnlBuscar.Size = new System.Drawing.Size(937, 323);
            this.pnlBuscar.TabIndex = 24;
            // 
            // grillaCajasACerrar
            // 
            this.grillaCajasACerrar.AllowUserToAddRows = false;
            this.grillaCajasACerrar.AllowUserToDeleteRows = false;
            this.grillaCajasACerrar.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.grillaCajasACerrar.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.ColumnHeader;
            this.grillaCajasACerrar.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.grillaCajasACerrar.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.id,
            this.usuarioInicio,
            this.vendedor,
            this.fechaHoraInicio,
            this.cajaInicio,
            this.gastos,
            this.ventas,
            this.verEgresosCaja,
            this.verVentas});
            this.grillaCajasACerrar.Location = new System.Drawing.Point(11, 117);
            this.grillaCajasACerrar.MultiSelect = false;
            this.grillaCajasACerrar.Name = "grillaCajasACerrar";
            this.grillaCajasACerrar.ReadOnly = true;
            this.grillaCajasACerrar.RowHeadersVisible = false;
            this.grillaCajasACerrar.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.AutoSizeToAllHeaders;
            this.grillaCajasACerrar.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.grillaCajasACerrar.Size = new System.Drawing.Size(542, 178);
            this.grillaCajasACerrar.TabIndex = 49;
            this.grillaCajasACerrar.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.grillaCajasACerrar_CellClick);
            // 
            // id
            // 
            this.id.DataPropertyName = "id";
            this.id.HeaderText = "Id";
            this.id.Name = "id";
            this.id.ReadOnly = true;
            this.id.Visible = false;
            this.id.Width = 22;
            // 
            // usuarioInicio
            // 
            this.usuarioInicio.DataPropertyName = "usuarioInicio";
            this.usuarioInicio.HeaderText = "usuarioInicio";
            this.usuarioInicio.Name = "usuarioInicio";
            this.usuarioInicio.ReadOnly = true;
            this.usuarioInicio.Visible = false;
            this.usuarioInicio.Width = 72;
            // 
            // vendedor
            // 
            this.vendedor.DataPropertyName = "nombre";
            this.vendedor.HeaderText = "Vendedor";
            this.vendedor.Name = "vendedor";
            this.vendedor.ReadOnly = true;
            this.vendedor.Width = 78;
            // 
            // fechaHoraInicio
            // 
            this.fechaHoraInicio.DataPropertyName = "fechaHoraInicio";
            dataGridViewCellStyle29.Format = "dd/MM/yyyy HH:mm";
            this.fechaHoraInicio.DefaultCellStyle = dataGridViewCellStyle29;
            this.fechaHoraInicio.HeaderText = "Fecha Apertura Caja";
            this.fechaHoraInicio.Name = "fechaHoraInicio";
            this.fechaHoraInicio.ReadOnly = true;
            this.fechaHoraInicio.Width = 99;
            // 
            // cajaInicio
            // 
            this.cajaInicio.DataPropertyName = "cajaInicio";
            dataGridViewCellStyle30.Format = "F2";
            this.cajaInicio.DefaultCellStyle = dataGridViewCellStyle30;
            this.cajaInicio.HeaderText = "Caja Inicial";
            this.cajaInicio.Name = "cajaInicio";
            this.cajaInicio.ReadOnly = true;
            this.cajaInicio.Width = 77;
            // 
            // gastos
            // 
            this.gastos.DataPropertyName = "gastos";
            dataGridViewCellStyle31.Format = "F2";
            this.gastos.DefaultCellStyle = dataGridViewCellStyle31;
            this.gastos.HeaderText = "EgresosCaja";
            this.gastos.Name = "gastos";
            this.gastos.ReadOnly = true;
            this.gastos.Width = 65;
            // 
            // ventas
            // 
            this.ventas.DataPropertyName = "ventas";
            dataGridViewCellStyle32.Format = "F2";
            this.ventas.DefaultCellStyle = dataGridViewCellStyle32;
            this.ventas.HeaderText = "Ventas";
            this.ventas.Name = "ventas";
            this.ventas.ReadOnly = true;
            this.ventas.Width = 65;
            // 
            // verEgresosCaja
            // 
            this.verEgresosCaja.HeaderText = "Ver EgresosCaja";
            this.verEgresosCaja.Name = "verEgresosCaja";
            this.verEgresosCaja.ReadOnly = true;
            this.verEgresosCaja.Text = "Ver";
            this.verEgresosCaja.ToolTipText = "Ver EgresosCaja";
            this.verEgresosCaja.UseColumnTextForButtonValue = true;
            this.verEgresosCaja.Width = 59;
            // 
            // verVentas
            // 
            this.verVentas.HeaderText = "Ver Ventas";
            this.verVentas.Name = "verVentas";
            this.verVentas.ReadOnly = true;
            this.verVentas.Text = "Ver";
            this.verVentas.ToolTipText = "Ver Ventas";
            this.verVentas.UseColumnTextForButtonValue = true;
            this.verVentas.Width = 59;
            // 
            // checkTicket
            // 
            this.checkTicket.AutoSize = true;
            this.checkTicket.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.checkTicket.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.checkTicket.ForeColor = System.Drawing.Color.Cornsilk;
            this.checkTicket.Location = new System.Drawing.Point(339, 25);
            this.checkTicket.Name = "checkTicket";
            this.checkTicket.Size = new System.Drawing.Size(64, 20);
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
            this.panel2.Controls.Add(this.txtFechaHoraCierre);
            this.panel2.Controls.Add(this.label3);
            this.panel2.Controls.Add(this.txtSucursal);
            this.panel2.Controls.Add(this.label1);
            this.panel2.Location = new System.Drawing.Point(12, 12);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(321, 99);
            this.panel2.TabIndex = 14;
            // 
            // lblUsuarioCierre
            // 
            this.lblUsuarioCierre.AutoSize = true;
            this.lblUsuarioCierre.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblUsuarioCierre.ForeColor = System.Drawing.Color.Cornsilk;
            this.lblUsuarioCierre.Location = new System.Drawing.Point(13, 41);
            this.lblUsuarioCierre.Name = "lblUsuarioCierre";
            this.lblUsuarioCierre.Size = new System.Drawing.Size(108, 16);
            this.lblUsuarioCierre.TabIndex = 17;
            this.lblUsuarioCierre.Text = "Usuario Cierre";
            // 
            // txtUserCierre
            // 
            this.txtUserCierre.BackColor = System.Drawing.SystemColors.ScrollBar;
            this.txtUserCierre.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtUserCierre.Location = new System.Drawing.Point(123, 38);
            this.txtUserCierre.Name = "txtUserCierre";
            this.txtUserCierre.ReadOnly = true;
            this.txtUserCierre.Size = new System.Drawing.Size(179, 22);
            this.txtUserCierre.TabIndex = 16;
            this.txtUserCierre.TabStop = false;
            // 
            // txtFechaHoraCierre
            // 
            this.txtFechaHoraCierre.BackColor = System.Drawing.SystemColors.ScrollBar;
            this.txtFechaHoraCierre.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtFechaHoraCierre.Location = new System.Drawing.Point(123, 66);
            this.txtFechaHoraCierre.Name = "txtFechaHoraCierre";
            this.txtFechaHoraCierre.ReadOnly = true;
            this.txtFechaHoraCierre.Size = new System.Drawing.Size(179, 22);
            this.txtFechaHoraCierre.TabIndex = 11;
            this.txtFechaHoraCierre.TabStop = false;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.ForeColor = System.Drawing.Color.Cornsilk;
            this.label3.Location = new System.Drawing.Point(20, 69);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(97, 16);
            this.label3.TabIndex = 12;
            this.label3.Text = "Fecha Cierre";
            // 
            // txtSucursal
            // 
            this.txtSucursal.BackColor = System.Drawing.SystemColors.ScrollBar;
            this.txtSucursal.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtSucursal.Location = new System.Drawing.Point(123, 10);
            this.txtSucursal.Name = "txtSucursal";
            this.txtSucursal.ReadOnly = true;
            this.txtSucursal.Size = new System.Drawing.Size(179, 22);
            this.txtSucursal.TabIndex = 7;
            this.txtSucursal.TabStop = false;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ForeColor = System.Drawing.Color.Cornsilk;
            this.label1.Location = new System.Drawing.Point(48, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(68, 16);
            this.label1.TabIndex = 8;
            this.label1.Text = "Sucursal";
            // 
            // panel1
            // 
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel1.Controls.Add(this.btnIngresoBilletes);
            this.panel1.Controls.Add(this.lblDiferenciaEntreCaja);
            this.panel1.Controls.Add(this.btnCajaAnterior);
            this.panel1.Controls.Add(this.controlEleccionImporte);
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
            this.panel1.Location = new System.Drawing.Point(559, 12);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(366, 283);
            this.panel1.TabIndex = 13;
            // 
            // lblDiferenciaEntreCaja
            // 
            this.lblDiferenciaEntreCaja.AutoSize = true;
            this.lblDiferenciaEntreCaja.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblDiferenciaEntreCaja.ForeColor = System.Drawing.Color.Orange;
            this.lblDiferenciaEntreCaja.Location = new System.Drawing.Point(159, 5);
            this.lblDiferenciaEntreCaja.Name = "lblDiferenciaEntreCaja";
            this.lblDiferenciaEntreCaja.Size = new System.Drawing.Size(153, 13);
            this.lblDiferenciaEntreCaja.TabIndex = 35;
            this.lblDiferenciaEntreCaja.Text = "Hay diferencias entre caja";
            this.lblDiferenciaEntreCaja.Visible = false;
            // 
            // btnCajaAnterior
            // 
            this.btnCajaAnterior.Location = new System.Drawing.Point(315, 19);
            this.btnCajaAnterior.Name = "btnCajaAnterior";
            this.btnCajaAnterior.Size = new System.Drawing.Size(36, 24);
            this.btnCajaAnterior.TabIndex = 34;
            this.btnCajaAnterior.Text = "Ver";
            this.btnCajaAnterior.UseVisualStyleBackColor = true;
            this.btnCajaAnterior.Click += new System.EventHandler(this.btnCajaAnterior_Click);
            // 
            // controlEleccionImporte
            // 
            this.controlEleccionImporte.Cursor = System.Windows.Forms.Cursors.Hand;
            this.controlEleccionImporte.LargeChange = 2;
            this.controlEleccionImporte.Location = new System.Drawing.Point(318, 207);
            this.controlEleccionImporte.Maximum = 1;
            this.controlEleccionImporte.Name = "controlEleccionImporte";
            this.controlEleccionImporte.Orientation = System.Windows.Forms.Orientation.Vertical;
            this.controlEleccionImporte.Size = new System.Drawing.Size(45, 68);
            this.controlEleccionImporte.TabIndex = 0;
            this.controlEleccionImporte.TickStyle = System.Windows.Forms.TickStyle.None;
            this.controlEleccionImporte.ValueChanged += new System.EventHandler(this.controlEleccionImporte_ValueChanged);
            // 
            // txtImporteRetirado
            // 
            this.txtImporteRetirado.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtImporteRetirado.Location = new System.Drawing.Point(147, 245);
            this.txtImporteRetirado.Name = "txtImporteRetirado";
            this.txtImporteRetirado.Size = new System.Drawing.Size(162, 24);
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
            this.lblImporteRetirado.Location = new System.Drawing.Point(17, 250);
            this.lblImporteRetirado.Name = "lblImporteRetirado";
            this.lblImporteRetirado.Size = new System.Drawing.Size(124, 16);
            this.lblImporteRetirado.TabIndex = 30;
            this.lblImporteRetirado.Text = "Importe a Retirar";
            this.toolTip1.SetToolTip(this.lblImporteRetirado, "Ingrese aquí la cantidad de dinero que se lleva el dueño.");
            // 
            // txtCajaInicioSiguiente
            // 
            this.txtCajaInicioSiguiente.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtCajaInicioSiguiente.Location = new System.Drawing.Point(147, 211);
            this.txtCajaInicioSiguiente.Name = "txtCajaInicioSiguiente";
            this.txtCajaInicioSiguiente.Size = new System.Drawing.Size(162, 24);
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
            this.lblCaja.Location = new System.Drawing.Point(100, 216);
            this.lblCaja.Name = "lblCaja";
            this.lblCaja.Size = new System.Drawing.Size(40, 16);
            this.lblCaja.TabIndex = 28;
            this.lblCaja.Text = "Caja";
            this.toolTip1.SetToolTip(this.lblCaja, "Aquí ingrese la cantidad de dinero que quedará en la caja.");
            // 
            // panel3
            // 
            this.panel3.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(26)))), ((int)(((byte)(74)))), ((int)(((byte)(147)))));
            this.panel3.Controls.Add(this.txtDiferencia);
            this.panel3.Controls.Add(this.label9);
            this.panel3.Location = new System.Drawing.Point(0, 162);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(365, 42);
            this.panel3.TabIndex = 27;
            // 
            // txtDiferencia
            // 
            this.txtDiferencia.BackColor = System.Drawing.SystemColors.ScrollBar;
            this.txtDiferencia.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtDiferencia.Location = new System.Drawing.Point(147, 9);
            this.txtDiferencia.Name = "txtDiferencia";
            this.txtDiferencia.ReadOnly = true;
            this.txtDiferencia.Size = new System.Drawing.Size(162, 24);
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
            this.label9.Location = new System.Drawing.Point(62, 14);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(79, 16);
            this.label9.TabIndex = 25;
            this.label9.Text = "Diferencia";
            this.toolTip1.SetToolTip(this.label9, "Aquí se muestra el sobrante");
            // 
            // txtCajaCierre
            // 
            this.txtCajaCierre.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtCajaCierre.Location = new System.Drawing.Point(147, 130);
            this.txtCajaCierre.Name = "txtCajaCierre";
            this.txtCajaCierre.Size = new System.Drawing.Size(162, 24);
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
            this.lblCajaCierre.Location = new System.Drawing.Point(54, 135);
            this.lblCajaCierre.Name = "lblCajaCierre";
            this.lblCajaCierre.Size = new System.Drawing.Size(86, 16);
            this.lblCajaCierre.TabIndex = 23;
            this.lblCajaCierre.Text = "Caja Cierre";
            this.toolTip1.SetToolTip(this.lblCajaCierre, "Aquí ingrese el total de \r\ndinero que hay en la caja.\r\n");
            // 
            // txtEgresosCaja
            // 
            this.txtEgresosCaja.BackColor = System.Drawing.SystemColors.ScrollBar;
            this.txtEgresosCaja.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtEgresosCaja.Location = new System.Drawing.Point(147, 91);
            this.txtEgresosCaja.Name = "txtEgresosCaja";
            this.txtEgresosCaja.ReadOnly = true;
            this.txtEgresosCaja.Size = new System.Drawing.Size(162, 24);
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
            this.lblEgresosCaja.Location = new System.Drawing.Point(84, 96);
            this.lblEgresosCaja.Name = "lblEgresosCaja";
            this.lblEgresosCaja.Size = new System.Drawing.Size(57, 16);
            this.lblEgresosCaja.TabIndex = 21;
            this.lblEgresosCaja.Text = "EgresosCaja";
            // 
            // txtVentas
            // 
            this.txtVentas.BackColor = System.Drawing.SystemColors.ScrollBar;
            this.txtVentas.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtVentas.Location = new System.Drawing.Point(147, 55);
            this.txtVentas.Name = "txtVentas";
            this.txtVentas.ReadOnly = true;
            this.txtVentas.Size = new System.Drawing.Size(162, 24);
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
            this.lblVentas.Location = new System.Drawing.Point(85, 60);
            this.lblVentas.Name = "lblVentas";
            this.lblVentas.Size = new System.Drawing.Size(56, 16);
            this.lblVentas.TabIndex = 19;
            this.lblVentas.Text = "Ventas";
            // 
            // txtCajaInicial
            // 
            this.txtCajaInicial.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtCajaInicial.Location = new System.Drawing.Point(147, 18);
            this.txtCajaInicial.Name = "txtCajaInicial";
            this.txtCajaInicial.Size = new System.Drawing.Size(162, 24);
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
            this.lblCajaInicial.Location = new System.Drawing.Point(56, 23);
            this.lblCajaInicial.Name = "lblCajaInicial";
            this.lblCajaInicial.Size = new System.Drawing.Size(85, 16);
            this.lblCajaInicial.TabIndex = 17;
            this.lblCajaInicial.Text = "Caja Inicial";
            // 
            // btnCerrarCaja
            // 
            this.btnCerrarCaja.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnCerrarCaja.Location = new System.Drawing.Point(790, 329);
            this.btnCerrarCaja.Name = "btnCerrarCaja";
            this.btnCerrarCaja.Size = new System.Drawing.Size(135, 30);
            this.btnCerrarCaja.TabIndex = 30;
            this.btnCerrarCaja.Text = "&Cerrar Caja";
            this.btnCerrarCaja.UseVisualStyleBackColor = true;
            this.btnCerrarCaja.Click += new System.EventHandler(this.btnCerrarCaja_Click);
            // 
            // btnIngresoBilletes
            // 
            this.btnIngresoBilletes.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.btnIngresoBilletes.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnIngresoBilletes.Location = new System.Drawing.Point(315, 130);
            this.btnIngresoBilletes.Name = "btnIngresoBilletes";
            this.btnIngresoBilletes.Size = new System.Drawing.Size(36, 24);
            this.btnIngresoBilletes.TabIndex = 38;
            this.btnIngresoBilletes.Text = "...";
            this.btnIngresoBilletes.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.toolTip1.SetToolTip(this.btnIngresoBilletes, "Presione este botón para ingresar las cantidades de billetes");
            this.btnIngresoBilletes.UseVisualStyleBackColor = true;
            this.btnIngresoBilletes.Click += new System.EventHandler(this.btnIngresoBilletes_Click);
            // 
            // formCerrarCajaMultiple
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(239)))), ((int)(((byte)(250)))));
            this.ClientSize = new System.Drawing.Size(937, 366);
            this.Controls.Add(this.btnCerrarCaja);
            this.Controls.Add(this.pnlBuscar);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "formCerrarCajaMultiple";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Cerrar Caja";
            this.Load += new System.EventHandler(this.formCerrarCajaMultiple_Load);
            this.pnlBuscar.ResumeLayout(false);
            this.pnlBuscar.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.grillaCajasACerrar)).EndInit();
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
        protected System.Windows.Forms.Label label3;
        protected System.Windows.Forms.TextBox txtFechaHoraCierre;
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
        private System.Windows.Forms.CheckBox checkTicket;
        private System.Windows.Forms.TrackBar controlEleccionImporte;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.Button btnCajaAnterior;
        protected System.Windows.Forms.Label lblDiferenciaEntreCaja;
        protected System.Windows.Forms.DataGridView grillaCajasACerrar;
        private System.Windows.Forms.DataGridViewTextBoxColumn id;
        private System.Windows.Forms.DataGridViewTextBoxColumn usuarioInicio;
        private System.Windows.Forms.DataGridViewTextBoxColumn vendedor;
        private System.Windows.Forms.DataGridViewTextBoxColumn fechaHoraInicio;
        private System.Windows.Forms.DataGridViewTextBoxColumn cajaInicio;
        private System.Windows.Forms.DataGridViewTextBoxColumn gastos;
        private System.Windows.Forms.DataGridViewTextBoxColumn ventas;
        private System.Windows.Forms.DataGridViewButtonColumn verEgresosCaja;
        private System.Windows.Forms.DataGridViewButtonColumn verVentas;
        private System.Windows.Forms.Button btnIngresoBilletes;
    }
}