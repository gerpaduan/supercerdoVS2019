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
            this.groupCambiarContra.SuspendLayout();
            this.groupCambiarDatos.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.grillaPermisos)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(14, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(55, 16);
            this.label1.TabIndex = 0;
            this.label1.Text = "Usuario";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(68, 28);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(77, 16);
            this.label2.TabIndex = 1;
            this.label2.Text = "Contraseña";
            // 
            // txtClave
            // 
            this.txtClave.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtClave.Location = new System.Drawing.Point(151, 25);
            this.txtClave.Name = "txtClave";
            this.txtClave.PasswordChar = '*';
            this.txtClave.Size = new System.Drawing.Size(141, 22);
            this.txtClave.TabIndex = 2;
            // 
            // btnGuardarContra
            // 
            this.btnGuardarContra.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnGuardarContra.ForeColor = System.Drawing.SystemColors.ControlText;
            this.btnGuardarContra.Location = new System.Drawing.Point(151, 110);
            this.btnGuardarContra.Name = "btnGuardarContra";
            this.btnGuardarContra.Size = new System.Drawing.Size(141, 27);
            this.btnGuardarContra.TabIndex = 4;
            this.btnGuardarContra.Text = "Guardar &Contraseña";
            this.btnGuardarContra.UseVisualStyleBackColor = true;
            this.btnGuardarContra.Click += new System.EventHandler(this.btnGuardarContra_Click);
            // 
            // comboUsuario
            // 
            this.comboUsuario.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboUsuario.FormattingEnabled = true;
            this.comboUsuario.Location = new System.Drawing.Point(75, 12);
            this.comboUsuario.Name = "comboUsuario";
            this.comboUsuario.Size = new System.Drawing.Size(140, 21);
            this.comboUsuario.TabIndex = 0;
            this.comboUsuario.SelectedValueChanged += new System.EventHandler(this.comboUsuario_SelectedValueChanged);
            this.comboUsuario.TextChanged += new System.EventHandler(this.comboUsuario_SelectedValueChanged);
            // 
            // txtNueva
            // 
            this.txtNueva.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtNueva.Location = new System.Drawing.Point(151, 53);
            this.txtNueva.Name = "txtNueva";
            this.txtNueva.PasswordChar = '*';
            this.txtNueva.Size = new System.Drawing.Size(141, 22);
            this.txtNueva.TabIndex = 3;
            this.txtNueva.TextChanged += new System.EventHandler(this.txtNueva_TextChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(25, 56);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(118, 16);
            this.label3.TabIndex = 6;
            this.label3.Text = "Nueva contraseña";
            // 
            // txtRepetir
            // 
            this.txtRepetir.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtRepetir.Location = new System.Drawing.Point(151, 78);
            this.txtRepetir.Name = "txtRepetir";
            this.txtRepetir.PasswordChar = '*';
            this.txtRepetir.Size = new System.Drawing.Size(141, 22);
            this.txtRepetir.TabIndex = 4;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(20, 84);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(122, 16);
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
            this.groupCambiarContra.Location = new System.Drawing.Point(290, 50);
            this.groupCambiarContra.Name = "groupCambiarContra";
            this.groupCambiarContra.Size = new System.Drawing.Size(298, 143);
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
            this.groupCambiarDatos.Location = new System.Drawing.Point(12, 50);
            this.groupCambiarDatos.Name = "groupCambiarDatos";
            this.groupCambiarDatos.Size = new System.Drawing.Size(272, 143);
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
            this.checkActivo.Location = new System.Drawing.Point(63, 82);
            this.checkActivo.Name = "checkActivo";
            this.checkActivo.Size = new System.Drawing.Size(64, 20);
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
            this.checkAdmin.Location = new System.Drawing.Point(17, 56);
            this.checkAdmin.Name = "checkAdmin";
            this.checkAdmin.Size = new System.Drawing.Size(110, 20);
            this.checkAdmin.TabIndex = 6;
            this.checkAdmin.Text = "Administrador";
            this.checkAdmin.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.checkAdmin.UseVisualStyleBackColor = true;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.Location = new System.Drawing.Point(50, 28);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(57, 16);
            this.label5.TabIndex = 1;
            this.label5.Text = "Nombre";
            // 
            // btnGuardarDatos
            // 
            this.btnGuardarDatos.Enabled = false;
            this.btnGuardarDatos.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnGuardarDatos.ForeColor = System.Drawing.SystemColors.ControlText;
            this.btnGuardarDatos.Location = new System.Drawing.Point(113, 110);
            this.btnGuardarDatos.Name = "btnGuardarDatos";
            this.btnGuardarDatos.Size = new System.Drawing.Size(141, 27);
            this.btnGuardarDatos.TabIndex = 7;
            this.btnGuardarDatos.Text = "Guardar &Datos";
            this.btnGuardarDatos.UseVisualStyleBackColor = true;
            this.btnGuardarDatos.Click += new System.EventHandler(this.btnGuardarDatos_Click);
            // 
            // txtNombre
            // 
            this.txtNombre.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtNombre.Location = new System.Drawing.Point(113, 25);
            this.txtNombre.Name = "txtNombre";
            this.txtNombre.ReadOnly = true;
            this.txtNombre.Size = new System.Drawing.Size(141, 22);
            this.txtNombre.TabIndex = 5;
            this.txtNombre.TabStop = false;
            // 
            // checkOlvidoClave
            // 
            this.checkOlvidoClave.AutoSize = true;
            this.checkOlvidoClave.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.checkOlvidoClave.Enabled = false;
            this.checkOlvidoClave.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.checkOlvidoClave.Location = new System.Drawing.Point(456, 27);
            this.checkOlvidoClave.Name = "checkOlvidoClave";
            this.checkOlvidoClave.Size = new System.Drawing.Size(126, 17);
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
            this.btnNuevoUsuario.Location = new System.Drawing.Point(220, 11);
            this.btnNuevoUsuario.Name = "btnNuevoUsuario";
            this.btnNuevoUsuario.Size = new System.Drawing.Size(141, 22);
            this.btnNuevoUsuario.TabIndex = 9;
            this.btnNuevoUsuario.Text = "&Nuevo Usuario";
            this.btnNuevoUsuario.UseVisualStyleBackColor = true;
            this.btnNuevoUsuario.Click += new System.EventHandler(this.btnNuevoUsuario_Click);
            // 
            // grillaPermisos
            // 
            this.grillaPermisos.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.grillaPermisos.Location = new System.Drawing.Point(10, 200);
            this.grillaPermisos.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.grillaPermisos.Name = "grillaPermisos";
            this.grillaPermisos.RowHeadersWidth = 51;
            this.grillaPermisos.RowTemplate.Height = 24;
            this.grillaPermisos.Size = new System.Drawing.Size(578, 374);
            this.grillaPermisos.TabIndex = 12;
            this.grillaPermisos.CellValueChanged += new System.Windows.Forms.DataGridViewCellEventHandler(this.grillaPermisos_CellValueChanged);
            this.grillaPermisos.CurrentCellDirtyStateChanged += new System.EventHandler(this.grillaPermisos_CurrentCellDirtyStateChanged);
            this.grillaPermisos.EditingControlShowing += new System.Windows.Forms.DataGridViewEditingControlShowingEventHandler(this.grillaPermisos_EditingControlShowing);
            // 
            // btnGuardarPermisos
            // 
            this.btnGuardarPermisos.Enabled = false;
            this.btnGuardarPermisos.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnGuardarPermisos.ForeColor = System.Drawing.SystemColors.ControlText;
            this.btnGuardarPermisos.Location = new System.Drawing.Point(441, 579);
            this.btnGuardarPermisos.Name = "btnGuardarPermisos";
            this.btnGuardarPermisos.Size = new System.Drawing.Size(141, 27);
            this.btnGuardarPermisos.TabIndex = 9;
            this.btnGuardarPermisos.Text = "Guardar &Permisos";
            this.btnGuardarPermisos.UseVisualStyleBackColor = true;
            this.btnGuardarPermisos.Click += new System.EventHandler(this.btnGuardarPermisos_Click);
            // 
            // FormUsuarios
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(43)))), ((int)(((byte)(77)))), ((int)(((byte)(129)))));
            this.ClientSize = new System.Drawing.Size(602, 609);
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
            this.Name = "FormUsuarios";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Edición Usuario";
            this.Activated += new System.EventHandler(this.FormUsuarios_Activated);
            this.Load += new System.EventHandler(this.FormLoginVendedor_Load);
            this.groupCambiarContra.ResumeLayout(false);
            this.groupCambiarContra.PerformLayout();
            this.groupCambiarDatos.ResumeLayout(false);
            this.groupCambiarDatos.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.grillaPermisos)).EndInit();
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
    }
}