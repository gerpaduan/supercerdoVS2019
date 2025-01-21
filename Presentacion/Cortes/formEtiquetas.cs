using iTextSharp.text.pdf;
using iTextSharp.text;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Presentacion.Cortes
{
    public partial class formEtiquetas : Form
    {
        public DataTable dtCortes = new DataTable();
        DataTable dtCortesFiltrado;
        // Crear una lista para almacenar los valores de idCorte seleccionados
        List<int> idsSeleccionados = new List<int>();
        public formEtiquetas()
        {
            InitializeComponent(); this.Icon = Properties.Resources.CarniSys_ICONO;
        }

        private void formEtiquetas_Load(object sender, EventArgs e)
        {
            // Desactivar la generación automática de columnas
            grillaCortes.AutoGenerateColumns = false;
            grillaCortes.DataSource = dtCortes;
            grillaCortes.ReadOnly = false;
            foreach (DataGridViewColumn columna in grillaCortes.Columns)
            {
                if (columna is DataGridViewCheckBoxColumn)
                {
                    columna.ReadOnly = false; // Asegurarte de que sea editable
                }
                else
                {
                    columna.ReadOnly = true;
                }
            }
        }

        public void filtarGrilla()
        {
            dtCortesFiltrado = dtCortes.Clone();

            // Verificar si el texto ingresado en txtDescripcion es un número
            bool esNumero = long.TryParse(txtBuscarCorte.Text, out long numero);

            // Filtrar dependiendo si es número o texto
            if (esNumero)
            {
                // Filtrar por la columna "codigo" si es un número
                var filasFiltradas = dtCortes.AsEnumerable()
                    .Where(row => row.Field<long>("codigo") == numero)
                    .ToList();

                // Agregar las filas filtradas al DataTable
                foreach (var fila in filasFiltradas)
                {
                    dtCortesFiltrado.ImportRow(fila);
                }
            }
            else
            {
                // Filtrar por la columna "cortes" si es texto
                var filasFiltradas = dtCortes.AsEnumerable()
                    .Where(row => row.Field<string>("corte").ToLower().Contains(txtBuscarCorte.Text.ToLower()))
                    .ToList();

                // Agregar las filas filtradas al DataTable
                foreach (var fila in filasFiltradas)
                {
                    dtCortesFiltrado.ImportRow(fila);
                }
            }

            grillaCortes.DataSource = dtCortesFiltrado;
            txtCantItems.Text = idsSeleccionados.Count.ToString();


            foreach (DataGridViewRow fila in grillaCortes.Rows)
            {
                int idCorteSelected = Convert.ToInt32(fila.Cells["idCorte"].Value);
                fila.Cells["seleccionar"].Value = idsSeleccionados.Contains(idCorteSelected);
            }
        }

        private void txtBuscarCorte_TextChanged(object sender, EventArgs e)
        {
            filtarGrilla();
        }

        private void grillaCortes_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            // Verificar que el clic no sea en el encabezado ni en una fila inexistente
            if (e.RowIndex >= 0 && e.ColumnIndex >= 0)
            {
                // Obtener la fila actual
                DataGridViewRow fila = grillaCortes.Rows[e.RowIndex];

                // Verificar si la columna checkbox existe
                if (grillaCortes.Columns["seleccionar"] is DataGridViewCheckBoxColumn)
                {
                    int idCorteSelected = Convert.ToInt32(fila.Cells["idCorte"].Value);
                    bool valorActual = Convert.ToBoolean(fila.Cells["seleccionar"].Value);
                    fila.Cells["seleccionar"].Value = !valorActual; // Invertir el valor
                    if (fila.Cells["seleccionar"].Value != null && (bool)fila.Cells["seleccionar"].Value)
                        idsSeleccionados.Add(idCorteSelected);
                    else
                        idsSeleccionados.Remove(idCorteSelected);   
                }
                txtCantItems.Text = idsSeleccionados.Count.ToString();
            }
        }


        private void grillaCortes_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            // Verificar que el clic sea en una columna de tipo checkbox
            if (e.ColumnIndex >= 0 && grillaCortes.Columns[e.ColumnIndex] is DataGridViewCheckBoxColumn)
            {
                grillaCortes_CellDoubleClick(sender, e);
            }
        }

        private void btnSeleccionarTodos_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow fila in grillaCortes.Rows)
            {
                int idCorteSelected = Convert.ToInt32(fila.Cells["idCorte"].Value);
                fila.Cells["seleccionar"].Value = true;
                idsSeleccionados.Add(idCorteSelected);
            }
        }

        private void btnQuitarTodos_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow fila in grillaCortes.Rows)
            {
                int idCorteSelected = Convert.ToInt32(fila.Cells["idCorte"].Value);
                fila.Cells["seleccionar"].Value = false;
                idsSeleccionados.Remove(idCorteSelected);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (idsSeleccionados.Count == 0)
            {
                MessageBox.Show("No se ha seleccionado ningún producto.");
                return;
            }
            etiquetaPDF();
        }
        private void etiquetaPDF()
        {
            //para cargar todos los datos en la grilla
            txtBuscarCorte.Text = "";
            dtCortesFiltrado = dtCortes.Clone();

            foreach (int item in idsSeleccionados)
            {
                var filasFiltradas = dtCortes.AsEnumerable()
                    .Where(row => row.Field<int>("idCorte") == item)
                    .ToList();

                // Agregar las filas filtradas al DataTable
                foreach (var fila in filasFiltradas)
                {
                    dtCortesFiltrado.ImportRow(fila);
                }
            }

            // Ruta donde se guardará el archivo PDF
            string rutaArchivo = @"C:\CarniSys_Docs\Etiquetas.pdf";

            // Crear un documento de  A4
            Document document = new Document(PageSize.A4);

            // Verificar y crear la carpeta si no existe
            string carpeta = Path.GetDirectoryName(rutaArchivo);
            if (!Directory.Exists(carpeta))
            {
                Directory.CreateDirectory(carpeta);
            }

            // Crear un escritor para el archivo PDF
            PdfWriter writer = PdfWriter.GetInstance(document, new FileStream(rutaArchivo, FileMode.Create));

            // Abrir el documento para escribir
            document.Open();

            // Configuración de las etiquetas
            float etiquetaAncho = 60f; // Ancho en mm
            float etiquetaAlto = 35f;  // Alto en mm
            float margenIzquierdo = 15f; // Márgen izquierdo en mm (puedes ajustar este valor)
            float margenSuperior = 10f; // Márgen superior en mm
            float espacioHorizontal = 0.5f; // Espacio entre etiquetas (cero en este caso)
            float espacioVertical = 0.1f;   // Espacio entre filas (cero en este caso)

            // Conversión de mm a puntos (1 mm = 2.8346 puntos)
            etiquetaAncho *= 2.8346f;
            etiquetaAlto *= 2.8346f;
            margenIzquierdo *= 2.8346f;
            margenSuperior *= 2.8346f;
            espacioHorizontal *= 2.8346f;
            espacioVertical *= 2.8346f;

            // Dimensiones de la hoja A4
            float hojaAncho = PageSize.A4.Width;
            float hojaAlto = PageSize.A4.Height;

            // Calcular el número de etiquetas por fila y columna
            int etiquetasPorFila = (int)((hojaAncho + espacioHorizontal) / (etiquetaAncho + espacioHorizontal));
            int etiquetasPorColumna = (int)((hojaAlto + espacioVertical) / (etiquetaAlto + espacioVertical));
            int etiquetasPorHoja = etiquetasPorFila * etiquetasPorColumna;

            // Datos de ejemplo

            int totalProductos = dtCortesFiltrado.Rows.Count;
            string[] nombresProductos = new string[totalProductos];// { "123456789*123456789*", "ABCDEFGHI0ABCDEFGHI0ABCDEFGHI", "ABCDEFGHI0ABCDEFGHI0ABCDEFGHI0", "12 CARACTERES", "Crema de almejas estilo Nueva Inglaterra\r\n", "Sandía", "Limones", "Mangos" };
            decimal[] precios = new decimal[totalProductos]; ;// { 100000.50m, 2.30m, 1.20m, 0.80m, 2.50m, 3.10m, 0.90m, 1.80m };
            string[] fechaActual = new string[totalProductos]; ; DateTime.Now.ToString("dd/MM/yyyy");
            iTextSharp.text.Font[] fontProduct = new iTextSharp.text.Font[totalProductos];
            iTextSharp.text.Font[] fontPrecioProd = new iTextSharp.text.Font[totalProductos];

            iTextSharp.text.Font fontProducto1 = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 15, iTextSharp.text.Font.NORMAL);
            iTextSharp.text.Font fontProducto2 = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 13, iTextSharp.text.Font.NORMAL);
            iTextSharp.text.Font fontProducto3 = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 12, iTextSharp.text.Font.NORMAL);

            iTextSharp.text.Font fontPrecio1 =new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 30, iTextSharp.text.Font.BOLD);
            iTextSharp.text.Font fontPrecio2 = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 27, iTextSharp.text.Font.BOLD);

            int longMaxFuente1 = 25;
            int longMaxFuente2 = 40;
            for (int i = 0; i < dtCortesFiltrado.Rows.Count; i++)
            {
                int longTextoProd = dtCortesFiltrado.Rows[i]["corte"].ToString().Length;

                if (longTextoProd > longMaxFuente2)
                {
                    nombresProductos[i] = dtCortesFiltrado.Rows[i]["corte"].ToString();//.Substring(0, longMaxFuente2);
                    fontProduct[i] = fontProducto3;
                }
                else
                {
                    if (longTextoProd > longMaxFuente1)
                    {
                        nombresProductos[i] = dtCortesFiltrado.Rows[i]["corte"].ToString();//.Substring(0, longMaxFuente1);
                        fontProduct[i] = fontProducto2;
                    }
                    else
                    {
                        nombresProductos[i] = dtCortesFiltrado.Rows[i]["corte"].ToString();
                        fontProduct[i] = fontProducto1;
                    }
                }

                precios[i] = Convert.ToDecimal(dtCortesFiltrado.Rows[i]["precioKg"].ToString());
                fontPrecioProd[i] = precios[i] > 99999 ? fontPrecio2 : fontPrecio1;
                //Numérico es difente a ese intervalo se saltea 1-99.997 
                fechaActual[i] = "COD: " + dtCortesFiltrado.Rows[i]["codigo"].ToString() + " \t | \t " + DateTime.Now.Date.ToShortDateString();

            }


            int productoIndex = 0;
            while (productoIndex < totalProductos)
            {
                // Agregar una nueva hoja si es necesario
                if (productoIndex > 0)
                {
                    document.NewPage();
                }

                // Crear las etiquetas
                for (int fila = 0; fila < etiquetasPorColumna; fila++)
                {
                    for (int columna = 0; columna < etiquetasPorFila; columna++)
                    {
                        if (productoIndex >= totalProductos)
                            break;

                        //// Calcular posición X y Y para la etiqueta
                        float x = margenIzquierdo + columna * etiquetaAncho;
                        float y = hojaAlto - margenSuperior - (fila * etiquetaAlto);

                        // Crear un nuevo contenido para la etiqueta
                        PdfPTable tablaEtiqueta = new PdfPTable(1)
                        {
                            TotalWidth = etiquetaAncho,
                            LockedWidth = true
                        };
                        // Crear la fuente para el nombre del producto (normal)
                        iTextSharp.text.Font fontProducto = fontProduct[productoIndex]; //new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 15, iTextSharp.text.Font.NORMAL);
                        // Crear la fuente para el precio (negrita y más grande)
                        iTextSharp.text.Font fontPrecio = fontPrecioProd[productoIndex];// new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 30, iTextSharp.text.Font.BOLD);
                        iTextSharp.text.Font fuenteFecha = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 6, iTextSharp.text.Font.ITALIC, BaseColor.DARK_GRAY);

                        // Primera fila: Nombre del producto
                        PdfPCell celdaProducto = new PdfPCell(new Phrase($"{nombresProductos[productoIndex]}", fontProducto))
                        {
                            Border = iTextSharp.text.Rectangle.NO_BORDER,
                            HorizontalAlignment = Element.ALIGN_CENTER,
                            VerticalAlignment = Element.ALIGN_MIDDLE,
                            MinimumHeight = etiquetaAlto / 2.7f, // Altura de 1/3 de la etiqueta
                            FixedHeight = etiquetaAlto / 2.7f
                        };
                        tablaEtiqueta.AddCell(celdaProducto);

                        // Segunda fila: Precio
                        PdfPCell celdaPrecio = new PdfPCell(new Phrase($"${precios[productoIndex]:#,0.00}", fontPrecio))
                        {
                            Border = iTextSharp.text.Rectangle.NO_BORDER,
                            HorizontalAlignment = Element.ALIGN_CENTER,
                            VerticalAlignment = Element.ALIGN_MIDDLE,
                            MinimumHeight = etiquetaAlto / 2.7f
                        };
                        tablaEtiqueta.AddCell(celdaPrecio);

                        // Tercera fila: Fecha
                        PdfPCell celdaFecha = new PdfPCell(new Phrase($"{fechaActual[productoIndex]}", fuenteFecha))
                        {
                            Border = iTextSharp.text.Rectangle.NO_BORDER,
                            HorizontalAlignment = Element.ALIGN_CENTER,
                            VerticalAlignment = Element.ALIGN_MIDDLE,
                            MinimumHeight = etiquetaAlto / 5
                        };
                        tablaEtiqueta.AddCell(celdaFecha);

                        // Crear un borde negro alrededor de la etiqueta
                        PdfPCell celdaConBorde = new PdfPCell(tablaEtiqueta)
                        {
                            Border = iTextSharp.text.Rectangle.BOX,
                            BorderWidth = 0.5f, // Grosor del borde
                            Padding = 2f // Espaciado interno
                        };

                        // Crear una tabla contenedora para aplicar el borde
                        PdfPTable tablaConBorde = new PdfPTable(1)
                        {
                            TotalWidth = etiquetaAncho - 2,
                            LockedWidth = true
                        };
                        tablaConBorde.AddCell(celdaConBorde);

                        // Posicionar la tabla en el documento
                        tablaConBorde.WriteSelectedRows(0, -1, x, y, writer.DirectContent);

                        // Avanzar al siguiente producto
                        productoIndex++;
                    }
                }
            }

            // Cerrar el documento
            document.Close();

            try
            {
                Process.Start(new ProcessStartInfo(rutaArchivo) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                //Console.WriteLine("Error al abrir el archivo PDF: " + ex.Message);
            }
        }
    }
}
