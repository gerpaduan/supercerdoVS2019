using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Printing;
using ZXing;
using ZXing.Common;
using ZXing.Rendering;
using System.Windows.Forms;

namespace Utilidades
{
    public class GenerarCodigoBarra
    {
        public void Main()
        {
            PrintDocument printDoc = new PrintDocument();
            PrintPreviewDialog printPreviewDialog = new PrintPreviewDialog(); ;
            printDoc.PrinterSettings.PrinterName = @"\\OficinaSM\Xprinter";

            // Crear un tamaño personalizado de página para un rollo de 57 mm de ancho
            PaperSize customPaperSize = new PaperSize("Rollo 57mm", 264, 3000); // Ancho = 224 décimas de pulgada (57 mm), Alto = 3000 décimas (p. ej., 300 mm)
            printDoc.DefaultPageSettings.PaperSize = customPaperSize;

            // Establecer márgenes si es necesario
            printDoc.DefaultPageSettings.Margins = new Margins(10, 10, 10, 10); // Márgenes de 10 píxeles

            // Configurar la orientación si es necesario
            printDoc.DefaultPageSettings.Landscape = false; // Si la orientación es vertical

            // Configurar el evento de impresión
            printDoc.PrintPage += new PrintPageEventHandler(PrintPage);

            printPreviewDialog.Document = printDoc;
            printPreviewDialog.ShowDialog();
            // Imprimir el documento
            printDoc.Print();

        }
        // Método para manejar la impresión
        private void PrintPage(object sender, PrintPageEventArgs e)
        {
            Graphics g = e.Graphics;

            // Aquí puedes dibujar lo que deseas en la página
            g.DrawString("Este es un ticket de ejemplo.", new Font("Arial", 12), Brushes.Black, new PointF(10, 10));

            // Si deseas continuar con varias páginas, puedes establecer e.HasMorePages en true
            // e.HasMorePages = true;
            string code39Code = "0123456789012";  // Texto para el código de barras Code 39
            string productName = "Café Premium"; // Nombre del producto
            string price = "$10.99";            // Precio
            string weight = "1.0 kg";           // Peso

            //Crear el escritor de código de barras(usando ZXing.Net)
            BarcodeWriter barcodeWriter = new BarcodeWriter
            {
                //Format = BarcodeFormat.CODE_39,
                Format = BarcodeFormat.UPC_E,
                Options = new ZXing.Common.EncodingOptions
                {
                    Width = 200,   // Ancho del código de barras
                    Height = 100,  // Alto del código de barras
                }
            };

            // Generar la imagen del código de barras
            Bitmap barcodeBitmap = barcodeWriter.Write(code39Code);

            // Dibujar el código de barras
            e.Graphics.DrawImage(barcodeBitmap, 10, 10);
        }
        static void PrintLabel(object sender, PrintPageEventArgs e)
        {
        }
    }

}
