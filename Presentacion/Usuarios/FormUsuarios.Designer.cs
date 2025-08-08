namespace Presentacion.Usuario
{
    partial class FormUsuarios
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
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.txtClave = new System.Windows.Forms.TextBox();
            this.btnGuardarContra = new System.Windows.Forms.Button();
            this.comboUsuario = new System.Windows.Forms.ComboBox();
            this.txtNueva = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.txtRepetir = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.groupCambiarContra = new System.Windows.Forms.GroupBox();
            this.groupCambiarDatos = new System.Windows.Forms.GroupBox();
            this.checkActivo = new System.Windows.Forms.CheckBox();
            this.checkAdmin = new System.Windows.Forms.CheckBox();
            this.label5 = new System.Windows.Forms.Label();
            this.btnGuardarDatos = new System.Windows.Forms.Button();
            this.txtNombre = new System.Windows.Forms.TextBox();
            this.checkOlvidoClave = new System.Windows.Forms.CheckBox();
            this.btnNuevoUsuario = new System.Windows.Forms.Button();
            this.grillaPermisos = new System.Windows.Forms.DataGridView();
            this.btnGuardarPermisos = new System.Windows.Forms.Button();
            this.checkMarcarVer = new System.Windows.Forms.CheckBox();
            this.checkMarcarEditar = new System.Windows.Forms.CheckBox();
            this.panelMarcarTodos = new System.Windows.Forms.Panel();
            this.comboPropioTodos = new System.Windows.Forms.ComboBox();
            this.label7 = new System.Windows.Forms.Label();
            this.txtDiasVer = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.txtDiasEditar = new System.Windows.Forms.TextBox();
            this.checkCambiosEnLote = new System.Windows.Forms.CheckBox();
            this.groupCambiarContra.SuspendLayout();
            this.groupCambiarDatos.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.grillaPermisos)).BeginInit();
            this.panelMarcarTodos.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(17, 17);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(67, 20);
            this.label1.TabIndex = 0;
            this.label1.Text = "Usuario";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(91, 34);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(95, 20);
            this.label2.TabIndex = 1;
            this.label2.Text = "Contraseña";
            // 
            // txtClave
            // 
            this.txtClave.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtClave.Location = new System.Drawing.Point(201, 31);
            this.txtClave.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.txtClave.Name = "txtClave";
            this.txtClave.PasswordChar = '*';
            this.txtClave.Size = new System.Drawing.Size(187, 26);
            this.txtClave.TabIndex = 2;
            // 
            // btnGuardarContra
            // 
            this.btnGuardarContra.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnGuardarContra.ForeColor = System.Drawing.SystemColors.ControlText;
            this.btnGuardarContra.Location = new System.Drawing.Point(201, 135);
            this.btnGuardarContra.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnGuardarContra.Name = "btnGuardarContra";
            this.btnGuardarContra.Size = new System.Drawing.Size(188, 33);
            this.btnGuardarContra.TabIndex = 4;
            this.btnGuardarContra.Text = "Guardar &Contraseña";
            this.btnGuardarContra.UseVisualStyleBackColor = true;
            this.btnGuardarContra.Click += new System.EventHandler(this.btnGuardarContra_Click);
            // 
            // comboUsuario
            // 
            this.comboUsuario.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboUsuario.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F);
            this.comboUsuario.FormattingEnabled = true;
            this.comboUsuario.Location = new System.Drawing.Point(99, 12);
            this.comboUsuario.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.comboUsuario.Name = "comboUsuario";
            this.comboUsuario.Size = new System.Drawing.Size(185, 28);
            this.comboUsuario.TabIndex = 0;
            this.comboUsuario.SelectedValueChanged += new System.EventHandler(this.comboUsuario_SelectedValueChanged);
            // 
            // txtNueva
            // 
            this.txtNueva.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtNueva.Location = new System.Drawing.Point(201, 65);
            this.txtNueva.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.txtNueva.Name = "txtNueva";
            this.txtNueva.PasswordChar = '*';
            this.txtNueva.Size = new System.Drawing.Size(187, 26);
            this.txtNueva.TabIndex = 3;
            this.txtNueva.TextChanged += new System.EventHandler(this.txtNueva_TextChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(33, 69);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(144, 20);
            this.label3.TabIndex = 6;
            this.label3.Text = "Nueva contraseña";
            // 
            // txtRepetir
            // 
            this.txtRepetir.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtRepetir.Location = new System.Drawing.Point(201, 96);
            this.txtRepetir.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.txtRepetir.Name = "txtRepetir";
            this.txtRepetir.PasswordChar = '*';
            this.txtRepetir.Size = new System.Drawing.Size(187, 26);
            this.txtRepetir.TabIndex = 4;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(27, 103);
            this.label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(151, 20);
            this.label4.TabIndex = 8;
            this.label4.Text = "Repetir contraseña";
            // 
            // groupCambiarContra
            // 
            this.groupCambiarContra.Controls.Add(this.txtRepetir);
            this.groupCambiarContra.Controls.Add(this.label2);
            this.groupCambiarContra.Controls.Add(this.btnGuardarContra);
            this.groupCambiarContra.Controls.Add(this.label4);
            this.groupCambiarContra.Controls.Add(this.txtClave);
            this.groupCambiarContra.Controls.Add(this.txtNueva);
            this.groupCambiarContra.Controls.Add(this.label3);
            this.groupCambiarContra.ForeColor = System.Drawing.Color.Cornsilk;
            this.groupCambiarContra.Location = new System.Drawing.Point(387, 62);
            this.groupCambiarContra.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.groupCambiarContra.Name = "groupCambiarContra";
            this.groupCambiarContra.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.groupCambiarContra.Size = new System.Drawing.Size(397, 176);
            this.groupCambiarContra.TabIndex = 10;
            this.groupCambiarContra.TabStop = false;
            this.groupCambiarContra.Text = "Cambiar contraseña";
            // 
            // groupCambiarDatos
            // 
            this.groupCambiarDatos.Controls.Add(this.checkActivo);
            this.groupCambiarDatos.Controls.Add(this.checkAdmin);
            this.groupCambiarDatos.Controls.Add(this.label5);
            this.groupCambiarDatos.Controls.Add(this.btnGuardarDatos);
            this.groupCambiarDatos.Controls.Add(this.txtNombre);
            this.groupCambiarDatos.ForeColor = System.Drawing.Color.Cornsilk;
            this.groupCambiarDatos.Location = new System.Drawing.Point(16, 62);
            this.groupCambiarDatos.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.groupCambiarDatos.Name = "groupCambiarDatos";
            this.groupCambiarDatos.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.groupCambiarDatos.Size = new System.Drawing.Size(363, 176);
            this.groupCambiarDatos.TabIndex = 11;
            this.groupCambiarDatos.TabStop = false;
            this.groupCambiarDatos.Text = "Cambiar datos";
            // 
            // checkActivo
            // 
            this.checkActivo.AutoSize = true;
            this.checkActivo.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.checkActivo.Enabled = false;
            this.checkActivo.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F);
            this.checkActivo.Location = new System.Drawing.Point(84, 101);
            this.checkActivo.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.checkActivo.Name = "checkActivo";
            this.checkActivo.Size = new System.Drawing.Size(77, 24);
            this.checkActivo.TabIndex = 8;
            this.checkActivo.Text = "Activo";
            this.checkActivo.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.checkActivo.UseVisualStyleBackColor = true;
            // 
            // checkAdmin
            // 
            this.checkAdmin.AutoSize = true;
            this.checkAdmin.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.checkAdmin.Enabled = false;
            this.checkAdmin.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F);
            this.checkAdmin.Location = new System.Drawing.Point(23, 69);
            this.checkAdmin.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.checkAdmin.Name = "checkAdmin";
            this.checkAdmin.Size = new System.Drawing.Size(135, 24);
            this.checkAdmin.TabIndex = 6;
            this.checkAdmin.Text = "Administrador";
            this.checkAdmin.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.checkAdmin.UseVisualStyleBackColor = true;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.Location = new System.Drawing.Point(67, 34);
            this.label5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(68, 20);
            this.label5.TabIndex = 1;
            this.label5.Text = "Nombre";
            // 
            // btnGuardarDatos
            // 
            this.btnGuardarDatos.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnGuardarDatos.ForeColor = System.Drawing.SystemColors.ControlText;
            this.btnGuardarDatos.Location = new System.Drawing.Point(151, 135);
            this.btnGuardarDatos.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnGuardarDatos.Name = "btnGuardarDatos";
            this.btnGuardarDatos.Size = new System.Drawing.Size(188, 33);
            this.btnGuardarDatos.TabIndex = 7;
            this.btnGuardarDatos.Text = "Guardar &Datos";
            this.btnGuardarDatos.UseVisualStyleBackColor = true;
            this.btnGuardarDatos.Click += new System.EventHandler(this.btnGuardarDatos_Click);
            // 
            // txtNombre
            // 
            this.txtNombre.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtNombre.Location = new System.Drawing.Point(151, 31);
            this.txtNombre.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.txtNombre.Name = "txtNombre";
            this.txtNombre.ReadOnly = true;
            this.txtNombre.Size = new System.Drawing.Size(187, 26);
            this.txtNombre.TabIndex = 5;
            this.txtNombre.TabStop = false;
            // 
            // checkOlvidoClave
            // 
            this.checkOlvidoClave.AutoSize = true;
            this.checkOlvidoClave.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.checkOlvidoClave.Enabled = false;
            this.checkOlvidoClave.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.checkOlvidoClave.Location = new System.Drawing.Point(608, 33);
            this.checkOlvidoClave.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.checkOlvidoClave.Name = "checkOlvidoClave";
            this.checkOlvidoClave.Size = new System.Drawing.Size(164, 21);
            this.checkOlvidoClave.TabIndex = 6;
            this.checkOlvidoClave.Text = "Olvidó su contraseña";
            this.checkOlvidoClave.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.checkOlvidoClave.UseVisualStyleBackColor = true;
            this.checkOlvidoClave.CheckedChanged += new System.EventHandler(this.checkOlvidoClave_CheckedChanged);
            // 
            // btnNuevoUsuario
            // 
            this.btnNuevoUsuario.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnNuevoUsuario.ForeColor = System.Drawing.SystemColors.ControlText;
            this.btnNuevoUsuario.Location = new System.Drawing.Point(293, 11);
            this.btnNuevoUsuario.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnNuevoUsuario.Name = "btnNuevoUsuario";
            this.btnNuevoUsuario.Size = new System.Drawing.Size(188, 32);
            this.btnNuevoUsuario.TabIndex = 9;
            this.btnNuevoUsuario.Text = "&Nuevo Usuario";
            this.btnNuevoUsuario.UseVisualStyleBackColor = true;
            this.btnNuevoUsuario.Click += new System.EventHandler(this.btnNuevoUsuario_Click);
            // 
            // grillaPermisos
            // 
            this.grillaPermisos.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.grillaPermisos.Location = new System.Drawing.Point(13, 343);
            this.grillaPermisos.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.grillaPermisos.Name = "grillaPermisos";
            this.grillaPermisos.RowHeadersWidth = 51;
            this.grillaPermisos.RowTemplate.Height = 24;
            this.grillaPermisos.Size = new System.Drawing.Size(771, 363);
            this.grillaPermisos.TabIndex = 12;
            this.grillaPermisos.CellValueChanged += new System.Windows.Forms.DataGridViewCellEventHandler(this.grillaPermisos_CellValueChanged);
            this.grillaPermisos.CurrentCellDirtyStateChanged += new System.EventHandler(this.grillaPermisos_CurrentCellDirtyStateChanged);
            this.grillaPermisos.EditingControlShowing += new System.Windows.Forms.DataGridViewEditingControlShowingEventHandler(this.grillaPermisos_EditingControlShowing);
            // 
            // btnGuardarPermisos
            // 
            this.btnGuardarPermisos.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnGuardarPermisos.ForeColor = System.Drawing.SystemColors.ControlText;
            this.btnGuardarPermisos.Location = new System.Drawing.Point(588, 713);
            this.btnGuardarPermisos.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnGuardarPermisos.Name = "btnGuardarPermisos";
            this.btnGuardarPermisos.Size = new System.Drawing.Size(188, 33);
            this.btnGuardarPermisos.TabIndex = 9;
            this.btnGuardarPermisos.Text = "Editar &Permisos";
            this.btnGuardarPermisos.UseVisualStyleBackColor = true;
            this.btnGuardarPermisos.Click += new System.EventHandler(this.btnGuardarPermisos_Click);
            // 
            // checkMarcarVer
            // 
            this.checkMarcarVer.AutoSize = true;
            this.checkMarcarVer.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.checkMarcarVer.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F);
            this.checkMarcarVer.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.checkMarcarVer.Location = new System.Drawing.Point(117, 4);
            this.checkMarcarVer.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.checkMarcarVer.Name = "checkMarcarVer";
            this.checkMarcarVer.Size = new System.Drawing.Size(177, 24);
            this.checkMarcarVer.TabIndex = 9;
            this.checkMarcarVer.Text = "Ver - Marcar Todos";
            this.checkMarcarVer.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.checkMarcarVer.UseVisualStyleBackColor = true;
            this.checkMarcarVer.CheckedChanged += new System.EventHandler(this.checkMarcarVer_CheckedChanged);
            // 
            // checkMarcarEditar
            // 
            this.checkMarcarEditar.AutoSize = true;
            this.checkMarcarEditar.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.checkMarcarEditar.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F);
            this.checkMarcarEditar.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.checkMarcarEditar.Location = new System.Drawing.Point(337, 4);
            this.checkMarcarEditar.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.checkMarcarEditar.Name = "checkMarcarEditar";
            this.checkMarcarEditar.Size = new System.Drawing.Size(195, 24);
            this.checkMarcarEditar.TabIndex = 13;
            this.checkMarcarEditar.Text = "Editar - Marcar Todos";
            this.checkMarcarEditar.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.checkMarcarEditar.UseVisualStyleBackColor = true;
            this.checkMarcarEditar.CheckedChanged += new System.EventHandler(this.checkMarcarEditar_CheckedChanged);
            // 
            // panelMarcarTodos
            // 
            this.panelMarcarTodos.BackColor = System.Drawing.SystemColors.ActiveCaption;
            this.panelMarcarTodos.Controls.Add(this.comboPropioTodos);
            this.panelMarcarTodos.Controls.Add(this.checkMarcarEditar);
            this.panelMarcarTodos.Controls.Add(this.checkMarcarVer);
            this.panelMarcarTodos.Controls.Add(this.label7);
            this.panelMarcarTodos.Controls.Add(this.txtDiasVer);
            this.panelMarcarTodos.Controls.Add(this.label6);
            this.panelMarcarTodos.Controls.Add(this.txtDiasEditar);
            this.panelMarcarTodos.Enabled = false;
            this.panelMarcarTodos.Location = new System.Drawing.Point(16, 279);
            this.panelMarcarTodos.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.panelMarcarTodos.Name = "panelMarcarTodos";
            this.panelMarcarTodos.Size = new System.Drawing.Size(768, 58);
            this.panelMarcarTodos.TabIndex = 13;
            // 
            // comboPropioTodos
            // 
            this.comboPropioTodos.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboPropioTodos.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.comboPropioTodos.FormattingEnabled = true;
            this.comboPropioTodos.Items.AddRange(new object[] {
            "Propios",
            "Todos"});
            this.comboPropioTodos.Location = new System.Drawing.Point(611, 26);
            this.comboPropioTodos.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.comboPropioTodos.Name = "comboPropioTodos";
            this.comboPropioTodos.Size = new System.Drawing.Size(152, 26);
            this.comboPropioTodos.TabIndex = 17;
            this.comboPropioTodos.Visible = false;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label7.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.label7.Location = new System.Drawing.Point(555, 32);
            this.label7.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(40, 20);
            this.label7.TabIndex = 16;
            this.label7.Text = "días";
            // 
            // txtDiasVer
            // 
            this.txtDiasVer.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F);
            this.txtDiasVer.Location = new System.Drawing.Point(251, 28);
            this.txtDiasVer.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.txtDiasVer.Name = "txtDiasVer";
            this.txtDiasVer.Size = new System.Drawing.Size(56, 26);
            this.txtDiasVer.TabIndex = 14;
            this.txtDiasVer.TextChanged += new System.EventHandler(this.txtDiasVer_TextChanged);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.label6.Location = new System.Drawing.Point(311, 32);
            this.label6.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(40, 20);
            this.label6.TabIndex = 9;
            this.label6.Text = "días";
            // 
            // txtDiasEditar
            // 
            this.txtDiasEditar.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F);
            this.txtDiasEditar.Location = new System.Drawing.Point(481, 28);
            this.txtDiasEditar.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.txtDiasEditar.Name = "txtDiasEditar";
            this.txtDiasEditar.Size = new System.Drawing.Size(64, 26);
            this.txtDiasEditar.TabIndex = 15;
            this.txtDiasEditar.TextChanged += new System.EventHandler(this.txtDiasEditar_TextChanged);
            // 
            // checkCambiosEnLote
            // 
            this.checkCambiosEnLote.AutoSize = true;
            this.checkCambiosEnLote.CheckAlign = System.Drawing.ContentAlignment.TopLeft;
            this.checkCambiosEnLote.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F);
            this.checkCambiosEnLote.Location = new System.Drawing.Point(16, 251);
            this.checkCambiosEnLote.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.checkCambiosEnLote.Name = "checkCambiosEnLote";
            this.checkCambiosEnLote.Size = new System.Drawing.Size(231, 24);
            this.checkCambiosEnLote.TabIndex = 18;
            this.checkCambiosEnLote.Text = "Cambiar Permisos en Lote";
            this.checkCambiosEnLote.TextAlign = System.Drawing.ContentAlignment.BottomRight;
            this.checkCambiosEnLote.UseVisualStyleBackColor = true;
            this.checkCambiosEnLote.CheckedChanged += new System.EventHandler(this.checkCambiosEnLote_CheckedChanged);
            // 
            // FormUsuarios
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(43)))), ((int)(((byte)(77)))), ((int)(((byte)(129)))));
            this.ClientSize = new System.Drawing.Size(803, 750);
            this.Controls.Add(this.checkCambiosEnLote);
            this.Controls.Add(this.panelMarcarTodos);
            this.Controls.Add(this.btnGuardarPermisos);
            this.Controls.Add(this.grillaPermisos);
            this.Controls.Add(this.btnNuevoUsuario);
            this.Controls.Add(this.checkOlvidoClave);
            this.Controls.Add(this.groupCambiarDatos);
            this.Controls.Add(this.groupCambiarContra);
            this.Controls.Add(this.comboUsuario);
            this.Controls.Add(this.label1);
            this.ForeColor = System.Drawing.Color.Cornsilk;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.Name = "FormUsuarios";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Edición Usuario";
            this.Activated += new System.EventHandler(this.FormUsuarios_Activated);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormUsuarios_FormClosing);
            this.Load += new System.EventHandler(this.FormLoginVendedor_Load);
            this.groupCambiarContra.ResumeLayout(false);
            this.groupCambiarContra.PerformLayout();
            this.groupCambiarDatos.ResumeLayout(false);
            this.groupCambiarDatos.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.grillaPermisos)).EndInit();
            this.panelMarcarTodos.ResumeLayout(false);
            this.panelMarcarTodos.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtClave;
        private System.Windows.Forms.Button btnGuardarContra;
        private System.Windows.Forms.ComboBox comboUsuario;
        private System.Windows.Forms.TextBox txtNueva;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtRepetir;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.GroupBox groupCambiarContra;
        private System.Windows.Forms.GroupBox groupCambiarDatos;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Button btnGuardarDatos;
        private System.Windows.Forms.TextBox txtNombre;
        private System.Windows.Forms.CheckBox checkAdmin;
        private System.Windows.Forms.CheckBox checkOlvidoClave;
        private System.Windows.Forms.CheckBox checkActivo;
        private System.Windows.Forms.Button btnNuevoUsuario;
        private System.Windows.Forms.DataGridView grillaPermisos;
        private System.Windows.Forms.Button btnGuardarPermisos;
        private System.Windows.Forms.CheckBox checkMarcarVer;
        private System.Windows.Forms.CheckBox checkMarcarEditar;
        private System.Windows.Forms.Panel panelMarcarTodos;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox txtDiasVer;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox txtDiasEditar;
        private System.Windows.Forms.CheckBox checkCambiosEnLote;
        private System.Windows.Forms.ComboBox comboPropioTodos;
    }
}