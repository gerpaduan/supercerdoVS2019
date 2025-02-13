using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing.Printing;
using System.Runtime.InteropServices;

namespace Presentacion.Ticket
{
    class ESC_POS_Printer
    {
        [DllImport("winspool.drv", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool OpenPrinter(string szPrinterName, out IntPtr hPrinter, IntPtr pd);

        [DllImport("winspool.drv", CharSet = CharSet.Auto)]
        public extern static bool StartDocPrinter(IntPtr hPrinter, Int32 level, [In] ref DOCINFO di);

        [DllImport("winspool.drv", CharSet = CharSet.Auto)]
        public extern static bool StartPagePrinter(IntPtr hPrinter);

        [DllImport("winspool.drv", CharSet = CharSet.Auto)]
        public extern static bool EndPagePrinter(IntPtr hPrinter);

        [DllImport("winspool.drv", CharSet = CharSet.Auto)]
        public extern static bool EndDocPrinter(IntPtr hPrinter);

        [DllImport("winspool.drv", CharSet = CharSet.Auto)]
        public extern static bool WritePrinter(IntPtr hPrinter, IntPtr pBytes, Int32 dwCount, out Int32 dwWritten);

        [DllImport("kernel32.dll")]
        public extern static IntPtr GlobalAlloc(int uFlags, int dwBytes);

        [DllImport("kernel32.dll")]
        public extern static IntPtr GlobalFree(IntPtr hMem);

        public struct DOCINFO
        {
            [MarshalAs(UnmanagedType.LPStr)]
            public string pDocName;
            [MarshalAs(UnmanagedType.LPStr)]
            public string pOutputFile;
            [MarshalAs(UnmanagedType.LPStr)]
            public string pDataType;
        }

        // Enviar los datos a la impresora
        public static void SendToPrinter(string printerName, string text)
        {
            IntPtr hPrinter;
            DOCINFO di = new DOCINFO();
            di.pDocName = "Test Print";
            di.pDataType = "RAW";

            OpenPrinter(printerName, out hPrinter, IntPtr.Zero);
            StartDocPrinter(hPrinter, 1, ref di);
            StartPagePrinter(hPrinter);

            // ESC/POS comandos para inicializar la impresora y texto
            byte[] esc_pos_init = new byte[] { 27, 64 };  // ESC @ - Inicializar la impresora
            byte[] esc_pos_text = System.Text.Encoding.ASCII.GetBytes(text);  // Texto para imprimir
            byte[] esc_pos_line_feed = new byte[] { 10 };  // LF (salto de línea)

            // Enviar datos a la impresora
            SendBytesToPrinter(hPrinter, esc_pos_init);
            SendBytesToPrinter(hPrinter, esc_pos_text);
            SendBytesToPrinter(hPrinter, esc_pos_line_feed);

            // Finalizar la impresión
            EndPagePrinter(hPrinter);
            EndDocPrinter(hPrinter);
        }

        // Enviar los bytes a la impresora
        private static void SendBytesToPrinter(IntPtr hPrinter, byte[] bytes)
        {
            IntPtr pBytes;
            int dwCount = bytes.Length;

            // Asignar memoria para los bytes
            pBytes = Marshal.AllocCoTaskMem(dwCount);
            Marshal.Copy(bytes, 0, pBytes, dwCount);

            // Enviar los datos a la impresora
            int dwWritten = 0;
            bool result = WritePrinter(hPrinter, pBytes, dwCount, out dwWritten);

            if (!result || dwWritten != dwCount)
            {
                Console.WriteLine("Error al escribir en la impresora. Código: " + Marshal.GetLastWin32Error());
            }

            // Liberar la memoria no administrada
            Marshal.FreeCoTaskMem(pBytes);
        }
    }
}
