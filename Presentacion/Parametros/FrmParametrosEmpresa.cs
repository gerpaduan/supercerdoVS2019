using System;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using Utilidades;

namespace Presentacion
{
    public class FrmParametrosEmpresa : Form
    {
        // Ajustá estos valores si tu tabla Parametros usa otros "tipo"
        // (El "tipo" viene de dbo.Parametros.tipo)
        private const int TIPO_DECIMAL = 1;
        private const int TIPO_BOOL = 2;
        private const int TIPO_INT = 3;
        private const int TIPO_LONG = 4;
        private const int TIPO_STRING = 0;

        private readonly IEmpresaContext _empresa;
        private readonly Negocio.Parametros oParametroN;

        private DataTable _dt;
        private BindingSource _bs;

        private DataGridView dgv;
        private Button btnGuardar;
        private Button btnCerrar;
        private Label lblInfo;

        public FrmParametrosEmpresa(IEmpresaContext empresa)
        {
            if (empresa == null) throw new ArgumentNullException("empresa");
            _empresa = empresa;
            oParametroN = new Negocio.Parametros(_empresa);

            InitializeComponent();
        }

        private void FrmParametrosEmpresa_Load(object sender, EventArgs e)
        {
            Cargar();
        }

        private void Cargar()
        {
            _dt = oParametroN.ObtenerGrid();

            // Columnas esperadas:
            // idParametro, nombre, descripcion, tipo, valor
            if (!_dt.Columns.Contains("idParametro")) throw new Exception("No existe columna idParametro en la consulta.");
            if (!_dt.Columns.Contains("nombre")) throw new Exception("No existe columna nombre en la consulta.");
            if (!_dt.Columns.Contains("descripcion")) throw new Exception("No existe columna descripcion en la consulta.");
            if (!_dt.Columns.Contains("tipo")) throw new Exception("No existe columna tipo en la consulta.");
            if (!_dt.Columns.Contains("valor")) throw new Exception("No existe columna valor en la consulta.");

            // Agrego columnas auxiliares para edición amigable
            if (!_dt.Columns.Contains("valorBool"))
                _dt.Columns.Add("valorBool", typeof(bool));

            if (!_dt.Columns.Contains("valorEdit"))
                _dt.Columns.Add("valorEdit", typeof(string));

            // Inicializar auxiliares desde "valor"
            foreach (DataRow r in _dt.Rows)
            {
                int tipo = ToInt(r["tipo"], TIPO_STRING);
                string val = r["valor"] == DBNull.Value ? "" : Convert.ToString(r["valor"]);

                if (tipo == TIPO_BOOL)
                {
                    r["valorBool"] = (val.Trim() == "1" || val.Trim().ToLower() == "true");
                    r["valorEdit"] = ""; // no se usa
                }
                else
                {
                    r["valorBool"] = false; // no se usa
                    r["valorEdit"] = val;
                }
            }

            _bs = new BindingSource();
            _bs.DataSource = _dt;

            dgv.AutoGenerateColumns = false;
            dgv.DataSource = _bs;
        }

        private void BtnGuardar_Click(object sender, EventArgs e)
        {
            dgv.EndEdit();
            _bs.EndEdit();

            // Validación + pasar auxiliares a "valor"
            foreach (DataRow r in _dt.Rows)
            {
                if (r.RowState == DataRowState.Deleted) continue;

                int tipo = ToInt(r["tipo"], TIPO_STRING);

                if (tipo == TIPO_BOOL)
                {
                    bool b = r["valorBool"] != DBNull.Value && Convert.ToBoolean(r["valorBool"]);
                    r["valor"] = b ? "1" : "0";
                }
                else
                {
                    string s = r["valorEdit"] == DBNull.Value ? "" : Convert.ToString(r["valorEdit"]);
                    s = (s ?? "").Trim();

                    // Validaciones por tipo
                    if (tipo == TIPO_INT)
                    {
                        int tmp;
                        if (!int.TryParse(s, out tmp))
                        {
                            MessageBox.Show("Valor inválido (int) para: " + Convert.ToString(r["nombre"]),
                                "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }
                    }
                    else if (tipo == TIPO_LONG)
                    {
                        long tmp;
                        if (!long.TryParse(s, out tmp))
                        {
                            MessageBox.Show("Valor inválido (long) para: " + Convert.ToString(r["nombre"]),
                                "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }
                    }
                    else if (tipo == TIPO_DECIMAL)
                    {
                        // acepto coma o punto
                        string norm = s.Replace(',', '.');
                        decimal tmp;
                        if (!decimal.TryParse(norm, NumberStyles.Any, CultureInfo.InvariantCulture, out tmp))
                        {
                            MessageBox.Show("Valor inválido (decimal) para: " + Convert.ToString(r["nombre"]),
                                "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }

                        // guardo en formato invariant (punto)
                        s = tmp.ToString(CultureInfo.InvariantCulture);
                    }

                    r["valor"] = s;
                }
            }

            try
            {
                // Guarda por empresa (MERGE en EmpresaParametros)
                oParametroN.GuardarGrid(_dt);

                // Refrescar cache de parámetros (si lo usás en runtime)
                try
                {
                    // si tu app lo tiene
                    if (FormPrincipal.ParametrosCTX != null)
                        FormPrincipal.ParametrosCTX.Reload();
                }
                catch { /* ignorar */ }

                MessageBox.Show("Parámetros guardados correctamente.", "OK",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al guardar parámetros:\n\n" + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnCerrar_Click(object sender, EventArgs e)
        {
            Close();
        }

        private int ToInt(object o, int def)
        {
            if (o == null || o == DBNull.Value) return def;
            int x;
            return int.TryParse(Convert.ToString(o), out x) ? x : def;
        }

        private void InitializeComponent()
        {
            this.dgv = new System.Windows.Forms.DataGridView();
            this.btnGuardar = new System.Windows.Forms.Button();
            this.btnCerrar = new System.Windows.Forms.Button();
            this.lblInfo = new System.Windows.Forms.Label();

            ((System.ComponentModel.ISupportInitialize)(this.dgv)).BeginInit();
            this.SuspendLayout();

            // lblInfo
            this.lblInfo.AutoSize = true;
            this.lblInfo.Location = new System.Drawing.Point(12, 9);
            this.lblInfo.Name = "lblInfo";
            this.lblInfo.Size = new System.Drawing.Size(640, 13);
            this.lblInfo.Text = "Configuración de parámetros por empresa. Tipos: decimal (número), int, long, bool (checkbox).";

            // dgv
            this.dgv.AllowUserToAddRows = false;
            this.dgv.AllowUserToDeleteRows = false;
            this.dgv.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top
                | System.Windows.Forms.AnchorStyles.Bottom)
                | System.Windows.Forms.AnchorStyles.Left)
                | System.Windows.Forms.AnchorStyles.Right)));
            this.dgv.Location = new System.Drawing.Point(12, 30);
            this.dgv.Name = "dgv";
            this.dgv.RowHeadersVisible = false;
            this.dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            this.dgv.Size = new System.Drawing.Size(860, 430);
            this.dgv.TabIndex = 0;

            // Columnas
            var colNombre = new DataGridViewTextBoxColumn();
            colNombre.Name = "colNombre";
            colNombre.HeaderText = "Parámetro";
            colNombre.DataPropertyName = "nombre";
            colNombre.ReadOnly = true;
            colNombre.Width = 200;

            var colDesc = new DataGridViewTextBoxColumn();
            colDesc.Name = "colDesc";
            colDesc.HeaderText = "Descripción";
            colDesc.DataPropertyName = "descripcion";
            colDesc.ReadOnly = true;
            colDesc.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;

            var colValorEdit = new DataGridViewTextBoxColumn();
            colValorEdit.Name = "colValorEdit";
            colValorEdit.HeaderText = "Valor";
            colValorEdit.DataPropertyName = "valorEdit";
            colValorEdit.Width = 160;

            var colBool = new DataGridViewCheckBoxColumn();
            colBool.Name = "colBool";
            colBool.HeaderText = "Bool";
            colBool.DataPropertyName = "valorBool";
            colBool.Width = 60;

            // ocultas
            var colTipo = new DataGridViewTextBoxColumn();
            colTipo.Name = "colTipo";
            colTipo.HeaderText = "tipo";
            colTipo.DataPropertyName = "tipo";
            colTipo.Visible = false;

            var colId = new DataGridViewTextBoxColumn();
            colId.Name = "colId";
            colId.HeaderText = "idParametro";
            colId.DataPropertyName = "idParametro";
            colId.Visible = false;

            this.dgv.Columns.AddRange(new DataGridViewColumn[]
            {
                colNombre, colDesc, colValorEdit, colBool, colTipo, colId
            });

            // Evento para mostrar solo columna valor o bool según tipo
            this.dgv.CellFormatting += (s, e) =>
            {
                if (_dt == null || e.RowIndex < 0) return;
                if (dgv.Columns[e.ColumnIndex].Name != "colValorEdit" &&
                    dgv.Columns[e.ColumnIndex].Name != "colBool") return;

                var row = ((DataRowView)dgv.Rows[e.RowIndex].DataBoundItem).Row;
                int tipo = ToInt(row["tipo"], TIPO_STRING);

                bool esBool = (tipo == TIPO_BOOL);

                // Hacemos "visual": si es bool, deshabilitamos edición del texto y viceversa
                if (dgv.Columns[e.ColumnIndex].Name == "colBool")
                {
                    dgv.Rows[e.RowIndex].Cells[e.ColumnIndex].ReadOnly = !esBool;
                    dgv.Rows[e.RowIndex].Cells[e.ColumnIndex].Style.BackColor = esBool ? Color.White : Color.LightGray;
                }
                if (dgv.Columns[e.ColumnIndex].Name == "colValorEdit")
                {
                    dgv.Rows[e.RowIndex].Cells[e.ColumnIndex].ReadOnly = esBool;
                    dgv.Rows[e.RowIndex].Cells[e.ColumnIndex].Style.BackColor = esBool ? Color.LightGray : Color.White;
                }
            };

            // btnGuardar
            this.btnGuardar.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnGuardar.Location = new System.Drawing.Point(676, 470);
            this.btnGuardar.Name = "btnGuardar";
            this.btnGuardar.Size = new System.Drawing.Size(95, 30);
            this.btnGuardar.Text = "Guardar";
            this.btnGuardar.UseVisualStyleBackColor = true;
            this.btnGuardar.Click += new System.EventHandler(this.BtnGuardar_Click);

            // btnCerrar
            this.btnCerrar.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCerrar.Location = new System.Drawing.Point(777, 470);
            this.btnCerrar.Name = "btnCerrar";
            this.btnCerrar.Size = new System.Drawing.Size(95, 30);
            this.btnCerrar.Text = "Cerrar";
            this.btnCerrar.UseVisualStyleBackColor = true;
            this.btnCerrar.Click += new System.EventHandler(this.BtnCerrar_Click);

            // Form
            this.ClientSize = new System.Drawing.Size(884, 512);
            this.Controls.Add(this.lblInfo);
            this.Controls.Add(this.dgv);
            this.Controls.Add(this.btnGuardar);
            this.Controls.Add(this.btnCerrar);
            this.Name = "FrmParametrosEmpresa";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Text = "Parámetros de la Empresa";
            this.Load += new System.EventHandler(this.FrmParametrosEmpresa_Load);

            ((System.ComponentModel.ISupportInitialize)(this.dgv)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}
