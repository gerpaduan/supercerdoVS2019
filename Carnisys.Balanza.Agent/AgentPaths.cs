using System;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Carnisys.Balanza.Agent
{
    // Rutas y nombres del agente en un solo lugar (config, log, carpeta de instalacion, exe, nombres de mutex/evento).
    // Variable de entorno CARNISYS_BALANZA_DIR (solo para pruebas y diagnostico): reemplaza la carpeta del agente
    // (%LocalAppData%\CarniSys\BalanzaAgent) y apaga el registro de inicio con Windows, para poder probar el instalador
    // sin tocar el agente real instalado en la PC.
    internal static class AgentPaths
    {
        public const string VariableCarpeta = "CARNISYS_BALANZA_DIR";
        public const string NombreExe = "Carnisys.Balanza.Agent.exe";
        public const string NombreProceso = "Carnisys.Balanza.Agent";

        private static readonly string CarpetaPorDefecto = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "CarniSys",
            "BalanzaAgent");

        // Carpeta donde se instala el agente y donde viven config.json y logs.
        public static string Carpeta
        {
            get
            {
                string valor = Environment.GetEnvironmentVariable(VariableCarpeta);
                return string.IsNullOrWhiteSpace(valor) ? CarpetaPorDefecto : Path.GetFullPath(valor.Trim());
            }
        }

        // true cuando la carpeta viene de la variable de entorno (prueba): no se toca el inicio de Windows.
        public static bool ModoPrueba
        {
            get { return !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(VariableCarpeta)); }
        }

        public static string RutaExeInstalado
        {
            get { return Path.Combine(Carpeta, NombreExe); }
        }

        public static string CarpetaLogs
        {
            get { return Path.Combine(Carpeta, "logs"); }
        }

        // Donde el agente deja el .exe nuevo que bajo para actualizarse (se borra al arrancar la version instalada).
        public static string CarpetaActualizacion
        {
            get { return Path.Combine(Carpeta, "actualizacion"); }
        }

        public static string RutaExeActual()
        {
            try
            {
                using (var proceso = Process.GetCurrentProcess())
                {
                    return proceso.MainModule != null ? proceso.MainModule.FileName : null;
                }
            }
            catch
            {
                return null;
            }
        }

        // El exe en ejecucion es el instalado (corre desde la carpeta del agente) y no el descargado a Descargas/Escritorio.
        public static bool EsCopiaInstalada()
        {
            string actual = RutaExeActual();
            if (string.IsNullOrWhiteSpace(actual))
            {
                // Sin poder saber desde donde corre, no se ofrece instalar: mejor arrancar el agente que reinstalar en bucle.
                return true;
            }

            return string.Equals(
                Path.GetFullPath(actual),
                Path.GetFullPath(RutaExeInstalado),
                StringComparison.OrdinalIgnoreCase);
        }

        // Sufijo estable por carpeta: el agente real y uno de prueba (otra carpeta) no comparten mutex ni evento.
        public static string SufijoInstancia()
        {
            using (var sha = SHA1.Create())
            {
                byte[] hash = sha.ComputeHash(Encoding.UTF8.GetBytes(Carpeta.ToLowerInvariant()));
                return BitConverter.ToString(hash, 0, 6).Replace("-", string.Empty);
            }
        }

        public static string NombreMutex()
        {
            return "Carnisys.Balanza.Agent.Instancia." + SufijoInstancia();
        }

        public static string NombreEventoConfigurar()
        {
            return "Carnisys.Balanza.Agent.Configurar." + SufijoInstancia();
        }

        public static string NombreEventoSalir()
        {
            return "Carnisys.Balanza.Agent.Salir." + SufijoInstancia();
        }

        public static string Version()
        {
            var version = typeof(AgentPaths).Assembly.GetName().Version;
            return version != null ? version.ToString(3) : "?";
        }
    }
}
