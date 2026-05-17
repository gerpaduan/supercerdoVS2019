using System;
using System.Diagnostics;
using Microsoft.Win32;

namespace CarniSys.PrintAgent
{
    internal static class StartupRegistration
    {
        private const string RunKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";
        private const string ValueName = "CarniSys.PrintAgent";

        public static void EnsureRegistered()
        {
            try
            {
                string executablePath = GetExecutablePath();
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
            catch
            {
                // Si no se puede registrar el inicio automático, el agente sigue funcionando manualmente.
            }
        }

        private static string GetExecutablePath()
        {
            try
            {
                using (var process = Process.GetCurrentProcess())
                {
                    return process.MainModule != null ? process.MainModule.FileName : null;
                }
            }
            catch
            {
                return null;
            }
        }
    }
}
