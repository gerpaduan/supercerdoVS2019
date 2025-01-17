using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Cryptography;
using System.Text;
using System.Web.Services.Description;
using System.Web.UI.WebControls;
using System.Windows.Forms;
using OfficeOpenXml;

using Presentacion.Cortes;
using Presentacion.Personas;
using static System.Net.WebRequestMethods;

namespace Presentacion
{
    public partial class formCortes : formBaseColor, InterfacePersona
    {
        Negocio.Corte oCorteN = new Negocio.Corte();
        Entidades.Corte oCorteE;
        Entidades.Corte oCorteMaestroE;

        DataTable dtCortes;
        DataTable dtCortesFiltrado;

        bool comboCargado = false;
        bool mostrarMensajeExport = false;
        long codigoDesde, codigoHasta;
        int idMarca = -1;//-1 busca a todos
        public formCortes()
        {
            InitializeComponent();
        }

        #region eventos
        private void nuevo_Click(object sender, EventArgs e)
        {
            nuevoCorte();
        }

        private void nuevoCorte()
        {
            //formNuevoCorte frmNuevoCorte = new formNuevoCorte();
            //frmNuevoCorte.frmCorte = this;
            //frmNuevoCorte.ShowDialog(this);

            if (Application.OpenForms["formNuevoCorte"] != null)
            {

                Application.OpenForms["formNuevoCorte"].Activate();
                Application.OpenForms["formNuevoCorte"].WindowState = FormWindowState.Normal;
            }
            else
            {
                //Presentacion.Caja.FormLoginVendedor frmLogin = new Presentacion.Caja.FormLoginVendedor();
                //frmLogin.soloActivos = true;
                //frmLogin.ShowDialog(this);

                //if (oUsuario == null)
                //    return;

                formNuevoCorte frmNuevoCorte = new formNuevoCorte();
                frmNuevoCorte.frmCorte = this;
                frmNuevoCorte.Show(this);
            }
        }

        private void modificar_Click(object sender, EventArgs e)
        {
            modificarCorte();

            /////Actualizando Nivel 
            /////
            //for (int i = 0; i < grillaCortes.Rows.Count; i++)
            //{
            //    cargarCorte(i);
            //    oCorteN.addOrEditCorte(oCorteE);
            //}
        }
    
        private void stock_Click(object sender, EventArgs e)
        {
            formIngresoEmbutido frmIngresoEmbutido = new formIngresoEmbutido();
            frmIngresoEmbutido.ShowDialog();
        }
        
        private void grillaCortes_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            infoCorte();
        }
        private void btnBuscar_Click(object sender, EventArgs e)
        {
            buscarCorte();
        }
        private void txtBuscarCorte_TextChanged(object sender, EventArgs e)
        {
            //buscarCorte();
            //lblActualizar.Visible = true;
        }
        #endregion

        #region metodos

        public void cargarGrilla()
        {
            ///TODO: hacer prueba exhuastiva en cargar miles de Cortes
            ///
            if (!comboCargado)
                return;

            Utilidades.BarraProgreso barraProgreso = new Utilidades.BarraProgreso("Cargando productos", "Cargando...");
            barraProgreso.Show();

            lblActualizar.Visible = false;

            string txtBusqueda = this.txtBuscarCorte.Text.Trim();

            grillaCortes.AutoGenerateColumns = false;

            dtCortes = oCorteN.buscarCorte(txtBusqueda);
            grillaCortes.DataSource = dtCortes;
            filtarGrilla();
        }
        
        public void buscarCorte()
        {
            cargarGrilla();
            return;

            //se llama a cargar grilla directamente
            lblActualizar.Visible = false;
            oCorteN = new Negocio.Corte();

            string txtBusqueda = this.txtBuscarCorte.Text.Trim();

            grillaCortes.AutoGenerateColumns = false;

            dtCortes = oCorteN.buscarCorte(txtBusqueda);
            grillaCortes.DataSource = dtCortes;
            filtarGrilla();
        }

        private void modificarCorte()
        {
            try
            {
                int idCorte = Convert.ToInt32(grillaCortes.CurrentRow.Cells["idCorte"].Value.ToString());
                formNuevoCorte frmNuevoCorte = new formNuevoCorte();
                frmNuevoCorte.idCorte = idCorte;
                frmNuevoCorte.frmCorte = this;
                frmNuevoCorte.ShowDialog();

                //ExportarDataTableAExcel();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void modificarPrecios_Click(object sender, EventArgs e)
        {
            try
            {
                if (!Usuarios.FormValidarPermiso.validarPermiso())
                {
                    this.Close();
                }
                else
                {
                    List<Entidades.Corte> listCortes = new List<Entidades.Corte>();

                    foreach (DataGridViewRow filaCorte in grillaCortes.Rows)
                    {
                        cargarCorte(filaCorte.Index);
                        listCortes.Add(oCorteE);
                    }

                    formModificarPrecios frmModificarPrecios = new formModificarPrecios();

                    foreach (Entidades.Corte filaCorte in listCortes)
                    {
                        if (!frmModificarPrecios.finalizarMod)
                        {
                            //CargarCorte(filaCorte.Index);
                            frmModificarPrecios.obtenerCorteFormCortes(filaCorte, listCortes, this);
                            frmModificarPrecios.ShowDialog();

                            //si se modificó por porcentaje que cierra una vez q finalizó la modificacion en lotes
                            if (frmModificarPrecios.precioPorPorc)
                                return;
                        }
                        else
                        {
                            break;
                        }
                    }

                    //ExportarDataTableAExcel();
                }      
            }
            catch (Exception ex)
            {                
                MessageBox.Show(ex.Message);
            }            
        }

        private void infoCorte()
        {
            try
            {
                int idCorte = Convert.ToInt32(grillaCortes.CurrentRow.Cells["idCorte"].Value.ToString());
                formInfoCorte frmInfoCorte = new formInfoCorte();
                frmInfoCorte.idCorte = idCorte;
                frmInfoCorte.oFrmCortes = this;
                frmInfoCorte.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void cargarCorte(int fila)
        {
            oCorteE = new Entidades.Corte();
            oCorteMaestroE = new Entidades.Corte();

            oCorteE.idCorte = Convert.ToInt32(grillaCortes.Rows[fila].Cells["idCorte"].Value.ToString());
            oCorteE = oCorteN.getCorteById(oCorteE.idCorte, true);
        }        

        #endregion

        private void btnSeleccionar_Click(object sender, EventArgs e)
        {
            infoCorte();
        }

        private void btnCancelar_Click(object sender, EventArgs e)
        {
            this.Close();            
        }

        private void formCortes_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode== Keys.M)
            {
                modificarCorte();
            }

            if (e.Control && e.KeyCode==Keys.B)
            {
                txtBuscarCorte.Focus();                
            }
        }

        private void Imprimir_Click(object sender, EventArgs e)
        {
            Ticket.formTipoTicket tipoTicket = new Presentacion.Ticket.formTipoTicket();
            tipoTicket.cortesConPrecios(dtCortesFiltrado);
            //imprimirReporte();
        }

        private void formCortes_Load(object sender, EventArgs e)
        {
            this.Text += Utilidades.Conexion.getSucursalConexion();
            comboTipo.DataSource = oCorteN.obtenerTiposProducto(true);
            comboTipo.DisplayMember = "tipo";
            comboTipo.ValueMember = "tipo";
            comboTipo.SelectedIndex = 0;
            comboCargado = true;
            //cargarGrilla();
            this.txtBuscarCorte.Select();
        }

        private void txtCodigoDesde_TextChanged(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(txtCodigoDesde.Text) && Utilidades.Util_Form.validarCampoNumeroEntero(txtCodigoDesde.Text, "Desde"))
            {
                codigoDesde = Convert.ToInt64(txtCodigoDesde.Text);
            }
            filtarGrilla();
        }

        private void txtCodigohasta_TextChanged(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(txtCodigohasta.Text) && Utilidades.Util_Form.validarCampoNumeroEntero(txtCodigohasta.Text, "Hasta"))
            {
                codigoHasta = Convert.ToInt64(txtCodigohasta.Text);
            }
            filtarGrilla();
        }

        public void filtarGrilla()
        {
            if (!comboCargado || dtCortes == null)
                return;

            dtCortesFiltrado = dtCortes.Clone();
            // Presuming the DataTable has a column named Date.
            string expresion = !string.IsNullOrEmpty(txtCodigoDesde.Text) ? "codigo >= " + codigoDesde : "true";
            expresion+= " and ";
            expresion += !string.IsNullOrEmpty(txtCodigohasta.Text) ? "codigo <= " + codigoHasta :  "true";
            if (!string.IsNullOrEmpty(comboTipo.Text) && !comboTipo.Text.ToUpper().Equals("TODOS"))
            {
                expresion += " and ";
                expresion += !string.IsNullOrEmpty(comboTipo.Text) ? "tipo = \'" + comboTipo.Text + "\'" : "true";
            }
            if (!string.IsNullOrEmpty(txtBuscarCorte.Text))
            {
                string buscaPorCodigo = (long.TryParse(txtBuscarCorte.Text, out long numero)) ? "codigo = " + numero : "true";

                expresion += " and ";
                expresion += " ( corte like \'" + txtBuscarCorte.Text + "%\' or " + buscaPorCodigo +" ) ";
            }
            if (!string.IsNullOrEmpty(txtBuscarMaestro.Text))
            {
                expresion += " and ";
                expresion += " corteMaestro like \'%" + txtBuscarMaestro.Text + "%\'";
            }
            if (!string.IsNullOrEmpty(txtMarca.Text) && !txtMarca.Text.ToUpper().Equals("TODAS"))
            {
                expresion += " and ";
                expresion += !string.IsNullOrEmpty(txtMarca.Text) ? "marca = \'" + txtMarca.Text + "\'" : "true";
            }

            DataRow[] foundRows;
            // Use the Select method to find all rows matching the filter.
            foundRows = dtCortes.Select(expresion);//, "codigo");

            foreach (DataRow row in foundRows)
            {
                dtCortesFiltrado.ImportRow(row);
            }
            grillaCortes.DataSource = dtCortesFiltrado;
            txtCantItems.Text = dtCortesFiltrado.Rows.Count.ToString();
        }

        protected override bool ProcessCmdKey(ref System.Windows.Forms.Message msg, Keys keyData)
        {
            if (keyData == Keys.Escape)
            {
                this.Close();
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void tipos_Click(object sender, EventArgs e)
        {
            if (Application.OpenForms["formTiposProducto"] != null)
            {

                Application.OpenForms["formTiposProducto"].Activate();
                Application.OpenForms["formTiposProducto"].WindowState = FormWindowState.Normal;

            }
            else
            {
                formTiposProducto frmTiposProducto = new formTiposProducto();
                frmTiposProducto.Show();
            }
        }

        private void btnCostoPorCobro_Click(object sender, EventArgs e)
        {
            formAddOrEditCostoCobro frmCostoPorCobre = new formAddOrEditCostoCobro();
            frmCostoPorCobre.ShowDialog();
        }

        private void txtBuscarCorte_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyValue.Equals(13))
            {
                cargarGrilla();
            }
        }

        private void exportar_Click(object sender, EventArgs e)
        {
            ExportarDataTableAExcel();
            mostrarMensajeExport = true;
        }

        private void systelPLU_Click(object sender, EventArgs e)
        {
            try
            {
                string ruta = ConfigurationManager.AppSettings["rutaPDF"].ToString();
                DataTable dataTable = dtCortesFiltrado;// oCorteN.lista_precios();
                string rutaArchivo = @ruta + "\\PLU_Systel.csv";

                MessageBox.Show("Se exportarán aquellos productos de la grilla dónde el código esté entre 1-99997 (código soportado por Systel).\n\n"
                    + "Ubicación: "+rutaArchivo, "Exportar lista Systel",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Establecer el contexto de la licencia para evitar la excepción
                ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;


                StringBuilder sb = new StringBuilder();

                string fila = "";
                // Agregar filas
                for (int i = 0; i < dataTable.Rows.Count; i++)
                {
                    //Numérico es difente a ese intervalo se saltea 1-99.997 
                    long codigo = Convert.ToInt64(dataTable.Rows[i]["codigo"].ToString());
                    if (codigo > 0 && codigo < 99998)
                    {
                        string seccion = "1";
                        string descripcion = dataTable.Rows[i]["corte"].ToString().Length > 18 ? dataTable.Rows[i]["corte"].ToString().Substring(0, 18) : dataTable.Rows[i]["corte"].ToString();
                        string precio = float.Parse(dataTable.Rows[i]["precioKg"].ToString()).ToString("F2");
                        string tipoVenta = dataTable.Rows[i]["pesable"].ToString().Equals("1") ? "P" : "U"; //(bool)(dataTable.Rows[i]["pesable"]) ? "P" : "U";

                        fila = seccion + ";" + codigo.ToString() + ";" + descripcion + ";" +
                            codigo.ToString() + ";" + precio + ";0,00" + ";" + tipoVenta + ";;";

                        sb.Append(fila);
                    }
                    //sb.Length--; // Eliminar el último delimitador
                    sb.AppendLine();
                }

                // Sobreescribir el archivo CSV
                System.IO.File.WriteAllText(rutaArchivo, sb.ToString(), Encoding.UTF8);

                if (true)
                {
                    MessageBox.Show("La exportación se realizó correctamente.\n\n", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    mostrarMensajeExport = false;
                }            
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al exportar lista de precios excel automaticamente.\n\n" + ex.Message, "", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void importarCSV_Click(object sender, EventArgs e)
        {
            try
            {
                if (FormPrincipal.oUserAdmin == null)
                {
                    MessageBox.Show("No tienes permiso para acceder al area seleccionada.");
                    return;
                }

                string ruta = ConfigurationManager.AppSettings["rutaPDF"].ToString();
                DataTable dataTable = dtCortesFiltrado;// oCorteN.lista_precios();
                string rutaArchivo = @ruta + "\\PLU_Systel.csv";

                DialogResult resp = MessageBox.Show("La importación creará productos para codigos nuevos y modificará datos de existentes.\n"+
                    "¿Está seguro de importar lista de productos CSV?\n\n"+ "Ubicación: " + rutaArchivo, "Se perderán las modificaciones",
                                MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);

                if (resp.Equals(DialogResult.No))
                    return;

                // Establecer el contexto de la licencia para evitar la excepción
                ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;


                // Verificar si la carpeta existe, si no, crearla
                if (!Directory.Exists(@ruta))
                    Directory.CreateDirectory(@ruta);

                // Crear el archivo de Excel
                FileInfo archivo = new FileInfo(rutaArchivo);

                // Verificar si el archivo ya existe; si es así, eliminarlo
                if (System.IO.File.Exists(rutaArchivo))
                {
                    //Sección; Código PLU; Descripción; Número de PLU; Precio de lista 1; Precio de lista 2; Tipo de venta; Vencimiento; Ingredientes
                    var lines = System.IO.File.ReadAllLines(rutaArchivo);

                    string codigo , descripcion ,  precio , tipoVenta ;
                    foreach (var line in lines)
                    {
                        oCorteE = new Entidades.Corte();

                        var values = line.Split(';'); // Divide los valores por comas
                        //Console.WriteLine($"Columna 1: {values[0]}, Columna 2: {values[1]}");
                        codigo = values[1];
                        descripcion = values[2];
                        precio = values[4];
                        tipoVenta = values[6];

                        if (codigo.Length > 10)
                        {
                            string d = "0";
                        }
                        oCorteE.Codigo = Convert.ToInt64(codigo);
                        oCorteE.CorteDesc = descripcion;
                        oCorteE.precioKg = Utilidades.Util_Form.convertFloat(precio, false);
                        oCorteE.Promedio = Utilidades.Util_Form.convertFloat("0", false);
                        oCorteE.Tipo = values[7];
                        //< !--Alicuotas IVA->ID 3 = 0 % | ID 4 = 10.5 % | ID 5 = 21 % | ID 6 = 27 % -->
                        oCorteE.IdAlicuotaIva = Convert.ToInt32(values[8]);
                        oCorteE.AlicuotaIva = Utilidades.Util_Form.convertFloat(values[9], false);
                        oCorteE.PuntoStock = Convert.ToInt32(values[10]);
                        oCorteE.IngresoRapidoEmbutido = false;
                        oCorteE.Pesable = tipoVenta == "P" ? true : false;
                        oCorteE.EnCierreStock = true;
                        oCorteE.Habilitado = true;
                        oCorteE.independiente = true ? 1 : 0;
                        oCorteE.CorteMaestro = null;
                        oCorteE.Porcentaje = 100;
                        oCorteE.porcentajeHueso = 0;
                        oCorteE.desvioEstandar = 0;

                        oCorteN.addOrEditCorte(oCorteE);
                    }

                    if (true)
                    {
                        MessageBox.Show("La importación se realizó correctamente.\n\n", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        mostrarMensajeExport = false;
                    }
                    
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al importar lista de precios excel automaticamente.\n\n" + ex.Message, "", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnBuscarCliente_Click(object sender, EventArgs e)
        {
            buscarMarca();
        }

        private void buscarMarca()
        {
            formBuscarPersona frmBuscarPersona = new formBuscarPersona();
            frmBuscarPersona.Show(this);
        }

        public void EnviarPersona(Entidades.Persona persona)
        {
            this.txtMarca.Text = persona.Identificacion;
            idMarca = persona.idPersona;
            //cargarGrilla();
            filtarGrilla();
        }

        private void btnLimpiar_Click(object sender, EventArgs e)
        {
            if (idMarca != -1)//validacion para evitar conexion a la BD
            {
                idMarca = -1;
                txtMarca.Text = "TODAS";
                filtarGrilla();
            }
        }

        public void ExportarDataTableAExcel()
        {
            try
            {
                if (dtCortesFiltrado == null || dtCortesFiltrado.Rows.Count == 0)
                {
                    MessageBox.Show("No hay datos para exportar");
                    return;
                }
                // Crear el formulario para pedir el nombre del archivo
                string nombreArchivo = MostrarDialogoNombreArchivo();

                //si es null se aborta la accion
                if (nombreArchivo == null)
                    return;

                // Establecer el contexto de la licencia para evitar la excepción
                ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;

                DataTable dataTable = dtCortesFiltrado;// oCorteN.lista_precios();

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

                    int indexCol = 1;
                    // Agregar encabezados
                    for (int i = 0; i < dataTable.Columns.Count; i++)
                    {
                        //codigo corte   precioKg efectivo    debito credito Qr Transf
                        if (dataTable.Columns[i].ColumnName.Equals("codigo") || dataTable.Columns[i].ColumnName.Equals("corte") ||
                            dataTable.Columns[i].ColumnName.Equals("precioKg") || dataTable.Columns[i].ColumnName.Equals("efectivo") ||
                            dataTable.Columns[i].ColumnName.Equals("debito") || dataTable.Columns[i].ColumnName.Equals("credito") ||
                            dataTable.Columns[i].ColumnName.Equals("Qr") || dataTable.Columns[i].ColumnName.Equals("Transf"))
                        {
                            hoja.Cells[1, indexCol++].Value = dataTable.Columns[i].ColumnName;                           
                        }
                    }
                                        
                    // Agregar datos
                    for (int i = 0; i < dataTable.Rows.Count; i++)
                    {
                        indexCol = 1;
                        for (int j = 0; j < dataTable.Columns.Count; j++)
                        {
                            if (dataTable.Columns[j].ColumnName.Equals("codigo") || dataTable.Columns[j].ColumnName.Equals("corte") ||
                                dataTable.Columns[j].ColumnName.Equals("precioKg") || dataTable.Columns[j].ColumnName.Equals("efectivo") ||
                                 dataTable.Columns[j].ColumnName.Equals("debito") || dataTable.Columns[j].ColumnName.Equals("credito") ||
                                dataTable.Columns[j].ColumnName.Equals("Qr") || dataTable.Columns[j].ColumnName.Equals("Transf"))
                                hoja.Cells[i + 2, indexCol++].Value = dataTable.Rows[i][j];
                        }
                    }
                    //// Agregar encabezados
                    //for (int i = 0; i < dataTable.Columns.Count; i++)
                    //{
                    //    hoja.Cells[1, i + 1].Value = dataTable.Columns[i].ColumnName;
                    //}

                    //// Agregar datos
                    //for (int i = 0; i < dataTable.Rows.Count; i++)
                    //{
                    //    for (int j = 0; j < dataTable.Columns.Count; j++)
                    //    {
                    //        hoja.Cells[i + 2, j + 1].Value = dataTable.Rows[i][j];
                    //    }
                    //}

                    // Guardar el archivo
                    excel.Save();
                    if (mostrarMensajeExport)
                    {
                        MessageBox.Show("La exportación se realizó correctamente.\n\n", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        mostrarMensajeExport = false;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al exportar lista de precios excel automaticamente.\n\n"+ex.Message,"",MessageBoxButtons.OK,MessageBoxIcon.Error);
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

            System.Windows.Forms.Label lblNombre = new System.Windows.Forms.Label
            {
                Text = "Ingrese el nombre del archivo:",
                Top = 10,
                Left = 10,
                Width = 360
            };

            System.Windows.Forms.TextBox txtNombre = new System.Windows.Forms.TextBox
            {
                Text = "ListaPrecio_" + DateTime.Today.ToShortDateString().Replace('/','-'),
                Top = 40,
                Left = 10,
                Width = 360
            };

            System.Windows.Forms.Button btnAceptar = new System.Windows.Forms.Button
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
