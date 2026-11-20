using System;
using System.IO;
using System.Runtime.Serialization.Json;

namespace Carnisys.Balanza.Agent
{
    internal static class ConfigStore
    {
        // Carpeta y archivo de config: salen de AgentPaths (acepta la variable CARNISYS_BALANZA_DIR para pruebas).
        private static string BasePath { get { return AgentPaths.Carpeta; } }

        private static string ConfigPath { get { return Path.Combine(BasePath, "config.json"); } }

        public static bool ExisteConfig()
        {
            return File.Exists(ConfigPath);
        }

        public static AgentConfig Load()
        {
            try
            {
                if (!File.Exists(ConfigPath))
                {
                    return CreateDefault();
                }

                using (var stream = File.OpenRead(ConfigPath))
                {
                    var serializer = new DataContractJsonSerializer(typeof(AgentConfig));
                    var config = serializer.ReadObject(stream) as AgentConfig;
                    return Normalize(config);
                }
            }
            catch
            {
                return CreateDefault();
            }
        }

        public static void Save(AgentConfig config)
        {
            Directory.CreateDirectory(BasePath);
            using (var stream = File.Create(ConfigPath))
            {
                var serializer = new DataContractJsonSerializer(typeof(AgentConfig));
                serializer.WriteObject(stream, Normalize(config));
            }
        }

        public static string GetBasePath()
        {
            Directory.CreateDirectory(BasePath);
            return BasePath;
        }

        public static AgentConfig Normalize(AgentConfig config)
        {
            config = config ?? new AgentConfig();
            config.Balanza = NormalizeBalanza(config.Balanza);
            config.Api = NormalizeApi(config.Api);
            config.Actualizacion = NormalizeActualizacion(config.Actualizacion);
            return config;
        }

        public static BalanzaConfig NormalizeBalanza(BalanzaConfig config)
        {
            config = config ?? new BalanzaConfig();
            config.Marca = string.IsNullOrWhiteSpace(config.Marca) ? "Systel" : config.Marca.Trim();
            config.Modelo = string.IsNullOrWhiteSpace(config.Modelo)
                ? (string.Equals(config.Marca, "Kretz", StringComparison.OrdinalIgnoreCase) ? "KretzGenerica" : "SystelGenerica")
                : config.Modelo.Trim();
            config.Puerto = string.IsNullOrWhiteSpace(config.Puerto) ? string.Empty : config.Puerto.Trim().ToUpperInvariant();
            config.BaudRate = config.BaudRate > 0 ? config.BaudRate : 9600;
            config.DataBits = config.DataBits > 0 ? config.DataBits : 8;
            config.Parity = string.IsNullOrWhiteSpace(config.Parity) ? "None" : config.Parity.Trim();
            config.StopBits = string.IsNullOrWhiteSpace(config.StopBits)
                ? (string.Equals(config.Marca, "Kretz", StringComparison.OrdinalIgnoreCase) ? "Two" : "One")
                : config.StopBits.Trim();
            config.IntervaloLecturaMs = config.IntervaloLecturaMs >= 100 ? config.IntervaloLecturaMs : 150;
            return config;
        }

        // Origenes por defecto desde los que se acepta actualizar el agente (ver Actualizador).
        public static readonly string[] OrigenesActualizacionPorDefecto = { "https://carnisys.com", "https://www.carnisys.com" };

        public static ActualizacionConfig NormalizeActualizacion(ActualizacionConfig config)
        {
            config = config ?? new ActualizacionConfig();
            if (config.OrigenesPermitidos == null || config.OrigenesPermitidos.Count == 0)
            {
                config.OrigenesPermitidos = new System.Collections.Generic.List<string>(OrigenesActualizacionPorDefecto);
            }
            return config;
        }

        public static ApiConfig NormalizeApi(ApiConfig config)
        {
            config = config ?? new ApiConfig();
            config.Host = "127.0.0.1";
            config.Port = config.Port > 0 ? config.Port : 5100;
            return config;
        }

        private static AgentConfig CreateDefault()
        {
            return Normalize(new AgentConfig());
        }
    }
}
