using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Configuration;
using System.Threading;
using System.Drawing;

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

            Application.ThreadException += new ThreadExceptionEventHandler(Application_ThreadException);

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

        //Atrapar la excepcion *Blog Leandro Tuttini* 
        static void Application_ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
        {
            MessageBox.Show(e.Exception.Message);
        }
    }
}
