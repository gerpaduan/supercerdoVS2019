using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Presentacion.Caja;
using System.Configuration;
using iTextSharp.text.pdf;
using iTextSharp.text;
using System.IO;
using OfficeOpenXml;

namespace Presentacion
{
    public partial class formGetAllLineaExpendio : Form
    {
        private bool logueado = false;

        public bool Logueado
        {
            get { return logueado; }
            set { logueado = value; }
        }

        public Entidades.Usuario oUsuarioE ;
        public int idPersona; //cliente
        public int idSucursal;

        public DataTable dtSucursales;

        public Negocio.Sucursal oSucursalN = new Negocio.Sucursal();
        public Negocio.Venta oVentaN = new Negocio.Venta();
        public Negocio.Usuario oUsuarioN = new Negocio.Usuario();

        public DataTable dtExpendios;
        DataTable tablaFiltrada;

        bool cargar = false;
        public formGetAllLineaExpendio()
        {
            InitializeComponent(); this.Icon = Properties.Resources.CarniSys_ICONO;            
        }

        public void cargarGrilla()
        {
            try
            {
                if (cargar)
                {
                    lblActualizar.Visible = false;
                    dtExpendios = new DataTable();
                    // Resta de DateTime
                    TimeSpan diferencia = DateTime.Now - fechaDesde.Value;
                    int minutos = (int)diferencia.TotalMinutes;
                    dtExpendios = oVentaN.obtenerUltimosExpendios(minutos, Convert.ToInt32(comboSucursal.SelectedValue));
                    grillaExpendios.DataSource = dtExpendios;
                    grillaExpendios.Columns["idVenta"].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                    grillaExpendios.Columns["fechaExpendio"].DefaultCellStyle.Format = "dd/MM/yyyy HH:mm:ss";
                    grillaExpendios.Columns["idExpendio"].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                    grillaExpendios.Columns["identificacionExpendio"].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                    grillaExpendios.Columns["sector"].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                    grillaExpendios.Columns["codigo"].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                    grillaExpendios.Columns["corte"].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                    grillaExpendios.Columns["cantKg"].DefaultCellStyle.Format = "F3";

                    //pintar filas para igual idExpendio
                    if (oUsuarioE.Admin)
                    {
                        int ultimoidExpendio = grillaExpendios.Rows.Count > 0 ? Convert.ToInt32(grillaExpendios.Rows[0].Cells["idExpendio"].Value) : 0;
                        int cantMismoId = 0;
                        Color ultimoColorFila = Color.LightGray;
                        for (int i = 0; i < grillaExpendios.Rows.Count; i++)
                        {
                            if (ultimoidExpendio == Convert.ToInt32(grillaExpendios.Rows[i].Cells["idExpendio"].Value))
                            {
                                grillaExpendios.Rows[i].DefaultCellStyle.BackColor = ultimoColorFila;
                                cantMismoId++;
                            }
                            else
                            {
                                ultimoColorFila = Color.LightGray == ultimoColorFila ? Color.LightGreen : Color.LightGray;
                                grillaExpendios.Rows[i].DefaultCellStyle.BackColor = ultimoColorFila;
                                cantMismoId = 0;
                            }
    
                            //Se setea el ultimo idExpendio
                            ultimoidExpendio = Convert.ToInt32(grillaExpendios.Rows[i].Cells["idExpendio"].Value);
                        }
                    }

                    cargarTotales();
                    filtrarExpendio();
                } 
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void cargarTotales()
        {
            float totalKgs = 0, totalS = 0;

            foreach (DataRow venta in dtExpendios.Rows)
            {
                totalKgs += float.Parse(venta["cantKg"].ToString());
                totalS += float.Parse(venta["total"].ToString());

            }
            txtTotalKgs.Text = String.Format("{0:0.00}", totalKgs);
            txtTotalS.Text = oUsuarioE.Admin ?  String.Format("{0:0.00}", totalS) : "";
        }

        private void cargarSucursal()
        {
            dtSucursales = new DataTable();
            oSucursalN = new Negocio.Sucursal();
            dtSucursales = oSucursalN.obtenerSucursalesConTodas();
            comboSucursal.DataSource = dtSucursales;
            comboSucursal.DisplayMember = "sucursal";
            comboSucursal.ValueMember = "idSucursal";
            comboSucursal.SelectedValue = Convert.ToInt32(Utilidades.Conexion.getIdSucursalConexion());
        }
               

        private void btnBuscar_Click(object sender, EventArgs e)
        {
            cargarGrilla();
        }


        private void filtrarExpendio()
        {
            ///TODO: 
            ///exportar a exel la fecha en formato correcto
            ///actualizar totales y poner cantidad items
            ///

            if (!cargar)
                return;

            // Crear un nuevo DataTable con la misma estructura que el original
            tablaFiltrada = dtExpendios.Clone();

            if (dtExpendios.Rows.Count > 0)// && (txtBuscarExpendio.Text != "" || changeComboExpendio))
            {
                try
                {
                    //para poder filtar identif.Cliente q es string se asigna '0' a filtro de idExpendio
                    string filtroPorId = int.TryParse(txtBuscarExpendio.Text, out int numero) ? txtBuscarExpendio.Text : "0";
                    string filtroPorIdentif = "identificacionExpendio LIKE " + (int.TryParse(txtBuscarExpendio.Text, out int numero1) ? "'" + txtBuscarExpendio.Text + "'" : "'%" + txtBuscarExpendio.Text + "%'");
                    string filtroExpendio = !string.IsNullOrWhiteSpace(txtBuscarExpendio.Text) ? "(idExpendio = " + filtroPorId + " OR " + filtroPorIdentif + " ) " : string.Empty;

                    string filtroCombo = comboExpendioEstado.Text == "PENDIENTES" ? "(idVenta IS NULL OR idVenta = 0)" :
                        comboExpendioEstado.Text == "ASIGNADOS" ? "(idVenta IS NOT NULL AND idVenta > 0)" : "";
                    string filtroVendedor = (comboUsuario.Text == "Todos" ? "" : " Vendedor LIKE '" + comboUsuario.Text + "'");
                    string filtroSector = (comboSector.Text.ToUpper() == "TODOS" ? "" : " sector LIKE '" + comboSector.Text + "'");
                    string and = !string.IsNullOrEmpty(filtroCombo) && !string.IsNullOrEmpty(filtroVendedor) ? " AND " : "";
                    filtroExpendio += !string.IsNullOrEmpty(filtroExpendio) && !string.IsNullOrEmpty(and) ? " AND " : "";
                    string filtroCompleto = filtroExpendio + filtroCombo + and + filtroVendedor;
                    filtroCompleto = !string.IsNullOrEmpty(filtroSector) && !string.IsNullOrEmpty(filtroCompleto) ?
                        filtroCompleto + " AND " + filtroSector : filtroSector;
                    DataRow[] filas = dtExpendios.Select(filtroCompleto);

                    // Crear un nuevo DataTable con la misma estructura que el original
                    tablaFiltrada = dtExpendios.Clone();

                    // Importar cada DataRow del arreglo al nuevo DataTable
                    foreach (DataRow fila in filas)
                    {
                        tablaFiltrada.ImportRow(fila);
                    }

                    // Asignar el DataTable filtrado al DataGridView
                    grillaExpendios.DataSource = tablaFiltrada;

                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error al filtrar grillan\n" + ex.Message);
                }
            }
            else
            {
                tablaFiltrada = dtExpendios;
            }
            grillaExpendios.DataSource = tablaFiltrada;
        }


        private void btnCancelar_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void formGetAllLineaExpendio_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode==Keys.N)
            {
            }
        }

        private void comboSucursal_SelectedIndexChanged(object sender, EventArgs e)
        {
            cargarGrilla();
        }

        private void formGetAllLineaExpendio_Load(object sender, EventArgs e)
        {
            this.Text += Utilidades.Conexion.getSucursalConexion();
            DateTime fechaActual = DateTime.Now; // Fecha y hora actuales
            DateTime fechaHoyConHoraCero = new DateTime(fechaActual.Year, fechaActual.Month, fechaActual.Day, 0, 0, 0);
            fechaDesde.Value = fechaHoyConHoraCero;
            cargarSucursal();
            cargarComboVendedor();
            cargarComboSectores();
            if (!oUsuarioE.Admin)
            {
                fechaDesde.Enabled = false;
                comboUsuario.SelectedValue = oUsuarioE.Id;
                comboSucursal.SelectedValue = idSucursal;
                comboSucursal.Enabled = false ;
                comboUsuario.Enabled = false ;
            }
            comboExpendioEstado.SelectedIndex = 0;
            cargar = true;
            cargarGrilla();
            filtrarExpendio();
        }

        private void cargarComboVendedor()
        {
            comboUsuario.DataSource = oUsuarioN.obtenerUsuariosConTodos(true);
            comboUsuario.DisplayMember = "nombre";
            comboUsuario.ValueMember = "id";
            comboUsuario.SelectedIndex = 0; 
        }

        private void cargarComboSectores()
        {
            comboSector.DataSource = oVentaN.obtenerSectoresConTodos();
            comboSector.DisplayMember = "sector";
            comboSector.ValueMember = "sector";
            comboSector.SelectedIndex = 0;
        }

        private void txtDescripcion_KeyDown(object sender, KeyEventArgs e)
        {
            lblActualizar.Visible = true;
            if (e.KeyValue.Equals(13))
            {
                cargarGrilla();
            }
        }

        private void fechaDesde_ValueChanged(object sender, EventArgs e)
        {
            lblActualizar.Visible = true;
        }

        private void menuDuplicar_Click(object sender, EventArgs e)
        {
            formGetAllLineaExpendio frmVentaDuplicar = new formGetAllLineaExpendio();
            frmVentaDuplicar.Show();
        }

        private void comboExpendioEstado_TextChanged(object sender, EventArgs e)
        {
            filtrarExpendio();
        }

        private void comboUsuario_SelectedValueChanged(object sender, EventArgs e)
        {
            filtrarExpendio();
        }

        private void exportPDF_Click(object sender, EventArgs e)
        {

        }
        public void ExportarDataTableAExcel()
        {
            try
            {
                // Crear el formulario para pedir el nombre del archivo
                string nombreArchivo = MostrarDialogoNombreArchivo();

                //si es null se aborta la accion
                if (nombreArchivo == null)
                    return;

                // Establecer el contexto de la licencia para evitar la excepción
                ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;

                //string nombreArchivo = fechaDesde.Value.Year.ToString()+ fechaDesde.Value.Month.ToString()+ fechaDesde.Value.Day.ToString();
                //nombreArchivo += "Expendios.xlsx";
                nombreArchivo += ".xlsx";
                string ruta = ConfigurationManager.AppSettings["rutaPDF"].ToString();
                string rutaArchivo = @ruta + "\\"+nombreArchivo;

                // Verificar si la carpeta existe, si no, crearla
                if (!Directory.Exists(@ruta))
                    Directory.CreateDirectory(@ruta);

                // Crear el archivo de Excel
                FileInfo archivo = new FileInfo(rutaArchivo);

                // Verificar si el archivo ya existe; si es así, eliminarlo
                if (archivo.Exists)
                {
                    archivo.Delete();
                }

                // Crear y llenar el archivo Excel
                using (ExcelPackage excel = new ExcelPackage(archivo))
                {
                    // Crear una hoja de trabajo
                    ExcelWorksheet hoja = excel.Workbook.Worksheets.Add(DateTime.Now.ToShortDateString());

                    // Agregar encabezados
                    for (int i = 0; i < tablaFiltrada.Columns.Count; i++)
                    {
                        hoja.Cells[1, i + 1].Value = tablaFiltrada.Columns[i].ColumnName;
                    }

                    // Agregar datos
                    for (int i = 0; i < tablaFiltrada.Rows.Count; i++)
                    {
                        for (int j = 0; j < tablaFiltrada.Columns.Count; j++)
                        {
                            var value = tablaFiltrada.Rows[i][j];

                            // Verifica si el valor es de tipo DateTime
                            if (value is DateTime dateTimeValue)
                            {
                                // Aplica el formato deseado para las fechas
                                hoja.Cells[i + 2, j + 1].Value = dateTimeValue.ToString("dd/MM/yyyy HH:mm"); // Cambia el formato según necesidad
                            }
                            else
                            {
                                hoja.Cells[i + 2, j + 1].Value = value;
                            }

                            //hoja.Cells[i + 2, j + 1].Value = tablaFiltrada.Rows[i][j];
                        }
                    }

                    // Guardar el archivo
                    excel.Save();
                    MessageBox.Show("La exportación se realizó correctamente.\n\n", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
                       
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al exportar lista.\n\n" + ex.Message, "", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void exportExcel_Click(object sender, EventArgs e)
        {
            ExportarDataTableAExcel();
        }

        private string MostrarDialogoNombreArchivo()
        {
            // Crear un formulario para ingresar el nombre
            Form dialogo = new Form
            {
                Width = 400,
                Height = 150,
                Text = "Nombre del archivo",
                StartPosition = FormStartPosition.CenterParent
            };

            Label lblNombre = new Label
            {
                Text = "Ingrese el nombre del archivo:",
                Top = 10,
                Left = 10,
                Width = 360
            };

            TextBox txtNombre = new TextBox
            {
                Top = 40,
                Left = 10,
                Width = 360
            };

            Button btnAceptar = new Button
            {
                Text = "Aceptar",
                Top = 80,
                Left = 150,
                DialogResult = DialogResult.OK
            };

            dialogo.Controls.Add(lblNombre);
            dialogo.Controls.Add(txtNombre);
            dialogo.Controls.Add(btnAceptar);
            dialogo.AcceptButton = btnAceptar;

            if (dialogo.ShowDialog() == DialogResult.OK)
            {
                if (string.IsNullOrEmpty(txtNombre.Text))
                {
                    MessageBox.Show("Debe ingresar un nombre para el archivo a exportar");
                    return null;
                }
                return txtNombre.Text.Trim();
            }

            return null;
        }

        private void txtBuscarExpendio_TextChanged(object sender, EventArgs e)
        {
            filtrarExpendio();
        }

        private void comboSector_TextChanged(object sender, EventArgs e)
        {
            filtrarExpendio();
        }
    }
}
