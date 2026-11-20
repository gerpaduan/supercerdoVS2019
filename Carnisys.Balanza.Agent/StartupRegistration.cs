using System;
using System.Diagnostics;
using Microsoft.Win32;

namespace Carnisys.Balanza.Agent
{
    internal static class StartupRegistration
    {
        private const string RunKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";
        private const string ValueName = "Carnisys.Balanza.Agent";

        public static void EnsureRegistered()
        {
            Register(GetExecutablePath());
        }

        // Registra el agente para iniciar con Windows (solo el usuario actual, sin permisos de administrador).
        // En modo prueba (CARNISYS_BALANZA_DIR) no toca el registro.
        public static void Register(string executablePath)
        {
            if (AgentPaths.ModoPrueba)
            {
                return;
            }

            try
            {
                if (string.IsNullOrWhiteSpace(executablePath))
                {
                    return;
                }

                string expectedValue = "\"" + executablePath + "\"";

                using (var key = Registry.CurrentUser.CreateSubKey(RunKeyPath))
                {
                    if (key == null)
                    {
                        return;
                    }

                    string currentValue = key.GetValue(ValueName) as string;
                    if (!string.Equals(currentValue, expectedValue, StringComparison.OrdinalIgnoreCase))
                    {
                        key.SetValue(ValueName, expectedValue);
                    }
                }
            }
            catch (Exception ex)
            {
                AgentLog.Error("No se pudo registrar el inicio con Windows.", ex);
            }
        }

        private static string GetExecutablePath()
        {
            return AgentPaths.RutaExeActual();
        }
    }
}
