using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Runtime.InteropServices;
using Org.BouncyCastle.Utilities;

namespace Presentacion.Ticket
{
    #region Clase para generar ticket
    // La clase "CreaTicket" tiene varios metodos para imprimir con diferentes formatos (izquierda, derecha, centrado, desripcion precio,etc), a
    // continuacion se muestra el metodo con ejemplo de parametro que acepta, longitud maxima y un ejemplo de como imprimira, esta clase esta 
    // basada en una impresora Epson de matriz de puntos con impresion maxima de 40 caracteres por renglon
    // METODO                                      MAX_LONG                        EJEMPLOS
    //--------------------------------------------------------------------------------------------------------------------------
    // TextoIzquierda("Empleado 1")                    40                      Empleado 1      
    // TextoDerecha("Caja 1")                          40                                                        Caja 1
    // TextoCentro("Ticket")                           40                                         Ticket   
    // TextoExtremos("Fecha 6/1/2011","Hora:13:25")     18 y 18                 Fecha 6/1/2011                Hora:13:25
    // EncabezadoVenta()                                n/a                     Articulo        Can    P.Unit    Importe
    // LineasGuion()                                    n/a                     ----------------------------------------
    // AgregaArticulo("Aspirina","2",45.25,90.5)        16,3,10,11              Aspirina          2    $45.25     $90.50
    // LineasTotales()                                  n/a                                                ----------
    // AgregaTotales("Subtotal",235.25)                 25 y 15                Subtotal                         $235.25
    // LineasAsterisco()                                n/a                     ****************************************
    // LineasIgual()                                    n/a                     ========================================
    // CortaTicket()
    // AbreCajon()
    public class CreaTicket
    {
        public bool imprimir = false;
        string ticket = "";
        string parte1, parte2;
        string impresora = ConfigurationManager.AppSettings["impresora"].ToString();//"Epson Stylus COLOR 670 ESC/P 2 (Copiar 1)";//"\\\\FARMACIA-PVENTA\\Generic / Text Only"; // nombre exacto de la impresora como esta en el panel de control
        int max, cort;
        int cantMaxChar = 32;
        public void LineasEnBlanco(int cantLineas)
        {
            ticket += "\n";
            for (int i = 1; i < cantLineas; i++)
            {
                ticket += "\n";
            }
            //RawPrinterHelper.SendStringToPrinter(impresora, ticket, imprimir); // imprime linea
        }
        public void LineasGuion()
        {
            ticket += "--------------------\n";   // agrega lineas separadoras -
            ////RawPrinterHelper.SendStringToPrinter(impresora, ticket, imprimir); // imprime linea
        }
        public void LineasAsterisco()
        {
            ticket += "********************\n";   // agrega lineas separadoras *
            //RawPrinterHelper.SendStringToPrinter(impresora, ticket, imprimir); // imprime linea
        }
        public void LineasIgual()
        {
            ticket += "====================\n";   // agrega lineas separadoras =
            //RawPrinterHelper.SendStringToPrinter(impresora, ticket, imprimir); // imprime linea
        }
        public void LineasTotales()
        {
            ticket += "         -----------\n"; ;   // agrega lineas de total
            //RawPrinterHelper.SendStringToPrinter(impresora, ticket, imprimir); // imprime linea
        }
        public void EncabezadoVenta()
        {
            ticket += "Articulo        Can    P.Unit    Importe\n";   // agrega lineas de  encabezados
            //RawPrinterHelper.SendStringToPrinter(impresora, ticket, imprimir); // imprime texto
        }
        public void GraciasPorSuCompra()
        {
            string texto = "**Gracias por su compra**\n";
            TextoCentro(texto);
            ////RawPrinterHelper.SendStringToPrinter(impresora, ticket, imprimir); // imprime linea
        }
        public void NoValidoComoFactura()
        {
            string texto = "-No valido como Factura-\n";
            TextoCentro(texto);
            //RawPrinterHelper.SendStringToPrinter(impresora, ticket, imprimir); // imprime linea
        }
        public void TextoIzquierda(string par1)                          // agrega texto a la izquierda
        {
            max = par1.Length;
            if (max > cantMaxChar)                                 // **********
            {
                cort = max - cantMaxChar;
                parte1 = par1.Remove(cantMaxChar, cort);        // si es mayor que cantMaxChar caracteres, lo corta
            }
            else { parte1 = par1; }                      // **********
            ticket += parte1 + "\n";
            //RawPrinterHelper.SendStringToPrinter(impresora, ticket, imprimir); // imprime texto
        }
        public void TextoDerecha(string par1)
        {
            ticket += "";
            max = par1.Length;
            if (max > cantMaxChar)                                 // **********
            {
                cort = max - cantMaxChar;
                parte1 = par1.Remove(cantMaxChar, cort);           // si es mayor que cantMaxChar caracteres, lo corta
            }
            else { parte1 = par1; }                      // **********
            max = cantMaxChar - par1.Length;                     // obtiene la cantidad de espacios para llegar a cantMaxChar
            for (int i = 0; i < max; i++)
            {
                ticket += " ";                          // agrega espacios para alinear a la derecha
            }
            ticket += parte1 + "\n";                    //Agrega el texto
            
            //RawPrinterHelper.SendStringToPrinter(impresora, ticket, imprimir); // imprime texto
        }
        public void TextoCentro(string par1)
        {
            ticket += "";
            max = par1.Length;
            if (max > cantMaxChar)                                 // **********
            {
                cort = max - cantMaxChar;
                parte1 = par1.Remove(cantMaxChar, cort);          // si es mayor que cantMaxChar caracteres, lo corta
            }
            else { parte1 = par1; }                      // **********
            max = (int)(cantMaxChar - parte1.Length) / 2;         // saca la cantidad de espacios libres y divide entre dos
            for (int i = 0; i < max; i++)                // **********
            {
                ticket += " ";                           // Agrega espacios antes del texto a centrar
            }                                            // **********
            ticket += parte1 + "\n";
            //RawPrinterHelper.SendStringToPrinter(impresora, ticket, imprimir); // imprime texto
        }
        public void TextoExtremos(string par1, string par2)
        {
            max = par1.Length;
            if (max > 18)                                 // **********
            {
                cort = max - 18;
                parte1 = par1.Remove(18, cort);          // si par1 es mayor que 18 lo corta
            }
            else { parte1 = par1; }                      // **********
            ticket += parte1;                             // agrega el primer parametro
            max = par2.Length;
            if (max > 18)                                 // **********
            {
                cort = max - 18;
                parte2 = par2.Remove(18, cort);          // si par2 es mayor que 18 lo corta
            }
            else { parte2 = par2; }
            max = cantMaxChar - (parte1.Length + parte2.Length);
            for (int i = 0; i < max; i++)                 // **********
            {
                ticket += " ";                            // Agrega espacios para poner par2 al final
            }                                             // **********
            ticket += parte2+"\n";                     // agrega el segundo parametro al final
            
            //RawPrinterHelper.SendStringToPrinter(impresora, ticket, imprimir); // imprime texto
        }

        public void TextoMuchasLineas(string par1)                          // agrega texto a la izquierda
        {
            max = par1.Length;
            int nroVueltas = 1;
            parte1 = "";
            string tabulacion = "   ";
            for (int index = 0; index < max; index++)
            {
                //si se están imprimiendo mas de 5 lineas se corta
                if (nroVueltas > 5)
                {
                    parte1 += "...";
                    break;
                }
                if (index == (cantMaxChar * nroVueltas))
                {
                    nroVueltas++;
                    parte1 += "\n";
                    parte1 += tabulacion;
                }
                parte1 += par1[index].ToString();
            }
            ticket += parte1 + "\n";
            //RawPrinterHelper.SendStringToPrinter(impresora, ticket, imprimir); // imprime texto
        }

        public void AgregaTotales(string par1, double total)
        {
            ticket += par1;
            parte2 = total.ToString("F2");
            int espacios = cantMaxChar - parte2.Length;
            for (int i = par1.Length; i < espacios; i++)                // **********
            {
                ticket += " ";                           // Agrega espacios
            }
            ticket += parte2+"\n";
            //RawPrinterHelper.SendStringToPrinter(impresora, ticket, imprimir); // imprime texto
        }
        public void AgregaArticulo(string producto, double cant, double precio, double total)
        {
            string cantidad = cant.ToString("F3") + " x " + precio.ToString("F2");
            ticket += cantidad+"\n";

            int longProd = producto.Length;
            int maxCharProd = 22;
            if (longProd > maxCharProd)                                 
            {
                cort = longProd - maxCharProd;
                producto = producto.Remove(maxCharProd, cort);
                longProd = producto.Length;
            }
            ticket += producto;
            

            int longTotal = total.ToString("F2").Length;
            int espacios = cantMaxChar - longTotal;

            for (int i = longProd; i < espacios; i++)                
            {
                ticket += " ";                           
            }                                            
            ticket += total.ToString("F2") + "\n";
            //RawPrinterHelper.SendStringToPrinter(impresora, ticket, imprimir);

        }

        public void realizarImpresion()
        {
            RawPrinterHelper.SendStringToPrinter(impresora, ticket, imprimir);
        }
        static void PrintBarcode(string printerName, string barcodeData)
        {
            byte[] init = new byte[] { 0x1B, 0x40 }; // Inicializar impresora
            byte[] setHRI = new byte[] { 0x1D, 0x48, 0x02 }; // Mostrar caracteres debajo del código de barras
            byte[] setHeight = new byte[] { 0x1D, 0x68, 0x60 }; // Altura del código de barras (96)
            byte[] setWidth = new byte[] { 0x1D, 0x77, 0x03 }; // Ancho del código de barras
            byte[] selectBarcodeType = new byte[] { 0x1D, 0x6B, 0x49 }; // Código de barras tipo CODE128
            byte[] barcodeLength = new byte[] { (byte)(barcodeData.Length + 2) }; // Longitud del código de barras
            //byte[] startCode128 = new byte[] { 0x7B, 0x42 }; // Código de inicio para CODE128
            byte[] startCode128 = new byte[] { 0x1D, 0x6B, 0x04 }; // Cambia a CODE39
            byte[] barcodeBytes = Encoding.ASCII.GetBytes(barcodeData);
            byte[] lineFeed = new byte[] { 0x0A }; // Salto de línea

            byte[] command = Combine(init, setHRI, setHeight, setWidth, selectBarcodeType, barcodeLength, startCode128, barcodeBytes, lineFeed);

            if (RawPrinterHelper.SendBytesToPrintCodeBar(printerName, command, barcodeData))
                Console.WriteLine("Código de barras enviado a la impresora.");
            else
                Console.WriteLine("Error al enviar el código de barras.");
        }
        static byte[] Combine(params byte[][] arrays)
        {
            int length = 0;
            foreach (byte[] arr in arrays) length += arr.Length;
            byte[] result = new byte[length];
            int offset = 0;
            foreach (byte[] arr in arrays)
            {
                Buffer.BlockCopy(arr, 0, result, offset, arr.Length);
                offset += arr.Length;
            }
            return result;
        }
        static void TestPrint(string printerName)
        {
            string prueba = "Hola, esto es una prueba.\n\n";
            byte[] testMessage = Encoding.ASCII.GetBytes(prueba);


            if (RawPrinterHelper.SendBytesToPrintCodeBar(printerName, testMessage, prueba))
                Console.WriteLine("Mensaje enviado a la impresora.");
            else
                Console.WriteLine("Error al enviar mensaje a la impresora.");

            printerName = @"\\OficinaSM\Xprinter";
            string barcode = "123456789012";  // Código de barras de ejemplo

            // Enviar el código de barras a la impresora
            bool result = RawPrinterHelper.SendBarcodeToPrinter(printerName, barcode, true);
        }

        public void realizarImpresionCodigoBarra(string szString)
        {
            string szPrinterName = @"\\OficinaSM\Xprinter";// impresora;
            string printerName = "XPrinter"; // Cambia al nombre real de la impresora
            string barcodeData = "123456789012"; // Código de barras a imprimir

            //RawPrinterHelper.SendStringToPrinter(szPrinterName, barcodeData+"\n\n\n", imprimir);

            TestPrint(szPrinterName);
            //PrintBarcode(szPrinterName, barcodeData);

            //byte[] testMessage = Encoding.ASCII.GetBytes("Prueba de impresión\n\n");

            //IntPtr pUnmanagedBytes = Marshal.AllocCoTaskMem(testMessage.Length);
            //Marshal.Copy(testMessage, 0, pUnmanagedBytes, testMessage.Length);

            //bool exito = RawPrinterHelper.SendBytesToPrinter(szPrinterName, pUnmanagedBytes, testMessage.Length);

            //Marshal.FreeCoTaskMem(pUnmanagedBytes);

            //return;

            //// Configurar el tipo de código de barras (CODE128)
            //byte[] selectBarcodeType = new byte[] { 0x1D, 0x6B, 0x02 }; // GS k (08 = CODE128)

            //// Datos del código de barras (PE156 en formato CODE128)
            //byte[] barcodeData = Encoding.ASCII.GetBytes("PE156");

            //// Longitud del código de barras (requerido para CODE128 en algunas impresoras)
            //byte[] barcodeLength = new byte[] { (byte)barcodeData.Length };

            //// Salto de línea después del código de barras
            //byte[] newLine = new byte[] { 0x0A };

            //// Concatenar los datos en un solo array
            //byte[] printData = new byte[selectBarcodeType.Length + barcodeLength.Length + barcodeData.Length + newLine.Length];
            //selectBarcodeType.CopyTo(printData, 0);
            //barcodeLength.CopyTo(printData, selectBarcodeType.Length);
            //barcodeData.CopyTo(printData, selectBarcodeType.Length + barcodeLength.Length);
            //newLine.CopyTo(printData, selectBarcodeType.Length + barcodeLength.Length + barcodeData.Length);

            //// Convertir a puntero
            //IntPtr pUnmanagedBytes = Marshal.AllocCoTaskMem(printData.Length);
            //Marshal.Copy(printData, 0, pUnmanagedBytes, printData.Length);

            //// Enviar a la impresora
            //bool exito = RawPrinterHelper.SendBytesToPrinter(szPrinterName, pUnmanagedBytes, printData.Length);

            //// Liberar memoria
            //Marshal.FreeCoTaskMem(pUnmanagedBytes);


            //// Comando ESC/POS para imprimir un código de barras CODE128
            //byte[] datos = new byte[]
            //{
            //0x1D, 0x6B, 0x49, // ESC k - Seleccionar código de barras tipo CODE128
            //0x0C,             // Longitud del código
            //(byte)'1', (byte)'2', // Código de barras
            //0x0A              // Salto de línea
            //};

            //// Convertir el array de bytes a un puntero en memoria
            //IntPtr pUnmanagedBytes = Marshal.AllocCoTaskMem(datos.Length);
            //Marshal.Copy(datos, 0, pUnmanagedBytes, datos.Length);

            //// Llamar a la función con los parámetros correctos
            //bool exito = RawPrinterHelper.SendBytesToPrinter(szPrinterName, pUnmanagedBytes, datos.Length);
            //// Liberar la memoria asignada
            //Marshal.FreeCoTaskMem(pUnmanagedBytes);
            //return;


            ////Comando ESC/ POS para imprimir código de barras "PE156" en formato CODE128
            //byte[] barcodeCommand = new byte[]
            //{
            //0x1D, 0x6B, 0x49, 6,   // (GS k) Comando para CODE128, longitud del código de barras (6 caracteres)
            //(byte)'P', (byte)'E', (byte)'1', (byte)'5', (byte)'6' // Datos del código de barras
            //};

            //// Línea nueva después del código de barras
            //byte[] newLine = new byte[] { 0x0A };

            //// Unir los arrays de bytes (código de barras + salto de línea)
            //byte[] printData = new byte[barcodeCommand.Length + newLine.Length];
            //barcodeCommand.CopyTo(printData, 0);
            //newLine.CopyTo(printData, barcodeCommand.Length);

            //// Convertir el array de bytes a un puntero en memoria
            //IntPtr pUnmanagedBytes = Marshal.AllocCoTaskMem(printData.Length);
            //Marshal.Copy(printData, 0, pUnmanagedBytes, printData.Length);

            //// Enviar datos a la impresora
            //bool exito = RawPrinterHelper.SendBytesToPrinter(szPrinterName, pUnmanagedBytes, printData.Length);

            //// Liberar la memoria asignada
            //Marshal.FreeCoTaskMem(pUnmanagedBytes);

            //return;
            // Send the converted ANSI string to the printer.
            //RawPrinterHelper.SendBytesToPrinter(szPrinterName, pBytes, dwCount);

            //RawPrinterHelper.SendStringToPrinter(impresora, ticket, imprimir);
            //public static bool SendStringToPrinter()
            //{
            //if (true)//imprimir)
            //    {
            //        IntPtr pBytes;
            //        Int32 dwCount;
            //        // How many characters are in the string?
            //        dwCount = szString.Length;
            //        // Assume that the printer is expecting ANSI text, and then convert
            //        // the string to ANSI text.
            //        pBytes = Marshal.AllocCoTaskMem(datos.Length);// Marshal.StringToCoTaskMemAnsi(szString);
            //        // Send the converted ANSI string to the printer.
            //        RawPrinterHelper.SendBytesToPrinter(szPrinterName, pBytes, dwCount);
            //        Marshal.FreeCoTaskMem(pBytes);
            //    }
            //    return true;
            //}
        }

        public void CortaTicket()
        {
            string corte = "\x1B" + "m";                  // caracteres de corte
            string avance = "\x1B" + "d" + "\x09";        // avanza 9 renglones
            //RawPrinterHelper.SendStringToPrinter(impresora, avance, imprimir); // avanza
            //RawPrinterHelper.SendStringToPrinter(impresora, corte, imprimir); // corta
        }
        public void AbreCajon()
        {
            string cajon0 = "\x1B" + "p" + "\x00" + "\x0F" + "\x96";                  // caracteres de apertura cajon 0
            string cajon1 = "\x1B" + "p" + "\x01" + "\x0F" + "\x96";                 // caracteres de apertura cajon 1
            //RawPrinterHelper.SendStringToPrinter(impresora, cajon0, imprimir); // abre cajon0
            //RawPrinterHelper.SendStringToPrinter(impresora, cajon1, imprimir); // abre cajon1
        }
    }
    #endregion
}
