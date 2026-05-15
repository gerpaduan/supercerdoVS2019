using System;
using System.Windows.Forms;

namespace CarniSys.PrintAgent
{
    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new AgentApplicationContext());
        }
    }
}
