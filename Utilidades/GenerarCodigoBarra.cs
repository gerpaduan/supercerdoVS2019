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
        public static bool ValidarEAN(string codigo)
        {
            if (string.IsNullOrWhiteSpace(codigo))
                return false;

            if (!(codigo.Length == 8 || codigo.Length == 13))
                return false;

            if (!codigo.All(char.IsDigit))
                return false;

            int length = codigo.Length;
            int checkDigit = int.Parse(codigo[length - 1].ToString());
            int sum = 0;

            for (int i = 0; i < length - 1; i++)
            {
                int digit = int.Parse(codigo[i].ToString());

                if (length == 13)
                {
                    // EAN-13: posiciones pares (índice impar) * 3
                    sum += (i % 2 == 0) ? digit : digit * 3;
                }
                else if (length == 8)
                {
                    // EAN-8: posiciones impares (índice par) * 3
                    sum += (i % 2 == 0) ? digit * 3 : digit;
                }
            }

            int calculated = (10 - (sum % 10)) % 10;
            return calculated == checkDigit;
        }


        public void Main()
        {
            PrintDocument pd = new PrintDocument();
            pd.PrinterSettings.PrinterName = @"\\OficinaSM\quo";

            pd.PrintPage += (sender, e) =>
            {
                using (Font font = new Font("Arial", 10))
                using (Pen pen = new Pen(Color.Black, 2))
                {
                    e.Graphics.DrawString("Producto X", font, Brushes.Black, 10, 10);

                    using (Bitmap bmp = GenerateBarcode("123456789012"))
                    {
                        e.Graphics.DrawImage(bmp, 10, 30);
                    }
                }
            };

            PrintPreviewDialog printPreviewDialog = new PrintPreviewDialog(); ;
            printPreviewDialog.Document = pd;
            printPreviewDialog.ShowDialog();
            // Imprimir el documento
            pd.Print();

            pd.Print();


           // PrintDocument printDoc = new PrintDocument();
           // PrintPreviewDialog printPreviewDialog = new PrintPreviewDialog(); ;
           // printDoc.PrinterSettings.PrinterName = @"\\OficinaSM\Xprinter";
           // printDoc.PrinterSettings.PrinterName = @"\\OficinaSM\quo";

           // //Crear un tamaño personalizado de página para un rollo de 57 mm de ancho
           //PaperSize customPaperSize = new PaperSize("Rollo 57mm", 264, 3000); // Ancho = 224 décimas de pulgada (57 mm), Alto = 3000 décimas (p. ej., 300 mm)
           // printDoc.DefaultPageSettings.PaperSize = customPaperSize;

           // //Establecer márgenes si es necesario
           // printDoc.DefaultPageSettings.Margins = new Margins(10, 10, 10, 10); // Márgenes de 10 píxeles

           // // Configurar la orientación si es necesario
           // printDoc.DefaultPageSettings.Landscape = false; // Si la orientación es vertical

           // // Configurar el evento de impresión
           // printDoc.PrintPage += new PrintPageEventHandler(PrintPage);

           // printPreviewDialog.Document = printDoc;
           // printPreviewDialog.ShowDialog();
           // // Imprimir el documento
           // printDoc.Print();

        }

        static Bitmap GenerateBarcode(string data)
        {
            Zen.Barcode.Code128BarcodeDraw barcode = Zen.Barcode.BarcodeDrawFactory.Code128WithChecksum;
            return new Bitmap(barcode.Draw(data, 50));
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
                Format = BarcodeFormat.EAN_13,
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
