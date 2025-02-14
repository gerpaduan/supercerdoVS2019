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


        static byte[] CombineByteArrays(params byte[][] arrays)
        {
            int totalLength = 0;
            foreach (var arr in arrays) totalLength += arr.Length;

            byte[] result = new byte[totalLength];
            int offset = 0;

            foreach (var arr in arrays)
            {
                Buffer.BlockCopy(arr, 0, result, offset, arr.Length);
                offset += arr.Length;
            }

            return result;
        }

        public void realizarImpresionCodigoBarra(string szString)
        {
            string printerName = impresora; // Asegúrate de que este sea el nombre correcto de la impresora

            // Reset de la impresora
            byte[] init = new byte[] { 0x1B, 0x40 };

            // Centrar el contenido (tanto el texto como el código de barras)
            byte[] centerAlign = new byte[] { 0x1B, 0x61, 1 };

            // Texto de prueba antes del código de barras
            byte[] printText = Encoding.ASCII.GetBytes("\n");

            // Configurar la altura del código de barras
            byte[] setBarcodeHeight = new byte[] { 0x1D, 0x68, 100 };

            // Configurar el ancho del código de barras
            byte[] setBarcodeWidth = new byte[] { 0x1D, 0x77, 3 };

            // Configurar para mostrar el texto debajo del código de barras
            byte[] showText = new byte[] { 0x1D, 0x48, 2 };

            //// Imprimir un código de barras tipo CODE128 con "123456"
            //byte[] barcodeCommand = new byte[]
            //{
            //0x1D, 0x6B, 73, // Comando para CODE128
            //6,  // Longitud de los datos (6 caracteres)
            //(byte)'P', (byte)'E', (byte)'3', (byte)'4', (byte)'5', (byte)'6'
            //};
            // Construir el comando del código de barras dinámicamente
            string barcodeData = szString;
            byte[] barcodeCommand = new byte[4 + barcodeData.Length];
            barcodeCommand[0] = 0x1D; // Comando GS
            barcodeCommand[1] = 0x6B; // Comando para código de barras
            barcodeCommand[2] = 73; // 73 CODE128 | 4 CODE39 | 2 EAN13
            barcodeCommand[3] = (byte)barcodeData.Length; // Longitud del código

            for (int i = 0; i < barcodeData.Length; i++)
            {
                barcodeCommand[4 + i] = (byte)barcodeData[i]; // Insertar los caracteres del código
            }

            // Salto de línea y corte de papel
            byte[] newLines = new byte[] { 0x0A, 0x0A, 0x0A };
            byte[] cutPaper = new byte[] { 0x1D, 0x56, 0x41, 0x00 };

            // Concatenar comandos
            byte[] dataToSend = CombineByteArrays(init, centerAlign, printText, setBarcodeHeight, setBarcodeWidth, showText, barcodeCommand, newLines, cutPaper);

            // Enviar datos a la impresora
            if (RawPrinterHelper.SendBytesToPrinter(printerName, dataToSend))
            {
                Console.WriteLine("Código de barras impreso correctamente.");
            }
            else
            {
                Console.WriteLine("Error al imprimir.");
            }
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
