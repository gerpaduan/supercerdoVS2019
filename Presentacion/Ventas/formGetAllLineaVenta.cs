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

        public DataTable dtSucursales;

        public Negocio.Sucursal oSucursalN = new Negocio.Sucursal();
        public Negocio.Venta oVentaN = new Negocio.Venta();
        public Negocio.Usuario oUsuarioN = new Negocio.Usuario();

        public DataTable dtVentas;

        bool cargar = false;
        public formGetAllLineaVenta()
        {
            InitializeComponent();            
        }

        public void cargarGrilla()
        {
            try
            {
                if (cargar)
                {
                    lblActualizar.Visible = false;
                    dtVentas = new DataTable();
                    dtVentas = oVentaN.getAllLineasVenta(Convert.ToInt32(comboSucursal.SelectedValue.ToString()), 
                        Convert.ToInt32(comboUsuario.SelectedValue.ToString()), fechaDesde.Value, fechaHasta.Value, 
                        txtDescripcion.Text.Trim());
                    grillaVentas.DataSource = dtVentas;
                    grillaVentas.Columns["fechaVenta"].DefaultCellStyle.Format = "dd/MM/yyyy HH:mm:ss";
                    grillaVentas.Columns["fechaVenta"].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCellsExceptHeader;
                    grillaVentas.Columns["cantKg"].DefaultCellStyle.Format = "F3";
                    grillaVentas.Columns["precioKg"].DefaultCellStyle.Format = "F2";
                    grillaVentas.Columns["bonificacion"].DefaultCellStyle.Format = "F2";
                    grillaVentas.Columns["totalCorte"].DefaultCellStyle.Format = "F2";


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

                //if (Application.OpenForms["formInfoVenta"] != null)
                //{
                //    Application.OpenForms["formInfoVenta"].Activate();
                //    Application.OpenForms["formInfoVenta"].WindowState = FormWindowState.Normal;
                //}
                //else
                //{
                //    formInfoVenta frmInfoVenta = new formInfoVenta();
                //    frmInfoVenta.idVenta = idVenta;
                //    frmInfoVenta.Show();
                //}
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
            cargar = true;
            cargarGrilla();
        }

        private void cargarComboVendedor()
        {
            comboUsuario.DataSource = oUsuarioN.obtenerUsuariosConTodos();
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

        private void exportPDF_Click(object sender, EventArgs e)
        {
            try
            {
                //Creating iTextSharp Table from the DataTable data
                PdfPTable pdfTable = new PdfPTable(6);//grillaVentas.ColumnCount);
                pdfTable.DefaultCell.Padding = 3;
                pdfTable.WidthPercentage = 100;
                pdfTable.HorizontalAlignment = Element.ALIGN_LEFT;
                iTextSharp.text.Font fontsubtit = FontFactory.GetFont("Arial", 9);

                string encabezado = "";

                //Adding Header row
                foreach (DataGridViewColumn column in grillaVentas.Columns)
                {
                    if (column.Index == 1 || column.Index == 3 || column.Index == 6 ||
                            column.Index == 7 || column.Index == 8 || column.Index == 9)
                    {
                        PdfPCell cell = new PdfPCell(new Phrase(column.HeaderText, fontsubtit));
                        cell.BackgroundColor = new iTextSharp.text.BaseColor(240, 240, 240);  //.text.Color(240, 240, 240);

                        encabezado = "Cliente: " + txtDescripcion.Text;

                        encabezado += " \n\n Desde: " + fechaDesde.Text +
                                " | Hasta: " + fechaHasta.Text + "\n\n";

                        pdfTable.AddCell(cell);
                    }
                }

                //Adding DataRow
                foreach (DataGridViewRow row in grillaVentas.Rows)
                {
                    foreach (DataGridViewCell cell in row.Cells)
                    {
                        if (cell.ColumnIndex == 1 || cell.ColumnIndex == 3 || cell.ColumnIndex == 6 ||
                            cell.ColumnIndex == 7 || cell.ColumnIndex == 8 || cell.ColumnIndex == 9)
                        {

                            string valueCell = "";
                            if (cell.ValueType.Name.Equals("Double") || cell.ValueType.Name.Equals("Decimal"))
                            {
                                valueCell = String.Format("{0:0.00}", cell.Value);
                            }
                            else
                            {
                                valueCell = cell.Value.ToString();
                                valueCell = (valueCell.Length > 16) ? valueCell.Substring(0, 16) : valueCell;
                            }
                            pdfTable.AddCell(new Phrase(valueCell, fontsubtit));
                        }
                    }
                }

                //agregando encabezado
                Paragraph parrafo = new Paragraph();
                parrafo.Alignment = Element.ALIGN_CENTER;
                parrafo.Font = FontFactory.GetFont("Arial", 9);
                parrafo.Add(encabezado);

                //linea Totales

                Paragraph lineaTotales = new Paragraph();
                lineaTotales.Alignment = Element.ALIGN_RIGHT;
                lineaTotales.Font = FontFactory.GetFont("Arial", 12);
                lineaTotales.Add("TOTAL   $"+txtTotalS.Text);

                string fileName = System.IO.Path.GetTempPath() + Guid.NewGuid().ToString() + ".pdf";
                using (FileStream stream = new FileStream(fileName, FileMode.Create))
                {
                    Document pdfDoc = new Document(PageSize.A4, 10f, 10f, 10f, 0f);
                    PdfWriter.GetInstance(pdfDoc, stream);
                    pdfDoc.Open();
                    pdfDoc.Add(parrafo);
                    pdfDoc.Add(pdfTable);
                    pdfDoc.Add(lineaTotales);
                    pdfDoc.Close();
                    stream.Close();

                    System.Diagnostics.Process prc = new System.Diagnostics.Process();
                    prc.StartInfo.FileName = fileName;
                    prc.Start();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }           
        }
    }
}
