using System;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace Carnisys.Balanza.Agent
{
    internal static class Program
    {
        // --configure: abre la ventana de configuracion al iniciar. --sin-instalar: corre como agente desde donde esta (desarrollo).
        [STAThread]
        private static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Todo error no capturado queda en el registro (antes el agente se caia sin dejar rastro).
            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
                AgentLog.Error("Error no controlado (el agente se cierra).", e.ExceptionObject as Exception);
            Application.ThreadException += (s, e) => AgentLog.Error("Error no controlado en la interfaz.", e.Exception);

            bool abrirConfiguracion = TieneArgumento(args, "--configure") || TieneArgumento(args, "/configure");
            bool sinInstalar = TieneArgumento(args, "--sin-instalar");
            // --actualizacion: lo ejecuta el propio agente tras bajar y verificar una version nueva: se instala sin esperar clics.
            bool actualizacion = TieneArgumento(args, "--actualizacion");

            // Ejecutado desde Descargas/Escritorio (no desde la carpeta del agente): es el instalador de un solo archivo.
            if (!sinInstalar && !AgentPaths.EsCopiaInstalada())
            {
                using (var instalador = new InstallerForm(actualizacion))
                {
                    Application.Run(instalador);
                }
                return;
            }

            // Una sola instancia por carpeta de agente: si ya corre uno, se le pide mostrar la configuracion y esta se cierra.
            bool esLaPrimera;
            using (var mutex = new Mutex(true, AgentPaths.NombreMutex(), out esLaPrimera))
            {
                if (!esLaPrimera)
                {
                    PedirConfiguracionAlAgenteEnCurso();
                    return;
                }

                AgentLog.Info("Agente " + AgentPaths.Version() + " iniciado desde " + AgentPaths.RutaExeActual());
                Application.Run(new AgentApplicationContext(abrirConfiguracion));
                GC.KeepAlive(mutex);
            }
        }

        private static bool TieneArgumento(string[] args, string nombre)
        {
            return args != null && args.Any(a => string.Equals(a, nombre, StringComparison.OrdinalIgnoreCase));
        }

        private static void PedirConfiguracionAlAgenteEnCurso()
        {
            try
            {
                using (var evento = EventWaitHandle.OpenExisting(AgentPaths.NombreEventoConfigurar()))
                {
                    evento.Set();
                }
            }
            catch (WaitHandleCannotBeOpenedException)
            {
                // El agente en curso es de una version vieja (sin escucha de senales): no hay a quien avisar.
                AgentLog.Warn("Ya hay un agente corriendo y no escucha pedidos de configuracion (version anterior).");
            }
        }
    }
}
