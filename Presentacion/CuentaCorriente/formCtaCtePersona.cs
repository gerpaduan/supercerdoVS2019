using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Presentacion.Ventas;
using Presentacion.Caja;
using iTextSharp.text.pdf;
using iTextSharp.text;
using System.IO;

namespace Presentacion.CuentaCorriente
{
    public partial class formCtaCtePersona : Form, InterfaceUsuario
    {
        Negocio.CuentaCorriente oCtaCteN = new Negocio.CuentaCorriente();
        Negocio.Usuario oUsuarioN = new Negocio.Usuario();
        public Entidades.Usuario oUsuario;
        public Entidades.CierreCaja oCierreCajaE;

        public int idPersona;
        DataTable dtMov;
        Entidades.Persona oPersonaE;
        DateTime limitFechaDesde = DateTime.Today.AddDays(-Entidades.Parametros.diasLimitFechaDesde);
        DateTime ultimaFechaDesde; //guarda la ultima fecha de la busqueda exitosa
        public bool desdePOS = true; //para indicar que es llamado desde el form POS

        public formCtaCtePersona()
        {
            InitializeComponent(); this.Icon = Properties.Resources.CarniSys_ICONO;
        }

        private void formCtaCtePersona_Load(object sender, EventArgs e)
        {
            try
            {
                oUsuario = desdePOS ? oCierreCajaE.UsuarioInicio : FormPrincipal.oUserLogueado;
                if (!(oUsuarioN.tienePermiso(oUsuario, this.Name, DateTime.Today, Utilidades.ValoresParametrosMetodos.IdCreadorNulo())))
                {
                    Utilidades.Mensajes.ErrorPermisoAcceso();
                    this.Close();
                    return;
                }

                Negocio.Persona oPersonaN = new Negocio.Persona();
                oPersonaE = oPersonaN.findById(idPersona);
                txtPersona.Text = oPersonaE.razonSocial;
                ////Ocultar Ultimas Ventas para SuperCerdo si cliente es Empleado o Cliente tiene CtaCte por defecto
                if ((FormPrincipal.soyYo && !oUsuario.Admin) &&
                    (oPersonaE.CtaCte || oPersonaE.razonSocial.ToLower().Contains("empleado") || oPersonaE.razonSocial.ToLower().Contains("empleada")))
                {
                    MessageBox.Show("No se pueden modificar los registros de la Persona seleccionada.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    this.Close();
                    return;
                }

                if (!oPersonaE.Identificacion.Equals(oPersonaE.razonSocial))
                    txtPersona.Text = oPersonaE.Identificacion + " / " + oPersonaE.razonSocial;
                fechaDesdePick.Value = ultimaFechaDesde = limitFechaDesde;
                cargarGrilla();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public void cargarGrilla()
        {
            try
            {
                if (fechaDesdePick.Value < limitFechaDesde && 
                    !oUsuarioN.tienePermiso(oUsuario, this.Name, fechaDesdePick.Value,
                    Utilidades.ValoresParametrosMetodos.IdCreadorNulo()))
                {
                    ///si ultimaFechaDesde es menor al limitFechaDesde significa que el usuario tiene permiso para fecha anterior
                    ///
                    if (ultimaFechaDesde < limitFechaDesde)
                    {
                        Utilidades.Mensajes.ErrorPermisoAcceso();
                        fechaDesdePick.Value = ultimaFechaDesde;
                        return;
                    }

                    MessageBox.Show("No tiene permiso para filtrar a partir de la fecha desde seleccionada");
                    fechaDesdePick.Value = limitFechaDesde;
                }

                dtMov = oCtaCteN.getCtaCteByIdPersona(idPersona, fechaDesdePick.Value);

                ultimaFechaDesde = fechaDesdePick.Value;

                if (!checkSinRegRepetidos.Checked)
                {
                    int[] aBorrar = new int[dtMov.Rows.Count];
                    for (int i = 0; i < aBorrar.Length; i++)
                    {
                        aBorrar[i] = -1;
                    }

                    for (int filaPrimer = 0; filaPrimer < dtMov.Rows.Count; filaPrimer++)
                    {
                        for (int fila = 0; fila < dtMov.Rows.Count; fila++)
                        {
                            if (aBorrar[filaPrimer] == 1)
                                break;

                            string tablaPrimer = dtMov.Rows[filaPrimer]["tabla"].ToString();
                            string idtablaPrimer = dtMov.Rows[filaPrimer]["idTabla"].ToString();
                            string sucursalPrimer = dtMov.Rows[filaPrimer]["sucursal"].ToString();
                            int idPrimer = Convert.ToInt32(dtMov.Rows[filaPrimer]["id"].ToString());

                            string tabla = dtMov.Rows[fila]["tabla"].ToString();
                            string idtabla = dtMov.Rows[fila]["idTabla"].ToString();
                            string sucursal = dtMov.Rows[fila]["sucursal"].ToString();
                            int id = Convert.ToInt32(dtMov.Rows[fila]["id"].ToString());

                            if (tabla.Equals(tablaPrimer) && idtabla.Equals(idtablaPrimer) &&
                                 sucursal.Equals(sucursalPrimer) && id < idPrimer)
                            {
                                aBorrar[fila] = 1;
                            }
                        }
                    }

                    for (int i = 0; i < aBorrar.Length; i++)
                    {
                        if (aBorrar[i] == 1)
                            dtMov.Rows[i].Delete();
                    }

                    dtMov.AcceptChanges();
                }

                grillaMovCtaCte.DataSource = dtMov;
                grillaMovCtaCte.AutoGenerateColumns = false;

                grillaMovCtaCte.Columns["idPersona"].Visible = false;
                grillaMovCtaCte.Columns["razonSocial"].Visible = false;
                grillaMovCtaCte.Columns["id"].Visible = true;// false;
                grillaMovCtaCte.Columns["id"].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                grillaMovCtaCte.Columns["tabla"].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                grillaMovCtaCte.Columns["idTabla"].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                grillaMovCtaCte.Columns["tipo"].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                grillaMovCtaCte.Columns["detalle"].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;

                //formato
                grillaMovCtaCte.Columns["tabla"].HeaderText = "Operacion";
                grillaMovCtaCte.Columns["importe"].DefaultCellStyle.Format = "N2";
                grillaMovCtaCte.Columns["Saldo"].DefaultCellStyle.Format = "N2";

                if (grillaMovCtaCte.Rows.Count > 0)
                {
                    grillaMovCtaCte.Rows[0].Selected = false;
                }

                lblActualizar.Visible = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnSeleccionar_Click(object sender, EventArgs e)
        {
            try
            {
                if (grillaMovCtaCte.CurrentRow == null)
                {
                    MessageBox.Show("Seleccione un registro");
                    return;
                }

                string tabla = grillaMovCtaCte.CurrentRow.Cells["tabla"].Value.ToString();
                int idTabla = Convert.ToInt32(grillaMovCtaCte.CurrentRow.Cells["idTabla"].Value.ToString());

                if (idTabla == 0)
                {
                    MessageBox.Show("Seleccione un registro válido");
                    return;
                }

                Entidades.MovCtaCte oMovCtaCteE = new Entidades.MovCtaCte();
                Entidades.MovCtaCte.tablas tablaEnum = oMovCtaCteE.getTablaEnum(tabla);
                switch (tablaEnum)
                {
                    case Entidades.MovCtaCte.tablas.Ventas:
                        infoVenta(idTabla);
                        break;
                    case Entidades.MovCtaCte.tablas.Compras: 
                        Compras.formModificarCompra frmModificarCompra = new Compras.formModificarCompra();
                        frmModificarCompra.cargarParametros(null, idTabla);
                        frmModificarCompra.Show();
                        break;
                    case Entidades.MovCtaCte.tablas.Pagos:
                        Pagos.formAddOrEditPago frmAddOrEditPago = new Presentacion.Pagos.formAddOrEditPago();
                        frmAddOrEditPago.frmCtaCtePersona = this;
                        frmAddOrEditPago.idPago = idTabla;
                        frmAddOrEditPago.desdePOS = desdePOS;
                        frmAddOrEditPago.oCierreCajaE = oCierreCajaE;
                        frmAddOrEditPago.Show();
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }

        private void btnCancelar_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnBuscar_Click(object sender, EventArgs e)
        {
            cargarGrilla();
        }

        private void infoVenta(int idVenta)
        {

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

        private void menuNuevoPago_Click(object sender, EventArgs e)
        {
            Pagos.formAddOrEditPago frmAddOrEditPago = new Presentacion.Pagos.formAddOrEditPago();
            frmAddOrEditPago.frmCtaCtePersona = this;
            frmAddOrEditPago.oPersonaE = oPersonaE;
            frmAddOrEditPago.oUsuario = desdePOS ? oUsuario : null;//forzo que fuera de POS se llame al logueo
            frmAddOrEditPago.desdePOS = desdePOS;
            frmAddOrEditPago.oCierreCajaE = oCierreCajaE;
            frmAddOrEditPago.ShowDialog();
        }
        
        public void EnviarUsuario(Entidades.Usuario usuario)
        {
            oUsuario = usuario;
        }

        private void checkSinRegRepetidos_CheckedChanged(object sender, EventArgs e)
        {
            cargarGrilla();
        }

        private void fechaDesdePick_KeyDown(object sender, KeyEventArgs e)
        {
            lblActualizar.Visible = true;
            if (e.KeyValue.Equals(13))
            {
                cargarGrilla();
            }
        }

        private void fechaDesdePick_ValueChanged(object sender, EventArgs e)
        {
            lblActualizar.Visible = true;
        }

        private void Imprimir_Click(object sender, EventArgs e)
        {
            //Ticket.formTipoTicket tipoTicket = new Presentacion.Ticket.formTipoTicket();
            //tipoTicket.ctaCtePersona(oPersonaE, dtMov);
            try
            {
                DataTable dtMov = oCtaCteN.getCtaCteByIdPersona(idPersona, fechaDesdePick.Value);

                // Obtener persona y saldo igual que en la vista
                string persona = oPersonaE.razonSocial;

                foreach (char c in Path.GetInvalidFileNameChars())
                {
                    persona = persona.Replace(c, '_');
                }

                byte[] pdfBytes = Utilidades.GenerarDocs.GenerarPdfCtaCtePersona(dtMov, fechaDesdePick.Value); // GenerarPdfPersona(dtMov, persona, saldo, fechaD);

                // 2. Guardar el PDF en una ruta temporal automática
                string tempPath = Path.Combine(Path.GetTempPath(), $"{persona}_CuentaCorriente.pdf");
                File.WriteAllBytes(tempPath, pdfBytes);

                // 3. Abrir el PDF
                System.Diagnostics.Process.Start(tempPath);

                return;

                //Creating iTextSharp Table from the DataTable data
                PdfPTable pdfTable = new PdfPTable(6);//grillaMovCtaCte.ColumnCount);
                pdfTable.DefaultCell.Padding = 3;
                pdfTable.WidthPercentage = 100;
                pdfTable.HorizontalAlignment = Element.ALIGN_LEFT;
                iTextSharp.text.Font fontsubtit = FontFactory.GetFont("Arial", 9);


                string encabezado = "Cuenta Corriente\n";
                encabezado += "Razon Social: " + txtPersona.Text + " || Desde: " + fechaDesdePick.Value.ToShortDateString() +"\n\n";

                int indexDetalle = 7;

                //Adding Header row
                foreach (DataGridViewColumn column in grillaMovCtaCte.Columns)
                {
                    if (column.Index == 3 || column.Index == 4 || //column.Index == 5 ||
                            column.Index == 6 || column.Index == 7 || column.Index == 9 || column.Index == 10)
                    {
                        PdfPCell cell = new PdfPCell(new Phrase(column.HeaderText, fontsubtit));
                        cell.BackgroundColor = new iTextSharp.text.BaseColor(240, 240, 240);  //.text.Color(240, 240, 240);
                        pdfTable.AddCell(cell);
                    }
                }

                //Adding DataRow
                foreach (DataGridViewRow row in grillaMovCtaCte.Rows)
                {
                    foreach (DataGridViewCell cell in row.Cells)
                    {
                        if (cell.ColumnIndex == 3 || cell.ColumnIndex == 4 || //cell.ColumnIndex == 5 ||
                            cell.ColumnIndex == 6 || cell.ColumnIndex == 7 || cell.ColumnIndex == 9 || cell.ColumnIndex == 10)
                        {

                            string valueCell = "";
                            if (cell.ValueType.Name.Equals("Double") || cell.ValueType.Name.Equals("Decimal"))
                            {
                                valueCell = String.Format("{0:0.00}", cell.Value);
                            }
                            else
                            {
                                valueCell = cell.Value.ToString();
                                //valueCell = (valueCell.Length > 10) ?
                                //    (cell.ColumnIndex == indexDetalle ? (valueCell.Length > 40 ? valueCell.Substring(0, 40) : valueCell) : valueCell.Substring(0, 10)) 
                                //    : valueCell;
                            }
                            pdfTable.AddCell(new Phrase(valueCell, fontsubtit));
                        }
                    }
                }

                //agregando encabezado
                Paragraph parrafo = new Paragraph();
                parrafo.Alignment = Element.ALIGN_CENTER;
                parrafo.Font = FontFactory.GetFont("Arial", 11); 
                parrafo.Add(encabezado);

                string fileName = System.IO.Path.GetTempPath() + Guid.NewGuid().ToString() + ".pdf";
                using (FileStream stream = new FileStream(fileName, FileMode.Create))
                {
                    Document pdfDoc = new Document(PageSize.A4, 10f, 10f, 10f, 0f);
                    PdfWriter.GetInstance(pdfDoc, stream);
                    pdfDoc.Open();
                    pdfDoc.Add(parrafo);
                    pdfDoc.Add(pdfTable);
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

        private void btnWhatsApp_Click(object sender, EventArgs e)
        {
            Utilidades.Util_Form.enviarWhatsApp(oPersonaE.Telefono);
        }
    }
}
