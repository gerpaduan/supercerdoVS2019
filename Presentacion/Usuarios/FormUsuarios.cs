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
        public FormUsuarios()
        {
            InitializeComponent(); this.Icon = Properties.Resources.CarniSys_ICONO;
        }

        private void FormLoginVendedor_Load(object sender, EventArgs e)
        {
            this.Text += Utilidades.Conexion.getSucursalConexion();
            cargarCombo();
            validarLogueoAdmin();

            //crear grilla permisos
            //GrillaPermisosLoad();
            ConfigurarGrillaPermisos();
            CargarPermisosEnGrilla();

            foreach (DataGridViewRow row in grillaPermisos.Rows)
            {
                bool verMarcado = Convert.ToBoolean(row.Cells["Ver"].Value);
                bool editarMarcado = Convert.ToBoolean(row.Cells["Editar"].Value);

                row.Cells["HastaDiasAtras"].ReadOnly = !verMarcado;
                row.Cells["HastaDiasAtras2"].ReadOnly = !editarMarcado;
                row.Cells["PermisoEdicion"].ReadOnly = !editarMarcado;
            }
        }
        private void CargarPermisosEnGrilla()
        {
            grillaPermisos.Rows.Clear();

            foreach (var permiso in Permisos)
            {
                bool verMarcado = permiso.DiasPermitidosVer != -1;
                bool editarMarcado = permiso.DiasPermitidosEditar != -1;

                grillaPermisos.Rows.Add(
                    permiso.IdForm,
                    permiso.Formulario.NombreForm,
                    verMarcado,
                    editarMarcado,
                    verMarcado ? permiso.DiasPermitidosVer : -1,
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

            var row = grillaPermisos.Rows[e.RowIndex];

            // Si cambia el CheckBox de Ver
            if (grillaPermisos.Columns[e.ColumnIndex].Name == "Ver")
            {
                bool verMarcado = Convert.ToBoolean(row.Cells["Ver"].Value);
                row.Cells["HastaDiasAtras"].ReadOnly = !verMarcado;
                row.Cells["HastaDiasAtras"].Value = verMarcado ? 0 : -1;
            }

            if (grillaPermisos.Columns[e.ColumnIndex].Name == "Editar")
            {
                bool editarMarcado = Convert.ToBoolean(row.Cells["Editar"].Value);
                row.Cells["HastaDiasAtras2"].ReadOnly = !editarMarcado;
                row.Cells["HastaDiasAtras2"].Value = editarMarcado ? 0 : -1;

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
                        NombreForm = row.Cells["Formulario"].Value.ToString()
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
                HeaderText = "Hasta Días Atrás",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.ColumnHeader
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
                HeaderText = "Hasta Días Atrás 2",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.ColumnHeader
            };
            grillaPermisos.Columns.Add(colHastaDiasAtras2);

            // Columna PermisoEdicion como ComboBox
            var colPermisoEdicion = new DataGridViewComboBoxColumn
            {
                Name = "PermisoEdicion",
                HeaderText = "Permiso Edición",
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
            grillaPermisos.Columns["HastaDiasAtras"].AutoSizeMode = DataGridViewAutoSizeColumnMode.ColumnHeader;
            grillaPermisos.Columns["HastaDiasAtras2"].AutoSizeMode = DataGridViewAutoSizeColumnMode.ColumnHeader;
            grillaPermisos.Columns["PermisoEdicion"].AutoSizeMode = DataGridViewAutoSizeColumnMode.ColumnHeader;


            grillaPermisos.DefaultCellStyle.Font = new Font("Segoe UI", 12F, FontStyle.Regular);
            //grillaPermisos.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
        }

        private void GrillaPermisosLoad()
        {
            // Configuración básica
            grillaPermisos.AutoGenerateColumns = false;
            grillaPermisos.AllowUserToAddRows = false;

            // 1. Columna de texto (Formulario)
            DataGridViewTextBoxColumn colFormulario = new DataGridViewTextBoxColumn();
            colFormulario.HeaderText = "Formulario";
            colFormulario.Name = "Formulario";
            colFormulario.ReadOnly = true;
            grillaPermisos.Columns.Add(colFormulario);

            // 2. Columna checkbox (Ver)
            DataGridViewCheckBoxColumn colVer = new DataGridViewCheckBoxColumn();
            colVer.HeaderText = "Ver";
            colVer.Name = "Ver";
            grillaPermisos.Columns.Add(colVer);

            // 3. Columna numérica (Hasta Días Atrás)
            DataGridViewTextBoxColumn colHastaDiasAtras = new DataGridViewTextBoxColumn();
            colHastaDiasAtras.HeaderText = "Hasta(Días Atrás)";
            colHastaDiasAtras.Name = "HastaDiasAtras";
            grillaPermisos.Columns.Add(colHastaDiasAtras);

            // 4. Columna checkbox (Editar)
            DataGridViewCheckBoxColumn colEditar = new DataGridViewCheckBoxColumn();
            colEditar.HeaderText = "Editar";
            colEditar.Name = "Editar";
            grillaPermisos.Columns.Add(colEditar);

            // 5. Columna numérica (Hasta Días Atrás 2)
            DataGridViewTextBoxColumn colHastaDiasAtras2 = new DataGridViewTextBoxColumn();
            colHastaDiasAtras2.HeaderText = "Hasta(Días Atrás)";
            colHastaDiasAtras2.Name = "HastaDiasAtras2";
            grillaPermisos.Columns.Add(colHastaDiasAtras2);

            // 6. Columna ComboBox (Permiso Edición)
            DataGridViewComboBoxColumn colPermisoEdicion = new DataGridViewComboBoxColumn();
            colPermisoEdicion.HeaderText = "Permiso Edición";
            colPermisoEdicion.Name = "PermisoEdicion";
            colPermisoEdicion.Items.Add("Todos");
            colPermisoEdicion.Items.Add("Propios");
            grillaPermisos.Columns.Add(colPermisoEdicion);

            // Ajustar ancho al encabezado
            grillaPermisos.RowHeadersVisible = false;
            grillaPermisos.Columns["Formulario"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            grillaPermisos.Columns["Ver"].AutoSizeMode = DataGridViewAutoSizeColumnMode.ColumnHeader;
            grillaPermisos.Columns["Editar"].AutoSizeMode = DataGridViewAutoSizeColumnMode.ColumnHeader;
            grillaPermisos.Columns["HastaDiasAtras"].AutoSizeMode = DataGridViewAutoSizeColumnMode.ColumnHeader;
            grillaPermisos.Columns["HastaDiasAtras2"].AutoSizeMode = DataGridViewAutoSizeColumnMode.ColumnHeader;
            grillaPermisos.Columns["PermisoEdicion"].AutoSizeMode = DataGridViewAutoSizeColumnMode.ColumnHeader;

            grillaPermisos.DefaultCellStyle.ForeColor = Color.Black;

            // Filas de ejemplo
            grillaPermisos.Rows.Add("Ventas", true, 30, true, 15, "Todos");
            grillaPermisos.Rows.Add("Compras", false, 10, true, 5, "Propios");
            grillaPermisos.Rows.Add("Inventario", true, 60, false, 0, "Todos");
            grillaPermisos.Rows.Add("Usuarios", false, 0, false, 0, "Propios");
        }
        private void grillaPermisos_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
        {
            // Validar columnas 2 y 4 (numéricas)
            if (grillaPermisos.CurrentCell.ColumnIndex == 2 || grillaPermisos.CurrentCell.ColumnIndex == 4)
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
            txtNombre.Enabled = FormPrincipal.logueado;
            checkAdmin.Enabled = FormPrincipal.logueado;
            checkActivo.Enabled = FormPrincipal.logueado;
            //btnNuevoUsuario.Enabled = FormPrincipal.logueado;
            btnGuardarDatos.Enabled = FormPrincipal.logueado;
            btnGuardarPermisos.Enabled = FormPrincipal.logueado;
            checkOlvidoClave.Enabled = FormPrincipal.logueado;
        }

        private void cargarCombo()
        {
            comboUsuario.DataSource = oUsuarioN.obtenerUsuarios(false);
            comboUsuario.DisplayMember = "usuario";
            comboUsuario.ValueMember = "usuario";
        }

        private void comboUsuario_SelectedValueChanged(object sender, EventArgs e)
        {
            oUsuarioE = comboUsuario.SelectedValue != null ? oUsuarioN.getUser(comboUsuario.SelectedValue.ToString()) : null;
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
            
        }

        private void btnGuardarDatos_Click(object sender, EventArgs e)
        {
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
            if (!FormPrincipal.logueado)
            {
                MessageBox.Show("Debe iniciar sesion con un usuario administrador para crear nuevos usuarios.", "Inicie sesión");
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
            grillaPermisos.EndEdit(); // Asegura que los cambios en celdas se confirmen
            var permisos = ReconstruirListaDesdeGrilla();
            oUsuarioN.AddOrEditPermisos(permisos);
            MessageBox.Show("Permisos guardados correctamente.");
        }
    }
}
