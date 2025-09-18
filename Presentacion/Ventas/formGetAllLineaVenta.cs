using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Presentacion.Ventas;
using System.Configuration;
using iTextSharp.text.pdf;
using iTextSharp.text;
using System.IO;
using OfficeOpenXml;

namespace Presentacion
{
    public partial class formGetAllLineaVenta : Form
    {
        private bool logueado = false;

        public bool Logueado
        {
            get { return logueado; }
            set { logueado = value; }
        }

        public bool verUltimasVentasClientes = false;
        public int idPersona; //cliente
        public int idSucursal;
        public bool desdeCajaVenta = false;

        public DataTable dtSucursales;

        public Negocio.Sucursal oSucursalN = new Negocio.Sucursal();
        public Negocio.Venta oVentaN = new Negocio.Venta();
        public Negocio.Usuario oUsuarioN = new Negocio.Usuario();
        public Entidades.CierreCaja oCierreE;

        public DataTable dtVentas;

        bool cargar = false;
        bool cerrarForm = false;
        public formGetAllLineaVenta()
        {
            InitializeComponent(); this.Icon = Properties.Resources.CarniSys_ICONO;            
        }

        public void cargarGrilla()
        {
            try
            {
                //CerrarForm = true para evitar que se muestre dos veces el cartel del mensaje
                if (!cerrarForm && !oUsuarioN.tienePermiso(desdeCajaVenta ? oCierreE.UsuarioInicio : FormPrincipal.oUserLogueado, this.Name, DateTime.Today, Utilidades.ValoresParametrosMetodos.IdCreadorNulo()))
                {
                    Utilidades.Mensajes.ErrorPermisoAcceso();
                    //if (!cargar)
                        cerrarForm = true;
                    return;
                }

                if (cargar)
                {

                    lblActualizar.Visible = false;

                    Utilidades.BarraProgreso barraProgreso = new Utilidades.BarraProgreso("Cargando lineas de ventas", "Cargando...");
                    barraProgreso.Show();

                    dtVentas = new DataTable();
                    dtVentas = verUltimasVentasClientes ? oVentaN.ultimasVentasCliente(idSucursal, idPersona) : 
                        oVentaN.getAllLineasVenta(Convert.ToInt32(comboSucursal.SelectedValue.ToString()), 
                        Convert.ToInt32(comboUsuario.SelectedValue.ToString()), fechaDesde.Value, fechaHasta.Value, 
                        txtDescripcion.Text.Trim());
                    grillaVentas.DataSource = dtVentas;
                    grillaVentas.Columns["fechaVenta"].DefaultCellStyle.Format = "dd/MM/yyyy HH:mm:ss";
                    grillaVentas.Columns["fechaVenta"].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCellsExceptHeader;
                    grillaVentas.Columns["cantKg"].DefaultCellStyle.Format = "F3";
                    grillaVentas.Columns["cantKg"].HeaderText = "cantidad";
                    grillaVentas.Columns["precioKg"].DefaultCellStyle.Format = "F2";
                    grillaVentas.Columns["totalCorte"].HeaderText = "Tot.Prod.";
                    grillaVentas.Columns["bonificacion"].DefaultCellStyle.Format = "F2";
                    grillaVentas.Columns["totalCorte"].DefaultCellStyle.Format = "F2";

                    //pintar filas para igual idVenta
                    if (verUltimasVentasClientes)
                    {
                        int ultimoIdVenta = grillaVentas.Rows.Count > 0 ? Convert.ToInt32(grillaVentas.Rows[0].Cells["idVenta"].Value) : 0;
                        int cantMismoId = 0;
                        Color ultimoColorFila = Color.LightGray;
                        for (int i = 0; i < grillaVentas.Rows.Count; i++)
                        {
                            if (ultimoIdVenta == Convert.ToInt32(grillaVentas.Rows[i].Cells["idVenta"].Value))
                            {
                                grillaVentas.Rows[i].DefaultCellStyle.BackColor = ultimoColorFila;
                                cantMismoId++;
                            }
                            else
                            {
                                ultimoColorFila = Color.LightGray == ultimoColorFila ? Color.LightGreen : Color.LightGray;
                                grillaVentas.Rows[i].DefaultCellStyle.BackColor = ultimoColorFila;
                                cantMismoId = 0;
                            }    
                            //Se setea el ultimo IdVenta
                            ultimoIdVenta = Convert.ToInt32(grillaVentas.Rows[i].Cells["idVenta"].Value);
                        }
                    }

                    cargarTotales();
                } 
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void cargarTotales()
        {
            float totalKgs=0,totalS=0;

            txtCantItems.Text = grillaVentas.Rows.Count.ToString();
            foreach (DataRow venta in dtVentas.Rows)
            {
                totalKgs += float.Parse(venta["cantKg"].ToString());
                totalS += float.Parse(venta["totalCorte"].ToString());

            }
            txtTotalKgs.Text = String.Format("{0:0.00}", totalKgs);
            if (Presentacion.FormPrincipal.logueado)
            {
                txtTotalS.Text = String.Format("{0:0.00}", totalS );
            }
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

        private void infoVenta()
        {
            try
            {
                int idVenta = Convert.ToInt32(grillaVentas.CurrentRow.Cells["idVenta"].Value.ToString());

                if (desdeCajaVenta)
                {
                    Caja.formUltimaVenta frmUltimaVenta = new Caja.formUltimaVenta();
                    frmUltimaVenta.oCierreE = oCierreE;
                    frmUltimaVenta.oUltimaVenta = oVentaN.getVentaById(idVenta);
                    frmUltimaVenta.ShowDialog();
                    return;
                }

                bool formAbierto = false;
                foreach (Form frm in Application.OpenForms)
                {
                    if (frm.GetType() == typeof(formInfoVenta))
                    {
                        foreach (Control ctrl in frm.Controls)
                        {
                            if (ctrl.Name.Equals("idVentaLabel") && ctrl.Text.Equals(idVenta.ToString()))
                            {
                                frm.BringToFront();
                                formAbierto = true;
                                break;
                            }
                        }
                    }
                }
                if (!formAbierto)
                {
                    formInfoVenta frmInfoVenta = new formInfoVenta();
                    frmInfoVenta.idVenta = idVenta;
                    frmInfoVenta.Show();
                }
            }
            catch (Exception)
            {
                MessageBox.Show("No se pudo obtener la información de la venta.\nVerifique que el pesaje corresponda a una venta");
            }
        }

        private void btnBuscar_Click(object sender, EventArgs e)
        {
            cargarGrilla();
        }

        private void btnSeleccionar_Click(object sender, EventArgs e)
        {
            infoVenta();
        }

        private void btnCancelar_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void grillaVentas_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            infoVenta();
        }

        private void formGetAllLineaVenta_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode==Keys.N)
            {
            }
        }

        private void comboSucursal_SelectedIndexChanged(object sender, EventArgs e)
        {
            cargarGrilla();
        }

        private void formGetAllLineaVenta_Load(object sender, EventArgs e)
        {

            this.Text += Utilidades.Conexion.getSucursalConexion();
            DateTime today = DateTime.Today.Date.AddHours(24);
            fechaHasta.Value = today.AddMilliseconds(-1);
            fechaDesde.Value = today.AddDays(-1);
            cargarSucursal();
            cargarComboVendedor();
            //si se llama form desde formVentaCaja
            if (verUltimasVentasClientes)
            {
                fechaDesde.Value = DateTime.Today.AddYears((2000 - DateTime.Today.Year));
                pnlBuscar.Enabled = false;
                pnlBuscar.BringToFront();
                barraControl.Enabled = false;
            }

            cargar = true;
            if (cerrarForm)
            {
                this.Close();
                return;
            }
            cargarGrilla();
            if (cerrarForm)
            {
                this.Close();
                return;
            }
        }

        private void cargarComboVendedor()
        {
            comboUsuario.DataSource = oUsuarioN.obtenerUsuariosConTodos(true);
            comboUsuario.DisplayMember = "nombre";
            comboUsuario.ValueMember = "id";
            comboUsuario.SelectedIndex = 0; 
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
            formGetAllLineaVenta frmVentaDuplicar = new formGetAllLineaVenta();
            frmVentaDuplicar.Show();
        }

        //private void exportPDF_Click(object sender, EventArgs e)
        //{
        //    try
        //    {
        //        //Creating iTextSharp Table from the DataTable data
        //        PdfPTable pdfTable = new PdfPTable(8);//grillaVentas.ColumnCount);
        //        pdfTable.DefaultCell.Padding = 3;
        //        pdfTable.WidthPercentage = 100;
        //        pdfTable.HorizontalAlignment = Element.ALIGN_LEFT;
        //        iTextSharp.text.Font fontsubtit = FontFactory.GetFont("Arial", 9);

        //        string encabezado = "";

        //        //Adding Header row
        //        foreach (DataGridViewColumn column in grillaVentas.Columns)
        //        {
        //            if (column.Index == 0 || column.Index == 2 || column.Index == 3 || column.Index == 4
        //                || column.Index == 5 || column.Index == 6 ||
        //                    column.Index == 7 || column.Index == 8)// || column.Index == 9)
        //            {
        //                PdfPCell cell = new PdfPCell(new Phrase(column.HeaderText, fontsubtit));
        //                cell.BackgroundColor = new iTextSharp.text.BaseColor(240, 240, 240);  //.text.Color(240, 240, 240);

        //                encabezado = "Cliente: " + txtDescripcion.Text;

        //                encabezado += " \n\n Desde: " + fechaDesde.Text +
        //                        " | Hasta: " + fechaHasta.Text + "\n\n";

        //                pdfTable.AddCell(cell);
        //            }
        //        }

        //        //Adding DataRow
        //        foreach (DataGridViewRow row in grillaVentas.Rows)
        //        {
        //            foreach (DataGridViewCell cell in row.Cells)
        //            {
        //                if (cell.ColumnIndex == 0 || cell.ColumnIndex == 2 || cell.ColumnIndex == 3 ||
        //                    cell.ColumnIndex == 4 || cell.ColumnIndex == 5 || cell.ColumnIndex == 6 ||
        //                    cell.ColumnIndex == 7 || cell.ColumnIndex == 8)// || cell.ColumnIndex == 9)
        //                {

        //                    string valueCell = "";
        //                    if (cell.ValueType.Name.Equals("Double") || cell.ValueType.Name.Equals("Decimal"))
        //                    {
        //                        valueCell = String.Format("{0:0.00}", cell.Value);
        //                    }
        //                    else
        //                    {
        //                        valueCell = cell.Value.ToString();
        //                        valueCell = (valueCell.Length > 16) ? valueCell.Substring(0, 16) : valueCell;
        //                    }
        //                    pdfTable.AddCell(new Phrase(valueCell, fontsubtit));
        //                }
        //            }
        //        }

        //        //agregando encabezado
        //        Paragraph parrafo = new Paragraph();
        //        parrafo.Alignment = Element.ALIGN_CENTER;
        //        parrafo.Font = FontFactory.GetFont("Arial", 9);
        //        parrafo.Add(encabezado);

        //        //linea Totales

        //        Paragraph lineaTotales = new Paragraph();
        //        lineaTotales.Alignment = Element.ALIGN_RIGHT;
        //        lineaTotales.Font = FontFactory.GetFont("Arial", 12);
        //        lineaTotales.Add("TOTAL   $"+txtTotalS.Text);

        //        string fileName = System.IO.Path.GetTempPath() + Guid.NewGuid().ToString() + ".pdf";
        //        using (FileStream stream = new FileStream(fileName, FileMode.Create))
        //        {
        //            Document pdfDoc = new Document(PageSize.A4, 10f, 10f, 10f, 0f);
        //            PdfWriter.GetInstance(pdfDoc, stream);
        //            pdfDoc.Open();
        //            pdfDoc.Add(parrafo);
        //            pdfDoc.Add(pdfTable);
        //            pdfDoc.Add(lineaTotales);
        //            pdfDoc.Close();
        //            stream.Close();

        //            System.Diagnostics.Process prc = new System.Diagnostics.Process();
        //            prc.StartInfo.FileName = fileName;
        //            prc.Start();
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show(ex.Message);
        //    }           
        //}

        private void exportExcel_Click(object sender, EventArgs e)
        {
            ExportarDataTableAExcel();
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

                nombreArchivo += ".xlsx";
                string ruta = ConfigurationManager.AppSettings["rutaPDF"].ToString();
                string rutaArchivo = @ruta + "\\" + nombreArchivo;

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

                    int fila = 1; // Fila inicial en Excel
                    int columna = 1; // Columna inicial en Excel

                    // Exportar encabezados visibles
                    foreach (DataGridViewColumn col in grillaVentas.Columns)
                    {
                        if (col.Visible) // Solo columnas visibles
                        {
                            hoja.Cells[fila, columna].Value = col.HeaderText;
                            columna++;
                        }
                    }

                    fila++; // Avanzar a la siguiente fila (datos)
                    // Exportar filas visibles
                    foreach (DataGridViewRow row in grillaVentas.Rows)
                    {
                        if (!row.IsNewRow) // Evitar la fila vacía al final
                        {
                            columna = 1; // Reiniciar columna
                            foreach (DataGridViewColumn col in grillaVentas.Columns)
                            {
                                if (col.Visible) // Solo columnas visibles
                                {
                                    var value = row.Cells[col.Index].Value;

                                    // Verifica si el valor es de tipo DateTime
                                    if (value is DateTime dateTimeValue)
                                    {
                                        // Aplica el formato deseado para las fechas
                                        hoja.Cells[fila, columna].Value = dateTimeValue.ToString("dd/MM/yyyy HH:mm"); // Cambia el formato según necesidad
                                    }
                                    else
                                    {
                                        hoja.Cells[fila, columna].Value = value;
                                    }
                                    columna++;
                                }
                            }
                            fila++;
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
    }
}
