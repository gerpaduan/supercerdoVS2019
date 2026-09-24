using System;
using System.IO;
using System.Runtime.CompilerServices;

namespace Datos.BorradoresTests
{
    // Utilidades.Core.Conexion lee ConfigurationManager en campos static. En .NET moderno, bajo el testhost,
    // ConfigurationManager busca "testhost.dll.config" y no el App.config de este proyecto; se le indica
    // el archivo correcto ANTES de que cualquier prueba toque Conexion (por eso un ModuleInitializer).
    internal static class ConfigInicial
    {
        [ModuleInitializer]
        internal static void Inicializar()
        {
            string config = Path.Combine(AppContext.BaseDirectory, "Datos.BorradoresTests.dll.config");
            AppContext.SetData("APP_CONFIG_FILE", config);
        }
    }
}
