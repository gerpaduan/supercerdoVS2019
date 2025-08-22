using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Presentacion.Usuario
{
    public partial class FormUsuarios : Form
    {
        Negocio.Usuario oUsuarioN = new Negocio.Usuario();
        Entidades.Usuario oUsuarioE = new Entidades.Usuario();

        private List<Entidades.PermisosUsuarios> Permisos = new List<Entidades.PermisosUsuarios>();
        private bool grillaModificada = false;
        string ultimoUsuario;
        bool guardarPermisos = false;
        bool permisoEdicion = false;
        bool usuarioCargado = false;

        public FormUsuarios()
        {
            InitializeComponent(); this.Icon = Properties.Resources.CarniSys_ICONO;
        }

        private void FormLoginVendedor_Load(object sender, EventArgs e)
        {
            this.Text += Utilidades.Conexion.getSucursalConexion();
            cargarCombo();
            validarLogueoAdmin();

            CargarGrilla();
            BotonPermisos();
        }

        private void BotonPermisos()
        {
            ActualizarPermisosUsuario();
            btnGuardarPermisos.Text = guardarPermisos ? "Guardar &Permisos" : "Editar &Permisos";
            checkCambiosEnLote.Enabled = guardarPermisos;
            //checkCambiosEnLote.Checked = checkCambiosEnLote.Checked && !guardarPermisos ? false : checkCambiosEnLote.Checked;

            if (grillaPermisos.Columns.Count > 0)
            {
                grillaPermisos.Columns["Ver"].ReadOnly = !guardarPermisos;
                grillaPermisos.Columns["Editar"].ReadOnly = !guardarPermisos;
                grillaPermisos.Columns["HastaDiasAtras"].ReadOnly = !guardarPermisos;
                grillaPermisos.Columns["HastaDiasAtras2"].ReadOnly = !guardarPermisos;
                grillaPermisos.Columns["PermisoEdicion"].ReadOnly = !guardarPermisos;
            }
        }

        private void ActualizarPermisosUsuario()
        {
            permisoEdicion = FormPrincipal.oUserLogueado != null &&
                            oUsuarioN.tienePermiso(FormPrincipal.oUserLogueado, "FormNuevoUsuario", DateTime.Today, FormPrincipal.oUserLogueado.Id);
        }

        private void CargarGrilla()
        {
            ConfigurarGrillaPermisos();
            CargarPermisosEnGrilla();
            BotonPermisos();

            foreach (DataGridViewRow row in grillaPermisos.Rows)
            {
                bool verMarcado = Convert.ToBoolean(row.Cells["Ver"].Value);
                bool editarMarcado = Convert.ToBoolean(row.Cells["Editar"].Value);

                row.Cells["HastaDiasAtras"].ReadOnly = !guardarPermisos || (!verMarcado && !guardarPermisos);
                row.Cells["HastaDiasAtras2"].ReadOnly = !guardarPermisos || (!verMarcado && !guardarPermisos);
                row.Cells["PermisoEdicion"].ReadOnly = !guardarPermisos || (!verMarcado && !guardarPermisos);
            }
            //se reestablece la variable
            grillaModificada = false;
        }

        private void CargarPermisosEnGrilla()
        {
            grillaPermisos.Rows.Clear();

            foreach (var permiso in Permisos.OrderBy(p => p.Formulario.NombreForm))
            {
                bool verMarcado = permiso.DiasPermitidosVer != -1;
                bool editarMarcado = permiso.DiasPermitidosEditar != -1;

                grillaPermisos.Rows.Add(
                    permiso.IdForm,
                    permiso.Formulario.NombreForm,
                    permiso.Formulario.Descripcion,
                    verMarcado,
                    verMarcado ? permiso.DiasPermitidosVer : -1,
                    editarMarcado,
                    editarMarcado ? permiso.DiasPermitidosEditar : -1,
                    permiso.SoloRegistrosPropios // true -> Propios, false -> Todos
                );
            }
        }
        private void grillaPermisos_CurrentCellDirtyStateChanged(object sender, EventArgs e)
        {
            if (grillaPermisos.IsCurrentCellDirty)
            {
                grillaPermisos.CommitEdit(DataGridViewDataErrorContexts.Commit);
            }
        }

        private void grillaPermisos_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;


            if (e.RowIndex >= 0) grillaModificada = true;

            var row = grillaPermisos.Rows[e.RowIndex];


            var valor = row.Cells["Formulario"].Value?.ToString();

            int valorDiasAtras = valor.Equals("Cheques") ? 30 : 0;

            // Si cambia el CheckBox de Ver
            if (grillaPermisos.Columns[e.ColumnIndex].Name == "Ver")
            {
                bool verMarcado = Convert.ToBoolean(row.Cells["Ver"].Value);
                row.Cells["HastaDiasAtras"].ReadOnly = !verMarcado;
                //si es cheque se establece 30 dias por defecto para abarcar la fecha de vencimiento
                row.Cells["HastaDiasAtras"].Value = verMarcado ? valorDiasAtras : -1;
            }

            if (grillaPermisos.Columns[e.ColumnIndex].Name == "Editar")
            {
                bool editarMarcado = Convert.ToBoolean(row.Cells["Editar"].Value);
                row.Cells["HastaDiasAtras2"].ReadOnly = !editarMarcado;
                row.Cells["HastaDiasAtras2"].Value = editarMarcado ? valorDiasAtras : -1;

                // Habilitar o deshabilitar el ComboBox de PermisoEdicion
                row.Cells["PermisoEdicion"].ReadOnly = !editarMarcado;
                if (!editarMarcado)
                    row.Cells["PermisoEdicion"].Value = true; // por defecto "Propios"
            }
        }

        // ---------- RECONSTRUIR LISTA DESDE GRILLA ----------
        private List<Entidades.PermisosUsuarios> ReconstruirListaDesdeGrilla()
        {
            var permisos = new List<Entidades.PermisosUsuarios>();

            foreach (DataGridViewRow row in grillaPermisos.Rows)
            {
                if (row.IsNewRow) continue;

                // Obtener valores principales
                int idForm = Convert.ToInt32(row.Cells["IdForm"].Value);
                bool verMarcado = Convert.ToBoolean(row.Cells["Ver"].Value);
                bool editarMarcado = Convert.ToBoolean(row.Cells["Editar"].Value);

                int diasPermitidosVer = verMarcado
                    ? Convert.ToInt32(row.Cells["HastaDiasAtras"].Value)
                    : -1;

                int diasPermitidosEditar = editarMarcado
                    ? Convert.ToInt32(row.Cells["HastaDiasAtras2"].Value)
                    : -1;

                bool soloRegistrosPropios = editarMarcado
                    ? (row.Cells["PermisoEdicion"].Value == null
                        ? true // por defecto "Propios"
                        : Convert.ToBoolean(row.Cells["PermisoEdicion"].Value))
                    : true; // Si no edita, forzar "Propios"

                var permiso = new Entidades.PermisosUsuarios
                {
                    IdUsuario = oUsuarioE.Id,
                    IdForm = idForm,
                    DiasPermitidosVer = diasPermitidosVer,
                    DiasPermitidosEditar = diasPermitidosEditar,
                    SoloRegistrosPropios = soloRegistrosPropios,
                    Formulario = new Entidades.Formulario
                    {
                        IdForm = idForm,
                        NombreForm = row.Cells["Formulario"].Value.ToString(),
                        Descripcion = row.Cells["Descripcion"].Value.ToString()
                    }
                };

                permisos.Add(permiso);
            }

            return permisos;
        }

        private void ConfigurarGrillaPermisos()
        {
            grillaPermisos.Columns.Clear();
            grillaPermisos.AutoGenerateColumns = false;
            grillaPermisos.AllowUserToAddRows = false;

            grillaPermisos.DefaultCellStyle.ForeColor = Color.Black;
            grillaPermisos.RowTemplate.Height = 28; // Cambiá 30 por el valor que necesites
            grillaPermisos.AllowUserToResizeRows = false;
            // Columna oculta IdForm
            var colIdForm = new DataGridViewTextBoxColumn
            {
                Name = "IdForm",
                HeaderText = "IdForm",
                Visible = false
            };
            grillaPermisos.Columns.Add(colIdForm);

            // Formulario
            var colFormulario = new DataGridViewTextBoxColumn
            {
                Name = "Formulario",
                HeaderText = "Formulario",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                ReadOnly = true
            };
            grillaPermisos.Columns.Add(colFormulario);

            // Formulario
            var colDesc = new DataGridViewTextBoxColumn
            {
                Name = "Descripcion",
                HeaderText = "Detalle",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.ColumnHeader,
                ReadOnly = true
            };
            grillaPermisos.Columns.Add(colDesc);

            // Ver (CheckBox)
            var colVer = new DataGridViewCheckBoxColumn
            {
                Name = "Ver",
                HeaderText = "Ver",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.ColumnHeader
            };
            grillaPermisos.Columns.Add(colVer);


            // HastaDiasAtras
            var colHastaDiasAtras = new DataGridViewTextBoxColumn
            {
                Name = "HastaDiasAtras",
                HeaderText = "Días Atrás",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.None,
                Width = 65
            };
            grillaPermisos.Columns.Add(colHastaDiasAtras);

            // Editar (CheckBox)
            var colEditar = new DataGridViewCheckBoxColumn
            {
                Name = "Editar",
                HeaderText = "Editar",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.ColumnHeader
            };
            grillaPermisos.Columns.Add(colEditar);


            // HastaDiasAtras2
            var colHastaDiasAtras2 = new DataGridViewTextBoxColumn
            {
                Name = "HastaDiasAtras2",
                HeaderText = "Días Atrás",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.None,
                Width = 65
            };
            grillaPermisos.Columns.Add(colHastaDiasAtras2);

            // Columna PermisoEdicion como ComboBox
            var colPermisoEdicion = new DataGridViewComboBoxColumn
            {
                Name = "PermisoEdicion",
                HeaderText = "Alcance",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.ColumnHeader,
                DataSource = new[]
                {
                    new { Texto = "Propios", Valor = true },
                    new { Texto = "Todos", Valor = false }
                },
                DisplayMember = "Texto",
                ValueMember = "Valor"
            };
            grillaPermisos.Columns.Add(colPermisoEdicion);


            // Ajustar ancho al encabezado
            grillaPermisos.RowHeadersVisible = false;
            grillaPermisos.Columns["Formulario"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            grillaPermisos.Columns["Ver"].AutoSizeMode = DataGridViewAutoSizeColumnMode.ColumnHeader;
            grillaPermisos.Columns["Editar"].AutoSizeMode = DataGridViewAutoSizeColumnMode.ColumnHeader;
            //grillaPermisos.Columns["HastaDiasAtras"].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            //grillaPermisos.Columns["HastaDiasAtras2"].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            grillaPermisos.Columns["PermisoEdicion"].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;


            grillaPermisos.DefaultCellStyle.Font = new Font("Segoe UI", 11F, FontStyle.Regular);
            grillaPermisos.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
        }

        private void grillaPermisos_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
        {
            // Validar columnas 2 y 4 (numéricas)
            //if (grillaPermisos.CurrentCell.ColumnIndex == 2 || grillaPermisos.CurrentCell.ColumnIndex == 4)
            if (grillaPermisos.CurrentCell.ColumnIndex == 3 || grillaPermisos.CurrentCell.ColumnIndex == 5)
            {
                TextBox tb = e.Control as TextBox;
                if (tb != null)
                {
                    tb.KeyPress -= Numeric_KeyPress;
                    tb.KeyPress += Numeric_KeyPress;
                }
            }
        }

        private void Numeric_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private void validarLogueoAdmin()
        {
            ActualizarPermisosUsuario();
            
            txtNombre.Enabled = permisoEdicion;
            txtNombre.ReadOnly = !permisoEdicion;
            checkAdmin.Enabled = permisoEdicion;
            checkActivo.Enabled = permisoEdicion;
            //btnNuevoUsuario.Enabled = FormPrincipal.logueado;
            //btnGuardarDatos.Enabled = FormPrincipal.logueado;
            //btnGuardarPermisos.Enabled = FormPrincipal.logueado;
            checkOlvidoClave.Enabled = permisoEdicion;
        }

        private void cargarCombo()
        {
            comboUsuario.DataSource = oUsuarioN.obtenerUsuarios(false);
            comboUsuario.DisplayMember = "usuario";
            comboUsuario.ValueMember = "usuario";
        }

        private void comboUsuario_SelectedValueChanged(object sender, EventArgs e)
        {
            usuarioCargado = false;

            string usuarioSelected  = ((DataRowView)comboUsuario.SelectedItem)["usuario"].ToString();
            if (grillaModificada && ValidarCambiosSinGuardar() == DialogResult.No)
            {
                if (ultimoUsuario != null && comboUsuario.SelectedValue != null)
                {
                    comboUsuario.SelectedItem = ultimoUsuario;
                    return;
                }
            }

            oUsuarioE = !string.IsNullOrWhiteSpace(usuarioSelected)
                    ? oUsuarioN.getUser(usuarioSelected)
                    : null;

            if (oUsuarioE != null)
            {
                txtNombre.Text = oUsuarioE.Nombre;
                checkAdmin.Checked = oUsuarioE.Admin;
                checkActivo.Checked = oUsuarioE.Activo;
                txtClave.Text = txtNueva.Text = txtRepetir.Text = "";

                //Cargo los permisos
                Permisos = oUsuarioN.getPermisosUsuario(oUsuarioE.Id);
                oUsuarioE.Permisos = Permisos;
            }
            else
            {
                txtNombre.Text = "";
                checkAdmin.Checked = false;
                checkActivo.Checked = false;
            }
            checkOlvidoClave.Checked = false;

            CargarGrilla();

            ultimoUsuario = usuarioSelected;
            usuarioCargado = true;
        }

        private void btnGuardarDatos_Click(object sender, EventArgs e)
        {
            ActualizarPermisosUsuario();
            if (!permisoEdicion)
            {
                Utilidades.Mensajes.ErrorPermisoEdicion();
                return;
            }

            if (oUsuarioE != null)
            {
                oUsuarioE.Nombre = txtNombre.Text;
                oUsuarioE.Admin = checkAdmin.Checked;
                oUsuarioE.Activo = checkActivo.Checked;

                //el usuario Admin es reservado para el desarrollador del sistema
                if (oUsuarioE.User.Equals("admin"))
                {
                    MessageBox.Show("El usuario Admin es reservado para el desarrollador del sistema", "Reservado", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                addOrEditUser();
                cargarCombo();
            }
            else
            {
                MessageBox.Show("No seleccionó ningún usuario.", "El usuario no existe", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void addOrEditUser()
        {
            try
            {
                oUsuarioN.addOrEditUser(oUsuarioE);
                MessageBox.Show("Los datos se guardaron correctamente!", "Datos guardados");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error al guardar usuario", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnGuardarContra_Click(object sender, EventArgs e)
        {
            string mensaje = "Errores:\n\n";
            bool errores = false;
            oUsuarioE = checkOlvidoClave.Checked ? oUsuarioE : oUsuarioN.validarUsuario(comboUsuario.Text, txtClave.Text, false);            
            if (oUsuarioE == null)
            {
                errores = true;
                mensaje += "-Contraseña incorrecta\n";
            } 
            if (!txtNueva.Text.Equals(txtRepetir.Text))
            {
                errores = true;
                mensaje += "-La nueva contraseña no coinciden.\n";
            }
            if (txtNueva.Text.Equals("") || txtRepetir.Text.Equals(""))
            {
                errores = true;
                mensaje += "-La nueva contraseña no puede ser vacia.\n";
            }
            if (txtNueva.Text.Contains(" ") || txtRepetir.Text.Contains(" "))
            {
                errores = true;
                mensaje += "-La nueva contraseña no puede contener espacios en blanco.\n";
            }

            if (!errores)
            {
                oUsuarioE.Clave = txtNueva.Text;
                addOrEditUser();

                //limpio campos
                txtClave.Text = txtNueva.Text = txtRepetir.Text = "";
                checkOlvidoClave.Checked = false;
            }
            else
            {
                MessageBox.Show(mensaje, "Cambiar contraseña", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void checkOlvidoClave_CheckedChanged(object sender, EventArgs e)
        {
            txtClave.Text = checkOlvidoClave.Checked && oUsuarioE != null ? oUsuarioE.Clave : "";
            txtClave.ReadOnly = checkOlvidoClave.Checked;
            txtRepetir.ReadOnly = checkOlvidoClave.Checked;
        }

        private void txtNueva_TextChanged(object sender, EventArgs e)
        {
            if (checkOlvidoClave.Checked)
            {
                txtRepetir.Text = txtNueva.Text;
            }
        }

        private void btnNuevoUsuario_Click(object sender, EventArgs e)
        {
            ActualizarPermisosUsuario();
            if (!permisoEdicion)
            {
                Utilidades.Mensajes.ErrorPermisoEdicion();
                return;
            }

            FormNuevoUsuario formNuevoUsuario1 = new FormNuevoUsuario();
            formNuevoUsuario1.ShowDialog();
            //cargarCombo();
        }

        private void FormUsuarios_Activated(object sender, EventArgs e)
        {
            validarLogueoAdmin();
        }

        private void btnGuardarPermisos_Click(object sender, EventArgs e)
        {
            ActualizarPermisosUsuario();
            if (!permisoEdicion)
            {
                Utilidades.Mensajes.ErrorPermisoEdicion();
                return;
            }

            if (guardarPermisos)
            {
                grillaPermisos.EndEdit(); // Asegura que los cambios en celdas se confirmen
                var permisos = ReconstruirListaDesdeGrilla();
                oUsuarioN.AddOrEditPermisos(permisos);
                MessageBox.Show("Permisos guardados correctamente.");
                grillaModificada = false;
            }
            else
            {

            }

            guardarPermisos = !guardarPermisos;
            BotonPermisos();
        }

        private void checkMarcarEditar_CheckedChanged(object sender, EventArgs e)
        {
            bool marcar = checkMarcarEditar.Checked;

            foreach (DataGridViewRow row in grillaPermisos.Rows)
            {
                row.Cells["Editar"].Value = marcar;

                // Ajustar días y habilitar/deshabilitar combo de PermisoEdicion
                if (!marcar)
                {
                    row.Cells["HastaDiasAtras2"].Value = -1;
                    row.Cells["HastaDiasAtras2"].ReadOnly = true;

                    row.Cells["PermisoEdicion"].Value = true; // por defecto Propios
                    row.Cells["PermisoEdicion"].ReadOnly = true;
                }
                else
                {
                    row.Cells["HastaDiasAtras2"].Value = 0;
                    row.Cells["HastaDiasAtras2"].ReadOnly = false;

                    row.Cells["PermisoEdicion"].ReadOnly = false;
                }
            }
        }

        private void checkMarcarVer_CheckedChanged(object sender, EventArgs e)
        {
            bool marcar = checkMarcarVer.Checked;

            foreach (DataGridViewRow row in grillaPermisos.Rows)
            {
                row.Cells["Ver"].Value = marcar;

                // Si se desmarca, fijar el valor -1 y deshabilitar campo
                if (!marcar)
                {
                    row.Cells["HastaDiasAtras"].Value = -1;
                    row.Cells["HastaDiasAtras"].ReadOnly = true;
                }
                else
                {
                    row.Cells["HastaDiasAtras"].Value = 0;
                    row.Cells["HastaDiasAtras"].ReadOnly = false;
                }
            }
        }

        private void checkCambiosEnLote_CheckedChanged(object sender, EventArgs e)
        {
            panelMarcarTodos.Enabled = checkCambiosEnLote.Checked;
        }

        private void txtDiasVer_TextChanged(object sender, EventArgs e)
        {
            if (!int.TryParse(txtDiasVer.Text, out int diasVer)) diasVer = 0;

            foreach (DataGridViewRow row in grillaPermisos.Rows)
            {
                if (Convert.ToBoolean(row.Cells["Ver"].Value))
                {
                    row.Cells["HastaDiasAtras"].Value = diasVer;
                }
            }
        }

        private void txtDiasEditar_TextChanged(object sender, EventArgs e)
        {
            if (!int.TryParse(txtDiasEditar.Text, out int diasEditar)) diasEditar = 0;

            foreach (DataGridViewRow row in grillaPermisos.Rows)
            {
                if (Convert.ToBoolean(row.Cells["Editar"].Value))
                {
                    row.Cells["HastaDiasAtras2"].Value = diasEditar;
                }
            }
        }

        private void FormUsuarios_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (grillaModificada && ValidarCambiosSinGuardar() == DialogResult.No)
            {
                e.Cancel = true; // Cancela el cierre del formulario
            }
        }

        private DialogResult ValidarCambiosSinGuardar()
        {
            DialogResult resp = DialogResult.No;
            if (grillaModificada)
            {
                resp = MessageBox.Show(
                    "Hay cambios sin guardar en Permisos. ¿Desea salir de todos modos?",
                    "Confirmar salida",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning, 
                    MessageBoxDefaultButton.Button2
                );
            }
            return resp;
        }

        private void grillaPermisos_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            // Evitamos que se ejecute en encabezados
            if (e.RowIndex >= 0 && e.ColumnIndex >= 0)
            {
                // Verificamos si la columna es "Descripcion"
                if (grillaPermisos.Columns[e.ColumnIndex].Name == "Descripcion")
                {
                    string form = grillaPermisos["Formulario", e.RowIndex].Value?.ToString();
                    string valorCelda = grillaPermisos[e.ColumnIndex, e.RowIndex].Value?.ToString();

                    if (!string.IsNullOrEmpty(valorCelda))
                    {
                        Form detalleForm = new Form();
                        detalleForm.Text = form;
                        detalleForm.Size = new Size(400, 250);
                        detalleForm.StartPosition = FormStartPosition.CenterParent;
                        detalleForm.FormBorderStyle = FormBorderStyle.FixedDialog;
                        detalleForm.MaximizeBox = false;
                        detalleForm.MinimizeBox = false;
                        detalleForm.ShowIcon = false;
                        detalleForm.ShowInTaskbar = false;

                        TextBox txtDetalle = new TextBox();
                        txtDetalle.Multiline = true;
                        txtDetalle.ReadOnly = true;
                        txtDetalle.Dock = DockStyle.Fill;
                        txtDetalle.ScrollBars = ScrollBars.Vertical;
                        txtDetalle.BorderStyle = BorderStyle.None;
                        txtDetalle.BackColor = Color.White;
                        txtDetalle.Font = new Font("Segoe UI", 13F, FontStyle.Regular, GraphicsUnit.Point);
                        txtDetalle.Text = valorCelda;

                        // Evitar que quede seleccionado
                        txtDetalle.SelectionStart = 0;
                        txtDetalle.SelectionLength = 0;

                        detalleForm.Controls.Add(txtDetalle);
                        detalleForm.ShowDialog();
                    }
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            MessageBox.Show(
                "Seleccione la opción 'Ver' y/o 'Editar' para otorgar permisos al usuario.\n" +
                "En 'Días Atrás', indique hasta cuántos días previos a la fecha actual se aplicará el permiso.\n" +
                "Ejemplo: si ingresa 0, el usuario tendrá permiso solo para hoy; si ingresa 1, el usuario tendrá permiso solo para hoy y ayer; si ingresa 30, para los últimos 30 días.\n\n" +
                "En el campo 'Alcance', seleccione:\n" +
                "• 'Propios': solo permite editar los registros creados por el mismo usuario.\n" +
                "• 'Todos': permite editar cualquier registro, sin importar quién lo creó.\n\n" +
                "El alcance y los días aplican únicamente a las acciones habilitadas (Ver y/o Editar).",
                "Información",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );
        }

        private void checkAdmin_CheckedChanged(object sender, EventArgs e)
        {
            if (!usuarioCargado)
                return;

            if (checkAdmin.Checked)
            {
                MessageBox.Show(
                    "Si selecciona la opción 'Administrador', el usuario tendrá permisos ilimitados en todo el sistema, sin importar la lista de permisos configurada.\n" +
                    "Use esta opción solo para usuarios de máxima confianza.",
                    "Advertencia",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
            }
            else
            {
                MessageBox.Show(
                    "Al quitar la opción 'Administrador', deberá asignar manualmente los permisos de este usuario desde la lista correspondiente.",
                    "Información",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
            }
        }

        private void btnPermisosBasicos_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("¿Cargar Permisos Básicos? ", "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {

            }
        }
    }
}
