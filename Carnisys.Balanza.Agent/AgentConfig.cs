using System.Runtime.Serialization;

namespace Carnisys.Balanza.Agent
{
    [DataContract]
    public sealed class AgentConfig
    {
        [DataMember(Name = "balanza")]
        public BalanzaConfig Balanza { get; set; }

        [DataMember(Name = "api")]
        public ApiConfig Api { get; set; }

        [DataMember(Name = "actualizacion")]
        public ActualizacionConfig Actualizacion { get; set; }
    }

    // Sitios desde los que la web puede pedirle al agente que se actualice (Origin del navegador). Por defecto solo carnisys.com;
    // las instalaciones en red local (Servidor SM, San Lorenzo) se agregan a mano en config.json.
    [DataContract]
    public sealed class ActualizacionConfig
    {
        [DataMember(Name = "origenesPermitidos")]
        public System.Collections.Generic.List<string> OrigenesPermitidos { get; set; }
    }
}
