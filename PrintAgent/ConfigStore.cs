using System;
using System.IO;
using System.Runtime.Serialization.Json;

namespace CarniSys.PrintAgent
{
    internal static class ConfigStore
    {
        private static readonly string BasePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "CarniSys",
            "PrintAgent");

        private static readonly string ConfigPath = Path.Combine(BasePath, "config.json");

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

        private static AgentConfig CreateDefault()
        {
            return Normalize(new AgentConfig());
        }

        private static AgentConfig Normalize(AgentConfig config)
        {
            config = config ?? new AgentConfig();
            if (config.TicketMm != 80)
            {
                config.TicketMm = 58;
            }

            return config;
        }
    }
}
