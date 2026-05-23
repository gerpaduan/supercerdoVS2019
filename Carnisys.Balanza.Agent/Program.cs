using System;
using System.Linq;
using System.Windows.Forms;

namespace Carnisys.Balanza.Agent
{
    internal static class Program
    {
        [STAThread]
        private static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            bool abrirConfiguracion = args != null && args.Any(a =>
                string.Equals(a, "--configure", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(a, "/configure", StringComparison.OrdinalIgnoreCase));

            Application.Run(new AgentApplicationContext(abrirConfiguracion));
        }
    }
}
