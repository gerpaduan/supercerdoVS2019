using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace Carnisys.Balanza.Agent
{
    // Log a archivo del agente (2026-11-20): deja rastro de errores de lectura, reaperturas del puerto, lecturas congeladas y
    // clientes HTTP con problemas, para saber en el campo por que una balanza dejo de actualizar el peso.
    // Un archivo por dia en %LOCALAPPDATA%\CarniSys\BalanzaAgent\logs, se conservan DiasRetencion dias. Nunca se loguean datos
    // personales ni credenciales (el agente no maneja ninguno): solo puerto, marca y mensajes de error.
    internal static class AgentLog
    {
        private const int DiasRetencion = 14;
        private static readonly object Sync = new object();
        private static readonly string LogDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "CarniSys",
            "BalanzaAgent",
            "logs");

        private static DateTime _ultimaLimpieza = DateTime.MinValue;

        public static string Directorio => LogDir;

        public static void Info(string mensaje)
        {
            Escribir("INFO", mensaje);
        }

        public static void Warn(string mensaje)
        {
            Escribir("WARN", mensaje);
        }

        public static void Error(string mensaje, Exception ex = null)
        {
            Escribir("ERROR", ex == null ? mensaje : mensaje + " | " + ex.GetType().Name + ": " + ex.Message);
        }

        private static void Escribir(string nivel, string mensaje)
        {
            var ahora = DateTime.Now;
            string linea = ahora.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture) + " [" + nivel + "] " + (mensaje ?? string.Empty);

            lock (Sync)
            {
                try
                {
                    Directory.CreateDirectory(LogDir);
                    string archivo = Path.Combine(LogDir, "agente-" + ahora.ToString("yyyyMMdd", CultureInfo.InvariantCulture) + ".log");
                    File.AppendAllText(archivo, linea + Environment.NewLine, Encoding.UTF8);
                    LimpiarViejos(ahora);
                }
                catch (Exception ex)
                {
                    // El log no puede loguear su propia falla en archivo (disco lleno, permisos): va a la salida de depuracion y el
                    // agente sigue leyendo la balanza, que es lo que importa.
                    Trace.WriteLine("AgentLog: no se pudo escribir el log: " + ex.Message + " | " + linea);
                }
            }
        }

        // Borra los archivos de log mas viejos que DiasRetencion; corre como mucho una vez por hora.
        private static void LimpiarViejos(DateTime ahora)
        {
            if ((ahora - _ultimaLimpieza).TotalHours < 1)
            {
                return;
            }

            _ultimaLimpieza = ahora;
            var limite = ahora.Date.AddDays(-DiasRetencion);
            foreach (var archivo in new DirectoryInfo(LogDir).GetFiles("agente-*.log").Where(f => f.LastWriteTime < limite))
            {
                archivo.Delete();
            }
        }
    }
}
