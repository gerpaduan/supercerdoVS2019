using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Configuration;

namespace Presentacion
{
    static class Program
    {
        /// <summary>
        /// Punto de entrada principal para la aplicación.
        /// </summary>
        [STAThread]

        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            string formInicioForm = ConfigurationManager.AppSettings["formInicioApp"].ToString();
            switch (formInicioForm)
            {
                case "0":
                    Application.Run(new FormPrincipal());
                    break;
                case "1":
                    Application.Run(new Cortes.formStockActual());
                    break;
                default:
                    break;
            }
        }
    }
}
