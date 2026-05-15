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
