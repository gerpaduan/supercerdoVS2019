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

            //return;

            //PrintDocument printDoc = new PrintDocument();
            //PrintPreviewDialog printPreviewDialog = new PrintPreviewDialog(); ;

            //// Ajusta con el nombre exacto de tu impresora térmica o de etiquetas
            //// 
            //printDoc.PrinterSettings.PrinterName = @"\\OficinaSM\Xprinter";
            ////printDoc.PrinterSettings.PrinterName = @"\\DESKTOP-69ORC98\Quo";
            ////printDoc.PrinterSettings.PrinterName = @"\\OficinaSM\QuoPrint_Systel";
            
            //// Verificar si la impresora está disponible
            //if (!printDoc.PrinterSettings.IsValid)
            //{
            //    Console.WriteLine("La impresora no está disponible.");
            //    return;
            //}

            //// Evento de impresión
            ////printDoc.PrintPage += new PrintPageEventHandler(PrintLabel);

            //printDoc.PrintPage += (sender, e) => {
            //    BarcodeWriter writer = new BarcodeWriter
            //    {
            //        Format = BarcodeFormat.CODE_128,
            //        Options = new ZXing.Common.EncodingOptions { Width = 300, Height = 100 }
            //    };

            //    Image barcodeImage = writer.Write("123456");
            //    e.Graphics.DrawImage(barcodeImage, 10, 10);
            //};

            //printPreviewDialog.Document = printDoc;
            //printPreviewDialog.ShowDialog();
            ////printDoc.Print();
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
             string code39Code = "PE12597";  // Texto para el código de barras Code 39
            string productName = "Café Premium"; // Nombre del producto
            string price = "$10.99";            // Precio
            string weight = "1.0 kg";           // Peso

            // Fuente para el código de barras (asegúrate de tener una fuente de Code 39 instalada)
            //Font barcodeFont = new Font("IDAutomationHC39M", 36, FontStyle.Regular);  // Fuente de código de barras

            //// Dibujar el código de barras como texto
            //e.Graphics.DrawString("*" + code39Code + "*", barcodeFont, Brushes.Black, 10, 10);

            //// Agregar nombre del producto debajo del código de barras
            //Font font = new Font("Arial", 8, FontStyle.Bold);
            //e.Graphics.DrawString(productName, font, Brushes.Black, 10, 50);

            //// Agregar precio
            //e.Graphics.DrawString("Precio: " + price, font, Brushes.Black, 10, 65);

            //// Agregar peso
            //e.Graphics.DrawString("Peso: " + weight, font, Brushes.Black, 10, 80);

            //string code39Code = "CODE39";  // Texto para el código de barras Code 39
            //string productName = "Café Premium"; // Nombre del producto
            //string price = "$10.99";            // Precio
            //string weight = "1.0 kg";           // Peso

            // Crear el escritor de código de barras (usando ZXing.Net)
            //BarcodeWriter barcodeWriter = new BarcodeWriter
            //{
            //    Format = BarcodeFormat.CODE_39,
            //    //Format = BarcodeFormat.CODE_128,
            //    Options = new ZXing.Common.EncodingOptions
            //    {
            //        Width = 200,   // Ancho del código de barras
            //        Height = 100,  // Alto del código de barras
            //    }
            //};

            //// Generar la imagen del código de barras
            //Bitmap barcodeBitmap = barcodeWriter.Write(code39Code);

            //// Dibujar el código de barras
            //e.Graphics.DrawImage(barcodeBitmap, 10, 10);

            //// Agregar nombre del producto debajo del código de barras
            //Font font = new Font("Arial", 8, FontStyle.Bold);
            //e.Graphics.DrawString(productName, font, Brushes.Black, 10, 120);

            //// Agregar precio
            //e.Graphics.DrawString("Precio: " + price, font, Brushes.Black, 10, 135);

            //// Agregar peso
            //e.Graphics.DrawString("Peso: " + weight, font, Brushes.Black, 10, 150);

            //string ean13Code = "123456789012";  // Código EAN-13
            //string productName = "Café Premium"; // Nombre del producto
            //string price = "$10.99";            // Precio
            //string weight = "1.0 kg";           // Peso

            //// Generar el código de barras
            //BarcodeWriter writer = new BarcodeWriter
            //{
            //    Format = BarcodeFormat.CODE_39,
            //    Options = new EncodingOptions
            //    {
            //        Width = 206, // Ajuste para que el código de barras se vea correctamente
            //        Height = 100,
            //        Margin = 0
            //    },
            //    Renderer = new BitmapRenderer()
            //};

            //// Generar la imagen del código de barras
            //Bitmap barcodeBitmap = writer.Write(ean13Code);

            //// Configuración del área de impresión (55mm x 44mm)
            //int labelWidthPx = (int)(55 * 3.75);  // Conversión mm → píxeles (55mm)
            //int labelHeightPx = (int)(44 * 3.75); // Conversión mm → píxeles (44mm)

            //// Ajustar el área de impresión
            //RectangleF labelArea = new RectangleF(10, 10, labelWidthPx, labelHeightPx);

            //// Dibujar un borde de la etiqueta (opcional)
            //e.Graphics.DrawRectangle(Pens.Black, Rectangle.Round(labelArea));

            //// Agregar nombre del producto debajo del código de barras
            //Font font = new Font("Arial", 8, FontStyle.Bold);


            //// Dibujar el código de barras centrado en la etiqueta
            //int x = (int)(labelArea.Left + (labelWidthPx - barcodeBitmap.Width) / 2);
            //int y = (int)(labelArea.Top + (labelHeightPx - barcodeBitmap.Height) / 2);
            //e.Graphics.DrawString("sigue cod barra\n", font, Brushes.Black, x, y);
            //e.Graphics.DrawImage(barcodeBitmap, x, y);

            //e.Graphics.DrawString(productName, font, Brushes.Black, 10, y + barcodeBitmap.Height + 5);

            //// Agregar precio
            //e.Graphics.DrawString("Precio: " + price, font, Brushes.Black, 10, y + barcodeBitmap.Height + 20);

            //// Agregar peso
            //e.Graphics.DrawString("Peso: " + weight, font, Brushes.Black, 10, y + barcodeBitmap.Height + 35);

            ////// Agregar el código EAN-13 debajo del precio (si lo necesitas)
            ////e.Graphics.DrawString("EAN: " + ean13Code, font, Brushes.Black, 10, y + barcodeBitmap.Height + 50);
        }
    }

}
