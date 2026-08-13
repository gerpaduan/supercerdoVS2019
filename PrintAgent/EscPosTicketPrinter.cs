using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CarniSys.PrintAgent
{
    internal static class EscPosTicketPrinter
    {
        public static void Print(TicketPrintRequest request, AgentConfig config)
        {
            if (request == null)
            {
                throw new InvalidOperationException("No se recibieron datos para imprimir.");
            }

            var printerName = !string.IsNullOrWhiteSpace(request.PrinterName)
                ? request.PrinterName
                : config != null ? config.PrinterName : "";

            if (string.IsNullOrWhiteSpace(printerName))
            {
                throw new InvalidOperationException("No hay impresora configurada.");
            }

            var data = new List<byte[]>
            {
                new byte[] { 0x1B, 0x40 }
            };

            string ticketText = string.Join("\n", (request.TicketLines ?? new List<string>())) + "\n";
            data.Add(Encoding.GetEncoding(850).GetBytes(ticketText));

            if (!string.IsNullOrWhiteSpace(request.BarcodeValue))
            {
                data.Add(BuildBarcodeBlock(request.BarcodeHeader ?? "", request.BarcodeValue));
            }
            else
            {
                data.Add(new byte[] { 0x0A, 0x0A, 0x0A });
            }

            // QR de AFIP (RG 4892/2020), debajo del ticket. request.QrValue ya viene armado por
            // el server (VentasController.ImprimirTicketPayload -> GenerarDocs.GenerarQrUrl) con
            // la URL completa a codificar; vacio si la venta no esta facturada.
            if (!string.IsNullOrWhiteSpace(request.QrValue))
            {
                data.Add(BuildQrBlock(request.QrValue));
            }

            data.Add(new byte[] { 0x1D, 0x56, 0x41, 0x00 });

            var raw = Combine(data);
            if (!RawPrinterHelper.SendBytesToPrinter(printerName, raw))
            {
                throw new InvalidOperationException("No se pudo enviar el ticket a la impresora seleccionada.");
            }
        }

        private static byte[] BuildBarcodeBlock(string header, string barcodeValue)
        {
            var parts = new List<byte[]>
            {
                new byte[] { 0x1B, 0x61, 0x01 }
            };

            if (!string.IsNullOrWhiteSpace(header))
            {
                parts.Add(Encoding.GetEncoding(850).GetBytes(header + "\n\n"));
            }

            parts.Add(new byte[] { 0x1D, 0x68, 90 });
            parts.Add(new byte[] { 0x1D, 0x77, 2 });
            parts.Add(new byte[] { 0x1D, 0x48, 2 });

            byte[] barcode = Encoding.ASCII.GetBytes(barcodeValue);
            var barcodeCommand = new byte[4 + barcode.Length];
            barcodeCommand[0] = 0x1D;
            barcodeCommand[1] = 0x6B;
            barcodeCommand[2] = 73;
            barcodeCommand[3] = (byte)barcode.Length;
            Array.Copy(barcode, 0, barcodeCommand, 4, barcode.Length);

            parts.Add(barcodeCommand);
            parts.Add(new byte[] { 0x0A, 0x0A, 0x0A });
            return Combine(parts);
        }

        // Comandos ESC/POS nativos GS ( k para QR (familia estandar Epson, ampliamente
        // soportada por impresoras termicas compatibles) -- la impresora arma el QR sola a
        // partir del string, no hace falta generar ninguna imagen/bitmap del lado del agente.
        // NOTA: no se pudo verificar contra una impresora fisica real (no disponible en este
        // entorno); la secuencia de bytes sigue la especificacion publica estandar (misma que
        // usan la gran mayoria de impresoras ESC/POS compatibles), pero queda pendiente de
        // confirmar en un ticket impreso real antes de dar esto por definitivamente probado.
        private static byte[] BuildQrBlock(string qrContent)
        {
            byte[] qrBytes = Encoding.ASCII.GetBytes(qrContent);

            var parts = new List<byte[]>
            {
                new byte[] { 0x1B, 0x61, 0x01 }, // centrado, mismo criterio que el codigo de barras
                new byte[] { 0x1D, 0x28, 0x6B, 0x04, 0x00, 0x31, 0x41, 0x32, 0x00 }, // modelo 2
                new byte[] { 0x1D, 0x28, 0x6B, 0x03, 0x00, 0x31, 0x43, 0x06 }, // tamano de modulo
                new byte[] { 0x1D, 0x28, 0x6B, 0x03, 0x00, 0x31, 0x45, 0x31 }  // correccion de errores nivel M
            };

            // Comando "store data": guarda el contenido del QR en el buffer de la impresora.
            // pL/pH = longitud de (cn + fn + m + datos) en little-endian, 2 bytes.
            int storeLen = qrBytes.Length + 3;
            var storeCommand = new byte[8 + qrBytes.Length];
            storeCommand[0] = 0x1D;
            storeCommand[1] = 0x28;
            storeCommand[2] = 0x6B;
            storeCommand[3] = (byte)(storeLen & 0xFF);
            storeCommand[4] = (byte)((storeLen >> 8) & 0xFF);
            storeCommand[5] = 0x31;
            storeCommand[6] = 0x50;
            storeCommand[7] = 0x30;
            Array.Copy(qrBytes, 0, storeCommand, 8, qrBytes.Length);
            parts.Add(storeCommand);

            parts.Add(new byte[] { 0x1D, 0x28, 0x6B, 0x03, 0x00, 0x31, 0x51, 0x30 }); // imprime el QR guardado
            parts.Add(new byte[] { 0x1B, 0x61, 0x00 }); // vuelve a alineacion izquierda
            parts.Add(new byte[] { 0x0A, 0x0A });

            return Combine(parts);
        }

        private static byte[] Combine(IEnumerable<byte[]> arrays)
        {
            int total = arrays.Sum(x => x.Length);
            var result = new byte[total];
            int offset = 0;
            foreach (var arr in arrays)
            {
                Buffer.BlockCopy(arr, 0, result, offset, arr.Length);
                offset += arr.Length;
            }

            return result;
        }
    }
}
